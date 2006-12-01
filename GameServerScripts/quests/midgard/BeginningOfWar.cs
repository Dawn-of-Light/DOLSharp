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
 * Directory: /scripts/quests/midgard/
 *
 * Description:
 *  Brief Walkthrough: 
 * 1) Travel to loc=41211,50221 Vale of Mularn to speak with Dalikor 
 * 2) Go to Griffin Handler Njiedi, at loc=55561,58225,5005 dir=126 Vale of Mularn, and purchase a griffin ticket to Gotar. 
 * 3) Once in Gotar (You'll be dropped off at the entrance of Ft. Alta.) look for MasterBriedi. He tends to roam in a circular pattern around the front of Ft. Alta, and may take a few clicks to get him to respond to you. I first found him at loc=26240,15706,4489 dir=292 Gotar and moving in the western direction. 
 * 4) Hand MasterBriedi the Scroll for Master Briedi from Dalikor. 
 * 5) /use the Master Briedi's List from Master Briedi 
 * 6) Kill an escaped thrall for his Tattered Shirt. 
 * 7) Kill a Smiera-Gatto for its smiera-gatto claw 
 * 8) Kill a Coastal Wolf for its coastal wolf blood. 
 * 9) Find Master Briedi and hand him the items he asks for that you have collected. 
 * 10) Master Briedi will teleport you back to Dalikor. Talk with Dalikor. 
 * 11) Right click on Master Briedi and then click on Dalikor for further instructions. 
 * 12) Go to loc= 39315,38457 Vale of Mularn, with Master Briedi in tow. When you get there you will see a swarm of Fallen Askefruer Guard's with Princess Aiyr in the middle of the swarm. Focus on killing Princess Aiyr, blue con to a level 4, Master Briedi will deal with the Askefruer Guard's. Once Princess Aiyr is dead (when she dies her head will appear in your inventory) you can help Master Briedi finish killing the other Askefruer. 
 * 13) Speak with Master Briedi after the fight. 
 * 14) Go back to Dalikor and type /whisper returned to Gotar. 
 * 15) Hand Dalikor Princess Aiyr's Head to collect your reward.
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

namespace DOL.GS.Quests.Midgard
{
	/* The first thing we do, is to declare the class we create
	 * as Quest. To do this, we derive from the abstract class
	 * AbstractQuest
	 * 	 
	 */

	public class BeginningOfWar : BaseDalikorQuest
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

		protected const string questTitle = "Beginning of War (Mid)";
		protected const int minimumLevel = 4;
		protected const int maximumLevel = 4;

		private static GameNPC dalikor = null;

		private static GameLocation locationBriediClone = new GameLocation(null, 100, 794455, 721224, 4989, 3292);

		private static GameNPC briedi = null;
		private GameNPC briediClone = null;

		private static GameNPC princessAiyr = null;
		private static GameNPC[] askefruerSorceress = new GameNPC[4];

		private bool princessAiyrAttackStarted = false;

		private static ItemTemplate tatteredShirt = null;
		private static ItemTemplate smieraGattoClaw = null;
		private static ItemTemplate coastalWolfBlood = null;

		private static ItemTemplate princessAiyrHead = null;

		private static ItemTemplate scrollBriedi = null;
		private static ItemTemplate listBriedi = null;
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
				* the world who comes from the Albion realm. If we find a the players,
				* this means we don't have to create a new one.
				* 
				* NOTE: You can do anything you want in this method, you don't have
				* to search for NPC's ... you could create a custom item, place it
				* on the ground and if a player picks it up, he will get the quest!
				* Just examples, do anything you like and feel comfortable with :)
				*/

			#region defineNPCs

			dalikor = GetDalikor();

            GameNPC[] npcs = WorldMgr.GetNPCsByName("Master Briedi", eRealm.Midgard);
			if (npcs.Length == 0)
			{
				briedi = new GameNPC();
				briedi.Model = 157;
				briedi.Name = "Master Briedi";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + briedi.Name + ", creating him ...");
				briedi.GuildName = "Part of " + questTitle + " Quest";
				briedi.Realm = (byte) eRealm.Midgard;
				briedi.CurrentRegionID = 103;

				briedi.Size = 50;
				briedi.Level = 45;
				briedi.X = GameLocation.ConvertLocalXToGlobalX(26240, 103);
				briedi.Y = GameLocation.ConvertLocalYToGlobalY(15706, 103);
				briedi.Z = 4489;
				briedi.Heading = 292;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 348);
				template.AddNPCEquipment(eInventorySlot.LegsArmor, 349);
				template.AddNPCEquipment(eInventorySlot.ArmsArmor, 350);
				template.AddNPCEquipment(eInventorySlot.HandsArmor, 351);
				template.AddNPCEquipment(eInventorySlot.FeetArmor, 352);
				template.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 640);
				briedi.Inventory = template.CloseTemplate();
				briedi.SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);
                briedi.AddToWorld();

//				briedi.AddNPCEquipment((byte) eEquipmentItems.TORSO, 348, 0, 0, 0);
//				briedi.AddNPCEquipment((byte) eEquipmentItems.LEGS, 349, 0, 0, 0);
//				briedi.AddNPCEquipment((byte) eEquipmentItems.ARMS, 350, 0, 0, 0);
//				briedi.AddNPCEquipment((byte) eEquipmentItems.HAND, 351, 0, 0, 0);
//				briedi.AddNPCEquipment((byte) eEquipmentItems.FEET, 352, 0, 0, 0);
//				briedi.AddNPCEquipment((byte) eEquipmentItems.TWO_HANDED, 640, 0, 0, 0);

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                {
                    briedi.SaveIntoDatabase();
                }

				
			}
			else
				briedi = npcs[0];


			npcs = WorldMgr.GetNPCsByName("Princess Aiyr", eRealm.None);
			if (npcs.Length == 0)
			{
				princessAiyr = new GameNPC();

				princessAiyr.Name = "Princess Aiyr";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + princessAiyr.Name + ", creating ...");
				princessAiyr.X = GameLocation.ConvertLocalXToGlobalX(39315, 100);
				princessAiyr.Y = GameLocation.ConvertLocalYToGlobalY(38957, 100);
				princessAiyr.Z = 5460;
				princessAiyr.Heading = 1;
				princessAiyr.Model = 678;
				princessAiyr.GuildName = "Part of " + questTitle + " Quest";
				princessAiyr.Realm = (byte) eRealm.None;
				princessAiyr.CurrentRegionID = 100;
				princessAiyr.Size = 49;
				princessAiyr.Level = 3;

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 80;
				brain.AggroRange = 1000;
				princessAiyr.SetOwnBrain(brain);

				if (SAVE_INTO_DATABASE)
					princessAiyr.SaveIntoDatabase();
				princessAiyr.AddToWorld();
			}
			else
			{
				princessAiyr = npcs[0];
			}

			int counter = 0;
			foreach (GameNPC npc in princessAiyr.GetNPCsInRadius(500))
			{
				if (npc.Name == "askefruer sorceress")
				{
					askefruerSorceress[counter] = npc;
					counter++;
				}
				if (counter == askefruerSorceress.Length)
					break;
			}

			for (int i = 0; i < askefruerSorceress.Length; i++)
			{
				if (askefruerSorceress[i] == null)
				{
					askefruerSorceress[i] = new GameNPC();
					askefruerSorceress[i].Model = 678;
					askefruerSorceress[i].Name = "askefruer sorceress";
					if (log.IsWarnEnabled)
						log.Warn("Could not find " + askefruerSorceress[i].Name + ", creating ...");
					askefruerSorceress[i].GuildName = "Part of " + questTitle + " Quest";
					askefruerSorceress[i].Realm = (byte) eRealm.None;
					askefruerSorceress[i].CurrentRegionID = 100;
					askefruerSorceress[i].Size = 35;
					askefruerSorceress[i].Level = 3;
					askefruerSorceress[i].X = princessAiyr.X + Util.Random(-150, 150);
					askefruerSorceress[i].Y = princessAiyr.Y + Util.Random(-150, 150);
					askefruerSorceress[i].Z = princessAiyr.Z;

					StandardMobBrain brain = new StandardMobBrain();
					brain.AggroLevel = 30;
					brain.AggroRange = 300;
					askefruerSorceress[i].SetOwnBrain(brain);

					askefruerSorceress[i].Heading = 93;
					//fairySorceress[i].EquipmentTemplateID = 200276;                

					//You don't have to store the created mob in the db if you don't want,
					//it will be recreated each time it is not found, just comment the following
					//line if you rather not modify your database
					if (SAVE_INTO_DATABASE)
						askefruerSorceress[i].SaveIntoDatabase();
					askefruerSorceress[i].AddToWorld();
				}
			}

			#endregion

			#region defineItems

			coastalWolfBlood = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "coastal_wolf_blood");
			if (coastalWolfBlood == null)
			{
				coastalWolfBlood = new ItemTemplate();
				coastalWolfBlood.Name = "Coastal Wolf Blood";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + coastalWolfBlood.Name + " , creating it ...");

				coastalWolfBlood.Weight = 110;
				coastalWolfBlood.Model = 99;

				coastalWolfBlood.Object_Type = (int) eObjectType.GenericItem;

				coastalWolfBlood.Id_nb = "coastal_wolf_blood";
				coastalWolfBlood.IsPickable = true;
				coastalWolfBlood.IsDropable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(coastalWolfBlood);
			}

			tatteredShirt = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "tattered_shirt");
			if (tatteredShirt == null)
			{
				tatteredShirt = new ItemTemplate();
				tatteredShirt.Name = "Tattered Shirt";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + tatteredShirt.Name + " , creating it ...");

				tatteredShirt.Weight = 10;
				tatteredShirt.Model = 139;

				tatteredShirt.Object_Type = (int) eObjectType.GenericItem;

				tatteredShirt.Id_nb = "tattered_shirt";
				tatteredShirt.IsPickable = true;
				tatteredShirt.IsDropable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(tatteredShirt);
			}

			scrollBriedi = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "scroll_for_briedi");
			if (scrollBriedi == null)
			{
				scrollBriedi = new ItemTemplate();
				scrollBriedi.Name = "Scroll for Master Briedi";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + scrollBriedi.Name + " , creating it ...");

				scrollBriedi.Weight = 10;
				scrollBriedi.Model = 498;

				scrollBriedi.Object_Type = (int) eObjectType.GenericItem;

				scrollBriedi.Id_nb = "scroll_for_briedi";
				scrollBriedi.IsPickable = true;
				scrollBriedi.IsDropable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(scrollBriedi);
			}

			listBriedi = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "list_for_briedi");
			if (listBriedi == null)
			{
				listBriedi = new ItemTemplate();
				listBriedi.Name = "List for Master Briedi";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + listBriedi.Name + " , creating it ...");

				listBriedi.Weight = 10;
				listBriedi.Model = 498;

				listBriedi.Object_Type = (int) eObjectType.GenericItem;

				listBriedi.Id_nb = "list_for_briedi";
				listBriedi.IsPickable = true;
				listBriedi.IsDropable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(listBriedi);
			}

			smieraGattoClaw = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "smiera_gatto_claw");
			if (smieraGattoClaw == null)
			{
				smieraGattoClaw = new ItemTemplate();
				smieraGattoClaw.Name = "Smiera-Gatto's Claw";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + smieraGattoClaw.Name + " , creating it ...");

				smieraGattoClaw.Weight = 2;
				smieraGattoClaw.Model = 106;

				smieraGattoClaw.Object_Type = (int) eObjectType.GenericItem;

				smieraGattoClaw.Id_nb = "smiera_gatto_claw";
				smieraGattoClaw.IsPickable = true;
				smieraGattoClaw.IsDropable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(smieraGattoClaw);
			}

			princessAiyrHead = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "princess_ayir_head");
			if (princessAiyrHead == null)
			{
				princessAiyrHead = new ItemTemplate();
				princessAiyrHead.Name = "Princess Ayir's Head";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + princessAiyrHead.Name + " , creating it ...");

				princessAiyrHead.Weight = 15;
				princessAiyrHead.Model = 503;

				princessAiyrHead.Object_Type = (int) eObjectType.GenericItem;

				princessAiyrHead.Id_nb = "princess_ayir_head";
				princessAiyrHead.IsPickable = true;
				princessAiyrHead.IsDropable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(princessAiyrHead);
			}

			// item db check
			recruitsHelm = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "recruits_studded_helm_mid");
			if (recruitsHelm == null)
			{
				recruitsHelm = new ItemTemplate();
				recruitsHelm.Name = "Recruit's Studded Helm (Mid)";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsHelm.Name + ", creating it ...");
				recruitsHelm.Level = 8;

				recruitsHelm.Weight = 24;
				recruitsHelm.Model = 824; // studded vest

				recruitsHelm.DPS_AF = 12; // Armour
				recruitsHelm.SPD_ABS = 19; // Absorption

				recruitsHelm.Object_Type = (int) eObjectType.Studded;
				recruitsHelm.Item_Type = (int) eEquipmentItems.HEAD;
				recruitsHelm.Id_nb = "recruits_studded_helm_mid";
				recruitsHelm.Gold = 0;
				recruitsHelm.Silver = 9;
				recruitsHelm.Copper = 0;
				recruitsHelm.IsPickable = true;
				recruitsHelm.IsDropable = true;
				recruitsHelm.Color = 14; // blue leather

				recruitsHelm.Bonus = 5; // default bonus

				recruitsHelm.Bonus1 = 4;
				recruitsHelm.Bonus1Type = (int) eStat.DEX;

				recruitsHelm.Bonus2 = 1;
				recruitsHelm.Bonus2Type = (int) eResist.Spirit;

				recruitsHelm.Bonus3 = 12;
				recruitsHelm.Bonus3Type = (int) eProperty.MaxHealth;

				recruitsHelm.Quality = 100;
				recruitsHelm.MaxQuality = 100;
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
				recruitsCap.Color = 36; // blue cloth

				recruitsCap.Bonus = 5; // default bonus

				recruitsCap.Bonus1 = 4;
				recruitsCap.Bonus1Type = (int) eStat.DEX;

				recruitsCap.Bonus2 = 20;
				recruitsCap.Bonus2Type = (int) eProperty.MaxHealth;

				recruitsCap.Bonus3 = 1;
				recruitsCap.Bonus3Type = (int) eResist.Spirit;

				recruitsCap.Quality = 100;
				recruitsCap.MaxQuality = 100;
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

			recruitsRing = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "recruits_pewter_ring");
			if (recruitsRing == null)
			{
				recruitsRing = new ItemTemplate();
				recruitsRing.Name = "Recruit's Pewter Ring";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsRing.Name + ", creating it ...");
				recruitsRing.Level = 6;

				recruitsRing.Weight = 2;
				recruitsRing.Model = 103;

				recruitsRing.Object_Type = (int) eObjectType.Magical;
				recruitsRing.Item_Type = (int) eEquipmentItems.R_RING;
				recruitsRing.Id_nb = "recruits_pewter_ring";
				recruitsRing.Gold = 0;
				recruitsRing.Silver = 9;
				recruitsRing.Copper = 0;
				recruitsRing.IsPickable = true;
				recruitsRing.IsDropable = true;

				recruitsRing.Bonus = 5; // default bonus

				recruitsRing.Bonus1 = 4;
				recruitsRing.Bonus1Type = (int) eStat.CON;

				recruitsRing.Bonus2 = 1;
				recruitsRing.Bonus2Type = (int) eResist.Crush;

				recruitsRing.Bonus3 = 12;
				recruitsRing.Bonus3Type = (int) eProperty.MaxHealth;

				recruitsRing.Quality = 100;
				recruitsRing.MaxQuality = 100;
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
			//We want to be notified whenever a player enters the world            
			GameEventMgr.AddHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));

			GameEventMgr.AddHandler(dalikor, GameLivingEvent.Interact, new DOLEventHandler(TalkToDalikor));
			GameEventMgr.AddHandler(dalikor, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToDalikor));

			GameEventMgr.AddHandler(briedi, GameLivingEvent.Interact, new DOLEventHandler(TalkToMasterBriedi));
			GameEventMgr.AddHandler(briedi, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterBriedi));

			GameEventMgr.AddHandler(princessAiyr, GameNPCEvent.OnAICallback, new DOLEventHandler(CheckNearPrincessAyir));

			/* Now we bring to dalikor the possibility to give this quest to players */
			dalikor.AddQuestToGive(typeof (BeginningOfWar));

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
			if (dalikor == null)
				return;

			/* Removing hooks works just as adding them but instead of 
			 * AddHandler, we call RemoveHandler, the parameters stay the same
			 */
			GameEventMgr.RemoveHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));

			GameEventMgr.RemoveHandler(dalikor, GameLivingEvent.Interact, new DOLEventHandler(TalkToDalikor));
			GameEventMgr.RemoveHandler(dalikor, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToDalikor));

			GameEventMgr.RemoveHandler(briedi, GameLivingEvent.Interact, new DOLEventHandler(TalkToMasterBriedi));
			GameEventMgr.RemoveHandler(briedi, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterBriedi));

			GameEventMgr.RemoveHandler(princessAiyr, GameNPCEvent.OnAICallback, new DOLEventHandler(CheckNearPrincessAyir));

			/* Now we remove to dalikor the possibility to give this quest to players */
			dalikor.RemoveQuestToGive(typeof (BeginningOfWar));

		}

		protected static void CheckNearPrincessAyir(DOLEvent e, object sender, EventArgs args)
		{
			GameNPC m_princessAyir = (GameNPC) sender;

			// if princess is dead no ned to checks ...
			if (!m_princessAyir.IsAlive || m_princessAyir.ObjectState != GameObject.eObjectState.Active)
				return;

			foreach (GamePlayer player in m_princessAyir.GetPlayersInRadius(1000))
			{
				BeginningOfWar quest = (BeginningOfWar) player.IsDoingQuest(typeof (BeginningOfWar));

				if (quest != null && !quest.princessAiyrAttackStarted && quest.Step == 16)
				{
					quest.princessAiyrAttackStarted = true;

					SendSystemMessage(player, "There they are. You take care of the princess I'll deal with the askefruer sorcesses littleone.");

					IAggressiveBrain aggroBrain = m_princessAyir.Brain as IAggressiveBrain;
					if (aggroBrain != null)
						aggroBrain.AddToAggroList(player, 50 + Util.Random(20));

					if (quest.briediClone != null)
					{
						foreach (GameNPC fairy in askefruerSorceress)
						{
							aggroBrain = quest.briediClone.Brain as IAggressiveBrain;
							if (aggroBrain != null)
								aggroBrain.AddToAggroList(fairy, Util.Random(50));
							aggroBrain = fairy.Brain as IAggressiveBrain;
							if (aggroBrain != null)
								aggroBrain.AddToAggroList(quest.briediClone, Util.Random(50));
						}
					}
				}
			}
		}

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToDalikor(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies
			//for the quest!		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(dalikor.CanGiveQuest(typeof (BeginningOfWar), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			BeginningOfWar quest = player.IsDoingQuest(typeof (BeginningOfWar)) as BeginningOfWar;

			dalikor.TurnTo(player);

			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...					
					dalikor.SayTo(player, "I hate to be the constant bearer of bad news, but unfortunately this time I am. It seems as thought the Askefruer are looking to make a move on [Mularn].");
					return;
				}
				else
				{
					switch (quest.Step)
					{
						case 13:
							dalikor.SayTo(player, "Welcome back recruit. You were gone for quite a while. Did things go alright with Master Briedi? Will he be [assisting] us?");
							break;
						case 15:
							dalikor.SayTo(player, "As I was saying, our scouts have reported that north from this spot is a small group of Askefruer that are planning an assault on Mularn. I don't think they are much of a threat to the guards, but they might do some harm to the [locals].");
							break;
						case 18:
							dalikor.SayTo(player, "Eeinken, back from battle so soon? Where is Master Briedi? Is he already [returned to Gotar]?");
							break;
						case 19:
							dalikor.SayTo(player, "The council has told me I may offer you these two items as a reward for your efforts. There is much I need to discuss with the council. We will speak again soon Eeinken.");
							quest.FinishQuest();
							break;
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
						case "Mularn":
							dalikor.SayTo(player, "Aye. Apparently they are trying to retaliate for us killing their leaders. We can't allow them to amass any sort of force to attack our locals. One of our scouts has reported that there is a small group of ten Askefruer [near here].");
							break;
						case "near here":
							dalikor.SayTo(player, "I don't expect you to be able to handle the entire group, no matter how skilled you are. No, I have something else [in mind].");
							break;
						case "in mind":
							dalikor.SayTo(player, "There is an elderly spiritmaster in Gotar near Fort Atla who will help us. His name is Master Briedi. He is very skilled and was a mighty adventurer at one time. I need for you to deliver a scroll to him. Will you [do that] for me?");
							break;
							//If the player offered his "help", we send the quest dialog now!
						case "do that":
							player.Out.SendCustomDialog("Will you take the letter to Master Briedi in Gotar for help with the Askefruer problem?", new CustomDialogResponse(CheckPlayerAcceptQuest));
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "assisting":
							dalikor.SayTo(player, "I knew he would. You and he are much alike. You both thirst for adventure. I admire that in you Eeinken. Now, I have some further information that I think you'll find [useful].");
							if (quest.Step == 13)
							{
								quest.Step = 14;
							}
							break;
						case "useful":
							dalikor.SayTo(player, "As I was saying...What the...?");
							if (quest.Step >= 14 && quest.Step <= 17)
							{
								SendEmoteMessage(player, "Dalikor cocks an eyebrow as a portal opens behind you.");

								bool briediCloneCreated = false;
								if (player.PlayerGroup != null)
								{
									foreach (GamePlayer groupMember in player.PlayerGroup.GetPlayersInTheGroup())
									{
										BeginningOfWar memberQuest = groupMember.IsDoingQuest(typeof (BeginningOfWar)) as BeginningOfWar;
										// we found another groupmember doing the same quest...
										if (memberQuest != null && memberQuest.briediClone != null)
										{
											briediCloneCreated = true;
											break;
										}
									}
								}

								if (!briediCloneCreated)
								{
									quest.CreateBriediClone();
									dalikor.SayTo(player, "Ah! Master Briedi, how nice to see you again. Recruit, please assist Master Briedi and bring him closer so he might be able to listen to the conversation as well. Right click on him to assist him.");
								}
							}
							break;

						case "locals":
							dalikor.SayTo(player, "You and Master Briedi will take out the camp and then return to me. You should concentrate on the leader while Master Briedi takes out the other soldiers. Alright Eeinken, it's time for you to head out. Be safe. Return to me when the Askefruer are dead.");
							if (quest.Step == 15)
							{
								quest.Step = 16;
							}
							break;
						case "returned to Gotar":
							dalikor.SayTo(player, "Ah, I suppose I should have expected him to do that. He would not have left if the job wasn't completed. Still, is there any proof that you have defeated the Askefruer responsible for this?");
							break;
						case "reward":
							dalikor.SayTo(player, "Yes, for you my young recruit, please take this helm and this ring. I know these magical items will help you as you make your way through this life. Be safe now Vinde. We will speak again very soon.");
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

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToMasterBriedi(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies
			//for the quest!		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(dalikor.CanGiveQuest(typeof (BeginningOfWar), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			BeginningOfWar quest = player.IsDoingQuest(typeof (BeginningOfWar)) as BeginningOfWar;

			briedi.TurnTo(player);
			if (e == GameLivingEvent.Interact)
			{
				if (quest != null)
				{
					if (quest.Step <= 2)
					{
						briedi.SayTo(player, "What? Who are you? What do you want? Can't you see I'm busy?");
					}
					else if (quest.Step == 3)
					{
						briedi.SayTo(player, "Ah, so my wayward son Dalikor has come seeking my help. Askefruer eh? Well, I suppose it could be worse. Listen youngun. You'll need to gather some mundane items for me if you want me to help you with this [mess].");
					}
					else if (quest.Step == 10)
					{
						briedi.SayTo(player, "Oh! You gave me such a fright! Next time don't sneak up on an old man, alright? Kids these days. Well, you're back, mouth agape. I assume you got my items? If so, why don't you give me the tattered shirt first?");
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
						case "mess":
							briedi.SayTo(player, "Here, I'll give you a [list]. Should be easy enough for you to find these.");
							SendEmoteMessage(player, "Master Briedi turns the scroll you gave him around and begins to scribble out some items on the other side.");
							break;

						case "list":
							briedi.SayTo(player, "Here. Now hurry up and get me those items. I don't have all day you know!");
							if (quest.Step == 3)
							{
								GiveItem(briedi, player, listBriedi);
								quest.Step = 4;
								briedi.SayTo(player, "Off we go!");
							}
							break;

						case "expedite":
							briedi.SayTo(player, "Off you go!");
							if (quest.Step == 13)
							{
								quest.TeleportTo(player, briedi, locationDalikor, 10);
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

		protected static void TalkToMasterBriediClone(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies
			//for the quest!		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(dalikor.CanGiveQuest(typeof (BeginningOfWar), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			BeginningOfWar quest = player.IsDoingQuest(typeof (BeginningOfWar)) as BeginningOfWar;

			if (e == GameLivingEvent.Interact)
			{
				//We check if the player is already doing the quest
				if (quest != null && quest.briediClone != null && quest.briediClone == sender)
				{
					quest.briediClone.TurnTo(player);

					if (quest.Step == 14)
					{
						briedi.SayTo(player, "Oh, it's you again, trying to scare me some more I see. I don't think I like you much.");
						briedi.SayTo(player, "Would you just keep listening to Dalikor? I don't have any of the answers, just all the magic!");

						quest.briediClone.MaxSpeedBase = player.MaxSpeedBase;
						quest.briediClone.Follow(player, 100, 3000);
						quest.Step = 15;
						if (player.PlayerGroup != null)
						{
							foreach (GamePlayer groupMember in player.PlayerGroup.GetPlayersInTheGroup())
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
						quest.briediClone.MaxSpeedBase = player.MaxSpeedBase;
						quest.briediClone.Follow(player, 100, 3000);
					}
					return;
				}
			}
		}

		protected virtual void ResetMasterBriedi()
		{
			if (briediClone != null && (briediClone.IsAlive || briediClone.ObjectState == GameObject.eObjectState.Active))
			{
				m_animSpellObjectQueue.Enqueue(briediClone);
				m_animSpellTeleportTimerQueue.Enqueue(new RegionTimer(briediClone, new RegionTimerCallback(MakeAnimEmoteSequence), 500));

				m_animEmoteObjectQueue.Enqueue(briediClone);
				m_animEmoteTeleportTimerQueue.Enqueue(new RegionTimer(briediClone, new RegionTimerCallback(MakeAnimEmoteSequence), 2500));
			}

			if (briediClone != null)
			{
				GameTimer castTimer = new RegionTimer(briediClone, new RegionTimerCallback(DeleteBriediClone), 3500);
			}
		}

		protected virtual int DeleteBriediClone(RegionTimer callingTimer)
		{
			if (briediClone != null)
			{
				GameEventMgr.RemoveHandler(briediClone, GameLivingEvent.Interact, new DOLEventHandler(TalkToMasterBriediClone));

				briediClone.Delete();
			}
			return 0;
		}

		protected virtual int TalkBriediClone(RegionTimer callingTimer)
		{
			if (Step == 17)
			{
				briediClone.SayTo(m_questPlayer, "Ah, what a well fought battle, don't you think Eeinken? Now look, you tell Dalikor that I went back home, alright? I don't fancy being out here in his part of the world. So don't forget to tell him.");
				briediClone.SayTo(m_questPlayer, "Off to Gotar I go!");

				ResetMasterBriedi();
				Step = 18;
			}
			return 0;
		}


		protected void CreateBriediClone()
		{
			if (briediClone == null)
			{
				briediClone = new GameNPC();
				briediClone.Name = briedi.Name;
				briediClone.Model = briedi.Model;
				briediClone.GuildName = briedi.GuildName;
				briediClone.Realm = briedi.Realm;
				briediClone.CurrentRegionID = locationBriediClone.RegionID;
				briediClone.Size = briedi.Size;
				briediClone.Level = 15; // to make the figthing against fairy sorceress a bit more dramatic :)

				briediClone.X = locationBriediClone.X + Util.Random(-150, 150);
				briediClone.Y = locationBriediClone.X + Util.Random(-150, 150);
				briediClone.Z = locationBriediClone.Y;
				briediClone.Heading = locationBriediClone.Heading;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 348);
				template.AddNPCEquipment(eInventorySlot.LegsArmor, 349);
				template.AddNPCEquipment(eInventorySlot.ArmsArmor, 350);
				template.AddNPCEquipment(eInventorySlot.HandsArmor, 351);
				template.AddNPCEquipment(eInventorySlot.FeetArmor, 352);
				template.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 640);
				briediClone.Inventory = template.CloseTemplate();
				briediClone.SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);

//				briediClone.AddNPCEquipment((byte) eEquipmentItems.TORSO, 348, 0, 0, 0);
//				briediClone.AddNPCEquipment((byte) eEquipmentItems.LEGS, 349, 0, 0, 0);
//				briediClone.AddNPCEquipment((byte) eEquipmentItems.ARMS, 350, 0, 0, 0);
//				briediClone.AddNPCEquipment((byte) eEquipmentItems.HAND, 351, 0, 0, 0);
//				briediClone.AddNPCEquipment((byte) eEquipmentItems.FEET, 352, 0, 0, 0);
//				briediClone.AddNPCEquipment((byte) eEquipmentItems.TWO_HANDED, 640, 0, 0, 0);

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 0;
				briediClone.SetOwnBrain(brain);

				briediClone.AddToWorld();

				GameEventMgr.AddHandler(briediClone, GameLivingEvent.Interact, new DOLEventHandler(TalkToMasterBriediClone));

				foreach (GamePlayer visPlayer in briediClone.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					visPlayer.Out.SendEmoteAnimation(briediClone, eEmote.Bind);
				}
			}
			else
			{
				TeleportTo(briediClone, briediClone, locationBriediClone);
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

				quest.ResetMasterBriedi();
			}
		}

		protected static void PlayerUseSlot(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = (GamePlayer) sender;
			if (player == null)
				return;

			BeginningOfWar quest = (BeginningOfWar) player.IsDoingQuest(typeof (BeginningOfWar));
			if (quest == null)
				return;

			UseSlotEventArgs uArgs = (UseSlotEventArgs) args;

			InventoryItem item = player.Inventory.GetItem((eInventorySlot)uArgs.Slot);
			if (item != null && item.Id_nb == listBriedi.Id_nb)
			{
				if (quest.Step >= 4 && quest.Step <= 8)
				{
					IList textList = new ArrayList();
					textList.Add("tattered shirt from escaped thrall");
					textList.Add("smiera-gatto's claw");
					textList.Add("coastal wolf blood");
					player.Out.SendCustomTextWindow("List of Master Briedi", textList);
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
					RemoveItem(player, listBriedi);
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
					quest.CreateBriediClone();
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
			if (player.HasFinishedQuest(typeof (StolenEggs)) == 0)
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
			if(dalikor.CanGiveQuest(typeof (BeginningOfWar), player)  <= 0)
				return;

			BeginningOfWar quest = player.IsDoingQuest(typeof (BeginningOfWar)) as BeginningOfWar;

			if (quest != null)
				return;

			if (response == 0x00)
			{
				dalikor.SayTo(player, "Oh well, if you change your mind, please come back!");
			}
			else
			{
				//Check if we can add the quest!
				if (!dalikor.GiveQuest(typeof (BeginningOfWar), player, 1))
					return;

				GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
				GameEventMgr.AddHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));

				dalikor.SayTo(player, "I thought you would. There's no adventure too dangerous for you, is there Eeinken? This task is simple enough. Take this scroll to Master Briedi in Gotar. I suggest you take a griffin so you can get there faster. Good luck.");

				GiveItem(dalikor, player, scrollBriedi);

				// dirty hack to keep step number same as in alb beginnWarQuest.
				quest = player.IsDoingQuest(typeof (BeginningOfWar)) as BeginningOfWar;
				if(quest != null) quest.Step = 2; 
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
				SendSystemMessage(player, "Good, no go out there and finish your work!");
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
						return "[Step #1] Listen to Dalikor.";
					case 2:
						return "[Step #2] Deliver the letter to Master Briedi in Gotar, near Fort Alta. Take the griffin in order to get there faster. Be sure to obtain a ticket to Gotar. Master Briedi can be found west of the entrance to Fort Alta.";
					case 3:
						return "[Step #3] Wait for Master Briedi to read the letter from Dalikor.";
					case 4:
						return "[Step #4] You must now READ Master Briedi's list. In order to do that, right click on the list in your inventory and type /use. Get the things on his list for him.";
					case 5:
						return "[Step #5] Find an escaped thrall. Slay him and take his tattered shirt.";
					case 6:
						return "[Step #6] Read the list to find out what the second item is.";
					case 7:
						return "[Step #7] Smiera-gattos wander all over the forests of Gotar. Slay one for its claw.";
					case 8:
						return "[Step #8] Read the list for the last time to find out what the final ingredient is.";
					case 9:
						return "[Step #9] Look around for a coastal wolf. Slay the beast and get its blood.";
					case 10:
						return "[Step #10] Return to Master Briedi near Fort Alta.";
					case 11:
						return "[Step #11] Now give Master Briedi the smiera-gatto claw.";
					case 12:
						return "[Step #12] Hand Master Briedi the last item, the coastal wolf blood.";
					case 13:
						return "[Step #13] Return to Dalikor at the guard tower near Mularn and let him know you've contacted Master Briedi and that he will be [assisting] with the Askefruer problem.";
					case 14:
						return "[Step #14] Listen to Dalikor give you the details behind his plan.";
					case 15:
						return "[Step #15] Help Master Briedi over to Dalikor. You can do this by right clicking on him.";
					case 16:
						return "[Step #16] Make your way north from Dalikor into the foothills. You will come to a small encampment. When you're there, defear the Askefruer leader.";
					case 17:
						return "[Step #17] When the fight is over, speak with Master Briedi.";
					case 18:
						return "[Step #18] Return to Dalikor at the guard tower near Mularn. Be sure to tell him that Master Briedi has [returned to Gotar].";
					case 19:
						return "[Step #19] Wait for Dalikor to reward you for your efforts om defeating the Askefruer army. If he forgets, ask him about the [reward].";

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
				if (gArgs.Target.Name == briedi.Name && gArgs.Item.Id_nb == scrollBriedi.Id_nb)
				{
					briedi.SayTo(m_questPlayer, "Hmph, what's this? A letter for me, eh? Well, let's see what it has to say.");
					RemoveItem(briedi, player, scrollBriedi);
					SendEmoteMessage(player, "Master Briedi unrolls the scroll and reads through it. When he is finished, he returns his attentions to you.");
					Step = 3;
					return;
				}
			}


			if (e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;

				if (gArgs.Target.Name == "escaped thrall" && (Step == 5 || Step == 4))
				{
					SendSystemMessage("You slay the escaped thrall and find a Tattered Shirt for Master Briedi.");
					GiveItem(gArgs.Target, player, tatteredShirt);
					Step = 6;
					return;
				}

				if (gArgs.Target.Name == "smiera-gatto" && (Step == 7 || Step == 6))
				{
					SendSystemMessage("You slay the Smiera-gatto and get it's claw for Master Briedi.");
					GiveItem(gArgs.Target, player, smieraGattoClaw);
					Step = 8;
					return;
				}

				if (gArgs.Target.Name == "coastal wolf" && (Step == 9 || Step == 8))
				{
					if (Step == 8)
					{
						SendSystemMessage(player, "The List disappears in a puff of blue smoke.");
						RemoveItem(player, listBriedi);
					}

					SendSystemMessage("You slay the wolf and retrieve some of its blood for Master Briedi.");
					GiveItem(gArgs.Target, player, coastalWolfBlood);

					Step = 10;
					return;
				}
			}

			if (e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;

				if (gArgs.Target.Name == briedi.Name)
				{
					if (Step == 10 && gArgs.Item.Id_nb == tatteredShirt.Id_nb)
					{
						briedi.SayTo(m_questPlayer, "Uh huh, yes...Alright, now give me that claw from those smiera-gattos.");
						RemoveItem(briedi, player, tatteredShirt);
						Step = 11;
						return;
					}
					else if (Step == 11 && gArgs.Item.Id_nb == smieraGattoClaw.Id_nb)
					{
						briedi.SayTo(m_questPlayer, "Yes, this is a claw alright. Now, for the coastal wolf blood. You better have gotten me enough of this.");
						RemoveItem(briedi, player, smieraGattoClaw);
						Step = 12;
						return;
					}
					else if (Step == 12 && gArgs.Item.Id_nb == coastalWolfBlood.Id_nb)
					{
						briedi.SayTo(m_questPlayer, "Well, it's a little less than I was hoping for, but it will do. Now listen kid. You go back to that no good son of mine and tell him I'll go ahead and help him with his little problem. In fact, let me [expedite] your journey for you.");
						RemoveItem(briedi, player, coastalWolfBlood);
						Step = 13;
						return;
					}
				}
			}

			if (Step == 16 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;

				if (gArgs.Target.Name == princessAiyr.Name)
				{
					SendSystemMessage("You slay the princess and take her head as proof.");
					GiveItem(gArgs.Target, player, princessAiyrHead);
					Step = 17;

					new RegionTimer(gArgs.Target, new RegionTimerCallback(TalkBriediClone), 7000);
					return;
				}
			}

			if (Step == 18 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == dalikor.Name && gArgs.Item.Id_nb == princessAiyrHead.Id_nb)
				{
					dalikor.SayTo(m_questPlayer, "Interesting. I shall present this to the council as well. I do so hope this is the end of our Askefruer problems, but somehow, I sense it is not. Nevertheless, I have here a [reward] for you from the council.");
					RemoveItem(dalikor, player, princessAiyrHead);
					Step = 19;
					return;
				}
			}
		}

		public override void AbortQuest()
		{
			ResetMasterBriedi();

			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, scrollBriedi, false);
			RemoveItem(m_questPlayer, listBriedi, false);
			RemoveItem(m_questPlayer, coastalWolfBlood, false);
			RemoveItem(m_questPlayer, tatteredShirt, false);
			RemoveItem(m_questPlayer, smieraGattoClaw, false);
			RemoveItem(m_questPlayer, princessAiyrHead, false);

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			// make sure to clean up, should be needed , but just to make certain
			ResetMasterBriedi();
			//Give reward to player here ...              
			m_questPlayer.GainExperience(507, 0, 0, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 8, Util.Random(50)), "You recieve {0} as a reward.");
			if (m_questPlayer.HasAbilityToUseItem(recruitsHelm))
				GiveItem(dalikor, m_questPlayer, recruitsHelm);
			else
				GiveItem(dalikor, m_questPlayer, recruitsCap);
			GiveItem(dalikor, m_questPlayer, recruitsRing);

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));

		}

	}
}
