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
 * 1) Travel to loc=26955,7789 Lough Derg to speak with Addrir. 
 * 2) Give Rumdor, at loc=27643,8423 Lough Derg, the ticket to Tir na mBeo from Addrir. 
 * 3) Give Aethic, at loc=23761,45658 Lough Derg, the Aethic the Sack of Supplies from Addrir. 
 * 4) Give Truichon, at loc=23949,43498 Lough Derg, the ticket to Ardee from Aethic. 
 * 5) Give Freagus, at loc=54195,51453 Connacht, the Crate of Vegetables from Aethic. 
 * 6) Talk to Freagus for your reward.
 */

using System;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.Language;
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

	public class ImportantDelivery : BaseAddrirQuest
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

		protected const string questTitle = "Important Delivery (Hib)";
		protected const int minimumLevel = 1;
		protected const int maximumLevel = 1;

		private static GameNPC addrir = null;

		private static GameNPC rumdor = null;
		private static GameNPC aethic = null;
		private static GameNPC truichon = null;
		private static GameNPC freagus = null;

		private static ItemTemplate ticketToTirnamBeo = null;
		private static ItemTemplate ticketToArdee = null;
		private static ItemTemplate sackOfSupplies = null;
		private static ItemTemplate crateOfVegetables = null;
		private static ItemTemplate recruitsCloak = null;
		private static ItemTemplate recruitsDiary = null;

		/* We need to define the constructors from the base class here, else there might be problems
		 * when loading this quest...
		 */
		public ImportantDelivery() : base()
		{
		}

		public ImportantDelivery(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public ImportantDelivery(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public ImportantDelivery(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Aethic", eRealm.Hibernia);
			if (npcs.Length == 0)
			{
				aethic = new GameNPC();
				aethic.Model = 361;
				aethic.Name = "Aethic";
				if (log.IsWarnEnabled)
					log.Warn("Could not find" + aethic.Name + " , creating ...");
				aethic.GuildName = "Part of " + questTitle + " Quest";
				aethic.Realm = eRealm.Hibernia;
				aethic.CurrentRegionID = 200;
				aethic.Size = 49;
				aethic.Level = 21;
				aethic.X = GameLocation.ConvertLocalXToGlobalX(23761, 200);
				aethic.Y = GameLocation.ConvertLocalYToGlobalY(45658, 200);
				aethic.Z = 5448;
				aethic.Heading = 320;
				//aethic.EquipmentTemplateID = "1707754";
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					aethic.SaveIntoDatabase();
				aethic.AddToWorld();
			}
			else
				aethic = npcs[0];

			npcs = WorldMgr.GetNPCsByName("Freagus", eRealm.Hibernia);
			if (npcs.Length == 0)
			{
				freagus = new GameStableMaster();
				freagus.Model = 361;
				freagus.Name = "Freagus";
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

			npcs = WorldMgr.GetNPCsByName("Rumdor", eRealm.Hibernia);
			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Rumdor, creating ...");
				rumdor = new GameStableMaster();
				rumdor.Model = 361;
				rumdor.Name = "Rumdor";
				rumdor.GuildName = "Stable Master";
				rumdor.Realm = eRealm.Hibernia;
				rumdor.CurrentRegionID = 200;
				rumdor.Size = 53;
				rumdor.Level = 33;
				rumdor.X = 347175;
				rumdor.Y = 491836;
				rumdor.Z = 5226;
				rumdor.Heading = 1262;
				rumdor.EquipmentTemplateID = "3800664";

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					rumdor.SaveIntoDatabase();

				rumdor.AddToWorld();
			}
			else
				rumdor = npcs[0];

			GameObject[] objects = WorldMgr.GetObjectsByName("Truichon", eRealm.Hibernia,typeof(GameStableMaster));
            if (npcs.Length == 0)
            {
                truichon = new GameStableMaster();
                truichon.Model = 361;
                truichon.Name = "Truichon";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + truichon.Name + ", creating ...");
                truichon.GuildName = "Stable Master";
                truichon.Realm = eRealm.Hibernia;
                truichon.CurrentRegionID = 1;
                truichon.Size = 50;
                truichon.Level = 33;
                truichon.X = 343464;
                truichon.Y = 526708;
                truichon.Z = 5448;
                truichon.Heading = 68;
                //truichon.EquipmentTemplateID = "5448";

                //You don't have to store the created mob in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    truichon.SaveIntoDatabase();
                truichon.AddToWorld();
            }
            else
            {
                truichon =(GameStableMaster) npcs[0];
            }

			#endregion

			#region defineItems

			ticketToTirnamBeo = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "hs_magmell_tirnambeo");
			if (ticketToTirnamBeo == null)
				ticketToTirnamBeo = CreateTicketTo("Tir na mBeo", "hs_magmell_tirnambeo");
			ticketToArdee = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "hs_tirnambeo_ardee");
			if (ticketToArdee == null)
				ticketToArdee = CreateTicketTo("Ardee", "hs_tirnambeo_ardee");

            recruitsDiary = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "recruits_diary");
			if (recruitsDiary == null)
			{
				recruitsDiary = new ItemTemplate();
				recruitsDiary.Name = "Recruits Diary";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsDiary.Name + " , creating it ...");
				recruitsDiary.Weight = 3;
				recruitsDiary.Model = 500;
				recruitsDiary.Object_Type = (int)eObjectType.GenericItem;
				recruitsDiary.Id_nb = "recruits_diary";
				recruitsDiary.IsPickable = true;
				recruitsDiary.IsDropable = false;

                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(recruitsDiary);
			}

			sackOfSupplies = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "sack_of_supplies");
			if (sackOfSupplies == null)
			{
				sackOfSupplies = new ItemTemplate();
				sackOfSupplies.Name = "Sack of Supplies";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + sackOfSupplies.Name + " , creating it ...");

				sackOfSupplies.Weight = 10;
				sackOfSupplies.Model = 488;

				sackOfSupplies.Object_Type = (int) eObjectType.GenericItem;

				sackOfSupplies.Id_nb = "sack_of_supplies";
				sackOfSupplies.IsPickable = true;
				sackOfSupplies.IsDropable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(sackOfSupplies);
			}

			crateOfVegetables = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "crate_of_vegetables");
			if (crateOfVegetables == null)
			{
				crateOfVegetables = new ItemTemplate();
				crateOfVegetables.Name = "Crate of Vegetables";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + crateOfVegetables.Name + " , creating it ...");

				crateOfVegetables.Weight = 15;
				crateOfVegetables.Model = 602;

				crateOfVegetables.Object_Type = (int) eObjectType.GenericItem;

				crateOfVegetables.Id_nb = "crate_of_vegetables";
				crateOfVegetables.IsPickable = true;
				crateOfVegetables.IsDropable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(crateOfVegetables);
			}

			// item db check
			recruitsCloak = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "recruits_cloak_hib");
			if (recruitsCloak == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Recruit's Cloak, creating it ...");
				recruitsCloak = new ItemTemplate();
				recruitsCloak.Name = "Recruit's Cloak (Hib)";
				recruitsCloak.Level = 3;

				recruitsCloak.Weight = 3;
				recruitsCloak.Model = 57;

				recruitsCloak.Object_Type = (int) eObjectType.Cloth;
				recruitsCloak.Item_Type = (int) eEquipmentItems.CLOAK;
				recruitsCloak.Id_nb = "recruits_cloak_hib";
				recruitsCloak.Gold = 0;
				recruitsCloak.Silver = 1;
				recruitsCloak.Copper = 0;
				recruitsCloak.IsPickable = true;
				recruitsCloak.IsDropable = true;
				recruitsCloak.Color = 69;

				recruitsCloak.Bonus = 1; // default bonus

				recruitsCloak.Bonus1 = 1;
				recruitsCloak.Bonus1Type = (int) eStat.CON;

				recruitsCloak.Bonus2 = 1;
				recruitsCloak.Bonus2Type = (int) eResist.Slash;

				recruitsCloak.Quality = 100;
				recruitsCloak.Condition = 1000;
				recruitsCloak.MaxCondition = 1000;
				recruitsCloak.Durability = 1000;
				recruitsCloak.MaxDurability = 1000;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsCloak);
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
			GameEventMgr.AddHandler(addrir, GameLivingEvent.Interact, new DOLEventHandler(TalkToAddrir));
			GameEventMgr.AddHandler(addrir, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToAddrir));

			GameEventMgr.AddHandler(freagus, GameLivingEvent.Interact, new DOLEventHandler(TalkToFreagus));
			GameEventMgr.AddHandler(freagus, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToFreagus));

			GameEventMgr.AddHandler(aethic, GameLivingEvent.Interact, new DOLEventHandler(TalkToAethic));
			GameEventMgr.AddHandler(aethic, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToAethic));

			/* Now we bring to addrir the possibility to give this quest to players */
			addrir.AddQuestToGive(typeof (ImportantDelivery));

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

			GameEventMgr.RemoveHandler(addrir, GameLivingEvent.Interact, new DOLEventHandler(TalkToAddrir));
			GameEventMgr.RemoveHandler(addrir, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToAddrir));

			GameEventMgr.RemoveHandler(freagus, GameLivingEvent.Interact, new DOLEventHandler(TalkToFreagus));
			GameEventMgr.RemoveHandler(freagus, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToFreagus));

			GameEventMgr.RemoveHandler(aethic, GameLivingEvent.Interact, new DOLEventHandler(TalkToAethic));
			GameEventMgr.RemoveHandler(aethic, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToAethic));

			/* Now we remove to addrir the possibility to give this quest to players */
			addrir.RemoveQuestToGive(typeof (ImportantDelivery));
		}

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToAddrir(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies
			//for the quest!		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(addrir.CanGiveQuest(typeof (ImportantDelivery), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			ImportantDelivery quest = player.IsDoingQuest(typeof (ImportantDelivery)) as ImportantDelivery;

			addrir.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...
					addrir.SayTo(player, LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.TalkToAddrir.Greetings"));
					return;
				}
				else
				{
					switch (quest.Step)
					{
						case 1:
							addrir.SayTo(player, LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.TalkToAddrir.Journal"));
							break;
						case 2:
							addrir.SayTo(player, LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.TalkToAddrir.OneStepCloser"));
							break;
						case 3:
							addrir.SayTo(player, LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.TalkToAddrir.HorseTicket"));
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
					if (wArgs.Text == LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.TalkToAddrir.CaseQuestion"))
					{
						addrir.SayTo(player, LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.TalkToAddrir.Question"));
					}
					//If the player offered his "help", we send the quest dialog now!
					if (wArgs.Text == LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.TalkToAddrir.CaseTraining"))
					{
						player.Out.SendQuestSubscribeCommand(addrir, QuestMgr.GetIDForQuestType(typeof(ImportantDelivery)), LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.TalkToAddrir.Training"));
					}
				}
				else
				{
					if (wArgs.Text == LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.TalkToAddrir.CaseCharSheet"))
					{
						addrir.SayTo(player, LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.TalkToAddrir.CharSheet"));
					}
					if (wArgs.Text == LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.TalkToAddrir.CaseInformation"))
					{
						addrir.SayTo(player, LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.TalkToAddrir.Information"));
						if (quest.Step == 1)
							quest.Step = 2;
					}
					if (wArgs.Text == LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.TalkToAddrir.CaseJournal"))
					{
						addrir.SayTo(player, LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.TalkToAddrir.Journal"));
						GiveItem(addrir, player, recruitsDiary);
					}
					if (wArgs.Text == LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.TalkToAddrir.CaseExpedite"))
					{
						addrir.SayTo(player, LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.TalkToAddrir.Expedite"));
						GiveItem(addrir, player, sackOfSupplies);
					}
					if (wArgs.Text == LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.TalkToAddrir.CaseHorseTicket"))
					{
						addrir.SayTo(player, LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.TalkToAddrir.HorseTicket"));
						if (quest.Step == 2)
						{
							GiveItem(addrir, player, ticketToTirnamBeo);
							quest.Step = 3;
						}
					}
					if (wArgs.Text == "abort")
					{
						addrir.SayTo(player, LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.TalkToAddrir.Abort"));
					}
				}
			}
		}

		protected static void TalkToAethic(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies
			//for the quest!		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(addrir.CanGiveQuest(typeof (ImportantDelivery), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			ImportantDelivery quest = player.IsDoingQuest(typeof (ImportantDelivery)) as ImportantDelivery;

			aethic.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					switch (quest.Step)
					{
						case 3:
						case 4:
							aethic.SayTo(player, LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.TalkToAethic.Greetings"));
							break;
						case 5:
							aethic.SayTo(player, LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.TalkToAethic.Supplies", player.Name));
							break;
						case 6:
							aethic.SayTo(player, LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.TalkToAethic.Vegetables"));
							break;
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
					if (wArgs.Text == LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.TalkToAethic.CaseErrand"))
					{
						addrir.SayTo(player, LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.TalkToAethic.Errand"));
						if (quest.Step == 5)
						{
							GiveItem(aethic, player, ticketToArdee);
							GiveItem(aethic, player, crateOfVegetables);
							quest.Step = 6;
						}
					}
				}
			}
		}

		protected static void TalkToFreagus(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies
			//for the quest!		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(addrir.CanGiveQuest(typeof (ImportantDelivery), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			ImportantDelivery quest = player.IsDoingQuest(typeof (ImportantDelivery)) as ImportantDelivery;

			freagus.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					if (quest.Step == 7)
					{
						freagus.SayTo(player, LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.TalkToFreagus.Welcome"));
					}
					else if (quest.Step == 8)
					{
						freagus.SayTo(player, LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.TalkToFreagus.GoodCrop"));
						quest.FinishQuest();
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
					if (wArgs.Text == LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.TalkToFreagus.CaseReward"))
					{
						freagus.SayTo(player, LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.TalkToFreagus.Reward", player.Name));
						freagus.SayTo(player, LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.TalkToFreagus.Errand"));
						if (quest.Step == 8)
						{
							quest.FinishQuest();
						}

					}
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
			if (player.IsDoingQuest(typeof (ImportantDelivery)) != null)
				return true;

			// This checks below are only performed is player isn't doing quest already

			if (!CheckPartAccessible(player, typeof (ImportantDelivery)))
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
			ImportantDelivery quest = player.IsDoingQuest(typeof (ImportantDelivery)) as ImportantDelivery;

			if (quest == null)
				return;

			if (response == 0x00)
			{
				SendSystemMessage(player, LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.CheckPlayerAbortQuest.NoAbort"));
			}
			else
			{
				SendSystemMessage(player, LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.CheckPlayerAbortQuest.Abort", questTitle));

				quest.AbortQuest();
			}
		}

		protected static void SubscribeQuest(DOLEvent e, object sender, EventArgs args)
		{
			QuestEventArgs qargs = args as QuestEventArgs;
			if (qargs == null)
				return;

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(ImportantDelivery)))
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
			if(addrir.CanGiveQuest(typeof (ImportantDelivery), player)  <= 0)
				return;

			ImportantDelivery quest = player.IsDoingQuest(typeof (ImportantDelivery)) as ImportantDelivery;

			if (quest != null)
				return;

			if (response == 0x00)
			{
				SendReply(player, LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.CheckPlayerAcceptQuest.NoAccept"));
			}
			else
			{
				//Check if we can add the quest!
				if (!addrir.GiveQuest(typeof (ImportantDelivery), player, 1))
					return;
				addrir.SayTo(player, LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.CheckPlayerAcceptQuest.Accept"));
			}
			//language manager support for items
			recruitsDiary.Name = LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.DefineItems.RecruitsDiary");
			sackOfSupplies.Name = LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.DefineItems.SackOfSupplies");
			crateOfVegetables.Name = LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.DefineItems.CrateOfVegetables");
			recruitsCloak.Name = LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.DefineItems.RecruitsCloak");
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
						return LanguageMgr.GetTranslation(m_questPlayer.Client, "Hib.ImportantDelivery.Description.Step1");
					case 2:
						return LanguageMgr.GetTranslation(m_questPlayer.Client, "Hib.ImportantDelivery.Description.Step2");
					case 3:
						return LanguageMgr.GetTranslation(m_questPlayer.Client, "Hib.ImportantDelivery.Description.Step3");
					case 4:
						return LanguageMgr.GetTranslation(m_questPlayer.Client, "Hib.ImportantDelivery.Description.Step4");
					case 5:
						return LanguageMgr.GetTranslation(m_questPlayer.Client, "Hib.ImportantDelivery.Description.Step5");
					case 6:
						return LanguageMgr.GetTranslation(m_questPlayer.Client, "Hib.ImportantDelivery.Description.Step6");
					case 7:
						return LanguageMgr.GetTranslation(m_questPlayer.Client, "Hib.ImportantDelivery.Description.Step7");
					case 8:
						return LanguageMgr.GetTranslation(m_questPlayer.Client, "Hib.ImportantDelivery.Description.Step8");
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player==null || player.IsDoingQuest(typeof (ImportantDelivery)) == null)
				return;

			if (Step == 3 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == rumdor.Name && gArgs.Item.Id_nb == ticketToTirnamBeo.Id_nb)
				{
					rumdor.SayTo(player, LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.Notify.RentingHorse", player.Name));
					Step = 4;
					return;
				}
			}

			if (Step >= 3 && Step <= 4 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == aethic.Name && gArgs.Item.Id_nb == sackOfSupplies.Id_nb)
				{
					aethic.SayTo(player, LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.Notify.Supplies", player.Name));
					RemoveItem(aethic, player, sackOfSupplies);
					Step = 5;
					return;
				}
			}

			if (Step == 6 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == truichon.Name && gArgs.Item.Id_nb == ticketToArdee.Id_nb)
				{
					Step = 7;
					return;
				}
			}

			if (Step >= 6 && Step <= 7 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == freagus.Name && gArgs.Item.Id_nb == crateOfVegetables.Id_nb)
				{
					freagus.SayTo(player, LanguageMgr.GetTranslation(player.Client, "Hib.ImportantDelivery.Notify.Vegetables"));
					RemoveItem(freagus, player, crateOfVegetables);
					Step = 8;
					return;
				}
			}

		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, ticketToArdee, false);
			RemoveItem(m_questPlayer, ticketToTirnamBeo, false);
			RemoveItem(m_questPlayer, sackOfSupplies, false);
			RemoveItem(m_questPlayer, crateOfVegetables, false);

		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			//Give reward to player here ...            
			GiveItem(freagus, m_questPlayer, recruitsCloak);

			m_questPlayer.GainExperience(12);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 1, Util.Random(50)), LanguageMgr.GetTranslation(m_questPlayer.Client, "Hib.ImportantDelivery.FinishQuest.RecieveReward"));
		}
	}
}