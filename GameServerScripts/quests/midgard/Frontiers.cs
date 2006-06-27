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
 * 1) Travel to loc=41211,50221 Vale of Mularn to speak with Dalikor. 
 * 2) Go see Griffin Handler Njiedi at loc=55561,58225 Vale of Mularn, hand him the Scroll for Griffin Handler Njiedi from Dalikor. 
 * 3) Purchase a Ticket to Svasud Faste (it's actually free), from Njiedi and hand him the ticket. 
 * 4) Click on Stor Gothi Annark, loc=10796,1259 Vale of Mularn, Svasud Faste, to be teleported to The Proving Grounds. 
 * 5) Go to Scryer Idora is at loc=33697,49656 The Proving Grounds, and hand her the Askefruer Plans. 
 * 6) Port back to Svasud Faste and give Vorgar, loc=10660,3437 Vale of Mularn, the ticket to Mularn from Scryer Idora. 
 * 7) The horse will deposit you in front of Dalikor, hand him the Translated Plans from Scryer Idora for your reward.
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

namespace DOL.GS.Quests.Midgard
{
	/* The first thing we do, is to declare the class we create
	 * as Quest. To do this, we derive from the abstract class
	 * AbstractQuest
	 * 	 
	 */

	public class Frontiers : BaseDalikorQuest
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

		protected const string questTitle = "Frontiers (Mid)";
		protected const int minimumLevel = 3;
		protected const int maximumLevel = 3;

		private static GameNPC dalikor = null;
		private static GameNPC annark = null;
		private static GameNPC idora = null;
		private static GameStableMaster vorgar = null;

		private static GameLocation locationIdora = null;
		private static GameLocation locationVorgar = null;

		private static GameStableMaster njiedi = null;
		private static GameNPC griffin = null;

		private static ItemTemplate translatedPlans = null;
		private static ItemTemplate askefruerPlans = null;
		private static ItemTemplate noteForNjiedi = null;
		private static ItemTemplate ticketToSvasudFaste = null;
		private static ItemTemplate ticketToMularn = null;
//		private static MerchantItem griffinTicketM;

		private static ItemTemplate recruitsLegs = null;
		private static ItemTemplate recruitsPants = null;

		// marker wether alice has finised translation the fairy plans
		private bool idoraDone = false;

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

			#region DefineNPCs

			dalikor = GetDalikor();


			GameNPC[] npcs = WorldMgr.GetNPCsByName("Stor Gothi Annark", eRealm.Midgard);
			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Stor Gothi Annark, creating ...");
				annark = new GameMob();
				annark.Model = 215;
				annark.Name = "Stor Gothi Annark";
				annark.GuildName = "Part of " + questTitle + " Quest";
				annark.Realm = (byte) eRealm.Midgard;
				annark.CurrentRegionID = 100;
				annark.Size = 51;
				annark.Level = 66;
				annark.X = 765357;
				annark.Y = 668790;
				annark.Z = 5759;
				annark.Heading = 7711;

				//annark.AddNPCEquipment((byte)eEquipmentItems.TORSO, 798, 0, 0, 0);
				//annark.AddNPCEquipment((byte)eEquipmentItems.RIGHT_HAND, 19, 0, 0, 0);

				annark.EquipmentTemplateID = "5100090";

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					annark.SaveIntoDatabase();
				annark.AddToWorld();
			}
			else
				annark = npcs[0];

			npcs = WorldMgr.GetNPCsByName("Scryer Idora", eRealm.Midgard);
			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Scryer Idora, creating ...");
				idora = new GameMob();
				idora.Model = 227;
				idora.Name = "Scryer Idora";
				idora.GuildName = "Part of " + questTitle + " Quest";
				idora.Realm = (byte) eRealm.Midgard;
				idora.CurrentRegionID = 234;
				idora.Size = 52;
				idora.Level = 50;
				idora.X = 558081;
				idora.Y = 573988;
				idora.Z = 8640;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 81);
				template.AddNPCEquipment(eInventorySlot.LegsArmor, 82);
				template.AddNPCEquipment(eInventorySlot.FeetArmor, 84);
				template.AddNPCEquipment(eInventorySlot.Cloak, 91);
				template.AddNPCEquipment(eInventorySlot.RightHandWeapon, 3);
				idora.Inventory = template.CloseTemplate();
				idora.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

//				idora.AddNPCEquipment(Slot.TORSO, 81, 0, 0, 0);
//				idora.AddNPCEquipment(Slot.LEGS, 82, 0, 0, 0);
//				idora.AddNPCEquipment(Slot.FEET, 84, 0, 0, 0);
//				idora.AddNPCEquipment(Slot.CLOAK, 91, 0, 0, 0);
//				idora.AddNPCEquipment(Slot.RIGHTHAND, 3, 0, 0, 0);

				idora.Heading = 1558;
				idora.MaxSpeedBase = 200;
				idora.EquipmentTemplateID = "200292";

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 500;
				idora.SetOwnBrain(brain);

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					idora.SaveIntoDatabase();
				idora.AddToWorld();
			}
			else
				idora = npcs[0];

			int tmpX, tmpY;
			idora.GetSpotFromHeading(30, out tmpX, out tmpY);
			locationIdora = new GameLocation(idora.CurrentZone.Description, idora.CurrentRegionID, (int) tmpX, (int) tmpY, idora.Z);

			ticketToSvasudFaste = CreateTicketTo("Svasud Faste");
			ticketToMularn = CreateTicketTo("Mularn");

			npcs = (GameNPC[]) WorldMgr.GetObjectsByName("Griffin Handler Njiedi", eRealm.Midgard, typeof (GameStableMaster));
			if (npcs.Length == 0)
			{
				njiedi = new GameStableMaster();
				njiedi.Model = 158;
				njiedi.Name = "Griffin Handler Njiedi";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + njiedi.Name + ", creating ...");
				njiedi.GuildName = "Stable Master";
				njiedi.Realm = (byte) eRealm.Midgard;
				njiedi.CurrentRegionID = 100;
				njiedi.Size = 51;
				njiedi.Level = 50;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 81, 10);
				template.AddNPCEquipment(eInventorySlot.LegsArmor, 82, 10);
				template.AddNPCEquipment(eInventorySlot.FeetArmor, 84, 10);
				template.AddNPCEquipment(eInventorySlot.Cloak, 57, 32);
				njiedi.Inventory = template.CloseTemplate();

//				njiedi.AddNPCEquipment(Slot.TORSO, 81, 10, 0, 0);
//				njiedi.AddNPCEquipment(Slot.LEGS, 82, 10, 0, 0);
//				njiedi.AddNPCEquipment(Slot.FEET, 84, 10, 0, 0);
//				njiedi.AddNPCEquipment(Slot.CLOAK, 57, 32, 0, 0);

				njiedi.X = GameLocation.ConvertLocalXToGlobalX(55561, 100);
				njiedi.Y = GameLocation.ConvertLocalYToGlobalY(58225, 100);
				njiedi.Z = 5005;
				njiedi.Heading = 126;

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 0;
				njiedi.SetOwnBrain(brain);

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					njiedi.SaveIntoDatabase();
				njiedi.AddToWorld();
			}
			else
			{
				njiedi = npcs[0] as GameStableMaster;
			}


			njiedi.TradeItems = new MerchantTradeItems(null);
			if (!njiedi.TradeItems.AddTradeItem(1, eMerchantWindowSlot.FirstEmptyInPage, ticketToSvasudFaste))
				if (log.IsWarnEnabled)
					log.Warn("ticketToSvasudFaste not added");


			foreach (GameNPC npc in njiedi.GetNPCsInRadius(400))
			{
                if (npc.Name == "Gryphon")
				{
					griffin = npc;
					break;
				}
			}
			if (griffin == null)
			{
				griffin = new GameMob();
				griffin.Model = 1236; // //819;
				griffin.Name = "Gryphon";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + griffin.Name + ", creating ...");
				griffin.GuildName = "Part of " + questTitle + " Quest";
				griffin.Realm = (byte) eRealm.Midgard;
				griffin.CurrentRegionID = njiedi.CurrentRegionID;
				griffin.Size = 50;
				griffin.Level = 50;
				griffin.X = njiedi.X + 80;
				griffin.Y = njiedi.Y + 100;
				griffin.Z = njiedi.Z;

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 0;
				griffin.SetOwnBrain(brain);

				griffin.Heading = 93;
				griffin.MaxSpeedBase = 400;
				//dragonfly.EquipmentTemplateID = 200276;                

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database

                if (SAVE_INTO_DATABASE)
                {
                    griffin.SaveIntoDatabase();
                }
				griffin.AddToWorld();
			}

			npcs = (GameNPC[]) WorldMgr.GetObjectsByName("Vorgar", eRealm.Midgard, typeof (GameStableMaster));
			if (npcs.Length == 0)
			{
				vorgar = new GameStableMaster();
				vorgar.Model = 52;
				vorgar.Name = "Vorgar";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + vorgar.Name + ", creating ...");
				vorgar.GuildName = "Stable Master";
				vorgar.Realm = (byte) eRealm.Midgard;
				vorgar.CurrentRegionID = 100;
				vorgar.Size = 51;
				vorgar.Level = 50;
				vorgar.X = GameLocation.ConvertLocalXToGlobalX(10660, 100);
				vorgar.Y = GameLocation.ConvertLocalYToGlobalY(3437, 100);
				vorgar.Z = 5717;
				vorgar.Heading = 327;
				vorgar.MaxSpeedBase = 200;

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 0;
				vorgar.SetOwnBrain(brain);

				//ulliam.EquipmentTemplateID = 200276;                

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                {
                    vorgar.SaveIntoDatabase();
                }

				vorgar.AddToWorld();
			}
			else
				vorgar = npcs[0] as GameStableMaster;

			vorgar.GetSpotFromHeading(30, out tmpX, out tmpY);
			locationVorgar = new GameLocation(vorgar.CurrentZone.Description, vorgar.CurrentRegionID, (int) tmpX, (int) tmpY, vorgar.Z);

			#endregion

			#region DefineItems

			// item db check
			noteForNjiedi = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "njiedi_note");
			if (noteForNjiedi == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Njiedi's Note, creating it ...");
				noteForNjiedi = new ItemTemplate();
				noteForNjiedi.Name = "Njiedi's Note";

				noteForNjiedi.Weight = 3;
				noteForNjiedi.Model = 498;

				noteForNjiedi.Object_Type = (int) eObjectType.GenericItem;

				noteForNjiedi.Id_nb = "njiedi_note";
				noteForNjiedi.IsPickable = true;
				noteForNjiedi.IsDropable = false;


				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(noteForNjiedi);
			}

			// item db check
			askefruerPlans = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "askefruer_plans");
			if (askefruerPlans == null)
			{
				askefruerPlans = new ItemTemplate();
				askefruerPlans.Name = "Askefruer Plans";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + askefruerPlans.Name + ", creating it ...");

				askefruerPlans.Weight = 3;
				askefruerPlans.Model = 498;

				askefruerPlans.Object_Type = (int) eObjectType.GenericItem;

				askefruerPlans.Id_nb = "askefruer_plans";
				askefruerPlans.IsPickable = true;
				askefruerPlans.IsDropable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(askefruerPlans);
			}

			translatedPlans = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "translated_askefruer_plans");
			if (translatedPlans == null)
			{
				translatedPlans = new ItemTemplate();
				translatedPlans.Name = "Translated Askefruer Plans";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + translatedPlans.Name + ", creating it ...");

				translatedPlans.Weight = 3;
				translatedPlans.Model = 498;

				translatedPlans.Object_Type = (int) eObjectType.GenericItem;

				translatedPlans.Id_nb = "translated_askefruer_plans";
				translatedPlans.IsPickable = true;
				translatedPlans.IsDropable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(translatedPlans);
			}

			// item db check
			recruitsLegs = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "recruits_studded_legs_mid");
			if (recruitsLegs == null)
			{
				recruitsLegs = new ItemTemplate();
				recruitsLegs.Name = "Recruit's Studded Legs (Mid)";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsLegs.Name + ", creating it ...");
				recruitsLegs.Level = 7;

				recruitsLegs.Weight = 42;
				recruitsLegs.Model = 82; // Studded Legs

				recruitsLegs.DPS_AF = 10; // Armour
				recruitsLegs.SPD_ABS = 19; // Absorption

				recruitsLegs.Object_Type = (int) eObjectType.Studded;
				recruitsLegs.Item_Type = (int) eEquipmentItems.LEGS;
				recruitsLegs.Id_nb = "recruits_studded_legs_mid";
				recruitsLegs.Gold = 0;
				recruitsLegs.Silver = 10;
				recruitsLegs.Copper = 0;
				recruitsLegs.IsPickable = true;
				recruitsLegs.IsDropable = true;
				recruitsLegs.Color = 14; // blue leather

				recruitsLegs.Bonus = 5; // default bonus

				recruitsLegs.Bonus1 = 12;
				recruitsLegs.Bonus1Type = (int) eProperty.MaxHealth; // hit


				recruitsLegs.Bonus2 = 2;
				recruitsLegs.Bonus2Type = (int) eResist.Slash;

				recruitsLegs.Bonus3 = 1;
				recruitsLegs.Bonus3Type = (int) eResist.Cold;

				recruitsLegs.Quality = 100;
				recruitsLegs.MaxQuality = 100;
				recruitsLegs.Condition = 1000;
				recruitsLegs.MaxCondition = 1000;
				recruitsLegs.Durability = 1000;
				recruitsLegs.MaxDurability = 1000;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsLegs);
			}

			// item db check
			recruitsPants = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "recruits_quilted_pants");
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
				recruitsPants.Gold = 0;
				recruitsPants.Silver = 10;
				recruitsPants.Copper = 0;
				recruitsPants.IsPickable = true;
				recruitsPants.IsDropable = true;
				recruitsPants.Color = 36;

				recruitsPants.Bonus = 5; // default bonus

				recruitsPants.Bonus1 = 12;
				recruitsPants.Bonus1Type = (int) eProperty.MaxHealth; // hit


				recruitsPants.Bonus2 = 2;
				recruitsPants.Bonus2Type = (int) eResist.Slash;

				recruitsPants.Bonus3 = 1;
				recruitsPants.Bonus3Type = (int) eResist.Cold;

				recruitsPants.Quality = 100;
				recruitsPants.MaxQuality = 100;
				recruitsPants.Condition = 1000;
				recruitsPants.MaxCondition = 1000;
				recruitsPants.Durability = 1000;
				recruitsPants.MaxDurability = 1000;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsPants);
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

			GameEventMgr.AddHandler(annark, GameObjectEvent.Interact, new DOLEventHandler(TalkToAnnark));

			GameEventMgr.AddHandler(njiedi, GameObjectEvent.Interact, new DOLEventHandler(TalkToNjiedi));

			GameEventMgr.AddHandler(idora, GameObjectEvent.Interact, new DOLEventHandler(TalkToIdora));
			GameEventMgr.AddHandler(idora, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToIdora));

			GameEventMgr.AddHandler(vorgar, GameObjectEvent.Interact, new DOLEventHandler(TalkToVorgar));
			GameEventMgr.AddHandler(vorgar, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToVorgar));

			/* Now we bring to dalikor the possibility to give this quest to players */
			dalikor.AddQuestToGive(typeof (Frontiers));

			if (log.IsInfoEnabled)
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

			GameEventMgr.RemoveHandler(dalikor, GameObjectEvent.Interact, new DOLEventHandler(TalkToDalikor));
			GameEventMgr.RemoveHandler(dalikor, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToDalikor));

			GameEventMgr.RemoveHandler(annark, GameObjectEvent.Interact, new DOLEventHandler(TalkToAnnark));

			GameEventMgr.RemoveHandler(njiedi, GameObjectEvent.Interact, new DOLEventHandler(TalkToNjiedi));

			GameEventMgr.RemoveHandler(idora, GameObjectEvent.Interact, new DOLEventHandler(TalkToIdora));
			GameEventMgr.RemoveHandler(idora, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToIdora));

			GameEventMgr.RemoveHandler(vorgar, GameObjectEvent.Interact, new DOLEventHandler(TalkToVorgar));
			GameEventMgr.RemoveHandler(vorgar, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToVorgar));

			/* Now we remove to dalikor the possibility to give this quest to players */
			dalikor.RemoveQuestToGive(typeof (Frontiers));
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
					quest.idoraDone = true;
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
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(dalikor.CanGiveQuest(typeof (Frontiers), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Frontiers quest = player.IsDoingQuest(typeof (Frontiers)) as Frontiers;

			//Did the player rightclick on NPC?
			dalikor.TurnTo(player);
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...
					dalikor.SayTo(player, "Greetings again recruit. I spoke with the elders, and they have told me to speak with you about taking these plans to [Scryer Idora].");
					return;
				}
				else
				{
					switch (quest.Step)
					{
						case 1:
							dalikor.SayTo(player, "Wonderful Eeinken. Now here, take the plans and also this scroll for Griffin Handler Njiedi. Give him the scroll so he knows where to send you. You can find him near the gates to Jordheim. I wish you luck and speed on your journey Eeinken");
							break;
						case 5:
							dalikor.SayTo(player, "It's good to see you've made it back from the Proving Grounds recruit Eeinken. Do you have the translated plans for me?");
							break;
						case 6:
							dalikor.SayTo(player, "It is as I feared. The Fallen Askefruer have their eyes set on taking over Mularn and using it as their new kingdom! Though, how they will achieve this is beyond my comprehension. I will surely have to speak with the elders more. But now that I have finished reading it, I have something here for your [efforts].");
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
							//If the player offered his "help", we send the quest dialog now!
						case "Scryer Idora":
							dalikor.SayTo(player, "She is currently helping the newly recruited in the Proving Grounds. We are desperate for her expertise in exotic languages. We believe she is the [only one] who can translate this quickly.");
							break;
						case "only one":
							dalikor.SayTo(player, "Will you help out the elders and the rest of Mularn by [delivering] these plans to her?");
							break;
						case "delivering":
							player.Out.SendCustomDialog("Will you take this package to the Frontiers for Dalikor?", new CustomDialogResponse(CheckPlayerAcceptQuest));
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "efforts":
							dalikor.SayTo(player, "The elders have instructed me to give you these leggings as a sign of appreciation for doing these tasks for Mularn. I will present what you have given us to the council. Don't wander far Eeinken, I'm sure there is much more to do.");
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

		protected static void TalkToVorgar(DOLEvent e, object sender, EventArgs args)
		{
		}

		protected static void TalkToNjiedi(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(dalikor.CanGiveQuest(typeof (Frontiers), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Frontiers quest = player.IsDoingQuest(typeof (Frontiers)) as Frontiers;

			dalikor.TurnTo(player);
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					if (quest.Step == 1)
					{
						njiedi.SayTo(player, "Greetings my friend. How may I help you today?");
					}
				}
			}
		}

		protected static void TalkToIdora(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(dalikor.CanGiveQuest(typeof (Frontiers), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Frontiers quest = player.IsDoingQuest(typeof (Frontiers)) as Frontiers;

			idora.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					//Player is not doing the quest...
					if (quest.Step == 3 || quest.Step == 2)
					{
						idora.SayTo(player, "Welcome to the Proving Grounds Viking. I am Idora, a shadowblade by profession, but a scryer by hobby. How may I help you today?");
					}
					else if (quest.Step == 4)
					{
						if (quest.idoraDone)
						{
							idora.SayTo(player, "Ah, yes, I am now [done] with the translation.");
						}
						else
						{
							idora.SayTo(player, "Wait a minute... I'm almost finished.");
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
							idora.SayTo(player, "Here you are my intreprid young traveler. It has all been translated now. Take it back to Dalikor. I hope I was able to help today. Come back and visit me soon!");
							idora.SayTo(player, "Oh and take this horse ticket and give it to Vorgar at Svasud Faste he will bring you back home safely.");
							if (quest.Step == 4)
							{
								GiveItem(idora, player, translatedPlans);
								GiveItem(idora, player, ticketToMularn);
								quest.Step = 5;

								quest.TeleportTo(player, idora, locationVorgar, 50);
							}
							break;
					}
				}
			}
		}

		protected static void TalkToAnnark(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(dalikor.CanGiveQuest(typeof (Frontiers), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Frontiers quest = player.IsDoingQuest(typeof (Frontiers)) as Frontiers;

			annark.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null && quest.Step == 2)
				{
					annark.SayTo(player, "Huginn and Munnin guide you all and return with news of your journeys.");
					quest.Step = 3;

					quest.TeleportTo(player, annark, locationIdora, 30);
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
			if(dalikor.CanGiveQuest(typeof (Frontiers), player)  <= 0)
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
				if (!dalikor.GiveQuest(typeof (Frontiers), player, 1))
					return;

				dalikor.SayTo(player, "Wonderful Eeinken. Now here, take the plans and also this scroll for Griffin Handler Njiedi. Give him the scroll so he knows where to send you. You can find him near the gates to Jordheim. I wish you luck and speed on your journey Eeinken.");

				GiveItem(dalikor, player, noteForNjiedi);
				GiveItem(dalikor, player, askefruerPlans);
				player.AddMoney(Money.GetMoney(0, 0, 0, 6, 0), "You recieve {0} for the ride to Svasud Faste");
			}
		}

		protected virtual int AliceTranslation(RegionTimer callingTimer)
		{
			m_questPlayer.Out.SendEmoteAnimation(idora, eEmote.Yes);
			idoraDone = true;
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
						return "[Step #1] You must deliver the Askefruer Plans Dalikor gave you to Scryer Idora in the Frontiers. You must first take the scroll to Griffin Handler Njiedi near the Jordheim Gates.";
					case 2:
						return "[Step #2] Take the griffin to Svasud Faste. When you are there, speak with Stor Gothi Annark who will then teleport you to the Frontiers.";
					case 3:
						return "[Step #3] Give the Askefruer Plans to Idora.";
					case 4:
						return "[Step #4] Wait for Scryer Idora to finish translating the plans. If she stops speaking with you, ask her if she is [done] with her translations.";
					case 5:
						return "[Step #5] Take the translated plans back to Dalikor at the tower near Mularn. You can give the ticket Idora gave you to Stable Master Vorgar for a faster ride home.";
					case 6:
						return "[Step #6] Wait for Dalikor to finsh reading the translated text.";
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player==null || player.IsDoingQuest(typeof (Frontiers)) == null)
				return;

			if (player.IsDoingQuest(typeof (Frontiers)) == null)
				return;

			if (Step == 1 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == njiedi.Name && gArgs.Item.Id_nb == noteForNjiedi.Id_nb)
				{
					RemoveItem(njiedi, player, noteForNjiedi);

					njiedi.TurnTo(m_questPlayer);
					njiedi.SayTo(m_questPlayer, "Ah, from my old friend Dalikor. Let's see what he says. Ah, I am to give you transportation to Svasud Faste. No problem. All you need to do is purchase a ticket from my store.");
					m_questPlayer.Out.SendEmoteAnimation(dalikor, eEmote.Ponder);

					Step = 2;
					return;
				}
			}
			else if ((Step == 3 || Step == 2) && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == idora.Name && gArgs.Item.Id_nb == askefruerPlans.Id_nb)
				{
					RemoveItem(idora, player, askefruerPlans);

					idora.TurnTo(m_questPlayer);
					idora.SayTo(m_questPlayer, "Hmm...What's this now? A letter? For me? Interesting. Ah, I see it is from my old friend, Dalikor, something about plans written in fairy. I can translate this if you can wait just a few moments.");
					m_questPlayer.Out.SendEmoteAnimation(idora, eEmote.Ponder);
					SendEmoteMessage(player, "Scryer Idora takes out a piece of parchment and begins to translate the scroll you brought to her. In just a few short minutes, she is done with the translation.");

					new RegionTimer(gArgs.Target, new RegionTimerCallback(AliceTranslation), 30000);

					Step = 4;
					return;
				}
			}
			else if (Step == 5 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == dalikor.Name && gArgs.Item.Id_nb == translatedPlans.Id_nb)
				{
					RemoveItem(dalikor, player, translatedPlans);

					dalikor.TurnTo(m_questPlayer);
					dalikor.SayTo(m_questPlayer, "Excellent work. Now, if you will please just wait a moment, I need to read this.");
					m_questPlayer.Out.SendEmoteAnimation(dalikor, eEmote.Ponder);
					SendEmoteMessage(player, "Dalikor holds up the parchment and slowly reads the information written on it. When he is done, he folds it up and places it in his pocket.");

					Step = 6;
					return;
				}
			}

		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			if (Step < 3 && m_questPlayer.Inventory.GetFirstItemByID(ticketToSvasudFaste.Id_nb, eInventorySlot.Min_Inv, eInventorySlot.Max_Inv) == null)
			{
				m_questPlayer.RemoveMoney(Money.GetMoney(0, 0, 0, 6, 0), null);
			}

			RemoveItem(m_questPlayer, ticketToSvasudFaste, false);
			RemoveItem(m_questPlayer, askefruerPlans, false);
			RemoveItem(m_questPlayer, ticketToMularn, false);
			RemoveItem(m_questPlayer, noteForNjiedi, false);
			RemoveItem(m_questPlayer, translatedPlans, false);

		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			//Give reward to player here ...            
			if (m_questPlayer.HasAbilityToUseItem(recruitsLegs))
				GiveItem(dalikor, m_questPlayer, recruitsLegs);
			else
				GiveItem(dalikor, m_questPlayer, recruitsPants);

			m_questPlayer.GainExperience(240, 0, 0, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 5, Util.Random(50)), "You recieve {0} as a reward.");

		}

	}
}
