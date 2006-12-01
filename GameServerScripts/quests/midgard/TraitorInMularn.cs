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
 * 2) Go to loc=56485,55806 Vale of Mularn and /use the Necklace of the Askefruer from Dalikor. 
 * 3) Speak with Lady Hinda when she arrives to receive the Askefruer Plans. 
 * 4) Take the Askefruer Plans back to Dalikor for your reward.
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

	public class TraitorInMularn : BaseDalikorQuest
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
		protected const string questTitle = "Traitor in Mularn";
		protected const int minimumLevel = 2;
		protected const int maximumLevel = 2;

		private static GameNPC dalikor = null;
		private static GameNPC ladyHinda = null;

		private static ItemTemplate necklaceOfDoppelganger = null;
		private static ItemTemplate askefruerPlans = null;
		private static ItemTemplate recruitsBoots = null;
		private static ItemTemplate recruitsQuiltedBoots = null;

		private static GameLocation hindaEnd = new GameLocation("Lady Hinda", 100, 100, 56484, 55671, 4682, 29);
		private static GameLocation hindaStart = new GameLocation("Lady Hinda", 100, 811022, 727295, 4677, 908);


		/* We need to define the constructors from the base class here, else there might be problems
		 * when loading this quest...
		 */

		public TraitorInMularn() : base()
		{
		}

		public TraitorInMularn(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public TraitorInMularn(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public TraitorInMularn(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Lady Hinda", eRealm.None);
			if (npcs.Length == 0)
			{
				//if (log.IsWarnEnabled) log.Warn("Could not find Lady Hinda, creating her ...");
				ladyHinda = new GameNPC();
				ladyHinda.Model = 678;
				ladyHinda.Name = "Lady Hinda";
				ladyHinda.GuildName = "Part of " + questTitle + " Quest";
				ladyHinda.Realm = (byte) eRealm.None;
				ladyHinda.CurrentRegionID = hindaStart.RegionID;
				ladyHinda.Size = 50;
				ladyHinda.Level = 30;
				ladyHinda.X = hindaStart.X;
				ladyHinda.Y = hindaStart.Y;
				ladyHinda.Z = hindaStart.Z;
				ladyHinda.Heading = hindaStart.Heading;

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 0;
				ladyHinda.SetOwnBrain(brain);

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				//ladyHinda.SaveIntoDatabase();                
				//ladyHinda.AddToWorld();
			}
			else
				ladyHinda = npcs[0];

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
				necklaceOfDoppelganger.MaxQuality = 100;
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
			askefruerPlans = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "askefruer_plans");
			if (askefruerPlans == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Askefruer Plans, creating it ...");
				askefruerPlans = new ItemTemplate();
				askefruerPlans.Name = "Akefruer Plans";

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

			// item db check
			recruitsBoots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "recruits_studded_boots_mid");
			if (recruitsBoots == null)
			{
				recruitsBoots = new ItemTemplate();
				recruitsBoots.Name = "Recruit's Studded Boots (Mid)";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsBoots.Name + ", creating it ...");
				recruitsBoots.Level = 7;

				recruitsBoots.Weight = 24;
				recruitsBoots.Model = 84; // studded Boots

				recruitsBoots.DPS_AF = 12; // Armour
				recruitsBoots.SPD_ABS = 19; // Absorption

				recruitsBoots.Object_Type = (int) eObjectType.Studded;
				recruitsBoots.Item_Type = (int) eEquipmentItems.FEET;
				recruitsBoots.Id_nb = "recruits_studded_boots_mid";
				recruitsBoots.Gold = 0;
				recruitsBoots.Silver = 10;
				recruitsBoots.Copper = 0;
				recruitsBoots.IsPickable = true;
				recruitsBoots.IsDropable = true;
				recruitsBoots.Color = 14;

				recruitsBoots.Bonus = 1; // default bonus

				recruitsBoots.Bonus1 = 3;
				recruitsBoots.Bonus1Type = (int) eStat.STR;


				recruitsBoots.Bonus2 = 1;
				recruitsBoots.Bonus2Type = (int) eStat.CON;

				recruitsBoots.Bonus3 = 1;
				recruitsBoots.Bonus3Type = (int) eResist.Spirit;

				recruitsBoots.Quality = 100;
				recruitsBoots.MaxQuality = 100;
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
				recruitsQuiltedBoots.Color = 36;

				recruitsQuiltedBoots.Bonus = 5; // default bonus

				recruitsQuiltedBoots.Bonus1 = 3;
				recruitsQuiltedBoots.Bonus1Type = (int) eStat.CON;


				recruitsQuiltedBoots.Bonus2 = 1;
				recruitsQuiltedBoots.Bonus2Type = (int) eStat.STR;

				recruitsQuiltedBoots.Bonus3 = 1;
				recruitsQuiltedBoots.Bonus3Type = (int) eResist.Spirit;

				recruitsQuiltedBoots.Quality = 100;
				recruitsQuiltedBoots.MaxQuality = 100;
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
			//We want to be notified whenever a player enters the world
			GameEventMgr.AddHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));

			GameEventMgr.AddHandler(dalikor, GameLivingEvent.Interact, new DOLEventHandler(TalkToDalikor));
			GameEventMgr.AddHandler(dalikor, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToDalikor));

			GameEventMgr.AddHandler(ladyHinda, GameLivingEvent.Interact, new DOLEventHandler(TalkToLadyHinda));
			GameEventMgr.AddHandler(ladyHinda, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToLadyHinda));

			/* Now we bring to dalikor the possibility to give this quest to players */
			dalikor.AddQuestToGive(typeof (TraitorInMularn));

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

			GameEventMgr.RemoveHandler(ladyHinda, GameLivingEvent.Interact, new DOLEventHandler(TalkToLadyHinda));
			GameEventMgr.RemoveHandler(ladyHinda, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToLadyHinda));

			/* Now we remove to dalikor the possibility to give this quest to players */
			dalikor.RemoveQuestToGive(typeof (TraitorInMularn));
		}

		protected static void PlayerLeftWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			TraitorInMularn quest = player.IsDoingQuest(typeof (TraitorInMularn)) as TraitorInMularn;
			if (quest != null)
			{
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
				// remorph player back...
				if (player.Model == ladyHinda.Model)
				{
					GameClient client = player.Client;
					player.Model = (ushort) client.Account.Characters[client.ActiveCharIndex].CreationModel;
					SendSystemMessage(player, "You change back to your normal form!");
				}

				if (quest.Step == 3)
				{
					if (ladyHinda != null)
						ladyHinda.Delete();
				}
			}
		}

		protected static void PlayerUseSlot(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = (GamePlayer) sender;
			// player already morphed...
			if (player.Model == ladyHinda.Model)
				return;

			TraitorInMularn quest = (TraitorInMularn) player.IsDoingQuest(typeof (TraitorInMularn));
			if (quest == null)
				return;

			if (quest.Step == 2 || quest.Step == 3)
			{
				UseSlotEventArgs uArgs = (UseSlotEventArgs) args;

				InventoryItem item = player.Inventory.GetItem((eInventorySlot)uArgs.Slot);
				if (item != null && item.Id_nb == necklaceOfDoppelganger.Id_nb)
				{
					if (WorldMgr.GetDistance(player, hindaEnd.X, hindaEnd.Y, hindaEnd.Z) < 2500)
					{
						foreach (GamePlayer visPlayer in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
						{
							visPlayer.Out.SendSpellCastAnimation(player, 1, 20);
						}

						player.Model = ladyHinda.Model; // morph to fairie
						SendSystemMessage(player, "You change into a new form!");
						new RegionTimer(player, new RegionTimerCallback(quest.ResetPlayerModel), 60000); // call after 10 minutes                    

						if (!ladyHinda.IsAlive || ladyHinda.ObjectState != GameObject.eObjectState.Active)
						{
							ladyHinda.X = hindaStart.X;
							ladyHinda.Y = hindaStart.Y;
							ladyHinda.Z = hindaStart.Z;
							ladyHinda.Heading = hindaStart.Heading;
							ladyHinda.AddToWorld();
							ladyHinda.WalkTo(hindaEnd.X, hindaEnd.Y, hindaEnd.Z, ladyHinda.MaxSpeed);
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

			TraitorInMularn quest = player.IsDoingQuest(typeof (TraitorInMularn)) as TraitorInMularn;
			if (quest != null)
			{
				GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
				GameEventMgr.AddHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));

				if (quest.Step == 3 && (!ladyHinda.IsAlive || ladyHinda.ObjectState != GameObject.eObjectState.Active))
				{
					ladyHinda.X = hindaEnd.X;
					ladyHinda.Y = hindaEnd.Y;
					ladyHinda.Z = hindaEnd.Z;
					ladyHinda.Heading = hindaEnd.Heading;
					ladyHinda.AddToWorld();
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

			if( dalikor.CanGiveQuest(typeof (TraitorInMularn), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			TraitorInMularn quest = player.IsDoingQuest(typeof (TraitorInMularn)) as TraitorInMularn;

			dalikor.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...
					dalikor.SayTo(player, "Recruit Eeinken. It seems as though we have caught a traitor within the walls of Mularn! A man by the name of Njarmir has been conspiring with the Askefruer. He has recently told us of a [meeting] he was to have with them.");
					return;
				}
				else
				{
					switch (quest.Step)
					{
						case 1:
							dalikor.SayTo(player, "Thank you recruit. Now, listen. The traitor has a necklace that allows him to change into the shape of an Askefruer. He says it makes them more comfortable with him. I [have] the necklace with me.");
							break;
						case 2:
						case 3:
							dalikor.SayTo(player, "The traitor described the location as a place between to the north-northeast from the griffin handler, near some small pine trees. I'm sorry there isn't more to go on, but that is all I have. Hurry now Eeinken.");
							break;
						case 4:
							dalikor.SayTo(player, "Welcome back recruit. Have you met with success? Were you able to secure any information from the Askefruer?");
							break;
						case 5:
							dalikor.SayTo(player, "Ah! The plans of the Askefruer. Ah, but they are in a language I do not understand. I will have to take this to the elders of Mularn for further study. Before I do that, though, I have [something] here for you.");
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
						case "meeting":
							dalikor.SayTo(player, "He was to get further instructions from the Askefruer about his mission to help their queen. Now that he is in custody, we need to be sure we do not tip our hand to the Askefruer too quickly. I am asking you to go in his [stead].");
							break;
						case "stead":
							dalikor.SayTo(player, "Will you do this for Mularn Eeinken? Will you go in this traitor's place and get the [information] we need to stop the Askefruer from continuing to make trouble for us?");
							break;

							//If the player offered his "help", we send the quest dialog now!
						case "information":
							player.Out.SendCustomDialog("Will you help Mularn by taking on this vital mission?", new CustomDialogResponse(CheckPlayerAcceptQuest));
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "have":
							dalikor.SayTo(player, "The traitor described the location as a place between to the north-northeast from the griffin handler, near some small pine trees. I'm sorry there isn't more to go on, but that is all I have. Hurry now Eeinken.");
							if (quest.Step == 1)
							{
								quest.Step = 2;
							}
							break;
							// step 5
						case "something":
							dalikor.SayTo(player, "You did such an excellent job going to the meeting and posing as an Askefruer that I have this for you. It isn't much, but it will help to protect you from the elements. Now, if you'll excuse me, I must be off to speak with the elders.");
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

		protected static void TalkToLadyHinda(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if( dalikor.CanGiveQuest(typeof (TraitorInMularn), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			TraitorInMularn quest = player.IsDoingQuest(typeof (TraitorInMularn)) as TraitorInMularn;

			ladyHinda.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null && quest.Step >= 2)
				{
					ladyHinda.SayTo(player, "Ah, our loyal ally, you have made it to our meeting. I was afraid you would not show. You have proven me wrong and strengthened my faith in your servitude to us. My time here is short, so I will make this [brief].");
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
							ladyHinda.SayTo(player, "I have with me the plans for tomorrow night. We will need your help in being prepared. Your instructions are written on the parchment. Take it and memorize your duties, lest our Queen be angered. I bid you farewell for now.");
							if (quest.Step == 3)
							{
								GiveItem(ladyHinda, player, askefruerPlans);

								new RegionTimer(ladyHinda, new RegionTimerCallback(quest.CastLadyFelin), 10000);
								new RegionTimer(ladyHinda, new RegionTimerCallback(quest.RemoveLadyFelin), 12000);

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
			if (player.IsDoingQuest(typeof (TraitorInMularn)) != null)
				return true;

			// This checks below are only performed is player isn't doing quest already
			if (player.HasFinishedQuest(typeof (Nuisances)) == 0)
				return false;

			if (!CheckPartAccessible(player, typeof (TraitorInMularn)))
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
			TraitorInMularn quest = player.IsDoingQuest(typeof (TraitorInMularn)) as TraitorInMularn;

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
			if( dalikor.CanGiveQuest(typeof (TraitorInMularn), player)  <= 0)
				return;

			TraitorInMularn quest = player.IsDoingQuest(typeof (TraitorInMularn)) as TraitorInMularn;

			if (quest != null)
				return;

			if (response == 0x00)
			{
				SendReply(player, "Oh well, if you change your mind, please come back!");
			}
			else
			{
				//Check if we can add the quest!
				if (!dalikor.GiveQuest(typeof (TraitorInMularn), player, 1))
					return;

				dalikor.SayTo(player, "Thank you recruit. Now, listen. The traitor has a necklace that allows him to change into the shape of an Askefruer. He says it makes them more comfortable with him. I [have] the necklace with me.");
				// give necklace                
				GiveItem(dalikor, player, necklaceOfDoppelganger);

				GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
				GameEventMgr.AddHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			}
		}

		protected virtual int CastLadyFelin(RegionTimer callingTimer)
		{
			foreach (GamePlayer visPlayer in ladyHinda.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				visPlayer.Out.SendSpellCastAnimation(ladyHinda, 1, 20);
			}
			return 0;
		}


		protected virtual int RemoveLadyFelin(RegionTimer callingTimer)
		{
			if (ladyHinda != null)
				ladyHinda.Delete();
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
						return "[Step #1] Listen to Dalikor outlines the details of this important mission. If he stops speaking with you ask him if you might [have] the necklace.";
					case 2:
						return "[Step #2] Take the necklace Dalikor has given you and look for the meeting location. Make your way to the Jordheim gates, they are to the southeast of Dalikor. Then travel north-northeast from the griffin trainer, into some pine trees.";
					case 3:
						return "[Step #3] Speak with Lady Hinda once she arrives.";
					case 4:
						return "[Step #4] Take the Askefruer Plans you received from Lady Hinda back to Dalikor for further analysis.";
					case 5:
						return "[Step #5] Wait for Dalikor to reward you. If he stops speaking with you, ask if there is [something] you might be given for your efforts.";
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player==null || player.IsDoingQuest(typeof (TraitorInMularn)) == null)
				return;

			if (Step == 4 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == dalikor.Name && gArgs.Item.Id_nb == askefruerPlans.Id_nb)
				{
					RemoveItem(dalikor, m_questPlayer, askefruerPlans);

					dalikor.TurnTo(m_questPlayer);
					dalikor.SayTo(m_questPlayer, "Ah! The plans of the Askefruer. Ah, but they are in a language I do not understand. I will have to take this to the elders of Mularn for further study. Before I do that, though, I have [something] here for you.");
					m_questPlayer.Out.SendEmoteAnimation(dalikor, eEmote.Ponder);
					Step = 5;
					return;
				}
			}

		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, necklaceOfDoppelganger, false);
			RemoveItem(m_questPlayer, askefruerPlans, false);

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
		}


		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(dalikor, m_questPlayer, necklaceOfDoppelganger);
			//Give reward to player here ...            
			if (m_questPlayer.HasAbilityToUseItem(recruitsBoots))
				GiveItem(dalikor, m_questPlayer, recruitsBoots);
			else
				GiveItem(dalikor, m_questPlayer, recruitsQuiltedBoots);

			m_questPlayer.GainExperience(40, 0, 0, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 4, Util.Random(50)), "You recieve {0} as a reward.");

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
		}

	}
}
