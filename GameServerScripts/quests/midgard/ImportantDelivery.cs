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
 * 2) Go to Gularg(the stable master), loc=50149,50338 Vale of Mularn, and hand him your Ticket to Haggerfel from Dalikor. 
 * 3) Give Abohas, loc=52274,29985 Vale of Mularn, the Sack of Supplies from Dalikor. 
 * 4) Give Yolafson, loc=51928,28692 Vale of Mularn, the Ticket to Vasudheim from Abohas. 
 * 5) Hand Harlfug, loc=52670,16862 East Svealand, the Crate of Vegetables from Abohas, chat with him to receive your reward.
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

namespace DOL.GS.Quests.Midgard
{
	/* The first thing we do, is to declare the class we create
	 * as Quest. To do this, we derive from the abstract class
	 * AbstractQuest
	 * 	 
	 */

	public class ImportantDelivery : BaseDalikorQuest
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

		protected const string questTitle = "Important Delivery (Mid)";
		protected const int minimumLevel = 1;
		protected const int maximumLevel = 1;

		private static GameNPC dalikor = null;

		private static GameNPC gularg = null;
		private static GameNPC abohas = null;
		private static GameNPC harlfug = null;

		private static GameNPC yolafson = null;

		private static ItemTemplate ticketToHaggerfel = null;
		private static ItemTemplate ticketToVasudheim = null;
		private static ItemTemplate sackOfSupplies = null;
		private static ItemTemplate crateOfVegetables = null;
		private static ItemTemplate recruitsCloak = null;


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

			dalikor = GetDalikor();

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Abohas", eRealm.Midgard);
			if (npcs.Length == 0)
			{
				abohas = new GameNPC();
				abohas.Model = 215;
				abohas.Name = "Abohas";
				if (log.IsWarnEnabled)
					log.Warn("Could not find" + abohas.Name + " , creating her ...");
				abohas.GuildName = "Part of " + questTitle + " Quest";
				abohas.Realm = (byte) eRealm.Midgard;
				abohas.CurrentRegionID = 100;
				abohas.Size = 49;
				abohas.Level = 21;
				abohas.X = GameLocation.ConvertLocalXToGlobalX(52274, 100);
				abohas.Y = GameLocation.ConvertLocalYToGlobalY(29985, 100);
				abohas.Z = 4960;
				abohas.Heading = 123;
				//abohas.EquipmentTemplateID = "1707754";
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					abohas.SaveIntoDatabase();
				abohas.AddToWorld();
			}
			else
				abohas = npcs[0];

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
				harlfug.CurrentRegionID = 100;
				harlfug.Size = 52;
				harlfug.Level = 41;
				harlfug.X = 773458;
				harlfug.Y = 754240;
				harlfug.Z = 4600;
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

			npcs = WorldMgr.GetNPCsByName("Gularg", eRealm.Midgard);
			if (npcs.Length == 0)
			{
				gularg = new GameStableMaster();
				gularg.Model = 212;
				gularg.Name = "Gularg";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + gularg.Name + ", creating her ...");
				gularg.GuildName = "Stable Master";
				gularg.Realm = (byte) eRealm.Midgard;
				gularg.CurrentRegionID = 100;
				gularg.Size = 50;
				gularg.Level = 41;
				gularg.X = 803766;
				gularg.Y = 721959;
				gularg.Z = 4686;
				gularg.Heading = 3925;
				gularg.EquipmentTemplateID = "5100798";

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					gularg.SaveIntoDatabase();

				gularg.AddToWorld();
			}
			else
				gularg = npcs[0];

			npcs = WorldMgr.GetNPCsByName("Yolafson", eRealm.Midgard);
			if (npcs.Length == 0)
			{
				yolafson = new GameStableMaster();
				yolafson.Model = 214;
				yolafson.Name = "Yolafson";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + yolafson.Name + ", creating her ...");
				yolafson.GuildName = "Stable Master";
				yolafson.Realm = (byte) eRealm.Midgard;
				yolafson.CurrentRegionID = 100;
				yolafson.Size = 51;
				yolafson.Level = 41;
				yolafson.X = 805721;
				yolafson.Y = 700414;
				yolafson.Z = 4960;
				yolafson.Heading = 1206;
				yolafson.EquipmentTemplateID = "5100798";

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					yolafson.SaveIntoDatabase();
				yolafson.AddToWorld();
			}
			else
				yolafson = npcs[0];

			#endregion

			#region defineItems

			ticketToHaggerfel = CreateTicketTo("Haggerfel");
			ticketToVasudheim = CreateTicketTo("Vasudheim");


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
			recruitsCloak = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "recruits_cloak_mid");
			if (recruitsCloak == null)
			{
				recruitsCloak = new ItemTemplate();
				recruitsCloak.Name = "Recruit's Cloak (Mid)";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsCloak.Name + ", creating it ...");
				recruitsCloak.Level = 3;

				recruitsCloak.Weight = 3;
				recruitsCloak.Model = 57;

				recruitsCloak.Object_Type = (int) eObjectType.Cloth;
				recruitsCloak.Item_Type = (int) eEquipmentItems.CLOAK;
				recruitsCloak.Id_nb = "recruits_cloak_mid";
				recruitsCloak.Gold = 0;
				recruitsCloak.Silver = 1;
				recruitsCloak.Copper = 0;
				recruitsCloak.IsPickable = true;
				recruitsCloak.IsDropable = true;
				recruitsCloak.Color = 44; // brown

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
			//We want to be notified whenever a player enters the world            
			GameEventMgr.AddHandler(dalikor, GameLivingEvent.Interact, new DOLEventHandler(TalkToDalikor));
			GameEventMgr.AddHandler(dalikor, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToDalikor));

			GameEventMgr.AddHandler(harlfug, GameLivingEvent.Interact, new DOLEventHandler(TalkToHarlfug));
			GameEventMgr.AddHandler(harlfug, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToHarlfug));

			GameEventMgr.AddHandler(abohas, GameLivingEvent.Interact, new DOLEventHandler(TalkToAbohas));
			GameEventMgr.AddHandler(abohas, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToAbohas));

			/* Now we bring to dalikor the possibility to give this quest to players */
			dalikor.AddQuestToGive(typeof (ImportantDelivery));

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

			GameEventMgr.RemoveHandler(dalikor, GameLivingEvent.Interact, new DOLEventHandler(TalkToDalikor));
			GameEventMgr.RemoveHandler(dalikor, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToDalikor));

			GameEventMgr.RemoveHandler(harlfug, GameLivingEvent.Interact, new DOLEventHandler(TalkToHarlfug));
			GameEventMgr.RemoveHandler(harlfug, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToHarlfug));

			GameEventMgr.RemoveHandler(abohas, GameLivingEvent.Interact, new DOLEventHandler(TalkToAbohas));
			GameEventMgr.RemoveHandler(abohas, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToAbohas));

			/* Now we remove to dalikor the possibility to give this quest to players */
			dalikor.RemoveQuestToGive(typeof (ImportantDelivery));
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

			if(dalikor.CanGiveQuest(typeof (ImportantDelivery), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			ImportantDelivery quest = player.IsDoingQuest(typeof (ImportantDelivery)) as ImportantDelivery;

			dalikor.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...
					dalikor.SayTo(player, "Greetings to you my young friend. I am Dalikor. I'm here to help you find your way around this vast realm. In the process, you'll have the ability to earn weapons, armor, coin and some levels. Would you like to start [training] now?");
					return;
				}
				else
				{
					switch (quest.Step)
					{
						case 2:
							dalikor.SayTo(player, "This journal will help you from time to time while you are doing various tasks for me. I like to call it a smart journal. It was made by one of the runemasters for new recruits like you. It will help to [expedite] your training.");
							break;
						case 3:
							dalikor.SayTo(player, "All you need to do is take this horse ticket to Gularg in Mularn. Hand him the ticket and you'll be on your way to Haggerfel. Be swift my young recruit. Time is of the essence.");
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
						case "training":
							dalikor.SayTo(player, "I thought you would. What I am here to do is to guide you through your first few seasons, until I feel you're confident and skilled enough to make it on your own in Albion. If you aren't properly trained, then what good are you to the realm? None, of course. Now, I will start your training off by asking you a simple quesion, whether or not you wish to [proceed] with your training. A dialogue box will pop up. Either press the Accept or Decline button.");
							break;
							//If the player offered his "help", we send the quest dialog now!
						case "proceed":
							player.Out.SendCustomDialog("Are you ready to begin your training?", new CustomDialogResponse(CheckPlayerAcceptQuest));
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "character sheet":
							dalikor.SayTo(player, "Your character sheet houses all of your character's information, such as attributes, weapon skill, base class and profession. If at any time you want to see your character's statistics, press the far left icon on the menu bar (it looks like a person with a circle around them) for more [information].");
							break;
						case "information":
							dalikor.SayTo(player, "I know this all seems a little overwhelming, but I have a special item here that will make this transition a smooth one. Please, take this [journal].");
							if (quest.Step == 1)
								quest.Step = 2;
							break;
						case "journal":
							dalikor.SayTo(player, "This journal will help you from time to time while you are doing various tasks for me. I like to call it a smart journal. It was made by one of the runemasters for new recruits like you. It will help to [expedite] your training.");
							break;
						case "expedite":
							dalikor.SayTo(player, "Now that I've given you a small introduction to the world of Midgard, let's get started with your first task. I need for you to deliver this package of supplies to Abohas in Haggerfel. Don't worry, I have a special [horse ticket] for you.");
							break;
						case "horse ticket":
							dalikor.SayTo(player, "All you need to do is take this horse ticket to Gularg in Mularn. Hand him the ticket and you'll be on your way to Haggerfel. Be swift my young recruit. Time is of the essence.");
							if (quest.Step == 2)
							{
								GiveItem(dalikor, player, ticketToHaggerfel);
								GiveItem(dalikor, player, sackOfSupplies);
								quest.Step = 3;
							}
							break;
						case "abort":
							player.Out.SendCustomDialog("Do you really want to abort this quest, \nall items gained during quest will be lost?", new CustomDialogResponse(CheckPlayerAbortQuest));
							break;
					}
				}
			}
		}

		protected static void TalkToAbohas(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies
			//for the quest!		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if( dalikor.CanGiveQuest(typeof (ImportantDelivery), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			ImportantDelivery quest = player.IsDoingQuest(typeof (ImportantDelivery)) as ImportantDelivery;

			abohas.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					switch (quest.Step)
					{
						case 3:
						case 4:
							abohas.SayTo(player, "Greetings traveler. I've not seen you around here before. You must be a new recruit. Well then, is there something I can help you with?");
							break;
						case 5:
							abohas.SayTo(player, "Oh, I see. Yes, from Dalikor. We've been waiting for these supplies for a while. It's good to have them. I don't suppose you're up for one more [errand], are you?");
							break;
						case 6:
							abohas.SayTo(player, "I need for you to deliver this crate of vegetables to Stable Master Harlfug in Vasudheim. Don't worry, I'll give you a ticket so you don't have to run there. Thank you my friend. Be swift so the vegetables don't rot.");
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
					switch (wArgs.Text)
					{
						case "errand":
							abohas.SayTo(player, "I need for you to deliver this crate of vegetables to Stable Master Harlfug in Vasudheim. Don't worry, I'll give you a ticket so you don't have to run there. Thank you my friend. Be swift so the vegetables don't rot.");
							if (quest.Step == 5)
							{
								GiveItem(abohas, player, ticketToVasudheim);
								GiveItem(abohas, player, crateOfVegetables);

								quest.Step = 6;
							}
							break;
					}
				}
			}
		}

		protected static void TalkToHarlfug(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies
			//for the quest!		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if( dalikor.CanGiveQuest(typeof (ImportantDelivery), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			ImportantDelivery quest = player.IsDoingQuest(typeof (ImportantDelivery)) as ImportantDelivery;

			harlfug.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					switch (quest.Step)
					{
						case 7:
							harlfug.SayTo(player, "Welcome to my stable friend. What can I do for you today?");
							break;
						case 8:
							harlfug.SayTo(player, "Ah, here we are. I know it isn't much, but I got it in a trade a while ago, and I don't have much use for it. I'm sure you can put it to use though, can't you? Let me know if you're in need of anything else. I have a few errands I need run.");
							quest.FinishQuest();
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
					switch (wArgs.Text)
					{
						case "reward":
							harlfug.SayTo(player, "Ah, here we are. I know it isn't much, but I got it in a trade a while ago, and I don't have much use for it. I'm sure you can put it to use though, can't you? Let me know if you're in need of anything else. I have a few errands I need run.");
							if (quest.Step == 8)
							{
								quest.FinishQuest();
							}
							break;
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
			if( dalikor.CanGiveQuest(typeof (ImportantDelivery), player)  <= 0)
				return;

			ImportantDelivery quest = player.IsDoingQuest(typeof (ImportantDelivery)) as ImportantDelivery;

			if (quest != null)
				return;

			if (response == 0x00)
			{
				SendReply(player, "Oh well, if you change your mind, please come back!");
			}
			else
			{
				//Check if we can add the quest!
				if (!dalikor.GiveQuest(typeof (ImportantDelivery), player, 1))
					return;

				SendReply(player, "Congratulations! You are now one step closer to understanding the world of Camelot! During this phase of your training, I will be sending you to different parts of the realm to deliver much needed supplies to various citizens. You will need to check your QUEST JOURNAL from time to time to see what you need to accomplish next on your quest. You can access the quest journal from the COMMAND button on your [character sheet].");
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
						return "[Step #1] Listen to Dalikor describe your training. If he stops speaking with you, ask him about you [character sheet].";
					case 2:
						return "[Step #2] Listen to Dalikor as he explains your journal. If he stops speaking with you, ask him how he can [expedite] your journeys.";
					case 3:
						return "[Step #3] Make your way east from the guard tower into Mularn. Find Stable Master Gularg and hand him your ticket.";
					case 4:
						return "[Step #4] Give your Sack of Supplies to Abohas in Haggerfel. You can do this the same way you handed Gularg your ticket.";
					case 5:
						return "[Step #5] Listen to what Dunan has to say. If he stops speaking with you, ask him if there is an [errand] he needs you to run for him.";
					case 6:
						return "[Step #6] Take the Ticket Abohas gave you to Stable Master Yolafson northeast outside of Haggerfel. Hand him the ticket so you can get to Stable Master Harlfug. Harlfug is in Vasudheim. Look for him to the south";
					case 7:
						return "[Step #7] Hand Harlfug the Crate of Vegetables. If you prefer, you may right click to interact with him first.";
					case 8:
						return "[Step #8] Listen to Stable Master Harlfug some more. If he stops speaking with you, ask him if he has a [reward] for your hard work and service to Midgard.";

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
				if (gArgs.Target.Name == gularg.Name && gArgs.Item.Id_nb == ticketToHaggerfel.Id_nb)
				{
					Step = 4;
					return;
				}
			}

			if (Step >= 3 && Step <= 4 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == abohas.Name && gArgs.Item.Id_nb == sackOfSupplies.Id_nb)
				{
					abohas.SayTo(player, "Oh, I see. Yes, from Dalikor. We've been waiting for these supplies for a while. It's good to have them. I don't suppose you're up for one more [errand], are you?");
					RemoveItem(abohas, player, sackOfSupplies);
					Step = 5;
					return;
				}
			}

			if (Step == 6 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == yolafson.Name && gArgs.Item.Id_nb == ticketToVasudheim.Id_nb)
				{
					Step = 7;
					return;
				}
			}

			if (Step >= 6 && Step <= 7 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == harlfug.Name && gArgs.Item.Id_nb == crateOfVegetables.Id_nb)
				{
					harlfug.SayTo(player, "Ah, the vegetables I've been waiting for from Abohas. Thank you for delivering them to me. I couldn't find anyone to look after my stable so I could go and get them. Let me see, I think a [reward] is in order for your hard work.");
					RemoveItem(harlfug, player, crateOfVegetables);
					Step = 8;
					return;
				}
			}

		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, ticketToVasudheim, false);
			RemoveItem(m_questPlayer, ticketToHaggerfel, false);
			RemoveItem(m_questPlayer, sackOfSupplies, false);
			RemoveItem(m_questPlayer, crateOfVegetables, false);

		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			//Give reward to player here ...            
			GiveItem(harlfug, m_questPlayer, recruitsCloak);

			m_questPlayer.GainExperience(12, 0, 0, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 1, Util.Random(50)), "You recieve {0} as a reward.");

		}

	}
}
