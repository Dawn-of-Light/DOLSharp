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
*  http://camelot.allakhazam.com/quests.html?cquest=3615
*/

using System;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.Behaviour;
using DOL.Language;
using log4net;

namespace DOL.GS.Quests.Midgard {
	public class MovementAndInteraction : BaseQuest
	{
		protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "Movement and Interaction";
		private const int minimumLevel = 1;
		private const int maximumLevel = 10;

		private const string questGiverName = "Jarl Thorsa";
		private const string zoneName = "Thorsa Faste";

		private const string questTargetName = "Master Gethin";
		private static GameNPC questGiver = null;
		private static GameNPC questTarget = null;

		private static GameLocation targetLocation = new GameLocation("Tutorial", 27, 225834, 232501, 5248);
		private static IArea targetArea = null;

		public MovementAndInteraction() : base()
		{
			Init();
		}

		public MovementAndInteraction(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
			Init();
		}

		public MovementAndInteraction(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
			Init();
		}

		public MovementAndInteraction(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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
			GameNPC[] npcs = WorldMgr.GetNPCsByName(questGiverName, eRealm.Midgard);

			if (npcs.Length == 0)
			{
				questGiver = new GameNPC();
				questGiver.Model = 190;
				questGiver.Name = questGiverName;
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + questGiver.Name + ", creating him ...");
				questGiver.Realm = eRealm.Midgard;
				questGiver.CurrentRegionID = 27;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.HandsArmor, 779, 0);		//Slot 22
				template.AddNPCEquipment(eInventorySlot.FeetArmor, 780, 0);			//Slot 23
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 776, 0);		//Slot 25
				template.AddNPCEquipment(eInventorySlot.Cloak, 677, 0);				//Slot 26
				template.AddNPCEquipment(eInventorySlot.LegsArmor, 777, 0);			//Slot 27
				template.AddNPCEquipment(eInventorySlot.ArmsArmor, 778, 0);			//Slot 28
				questGiver.Inventory = template.CloseTemplate();
				questGiver.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				questGiver.Size = 55;
				questGiver.Level = 70;
				questGiver.X = 226594;
				questGiver.Y = 232390;
				questGiver.Z = 5340;
				questGiver.Heading = 3060;

				if (SAVE_INTO_DATABASE)
					questGiver.SaveIntoDatabase();

				questGiver.AddToWorld();
			}
			else
				questGiver = npcs[0];

			npcs = WorldMgr.GetNPCsByName(questTargetName, eRealm.Midgard);

			if (npcs.Length == 0)
			{
				questTarget = new GameNPC();
				questTarget.Model = 73;
				questTarget.Name = questTargetName;
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + questTarget.Name + ", creating him ...");
				questTarget.Realm = eRealm.Midgard;
				questTarget.CurrentRegionID = 27;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.HandsArmor, 2945, 0);		//Slot 22
				template.AddNPCEquipment(eInventorySlot.FeetArmor, 2946, 0);		//Slot 23
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 668, 0);		//Slot 25
				template.AddNPCEquipment(eInventorySlot.Cloak, 677, 0);				//Slot 26
				template.AddNPCEquipment(eInventorySlot.LegsArmor, 2943, 0);		//Slot 27
				template.AddNPCEquipment(eInventorySlot.ArmsArmor, 2944, 0);		//Slot 28
				questTarget.Inventory = template.CloseTemplate();
				questTarget.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				questTarget.Size = 50;
				questTarget.Level = 38;
				questTarget.X = 225834;
				questTarget.Y = 232501;
				questTarget.Z = 5248;
				questTarget.Heading = 2935;

				if (SAVE_INTO_DATABASE)
					questTarget.SaveIntoDatabase();

				questTarget.AddToWorld();
			}
			else
				questTarget = npcs[0];
			#endregion

			#region defineAreas
			targetArea = WorldMgr.GetRegion(targetLocation.RegionID).AddArea(new Area.Circle("", targetLocation.X, targetLocation.Y, targetLocation.Z, 200));
			#endregion

			#region defineBehaviours
			QuestBuilder builder = QuestMgr.getBuilder(typeof(MovementAndInteraction));
			QuestBehaviour a = null;
			string message1 = "Welcome to " + zoneName + ", <Class>. Here you will learn the basic skills needed to defend yourself as you explore our realm and grow in power and wisdom. Now, without further delay, let's get you started on your [training].";
			string message2 = "If you exit through the doors behind me, you will enter the courtyard. In the courtyard, you will find Master Gethin, who will be your training instructor. Go now and speak to Master Gethin.";

			a = builder.CreateBehaviour(questGiver, -1);
			a.AddTrigger(eTriggerType.Interact, null, questGiver);
			a.AddRequirement(eRequirementType.QuestPending, typeof(MovementAndInteraction), null, (eComparator)5);
			a.AddAction(eActionType.GiveQuest, typeof(MovementAndInteraction), questGiver);
			AddBehaviour(a);

			a = builder.CreateBehaviour(questGiver, -1);
			a.AddTrigger(eTriggerType.Interact, null, questGiver);
			a.AddRequirement(eRequirementType.QuestStep, typeof(MovementAndInteraction), 1, (eComparator)3);
			a.AddAction(eActionType.Talk, message1, questGiver);
			AddBehaviour(a);

			a = builder.CreateBehaviour(questGiver, -1);
			a.AddTrigger(eTriggerType.Whisper, "training", questGiver);
			a.AddRequirement(eRequirementType.QuestStep, typeof(MovementAndInteraction), 1, (eComparator)3);
			a.AddAction(eActionType.IncQuestStep, typeof(MovementAndInteraction), null);
			a.AddAction(eActionType.Talk, message2, questGiver);
			AddBehaviour(a);

			a = builder.CreateBehaviour(questGiver, -1);
			a.AddTrigger(eTriggerType.Interact, null, questGiver);
			a.AddRequirement(eRequirementType.QuestStep, typeof(MovementAndInteraction), 2, (eComparator)3);
			a.AddAction(eActionType.Talk, message2, questGiver);
			AddBehaviour(a);

			a = builder.CreateBehaviour(questGiver, -1);
			a.AddTrigger(eTriggerType.EnterArea, null, targetArea);
			a.AddRequirement(eRequirementType.QuestStep, typeof(MovementAndInteraction), 2, (eComparator)3);
			a.AddAction(eActionType.IncQuestStep, typeof(MovementAndInteraction), null);
			AddBehaviour(a);

			a = builder.CreateBehaviour(questTarget, -1);
			a.AddTrigger(eTriggerType.Interact, null, questTarget);
			a.AddRequirement(eRequirementType.QuestStep, typeof(MovementAndInteraction), 3, (eComparator)3);
			a.AddAction(eActionType.FinishQuest, typeof(MovementAndInteraction), null);
			AddBehaviour(a);
			#endregion

			questGiver.AddQuestToGive(typeof(MovementAndInteraction));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			if (questGiver == null)
				return;

			questGiver.RemoveQuestToGive(typeof(MovementAndInteraction));
		}

		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 1:
						return "Speak with " + questGiverName + " about your [training].";
					case 2:
						return "Left-click on the large doors behind " + questGiverName + " to open them, then enter the courtyard of the keep. Look for Master Gethin, and when you find him, right-click on him.";
					case 3:
						return "Master Gethin, the training instructor of " + zoneName + ", is waiting to speak with you. Right-click on him begin a conversation.";
					default:
						return "No Queststep Description available.";
				}
			}
		}

		public override bool CheckQuestQualification(GamePlayer player)
		{
			// If the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(typeof(MovementAndInteraction)) != null)
				return true;

			// The checks below are only performed if the player isn't doing the quest already

			if (player.Level < minimumLevel || player.Level > maximumLevel)
				return false;

			return true;
		}
	}
}