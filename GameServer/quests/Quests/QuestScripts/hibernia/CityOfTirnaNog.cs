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
 * Directory: /scripts/quests/hibernia/
 *
 * Description:
 *  Brief Walkthrough: 
 *  1) Bombard can be found at loc=32479,62585 Black Mountains South. 
 *  2) Zone into Camelot and /USE the Assistant Necklace from Bombard. 
 *  3) Talk with the Camelot Assistant that appeared from using the necklace. Click on [vault keeper] option when it is given. 
 *  4) Hand Lord Urqhart the Scroll for Vault Keeper Urqhart from Bombard and then the Small Chest of Coins. 
 *  5) Click on the Camelot Assistant to teleport back to the main gate. 
 *  6) Go back to Bombard and hand him the Receipt for Bombard from Urqhart, chat with him a bit more for your reward.
 */

using System;
using System.Reflection;
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

namespace DOL.GS.Quests.Hibernia
{
	/* The first thing we do, is to declare the class we create
	 * as Quest. To do this, we derive from the abstract class
	 * AbstractQuest
	 * 	 
	 */

	public class CityOfTirnaNog : BaseAddrirQuest
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

		protected const string questTitle = "City of Tir na Nog";
		protected const int minimumLevel = 1;
		protected const int maximumLevel = 1;

		protected static GameLocation heroTrainer = new GameLocation("Hero Trainer", 201, 31555, 26198, 7767, 2462);
		protected static GameLocation bladeMasterTrainer = new GameLocation("Blademaster Trainer", 201, 29819, 26485, 7767, 1403);
		protected static GameLocation championTrainer = new GameLocation("Champion Trainer", 201, 31783, 27885, 7767, 3377);
		//protected static GameLocation mercenaryTrainer = new GameLocation("Mercenary Trainer", 10, 32826, 26831, 7995, 0);

		protected static GameLocation vaultKeeper = new GameLocation("Vault Keeper", 201, 33175, 31237, 8000, 1970);
		protected static GameLocation eastGates = new GameLocation("East Gates", 201, 22648, 34592, 6250, 995);

		private static GameNPC addrir = null;
		private static GameNPC hylvian = null;
		private static GameNPC freagus = null;
		private static GameNPC gweonry = null;

		private GameNPC assistant = null;
		private GameTimer assistantTimer = null;

		private static ItemTemplate ticketToTirnaNog = null;
		private static ItemTemplate ticketToMagMell = null;

		private static ItemTemplate ticketToArdee = null;

		private static ItemTemplate scrollHylvian = null;
		private static ItemTemplate receiptFreagus = null;
		private static ItemTemplate letterAddrir = null;
		private static ItemTemplate assistantNecklace = null;
		private static ItemTemplate chestOfCoins = null;
		private static ItemTemplate recruitsRoundShield = null;
		private static ItemTemplate recruitsBracer = null;


		/* We need to define the constructors from the base class here, else there might be problems
		 * when loading this quest...
		 */
		public CityOfTirnaNog() : base()
		{
		}

		public CityOfTirnaNog(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public CityOfTirnaNog(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public CityOfTirnaNog(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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

			addrir = GetAddrir();

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Bhreagar Hylvian", eRealm.Hibernia);
			if (npcs.Length == 0)
			{
				hylvian = new GameNPC();
				hylvian.Model = 384;
				hylvian.Name = "Bhreagar Hylvian";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + hylvian.Name + ", creating ...");
				hylvian.GuildName = "Part of " + questTitle + " Quest";
				hylvian.Realm = eRealm.Hibernia;
				hylvian.CurrentRegionID = 201;
				hylvian.Size = 51;
				hylvian.Level = 44;
				hylvian.X = 33163;
				hylvian.Y = 31142;
				hylvian.Z = 8000;
				hylvian.Heading = 11;
				hylvian.EquipmentTemplateID = "7400147";
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					hylvian.SaveIntoDatabase();

				hylvian.AddToWorld();
			}
			else
				hylvian = npcs[0];

			npcs = WorldMgr.GetNPCsByName("Freagus", eRealm.Hibernia);
			if (npcs.Length == 0)
			{
				freagus = new GameStableMaster();
				freagus.Model = 361;
				freagus.Name = "Bombard";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + freagus.Name + ", creating ...");
				freagus.GuildName = "Stable Master";
				freagus.Realm = eRealm.Hibernia;
				freagus.CurrentRegionID = 200;
				freagus.Size = 48;
				freagus.Level = 30;
				freagus.X = 341008;
				freagus.Y = 469180;
				freagus.Z = 5200;
				freagus.Heading = 1934;
				freagus.EquipmentTemplateID = "3800664";

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					freagus.SaveIntoDatabase();
				freagus.AddToWorld();
			}
			else
				freagus = npcs[0];

			npcs = WorldMgr.GetNPCsByName("Gweonry", eRealm.Hibernia);
			if (npcs.Length == 0)
			{
				gweonry = new GameStableMaster();
				gweonry.Model = 361;
				gweonry.Name = "Gweonry";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + gweonry.Name + ", creating ...");
				gweonry.GuildName = "Stable Master";
				gweonry.Realm = eRealm.Hibernia;
				gweonry.CurrentRegionID = 200;
				gweonry.Size = 48;
				gweonry.Level = 30;
				gweonry.X = GameLocation.ConvertLocalXToGlobalX(16334, 200);
				gweonry.Y = GameLocation.ConvertLocalYToGlobalY(3384, 200);
				gweonry.Z = 5200;
				gweonry.Heading = 245;
				//gweonry.EquipmentTemplateID="3800664";

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					gweonry.SaveIntoDatabase();
				gweonry.AddToWorld();
			}
			else
				gweonry = npcs[0];

			#endregion

			#region defineItems

			ticketToTirnaNog = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "hs_ardee_northtirnanog");
			if (ticketToTirnaNog == null)
				ticketToTirnaNog = CreateTicketTo("Tir na nOgh", "hs_ardee_northtirnanog");

			ticketToArdee = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "hs_easttirnanogh_ardee");
			if (ticketToArdee == null)
				ticketToArdee = CreateTicketTo("Ardee", "hs_easttirnanogh_ardee");
			
			ticketToMagMell = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "hs_ardee_magmell");
			if (ticketToMagMell == null)
				ticketToMagMell = CreateTicketTo("Mag Mell", "hs_ardee_magmell");

			scrollHylvian = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "scroll_for_hylvian");
			if (scrollHylvian == null)
			{
				scrollHylvian = new ItemTemplate();
				scrollHylvian.Name = "Scroll for Vault Keeper Hylvian";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + scrollHylvian.Name + " , creating it ...");

				scrollHylvian.Weight = 3;
				scrollHylvian.Model = 498;

				scrollHylvian.Object_Type = (int) eObjectType.GenericItem;

				scrollHylvian.Id_nb = "scroll_for_hylvian";
				scrollHylvian.IsPickable = true;
				scrollHylvian.IsDropable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(scrollHylvian);
			}


			receiptFreagus = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "receipt_for_freagus");
			if (receiptFreagus == null)
			{
				receiptFreagus = new ItemTemplate();
				receiptFreagus.Name = "Receipt for Freagus";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + receiptFreagus.Name + " , creating it ...");

				receiptFreagus.Weight = 3;
				receiptFreagus.Model = 498;

				receiptFreagus.Object_Type = (int) eObjectType.GenericItem;

				receiptFreagus.Id_nb = "receipt_for_freagus";
				receiptFreagus.IsPickable = true;
				receiptFreagus.IsDropable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(receiptFreagus);
			}

			chestOfCoins = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "small_chest_of_coins");
			if (chestOfCoins == null)
			{
				chestOfCoins = new ItemTemplate();
				chestOfCoins.Name = "Small Chest of Coins";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + chestOfCoins.Name + " , creating it ...");

				chestOfCoins.Weight = 15;
				chestOfCoins.Model = 602;

				chestOfCoins.Object_Type = (int) eObjectType.GenericItem;

				chestOfCoins.Id_nb = "small_chest_of_coins";
				chestOfCoins.IsPickable = true;
				chestOfCoins.IsDropable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(chestOfCoins);
			}

			letterAddrir = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "letter_for_addrir");
			if (letterAddrir == null)
			{
				letterAddrir = new ItemTemplate();
				letterAddrir.Name = "Letter for Addrir";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + letterAddrir.Name + " , creating it ...");

				letterAddrir.Weight = 3;
				letterAddrir.Model = 498;

				letterAddrir.Object_Type = (int) eObjectType.GenericItem;

				letterAddrir.Id_nb = "letter_for_addrir";
				letterAddrir.IsPickable = true;
				letterAddrir.IsDropable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(letterAddrir);
			}

			assistantNecklace = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "assistant_necklace");
			if (assistantNecklace == null)
			{
				assistantNecklace = new ItemTemplate();
				assistantNecklace.Name = "Assistant's Necklace";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + assistantNecklace.Name + " , creating it ...");

				assistantNecklace.Weight = 3;
				assistantNecklace.Model = 101;

				assistantNecklace.Object_Type = (int) eObjectType.Magical;
				assistantNecklace.Item_Type = (int) eEquipmentItems.NECK;

				assistantNecklace.Id_nb = "assistant_necklace";
				assistantNecklace.IsPickable = true;
				assistantNecklace.IsDropable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(assistantNecklace);
			}

			// item db check
			recruitsRoundShield = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "recruits_round_shield_hib");
			if (recruitsRoundShield == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Recruit's Round Shield, creating it ...");
				recruitsRoundShield = new ItemTemplate();
				recruitsRoundShield.Name = "Recruit's Round Shield (Hib)";
				recruitsRoundShield.Level = 4;

				recruitsRoundShield.Weight = 31;
				recruitsRoundShield.Model = 59; // studded Boots                

				recruitsRoundShield.Object_Type = 0x2A; // (int)eObjectType.Shield;
				recruitsRoundShield.Item_Type = (int) eEquipmentItems.LEFT_HAND;
				recruitsRoundShield.Id_nb = "recruits_round_shield_hib";
				recruitsRoundShield.Gold = 0;
				recruitsRoundShield.Silver = 4;
				recruitsRoundShield.Copper = 0;
				recruitsRoundShield.IsPickable = true;
				recruitsRoundShield.IsDropable = true;
				recruitsRoundShield.Color = 69;
				recruitsRoundShield.Hand = 2;
				recruitsRoundShield.DPS_AF = 1;
				recruitsRoundShield.SPD_ABS = 1;

				recruitsRoundShield.Type_Damage = 1;

				recruitsRoundShield.Bonus = 1; // default bonus

				recruitsRoundShield.Bonus1 = 1;
				recruitsRoundShield.Bonus1Type = (int) eStat.STR;

				recruitsRoundShield.Bonus2 = 1;
				recruitsRoundShield.Bonus2Type = (int) eResist.Body;

				recruitsRoundShield.Quality = 100;
				recruitsRoundShield.Condition = 1000;
				recruitsRoundShield.MaxCondition = 1000;
				recruitsRoundShield.Durability = 1000;
				recruitsRoundShield.MaxDurability = 1000;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsRoundShield);
			}

			// item db check
			recruitsBracer = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "recruits_silver_bracer");
			if (recruitsBracer == null)
			{
				recruitsBracer = new ItemTemplate();
				recruitsBracer.Name = "Recruit's Silver Bracer";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsBracer.Name + ", creating it ...");
				recruitsBracer.Level = 4;

				recruitsBracer.Weight = 10;
				recruitsBracer.Model = 130;

				recruitsBracer.Object_Type = (int) eObjectType.Magical;
				recruitsBracer.Item_Type = (int) eEquipmentItems.L_BRACER;
				recruitsBracer.Id_nb = "recruits_silver_bracer";
				recruitsBracer.Gold = 0;
				recruitsBracer.Silver = 4;
				recruitsBracer.Copper = 0;
				recruitsBracer.IsPickable = true;
				recruitsBracer.IsDropable = true;
				//recruitsBracer.Color = 36;
				//recruitsBracer.Hand = 2;
				//recruitsBracer.DPS_AF = 1;
				//recruitsBracer.SPD_ABS = 1;

				//recruitsBracer.Type_Damage = 1;

				recruitsBracer.Bonus = 1; // default bonus

				recruitsBracer.Bonus1 = 8;
				recruitsBracer.Bonus1Type = (int) eProperty.MaxHealth;

				recruitsBracer.Bonus2 = 1;
				recruitsBracer.Bonus2Type = (int) eResist.Crush;

				recruitsBracer.Quality = 100;
				recruitsBracer.Condition = 1000;
				recruitsBracer.MaxCondition = 1000;
				recruitsBracer.Durability = 1000;
				recruitsBracer.MaxDurability = 1000;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsBracer);
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

			GameEventMgr.AddHandler(addrir, GameLivingEvent.Interact, new DOLEventHandler(TalkToAddrir));
			GameEventMgr.AddHandler(addrir, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToAddrir));

			GameEventMgr.AddHandler(freagus, GameLivingEvent.Interact, new DOLEventHandler(TalkToFreagus));
			GameEventMgr.AddHandler(freagus, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToFreagus));

			GameEventMgr.AddHandler(hylvian, GameLivingEvent.Interact, new DOLEventHandler(TalkToHylvian));
			GameEventMgr.AddHandler(hylvian, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToHylvian));

			GameEventMgr.AddHandler(gweonry, GameLivingEvent.Interact, new DOLEventHandler(TalkToGweonry));

			/* Now we bring to freagus the possibility to give this quest to players */
			freagus.AddQuestToGive(typeof (CityOfTirnaNog));

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
			if (addrir == null)
				return;

			/* Removing hooks works just as adding them but instead of 
			 * AddHandler, we call RemoveHandler, the parameters stay the same
			 */
			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));

			GameEventMgr.RemoveHandler(addrir, GameLivingEvent.Interact, new DOLEventHandler(TalkToAddrir));
			GameEventMgr.RemoveHandler(addrir, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToAddrir));

			GameEventMgr.RemoveHandler(freagus, GameLivingEvent.Interact, new DOLEventHandler(TalkToFreagus));
			GameEventMgr.RemoveHandler(freagus, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToFreagus));

			GameEventMgr.RemoveHandler(hylvian, GameLivingEvent.Interact, new DOLEventHandler(TalkToHylvian));
			GameEventMgr.RemoveHandler(hylvian, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToHylvian));

			GameEventMgr.RemoveHandler(gweonry, GameLivingEvent.Interact, new DOLEventHandler(TalkToGweonry));

			/* Now we remove to freagus the possibility to give this quest to players */
			freagus.RemoveQuestToGive(typeof (CityOfTirnaNog));
		}

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToAddrir(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(freagus.CanGiveQuest(typeof (CityOfTirnaNog), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			CityOfTirnaNog quest = player.IsDoingQuest(typeof (CityOfTirnaNog)) as CityOfTirnaNog;

			addrir.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					switch (quest.Step)
					{
						case 9:
							addrir.SayTo(player, "Welcome back Lirone. You were gone for a while. Is everything alright?");
							break;
						case 10:
							addrir.SayTo(player, "Freagus speaks very highly of you. I think a [reward] is in store for you my young recruit.");
							break;
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
						case "reward":
							addrir.SayTo(player, "Ah yes, here we are. Excellent. You have done a great thing today by helping out Freagus. You will go far in Hibernia Lirone, I just know it. Don't wander too far, for I have more work that needs to be done.");
							if (quest.Step == 10)
							{
								quest.FinishQuest();
								addrir.SayTo(player, "I have another task for you Lirone. Speak with me when you are free.");
							}
							break;
					}
				}
			}
		}

		protected static void TalkToGweonry(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(freagus.CanGiveQuest(typeof (CityOfTirnaNog), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			CityOfTirnaNog quest = player.IsDoingQuest(typeof (CityOfTirnaNog)) as CityOfTirnaNog;

			gweonry.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					if (quest.Step > 6)
					{
						gweonry.SayTo(player, "Good day Lirone. I am here to assist the newly recruited. My tickets are free but are tied to certain quests. If you have been instructed to come to me, one of my tickets will be highlighted for you. Please purchase that one.");
					}
				}
				return;
			}
		}

		protected static void TalkToHylvian(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(freagus.CanGiveQuest(typeof (CityOfTirnaNog), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			CityOfTirnaNog quest = player.IsDoingQuest(typeof (CityOfTirnaNog)) as CityOfTirnaNog;

			hylvian.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					if (quest.Step < 6)
					{
						hylvian.SayTo(player, "Greetings to you my young adventurer. How may I help you today?");
					}
					else if (quest.Step == 6)
					{
						hylvian.SayTo(player, "Ah, I am now [done].");
					}

				}

				return;
			}
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				if (quest != null)
				{
					//Do some small talk :)
					switch (wArgs.Text)
					{
						case "done":
							hylvian.SayTo(player, "Here you are my friend. Please be sure to take this receipt back to Freagus for me. Be safe, and if you need to deposit some items, be sure to come back to me. If you wish to return to Ardee faster, visit with Gweonry outside of the Tir na Nog east gates. He will have a horse ticket ready for you.");
							if (quest.Step == 6)
							{
								GiveItem(hylvian, player, receiptFreagus);
								GiveItem(hylvian, player, ticketToArdee);
								quest.Step = 7;
							}
							break;
					}
				}
			}
		}

		protected static void TalkToFreagus(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(freagus.CanGiveQuest(typeof (CityOfTirnaNog), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			CityOfTirnaNog quest = player.IsDoingQuest(typeof (CityOfTirnaNog)) as CityOfTirnaNog;

			freagus.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					freagus.SayTo(player, "I am in need of some assistance. You see, I need to get this small chest of coins over to the vault keeper in Tir na Nog, but I never seem to have the [time] to do so.");
				}
				else
				{
					switch (quest.Step)
					{
						case 1:
							freagus.SayTo(player, "Take the horse to Tir na Nog, then, once you enter, USE the necklace. Don't worry, I'm sure your journal there will help you out. Come back after you've visited the vault keeper.");
							break;
						case 8:
							freagus.SayTo(player, "Welcome back Lirone. Did you have a good time in Tir na Nog? Did you get my coins to the Vault Keeper?");
							break;
					}
				}
				return;
			}
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				if (quest == null)
				{
					switch (wArgs.Text)
					{
						case "time":
							freagus.SayTo(player, "By the time I close my stable down for the evening, the vault keeper has already gone home. You have already helped me once Lirone, will you help me [again]?");
							break;
						case "again":
							player.Out.SendQuestSubscribeCommand(freagus, QuestMgr.GetIDForQuestType(typeof(CityOfTirnaNog)), "Will you travel to Tir na Nog for Freagus?");
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "Tir na Nog":
							freagus.SayTo(player, "First is a note for the vault keeper. Second is the chest of coins and third is a special necklace made by my [wife].");
							break;
						case "wife":
							freagus.SayTo(player, "She is an enchanter, but is currently away from Ardee right now. If she was here, I wouldn't need you to take my money to the vault keeper for me! Hehe! Anyhow, this necklace has been imbued with special powers to help you in the [city].");
							break;
						case "city":
							freagus.SayTo(player, "Take the horse to Tir na Nog, then, once you enter, USE the necklace. Don't worry, I'm sure your journal there will help you out. Come back after you've visited the vault keeper.");
							break;

						case "abort":
							player.Out.SendCustomDialog("Do you really want to abort this quest, \nall items gained during quest will be lost?", new CustomDialogResponse(CheckPlayerAbortQuest));
							break;
					}
				}
			}
		}

		protected static void TalkToAssistant(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(freagus.CanGiveQuest(typeof (CityOfTirnaNog), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			CityOfTirnaNog quest = player.IsDoingQuest(typeof (CityOfTirnaNog)) as CityOfTirnaNog;

			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null && quest.assistant == sender)
				{
					quest.assistant.TurnTo(player);
					quest.assistant.SayTo(player, "Greetings to you. I am here to assist you on your journey through Tir na Nog. Please listen [carefully] to my instructions.");
					if (quest.Step == 2)
					{
						quest.Step = 3;
					}
				}
				return;
			}
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				if (quest != null && quest.assistant == sender)
				{
					quest.assistant.TurnTo(player);
					//Do some small talk :)
					switch (wArgs.Text)
					{
						case "carefully":
							quest.assistant.SayTo(player, "I have a limited time duration. I will only be around for half an hour (2 real life minutes), so please use me wisely. I can teleport you to various [trainers] within Tir na Nog.");
							break;
						case "trainers":
							quest.assistant.SayTo(player, "I know that you have been given the task of delivering Freagus's coins to the vault keeper. I can teleport you there are well. All you need to do is make a [choice].");
							break;
						case "choice":
							quest.assistant.SayTo(player, "Where would you like to go to? The [champion] trainer; the [hero] trainer; the [blademaster] trainer, the [vault keeper], or to the [east gates]?");
							break;

						case "champion":
							quest.TeleportTo(championTrainer);
							break;
						case "hero":
							quest.TeleportTo(heroTrainer);
							break;
						case "blademaster":
							quest.TeleportTo(bladeMasterTrainer);
							break;
							//case "mercenary": quest.TeleportTo(mercenaryTrainer); break;
						case "east gates":
							quest.TeleportTo(eastGates);
							if (quest.Step == 7)
							{
								quest.Step = 8;
							}
							break;
						case "vault keeper":
							if (quest.Step == 3)
							{
								quest.Step = 4;
							}
							quest.TeleportTo(vaultKeeper);
							break;

						case "go away":
							quest.assistantTimer.Start(1);
							break;

					}
				}
			}
		}

		/**
         * Convinient Method for teleporintg with assistant 
         */

		protected void TeleportTo(GameLocation target)
		{
			TeleportTo(m_questPlayer, assistant, target, 5);
			TeleportTo(assistant, assistant, target, 25, 50);
		}


		protected static void PlayerEnterWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			CityOfTirnaNog quest = player.IsDoingQuest(typeof (CityOfTirnaNog)) as CityOfTirnaNog;
			if (quest != null)
			{
				GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
				GameEventMgr.AddHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			}
		}

		protected static void PlayerLeftWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			CityOfTirnaNog quest = player.IsDoingQuest(typeof (CityOfTirnaNog)) as CityOfTirnaNog;
			if (quest != null)
			{
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
				// remorph player back...
				if (quest.assistantTimer != null)
					quest.assistantTimer.Start(1);

			}
		}

		protected static void PlayerUseSlot(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = (GamePlayer) sender;
			if (player == null)
				return;

			// assistant works only in tir na nog...
			if (player.CurrentRegionID != 201)
				return;

			CityOfTirnaNog quest = (CityOfTirnaNog) player.IsDoingQuest(typeof (CityOfTirnaNog));
			if (quest == null)
				return;

			UseSlotEventArgs uArgs = (UseSlotEventArgs) args;

			InventoryItem item = player.Inventory.GetItem((eInventorySlot)uArgs.Slot);
			if (item != null && item.Id_nb == assistantNecklace.Id_nb)
			{
				foreach (GamePlayer visPlayer in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					visPlayer.Out.SendSpellCastAnimation(player, 1, 20);
				}

				RegionTimer createTimer = new RegionTimer(player, new RegionTimerCallback(quest.CreateAssistant), 2000);

				if (quest.Step == 1)
				{
					quest.Step = 2;
				}
			}

		}

		protected virtual int DeleteAssistant(RegionTimer callingTimer)
		{
			if (assistant != null)
			{
				assistant.Delete();

				GameEventMgr.RemoveHandler(assistant, GameLivingEvent.Interact, new DOLEventHandler(TalkToAssistant));
				GameEventMgr.RemoveHandler(assistant, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToAssistant));
			}
			return 0;
		}

		protected virtual int CreateAssistant(RegionTimer callingTimer)
		{
			if (assistant != null && assistant.ObjectState == GameObject.eObjectState.Active)
			{
				assistant.MoveTo(m_questPlayer.CurrentRegionID, m_questPlayer.X + 50, m_questPlayer.Y + 30, m_questPlayer.Z, m_questPlayer.Heading);
			}
			else
			{
				assistant = new GameNPC();
				assistant.Model = 951;
				assistant.Name = m_questPlayer.Name + "'s Assistant";
				assistant.GuildName = "Part of " + questTitle + " Quest";
				assistant.Realm = m_questPlayer.Realm;
				assistant.CurrentRegionID = m_questPlayer.CurrentRegionID;
				assistant.Size = 25;
				assistant.Level = 5;
				assistant.X = m_questPlayer.X + 50;
				assistant.Y = m_questPlayer.Y + 50;
				assistant.Z = m_questPlayer.Z;
				assistant.Heading = m_questPlayer.Heading;

				assistant.AddToWorld();

				GameEventMgr.AddHandler(assistant, GameLivingEvent.Interact, new DOLEventHandler(TalkToAssistant));
				GameEventMgr.AddHandler(assistant, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToAssistant));
			}

			foreach (GamePlayer visPlayer in assistant.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				visPlayer.Out.SendEmoteAnimation(assistant, eEmote.Bind);
			}

			if (assistantTimer != null)
			{
				assistantTimer.Stop();
			}
			assistantTimer = new RegionTimer(assistant, new RegionTimerCallback(DeleteAssistant), 120000);

			return 0;
		}

		/// <summary>
		/// This method checks if a player qualifies for this quest
		/// </summary>
		/// <returns>true if qualified, false if not</returns>
		public override bool CheckQuestQualification(GamePlayer player)
		{
			// if the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(typeof (CityOfTirnaNog)) != null)
				return true;

			// This checks below are only performed is player isn't doing quest already

			if (player.HasFinishedQuest(typeof (ImportantDelivery)) == 0)
				return false;

			if (!CheckPartAccessible(player, typeof (CityOfTirnaNog)))
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
			CityOfTirnaNog quest = player.IsDoingQuest(typeof (CityOfTirnaNog)) as CityOfTirnaNog;

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

		protected static void SubscribeQuest(DOLEvent e, object sender, EventArgs args)
		{
			QuestEventArgs qargs = args as QuestEventArgs;
			if (qargs == null)
				return;

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(CityOfTirnaNog)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

		/* This is our callback hook that will be called when the player clicks
		 * on any button in the quest offer dialog. We check if he accepts or
		 * declines here...
		 */

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			//We recheck the qualification, because we don't talk to players
			//who are not doing the quest
			if(freagus.CanGiveQuest(typeof (CityOfTirnaNog), player)  <= 0)
				return;

			CityOfTirnaNog quest = player.IsDoingQuest(typeof (CityOfTirnaNog)) as CityOfTirnaNog;

			if (quest != null)
				return;

			if (response == 0x00)
			{
				SendReply(player, "Oh well, if you change your mind, please come back!");
			}
			else
			{
				//Check if we can add the quest!
				if (!freagus.GiveQuest(typeof (CityOfTirnaNog), player, 1))
					return;

				freagus.SayTo(player, "Ah! Wonderful. I have several things for you for your trip into [Tir na Nog].");

				GiveItem(freagus, player, assistantNecklace);
				GiveItem(freagus, player, chestOfCoins);
				GiveItem(freagus, player, scrollHylvian);
				GiveItem(freagus, player, ticketToTirnaNog);
				player.GainExperience(7, true);

				GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
				GameEventMgr.AddHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
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
						return "[Step #1] Freagus has given you a Chest of Coins he wants delivered to the Vault Keeper. Make your way inside the gates of Tir na Nog. When you are at the East Gate, USE the necklace you were given. Take the hourse to Tir na Nog.";
					case 2:
						return "[Step #2] You have successfully USED the necklace. Speak with the Tir na Nog Assistant for further instructions.";
					case 3:
						return "[Step #3] Choose the destination you wish to know more about. If you do not need the Assistant, tell him to [go away] If you go straight to the Vault Keeper, be sure to hand him the scroll from Freagus.";
					case 4:
						return "[Step #4] Speak with Vault Keeper Bhreagar. Be sure to hand him the scroll from Freagus.";
					case 5:
						return "[Step #5] Hand the Small Chest of Coins over to Vault Keeper Bhreagar.";
					case 6:
						return "[Step #6] Wait for the Vault Keeper to finish counting the coins. If he stops speaking with you, ask him if he is [done] counting the coins.";
					case 7:
						return "[Step #7] If the Tir na Nog Assistant is still with you, ask him to teleport you to the [east gates].";
					case 8:
						return "[Step #8] Find Stable Master Gweonry outside Tir na Nog. Take the horse back to Ardee. Be sure to give Freagus the sealed scroll from Bhreagar.";
					case 9:
						return "[Step #9] Return to Mag Mell and hand Addrir the note. Wait for him to read the note from Freagus. If he stops speaking with you, ask if he is [finished] with the letter.";
					case 10:
						return "[Step #10] Wait for Addrir to reward you. If he stops speaking with you at any time, ask if there is a [reward] for your efforts.";

				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player ==null || player.IsDoingQuest(typeof (CityOfTirnaNog)) == null)
				return;

			if (Step <= 4 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == hylvian.Name && gArgs.Item.Id_nb == scrollHylvian.Id_nb)
				{
					hylvian.SayTo(player, "Ah, from Freagus. He wishes to make a deposit. Alright then, if you will, please hand me the Small Chest of Coins.");
					RemoveItem(hylvian, player, scrollHylvian);
					Step = 5;
					return;
				}
			}

			if (Step == 5 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == hylvian.Name && gArgs.Item.Id_nb == chestOfCoins.Id_nb)
				{
					hylvian.SayTo(player, "This chest is quite heavy. Business must be booming for Freagus. Now, if you will just wait but a moment, I will be sure to get a receipt for you to take back to him.");
					RemoveItem(hylvian, player, chestOfCoins);
					Step = 6;

					SendEmoteMessage(player, "Bhreagar begins to count the coins. When he is done, he pulls out a piece of parchment and writes a few things down. He then seals it with his personal seal.");
					return;
				}
			}

			if (Step == 8 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == freagus.Name && gArgs.Item.Id_nb == receiptFreagus.Id_nb)
				{
					freagus.SayTo(player, "Ah! My receipt. Thank you so much Lirone. Here, take this note back to Addrir in Mag Mell for me please. I would like to let him know what a fine person you are for assisting me twice now! Be safe on your journey back to Mag Mell.");

					RemoveItem(freagus, player, receiptFreagus);
					RemoveItem(freagus, player, assistantNecklace);

					GiveItem(freagus, player, ticketToMagMell);
					GiveItem(freagus, player, letterAddrir);
					Step = 9;
					return;
				}
			}

			if (Step == 9 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == addrir.Name && gArgs.Item.Id_nb == letterAddrir.Id_nb)
				{
					addrir.SayTo(player, "Hmm...What's this?");
					SendSystemMessage(player, "Addrir reads over the note carefully then returns his attentions to you.");
					player.Out.SendEmoteAnimation(addrir, eEmote.Ponder);

					RemoveItem(addrir, player, letterAddrir);
					Step = 10;
					return;
				}
			}

		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, assistantNecklace, false);
			RemoveItem(m_questPlayer, chestOfCoins, false);
			RemoveItem(m_questPlayer, letterAddrir, false);
			RemoveItem(m_questPlayer, scrollHylvian, false);
			RemoveItem(m_questPlayer, ticketToArdee, false);
			RemoveItem(m_questPlayer, ticketToMagMell, false);
			RemoveItem(m_questPlayer, ticketToTirnaNog, false);

			// remove the 7 xp you get on quest start for beeing so nice to bombard again.
			m_questPlayer.GainExperience(-7, true);

			if (assistantTimer != null)
			{
				assistantTimer.Start(1);
			}

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			//Give reward to player here ...            
			if (m_questPlayer.HasAbilityToUseItem(recruitsRoundShield))
				GiveItem(addrir, m_questPlayer, recruitsRoundShield);
			else
				GiveItem(addrir, m_questPlayer, recruitsBracer);

			m_questPlayer.GainExperience(26, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 2, Util.Random(50)), "You recieve {0} as a reward.");

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));

		}

	}
}
