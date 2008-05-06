
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

namespace DOL.GS.Quests.Midgard
{

	/* The first thing we do, is to declare the class we create
	* as Quest. To do this, we derive from the abstract class
	* BaseQuest	  	 
	*/
	public class trialofstrength : BaseQuest
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

		protected const string questTitle = "Trial of Strength";

		protected const int minimumLevel = 1;
		protected const int maximumLevel = 5;


		private static GameNPC ToroldSterkkriger = null;

		private static GameNPC JorundBruttstein = null;

		private static ItemTemplate marinefungusroot = null;


		// Custom Initialization Code Begin

		// Custom Initialization Code End

		/* 
		* Constructor
		*/
		public trialofstrength()
			: base()
		{
		}

		public trialofstrength(GamePlayer questingPlayer)
			: this(questingPlayer, 1)
		{
		}

		public trialofstrength(GamePlayer questingPlayer, int step)
			: base(questingPlayer, step)
		{
		}

		public trialofstrength(GamePlayer questingPlayer, DBQuest dbQuest)
			: base(questingPlayer, dbQuest)
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

			npcs = WorldMgr.GetNPCsByName("Torold Sterkkriger", (eRealm)2);
			if (npcs.Length == 0)
			{
				if (!WorldMgr.GetRegion(151).IsDisabled)
				{
					ToroldSterkkriger = new DOL.GS.GameNPC();
					ToroldSterkkriger.Model = 522;
					ToroldSterkkriger.Name = "Torold Sterkkriger";
					if (log.IsWarnEnabled)
						log.Warn("Could not find " + ToroldSterkkriger.Name + ", creating ...");
					ToroldSterkkriger.GuildName = "Part of " + questTitle + " Quest";
					ToroldSterkkriger.Realm = eRealm.Midgard;
					ToroldSterkkriger.CurrentRegionID = 151;
					ToroldSterkkriger.Size = 50;
					ToroldSterkkriger.Level = 55;
					ToroldSterkkriger.MaxSpeedBase = 191;
					ToroldSterkkriger.Faction = FactionMgr.GetFactionByID(0);
					ToroldSterkkriger.X = 287623;
					ToroldSterkkriger.Y = 355226;
					ToroldSterkkriger.Z = 3488;
					ToroldSterkkriger.Heading = 3788;
					ToroldSterkkriger.RespawnInterval = -1;
					ToroldSterkkriger.BodyType = 0;


					StandardMobBrain brain = new StandardMobBrain();
					brain.AggroLevel = 0;
					brain.AggroRange = 500;
					ToroldSterkkriger.SetOwnBrain(brain);

					//You don't have to store the created mob in the db if you don't want,
					//it will be recreated each time it is not found, just comment the following
					//line if you rather not modify your database
					if (SAVE_INTO_DATABASE)
						ToroldSterkkriger.SaveIntoDatabase();

					ToroldSterkkriger.AddToWorld();

				}
			}
			else
			{
				ToroldSterkkriger = npcs[0];
			}

			npcs = WorldMgr.GetNPCsByName("Jorund Bruttstein", (eRealm)2);
			if (npcs.Length == 0)
			{
				if (!WorldMgr.GetRegion(151).IsDisabled)
				{
					JorundBruttstein = new DOL.GS.GameNPC();
					JorundBruttstein.Model = 513;
					JorundBruttstein.Name = "Jorund Bruttstein";
					if (log.IsWarnEnabled)
						log.Warn("Could not find " + JorundBruttstein.Name + ", creating ...");
					JorundBruttstein.GuildName = "Part of " + questTitle + " Quest";
					JorundBruttstein.Realm = eRealm.Midgard;
					JorundBruttstein.CurrentRegionID = 151;
					JorundBruttstein.Size = 52;
					JorundBruttstein.Level = 50;
					JorundBruttstein.MaxSpeedBase = 191;
					JorundBruttstein.Faction = FactionMgr.GetFactionByID(0);
					JorundBruttstein.X = 287884;
					JorundBruttstein.Y = 356307;
					JorundBruttstein.Z = 3488;
					JorundBruttstein.Heading = 3163;
					JorundBruttstein.RespawnInterval = -1;
					JorundBruttstein.BodyType = 0;


					StandardMobBrain brain = new StandardMobBrain();
					brain.AggroLevel = 0;
					brain.AggroRange = 500;
					JorundBruttstein.SetOwnBrain(brain);

					//You don't have to store the created mob in the db if you don't want,
					//it will be recreated each time it is not found, just comment the following
					//line if you rather not modify your database
					if (SAVE_INTO_DATABASE)
						JorundBruttstein.SaveIntoDatabase();

					JorundBruttstein.AddToWorld();

				}
			}
			else
			{
				JorundBruttstein = npcs[0];
			}


			#endregion

			#region defineItems

			marinefungusroot = (ItemTemplate)GameServer.Database.GetDatabaseObjectFromIDnb(typeof (ItemTemplate), "marinefungusroot");
			if (marinefungusroot == null)
			{
				marinefungusroot = new ItemTemplate();
				marinefungusroot.Name = "Marine Fungus Root";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + marinefungusroot.Name + ", creating it ...");
				marinefungusroot.Level = 50;
				marinefungusroot.Weight = 5;
				marinefungusroot.Model = 546;
				marinefungusroot.Object_Type = 0;
				marinefungusroot.Item_Type = 40;
				marinefungusroot.Id_nb = "marinefungusroot";
				marinefungusroot.Hand = 0;
				marinefungusroot.Platinum = 0;
				marinefungusroot.Gold = 0;
				marinefungusroot.Silver = 0;
				marinefungusroot.Copper = 0;
				marinefungusroot.IsPickable = true;
				marinefungusroot.IsDropable = true;
				marinefungusroot.IsTradable = true;
				marinefungusroot.CanDropAsLoot = false;
				marinefungusroot.Color = 0;
				marinefungusroot.Bonus = 35; // default bonus				
				marinefungusroot.Bonus1 = 0;
				marinefungusroot.Bonus1Type = (int)0;
				marinefungusroot.Bonus2 = 0;
				marinefungusroot.Bonus2Type = (int)0;
				marinefungusroot.Bonus3 = 0;
				marinefungusroot.Bonus3Type = (int)0;
				marinefungusroot.Bonus4 = 0;
				marinefungusroot.Bonus4Type = (int)0;
				marinefungusroot.Bonus5 = 0;
				marinefungusroot.Bonus5Type = (int)0;
				marinefungusroot.Bonus6 = 0;
				marinefungusroot.Bonus6Type = (int)0;
				marinefungusroot.Bonus7 = 0;
				marinefungusroot.Bonus7Type = (int)0;
				marinefungusroot.Bonus8 = 0;
				marinefungusroot.Bonus8Type = (int)0;
				marinefungusroot.Bonus9 = 0;
				marinefungusroot.Bonus9Type = (int)0;
				marinefungusroot.Bonus10 = 0;
				marinefungusroot.Bonus10Type = (int)0;
				marinefungusroot.ExtraBonus = 0;
				marinefungusroot.ExtraBonusType = (int)0;
				marinefungusroot.Effect = 0;
				marinefungusroot.Emblem = 0;
				marinefungusroot.Charges = 0;
				marinefungusroot.MaxCharges = 0;
				marinefungusroot.SpellID = 0;
				marinefungusroot.ProcSpellID = 0;
				marinefungusroot.Type_Damage = 0;
				marinefungusroot.Realm = 0;
				marinefungusroot.MaxCount = 1;
				marinefungusroot.PackSize = 1;
				marinefungusroot.Extension = 0;
				marinefungusroot.Quality = 99;
				marinefungusroot.Condition = 50000;
				marinefungusroot.MaxCondition = 50000;
				marinefungusroot.Durability = 50000;
				marinefungusroot.MaxDurability = 50000;
				marinefungusroot.PoisonCharges = 0;
				marinefungusroot.PoisonMaxCharges = 0;
				marinefungusroot.PoisonSpellID = 0;
				marinefungusroot.ProcSpellID1 = 0;
				marinefungusroot.SpellID1 = 0;
				marinefungusroot.MaxCharges1 = 0;
				marinefungusroot.Charges1 = 0;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(marinefungusroot);
			}


			#endregion

			#region defineAreas

			#endregion

			#region defineQuestParts

			QuestBuilder builder = QuestMgr.getBuilder(typeof(trialofstrength));
			QuestBehaviour a;
			a = builder.CreateBehaviour(ToroldSterkkriger, -1);
			a.AddTrigger(eTriggerType.Interact, null, ToroldSterkkriger);
			a.AddRequirement(eRequirementType.QuestGivable, typeof(trialofstrength), ToroldSterkkriger);
			a.AddRequirement(eRequirementType.QuestPending, typeof(DOL.GS.Quests.Midgard.trialofstrength), null, (eComparator)5);
			a.AddRequirement(eRequirementType.Class, 35, false);
			a.AddAction(eActionType.Talk, "Hail. I am Torold, and I shall be your trainer in this wild land. King Goran Stonefist and his brother Stonelock have charged me with training all young Vikings to prepare them to join the ranks of King Goran's army to aid in the exploration of Aegir. Aegir is a wild, untamed place, and it's made even more dangerous for a your Viking like you by the presence of [Morvaltar].", ToroldSterkkriger);
			AddBehaviour(a);
			a = builder.CreateBehaviour(ToroldSterkkriger, -1);
			a.AddTrigger(eTriggerType.Interact, null, ToroldSterkkriger);
			a.AddRequirement(eRequirementType.QuestGivable, typeof(DOL.GS.Quests.Midgard.trialofstrength), ToroldSterkkriger);
			a.AddRequirement(eRequirementType.QuestPending, typeof(DOL.GS.Quests.Midgard.trialofstrength), null, (eComparator)5);
			a.AddRequirement(eRequirementType.Class, 38, false);
			a.AddAction(eActionType.Talk, "Hail. I am Torold, and I shall be your trainer in this wild land. King Goran Stonefist and his brother Stonelock have charged me with training all young Rogue to prepare them to join the ranks of King Goran's army to aid in the exploration of Aegir. Aegir is a wild, untamed place, and it's made even more dangerous for a your Rogue like you by the presence of [Morvaltar].", ToroldSterkkriger);
			AddBehaviour(a);
			a = builder.CreateBehaviour(ToroldSterkkriger, -1);
			a.AddTrigger(eTriggerType.Whisper, "Morvaltar", ToroldSterkkriger);
			a.AddRequirement(eRequirementType.QuestGivable, typeof(DOL.GS.Quests.Midgard.trialofstrength), ToroldSterkkriger);
			a.AddRequirement(eRequirementType.QuestPending, typeof(DOL.GS.Quests.Midgard.trialofstrength), null, (eComparator)5);
			a.AddAction(eActionType.Talk, "As you may know, the Morvaltar are your wild cousins. We of Midgard do not fear Valkyn like you, for as a group you have more than proven your loyalty to King Goran. Many Valkyn have also proven that they are strong, intelligent, and brave. These are [the qualities] that all Vikings must possess before they can choose where their destiny lies.", ToroldSterkkriger);
			AddBehaviour(a);
			a = builder.CreateBehaviour(ToroldSterkkriger, -1);
			a.AddTrigger(eTriggerType.Whisper, "the qualities", ToroldSterkkriger);
			a.AddRequirement(eRequirementType.QuestGivable, typeof(DOL.GS.Quests.Midgard.trialofstrength), ToroldSterkkriger);
			a.AddRequirement(eRequirementType.QuestPending, typeof(DOL.GS.Quests.Midgard.trialofstrength), null, (eComparator)5);
			a.AddRequirement(eRequirementType.Class, 35, false);
			a.AddAction(eActionType.Talk, "Now, young one, prepare yourself. You have three trials to face before I can allow you to make your destiny. These trials are designed to separate the true Vikings from the everyday Valkyn. So, prepare yourself now, for you trial is waiting for you. It is a test to [prove your strength]. Only the strongest will survive in this land.", ToroldSterkkriger);
			AddBehaviour(a);
			a = builder.CreateBehaviour(ToroldSterkkriger, -1);
			a.AddTrigger(eTriggerType.Whisper, "the qualities", ToroldSterkkriger);
			a.AddRequirement(eRequirementType.QuestGivable, typeof(DOL.GS.Quests.Midgard.trialofstrength), ToroldSterkkriger);
			a.AddRequirement(eRequirementType.QuestPending, typeof(DOL.GS.Quests.Midgard.trialofstrength), null, (eComparator)5);
			a.AddRequirement(eRequirementType.Class, 38, false);
			a.AddAction(eActionType.Talk, "Now, young one, prepare yourself. You have three trials to face before I can allow you to make your destiny. These trials are designed to separate the true Rogues from the everyday Valkyn. So, prepare yourself now, for you trial is waiting for you. It is a test to [prove your strength]. Only the strongest will survive in this land.", ToroldSterkkriger);
			AddBehaviour(a);
			a = builder.CreateBehaviour(ToroldSterkkriger, -1);
			a.AddTrigger(eTriggerType.Whisper, "prove your strength", ToroldSterkkriger);
			a.AddRequirement(eRequirementType.QuestGivable, typeof(DOL.GS.Quests.Midgard.trialofstrength), ToroldSterkkriger);
			a.AddRequirement(eRequirementType.QuestPending, typeof(DOL.GS.Quests.Midgard.trialofstrength), null, (eComparator)5);
			a.AddAction(eActionType.OfferQuest, typeof(trialofstrength), "Will you prove you have the strength to survive in the land of Aegir?");
			AddBehaviour(a);
			a = builder.CreateBehaviour(ToroldSterkkriger, -1);
			a.AddTrigger(eTriggerType.DeclineQuest, null, typeof(DOL.GS.Quests.Midgard.trialofstrength));
			a.AddAction(eActionType.Talk, "No problem. See you", ToroldSterkkriger);
			AddBehaviour(a);
			a = builder.CreateBehaviour(ToroldSterkkriger, -1);
			a.AddTrigger(eTriggerType.AcceptQuest, null, typeof(DOL.GS.Quests.Midgard.trialofstrength));
			a.AddAction(eActionType.GiveQuest, typeof(DOL.GS.Quests.Midgard.trialofstrength), ToroldSterkkriger);
			a.AddAction(eActionType.Talk, "Seek out Jorund Bruttstein in Aegirhamn. Tell him your name when he asks.", ToroldSterkkriger);
			AddBehaviour(a);
			a = builder.CreateBehaviour(JorundBruttstein, -1);
			a.AddTrigger(eTriggerType.Interact, null, JorundBruttstein);
			a.AddRequirement(eRequirementType.QuestStep, typeof(DOL.GS.Quests.Midgard.trialofstrength), 1, (eComparator)3);
			a.AddRequirement(eRequirementType.Class, 35, false);
			a.AddAction(eActionType.Talk, "Lo, I see a young Viking before me. What is your name? Speak up!", JorundBruttstein);
			a.AddAction(eActionType.Talk, "You are here to prove that you have the strength in you to endure in this land. The trial is simple in nature, but unless you are strong, may prove difficult to execute. You must venture out and face [one of the creatures] that roam this land.", JorundBruttstein);
			a.AddAction(eActionType.IncQuestStep, typeof(DOL.GS.Quests.Midgard.trialofstrength), null);
			AddBehaviour(a);
			a = builder.CreateBehaviour(JorundBruttstein, -1);
			a.AddTrigger(eTriggerType.Interact, null, JorundBruttstein);
			a.AddRequirement(eRequirementType.QuestStep, typeof(DOL.GS.Quests.Midgard.trialofstrength), 1, (eComparator)3);
			a.AddRequirement(eRequirementType.Class, 38, false);
			a.AddAction(eActionType.Talk, "Lo, I see a young Rogue before me. What is your name? Speak up!", JorundBruttstein);
			a.AddAction(eActionType.Talk, "You are here to prove that you have the strength in you to endure in this land. The trial is simple in nature, but unless you are strong, may prove difficult to execute. You must venture out and face [one of the creatures] that roam this land.", JorundBruttstein);
			a.AddAction(eActionType.IncQuestStep, typeof(DOL.GS.Quests.Midgard.trialofstrength), null);
			AddBehaviour(a);
			a = builder.CreateBehaviour(JorundBruttstein, -1);
			a.AddTrigger(eTriggerType.Whisper, "one of the creatures", JorundBruttstein);
			a.AddRequirement(eRequirementType.QuestStep, typeof(DOL.GS.Quests.Midgard.trialofstrength), 2, (eComparator)3);
			a.AddAction(eActionType.Talk, "You must seek out a marine fungus. They may be found in the waters south and southwest of here. Find one and defeat it. Once you have, obtain proof that you have accomplished this test. Return to me here.", JorundBruttstein);
			AddBehaviour(a);
			a = builder.CreateBehaviour(ToroldSterkkriger, -1);
			a.AddTrigger(eTriggerType.EnemyKilled, "marine fungus", null);
			a.AddRequirement(eRequirementType.QuestStep, typeof(DOL.GS.Quests.Midgard.trialofstrength), 2, (eComparator)3);
			a.AddAction(eActionType.GiveItem, marinefungusroot, null);
			a.AddAction(eActionType.IncQuestStep, typeof(DOL.GS.Quests.Midgard.trialofstrength), null);
			AddBehaviour(a);
			a = builder.CreateBehaviour(JorundBruttstein, -1);
			a.AddTrigger(eTriggerType.Interact, null, JorundBruttstein);
			a.AddRequirement(eRequirementType.QuestStep, typeof(DOL.GS.Quests.Midgard.trialofstrength), 3, (eComparator)3);
			a.AddAction(eActionType.Talk, "So, you managed to survive, but anyone can run away. Show me the proof you defeated the marine fungus.", JorundBruttstein);
			a.AddAction(eActionType.IncQuestStep, typeof(DOL.GS.Quests.Midgard.trialofstrength), null);
			AddBehaviour(a);
			a = builder.CreateBehaviour(JorundBruttstein, -1);
			a.AddTrigger(eTriggerType.GiveItem, JorundBruttstein, marinefungusroot);
			a.AddRequirement(eRequirementType.QuestStep, typeof(DOL.GS.Quests.Midgard.trialofstrength), 4, (eComparator)3);
			a.AddRequirement(eRequirementType.Class, 35, false);
			a.AddAction(eActionType.Talk, "Well done. I do not like to be so rough with young Vikings, but I must be. I do not want to see anyone die because I failed to do my job. But you have proven that you have the potential for great strength. I shall let your trainer know of your success.", JorundBruttstein);
			a.AddAction(eActionType.GiveXP, 20, null);
			a.AddAction(eActionType.GiveGold, 230, null);
			a.AddAction(eActionType.FinishQuest, typeof(DOL.GS.Quests.Midgard.trialofstrength), null);
			AddBehaviour(a);
			a = builder.CreateBehaviour(JorundBruttstein, -1);
			a.AddTrigger(eTriggerType.GiveItem, JorundBruttstein, marinefungusroot);
			a.AddRequirement(eRequirementType.QuestStep, typeof(DOL.GS.Quests.Midgard.trialofstrength), 4, (eComparator)3);
			a.AddRequirement(eRequirementType.Class, 38, false);
			a.AddAction(eActionType.Talk, "\"Well done. I do not like to be so rough with young Rogues, but I must be. I do not want to see anyone die because I failed to do my job. But you have proven that you have the potential for great strength. I shall let your trainer know of your success.", JorundBruttstein);
			a.AddAction(eActionType.GiveXP, 20, null);
			a.AddAction(eActionType.GiveGold, 230, null);
			a.AddAction(eActionType.FinishQuest, typeof(DOL.GS.Quests.Midgard.trialofstrength), null);
			AddBehaviour(a);

			#endregion

			// Custom Scriptloaded Code Begin

			// Custom Scriptloaded Code End
			if (ToroldSterkkriger != null)
			{
				ToroldSterkkriger.AddQuestToGive(typeof(trialofstrength));
			}
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{

			// Custom Scriptunloaded Code Begin

			// Custom Scriptunloaded Code End



			/* If ToroldSterkkriger has not been initialized, then we don't have to remove any
			 * hooks from him ;-)
			 */
			if (ToroldSterkkriger == null)
				return;
			/* Now we remove the possibility to give this quest to players */
			ToroldSterkkriger.RemoveQuestToGive(typeof(trialofstrength));
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
						return "[Step #1] Seek out Jorund Bruttstein in Aegirhamn. Tell him your name when he asks.";

					case 2:
						return "[Step #2] Obtain proof that you have defeated a marine fungus in combat. They can be found in the waters south of Aegirhamn as well as southwest of the city.";

					case 3:
						return "[Step #3]  Return to Jorund Bruttstein in Aegirhamn.";

					case 4:
						return "[Step #4] Give Jorund the marine fungus root.";

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
			if (player.IsDoingQuest(typeof(trialofstrength)) != null)
				return true;

			// Custom Code Begin

			// Custom  Code End


			if (player.Level > maximumLevel || player.Level < minimumLevel)
				return false;

			if (

			player.CharacterClass.ID != (byte)eCharacterClass.MidgardRogue &&
			player.CharacterClass.ID != (byte)eCharacterClass.Viking &&
				true)
			{
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
