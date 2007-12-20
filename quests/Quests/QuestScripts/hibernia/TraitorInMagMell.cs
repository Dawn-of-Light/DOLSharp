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
 * 2) Go to loc=12235,8713 Lough Derg. and /use the Sluagh Necklace from Addrir. 
 * 3) Speak with Lady Legada when she arrives to receive the Sluagh Plans. 
 * 4) Take the Sluagh Plans back to Addrir for your reward.
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

namespace DOL.GS.Quests.Hibernia
{
	/* The first thing we do, is to declare the class we create
	 * as Quest. To do this, we derive from the abstract class
	 * AbstractQuest
	 * 	 
	 */

	public class TraitorInMagMell : BaseAddrirQuest
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
		protected const string questTitle = "Traitor in Mag Mell";
		protected const int minimumLevel = 2;
		protected const int maximumLevel = 2;

		private static GameNPC addrir = null;
		private static GameNPC ladyLegada = null;

		private static ItemTemplate necklaceOfDoppelganger = null;
		private static ItemTemplate sluaghPlans = null;
		private static ItemTemplate recruitsBoots = null;
		private static ItemTemplate recruitsQuiltedBoots = null;

		private static GameLocation legadaEnd = new GameLocation("Lady Legada", 200, 200, 12235, 8713, 5304, 218);
		private static GameLocation legadaStart = new GameLocation("Lady Legada", 200, 330877, 492742, 5439, 2657);


		/* We need to define the constructors from the base class here, else there might be problems
		 * when loading this quest...
		 */
		public TraitorInMagMell() : base()
		{
		}

		public TraitorInMagMell(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public TraitorInMagMell(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public TraitorInMagMell(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Lady Legada", eRealm.None);
			if (npcs.Length == 0)
			{
				//if (log.IsWarnEnabled) log.Warn("Could not find Lady Felin, creating her ...");
				ladyLegada = new GameNPC();
				ladyLegada.Model = 679;
				ladyLegada.Name = "Lady Legada";
				ladyLegada.GuildName = "Part of " + questTitle + " Quest";
				ladyLegada.Realm = (byte) eRealm.None;
				ladyLegada.CurrentRegionID = legadaStart.RegionID;
				ladyLegada.Size = 50;
				ladyLegada.Level = 30;
				ladyLegada.X = legadaStart.X;
				ladyLegada.Y = legadaStart.Y;
				ladyLegada.Z = legadaStart.Z;
				ladyLegada.Heading = legadaStart.Heading;

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 0;
				ladyLegada.SetOwnBrain(brain);

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				//ladyFelin.SaveIntoDatabase();                
				//ladyFelin.AddToWorld();
			}
			else
				ladyLegada = npcs[0];

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
			sluaghPlans = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "sluagh_plans");
			if (sluaghPlans == null)
			{
				sluaghPlans = new ItemTemplate();
				sluaghPlans.Name = "Sluagh Plans";
				if (log.IsWarnEnabled)
					log.Warn("Could not find" + sluaghPlans.Name + " , creating it ...");

				sluaghPlans.Weight = 3;
				sluaghPlans.Model = 498;

				sluaghPlans.Object_Type = (int) eObjectType.GenericItem;

				sluaghPlans.Id_nb = "sluagh_plans";
				sluaghPlans.IsPickable = true;
				sluaghPlans.IsDropable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(sluaghPlans);
			}

			// item db check
			recruitsBoots = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "recruits_cailiocht_boots");
			if (recruitsBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Recruit's Cailiocht Boots, creating it ...");
				recruitsBoots = new ItemTemplate();
				recruitsBoots.Name = "Recruit's Cailiocht Boots";
				recruitsBoots.Level = 7;

				recruitsBoots.Weight = 24;
				recruitsBoots.Model = 84; // studded Boots

				recruitsBoots.DPS_AF = 12; // Armour
				recruitsBoots.SPD_ABS = 19; // Absorption

				recruitsBoots.Object_Type = (int) eObjectType.Reinforced;
				recruitsBoots.Item_Type = (int) eEquipmentItems.FEET;
				recruitsBoots.Id_nb = "recruits_cailiocht_boots";
				recruitsBoots.Gold = 0;
				recruitsBoots.Silver = 10;
				recruitsBoots.Copper = 0;
				recruitsBoots.IsPickable = true;
				recruitsBoots.IsDropable = true;
				recruitsBoots.Color = 13; // green leather

				recruitsBoots.Bonus = 1; // default bonus

				recruitsBoots.Bonus1 = 3;
				recruitsBoots.Bonus1Type = (int) eStat.STR;

				recruitsBoots.Bonus2 = 1;
				recruitsBoots.Bonus2Type = (int) eStat.DEX;

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
				recruitsQuiltedBoots.Color = 32;

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

			GameEventMgr.AddHandler(addrir, GameLivingEvent.Interact, new DOLEventHandler(TalkToAddrir));
			GameEventMgr.AddHandler(addrir, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToAddrir));

			GameEventMgr.AddHandler(ladyLegada, GameLivingEvent.Interact, new DOLEventHandler(TalkToLadyLegada));
			GameEventMgr.AddHandler(ladyLegada, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToLadyLegada));

			/* Now we bring to addrir the possibility to give this quest to players */
			addrir.AddQuestToGive(typeof (TraitorInMagMell));

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

			GameEventMgr.RemoveHandler(ladyLegada, GameLivingEvent.Interact, new DOLEventHandler(TalkToLadyLegada));
			GameEventMgr.RemoveHandler(ladyLegada, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToLadyLegada));

			/* Now we remove to addrir the possibility to give this quest to players */
			addrir.RemoveQuestToGive(typeof (TraitorInMagMell));
		}

		protected static void PlayerLeftWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			TraitorInMagMell quest = player.IsDoingQuest(typeof (TraitorInMagMell)) as TraitorInMagMell;
			if (quest != null)
			{
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
				// remorph player back...
				if (player.Model == ladyLegada.Model)
				{
					GameClient client = player.Client;
					player.Model = (ushort) client.Account.Characters[client.ActiveCharIndex].CreationModel;
					SendSystemMessage(player, "You change back to your normal form!");
				}

				if (quest.Step == 3)
				{
					if (ladyLegada != null)
						ladyLegada.Delete();
				}
			}
		}

		protected static void PlayerUseSlot(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = (GamePlayer) sender;
			// player already morphed...
			if (player.Model == ladyLegada.Model)
				return;

			TraitorInMagMell quest = (TraitorInMagMell) player.IsDoingQuest(typeof (TraitorInMagMell));
			if (quest == null)
				return;

			if (quest.Step == 2 || quest.Step == 3)
			{
				UseSlotEventArgs uArgs = (UseSlotEventArgs) args;

				InventoryItem item = player.Inventory.GetItem((eInventorySlot)uArgs.Slot);
				if (item != null && item.Id_nb == necklaceOfDoppelganger.Id_nb)
				{
					if (WorldMgr.GetDistance(player, legadaEnd.X, legadaEnd.Y, legadaEnd.Z) < 2500)
					{
						foreach (GamePlayer visPlayer in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
						{
							visPlayer.Out.SendSpellCastAnimation(player, 1, 20);
						}

						player.Model = ladyLegada.Model; // morph to fairie
						SendSystemMessage(player, "You change into a new form!");
						RegionTimer resetTimer = new RegionTimer(player, new RegionTimerCallback(quest.ResetPlayerModel), 60000); // call after 10 minutes                    

						if (!ladyLegada.IsAlive || ladyLegada.ObjectState != GameObject.eObjectState.Active)
						{
							ladyLegada.X = legadaStart.X;
							ladyLegada.Y = legadaStart.Y;
							ladyLegada.Z = legadaStart.Z;
							ladyLegada.Heading = legadaStart.Heading;
							ladyLegada.AddToWorld();
							ladyLegada.WalkTo(legadaEnd.X, legadaEnd.Y, legadaEnd.Z, ladyLegada.MaxSpeed);
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

			TraitorInMagMell quest = player.IsDoingQuest(typeof (TraitorInMagMell)) as TraitorInMagMell;
			if (quest != null)
			{
				GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
				GameEventMgr.AddHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));

				if (quest.Step == 3 && (!ladyLegada.IsAlive || ladyLegada.ObjectState != GameObject.eObjectState.Active))
				{
					ladyLegada.X = legadaEnd.X;
					ladyLegada.Y = legadaEnd.Y;
					ladyLegada.Z = legadaEnd.Z;
					ladyLegada.Heading = legadaEnd.Heading;
					ladyLegada.AddToWorld();
				}
			}
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

			if(addrir.CanGiveQuest(typeof (TraitorInMagMell), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			TraitorInMagMell quest = player.IsDoingQuest(typeof (TraitorInMagMell)) as TraitorInMagMell;

			addrir.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...
					addrir.SayTo(player, "Lirone, this Sluagh problem has its roots deeper into Mag Mell than I originally thought! We have apprehended a [traitor]!");
					return;
				}
				else
				{
					switch (quest.Step)
					{
						case 1:
							addrir.SayTo(player, "Go west from Mag Mell towards Tir na Nog. You will come to the base of the foot hills. Head southwest-south along the base of the hills. The necklace will alert you when you are in the correct place. Hurry now. Time is wasting.");
							break;
						case 4:
							addrir.SayTo(player, "Vinde, you've returned. I'm sure you were successful with your mission. Come come my friend. Hand over the information.");
							break;
						case 5:
							addrir.SayTo(player, "For you, a nice pair of boots to help keep your feet dry and clean. Thank you again for the help you've given me in this matter Lirone. I shall report this to Fagan at once.");
							if (quest.Step == 5)
							{
								quest.FinishQuest();
							}
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
							//case "ire fairies": SendReply(player,"I managed to have a little chat with some of the guardsmen, and it turns out there is a [traitor] here in Cotswold!"); break;
						case "traitor":
							addrir.SayTo(player, "Aye Lirone. I could scarcely believe my ears when I heard the news. A man by the name of Samyr has been in [league] with the Sluagh for a while now.");
							break;
						case "league":
							addrir.SayTo(player, "Samyr has given up some information, but not nearly as much as we hoped for. Fagan has asked me to recruit anyone I could to help deal with this problem. Are you up for the [challenge] Lirone?");
							break;

							//If the player offered his "help", we send the quest dialog now!
						case "challenge":
							player.Out.SendQuestSubscribeCommand(addrir, QuestMgr.GetIDForQuestType(typeof(TraitorInMagMell)), "Will you help Mag Mell by taking on this vital mission?");
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "necklace":
							addrir.SayTo(player, "Aye. I am giving it to you to use for this meeting. You will not have to say anything, or so Samyr says. Just go to the [meeting location], use the necklace and wait for the Sluagh.");
							break;
						case "meeting location":
							addrir.SayTo(player, "Go west from Mag Mell towards Tir na Nog. You will come to the base of the foot hills. Head southwest-south along the base of the hills. Hurry now. Time is wasting.");
							if (quest.Step == 1)
							{
								quest.Step = 2;
							}
							break;

							// step 5
						case "reward":
							addrir.SayTo(player, "For you, a nice pair of boots to help keep your feet dry and clean. Thank you again for the help you've given me in this matter Lirone. I shall report this to Fagan at once.");
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

		protected static void TalkToLadyLegada(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(addrir.CanGiveQuest(typeof (TraitorInMagMell), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			TraitorInMagMell quest = player.IsDoingQuest(typeof (TraitorInMagMell)) as TraitorInMagMell;

			ladyLegada.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null && quest.Step >= 2)
				{
					ladyLegada.SayTo(player, "Greetings Ally. It is good to see you here. My time is short, so I will make this [brief].");
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
							ladyLegada.SayTo(player, "Our Queen has laid out the plans for Mag Mell and is eager to have your assistance. Here are the plans, memorize your part. Do not fail us in this, lest dire circumstances befall you. Till we meet again.");
							if (quest.Step == 3)
							{
								GiveItem(ladyLegada, player, sluaghPlans);

								new RegionTimer(ladyLegada, new RegionTimerCallback(quest.CastLadyLegada), 10000);
								new RegionTimer(ladyLegada, new RegionTimerCallback(quest.RemoveLadyLegada), 12000);

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
			if (player.IsDoingQuest(typeof (TraitorInMagMell)) != null)
				return true;

			// This checks below are only performed is player isn't doing quest already
			if (player.HasFinishedQuest(typeof (Nuisances)) == 0)
				return false;

			if (!CheckPartAccessible(player, typeof (TraitorInMagMell)))
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
			TraitorInMagMell quest = player.IsDoingQuest(typeof (TraitorInMagMell)) as TraitorInMagMell;

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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(TraitorInMagMell)))
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
			if(addrir.CanGiveQuest(typeof (TraitorInMagMell), player)  <= 0)
				return;

			TraitorInMagMell quest = player.IsDoingQuest(typeof (TraitorInMagMell)) as TraitorInMagMell;

			if (quest != null)
				return;

			if (response == 0x00)
			{
				SendReply(player, "Oh well, if you change your mind, please come back!");
			}
			else
			{
				//Check if we can add the quest!
				if (!addrir.GiveQuest(typeof (TraitorInMagMell), player, 1))
					return;

				addrir.SayTo(player, "Excellent! Now listen, Samyr was to meet with the Sluagh today! He gave us a [necklace] that helped him transform into a Sluagh for a small amount of time.");
				// give necklace                
				GiveItem(addrir, player, necklaceOfDoppelganger);

				GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
				GameEventMgr.AddHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			}
		}

		protected virtual int CastLadyLegada(RegionTimer callingTimer)
		{
			foreach (GamePlayer visPlayer in ladyLegada.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				visPlayer.Out.SendSpellCastAnimation(ladyLegada, 1, 20);
			}
			return 0;

		}


		protected virtual int RemoveLadyLegada(RegionTimer callingTimer)
		{
			if (ladyLegada != null)
				ladyLegada.Delete();
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
						return "[Step #1] Listen as Addrir outlines the details of this important mission. If he stops speaking with you try interacting with him again.";
					case 2:
						return "[Step #2] Take the necklace Addrir has given you and look for the meeting location. Make your way to the Tir na Nog gates, they are to the west of Addrir. Then travel south-southeast along the base of the hills. Use the necklace near to the location.";
					case 3:
						return "[Step #3] Speak with Lady Legada once she arrives.";
					case 4:
						return "[Step #4] Take the Sluagh Plans you received from Lady Legada back to Addrir for further analysis.";
					case 5:
						return "[Step #5] Wait for Addrir to reward you. If he stops speaking with you, ask if he has a [reward] for your efforts.";

				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof (TraitorInMagMell)) == null)
				return;

			if (Step == 4 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == addrir.Name && gArgs.Item.Id_nb == sluaghPlans.Id_nb)
				{
					RemoveItem(addrir, m_questPlayer, sluaghPlans);

					addrir.TurnTo(m_questPlayer);
					addrir.SayTo(m_questPlayer, "Ah! Their plans, but alas, I can not read their language. Hrm...I shall have to think on this. I'm sure there is someone I can find to translate this for me. But never mind that right now. I have a [reward] for you for your hard work and bravery.");
					m_questPlayer.Out.SendEmoteAnimation(addrir, eEmote.Ponder);
					Step = 5;
					return;
				}
			}

		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, necklaceOfDoppelganger, false);
			RemoveItem(m_questPlayer, sluaghPlans, false);

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
		}


		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(addrir, m_questPlayer, necklaceOfDoppelganger);
			//Give reward to player here ...            
			if (m_questPlayer.HasAbilityToUseItem(recruitsBoots))
				GiveItem(addrir, m_questPlayer, recruitsBoots);
			else
				GiveItem(addrir, m_questPlayer, recruitsQuiltedBoots);

			m_questPlayer.GainExperience(40, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 4, Util.Random(50)), "You recieve {0} as a reward.");

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
		}

	}
}
