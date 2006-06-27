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
 * 1) Harlfug can be found at loc=52670,16862 East Svealand. 
 * 2) Zone into Jordheim and /USE the Assistant Necklace from Harlfug. 
 * 3) Talk with the Jordheim Assistant that appeared from using the necklace. Click on [vault keeper] option when it is given. 
 * 4) Hand Jarl Yuliwyf the Scroll for Vault Keeper Yuliwyf from Harlfug and then the Small Chest of Coins. 
 * 5) Click on the Jordheim Assistant to teleport back to the west gate. 
 * 6) Go back to Harlfug and hand him the Receipt for Harlfug from Jarl Yuliwyf. 
 * 7) Give Harlfug the Ticket to Mularn. 
 * 8) Give Dalikor the Note to Dalikor from Harlfug, chat with him a bit more for your reward.
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

namespace DOL.GS.Quests.Midgard
{

	/* The first thing we do, is to declare the quest requirement
	 * class linked with the new Quest. To do this, we derive 
	 * from the abstract class AbstractQuestDescriptor
	 */
	public class CityOfJordheimDescriptor : AbstractQuestDescriptor
	{
		/* This is the type of the quest class linked with 
		 * this requirement class, you must override the 
		 * base method like that
		 */
		public override Type LinkedQuestType
		{
			get { return typeof(CityOfJordheim); }
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
			if (player.HasFinishedQuest(typeof(ImportantDeliveryMid)) == 0)
				return false;

			if (!BaseDalikorQuest.CheckPartAccessible(player, typeof(CityOfJordheim)))
				return false;

			return base.CheckQuestQualification(player);
		}
	}


	/* The second thing we do, is to declare the class we create
	 * as Quest. We must make it persistant using attributes, to
	 * do this, we derive from the abstract class AbstractQuest
	 */
	[NHibernate.Mapping.Attributes.Subclass(NameType = typeof(CityOfJordheim), ExtendsType = typeof(AbstractQuest))] 
	public class CityOfJordheim : BaseDalikorQuest
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

		protected const string questTitle = "City of Jordheim";

		protected static GameLocation warriorTrainer = new GameLocation("Warrior Trainer", 101, 29902, 35916, 8005, 2275);
		protected static GameLocation savageTrainer = new GameLocation("Savage Trainer", 101, 28882, 35823, 8021, 2662);
		protected static GameLocation skaldTrainer = new GameLocation("Skald Trainer", 101, 30395, 34943, 8005, 4004);
		protected static GameLocation thaneTrainer = new GameLocation("Thane Trainer", 101, 29811, 34936, 8005, 159);

		protected static GameLocation westGates = new GameLocation("West Gates", 101, 36017, 32659, 8005, 1045);

		protected static GameLocation vaultKeeper = new GameLocation("Vault Keeper", 101, 31929, 28279, 8819, 2013);
		protected static GameLocation berserkerTrainer = new GameLocation("Berserker Trainer", 101, 30327, 35598, 8002, 1706);

		private static GameNPC dalikor = null;
		private static GameNPC yuliwyf = null;
		private static GameNPC harlfug = null;

		private GameNPC assistant = null;
		private GameTimer assistantTimer = null;

		private static TravelTicketTemplate ticketToMularn = null;
        private static GenericItemTemplate scrollYuliwyf = null;
        private static GenericItemTemplate receiptHarlfug = null;
        private static GenericItemTemplate letterDalikor = null;
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

			dalikor = GetDalikor();

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Jarl Yuliwyf", eRealm.Midgard);
			if (npcs.Length == 0)
			{
				yuliwyf = new GameMob();
				yuliwyf.Model = 159;
				yuliwyf.Name = "Jarl Yuliwyf";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + yuliwyf.Name + ", creating ...");
				yuliwyf.GuildName = "Part of " + questTitle + " Quest";
				yuliwyf.Realm = (byte) eRealm.Midgard;
				yuliwyf.RegionId = 101;
				yuliwyf.Size = 51;
				yuliwyf.Level = 50;
				yuliwyf.Position = new Point(31929, 28279, 8819);
				yuliwyf.Heading = 2013;
				yuliwyf.EquipmentTemplateID = "5101262";
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					yuliwyf.SaveIntoDatabase();

				yuliwyf.AddToWorld();
			}
			else
				yuliwyf = npcs[0];

			npcs = WorldMgr.GetNPCsByName("Harlfug", eRealm.Midgard);
			if (npcs.Length == 0)
			{
				harlfug = new GameStableMaster();
				harlfug.Model = 215;
				harlfug.Name = "Harlfug";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + harlfug.Name + ", creating her ...");
				harlfug.GuildName = "Stable Master";
				harlfug.Realm = (byte) eRealm.Midgard;
				harlfug.RegionId = 100;
				harlfug.Size = 52;
				harlfug.Level = 41;
				harlfug.Position = new Point(773458, 754240, 4600);
				harlfug.Heading = 2707;
				harlfug.EquipmentTemplateID = "5100798";

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					harlfug.SaveIntoDatabase();
				harlfug.AddToWorld();
			}
			else
				harlfug = npcs[0];

			#endregion

			#region defineItems

			ticketToMularn = CreateTicketTo("Mularn");

			scrollYuliwyf = (GenericItemTemplate)GameServer.Database.FindObjectByKey(typeof(GenericItemTemplate), "scroll_for_yuliwyf");
			if (scrollYuliwyf == null)
			{
				scrollYuliwyf = new GenericItemTemplate();
				scrollYuliwyf.Name = "Scroll for Jarl Yuliwyf";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + scrollYuliwyf.Name + " , creating it ...");

				scrollYuliwyf.Weight = 3;
				scrollYuliwyf.Model = 498;

				scrollYuliwyf.ItemTemplateID = "scroll_for_yuliwyf";

				scrollYuliwyf.IsDropable = false;
				scrollYuliwyf.IsSaleable = false;
				scrollYuliwyf.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(scrollYuliwyf);
			}


			receiptHarlfug = (GenericItemTemplate)GameServer.Database.FindObjectByKey(typeof(GenericItemTemplate), "receipt_for_harlfug");
			if (receiptHarlfug == null)
			{
				receiptHarlfug = new GenericItemTemplate();
				receiptHarlfug.Name = "Receipt for Harlfug";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + receiptHarlfug.Name + " , creating it ...");

				receiptHarlfug.Weight = 3;
				receiptHarlfug.Model = 498;

				receiptHarlfug.ItemTemplateID = "receipt_for_harlfug";

				receiptHarlfug.IsDropable = false;
				receiptHarlfug.IsSaleable = false;
				receiptHarlfug.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(receiptHarlfug);
			}

			chestOfCoins = (GenericItemTemplate)GameServer.Database.FindObjectByKey(typeof(GenericItemTemplate), "small_chest_of_coins");
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

			letterDalikor = (GenericItemTemplate)GameServer.Database.FindObjectByKey(typeof(GenericItemTemplate), "letter_for_dalikor");
			if (letterDalikor == null)
			{
				letterDalikor = new GenericItemTemplate();
				letterDalikor.Name = "Letter for Dalikor";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + letterDalikor.Name + " , creating it ...");

				letterDalikor.Weight = 3;
				letterDalikor.Model = 498;

				letterDalikor.ItemTemplateID = "letter_for_dalikor";

				letterDalikor.IsDropable = false;
				letterDalikor.IsSaleable = false;
				letterDalikor.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(letterDalikor);
			}

			assistantNecklace = (GenericItemTemplate)GameServer.Database.FindObjectByKey(typeof(GenericItemTemplate), "assistant_necklace");
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
			recruitsRoundShield = (ShieldTemplate)GameServer.Database.FindObjectByKey(typeof(ShieldTemplate), "recruits_round_shield_mid");
			if (recruitsRoundShield == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Recruit's Round Shield (Mid), creating it ...");
				recruitsRoundShield = new ShieldTemplate();
				recruitsRoundShield.Name = "Recruit's Round Shield (Mid)";
				recruitsRoundShield.Level = 4;

				recruitsRoundShield.Weight = 31;
				recruitsRoundShield.Model = 59; // studded Boots                

				recruitsRoundShield.ItemTemplateID = "recruits_round_shield_mid";
				recruitsRoundShield.Value = 400;

				recruitsRoundShield.IsDropable = true;
				recruitsRoundShield.IsSaleable = true;
				recruitsRoundShield.IsTradable = true;
				recruitsRoundShield.Color = 61;

				recruitsRoundShield.DamagePerSecond = 1;
				recruitsRoundShield.Speed = 1;
				recruitsRoundShield.Size = eShieldSize.Small;

				recruitsRoundShield.Bonus = 1; // default bonus

				recruitsRoundShield.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 3));
				recruitsRoundShield.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 1));

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsRoundShield);
			}

			// item db check
			recruitsBracer = (BracerTemplate)GameServer.Database.FindObjectByKey(typeof(BracerTemplate), "recruits_silver_bracer");
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

			GameEventMgr.AddHandler(dalikor, GameLivingEvent.Interact, new DOLEventHandler(TalkToDalikor));
			GameEventMgr.AddHandler(dalikor, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToDalikor));

			GameEventMgr.AddHandler(harlfug, GameLivingEvent.Interact, new DOLEventHandler(TalkToHarlfug));
			GameEventMgr.AddHandler(harlfug, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToHarlfug));

			GameEventMgr.AddHandler(yuliwyf, GameLivingEvent.Interact, new DOLEventHandler(TalkToYuliwyf));
			GameEventMgr.AddHandler(yuliwyf, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToYuliwyf));

			/* Now we bring to harlfug the possibility to give this quest to players */
			QuestMgr.AddQuestDescriptor(harlfug, typeof(CityOfJordheimDescriptor));

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

			GameEventMgr.RemoveHandler(harlfug, GameLivingEvent.Interact, new DOLEventHandler(TalkToHarlfug));
			GameEventMgr.RemoveHandler(harlfug, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToHarlfug));

			GameEventMgr.RemoveHandler(yuliwyf, GameLivingEvent.Interact, new DOLEventHandler(TalkToYuliwyf));
			GameEventMgr.RemoveHandler(yuliwyf, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToYuliwyf));

			/* Now we remove to harlfug the possibility to give this quest to players */
			QuestMgr.RemoveQuestDescriptor(harlfug, typeof(CityOfJordheimDescriptor));
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

			if (QuestMgr.CanGiveQuest(typeof(CityOfJordheim), player, harlfug) <= 0)
				return;

			//We also check if the player is already doing the quest
			CityOfJordheim quest = player.IsDoingQuest(typeof (CityOfJordheim)) as CityOfJordheim;

			dalikor.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					switch (quest.Step)
					{
						case 9:
							dalikor.SayTo(player, "This note puts you in high regard Eeinken. I'm glad to see that you are so willing to serve Midgard and so ready to help her citizens. For your selfless acts of servitude, I have this [reward] for you.");
							quest.Step = 10;
							break;
						case 10:
							dalikor.SayTo(player, "You have shown that you are all that a recruit can hope to be. This token of my esteem and gratitude is but a fraction of the gratitude Midgard will show you once you are a mighty warrior. Do not go far my young friend. I have more for you to do.");
							quest.FinishQuest();
							dalikor.SayTo(player, "Don't go too far recruit, we have more work to do.");
							break;
						default:
							dalikor.SayTo(player, "Ah, welcome back recruit Eeinken. You've been gone a while. Are things alright?");
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
							dalikor.SayTo(player, "You have shown that you are all that a recruit can hope to be. This token of my esteem and gratitude is but a fraction of the gratitude Midgard will show you once you are a mighty warrior. Do not go far my young friend. I have more for you to do.");
							if (quest.Step == 10)
							{
								quest.FinishQuest();
								dalikor.SayTo(player, "Don't go too far recruit, we have more work to do.");
							}
							break;
					}
				}

			}
		}

		protected static void TalkToYuliwyf(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if (QuestMgr.CanGiveQuest(typeof(CityOfJordheim), player, harlfug) <= 0)
				return;

			//We also check if the player is already doing the quest
			CityOfJordheim quest = player.IsDoingQuest(typeof (CityOfJordheim)) as CityOfJordheim;

			yuliwyf.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					if (quest.Step < 6)
					{
						dalikor.SayTo(player, "Welcome my friend. I am Vault Keeper Yuliwyf. How may I assist you this day?");
					}
					else if (quest.Step == 6)
					{
						dalikor.SayTo(player, "I believe I am [done] counting the coins.");
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
							dalikor.SayTo(player, "Harlfug's coins will be safe and sound for when he comes to Jordheim next time.Please, take this receipt back to him for me. Good luck.");
							if (quest.Step == 6)
							{
								player.ReceiveItem(dalikor, receiptHarlfug.CreateInstance());
								quest.Step = 7;
							}
							break;
					}
				}
			}
		}

		protected static void TalkToHarlfug(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if (QuestMgr.CanGiveQuest(typeof(CityOfJordheim), player, harlfug) <= 0)
				return;

			//We also check if the player is already doing the quest
			CityOfJordheim quest = player.IsDoingQuest(typeof (CityOfJordheim)) as CityOfJordheim;

			harlfug.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					harlfug.SayTo(player, "I wanted to thank you again for delivering these vegetables to me. My horses and my family will be able to eat well these next few days. However, I am in need of yet another [favor], if you are so inclined.");
				}
				return;
			}
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				if (quest == null)
				{
					//Do some small talk :)
					switch (wArgs.Text)
					{
						case "favor":
							harlfug.SayTo(player, "I have here a chest of coins that I need taken to the Vault Keeper inside of Jordheim. Since my stable is fairly busy, I don't have time to leave it, and by the time I do get a moment, the Vault Keeper has gone home for the day. It's a simple [errand], really.");
							break;
						case "errand":
							harlfug.SayTo(player, "I trust that you won't steal from me, so what do you say? Will you [do this] for me or not?");
							break;
						case "do this":
							player.Out.SendCustomDialog("Will you take these coins to the Vault Keeper Yuliwyf in Jordheim?", new CustomDialogResponse(CheckPlayerAcceptQuest));
							break;
					}
				}
				else
				{
					//Do some small talk :)
					switch (wArgs.Text)
					{
						case "necklace":
							harlfug.SayTo(player, "The necklace was made by my wife, who is a Spiritmaster. She is currently helping out with a few things in Aegirhamn, or else she could take the money to the vault keeper, hehe. Anyhow, you'll need to USE the necklace once you're inside Jordheim. I'm sure your journal there will be able to help you out. Don't worry, you'll get the hang of it. Be sure to give the Vault Keeper the scroll. Good luck, and thank you again.");
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

			if (QuestMgr.CanGiveQuest(typeof(CityOfJordheim), player, harlfug) <= 0)
				return;

			//We also check if the player is already doing the quest
			CityOfJordheim quest = player.IsDoingQuest(typeof (CityOfJordheim)) as CityOfJordheim;

			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null && quest.assistant == sender)
				{
					quest.assistant.TurnTo(player);
					quest.assistant.SayTo(player, "Greetings to you. I am here to assist you on your journey through Jordheim. Please listen [carefully] to my instructions.");
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
							quest.assistant.SayTo(player, "I have a limited time duration. I will only be around for half a day (15 real life minutes), so please use me wisely. I can teleport you to various [trainers] within Jordheim.");
							break;
						case "trainers":
							quest.assistant.SayTo(player, "I know that you have been given the task of delivering Harlfug's coins to the vault keeper. I can teleport you there are well. All you need to do is make a [choice].");
							break;
						case "choice":
							quest.assistant.SayTo(player, "Where would you like to go to? The [skald] trainer, the [warrior] trainer, the [savage] trainer, the [thane] trainer, the [berserker] trainer, the [vault keeper] or the [west gates]?");
							break;

						case "skald":
							quest.TeleportTo(skaldTrainer);
							break;
						case "warrior":
							quest.TeleportTo(warriorTrainer);
							break;
						case "savage":
							quest.TeleportTo(savageTrainer);
							break;
						case "thane":
							quest.TeleportTo(thaneTrainer);
							break;
						case "berserker":
							quest.TeleportTo(berserkerTrainer);
							break;
						case "west gates":
							quest.TeleportTo(westGates);
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

			CityOfJordheim quest = player.IsDoingQuest(typeof (CityOfJordheim)) as CityOfJordheim;
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

			CityOfJordheim quest = player.IsDoingQuest(typeof (CityOfJordheim)) as CityOfJordheim;
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

			// assistant works only in camelot...
			if (player.RegionId != 101)
				return;

			CityOfJordheim quest = (CityOfJordheim) player.IsDoingQuest(typeof (CityOfJordheim));
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
			if (QuestMgr.CanGiveQuest(typeof(CityOfJordheim), player, harlfug) <= 0)
				return;

			CityOfJordheim quest = player.IsDoingQuest(typeof (CityOfJordheim)) as CityOfJordheim;

			if (quest != null)
				return;

			if (response == 0x00)
			{
				SendReply(player, "Oh well, if you change your mind, please come back!");
			}
			else
			{
				//Check if we can add the quest!
				if (!QuestMgr.GiveQuestToPlayer(typeof(CityOfJordheim), player, harlfug))
					return;

				harlfug.SayTo(player, "Oh thank you Eeinken. This means a lot to me. Here, take this chest of coins, this scroll and this [necklace].");

				player.ReceiveItem(harlfug, assistantNecklace.CreateInstance());
				player.ReceiveItem(harlfug, chestOfCoins.CreateInstance());
				player.ReceiveItem(harlfug, scrollYuliwyf.CreateInstance());
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
						return "[Step #1] Harlfug has given you a small chest of coins that he needs taken to the vault keeper. Make your way inside the Jordheim gates and USE the necklace. Jordheim is across the bridge. If you lose the necklace, return to Harlfug for a new one.";
					case 2:
						return "[Step #2] You have successfully USED the necklace. Speak with the Jordheim Assistant for further instructions.";
					case 3:
						return "[Step #3] Chose the destination you wish to know more about. If you do not need the Assistant, tell him to [go away] If you go straight to the Vault Keeper, be sure to hand him the scroll from Harlfug. If you go link dead and the assistant is gone, go straight to the vault keeper.";
					case 4:
						return "[Step #4] Speak with Vault Keeper Yuliwyf.. Be sure to hand him the scroll from Harlfug.";
					case 5:
						return "[Step #5] Hand the Small Chest of Coins over to Vault Keeper Yuliwyf.";
					case 6:
						return "[Step #6] Wait for the Vault Keeper to finish counting the coins. If he stops speaking with you, ask him if he is [done] counting the coins.";
					case 7:
						return "[Step #7] Take the Receipt to Harlfug outside the Jordheim West Gates. If he is still with you, ask your Jordheim Assistant to transport you to the [west gates].";
					case 8:
						return "[Step #8] Take the horse back to Mularn. Be sure to give Dalikor the note from Harlfug.";
					case 9:
						return "[Step #9] Wait for Dalikor to finish reading the note from Harlfug. If he stops responding ask him if he is [finished] with the note.";
					case 10:
						return "[Step #10] Wait for Dalikor to reward you. If your he stops speaking with you at any time, ask him if there is a [reward] for your efforts.";
					default:
						return "[Step #" + Step + "] No Description entered for this step!";
				}
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof (CityOfJordheim)) == null)
				return;

			if (Step <= 4 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == yuliwyf.Name && gArgs.Item.Name == scrollYuliwyf.Name)
				{
					yuliwyf.SayTo(player, "What's this? Ah! From Stable Master Harlfug. Excellent. It looks like he wishes to make a deposit. If that is indeed the case, please hand me the Small Chest of Coins.");
					RemoveItemFromPlayer(yuliwyf, scrollYuliwyf);
					Step = 5;
					return;
				}
			}

			if (Step == 5 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == yuliwyf.Name && gArgs.Item.Name == chestOfCoins.Name)
				{
					yuliwyf.SayTo(player, "My my, this is quite heavy. Business must be very good for Harlfug. If you don't mind waiting for just a few moments, I will count these coins and give you a receipt.");
					RemoveItemFromPlayer(yuliwyf, chestOfCoins);
					Step = 6;
					return;
				}
			}

			if (Step == 7 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == harlfug.Name && gArgs.Item.Name == receiptHarlfug.Name)
				{
					harlfug.SayTo(player, "Ah, fantastic. I'm glad to know my money is now in a safe place. Thank you so much for doing that for me, and I hope the trip into Jordhiem was informative for you. Here, take this letter back to Dalikor in Mularn. I want for him to know what a fantastic job you did for me by delivering these vegetables from Haggerfel, and for taking care of some business in Jordheim for me. Thank you again Eeinken. I hope we speak again soon.");

					RemoveItemFromPlayer(harlfug, receiptHarlfug);
					RemoveItemFromPlayer(harlfug, assistantNecklace);

					GiveItemToPlayer(harlfug, ticketToMularn.CreateInstance());
					GiveItemToPlayer(harlfug, letterDalikor.CreateInstance());
					Step = 8;
					return;
				}
			}

			if (Step == 8 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == dalikor.Name && gArgs.Item.Name == letterDalikor.Name)
				{
					dalikor.SayTo(player, "Ah, from Harlfug. Let me see what is says. One moment please.");
					SendSystemMessage(player, "Dalikor reads the note from Harlfug carefully.");
					dalikor.Emote(eEmote.Ponder);

					RemoveItemFromPlayer(dalikor, letterDalikor);
					Step = 9;
					return;
				}
			}

		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			//Give reward to player here ...            
			if (m_questPlayer.HasAbilityToUseItem(recruitsRoundShield.CreateInstance() as EquipableItem))
				GiveItemToPlayer(dalikor, recruitsRoundShield.CreateInstance());
			else
				GiveItemToPlayer(dalikor, recruitsBracer.CreateInstance());

			m_questPlayer.GainExperience(26, 0, 0, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 2, Util.Random(50)), "You recieve {0} as a reward.");

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));

		}

	}
}
