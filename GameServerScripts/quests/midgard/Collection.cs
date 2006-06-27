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
 * 2) /Use the Dusty Old Map and go to loc=40124,44594 Vale of Mularn and kill Mitan for her wing. 
 * 3) /Use the Dusty Old Map and go to loc=46821,40884 Vale of Mularn and kill Ostadi for her wing. 
 * 4) /Use the Dusty Old Map and go to loc=56104,43865 Vale of Mularn and kill Seiki for her wing. 
 * 5) Return to Dalikor and hand him the wings when asked for your reward.
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

	public class Collection : BaseDalikorQuest
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

		protected const string questTitle = "Collection (Mid)";
		protected const int minimumLevel = 3;
		protected const int maximumLevel = 3;

		private static GameNPC dalikor = null;

		private static GameMob[] general = new GameMob[3];
		private static String[] generalNames = {"Mitan", "Ostadi", "Seiki"};
		private static GameLocation[] generalLocations = new GameLocation[3];

		private static ItemTemplate askefruerWings = null;
		private static ItemTemplate dustyOldMap = null;
		private static ItemTemplate recruitsArms = null;
		private static ItemTemplate recruitsSleeves = null;


		/* We need to define the constructors from the base class here, else there might be problems
		 * when loading this quest...
		 */

		public Collection() : base()
		{
		}

		public Collection(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public Collection(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public Collection(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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

			dalikor = GetDalikor();

			#region defineNPCs

			generalLocations[0] = new GameLocation(generalNames[0], 100, 100, 40124, 44594, 4712, 216);
			generalLocations[1] = new GameLocation(generalNames[1], 100, 100, 46821, 40884, 4972, 21);
			generalLocations[2] = new GameLocation(generalNames[2], 100, 100, 56104, 43865, 5460, 48);

			GameNPC[] npcs = null;
			for (int i = 0; i < general.Length; i++)
			{
				npcs = WorldMgr.GetNPCsByName(generalNames[i], eRealm.None);
				if (npcs.Length > 0)
					general[i] = npcs[0] as GameMob;
				else
				{
					if (log.IsWarnEnabled)
						log.Warn("Could not find " + generalNames[i] + ", creating ...");
					general[i] = new GameMob();

					general[i].Model = 678;

					general[i].GuildName = "Part of " + questTitle + " Quest";
					general[i].Name = generalNames[i];
					general[i].X = generalLocations[i].X;
					general[i].Y = generalLocations[i].Y;
					general[i].Z = generalLocations[i].Z;
					general[i].Heading = generalLocations[i].Heading;
					;

					general[i].Realm = (byte) eRealm.None;
					general[i].CurrentRegionID = generalLocations[i].RegionID;
					general[i].Size = 49;
					general[i].Level = 2;

					StandardMobBrain brain = new StandardMobBrain();
					brain.AggroLevel = 80;
					brain.AggroRange = 1000;
					general[i].SetOwnBrain(brain);

					if (SAVE_INTO_DATABASE)
						general[i].SaveIntoDatabase();

					general[i].AddToWorld();
				}
			}

			#endregion

			#region defineItems
			/*
			 * TODO Model for Campfire doesn't work. Very Stange!
			 */
			/*
			// Add campfires to generals 
			for (int i = 0; i < generalLocations.Length; i++)
			{
				GameStaticItem campfire = null;
				
				IEnumerable items =  WorldMgr.GetItemsCloseToSpot(generalLocations[i].RegionID,generalLocations[i].X, generalLocations[i].Y, generalLocations[i].Z, 400,true);
				foreach (GameObject obj in items)
				{
					if (obj is GameStaticItem && obj.Name=="Camp Fire")
					{
						campfire= (GameStaticItem) obj;
						break;
					}
				}

				if (campfire==null) 
				{				
					campfire = new GameStaticItem();
					campfire.Name="Camp Fire";
					
					campfire.Model = 2593;				
					campfire.Heading = generalLocations[i].Heading;
					campfire.X = generalLocations[i].X;
					campfire.Y = generalLocations[i].Y;
					campfire.Z = generalLocations[i].Z;
					campfire.CurrentRegionID = generalLocations[i].RegionID;
					
					if (SAVE_INTO_DATABASE) 
						campfire.SaveIntoDatabase();
					
					campfire.AddToWorld();

					DOLConsole.WriteLine("Camp Fire added"+generalNames[i]);
				}
			}
*/

			askefruerWings = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "askefruer_wings");
			if (askefruerWings == null)
			{
				askefruerWings = new ItemTemplate();
				askefruerWings.Name = "Wings of Askefruer";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + askefruerWings.Name + " , creating it ...");

				askefruerWings.Weight = 2;
				askefruerWings.Model = 551;

				askefruerWings.Object_Type = (int) eObjectType.GenericItem;

				askefruerWings.Id_nb = "askefruer_wings";
				askefruerWings.IsPickable = true;
				askefruerWings.IsDropable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(askefruerWings);
			}

			dustyOldMap = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "dusty_old_map");
			if (dustyOldMap == null)
			{
				dustyOldMap = new ItemTemplate();
				dustyOldMap.Name = "Dusty Old Map";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + dustyOldMap.Name + " , creating it ...");

				dustyOldMap.Weight = 10;
				dustyOldMap.Model = 498;

				dustyOldMap.Object_Type = (int) eObjectType.GenericItem;

				dustyOldMap.Id_nb = "dusty_old_map";
				dustyOldMap.IsPickable = true;
				dustyOldMap.IsDropable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(dustyOldMap);
			}


			// item db check
			recruitsArms = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "recruits_studded_arms_mid");
			if (recruitsArms == null)
			{
				recruitsArms = new ItemTemplate();
				recruitsArms.Name = "Recruit's Studded Arms (Mid)";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsArms.Name + ", creating it ...");
				recruitsArms.Level = 7;

				recruitsArms.Weight = 36;
				recruitsArms.Model = 83; // studded Boots

				recruitsArms.DPS_AF = 10; // Armour
				recruitsArms.SPD_ABS = 19; // Absorption

				recruitsArms.Object_Type = (int) eObjectType.Studded;
				recruitsArms.Item_Type = (int) eEquipmentItems.ARMS;
				recruitsArms.Id_nb = "recruits_studded_arms_mid";
				recruitsArms.Gold = 0;
				recruitsArms.Silver = 4;
				recruitsArms.Copper = 0;
				recruitsArms.IsPickable = true;
				recruitsArms.IsDropable = true;
				recruitsArms.Color = 36; // blue cloth

				recruitsArms.Bonus = 5; // default bonus

				recruitsArms.Bonus1 = 4;
				recruitsArms.Bonus1Type = (int) eStat.QUI;

				recruitsArms.Bonus2 = 1;
				recruitsArms.Bonus2Type = (int) eResist.Body;

				recruitsArms.Quality = 100;
				recruitsArms.MaxQuality = 100;
				recruitsArms.Condition = 1000;
				recruitsArms.MaxCondition = 1000;
				recruitsArms.Durability = 1000;
				recruitsArms.MaxDurability = 1000;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsArms);
			}

			recruitsSleeves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "recruits_quilted_sleeves");
			if (recruitsSleeves == null)
			{
				recruitsSleeves = new ItemTemplate();
				recruitsSleeves.Name = "Recruit's Quilted Sleeves";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsSleeves.Name + ", creating it ...");
				recruitsSleeves.Level = 7;

				recruitsSleeves.Weight = 12;
				recruitsSleeves.Model = 153;

				recruitsSleeves.DPS_AF = 6; // Armour
				recruitsSleeves.SPD_ABS = 0; // Absorption

				recruitsSleeves.Object_Type = (int) eObjectType.Cloth;
				recruitsSleeves.Item_Type = (int) eEquipmentItems.ARMS;
				recruitsSleeves.Id_nb = "recruits_quilted_sleeves";
				recruitsSleeves.Gold = 0;
				recruitsSleeves.Silver = 4;
				recruitsSleeves.Copper = 0;
				recruitsSleeves.IsPickable = true;
				recruitsSleeves.IsDropable = true;
				recruitsSleeves.Color = 27; // red cloth

				recruitsSleeves.Bonus = 5; // default bonus

				recruitsSleeves.Bonus1 = 4;
				recruitsSleeves.Bonus1Type = (int) eStat.DEX;

				recruitsSleeves.Bonus2 = 1;
				recruitsSleeves.Bonus2Type = (int) eResist.Body;

				recruitsSleeves.Quality = 100;
				recruitsSleeves.MaxQuality = 100;
				recruitsSleeves.Condition = 1000;
				recruitsSleeves.MaxCondition = 1000;
				recruitsSleeves.Durability = 1000;
				recruitsSleeves.MaxDurability = 1000;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsSleeves);
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

			/* Now we bring to dalikor the possibility to give this quest to players */
			dalikor.AddQuestToGive(typeof (Collection));

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

			/* Now we remove to dalikor the possibility to give this quest to players */
			dalikor.RemoveQuestToGive(typeof (Collection));
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

			if(dalikor.CanGiveQuest(typeof (Collection), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Collection quest = player.IsDoingQuest(typeof (Collection)) as Collection;

			dalikor.TurnTo(player);

			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...
					dalikor.SayTo(player, "Recruit, we have received further intelligence that the Askefruer are trying to [make a move] on Mularn.");
					return;
				}
				else
				{
					switch (quest.Step)
					{
						case 1:
							dalikor.SayTo(player, "Alright recruit, we haven't much time. Njarmir was unsure of exactly when the Askefruer would be sending their troops to these locations. I have a [map] that will assist you in finding these areas.");
							break;
						case 5:
							dalikor.SayTo(player, "Eeinken, you've returned safely. If you have successfully defeated the Askefruer, please, hand their wings to me.");
							quest.Step = 6;
							break;
						case 9:
							dalikor.SayTo(player, "These sleeves will come in handy while you're out fighting. A Viking is always in need of sturdy armor to protect his somewhat fragile hide as he battles the enemies of his realm. Thank you again for your assistance in this matter. Be safe.");
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
						case "make a move":
							dalikor.SayTo(player, "There are three prominent Askefruer generals, so to speak, within the Askefruer community. Their names are Mitan, Ostadi and Seiki. They are rumored to be very successful [military leaders] within the Askefruer.");
							break;
						case "military leaders":
							dalikor.SayTo(player, "We got all of this information from the traitor we caught a few days ago. We have been leaning on him for more information, and he finally cracked and told us all he knew. But we can't allow the Askefruer to make a multi-frontal [attack].");
							break;
						case "attack":
							dalikor.SayTo(player, "Njarmir has said these three Askefruer have not yet received their troops. They have scouted out their locations and are awaiting for others to come to them. I believe this is the perfect time to [strike].");
							break;
						case "strike":
							dalikor.SayTo(player, "If we can take down these leaders, the Askefruer will be signifigantly weakened. Perhaps it will stave off any future attacks. I'm asking you, Eeinken, to find these three Askefruer and destroy them. Will you [do it]?");
							break;

							//If the player offered his "help", we send the quest dialog now!
						case "do it":
							player.Out.SendCustomDialog("Will you find and slay these three Fallen Askefruer and return their wings to Dalikor?", new CustomDialogResponse(CheckPlayerAcceptQuest));
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "map":
							dalikor.SayTo(player, "Njarmir has provided us with a rough location of where he thinks the Askefruer are. He is not one hundred percent certain though, so you will need to do most of the searching on your own. USE the map from time to time to help you find these hidden camps. Remember, slay the Askefruer there. If you can, get their wings as proof of their demise. Be swift Eeinken. There isn't much time.");
							if (quest.Step == 1)
							{
								GiveItem(dalikor, player, dustyOldMap);
								quest.Step = 2;
							}
							break;

						case "reward":
							dalikor.SayTo(player, "These sleeves will come in handy while you're out fighting. A Viking is always in need of sturdy armor to protect his somewhat fragile hide as he battles the enemies of his realm. Thank you again for your assistance in this matter. Be safe.");
							if (quest.Step == 9)
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

		protected static void PlayerLeftWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			Collection quest = player.IsDoingQuest(typeof (Collection)) as Collection;
			if (quest != null)
			{
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
			}
		}

		protected static void PlayerEnterWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			Collection quest = player.IsDoingQuest(typeof (Collection)) as Collection;
			if (quest != null)
			{
				GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
				GameEventMgr.AddHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			}
		}

		protected static void PlayerUseSlot(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = (GamePlayer) sender;
			// player already morphed...            

			Collection quest = (Collection) player.IsDoingQuest(typeof (Collection));
			if (quest == null)
				return;

			UseSlotEventArgs uArgs = (UseSlotEventArgs) args;

			InventoryItem item = player.Inventory.GetItem((eInventorySlot)uArgs.Slot);
			if (item != null && item.Id_nb == dustyOldMap.Id_nb)
			{
				if (quest.Step == 2)
				{
					SendReply(player, "Travel north from Dalikor to the standing stone. From the standing stone, travel west-northwest. Look for the campfire.");
				}
				else if (quest.Step == 3)
				{
					SendReply(player, "The map has told you that the second are is to the east northeast of the first location. Look for the campfire.");
				}
				else if (quest.Step == 4)
				{
					SendReply(player, "You must travel south from the second location then east along the base of the hills. Follow the base of the hills east till you reach a small plateau. Look for a campfire.");
				}
				else if (quest.Step == 5)
				{
					SendReply(player, "Return to Dalikor at the guard tower outside of Mularn village.");
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
			if (player.IsDoingQuest(typeof (Collection)) != null)
				return true;

			// This checks below are only performed is player isn't doing quest already
			if (player.HasFinishedQuest(typeof (Frontiers)) == 0)
				return false;

			if (!CheckPartAccessible(player, typeof (Collection)))
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
			Collection quest = player.IsDoingQuest(typeof (Collection)) as Collection;

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
			if(dalikor.CanGiveQuest(typeof (Collection), player)  <= 0)
				return;

			Collection quest = player.IsDoingQuest(typeof (Collection)) as Collection;

			if (quest != null)
				return;

			if (response == 0x00)
			{
				SendReply(player, "Oh well, if you change your mind, please come back!");
			}
			else
			{
				//Check if we can add the quest!
				if (!dalikor.GiveQuest(typeof (Collection), player, 1))
					return;

				GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
				GameEventMgr.AddHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));

				SendReply(player, "Alright recruit, we haven't much time. Njarmir was unsure of exactly when the Askefruer would be sending their troops to these locations. I have a [map] that will assist you in finding these areas.");
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
						return "[Step #1] Listen to Dalikor. If he stops talking ask him about the [map].";
					case 2:
						return "[Step #2] You will now need to venture forth and find the locations of these Fallen Askefruer. USE your Dusty Old Map for directions. If you need to access your compass, use the SHIFT + C keys. To use an item, right click on it and type /use.";
					case 3:
						return "[Step #3] You will now need to venture forth and find the locations of the second Fallen Askefruer. USE your Dusty Old Map for directions. If you need to access your compass, use the SHIFT + C keys. To use an item, right click on it and type /use.";
					case 4:
						return "[Step #4] You will now need to venture forth and find the locations of the third Fallen Askefruer. USE your Dusty Old Map for directions. If you need to access your compass, use the SHIFT + C keys. To use an item, right click on it and type /use.";
					case 5:
						return "[Step #5] Return to Dalikor at the guard tower outside of Mularn village.";
					case 6:
						return "[Step #6] Hand Dalikor the Askefruer wings.";
					case 7:
						return "[Step #6] Hand Dalikor the Askefruer wings.";
					case 8:
						return "[Step #6] Hand Dalikor the Askefruer wings.";
					case 9:
						return "[Step #7] Wait for Dalikor to reward you. If he stops speaking with you, ask if there might be a reward] for your troubles.";
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof (Collection)) == null)
				return;

			if (Step >= 2 && Step <= 4 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;

				if (gArgs.Target.Name == generalNames[Step - 2])
				{
					SendSystemMessage("You slay the creature and take it's wing.");
					GiveItem(gArgs.Target, player, askefruerWings);
					SendSystemMessage(player, generalNames[Step - 2] + " yells at you, \"A curse on you land-bound monster! I curse you for killing me!\"");
					Step++;
					return;
				}
			}

			if (Step >= 6 && Step <= 8 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == dalikor.Name && gArgs.Item.Id_nb == askefruerWings.Id_nb)
				{
					if (Step == 6)
					{
						dalikor.SayTo(player, "This is a truly magnificent wing. It's sad, really, that it belonged to such an evil creature. Ah well. Please Eeinken, hand me the other wings.");
					}
					else if (Step == 7)
					{
						dalikor.SayTo(player, "Good job recruit. Now please, hand me the last wing.");
					}
					else if (Step == 8)
					{
						dalikor.SayTo(player, "Yes, the final wing. It appears as though you have finished the task at hand. I do not condone the needless killing of other beings, but I feel as though these Fallen Askefruer leave us little choice. But for your efforts, I have a [reward] for you.");
					}
					RemoveItem(dalikor, player, askefruerWings);
					Step++;
					return;
				}
			}

		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, dustyOldMap, false);
			RemoveItem(m_questPlayer, askefruerWings, false);
			RemoveItem(m_questPlayer, askefruerWings, false);
			RemoveItem(m_questPlayer, askefruerWings, false);

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			//Give reward to player here ...  
			RemoveItem(dalikor, m_questPlayer, dustyOldMap);

			if (m_questPlayer.HasAbilityToUseItem(recruitsArms))
				GiveItem(dalikor, m_questPlayer, recruitsArms);
			else
				GiveItem(dalikor, m_questPlayer, recruitsSleeves);

			m_questPlayer.GainExperience(240, 0, 0, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 6, Util.Random(50)), "You recieve {0} as a reward.");

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));

		}

	}
}
