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
 * Author:		Gandulf Kohlweiss
 * Date:			
 * Directory: /scripts/quests/albion/
 *
 * Description:
 *  Brief Walkthrough: 
 * 1) Travel to loc=19105,26552 Camelot Hills to speak with Master Frederick. 
 * 2) Go to Dragonfly Handler Colm, at loc=14040,29170 Camelot Hills (Cotswold), and purchase a dragonfly ticket to Avalon Marsh. 
 * 3) Once in Avalon Marsh look for Master Dunwyn, he tends to roam a bit from the guard tower and the Shrouded Isles portal. , I found him at loc=47591,20373,1840 dir=187 Avalon Marsh. Hand him the Scroll for Master Dunwyn from Master Frederick. 
 * 4) /use the List for Master Dunwyn from Master Dunwyn 
 * 5) Kill a swamp slime. 
 * 6) /use the List for Master Dunwyn from Master Dunwyn 
 * 7) Kill a river spriteling. 
 * 8) /use the List for Master Dunwyn from Master Dunwyn 
 * 9) Kill a swamp rat. 
 * 10) Go to Master Dunwyn at the Shrouded Isles portal and hand him the items he asks for that you have collected. 
 * 11) Master Dunwyn will teleport you back to Master Frederick. Talk with Master Frederick. 
 * 12) Right click on Master Dunwyn and the click on Master Frederick for instructions. 
 * 13) Go to loc=30425,24872 Camelot Hills with Master Dunwyn in tow. When you get there you will see a swarm of Ire Fairy Sorceress's there with Princess Obera. Focus on killing Princess Obera, blue con to a lvl 4, Master Dunwyn will deal with the Ire Fairy Sorceress's. Once Princess Obera is dead you can help Master Dunwyn finish killing the other Ire Fairies. 
 * 14) Talk with Master Dunwy after the fight. 
 * 15) Go back to Master Frederick and type /whisper returned to the Marsh. 
 * 16) Hand Master Frederick Princess Obera's Head to collect your reward.
 */

using System;
using System.Collections;
using System.Reflection;
using DOL.AI.Brain;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;
/* I suggest you declare yourself some namespaces for your quests
 * Like: DOL.GS.Quests.Albion
 *       DOL.GS.Quests.Midgard
 *       DOL.GS.Quests.Hibernia
 * Also this is the name that will show up in the database as QuestName
 * so setting good values here will result in easier to read and cleaner
 * Database Code
 */

namespace DOL.GS.Quests.Albion
{
	/* The first thing we do, is to declare the class we create
	 * as Quest. To do this, we derive from the abstract class
	 * AbstractQuest
	 * 	 
	 */

	public class BeginningOfWar : BaseFrederickQuest
	{
		/// <summary>
		/// Defines a logger for this class.
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

		protected static GameLocation locationFrederick = new GameLocation("Master Frederick", 1, 0, 19105 + 50, 26552 + 40, 2861, 0);

		protected const string questTitle = "Beginning of War";
		protected const int minimumLevel = 4;
		protected const int maximumLevel = 4;

		private static GameNPC masterFrederick = null;

		private static GameLocation locationDunwynClone = new GameLocation(null, 1, 567604, 509619, 2813, 3292);

		private static GameNPC dunwyn = null;
		private GameNPC dunwynClone = null;

		private static GameNPC princessObera = null;
		private static GameNPC[] fairySorceress = new GameNPC[4];

		private bool princessOberaAttackStarted = false;

		private static ItemTemplate swampSlimeItem = null;
		private static ItemTemplate swampRatTail = null;
		private static ItemTemplate princessOberasHead = null;
		private static ItemTemplate riverSpritlingClaw = null;

		private static ItemTemplate scrollDunwyn = null;
		private static ItemTemplate listDunwyn = null;
		private static ItemTemplate recruitsHelm = null;
		private static ItemTemplate recruitsCap = null;
		private static ItemTemplate recruitsRing = null;


		/* We need to define the constructors from the base class here, else there might be problems
		 * when loading this quest...
		 */
		public BeginningOfWar() : base()
		{
		}

		public BeginningOfWar(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public BeginningOfWar(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public BeginningOfWar(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
		{
		}


		/* The following method is called automatically when this quest class
		 * is loaded. You might notice that this method is the same as in standard
		 * game events. And yes, quests basically are game events for single players
		 * 
		 * To make this method automatically load, we have to declare it static
		 * and give it the [ScriptLoadedEvent] attribute. 
		 *
		 * Inside this method we initialize the quest. This is neccessary if we 
		 * want to set the quest hooks to the NPCs.
		 * 
		 * If you want, you can however add a quest to the player from ANY place
		 * inside your code, from events, from custom items, from anywhere you
		 * want. 
		 */

		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			if (!ServerProperties.Properties.LOAD_QUESTS)
				return;
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initializing ...");
			/* First thing we do in here is to search for the NPCs inside
			* the world who comes from the certain Realm. If we find a the players,
			* this means we don't have to create a new one.
			* 
			* NOTE: You can do anything you want in this method, you don't have
			* to search for NPC's ... you could create a custom item, place it
			* on the ground and if a player picks it up, he will get the quest!
			* Just examples, do anything you like and feel comfortable with :)
			*/

			#region defineNPCs

			masterFrederick = GetMasterFrederick();

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Master Dunwyn", eRealm.Albion);
			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Master Dunwyn, creating ...");
				dunwyn = new GameNPC();
				dunwyn.Model = 9;
				dunwyn.Name = "Master Dunwyn";
				dunwyn.GuildName = "Part of " + questTitle + " Quest";
				dunwyn.Realm = (byte) eRealm.Albion;
				dunwyn.CurrentRegionID = 1;

				dunwyn.Size = 50;
				dunwyn.Level = 20;
				dunwyn.X = 465383;
				dunwyn.Y = 634773;
				dunwyn.Z = 1840;
				dunwyn.Heading = 187;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 798);
				template.AddNPCEquipment(eInventorySlot.RightHandWeapon, 19);
				dunwyn.Inventory = template.CloseTemplate();

//				dunwyn.AddNPCEquipment((byte) eEquipmentItems.TORSO, 798, 0, 0, 0);
//				dunwyn.AddNPCEquipment((byte) eEquipmentItems.RIGHT_HAND, 19, 0, 0, 0);

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					dunwyn.SaveIntoDatabase();

				dunwyn.AddToWorld();
			}
			else
				dunwyn = npcs[0];


			npcs = WorldMgr.GetNPCsByName("Princess Obera", eRealm.None);
			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Princess Obera, creating ...");
				princessObera = new GameNPC();

				princessObera.Name = "Princess Obera";
				princessObera.X = 579289;
				princessObera.Y = 508200;
				princessObera.Z = 2779;
				princessObera.Heading = 347;
				princessObera.Model = 603;
				princessObera.GuildName = "Part of " + questTitle + " Quest";
				princessObera.Realm = (byte) eRealm.None;
				princessObera.CurrentRegionID = 1;
				princessObera.Size = 49;
				princessObera.Level = 3;

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 80;
				brain.AggroRange = 1000;
				princessObera.SetOwnBrain(brain);

				if (SAVE_INTO_DATABASE)
					princessObera.SaveIntoDatabase();
				princessObera.AddToWorld();
			}
			else
			{
				princessObera = (GameNPC) npcs[0];
			}

			int counter = 0;
			foreach (GameNPC npc in princessObera.GetNPCsInRadius(500))
			{
				if (npc.Name == "ire fairy sorceress")
				{
					fairySorceress[counter] = (GameNPC) npc;
					counter++;
				}
				if (counter == fairySorceress.Length)
					break;
			}

			for (int i = 0; i < fairySorceress.Length; i++)
			{
				if (fairySorceress[i] == null)
				{
					if (log.IsWarnEnabled)
						log.Warn("Could not find ire fairy sorceress, creating ...");
					fairySorceress[i] = new GameNPC();
					fairySorceress[i].Model = 603; // //819;
					fairySorceress[i].Name = "ire fairy sorceress";
					fairySorceress[i].GuildName = "Part of " + questTitle + " Quest";
					fairySorceress[i].Realm = (byte) eRealm.None;
					fairySorceress[i].CurrentRegionID = 1;
					fairySorceress[i].Size = 35;
					fairySorceress[i].Level = 3;
					fairySorceress[i].X = princessObera.X + Util.Random(-150, 150);
					fairySorceress[i].Y = princessObera.Y + Util.Random(-150, 150);
					fairySorceress[i].Z = princessObera.Z;

					StandardMobBrain brain = new StandardMobBrain();
					brain.AggroLevel = 30;
					brain.AggroRange = 300;
					fairySorceress[i].SetOwnBrain(brain);

					fairySorceress[i].Heading = 93;
					//fairySorceress[i].EquipmentTemplateID = 200276;                

					//You don't have to store the created mob in the db if you don't want,
					//it will be recreated each time it is not found, just comment the following
					//line if you rather not modify your database
					if (SAVE_INTO_DATABASE)
						fairySorceress[i].SaveIntoDatabase();
					fairySorceress[i].AddToWorld();
				}
			}

			#endregion

			#region defineItems

			swampRatTail = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "swamp_rat_tail");
			if (swampRatTail == null)
			{
				swampRatTail = new ItemTemplate();
				swampRatTail.Name = "Swamp Rat Tail";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + swampRatTail.Name + " , creating it ...");

				swampRatTail.Weight = 110;
				swampRatTail.Model = 515;

				swampRatTail.Object_Type = (int) eObjectType.GenericItem;

				swampRatTail.Id_nb = "swamp_rat_tail";
				swampRatTail.IsPickable = true;
				swampRatTail.IsDropable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(swampRatTail);
			}

			swampSlimeItem = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "swamp_slime");
			if (swampSlimeItem == null)
			{
				swampSlimeItem = new ItemTemplate();
				swampSlimeItem.Name = "Swamp Slime";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + swampSlimeItem.Name + " , creating it ...");

				swampSlimeItem.Weight = 10;
				swampSlimeItem.Model = 553;

				swampSlimeItem.Object_Type = (int) eObjectType.GenericItem;

				swampSlimeItem.Id_nb = "swamp_slime";
				swampSlimeItem.IsPickable = true;
				swampSlimeItem.IsDropable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(swampSlimeItem);
			}

			scrollDunwyn = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "scroll_for_dunwyn");
			if (scrollDunwyn == null)
			{
				scrollDunwyn = new ItemTemplate();
				scrollDunwyn.Name = "Scroll for Master Dunwyn";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + scrollDunwyn.Name + " , creating it ...");

				scrollDunwyn.Weight = 10;
				scrollDunwyn.Model = 498;

				scrollDunwyn.Object_Type = (int) eObjectType.GenericItem;

				scrollDunwyn.Id_nb = "scroll_for_dunwyn";
				scrollDunwyn.IsPickable = true;
				scrollDunwyn.IsDropable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(scrollDunwyn);
			}

			listDunwyn = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "list_for_dunwyn");
			if (listDunwyn == null)
			{
				listDunwyn = new ItemTemplate();
				listDunwyn.Name = "List for Master Dunwyn";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + listDunwyn.Name + " , creating it ...");

				listDunwyn.Weight = 10;
				listDunwyn.Model = 498;

				listDunwyn.Object_Type = (int) eObjectType.GenericItem;

				listDunwyn.Id_nb = "list_for_dunwyn";
				listDunwyn.IsPickable = true;
				listDunwyn.IsDropable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(listDunwyn);
			}

			riverSpritlingClaw = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "river_spritling_claw");
			if (riverSpritlingClaw == null)
			{
				riverSpritlingClaw = new ItemTemplate();
				riverSpritlingClaw.Name = "River Spriteling Claw";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + riverSpritlingClaw.Name + " , creating it ...");

				riverSpritlingClaw.Weight = 2;
				riverSpritlingClaw.Model = 106;

				riverSpritlingClaw.Object_Type = (int) eObjectType.GenericItem;

				riverSpritlingClaw.Id_nb = "river_spritling_claw";
				riverSpritlingClaw.IsPickable = true;
				riverSpritlingClaw.IsDropable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(riverSpritlingClaw);
			}

			princessOberasHead = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "princess_oberas_head");
			if (princessOberasHead == null)
			{
				princessOberasHead = new ItemTemplate();
				princessOberasHead.Name = "Princess Obera's Head";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + princessOberasHead.Name + " , creating it ...");

				princessOberasHead.Weight = 15;
				princessOberasHead.Model = 503;

				princessOberasHead.Object_Type = (int) eObjectType.GenericItem;

				princessOberasHead.Id_nb = "princess_oberas_head";
				princessOberasHead.IsPickable = true;
				princessOberasHead.IsDropable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(princessOberasHead);
			}

			// item db check
			recruitsHelm = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "recruits_studded_helm");
			if (recruitsHelm == null)
			{
				recruitsHelm = new ItemTemplate();
				recruitsHelm.Name = "Recruit's Studded Helm";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsHelm.Name + ", creating it ...");
				recruitsHelm.Level = 8;

				recruitsHelm.Weight = 24;
				recruitsHelm.Model = 824; // studded vest

				recruitsHelm.DPS_AF = 12; // Armour
				recruitsHelm.SPD_ABS = 19; // Absorption

				recruitsHelm.Object_Type = (int) eObjectType.Studded;
				recruitsHelm.Item_Type = (int) eEquipmentItems.HEAD;
				recruitsHelm.Id_nb = "recruits_studded_helm";
				recruitsHelm.Gold = 0;
				recruitsHelm.Silver = 9;
				recruitsHelm.Copper = 0;
				recruitsHelm.IsPickable = true;
				recruitsHelm.IsDropable = true;
				recruitsHelm.Color = 9; // red leather

				recruitsHelm.Bonus = 5; // default bonus

				recruitsHelm.Bonus1 = 4;
				recruitsHelm.Bonus1Type = (int) eStat.DEX;

				recruitsHelm.Bonus2 = 1;
				recruitsHelm.Bonus2Type = (int) eResist.Spirit;

				recruitsHelm.Bonus3 = 12;
				recruitsHelm.Bonus3Type = (int) eProperty.MaxHealth;

				recruitsHelm.Quality = 100;
				recruitsHelm.Condition = 1000;
				recruitsHelm.MaxCondition = 1000;
				recruitsHelm.Durability = 1000;
				recruitsHelm.MaxDurability = 1000;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsHelm);
			}

			// item db check
			recruitsCap = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "recruits_quilted_cap");
			if (recruitsCap == null)
			{
				recruitsCap = new ItemTemplate();
				recruitsCap.Name = "Recruit's Quilted Cap";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsCap.Name + ", creating it ...");
				recruitsCap.Level = 8;

				recruitsCap.Weight = 8;
				recruitsCap.Model = 822; // studded vest

				recruitsCap.DPS_AF = 6; // Armour
				recruitsCap.SPD_ABS = 0; // Absorption

				recruitsCap.Object_Type = (int) eObjectType.Cloth;
				recruitsCap.Item_Type = (int) eEquipmentItems.HEAD;
				recruitsCap.Id_nb = "recruits_quilted_cap";
				recruitsCap.Gold = 0;
				recruitsCap.Silver = 9;
				recruitsCap.Copper = 0;
				recruitsCap.IsPickable = true;
				recruitsCap.IsDropable = true;
				recruitsCap.Color = 27; // red cloth

				recruitsCap.Bonus = 5; // default bonus

				recruitsCap.Bonus1 = 4;
				recruitsCap.Bonus1Type = (int) eStat.DEX;

				recruitsCap.Bonus2 = 20;
				recruitsCap.Bonus2Type = (int) eProperty.MaxHealth;

				recruitsCap.Bonus3 = 1;
				recruitsCap.Bonus3Type = (int) eResist.Spirit;

				recruitsCap.Quality = 100;
				recruitsCap.Condition = 1000;
				recruitsCap.MaxCondition = 1000;
				recruitsCap.Durability = 1000;
				recruitsCap.MaxDurability = 1000;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsCap);
			}

			recruitsRing = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "recruits_silver_ring");
			if (recruitsRing == null)
			{
				recruitsRing = new ItemTemplate();
				recruitsRing.Name = "Recruit's Silver Ring";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsRing.Name + ", creating it ...");
				recruitsRing.Level = 6;

				recruitsRing.Weight = 2;
				recruitsRing.Model = 103; // studded vest                

				recruitsRing.Object_Type = (int) eObjectType.Magical;
				recruitsRing.Item_Type = (int) eEquipmentItems.R_RING;
				recruitsRing.Id_nb = "recruits_silver_ring";
				recruitsRing.Gold = 0;
				recruitsRing.Silver = 9;
				recruitsRing.Copper = 0;
				recruitsRing.IsPickable = true;
				recruitsRing.IsDropable = true;

				recruitsRing.Bonus = 5; // default bonus

				recruitsRing.Bonus1 = 20;
				recruitsRing.Bonus1Type = (int) eProperty.MaxHealth;

				recruitsRing.Quality = 100;
				recruitsRing.Condition = 1000;
				recruitsRing.MaxCondition = 1000;
				recruitsRing.Durability = 1000;
				recruitsRing.MaxDurability = 1000;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsRing);
			}

			#endregion

			/* Now we add some hooks to the npc we found.
			* Actually, we want to know when a player interacts with him.
			* So, we hook the right-click (interact) and the whisper method
			* of npc and set the callback method to the "TalkToXXX"
			* method. This means, the "TalkToXXX" method is called whenever
			* a player right clicks on him or when he whispers to him.
			*/

			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			//We want to be notified whenever a player enters the world  
			GameEventMgr.AddHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));

			GameEventMgr.AddHandler(masterFrederick, GameLivingEvent.Interact, new DOLEventHandler(TalkToMasterFrederick));
			GameEventMgr.AddHandler(masterFrederick, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterFrederick));

			GameEventMgr.AddHandler(dunwyn, GameLivingEvent.Interact, new DOLEventHandler(TalkToMasterDunwyn));
			GameEventMgr.AddHandler(dunwyn, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterDunwyn));

			GameEventMgr.AddHandler(princessObera, GameNPCEvent.OnAICallback, new DOLEventHandler(CheckNearPrincessObera));

			/* Now we bring to masterFrederick the possibility to give this quest to players */
			masterFrederick.AddQuestToGive(typeof (BeginningOfWar));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");

		}

		/* The following method is called automatically when this quest class
		 * is unloaded. 
		 * 
		 * Since we set hooks in the load method, it is good practice to remove
		 * those hooks again!
		 */

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			/* If sirQuait has not been initialized, then we don't have to remove any
			 * hooks from him ;-)
			 */
			if (masterFrederick == null)
				return;

			/* Removing hooks works just as adding them but instead of 
			 * AddHandler, we call RemoveHandler, the parameters stay the same
			 */

			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));

			GameEventMgr.RemoveHandler(masterFrederick, GameLivingEvent.Interact, new DOLEventHandler(TalkToMasterFrederick));
			GameEventMgr.RemoveHandler(masterFrederick, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterFrederick));

			GameEventMgr.RemoveHandler(dunwyn, GameLivingEvent.Interact, new DOLEventHandler(TalkToMasterDunwyn));
			GameEventMgr.RemoveHandler(dunwyn, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterDunwyn));

			GameEventMgr.RemoveHandler(princessObera, GameNPCEvent.OnAICallback, new DOLEventHandler(CheckNearPrincessObera));
	
			/* Now we remove to masterFrederick the possibility to give this quest to players */
			masterFrederick.RemoveQuestToGive(typeof (BeginningOfWar));
		}

		protected static void CheckNearPrincessObera(DOLEvent e, object sender, EventArgs args)
		{
			GameNPC princessObera = (GameNPC) sender;

			// if princess is dead no ned to checks ...
			if (!princessObera.IsAlive || princessObera.ObjectState != GameObject.eObjectState.Active)
				return;

			foreach (GamePlayer player in princessObera.GetPlayersInRadius(1000))
			{
				BeginningOfWar quest = (BeginningOfWar) player.IsDoingQuest(typeof (BeginningOfWar));

				if (quest != null && !quest.princessOberaAttackStarted && quest.Step == 16)
				{
					quest.princessOberaAttackStarted = true;

					if (quest.dunwynClone != null)
					{
						foreach (GamePlayer visPlayer in quest.dunwynClone.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
						{
							visPlayer.Out.SendSpellCastAnimation(quest.dunwynClone, 1, 20);
						}

						RegionTimer castTimer = new RegionTimer(quest.dunwynClone, new RegionTimerCallback(quest.CastDunwynClone), 2000);
					}
					SendSystemMessage(player, "There they are. You take care of the princess I'll deal with the fairy sorcesses littleone.");

					IAggressiveBrain aggroBrain = princessObera.Brain as IAggressiveBrain;
					if (aggroBrain != null)
						aggroBrain.AddToAggroList(player, 50 + Util.Random(20));

					if (quest.dunwynClone != null)
					{
						foreach (GameNPC fairy in fairySorceress)
						{
							aggroBrain = quest.dunwynClone.Brain as IAggressiveBrain;
							if (aggroBrain != null)
								aggroBrain.AddToAggroList(fairy, Util.Random(50));
							aggroBrain = fairy.Brain as IAggressiveBrain;
							if (aggroBrain != null)
								aggroBrain.AddToAggroList(quest.dunwynClone, Util.Random(50));
						}
					}
				}
			}
		}

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToMasterFrederick(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies
			//for the quest!		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(masterFrederick.CanGiveQuest(typeof (BeginningOfWar), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			BeginningOfWar quest = player.IsDoingQuest(typeof (BeginningOfWar)) as BeginningOfWar;

			masterFrederick.TurnTo(player);

			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...
					masterFrederick.SayTo(player, "Recruit, we need to take care of these fairies. I have heard from passers-by that they are trying to amass an army to defeat Cotswold! Can you [believe] that?");
					return;
				}
				else
				{
					if (quest.Step == 1)
					{
						masterFrederick.SayTo(player, "There is an old wizard I know by the name of Dunwyn. Well, Master Dunwyn, to show respect. He patrols a small section of road in the Avalon Marsh. Take this note to Master Dunwyn explaining our situation. I'm sure he would be interested in one more fight for the good of Albion.");
						GiveItem(masterFrederick, player, scrollDunwyn);
						quest.Step = 2;
					}
					else if (quest.Step == 13)
					{
						masterFrederick.SayTo(player, "Welcome back recruit. I take it you found Master Dunwyn? What did he say? Will he be [assisting] us in our fairy problem?");

					}
					else if (quest.Step == 15)
					{
						masterFrederick.SayTo(player, "Good job recruit. Now listen. Since Master Dunwyn has graciously agreed to help us with this fairy problem, let's [detail] what we're going to do.");
					}
					else if (quest.Step == 18)
					{
						masterFrederick.SayTo(player, "Ah, my favorite recruit, back from battle. Where is Master Dunwyn? Has he already [returned to the Marsh]?");

					}
					else if (quest.Step == 19)
					{
						masterFrederick.SayTo(player, "Yes, for you my young recruit, please take this helm and this ring. I know these magical items will help you as you make your way through this life. Be safe now Vinde. We will speak again very soon.");
						quest.FinishQuest();
					}
					return;
				}
			}
				// The player whispered to NPC (clicked on the text inside the [])
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				if (quest == null)
				{
					//Do some small talk :)
					switch (wArgs.Text)
					{
						case "believe":
							masterFrederick.SayTo(player, "I know, it's hard to take in. But listen Vinde, we must strike hard at the fairies. We must destroy this supposed army before they can come and wreck Cotswold. What do you say recruit? Are you [in] or not?");
							break;

							//If the player offered his "help", we send the quest dialog now!
						case "in":
							player.Out.SendQuestSubscribeCommand(masterFrederick, QuestMgr.GetIDForQuestType(typeof(BeginningOfWar)), "Will you take the letter to Master Dunwyn in Avalon Marsh for help with the fairy problem?");
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "solution":
							masterFrederick.SayTo(player, "There is an old wizard I know by the name of Dunwyn. Well, Master Dunwyn, to show respect. He patrols a small section of road in the Avalon Marsh at the guard tower near the big portal. Take this note to Master Dunwyn explaining our situation. I'm sure he would be interested in one more fight for the good of Albion.");
							if (quest.Step == 1)
							{
								GiveItem(masterFrederick, player, scrollDunwyn);
								quest.Step = 2;
							}
							break;
						case "assisting":
							masterFrederick.SayTo(player, "Excellent! I knew he would. Now listen, there isn't much time. We have to start [formulating] our plan of attack.");
							if (quest.Step == 13)
							{
								quest.Step = 14;
							}
							break;
						case "formulating":
							masterFrederick.SayTo(player, "Now, I think we should...What the...?");
							if (quest.Step >= 14 && quest.Step <= 17)
							{
								SendSystemMessage(player, "Master Frederick looks around as a gate starts to swirl open.");

								bool dunwynCloneCreated = false;
								if (player.Group != null)
								{
									foreach (GamePlayer groupMember in player.Group.GetPlayersInTheGroup())
									{
										BeginningOfWar memberQuest = groupMember.IsDoingQuest(typeof (BeginningOfWar)) as BeginningOfWar;
										// we found another groupmember doing the same quest...
										if (memberQuest != null && memberQuest.dunwynClone != null)
										{
											dunwynCloneCreated = true;
											break;
										}
									}
								}

								if (!dunwynCloneCreated)
								{
									quest.CreateDunwynClone();
									masterFrederick.SayTo(player, "Ah! Master Dunwyn! So good to have you here! Recruit, please help Master Dunwyn over here to me. To help him, right click on him.");
								}
							}
							break;

						case "detail":
							masterFrederick.SayTo(player, "What we've heard from travelers is that the fairies are amassing a small army to invade Cotswold. And by small, I don't mean their stature. In fact, it may be a large army, but that's where [Master Dunwyn] comes in.");
							break;

						case "Master Dunwyn":
							masterFrederick.SayTo(player, "He should be able to take out the majority of the fairies, but you need to make sure you find and kill their leader. She should be obvious. I don't know who she is, but I [trust] your instincts.");
							break;
						case "trust":
							masterFrederick.SayTo(player, "Travel due east from this tower until you reach the sunken statue. That is where I am told they are massing. Good luck to the both of you.");
							if (quest.Step == 15)
							{
								quest.Step = 16;
							}
							break;
						case "returned to the Marsh":
							masterFrederick.SayTo(player, "Ah well, it was expected. He prefers it there, for some odd reason. Though, it was nice to see him again. Perhaps when this fairy business is finally at a close, I'll go visit him. But first, what proof do you have the leader of this army is dead?");
							break;
						case "reward":
							masterFrederick.SayTo(player, "Yes, for you my young recruit, please take this helm and this ring. I know these magical items will help you as you make your way through this life. Be safe now Vinde. We will speak again very soon.");
							if (quest.Step == 19)
							{
								quest.FinishQuest();
							}
							break;

						case "abort":
							player.Out.SendCustomDialog("Do you really want to abort this quest, \nall items gained during quest will be lost?", new CustomDialogResponse(CheckPlayerAbortQuest));
							break;
					}
				}
			}
		}

		protected static void SubscribeQuest(DOLEvent e, object sender, EventArgs args)
		{
			QuestEventArgs qargs = args as QuestEventArgs;
			if (qargs == null)
				return;

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(BeginningOfWar)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToMasterDunwyn(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies
			//for the quest!		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(masterFrederick.CanGiveQuest(typeof (BeginningOfWar), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			BeginningOfWar quest = player.IsDoingQuest(typeof (BeginningOfWar)) as BeginningOfWar;

			dunwyn.TurnTo(player);
			if (e == GameLivingEvent.Interact)
			{
				if (quest != null)
				{
					if (quest.Step <= 2)
					{
						dunwyn.SayTo(player, "What? Who are you? What do you want? Can't you leave an old man in peace?");
					}
					else if (quest.Step == 3)
					{
						dunwyn.SayTo(player, "So...Frederick wants me to help fend off this fairy thingamabobber whatsamacallit. Well, fine! I see I didn't raise that boy right. Now lookee here, youngster. I have to make some [plans] before traipsing off to Camelot.");
					}
					else if (quest.Step == 10)
					{
						dunwyn.SayTo(player, "You're back faster than I thought. Alright then, showoff, give me that swamp slime.");
					}
					return;
				}
			} // The player whispered to NPC (clicked on the text inside the [])
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				if (quest != null)
				{
					//Do some small talk :)
					switch (wArgs.Text)
					{
						case "plans":
							dunwyn.SayTo(player, "Yes, plans! Have you ever heard of them? I swear, young people these days don't have a lick of common sense. I can't just run off to Camelot without making the proper preparations, now can I? Here, give me a [moment], will you?");
							SendSystemMessage(player, "Master Dunwyn takes the note and scribbles a few things on the back of it and hands it back to you.");
							break;

						case "moment":
							dunwyn.SayTo(player, "Alright young'un. You get me those supplies there on that list, and I'll be sure to go help you defeat that thingiemabobber whateveritscalled over there near Camelot. Now go on. Get!");
							if (quest.Step == 3)
							{
								GiveItem(player, listDunwyn);
								quest.Step = 4;
							}
							break;

						case "teleport":
							dunwyn.SayTo(player, "Hehe! Here you go!");
							if (quest.Step == 13)
							{
								quest.TeleportTo(player, dunwyn, locationFrederick, 10);
							}
							break;
					}
				}
			}
		}

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToMasterDunwynClone(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies
			//for the quest!		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(masterFrederick.CanGiveQuest(typeof (BeginningOfWar), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			BeginningOfWar quest = player.IsDoingQuest(typeof (BeginningOfWar)) as BeginningOfWar;

			if (e == GameLivingEvent.Interact)
			{
				//We check if the player is already doing the quest
				if (quest != null && quest.dunwynClone != null && quest.dunwynClone == sender)
				{
					quest.dunwynClone.TurnTo(player);

					if (quest.Step == 14)
					{
						quest.dunwynClone.SayTo(player, "Wha...? Who? Oh, it's you again. Yes yes. Come to help the feeble old man at your trainer's request. Confounded. Fine then. I'll follow you. Just walk slowly.");
						quest.dunwynClone.MaxSpeedBase = player.MaxSpeedBase;
						quest.dunwynClone.Follow(player, 100, 3000);
						quest.Step = 15;
						if (player.Group != null)
						{
							foreach (GamePlayer groupMember in player.Group.GetPlayersInTheGroup())
							{
								BeginningOfWar memberQuest = groupMember.IsDoingQuest(typeof (BeginningOfWar)) as BeginningOfWar;
								// we found another groupmember doing the same quest...
								if (memberQuest != null && memberQuest.Step == 14)
								{
									memberQuest.Step = 15;
								}
							}
						}
					}
					else if (quest.Step >= 15 && quest.Step < 17)
					{
						quest.dunwynClone.MaxSpeedBase = player.MaxSpeedBase;
						quest.dunwynClone.Follow(player, 100, 3000);
					}
					return;
				}
			}
		}

		protected virtual void ResetMasterDunwyn()
		{
			if (dunwynClone != null && (dunwynClone.IsAlive || dunwynClone.ObjectState == GameObject.eObjectState.Active))
			{
				m_animSpellObjectQueue.Enqueue(dunwynClone);
				m_animSpellTeleportTimerQueue.Enqueue(new RegionTimer(dunwynClone, new RegionTimerCallback(MakeAnimEmoteSequence), 500));

				m_animEmoteObjectQueue.Enqueue(dunwynClone);
				m_animEmoteTeleportTimerQueue.Enqueue(new RegionTimer(dunwynClone, new RegionTimerCallback(MakeAnimEmoteSequence), 2500));
			}

			if (dunwynClone != null)
			{
				RegionTimer castTimer = new RegionTimer(dunwynClone, new RegionTimerCallback(DeleteDunwynClone), 3500);
			}
		}

		protected virtual int CastDunwynClone(RegionTimer callingTimer)
		{
			foreach (GamePlayer visPlayer in fairySorceress[0].GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				foreach (GameNPC fairy in fairySorceress)
				{
					visPlayer.Out.SendSpellEffectAnimation(dunwynClone, fairy, 61, 10, false, 0x01);
				}
			}
			return 0;
		}

		protected virtual int DeleteDunwynClone(RegionTimer callingTimer)
		{
			if (dunwynClone != null)
			{
				GameEventMgr.RemoveHandler(dunwynClone, GameLivingEvent.Interact, new DOLEventHandler(TalkToMasterDunwynClone));

				dunwynClone.Delete();
			}
			return 0;
		}

		protected virtual int TalkDunwynClone(RegionTimer callingTimer)
		{
			if (Step == 17)
			{
				dunwynClone.SayTo(m_questPlayer, "Well youngun, I have to get back to my patrolling. Good job with that princess whoever. Now, be a sport and let Frederick know that I went back to the Marsh. Darn son of mine.");
				dunwynClone.SayTo(m_questPlayer, "Tally ho!");

				ResetMasterDunwyn();
				Step = 18;
			}
			return 0;
		}


		protected void CreateDunwynClone()
		{
			if (dunwynClone == null)
			{
				dunwynClone = new GameNPC();
				dunwynClone.Name = dunwyn.Name;
				dunwynClone.Model = dunwyn.Model;
				dunwynClone.GuildName = dunwyn.GuildName;
				dunwynClone.Realm = dunwyn.Realm;
				dunwynClone.CurrentRegionID = 1;
				dunwynClone.Size = dunwyn.Size;
				dunwynClone.Level = 15; // to make the figthing against fairy sorceress a bit more dramatic :)

				dunwynClone.X = 567604 + Util.Random(-150, 150);
				dunwynClone.Y = 509619 + Util.Random(-150, 150);
				dunwynClone.Z = 2813;
				dunwynClone.Heading = 3292;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 798);
				template.AddNPCEquipment(eInventorySlot.RightHandWeapon, 19);
				dunwynClone.Inventory = template.CloseTemplate();
				dunwynClone.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

//				dunwynClone.AddNPCEquipment((byte) eEquipmentItems.TORSO, 798, 0, 0, 0);
//				dunwynClone.AddNPCEquipment((byte) eEquipmentItems.RIGHT_HAND, 19, 0, 0, 0);

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 0;
				dunwynClone.SetOwnBrain(brain);

				dunwynClone.AddToWorld();

				GameEventMgr.AddHandler(dunwynClone, GameLivingEvent.Interact, new DOLEventHandler(TalkToMasterDunwynClone));

				foreach (GamePlayer visPlayer in dunwynClone.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					visPlayer.Out.SendEmoteAnimation(dunwynClone, eEmote.Bind);
				}
			}
			else
			{
				TeleportTo(dunwynClone, dunwynClone, locationDunwynClone);
			}

		}

		protected static void PlayerLeftWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			BeginningOfWar quest = player.IsDoingQuest(typeof (BeginningOfWar)) as BeginningOfWar;
			if (quest != null)
			{
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));

				quest.ResetMasterDunwyn();
			}
		}

		protected static void PlayerUseSlot(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = (GamePlayer) sender;
			if (player == null)
				return;

			// assistant works only in camelot...            
			BeginningOfWar quest = (BeginningOfWar) player.IsDoingQuest(typeof (BeginningOfWar));
			if (quest == null)
				return;

			UseSlotEventArgs uArgs = (UseSlotEventArgs) args;

			InventoryItem item = player.Inventory.GetItem((eInventorySlot)uArgs.Slot);
			if (item != null && item.Id_nb == listDunwyn.Id_nb)
			{
				if (quest.Step >= 4 && quest.Step <= 8)
				{
					IList textList = new ArrayList();
					textList.Add("swamp slime");
					textList.Add("river spriteling claw");
					textList.Add("swamp rat tail");
					player.Out.SendCustomTextWindow("List of Master Dunwyn", textList);
				}

				if (quest.Step == 4)
				{
					quest.Step = 5;
				}
				else if (quest.Step == 6)
				{
					quest.Step = 7;
				}
				else if (quest.Step == 8)
				{
					quest.Step = 9;
					SendSystemMessage(player, "The List disappears in a puff of blue smoke.");
					RemoveItem(player, listDunwyn);
				}
			}

		}

		protected static void PlayerEnterWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			BeginningOfWar quest = player.IsDoingQuest(typeof (BeginningOfWar)) as BeginningOfWar;
			if (quest != null)
			{
				GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
				GameEventMgr.AddHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));

				if (quest.Step >= 15 && quest.Step <= 17)
				{
					quest.CreateDunwynClone();
				}
			}
		}

		/// <summary>
		/// This method checks if a player qualifies for this quest
		/// </summary>
		/// <returns>true if qualified, false if not</returns>
		public override bool CheckQuestQualification(GamePlayer player)
		{
			// if the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(typeof (BeginningOfWar)) != null)
				return true;

			// This checks below are only performed is player isn't doing quest already
			if (player.HasFinishedQuest(typeof (IreFairyIre)) == 0)
				return false;

			if (!CheckPartAccessible(player, typeof (BeginningOfWar)))
				return false;

			if (player.Level < minimumLevel || player.Level > maximumLevel)
				return false;

			return true;
		}

		/* This is our callback hook that will be called when the player clicks
		 * on any button in the quest offer dialog. We check if he accepts or
		 * declines here...
		 */

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			//We recheck the qualification, because we don't talk to players
			//who are not doing the quest
			if(masterFrederick.CanGiveQuest(typeof (BeginningOfWar), player)  <= 0)
				return;

			BeginningOfWar quest = player.IsDoingQuest(typeof (BeginningOfWar)) as BeginningOfWar;

			if (quest != null)
				return;

			if (response == 0x00)
			{
				masterFrederick.SayTo(player, "Oh well, if you change your mind, please come back!");
			}
			else
			{
				//Check if we can add the quest!
				if (!masterFrederick.GiveQuest(typeof (BeginningOfWar), player, 1))
					return;

				GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
				GameEventMgr.AddHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));

				masterFrederick.SayTo(player, "Excellent recruit, simply excellent! Now listen, I know you won't be able to defeat this fairie army on your own. So I have a [solution] to this problem.");
			}
		}

		/* This is our callback hook that will be called when the player clicks
		 * on any button in the quest offer dialog. We check if he accepts or
		 * declines here...
		 */

		private static void CheckPlayerAbortQuest(GamePlayer player, byte response)
		{
			BeginningOfWar quest = player.IsDoingQuest(typeof (BeginningOfWar)) as BeginningOfWar;

			if (quest == null)
				return;

			if (response == 0x00)
			{
				masterFrederick.SayTo(player, "Good, no go out there and finish your work!");
			}
			else
			{
				SendSystemMessage(player, "Aborting Quest " + questTitle + ". You can start over again if you want.");
				quest.AbortQuest();
			}
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
						return "[Step #1] Liste to Master Frederick. If you go link dead or log out, ask him the Solution] to the problem might be.";
					case 2:
						return "[Step #2] Deliver the letter to Master Dunwyn in Avalon Marsh.";
					case 3:
						return "[Step #3] Wait for Master Dunwyn to read the letter. If he stops reading the letter, ask him if he has any [plans] for helping.";
					case 4:
						return "[Step #4] You must now READ Master Dunwyn's list. In order to do that, right click on the list in your inventory and type /use. Get the things on his list for him.";
					case 5:
						return "[Step #5] You must now find a swamp slime. Slay the creature and retieve some of its slime for Master Dunwyn.";
					case 6:
						return "[Step #6] Read the list to find out what the second item is.";
					case 7:
						return "[Step #7] Find a river spriteling. You can find them near small bodies of water in the Avalon Marsh. Slay one and get its claw.";
					case 8:
						return "[Step #8] Read the list for the last time to find out what the final ingredient is.";
					case 9:
						return "[Step #9] Look around for a swamp rat. There are many of them in Avalon Marsh. Slay the creature for its tail.";
					case 10:
						return "[Step #10] Return to Master Dunwyn near the Shrouded Isles portal. Be sure to give him the swamp slime first.";
					case 11:
						return "[Step #11] Now give Master Dunwyn the river spriteling claw.";
					case 12:
						return "[Step #12] Now give Master Dunwyn the swamp rat tail.";
					case 13:
						return "[Step #13] Return to Master Frederick and let him know you've contacted Master Dunwyn and that he will be [assisting] with the fairy problem.";
					case 14:
						return "[Step #14] Listen to Master Frederick give you the details behind his plan.";
					case 15:
						return "[Step #15] Accompany Master Dunwyn to Master Frederick.";
					case 16:
						return "[Step #16] Travel due East from the guard tower in Cotswold. You will see a sunken statue in the ground. That is where the fairies are amassing. Be sure Master Dunwyn follows you.";
					case 17:
						return "[Step #17] When the fighting is done, speak with Master Dunwyn. If you went linkdead, logged out or the like and Master Dunwyn is not there, return to Master Frederick in Cotswold.";
					case 18:
						return "[Step #18] Return to Master Frederick. Be sure to tell him that Master Dunwyn has [returned to the Marsh].";
					case 19:
						return "[Step #19] Wait for Master Frederick to reward you for your efforts in defeating the fairy army. If he forgets, ask him about the [reward].";

				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player==null || player.IsDoingQuest(typeof (BeginningOfWar)) == null)
				return;

			if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == dunwyn.Name && gArgs.Item.Id_nb == scrollDunwyn.Id_nb)
				{
					dunwyn.SayTo(player, "What? What's this? A letter, for me? Hrm...Let me see what it says. Yes...one moment.");
					RemoveItem(dunwyn, player, scrollDunwyn);
					Step = 3;
					return;
				}
			}


			if (e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;

				if (gArgs.Target.Name == "swamp slime" && (Step == 5 || Step == 4))
				{
					SendSystemMessage("You slay the swamp slime and retrieve some of it's slime for Master Dunwyn.");
					GiveItem(gArgs.Target, player, swampSlimeItem);
					Step = 6;
					return;
				}

				if (gArgs.Target.Name == "river spriteling" && (Step == 7 || Step == 6))
				{
					SendSystemMessage("You slay the spriteling and get it's claw for Master Dunwyn.");
					GiveItem(gArgs.Target, player, riverSpritlingClaw);
					Step = 8;
					return;
				}

				if (gArgs.Target.Name == "swamp rat" && (Step == 9 || Step == 8))
				{
					if (Step == 8)
					{
						SendSystemMessage(player, "The List disappears in a puff of blue smoke.");
						RemoveItem(player, listDunwyn);
					}

					SendSystemMessage("You slay the rat and retrieve it's tail for Master Dunwyn.");
					GiveItem(gArgs.Target, player, swampRatTail);

					Step = 10;
					return;
				}
			}

			if (e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;

				if (gArgs.Target.Name == dunwyn.Name)
				{
					if (Step == 10 && gArgs.Item.Id_nb == swampSlimeItem.Id_nb)
					{
						dunwyn.SayTo(player, "Well, least you managed to get a good amount of it. Ok, now give me the spriteling claw.");
						RemoveItem(dunwyn, player, swampSlimeItem);
						Step = 11;
						return;
					}
					else if (Step == 11 && gArgs.Item.Id_nb == riverSpritlingClaw.Id_nb)
					{
						dunwyn.SayTo(player, "Good, good. A nice sharp one. I like it. Now, give me the swamp rat tail. This had better be a good, thick one!");
						RemoveItem(dunwyn, player, riverSpritlingClaw);
						Step = 12;
						return;
					}
					else if (Step == 12 && gArgs.Item.Id_nb == swampRatTail.Id_nb)
					{
						dunwyn.SayTo(player, "Yes yes, this will have to do. Now listen here whipersnapper. I have a few things to do yet, so why don't you get yourself back to your trainer and I'll be along promptly. In fact, let me [teleport] you home. *hehe*");
						RemoveItem(dunwyn, player, swampRatTail);
						Step = 13;
						return;
					}
				}
			}

			if (Step == 16 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;

				if (gArgs.Target.Name == princessObera.Name)
				{
					SendSystemMessage("You slay the princess and take her head as proof.");
					GiveItem(gArgs.Target, player, princessOberasHead);
					Step = 17;

					new RegionTimer(gArgs.Target, new RegionTimerCallback(TalkDunwynClone), 7000);
					return;
				}
			}

			if (Step == 18 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == masterFrederick.Name && gArgs.Item.Id_nb == princessOberasHead.Id_nb)
				{
					masterFrederick.SayTo(player, "Excellent work recruit! My, this is a rather large fairy head, don't you think? Well, nevermind that. I'm sure we won't hear from those fairies ever again. And, as per our usual agreement, I have a [reward] for you.");
					RemoveItem(masterFrederick, player, princessOberasHead);
					Step = 19;
					return;
				}
			}
		}

		public override void AbortQuest()
		{
			ResetMasterDunwyn();

			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, scrollDunwyn, false);
			RemoveItem(m_questPlayer, listDunwyn, false);
			RemoveItem(m_questPlayer, swampRatTail, false);
			RemoveItem(m_questPlayer, swampSlimeItem, false);
			RemoveItem(m_questPlayer, riverSpritlingClaw, false);
			RemoveItem(m_questPlayer, princessOberasHead, false);

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			// make sure to clean up, should be needed , but just to make certain
			ResetMasterDunwyn();
			//Give reward to player here ...              
			m_questPlayer.GainExperience(507, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 8, Util.Random(50)), "You recieve {0} as a reward.");

			if (m_questPlayer.HasAbilityToUseItem(recruitsHelm))
				GiveItem(masterFrederick, m_questPlayer, recruitsHelm);
			else
				GiveItem(masterFrederick, m_questPlayer, recruitsCap);
			GiveItem(masterFrederick, m_questPlayer, recruitsRing);

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));

		}

	}
}
