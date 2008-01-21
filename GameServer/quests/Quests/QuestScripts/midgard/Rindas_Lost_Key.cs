	
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
	public class rindaslostkey : BaseQuest
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

		protected const string questTitle = "Rinda's Lost Key";

		protected const int minimumLevel = 1;
		protected const int maximumLevel = 11;
	
	
		private static GameNPC DwarvenGuardRinda = null;
		
		private static GameNPC hobgoblinsnakefinder = null;
		
		private static ItemTemplate rindaskey = null;
		
		private static ItemTemplate ironkeychain = null;
		
		private static ItemTemplate silverringofhealth = null;
		

		// Custom Initialization Code Begin
		
		// Custom Initialization Code End

		/* 
		* Constructor
		*/
		public rindaslostkey() : base()
		{
		}

		public rindaslostkey(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public rindaslostkey(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public rindaslostkey(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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
	
			npcs = WorldMgr.GetNPCsByName("Dwarven Guard Rinda",(eRealm) 2);
			if (npcs.Length == 0)
			{
				if (!WorldMgr.GetRegion(100).IsDisabled)
				{
				DwarvenGuardRinda = new DOL.GS.GameNPC();
					DwarvenGuardRinda.Model = 238;
				DwarvenGuardRinda.Name = "Dwarven Guard Rinda";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + DwarvenGuardRinda.Name + ", creating ...");
				DwarvenGuardRinda.GuildName = "Part of " + questTitle + " Quest";
				DwarvenGuardRinda.Realm = eRealm.Midgard;
				DwarvenGuardRinda.CurrentRegionID = 100;
				DwarvenGuardRinda.Size = 53;
				DwarvenGuardRinda.Level = 41;
				DwarvenGuardRinda.MaxSpeedBase = 191;
				DwarvenGuardRinda.Faction = FactionMgr.GetFactionByID(0);
				DwarvenGuardRinda.X = 805496;
				DwarvenGuardRinda.Y = 701215;
				DwarvenGuardRinda.Z = 4960;
				DwarvenGuardRinda.Heading = 1570;
				DwarvenGuardRinda.RespawnInterval = -1;
				DwarvenGuardRinda.BodyType = 0;
				

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 500;
				DwarvenGuardRinda.SetOwnBrain(brain);
				
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					DwarvenGuardRinda.SaveIntoDatabase();
					
				DwarvenGuardRinda.AddToWorld();
				
				}
			}
			else 
			{
				DwarvenGuardRinda = npcs[0];
			}
		
			npcs = WorldMgr.GetNPCsByName("hobgoblin snake-finder",(eRealm) 0);
			if (npcs.Length == 0)
			{
				if (!WorldMgr.GetRegion(100).IsDisabled)
				{
				hobgoblinsnakefinder = new DOL.GS.GameNPC();
					hobgoblinsnakefinder.Model = 251;
				hobgoblinsnakefinder.Name = "hobgoblin snake-finder";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + hobgoblinsnakefinder.Name + ", creating ...");
				hobgoblinsnakefinder.GuildName = "Part of " + questTitle + " Quest";
				hobgoblinsnakefinder.Realm = eRealm.None;
				hobgoblinsnakefinder.CurrentRegionID = 100;
				hobgoblinsnakefinder.Size = 37;
				hobgoblinsnakefinder.Level = 1;
				hobgoblinsnakefinder.MaxSpeedBase = 191;
				hobgoblinsnakefinder.Faction = FactionMgr.GetFactionByID(0);
				hobgoblinsnakefinder.X = 803189;
				hobgoblinsnakefinder.Y = 695157;
				hobgoblinsnakefinder.Z = 4926;
				hobgoblinsnakefinder.Heading = 125;
				hobgoblinsnakefinder.RespawnInterval = -1;
				hobgoblinsnakefinder.BodyType = 0;
				

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 500;
				hobgoblinsnakefinder.SetOwnBrain(brain);
				
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					hobgoblinsnakefinder.SaveIntoDatabase();
					
				hobgoblinsnakefinder.AddToWorld();
				
				}
			}
			else 
			{
				hobgoblinsnakefinder = npcs[0];
			}
		

			#endregion

			#region defineItems

		rindaskey = (ItemTemplate) DatabaseLayer.Instance.SelectObject(typeof (ItemTemplate),"Id_nb", "rindaskey");
			if (rindaskey == null)
			{
				rindaskey = new ItemTemplate();
				rindaskey.Name = "Rinda's Key";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + rindaskey.Name + ", creating it ...");
				rindaskey.Level = 50;
				rindaskey.Weight = 5;
				rindaskey.Model = 583;
				rindaskey.Object_Type = 0;
				rindaskey.Item_Type = 40;
				rindaskey.Id_nb = "rindaskey";
				rindaskey.Hand = 0;
				rindaskey.Platinum = 0;
				rindaskey.Gold = 0;
				rindaskey.Silver = 0;
				rindaskey.Copper = 0;
				rindaskey.IsPickable = true;
				rindaskey.IsDropable = true;
				rindaskey.IsTradable = true;
				rindaskey.CanDropAsLoot = false;
				rindaskey.Color = 0;
				rindaskey.Bonus = 35; // default bonus				
				rindaskey.Bonus1 = 0;
				rindaskey.Bonus1Type = (int) 0;
				rindaskey.Bonus2 = 0;
				rindaskey.Bonus2Type = (int) 0;
				rindaskey.Bonus3 = 0;
				rindaskey.Bonus3Type = (int) 0;
				rindaskey.Bonus4 = 0;
				rindaskey.Bonus4Type = (int) 0;
				rindaskey.Bonus5 = 0;
				rindaskey.Bonus5Type = (int) 0;
				rindaskey.Bonus6 = 0;
				rindaskey.Bonus6Type = (int) 0;
				rindaskey.Bonus7 = 0;
				rindaskey.Bonus7Type = (int) 0;
				rindaskey.Bonus8 = 0;
				rindaskey.Bonus8Type = (int) 0;
				rindaskey.Bonus9 = 0;
				rindaskey.Bonus9Type = (int) 0;
				rindaskey.Bonus10 = 0;
				rindaskey.Bonus10Type = (int) 0;
				rindaskey.ExtraBonus = 0;
				rindaskey.ExtraBonusType = (int) 0;
				rindaskey.Effect = 0;
				rindaskey.Emblem = 0;
				rindaskey.Charges = 0;
				rindaskey.MaxCharges = 0;
				rindaskey.SpellID = 0;
				rindaskey.ProcSpellID = 0;
				rindaskey.Type_Damage = 0;
				rindaskey.Realm = 0;
				rindaskey.MaxCount = 1;
				rindaskey.PackSize = 1;
				rindaskey.Extension = 0;
				rindaskey.Quality = 99;				
				rindaskey.Condition = 50000;
				rindaskey.MaxCondition = 50000;
				rindaskey.Durability = 50000;
				rindaskey.MaxDurability = 50000;
				rindaskey.PoisonCharges = 0;
				rindaskey.PoisonMaxCharges = 0;
				rindaskey.PoisonSpellID = 0;
				rindaskey.ProcSpellID1 = 0;
				rindaskey.SpellID1 = 0;
				rindaskey.MaxCharges1 = 0;
				rindaskey.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(rindaskey);
				}
			ironkeychain = (ItemTemplate) DatabaseLayer.Instance.SelectObject(typeof (ItemTemplate),"Id_nb", "ironkeychain");
			if (ironkeychain == null)
			{
				ironkeychain = new ItemTemplate();
				ironkeychain.Name = "Iron Keychain";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + ironkeychain.Name + ", creating it ...");
				ironkeychain.Level = 50;
				ironkeychain.Weight = 5;
				ironkeychain.Model = 583;
				ironkeychain.Object_Type = 0;
				ironkeychain.Item_Type = 40;
				ironkeychain.Id_nb = "ironkeychain";
				ironkeychain.Hand = 0;
				ironkeychain.Platinum = 0;
				ironkeychain.Gold = 0;
				ironkeychain.Silver = 0;
				ironkeychain.Copper = 0;
				ironkeychain.IsPickable = true;
				ironkeychain.IsDropable = true;
				ironkeychain.IsTradable = true;
				ironkeychain.CanDropAsLoot = false;
				ironkeychain.Color = 0;
				ironkeychain.Bonus = 35; // default bonus				
				ironkeychain.Bonus1 = 0;
				ironkeychain.Bonus1Type = (int) 0;
				ironkeychain.Bonus2 = 0;
				ironkeychain.Bonus2Type = (int) 0;
				ironkeychain.Bonus3 = 0;
				ironkeychain.Bonus3Type = (int) 0;
				ironkeychain.Bonus4 = 0;
				ironkeychain.Bonus4Type = (int) 0;
				ironkeychain.Bonus5 = 0;
				ironkeychain.Bonus5Type = (int) 0;
				ironkeychain.Bonus6 = 0;
				ironkeychain.Bonus6Type = (int) 0;
				ironkeychain.Bonus7 = 0;
				ironkeychain.Bonus7Type = (int) 0;
				ironkeychain.Bonus8 = 0;
				ironkeychain.Bonus8Type = (int) 0;
				ironkeychain.Bonus9 = 0;
				ironkeychain.Bonus9Type = (int) 0;
				ironkeychain.Bonus10 = 0;
				ironkeychain.Bonus10Type = (int) 0;
				ironkeychain.ExtraBonus = 0;
				ironkeychain.ExtraBonusType = (int) 0;
				ironkeychain.Effect = 0;
				ironkeychain.Emblem = 0;
				ironkeychain.Charges = 0;
				ironkeychain.MaxCharges = 0;
				ironkeychain.SpellID = 0;
				ironkeychain.ProcSpellID = 0;
				ironkeychain.Type_Damage = 0;
				ironkeychain.Realm = 0;
				ironkeychain.MaxCount = 1;
				ironkeychain.PackSize = 1;
				ironkeychain.Extension = 0;
				ironkeychain.Quality = 99;				
				ironkeychain.Condition = 50000;
				ironkeychain.MaxCondition = 50000;
				ironkeychain.Durability = 50000;
				ironkeychain.MaxDurability = 50000;
				ironkeychain.PoisonCharges = 0;
				ironkeychain.PoisonMaxCharges = 0;
				ironkeychain.PoisonSpellID = 0;
				ironkeychain.ProcSpellID1 = 0;
				ironkeychain.SpellID1 = 0;
				ironkeychain.MaxCharges1 = 0;
				ironkeychain.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(ironkeychain);
				}
			silverringofhealth = (ItemTemplate) DatabaseLayer.Instance.SelectObject(typeof (ItemTemplate),"Id_nb", "silverringofhealth");
			if (silverringofhealth == null)
			{
				silverringofhealth = new ItemTemplate();
				silverringofhealth.Name = "Silver Ring of Health";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + silverringofhealth.Name + ", creating it ...");
				silverringofhealth.Level = 5;
				silverringofhealth.Weight = 5;
				silverringofhealth.Model = 103;
				silverringofhealth.Object_Type = 41;
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
			

			#endregion

			#region defineAreas
			
		#endregion
		
		#region defineQuestParts

		QuestBuilder builder = QuestMgr.getBuilder(typeof(rindaslostkey));
			QuestBehaviour a;
			a = builder.CreateBehaviour(DwarvenGuardRinda,-1);
				a.AddTrigger(eTriggerType.Interact,null,DwarvenGuardRinda);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.rindaslostkey),DwarvenGuardRinda);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.rindaslostkey),null,(eComparator)5);
			a.AddAction(eActionType.Talk,"Greetings, and welcome to the dwarf town of Haggerfel. I wish I could stay and talk with you for a moment, but I am in a hurry to find my [lost keys].",DwarvenGuardRinda);
			AddBehaviour(a);
			a = builder.CreateBehaviour(DwarvenGuardRinda,-1);
				a.AddTrigger(eTriggerType.Whisper,"lost keys",DwarvenGuardRinda);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.rindaslostkey),DwarvenGuardRinda);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.rindaslostkey),null,(eComparator)5);
			a.AddAction(eActionType.Talk,"Oh yes. It is dreadful that I was so careless as to lost them like this! You see, I am responsible for helping to lock the money the merchants bring in during the day into a large chest. I am the only one with the keys, and I have [misplaced them].",DwarvenGuardRinda);
			AddBehaviour(a);
			a = builder.CreateBehaviour(DwarvenGuardRinda,-1);
				a.AddTrigger(eTriggerType.Whisper,"misplaced them",DwarvenGuardRinda);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.rindaslostkey),DwarvenGuardRinda);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.rindaslostkey),null,(eComparator)5);
			a.AddAction(eActionType.Talk,"Oh, this makes me so angry! I have searched all around Haggerfel for them, but they are no where to be found. I don't suppose you would have a [little time] to help me out, would you?",DwarvenGuardRinda);
			AddBehaviour(a);
			a = builder.CreateBehaviour(DwarvenGuardRinda,-1);
				a.AddTrigger(eTriggerType.Whisper,"little time",DwarvenGuardRinda);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.rindaslostkey),DwarvenGuardRinda);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.rindaslostkey),null,(eComparator)5);
			a.AddAction(eActionType.OfferQuest,typeof(DOL.GS.Quests.Midgard.rindaslostkey),"Will you help Rinda find her keys?");
			AddBehaviour(a);
			a = builder.CreateBehaviour(DwarvenGuardRinda,-1);
				a.AddTrigger(eTriggerType.DeclineQuest,null,typeof(DOL.GS.Quests.Midgard.rindaslostkey));
			a.AddAction(eActionType.Talk,"No problem. See you",DwarvenGuardRinda);
			AddBehaviour(a);
			a = builder.CreateBehaviour(DwarvenGuardRinda,-1);
				a.AddTrigger(eTriggerType.AcceptQuest,null,typeof(DOL.GS.Quests.Midgard.rindaslostkey));
			a.AddAction(eActionType.GiveQuest,typeof(DOL.GS.Quests.Midgard.rindaslostkey),DwarvenGuardRinda);
			a.AddAction(eActionType.Talk,"Thank you! Thank you! I know there are some hobgoblins around that like to play jokes on people, viscious and mean ones. Why don't you check there while I check the merchant huts again?",DwarvenGuardRinda);
			AddBehaviour(a);
			a = builder.CreateBehaviour(hobgoblinsnakefinder,-1);
				a.AddTrigger(eTriggerType.Interact,null,hobgoblinsnakefinder);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.rindaslostkey),1,(eComparator)3);
			a.AddAction(eActionType.Talk,"Ha ha! Me no have keys! Feed to hungry bear! haha! You no help Rinda...Gold be ours!",hobgoblinsnakefinder);
			a.AddAction(eActionType.GiveItem,ironkeychain,hobgoblinsnakefinder);
			a.AddAction(eActionType.IncQuestStep,typeof(DOL.GS.Quests.Midgard.rindaslostkey),null);
			AddBehaviour(a);
			a = builder.CreateBehaviour(DwarvenGuardRinda,-1);
				a.AddTrigger(eTriggerType.EnemyKilled,"black mauler cub",null);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.rindaslostkey),2,(eComparator)3);
			a.AddAction(eActionType.GiveItem,rindaskey,null);
			a.AddAction(eActionType.IncQuestStep,typeof(rindaslostkey),null);
			AddBehaviour(a);
			a = builder.CreateBehaviour(DwarvenGuardRinda,-1);
				a.AddTrigger(eTriggerType.Interact,null,DwarvenGuardRinda);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.rindaslostkey),3,(eComparator)3);
			a.AddAction(eActionType.Talk,"I couldn't find them again in the village. Did you have better luck?",DwarvenGuardRinda);
			AddBehaviour(a);
			a = builder.CreateBehaviour(DwarvenGuardRinda,-1);
				a.AddTrigger(eTriggerType.GiveItem,DwarvenGuardRinda,ironkeychain);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.rindaslostkey),3,(eComparator)3);
			a.AddAction(eActionType.Talk,"Oh my keychain. Well, this is a good start. But you didn't happen to find my key, did you?",DwarvenGuardRinda);
			a.AddAction(eActionType.GiveXP,20,null);
			a.AddAction(eActionType.GiveGold,225,null);
			a.AddAction(eActionType.TakeItem,ironkeychain,null);
			AddBehaviour(a);
			a = builder.CreateBehaviour(DwarvenGuardRinda,-1);
				a.AddTrigger(eTriggerType.GiveItem,DwarvenGuardRinda,rindaskey);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.rindaslostkey),3,(eComparator)3);
			a.AddRequirement(eRequirementType.InventoryItem,ironkeychain,null,(eComparator)3);
			a.AddAction(eActionType.Talk,"Ah! This is great! My key! You have done a fabulous job in helping me. Please accept this coin in return for your time and effort. Thank you so much! Now, I can safely lock up the town's money. Thank you!",DwarvenGuardRinda);
			a.AddAction(eActionType.TakeItem,rindaskey,null);
			a.AddAction(eActionType.TakeItem,ironkeychain,null);
			a.AddAction(eActionType.GiveXP,40,null);
			a.AddAction(eActionType.GiveGold,450,null);
			a.AddAction(eActionType.FinishQuest,typeof(DOL.GS.Quests.Midgard.rindaslostkey),null);
			a.AddAction(eActionType.GiveItem,silverringofhealth,DwarvenGuardRinda);
			AddBehaviour(a);
			a = builder.CreateBehaviour(DwarvenGuardRinda,-1);
				a.AddTrigger(eTriggerType.GiveItem,DwarvenGuardRinda,rindaskey);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.rindaslostkey),3,(eComparator)3);
			a.AddRequirement(eRequirementType.InventoryItem,ironkeychain,null,(eComparator)1);
			a.AddAction(eActionType.Talk,"Ah! This is great! My key! You have done a fabulous job in helping me. Please accept this coin in return for your time and effort. Thank you so much! Now, I can safely lock up the town's money. Thank you!",DwarvenGuardRinda);
			a.AddAction(eActionType.GiveXP,20,null);
			a.AddAction(eActionType.GiveGold,225,null);
			a.AddAction(eActionType.FinishQuest,typeof(DOL.GS.Quests.Midgard.rindaslostkey),null);
			a.AddAction(eActionType.TakeItem,rindaskey,null);
			a.AddAction(eActionType.GiveItem,silverringofhealth,DwarvenGuardRinda);
			AddBehaviour(a);
			
			#endregion

			// Custom Scriptloaded Code Begin
			
			// Custom Scriptloaded Code End
			if (DwarvenGuardRinda!=null) {
				DwarvenGuardRinda.AddQuestToGive(typeof (rindaslostkey));
			}
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{			
				
			// Custom Scriptunloaded Code Begin
			
			// Custom Scriptunloaded Code End

			

			/* If DwarvenGuardRinda has not been initialized, then we don't have to remove any
			 * hooks from him ;-)
			 */
			if (DwarvenGuardRinda == null)
				return;
			/* Now we remove the possibility to give this quest to players */			
			DwarvenGuardRinda.RemoveQuestToGive(typeof (rindaslostkey));
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
						return "[Step #1]  Search for a hobgoblin snake-finder around Haggerfel and see if it has Rinda's keys.";
				
					case 2:
						return "[Step #2]  Look for the black mauler cub that ate Rinda's keys. Slay the beast and retrieve the keys for Rinda.";
				
					case 3:
						return "[Step #3] Return the keys and keychain to Rinda in Haggerfel.";
				
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
			if (player.IsDoingQuest(typeof (rindaslostkey)) != null)
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
