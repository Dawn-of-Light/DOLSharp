	
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
using DOL.Database2;
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
			if (silverringofhealth == null)
			{
				silverringofhealth = new ItemTemplate();
				silverringofhealth.Name = "Silver Ring of Health";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + silverringofhealth.Name + ", creating it ...");
				silverringofhealth.Level = 5;
				silverringofhealth.Weight = 5;
				silverringofhealth.Model = 103;
				silverringofhealth.Object_Type = 0;
				silverringofhealth.Item_Type = 35;
				silverringofhealth.Id_nb = "silverringofhealth";
				silverringofhealth.Hand = 0;
				silverringofhealth.Platinum = 0;
				silverringofhealth.Gold = 0;
				silverringofhealth.Silver = 0;
				silverringofhealth.Copper = 0;
				silverringofhealth.IsPickable = true;
				silverringofhealth.IsDropable = true;
				silverringofhealth.IsTradable = false;
				silverringofhealth.CanDropAsLoot = false;
				silverringofhealth.Color = 0;
				silverringofhealth.Bonus = 5; // default bonus				
				silverringofhealth.Bonus1 = 12;
				silverringofhealth.Bonus1Type = (int) 10;
				silverringofhealth.Bonus2 = 0;
				silverringofhealth.Bonus2Type = (int) 0;
				silverringofhealth.Bonus3 = 0;
				silverringofhealth.Bonus3Type = (int) 0;
				silverringofhealth.Bonus4 = 0;
				silverringofhealth.Bonus4Type = (int) 0;
				silverringofhealth.Bonus5 = 0;
				silverringofhealth.Bonus5Type = (int) 0;
				silverringofhealth.Bonus6 = 0;
				silverringofhealth.Bonus6Type = (int) 0;
				silverringofhealth.Bonus7 = 0;
				silverringofhealth.Bonus7Type = (int) 0;
				silverringofhealth.Bonus8 = 0;
				silverringofhealth.Bonus8Type = (int) 0;
				silverringofhealth.Bonus9 = 0;
				silverringofhealth.Bonus9Type = (int) 0;
				silverringofhealth.Bonus10 = 0;
				silverringofhealth.Bonus10Type = (int) 0;
				silverringofhealth.ExtraBonus = 0;
				silverringofhealth.ExtraBonusType = (int) 0;
				silverringofhealth.Effect = 0;
				silverringofhealth.Emblem = 0;
				silverringofhealth.Charges = 0;
				silverringofhealth.MaxCharges = 0;
				silverringofhealth.SpellID = 0;
				silverringofhealth.ProcSpellID = 0;
				silverringofhealth.Type_Damage = 0;
				silverringofhealth.Realm = 0;
				silverringofhealth.MaxCount = 1;
				silverringofhealth.PackSize = 1;
				silverringofhealth.Extension = 0;
				silverringofhealth.Quality = 100;				
				silverringofhealth.Condition = 100;
				silverringofhealth.MaxCondition = 100;
				silverringofhealth.Durability = 100;
				silverringofhealth.MaxDurability = 100;
				silverringofhealth.PoisonCharges = 0;
				silverringofhealth.PoisonMaxCharges = 0;
				silverringofhealth.PoisonSpellID = 0;
				silverringofhealth.ProcSpellID1 = 0;
				silverringofhealth.SpellID1 = 0;
				silverringofhealth.MaxCharges1 = 0;
				silverringofhealth.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(silverringofhealth);
				}
			blackmaulercubpelt = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "blackmaulercubpelt");
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
				blackmaulercubpelt.Platinum = 0;
				blackmaulercubpelt.Gold = 0;
				blackmaulercubpelt.Silver = 0;
				blackmaulercubpelt.Copper = 0;
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
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(blackmaulercubpelt);
				}
			

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
