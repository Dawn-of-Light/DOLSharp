	
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
	public class thebirthdaygift : BaseQuest
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

		protected const string questTitle = "The Birthday Gift";

		protected const int minimumLevel = 1;
		protected const int maximumLevel = 4;
	
	
		private static GameNPC BarkeepNognar = null;
		
		private static GameNPC BarkeepPrugar = null;
		
		private static ItemTemplate rattlingskeletonpendant = null;
		
		private static ItemTemplate giftandnoteforprugar = null;
		

		// Custom Initialization Code Begin
		
		// Custom Initialization Code End

		/* 
		* Constructor
		*/
		public thebirthdaygift() : base()
		{
		}

		public thebirthdaygift(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public thebirthdaygift(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public thebirthdaygift(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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
	
			npcs = WorldMgr.GetNPCsByName("Barkeep Nognar",(eRealm) 2);
			if (npcs.Length == 0)
			{
				if (!WorldMgr.GetRegion(100).IsDisabled)
				{
				BarkeepNognar = new DOL.GS.GameMerchant();
					BarkeepNognar.Model = 212;
				BarkeepNognar.Name = "Barkeep Nognar";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + BarkeepNognar.Name + ", creating ...");
				BarkeepNognar.GuildName = "Part of " + questTitle + " Quest";
				BarkeepNognar.Realm = eRealm.Midgard;
				BarkeepNognar.CurrentRegionID = 100;
				BarkeepNognar.Size = 58;
				BarkeepNognar.Level = 15;
				BarkeepNognar.MaxSpeedBase = 191;
				BarkeepNognar.Faction = FactionMgr.GetFactionByID(0);
				BarkeepNognar.X = 805429;
				BarkeepNognar.Y = 726478;
				BarkeepNognar.Z = 4717;
				BarkeepNognar.Heading = 4073;
				BarkeepNognar.RespawnInterval = -1;
				BarkeepNognar.BodyType = 0;
				

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 500;
				BarkeepNognar.SetOwnBrain(brain);
				
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					BarkeepNognar.SaveIntoDatabase();
					
				BarkeepNognar.AddToWorld();
				
				}
			}
			else 
			{
				BarkeepNognar = npcs[0];
			}
		
			npcs = WorldMgr.GetNPCsByName("Barkeep Prugar",(eRealm) 2);
			if (npcs.Length == 0)
			{
				if (!WorldMgr.GetRegion(101).IsDisabled)
				{
				BarkeepPrugar = new DOL.GS.GameMerchant();
					BarkeepPrugar.Model = 213;
				BarkeepPrugar.Name = "Barkeep Prugar";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + BarkeepPrugar.Name + ", creating ...");
				BarkeepPrugar.GuildName = "Part of " + questTitle + " Quest";
				BarkeepPrugar.Realm = eRealm.Midgard;
				BarkeepPrugar.CurrentRegionID = 101;
				BarkeepPrugar.Size = 60;
				BarkeepPrugar.Level = 15;
				BarkeepPrugar.MaxSpeedBase = 191;
				BarkeepPrugar.Faction = FactionMgr.GetFactionByID(0);
				BarkeepPrugar.X = 33230;
				BarkeepPrugar.Y = 34802;
				BarkeepPrugar.Z = 8027;
				BarkeepPrugar.Heading = 1194;
				BarkeepPrugar.RespawnInterval = -1;
				BarkeepPrugar.BodyType = 0;
				

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 500;
				BarkeepPrugar.SetOwnBrain(brain);
				
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					BarkeepPrugar.SaveIntoDatabase();
					
				BarkeepPrugar.AddToWorld();
				
				}
			}
			else 
			{
				BarkeepPrugar = npcs[0];
			}
		

			#endregion

			#region defineItems

		rattlingskeletonpendant = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "rattlingskeletonpendant");
			if (rattlingskeletonpendant == null)
			{
				rattlingskeletonpendant = new ItemTemplate();
				rattlingskeletonpendant.Name = "Rattling Skeleton Pendant";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + rattlingskeletonpendant.Name + ", creating it ...");
				rattlingskeletonpendant.Level = 50;
				rattlingskeletonpendant.Weight = 5;
				rattlingskeletonpendant.Model = 101;
				rattlingskeletonpendant.Object_Type = 0;
				rattlingskeletonpendant.Item_Type = 40;
				rattlingskeletonpendant.Id_nb = "rattlingskeletonpendant";
				rattlingskeletonpendant.Hand = 0;
				rattlingskeletonpendant.Platinum = 0;
				rattlingskeletonpendant.Gold = 0;
				rattlingskeletonpendant.Silver = 0;
				rattlingskeletonpendant.Copper = 0;
				rattlingskeletonpendant.IsPickable = true;
				rattlingskeletonpendant.IsDropable = true;
				rattlingskeletonpendant.IsTradable = true;
				rattlingskeletonpendant.CanDropAsLoot = false;
				rattlingskeletonpendant.Color = 0;
				rattlingskeletonpendant.Bonus = 35; // default bonus				
				rattlingskeletonpendant.Bonus1 = 0;
				rattlingskeletonpendant.Bonus1Type = (int) 0;
				rattlingskeletonpendant.Bonus2 = 0;
				rattlingskeletonpendant.Bonus2Type = (int) 0;
				rattlingskeletonpendant.Bonus3 = 0;
				rattlingskeletonpendant.Bonus3Type = (int) 0;
				rattlingskeletonpendant.Bonus4 = 0;
				rattlingskeletonpendant.Bonus4Type = (int) 0;
				rattlingskeletonpendant.Bonus5 = 0;
				rattlingskeletonpendant.Bonus5Type = (int) 0;
				rattlingskeletonpendant.Bonus6 = 0;
				rattlingskeletonpendant.Bonus6Type = (int) 0;
				rattlingskeletonpendant.Bonus7 = 0;
				rattlingskeletonpendant.Bonus7Type = (int) 0;
				rattlingskeletonpendant.Bonus8 = 0;
				rattlingskeletonpendant.Bonus8Type = (int) 0;
				rattlingskeletonpendant.Bonus9 = 0;
				rattlingskeletonpendant.Bonus9Type = (int) 0;
				rattlingskeletonpendant.Bonus10 = 0;
				rattlingskeletonpendant.Bonus10Type = (int) 0;
				rattlingskeletonpendant.ExtraBonus = 0;
				rattlingskeletonpendant.ExtraBonusType = (int) 0;
				rattlingskeletonpendant.Effect = 0;
				rattlingskeletonpendant.Emblem = 0;
				rattlingskeletonpendant.Charges = 0;
				rattlingskeletonpendant.MaxCharges = 0;
				rattlingskeletonpendant.SpellID = 0;
				rattlingskeletonpendant.ProcSpellID = 0;
				rattlingskeletonpendant.Type_Damage = 0;
				rattlingskeletonpendant.Realm = 0;
				rattlingskeletonpendant.MaxCount = 1;
				rattlingskeletonpendant.PackSize = 1;
				rattlingskeletonpendant.Extension = 0;
				rattlingskeletonpendant.Quality = 99;				
				rattlingskeletonpendant.Condition = 50000;
				rattlingskeletonpendant.MaxCondition = 50000;
				rattlingskeletonpendant.Durability = 50000;
				rattlingskeletonpendant.MaxDurability = 50000;
				rattlingskeletonpendant.PoisonCharges = 0;
				rattlingskeletonpendant.PoisonMaxCharges = 0;
				rattlingskeletonpendant.PoisonSpellID = 0;
				rattlingskeletonpendant.ProcSpellID1 = 0;
				rattlingskeletonpendant.SpellID1 = 0;
				rattlingskeletonpendant.MaxCharges1 = 0;
				rattlingskeletonpendant.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(rattlingskeletonpendant);
				}
			giftandnoteforprugar = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "giftandnoteforprugar");
			if (giftandnoteforprugar == null)
			{
				giftandnoteforprugar = new ItemTemplate();
				giftandnoteforprugar.Name = "Gift and Note for Prugar";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + giftandnoteforprugar.Name + ", creating it ...");
				giftandnoteforprugar.Level = 50;
				giftandnoteforprugar.Weight = 5;
				giftandnoteforprugar.Model = 1347;
				giftandnoteforprugar.Object_Type = 0;
				giftandnoteforprugar.Item_Type = 40;
				giftandnoteforprugar.Id_nb = "giftandnoteforprugar";
				giftandnoteforprugar.Hand = 0;
				giftandnoteforprugar.Platinum = 0;
				giftandnoteforprugar.Gold = 0;
				giftandnoteforprugar.Silver = 0;
				giftandnoteforprugar.Copper = 0;
				giftandnoteforprugar.IsPickable = true;
				giftandnoteforprugar.IsDropable = true;
				giftandnoteforprugar.IsTradable = true;
				giftandnoteforprugar.CanDropAsLoot = false;
				giftandnoteforprugar.Color = 0;
				giftandnoteforprugar.Bonus = 35; // default bonus				
				giftandnoteforprugar.Bonus1 = 0;
				giftandnoteforprugar.Bonus1Type = (int) 0;
				giftandnoteforprugar.Bonus2 = 0;
				giftandnoteforprugar.Bonus2Type = (int) 0;
				giftandnoteforprugar.Bonus3 = 0;
				giftandnoteforprugar.Bonus3Type = (int) 0;
				giftandnoteforprugar.Bonus4 = 0;
				giftandnoteforprugar.Bonus4Type = (int) 0;
				giftandnoteforprugar.Bonus5 = 0;
				giftandnoteforprugar.Bonus5Type = (int) 0;
				giftandnoteforprugar.Bonus6 = 0;
				giftandnoteforprugar.Bonus6Type = (int) 0;
				giftandnoteforprugar.Bonus7 = 0;
				giftandnoteforprugar.Bonus7Type = (int) 0;
				giftandnoteforprugar.Bonus8 = 0;
				giftandnoteforprugar.Bonus8Type = (int) 0;
				giftandnoteforprugar.Bonus9 = 0;
				giftandnoteforprugar.Bonus9Type = (int) 0;
				giftandnoteforprugar.Bonus10 = 0;
				giftandnoteforprugar.Bonus10Type = (int) 0;
				giftandnoteforprugar.ExtraBonus = 0;
				giftandnoteforprugar.ExtraBonusType = (int) 0;
				giftandnoteforprugar.Effect = 0;
				giftandnoteforprugar.Emblem = 0;
				giftandnoteforprugar.Charges = 0;
				giftandnoteforprugar.MaxCharges = 0;
				giftandnoteforprugar.SpellID = 0;
				giftandnoteforprugar.ProcSpellID = 0;
				giftandnoteforprugar.Type_Damage = 0;
				giftandnoteforprugar.Realm = 0;
				giftandnoteforprugar.MaxCount = 1;
				giftandnoteforprugar.PackSize = 1;
				giftandnoteforprugar.Extension = 0;
				giftandnoteforprugar.Quality = 99;				
				giftandnoteforprugar.Condition = 50000;
				giftandnoteforprugar.MaxCondition = 50000;
				giftandnoteforprugar.Durability = 50000;
				giftandnoteforprugar.MaxDurability = 50000;
				giftandnoteforprugar.PoisonCharges = 0;
				giftandnoteforprugar.PoisonMaxCharges = 0;
				giftandnoteforprugar.PoisonSpellID = 0;
				giftandnoteforprugar.ProcSpellID1 = 0;
				giftandnoteforprugar.SpellID1 = 0;
				giftandnoteforprugar.MaxCharges1 = 0;
				giftandnoteforprugar.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(giftandnoteforprugar);
				}
			

			#endregion

			#region defineAreas
			
		#endregion
		
		#region defineQuestParts

		QuestBuilder builder = QuestMgr.getBuilder(typeof(thebirthdaygift));
			QuestBehaviour a;
			a = builder.CreateBehaviour(BarkeepNognar,-1);
				a.AddTrigger(eTriggerType.Interact,null,BarkeepNognar);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.thebirthdaygift),BarkeepNognar);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.thebirthdaygift),null,(eComparator)5);
			a.AddAction(eActionType.Talk,"Well hello there. Please come have a drink and talk. I have a small favor to ask. That?s if you are [interested].",BarkeepNognar);
			AddBehaviour(a);
			a = builder.CreateBehaviour(BarkeepNognar,-1);
				a.AddTrigger(eTriggerType.Whisper,"interested",BarkeepNognar);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.thebirthdaygift),BarkeepNognar);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.thebirthdaygift),null,(eComparator)5);
			a.AddAction(eActionType.Talk,"This bar, as well as one in Jordheim, has been passed down from generation to generation. They began with my great grandfather and have been passed down until they have reached my brother and me. We are honored to serve the people of Mularn and Jordheim for so many years. The job is extremely tiring and demanding and time off is a treasured experience. My apprentice will not be returning until the day after tomorrow and that will be [too late].",BarkeepNognar);
			AddBehaviour(a);
			a = builder.CreateBehaviour(BarkeepNognar,-1);
				a.AddTrigger(eTriggerType.Whisper,"too late",BarkeepNognar);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.thebirthdaygift),BarkeepNognar);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.thebirthdaygift),null,(eComparator)5);
			a.AddAction(eActionType.Talk,"See, my younger brother?s birthday is tomorrow and I want to get him something special. I want to get him a gift that will remind him of our childhood. That was a time when we where not so caught up in work and the [happenings] around us.",BarkeepNognar);
			AddBehaviour(a);
			a = builder.CreateBehaviour(BarkeepNognar,-1);
				a.AddTrigger(eTriggerType.Whisper,"happenings",BarkeepNognar);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.thebirthdaygift),BarkeepNognar);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.thebirthdaygift),null,(eComparator)5);
			a.AddAction(eActionType.Talk,"As children we used to play in the field beside this bar and tease the rattling skeletons to no end. The skeletons wore pendants which, as children, we thought were magical. We would try our hardest to snatch one from around their neck but never had the courage to do it. The pendants have no magical powers and are just common adornments of the skeletons. We [discovered] this when we where a bit older.",BarkeepNognar);
			AddBehaviour(a);
			a = builder.CreateBehaviour(BarkeepNognar,-1);
				a.AddTrigger(eTriggerType.Whisper,"discovered",BarkeepNognar);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.thebirthdaygift),BarkeepNognar);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.thebirthdaygift),null,(eComparator)5);
			a.AddAction(eActionType.Talk,"I wish to get one of those pendants for Prugar for his birthday. I think it would be a fun way to remind him of how close we used to be and how much fun we had as kids. I also want to remind him not to be so serious all of the time. ",BarkeepNognar);
			a.AddAction(eActionType.OfferQuest,typeof(DOL.GS.Quests.Midgard.thebirthdaygift),"Will you help get Prugar's birthday gift? (Levels 1-4)");
			AddBehaviour(a);
			a = builder.CreateBehaviour(BarkeepNognar,-1);
				a.AddTrigger(eTriggerType.DeclineQuest,null,typeof(DOL.GS.Quests.Midgard.thebirthdaygift));
			a.AddAction(eActionType.Talk,"No problem. See you.",BarkeepNognar);
			AddBehaviour(a);
			a = builder.CreateBehaviour(BarkeepNognar,-1);
				a.AddTrigger(eTriggerType.AcceptQuest,null,typeof(DOL.GS.Quests.Midgard.thebirthdaygift));
			a.AddAction(eActionType.GiveQuest,typeof(DOL.GS.Quests.Midgard.thebirthdaygift),BarkeepNognar);
			a.AddAction(eActionType.Talk,"Thank you. This means the world to me and I greatly [appreciate] your help.",BarkeepNognar);
			AddBehaviour(a);
			a = builder.CreateBehaviour(BarkeepNognar,-1);
				a.AddTrigger(eTriggerType.Whisper,"appreciate",BarkeepNognar);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.thebirthdaygift),1,(eComparator)3);
			a.AddAction(eActionType.Talk,"All you need to do is go to the northwest side of the bar and kill a rattling skeleton. Once you get a pendant return to me with it.",BarkeepNognar);
			AddBehaviour(a);
			a = builder.CreateBehaviour(BarkeepNognar,-1);
				a.AddTrigger(eTriggerType.EnemyKilled,"rattling skeleton",null);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.thebirthdaygift),1,(eComparator)3);
			a.AddAction(eActionType.GiveItem,rattlingskeletonpendant,null);
			a.AddAction(eActionType.IncQuestStep,typeof(DOL.GS.Quests.Midgard.thebirthdaygift),null);
			AddBehaviour(a);
			a = builder.CreateBehaviour(BarkeepNognar,-1);
				a.AddTrigger(eTriggerType.Interact,null,BarkeepNognar);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.thebirthdaygift),2,(eComparator)3);
			a.AddAction(eActionType.Talk,"You have returned safely. Were you able to get a pendant from one of the skeletons? ",BarkeepNognar);
			AddBehaviour(a);
			a = builder.CreateBehaviour(BarkeepNognar,-1);
				a.AddTrigger(eTriggerType.GiveItem,BarkeepNognar,rattlingskeletonpendant);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.thebirthdaygift),2,(eComparator)3);
			a.AddAction(eActionType.Talk,"This is perfect! I hope Prugar finds as much humor and meaning in this gift as I do. If you give me one moment I will [wrap this gift] and compose a letter to be delivered along with it.",BarkeepNognar);
			a.AddAction(eActionType.TakeItem,rattlingskeletonpendant,null);
			AddBehaviour(a);
			a = builder.CreateBehaviour(BarkeepNognar,-1);
				a.AddTrigger(eTriggerType.Whisper,"wrap this gift",BarkeepNognar);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.thebirthdaygift),2,(eComparator)3);
			a.AddRequirement(eRequirementType.InventoryItem,rattlingskeletonpendant,null,(eComparator)1);
			a.AddAction(eActionType.Talk,"Here you go. If you would please deliver this to Prugar, that would be great. He can be found in Jordheim at the bar. As you enter Jordheim?s gates stay to the right. When you come to the next fork stay left. The bar is the long building on your right. The bar's door is on the end with a scrolled sign hanging above it. Thanks again. ",BarkeepNognar);
			a.AddAction(eActionType.IncQuestStep,typeof(DOL.GS.Quests.Midgard.thebirthdaygift),null);
			a.AddAction(eActionType.GiveItem,giftandnoteforprugar,BarkeepNognar);
			AddBehaviour(a);
			a = builder.CreateBehaviour(BarkeepPrugar,-1);
				a.AddTrigger(eTriggerType.Interact,null,BarkeepPrugar);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.thebirthdaygift),3,(eComparator)3);
			a.AddAction(eActionType.Talk,"Hello stranger! You have a gift for me? Who sent it?",BarkeepPrugar);
			AddBehaviour(a);
			a = builder.CreateBehaviour(BarkeepPrugar,-1);
				a.AddTrigger(eTriggerType.GiveItem,BarkeepPrugar,giftandnoteforprugar);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.thebirthdaygift),3,(eComparator)3);
			a.AddAction(eActionType.Talk,"My dear brother remembered my birthday. Thank you so much. This is such a wonderful [surprise].",BarkeepPrugar);
			a.AddAction(eActionType.TakeItem,giftandnoteforprugar,null);
			AddBehaviour(a);
			a = builder.CreateBehaviour(BarkeepPrugar,-1);
				a.AddTrigger(eTriggerType.Whisper,"surprise",BarkeepPrugar);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.thebirthdaygift),3,(eComparator)3);
			a.AddRequirement(eRequirementType.InventoryItem,giftandnoteforprugar,null,(eComparator)1);
			a.AddAction(eActionType.Talk,"Please take this coin as a token of my appreciation. If you are interested, I have another task for you.",BarkeepPrugar);
			a.AddAction(eActionType.GiveXP,20,null);
			a.AddAction(eActionType.GiveGold,27,null);
			a.AddAction(eActionType.FinishQuest,typeof(DOL.GS.Quests.Midgard.thebirthdaygift),null);
			AddBehaviour(a);
			
			#endregion

			// Custom Scriptloaded Code Begin
			
			// Custom Scriptloaded Code End
			if (BarkeepNognar!=null) {
				BarkeepNognar.AddQuestToGive(typeof (thebirthdaygift));
			}
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{			
				
			// Custom Scriptunloaded Code Begin
			
			// Custom Scriptunloaded Code End

			

			/* If BarkeepNognar has not been initialized, then we don't have to remove any
			 * hooks from him ;-)
			 */
			if (BarkeepNognar == null)
				return;
			/* Now we remove the possibility to give this quest to players */			
			BarkeepNognar.RemoveQuestToGive(typeof (thebirthdaygift));
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
						return "[Step #1] Speak with Barkeep Nognar about his brother's gift. When he is done speaking make your way to the field northwest of the bar in Mularn and kill a rattling skeleton. Once you obtain the pendant return to Barkeep Nognar.";
				
					case 2:
						return "[Step #2] Once the rattling pendant has been obtained return to Barkeep Nognar in Mularn.";
				
					case 3:
						return "[Step #3] As you enter Jordheim stay to the right. When you come to the next fork stay left. The bar is on your right and the door is on the end with a scrolled sign above it. Speak with Barkeep Prugar, when asked hand him his gift.";
				
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
			if (player.IsDoingQuest(typeof (thebirthdaygift)) != null)
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
