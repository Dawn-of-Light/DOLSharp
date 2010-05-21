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
* Author:	uhlersoth (uhlersoth@gmail.com)
* Date:		2010-05-10
*
* Notes:
*  http://camelot.allakhazam.com/quests.html?cquest=3616
*/

using System;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.Behaviour;
using DOL.Language;
using log4net;

namespace DOL.GS.Quests.Hibernia
{
	public class BasicsOfCombat : BaseQuest
	{
		protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "Basics of Combat";
		protected const eRealm realm = eRealm.Hibernia;
		private const int minimumLevel = 1;
		private const int maximumLevel = 10;

		private const string questGiverName = "Master Gethin";
		private static GameNPC questGiver = null;

		private const string flimsyName = "the flimsy";
		private const int radius = 200;
		private const int flimsyX = 356185;
		private const int flimsyY = 363777;
		private const int flimsyZ = 0;

		private const string weakName = "weak";
		private const string standardName = "standard";
		private const string sturdyName = "sturdy";
		private const string hardenedName = "hardened";
		private const string toughName = "tough";
		private const string unyieldingName = "unyielding";
		private const string sparringGuardName = "sparring guard";

		private const string message1 = "Welcome, recruit! I am Master Gethin, and I will be your training instructor. We're at war with friends and foes of all kinds, so that's about as much small talk as you'll get out of me. Let's get on with your training, then! Are you [ready to learn]?";
		private const string message2 = "Good! First I'll teach you how to size up an enemy. Along the wall behind me, you'll see a row of practice dummies. Run up to the leftmost practice dummy, and click on it now.";
		private const string message3 = "Very good. When you have a monster targeted, its name and hit bar will appear at the bottom of your target window. Notice that as you continue attacking the practice dummy, its hit bar decreases. Were this a real monster, when the hit bar was depleted, it would die.";
		private const string message4 = "Press your attack key again to stop attacking, and take a moment to look at your target window. You'll notice that the flimsy dummy's name is colored grey, meaning the target is far weaker than you and poses no challenge relative to the level of your character. Next to the flimsy dummy is a weak dummy. Right-click on the weak dummy now.";
		private const string message5 = "This practice dummy has a green name, meaning it is weaker than you, and will be easily defeated. It's advisable to fight creatures with a green or blue names. Creatures with yellow, orange, red and purple names are more powerful and should only be attempted with help. You can continue to click on the other practice dummies if you wish to learn more. When you are ready to advance, right-click on Master Gethin.";
		private const string message6 = "The blue color of this dummy's name indicates that it is only slightly weaker than you, and should be reasonably easy to defeat.";
		private const string message7 = "This dummy's name is colored yellow, meaning that it is an even match for you. A fight with a creature of this type will be a challenge, and you will have an equal chance of success or failure.";
		private const string message8 = "A creature with an orange name is more powerful than you, but only slightly. A conflict with a creature of this type is risky, and should only be attempted with help.";
		private const string message9 = "When a creature's name is colored red, beware. Fighting such a creature alone will usually result in the death of your character.";
		private const string message10 = "A purple name indicates that a creature is vastly more powerful than you. Even with the help of a group, a creature of this magnitude will be difficult for you to defeat.";
		private const string message11 = "Now that you've had a look at the practice dummies, it's time to try out your skills against a live opponent. Nearby are some sparring guards. Attack one of them now to begin a sparring match.";
		private const string message12 = "Well fought! Rest now and when you're ready, right-click on Master Gethin to continue.";

		private const string missingInst = "Attack the practice dummy. To begin your attack, press the attack key. By default, this is your number 1 key.";

		private static IArea targetArea = null;

		public BasicsOfCombat()
			: base()
		{
			Init();
		}

		public BasicsOfCombat(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
			Init();
		}

		public BasicsOfCombat(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
			Init();
		}

		public BasicsOfCombat(GamePlayer questingPlayer, DBQuest dbQuest)
			: base(questingPlayer, dbQuest)
		{
			Init();
		}

		private void Init()
		{
			Level = 1;
		}

		public override int Level
		{
			get
			{
				return minimumLevel;
			}
		}

		public override string Name
		{
			get
			{
				return questTitle;
			}
		}

		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			if (!ServerProperties.Properties.LOAD_QUESTS)
				return;

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initializing ...");

			#region defineNPCS
			// Master Gethin
			GameNPC[] npcs = WorldMgr.GetNPCsByName(questGiverName, realm);

			if (npcs.Length == 0)
			{
				questGiver = new GameNPC();
				questGiver.Model = 350;
				questGiver.Name = questGiverName;
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + questGiver.Name + ", creating him ...");
				//k109: My preference, no guildname for quest NPCs.  Uncomment if you like that...
				//Richael.GuildName = "Part of " + questTitle + " Quest";
				questGiver.Realm = realm;
				questGiver.CurrentRegionID = 27;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.HandsArmor, 391, 0);		//Slot 22
				template.AddNPCEquipment(eInventorySlot.FeetArmor, 392, 0);			//Slot 23
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 667, 0);		//Slot 25
				template.AddNPCEquipment(eInventorySlot.Cloak, 678, 0);				//Slot 26
				template.AddNPCEquipment(eInventorySlot.LegsArmor, 389, 0);			//Slot 27
				template.AddNPCEquipment(eInventorySlot.ArmsArmor, 390, 0);			//Slot 28
				questGiver.Inventory = template.CloseTemplate();
				questGiver.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				questGiver.Size = 50;
				questGiver.Level = 65;
				questGiver.X = 356932;
				questGiver.Y = 363575;
				questGiver.Z = 5248;
				questGiver.Heading = 2912;

				if (SAVE_INTO_DATABASE)
					questGiver.SaveIntoDatabase();

				questGiver.AddToWorld();
			}
			else
				questGiver = npcs[0];

			#endregion

			#region defineAreas
			targetArea = questGiver.CurrentRegion.AddArea(new Area.Circle("", flimsyX, flimsyY, flimsyZ, radius));
			targetArea.RegisterPlayerEnter(new DOLEventHandler(PlayerApproachesFlimsyDummy));
			#endregion

			#region defineBehaviours
			QuestBuilder builder = QuestMgr.getBuilder(typeof(BasicsOfCombat));
			QuestBehaviour a = null;

			a = builder.CreateBehaviour(questGiver, -1);
			a.AddTrigger(eTriggerType.Interact, null, questGiver);
			a.AddRequirement(eRequirementType.QuestPending, typeof(BasicsOfCombat), null, (eComparator)5);
			a.AddRequirement(eRequirementType.QuestGivable, typeof(BasicsOfCombat), questGiver);
			a.AddAction(eActionType.Talk, message1, questGiver);
			AddBehaviour(a);

			a = builder.CreateBehaviour(questGiver, -1);
			a.AddTrigger(eTriggerType.Whisper, "ready to learn", questGiver);
			a.AddRequirement(eRequirementType.QuestPending, typeof(BasicsOfCombat), null, (eComparator)5);
			a.AddAction(eActionType.OfferQuest, typeof(BasicsOfCombat), "Do you want to learn about the Basics of Combat?");
			AddBehaviour(a);

			a = builder.CreateBehaviour(questGiver, -1);
			a.AddTrigger(eTriggerType.AcceptQuest, null, typeof(BasicsOfCombat));
			a.AddAction(eActionType.Talk, message2, questGiver);
			a.AddAction(eActionType.GiveQuest, typeof(BasicsOfCombat), questGiver);
			AddBehaviour(a);

			GameNPC[] weakDummies = (GameNPC[])WorldMgr.GetObjectsByName(weakName, eRealm.None, typeof(GameTrainingDummy));
			if (weakDummies != null)
			{
				for (int i = 0; i < weakDummies.Length; i++)
				{
					a = builder.CreateBehaviour(weakDummies[i], -1);
					a.AddTrigger(eTriggerType.Interact, null, weakDummies[i]);
					a.AddRequirement(eRequirementType.QuestStep, typeof(BasicsOfCombat), 3, eComparator.Equal);
					a.AddAction(eActionType.Talk, message5, questGiver);
					a.AddAction(eActionType.IncQuestStep, typeof(BasicsOfCombat), null);
					AddBehaviour(a);
				}
			}

			GameNPC[] standardDummies = (GameNPC[])WorldMgr.GetObjectsByName(standardName, eRealm.None, typeof(GameTrainingDummy));
			if (standardDummies != null)
			{
				for (int i = 0; i < standardDummies.Length; i++)
				{
					a = builder.CreateBehaviour(standardDummies[i], -1);
					a.AddTrigger(eTriggerType.Interact, null, standardDummies[i]);
					a.AddRequirement(eRequirementType.QuestStep, typeof(BasicsOfCombat), 4, eComparator.Equal);
					a.AddAction(eActionType.Talk, message6, questGiver);
					AddBehaviour(a);
				}
			}

			GameNPC[] sturdyDummies = (GameNPC[])WorldMgr.GetObjectsByName(sturdyName, eRealm.None, typeof(GameTrainingDummy));
			if (sturdyDummies != null)
			{
				for (int i = 0; i < sturdyDummies.Length; i++)
				{
					a = builder.CreateBehaviour(sturdyDummies[i], -1);
					a.AddTrigger(eTriggerType.Interact, null, sturdyDummies[i]);
					a.AddRequirement(eRequirementType.QuestStep, typeof(BasicsOfCombat), 4, eComparator.Equal);
					a.AddAction(eActionType.Talk, message7, questGiver);
					AddBehaviour(a);
				}
			}

			GameNPC[] hardenedDummies = (GameNPC[])WorldMgr.GetObjectsByName(hardenedName, eRealm.None, typeof(GameTrainingDummy));
			if (hardenedDummies != null)
			{
				for (int i = 0; i < hardenedDummies.Length; i++)
				{
					a = builder.CreateBehaviour(hardenedDummies[i], -1);
					a.AddTrigger(eTriggerType.Interact, null, hardenedDummies[i]);
					a.AddRequirement(eRequirementType.QuestStep, typeof(BasicsOfCombat), 4, eComparator.Equal);
					a.AddAction(eActionType.Talk, message8, questGiver);
					AddBehaviour(a);
				}
			}

			GameNPC[] toughDummies = (GameNPC[])WorldMgr.GetObjectsByName(toughName, eRealm.None, typeof(GameTrainingDummy));
			if (toughDummies != null)
			{
				for (int i = 0; i < toughDummies.Length; i++)
				{
					a = builder.CreateBehaviour(toughDummies[i], -1);
					a.AddTrigger(eTriggerType.Interact, null, toughDummies[i]);
					a.AddRequirement(eRequirementType.QuestStep, typeof(BasicsOfCombat), 4, eComparator.Equal);
					a.AddAction(eActionType.Talk, message9, questGiver);
					AddBehaviour(a);
				}
			}

			GameNPC[] unyieldingDummies = (GameNPC[])WorldMgr.GetObjectsByName(unyieldingName, eRealm.None, typeof(GameTrainingDummy));
			if (unyieldingDummies != null)
			{
				for (int i = 0; i < unyieldingDummies.Length; i++)
				{
					a = builder.CreateBehaviour(unyieldingDummies[i], -1);
					a.AddTrigger(eTriggerType.Interact, null, unyieldingDummies[i]);
					a.AddRequirement(eRequirementType.QuestStep, typeof(BasicsOfCombat), 4, eComparator.Equal);
					a.AddAction(eActionType.Talk, message10, questGiver);
					AddBehaviour(a);
				}
			}

			a = builder.CreateBehaviour(questGiver, -1);
			a.AddTrigger(eTriggerType.Interact, null, questGiver);
			a.AddRequirement(eRequirementType.QuestStep, typeof(BasicsOfCombat), 4, eComparator.Equal);
			a.AddAction(eActionType.IncQuestStep, typeof(BasicsOfCombat), null);
			a.AddAction(eActionType.Talk, message11, questGiver);
			AddBehaviour(a);

			a = builder.CreateBehaviour(questGiver, -1);
			a.AddTrigger(eTriggerType.EnemyKilled, sparringGuardName, null);
			a.AddRequirement(eRequirementType.QuestStep, typeof(BasicsOfCombat), 5, eComparator.Equal);
			a.AddAction(eActionType.IncQuestStep, typeof(BasicsOfCombat), null);
			a.AddAction(eActionType.CustomDialog, message12);
			AddBehaviour(a);

			a = builder.CreateBehaviour(questGiver, -1);
			a.AddTrigger(eTriggerType.Interact, null, questGiver);
			a.AddRequirement(eRequirementType.QuestStep, typeof(BasicsOfCombat), 6, eComparator.Equal);
			a.AddAction(eActionType.FinishQuest, typeof(BasicsOfCombat), null);
			AddBehaviour(a);

			GameEventMgr.AddHandler(GamePlayerEvent.ChangeTarget, new DOLEventHandler(TargetChanged));
			GameEventMgr.AddHandler(GameLivingEvent.AttackFinished, new DOLEventHandler(TargetAttacked));
			#endregion

			questGiver.AddQuestToGive(typeof(BasicsOfCombat));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			if (questGiver == null)
				return;

			GameEventMgr.RemoveHandler(GamePlayerEvent.ChangeTarget, new DOLEventHandler(TargetChanged));
			GameEventMgr.RemoveHandler(GameLivingEvent.AttackFinished, new DOLEventHandler(TargetAttacked));

			targetArea.UnRegisterPlayerEnter(new DOLEventHandler(PlayerApproachesFlimsyDummy));

			questGiver.RemoveQuestToGive(typeof(BasicsOfCombat));
		}

		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 1:
						return "Along the courtyard wall behind Master Gethin there is a row of practice dummies. The leftmost of these is called a 'flimsy dummy'. Left-click on the flimsy dummy to target it.";
					case 2:
						return "Attack the practice dummy. You must be standing near to it and have it targeted. To begin your attack, press the attack key. By default, this is your number 1 key.";
					case 3:
						return "To the right of the flimsy dummy is a weak dummy. Right-click on the weak dummy now.";
					case 4:
						return "Right-click on the other practice dummies to learn more about the various colored names, and what they tell you about the difficulty levels of your opponent. When you're done, right-click on Master Gethin.";
					case 5:
						return "It's time to test your fighting ability against a live opponent. There are some sparring guards standing near Master Gethin in the courtyard. When you're ready for a sparring match, simply initiate an attack against a sparing guard.";
					case 6:
						return "When you finish fighting the sparring guard, speak to Master Gethin.";
					default:
						return "No Queststep Description available.";
				}
			}
		}

		public override bool CheckQuestQualification(GamePlayer player)
		{
			// If the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(typeof(BasicsOfCombat)) != null)
				return true;

			// The checks below are only performed if the player isn't doing the quest already

			if (player.Level < minimumLevel || player.Level > maximumLevel)
				return false;

			return true;
		}

		protected static void TargetChanged(DOLEvent e, object sender, EventArgs args)
		{
			AreaEventArgs aargs = args as AreaEventArgs;
			GamePlayer player = sender as GamePlayer;

			if (player == null)
				return;

			if (player.IsDoingQuest(typeof(BasicsOfCombat)) == null)
				return;

			BasicsOfCombat quest = player.IsDoingQuest(typeof(BasicsOfCombat)) as BasicsOfCombat;

			if (quest.Step == 1 && player.TargetObject != null && player.TargetObject.GetName(0, false) == flimsyName && player.IsWithinRadius(player.TargetObject, radius))
			{
				quest.Step = quest.Step + 1;
				questGiver.SayTo(player, message3);
				player.Out.SendCustomDialog(missingInst, null);
				return;
			}
		}

		protected static void PlayerApproachesFlimsyDummy(DOLEvent e, object sender, EventArgs args)
		{
			AreaEventArgs aargs = args as AreaEventArgs;
			GamePlayer player = aargs.GameObject as GamePlayer;
			BasicsOfCombat quest = player.IsDoingQuest(typeof(BasicsOfCombat)) as BasicsOfCombat;

			if (player == null)
				return;

			if (quest == null)
				return;

			if (quest.Step == 1 && player.TargetObject != null && player.TargetObject.GetName(0, false) == flimsyName)
			{
				quest.Step = quest.Step + 1;
				questGiver.SayTo(player, message3);
				player.Out.SendCustomDialog(missingInst, null);
				return;
			}
		}

		protected static void TargetAttacked(DOLEvent e, object sender, EventArgs args)
		{
			AreaEventArgs aargs = args as AreaEventArgs;
			GamePlayer player = sender as GamePlayer;

			if (player == null)
				return;

			if (player.IsDoingQuest(typeof(BasicsOfCombat)) == null)
				return;

			BasicsOfCombat quest = player.IsDoingQuest(typeof(BasicsOfCombat)) as BasicsOfCombat;

			if (quest.Step == 2 && player.TargetObject != null && player.TargetObject.GetName(0, false) == flimsyName)
			{
				quest.Step = quest.Step + 1;
				questGiver.SayTo(player, message4);
				return;
			}
		}
	}
}
