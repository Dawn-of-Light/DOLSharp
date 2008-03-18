	
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
	public class rindaslostkey : BaseQuest
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

		protected const string questTitle = "Rinda's Lost Key";

		protected const int minimumLevel = 1;
		protected const int maximumLevel = 11;
	
	
		private static GameNPC DwarvenGuardRinda = null;
		
		private static GameNPC hobgoblinsnakefinder = null;
		
		private static ItemTemplate rindaskey = null;
		
		private static ItemTemplate ironkeychain = null;
		
		private static ItemTemplate silverringofhealth = null;
		

		// Custom Initialization Code Begin
		
		// Custom Initialization Code End

		/* 
		* Constructor
		*/
		public rindaslostkey() : base()
		{
		}

		public rindaslostkey(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public rindaslostkey(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public rindaslostkey(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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
	
			npcs = WorldMgr.GetNPCsByName("Dwarven Guard Rinda",(eRealm) 2);
			if (npcs.Length == 0)
			{
				if (!WorldMgr.GetRegion(100).IsDisabled)
				{
				DwarvenGuardRinda = new DOL.GS.GameNPC();
					DwarvenGuardRinda.Model = 238;
				DwarvenGuardRinda.Name = "Dwarven Guard Rinda";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + DwarvenGuardRinda.Name + ", creating ...");
				DwarvenGuardRinda.GuildName = "Part of " + questTitle + " Quest";
				DwarvenGuardRinda.Realm = eRealm.Midgard;
				DwarvenGuardRinda.CurrentRegionID = 100;
				DwarvenGuardRinda.Size = 53;
				DwarvenGuardRinda.Level = 41;
				DwarvenGuardRinda.MaxSpeedBase = 191;
				DwarvenGuardRinda.Faction = FactionMgr.GetFactionByID(0);
				DwarvenGuardRinda.X = 805496;
				DwarvenGuardRinda.Y = 701215;
				DwarvenGuardRinda.Z = 4960;
				DwarvenGuardRinda.Heading = 1570;
				DwarvenGuardRinda.RespawnInterval = -1;
				DwarvenGuardRinda.BodyType = 0;
				

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 500;
				DwarvenGuardRinda.SetOwnBrain(brain);
				
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					DwarvenGuardRinda.SaveIntoDatabase();
					
				DwarvenGuardRinda.AddToWorld();
				
				}
			}
			else 
			{
				DwarvenGuardRinda = npcs[0];
			}
		
			npcs = WorldMgr.GetNPCsByName("hobgoblin snake-finder",(eRealm) 0);
			if (npcs.Length == 0)
			{
				if (!WorldMgr.GetRegion(100).IsDisabled)
				{
				hobgoblinsnakefinder = new DOL.GS.GameNPC();
					hobgoblinsnakefinder.Model = 251;
				hobgoblinsnakefinder.Name = "hobgoblin snake-finder";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + hobgoblinsnakefinder.Name + ", creating ...");
				hobgoblinsnakefinder.GuildName = "Part of " + questTitle + " Quest";
				hobgoblinsnakefinder.Realm = eRealm.None;
				hobgoblinsnakefinder.CurrentRegionID = 100;
				hobgoblinsnakefinder.Size = 37;
				hobgoblinsnakefinder.Level = 1;
				hobgoblinsnakefinder.MaxSpeedBase = 191;
				hobgoblinsnakefinder.Faction = FactionMgr.GetFactionByID(0);
				hobgoblinsnakefinder.X = 803189;
				hobgoblinsnakefinder.Y = 695157;
				hobgoblinsnakefinder.Z = 4926;
				hobgoblinsnakefinder.Heading = 125;
				hobgoblinsnakefinder.RespawnInterval = -1;
				hobgoblinsnakefinder.BodyType = 0;
				

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 500;
				hobgoblinsnakefinder.SetOwnBrain(brain);
				
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					hobgoblinsnakefinder.SaveIntoDatabase();
					
				hobgoblinsnakefinder.AddToWorld();
				
				}
			}
			else 
			{
				hobgoblinsnakefinder = npcs[0];
			}
		

			#endregion

			#region defineItems
			rindaskey = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "rindaskey");
			ironkeychain = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ironkeychain");
			silverringofhealth = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "silverringofhealth");
			#endregion

			#region defineAreas
			
		#endregion
		
		#region defineQuestParts

		QuestBuilder builder = QuestMgr.getBuilder(typeof(rindaslostkey));
			QuestBehaviour a;
			a = builder.CreateBehaviour(DwarvenGuardRinda,-1);
				a.AddTrigger(eTriggerType.Interact,null,DwarvenGuardRinda);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.rindaslostkey),DwarvenGuardRinda);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.rindaslostkey),null,(eComparator)5);
			a.AddAction(eActionType.Talk,"Greetings, and welcome to the dwarf town of Haggerfel. I wish I could stay and talk with you for a moment, but I am in a hurry to find my [lost keys].",DwarvenGuardRinda);
			AddBehaviour(a);
			a = builder.CreateBehaviour(DwarvenGuardRinda,-1);
				a.AddTrigger(eTriggerType.Whisper,"lost keys",DwarvenGuardRinda);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.rindaslostkey),DwarvenGuardRinda);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.rindaslostkey),null,(eComparator)5);
			a.AddAction(eActionType.Talk,"Oh yes. It is dreadful that I was so careless as to lost them like this! You see, I am responsible for helping to lock the money the merchants bring in during the day into a large chest. I am the only one with the keys, and I have [misplaced them].",DwarvenGuardRinda);
			AddBehaviour(a);
			a = builder.CreateBehaviour(DwarvenGuardRinda,-1);
				a.AddTrigger(eTriggerType.Whisper,"misplaced them",DwarvenGuardRinda);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.rindaslostkey),DwarvenGuardRinda);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.rindaslostkey),null,(eComparator)5);
			a.AddAction(eActionType.Talk,"Oh, this makes me so angry! I have searched all around Haggerfel for them, but they are no where to be found. I don't suppose you would have a [little time] to help me out, would you?",DwarvenGuardRinda);
			AddBehaviour(a);
			a = builder.CreateBehaviour(DwarvenGuardRinda,-1);
				a.AddTrigger(eTriggerType.Whisper,"little time",DwarvenGuardRinda);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.rindaslostkey),DwarvenGuardRinda);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.rindaslostkey),null,(eComparator)5);
			a.AddAction(eActionType.OfferQuest,typeof(DOL.GS.Quests.Midgard.rindaslostkey),"Will you help Rinda find her keys?");
			AddBehaviour(a);
			a = builder.CreateBehaviour(DwarvenGuardRinda,-1);
				a.AddTrigger(eTriggerType.DeclineQuest,null,typeof(DOL.GS.Quests.Midgard.rindaslostkey));
			a.AddAction(eActionType.Talk,"No problem. See you",DwarvenGuardRinda);
			AddBehaviour(a);
			a = builder.CreateBehaviour(DwarvenGuardRinda,-1);
				a.AddTrigger(eTriggerType.AcceptQuest,null,typeof(DOL.GS.Quests.Midgard.rindaslostkey));
			a.AddAction(eActionType.GiveQuest,typeof(DOL.GS.Quests.Midgard.rindaslostkey),DwarvenGuardRinda);
			a.AddAction(eActionType.Talk,"Thank you! Thank you! I know there are some hobgoblins around that like to play jokes on people, viscious and mean ones. Why don't you check there while I check the merchant huts again?",DwarvenGuardRinda);
			AddBehaviour(a);
			a = builder.CreateBehaviour(hobgoblinsnakefinder,-1);
				a.AddTrigger(eTriggerType.Interact,null,hobgoblinsnakefinder);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.rindaslostkey),1,(eComparator)3);
			a.AddAction(eActionType.Talk,"Ha ha! Me no have keys! Feed to hungry bear! haha! You no help Rinda...Gold be ours!",hobgoblinsnakefinder);
			a.AddAction(eActionType.GiveItem,ironkeychain,hobgoblinsnakefinder);
			a.AddAction(eActionType.IncQuestStep,typeof(DOL.GS.Quests.Midgard.rindaslostkey),null);
			AddBehaviour(a);
			a = builder.CreateBehaviour(DwarvenGuardRinda,-1);
				a.AddTrigger(eTriggerType.EnemyKilled,"black mauler cub",null);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.rindaslostkey),2,(eComparator)3);
			a.AddAction(eActionType.GiveItem,rindaskey,null);
			a.AddAction(eActionType.IncQuestStep,typeof(rindaslostkey),null);
			AddBehaviour(a);
			a = builder.CreateBehaviour(DwarvenGuardRinda,-1);
				a.AddTrigger(eTriggerType.Interact,null,DwarvenGuardRinda);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.rindaslostkey),3,(eComparator)3);
			a.AddAction(eActionType.Talk,"I couldn't find them again in the village. Did you have better luck?",DwarvenGuardRinda);
			AddBehaviour(a);
			a = builder.CreateBehaviour(DwarvenGuardRinda,-1);
				a.AddTrigger(eTriggerType.GiveItem,DwarvenGuardRinda,ironkeychain);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.rindaslostkey),3,(eComparator)3);
			a.AddAction(eActionType.Talk,"Oh my keychain. Well, this is a good start. But you didn't happen to find my key, did you?",DwarvenGuardRinda);
			a.AddAction(eActionType.GiveXP,20,null);
			a.AddAction(eActionType.GiveGold,225,null);
			a.AddAction(eActionType.TakeItem,ironkeychain,null);
			AddBehaviour(a);
			a = builder.CreateBehaviour(DwarvenGuardRinda,-1);
				a.AddTrigger(eTriggerType.GiveItem,DwarvenGuardRinda,rindaskey);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.rindaslostkey),3,(eComparator)3);
			a.AddRequirement(eRequirementType.InventoryItem,ironkeychain,null,(eComparator)3);
			a.AddAction(eActionType.Talk,"Ah! This is great! My key! You have done a fabulous job in helping me. Please accept this coin in return for your time and effort. Thank you so much! Now, I can safely lock up the town's money. Thank you!",DwarvenGuardRinda);
			a.AddAction(eActionType.TakeItem,rindaskey,null);
			a.AddAction(eActionType.TakeItem,ironkeychain,null);
			a.AddAction(eActionType.GiveXP,40,null);
			a.AddAction(eActionType.GiveGold,450,null);
			a.AddAction(eActionType.FinishQuest,typeof(DOL.GS.Quests.Midgard.rindaslostkey),null);
			a.AddAction(eActionType.GiveItem,silverringofhealth,DwarvenGuardRinda);
			AddBehaviour(a);
			a = builder.CreateBehaviour(DwarvenGuardRinda,-1);
				a.AddTrigger(eTriggerType.GiveItem,DwarvenGuardRinda,rindaskey);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.rindaslostkey),3,(eComparator)3);
			a.AddRequirement(eRequirementType.InventoryItem,ironkeychain,null,(eComparator)1);
			a.AddAction(eActionType.Talk,"Ah! This is great! My key! You have done a fabulous job in helping me. Please accept this coin in return for your time and effort. Thank you so much! Now, I can safely lock up the town's money. Thank you!",DwarvenGuardRinda);
			a.AddAction(eActionType.GiveXP,20,null);
			a.AddAction(eActionType.GiveGold,225,null);
			a.AddAction(eActionType.FinishQuest,typeof(DOL.GS.Quests.Midgard.rindaslostkey),null);
			a.AddAction(eActionType.TakeItem,rindaskey,null);
			a.AddAction(eActionType.GiveItem,silverringofhealth,DwarvenGuardRinda);
			AddBehaviour(a);
			
			#endregion

			// Custom Scriptloaded Code Begin
			
			// Custom Scriptloaded Code End
			if (DwarvenGuardRinda!=null) {
				DwarvenGuardRinda.AddQuestToGive(typeof (rindaslostkey));
			}
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{			
				
			// Custom Scriptunloaded Code Begin
			
			// Custom Scriptunloaded Code End

			

			/* If DwarvenGuardRinda has not been initialized, then we don't have to remove any
			 * hooks from him ;-)
			 */
			if (DwarvenGuardRinda == null)
				return;
			/* Now we remove the possibility to give this quest to players */			
			DwarvenGuardRinda.RemoveQuestToGive(typeof (rindaslostkey));
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
						return "[Step #1]  Search for a hobgoblin snake-finder around Haggerfel and see if it has Rinda's keys.";
				
					case 2:
						return "[Step #2]  Look for the black mauler cub that ate Rinda's keys. Slay the beast and retrieve the keys for Rinda.";
				
					case 3:
						return "[Step #3] Return the keys and keychain to Rinda in Haggerfel.";
				
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
			if (player.IsDoingQuest(typeof (rindaslostkey)) != null)
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
