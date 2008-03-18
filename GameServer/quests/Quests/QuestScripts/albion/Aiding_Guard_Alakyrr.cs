	
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
			quarterfulltenebrousflask = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "quarterfulltenebrousflask");
			halffulltenebrousflask = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "halffulltenebrousflask");
			threequarterfulltenebrousflask = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "threequarterfulltenebrousflask");
			fullflaskoftenebrousessence = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "fullflaskoftenebrousessence");
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
