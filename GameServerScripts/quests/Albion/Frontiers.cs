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
 *  1) Travel to loc=19105,26552 Camelot Hills to speak with Master Frederick.
 *  2) Go see Dragonfly Handler Colm at loc=13911,29125 Camelot Hills, hand him the Note for Colm from Master Frederick.
 *  3) Purchace a Dragonfly Ticket to Castle Sauvage (it's actually free), from Colm and hand him the ticket.
 *  4) Click on Master Visur, loc=35852,3511 Camelot Hills, Castle Sauvage, to be teleported to The Proving Grounds.
 *  5) Go to Scryer Alice is at loc=48045,24732 The Proving Grounds, and hand her the Ire Fairy Plans.
 *  6) Port back to Castle Sauvage and give Ulliam, loc=35468,5134 Camelot Hills, the Ticket to Cotswold from Scryer Alice.
 *  7) The horse will deposit you in front of Master Frederick, hand him the Translated Plans from Scryer Alice for your reward.
 */

using System;
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

	public class Frontiers : BaseFrederickQuest
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

		protected const string questTitle = "Frontiers";
		protected const int minimumLevel = 3;
		protected const int maximumLevel = 3;

		private static GameNPC masterFrederick = null;
		private static GameNPC masterVisur = null;
		private static GameNPC alice = null;
		private static GameStableMaster uliam = null;

		private static GameLocation locationAlice = null;
		private static GameLocation locationUliam = null;

		private static GameStableMaster colm = null;
		private static GameNPC dragonfly = null;

		private static ItemTemplate translatedPlans = null;
		private static ItemTemplate fairyPlans = null;
		private static ItemTemplate noteFormColm = null;
		private static ItemTemplate dragonflyTicket = null;
		private static ItemTemplate horseTicket = null;
//		private static MerchantItem dragonflyTicketM;

		private static ItemTemplate recruitsLegs = null;
		private static ItemTemplate recruitsPants = null;

		// marker wether alice has finised translation the fairy plans
		private bool aliceDone = false;

		/* We need to define the constructors from the base class here, else there might be problems
		 * when loading this quest...
		 */
		public Frontiers() : base()
		{
		}

		public Frontiers(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public Frontiers(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public Frontiers(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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

			#region DefineNPCs

			masterFrederick = GetMasterFrederick();


			GameNPC[] npcs = WorldMgr.GetNPCsByName("Master Visur", eRealm.Albion);
			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Master Visur, creating ...");
				masterVisur = new GameNPC();
				masterVisur.Model = 61;
				masterVisur.Name = "Master Visur";
				masterVisur.GuildName = "Part of " + questTitle + " Quest";
				masterVisur.Realm = eRealm.Albion;
				masterVisur.CurrentRegionID = 1;
				masterVisur.Size = 49;
				masterVisur.Level = 55;
				masterVisur.X = 585589;
				masterVisur.Y = 478396;
				masterVisur.Z = 3368;
				masterVisur.Heading = 56;
				masterVisur.MaxSpeedBase = 200;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 798);
				template.AddNPCEquipment(eInventorySlot.RightHandWeapon, 19);
				masterVisur.Inventory = template.CloseTemplate();
				masterVisur.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

//				masterVisur.AddNPCEquipment((byte) eEquipmentItems.TORSO, 798, 0, 0, 0);
//				masterVisur.AddNPCEquipment((byte) eEquipmentItems.RIGHT_HAND, 19, 0, 0, 0);

				masterVisur.EquipmentTemplateID = "3400843";

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					masterVisur.SaveIntoDatabase();
				masterVisur.AddToWorld();
			}
			else
				masterVisur = npcs[0];

			npcs = WorldMgr.GetNPCsByName("Scryer Alice", eRealm.Albion);
			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Alice, creating ...");
				alice = new GameNPC();
				alice.Model = 52;
				alice.Name = "Scryer Alice";
				alice.GuildName = "Part of " + questTitle + " Quest";
				alice.Realm = eRealm.Albion;
				alice.CurrentRegionID = 1;
				alice.Size = 51;
				alice.Level = 50;
				alice.X = 436598;
				alice.Y = 650425;
				alice.Z = 2448;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 81);
				template.AddNPCEquipment(eInventorySlot.LegsArmor, 82);
				template.AddNPCEquipment(eInventorySlot.FeetArmor, 84);
				template.AddNPCEquipment(eInventorySlot.Cloak, 91);
				template.AddNPCEquipment(eInventorySlot.RightHandWeapon, 3);
				alice.Inventory = template.CloseTemplate();
				alice.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

//				alice.AddNPCEquipment(Slot.TORSO, 81, 0, 0, 0);
//				alice.AddNPCEquipment(Slot.LEGS, 82, 0, 0, 0);
//				alice.AddNPCEquipment(Slot.FEET, 84, 0, 0, 0);
//				alice.AddNPCEquipment(Slot.CLOAK, 91, 0, 0, 0);
//				alice.AddNPCEquipment(Slot.RIGHTHAND, 3, 0, 0, 0);

				alice.Heading = 3766;
				alice.MaxSpeedBase = 200;
				alice.EquipmentTemplateID = "200276";
				alice.Flags = 18;

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 0;
				alice.SetOwnBrain(brain);

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					alice.SaveIntoDatabase();
				alice.AddToWorld();
			}
			else
				alice = npcs[0];

			Point2D point = alice.GetPointFromHeading( alice.Heading, 30 );
			locationAlice = new GameLocation(alice.CurrentZone.Description, alice.CurrentRegionID, point.X, point.Y, alice.Z);

			dragonflyTicket = CreateTicketTo("Castle Sauvage", "hs_src_castlesauvage");
			horseTicket = CreateTicketTo("Camelot Hills", "hs_src_camelothills");

			npcs = (GameNPC[]) WorldMgr.GetObjectsByName("Dragonfly Handler Colm", eRealm.Albion, typeof (GameStableMaster));
			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Dragonfly Handler Colm, creating ...");
				colm = new GameStableMaster();
				colm.Model = 78;
				colm.Name = "Dragonfly Handler Colm";
				colm.GuildName = "Stable Master";
				colm.Realm = eRealm.Albion;
				colm.CurrentRegionID = 1;
				colm.Size = 51;
				colm.Level = 50;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 81, 10);
				template.AddNPCEquipment(eInventorySlot.LegsArmor, 82, 10);
				template.AddNPCEquipment(eInventorySlot.FeetArmor, 84, 10);
				template.AddNPCEquipment(eInventorySlot.Cloak, 57, 32);
				colm.Inventory = template.CloseTemplate();

//				colm.AddNPCEquipment(Slot.TORSO, 81, 10, 0, 0);
//				colm.AddNPCEquipment(Slot.LEGS, 82, 10, 0, 0);
//				colm.AddNPCEquipment(Slot.FEET, 84, 10, 0, 0);
//				colm.AddNPCEquipment(Slot.CLOAK, 57, 32, 0, 0);

				colm.X = 562775;
				colm.Y = 512453;
				colm.Z = 2438;
				colm.Heading = 158;
				colm.MaxSpeedBase = 200;

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 0;
				colm.SetOwnBrain(brain);

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					colm.SaveIntoDatabase();
				colm.AddToWorld();
			}
			else
			{
				colm = npcs[0] as GameStableMaster;
			}


			colm.TradeItems = new MerchantTradeItems(null);
			if (!colm.TradeItems.AddTradeItem(0, eMerchantWindowSlot.FirstEmptyInPage, dragonflyTicket))
				if (log.IsWarnEnabled)
					log.Warn("dragonflyTicket not added");


			foreach (GameNPC npc in colm.GetNPCsInRadius(400))
			{
				if (npc.Name == "dragonfly hatchling")
				{
					dragonfly = npc;
					break;
				}
			}
			if (dragonfly == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Dragon Fly, creating ...");
				dragonfly = new GameNPC();
				dragonfly.Model = 1207;
				dragonfly.Name = "dragonfly hatchling";
				dragonfly.GuildName = "Part of " + questTitle + " Quest";
				dragonfly.Realm = eRealm.None;
				dragonfly.CurrentRegionID = 1;
				dragonfly.Size = 25;
				dragonfly.Level = 31;
				dragonfly.X = colm.X + 80;
				dragonfly.Y = colm.Y + 100;
				dragonfly.Z = colm.Z;

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 0;
				dragonfly.SetOwnBrain(brain);

				dragonfly.Heading = 2434;
				dragonfly.MaxSpeedBase = 400;
				//dragonfly.EquipmentTemplateID = 200276;

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database

				if (SAVE_INTO_DATABASE)
					dragonfly.SaveIntoDatabase();
				dragonfly.AddToWorld();
			}

			npcs = (GameNPC[]) WorldMgr.GetObjectsByName("Uliam", eRealm.Albion, typeof (GameStableMaster));
			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Uliam, creating ...");
				uliam = new GameStableMaster();
				uliam.Model = 52;
				uliam.Name = "Uliam";
				uliam.GuildName = "Stable Master";
				uliam.Realm = eRealm.Albion;
				uliam.CurrentRegionID = 1;
				uliam.Size = 51;
				uliam.Level = 50;
				uliam.X = 585609;
				uliam.Y = 478980;
				uliam.Z = 2183;
				uliam.Heading = 93;
				uliam.MaxSpeedBase = 200;

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 0;
				uliam.SetOwnBrain(brain);

				//ulliam.EquipmentTemplateID = 200276;

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					uliam.SaveIntoDatabase();

				uliam.AddToWorld();
			}
			else
				uliam = npcs[0] as GameStableMaster;

			Point2D uliamloc = uliam.GetPointFromHeading( uliam.Heading, 30 );
			locationUliam = new GameLocation(uliam.CurrentZone.Description, uliam.CurrentRegionID, uliam.X, uliam.Y, uliam.Z);

			/*
            foreach (GameNPC npc in WorldMgr.GetNPCsCloseToObject(uliam, 400))
            {
                if (npc.Name == "horse")
                {
                    horse = npc;
                    break;
                }
            }
            
            if (horse == null)
            {
                if(Log.IsWarnEnabled)
									Log.Warn("Could not find Horse near Uliam, creating ...");
                horse = new GameNPC();
                horse.Model = 450; // //819;
                horse.Name = "horse";
                horse.GuildName = "Part of " + questTitle + " Quest";
                horse.Realm = (byte)eRealm.None;
                horse.CurrentRegionID = 1;
                horse.Size = 63;
                horse.Level = 55;
                horse.X = uliam.X + 80;
                horse.Y = uliam.Y + 130;
                horse.Z = uliam.Z;
                horse.Heading = 93;
                                
                horse.AggroLevel = 0;
                horse.AggroRange = 0;
                //horse.EquipmentTemplateID = 200276;

                //You don't have to store the created mob in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    horse.SaveIntoDatabase();
                horse.AddToWorld();
            }
			 */

			#endregion

			#region DefineItems

			// item db check
			noteFormColm = GameServer.Database.FindObjectByKey<ItemTemplate>("colms_note");
			if (noteFormColm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Colm's Note, creating it ...");
				noteFormColm = new ItemTemplate();
				noteFormColm.Name = "Colm's Note";

				noteFormColm.Weight = 3;
				noteFormColm.Model = 498;

				noteFormColm.Object_Type = (int) eObjectType.GenericItem;

				noteFormColm.Id_nb = "colms_note";
				noteFormColm.IsPickable = true;
				noteFormColm.IsDropable = false;


				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				
					GameServer.Database.AddObject(noteFormColm);
			}

			// item db check
			fairyPlans = GameServer.Database.FindObjectByKey<ItemTemplate>("ire_fairy_plans");
			if (fairyPlans == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Ire Fairy Plans, creating it ...");
				fairyPlans = new ItemTemplate();
				fairyPlans.Name = "Ire Fairy Plans";

				fairyPlans.Weight = 3;
				fairyPlans.Model = 498;

				fairyPlans.Object_Type = (int) eObjectType.GenericItem;

				fairyPlans.Id_nb = "ire_fairy_plans";
				fairyPlans.IsPickable = true;
				fairyPlans.IsDropable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				
					GameServer.Database.AddObject(fairyPlans);
			}

			translatedPlans = GameServer.Database.FindObjectByKey<ItemTemplate>("translated_ire_fairy_plans");
			if (translatedPlans == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Translated Ire Fairy Plans, creating it ...");
				translatedPlans = new ItemTemplate();
				translatedPlans.Name = "Translated Ire Fairy Plans";

				translatedPlans.Weight = 3;
				translatedPlans.Model = 498;

				translatedPlans.Object_Type = (int) eObjectType.GenericItem;

				translatedPlans.Id_nb = "translated_ire_fairy_plans";
				translatedPlans.IsPickable = true;
				translatedPlans.IsDropable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				
					GameServer.Database.AddObject(translatedPlans);
			}

			// item db check
			recruitsLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("recruits_studded_legs");
			if (recruitsLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Recruit's Studded Legs, creating it ...");
				recruitsLegs = new ItemTemplate();
				recruitsLegs.Name = "Recruit's Studded Legs";
				recruitsLegs.Level = 7;

				recruitsLegs.Weight = 42;
				recruitsLegs.Model = 82; // Studded Legs

				recruitsLegs.DPS_AF = 10; // Armour
				recruitsLegs.SPD_ABS = 19; // Absorption

				recruitsLegs.Object_Type = (int) eObjectType.Studded;
				recruitsLegs.Item_Type = (int) eEquipmentItems.LEGS;
				recruitsLegs.Id_nb = "recruits_studded_legs";
				recruitsLegs.Price = Money.GetMoney(0,0,0,10,0);
				recruitsLegs.IsPickable = true;
				recruitsLegs.IsDropable = true;
				recruitsLegs.Color = 9; // red leather

				recruitsLegs.Bonus = 5; // default bonus

				recruitsLegs.Bonus1 = 10;
				recruitsLegs.Bonus1Type = (int) eProperty.MaxHealth; // hit


				recruitsLegs.Bonus2 = 2;
				recruitsLegs.Bonus2Type = (int) eResist.Slash;

				recruitsLegs.Bonus3 = 1;
				recruitsLegs.Bonus3Type = (int) eResist.Cold;

				recruitsLegs.Quality = 100;
				recruitsLegs.Condition = 1000;
				recruitsLegs.MaxCondition = 1000;
				recruitsLegs.Durability = 1000;
				recruitsLegs.MaxDurability = 1000;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				
					GameServer.Database.AddObject(recruitsLegs);
			}

			// item db check
			recruitsPants = GameServer.Database.FindObjectByKey<ItemTemplate>("recruits_quilted_pants");
			if (recruitsPants == null)
			{
				recruitsPants = new ItemTemplate();
				recruitsPants.Name = "Recruit's Quilted Pants";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsPants.Name + ", creating it ...");
				recruitsPants.Level = 7;

				recruitsPants.Weight = 14;
				recruitsPants.Model = 152; // cloth Legs

				recruitsPants.DPS_AF = 5; // Armour
				recruitsPants.SPD_ABS = 0; // Absorption

				recruitsPants.Object_Type = (int) eObjectType.Cloth;
				recruitsPants.Item_Type = (int) eEquipmentItems.LEGS;
				recruitsPants.Id_nb = "recruits_quilted_pants";
				recruitsPants.Price = Money.GetMoney(0,0,0,10,0);
				recruitsPants.IsPickable = true;
				recruitsPants.IsDropable = true;
				recruitsPants.Color = 17; // red leather

				recruitsPants.Bonus = 5; // default bonus

				recruitsPants.Bonus1 = 12;
				recruitsPants.Bonus1Type = (int) eProperty.MaxHealth; // hit


				recruitsPants.Bonus2 = 2;
				recruitsPants.Bonus2Type = (int) eResist.Slash;

				recruitsPants.Bonus3 = 1;
				recruitsPants.Bonus3Type = (int) eResist.Cold;

				recruitsPants.Quality = 100;
				recruitsPants.Condition = 1000;
				recruitsPants.MaxCondition = 1000;
				recruitsPants.Durability = 1000;
				recruitsPants.MaxDurability = 1000;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				
					GameServer.Database.AddObject(recruitsPants);
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

			GameEventMgr.AddHandler(masterVisur, GameObjectEvent.Interact, new DOLEventHandler(TalkToMasterVisur));

			GameEventMgr.AddHandler(colm, GameObjectEvent.Interact, new DOLEventHandler(TalkToColm));

			GameEventMgr.AddHandler(alice, GameObjectEvent.Interact, new DOLEventHandler(TalkToAlice));
			GameEventMgr.AddHandler(alice, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToAlice));

			/* Now we bring to masterFrederick the possibility to give this quest to players */
			masterFrederick.AddQuestToGive(typeof (Frontiers));

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

			GameEventMgr.RemoveHandler(masterFrederick, GameObjectEvent.Interact, new DOLEventHandler(TalkToMasterFrederick));
			GameEventMgr.RemoveHandler(masterFrederick, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterFrederick));

			GameEventMgr.RemoveHandler(masterVisur, GameObjectEvent.Interact, new DOLEventHandler(TalkToMasterVisur));

			GameEventMgr.RemoveHandler(colm, GameObjectEvent.Interact, new DOLEventHandler(TalkToColm));

			GameEventMgr.RemoveHandler(alice, GameObjectEvent.Interact, new DOLEventHandler(TalkToAlice));
			GameEventMgr.RemoveHandler(alice, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToAlice));

			/* Now we remove to masterFrederick the possibility to give this quest to players */
			masterFrederick.RemoveQuestToGive(typeof (Frontiers));
		}

		protected static void PlayerEnterWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			Frontiers quest = player.IsDoingQuest(typeof (Frontiers)) as Frontiers;
			if (quest != null)
			{
				// if player reenters during step 4 alice will have finished translation anyway...
				if (quest.Step == 4)
				{
					quest.aliceDone = true;
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
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(masterFrederick.CanGiveQuest(typeof (Frontiers), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Frontiers quest = player.IsDoingQuest(typeof (Frontiers)) as Frontiers;

			//Did the player rightclick on NPC?
			masterFrederick.TurnTo(player);
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...
					masterFrederick.SayTo(player, "Ah, Vinde. Yes, I've been waiting for you. I have here a parchment that I need taken to the Caer Witrin. I know it sounds a little overwhelming, but I'm sure you can handle it. Will you take this to the [Caer Witrin] for me?");
					return;
				}
				else
				{
					if (quest.Step == 1)
					{
						masterFrederick.SayTo(player, "Alright, there is no time to lose. Take this parchment to my good friend Colm. He is a dragonfly handler here in Cotswold. He will get you set up on a dragonfly to make the trip to Castle Sauvage. Hurry now, do not tarry.");
					}
					else if (quest.Step == 5)
					{
						masterFrederick.SayTo(player, "Welcome back Vinde. I take it you went to the Caer Witrin? Tell me, did Scryer Alice translate the parchment for me?");
					}
					else if (quest.Step == 6)
					{
						masterFrederick.SayTo(player, "You have done a great service to Cotswold this day my friend. I am truly glad that you have chosen this path. I have something here for your [efforts].");
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
							//If the player offered his "help", we send the quest dialog now!
						case "Caer Witrin":
							player.Out.SendQuestSubscribeCommand(masterFrederick, QuestMgr.GetIDForQuestType(typeof(Frontiers)), "Will you take this package to Scryer Alice for Master Frederick?");
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "efforts":
							masterFrederick.SayTo(player, "I know they aren't much, but they will protect you from the wilds of Albion, at least for a little while. Thank you again for your service to Albion. I have another mission, if you're interested. Speak with me further for more information.");
							if (quest.Step == 6)
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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(Frontiers)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

		protected static void TalkToColm(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(masterFrederick.CanGiveQuest(typeof (Frontiers), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Frontiers quest = player.IsDoingQuest(typeof (Frontiers)) as Frontiers;
			colm.TurnTo(player);
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					if (quest.Step == 1)
					{
						colm.SayTo(player, "Greetings my friend. How may I help you today?");
					}
				}
			}
		}

		protected static void TalkToAlice(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(masterFrederick.CanGiveQuest(typeof (Frontiers), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Frontiers quest = player.IsDoingQuest(typeof (Frontiers)) as Frontiers;

			alice.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					//Player is not doing the quest...
					if (quest.Step == 3 || quest.Step == 2)
					{
						alice.SayTo(player, "Welcome! I am Alice, a scout by profession, but a scryer by hobby. How may I help you today?");
					}
					else if (quest.Step == 4)
					{
						if (quest.aliceDone)
						{
							alice.SayTo(player, "Ah, yes, I am now [done] with the translation.");
						}
						else
						{
							alice.SayTo(player, "Wait a minute... I'm almost finished.");
						}
					}
					return;
				}

			}
			// The player whispered to NPC (clicked on the text inside the [])
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				if (quest != null)
				{
					//Do some small talk :)
					switch (wArgs.Text)
					{
						case "done":
							alice.SayTo(player, "Here you are my intreprid young traveler. It has all been translated now. Take it back to Master Frederick. I hope I was able to help today. Come back and visit me soon!");
							alice.SayTo(player, "Oh and take this horse ticket and give it to Uliam at Castle Sauvage he will bring you back home safely.");
							if (quest.Step == 4)
							{
								GiveItem(alice, player, translatedPlans);
								GiveItem(alice, player, horseTicket);
								quest.Step = 5;

								quest.TeleportTo(player, alice, locationUliam, 50);
							}
							break;
					}
				}
			}
		}

		protected static void TalkToMasterVisur(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(masterFrederick.CanGiveQuest(typeof (Frontiers), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Frontiers quest = player.IsDoingQuest(typeof (Frontiers)) as Frontiers;

			masterVisur.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null && quest.Step == 2)
				{
					masterVisur.SayTo(player, "From sodden ground to the glow of the moon, let each vessel in this circle depart to lands now lost from the light of our fair Camelot!");
					quest.Step = 3;

					quest.TeleportTo(player, masterVisur, locationAlice, 30);
					return;
				}

				return;
			}

		}

		/// <summary>
		/// This method checks if a player qualifies for this quest
		/// </summary>
		/// <returns>true if qualified, false if not</returns>
		public override bool CheckQuestQualification(GamePlayer player)
		{
			// if the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(typeof (Frontiers)) != null)
				return true;

			// This checks below are only performed is player isn't doing quest already
			if (!CheckPartAccessible(player, typeof (Frontiers)))
				return false;

			if (player.Level < minimumLevel || player.Level > maximumLevel)
				return false;

			return true;
		}


		/* This is our callback hook that will be called when the player clicks
		 * on any button in the quest offer dialog. We check if he accepts or
		 * declines here...
		 */

		private static void CheckPlayerAbortQuest(GamePlayer player, byte response)
		{
			Frontiers quest = player.IsDoingQuest(typeof (Frontiers)) as Frontiers;

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

		/* This is our callback hook that will be called when the player clicks
		 * on any button in the quest offer dialog. We check if he accepts or
		 * declines here...
		 */

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			//We recheck the qualification, because we don't talk to players
			//who are not doing the quest
			if(masterFrederick.CanGiveQuest(typeof (Frontiers), player)  <= 0)
				return;

			Frontiers quest = player.IsDoingQuest(typeof (Frontiers)) as Frontiers;

			if (quest != null)
				return;

			if (response == 0x00)
			{
				SendReply(player, "Oh well, if you change your mind, please come back!");
			}
			else
			{
				//Check if we can add the quest!
				if (!masterFrederick.GiveQuest(typeof (Frontiers), player, 1))
					return;

				masterFrederick.SayTo(player, "Alright, there is no time to lose. Take this parchment to my good friend Colm. He is a dragonfly handler here in Cotswold. He will get you set up on a dragonfly to make the trip to Castle Sauvage. Hurry now, do not tarry. And don't forget to give the parchment to Colm!");

				GiveItem(masterFrederick, player, noteFormColm);
				GiveItem(masterFrederick, player, fairyPlans);
				player.AddMoney(Money.GetMoney(0, 0, 0, 6, 0), "You recieve {0} for the ride to Castle Sauvage");
			}
		}

		protected virtual int AliceTranslation(RegionTimer callingTimer)
		{
			m_questPlayer.Out.SendEmoteAnimation(alice, eEmote.Yes);
			aliceDone = true;
			return 0;
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
						return "[Step #1] You must deliver the package Master Frederick gave you to Scryer Alice in the Frontiers. First, take the scroll to Dragonfly Handler Colm to him in Cotswold.";
					case 2:
						return "[Step #2] Take the dragonfly to Castle Sauvage. When you are there, speak with the Master Visur who will then teleport you to Caer Witrin.";
					case 3:
						return "[Step #3] Give the plans to Scryer Alice.";
					case 4:
						return "[Step #4] Wait for Scryer Alice to finish translating the plans. If she stops speaking with you, ask her if she is done with her translations.";
					case 5:
						return "[Step #5] Take the translated plans back to Master Frederick. Alice has give you a horse ticket so you can get home faster. Give it to Uliam.";
					case 6:
						return "[Step #6] Wait for Master Frederick to finish reading the translated plans.";
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player==null || player.IsDoingQuest(typeof (Frontiers)) == null)
				return;

			if (Step == 1 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == colm.Name && gArgs.Item.Id_nb == noteFormColm.Id_nb)
				{
					RemoveItem(colm, player, noteFormColm);

					colm.TurnTo(m_questPlayer);
					colm.SayTo(m_questPlayer, "Ah, from Master Frederick. Let's see what he says. Ah, I am to give you transportation to Castle Sauvage. No problem. All you need to do is purchase a ticket from my store.");
					m_questPlayer.Out.SendEmoteAnimation(masterFrederick, eEmote.Ponder);

					Step = 2;
					return;
				}
			}
			else if ((Step == 3 || Step == 2) && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == alice.Name && gArgs.Item.Id_nb == fairyPlans.Id_nb)
				{
					RemoveItem(alice, player, fairyPlans);

					alice.TurnTo(m_questPlayer);
					alice.SayTo(m_questPlayer, "Hmm...What's this now? A letter? For me? Interesting. Ah, I see it is from Master Frederick, something about plans written in fairy. I can translate this if you can wait just a few moments.");
					m_questPlayer.Out.SendEmoteAnimation(alice, eEmote.Ponder);

					new RegionTimer(alice, new RegionTimerCallback(AliceTranslation), 30000);

					Step = 4;
					return;
				}
			}
			else if (Step == 5 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == masterFrederick.Name && gArgs.Item.Id_nb == translatedPlans.Id_nb)
				{
					RemoveItem(masterFrederick, player, translatedPlans);

					masterFrederick.TurnTo(m_questPlayer);
					masterFrederick.SayTo(m_questPlayer, "Excellent! Let me just read this over for a moment.");
					m_questPlayer.Out.SendEmoteAnimation(masterFrederick, eEmote.Ponder);

					Step = 6;
					return;
				}
			}

		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			if (Step < 3 && m_questPlayer.Inventory.GetFirstItemByID(dragonflyTicket.Id_nb, eInventorySlot.Min_Inv, eInventorySlot.Max_Inv) == null)
			{
				m_questPlayer.RemoveMoney(Money.GetMoney(0, 0, 0, 6, 0), null);
			}

			RemoveItem(m_questPlayer, dragonflyTicket, false);
			RemoveItem(m_questPlayer, fairyPlans, false);
			RemoveItem(m_questPlayer, horseTicket, false);
			RemoveItem(m_questPlayer, noteFormColm, false);
			RemoveItem(m_questPlayer, translatedPlans, false);

		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			//Give reward to player here ...
			if (m_questPlayer.HasAbilityToUseItem(recruitsLegs))
				GiveItem(masterFrederick, m_questPlayer, recruitsLegs);
			else
				GiveItem(masterFrederick, m_questPlayer, recruitsPants);

			m_questPlayer.GainExperience(GameLiving.eXPSource.Quest, 240, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 5, Util.Random(50)), "You recieve {0} as a reward.");

		}

	}
}
