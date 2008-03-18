	
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
	public class Abearybadproblem : BaseQuest
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

		protected const string questTitle = "A Beary Bad Problem";

		protected const int minimumLevel = 1;
		protected const int maximumLevel = 50;
	
	
		private static GameNPC VikingKreimhilde = null;
		
		private static ItemTemplate silverringofhealth = null;
		
		private static ItemTemplate blackmaulercubpelt = null;
		

		// Custom Initialization Code Begin
		
		// Custom Initialization Code End

		/* 
		* Constructor
		*/
		public Abearybadproblem() : base()
		{
		}

		public Abearybadproblem(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public Abearybadproblem(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public Abearybadproblem(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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
	
			npcs = WorldMgr.GetNPCsByName("Viking Kreimhilde",(eRealm) 2);
			if (npcs.Length == 0)
			{			
				VikingKreimhilde = new DOL.GS.GameNPC();
					VikingKreimhilde.Model = 218;
				VikingKreimhilde.Name = "Viking Kreimhilde";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + VikingKreimhilde.Name + ", creating ...");
				VikingKreimhilde.GuildName = "Part of " + questTitle + " Quest";
				VikingKreimhilde.Realm = eRealm.Midgard;
				VikingKreimhilde.CurrentRegionID = 100;
				VikingKreimhilde.Size = 51;
				VikingKreimhilde.Level = 50;
				VikingKreimhilde.MaxSpeedBase = 191;
				VikingKreimhilde.Faction = FactionMgr.GetFactionByID(0);
				VikingKreimhilde.X = 803999;
				VikingKreimhilde.Y = 726551;
				VikingKreimhilde.Z = 4752;
				VikingKreimhilde.Heading = 2116;
				VikingKreimhilde.RespawnInterval = -1;
				VikingKreimhilde.BodyType = 0;
				

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 500;
				VikingKreimhilde.SetOwnBrain(brain);
				
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					VikingKreimhilde.SaveIntoDatabase();
					
				VikingKreimhilde.AddToWorld();
				
			}
			else 
			{
				VikingKreimhilde = npcs[0];
			}
		

			#endregion

			#region defineItems
			silverringofhealth = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "silverringofhealth");
			blackmaulercubpelt = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "blackmaulercubpelt");
			#endregion

			#region defineAreas
			
		#endregion
		
		#region defineQuestParts

		QuestBuilder builder = QuestMgr.getBuilder(typeof(Abearybadproblem));
			QuestBehaviour a;
			a = builder.CreateBehaviour(VikingKreimhilde,-1);
				a.AddTrigger(eTriggerType.Interact,null,VikingKreimhilde);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.Abearybadproblem),VikingKreimhilde);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.Abearybadproblem),null,(eComparator)5);
			a.AddAction(eActionType.Talk,"So you're one of the new arrivals, eh? Sorry, I overheard your conversation with your trainer over there.",VikingKreimhilde);
			a.AddAction(eActionType.Talk,"What you were told about our lack of local Viking guards is quite true, I'm afraid. King Eirik has spread out our defenses quite thin, leaving folks around these parts pretty nervous about their safety. Not that he can be blamed, we just have too many foes to worry about in these trying times. ",VikingKreimhilde);
			a.AddAction(eActionType.Talk,"If you're truly willing to help like you say, then I could use your help. The population of \"natural\" wildlife has been getting out of hand lately, and there are more and more accounts of unfortunate \"incidents\" involving out townsfolk and wild animals. Recently, a young boy was savagely attacked by a black mauler bear while hiking with his family in the nearby mountains to the north.",VikingKreimhilde);
			a.AddAction(eActionType.Talk,"I've had a few people already volunteer to help thin out the nearby bear population. Would you be willing to help out, as well?",VikingKreimhilde);
			a.AddAction(eActionType.OfferQuest,typeof(DOL.GS.Quests.Midgard.Abearybadproblem),"Accept A beary Bad Problem Quest?");
			AddBehaviour(a);
			a = builder.CreateBehaviour(VikingKreimhilde,-1);
				a.AddTrigger(eTriggerType.AcceptQuest,null,typeof(DOL.GS.Quests.Midgard.Abearybadproblem));
			a.AddAction(eActionType.GiveQuest,typeof(DOL.GS.Quests.Midgard.Abearybadproblem),VikingKreimhilde);
			a.AddAction(eActionType.SetQuestStep,typeof(DOL.GS.Quests.Midgard.Abearybadproblem),1);
			AddBehaviour(a);
			a = builder.CreateBehaviour(VikingKreimhilde,-1);
				a.AddTrigger(eTriggerType.DeclineQuest,null,typeof(DOL.GS.Quests.Midgard.Abearybadproblem));
			AddBehaviour(a);
			a = builder.CreateBehaviour(VikingKreimhilde,-1);
				a.AddTrigger(eTriggerType.EnemyKilled,"black mauler cub",null);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.Abearybadproblem),1,(eComparator)3);
			a.AddAction(eActionType.GiveItem,blackmaulercubpelt,null);
			a.AddAction(eActionType.SetQuestStep,typeof(DOL.GS.Quests.Midgard.Abearybadproblem),2);
			AddBehaviour(a);
			a = builder.CreateBehaviour(VikingKreimhilde,-1);
				a.AddTrigger(eTriggerType.Interact,null,VikingKreimhilde);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.Abearybadproblem),2,(eComparator)3);
			a.AddAction(eActionType.Talk,"Good Job!",VikingKreimhilde);
			a.AddAction(eActionType.GiveXP,22,null);
			a.AddAction(eActionType.GiveGold,23,null);
			a.AddAction(eActionType.TakeItem,blackmaulercubpelt,null);
			a.AddAction(eActionType.GiveItem,silverringofhealth,VikingKreimhilde);
			a.AddAction(eActionType.FinishQuest,typeof(DOL.GS.Quests.Midgard.Abearybadproblem),null);
			AddBehaviour(a);
			
			#endregion

			// Custom Scriptloaded Code Begin
			
			// Custom Scriptloaded Code End

			VikingKreimhilde.AddQuestToGive(typeof (Abearybadproblem));
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{			
				
			// Custom Scriptunloaded Code Begin
			
			// Custom Scriptunloaded Code End

			

			/* If VikingKreimhilde has not been initialized, then we don't have to remove any
			 * hooks from him ;-)
			 */
			if (VikingKreimhilde == null)
				return;
			/* Now we remove the possibility to give this quest to players */			
			VikingKreimhilde.RemoveQuestToGive(typeof (Abearybadproblem));
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
						return "[Step #1] Kill a Black Mauler Cub";
				
					case 2:
						return "[Step #2] Return to Viking";
				
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
			if (player.IsDoingQuest(typeof (Abearybadproblem)) != null)
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
