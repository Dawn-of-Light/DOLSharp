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

namespace DOL.GS.Quests.Hibernia {
	public class MovementAndInteraction : BaseQuest
	{
		protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "Movement and Interaction";
		private const int minimumLevel = 1;
		private const int maximumLevel = 10;

		private const string questGiverName = "Chieftess Cormyra";
		private const string zoneName = "Dun Cormyra";

		private const string questTargetName = "Master Gethin";
		private static GameNPC questGiver = null;
		private static GameNPC questTarget = null;

		private static GameLocation targetLocation = new GameLocation("Tutorial", 27, 356932, 363575, 5248);
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
			GameNPC[] npcs = WorldMgr.GetNPCsByName(questGiverName, eRealm.Hibernia);

			if (npcs.Length == 0)
			{
				questGiver = new GameNPC();
				questGiver.Model = 386;
				questGiver.Name = questGiverName;
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + questGiver.Name + ", creating her ...");
				questGiver.Realm = eRealm.Hibernia;
				questGiver.CurrentRegionID = 27;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.HandsArmor, 813, 0);		//Slot 22
				template.AddNPCEquipment(eInventorySlot.FeetArmor, 814, 0);			//Slot 23
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 810, 0);		//Slot 25
				template.AddNPCEquipment(eInventorySlot.Cloak, 678, 0);				//Slot 26
				template.AddNPCEquipment(eInventorySlot.LegsArmor, 811, 0);			//Slot 27
				template.AddNPCEquipment(eInventorySlot.ArmsArmor, 812, 0);			//Slot 28
				questGiver.Inventory = template.CloseTemplate();
				questGiver.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				questGiver.Size = 50;
				questGiver.Level = 70;
				questGiver.X = 357695;
				questGiver.Y = 363457;
				questGiver.Z = 5340;
				questGiver.Heading = 3117;

				if (SAVE_INTO_DATABASE)
					questGiver.SaveIntoDatabase();

				questGiver.AddToWorld();
			}
			else
				questGiver = npcs[0];

			npcs = WorldMgr.GetNPCsByName(questTargetName, eRealm.Hibernia);

			if (npcs.Length == 0)
			{
				questTarget = new GameNPC();
				questTarget.Model = 350;
				questTarget.Name = questTargetName;
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + questTarget.Name + ", creating him ...");
				questTarget.Realm = eRealm.Hibernia;
				questTarget.CurrentRegionID = 27;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.HandsArmor, 391, 0);		//Slot 22
				template.AddNPCEquipment(eInventorySlot.FeetArmor, 392, 0);			//Slot 23
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 667, 0);		//Slot 25
				template.AddNPCEquipment(eInventorySlot.Cloak, 678, 0);				//Slot 26
				template.AddNPCEquipment(eInventorySlot.LegsArmor, 389, 0);			//Slot 27
				template.AddNPCEquipment(eInventorySlot.ArmsArmor, 390, 0);			//Slot 28
				questTarget.Inventory = template.CloseTemplate();
				questTarget.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				questTarget.Size = 50;
				questTarget.Level = 65;
				questTarget.X = 356932;
				questTarget.Y = 363575;
				questTarget.Z = 5248;
				questTarget.Heading = 2912;

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