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
 * 1) Travel to loc=19105,26552 Camelot Hills to speak with Master Frederick 
 * 2) Go to loc=10135,31616 Camelot Hills and /use the Necklace of the Doppleganger from Master Frederick 
 * 3) Speak with Lady Felin when she arrives to receive the Ire Fairy Plans. 
 * 4) Take the Ire Fairy Plans back to Master Frederick for your reward.
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

	public class TraitorInCotswold : BaseFrederickQuest
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
		protected const string questTitle = "Traitor in Cotswold";
		protected const int minimumLevel = 2;
		protected const int maximumLevel = 2;

		private static GameNPC masterFrederick = null;
		private static GameNPC ladyFelin = null;

		private static ItemTemplate necklaceOfDoppelganger = null;
		private static ItemTemplate fairyPlans = null;
		private static ItemTemplate recruitsBoots = null;
		private static ItemTemplate recruitsQuiltedBoots = null;

		private static GameLocation felinEnd = new GameLocation("Lady Felin", 1, 558999, 514944, 2628, 2332);
		private static GameLocation felinStart = new GameLocation("Lady Felin", 1, 558846, 516434, 2519, 2332);


		/* We need to define the constructors from the base class here, else there might be problems
		 * when loading this quest...
		 */
		public TraitorInCotswold() : base()
		{
		}

		public TraitorInCotswold(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public TraitorInCotswold(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public TraitorInCotswold(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Lady Felin", eRealm.None);
			if (npcs.Length == 0)
			{
				//if (log.IsWarnEnabled) log.Warn("Could not find Lady Felin, creating her ...");
				ladyFelin = new GameNPC();
				ladyFelin.Model = 603;
				ladyFelin.Name = "Lady Felin";
				ladyFelin.GuildName = "Part of " + questTitle + " Quest";
				ladyFelin.Realm = eRealm.None;
				ladyFelin.CurrentRegionID = 1;
				ladyFelin.Size = 50;
				ladyFelin.Level = 30;
				ladyFelin.X = 558846;
				ladyFelin.Y = 516434;
				ladyFelin.Z = 2519;
				ladyFelin.Heading = 2332;

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 0;
				ladyFelin.SetOwnBrain(brain);

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				//ladyFelin.SaveIntoDatabase();                
				//ladyFelin.AddToWorld();
			}
			else
				ladyFelin = npcs[0];

			#endregion

			#region defineItems

			// item db check
			necklaceOfDoppelganger = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "necklace_of_the_doppelganger");
			if (necklaceOfDoppelganger == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Necklace of the Doppelganger, creating it ...");
				necklaceOfDoppelganger = new ItemTemplate();
				necklaceOfDoppelganger.Name = "Necklace of the Doppelganger";
				necklaceOfDoppelganger.Level = 2;
				necklaceOfDoppelganger.Weight = 2;
				necklaceOfDoppelganger.Model = 101;

				necklaceOfDoppelganger.Object_Type = (int) eObjectType.Magical;
				necklaceOfDoppelganger.Item_Type = (int) eEquipmentItems.NECK;
				necklaceOfDoppelganger.Id_nb = "necklace_of_the_doppelganger";
				necklaceOfDoppelganger.Gold = 0;
				necklaceOfDoppelganger.Silver = 0;
				necklaceOfDoppelganger.Copper = 0;
				necklaceOfDoppelganger.IsPickable = true;
				necklaceOfDoppelganger.IsDropable = false;

				necklaceOfDoppelganger.Quality = 100;
				necklaceOfDoppelganger.Condition = 1000;
				necklaceOfDoppelganger.MaxCondition = 1000;
				necklaceOfDoppelganger.Durability = 1000;
				necklaceOfDoppelganger.MaxDurability = 1000;


				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(necklaceOfDoppelganger);
			}

			// item db check
			fairyPlans = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ire_fairy_plans");
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
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(fairyPlans);
			}

			// item db check
			recruitsBoots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "recruits_studded_boots");
			if (recruitsBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Recruit's Studded Boots, creating it ...");
				recruitsBoots = new ItemTemplate();
				recruitsBoots.Name = "Recruit's Studded Boots";
				recruitsBoots.Level = 7;

				recruitsBoots.Weight = 24;
				recruitsBoots.Model = 84; // studded Boots

				recruitsBoots.DPS_AF = 12; // Armour
				recruitsBoots.SPD_ABS = 19; // Absorption

				recruitsBoots.Object_Type = (int) eObjectType.Studded;
				recruitsBoots.Item_Type = (int) eEquipmentItems.FEET;
				recruitsBoots.Id_nb = "recruits_studded_boots";
				recruitsBoots.Gold = 0;
				recruitsBoots.Silver = 10;
				recruitsBoots.Copper = 0;
				recruitsBoots.IsPickable = true;
				recruitsBoots.IsDropable = true;
				recruitsBoots.Color = 9; // red leather

				recruitsBoots.Bonus = 5; // default bonus

				recruitsBoots.Bonus1 = 1;
				recruitsBoots.Bonus1Type = (int) eStat.STR;

				recruitsBoots.Bonus2 = 3;
				recruitsBoots.Bonus2Type = (int) eStat.CON;

				recruitsBoots.Bonus3 = 1;
				recruitsBoots.Bonus3Type = (int) eResist.Spirit;

				recruitsBoots.Quality = 100;
				recruitsBoots.Condition = 1000;
				recruitsBoots.MaxCondition = 1000;
				recruitsBoots.Durability = 1000;
				recruitsBoots.MaxDurability = 1000;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsBoots);
			}

			// item db check
			recruitsQuiltedBoots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "recruits_quilted_boots");
			if (recruitsQuiltedBoots == null)
			{
				recruitsQuiltedBoots = new ItemTemplate();
				recruitsQuiltedBoots.Name = "Recruit's Quilted Boots";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsQuiltedBoots.Name + ", creating it ...");
				recruitsQuiltedBoots.Level = 7;

				recruitsQuiltedBoots.Weight = 8;
				recruitsQuiltedBoots.Model = 155; // studded Boots

				recruitsQuiltedBoots.DPS_AF = 6; // Armour
				recruitsQuiltedBoots.SPD_ABS = 0; // Absorption

				recruitsQuiltedBoots.Object_Type = (int) eObjectType.Cloth;
				recruitsQuiltedBoots.Item_Type = (int) eEquipmentItems.FEET;
				recruitsQuiltedBoots.Id_nb = "recruits_quilted_boots";
				recruitsQuiltedBoots.Gold = 0;
				recruitsQuiltedBoots.Silver = 10;
				recruitsQuiltedBoots.Copper = 0;
				recruitsQuiltedBoots.IsPickable = true;
				recruitsQuiltedBoots.IsDropable = true;
				recruitsQuiltedBoots.Color = 27; // red leather

				recruitsQuiltedBoots.Bonus = 5; // default bonus

				recruitsQuiltedBoots.Bonus1 = 3;
				recruitsQuiltedBoots.Bonus1Type = (int) eStat.CON;


				recruitsQuiltedBoots.Bonus2 = 1;
				recruitsQuiltedBoots.Bonus2Type = (int) eStat.STR;

				recruitsQuiltedBoots.Bonus3 = 1;
				recruitsQuiltedBoots.Bonus3Type = (int) eResist.Spirit;

				recruitsQuiltedBoots.Quality = 100;
				recruitsQuiltedBoots.Condition = 1000;
				recruitsQuiltedBoots.MaxCondition = 1000;
				recruitsQuiltedBoots.Durability = 1000;
				recruitsQuiltedBoots.MaxDurability = 1000;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsQuiltedBoots);
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

			GameEventMgr.AddHandler(ladyFelin, GameLivingEvent.Interact, new DOLEventHandler(TalkToLadyFelin));
			GameEventMgr.AddHandler(ladyFelin, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToLadyFelin));

			/* Now we bring to masterFrederick the possibility to give this quest to players */
			masterFrederick.AddQuestToGive(typeof (TraitorInCotswold));

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

			GameEventMgr.RemoveHandler(ladyFelin, GameLivingEvent.Interact, new DOLEventHandler(TalkToLadyFelin));
			GameEventMgr.RemoveHandler(ladyFelin, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToLadyFelin));
		
			/* Now we remove to masterFrederick the possibility to give this quest to players */
			masterFrederick.RemoveQuestToGive(typeof (TraitorInCotswold));
		}

		protected static void PlayerLeftWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			TraitorInCotswold quest = player.IsDoingQuest(typeof (TraitorInCotswold)) as TraitorInCotswold;
			if (quest != null)
			{
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
				// remorph player back...
				if (player.Model == ladyFelin.Model)
				{
					GameClient client = player.Client;
					player.Model = (ushort) client.Account.Characters[client.ActiveCharIndex].CreationModel;
					SendSystemMessage(player, "You change back to your normal form!");
				}

				if (quest.Step == 3)
				{
					if (ladyFelin != null)
						ladyFelin.Delete();
				}
			}
		}

		protected static void PlayerUseSlot(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = (GamePlayer) sender;
			// player already morphed...
			if (player.Model == ladyFelin.Model)
				return;

			TraitorInCotswold quest = (TraitorInCotswold) player.IsDoingQuest(typeof (TraitorInCotswold));
			if (quest == null)
				return;

			if (quest.Step == 2 || quest.Step == 3)
			{
				UseSlotEventArgs uArgs = (UseSlotEventArgs) args;

				InventoryItem item = player.Inventory.GetItem((eInventorySlot)uArgs.Slot);
				if (item != null && item.Id_nb == necklaceOfDoppelganger.Id_nb)
				{
					if (player.IsWithinRadius( felinEnd, 2500 ))
					{
						foreach (GamePlayer visPlayer in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
						{
							visPlayer.Out.SendSpellCastAnimation(player, 1, 20);
						}

						player.Model = ladyFelin.Model; // morph to fairie
						SendSystemMessage(player, "You change into a new form!");
						new RegionTimer(player, new RegionTimerCallback(quest.ResetPlayerModel), 60000); // call after 10 minutes                    

						if (!ladyFelin.IsAlive || ladyFelin.ObjectState != GameObject.eObjectState.Active)
						{
							ladyFelin.X = felinStart.X;
							ladyFelin.Y = felinStart.Y;
							ladyFelin.Z = felinStart.Z;
							ladyFelin.Heading = felinStart.Heading;
							ladyFelin.AddToWorld();
							ladyFelin.WalkTo(felinEnd.X, felinEnd.Y, felinEnd.Z, ladyFelin.MaxSpeed);
						}
						quest.Step = 3;
					}
				}
			}
		}

		protected static void PlayerEnterWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			TraitorInCotswold quest = player.IsDoingQuest(typeof (TraitorInCotswold)) as TraitorInCotswold;
			if (quest != null)
			{
				GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
				GameEventMgr.AddHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));

				if (quest.Step == 3 && (!ladyFelin.IsAlive || ladyFelin.ObjectState != GameObject.eObjectState.Active))
				{
					ladyFelin.X = felinEnd.X;
					ladyFelin.Y = felinEnd.Y;
					ladyFelin.Z = felinEnd.Z;
					ladyFelin.Heading = felinEnd.Heading;
					ladyFelin.AddToWorld();
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

			if(masterFrederick.CanGiveQuest(typeof (TraitorInCotswold), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			TraitorInCotswold quest = player.IsDoingQuest(typeof (TraitorInCotswold)) as TraitorInCotswold;

			masterFrederick.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...
					masterFrederick.SayTo(player, "Vinde! I have some important news regarding the recent capture of the [ire fairies].");
					return;
				}
				else
				{
					if (quest.Step == 4)
					{
						masterFrederick.SayTo(player, "Vinde, you've returned. I'm sure you were successful with your mission. Come come my friend. Hand over the information.");

					}
					else if (quest.Step == 5)
					{
						masterFrederick.SayTo(player, "Hrm...These plans are sketchy, at best. I wonder if Shaemus was supposed to add in his two coppers. Interesting nonetheless. Thank you for helping us out Vinde. I have [something] here for you.");

					}
					else
					{
						masterFrederick.SayTo(player, "I know, it's not our speciality, but it's something we have to do for the betterment of the realm! Now, according to Shaemus, he was meeting them at a [small grove] to the south of Cotswold.");
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
						case "ire fairies":
							masterFrederick.SayTo(player, "I managed to have a little chat with some of the guardsmen, and it turns out there is a [traitor] here in Cotswold!");
							break;
						case "traitor":
							masterFrederick.SayTo(player, "I know, it's shocking. I can hardly believe it myself. The guards were more than happy to share their information, so I wanted to pass it along to you. Apparently, this man named Shaemus was in [league] with the fairies.");
							break;
						case "league":
							masterFrederick.SayTo(player, "His deal with them was to share in the profits of their stealing. I can't believe someone's greed would allow them to make deals with ire fairies! Well, anyhow, he gave us some further information on his [dealings] with them.");
							break;
						case "dealings":
							masterFrederick.SayTo(player, "Shaemus uses a necklace that changes his appearance so he looks like one of the ire fairies. He said it's easier to deal with them that way. The guards confiscated it and it's currently in my possession. I was wondering if you would help us with this severe [problem]. What do you say Vinde, will you help us?");
							break;

							//If the player offered his "help", we send the quest dialog now!
						case "problem":
							player.Out.SendQuestSubscribeCommand(masterFrederick, QuestMgr.GetIDForQuestType(typeof(TraitorInCotswold)), "Will you help Cotswold by taking on this vital mission?");
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "infiltrate":
							masterFrederick.SayTo(player, "I know, it's not our speciality, but it's something we have to do for the betterment of the realm! Now, according to Shaemus, he was meeting them at a [small grove] to the south of Cotswold.");
							break;
						case "small grove":
							masterFrederick.SayTo(player, "He was supposed to be getting more information on their plans, so you shouldn't need to say much. Make your way to the grove and wait for the ire fairies. Return to me when you've gotten the information we're looking for.");
							if (quest.Step == 1)
							{
								quest.Step = 2;
							}
							break;
							// step 5
						case "something":
							masterFrederick.SayTo(player, "Here you are my friend. These boots will help keep your feet dry and warm on those days when you think the weather in Albion is against you. Be well my friend. We will talk again soon.");
							if (quest.Step == 5)
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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(TraitorInCotswold)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

		protected static void TalkToLadyFelin(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(masterFrederick.CanGiveQuest(typeof (TraitorInCotswold), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			TraitorInCotswold quest = player.IsDoingQuest(typeof (TraitorInCotswold)) as TraitorInCotswold;

			ladyFelin.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null && quest.Step >= 2)
				{
					ladyFelin.SayTo(player, "Ah, our loyal ally, you have made it to our meeting. I was afraid you would not show. You have proven me wrong and strengthened my faith in your servitude to us. My time here is short, so I will make this [brief].");
					return;
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
						case "brief":
							ladyFelin.SayTo(player, "I have with me the plans for tomorrow night. We will need your help in being prepared. Your instructions are written on the parchment. Take it and memorize your duties, lest our Queen be angered. I bid you farewell for now.");
							if (quest.Step == 3)
							{
								GiveItem(ladyFelin, player, fairyPlans);

								new RegionTimer(ladyFelin, new RegionTimerCallback(quest.CastLadyFelin), 10000);
								new RegionTimer(ladyFelin, new RegionTimerCallback(quest.RemoveLadyFelin), 12000);

								quest.Step = 4;
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
			if (player.IsDoingQuest(typeof (TraitorInCotswold)) != null)
				return true;

			// This checks below are only performed is player isn't doing quest already
			if (player.HasFinishedQuest(typeof (Nuisances)) == 0)
				return false;

			if (!CheckPartAccessible(player, typeof (TraitorInCotswold)))
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
			TraitorInCotswold quest = player.IsDoingQuest(typeof (TraitorInCotswold)) as TraitorInCotswold;

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
			if(masterFrederick.CanGiveQuest(typeof (TraitorInCotswold), player)  <= 0)
				return;

			TraitorInCotswold quest = player.IsDoingQuest(typeof (TraitorInCotswold)) as TraitorInCotswold;

			if (quest != null)
				return;

			if (response == 0x00)
			{
				SendReply(player, "Oh well, if you change your mind, please come back!");
			}
			else
			{
				//Check if we can add the quest!
				if (!masterFrederick.GiveQuest(typeof (TraitorInCotswold), player, 1))
					return;

				masterFrederick.SayTo(player, "Excellent job recruit. He said his contact's name was Lady Felin. I wasn't aware fairies had any sort of royalty, but that's not important. Here, take this. You'll need to use it in order to [infiltrate] the meeting tonight.");
				// give necklace                
				GiveItem(masterFrederick, player, necklaceOfDoppelganger);

				GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
				GameEventMgr.AddHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			}
		}

		protected virtual int CastLadyFelin(RegionTimer callingTimer)
		{
			foreach (GamePlayer visPlayer in ladyFelin.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				visPlayer.Out.SendSpellCastAnimation(ladyFelin, 1, 20);
			}
			return 0;

		}


		protected virtual int RemoveLadyFelin(RegionTimer callingTimer)
		{
			if (ladyFelin != null)
				ladyFelin.Delete();
			return 0;
		}


		protected virtual int ResetPlayerModel(RegionTimer callingTimer)
		{
			GameClient client = m_questPlayer.Client;
			m_questPlayer.Model = (ushort) client.Account.Characters[client.ActiveCharIndex].CreationModel;
			SendSystemMessage("You change back to your normal form!");
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
						return "[Step #1] Speak with Master Frederick.";
					case 2:
						return "[Step #2] Take the necklace and look for the meeting location. Travel to the bridge that leads to Camelot and then head southeast-south. When you find the location, you will need to USE your necklace. Be sure to speak with Lady Felin.";
					case 3:
						return "[Step #3] Speak with Lady Felin once she arrives.";
					case 4:
						return "[Step #4] Take the plans you received from Lady Felin back to Master Frederick for further analysis.";
					case 5:
						return "[Step #5] Wait for Master Frederick to reward you. If he stops speaking with you, ask if there is [something] you might be given for your efforts.";

				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player==null || player.IsDoingQuest(typeof (TraitorInCotswold)) == null)
				return;

			if (Step == 4 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == masterFrederick.Name && gArgs.Item.Id_nb == fairyPlans.Id_nb)
				{
					RemoveItem(masterFrederick, m_questPlayer, fairyPlans);

					masterFrederick.TurnTo(m_questPlayer);
					masterFrederick.SayTo(m_questPlayer, "This is a very small parchment. Hrm...Let me see if I can make out the words.");
					m_questPlayer.Out.SendEmoteAnimation(masterFrederick, eEmote.Ponder);
					Step = 5;
					return;
				}
			}

		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, necklaceOfDoppelganger, false);
			RemoveItem(m_questPlayer, fairyPlans, false);

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
		}


		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(masterFrederick, m_questPlayer, necklaceOfDoppelganger);
			//Give reward to player here ...            
			if (m_questPlayer.HasAbilityToUseItem(recruitsBoots))
				GiveItem(masterFrederick, m_questPlayer, recruitsBoots);
			else
				GiveItem(masterFrederick, m_questPlayer, recruitsQuiltedBoots);

			m_questPlayer.GainExperience(GameLiving.eXPSource.Quest, 40, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 4, Util.Random(50)), "You recieve {0} as a reward.");

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
		}

	}
}
