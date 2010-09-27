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
* Date:		2010-06-18
*
* Notes:
*  http://camelot.allakhazam.com/quests.html?cquest=2560
*/

using System;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.Behaviour;
using DOL.GS.Behaviour.Actions;
using DOL.Language;
using log4net;
using DOL.GS.Behaviour.Requirements;

namespace DOL.GS.Quests.Hibernia
{
	public class BonesToBlades : BaseQuest
	{
		protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "Bones to Blades";
		private const int minimumLevel = 2;
		private const int maximumLevel = 5;

		private const string questGiverName = "Wony";
		private const string zoneName = "Lough Derg";

		private const string questTargetName = "Jahan";
		private static GameNPC questGiver = null;
		private static GameNPC questTarget = null;

		private static ItemTemplate armBone = null;
		private static ItemTemplate carvedBoneHilt = null;

		private static string[] mobTypes = { "skeletal pawn", "skeletal minion" };

		//private static GameLocation targetLocation = new GameLocation("Tutorial", 27, 225834, 232501, 5248);
		//private static IArea targetArea = null;

		public BonesToBlades()
			: base()
		{
			Init();
		}

		public BonesToBlades(GamePlayer questingPlayer)
			: this(questingPlayer, 1)
		{
			Init();
		}

		public BonesToBlades(GamePlayer questingPlayer, int step)
			: base(questingPlayer, step)
		{
			Init();
		}

		public BonesToBlades(GamePlayer questingPlayer, DBQuest dbQuest)
			: base(questingPlayer, dbQuest)
		{
			Init();
		}

		private void Init()
		{
			Level = 2;
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
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + questGiver.Name + ", creating her ...");

				questGiver = new GameNPC();
				questGiver.Name = questGiverName;
				questGiver.Realm = eRealm.Hibernia;
				questGiver.CurrentRegionID = 200;

				// select * from NPCEquipment where TemplateID in (select EquipmentTemplateID from Mob where name = ?)
				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 448, 0);		// Slot 12
				template.AddNPCEquipment(eInventorySlot.FeetArmor, 427, 0);			// Slot 23
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 423, 0);		// Slot 25
				template.AddNPCEquipment(eInventorySlot.LegsArmor, 424, 0);			// Slot 27
				template.AddNPCEquipment(eInventorySlot.ArmsArmor, 425, 0);			// Slot 28
				questGiver.Inventory = template.CloseTemplate();
				questGiver.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				questGiver.Model = 388;
				questGiver.Size = 51;
				questGiver.Level = 35;
				questGiver.X = 346768;
				questGiver.Y = 489521;
				questGiver.Z = 5200;
				questGiver.Heading = 2594;

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
				questTarget.Name = questTargetName;
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + questTarget.Name + ", creating him ...");
				questTarget.Realm = eRealm.Hibernia;
				questTarget.CurrentRegionID = 200;

				// select * from NPCEquipment where TemplateID in (select EquipmentTemplateID from Mob where name = ?)
				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.HandsArmor, 411, 0);		// Slot 22
				template.AddNPCEquipment(eInventorySlot.FeetArmor, 412, 0);		// Slot 23
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 408, 0);		// Slot 25
				template.AddNPCEquipment(eInventorySlot.Cloak, 57, 34);				// Slot 26
				template.AddNPCEquipment(eInventorySlot.LegsArmor, 409, 0);		// Slot 27
				template.AddNPCEquipment(eInventorySlot.ArmsArmor, 410, 0);		// Slot 28
				questTarget.Inventory = template.CloseTemplate();
				questTarget.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				questTarget.Model = 381;
				questTarget.Size = 50;
				questTarget.Level = 12;
				questTarget.X = 347327;
				questTarget.Y = 492700;
				questTarget.Z = 5199;
				questTarget.Heading = 2468;

				if (SAVE_INTO_DATABASE)
					questTarget.SaveIntoDatabase();

				questTarget.AddToWorld();
			}
			else
				questTarget = npcs[0];
			#endregion

			/*
			#region defineAreas
			targetArea = WorldMgr.GetRegion(targetLocation.RegionID).AddArea(new Area.Circle("", targetLocation.X, targetLocation.Y, targetLocation.Z, 200));
			#endregion
			*/

			#region defineItems
			armBone = GameServer.Database.FindObjectByKey<ItemTemplate>("BonesToBlades-armbone");
			if (armBone == null) {
				armBone = new ItemTemplate();
				armBone.Name = "Arm Bone";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + armBone.Name + ", creating it ...");
				armBone.Level = 1;
				armBone.Weight = 1;
				armBone.Model = 497;
				armBone.Object_Type = (int) eObjectType.GenericItem;
				armBone.Item_Type = -1;
				armBone.Id_nb = "BonesToBlades-armbone";
				armBone.Hand = 0;
				armBone.Price = 0;
				armBone.IsTradable = false;
				armBone.Color = 0;
				armBone.Bonus = 0; // default bonus				
				armBone.Bonus1 = 0;
				armBone.Bonus1Type = (int)0;
				armBone.Bonus2 = 0;
				armBone.Bonus2Type = (int)0;
				armBone.Bonus3 = 0;
				armBone.Bonus3Type = (int)0;
				armBone.Bonus4 = 0;
				armBone.Bonus4Type = (int)0;
				armBone.Bonus5 = 0;
				armBone.Bonus5Type = (int)0;
				armBone.Bonus6 = 0;
				armBone.Bonus6Type = (int)0;
				armBone.Bonus7 = 0;
				armBone.Bonus7Type = (int)0;
				armBone.Bonus8 = 0;
				armBone.Bonus8Type = (int)0;
				armBone.Bonus9 = 0;
				armBone.Bonus9Type = (int)0;
				armBone.Bonus10 = 0;
				armBone.Bonus10Type = (int)0;
				armBone.ExtraBonus = 0;
				armBone.ExtraBonusType = (int)0;
				armBone.Effect = 0;
				armBone.Emblem = 0;
				armBone.Charges = 0;
				armBone.MaxCharges = 0;
				armBone.SpellID = 0;
				armBone.ProcSpellID = 0;
				armBone.Type_Damage = 0;
				armBone.Realm = 0;
				armBone.MaxCount = 1;
				armBone.PackSize = 1;
				armBone.Extension = 0;
				armBone.Quality = 100;
				armBone.Condition = 100;
				armBone.MaxCondition = 100;
				armBone.Durability = 100;
				armBone.MaxDurability = 100;
				armBone.PoisonCharges = 0;
				armBone.PoisonMaxCharges = 0;
				armBone.PoisonSpellID = 0;
				armBone.ProcSpellID1 = 0;
				armBone.SpellID1 = 0;
				armBone.MaxCharges1 = 0;
				armBone.Charges1 = 0;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(armBone);
			}

			carvedBoneHilt = GameServer.Database.FindObjectByKey<ItemTemplate>("BonesToBlades-carvedBoneHilts");
			if (carvedBoneHilt == null)
			{
				carvedBoneHilt = new ItemTemplate();
				carvedBoneHilt.Name = "Two Carved Bone Hilts";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + carvedBoneHilt.Name + ", creating it ...");
				carvedBoneHilt.Level = 1;
				carvedBoneHilt.Weight = 1;
				carvedBoneHilt.Model = 497;
				carvedBoneHilt.Object_Type = (int)eObjectType.GenericItem;
				carvedBoneHilt.Item_Type = -1;
				carvedBoneHilt.Id_nb = "BonesToBlades-carvedBoneHilts";
				carvedBoneHilt.Hand = 0;
				carvedBoneHilt.Price = 0;
				carvedBoneHilt.IsTradable = false;
				carvedBoneHilt.Color = 0;
				carvedBoneHilt.Bonus = 0; // default bonus				
				carvedBoneHilt.Bonus1 = 0;
				carvedBoneHilt.Bonus1Type = (int)0;
				carvedBoneHilt.Bonus2 = 0;
				carvedBoneHilt.Bonus2Type = (int)0;
				carvedBoneHilt.Bonus3 = 0;
				carvedBoneHilt.Bonus3Type = (int)0;
				carvedBoneHilt.Bonus4 = 0;
				carvedBoneHilt.Bonus4Type = (int)0;
				carvedBoneHilt.Bonus5 = 0;
				carvedBoneHilt.Bonus5Type = (int)0;
				carvedBoneHilt.Bonus6 = 0;
				carvedBoneHilt.Bonus6Type = (int)0;
				carvedBoneHilt.Bonus7 = 0;
				carvedBoneHilt.Bonus7Type = (int)0;
				carvedBoneHilt.Bonus8 = 0;
				carvedBoneHilt.Bonus8Type = (int)0;
				carvedBoneHilt.Bonus9 = 0;
				carvedBoneHilt.Bonus9Type = (int)0;
				carvedBoneHilt.Bonus10 = 0;
				carvedBoneHilt.Bonus10Type = (int)0;
				carvedBoneHilt.ExtraBonus = 0;
				carvedBoneHilt.ExtraBonusType = (int)0;
				carvedBoneHilt.Effect = 0;
				carvedBoneHilt.Emblem = 0;
				carvedBoneHilt.Charges = 0;
				carvedBoneHilt.MaxCharges = 0;
				carvedBoneHilt.SpellID = 0;
				carvedBoneHilt.ProcSpellID = 0;
				carvedBoneHilt.Type_Damage = 0;
				carvedBoneHilt.Realm = 0;
				carvedBoneHilt.MaxCount = 1;
				carvedBoneHilt.PackSize = 1;
				carvedBoneHilt.Extension = 0;
				carvedBoneHilt.Quality = 100;
				carvedBoneHilt.Condition = 100;
				carvedBoneHilt.MaxCondition = 100;
				carvedBoneHilt.Durability = 100;
				carvedBoneHilt.MaxDurability = 100;
				carvedBoneHilt.PoisonCharges = 0;
				carvedBoneHilt.PoisonMaxCharges = 0;
				carvedBoneHilt.PoisonSpellID = 0;
				carvedBoneHilt.ProcSpellID1 = 0;
				carvedBoneHilt.SpellID1 = 0;
				carvedBoneHilt.MaxCharges1 = 0;
				carvedBoneHilt.Charges1 = 0;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(carvedBoneHilt);
			}
			#endregion

			#region defineBehaviours
			QuestBuilder builder = QuestMgr.getBuilder(typeof(BonesToBlades));
			QuestBehaviour a = null;
			string message1 = "Oh, have you been standing there long, <Class>? If you have, I apologize for ignoring you. I find my mind is quite distracted these days after the meeting I had yesterday with a [new client].";
			string message2 = "Ah, I would love to tell you the name of the client, but I promised to keep their identity a secret. I keep the identity of all my clients secret because I don't want the other merchants in this area to try to steal them away from me! See, there I go again, getting distracted when I should be thinking about [those blades].";
			string message3 = "Day in and day out I sell these plain, everyday blades. But from time to time clients do approach me to commission special blades to be made. My newest client came to me and requested that I make a set of matched falcatas to be given as [a gift] to this client's daughter upon her next birthday.";
			string message4 = "The daughter is a highly skilled blademaster who is preparing to venture into the Frontiers. My client wants the daughter to be as well armed as possible and I seemed to have developed a reputation as a maker of some of the best custom blades out there. I hope I can [live up to] that reputation.";
			string message5 = "I fear I have agreed to make these falcatas before I checked to see if I have all the supplies I need. It turns out I'm missing a few things, and I need to have the blades done within a few days. Making the blades alone will take me all that time without having to gather the materials for [the hilt] of each blade.";
			string message6 = "My client has asked that the hilts of these falcatas be carved from bone. I would be happy to do that, but I don't have the bones I need. I need to get them and send them over to Jahan so he can carve them for me. Say, if you're not busy, perhaps [you can go] get the bones I need. I can pay you for your time.";
			string message7 = "I cannot thank you enough for agreeing to help me. I've found that when people ask for carved bone hilts that the best bones come from the skeletal pawns and minions that roam the lands [just outside] of Mag Mell.";
			string message8 = "You'll find the skeletal pawns and minions across the road on the hill northwest of here. I've also seen them in the field and by the standing stone in the woods east-southeast of Rumdor the Stable Master here in Mag Mell. Get two arm bones from either the pawns or the minions. When you have them, take them to Jahan here in Mag Mell. He will carve them for me. Good luck, <Class>!";
			string message9 = "Ah, you must be the young <Class> that Wony told me about. She said you would have two arm bones that needed carving for hilts. Wony thinks it will take me a few days to get those bones carved but I have [a surprise] for her.";
			string message10 = "I already had some bones among my supplies. I went ahead and carved them while you were out obtaining more bones. I'll give you the carved ones and I'll take the ones you have so I can keep my supplies well stocked. Why don't you hand me those two arm bones now?";
			string message11 = "There you go, <Class>. Take those two carved bones to Wony right now. It will put her mind at ease having those hilts already taken care of. Don't worry about payment for my work; Wony has taken care of that already.";
			string message12 = "<Class>, what are you doing back here already? I told you to take the arm bones to Jahan so he can carve them! You should have listened to me! Now what am I going to do?";
			string message13 = "What is this? These hilts are already carved! Jahan played a trick on me, didn't he? He already had these done. I guess the arm bones I had you collect will get used the next time I need bone hilts. I am sorry for yelling at you when I should have been offering you [the payment] I promised you.";
			string message14 = "There we go, <Class>. Thank you so much for helping me get these bone hilts. I shall be able to get the matching falcatas done on time and keep my new client. Perhaps one day you will have enough platinum to hire me to make custom blades for you. Until then, be well!";

			a = builder.CreateBehaviour(questGiver, -1);
			a.AddTrigger(eTriggerType.Interact, null, questGiver);
			a.AddRequirement(eRequirementType.QuestGivable, typeof(BonesToBlades), questGiver);
			a.AddRequirement(eRequirementType.QuestPending, typeof(BonesToBlades), null, (eComparator)5);
			a.AddAction(eActionType.Talk, message1, questGiver);
			AddBehaviour(a);

			a = builder.CreateBehaviour(questGiver, -1);
			a.AddTrigger(eTriggerType.Whisper, "new client", questGiver);
			a.AddRequirement(eRequirementType.QuestGivable, typeof(BonesToBlades), questGiver);
			a.AddRequirement(eRequirementType.QuestPending, typeof(BonesToBlades), null, (eComparator)5);
			a.AddAction(eActionType.Talk, message2, questGiver);
			AddBehaviour(a);

			a = builder.CreateBehaviour(questGiver, -1);
			a.AddTrigger(eTriggerType.Whisper, "those blades", questGiver);
			a.AddRequirement(eRequirementType.QuestGivable, typeof(BonesToBlades), questGiver);
			a.AddRequirement(eRequirementType.QuestPending, typeof(BonesToBlades), null, (eComparator)5);
			a.AddAction(eActionType.Talk, message3, questGiver);
			AddBehaviour(a);

			a = builder.CreateBehaviour(questGiver, -1);
			a.AddTrigger(eTriggerType.Whisper, "a gift", questGiver);
			a.AddRequirement(eRequirementType.QuestGivable, typeof(BonesToBlades), questGiver);
			a.AddRequirement(eRequirementType.QuestPending, typeof(BonesToBlades), null, (eComparator)5);
			a.AddAction(eActionType.Talk, message4, questGiver);
			AddBehaviour(a);

			a = builder.CreateBehaviour(questGiver, -1);
			a.AddTrigger(eTriggerType.Whisper, "live up to", questGiver);
			a.AddRequirement(eRequirementType.QuestGivable, typeof(BonesToBlades), questGiver);
			a.AddRequirement(eRequirementType.QuestPending, typeof(BonesToBlades), null, (eComparator)5);
			a.AddAction(eActionType.Talk, message5, questGiver);
			AddBehaviour(a);

			a = builder.CreateBehaviour(questGiver, -1);
			a.AddTrigger(eTriggerType.Whisper, "the hilt", questGiver);
			a.AddRequirement(eRequirementType.QuestGivable, typeof(BonesToBlades), questGiver);
			a.AddRequirement(eRequirementType.QuestPending, typeof(BonesToBlades), null, (eComparator)5);
			a.AddAction(new MessageAction(questGiver, questGiverName + " blushes a deep red.", eTextType.Emote));
			a.AddAction(eActionType.Talk, message6, questGiver);
			AddBehaviour(a);

			a = builder.CreateBehaviour(questGiver, -1);
			a.AddTrigger(eTriggerType.Whisper, "you can go", questGiver);
			a.AddRequirement(eRequirementType.QuestGivable, typeof(BonesToBlades), questGiver);
			a.AddRequirement(eRequirementType.QuestPending, typeof(BonesToBlades), null, (eComparator)5);
			a.AddAction(eActionType.OfferQuest, typeof(BonesToBlades), "Do you want to help Wony?");
			AddBehaviour(a);

			a = builder.CreateBehaviour(questGiver, 1);
			a.AddTrigger(eTriggerType.AcceptQuest, null, typeof(BonesToBlades));
			a.AddAction(eActionType.Talk, message7, questGiver);
			a.AddAction(eActionType.GiveQuest, typeof(BonesToBlades), questGiver);
			AddBehaviour(a);

			a = builder.CreateBehaviour(questGiver, -1);
			a.AddTrigger(eTriggerType.Whisper, "just outside", questGiver);
			a.AddRequirement(eRequirementType.QuestStep, typeof(BonesToBlades), 1, eComparator.Equal);
			a.AddAction(eActionType.Talk, message8, questGiver);
			AddBehaviour(a);

			//a = builder.CreateBehaviour(questGiver, -1);
			//a.AddTrigger(eTriggerType.EnemyKilled, "skeletal pawn", null);
			//a.AddTrigger(eTriggerType.EnemyKilled, "skeletal minion", null);
			//a.AddRequirement(eRequirementType.QuestStep, typeof(BonesToBlades), 0, eComparator.Greater);
			//a.AddRequirement(eRequirementType.InventoryItem, armBone, 2, eComparator.Less);
			//a.AddAction(eActionType.GiveItem, armBone, null);
			//AddBehaviour(a);

			//a = builder.CreateBehaviour(questGiver, -1);
			//a.AddTrigger(eTriggerType.EnemyKilled, mobTypes[0], null);
			//a.AddTrigger(eTriggerType.EnemyKilled, mobTypes[1], null);
			//a.AddRequirement(eRequirementType.QuestStep, typeof(BonesToBlades), 1, eComparator.Equal);
			//a.AddRequirement(eRequirementType.InventoryItem, armBone, 1, eComparator.Greater);
			//a.AddAction(eActionType.SetQuestStep, typeof(BonesToBlades), 2);
			//AddBehaviour(a);

			a = builder.CreateBehaviour(questTarget, -1);
			a.AddTrigger(eTriggerType.Interact, null, questTarget);
			a.AddRequirement(eRequirementType.QuestStep, typeof(BonesToBlades), 2, eComparator.Equal);
			a.AddAction(eActionType.Talk, message9, questTarget);
			AddBehaviour(a);

			a = builder.CreateBehaviour(questTarget, -1);
			a.AddTrigger(eTriggerType.Whisper, "a surprise", questTarget);
			a.AddRequirement(eRequirementType.QuestStep, typeof(BonesToBlades), 2, eComparator.Equal);
			a.AddAction(eActionType.Talk, message10, questTarget);
			a.AddAction(eActionType.SetQuestStep, typeof(BonesToBlades), 3);
			AddBehaviour(a);

			a = builder.CreateBehaviour(questTarget, -1);
			a.AddTrigger(eTriggerType.GiveItem, questTarget, armBone);
			a.AddRequirement(eRequirementType.QuestStep, typeof(BonesToBlades), 4, eComparator.Equal);
			a.AddAction(eActionType.SetQuestStep, typeof(BonesToBlades), 5);
			a.AddAction(eActionType.TakeItem, armBone, null);
			a.AddAction(eActionType.GiveItem, carvedBoneHilt, null);
			a.AddAction(eActionType.Talk, message11, questTarget);
			AddBehaviour(a);

			a = builder.CreateBehaviour(questTarget, -1);
			a.AddTrigger(eTriggerType.GiveItem, questTarget, armBone);
			a.AddRequirement(eRequirementType.QuestStep, typeof(BonesToBlades), 3, eComparator.Equal);
			a.AddAction(eActionType.SetQuestStep, typeof(BonesToBlades), 4);
			a.AddAction(eActionType.TakeItem, armBone, null);
			AddBehaviour(a);

			a = builder.CreateBehaviour(questGiver, -1);
			a.AddTrigger(eTriggerType.Interact, null, questGiver);
			a.AddRequirement(eRequirementType.QuestStep, typeof(BonesToBlades), 5, eComparator.Equal);
			a.AddAction(eActionType.Talk, message12, questGiver);
			a.AddAction(eActionType.SetQuestStep, typeof(BonesToBlades), 6);
			AddBehaviour(a);

			a = builder.CreateBehaviour(questGiver, -1);
			a.AddTrigger(eTriggerType.GiveItem, questGiver, carvedBoneHilt);
			a.AddRequirement(eRequirementType.QuestStep, typeof(BonesToBlades), 6, eComparator.Equal);
			a.AddAction(eActionType.SetQuestStep, typeof(BonesToBlades), 7);
			a.AddAction(eActionType.TakeItem, carvedBoneHilt, null);
			a.AddAction(eActionType.Talk, message13, questGiver);
			AddBehaviour(a);

			a = builder.CreateBehaviour(questGiver, -1);
			a.AddTrigger(eTriggerType.Whisper, "the payment", questGiver);
			a.AddRequirement(eRequirementType.QuestStep, typeof(BonesToBlades), 7, eComparator.Equal);
			a.AddAction(eActionType.Talk, message14, questGiver);
			a.AddAction(eActionType.GiveXP, 20, null);
			a.AddAction(eActionType.GiveGold, 37, null);
			a.AddAction(eActionType.FinishQuest, typeof(BonesToBlades), null);
			AddBehaviour(a);
			#endregion

			questGiver.AddQuestToGive(typeof(BonesToBlades));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			if (questGiver == null)
				return;

			questGiver.RemoveQuestToGive(typeof(BonesToBlades));
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			base.Notify(e, sender, args);
			GamePlayer player = sender as GamePlayer;

			if (player == null)
				return;

			AbstractQuest quest = player.IsDoingQuest(typeof(BonesToBlades));
			if (quest == null)
				return;

			if (Step > 0 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
				for (int i = 0; i < mobTypes.Length; i++)
				{
					if (gArgs.Target.Name.IndexOf(mobTypes[i]) >= 0)
					{
						InventoryItemRequirement requirement = new InventoryItemRequirement(null, armBone, 2, eComparator.Less);
						if (requirement.Check(e, player, args))
						{
							GiveItemAction action = new GiveItemAction(null, armBone, null);
							action.Perform(e, sender, args);
						}

						if (!requirement.Check(e, player, args))
						{
							if (quest.Step == 1)
							{
								quest.Step = 2;
							}
						}

						return;
					}
				}
			}
		}

		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 1:
						return "Get two Arm Bones from either the skeletal pawns or the skeletal minions.";
					case 2:
						return "Talk to Jahan in Mag Mell.";
					case 3:
						return "Give one of the Arm Bones to Jahan.";
					case 4:
						return "Give another one of the Arm Bones to Jahan.";
					case 5:
						return "Talk to Wony in Mag Mell.";
					case 6:
						return "Give the Two Carved Bone Hilts to Wony.";
					case 7:
						return "Talk to Wony about [the payment].";
					default:
						return "No Queststep Description available.";
				}
			}
		}

		public override bool CheckQuestQualification(GamePlayer player)
		{
			// If the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(typeof(BonesToBlades)) != null)
				return true;

			// The checks below are only performed if the player isn't doing the quest already

			if (player.Level < minimumLevel || player.Level > maximumLevel)
				return false;

			return true;
		}
	}
}
