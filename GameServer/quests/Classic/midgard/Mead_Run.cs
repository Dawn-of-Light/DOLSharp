	
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
	public class meadrun : BaseQuest
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

		protected const string questTitle = "Mead Run";

		protected const int minimumLevel = 1;
		protected const int maximumLevel = 4;
	
	
		private static GameNPC Audun = null;
		
		private static GameNPC GuardOlja = null;
		
		private static ItemTemplate emptybottle = null;
		
		private static ItemTemplate bottleofmead = null;
		

		// Custom Initialization Code Begin
		
		// Custom Initialization Code End

		/* 
		* Constructor
		*/
		public meadrun() : base()
		{
		}

		public meadrun(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public meadrun(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public meadrun(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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
	
			npcs = WorldMgr.GetNPCsByName("Audun",(eRealm) 2);
			if (npcs.Length == 0)
			{
				if (!WorldMgr.GetRegion(101).IsDisabled)
				{
				Audun = new DOL.GS.GameNPC();
					Audun.Model = 232;
				Audun.Name = "Audun";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Audun.Name + ", creating ...");
				Audun.GuildName = "Part of " + questTitle + " Quest";
				Audun.Realm = eRealm.Midgard;
				Audun.CurrentRegionID = 101;
				Audun.Size = 48;
				Audun.Level = 49;
				Audun.MaxSpeedBase = 191;
				Audun.Faction = FactionMgr.GetFactionByID(0);
				Audun.X = 33283;
				Audun.Y = 35305;
				Audun.Z = 8027;
				Audun.Heading = 1763;
				Audun.RespawnInterval = -1;
				Audun.BodyType = 0;
				

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 500;
				Audun.SetOwnBrain(brain);
				
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					Audun.SaveIntoDatabase();
					
				Audun.AddToWorld();
				
				}
			}
			else 
			{
				Audun = npcs[0];
			}
		
			npcs = WorldMgr.GetNPCsByName("Guard Olja",(eRealm) 2);
			if (npcs.Length == 0)
			{
				if (!WorldMgr.GetRegion(229).IsDisabled)
				{
				GuardOlja = new DOL.GS.GameNPC();
					GuardOlja.Model = 180;
				GuardOlja.Name = "Guard Olja";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + GuardOlja.Name + ", creating ...");
				GuardOlja.GuildName = "Part of " + questTitle + " Quest";
				GuardOlja.Realm = eRealm.Midgard;
				GuardOlja.CurrentRegionID = 229;
				GuardOlja.Size = 50;
				GuardOlja.Level = 50;
				GuardOlja.MaxSpeedBase = 191;
				GuardOlja.Faction = FactionMgr.GetFactionByID(0);
				GuardOlja.X = 47994;
				GuardOlja.Y = 37341;
				GuardOlja.Z = 21812;
				GuardOlja.Heading = 204;
				GuardOlja.RespawnInterval = -1;
				GuardOlja.BodyType = 0;
				

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 500;
				GuardOlja.SetOwnBrain(brain);
				
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GuardOlja.SaveIntoDatabase();
					
				GuardOlja.AddToWorld();
				
				}
			}
			else 
			{
				GuardOlja = npcs[0];
			}
		

			#endregion

			#region defineItems

		emptybottle = GameServer.Database.FindObjectByKey<ItemTemplate>("emptybottle");
			if (emptybottle == null)
			{
				emptybottle = new ItemTemplate();
				emptybottle.Name = "Empty Bottle";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + emptybottle.Name + ", creating it ...");
				emptybottle.Level = 50;
				emptybottle.Weight = 5;
				emptybottle.Model = 554;
				emptybottle.Object_Type = 0;
				emptybottle.Item_Type = 40;
				emptybottle.Id_nb = "emptybottle";
				emptybottle.Hand = 0;
				emptybottle.Platinum = 0;
				emptybottle.Gold = 0;
				emptybottle.Silver = 0;
				emptybottle.Copper = 0;
				emptybottle.IsPickable = true;
				emptybottle.IsDropable = true;
				emptybottle.IsTradable = true;
				emptybottle.CanDropAsLoot = false;
				emptybottle.Color = 0;
				emptybottle.Bonus = 35; // default bonus				
				emptybottle.Bonus1 = 0;
				emptybottle.Bonus1Type = (int) 0;
				emptybottle.Bonus2 = 0;
				emptybottle.Bonus2Type = (int) 0;
				emptybottle.Bonus3 = 0;
				emptybottle.Bonus3Type = (int) 0;
				emptybottle.Bonus4 = 0;
				emptybottle.Bonus4Type = (int) 0;
				emptybottle.Bonus5 = 0;
				emptybottle.Bonus5Type = (int) 0;
				emptybottle.Bonus6 = 0;
				emptybottle.Bonus6Type = (int) 0;
				emptybottle.Bonus7 = 0;
				emptybottle.Bonus7Type = (int) 0;
				emptybottle.Bonus8 = 0;
				emptybottle.Bonus8Type = (int) 0;
				emptybottle.Bonus9 = 0;
				emptybottle.Bonus9Type = (int) 0;
				emptybottle.Bonus10 = 0;
				emptybottle.Bonus10Type = (int) 0;
				emptybottle.ExtraBonus = 0;
				emptybottle.ExtraBonusType = (int) 0;
				emptybottle.Effect = 0;
				emptybottle.Emblem = 0;
				emptybottle.Charges = 0;
				emptybottle.MaxCharges = 0;
				emptybottle.SpellID = 0;
				emptybottle.ProcSpellID = 0;
				emptybottle.Type_Damage = 0;
				emptybottle.Realm = 0;
				emptybottle.MaxCount = 1;
				emptybottle.PackSize = 1;
				emptybottle.Extension = 0;
				emptybottle.Quality = 99;				
				emptybottle.Condition = 100;
				emptybottle.MaxCondition = 100;
				emptybottle.Durability = 100;
				emptybottle.MaxDurability = 100;
				emptybottle.PoisonCharges = 0;
				emptybottle.PoisonMaxCharges = 0;
				emptybottle.PoisonSpellID = 0;
				emptybottle.ProcSpellID1 = 0;
				emptybottle.SpellID1 = 0;
				emptybottle.MaxCharges1 = 0;
				emptybottle.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(emptybottle);
				}
			bottleofmead = GameServer.Database.FindObjectByKey<ItemTemplate>("bottleofmead");
			if (bottleofmead == null)
			{
				bottleofmead = new ItemTemplate();
				bottleofmead.Name = "Bottle of Mead";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + bottleofmead.Name + ", creating it ...");
				bottleofmead.Level = 50;
				bottleofmead.Weight = 5;
				bottleofmead.Model = 554;
				bottleofmead.Object_Type = 0;
				bottleofmead.Item_Type = 40;
				bottleofmead.Id_nb = "bottleofmead";
				bottleofmead.Hand = 0;
				bottleofmead.Platinum = 0;
				bottleofmead.Gold = 0;
				bottleofmead.Silver = 0;
				bottleofmead.Copper = 0;
				bottleofmead.IsPickable = true;
				bottleofmead.IsDropable = true;
				bottleofmead.IsTradable = true;
				bottleofmead.CanDropAsLoot = false;
				bottleofmead.Color = 0;
				bottleofmead.Bonus = 35; // default bonus				
				bottleofmead.Bonus1 = 0;
				bottleofmead.Bonus1Type = (int)0;
				bottleofmead.Bonus2 = 0;
				bottleofmead.Bonus2Type = (int)0;
				bottleofmead.Bonus3 = 0;
				bottleofmead.Bonus3Type = (int)0;
				bottleofmead.Bonus4 = 0;
				bottleofmead.Bonus4Type = (int)0;
				bottleofmead.Bonus5 = 0;
				bottleofmead.Bonus5Type = (int)0;
				bottleofmead.Bonus6 = 0;
				bottleofmead.Bonus6Type = (int)0;
				bottleofmead.Bonus7 = 0;
				bottleofmead.Bonus7Type = (int)0;
				bottleofmead.Bonus8 = 0;
				bottleofmead.Bonus8Type = (int)0;
				bottleofmead.Bonus9 = 0;
				bottleofmead.Bonus9Type = (int)0;
				bottleofmead.Bonus10 = 0;
				bottleofmead.Bonus10Type = (int)0;
				bottleofmead.ExtraBonus = 0;
				bottleofmead.ExtraBonusType = (int)0;
				bottleofmead.Effect = 0;
				bottleofmead.Emblem = 0;
				bottleofmead.Charges = 0;
				bottleofmead.MaxCharges = 0;
				bottleofmead.SpellID = 0;
				bottleofmead.ProcSpellID = 0;
				bottleofmead.Type_Damage = 0;
				bottleofmead.Realm = 0;
				bottleofmead.MaxCount = 1;
				bottleofmead.PackSize = 1;
				bottleofmead.Extension = 0;
				bottleofmead.Quality = 99;
				bottleofmead.Condition = 100;
				bottleofmead.MaxCondition = 100;
				bottleofmead.Durability = 100;
				bottleofmead.MaxDurability = 100;
				bottleofmead.PoisonCharges = 0;
				bottleofmead.PoisonMaxCharges = 0;
				bottleofmead.PoisonSpellID = 0;
				bottleofmead.ProcSpellID1 = 0;
				bottleofmead.SpellID1 = 0;
				bottleofmead.MaxCharges1 = 0;
				bottleofmead.Charges1 = 0;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(bottleofmead);
			}
			

			#endregion

			#region defineAreas
			
		#endregion
		
		#region defineQuestParts

		QuestBuilder builder = QuestMgr.getBuilder(typeof(meadrun));
			QuestBehaviour a;
			a = builder.CreateBehaviour(Audun,-1);
				a.AddTrigger(eTriggerType.Interact,null,Audun);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.meadrun),Audun);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.meadrun),null,(eComparator)5);
			a.AddAction(eActionType.Talk,"Greetings. You appear to be down on your luck. I have a [proposition] for you if you're interested.",null);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Audun,-1);
				a.AddTrigger(eTriggerType.Whisper,"proposition",Audun);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.meadrun),Audun);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.meadrun),null,(eComparator)5);
			a.AddAction(eActionType.Talk,"I need to deliver some mead to the guards just inside the Burial Grounds. There is a bit of coin to be had if you would deliver the mead for me.",Audun);
			a.AddAction(eActionType.OfferQuest,typeof(DOL.GS.Quests.Midgard.meadrun),"Will you deliver the mead for Audun? [Levels 1-4]");
			AddBehaviour(a);
			a = builder.CreateBehaviour(Audun,-1);
				a.AddTrigger(eTriggerType.DeclineQuest,null,typeof(DOL.GS.Quests.Midgard.meadrun));
			a.AddAction(eActionType.Talk,"No problem. See you",Audun);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Audun,-1);
				a.AddTrigger(eTriggerType.AcceptQuest,null,typeof(DOL.GS.Quests.Midgard.meadrun));
			a.AddAction(eActionType.Talk,"Here take the mead to Guard Olja inside the entrance of the Burial Grounds.",Audun);
			a.AddAction(eActionType.GiveItem,bottleofmead,Audun);
			a.AddAction(eActionType.GiveQuest,typeof(DOL.GS.Quests.Midgard.meadrun),Audun);
			AddBehaviour(a);
			a = builder.CreateBehaviour(GuardOlja,-1);
				a.AddTrigger(eTriggerType.GiveItem,GuardOlja,bottleofmead);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.meadrun),null);
			a.AddAction(eActionType.Talk,"Thanks. Here, take this empty bottle back to Auduan.",GuardOlja);
			a.AddAction(eActionType.GiveItem,emptybottle,GuardOlja);
			a.AddAction(eActionType.TakeItem,bottleofmead,null);
			a.AddAction(eActionType.IncQuestStep,typeof(DOL.GS.Quests.Midgard.meadrun),null);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Audun,-1);
				a.AddTrigger(eTriggerType.GiveItem,Audun,emptybottle);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.meadrun),2,(eComparator)3);
			a.AddAction(eActionType.Talk,"Good work. Here is that bit of coin I was talking about. Check back with me later, and I may have more work for you.",Audun);
			a.AddAction(eActionType.GiveXP,5,null);
			a.AddAction(eActionType.GiveGold,27,null);
			a.AddAction(eActionType.FinishQuest,typeof(DOL.GS.Quests.Midgard.meadrun),null);
			AddBehaviour(a);
			
			#endregion

			// Custom Scriptloaded Code Begin
			
			// Custom Scriptloaded Code End
			if (Audun!=null) {
				Audun.AddQuestToGive(typeof (meadrun));
			}
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{			
				
			// Custom Scriptunloaded Code Begin
			
			// Custom Scriptunloaded Code End

			

			/* If Audun has not been initialized, then we don't have to remove any
			 * hooks from him ;-)
			 */
			if (Audun == null)
				return;
			/* Now we remove the possibility to give this quest to players */			
			Audun.RemoveQuestToGive(typeof (meadrun));
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
						return "[Step #1] Deliver the mead to Guard Olja in the Burial Grounds. You will find the guard in the tunnel leading from Jordheim to the main chamber.";
				
					case 2:
						return "[Step #2] Return to Audun in Jordheim with the empty bottle.";
				
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
			if (player.IsDoingQuest(typeof (meadrun)) != null)
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
