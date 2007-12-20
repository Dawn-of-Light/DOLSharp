	
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

	namespace DOL.GS.Quests.Midgard {
	
     /* The first thing we do, is to declare the class we create
	 * as Quest. To do this, we derive from the abstract class
	 * BaseQuest	  	 
	 */
	public class sveabonehiltsword : BaseQuest
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

		protected const string questTitle = "Sveabone Hilt Sword";

		protected const int minimumLevel = 1;
		protected const int maximumLevel = 5;
	
	
		private static GameNPC Gridash = null;
		
		private static ItemTemplate bronze_short_sword = null;
		
		private static ItemTemplate sveawolftooth = null;
		
		private static ItemTemplate sveabone_hilt_sword = null;
		

		// Custom Initialization Code Begin
		
		// Custom Initialization Code End

		/* 
		* Constructor
		*/
		public sveabonehiltsword() : base()
		{
		}

		public sveabonehiltsword(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public sveabonehiltsword(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public sveabonehiltsword(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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
	
			npcs = WorldMgr.GetNPCsByName("Gridash",(eRealm) 2);
			if (npcs.Length == 0)
			{
				if (!WorldMgr.GetRegion(100).IsDisabled)
				{
				Gridash = new DOL.GS.GameNPC();
					Gridash.Model = 137;
				Gridash.Name = "Gridash";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Gridash.Name + ", creating ...");
				Gridash.GuildName = "Part of " + questTitle + " Quest";
				Gridash.Realm = (byte) 2;
				Gridash.CurrentRegionID = 100;
				Gridash.Size = 51;
				Gridash.Level = 21;
				Gridash.MaxSpeedBase = 191;
				Gridash.Faction = FactionMgr.GetFactionByID(0);
				Gridash.X = 772795;
				Gridash.Y = 753335;
				Gridash.Z = 4600;
				Gridash.Heading = 3356;
				Gridash.RespawnInterval = -1;
				Gridash.BodyType = 0;
				

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 500;
				Gridash.SetOwnBrain(brain);
				
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					Gridash.SaveIntoDatabase();
					
				Gridash.AddToWorld();
				
				}
			}
			else 
			{
				Gridash = npcs[0];
			}
		

			#endregion

			#region defineItems

		bronze_short_sword = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "bronze_short_sword");
			if (bronze_short_sword == null)
			{
				bronze_short_sword = new ItemTemplate();
				bronze_short_sword.Name = "bronze short sword";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + bronze_short_sword.Name + ", creating it ...");
				bronze_short_sword.Level = 1;
				bronze_short_sword.Weight = 180;
				bronze_short_sword.Model = 3;
				bronze_short_sword.Object_Type = 3;
				bronze_short_sword.Item_Type = 11;
				bronze_short_sword.Id_nb = "bronze_short_sword";
				bronze_short_sword.Hand = 2;
				bronze_short_sword.Platinum = 0;
				bronze_short_sword.Gold = 0;
				bronze_short_sword.Silver = 1;
				bronze_short_sword.Copper = 50;
				bronze_short_sword.IsPickable = true;
				bronze_short_sword.IsDropable = true;
				bronze_short_sword.IsTradable = false;
				bronze_short_sword.CanDropAsLoot = true;
				bronze_short_sword.Color = 0;
				bronze_short_sword.Bonus = 0; // default bonus				
				bronze_short_sword.Bonus1 = 0;
				bronze_short_sword.Bonus1Type = (int) 0;
				bronze_short_sword.Bonus2 = 0;
				bronze_short_sword.Bonus2Type = (int) 0;
				bronze_short_sword.Bonus3 = 0;
				bronze_short_sword.Bonus3Type = (int) 0;
				bronze_short_sword.Bonus4 = 0;
				bronze_short_sword.Bonus4Type = (int) 0;
				bronze_short_sword.Bonus5 = 0;
				bronze_short_sword.Bonus5Type = (int) 0;
				bronze_short_sword.Bonus6 = 0;
				bronze_short_sword.Bonus6Type = (int) 0;
				bronze_short_sword.Bonus7 = 0;
				bronze_short_sword.Bonus7Type = (int) 0;
				bronze_short_sword.Bonus8 = 0;
				bronze_short_sword.Bonus8Type = (int) 0;
				bronze_short_sword.Bonus9 = 0;
				bronze_short_sword.Bonus9Type = (int) 0;
				bronze_short_sword.Bonus10 = 0;
				bronze_short_sword.Bonus10Type = (int) 0;
				bronze_short_sword.ExtraBonus = 0;
				bronze_short_sword.ExtraBonusType = (int) 0;
				bronze_short_sword.Effect = 0;
				bronze_short_sword.Emblem = 0;
				bronze_short_sword.Charges = 0;
				bronze_short_sword.MaxCharges = 0;
				bronze_short_sword.SpellID = 0;
				bronze_short_sword.ProcSpellID = 0;
				bronze_short_sword.Type_Damage = 2;
				bronze_short_sword.Realm = 1;
				bronze_short_sword.MaxCount = 1;
				bronze_short_sword.PackSize = 1;
				bronze_short_sword.Extension = 0;
				bronze_short_sword.Quality = 85;				
				bronze_short_sword.Condition = 50000;
				bronze_short_sword.MaxCondition = 50000;
				bronze_short_sword.Durability = 100;
				bronze_short_sword.MaxDurability = 100;
				bronze_short_sword.PoisonCharges = 0;
				bronze_short_sword.PoisonMaxCharges = 0;
				bronze_short_sword.PoisonSpellID = 0;
				bronze_short_sword.ProcSpellID1 = 0;
				bronze_short_sword.SpellID1 = 0;
				bronze_short_sword.MaxCharges1 = 0;
				bronze_short_sword.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(bronze_short_sword);
				}
			sveawolftooth = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "sveawolftooth");
			if (sveawolftooth == null)
			{
				sveawolftooth = new ItemTemplate();
				sveawolftooth.Name = "Sveawolf Tooth";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + sveawolftooth.Name + ", creating it ...");
				sveawolftooth.Level = 50;
				sveawolftooth.Weight = 5;
				sveawolftooth.Model = 106;
				sveawolftooth.Object_Type = 0;
				sveawolftooth.Item_Type = 40;
				sveawolftooth.Id_nb = "sveawolftooth";
				sveawolftooth.Hand = 0;
				sveawolftooth.Platinum = 0;
				sveawolftooth.Gold = 0;
				sveawolftooth.Silver = 0;
				sveawolftooth.Copper = 0;
				sveawolftooth.IsPickable = true;
				sveawolftooth.IsDropable = true;
				sveawolftooth.IsTradable = true;
				sveawolftooth.CanDropAsLoot = false;
				sveawolftooth.Color = 0;
				sveawolftooth.Bonus = 35; // default bonus				
				sveawolftooth.Bonus1 = 0;
				sveawolftooth.Bonus1Type = (int) 0;
				sveawolftooth.Bonus2 = 0;
				sveawolftooth.Bonus2Type = (int) 0;
				sveawolftooth.Bonus3 = 0;
				sveawolftooth.Bonus3Type = (int) 0;
				sveawolftooth.Bonus4 = 0;
				sveawolftooth.Bonus4Type = (int) 0;
				sveawolftooth.Bonus5 = 0;
				sveawolftooth.Bonus5Type = (int) 0;
				sveawolftooth.Bonus6 = 0;
				sveawolftooth.Bonus6Type = (int) 0;
				sveawolftooth.Bonus7 = 0;
				sveawolftooth.Bonus7Type = (int) 0;
				sveawolftooth.Bonus8 = 0;
				sveawolftooth.Bonus8Type = (int) 0;
				sveawolftooth.Bonus9 = 0;
				sveawolftooth.Bonus9Type = (int) 0;
				sveawolftooth.Bonus10 = 0;
				sveawolftooth.Bonus10Type = (int) 0;
				sveawolftooth.ExtraBonus = 0;
				sveawolftooth.ExtraBonusType = (int) 0;
				sveawolftooth.Effect = 0;
				sveawolftooth.Emblem = 0;
				sveawolftooth.Charges = 0;
				sveawolftooth.MaxCharges = 0;
				sveawolftooth.SpellID = 0;
				sveawolftooth.ProcSpellID = 0;
				sveawolftooth.Type_Damage = 0;
				sveawolftooth.Realm = 0;
				sveawolftooth.MaxCount = 1;
				sveawolftooth.PackSize = 1;
				sveawolftooth.Extension = 0;
				sveawolftooth.Quality = 99;				
				sveawolftooth.Condition = 50000;
				sveawolftooth.MaxCondition = 50000;
				sveawolftooth.Durability = 50000;
				sveawolftooth.MaxDurability = 50000;
				sveawolftooth.PoisonCharges = 0;
				sveawolftooth.PoisonMaxCharges = 0;
				sveawolftooth.PoisonSpellID = 0;
				sveawolftooth.ProcSpellID1 = 0;
				sveawolftooth.SpellID1 = 0;
				sveawolftooth.MaxCharges1 = 0;
				sveawolftooth.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(sveawolftooth);
				}
			sveabone_hilt_sword = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "sveabone_hilt_sword");
			if (sveabone_hilt_sword == null)
			{
				sveabone_hilt_sword = new ItemTemplate();
				sveabone_hilt_sword.Name = "Sveabone Hilt Sword";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + sveabone_hilt_sword.Name + ", creating it ...");
				sveabone_hilt_sword.Level = 1;
				sveabone_hilt_sword.Weight = 18;
				sveabone_hilt_sword.Model = 311;
				sveabone_hilt_sword.Object_Type = 11;
				sveabone_hilt_sword.Item_Type = 10;
				sveabone_hilt_sword.Id_nb = "sveabone_hilt_sword";
				sveabone_hilt_sword.Hand = 0;
				sveabone_hilt_sword.Platinum = 0;
				sveabone_hilt_sword.Gold = 0;
				sveabone_hilt_sword.Silver = 0;
				sveabone_hilt_sword.Copper = 0;
				sveabone_hilt_sword.IsPickable = true;
				sveabone_hilt_sword.IsDropable = true;
				sveabone_hilt_sword.IsTradable = false;
				sveabone_hilt_sword.CanDropAsLoot = false;
				sveabone_hilt_sword.Color = 0;
				sveabone_hilt_sword.Bonus = 0; // default bonus				
				sveabone_hilt_sword.Bonus1 = 1;
				sveabone_hilt_sword.Bonus1Type = (int) 52;
				sveabone_hilt_sword.Bonus2 = 0;
				sveabone_hilt_sword.Bonus2Type = (int) 0;
				sveabone_hilt_sword.Bonus3 = 0;
				sveabone_hilt_sword.Bonus3Type = (int) 0;
				sveabone_hilt_sword.Bonus4 = 0;
				sveabone_hilt_sword.Bonus4Type = (int) 0;
				sveabone_hilt_sword.Bonus5 = 0;
				sveabone_hilt_sword.Bonus5Type = (int) 0;
				sveabone_hilt_sword.Bonus6 = 0;
				sveabone_hilt_sword.Bonus6Type = (int) 0;
				sveabone_hilt_sword.Bonus7 = 0;
				sveabone_hilt_sword.Bonus7Type = (int) 0;
				sveabone_hilt_sword.Bonus8 = 0;
				sveabone_hilt_sword.Bonus8Type = (int) 0;
				sveabone_hilt_sword.Bonus9 = 0;
				sveabone_hilt_sword.Bonus9Type = (int) 0;
				sveabone_hilt_sword.Bonus10 = 0;
				sveabone_hilt_sword.Bonus10Type = (int) 0;
				sveabone_hilt_sword.ExtraBonus = 0;
				sveabone_hilt_sword.ExtraBonusType = (int) 0;
				sveabone_hilt_sword.Effect = 0;
				sveabone_hilt_sword.Emblem = 0;
				sveabone_hilt_sword.Charges = 0;
				sveabone_hilt_sword.MaxCharges = 0;
				sveabone_hilt_sword.SpellID = 0;
				sveabone_hilt_sword.ProcSpellID = 0;
				sveabone_hilt_sword.Type_Damage = 2;
				sveabone_hilt_sword.Realm = 0;
				sveabone_hilt_sword.MaxCount = 1;
				sveabone_hilt_sword.PackSize = 1;
				sveabone_hilt_sword.Extension = 0;
				sveabone_hilt_sword.Quality = 99;				
				sveabone_hilt_sword.Condition = 100;
				sveabone_hilt_sword.MaxCondition = 100;
				sveabone_hilt_sword.Durability = 100;
				sveabone_hilt_sword.MaxDurability = 100;
				sveabone_hilt_sword.PoisonCharges = 0;
				sveabone_hilt_sword.PoisonMaxCharges = 0;
				sveabone_hilt_sword.PoisonSpellID = 0;
				sveabone_hilt_sword.ProcSpellID1 = 0;
				sveabone_hilt_sword.SpellID1 = 0;
				sveabone_hilt_sword.MaxCharges1 = 0;
				sveabone_hilt_sword.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(sveabone_hilt_sword);
				}
			

			#endregion

			#region defineAreas
			
		#endregion
		
		#region defineQuestParts

		QuestBuilder builder = QuestMgr.getBuilder(typeof(sveabonehiltsword));
			QuestBehaviour a;
			a = builder.CreateBehaviour(Gridash,-1);
				a.AddTrigger(eTriggerType.Interact,null,Gridash);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.sveabonehiltsword),Gridash);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.sveabonehiltsword),null,(eComparator)5);
			a.AddAction(eActionType.Talk,"Hail young one! I have noticed that you do not carry one of my fine [bone hilt swords].",Gridash);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Gridash,-1);
				a.AddTrigger(eTriggerType.Whisper,"bone hilt swords",Gridash);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.sveabonehiltsword),Gridash);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.sveabonehiltsword),null,(eComparator)5);
			a.AddAction(eActionType.Talk,"Aye! They are some of my finest works! I don't suppose you [would care for one] eh?",Gridash);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Gridash,-1);
				a.AddTrigger(eTriggerType.Whisper,"would care for one",Gridash);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.sveabonehiltsword),Gridash);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.sveabonehiltsword),null,(eComparator)5);
			a.AddAction(eActionType.OfferQuest,typeof(DOL.GS.Quests.Midgard.sveabonehiltsword),"Will you do what is asked so that Gridash can make a Sveabone hilt sword?");
			AddBehaviour(a);
			a = builder.CreateBehaviour(Gridash,-1);
				a.AddTrigger(eTriggerType.DeclineQuest,null,typeof(DOL.GS.Quests.Midgard.sveabonehiltsword));
			a.AddAction(eActionType.Talk,"No problem. See you",Gridash);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Gridash,-1);
				a.AddTrigger(eTriggerType.AcceptQuest,null,typeof(DOL.GS.Quests.Midgard.sveabonehiltsword));
			a.AddAction(eActionType.Talk,"Oh great then! Run along and buy me a bronze short sword. I should be ready once you return.",Gridash);
			a.AddAction(eActionType.GiveQuest,typeof(DOL.GS.Quests.Midgard.sveabonehiltsword),Gridash);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Gridash,-1);
				a.AddTrigger(eTriggerType.GiveItem,Gridash,bronze_short_sword);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.sveabonehiltsword),null);
			a.AddAction(eActionType.Talk,"Good work! I cannot seem to find anymore sveawolf teeth to fashion your weapon. Find one and return it to me.",Gridash);
			a.AddAction(eActionType.TakeItem,bronze_short_sword,null);
			a.AddAction(eActionType.IncQuestStep,typeof(sveabonehiltsword),null);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Gridash,-1);
				a.AddTrigger(eTriggerType.EnemyKilled,"young sveawof",null);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.sveabonehiltsword),2,(eComparator)3);
			a.AddAction(eActionType.GiveItem,sveawolftooth,null);
			a.AddAction(eActionType.IncQuestStep,typeof(DOL.GS.Quests.Midgard.sveabonehiltsword),null);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Gridash,-1);
				a.AddTrigger(eTriggerType.GiveItem,Gridash,sveawolftooth);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.sveabonehiltsword),3,(eComparator)3);
			a.AddAction(eActionType.Talk,"Here is your completed work. I hope it serves you well!",Gridash);
			a.AddAction(eActionType.TakeItem,sveawolftooth,null);
			a.AddAction(eActionType.GiveItem,sveabone_hilt_sword,Gridash);
			a.AddAction(eActionType.GiveXP,20,null);
			a.AddAction(eActionType.GiveGold,450,null);
			a.AddAction(eActionType.FinishQuest,typeof(DOL.GS.Quests.Midgard.sveabonehiltsword),null);
			AddBehaviour(a);
			
			#endregion

			// Custom Scriptloaded Code Begin
			
			// Custom Scriptloaded Code End
			if (Gridash!=null) {
				Gridash.AddQuestToGive(typeof (sveabonehiltsword));
			}
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{			
				
			// Custom Scriptunloaded Code Begin
			
			// Custom Scriptunloaded Code End

			

			/* If Gridash has not been initialized, then we don't have to remove any
			 * hooks from him ;-)
			 */
			if (Gridash == null)
				return;
			/* Now we remove the possibility to give this quest to players */			
			Gridash.RemoveQuestToGive(typeof (sveabonehiltsword));
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
						return "[Step #1] Buy a bronze short sword and return it to Gridash";
				
					case 2:
						return "[Step #2] Kill a sveawolf and return to Gridash with it's tooth.";
				
					case 3:
						return "[Step #3] Return to Gridash in East Svealand with a sveawolf tooth.";
				
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
			if (player.IsDoingQuest(typeof (sveabonehiltsword)) != null)
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
