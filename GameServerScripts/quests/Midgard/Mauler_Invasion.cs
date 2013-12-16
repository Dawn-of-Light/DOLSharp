	
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
using DOL.AI.Brain;
using DOL.Database;
using DOL.Events;
using DOL.GS.Behaviour;
using DOL.GS.Behaviour.Attributes;
using DOL.GS.PacketHandler;
using DOL.GS.Quests;
using DOL.Language;
using log4net;

	namespace DOL.GS.Quests.Midgard {
	
     /* The first thing we do, is to declare the class we create
	 * as Quest. To do this, we derive from the abstract class
	 * BaseQuest	  	 
	 */
	public class maulerinvasion : BaseQuest
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

        protected static string questTitle = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.MaulerInvasion.QuestTitle");

		protected const int minimumLevel = 1;
		protected const int maximumLevel = 5;
	
	
		private static GameNPC VikingKreimhilde = null;
		
		private static ItemTemplate blackmaulercubpelt = null;
		

		// Custom Initialization Code Begin
		
		// Custom Initialization Code End

		/* 
		* Constructor
		*/
		public maulerinvasion() : base()
		{
		}

		public maulerinvasion(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public maulerinvasion(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public maulerinvasion(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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
	
            npcs = WorldMgr.GetNPCsByName(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.MaulerInvasion.NPCKreimhilde"), (eRealm)2);

			if (npcs.Length == 0)
			{			
				VikingKreimhilde = new DOL.GS.GameNPC();
				VikingKreimhilde.Model = 218;
                VikingKreimhilde.Name = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.MaulerInvasion.NPCKreimhilde");
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

		blackmaulercubpelt = GameServer.Database.FindObjectByKey<ItemTemplate>("blackmaulercubpelt");
			if (blackmaulercubpelt == null)
			{
				blackmaulercubpelt = new ItemTemplate();
				blackmaulercubpelt.Name = "Black Mauler Cub Pelt";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + blackmaulercubpelt.Name + ", creating it ...");
				blackmaulercubpelt.Level = 1;
				blackmaulercubpelt.Weight = 5;
				blackmaulercubpelt.Model = 100;
				blackmaulercubpelt.Object_Type = 0;
				blackmaulercubpelt.Item_Type = 40;
				blackmaulercubpelt.Id_nb = "blackmaulercubpelt";
				blackmaulercubpelt.Hand = 0;
				blackmaulercubpelt.Price = 0;
				blackmaulercubpelt.IsPickable = true;
				blackmaulercubpelt.IsDropable = true;
				blackmaulercubpelt.IsTradable = true;
				blackmaulercubpelt.CanDropAsLoot = false;
				blackmaulercubpelt.Color = 0;
				blackmaulercubpelt.Bonus = 35; // default bonus				
				blackmaulercubpelt.Bonus1 = 0;
				blackmaulercubpelt.Bonus1Type = (int) 0;
				blackmaulercubpelt.Bonus2 = 0;
				blackmaulercubpelt.Bonus2Type = (int) 0;
				blackmaulercubpelt.Bonus3 = 0;
				blackmaulercubpelt.Bonus3Type = (int) 0;
				blackmaulercubpelt.Bonus4 = 0;
				blackmaulercubpelt.Bonus4Type = (int) 0;
				blackmaulercubpelt.Bonus5 = 0;
				blackmaulercubpelt.Bonus5Type = (int) 0;
				blackmaulercubpelt.Bonus6 = 0;
				blackmaulercubpelt.Bonus6Type = (int) 0;
				blackmaulercubpelt.Bonus7 = 0;
				blackmaulercubpelt.Bonus7Type = (int) 0;
				blackmaulercubpelt.Bonus8 = 0;
				blackmaulercubpelt.Bonus8Type = (int) 0;
				blackmaulercubpelt.Bonus9 = 0;
				blackmaulercubpelt.Bonus9Type = (int) 0;
				blackmaulercubpelt.Bonus10 = 0;
				blackmaulercubpelt.Bonus10Type = (int) 0;
				blackmaulercubpelt.ExtraBonus = 0;
				blackmaulercubpelt.ExtraBonusType = (int) 0;
				blackmaulercubpelt.Effect = 0;
				blackmaulercubpelt.Emblem = 0;
				blackmaulercubpelt.Charges = 0;
				blackmaulercubpelt.MaxCharges = 0;
				blackmaulercubpelt.SpellID = 0;
				blackmaulercubpelt.ProcSpellID = 0;
				blackmaulercubpelt.Type_Damage = 0;
				blackmaulercubpelt.Realm = 0;
				blackmaulercubpelt.MaxCount = 1;
				blackmaulercubpelt.PackSize = 1;
				blackmaulercubpelt.Extension = 0;
				blackmaulercubpelt.Quality = 99;				
				blackmaulercubpelt.Condition = 100;
				blackmaulercubpelt.MaxCondition = 100;
				blackmaulercubpelt.Durability = 100;
				blackmaulercubpelt.MaxDurability = 100;
				blackmaulercubpelt.PoisonCharges = 0;
				blackmaulercubpelt.PoisonMaxCharges = 0;
				blackmaulercubpelt.PoisonSpellID = 0;
				blackmaulercubpelt.ProcSpellID1 = 0;
				blackmaulercubpelt.SpellID1 = 0;
				blackmaulercubpelt.MaxCharges1 = 0;
				blackmaulercubpelt.Charges1 = 0;
				
				GameServer.Database.AddObject(blackmaulercubpelt);
				}

			#endregion

			#region defineAreas
			
		#endregion
		
		#region defineQuestParts

		QuestBuilder builder = QuestMgr.getBuilder(typeof(maulerinvasion));
			QuestBehaviour a;
			a = builder.CreateBehaviour(VikingKreimhilde,-1);
				a.AddTrigger(eTriggerType.Interact,null,VikingKreimhilde);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.maulerinvasion),VikingKreimhilde);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.maulerinvasion),null,(eComparator)5);
            a.AddAction(eActionType.Talk, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.MaulerInvasion.Talk1"), VikingKreimhilde);
        AddBehaviour(a);
			a = builder.CreateBehaviour(VikingKreimhilde,-1);
            a.AddTrigger(eTriggerType.Whisper, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.MaulerInvasion.Whisper1"), VikingKreimhilde);
        a.AddRequirement(eRequirementType.QuestGivable, typeof(DOL.GS.Quests.Midgard.maulerinvasion), VikingKreimhilde);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.maulerinvasion),null,(eComparator)5);
            a.AddAction(eActionType.Talk, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.MaulerInvasion.Talk2"), VikingKreimhilde);
        AddBehaviour(a);
			a = builder.CreateBehaviour(VikingKreimhilde,-1);
            a.AddTrigger(eTriggerType.Whisper, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.MaulerInvasion.Whisper2"), VikingKreimhilde);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.maulerinvasion),VikingKreimhilde);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.maulerinvasion),null,(eComparator)5);
            a.AddAction(eActionType.Talk, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.MaulerInvasion.Talk3"), VikingKreimhilde);
        AddBehaviour(a);
			a = builder.CreateBehaviour(VikingKreimhilde,-1);
            a.AddTrigger(eTriggerType.Whisper, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.MaulerInvasion.Whisper3"), VikingKreimhilde);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.maulerinvasion),VikingKreimhilde);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.maulerinvasion),null,(eComparator)5);
            a.AddAction(eActionType.Talk, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.MaulerInvasion.Talk4"), null);
        AddBehaviour(a);
			a = builder.CreateBehaviour(VikingKreimhilde,-1);
            a.AddTrigger(eTriggerType.Whisper, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.MaulerInvasion.Whisper4"), VikingKreimhilde);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.maulerinvasion),VikingKreimhilde);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.maulerinvasion),null,(eComparator)5);
            a.AddAction(eActionType.Talk, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.MaulerInvasion.Talk5"), VikingKreimhilde);
        AddBehaviour(a);
			a = builder.CreateBehaviour(VikingKreimhilde,-1);
            a.AddTrigger(eTriggerType.Whisper, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.MaulerInvasion.Whisper5"), VikingKreimhilde);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.maulerinvasion),VikingKreimhilde);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.maulerinvasion),null,(eComparator)5);
            a.AddAction(eActionType.OfferQuest, typeof(DOL.GS.Quests.Midgard.maulerinvasion), LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.MaulerInvasion.OfferQuest"));
			AddBehaviour(a);
			a = builder.CreateBehaviour(VikingKreimhilde,-1);
				a.AddTrigger(eTriggerType.DeclineQuest,null,typeof(DOL.GS.Quests.Midgard.maulerinvasion));
            a.AddAction(eActionType.Talk, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.MaulerInvasion.Talk6"), VikingKreimhilde);
        AddBehaviour(a);
			a = builder.CreateBehaviour(VikingKreimhilde,-1);
				a.AddTrigger(eTriggerType.AcceptQuest,null,typeof(DOL.GS.Quests.Midgard.maulerinvasion));
			a.AddAction(eActionType.GiveQuest,typeof(DOL.GS.Quests.Midgard.maulerinvasion),VikingKreimhilde);
            a.AddAction(eActionType.Talk, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.MaulerInvasion.Talk7"), VikingKreimhilde);
        AddBehaviour(a);
			a = builder.CreateBehaviour(VikingKreimhilde,-1);
            a.AddTrigger(eTriggerType.EnemyKilled, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.MaulerInvasion.EnemyKilled"), null);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.maulerinvasion),3,(eComparator)1);
			a.AddAction(eActionType.GiveItem,blackmaulercubpelt,null);
			a.AddAction(eActionType.IncQuestStep,typeof(DOL.GS.Quests.Midgard.maulerinvasion),null);
			AddBehaviour(a);
			a = builder.CreateBehaviour(VikingKreimhilde,-1);
				a.AddTrigger(eTriggerType.Interact,null,VikingKreimhilde);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.maulerinvasion),3,(eComparator)3);
            a.AddAction(eActionType.Talk, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.MaulerInvasion.Talk8"), VikingKreimhilde);
        a.AddAction(eActionType.SetQuestStep,typeof(maulerinvasion),4);
			AddBehaviour(a);
			a = builder.CreateBehaviour(VikingKreimhilde,-1);
				a.AddTrigger(eTriggerType.GiveItem,VikingKreimhilde,blackmaulercubpelt);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.maulerinvasion),4,(eComparator)3);
            a.AddAction(eActionType.Talk, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.MaulerInvasion.Talk9"), VikingKreimhilde);
        a.AddAction(eActionType.SetQuestStep,typeof(DOL.GS.Quests.Midgard.maulerinvasion),5);
			a.AddAction(eActionType.TakeItem,blackmaulercubpelt,null);
			AddBehaviour(a);
			a = builder.CreateBehaviour(VikingKreimhilde,-1);
				a.AddTrigger(eTriggerType.GiveItem,VikingKreimhilde,blackmaulercubpelt);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.maulerinvasion),5,(eComparator)3);
			a.AddRequirement(eRequirementType.Level,1,null,(eComparator)3);
            a.AddAction(eActionType.Talk, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.MaulerInvasion.Talk10"), VikingKreimhilde);
        a.AddAction(eActionType.GiveXP,5,null);
			a.AddAction(eActionType.GiveGold,27,null);
			a.AddAction(eActionType.FinishQuest,typeof(DOL.GS.Quests.Midgard.maulerinvasion),null);
			a.AddAction(eActionType.TakeItem,blackmaulercubpelt,null);
			AddBehaviour(a);
			a = builder.CreateBehaviour(VikingKreimhilde,-1);
				a.AddTrigger(eTriggerType.GiveItem,VikingKreimhilde,blackmaulercubpelt);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.maulerinvasion),5,(eComparator)3);
			a.AddRequirement(eRequirementType.Level,2,null,(eComparator)3);
            a.AddAction(eActionType.Talk, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.MaulerInvasion.Talk10"), VikingKreimhilde);
        a.AddAction(eActionType.GiveXP,20,null);
			a.AddAction(eActionType.GiveGold,27,null);
			a.AddAction(eActionType.FinishQuest,typeof(DOL.GS.Quests.Midgard.maulerinvasion),null);
			a.AddAction(eActionType.TakeItem,blackmaulercubpelt,null);
			AddBehaviour(a);
			a = builder.CreateBehaviour(VikingKreimhilde,-1);
				a.AddTrigger(eTriggerType.GiveItem,VikingKreimhilde,blackmaulercubpelt);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.maulerinvasion),5,(eComparator)3);
			a.AddRequirement(eRequirementType.Level,3,null,(eComparator)3);
            a.AddAction(eActionType.Talk, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.MaulerInvasion.Talk10"), VikingKreimhilde);
        a.AddAction(eActionType.GiveXP,40,null);
			a.AddAction(eActionType.GiveGold,27,null);
			a.AddAction(eActionType.FinishQuest,typeof(DOL.GS.Quests.Midgard.maulerinvasion),null);
			a.AddAction(eActionType.TakeItem,blackmaulercubpelt,null);
			AddBehaviour(a);
			a = builder.CreateBehaviour(VikingKreimhilde,-1);
				a.AddTrigger(eTriggerType.GiveItem,VikingKreimhilde,blackmaulercubpelt);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.maulerinvasion),5,(eComparator)3);
			a.AddRequirement(eRequirementType.Level,4,null,(eComparator)3);
            a.AddAction(eActionType.Talk, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.MaulerInvasion.Talk10"), VikingKreimhilde);
        a.AddAction(eActionType.GiveXP,60,null);
			a.AddAction(eActionType.GiveGold,27,null);
			a.AddAction(eActionType.FinishQuest,typeof(DOL.GS.Quests.Midgard.maulerinvasion),null);
			a.AddAction(eActionType.TakeItem,blackmaulercubpelt,null);
			AddBehaviour(a);
			
			#endregion

			// Custom Scriptloaded Code Begin
			
			// Custom Scriptloaded Code End

			VikingKreimhilde.AddQuestToGive(typeof (maulerinvasion));
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
			VikingKreimhilde.RemoveQuestToGive(typeof (maulerinvasion));
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
                        return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.MaulerInvasion.Step1");
				
					case 2:
                        return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.MaulerInvasion.Step2");
				
					case 3:
                        return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.MaulerInvasion.Step3");
				
					case 4:
                        return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.MaulerInvasion.Step4");
				
					case 5:
                        return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.MaulerInvasion.Step5");
				
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
			if (player.IsDoingQuest(typeof (maulerinvasion)) != null)
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
