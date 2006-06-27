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
using DOL.GS.Database;
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
	/* The first thing we do, is to declare the quest requirement
	 * class linked with the new Quest. To do this, we derive 
	 * from the abstract class AbstractQuestDescriptor
	 */
	public class CityOfTirnaNogDescriptor : AbstractQuestDescriptor
	{
		/* This is the type of the quest class linked with 
		 * this requirement class, you must override the 
		 * base method like that
		 */
		public override Type LinkedQuestType
		{
			get { return typeof(CityOfTirnaNog); }
		}

		/* This value is used to retrieves how maximum level needed
		 * to be able to make this quest. Override it only if you need, 
		 * the default value is 50
		 */
		public override int MaxLevel
		{
			get { return 1; }
		}

		/* This method is used to know if the player is qualified to 
		 * do the quest. The base method always test his level and
		 * how many time the quest has been done. Override it only if 
		 * you want to add a custom test (here we test also the class name)
		 */
		public override bool CheckQuestQualification(GamePlayer player)
		{
			// This checks below are only performed is player isn't doing quest already

			if (player.HasFinishedQuest(typeof(ImportantDeliveryHib)) == 0)
				return false;

			if (!BaseAddirQuest.CheckPartAccessible(player, typeof(CityOfTirnaNog)))
				return false;

			return base.CheckQuestQualification(player);
		}
	}


	/* The second thing we do, is to declare the class we create
	 * as Quest. We must make it persistant using attributes, to
	 * do this, we derive from the abstract class AbstractQuest
	 */
	[NHibernate.Mapping.Attributes.Subclass(NameType = typeof(CityOfTirnaNog), ExtendsType = typeof(AbstractQuest))] 
	public class CityOfTirnaNog : BaseAddirQuest
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

		private static TravelTicketTemplate ticketToTirnaNog = null;
		private static TravelTicketTemplate ticketToMagMell = null;

		private static TravelTicketTemplate ticketToArdee = null;

		private static GenericItemTemplate scrollHylvian = null;
		private static GenericItemTemplate receiptFreagus = null;
		private static GenericItemTemplate letterAddrir = null;
		private static GenericItemTemplate assistantNecklace = null;
		private static GenericItemTemplate chestOfCoins = null;
		private static ShieldTemplate recruitsRoundShield = null;
		private static BracerTemplate recruitsBracer = null;

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

			#region defineNPCs

			addrir = GetAddrir();

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Bhreagar Hylvian", eRealm.Hibernia);
			if (npcs.Length == 0)
			{
				hylvian = new GameMob();
				hylvian.Model = 384;
				hylvian.Name = "Bhreagar Hylvian";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + hylvian.Name + ", creating ...");
				hylvian.GuildName = "Part of " + questTitle + " Quest";
				hylvian.Realm = (byte) eRealm.Hibernia;
				hylvian.RegionId = 201;
				hylvian.Size = 51;
				hylvian.Level = 44;
				hylvian.Position = new Point(33163, 31142, 8000);
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
				freagus.Realm = (byte) eRealm.Hibernia;
				freagus.RegionId = 200;
				freagus.Size = 48;
				freagus.Level = 30;
				freagus.Position = new Point(341008, 469180, 5200);
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
				gweonry.Realm = (byte) eRealm.Hibernia;
				gweonry.RegionId = 200;
				gweonry.Size = 48;
				gweonry.Level = 30;
				Zone z = WorldMgr.GetRegion(200).GetZone(200);
				gweonry.Position = z.ToRegionPosition(new Point(16334, 3384, 5200));
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

			ticketToArdee = CreateTicketTo("Ardee");
			ticketToMagMell = CreateTicketTo("Mag Mell");
			ticketToTirnaNog = CreateTicketTo("Tir na Nog");

			scrollHylvian = (GenericItemTemplate) GameServer.Database.FindObjectByKey(typeof (GenericItemTemplate), "scroll_for_hylvian");
			if (scrollHylvian == null)
			{
				scrollHylvian = new GenericItemTemplate();
				scrollHylvian.Name = "Scroll for Vault Keeper Hylvian";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + scrollHylvian.Name + " , creating it ...");

				scrollHylvian.Weight = 3;
				scrollHylvian.Model = 498;

				scrollHylvian.ItemTemplateID = "scroll_for_hylvian";

				scrollHylvian.IsDropable = false;
				scrollHylvian.IsSaleable = false;
				scrollHylvian.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(scrollHylvian);
			}


			receiptFreagus = (GenericItemTemplate) GameServer.Database.FindObjectByKey(typeof (GenericItemTemplate), "receipt_for_freagus");
			if (receiptFreagus == null)
			{
				receiptFreagus = new GenericItemTemplate();
				receiptFreagus.Name = "Receipt for Freagus";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + receiptFreagus.Name + " , creating it ...");

				receiptFreagus.Weight = 3;
				receiptFreagus.Model = 498;

				receiptFreagus.ItemTemplateID = "receipt_for_freagus";

				receiptFreagus.IsDropable = false;
				receiptFreagus.IsSaleable = false;
				receiptFreagus.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(receiptFreagus);
			}

			chestOfCoins = (GenericItemTemplate) GameServer.Database.FindObjectByKey(typeof (GenericItemTemplate), "small_chest_of_coins");
			if (chestOfCoins == null)
			{
				chestOfCoins = new GenericItemTemplate();
				chestOfCoins.Name = "Small Chest of Coins";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + chestOfCoins.Name + " , creating it ...");

				chestOfCoins.Weight = 15;
				chestOfCoins.Model = 602;

				chestOfCoins.ItemTemplateID = "small_chest_of_coins";

				chestOfCoins.IsDropable = false;
				chestOfCoins.IsSaleable = false;
				chestOfCoins.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(chestOfCoins);
			}

			letterAddrir = (GenericItemTemplate) GameServer.Database.FindObjectByKey(typeof (GenericItemTemplate), "letter_for_addrir");
			if (letterAddrir == null)
			{
				letterAddrir = new GenericItemTemplate();
				letterAddrir.Name = "Letter for Addrir";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + letterAddrir.Name + " , creating it ...");

				letterAddrir.Weight = 3;
				letterAddrir.Model = 498;

				letterAddrir.ItemTemplateID = "letter_for_addrir";

				letterAddrir.IsDropable = false;
				letterAddrir.IsSaleable = false;
				letterAddrir.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(letterAddrir);
			}

			assistantNecklace = (GenericItemTemplate) GameServer.Database.FindObjectByKey(typeof (GenericItemTemplate), "assistant_necklace");
			if (assistantNecklace == null)
			{
				assistantNecklace = new GenericItemTemplate();
				assistantNecklace.Name = "Assistant's Necklace";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + assistantNecklace.Name + " , creating it ...");

				assistantNecklace.Weight = 3;
				assistantNecklace.Model = 101;

				assistantNecklace.ItemTemplateID = "assistant_necklace";

				assistantNecklace.IsDropable = false;
				assistantNecklace.IsSaleable = false;
				assistantNecklace.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(assistantNecklace);
			}

			// item db check
			recruitsRoundShield = (ShieldTemplate) GameServer.Database.FindObjectByKey(typeof (ShieldTemplate), "recruits_round_shield_hib");
			if (recruitsRoundShield == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Recruit's Round Shield, creating it ...");
				recruitsRoundShield = new ShieldTemplate();
				recruitsRoundShield.Name = "Recruit's Round Shield (Hib)";
				recruitsRoundShield.Level = 4;

				recruitsRoundShield.Weight = 31;
				recruitsRoundShield.Model = 59; // studded Boots                

				recruitsRoundShield.ItemTemplateID = "recruits_round_shield_hib";
				recruitsRoundShield.Value = 400;

				recruitsRoundShield.IsDropable = true;
				recruitsRoundShield.IsSaleable = true;
				recruitsRoundShield.IsTradable = true;
				recruitsRoundShield.Color = 69;
				recruitsRoundShield.Speed = 1;
				recruitsRoundShield.DamagePerSecond = 1;

				recruitsRoundShield.Bonus = 1; // default bonus

				recruitsRoundShield.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 1));
				recruitsRoundShield.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 1));

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsRoundShield);
			}

			// item db check
			recruitsBracer = (BracerTemplate) GameServer.Database.FindObjectByKey(typeof (BracerTemplate), "recruits_silver_bracer");
			if (recruitsBracer == null)
			{
				recruitsBracer = new BracerTemplate();
				recruitsBracer.Name = "Recruit's Silver Bracer";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsBracer.Name + ", creating it ...");
				recruitsBracer.Level = 4;

				recruitsBracer.Weight = 10;
				recruitsBracer.Model = 130;

				recruitsBracer.ItemTemplateID = "recruits_silver_bracer";
				recruitsBracer.Value = 400;

				recruitsBracer.IsDropable = true;
				recruitsBracer.IsSaleable = true;
				recruitsBracer.IsTradable = true;

				recruitsBracer.Bonus = 1; // default bonus

				recruitsBracer.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 8));
				recruitsBracer.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 1));

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
			QuestMgr.AddQuestDescriptor(freagus, typeof(CityOfTirnaNogDescriptor));

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
			GameEventMgr.RemoveHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));

			GameEventMgr.RemoveHandler(addrir, GameLivingEvent.Interact, new DOLEventHandler(TalkToAddrir));
			GameEventMgr.RemoveHandler(addrir, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToAddrir));

			GameEventMgr.RemoveHandler(freagus, GameLivingEvent.Interact, new DOLEventHandler(TalkToFreagus));
			GameEventMgr.RemoveHandler(freagus, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToFreagus));

			GameEventMgr.RemoveHandler(hylvian, GameLivingEvent.Interact, new DOLEventHandler(TalkToHylvian));
			GameEventMgr.RemoveHandler(hylvian, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToHylvian));

			GameEventMgr.RemoveHandler(gweonry, GameLivingEvent.Interact, new DOLEventHandler(TalkToGweonry));

			/* Now we remove to freagus the possibility to give this quest to players */
			QuestMgr.RemoveQuestDescriptor(freagus, typeof(CityOfTirnaNogDescriptor));
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

			if (QuestMgr.CanGiveQuest(typeof(CityOfTirnaNog), player, freagus) <= 0)
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

			if (QuestMgr.CanGiveQuest(typeof(CityOfTirnaNog), player, freagus) <= 0)
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

			if (QuestMgr.CanGiveQuest(typeof(CityOfTirnaNog), player, freagus) <= 0)
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
								player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, receiptFreagus.CreateInstance());
								player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, ticketToArdee.CreateInstance());
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

			if (QuestMgr.CanGiveQuest(typeof(CityOfTirnaNog), player, freagus) <= 0)
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
							player.Out.SendCustomDialog("Will you travel to Tir na Nog for Freagus?", new CustomDialogResponse(CheckPlayerAcceptQuest));
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

			if (QuestMgr.CanGiveQuest(typeof(CityOfTirnaNog), player, freagus) <= 0)
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
			if (player.RegionId != 201)
				return;

			CityOfTirnaNog quest = (CityOfTirnaNog) player.IsDoingQuest(typeof (CityOfTirnaNog));
			if (quest == null)
				return;

			UseSlotEventArgs uArgs = (UseSlotEventArgs) args;

			GenericItem item = player.Inventory.GetItem((eInventorySlot)uArgs.Slot);
			if (item != null && item.Name == assistantNecklace.Name)
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
				Point pos = m_questPlayer.Position;
				pos.X += 50;
				pos.Y += 30;
				assistant.MoveTo((ushort)m_questPlayer.RegionId, pos, (ushort)m_questPlayer.Heading);
			}
			else
			{
				assistant = new GameMob();
				assistant.Model = 951;
				assistant.Name = m_questPlayer.Name + "'s Assistant";
				assistant.GuildName = "Part of " + questTitle + " Quest";
				assistant.Realm = m_questPlayer.Realm;
				assistant.RegionId = m_questPlayer.RegionId;
				assistant.Size = 25;
				assistant.Level = 5;
				Point pos = m_questPlayer.Position;
				pos.X += 50;
				pos.Y += 50;
				assistant.Position = pos;
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

		/* This is our callback hook that will be called when the player clicks
		 * on any button in the quest offer dialog. We check if he accepts or
		 * declines here...
		 */

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			//We recheck the qualification, because we don't talk to players
			//who are not doing the quest
			if (QuestMgr.CanGiveQuest(typeof(CityOfTirnaNog), player, freagus) <= 0)
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
				if (!QuestMgr.GiveQuestToPlayer(typeof(CityOfTirnaNog), player))
					return;

				freagus.SayTo(player, "Ah! Wonderful. I have several things for you for your trip into [Tir na Nog].");
				player.ReceiveItem(freagus, assistantNecklace.CreateInstance());
				player.ReceiveItem(freagus, chestOfCoins.CreateInstance());
				player.ReceiveItem(freagus, scrollHylvian.CreateInstance());
				player.ReceiveItem(freagus, ticketToTirnaNog.CreateInstance());
				player.GainExperience(7, 0, 0, true);

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
					default:
						return "[Step #" + Step + "] No Description entered for this step!";
				}
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
				if (gArgs.Target.Name == hylvian.Name && gArgs.Item.Name == scrollHylvian.Name)
				{
					hylvian.SayTo(player, "Ah, from Freagus. He wishes to make a deposit. Alright then, if you will, please hand me the Small Chest of Coins.");
					RemoveItemFromPlayer(hylvian, scrollHylvian);
					Step = 5;
					return;
				}
			}

			if (Step == 5 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == hylvian.Name && gArgs.Item.Name == chestOfCoins.Name)
				{
					hylvian.SayTo(player, "This chest is quite heavy. Business must be booming for Freagus. Now, if you will just wait but a moment, I will be sure to get a receipt for you to take back to him.");
					RemoveItemFromPlayer(hylvian, chestOfCoins);
					Step = 6;

					SendEmoteMessage(player, "Bhreagar begins to count the coins. When he is done, he pulls out a piece of parchment and writes a few things down. He then seals it with his personal seal.");
					return;
				}
			}

			if (Step == 8 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == freagus.Name && gArgs.Item.Name == receiptFreagus.Name)
				{
					freagus.SayTo(player, "Ah! My receipt. Thank you so much Lirone. Here, take this note back to Addrir in Mag Mell for me please. I would like to let him know what a fine person you are for assisting me twice now! Be safe on your journey back to Mag Mell.");

					RemoveItemFromPlayer(freagus, receiptFreagus);
					RemoveItemFromPlayer(freagus, assistantNecklace);

					GiveItemToPlayer(freagus, ticketToMagMell.CreateInstance());
					GiveItemToPlayer(freagus, letterAddrir.CreateInstance());
					Step = 9;
					return;
				}
			}

			if (Step == 9 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == addrir.Name && gArgs.Item.Name == letterAddrir.Name)
				{
					addrir.SayTo(player, "Hmm...What's this?");
					SendSystemMessage(player, "Addrir reads over the note carefully then returns his attentions to you.");
					addrir.Emote(eEmote.Ponder);

					RemoveItemFromPlayer(addrir, letterAddrir);
					Step = 10;
					return;
				}
			}

		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			//Give reward to player here ...            
			if (m_questPlayer.HasAbilityToUseItem(recruitsRoundShield.CreateInstance() as EquipableItem))
				GiveItemToPlayer(addrir, recruitsRoundShield.CreateInstance());
			else
				GiveItemToPlayer(addrir, recruitsBracer.CreateInstance());

			m_questPlayer.GainExperience(26, 0, 0, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 2, Util.Random(50)), "You recieve {0} as a reward.");

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));

		}

	}
}
