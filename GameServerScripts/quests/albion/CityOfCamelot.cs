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

namespace DOL.GS.Quests.Albion
{
	/* The first thing we do, is to declare the class we create
	 * as Quest. To do this, we derive from the abstract class
	 * AbstractQuest
	 * 	 
	 */

	public class CityOfCamelot : BaseFrederickQuest
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

		protected const string questTitle = "City of Camelot";
		protected const int minimumLevel = 1;
		protected const int maximumLevel = 1;

		protected static GameLocation armsManTrainer = new GameLocation("Armsman Trainer", 10, 30985, 31520, 8216, 0);
		protected static GameLocation reaverTrainer = new GameLocation("Reaver Trainer", 10, 32545, 26900, 7830, 0);
		protected static GameLocation paladinTrainer = new GameLocation("Paladin Trainer", 10, 38920, 27845, 8636, 0);
		protected static GameLocation mercenaryTrainer = new GameLocation("Mercenary Trainer", 10, 32826, 26831, 7995, 0);

		protected static GameLocation vaultKeeper = new GameLocation("Vault Keeper", 10, 35408, 23830, 8751, 0);
		protected static GameLocation mainGates = new GameLocation("Main Gates", 10, 40069, 26463, 8255, 0);

		private static GameNPC masterFrederick = null;
		private static GameNPC lordUrqhart = null;
		private static GameNPC bombard = null;

		private GameNPC assistant = null;
		private RegionTimer assistantTimer = null;

		private static ItemTemplate ticketToCotswold = null;
		private static ItemTemplate scrollUrqhart = null;
		private static ItemTemplate receiptBombard = null;
		private static ItemTemplate letterFrederick = null;
		private static ItemTemplate assistantNecklace = null;
		private static ItemTemplate chestOfCoins = null;
		private static ItemTemplate recruitsRoundShield = null;
		private static ItemTemplate recruitsBracer = null;


		/* We need to define the constructors from the base class here, else there might be problems
		 * when loading this quest...
		 */
		public CityOfCamelot() : base()
		{
		}

		public CityOfCamelot(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public CityOfCamelot(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public CityOfCamelot(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Lord Urqhart", eRealm.Albion);
			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Lord Urqhart, creating ...");
				lordUrqhart = new GameNPC();
				lordUrqhart.Model = 79;
				lordUrqhart.Name = "Lord Urqhart";
				lordUrqhart.GuildName = "Vault Keeper";
				lordUrqhart.Realm = (byte) eRealm.Albion;
				lordUrqhart.CurrentRegionID = 10;
				lordUrqhart.Size = 49;
				lordUrqhart.Level = 50;
				lordUrqhart.X = 35500;
				lordUrqhart.Y = 23857;
				lordUrqhart.Z = 8751;
				lordUrqhart.Heading = 603;
# warning TODO get right number
				//lordUrqhart.EquipmentTemplateID = "1707104";
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					lordUrqhart.SaveIntoDatabase();

				lordUrqhart.AddToWorld();
			}
			else
				lordUrqhart = npcs[0];

			npcs = WorldMgr.GetNPCsByName("Bombard", eRealm.Albion);
			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bombard, creating her ...");
				bombard = new GameStableMaster();
				bombard.Model = 8;
				bombard.Name = "Bombard";
				bombard.GuildName = "Stable Master";
				bombard.Realm = (byte) eRealm.Albion;
				bombard.CurrentRegionID = 1;
				bombard.Size = 49;
				bombard.Level = 4;
				bombard.X = 515718;
				bombard.Y = 496739;
				bombard.Z = 3352;
				bombard.Heading = 2500;

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					bombard.SaveIntoDatabase();
				bombard.AddToWorld();
			}
			else
				bombard = npcs[0];

			#endregion

			#region defineItems

			ticketToCotswold = CreateTicketTo("ticket to Camelot Hills", "");

			scrollUrqhart = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "scroll_for_urqhart");
			if (scrollUrqhart == null)
			{
				scrollUrqhart = new ItemTemplate();
				scrollUrqhart.Name = "Scroll for Vault Keeper Urqhart";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + scrollUrqhart.Name + " , creating it ...");

				scrollUrqhart.Weight = 3;
				scrollUrqhart.Model = 498;

				scrollUrqhart.Object_Type = (int) eObjectType.GenericItem;

				scrollUrqhart.Id_nb = "scroll_for_urqhart";
				scrollUrqhart.IsPickable = true;
				scrollUrqhart.IsDropable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(scrollUrqhart);
			}


			receiptBombard = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "receipt_for_bombard");
			if (receiptBombard == null)
			{
				receiptBombard = new ItemTemplate();
				receiptBombard.Name = "Receipt for Bombard";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + receiptBombard.Name + " , creating it ...");

				receiptBombard.Weight = 3;
				receiptBombard.Model = 498;

				receiptBombard.Object_Type = (int) eObjectType.GenericItem;

				receiptBombard.Id_nb = "receipt_for_bombard";
				receiptBombard.IsPickable = true;
				receiptBombard.IsDropable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(receiptBombard);
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

			letterFrederick = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "letter_for_frederick");
			if (letterFrederick == null)
			{
				letterFrederick = new ItemTemplate();
				letterFrederick.Name = "Letter for Frederick";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + letterFrederick.Name + " , creating it ...");

				letterFrederick.Weight = 3;
				letterFrederick.Model = 498;

				letterFrederick.Object_Type = (int) eObjectType.GenericItem;

				letterFrederick.Id_nb = "letter_for_frederick";
				letterFrederick.IsPickable = true;
				letterFrederick.IsDropable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(letterFrederick);
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
			recruitsRoundShield = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "recruits_round_shield");
			if (recruitsRoundShield == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Recruit's Round Shield, creating it ...");
				recruitsRoundShield = new ItemTemplate();
				recruitsRoundShield.Name = "Recruit's Round Shield";
				recruitsRoundShield.Level = 4;

				recruitsRoundShield.Weight = 31;
				recruitsRoundShield.Model = 59; // studded Boots                

				recruitsRoundShield.Object_Type = 0x2A; // (int)eObjectType.Shield;
				recruitsRoundShield.Item_Type = (int) eEquipmentItems.LEFT_HAND;
				recruitsRoundShield.Id_nb = "recruits_round_shield";
				recruitsRoundShield.Gold = 0;
				recruitsRoundShield.Silver = 4;
				recruitsRoundShield.Copper = 0;
				recruitsRoundShield.IsPickable = true;
				recruitsRoundShield.IsDropable = true;
				recruitsRoundShield.Color = 36;
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

			GameEventMgr.AddHandler(masterFrederick, GameLivingEvent.Interact, new DOLEventHandler(TalkToMasterFrederick));
			GameEventMgr.AddHandler(masterFrederick, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterFrederick));

			GameEventMgr.AddHandler(bombard, GameLivingEvent.Interact, new DOLEventHandler(TalkToBombard));
			GameEventMgr.AddHandler(bombard, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToBombard));

			GameEventMgr.AddHandler(lordUrqhart, GameLivingEvent.Interact, new DOLEventHandler(TalkToUrqhart));
			GameEventMgr.AddHandler(lordUrqhart, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToUrqhart));

			/* Now we bring to bombard the possibility to give this quest to players */
			bombard.AddQuestToGive(typeof (CityOfCamelot));

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

			GameEventMgr.RemoveHandler(bombard, GameLivingEvent.Interact, new DOLEventHandler(TalkToBombard));
			GameEventMgr.RemoveHandler(bombard, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToBombard));

			GameEventMgr.RemoveHandler(lordUrqhart, GameLivingEvent.Interact, new DOLEventHandler(TalkToUrqhart));
			GameEventMgr.RemoveHandler(lordUrqhart, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToUrqhart));

			/* Now we remove to bombard the possibility to give this quest to players */
			bombard.RemoveQuestToGive(typeof (CityOfCamelot));
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

			if(bombard.CanGiveQuest(typeof (CityOfCamelot), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			CityOfCamelot quest = player.IsDoingQuest(typeof (CityOfCamelot)) as CityOfCamelot;

			masterFrederick.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					//Player is not doing the quest...
					if (quest.Step == 9)
					{
						masterFrederick.SayTo(player, "This note puts you in high regard Vinde. I'm glad to see that you are so willing to serve Albion and so ready to help her citizens. For your selfless acts of servitude, I have this [reward] for you.");
						quest.Step = 10;
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
							masterFrederick.SayTo(player, "You have shown that you are all that a recruit can hope to be. This token of my esteem and gratitude is but a fraction of the gratitude Albion will show you once you are a mighty warrior. Do not go far my young friend. I have more for you to do.");
							if (quest.Step == 10)
							{
								quest.FinishQuest();
								masterFrederick.SayTo(player, "Don't wander too far my young recruit. There are more things for you to accomplish. Visit me when you have reached your second season.");
							}
							break;
					}
				}

			}
		}

		protected static void TalkToUrqhart(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(bombard.CanGiveQuest(typeof (CityOfCamelot), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			CityOfCamelot quest = player.IsDoingQuest(typeof (CityOfCamelot)) as CityOfCamelot;

			lordUrqhart.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					if (quest.Step < 6)
					{
						lordUrqhart.SayTo(player, "Greetings friend. How may I be of assistance to you today?");
					}
					else if (quest.Step == 6)
					{
						lordUrqhart.SayTo(player, "Ah, I am now [done].");
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
							lordUrqhart.SayTo(player, "Alright, here you are my friend, as promised. Please be sure to return it to Bombard. Have a nice day!");
							if (quest.Step == 6)
							{
								GiveItem(lordUrqhart, player, receiptBombard);
								quest.Step = 7;
							}
							break;
					}
				}
			}
		}

		protected static void TalkToBombard(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(bombard.CanGiveQuest(typeof (CityOfCamelot), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			CityOfCamelot quest = player.IsDoingQuest(typeof (CityOfCamelot)) as CityOfCamelot;

			bombard.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					bombard.SayTo(player, "I wanted to thank you again for delivering these vegetables to me. My horses and my family will be able to eat well these next few days. However, I am in need of yet another [favor], if you are so inclined.");
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
							bombard.SayTo(player, "I have here a chest of coins that I need taken to the Vault Keeper inside of Camelot City. Since my stable is fairly busy, I don't have time to leave it, and by the time I do get a moment, the banker has gone home for the day. It's a simple [errand], really.");
							break;
						case "errand":
							bombard.SayTo(player, "I trust that you won't steal from me, so what do you say? Will you [do this] for me or not?");
							break;
						case "do this":
							player.Out.SendQuestSubscribeCommand(bombard, QuestMgr.GetIDForQuestType(typeof(CityOfCamelot)), "Will you take these coins to Vault Keeper Urqhart in Camelot?");
							break;
					}
				}
				else
				{
					//Do some small talk :)
					switch (wArgs.Text)
					{
						case "necklace":
							bombard.SayTo(player, "The necklace was made by my wife, a Cabalist with the Academy. She is currently helping out with a few things in Avalon Marsh, or else she could take the money to the vault keeper, hehe. Anyhow, you'll need to USE the necklace once you're inside Camelot City. I'm sure your journal there will be able to help you out. Don't worry, you'll get the hang of it. Be sure to give the Vault Keeper the scroll. Good luck, and thank you again.");
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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(CityOfCamelot)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

		protected static void TalkToAssistant(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(bombard.CanGiveQuest(typeof (CityOfCamelot), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			CityOfCamelot quest = player.IsDoingQuest(typeof (CityOfCamelot)) as CityOfCamelot;

			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null && quest.assistant == sender)
				{
					quest.assistant.TurnTo(player);
					quest.assistant.SayTo(player, "Greetings to you. I am here to assist you on your journey through Camelot. Please listen [carefully] to my instructions.");
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
							quest.assistant.SayTo(player, "I have a limited time duration. I will only be around for about an hour (2 real life minutes), so please use me wisely. I can teleport you to various [trainers] within Camelot.");
							break;
						case "trainers":
							quest.assistant.SayTo(player, "I know that you have been given the task of delivering Bombard's coins to the vault keeper. I can teleport you there as well. All you need to do is make a [choice].");
							break;
						case "choice":
							quest.assistant.SayTo(player, "Where would you like to go to? The [paladin] trainer; the [armsman] trainer; the [reaver] trainer; the [mercenary] trainer; to the [vault keeper] or back tp the [main gates]?");
							break;

						case "paladin":
							quest.TeleportTo(paladinTrainer);
							break;
						case "armsman":
							quest.TeleportTo(armsManTrainer);
							break;
						case "reaver":
							quest.TeleportTo(reaverTrainer);
							break;
						case "mercenary":
							quest.TeleportTo(mercenaryTrainer);
							break;
						case "main gates":
							quest.TeleportTo(mainGates);
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

			CityOfCamelot quest = player.IsDoingQuest(typeof (CityOfCamelot)) as CityOfCamelot;
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

			CityOfCamelot quest = player.IsDoingQuest(typeof (CityOfCamelot)) as CityOfCamelot;
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
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			// assistant works only in camelot...
			if (player.CurrentRegionID != 10)
				return;

			CityOfCamelot quest = (CityOfCamelot) player.IsDoingQuest(typeof (CityOfCamelot));
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

		protected virtual int CreateAssistant(RegionTimer timer)
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
			if (player.IsDoingQuest(typeof (CityOfCamelot)) != null)
				return true;

			// This checks below are only performed is player isn't doing quest already

			if (player.HasFinishedQuest(typeof (ImportantDelivery)) == 0)
				return false;

			if (!CheckPartAccessible(player, typeof (CityOfCamelot)))
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
			CityOfCamelot quest = player.IsDoingQuest(typeof (CityOfCamelot)) as CityOfCamelot;

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
			if(bombard.CanGiveQuest(typeof (CityOfCamelot), player)  <= 0)
				return;

			CityOfCamelot quest = player.IsDoingQuest(typeof (CityOfCamelot)) as CityOfCamelot;

			if (quest != null)
				return;

			if (response == 0x00)
			{
				SendReply(player, "Oh well, if you change your mind, please come back!");
			}
			else
			{
				//Check if we can add the quest!
				if (!bombard.GiveQuest(typeof (CityOfCamelot), player, 1))
					return;

				bombard.SayTo(player, "Oh thank you Vinde. This means a lot to me. Here, take this chest of coins, this scroll and this [necklace].");

				GiveItem(bombard, player, assistantNecklace);
				GiveItem(bombard, player, chestOfCoins);
				GiveItem(bombard, player, scrollUrqhart);
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
						return "[Step #1] Bombard has given you a Chest of Coins he needs taken to the vault keeper. Make your way inside the gates of Camelot and /USE the item Bombard gave you. The gates of Camelot are between the two large stone soldiers near Bombard.";
					case 2:
						return "[Step #2] You have successfully USED the necklace. Speak with the Camelot Assistant for further instructions.";
					case 3:
						return "[Step #3] Chose the destination you wish to know more about. If you do not need the Assistant, tell him to [go away] If you go straight to the Vault Keeper, be sure to hand him the scroll from Bombard.";
					case 4:
						return "[Step #4] Speak with Lord Urqhart. Be sure to give him the note Bombard gave you.";
					case 5:
						return "[Step #5] Hand the Small Chest of Coins over to Lord Urqhart.";
					case 6:
						return "[Step #6] Wait for the Vault Keeper to finish counting the coins. If he stops speaking with you, ask him if he is [done] counting the coins.";
					case 7:
						return "[Step #7] Take the Receipt to Bombard outside the Camelot Gates. Ask your Camelot Assistant to transport you back to the [main gates].";
					case 8:
						return "[Step #8] Take the horse back to Cotswold. Be sure to give Master Frederick the note from Bombard.";
					case 9:
						return "[Step #9] Wait for Master Frederick to finish reading the note from Bombard. If he stops speaking with you, ask him if he is [done] with the letter.";
					case 10:
						return "[Step #10] Wait for Master Frederick to reward you. If your trainer stops speaking with you at any time, ask him if there is a [reward] for your efforts.";

				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player==null || player.IsDoingQuest(typeof (CityOfCamelot)) == null)
				return;

			if (Step <= 4 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == lordUrqhart.Name && gArgs.Item.Id_nb == scrollUrqhart.Id_nb)
				{
					lordUrqhart.SayTo(player, "Ah, a note from Bombard. Excellent. I see he wishes to make a deposit. Alright then, just hand me the chest please.");
					RemoveItem(lordUrqhart, player, scrollUrqhart);
					Step = 5;
					return;
				}
			}

			if (Step == 5 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == lordUrqhart.Name && gArgs.Item.Id_nb == chestOfCoins.Id_nb)
				{
					lordUrqhart.SayTo(player, "My this is heavy. Business must be booming for him! Well, one moment and I will write you a receipt to take back to him.");
					RemoveItem(lordUrqhart, player, chestOfCoins);
					Step = 6;
					return;
				}
			}

			if (Step == 7 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == bombard.Name && gArgs.Item.Id_nb == receiptBombard.Id_nb)
				{
					bombard.SayTo(player, "Ah, fantastic. I'm glad to know my money is now in a safe place. Thank you so much for doing that for me, and I hope the trip into Camelot was informative for you. Here, take this letter back to Master Frederick in Cotswold. I want for him to know what a fantastic job you did for me by delivering these vegetables from Ludlow, and for taking care of some business in Camelot for me. Thank you again Vinde. I hope we speak again soon.");

					RemoveItem(bombard, player, receiptBombard);
					RemoveItem(bombard, player, assistantNecklace);

					GiveItem(bombard, player, ticketToCotswold);
					GiveItem(bombard, player, letterFrederick);
					Step = 8;
					return;
				}
			}

			if (Step == 8 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == masterFrederick.Name && gArgs.Item.Id_nb == letterFrederick.Id_nb)
				{
					masterFrederick.SayTo(player, "Ah, from Bombard. Let me see what is says. One moment please.");
					SendSystemMessage(player, "Master Frederick reads the note from Bombard carefully.");
					player.Out.SendEmoteAnimation(masterFrederick, eEmote.Ponder);

					RemoveItem(masterFrederick, player, letterFrederick);
					Step = 9;
					return;
				}
			}

		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, assistantNecklace, false);
			RemoveItem(m_questPlayer, chestOfCoins, false);
			RemoveItem(m_questPlayer, letterFrederick, false);
			RemoveItem(m_questPlayer, scrollUrqhart, false);
			RemoveItem(m_questPlayer, ticketToCotswold, false);

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
				GiveItem(masterFrederick, m_questPlayer, recruitsRoundShield);
			else
				GiveItem(masterFrederick, m_questPlayer, recruitsBracer);

			m_questPlayer.GainExperience(26, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 2, Util.Random(50)), "You recieve {0} as a reward.");

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));

		}

	}
}
