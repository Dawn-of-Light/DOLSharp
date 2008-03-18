	
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
	public class badfood : BaseQuest
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

		protected const string questTitle = "Bad Food";

		protected const int minimumLevel = 1;
		protected const int maximumLevel = 5;
	
	
		private static GameNPC Pedra = null;
		
		private static GameNPC Kedra = null;
		
		private static ItemTemplate marinefungus = null;
		

		// Custom Initialization Code Begin
		
		// Custom Initialization Code End

		/* 
		* Constructor
		*/
		public badfood() : base()
		{
		}

		public badfood(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public badfood(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public badfood(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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
	
			npcs = WorldMgr.GetNPCsByName("Pedra",(eRealm) 2);
			if (npcs.Length == 0)
			{			
				Pedra = new DOL.GS.GameNPC();
					Pedra.Model = 214;
				Pedra.Name = "Pedra";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Pedra.Name + ", creating ...");
				Pedra.GuildName = "Part of " + questTitle + " Quest";
				Pedra.Realm = eRealm.Midgard;
				Pedra.CurrentRegionID = 151;
				Pedra.Size = 51;
				Pedra.Level = 50;
				Pedra.MaxSpeedBase = 191;
				Pedra.Faction = FactionMgr.GetFactionByID(0);
				Pedra.X = 289895;
				Pedra.Y = 356014;
				Pedra.Z = 3866;
				Pedra.Heading = 1661;
				Pedra.RespawnInterval = -1;
				Pedra.BodyType = 0;
				

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 500;
				Pedra.SetOwnBrain(brain);
				
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					Pedra.SaveIntoDatabase();
					
				Pedra.AddToWorld();
				
			}
			else 
			{
				Pedra = npcs[0];
			}
		
			npcs = WorldMgr.GetNPCsByName("Kedra",(eRealm) 2);
			if (npcs.Length == 0)
			{			
				Kedra = new GameHealer();
					Kedra.Model = 206;
				Kedra.Name = "Kedra";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Kedra.Name + ", creating ...");
				Kedra.GuildName = "Part of " + questTitle + " Quest";
				Kedra.Realm = eRealm.Midgard;
				Kedra.CurrentRegionID = 151;
				Kedra.Size = 51;
				Kedra.Level = 50;
				Kedra.MaxSpeedBase = 191;
				Kedra.Faction = FactionMgr.GetFactionByID(0);
				Kedra.X = 289612;
				Kedra.Y = 354560;
				Kedra.Z = 3866;
				Kedra.Heading = 3902;
				Kedra.RespawnInterval = -1;
				Kedra.BodyType = 0;
				

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 500;
				Kedra.SetOwnBrain(brain);
				
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					Kedra.SaveIntoDatabase();
					
				Kedra.AddToWorld();
				
			}
			else 
			{
				Kedra = npcs[0];
			}
		

			#endregion

			#region defineItems
			marinefungus = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "marinefungus");
			#endregion

			#region defineAreas
			
		#endregion
		
		#region defineQuestParts

		QuestBuilder builder = QuestMgr.getBuilder(typeof(badfood));
			QuestBehaviour a;
			a = builder.CreateBehaviour(Pedra,-1);
				a.AddTrigger(eTriggerType.Interact,null,Pedra);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.badfood),Pedra);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.badfood),null,(eComparator)5);
			a.AddAction(eActionType.Talk,"Oh, I am not feeling very well at all. I don't think there's one member of this town that hasn't felt a little queasy at least (once?) from all the bad fish we've [eaten].",Pedra);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Pedra,-1);
				a.AddTrigger(eTriggerType.Whisper,"eaten",Pedra);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.badfood),Pedra);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.badfood),null,(eComparator)5);
			a.AddAction(eActionType.Talk,"Aye, and now Kedra, the healer, is busy sorting out the worst! It turns out we are ate some bad fish. Now, Kedra says she is running our of her cure, and doesn't have time to get any more of the ingredients! Do you think you could help?",Pedra);
			a.AddAction(eActionType.OfferQuest,typeof(DOL.GS.Quests.Midgard.badfood),"Will you help the healer out?");
			AddBehaviour(a);
			a = builder.CreateBehaviour(Pedra,-1);
				a.AddTrigger(eTriggerType.AcceptQuest,null,typeof(DOL.GS.Quests.Midgard.badfood));
			a.AddAction(eActionType.GiveQuest,typeof(DOL.GS.Quests.Midgard.badfood),Pedra);
			a.AddAction(eActionType.Talk,"Oh good! Talk to Kedra about bad fish. She'll tell you what needs to be done.",Pedra);
			a.AddAction(eActionType.SetQuestStep,typeof(DOL.GS.Quests.Midgard.badfood),2);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Kedra,-1);
				a.AddTrigger(eTriggerType.Interact,null,Kedra);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.badfood),2,(eComparator)3);
			a.AddAction(eActionType.Talk,"Fish! The entire town has eaten bad fish! It is crazy! I told them not to! I told them the moon's position in the sky indicated a bad harvest of fish, but did anyone listen? And now, everyone is [sick].",Kedra);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Kedra,-1);
				a.AddTrigger(eTriggerType.Whisper,"sick",Kedra);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.badfood),2,(eComparator)3);
			a.AddAction(eActionType.Talk,"Yes. Sick. Why are you here? Are you sick? You don't look sick to me. You can help me. I am running out of my cure! I don't have time to go get the [marine fungus] I need!",Kedra);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Kedra,-1);
				a.AddTrigger(eTriggerType.Whisper,"marine fungus",Kedra);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.badfood),2,(eComparator)3);
			a.AddAction(eActionType.SetQuestStep,typeof(DOL.GS.Quests.Midgard.badfood),3);
			a.AddAction(eActionType.Talk,"Aye. I mash it up and add herbs to it. It's a great cure! I need more though! Go get me a marine fungus. I will pay you for it! And the whole town will thank you, when they are through wretching up the illness, that is.",Kedra);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Pedra,-1);
				a.AddTrigger(eTriggerType.EnemyKilled,"marine fungus",null);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.badfood),3,(eComparator)3);
			a.AddAction(eActionType.GiveItem,marinefungus,null);
			a.AddAction(eActionType.SetQuestStep,typeof(DOL.GS.Quests.Midgard.badfood),4);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Kedra,-1);
				a.AddTrigger(eTriggerType.Interact,null,Kedra);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.badfood),4,(eComparator)3);
			a.AddAction(eActionType.Talk,"Oh thank you! The stench of the ill is getting to me! I can make a few more batches of my cure after this, and then be done with it all! This will teach everyone to ignore me! Let's see if they eat the fish the next time I say the moon isn't right! Here, I hope this is enough. Thank you for your aid!",Kedra);
			a.AddAction(eActionType.GiveGold,305,null);
			a.AddAction(eActionType.GiveXP,20,null);
			a.AddAction(eActionType.FinishQuest,typeof(DOL.GS.Quests.Midgard.badfood),null);
			a.AddAction(eActionType.TakeItem,marinefungus,null);
			AddBehaviour(a);
			
			#endregion

			// Custom Scriptloaded Code Begin
			
			// Custom Scriptloaded Code End

			Pedra.AddQuestToGive(typeof (badfood));
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{			
				
			// Custom Scriptunloaded Code Begin
			
			// Custom Scriptunloaded Code End

			

			/* If Pedra has not been initialized, then we don't have to remove any
			 * hooks from him ;-)
			 */
			if (Pedra == null)
				return;
			/* Now we remove the possibility to give this quest to players */			
			Pedra.RemoveQuestToGive(typeof (badfood));
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
						return "[Step #1] Talk to Pedra";
				
					case 2:
						return "[Step #2] Talk to Kedra, the healer, about the illness in the town.";
				
					case 3:
						return "[Step #3] Kill a marine fungus for the healer, They can be found south, across the bridge.";
				
					case 4:
						return "[Step #4] Return to Kedra with the marine fungus.";
				
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
			if (player.IsDoingQuest(typeof (badfood)) != null)
				return true;
				
			// Custom Code Begin
			
			// Custom  Code End
			
		
			if (player.Level > maximumLevel || player.Level < minimumLevel )
				return false;
		
			if (
		
			player.CharacterClass.ID != (byte) eCharacterClass.Mystic && 
			player.CharacterClass.ID != (byte) eCharacterClass.Seer && 
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
