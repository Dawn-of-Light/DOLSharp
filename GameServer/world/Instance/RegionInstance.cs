/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using DOL.GS;
using DOL.Database;

using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Description of RegionInstance.
	/// Clone a RegionData to a New BaseInstance
	/// Can duplicate any "Region" of the Game into a dedicated Instance
	/// Handle Zones and Areas, doesn't Handle Persistence.
	/// </summary>
	public class RegionInstance : BaseInstance
	{
		/// <summary>
		/// Console Logger
		/// </summary>
		private static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
		/// <summary>
		/// List Containing players in instance
		/// </summary>
		private List<GamePlayer> m_players_in;
		
		/// <summary>
		/// Entrance location of Instance, needed to force exit players.
		/// </summary>
		protected GameLocation m_sourceentrance;
		
		/// <summary>
		/// List Containing players in instance
		/// </summary>
		protected List<GamePlayer> PlayersInside
		{
			get { return m_players_in; }
		}

		/// <summary>
		/// Entrance location of Instance, needed to force exit players.
		/// </summary>
		public GameLocation SourceEntrance
		{
			get { return m_sourceentrance; }
			set { m_sourceentrance = value; }
		}
		
		/// <summary>
		/// On Player Enter override to add him to container
		/// </summary>
		/// <param name="player"></param>
		public override void OnPlayerEnterInstance(GamePlayer player)
		{
			//Add Player
			this.m_players_in.Add(player);
			//Stop the timer to prevent the region's removal.
			base.OnPlayerEnterInstance(player);
		}
		
		/// <summary>
		/// On Player Exit override to remove him from container
		/// </summary>
		/// <param name="player"></param>
		public override void OnPlayerLeaveInstance(GamePlayer player)
        {
            //Decrease the amount of players
            base.OnPlayerLeaveInstance(player);
            this.m_players_in.Remove(player);
        }
		
		/// <summary>
		/// RegionInstance Constructor
		/// </summary>
		/// <param name="player"></param>
		public RegionInstance(ushort ID, GameTimer.TimeManager time, RegionData dat)
			: base(ID, time, dat)
		{	
			this.m_players_in = new List<GamePlayer>();
			this.DestroyWhenEmpty = false;
		}
		
		/// <summary>
		/// Load from Database override to clone objects from original Region.
		/// Loads Objects, Mobs, Areas from Database using "SkinID"
		/// </summary>
		public override void LoadFromDatabase(Mob[] mobObjs, ref long mobCount, ref long merchantCount, ref long itemCount, ref long bindCount)
        {
            if (!LoadObjects)
                return;

            Assembly gasm = Assembly.GetAssembly(typeof(GameServer));
            var staticObjs = GameServer.Database.SelectObjects<WorldObject>("Region = " + Skin);
            var areaObjs = GameServer.Database.SelectObjects<DBArea>("Region = " + Skin);
            
            
            int count = mobObjs.Length + staticObjs.Count;
            if (count > 0) PreAllocateRegionSpace(count + 100);
            
            int myItemCount = staticObjs.Count;
            int myMobCount = 0;
            int myMerchantCount = 0;
            
            string allErrors = string.Empty;

            if (mobObjs.Length > 0)
            {
                foreach (Mob mob in mobObjs)
                {
                    GameNPC myMob = null;
                    string error = string.Empty;
  
                    // Default Classtype
                    string classtype = ServerProperties.Properties.GAMENPC_DEFAULT_CLASSTYPE;
                    
                    // load template if any
                    INpcTemplate template = null;
                    if(mob.NPCTemplateID != -1)
                    {
                    	template = NpcTemplateMgr.GetTemplate(mob.NPCTemplateID);
                    }
                    

                    if (mob.Guild.Length > 0 && mob.Realm >= 0 && mob.Realm <= (int)eRealm._Last)
                    {
                        Type type = ScriptMgr.FindNPCGuildScriptClass(mob.Guild, (eRealm)mob.Realm);
                        if (type != null)
                        {
                            try
                            {
                                
                                myMob = (GameNPC)type.Assembly.CreateInstance(type.FullName);
                               	
                            }
                            catch (Exception e)
                            {
                                if (log.IsErrorEnabled)
                                    log.Error("LoadFromDatabase", e);
                            }
                        }
                    }

  
                    if (myMob == null)
                    {
                    	if(template != null && template.ClassType != null && template.ClassType.Length > 0 && template.ClassType != Mob.DEFAULT_NPC_CLASSTYPE && template.ReplaceMobValues)
                    	{
                			classtype = template.ClassType;
                    	}
                        else if (mob.ClassType != null && mob.ClassType.Length > 0 && mob.ClassType != Mob.DEFAULT_NPC_CLASSTYPE)
                        {
                            classtype = mob.ClassType;
                        }

                        try
                        {
                            myMob = (GameNPC)gasm.CreateInstance(classtype, false);
                        }
                        catch
                        {
                            error = classtype;
                        }

                        if (myMob == null)
                        {
                            foreach (Assembly asm in ScriptMgr.Scripts)
                            {
                                try
                                {
                                    myMob = (GameNPC)asm.CreateInstance(classtype, false);
                                    error = string.Empty;
                                }
                                catch
                                {
                                    error = classtype;
                                }

                                if (myMob != null)
                                    break;
                            }

                            if (myMob == null)
                            {
                                myMob = new GameNPC();
                                error = classtype;
                            }
                        }
                    }

                    if (!allErrors.Contains(error))
                        allErrors += " " + error + ",";

                    if (myMob != null)
                    {
                        try
                        {
                        	Mob clone = (Mob)mob.Clone();
                        	clone.AllowAdd = false;
                        	clone.AllowDelete = false;
                        	clone.Region = this.ID;
                        	
                        	myMob.LoadFromDatabase(clone);

                            if (myMob is GameMerchant)
                            {
                                myMerchantCount++;
                            }
                            else
                            {
                                myMobCount++;
                            }
                        }
                        catch (Exception e)
                        {
                            if (log.IsErrorEnabled)
                                log.Error("Failed: " + myMob.GetType().FullName + ":LoadFromDatabase(" + mob.GetType().FullName + ");", e);
                            throw;
                        }

                        myMob.AddToWorld();
                    }
                }
            }

            if (staticObjs.Count > 0)
            {
                foreach (WorldObject item in staticObjs)
                {
                	WorldObject itemclone = (WorldObject)item.Clone();
                	itemclone.AllowAdd = false;
                	itemclone.AllowDelete = false;
                	itemclone.Region = this.ID;
                	
                    GameStaticItem myItem;
                    if (!string.IsNullOrEmpty(itemclone.ClassType))
                    {
                        myItem = gasm.CreateInstance(itemclone.ClassType, false) as GameStaticItem;
                        if (myItem == null)
                        {
                            foreach (Assembly asm in ScriptMgr.Scripts)
                            {
                                try
                                {
                                    myItem = (GameStaticItem)asm.CreateInstance(itemclone.ClassType, false);
                                }
                                catch { }
                                if (myItem != null)
                                    break;
                            }
                            if (myItem == null)
                                myItem = new GameStaticItem();
                        }
                    }
                    else
                        myItem = new GameStaticItem();

                    myItem.AddToWorld();
                }
            }

            int areaCnt = 0;
            // Add missing area
            foreach(DBArea area in areaObjs) 
            {
            	// Don't bind in instance.
            	if(area.ClassType.Equals("DOL.GS.Area+BindArea"))
            		continue;
            	
            	// clone DB object.
            	DBArea newDBArea = ((DBArea)area.Clone());
            	newDBArea.AllowAdd = false;
            	newDBArea.Region = this.ID;
            	// Instantiate Area with cloned DB object and add to region
            	try
            	{
            		AbstractArea newArea = (AbstractArea)gasm.CreateInstance(newDBArea.ClassType, false);
            		newArea.LoadFromDatabase(newDBArea);
					newArea.Sound = newDBArea.Sound;
					newArea.CanBroadcast = newDBArea.CanBroadcast;
					newArea.CheckLOS = newDBArea.CheckLOS;
					this.AddArea(newArea);
					areaCnt++;
	            }
            	catch
            	{
            		log.Warn("area type " + area.ClassType + " cannot be created, skipping");
            		continue;
            	}

            }
            
            if (myMobCount + myItemCount + myMerchantCount > 0)
            {
                if (log.IsInfoEnabled)
                    log.Info(String.Format("AdventureWingInstance: {0} ({1}) loaded {2} mobs, {3} merchants, {4} items, {5}/{6} areas from DB ({7})", Description, ID, myMobCount, myMerchantCount, myItemCount, areaCnt, areaObjs.Count, TimeManager.Name));

                log.Debug("Used Memory: " + GC.GetTotalMemory(false) / 1024 / 1024 + "MB");

                if (allErrors != string.Empty)
                    log.Error("Error loading the following NPC ClassType(s), GameNPC used instead:" + allErrors.TrimEnd(','));

                Thread.Sleep(0);  // give up remaining thread time to other resources
            }
            mobCount += myMobCount;
            merchantCount += myMerchantCount;
            itemCount += myItemCount;
        }

	}
}
