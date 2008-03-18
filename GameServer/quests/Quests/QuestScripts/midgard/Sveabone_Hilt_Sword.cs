	
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
				Gridash.Realm = eRealm.Midgard;
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
			sveawolftooth = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "sveawolftooth");
			sveabone_hilt_sword = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "sveabone_hilt_sword");
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
