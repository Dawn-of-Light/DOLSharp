	
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

/*
* Author:	Lamyuras
* Date:		
*
* Notes:
*  
*/

using System;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;
using DOL.GS.Quests;
using DOL.AI.Brain;

namespace DOL.GS.Quests.Midgard {
	
     /* The first thing we do, is to declare the class we create
	 * as Quest. To do this, we derive from the abstract class
	 * BaseQuest	  	 
	 */
	public class childsplay : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		///
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/* Declare the variables we need inside our quest.
		* You can declare static variables here, which will be available in
		* ALL instance of your quest and should be initialized ONLY ONCE inside
		* the OnScriptLoaded method.
		*
		* Or declare nonstatic variables here which can be unique for each Player
		* and change through the quest journey...
		*
		*/

		protected const string questTitle = "Child's Play";

		protected const int minimumLevel = 1;
		protected const int maximumLevel = 4;
	
	
		private static GameNPC Statuedemonsbreach = null;
		
		private static GameNPC Charles = null;
		
		private static ItemTemplate daringpaddedboots = null;
		
		private static ItemTemplate daringpaddedcap = null;
		
		private static ItemTemplate daringpaddedgloves = null;
		
		private static ItemTemplate daringpaddedpants = null;
		
		private static ItemTemplate daringpaddedsleeves = null;
		
		private static ItemTemplate daringpaddedvest = null;
		
		private static ItemTemplate daringleatherboots = null;
		
		private static ItemTemplate daringleathercap = null;
		
		private static ItemTemplate daringleathergloves = null;
		
		private static ItemTemplate daringleatherjerkin = null;
		
		private static ItemTemplate daringleatherleggings = null;
		
		private static ItemTemplate daringleathersleeves = null;
		
		private static ItemTemplate daringstuddedboots = null;
		
		private static ItemTemplate daringstuddedcap = null;
		
		private static ItemTemplate daringstuddedgloves = null;
		
		private static ItemTemplate daringstuddedjerkin = null;
		
		private static ItemTemplate daringstuddedleggings = null;
		
		private static ItemTemplate daringstuddedsleeves = null;
		
		private static AbstractArea Statua = null;
		

		// Custom Initialization Code Begin
		
		// Custom Initialization Code End

		/* 
		* Constructor
		*/
		public childsplay() : base()
		{
		}

		public childsplay(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public childsplay(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public childsplay(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
	{
	}

	[ScriptLoadedEvent]
	public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
	{
	if (!ServerProperties.Properties.LOAD_QUESTS)
		return;
	if (WorldMgr.GetRegion(489).IsDisabled)
		return;
	if (log.IsInfoEnabled)
		log.Info("Quest \"" + questTitle + "\" initializing ...");

	#region defineNPCs
	GameNPC[] npcs;
	
			npcs = WorldMgr.GetNPCsByName("Statue demons breach",(eRealm) 0);
			if (npcs.Length == 0)
			{			
				Statuedemonsbreach = new DOL.GS.GameNPC();
					Statuedemonsbreach.Model = 635;
				Statuedemonsbreach.Name = "Statue demons breach";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Statuedemonsbreach.Name + ", creating ...");
				Statuedemonsbreach.GuildName = "Part of " + questTitle + " Quest";
				Statuedemonsbreach.Realm = (byte) 0;
				Statuedemonsbreach.CurrentRegionID = 489;
				Statuedemonsbreach.Size = 127;
				Statuedemonsbreach.Level = 128;
				Statuedemonsbreach.MaxSpeedBase = 191;
				Statuedemonsbreach.Faction = FactionMgr.GetFactionByID(0);
				Statuedemonsbreach.X = 27577;
				Statuedemonsbreach.Y = 39982;
				Statuedemonsbreach.Z = 14488;
				Statuedemonsbreach.Heading = 4061;
				Statuedemonsbreach.RespawnInterval = -1;
				Statuedemonsbreach.BodyType = 0;
				

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 500;
				Statuedemonsbreach.SetOwnBrain(brain);
				
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					Statuedemonsbreach.SaveIntoDatabase();
					
				Statuedemonsbreach.AddToWorld();
				
			}
			else 
			{
				Statuedemonsbreach = npcs[0];
			}
		
			npcs = WorldMgr.GetNPCsByName("Charles",(eRealm) 2);
			if (npcs.Length == 0)
			{			
				Charles = new DOL.GS.GameNPC();
					Charles.Model = 142;
				Charles.Name = "Charles";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Charles.Name + ", creating ...");
				Charles.GuildName = "Part of " + questTitle + " Quest";
				Charles.Realm = (byte) 2;
				Charles.CurrentRegionID = 100;
				Charles.Size = 37;
				Charles.Level = 1;
				Charles.MaxSpeedBase = 191;
				Charles.Faction = FactionMgr.GetFactionByID(0);
				Charles.X = 803946;
				Charles.Y = 727221;
				Charles.Z = 4680;
				Charles.Heading = 1592;
				Charles.RespawnInterval = -1;
				Charles.BodyType = 0;
				

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 500;
				Charles.SetOwnBrain(brain);
				
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					Charles.SaveIntoDatabase();
					
				Charles.AddToWorld();
				
			}
			else 
			{
				Charles = npcs[0];
			}
		

			#endregion

			#region defineItems

		daringpaddedboots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "daringpaddedboots");
			if (daringpaddedboots == null)
			{
				daringpaddedboots = new ItemTemplate();
				daringpaddedboots.Name = "Daring Padded Boots";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + daringpaddedboots.Name + ", creating it ...");
				daringpaddedboots.Level = 5;
				daringpaddedboots.Weight = 8;
				daringpaddedboots.Model = 249;
				daringpaddedboots.Object_Type = 32;
				daringpaddedboots.Item_Type = 23;
				daringpaddedboots.Id_nb = "daringpaddedboots";
				daringpaddedboots.Hand = 0;
				daringpaddedboots.Platinum = 0;
				daringpaddedboots.Gold = 0;
				daringpaddedboots.Silver = 0;
				daringpaddedboots.Copper = 0;
				daringpaddedboots.IsPickable = true;
				daringpaddedboots.IsDropable = true;
				daringpaddedboots.IsTradable = true;
				daringpaddedboots.CanDropAsLoot = false;
				daringpaddedboots.Color = 0;
				daringpaddedboots.Bonus = 0; // default bonus				
				daringpaddedboots.Bonus1 = 4;
				daringpaddedboots.Bonus1Type = (int) 3;
				daringpaddedboots.Bonus2 = 0;
				daringpaddedboots.Bonus2Type = (int) 0;
				daringpaddedboots.Bonus3 = 0;
				daringpaddedboots.Bonus3Type = (int) 0;
				daringpaddedboots.Bonus4 = 0;
				daringpaddedboots.Bonus4Type = (int) 0;
				daringpaddedboots.Bonus5 = 0;
				daringpaddedboots.Bonus5Type = (int) 0;
				daringpaddedboots.Bonus6 = 0;
				daringpaddedboots.Bonus6Type = (int) 0;
				daringpaddedboots.Bonus7 = 0;
				daringpaddedboots.Bonus7Type = (int) 0;
				daringpaddedboots.Bonus8 = 0;
				daringpaddedboots.Bonus8Type = (int) 0;
				daringpaddedboots.Bonus9 = 0;
				daringpaddedboots.Bonus9Type = (int) 0;
				daringpaddedboots.Bonus10 = 0;
				daringpaddedboots.Bonus10Type = (int) 0;
				daringpaddedboots.ExtraBonus = 0;
				daringpaddedboots.ExtraBonusType = (int) 0;
				daringpaddedboots.Effect = 0;
				daringpaddedboots.Emblem = 0;
				daringpaddedboots.Charges = 0;
				daringpaddedboots.MaxCharges = 0;
				daringpaddedboots.SpellID = 0;
				daringpaddedboots.ProcSpellID = 0;
				daringpaddedboots.Type_Damage = 0;
				daringpaddedboots.Realm = 0;
				daringpaddedboots.MaxCount = 1;
				daringpaddedboots.PackSize = 1;
				daringpaddedboots.Extension = 0;
				daringpaddedboots.Quality = 89;				
				daringpaddedboots.Condition = 100;
				daringpaddedboots.MaxCondition = 100;
				daringpaddedboots.Durability = 100;
				daringpaddedboots.MaxDurability = 100;
				daringpaddedboots.PoisonCharges = 0;
				daringpaddedboots.PoisonMaxCharges = 0;
				daringpaddedboots.PoisonSpellID = 0;
				daringpaddedboots.ProcSpellID1 = 0;
				daringpaddedboots.SpellID1 = 0;
				daringpaddedboots.MaxCharges1 = 0;
				daringpaddedboots.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(daringpaddedboots);
				}
			daringpaddedcap = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "daringpaddedcap");
			if (daringpaddedcap == null)
			{
				daringpaddedcap = new ItemTemplate();
				daringpaddedcap.Name = "Daring Padded Cap";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + daringpaddedcap.Name + ", creating it ...");
				daringpaddedcap.Level = 5;
				daringpaddedcap.Weight = 8;
				daringpaddedcap.Model = 825;
				daringpaddedcap.Object_Type = 32;
				daringpaddedcap.Item_Type = 21;
				daringpaddedcap.Id_nb = "daringpaddedcap";
				daringpaddedcap.Hand = 0;
				daringpaddedcap.Platinum = 0;
				daringpaddedcap.Gold = 0;
				daringpaddedcap.Silver = 0;
				daringpaddedcap.Copper = 0;
				daringpaddedcap.IsPickable = true;
				daringpaddedcap.IsDropable = true;
				daringpaddedcap.IsTradable = true;
				daringpaddedcap.CanDropAsLoot = false;
				daringpaddedcap.Color = 0;
				daringpaddedcap.Bonus = 0; // default bonus				
				daringpaddedcap.Bonus1 = 4;
				daringpaddedcap.Bonus1Type = (int) 2;
				daringpaddedcap.Bonus2 = 0;
				daringpaddedcap.Bonus2Type = (int) 0;
				daringpaddedcap.Bonus3 = 0;
				daringpaddedcap.Bonus3Type = (int) 0;
				daringpaddedcap.Bonus4 = 0;
				daringpaddedcap.Bonus4Type = (int) 0;
				daringpaddedcap.Bonus5 = 0;
				daringpaddedcap.Bonus5Type = (int) 0;
				daringpaddedcap.Bonus6 = 0;
				daringpaddedcap.Bonus6Type = (int) 0;
				daringpaddedcap.Bonus7 = 0;
				daringpaddedcap.Bonus7Type = (int) 0;
				daringpaddedcap.Bonus8 = 0;
				daringpaddedcap.Bonus8Type = (int) 0;
				daringpaddedcap.Bonus9 = 0;
				daringpaddedcap.Bonus9Type = (int) 0;
				daringpaddedcap.Bonus10 = 0;
				daringpaddedcap.Bonus10Type = (int) 0;
				daringpaddedcap.ExtraBonus = 0;
				daringpaddedcap.ExtraBonusType = (int) 0;
				daringpaddedcap.Effect = 0;
				daringpaddedcap.Emblem = 0;
				daringpaddedcap.Charges = 0;
				daringpaddedcap.MaxCharges = 0;
				daringpaddedcap.SpellID = 0;
				daringpaddedcap.ProcSpellID = 0;
				daringpaddedcap.Type_Damage = 0;
				daringpaddedcap.Realm = 0;
				daringpaddedcap.MaxCount = 1;
				daringpaddedcap.PackSize = 1;
				daringpaddedcap.Extension = 0;
				daringpaddedcap.Quality = 89;				
				daringpaddedcap.Condition = 100;
				daringpaddedcap.MaxCondition = 100;
				daringpaddedcap.Durability = 100;
				daringpaddedcap.MaxDurability = 100;
				daringpaddedcap.PoisonCharges = 0;
				daringpaddedcap.PoisonMaxCharges = 0;
				daringpaddedcap.PoisonSpellID = 0;
				daringpaddedcap.ProcSpellID1 = 0;
				daringpaddedcap.SpellID1 = 0;
				daringpaddedcap.MaxCharges1 = 0;
				daringpaddedcap.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(daringpaddedcap);
				}
			daringpaddedgloves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "daringpaddedgloves");
			if (daringpaddedgloves == null)
			{
				daringpaddedgloves = new ItemTemplate();
				daringpaddedgloves.Name = "Daring Padded Gloves";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + daringpaddedgloves.Name + ", creating it ...");
				daringpaddedgloves.Level = 5;
				daringpaddedgloves.Weight = 8;
				daringpaddedgloves.Model = 248;
				daringpaddedgloves.Object_Type = 32;
				daringpaddedgloves.Item_Type = 22;
				daringpaddedgloves.Id_nb = "daringpaddedgloves";
				daringpaddedgloves.Hand = 0;
				daringpaddedgloves.Platinum = 0;
				daringpaddedgloves.Gold = 0;
				daringpaddedgloves.Silver = 0;
				daringpaddedgloves.Copper = 0;
				daringpaddedgloves.IsPickable = true;
				daringpaddedgloves.IsDropable = true;
				daringpaddedgloves.IsTradable = true;
				daringpaddedgloves.CanDropAsLoot = false;
				daringpaddedgloves.Color = 0;
				daringpaddedgloves.Bonus = 0; // default bonus				
				daringpaddedgloves.Bonus1 = 4;
				daringpaddedgloves.Bonus1Type = (int) 3;
				daringpaddedgloves.Bonus2 = 0;
				daringpaddedgloves.Bonus2Type = (int) 0;
				daringpaddedgloves.Bonus3 = 0;
				daringpaddedgloves.Bonus3Type = (int) 0;
				daringpaddedgloves.Bonus4 = 0;
				daringpaddedgloves.Bonus4Type = (int) 0;
				daringpaddedgloves.Bonus5 = 0;
				daringpaddedgloves.Bonus5Type = (int) 0;
				daringpaddedgloves.Bonus6 = 0;
				daringpaddedgloves.Bonus6Type = (int) 0;
				daringpaddedgloves.Bonus7 = 0;
				daringpaddedgloves.Bonus7Type = (int) 0;
				daringpaddedgloves.Bonus8 = 0;
				daringpaddedgloves.Bonus8Type = (int) 0;
				daringpaddedgloves.Bonus9 = 0;
				daringpaddedgloves.Bonus9Type = (int) 0;
				daringpaddedgloves.Bonus10 = 0;
				daringpaddedgloves.Bonus10Type = (int) 0;
				daringpaddedgloves.ExtraBonus = 0;
				daringpaddedgloves.ExtraBonusType = (int) 0;
				daringpaddedgloves.Effect = 0;
				daringpaddedgloves.Emblem = 0;
				daringpaddedgloves.Charges = 0;
				daringpaddedgloves.MaxCharges = 0;
				daringpaddedgloves.SpellID = 0;
				daringpaddedgloves.ProcSpellID = 0;
				daringpaddedgloves.Type_Damage = 0;
				daringpaddedgloves.Realm = 0;
				daringpaddedgloves.MaxCount = 1;
				daringpaddedgloves.PackSize = 1;
				daringpaddedgloves.Extension = 0;
				daringpaddedgloves.Quality = 89;				
				daringpaddedgloves.Condition = 100;
				daringpaddedgloves.MaxCondition = 100;
				daringpaddedgloves.Durability = 100;
				daringpaddedgloves.MaxDurability = 100;
				daringpaddedgloves.PoisonCharges = 0;
				daringpaddedgloves.PoisonMaxCharges = 0;
				daringpaddedgloves.PoisonSpellID = 0;
				daringpaddedgloves.ProcSpellID1 = 0;
				daringpaddedgloves.SpellID1 = 0;
				daringpaddedgloves.MaxCharges1 = 0;
				daringpaddedgloves.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(daringpaddedgloves);
				}
			daringpaddedpants = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "daringpaddedpants");
			if (daringpaddedpants == null)
			{
				daringpaddedpants = new ItemTemplate();
				daringpaddedpants.Name = "Daring Padded Pants";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + daringpaddedpants.Name + ", creating it ...");
				daringpaddedpants.Level = 5;
				daringpaddedpants.Weight = 14;
				daringpaddedpants.Model = 246;
				daringpaddedpants.Object_Type = 32;
				daringpaddedpants.Item_Type = 27;
				daringpaddedpants.Id_nb = "daringpaddedpants";
				daringpaddedpants.Hand = 0;
				daringpaddedpants.Platinum = 0;
				daringpaddedpants.Gold = 0;
				daringpaddedpants.Silver = 0;
				daringpaddedpants.Copper = 0;
				daringpaddedpants.IsPickable = true;
				daringpaddedpants.IsDropable = true;
				daringpaddedpants.IsTradable = true;
				daringpaddedpants.CanDropAsLoot = false;
				daringpaddedpants.Color = 0;
				daringpaddedpants.Bonus = 0; // default bonus				
				daringpaddedpants.Bonus1 = 4;
				daringpaddedpants.Bonus1Type = (int) 2;
				daringpaddedpants.Bonus2 = 0;
				daringpaddedpants.Bonus2Type = (int) 0;
				daringpaddedpants.Bonus3 = 0;
				daringpaddedpants.Bonus3Type = (int) 0;
				daringpaddedpants.Bonus4 = 0;
				daringpaddedpants.Bonus4Type = (int) 0;
				daringpaddedpants.Bonus5 = 0;
				daringpaddedpants.Bonus5Type = (int) 0;
				daringpaddedpants.Bonus6 = 0;
				daringpaddedpants.Bonus6Type = (int) 0;
				daringpaddedpants.Bonus7 = 0;
				daringpaddedpants.Bonus7Type = (int) 0;
				daringpaddedpants.Bonus8 = 0;
				daringpaddedpants.Bonus8Type = (int) 0;
				daringpaddedpants.Bonus9 = 0;
				daringpaddedpants.Bonus9Type = (int) 0;
				daringpaddedpants.Bonus10 = 0;
				daringpaddedpants.Bonus10Type = (int) 0;
				daringpaddedpants.ExtraBonus = 0;
				daringpaddedpants.ExtraBonusType = (int) 0;
				daringpaddedpants.Effect = 0;
				daringpaddedpants.Emblem = 0;
				daringpaddedpants.Charges = 0;
				daringpaddedpants.MaxCharges = 0;
				daringpaddedpants.SpellID = 0;
				daringpaddedpants.ProcSpellID = 0;
				daringpaddedpants.Type_Damage = 0;
				daringpaddedpants.Realm = 0;
				daringpaddedpants.MaxCount = 1;
				daringpaddedpants.PackSize = 1;
				daringpaddedpants.Extension = 0;
				daringpaddedpants.Quality = 89;				
				daringpaddedpants.Condition = 100;
				daringpaddedpants.MaxCondition = 100;
				daringpaddedpants.Durability = 100;
				daringpaddedpants.MaxDurability = 100;
				daringpaddedpants.PoisonCharges = 0;
				daringpaddedpants.PoisonMaxCharges = 0;
				daringpaddedpants.PoisonSpellID = 0;
				daringpaddedpants.ProcSpellID1 = 0;
				daringpaddedpants.SpellID1 = 0;
				daringpaddedpants.MaxCharges1 = 0;
				daringpaddedpants.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(daringpaddedpants);
				}
			daringpaddedsleeves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "daringpaddedsleeves");
			if (daringpaddedsleeves == null)
			{
				daringpaddedsleeves = new ItemTemplate();
				daringpaddedsleeves.Name = "Daring Padded Sleeves";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + daringpaddedsleeves.Name + ", creating it ...");
				daringpaddedsleeves.Level = 5;
				daringpaddedsleeves.Weight = 12;
				daringpaddedsleeves.Model = 247;
				daringpaddedsleeves.Object_Type = 32;
				daringpaddedsleeves.Item_Type = 28;
				daringpaddedsleeves.Id_nb = "daringpaddedsleeves";
				daringpaddedsleeves.Hand = 0;
				daringpaddedsleeves.Platinum = 0;
				daringpaddedsleeves.Gold = 0;
				daringpaddedsleeves.Silver = 0;
				daringpaddedsleeves.Copper = 0;
				daringpaddedsleeves.IsPickable = true;
				daringpaddedsleeves.IsDropable = true;
				daringpaddedsleeves.IsTradable = true;
				daringpaddedsleeves.CanDropAsLoot = false;
				daringpaddedsleeves.Color = 0;
				daringpaddedsleeves.Bonus = 0; // default bonus				
				daringpaddedsleeves.Bonus1 = 4;
				daringpaddedsleeves.Bonus1Type = (int) 6;
				daringpaddedsleeves.Bonus2 = 0;
				daringpaddedsleeves.Bonus2Type = (int) 0;
				daringpaddedsleeves.Bonus3 = 0;
				daringpaddedsleeves.Bonus3Type = (int) 0;
				daringpaddedsleeves.Bonus4 = 0;
				daringpaddedsleeves.Bonus4Type = (int) 0;
				daringpaddedsleeves.Bonus5 = 0;
				daringpaddedsleeves.Bonus5Type = (int) 0;
				daringpaddedsleeves.Bonus6 = 0;
				daringpaddedsleeves.Bonus6Type = (int) 0;
				daringpaddedsleeves.Bonus7 = 0;
				daringpaddedsleeves.Bonus7Type = (int) 0;
				daringpaddedsleeves.Bonus8 = 0;
				daringpaddedsleeves.Bonus8Type = (int) 0;
				daringpaddedsleeves.Bonus9 = 0;
				daringpaddedsleeves.Bonus9Type = (int) 0;
				daringpaddedsleeves.Bonus10 = 0;
				daringpaddedsleeves.Bonus10Type = (int) 0;
				daringpaddedsleeves.ExtraBonus = 0;
				daringpaddedsleeves.ExtraBonusType = (int) 0;
				daringpaddedsleeves.Effect = 0;
				daringpaddedsleeves.Emblem = 0;
				daringpaddedsleeves.Charges = 0;
				daringpaddedsleeves.MaxCharges = 0;
				daringpaddedsleeves.SpellID = 0;
				daringpaddedsleeves.ProcSpellID = 0;
				daringpaddedsleeves.Type_Damage = 0;
				daringpaddedsleeves.Realm = 0;
				daringpaddedsleeves.MaxCount = 1;
				daringpaddedsleeves.PackSize = 1;
				daringpaddedsleeves.Extension = 0;
				daringpaddedsleeves.Quality = 89;				
				daringpaddedsleeves.Condition = 100;
				daringpaddedsleeves.MaxCondition = 100;
				daringpaddedsleeves.Durability = 100;
				daringpaddedsleeves.MaxDurability = 100;
				daringpaddedsleeves.PoisonCharges = 0;
				daringpaddedsleeves.PoisonMaxCharges = 0;
				daringpaddedsleeves.PoisonSpellID = 0;
				daringpaddedsleeves.ProcSpellID1 = 0;
				daringpaddedsleeves.SpellID1 = 0;
				daringpaddedsleeves.MaxCharges1 = 0;
				daringpaddedsleeves.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(daringpaddedsleeves);
				}
			daringpaddedvest = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "daringpaddedvest");
			if (daringpaddedvest == null)
			{
				daringpaddedvest = new ItemTemplate();
				daringpaddedvest.Name = "Daring Padded Vest";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + daringpaddedvest.Name + ", creating it ...");
				daringpaddedvest.Level = 5;
				daringpaddedvest.Weight = 20;
				daringpaddedvest.Model = 245;
				daringpaddedvest.Object_Type = 32;
				daringpaddedvest.Item_Type = 25;
				daringpaddedvest.Id_nb = "daringpaddedvest";
				daringpaddedvest.Hand = 0;
				daringpaddedvest.Platinum = 0;
				daringpaddedvest.Gold = 0;
				daringpaddedvest.Silver = 0;
				daringpaddedvest.Copper = 0;
				daringpaddedvest.IsPickable = true;
				daringpaddedvest.IsDropable = true;
				daringpaddedvest.IsTradable = true;
				daringpaddedvest.CanDropAsLoot = false;
				daringpaddedvest.Color = 0;
				daringpaddedvest.Bonus = 0; // default bonus				
				daringpaddedvest.Bonus1 = 12;
				daringpaddedvest.Bonus1Type = (int) 10;
				daringpaddedvest.Bonus2 = 0;
				daringpaddedvest.Bonus2Type = (int) 0;
				daringpaddedvest.Bonus3 = 0;
				daringpaddedvest.Bonus3Type = (int) 0;
				daringpaddedvest.Bonus4 = 0;
				daringpaddedvest.Bonus4Type = (int) 0;
				daringpaddedvest.Bonus5 = 0;
				daringpaddedvest.Bonus5Type = (int) 0;
				daringpaddedvest.Bonus6 = 0;
				daringpaddedvest.Bonus6Type = (int) 0;
				daringpaddedvest.Bonus7 = 0;
				daringpaddedvest.Bonus7Type = (int) 0;
				daringpaddedvest.Bonus8 = 0;
				daringpaddedvest.Bonus8Type = (int) 0;
				daringpaddedvest.Bonus9 = 0;
				daringpaddedvest.Bonus9Type = (int) 0;
				daringpaddedvest.Bonus10 = 0;
				daringpaddedvest.Bonus10Type = (int) 0;
				daringpaddedvest.ExtraBonus = 0;
				daringpaddedvest.ExtraBonusType = (int) 0;
				daringpaddedvest.Effect = 0;
				daringpaddedvest.Emblem = 0;
				daringpaddedvest.Charges = 0;
				daringpaddedvest.MaxCharges = 0;
				daringpaddedvest.SpellID = 0;
				daringpaddedvest.ProcSpellID = 0;
				daringpaddedvest.Type_Damage = 0;
				daringpaddedvest.Realm = 0;
				daringpaddedvest.MaxCount = 1;
				daringpaddedvest.PackSize = 1;
				daringpaddedvest.Extension = 0;
				daringpaddedvest.Quality = 89;				
				daringpaddedvest.Condition = 100;
				daringpaddedvest.MaxCondition = 100;
				daringpaddedvest.Durability = 100;
				daringpaddedvest.MaxDurability = 100;
				daringpaddedvest.PoisonCharges = 0;
				daringpaddedvest.PoisonMaxCharges = 0;
				daringpaddedvest.PoisonSpellID = 0;
				daringpaddedvest.ProcSpellID1 = 0;
				daringpaddedvest.SpellID1 = 0;
				daringpaddedvest.MaxCharges1 = 0;
				daringpaddedvest.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(daringpaddedvest);
				}
			daringleatherboots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "daringleatherboots");
			if (daringleatherboots == null)
			{
				daringleatherboots = new ItemTemplate();
				daringleatherboots.Name = "Daring Leather Boots";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + daringleatherboots.Name + ", creating it ...");
				daringleatherboots.Level = 5;
				daringleatherboots.Weight = 16;
				daringleatherboots.Model = 284;
				daringleatherboots.Object_Type = 33;
				daringleatherboots.Item_Type = 23;
				daringleatherboots.Id_nb = "daringleatherboots";
				daringleatherboots.Hand = 0;
				daringleatherboots.Platinum = 0;
				daringleatherboots.Gold = 0;
				daringleatherboots.Silver = 0;
				daringleatherboots.Copper = 0;
				daringleatherboots.IsPickable = true;
				daringleatherboots.IsDropable = true;
				daringleatherboots.IsTradable = true;
				daringleatherboots.CanDropAsLoot = false;
				daringleatherboots.Color = 0;
				daringleatherboots.Bonus = 0; // default bonus				
				daringleatherboots.Bonus1 = 4;
				daringleatherboots.Bonus1Type = (int) 3;
				daringleatherboots.Bonus2 = 0;
				daringleatherboots.Bonus2Type = (int) 0;
				daringleatherboots.Bonus3 = 0;
				daringleatherboots.Bonus3Type = (int) 0;
				daringleatherboots.Bonus4 = 0;
				daringleatherboots.Bonus4Type = (int) 0;
				daringleatherboots.Bonus5 = 0;
				daringleatherboots.Bonus5Type = (int) 0;
				daringleatherboots.Bonus6 = 0;
				daringleatherboots.Bonus6Type = (int) 0;
				daringleatherboots.Bonus7 = 0;
				daringleatherboots.Bonus7Type = (int) 0;
				daringleatherboots.Bonus8 = 0;
				daringleatherboots.Bonus8Type = (int) 0;
				daringleatherboots.Bonus9 = 0;
				daringleatherboots.Bonus9Type = (int) 0;
				daringleatherboots.Bonus10 = 0;
				daringleatherboots.Bonus10Type = (int) 0;
				daringleatherboots.ExtraBonus = 0;
				daringleatherboots.ExtraBonusType = (int) 0;
				daringleatherboots.Effect = 0;
				daringleatherboots.Emblem = 0;
				daringleatherboots.Charges = 0;
				daringleatherboots.MaxCharges = 0;
				daringleatherboots.SpellID = 0;
				daringleatherboots.ProcSpellID = 0;
				daringleatherboots.Type_Damage = 0;
				daringleatherboots.Realm = 0;
				daringleatherboots.MaxCount = 1;
				daringleatherboots.PackSize = 1;
				daringleatherboots.Extension = 0;
				daringleatherboots.Quality = 89;				
				daringleatherboots.Condition = 100;
				daringleatherboots.MaxCondition = 100;
				daringleatherboots.Durability = 100;
				daringleatherboots.MaxDurability = 100;
				daringleatherboots.PoisonCharges = 0;
				daringleatherboots.PoisonMaxCharges = 0;
				daringleatherboots.PoisonSpellID = 0;
				daringleatherboots.ProcSpellID1 = 0;
				daringleatherboots.SpellID1 = 0;
				daringleatherboots.MaxCharges1 = 0;
				daringleatherboots.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(daringleatherboots);
				}
			daringleathercap = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "daringleathercap");
			if (daringleathercap == null)
			{
				daringleathercap = new ItemTemplate();
				daringleathercap.Name = "Daring Leather Cap";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + daringleathercap.Name + ", creating it ...");
				daringleathercap.Level = 5;
				daringleathercap.Weight = 16;
				daringleathercap.Model = 337;
				daringleathercap.Object_Type = 33;
				daringleathercap.Item_Type = 21;
				daringleathercap.Id_nb = "daringleathercap";
				daringleathercap.Hand = 0;
				daringleathercap.Platinum = 0;
				daringleathercap.Gold = 0;
				daringleathercap.Silver = 0;
				daringleathercap.Copper = 0;
				daringleathercap.IsPickable = true;
				daringleathercap.IsDropable = true;
				daringleathercap.IsTradable = true;
				daringleathercap.CanDropAsLoot = false;
				daringleathercap.Color = 0;
				daringleathercap.Bonus = 0; // default bonus				
				daringleathercap.Bonus1 = 4;
				daringleathercap.Bonus1Type = (int) 3;
				daringleathercap.Bonus2 = 0;
				daringleathercap.Bonus2Type = (int) 0;
				daringleathercap.Bonus3 = 0;
				daringleathercap.Bonus3Type = (int) 0;
				daringleathercap.Bonus4 = 0;
				daringleathercap.Bonus4Type = (int) 0;
				daringleathercap.Bonus5 = 0;
				daringleathercap.Bonus5Type = (int) 0;
				daringleathercap.Bonus6 = 0;
				daringleathercap.Bonus6Type = (int) 0;
				daringleathercap.Bonus7 = 0;
				daringleathercap.Bonus7Type = (int) 0;
				daringleathercap.Bonus8 = 0;
				daringleathercap.Bonus8Type = (int) 0;
				daringleathercap.Bonus9 = 0;
				daringleathercap.Bonus9Type = (int) 0;
				daringleathercap.Bonus10 = 0;
				daringleathercap.Bonus10Type = (int) 0;
				daringleathercap.ExtraBonus = 0;
				daringleathercap.ExtraBonusType = (int) 0;
				daringleathercap.Effect = 0;
				daringleathercap.Emblem = 0;
				daringleathercap.Charges = 0;
				daringleathercap.MaxCharges = 0;
				daringleathercap.SpellID = 0;
				daringleathercap.ProcSpellID = 0;
				daringleathercap.Type_Damage = 0;
				daringleathercap.Realm = 0;
				daringleathercap.MaxCount = 1;
				daringleathercap.PackSize = 1;
				daringleathercap.Extension = 0;
				daringleathercap.Quality = 89;				
				daringleathercap.Condition = 100;
				daringleathercap.MaxCondition = 100;
				daringleathercap.Durability = 100;
				daringleathercap.MaxDurability = 100;
				daringleathercap.PoisonCharges = 0;
				daringleathercap.PoisonMaxCharges = 0;
				daringleathercap.PoisonSpellID = 0;
				daringleathercap.ProcSpellID1 = 0;
				daringleathercap.SpellID1 = 0;
				daringleathercap.MaxCharges1 = 0;
				daringleathercap.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(daringleathercap);
				}
			daringleathergloves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "daringleathergloves");
			if (daringleathergloves == null)
			{
				daringleathergloves = new ItemTemplate();
				daringleathergloves.Name = "Daring Leather Gloves";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + daringleathergloves.Name + ", creating it ...");
				daringleathergloves.Level = 5;
				daringleathergloves.Weight = 16;
				daringleathergloves.Model = 283;
				daringleathergloves.Object_Type = 33;
				daringleathergloves.Item_Type = 22;
				daringleathergloves.Id_nb = "daringleathergloves";
				daringleathergloves.Hand = 0;
				daringleathergloves.Platinum = 0;
				daringleathergloves.Gold = 0;
				daringleathergloves.Silver = 0;
				daringleathergloves.Copper = 0;
				daringleathergloves.IsPickable = true;
				daringleathergloves.IsDropable = true;
				daringleathergloves.IsTradable = true;
				daringleathergloves.CanDropAsLoot = false;
				daringleathergloves.Color = 0;
				daringleathergloves.Bonus = 0; // default bonus				
				daringleathergloves.Bonus1 = 4;
				daringleathergloves.Bonus1Type = (int) 2;
				daringleathergloves.Bonus2 = 0;
				daringleathergloves.Bonus2Type = (int) 0;
				daringleathergloves.Bonus3 = 0;
				daringleathergloves.Bonus3Type = (int) 0;
				daringleathergloves.Bonus4 = 0;
				daringleathergloves.Bonus4Type = (int) 0;
				daringleathergloves.Bonus5 = 0;
				daringleathergloves.Bonus5Type = (int) 0;
				daringleathergloves.Bonus6 = 0;
				daringleathergloves.Bonus6Type = (int) 0;
				daringleathergloves.Bonus7 = 0;
				daringleathergloves.Bonus7Type = (int) 0;
				daringleathergloves.Bonus8 = 0;
				daringleathergloves.Bonus8Type = (int) 0;
				daringleathergloves.Bonus9 = 0;
				daringleathergloves.Bonus9Type = (int) 0;
				daringleathergloves.Bonus10 = 0;
				daringleathergloves.Bonus10Type = (int) 0;
				daringleathergloves.ExtraBonus = 0;
				daringleathergloves.ExtraBonusType = (int) 0;
				daringleathergloves.Effect = 0;
				daringleathergloves.Emblem = 0;
				daringleathergloves.Charges = 0;
				daringleathergloves.MaxCharges = 0;
				daringleathergloves.SpellID = 0;
				daringleathergloves.ProcSpellID = 0;
				daringleathergloves.Type_Damage = 0;
				daringleathergloves.Realm = 0;
				daringleathergloves.MaxCount = 1;
				daringleathergloves.PackSize = 1;
				daringleathergloves.Extension = 0;
				daringleathergloves.Quality = 89;				
				daringleathergloves.Condition = 100;
				daringleathergloves.MaxCondition = 100;
				daringleathergloves.Durability = 100;
				daringleathergloves.MaxDurability = 100;
				daringleathergloves.PoisonCharges = 0;
				daringleathergloves.PoisonMaxCharges = 0;
				daringleathergloves.PoisonSpellID = 0;
				daringleathergloves.ProcSpellID1 = 0;
				daringleathergloves.SpellID1 = 0;
				daringleathergloves.MaxCharges1 = 0;
				daringleathergloves.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(daringleathergloves);
				}
			daringleatherjerkin = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "daringleatherjerkin");
			if (daringleatherjerkin == null)
			{
				daringleatherjerkin = new ItemTemplate();
				daringleatherjerkin.Name = "Daring Leather Jerkin";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + daringleatherjerkin.Name + ", creating it ...");
				daringleatherjerkin.Level = 5;
				daringleatherjerkin.Weight = 16;
				daringleatherjerkin.Model = 280;
				daringleatherjerkin.Object_Type = 33;
				daringleatherjerkin.Item_Type = 25;
				daringleatherjerkin.Id_nb = "daringleatherjerkin";
				daringleatherjerkin.Hand = 0;
				daringleatherjerkin.Platinum = 0;
				daringleatherjerkin.Gold = 0;
				daringleatherjerkin.Silver = 0;
				daringleatherjerkin.Copper = 0;
				daringleatherjerkin.IsPickable = true;
				daringleatherjerkin.IsDropable = true;
				daringleatherjerkin.IsTradable = true;
				daringleatherjerkin.CanDropAsLoot = false;
				daringleatherjerkin.Color = 0;
				daringleatherjerkin.Bonus = 0; // default bonus				
				daringleatherjerkin.Bonus1 = 12;
				daringleatherjerkin.Bonus1Type = (int) 10;
				daringleatherjerkin.Bonus2 = 0;
				daringleatherjerkin.Bonus2Type = (int) 0;
				daringleatherjerkin.Bonus3 = 0;
				daringleatherjerkin.Bonus3Type = (int) 0;
				daringleatherjerkin.Bonus4 = 0;
				daringleatherjerkin.Bonus4Type = (int) 0;
				daringleatherjerkin.Bonus5 = 0;
				daringleatherjerkin.Bonus5Type = (int) 0;
				daringleatherjerkin.Bonus6 = 0;
				daringleatherjerkin.Bonus6Type = (int) 0;
				daringleatherjerkin.Bonus7 = 0;
				daringleatherjerkin.Bonus7Type = (int) 0;
				daringleatherjerkin.Bonus8 = 0;
				daringleatherjerkin.Bonus8Type = (int) 0;
				daringleatherjerkin.Bonus9 = 0;
				daringleatherjerkin.Bonus9Type = (int) 0;
				daringleatherjerkin.Bonus10 = 0;
				daringleatherjerkin.Bonus10Type = (int) 0;
				daringleatherjerkin.ExtraBonus = 0;
				daringleatherjerkin.ExtraBonusType = (int) 0;
				daringleatherjerkin.Effect = 0;
				daringleatherjerkin.Emblem = 0;
				daringleatherjerkin.Charges = 0;
				daringleatherjerkin.MaxCharges = 0;
				daringleatherjerkin.SpellID = 0;
				daringleatherjerkin.ProcSpellID = 0;
				daringleatherjerkin.Type_Damage = 0;
				daringleatherjerkin.Realm = 0;
				daringleatherjerkin.MaxCount = 1;
				daringleatherjerkin.PackSize = 1;
				daringleatherjerkin.Extension = 0;
				daringleatherjerkin.Quality = 89;				
				daringleatherjerkin.Condition = 100;
				daringleatherjerkin.MaxCondition = 100;
				daringleatherjerkin.Durability = 100;
				daringleatherjerkin.MaxDurability = 100;
				daringleatherjerkin.PoisonCharges = 0;
				daringleatherjerkin.PoisonMaxCharges = 0;
				daringleatherjerkin.PoisonSpellID = 0;
				daringleatherjerkin.ProcSpellID1 = 0;
				daringleatherjerkin.SpellID1 = 0;
				daringleatherjerkin.MaxCharges1 = 0;
				daringleatherjerkin.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(daringleatherjerkin);
				}
			daringleatherleggings = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "daringleatherleggings");
			if (daringleatherleggings == null)
			{
				daringleatherleggings = new ItemTemplate();
				daringleatherleggings.Name = "Daring Leather Leggings";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + daringleatherleggings.Name + ", creating it ...");
				daringleatherleggings.Level = 5;
				daringleatherleggings.Weight = 16;
				daringleatherleggings.Model = 281;
				daringleatherleggings.Object_Type = 33;
				daringleatherleggings.Item_Type = 27;
				daringleatherleggings.Id_nb = "daringleatherleggings";
				daringleatherleggings.Hand = 0;
				daringleatherleggings.Platinum = 0;
				daringleatherleggings.Gold = 0;
				daringleatherleggings.Silver = 0;
				daringleatherleggings.Copper = 0;
				daringleatherleggings.IsPickable = true;
				daringleatherleggings.IsDropable = true;
				daringleatherleggings.IsTradable = true;
				daringleatherleggings.CanDropAsLoot = false;
				daringleatherleggings.Color = 0;
				daringleatherleggings.Bonus = 0; // default bonus				
				daringleatherleggings.Bonus1 = 4;
				daringleatherleggings.Bonus1Type = (int) 2;
				daringleatherleggings.Bonus2 = 0;
				daringleatherleggings.Bonus2Type = (int) 0;
				daringleatherleggings.Bonus3 = 0;
				daringleatherleggings.Bonus3Type = (int) 0;
				daringleatherleggings.Bonus4 = 0;
				daringleatherleggings.Bonus4Type = (int) 0;
				daringleatherleggings.Bonus5 = 0;
				daringleatherleggings.Bonus5Type = (int) 0;
				daringleatherleggings.Bonus6 = 0;
				daringleatherleggings.Bonus6Type = (int) 0;
				daringleatherleggings.Bonus7 = 0;
				daringleatherleggings.Bonus7Type = (int) 0;
				daringleatherleggings.Bonus8 = 0;
				daringleatherleggings.Bonus8Type = (int) 0;
				daringleatherleggings.Bonus9 = 0;
				daringleatherleggings.Bonus9Type = (int) 0;
				daringleatherleggings.Bonus10 = 0;
				daringleatherleggings.Bonus10Type = (int) 0;
				daringleatherleggings.ExtraBonus = 0;
				daringleatherleggings.ExtraBonusType = (int) 0;
				daringleatherleggings.Effect = 0;
				daringleatherleggings.Emblem = 0;
				daringleatherleggings.Charges = 0;
				daringleatherleggings.MaxCharges = 0;
				daringleatherleggings.SpellID = 0;
				daringleatherleggings.ProcSpellID = 0;
				daringleatherleggings.Type_Damage = 0;
				daringleatherleggings.Realm = 0;
				daringleatherleggings.MaxCount = 1;
				daringleatherleggings.PackSize = 1;
				daringleatherleggings.Extension = 0;
				daringleatherleggings.Quality = 89;				
				daringleatherleggings.Condition = 100;
				daringleatherleggings.MaxCondition = 100;
				daringleatherleggings.Durability = 100;
				daringleatherleggings.MaxDurability = 100;
				daringleatherleggings.PoisonCharges = 0;
				daringleatherleggings.PoisonMaxCharges = 0;
				daringleatherleggings.PoisonSpellID = 0;
				daringleatherleggings.ProcSpellID1 = 0;
				daringleatherleggings.SpellID1 = 0;
				daringleatherleggings.MaxCharges1 = 0;
				daringleatherleggings.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(daringleatherleggings);
				}
			daringleathersleeves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "daringleathersleeves");
			if (daringleathersleeves == null)
			{
				daringleathersleeves = new ItemTemplate();
				daringleathersleeves.Name = "Daring Leather Sleeves";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + daringleathersleeves.Name + ", creating it ...");
				daringleathersleeves.Level = 5;
				daringleathersleeves.Weight = 16;
				daringleathersleeves.Model = 282;
				daringleathersleeves.Object_Type = 33;
				daringleathersleeves.Item_Type = 28;
				daringleathersleeves.Id_nb = "daringleathersleeves";
				daringleathersleeves.Hand = 0;
				daringleathersleeves.Platinum = 0;
				daringleathersleeves.Gold = 0;
				daringleathersleeves.Silver = 0;
				daringleathersleeves.Copper = 0;
				daringleathersleeves.IsPickable = true;
				daringleathersleeves.IsDropable = true;
				daringleathersleeves.IsTradable = true;
				daringleathersleeves.CanDropAsLoot = false;
				daringleathersleeves.Color = 0;
				daringleathersleeves.Bonus = 0; // default bonus				
				daringleathersleeves.Bonus1 = 4;
				daringleathersleeves.Bonus1Type = (int) 1;
				daringleathersleeves.Bonus2 = 0;
				daringleathersleeves.Bonus2Type = (int) 0;
				daringleathersleeves.Bonus3 = 0;
				daringleathersleeves.Bonus3Type = (int) 0;
				daringleathersleeves.Bonus4 = 0;
				daringleathersleeves.Bonus4Type = (int) 0;
				daringleathersleeves.Bonus5 = 0;
				daringleathersleeves.Bonus5Type = (int) 0;
				daringleathersleeves.Bonus6 = 0;
				daringleathersleeves.Bonus6Type = (int) 0;
				daringleathersleeves.Bonus7 = 0;
				daringleathersleeves.Bonus7Type = (int) 0;
				daringleathersleeves.Bonus8 = 0;
				daringleathersleeves.Bonus8Type = (int) 0;
				daringleathersleeves.Bonus9 = 0;
				daringleathersleeves.Bonus9Type = (int) 0;
				daringleathersleeves.Bonus10 = 0;
				daringleathersleeves.Bonus10Type = (int) 0;
				daringleathersleeves.ExtraBonus = 0;
				daringleathersleeves.ExtraBonusType = (int) 0;
				daringleathersleeves.Effect = 0;
				daringleathersleeves.Emblem = 0;
				daringleathersleeves.Charges = 0;
				daringleathersleeves.MaxCharges = 0;
				daringleathersleeves.SpellID = 0;
				daringleathersleeves.ProcSpellID = 0;
				daringleathersleeves.Type_Damage = 0;
				daringleathersleeves.Realm = 0;
				daringleathersleeves.MaxCount = 1;
				daringleathersleeves.PackSize = 1;
				daringleathersleeves.Extension = 0;
				daringleathersleeves.Quality = 89;				
				daringleathersleeves.Condition = 100;
				daringleathersleeves.MaxCondition = 100;
				daringleathersleeves.Durability = 100;
				daringleathersleeves.MaxDurability = 100;
				daringleathersleeves.PoisonCharges = 0;
				daringleathersleeves.PoisonMaxCharges = 0;
				daringleathersleeves.PoisonSpellID = 0;
				daringleathersleeves.ProcSpellID1 = 0;
				daringleathersleeves.SpellID1 = 0;
				daringleathersleeves.MaxCharges1 = 0;
				daringleathersleeves.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(daringleathersleeves);
				}
			daringstuddedboots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "daringstuddedboots");
			if (daringstuddedboots == null)
			{
				daringstuddedboots = new ItemTemplate();
				daringstuddedboots.Name = "Daring Studded Boots";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + daringstuddedboots.Name + ", creating it ...");
				daringstuddedboots.Level = 5;
				daringstuddedboots.Weight = 24;
				daringstuddedboots.Model = 234;
				daringstuddedboots.Object_Type = 34;
				daringstuddedboots.Item_Type = 23;
				daringstuddedboots.Id_nb = "daringstuddedboots";
				daringstuddedboots.Hand = 0;
				daringstuddedboots.Platinum = 0;
				daringstuddedboots.Gold = 0;
				daringstuddedboots.Silver = 0;
				daringstuddedboots.Copper = 0;
				daringstuddedboots.IsPickable = true;
				daringstuddedboots.IsDropable = true;
				daringstuddedboots.IsTradable = true;
				daringstuddedboots.CanDropAsLoot = false;
				daringstuddedboots.Color = 0;
				daringstuddedboots.Bonus = 0; // default bonus				
				daringstuddedboots.Bonus1 = 4;
				daringstuddedboots.Bonus1Type = (int) 3;
				daringstuddedboots.Bonus2 = 0;
				daringstuddedboots.Bonus2Type = (int) 0;
				daringstuddedboots.Bonus3 = 0;
				daringstuddedboots.Bonus3Type = (int) 0;
				daringstuddedboots.Bonus4 = 0;
				daringstuddedboots.Bonus4Type = (int) 0;
				daringstuddedboots.Bonus5 = 0;
				daringstuddedboots.Bonus5Type = (int) 0;
				daringstuddedboots.Bonus6 = 0;
				daringstuddedboots.Bonus6Type = (int) 0;
				daringstuddedboots.Bonus7 = 0;
				daringstuddedboots.Bonus7Type = (int) 0;
				daringstuddedboots.Bonus8 = 0;
				daringstuddedboots.Bonus8Type = (int) 0;
				daringstuddedboots.Bonus9 = 0;
				daringstuddedboots.Bonus9Type = (int) 0;
				daringstuddedboots.Bonus10 = 0;
				daringstuddedboots.Bonus10Type = (int) 0;
				daringstuddedboots.ExtraBonus = 0;
				daringstuddedboots.ExtraBonusType = (int) 0;
				daringstuddedboots.Effect = 0;
				daringstuddedboots.Emblem = 0;
				daringstuddedboots.Charges = 0;
				daringstuddedboots.MaxCharges = 0;
				daringstuddedboots.SpellID = 0;
				daringstuddedboots.ProcSpellID = 0;
				daringstuddedboots.Type_Damage = 0;
				daringstuddedboots.Realm = 0;
				daringstuddedboots.MaxCount = 1;
				daringstuddedboots.PackSize = 1;
				daringstuddedboots.Extension = 0;
				daringstuddedboots.Quality = 89;				
				daringstuddedboots.Condition = 100;
				daringstuddedboots.MaxCondition = 100;
				daringstuddedboots.Durability = 100;
				daringstuddedboots.MaxDurability = 100;
				daringstuddedboots.PoisonCharges = 0;
				daringstuddedboots.PoisonMaxCharges = 0;
				daringstuddedboots.PoisonSpellID = 0;
				daringstuddedboots.ProcSpellID1 = 0;
				daringstuddedboots.SpellID1 = 0;
				daringstuddedboots.MaxCharges1 = 0;
				daringstuddedboots.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(daringstuddedboots);
				}
			daringstuddedcap = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "daringstuddedcap");
			if (daringstuddedcap == null)
			{
				daringstuddedcap = new ItemTemplate();
				daringstuddedcap.Name = "Daring Studded Cap";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + daringstuddedcap.Name + ", creating it ...");
				daringstuddedcap.Level = 5;
				daringstuddedcap.Weight = 24;
				daringstuddedcap.Model = 829;
				daringstuddedcap.Object_Type = 34;
				daringstuddedcap.Item_Type = 21;
				daringstuddedcap.Id_nb = "daringstuddedcap";
				daringstuddedcap.Hand = 0;
				daringstuddedcap.Platinum = 0;
				daringstuddedcap.Gold = 0;
				daringstuddedcap.Silver = 0;
				daringstuddedcap.Copper = 0;
				daringstuddedcap.IsPickable = true;
				daringstuddedcap.IsDropable = true;
				daringstuddedcap.IsTradable = true;
				daringstuddedcap.CanDropAsLoot = false;
				daringstuddedcap.Color = 0;
				daringstuddedcap.Bonus = 0; // default bonus				
				daringstuddedcap.Bonus1 = 4;
				daringstuddedcap.Bonus1Type = (int) 3;
				daringstuddedcap.Bonus2 = 0;
				daringstuddedcap.Bonus2Type = (int) 0;
				daringstuddedcap.Bonus3 = 0;
				daringstuddedcap.Bonus3Type = (int) 0;
				daringstuddedcap.Bonus4 = 0;
				daringstuddedcap.Bonus4Type = (int) 0;
				daringstuddedcap.Bonus5 = 0;
				daringstuddedcap.Bonus5Type = (int) 0;
				daringstuddedcap.Bonus6 = 0;
				daringstuddedcap.Bonus6Type = (int) 0;
				daringstuddedcap.Bonus7 = 0;
				daringstuddedcap.Bonus7Type = (int) 0;
				daringstuddedcap.Bonus8 = 0;
				daringstuddedcap.Bonus8Type = (int) 0;
				daringstuddedcap.Bonus9 = 0;
				daringstuddedcap.Bonus9Type = (int) 0;
				daringstuddedcap.Bonus10 = 0;
				daringstuddedcap.Bonus10Type = (int) 0;
				daringstuddedcap.ExtraBonus = 0;
				daringstuddedcap.ExtraBonusType = (int) 0;
				daringstuddedcap.Effect = 0;
				daringstuddedcap.Emblem = 0;
				daringstuddedcap.Charges = 0;
				daringstuddedcap.MaxCharges = 0;
				daringstuddedcap.SpellID = 0;
				daringstuddedcap.ProcSpellID = 0;
				daringstuddedcap.Type_Damage = 0;
				daringstuddedcap.Realm = 0;
				daringstuddedcap.MaxCount = 1;
				daringstuddedcap.PackSize = 1;
				daringstuddedcap.Extension = 0;
				daringstuddedcap.Quality = 89;				
				daringstuddedcap.Condition = 100;
				daringstuddedcap.MaxCondition = 100;
				daringstuddedcap.Durability = 100;
				daringstuddedcap.MaxDurability = 100;
				daringstuddedcap.PoisonCharges = 0;
				daringstuddedcap.PoisonMaxCharges = 0;
				daringstuddedcap.PoisonSpellID = 0;
				daringstuddedcap.ProcSpellID1 = 0;
				daringstuddedcap.SpellID1 = 0;
				daringstuddedcap.MaxCharges1 = 0;
				daringstuddedcap.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(daringstuddedcap);
				}
			daringstuddedgloves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "daringstuddedgloves");
			if (daringstuddedgloves == null)
			{
				daringstuddedgloves = new ItemTemplate();
				daringstuddedgloves.Name = "Daring Studded Gloves";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + daringstuddedgloves.Name + ", creating it ...");
				daringstuddedgloves.Level = 5;
				daringstuddedgloves.Weight = 24;
				daringstuddedgloves.Model = 233;
				daringstuddedgloves.Object_Type = 34;
				daringstuddedgloves.Item_Type = 22;
				daringstuddedgloves.Id_nb = "daringstuddedgloves";
				daringstuddedgloves.Hand = 0;
				daringstuddedgloves.Platinum = 0;
				daringstuddedgloves.Gold = 0;
				daringstuddedgloves.Silver = 0;
				daringstuddedgloves.Copper = 0;
				daringstuddedgloves.IsPickable = true;
				daringstuddedgloves.IsDropable = true;
				daringstuddedgloves.IsTradable = true;
				daringstuddedgloves.CanDropAsLoot = false;
				daringstuddedgloves.Color = 0;
				daringstuddedgloves.Bonus = 0; // default bonus				
				daringstuddedgloves.Bonus1 = 4;
				daringstuddedgloves.Bonus1Type = (int) 4;
				daringstuddedgloves.Bonus2 = 0;
				daringstuddedgloves.Bonus2Type = (int) 0;
				daringstuddedgloves.Bonus3 = 0;
				daringstuddedgloves.Bonus3Type = (int) 0;
				daringstuddedgloves.Bonus4 = 0;
				daringstuddedgloves.Bonus4Type = (int) 0;
				daringstuddedgloves.Bonus5 = 0;
				daringstuddedgloves.Bonus5Type = (int) 0;
				daringstuddedgloves.Bonus6 = 0;
				daringstuddedgloves.Bonus6Type = (int) 0;
				daringstuddedgloves.Bonus7 = 0;
				daringstuddedgloves.Bonus7Type = (int) 0;
				daringstuddedgloves.Bonus8 = 0;
				daringstuddedgloves.Bonus8Type = (int) 0;
				daringstuddedgloves.Bonus9 = 0;
				daringstuddedgloves.Bonus9Type = (int) 0;
				daringstuddedgloves.Bonus10 = 0;
				daringstuddedgloves.Bonus10Type = (int) 0;
				daringstuddedgloves.ExtraBonus = 0;
				daringstuddedgloves.ExtraBonusType = (int) 0;
				daringstuddedgloves.Effect = 0;
				daringstuddedgloves.Emblem = 0;
				daringstuddedgloves.Charges = 0;
				daringstuddedgloves.MaxCharges = 0;
				daringstuddedgloves.SpellID = 0;
				daringstuddedgloves.ProcSpellID = 0;
				daringstuddedgloves.Type_Damage = 0;
				daringstuddedgloves.Realm = 0;
				daringstuddedgloves.MaxCount = 1;
				daringstuddedgloves.PackSize = 1;
				daringstuddedgloves.Extension = 0;
				daringstuddedgloves.Quality = 89;				
				daringstuddedgloves.Condition = 100;
				daringstuddedgloves.MaxCondition = 100;
				daringstuddedgloves.Durability = 100;
				daringstuddedgloves.MaxDurability = 100;
				daringstuddedgloves.PoisonCharges = 0;
				daringstuddedgloves.PoisonMaxCharges = 0;
				daringstuddedgloves.PoisonSpellID = 0;
				daringstuddedgloves.ProcSpellID1 = 0;
				daringstuddedgloves.SpellID1 = 0;
				daringstuddedgloves.MaxCharges1 = 0;
				daringstuddedgloves.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(daringstuddedgloves);
				}
			daringstuddedjerkin = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "daringstuddedjerkin");
			if (daringstuddedjerkin == null)
			{
				daringstuddedjerkin = new ItemTemplate();
				daringstuddedjerkin.Name = "Daring Studded Jerkin";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + daringstuddedjerkin.Name + ", creating it ...");
				daringstuddedjerkin.Level = 5;
				daringstuddedjerkin.Weight = 60;
				daringstuddedjerkin.Model = 230;
				daringstuddedjerkin.Object_Type = 34;
				daringstuddedjerkin.Item_Type = 25;
				daringstuddedjerkin.Id_nb = "daringstuddedjerkin";
				daringstuddedjerkin.Hand = 0;
				daringstuddedjerkin.Platinum = 0;
				daringstuddedjerkin.Gold = 0;
				daringstuddedjerkin.Silver = 0;
				daringstuddedjerkin.Copper = 0;
				daringstuddedjerkin.IsPickable = true;
				daringstuddedjerkin.IsDropable = true;
				daringstuddedjerkin.IsTradable = true;
				daringstuddedjerkin.CanDropAsLoot = false;
				daringstuddedjerkin.Color = 0;
				daringstuddedjerkin.Bonus = 0; // default bonus				
				daringstuddedjerkin.Bonus1 = 12;
				daringstuddedjerkin.Bonus1Type = (int) 10;
				daringstuddedjerkin.Bonus2 = 0;
				daringstuddedjerkin.Bonus2Type = (int) 0;
				daringstuddedjerkin.Bonus3 = 0;
				daringstuddedjerkin.Bonus3Type = (int) 0;
				daringstuddedjerkin.Bonus4 = 0;
				daringstuddedjerkin.Bonus4Type = (int) 0;
				daringstuddedjerkin.Bonus5 = 0;
				daringstuddedjerkin.Bonus5Type = (int) 0;
				daringstuddedjerkin.Bonus6 = 0;
				daringstuddedjerkin.Bonus6Type = (int) 0;
				daringstuddedjerkin.Bonus7 = 0;
				daringstuddedjerkin.Bonus7Type = (int) 0;
				daringstuddedjerkin.Bonus8 = 0;
				daringstuddedjerkin.Bonus8Type = (int) 0;
				daringstuddedjerkin.Bonus9 = 0;
				daringstuddedjerkin.Bonus9Type = (int) 0;
				daringstuddedjerkin.Bonus10 = 0;
				daringstuddedjerkin.Bonus10Type = (int) 0;
				daringstuddedjerkin.ExtraBonus = 0;
				daringstuddedjerkin.ExtraBonusType = (int) 0;
				daringstuddedjerkin.Effect = 0;
				daringstuddedjerkin.Emblem = 0;
				daringstuddedjerkin.Charges = 0;
				daringstuddedjerkin.MaxCharges = 0;
				daringstuddedjerkin.SpellID = 0;
				daringstuddedjerkin.ProcSpellID = 0;
				daringstuddedjerkin.Type_Damage = 0;
				daringstuddedjerkin.Realm = 0;
				daringstuddedjerkin.MaxCount = 1;
				daringstuddedjerkin.PackSize = 1;
				daringstuddedjerkin.Extension = 0;
				daringstuddedjerkin.Quality = 89;				
				daringstuddedjerkin.Condition = 100;
				daringstuddedjerkin.MaxCondition = 100;
				daringstuddedjerkin.Durability = 100;
				daringstuddedjerkin.MaxDurability = 100;
				daringstuddedjerkin.PoisonCharges = 0;
				daringstuddedjerkin.PoisonMaxCharges = 0;
				daringstuddedjerkin.PoisonSpellID = 0;
				daringstuddedjerkin.ProcSpellID1 = 0;
				daringstuddedjerkin.SpellID1 = 0;
				daringstuddedjerkin.MaxCharges1 = 0;
				daringstuddedjerkin.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(daringstuddedjerkin);
				}
			daringstuddedleggings = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "daringstuddedleggings");
			if (daringstuddedleggings == null)
			{
				daringstuddedleggings = new ItemTemplate();
				daringstuddedleggings.Name = "Daring Studded Leggings";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + daringstuddedleggings.Name + ", creating it ...");
				daringstuddedleggings.Level = 5;
				daringstuddedleggings.Weight = 42;
				daringstuddedleggings.Model = 231;
				daringstuddedleggings.Object_Type = 34;
				daringstuddedleggings.Item_Type = 27;
				daringstuddedleggings.Id_nb = "daringstuddedleggings";
				daringstuddedleggings.Hand = 0;
				daringstuddedleggings.Platinum = 0;
				daringstuddedleggings.Gold = 0;
				daringstuddedleggings.Silver = 0;
				daringstuddedleggings.Copper = 0;
				daringstuddedleggings.IsPickable = true;
				daringstuddedleggings.IsDropable = true;
				daringstuddedleggings.IsTradable = true;
				daringstuddedleggings.CanDropAsLoot = false;
				daringstuddedleggings.Color = 0;
				daringstuddedleggings.Bonus = 0; // default bonus				
				daringstuddedleggings.Bonus1 = 4;
				daringstuddedleggings.Bonus1Type = (int) 4;
				daringstuddedleggings.Bonus2 = 0;
				daringstuddedleggings.Bonus2Type = (int) 0;
				daringstuddedleggings.Bonus3 = 0;
				daringstuddedleggings.Bonus3Type = (int) 0;
				daringstuddedleggings.Bonus4 = 0;
				daringstuddedleggings.Bonus4Type = (int) 0;
				daringstuddedleggings.Bonus5 = 0;
				daringstuddedleggings.Bonus5Type = (int) 0;
				daringstuddedleggings.Bonus6 = 0;
				daringstuddedleggings.Bonus6Type = (int) 0;
				daringstuddedleggings.Bonus7 = 0;
				daringstuddedleggings.Bonus7Type = (int) 0;
				daringstuddedleggings.Bonus8 = 0;
				daringstuddedleggings.Bonus8Type = (int) 0;
				daringstuddedleggings.Bonus9 = 0;
				daringstuddedleggings.Bonus9Type = (int) 0;
				daringstuddedleggings.Bonus10 = 0;
				daringstuddedleggings.Bonus10Type = (int) 0;
				daringstuddedleggings.ExtraBonus = 0;
				daringstuddedleggings.ExtraBonusType = (int) 0;
				daringstuddedleggings.Effect = 0;
				daringstuddedleggings.Emblem = 0;
				daringstuddedleggings.Charges = 0;
				daringstuddedleggings.MaxCharges = 0;
				daringstuddedleggings.SpellID = 0;
				daringstuddedleggings.ProcSpellID = 0;
				daringstuddedleggings.Type_Damage = 0;
				daringstuddedleggings.Realm = 0;
				daringstuddedleggings.MaxCount = 1;
				daringstuddedleggings.PackSize = 1;
				daringstuddedleggings.Extension = 0;
				daringstuddedleggings.Quality = 89;				
				daringstuddedleggings.Condition = 100;
				daringstuddedleggings.MaxCondition = 100;
				daringstuddedleggings.Durability = 100;
				daringstuddedleggings.MaxDurability = 100;
				daringstuddedleggings.PoisonCharges = 0;
				daringstuddedleggings.PoisonMaxCharges = 0;
				daringstuddedleggings.PoisonSpellID = 0;
				daringstuddedleggings.ProcSpellID1 = 0;
				daringstuddedleggings.SpellID1 = 0;
				daringstuddedleggings.MaxCharges1 = 0;
				daringstuddedleggings.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(daringstuddedleggings);
				}
			daringstuddedsleeves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "daringstuddedsleeves");
			if (daringstuddedsleeves == null)
			{
				daringstuddedsleeves = new ItemTemplate();
				daringstuddedsleeves.Name = "Daring Studded Sleeves";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + daringstuddedsleeves.Name + ", creating it ...");
				daringstuddedsleeves.Level = 5;
				daringstuddedsleeves.Weight = 36;
				daringstuddedsleeves.Model = 232;
				daringstuddedsleeves.Object_Type = 34;
				daringstuddedsleeves.Item_Type = 28;
				daringstuddedsleeves.Id_nb = "daringstuddedsleeves";
				daringstuddedsleeves.Hand = 0;
				daringstuddedsleeves.Platinum = 0;
				daringstuddedsleeves.Gold = 0;
				daringstuddedsleeves.Silver = 0;
				daringstuddedsleeves.Copper = 0;
				daringstuddedsleeves.IsPickable = true;
				daringstuddedsleeves.IsDropable = true;
				daringstuddedsleeves.IsTradable = true;
				daringstuddedsleeves.CanDropAsLoot = false;
				daringstuddedsleeves.Color = 0;
				daringstuddedsleeves.Bonus = 0; // default bonus				
				daringstuddedsleeves.Bonus1 = 4;
				daringstuddedsleeves.Bonus1Type = (int) 1;
				daringstuddedsleeves.Bonus2 = 0;
				daringstuddedsleeves.Bonus2Type = (int) 0;
				daringstuddedsleeves.Bonus3 = 0;
				daringstuddedsleeves.Bonus3Type = (int) 0;
				daringstuddedsleeves.Bonus4 = 0;
				daringstuddedsleeves.Bonus4Type = (int) 0;
				daringstuddedsleeves.Bonus5 = 0;
				daringstuddedsleeves.Bonus5Type = (int) 0;
				daringstuddedsleeves.Bonus6 = 0;
				daringstuddedsleeves.Bonus6Type = (int) 0;
				daringstuddedsleeves.Bonus7 = 0;
				daringstuddedsleeves.Bonus7Type = (int) 0;
				daringstuddedsleeves.Bonus8 = 0;
				daringstuddedsleeves.Bonus8Type = (int) 0;
				daringstuddedsleeves.Bonus9 = 0;
				daringstuddedsleeves.Bonus9Type = (int) 0;
				daringstuddedsleeves.Bonus10 = 0;
				daringstuddedsleeves.Bonus10Type = (int) 0;
				daringstuddedsleeves.ExtraBonus = 0;
				daringstuddedsleeves.ExtraBonusType = (int) 0;
				daringstuddedsleeves.Effect = 0;
				daringstuddedsleeves.Emblem = 0;
				daringstuddedsleeves.Charges = 0;
				daringstuddedsleeves.MaxCharges = 0;
				daringstuddedsleeves.SpellID = 0;
				daringstuddedsleeves.ProcSpellID = 0;
				daringstuddedsleeves.Type_Damage = 0;
				daringstuddedsleeves.Realm = 0;
				daringstuddedsleeves.MaxCount = 1;
				daringstuddedsleeves.PackSize = 1;
				daringstuddedsleeves.Extension = 0;
				daringstuddedsleeves.Quality = 89;				
				daringstuddedsleeves.Condition = 100;
				daringstuddedsleeves.MaxCondition = 100;
				daringstuddedsleeves.Durability = 100;
				daringstuddedsleeves.MaxDurability = 100;
				daringstuddedsleeves.PoisonCharges = 0;
				daringstuddedsleeves.PoisonMaxCharges = 0;
				daringstuddedsleeves.PoisonSpellID = 0;
				daringstuddedsleeves.ProcSpellID1 = 0;
				daringstuddedsleeves.SpellID1 = 0;
				daringstuddedsleeves.MaxCharges1 = 0;
				daringstuddedsleeves.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(daringstuddedsleeves);
				}
			

			#endregion

			#region defineAreas
			Statua = new Area.Circle("Statua",27577,39982,14488,500);											
					Statua.IsSafeArea = false;
				Statua.CanBroadcast = false;
				Statua.DisplayMessage = false;
				Statua.CheckLOS = false;
				
				Region StatuaRegion = WorldMgr.GetRegion(489);
				if (StatuaRegion != null)
					StatuaRegion.AddArea(Statua);
			
		
		#endregion
		
		#region defineQuestParts

		QuestBuilder builder = QuestMgr.getBuilder(typeof(childsplay));
			BaseQuestPart a;
			a = builder.CreateQuestPart(Charles,-1);
				a.AddTrigger(eTriggerType.Interact,null,Charles);
			a.AddRequirement(eRequirementType.QuestGivable,"DOL.GS.Quests.Midgard.childsplay",Charles);
			a.AddRequirement(eRequirementType.QuestPending,"DOL.GS.Quests.Midgard.childsplay",(eComparator)5);
			a.AddAction(eActionType.Talk,"Hello there, Viking. Have you ever gotten tired of playing the same game over and over? That's what it's like here. All the adults say we can do is sit here and play 'Truth or Dare.' Sure, it was fun at first, but most of us have gotten bored and Raymond's too chicken to do any of the [dares] I think of for him.",Charles);
			AddQuestPart(a);
			a = builder.CreateQuestPart(Charles,-1);
				a.AddTrigger(eTriggerType.Whisper,"dares",Charles);
			a.AddRequirement(eRequirementType.QuestGivable,"DOL.GS.Quests.Midgard.childsplay",Charles);
			a.AddRequirement(eRequirementType.QuestPending,"DOL.GS.Quests.Midgard.childsplay",(eComparator)5);
			a.AddAction(eActionType.Talk,"It was a lot more fun when we could play 'Spin the Elixir' in the tavern basement with the girls, but Tiff's parents caught us there and we've been treated like babies ever since. I bet this game would be a whole lot better if you could [join] us. I'd even kick Raymond out and think up the bestest dare ever, just for you.",Charles);
			AddQuestPart(a);
			a = builder.CreateQuestPart(Charles,-1);
				a.AddTrigger(eTriggerType.Whisper,"join",Charles);
			a.AddRequirement(eRequirementType.QuestGivable,"DOL.GS.Quests.Midgard.childsplay",Charles);
			a.AddRequirement(eRequirementType.QuestPending,"DOL.GS.Quests.Midgard.childsplay",(eComparator)5);
			a.AddAction(eActionType.Talk,"Hmm. Give me a minute. I know just the thing. The guards keep talking about a big, scary statue in the demon dungeon beyond the limits of town. I dare you to go in and touch it, and come back out without getting eaten by a monster! What do you say?",Charles);
			a.AddAction(eActionType.OfferQuest,"DOL.GS.Quests.Midgard.childsplay","Will you join Charles's game of 'Truth or Dare'? [Level 1]");
			AddQuestPart(a);
			a = builder.CreateQuestPart(Charles,-1);
				a.AddTrigger(eTriggerType.DeclineQuest,null,"DOL.GS.Quests.Midgard.childsplay");
			a.AddAction(eActionType.Talk,"No Problem. See you.",Charles);
			AddQuestPart(a);
			a = builder.CreateQuestPart(Charles,-1);
				a.AddTrigger(eTriggerType.AcceptQuest,null,"DOL.GS.Quests.Midgard.childsplay");
			a.AddAction(eActionType.Talk,"Great, we'll, uh, wait right here while you go do it. See, Raymond, not everyone's a big chicken like you. Chicken!",Charles);
			a.AddAction(eActionType.GiveQuest,"DOL.GS.Quests.Midgard.childsplay",Charles);
			AddQuestPart(a);
			a = builder.CreateQuestPart(Charles,-1);
				a.AddTrigger(eTriggerType.EnterArea,null,Statua);
			a.AddAction(eActionType.IncQuestStep,"DOL.GS.Quests.Midgard.childsplay");
			AddQuestPart(a);
			a = builder.CreateQuestPart(Charles,-1);
				a.AddTrigger(eTriggerType.Interact,null,Charles);
			a.AddRequirement(eRequirementType.QuestStep,"DOL.GS.Quests.Midgard.childsplay",2,(eComparator)3);
			a.AddRequirement(eRequirementType.Class,35);
			a.AddAction(eActionType.Talk,"You made it back alive! I mean, uh, welcome back! How was the statue? Did you get scared? Does this mean you're going to stay and play the game with us again? If you are, you can have these. I don't need them anymore.",Charles);
			a.AddAction(eActionType.GiveItem,daringstuddedboots,Charles);
			a.AddAction(eActionType.GiveItem,daringstuddedcap,Charles);
			a.AddAction(eActionType.GiveItem,daringstuddedgloves,Charles);
			a.AddAction(eActionType.GiveItem,daringstuddedjerkin,Charles);
			a.AddAction(eActionType.GiveItem,daringstuddedleggings,Charles);
			a.AddAction(eActionType.GiveItem,daringstuddedsleeves,Charles);
			a.AddAction(eActionType.GiveGold,67);
			a.AddAction(eActionType.GiveXP,2);
			a.AddAction(eActionType.FinishQuest,"DOL.GS.Quests.Midgard.childsplay");
			AddQuestPart(a);
			a = builder.CreateQuestPart(Charles,-1);
				a.AddTrigger(eTriggerType.Interact,null,Charles);
			a.AddRequirement(eRequirementType.QuestStep,"DOL.GS.Quests.Midgard.childsplay",2,(eComparator)3);
			a.AddRequirement(eRequirementType.Class,37);
			a.AddAction(eActionType.Talk,"You made it back alive! I mean, uh, welcome back! How was the statue? Did you get scared? Does this mean you're going to stay and play the game with us again? If you are, you can have these. I don't need them anymore.",Charles);
			a.AddAction(eActionType.GiveItem,daringleatherboots,Charles);
			a.AddAction(eActionType.GiveItem,daringleathercap,Charles);
			a.AddAction(eActionType.GiveItem,daringleathergloves,Charles);
			a.AddAction(eActionType.GiveItem,daringleatherjerkin,Charles);
			a.AddAction(eActionType.GiveItem,daringleatherleggings,Charles);
			a.AddAction(eActionType.GiveItem,daringleathersleeves,Charles);
			a.AddAction(eActionType.GiveGold,67);
			a.AddAction(eActionType.GiveXP,2);
			a.AddAction(eActionType.FinishQuest,"DOL.GS.Quests.Midgard.childsplay");
			AddQuestPart(a);
			a = builder.CreateQuestPart(Charles,-1);
				a.AddTrigger(eTriggerType.Interact,null,Charles);
			a.AddRequirement(eRequirementType.QuestStep,"DOL.GS.Quests.Midgard.childsplay",2,(eComparator)3);
			a.AddRequirement(eRequirementType.Class,38);
			a.AddAction(eActionType.Talk,"You made it back alive! I mean, uh, welcome back! How was the statue? Did you get scared? Does this mean you're going to stay and play the game with us again? If you are, you can have these. I don't need them anymore.",Charles);
			a.AddAction(eActionType.GiveItem,daringleatherboots,Charles);
			a.AddAction(eActionType.GiveItem,daringleathercap,Charles);
			a.AddAction(eActionType.GiveItem,daringleathergloves,Charles);
			a.AddAction(eActionType.GiveItem,daringleatherjerkin,Charles);
			a.AddAction(eActionType.GiveItem,daringleatherleggings,Charles);
			a.AddAction(eActionType.GiveItem,daringleathersleeves,Charles);
			a.AddAction(eActionType.GiveXP,2);
			a.AddAction(eActionType.GiveGold,67);
			a.AddAction(eActionType.FinishQuest,"DOL.GS.Quests.Midgard.childsplay");
			AddQuestPart(a);
			a = builder.CreateQuestPart(Charles,-1);
				a.AddTrigger(eTriggerType.Interact,null,Charles);
			a.AddRequirement(eRequirementType.QuestStep,"DOL.GS.Quests.Midgard.childsplay",2,(eComparator)3);
			a.AddRequirement(eRequirementType.Class,36);
			a.AddAction(eActionType.Talk,"You made it back alive! I mean, uh, welcome back! How was the statue? Did you get scared? Does this mean you're going to stay and play the game with us again? If you are, you can have these. I don't need them anymore.",Charles);
			a.AddAction(eActionType.GiveItem,daringpaddedboots,Charles);
			a.AddAction(eActionType.GiveItem,daringpaddedcap,Charles);
			a.AddAction(eActionType.GiveItem,daringpaddedgloves,Charles);
			a.AddAction(eActionType.GiveItem,daringpaddedpants,Charles);
			a.AddAction(eActionType.GiveItem,daringpaddedvest,Charles);
			a.AddAction(eActionType.GiveItem,daringpaddedsleeves,Charles);
			a.AddAction(eActionType.GiveXP,2);
			a.AddAction(eActionType.GiveGold,67);
			a.AddAction(eActionType.FinishQuest,"DOL.GS.Quests.Midgard.childsplay");
			AddQuestPart(a);
			
			#endregion

			// Custom Scriptloaded Code Begin
			
			// Custom Scriptloaded Code End

			Charles.AddQuestToGive(typeof (childsplay));
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{			
				
			// Custom Scriptunloaded Code Begin
			
			// Custom Scriptunloaded Code End

							
				Region StatuaRegion = WorldMgr.GetRegion(489);
				if (StatuaRegion != null)
						StatuaRegion.RemoveArea(Statua);
			

			/* If Charles has not been initialized, then we don't have to remove any
			 * hooks from him ;-)
			 */
			if (Charles == null)
				return;
			/* Now we remove to SirQuait the possibility to give this quest to players */			
			Charles.RemoveQuestToGive(typeof (childsplay));
		}

		/* Now we set the quest name.
		* If we don't override the base method, then the quest
		* will have the name "UNDEFINED QUEST NAME" and we don't
		* want that, do we? ;-)
		*/

		public override string Name
		{
			get { return questTitle; }
		}

		/* Now we set the quest step descriptions.
		* If we don't override the base method, then the quest
		* description for ALL steps will be "UNDEFINDED QUEST DESCRIPTION"
		* and this isn't something nice either ;-)
		*/
		public override string Description
		{
			get
			{
			switch (Step)
			{
				
					case 1:
						return "[Step #1] Charles has dared you to touch the statue in the center of the Demons' Breach dungeon. Travel a shot distance northeast of Mularn's bindstone to find the dungeon entrance.";
				
					case 2:
						return "[Step #2] Return to Charles in Mularn and let him know you've completed his dare.";
				
					default:
						return " No Queststep Description available.";
				}				
			}
		}
		
		/// <summary>
		/// This method checks if a player is qualified for this quest
		/// </summary>
		/// <returns>true if qualified, false if not</returns>
		public override bool CheckQuestQualification(GamePlayer player)
		{		
			// if the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(typeof (childsplay)) != null)
				return true;
				
			// Custom Code Begin
			
			// Custom  Code End
			
		
			if (player.Level > maximumLevel || player.Level < minimumLevel )
				return false;
		
			if (
		
			player.CharacterClass.ID != (byte) eCharacterClass.MidgardRogue && 
			player.CharacterClass.ID != (byte) eCharacterClass.Mystic && 
			player.CharacterClass.ID != (byte) eCharacterClass.Seer && 
			player.CharacterClass.ID != (byte) eCharacterClass.Viking && 
				true) {
				return false;			
			}
		
			return true;
		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...
		}
	}
}
