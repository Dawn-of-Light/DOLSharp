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
 * 1) Travel to loc=19105,26552 Camelot Hills to speak with Master Frederick. 
 * 2) /Use the Dusty Old Map and go to loc=19547,19079 Camelot Hills and kill Palearis for her wing. 
 * 3) /Use the Dusty Old Map and go to loc=23423,16337 Camelot Hills and kill Bohad for her wing. 
 * 4) /Use the Dusty Old Map and go to loc=23036,27231 Camelot Hills and kill Fluvale for her wing. 
 * 5) Return to Master Frederick and hand him the wings when asked for your reward.
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

	public class Collection : BaseFrederickQuest
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

		protected const string questTitle = "Collection";
		protected const int minimumLevel = 3;
		protected const int maximumLevel = 3;

		private static GameNPC masterFrederick = null;

		private static GameMob[] general = new GameMob[3];
		private static String[] generalNames = {"Palearis", "Bohad", "Fluvale"};
		private static GameLocation[] generalLocations = new GameLocation[3];


		private static ItemTemplate fairyGeneralWings = null;
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

			#region defineNPCS

			masterFrederick = GetMasterFrederick();

			generalLocations[0] = new GameLocation(generalNames[0], 1, 568589, 501801, 2134, 23);
			generalLocations[1] = new GameLocation(generalNames[1], 1, 572320, 499246, 2472, 14);
			generalLocations[2] = new GameLocation(generalNames[2], 1, 571900, 510559, 2210, 170);

			GameNPC[] npcs = null;
			for (int i = 0; i < general.Length; i++)
			{
				npcs = WorldMgr.GetNPCsByName(generalNames[i], eRealm.None);
				if (npcs.Length > 0)
					general[i] = npcs[0] as GameMob;
				else
				{
					if (log.IsWarnEnabled)
						log.Warn("Could not find " + generalNames[i] + ", creating her ...");
					general[i] = new GameMob();

					general[i].Model = 603;

					general[i].Name = generalNames[i];
					general[i].X = generalLocations[i].X;
					general[i].Y = generalLocations[i].Y;
					general[i].Z = generalLocations[i].Z;
					general[i].Heading = generalLocations[i].Heading;
					;

					general[i].GuildName = "Part of " + questTitle + " Quest";
					general[i].Realm = (byte) eRealm.None;
					general[i].CurrentRegionID = generalLocations[i].RegionID;
					;
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

			fairyGeneralWings = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "fairy_general_wings");
			if (fairyGeneralWings == null)
			{
				fairyGeneralWings = new ItemTemplate();
				fairyGeneralWings.Name = "Wings of Fairy General";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + fairyGeneralWings.Name + " , creating it ...");

				fairyGeneralWings.Weight = 2;
				fairyGeneralWings.Model = 551;

				fairyGeneralWings.Object_Type = (int) eObjectType.GenericItem;

				fairyGeneralWings.Id_nb = "fairy_general_wings";
				fairyGeneralWings.IsPickable = true;
				fairyGeneralWings.IsDropable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(fairyGeneralWings);
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
			recruitsArms = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "recruits_studded_arms");
			if (recruitsArms == null)
			{
				recruitsArms = new ItemTemplate();
				recruitsArms.Name = "Recruit's Studded Arms";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsArms.Name + ", creating it ...");
				recruitsArms.Level = 7;

				recruitsArms.Weight = 36;
				recruitsArms.Model = 83; // studded Boots

				recruitsArms.DPS_AF = 10; // Armour
				recruitsArms.SPD_ABS = 19; // Absorption

				recruitsArms.Object_Type = (int) eObjectType.Studded;
				recruitsArms.Item_Type = (int) eEquipmentItems.ARMS;
				recruitsArms.Id_nb = "recruits_studded_arms";
				recruitsArms.Gold = 0;
				recruitsArms.Silver = 4;
				recruitsArms.Copper = 0;
				recruitsArms.IsPickable = true;
				recruitsArms.IsDropable = true;
				recruitsArms.Color = 9; // red leather

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

			GameEventMgr.AddHandler(masterFrederick, GameLivingEvent.Interact, new DOLEventHandler(TalkToMasterFrederick));
			GameEventMgr.AddHandler(masterFrederick, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterFrederick));

			/* Now we bring to masterFrederick the possibility to give this quest to players */
			masterFrederick.AddQuestToGive(typeof (Collection));

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
			GameEventMgr.RemoveHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));

			GameEventMgr.RemoveHandler(masterFrederick, GameLivingEvent.Interact, new DOLEventHandler(TalkToMasterFrederick));
			GameEventMgr.RemoveHandler(masterFrederick, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterFrederick));
		
			/* Now we remove to masterFrederick the possibility to give this quest to players */
			masterFrederick.RemoveQuestToGive(typeof (Collection));
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

			if(masterFrederick.CanGiveQuest(typeof (Collection), player)  <= 0)
				return;


			//We also check if the player is already doing the quest
			Collection quest = player.IsDoingQuest(typeof (Collection)) as Collection;

			masterFrederick.TurnTo(player);

			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...
					masterFrederick.SayTo(player, "My young recruit. You are advancing very quickly, but now I have a somewhat [difficult assignment] I need for you to undertake.");
					return;
				}
				else
				{
					if (quest.Step == 1)
					{
						masterFrederick.SayTo(player, "Alright recruit, let's get started. You will need to find and defeat three key ire fairy generals, so to speak. According to my map, there is a [camp] to the north of this tower, to the northeast and to the south, near the stable across the road.");

					}
					else if (quest.Step == 5)
					{
						masterFrederick.SayTo(player, "Welcome back Vinde. I take it you were successful in your mission. If so, hand me the first fairie wing.");
						quest.Step = 6;
					}
					else if (quest.Step == 9)
					{
						masterFrederick.SayTo(player, "For you, a pair of rugged sleeves, great for protecting your arms from the wildlife around here. Thank you again Vinde. I promise my next task will be a little easier on the fighting. *hehe*");
						quest.FinishQuest();
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
						case "difficult assignment":
							masterFrederick.SayTo(player, "The plans I had translated by Scryer Alice revealed more of what the ire fairies plan on doing to and with Cotswold. I'm afraid we need to [teach them a lesson].");
							break;
						case "teach them a lesson":
							masterFrederick.SayTo(player, "We need to curb these issues before they get out of hand. Will you assist me with this [situation]?");
							break;

							//If the player offered his "help", we send the quest dialog now!
						case "situation":
							player.Out.SendCustomDialog("Will you find and slay these three ire fairies?", new CustomDialogResponse(CheckPlayerAcceptQuest));
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "camp":
							masterFrederick.SayTo(player, "You will need to use your compass in order to find your way to these locations. If your compass is not yet visible on your screen, use your shift + C keys to display it. Good luck.recruit. We're counting on you.");
							if (quest.Step == 1)
							{
								GiveItem(masterFrederick, player, dustyOldMap);
								quest.Step = 2;
							}
							break;

						case "reward":
							masterFrederick.SayTo(player, "For you, a pair of rugged sleeves, great for protecting your arms from the wildlife around here. Thank you again Vinde. I promise my next task will be a little easier on the fighting. *hehe*");
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
					SendReply(player, "Travel north from the guard tower and look for the campfire. Do not go too far past the field.");
				}
				else if (quest.Step == 3)
				{
					SendReply(player, " From the first location, travel east-northeast and search for the campfire. Look near the weeping willow trees. You have two more fairy generals to find.");
				}
				else if (quest.Step == 4)
				{
					SendReply(player, "Now make your way south from the second location to find the last general. She might be near the road, close to the field. Look around for a campfire. You have one more fairy general to find.");
				}
				else if (quest.Step == 5)
				{
					SendReply(player, "From the third location, travel west across the road to reach Master Frederick.");
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
			if(masterFrederick.CanGiveQuest(typeof (Collection), player)  <= 0)
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
				if (!masterFrederick.GiveQuest(typeof (Collection), player, 1))
					return;

				GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
				GameEventMgr.AddHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));

				masterFrederick.SayTo(player, "Alright recruit, let's get started. You will need to find and defeat three key ire fairy generals, so to speak. According to my map, there is a [camp] to the north of this tower, to the northeast and to the south, near the stable across the road.");
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
						return "[Step #1] Listen to Master Frederick. If you are stuck and he won't talk with you, ask him what [camp] he knows of for the Ire Fairies.";
					case 2:
						return "[Step #2] You must now USE the Dusty Old Map for directions to the first Ire Fairy encampment. To use an item, right click on it and type /use.";
					case 3:
						return "[Step #3] You must now USE the Dusty Old Map for the directions to the second Ire Fairy encampment. To use an item, right click on it and type /use.";
					case 4:
						return "[Step #4] You must now USE the Dusty Old Map for directions to the last Ire Fairy encampment. To use an item, right click on it and type /use.";
					case 5:
						return "[Step #5] Return to Master Frederick in Cotswold. From the third location, travel west across the road.";
					case 6:
						return "[Step #6] Hand Master Frederick the fairy wings.";
					case 7:
						return "[Step #6] Hand Master Frederick the fairy wings.";
					case 8:
						return "[Step #6] Hand Master Frederick the fairy wings.";
					case 9:
						return "[Step #7] Wait for Master Frederick to reward you. If he stops speaking with you, ask if you might have a [reward] for your troubles.";
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player==null || player.IsDoingQuest(typeof (Collection)) == null)
				return;

			if (Step >= 2 && Step <= 4 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;

				if (gArgs.Target.Name == generalNames[Step - 2])
				{
					SendSystemMessage("You slay the creature and take it's wing.");
					GiveItem(gArgs.Target, player, fairyGeneralWings);
					SendSystemMessage(player, generalNames[Step - 2] + " yells at you, \"A curse on you land-bound monster! I curse you for killing me!\"");
					Step++;
					return;
				}
			}

			if (Step >= 6 && Step <= 8 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == masterFrederick.Name && gArgs.Item.Id_nb == fairyGeneralWings.Id_nb)
				{
					if (Step == 6)
					{
						masterFrederick.SayTo(player, "Excellent. Please, hand me the second one now.");
					}
					else if (Step == 7)
					{
						masterFrederick.SayTo(player, "Yes, now please, hand me the third one.");
					}
					else if (Step == 8)
					{
						masterFrederick.SayTo(player, "Ah, the third wing. Yes, excellent job Vinde. Now, for your [reward]");
					}
					RemoveItem(masterFrederick, player, fairyGeneralWings);
					Step++;
					return;
				}
			}

		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, dustyOldMap, false);
			RemoveItem(m_questPlayer, fairyGeneralWings, false);
			RemoveItem(m_questPlayer, fairyGeneralWings, false);
			RemoveItem(m_questPlayer, fairyGeneralWings, false);

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			//Give reward to player here ...  
			RemoveItem(masterFrederick, m_questPlayer, dustyOldMap);

			if (m_questPlayer.HasAbilityToUseItem(recruitsArms))
				GiveItem(masterFrederick, m_questPlayer, recruitsArms);
			else
				GiveItem(masterFrederick, m_questPlayer, recruitsSleeves);

			m_questPlayer.GainExperience(240, 0, 0, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 6, Util.Random(50)), "You recieve {0} as a reward.");

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));

		}

	}
}
