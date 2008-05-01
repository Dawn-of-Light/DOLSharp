	
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
using DOL.GS.Behaviour;
using DOL.GS.Behaviour.Attributes;
using DOL.AI.Brain;

	namespace DOL.GS.Quests.Albion {
	
     /* The first thing we do, is to declare the class we create
	 * as Quest. To do this, we derive from the abstract class
	 * BaseQuest	  	 
	 */
	public class Aidingguardalakyrr : BaseQuest
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

		protected const string questTitle = "Aiding Guard Alakyrr";

		protected const int minimumLevel = 1;
		protected const int maximumLevel = 4;
	
	
		private static GameNPC GuardAlakyrr = null;
		
		private static ItemTemplate enchanterdtenebrousflask = null;
		
		private static ItemTemplate quarterfulltenebrousflask = null;
		
		private static ItemTemplate halffulltenebrousflask = null;
		
		private static ItemTemplate threequarterfulltenebrousflask = null;
		
		private static ItemTemplate fullflaskoftenebrousessence = null;
		

		// Custom Initialization Code Begin
		
		// Custom Initialization Code End

		/* 
		* Constructor
		*/
		public Aidingguardalakyrr() : base()
		{
		}

		public Aidingguardalakyrr(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public Aidingguardalakyrr(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public Aidingguardalakyrr(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
	{
	}

	[ScriptLoadedEvent]
	public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
	{
	if (!ServerProperties.Properties.LOAD_QUESTS)
		return;
	if (log.IsInfoEnabled)
		log.Info("Quest \"" + questTitle + "\" initializing ...");

	#region defineNPCs
	GameNPC[] npcs;
	
			npcs = WorldMgr.GetNPCsByName("Guard Alakyrr",(eRealm) 1);
			if (npcs.Length == 0)
			{
				if (!WorldMgr.GetRegion(63).IsDisabled)
				{
				GuardAlakyrr = new DOL.GS.GameNPC();
					GuardAlakyrr.Model = 748;
				GuardAlakyrr.Name = "Guard Alakyrr";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + GuardAlakyrr.Name + ", creating ...");
				GuardAlakyrr.GuildName = "Part of " + questTitle + " Quest";
				GuardAlakyrr.Realm = eRealm.Albion;
				GuardAlakyrr.CurrentRegionID = 63;
				GuardAlakyrr.Size = 50;
				GuardAlakyrr.Level = 30;
				GuardAlakyrr.MaxSpeedBase = 191;
				GuardAlakyrr.Faction = FactionMgr.GetFactionByID(0);
				GuardAlakyrr.X = 28707;
				GuardAlakyrr.Y = 20147;
				GuardAlakyrr.Z = 16760;
				GuardAlakyrr.Heading = 4016;
				GuardAlakyrr.RespawnInterval = -1;
				GuardAlakyrr.BodyType = 0;
				

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 500;
				GuardAlakyrr.SetOwnBrain(brain);
				
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GuardAlakyrr.SaveIntoDatabase();
					
				GuardAlakyrr.AddToWorld();
				
				}
			}
			else 
			{
				GuardAlakyrr = npcs[0];
			}
		

			#endregion

			#region defineItems

		enchanterdtenebrousflask = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "enchanterdtenebrousflask");
			if (enchanterdtenebrousflask == null)
			{
				enchanterdtenebrousflask = new ItemTemplate();
				enchanterdtenebrousflask.Name = "Enchanted Tenebrous Flask";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + enchanterdtenebrousflask.Name + ", creating it ...");
				enchanterdtenebrousflask.Level = 1;
				enchanterdtenebrousflask.Weight = 1;
				enchanterdtenebrousflask.Model = 490;
				enchanterdtenebrousflask.Object_Type = 0;
				enchanterdtenebrousflask.Item_Type = -1;
				enchanterdtenebrousflask.Id_nb = "enchanterdtenebrousflask";
				enchanterdtenebrousflask.Hand = 0;
				enchanterdtenebrousflask.Platinum = 0;
				enchanterdtenebrousflask.Gold = 0;
				enchanterdtenebrousflask.Silver = 0;
				enchanterdtenebrousflask.Copper = 0;
				enchanterdtenebrousflask.IsPickable = true;
				enchanterdtenebrousflask.IsDropable = false;
				enchanterdtenebrousflask.IsTradable = true;
				enchanterdtenebrousflask.CanDropAsLoot = true;
				enchanterdtenebrousflask.Color = 0;
				enchanterdtenebrousflask.Bonus = 0; // default bonus				
				enchanterdtenebrousflask.Bonus1 = 0;
				enchanterdtenebrousflask.Bonus1Type = (int) 0;
				enchanterdtenebrousflask.Bonus2 = 0;
				enchanterdtenebrousflask.Bonus2Type = (int) 0;
				enchanterdtenebrousflask.Bonus3 = 0;
				enchanterdtenebrousflask.Bonus3Type = (int) 0;
				enchanterdtenebrousflask.Bonus4 = 0;
				enchanterdtenebrousflask.Bonus4Type = (int) 0;
				enchanterdtenebrousflask.Bonus5 = 0;
				enchanterdtenebrousflask.Bonus5Type = (int) 0;
				enchanterdtenebrousflask.Bonus6 = 0;
				enchanterdtenebrousflask.Bonus6Type = (int) 0;
				enchanterdtenebrousflask.Bonus7 = 0;
				enchanterdtenebrousflask.Bonus7Type = (int) 0;
				enchanterdtenebrousflask.Bonus8 = 0;
				enchanterdtenebrousflask.Bonus8Type = (int) 0;
				enchanterdtenebrousflask.Bonus9 = 0;
				enchanterdtenebrousflask.Bonus9Type = (int) 0;
				enchanterdtenebrousflask.Bonus10 = 0;
				enchanterdtenebrousflask.Bonus10Type = (int) 0;
				enchanterdtenebrousflask.ExtraBonus = 0;
				enchanterdtenebrousflask.ExtraBonusType = (int) 0;
				enchanterdtenebrousflask.Effect = 0;
				enchanterdtenebrousflask.Emblem = 0;
				enchanterdtenebrousflask.Charges = 0;
				enchanterdtenebrousflask.MaxCharges = 0;
				enchanterdtenebrousflask.SpellID = 0;
				enchanterdtenebrousflask.ProcSpellID = 0;
				enchanterdtenebrousflask.Type_Damage = 0;
				enchanterdtenebrousflask.Realm = 0;
				enchanterdtenebrousflask.MaxCount = 1;
				enchanterdtenebrousflask.PackSize = 1;
				enchanterdtenebrousflask.Extension = 0;
				enchanterdtenebrousflask.Quality = 100;				
				enchanterdtenebrousflask.Condition = 100;
				enchanterdtenebrousflask.MaxCondition = 100;
				enchanterdtenebrousflask.Durability = 100;
				enchanterdtenebrousflask.MaxDurability = 100;
				enchanterdtenebrousflask.PoisonCharges = 0;
				enchanterdtenebrousflask.PoisonMaxCharges = 0;
				enchanterdtenebrousflask.PoisonSpellID = 0;
				enchanterdtenebrousflask.ProcSpellID1 = 0;
				enchanterdtenebrousflask.SpellID1 = 0;
				enchanterdtenebrousflask.MaxCharges1 = 0;
				enchanterdtenebrousflask.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(enchanterdtenebrousflask);
				}
			quarterfulltenebrousflask = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "quarterfulltenebrousflask");
			if (quarterfulltenebrousflask == null)
			{
				quarterfulltenebrousflask = new ItemTemplate();
				quarterfulltenebrousflask.Name = "Quarter Full Tenebrous Flask";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + quarterfulltenebrousflask.Name + ", creating it ...");
				quarterfulltenebrousflask.Level = 1;
				quarterfulltenebrousflask.Weight = 1;
				quarterfulltenebrousflask.Model = 490;
				quarterfulltenebrousflask.Object_Type = 0;
				quarterfulltenebrousflask.Item_Type = -1;
				quarterfulltenebrousflask.Id_nb = "quarterfulltenebrousflask";
				quarterfulltenebrousflask.Hand = 0;
				quarterfulltenebrousflask.Platinum = 0;
				quarterfulltenebrousflask.Gold = 0;
				quarterfulltenebrousflask.Silver = 0;
				quarterfulltenebrousflask.Copper = 0;
				quarterfulltenebrousflask.IsPickable = true;
				quarterfulltenebrousflask.IsDropable = false;
				quarterfulltenebrousflask.IsTradable = true;
				quarterfulltenebrousflask.CanDropAsLoot = true;
				quarterfulltenebrousflask.Color = 0;
				quarterfulltenebrousflask.Bonus = 0; // default bonus				
				quarterfulltenebrousflask.Bonus1 = 0;
				quarterfulltenebrousflask.Bonus1Type = (int) 0;
				quarterfulltenebrousflask.Bonus2 = 0;
				quarterfulltenebrousflask.Bonus2Type = (int) 0;
				quarterfulltenebrousflask.Bonus3 = 0;
				quarterfulltenebrousflask.Bonus3Type = (int) 0;
				quarterfulltenebrousflask.Bonus4 = 0;
				quarterfulltenebrousflask.Bonus4Type = (int) 0;
				quarterfulltenebrousflask.Bonus5 = 0;
				quarterfulltenebrousflask.Bonus5Type = (int) 0;
				quarterfulltenebrousflask.Bonus6 = 0;
				quarterfulltenebrousflask.Bonus6Type = (int) 0;
				quarterfulltenebrousflask.Bonus7 = 0;
				quarterfulltenebrousflask.Bonus7Type = (int) 0;
				quarterfulltenebrousflask.Bonus8 = 0;
				quarterfulltenebrousflask.Bonus8Type = (int) 0;
				quarterfulltenebrousflask.Bonus9 = 0;
				quarterfulltenebrousflask.Bonus9Type = (int) 0;
				quarterfulltenebrousflask.Bonus10 = 0;
				quarterfulltenebrousflask.Bonus10Type = (int) 0;
				quarterfulltenebrousflask.ExtraBonus = 0;
				quarterfulltenebrousflask.ExtraBonusType = (int) 0;
				quarterfulltenebrousflask.Effect = 0;
				quarterfulltenebrousflask.Emblem = 0;
				quarterfulltenebrousflask.Charges = 0;
				quarterfulltenebrousflask.MaxCharges = 0;
				quarterfulltenebrousflask.SpellID = 0;
				quarterfulltenebrousflask.ProcSpellID = 0;
				quarterfulltenebrousflask.Type_Damage = 0;
				quarterfulltenebrousflask.Realm = 0;
				quarterfulltenebrousflask.MaxCount = 1;
				quarterfulltenebrousflask.PackSize = 1;
				quarterfulltenebrousflask.Extension = 0;
				quarterfulltenebrousflask.Quality = 100;				
				quarterfulltenebrousflask.Condition = 100;
				quarterfulltenebrousflask.MaxCondition = 100;
				quarterfulltenebrousflask.Durability = 100;
				quarterfulltenebrousflask.MaxDurability = 100;
				quarterfulltenebrousflask.PoisonCharges = 0;
				quarterfulltenebrousflask.PoisonMaxCharges = 0;
				quarterfulltenebrousflask.PoisonSpellID = 0;
				quarterfulltenebrousflask.ProcSpellID1 = 0;
				quarterfulltenebrousflask.SpellID1 = 0;
				quarterfulltenebrousflask.MaxCharges1 = 0;
				quarterfulltenebrousflask.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(quarterfulltenebrousflask);
				}
			halffulltenebrousflask = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "halffulltenebrousflask");
			if (halffulltenebrousflask == null)
			{
				halffulltenebrousflask = new ItemTemplate();
				halffulltenebrousflask.Name = "Half Full Tenebrous Flask";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + halffulltenebrousflask.Name + ", creating it ...");
				halffulltenebrousflask.Level = 1;
				halffulltenebrousflask.Weight = 1;
				halffulltenebrousflask.Model = 490;
				halffulltenebrousflask.Object_Type = 0;
				halffulltenebrousflask.Item_Type = -1;
				halffulltenebrousflask.Id_nb = "halffulltenebrousflask";
				halffulltenebrousflask.Hand = 0;
				halffulltenebrousflask.Platinum = 0;
				halffulltenebrousflask.Gold = 0;
				halffulltenebrousflask.Silver = 0;
				halffulltenebrousflask.Copper = 0;
				halffulltenebrousflask.IsPickable = true;
				halffulltenebrousflask.IsDropable = false;
				halffulltenebrousflask.IsTradable = true;
				halffulltenebrousflask.CanDropAsLoot = true;
				halffulltenebrousflask.Color = 0;
				halffulltenebrousflask.Bonus = 0; // default bonus				
				halffulltenebrousflask.Bonus1 = 0;
				halffulltenebrousflask.Bonus1Type = (int) 0;
				halffulltenebrousflask.Bonus2 = 0;
				halffulltenebrousflask.Bonus2Type = (int) 0;
				halffulltenebrousflask.Bonus3 = 0;
				halffulltenebrousflask.Bonus3Type = (int) 0;
				halffulltenebrousflask.Bonus4 = 0;
				halffulltenebrousflask.Bonus4Type = (int) 0;
				halffulltenebrousflask.Bonus5 = 0;
				halffulltenebrousflask.Bonus5Type = (int) 0;
				halffulltenebrousflask.Bonus6 = 0;
				halffulltenebrousflask.Bonus6Type = (int) 0;
				halffulltenebrousflask.Bonus7 = 0;
				halffulltenebrousflask.Bonus7Type = (int) 0;
				halffulltenebrousflask.Bonus8 = 0;
				halffulltenebrousflask.Bonus8Type = (int) 0;
				halffulltenebrousflask.Bonus9 = 0;
				halffulltenebrousflask.Bonus9Type = (int) 0;
				halffulltenebrousflask.Bonus10 = 0;
				halffulltenebrousflask.Bonus10Type = (int) 0;
				halffulltenebrousflask.ExtraBonus = 0;
				halffulltenebrousflask.ExtraBonusType = (int) 0;
				halffulltenebrousflask.Effect = 0;
				halffulltenebrousflask.Emblem = 0;
				halffulltenebrousflask.Charges = 0;
				halffulltenebrousflask.MaxCharges = 0;
				halffulltenebrousflask.SpellID = 0;
				halffulltenebrousflask.ProcSpellID = 0;
				halffulltenebrousflask.Type_Damage = 0;
				halffulltenebrousflask.Realm = 0;
				halffulltenebrousflask.MaxCount = 1;
				halffulltenebrousflask.PackSize = 1;
				halffulltenebrousflask.Extension = 0;
				halffulltenebrousflask.Quality = 100;				
				halffulltenebrousflask.Condition = 100;
				halffulltenebrousflask.MaxCondition = 100;
				halffulltenebrousflask.Durability = 100;
				halffulltenebrousflask.MaxDurability = 100;
				halffulltenebrousflask.PoisonCharges = 0;
				halffulltenebrousflask.PoisonMaxCharges = 0;
				halffulltenebrousflask.PoisonSpellID = 0;
				halffulltenebrousflask.ProcSpellID1 = 0;
				halffulltenebrousflask.SpellID1 = 0;
				halffulltenebrousflask.MaxCharges1 = 0;
				halffulltenebrousflask.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(halffulltenebrousflask);
				}
			threequarterfulltenebrousflask = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "threequarterfulltenebrousflask");
			if (threequarterfulltenebrousflask == null)
			{
				threequarterfulltenebrousflask = new ItemTemplate();
				threequarterfulltenebrousflask.Name = "3 Quarter Full Tenebrous Flask";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + threequarterfulltenebrousflask.Name + ", creating it ...");
				threequarterfulltenebrousflask.Level = 1;
				threequarterfulltenebrousflask.Weight = 1;
				threequarterfulltenebrousflask.Model = 490;
				threequarterfulltenebrousflask.Object_Type = 0;
				threequarterfulltenebrousflask.Item_Type = -1;
				threequarterfulltenebrousflask.Id_nb = "threequarterfulltenebrousflask";
				threequarterfulltenebrousflask.Hand = 0;
				threequarterfulltenebrousflask.Platinum = 0;
				threequarterfulltenebrousflask.Gold = 0;
				threequarterfulltenebrousflask.Silver = 0;
				threequarterfulltenebrousflask.Copper = 0;
				threequarterfulltenebrousflask.IsPickable = true;
				threequarterfulltenebrousflask.IsDropable = false;
				threequarterfulltenebrousflask.IsTradable = true;
				threequarterfulltenebrousflask.CanDropAsLoot = true;
				threequarterfulltenebrousflask.Color = 0;
				threequarterfulltenebrousflask.Bonus = 0; // default bonus				
				threequarterfulltenebrousflask.Bonus1 = 0;
				threequarterfulltenebrousflask.Bonus1Type = (int) 0;
				threequarterfulltenebrousflask.Bonus2 = 0;
				threequarterfulltenebrousflask.Bonus2Type = (int) 0;
				threequarterfulltenebrousflask.Bonus3 = 0;
				threequarterfulltenebrousflask.Bonus3Type = (int) 0;
				threequarterfulltenebrousflask.Bonus4 = 0;
				threequarterfulltenebrousflask.Bonus4Type = (int) 0;
				threequarterfulltenebrousflask.Bonus5 = 0;
				threequarterfulltenebrousflask.Bonus5Type = (int) 0;
				threequarterfulltenebrousflask.Bonus6 = 0;
				threequarterfulltenebrousflask.Bonus6Type = (int) 0;
				threequarterfulltenebrousflask.Bonus7 = 0;
				threequarterfulltenebrousflask.Bonus7Type = (int) 0;
				threequarterfulltenebrousflask.Bonus8 = 0;
				threequarterfulltenebrousflask.Bonus8Type = (int) 0;
				threequarterfulltenebrousflask.Bonus9 = 0;
				threequarterfulltenebrousflask.Bonus9Type = (int) 0;
				threequarterfulltenebrousflask.Bonus10 = 0;
				threequarterfulltenebrousflask.Bonus10Type = (int) 0;
				threequarterfulltenebrousflask.ExtraBonus = 0;
				threequarterfulltenebrousflask.ExtraBonusType = (int) 0;
				threequarterfulltenebrousflask.Effect = 0;
				threequarterfulltenebrousflask.Emblem = 0;
				threequarterfulltenebrousflask.Charges = 0;
				threequarterfulltenebrousflask.MaxCharges = 0;
				threequarterfulltenebrousflask.SpellID = 0;
				threequarterfulltenebrousflask.ProcSpellID = 0;
				threequarterfulltenebrousflask.Type_Damage = 0;
				threequarterfulltenebrousflask.Realm = 0;
				threequarterfulltenebrousflask.MaxCount = 1;
				threequarterfulltenebrousflask.PackSize = 1;
				threequarterfulltenebrousflask.Extension = 0;
				threequarterfulltenebrousflask.Quality = 100;				
				threequarterfulltenebrousflask.Condition = 100;
				threequarterfulltenebrousflask.MaxCondition = 100;
				threequarterfulltenebrousflask.Durability = 100;
				threequarterfulltenebrousflask.MaxDurability = 100;
				threequarterfulltenebrousflask.PoisonCharges = 0;
				threequarterfulltenebrousflask.PoisonMaxCharges = 0;
				threequarterfulltenebrousflask.PoisonSpellID = 0;
				threequarterfulltenebrousflask.ProcSpellID1 = 0;
				threequarterfulltenebrousflask.SpellID1 = 0;
				threequarterfulltenebrousflask.MaxCharges1 = 0;
				threequarterfulltenebrousflask.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(threequarterfulltenebrousflask);
				}
			fullflaskoftenebrousessence = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "fullflaskoftenebrousessence");
			if (fullflaskoftenebrousessence == null)
			{
				fullflaskoftenebrousessence = new ItemTemplate();
				fullflaskoftenebrousessence.Name = "Full Flask of Tenebrous Essence";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + fullflaskoftenebrousessence.Name + ", creating it ...");
				fullflaskoftenebrousessence.Level = 1;
				fullflaskoftenebrousessence.Weight = 1;
				fullflaskoftenebrousessence.Model = 490;
				fullflaskoftenebrousessence.Object_Type = 0;
				fullflaskoftenebrousessence.Item_Type = 0;
				fullflaskoftenebrousessence.Id_nb = "fullflaskoftenebrousessence";
				fullflaskoftenebrousessence.Hand = 0;
				fullflaskoftenebrousessence.Platinum = 0;
				fullflaskoftenebrousessence.Gold = 0;
				fullflaskoftenebrousessence.Silver = 0;
				fullflaskoftenebrousessence.Copper = 0;
				fullflaskoftenebrousessence.IsPickable = true;
				fullflaskoftenebrousessence.IsDropable = true;
				fullflaskoftenebrousessence.IsTradable = true;
				fullflaskoftenebrousessence.CanDropAsLoot = false;
				fullflaskoftenebrousessence.Color = 0;
				fullflaskoftenebrousessence.Bonus = 0; // default bonus				
				fullflaskoftenebrousessence.Bonus1 = 0;
				fullflaskoftenebrousessence.Bonus1Type = (int) 0;
				fullflaskoftenebrousessence.Bonus2 = 0;
				fullflaskoftenebrousessence.Bonus2Type = (int) 0;
				fullflaskoftenebrousessence.Bonus3 = 0;
				fullflaskoftenebrousessence.Bonus3Type = (int) 0;
				fullflaskoftenebrousessence.Bonus4 = 0;
				fullflaskoftenebrousessence.Bonus4Type = (int) 0;
				fullflaskoftenebrousessence.Bonus5 = 0;
				fullflaskoftenebrousessence.Bonus5Type = (int) 0;
				fullflaskoftenebrousessence.Bonus6 = 0;
				fullflaskoftenebrousessence.Bonus6Type = (int) 0;
				fullflaskoftenebrousessence.Bonus7 = 0;
				fullflaskoftenebrousessence.Bonus7Type = (int) 0;
				fullflaskoftenebrousessence.Bonus8 = 0;
				fullflaskoftenebrousessence.Bonus8Type = (int) 0;
				fullflaskoftenebrousessence.Bonus9 = 0;
				fullflaskoftenebrousessence.Bonus9Type = (int) 0;
				fullflaskoftenebrousessence.Bonus10 = 0;
				fullflaskoftenebrousessence.Bonus10Type = (int) 0;
				fullflaskoftenebrousessence.ExtraBonus = 0;
				fullflaskoftenebrousessence.ExtraBonusType = (int) 0;
				fullflaskoftenebrousessence.Effect = 0;
				fullflaskoftenebrousessence.Emblem = 0;
				fullflaskoftenebrousessence.Charges = 0;
				fullflaskoftenebrousessence.MaxCharges = 0;
				fullflaskoftenebrousessence.SpellID = 0;
				fullflaskoftenebrousessence.ProcSpellID = 0;
				fullflaskoftenebrousessence.Type_Damage = 0;
				fullflaskoftenebrousessence.Realm = 0;
				fullflaskoftenebrousessence.MaxCount = 1;
				fullflaskoftenebrousessence.PackSize = 1;
				fullflaskoftenebrousessence.Extension = 0;
				fullflaskoftenebrousessence.Quality = 100;				
				fullflaskoftenebrousessence.Condition = 100;
				fullflaskoftenebrousessence.MaxCondition = 100;
				fullflaskoftenebrousessence.Durability = 100;
				fullflaskoftenebrousessence.MaxDurability = 100;
				fullflaskoftenebrousessence.PoisonCharges = 0;
				fullflaskoftenebrousessence.PoisonMaxCharges = 0;
				fullflaskoftenebrousessence.PoisonSpellID = 0;
				fullflaskoftenebrousessence.ProcSpellID1 = 0;
				fullflaskoftenebrousessence.SpellID1 = 0;
				fullflaskoftenebrousessence.MaxCharges1 = 0;
				fullflaskoftenebrousessence.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(fullflaskoftenebrousessence);
				}
			

			#endregion

			#region defineAreas
			
		#endregion
		
		#region defineQuestParts

		QuestBuilder builder = QuestMgr.getBuilder(typeof(Aidingguardalakyrr));
			QuestBehaviour a;
			a = builder.CreateBehaviour(GuardAlakyrr,-1);
				a.AddTrigger(eTriggerType.Interact,null,GuardAlakyrr);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),GuardAlakyrr);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),null,(eComparator)5);
			a.AddAction(eActionType.Talk,"You are new to this area aren't you? Yes, you look to be one of those from upper Albion. You will find that this lower realm is not as [familiar] as upper Albion and your Camelot.",GuardAlakyrr);
			AddBehaviour(a);
			a = builder.CreateBehaviour(GuardAlakyrr,-1);
				a.AddTrigger(eTriggerType.Whisper,"familiar",GuardAlakyrr);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),GuardAlakyrr);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),null,(eComparator)5);
			a.AddAction(eActionType.Talk,"Everything here is quite unlike upper Albion, from the surroundings to the creatures. We, the chosen of Arawn, are not even familiar with some of the [creatures] which lurk in the Aqueducts.",GuardAlakyrr);
			AddBehaviour(a);
			a = builder.CreateBehaviour(GuardAlakyrr,-1);
				a.AddTrigger(eTriggerType.Whisper,"creatures",GuardAlakyrr);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),GuardAlakyrr);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),null,(eComparator)5);
			a.AddAction(eActionType.Talk,"We have been trying to study some of these creatures so that we can learn more of their origins to better defend ourselves. Our forces are spread thin with the war against the evil army being [waged] below.",GuardAlakyrr);
			AddBehaviour(a);
			a = builder.CreateBehaviour(GuardAlakyrr,-1);
				a.AddTrigger(eTriggerType.Whisper,"waged",GuardAlakyrr);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),GuardAlakyrr);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),null,(eComparator)5);
			a.AddAction(eActionType.Talk,"We have not had enough troops to spare to help us combat some of these hostile beings. The [tenebrae] are some of these creatures.",GuardAlakyrr);
			AddBehaviour(a);
			a = builder.CreateBehaviour(GuardAlakyrr,-1);
				a.AddTrigger(eTriggerType.Whisper,"tenebrae",GuardAlakyrr);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),GuardAlakyrr);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),null,(eComparator)5);
			a.AddAction(eActionType.Talk,"They seem to hate all that is living, and it is an intense hate, indeed. Their origins are shrouded in mystery, but we do know that they were created out of [darkness].",GuardAlakyrr);
			AddBehaviour(a);
			a = builder.CreateBehaviour(GuardAlakyrr,-1);
				a.AddTrigger(eTriggerType.Whisper,"darkness",GuardAlakyrr);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),GuardAlakyrr);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),null,(eComparator)5);
			a.AddAction(eActionType.Talk,"The attacks by the tenebrae have been numerous, and we need to cease some of these attacks. I am authorized to provide money to any who can slay these tenebrae and bring me proof of their deed. Will you [aid] us in this time of need?",GuardAlakyrr);
			AddBehaviour(a);
			a = builder.CreateBehaviour(GuardAlakyrr,-1);
				a.AddTrigger(eTriggerType.Whisper,"aid",GuardAlakyrr);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),GuardAlakyrr);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),null,(eComparator)5);
			a.AddAction(eActionType.OfferQuest,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),"Will you slay the tenebrae for Guard Alakyrr? [Levels 1-4]");
			AddBehaviour(a);
			a = builder.CreateBehaviour(GuardAlakyrr,-1);
				a.AddTrigger(eTriggerType.DeclineQuest,null,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr));
			a.AddAction(eActionType.Talk,"No problem. See you",GuardAlakyrr);
			AddBehaviour(a);
			a = builder.CreateBehaviour(GuardAlakyrr,-1);
				a.AddTrigger(eTriggerType.AcceptQuest,null,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr));
			a.AddAction(eActionType.Talk,"Very well. Please travel south down this tunnel to what we guards call the Tenebrous Quarter and slay tenebrae. I will need you to collect some of their essence so that we can send it to our scholars to [study].",GuardAlakyrr);
			a.AddAction(eActionType.GiveQuest,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),GuardAlakyrr);
			AddBehaviour(a);
			a = builder.CreateBehaviour(GuardAlakyrr,-1);
				a.AddTrigger(eTriggerType.Whisper,"study",GuardAlakyrr);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),1,(eComparator)3);
			a.AddAction(eActionType.Talk,"You will need to slay tenebrae and then use an enchanted Tenebrous Flask, while inside the Tenebrous Quarter, to capture their essence. I need you to obtain a full flask of Tenebrous Essence. Return to me once you complete this duty and I will reward you for your efforts.",GuardAlakyrr);
			a.AddAction(eActionType.GiveItem,enchanterdtenebrousflask,GuardAlakyrr);
			a.AddAction(eActionType.IncQuestStep,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),null);
			AddBehaviour(a);
			a = builder.CreateBehaviour(GuardAlakyrr,-1);
				a.AddTrigger(eTriggerType.EnemyKilled,"tenebrous fighter",null);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),2,(eComparator)3);
			a.AddAction(eActionType.Message,"A tenebrous shadow is released. Use the Enchanted Tenebrous Flask to capture the Tenebrous Essence.",(eTextType)2);
			a.AddAction(eActionType.IncQuestStep,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),null);
			AddBehaviour(a);
			a = builder.CreateBehaviour(GuardAlakyrr,-1);
				a.AddTrigger(eTriggerType.ItemUsed,null,enchanterdtenebrousflask);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),3,(eComparator)3);
			a.AddAction(eActionType.Message,"You attempt to use the Enchanted Tenebrous Flask.",(eTextType)2);
			a.AddAction(eActionType.Message,"You use the Enchanted Tenebrous Flask.",(eTextType)2);
			a.AddAction(eActionType.Message,"The flask is one quarter full.",(eTextType)2);
			a.AddAction(eActionType.ReplaceItem,enchanterdtenebrousflask,quarterfulltenebrousflask);
			a.AddAction(eActionType.IncQuestStep,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),null);
			AddBehaviour(a);
			a = builder.CreateBehaviour(GuardAlakyrr,-1);
				a.AddTrigger(eTriggerType.EnemyKilled,"tenebrous fighter",null);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),4,(eComparator)3);
			a.AddAction(eActionType.Message,"A tenebrous shadow is released. Use the Enchanted Tenebrous Flask to capture the Tenebrous Essence.",(eTextType)2);
			a.AddAction(eActionType.IncQuestStep,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),null);
			AddBehaviour(a);
			a = builder.CreateBehaviour(GuardAlakyrr,-1);
				a.AddTrigger(eTriggerType.ItemUsed,null,quarterfulltenebrousflask);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),5,(eComparator)3);
			a.AddAction(eActionType.Message,"You attempt to use the Quarter Full Tenebrous Flask.",(eTextType)2);
			a.AddAction(eActionType.Message,"You use the Quarter Full Tenebrous Flask. ",(eTextType)2);
			a.AddAction(eActionType.Message,"The flask is half full.",(eTextType)2);
			a.AddAction(eActionType.IncQuestStep,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),null);
			a.AddAction(eActionType.ReplaceItem,quarterfulltenebrousflask,halffulltenebrousflask);
			AddBehaviour(a);
			a = builder.CreateBehaviour(GuardAlakyrr,-1);
				a.AddTrigger(eTriggerType.EnemyKilled,"tenebrous fighter",null);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),6,(eComparator)3);
			a.AddAction(eActionType.Message,"A tenebrous shadow is released. Use the Enchanted Tenebrous Flask to capture the Tenebrous Essence.",(eTextType)2);
			a.AddAction(eActionType.IncQuestStep,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),null);
			AddBehaviour(a);
			a = builder.CreateBehaviour(GuardAlakyrr,-1);
				a.AddTrigger(eTriggerType.ItemUsed,null,halffulltenebrousflask);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),7,(eComparator)3);
			a.AddAction(eActionType.Message,"You attempt to use the Half Full Tenebrous Flask.",(eTextType)2);
			a.AddAction(eActionType.Message,"You use the Half Full Tenebrous Flask.",(eTextType)2);
			a.AddAction(eActionType.Message,"The flask is three quarters full.",(eTextType)2);
			a.AddAction(eActionType.IncQuestStep,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),null);
			a.AddAction(eActionType.ReplaceItem,halffulltenebrousflask,threequarterfulltenebrousflask);
			AddBehaviour(a);
			a = builder.CreateBehaviour(GuardAlakyrr,-1);
				a.AddTrigger(eTriggerType.EnemyKilled,"tenebrous fighter",null);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),8,(eComparator)3);
			a.AddAction(eActionType.Message,"A tenebrous shadow is released. Use the Enchanted Tenebrous Flask to capture the Tenebrous Essence.",(eTextType)2);
			a.AddAction(eActionType.IncQuestStep,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),null);
			AddBehaviour(a);
			a = builder.CreateBehaviour(GuardAlakyrr,-1);
				a.AddTrigger(eTriggerType.ItemUsed,null,threequarterfulltenebrousflask);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),9,(eComparator)3);
			a.AddAction(eActionType.Message,"You attempt to use the 3 Quarters Full Tenebrous Flask.",(eTextType)2);
			a.AddAction(eActionType.Message,"You use the 3 Quarters Full Tenebrous Flask.",(eTextType)2);
			a.AddAction(eActionType.Message,"You fill the flask with Tenebrous Essence.",(eTextType)2);
			a.AddAction(eActionType.IncQuestStep,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),null);
			a.AddAction(eActionType.ReplaceItem,threequarterfulltenebrousflask,fullflaskoftenebrousessence);
			AddBehaviour(a);
			a = builder.CreateBehaviour(GuardAlakyrr,-1);
				a.AddTrigger(eTriggerType.Interact,null,GuardAlakyrr);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),10,(eComparator)3);
			a.AddAction(eActionType.Talk,"It was Arawn's will to allow your return. I am grateful that you made it back. Please give me the Full Flask of Tenebrous Essence.",GuardAlakyrr);
			a.AddAction(eActionType.IncQuestStep,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),null);
			AddBehaviour(a);
			a = builder.CreateBehaviour(GuardAlakyrr,-1);
				a.AddTrigger(eTriggerType.GiveItem,GuardAlakyrr,fullflaskoftenebrousessence);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),11,(eComparator)3);
			a.AddAction(eActionType.Talk,"Thank you. This is a promising specimen for study. Please take this coin as a show of our appreciation. Blessings of Arawn be upon you.",GuardAlakyrr);
			a.AddAction(eActionType.TakeItem,fullflaskoftenebrousessence,null);
			a.AddAction(eActionType.GiveXP,60,null);
			a.AddAction(eActionType.GiveGold,27,null);
			a.AddAction(eActionType.FinishQuest,typeof(DOL.GS.Quests.Albion.Aidingguardalakyrr),null);
			AddBehaviour(a);
			
			#endregion

			// Custom Scriptloaded Code Begin
			
			// Custom Scriptloaded Code End
			if (GuardAlakyrr!=null) {
				GuardAlakyrr.AddQuestToGive(typeof (Aidingguardalakyrr));
			}
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{			
				
			// Custom Scriptunloaded Code Begin
			
			// Custom Scriptunloaded Code End

			

			/* If GuardAlakyrr has not been initialized, then we don't have to remove any
			 * hooks from him ;-)
			 */
			if (GuardAlakyrr == null)
				return;
			/* Now we remove the possibility to give this quest to players */			
			GuardAlakyrr.RemoveQuestToGive(typeof (Aidingguardalakyrr));
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
						return "[Step #1]  Speak with Guard Alakyrr about the tenebrae.";
				
					case 2:
						return "[Step #2] Go south of Guard Alakyrr to the Tenebrous Quarter and slay tenebrous fighters. After you kill them use the flask to capture their essence. You must do this while inside the Tenebrous Quarter. You must obtain a full flask of essence.";
				
					case 3:
						return "[Step #3] /Use the Enchanted Tenebrous Flask to obtain the tenebrous essence. You must use this item while inside the Tenebrous Quarter. (To use an item right click on the item and type /use).";
				
					case 4:
						return "[Step #4] Go south of Guard Alakyrr to the Tenebrous Quarter and slay tenebrous fighters. After you kill them use the flask to capture their essence. You must do this while inside the Tenebrous Quarter. You must obtain a full flask of essence.";
				
					case 5:
						return "[Step #5] /Use the Enchanted Tenebrous Flask to obtain the tenebrous essence. You must use this item while inside the Tenebrous Quarter. (To use an item right click on the item and type /use).";
				
					case 6:
						return "[Step #6] Go south of Guard Alakyrr to the Tenebrous Quarter and slay tenebrous fighters. After you kill them use the flask to capture their essence. You must do this while inside the Tenebrous Quarter. You must obtain a full flask of essence.";
				
					case 7:
						return "[Step #7] /Use the Enchanted Tenebrous Flask to obtain the tenebrous essence. You must use this item while inside the Tenebrous Quarter. (To use an item right click on the item and type /use).";
				
					case 8:
						return "[Step #8] Go south of Guard Alakyrr to the Tenebrous Quarter and slay tenebrous fighters. After you kill them use the flask to capture their essence. You must do this while inside the Tenebrous Quarter. You must obtain a full flask of essence.";
				
					case 9:
						return "[Step #9] /Use the Enchanted Tenebrous Flask to obtain the tenebrous essence. You must use this item while inside the Tenebrous Quarter. (To use an item right click on the item and type /use).";
				
					case 10:
						return "[Step #10] Return to Guard Alakyrr with the Full Flask of Tenebrous Essence.";
				
					case 11:
						return "[Step #11] Give Guard Alakyrr the Full Flask of Tenebrous Essence to receive your reward.";
				
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
			if (player.IsDoingQuest(typeof (Aidingguardalakyrr)) != null)
				return true;
				
			// Custom Code Begin
			
			// Custom  Code End
			
		
			if (player.Level > maximumLevel || player.Level < minimumLevel )
				return false;
		
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
