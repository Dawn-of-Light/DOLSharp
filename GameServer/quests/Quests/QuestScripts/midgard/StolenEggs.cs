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
 * 2) Go speak with Viking Hyndla loc=53049,58068 Vale of Mularn.Click on her and type /whisper Askefruer. 
 * 3) Run to loc=54739,18264 and kill the Askefruer Trainer. 
 * 4) Click on the grifflet and take him back to Griffin Handler Njiedi at loc=55561,58225 Vale of Mularn. 
 * 5) Give Griffin Handler Njiedi the Askefruer Trainer's Whip and receive part of your reward. 
 * 6) Go speak with Dalikor and /whisper successfully, to receive the rest of your reward.
 */

using System;
using System.Reflection;
using DOL.AI.Brain;
using DOL.Database2;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;
/* I suggest you declare yourself some namespaces for your quests
 * Like: DOL.GS.Quests.Albion
 *       DOL.GS.Quests.Midgard
 *       DOL.GS.Quests.Hibernia
 * Also this is the name that will show up in the database as QuestName
 * so setting good values here will result in easier to read and cleaner
 * GS Code
 */

namespace DOL.GS.Quests.Midgard
{
	/* The first thing we do, is to declare the class we create
	 * as Quest. To do this, we derive from the abstract class
	 * AbstractQuest
	 * 	 
	 */

	public class StolenEggs : BaseDalikorQuest
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

		protected const string questTitle = "Stolen Eggs";
		protected const int minimumLevel = 4;
		protected const int maximumLevel = 4;

		private static GameNPC dalikor = null;
		private static GameStableMaster njiedi = null;
		private static GameNPC hyndla = null;

		private static GameNPC askefruerTrainer = null;
		private GameNPC grifflet = null;

		private bool askefruerGriffinHandlerAttackStarted = false;

		private static ItemTemplate trainerWhip = null;
		private static ItemTemplate dustyOldMap = null;
		private static ItemTemplate recruitsVest = null;
		private static ItemTemplate recruitsQuiltedVest = null;


		/* We need to define the constructors from the base class here, else there might be problems
		 * when loading this quest...
		 */

		public StolenEggs() : base()
		{
		}

		public StolenEggs(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public StolenEggs(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public StolenEggs(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Viking Hyndla", eRealm.Midgard);
			if (npcs.Length == 0)
			{
				hyndla = new GameNPC();
				hyndla.Model = 9;
				hyndla.Name = "Viking Hyndla";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + hyndla.Name + ", creating ...");
				hyndla.GuildName = "Part of " + questTitle + " Quest";
				hyndla.Realm = eRealm.Midgard;
				hyndla.CurrentRegionID = 100;

				hyndla.Size = 50;
				hyndla.Level = 40;
				hyndla.X = GameLocation.ConvertLocalXToGlobalX(53049, 100);
				hyndla.Y = GameLocation.ConvertLocalYToGlobalY(58068, 100);
				hyndla.Z = 4985;
				hyndla.Heading = 150;

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					hyndla.SaveIntoDatabase();

				hyndla.AddToWorld();
			}
			else
				hyndla = npcs[0];

			npcs = (GameNPC[]) WorldMgr.GetObjectsByName("Griffin Handler Njiedi", eRealm.Midgard, typeof (GameStableMaster));
			if (npcs.Length == 0)
			{
				njiedi = new GameStableMaster();
				njiedi.Model = 158;
				njiedi.Name = "Griffin Handler Njiedi";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + njiedi.Name + ", creating ...");
				njiedi.GuildName = "Stable Master";
				njiedi.Realm = eRealm.Midgard;
				njiedi.CurrentRegionID = 100;
				njiedi.Size = 51;
				njiedi.Level = 50;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 81, 10);
				template.AddNPCEquipment(eInventorySlot.LegsArmor, 82, 10);
				template.AddNPCEquipment(eInventorySlot.FeetArmor, 84);
				template.AddNPCEquipment(eInventorySlot.Cloak, 57, 32);
				njiedi.Inventory = template.CloseTemplate();

//				njiedi.AddNPCEquipment(Slot.TORSO, 81, 10, 0, 0);
//				njiedi.AddNPCEquipment(Slot.LEGS, 82, 10, 0, 0);
//				njiedi.AddNPCEquipment(Slot.FEET, 84, 10, 0, 0);
//				njiedi.AddNPCEquipment(Slot.CLOAK, 57, 32, 0, 0);

				njiedi.X = GameLocation.ConvertLocalXToGlobalX(55561, 100);
				njiedi.Y = GameLocation.ConvertLocalYToGlobalY(58225, 100);
				njiedi.Z = 5005;
				njiedi.Heading = 126;

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 0;
				njiedi.SetOwnBrain(brain);

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					njiedi.SaveIntoDatabase();
				njiedi.AddToWorld();
			}
			else
			{
				njiedi = npcs[0] as GameStableMaster;
			}

			npcs = WorldMgr.GetNPCsByName("Askefruer Trainer", eRealm.None);
			if (npcs.Length == 0)
			{
				askefruerTrainer = new GameNPC();

				askefruerTrainer.Name = "Askefruer Trainer";
				askefruerTrainer.X = GameLocation.ConvertLocalXToGlobalX(54739, 100);
				askefruerTrainer.Y = GameLocation.ConvertLocalYToGlobalY(18264, 100);
				askefruerTrainer.Z = 5195;
				askefruerTrainer.Heading = 79;
				askefruerTrainer.Model = 678;
				askefruerTrainer.GuildName = "Part of " + questTitle + " Quest";
				askefruerTrainer.Realm = eRealm.None;
				askefruerTrainer.CurrentRegionID = 100;
				askefruerTrainer.Size = 49;
				askefruerTrainer.Level = 3;

				// Leave at default values to be one the save side ...
				//fairyDragonflyHandler.AggroLevel = 80;
				//fairyDragonflyHandler.AggroRange = 1000;

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					askefruerTrainer.SaveIntoDatabase();

				askefruerTrainer.AddToWorld();
			}
			else
				askefruerTrainer = npcs[0];

			#endregion

			#region defineItems

			trainerWhip = (ItemTemplate) DatabaseLayer.Instance.SelectObject(typeof (ItemTemplate),"Id_nb", "askefruer_whip");
			if (trainerWhip == null)
			{
				trainerWhip = new ItemTemplate();
				trainerWhip.Name = "Askefruer Trainer's Whip";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + trainerWhip.Name + " , creating it ...");

				trainerWhip.Weight = 15;
				trainerWhip.Model = 859;

				trainerWhip.Object_Type = (int) eObjectType.GenericItem;

				trainerWhip.Id_nb = "askefruer_whip";
				trainerWhip.IsPickable = true;
				trainerWhip.IsDropable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(trainerWhip);
			}

			// item db check
			recruitsVest = (ItemTemplate) DatabaseLayer.Instance.SelectObject(typeof (ItemTemplate),"Id_nb", "recruits_studded_vest_mid");
			if (recruitsVest == null)
			{
				recruitsVest = new ItemTemplate();
				recruitsVest.Name = "Recruit's Studded Vest (Mid)";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsVest.Name + ", creating it ...");
				recruitsVest.Level = 8;

				recruitsVest.Weight = 60;
				recruitsVest.Model = 81; // studded vest

				recruitsVest.DPS_AF = 12; // Armour
				recruitsVest.SPD_ABS = 19; // Absorption

				recruitsVest.Object_Type = (int) eObjectType.Studded;
				recruitsVest.Item_Type = (int) eEquipmentItems.TORSO;
				recruitsVest.Id_nb = "recruits_studded_vest_mid";
				recruitsVest.Gold = 0;
				recruitsVest.Silver = 9;
				recruitsVest.Copper = 0;
				recruitsVest.IsPickable = true;
				recruitsVest.IsDropable = true;
				recruitsVest.CanDropAsLoot = false;
				recruitsVest.Color = 14; // blue leather

				recruitsVest.Bonus = 5; // default bonus

				recruitsVest.Bonus1 = 3;
				recruitsVest.Bonus1Type = (int) eStat.STR;

				recruitsVest.Bonus2 = 4;
				recruitsVest.Bonus2Type = (int) eStat.CON;

				recruitsVest.Bonus3 = 1;
				recruitsVest.Bonus3Type = (int) eResist.Body;

				recruitsVest.Quality = 100;
				recruitsVest.Condition = 1000;
				recruitsVest.MaxCondition = 1000;
				recruitsVest.Durability = 1000;
				recruitsVest.MaxDurability = 1000;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsVest);
			}

			// item db check
			recruitsQuiltedVest = (ItemTemplate) DatabaseLayer.Instance.SelectObject(typeof (ItemTemplate),"Id_nb", "recruits_quilted_vest");
			if (recruitsQuiltedVest == null)
			{
				recruitsQuiltedVest = new ItemTemplate();
				recruitsQuiltedVest.Name = "Recruit's Quilted Vest";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsQuiltedVest.Name + ", creating it ...");
				recruitsQuiltedVest.Level = 8;

				recruitsQuiltedVest.Weight = 20;
				recruitsQuiltedVest.Model = 151; // studded vest

				recruitsQuiltedVest.DPS_AF = 6; // Armour
				recruitsQuiltedVest.SPD_ABS = 0; // Absorption

				recruitsQuiltedVest.Object_Type = (int) eObjectType.Cloth;
				recruitsQuiltedVest.Item_Type = (int) eEquipmentItems.TORSO;
				recruitsQuiltedVest.Id_nb = "recruits_quilted_vest";
				recruitsQuiltedVest.Gold = 0;
				recruitsQuiltedVest.Silver = 9;
				recruitsQuiltedVest.Copper = 0;
				recruitsQuiltedVest.IsPickable = true;
				recruitsQuiltedVest.IsDropable = true;
				recruitsQuiltedVest.Color = 36;

				recruitsQuiltedVest.Bonus = 5; // default bonus

				recruitsQuiltedVest.Bonus1 = 4;
				recruitsQuiltedVest.Bonus1Type = (int) eStat.INT;

				recruitsQuiltedVest.Bonus2 = 3;
				recruitsQuiltedVest.Bonus2Type = (int) eStat.DEX;

				recruitsQuiltedVest.Quality = 100;
				recruitsQuiltedVest.Condition = 1000;
				recruitsQuiltedVest.MaxCondition = 1000;
				recruitsQuiltedVest.Durability = 1000;
				recruitsQuiltedVest.MaxDurability = 1000;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsQuiltedVest);
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
			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.AddHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));

			GameEventMgr.AddHandler(dalikor, GameLivingEvent.Interact, new DOLEventHandler(TalkToDalikor));
			GameEventMgr.AddHandler(dalikor, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToDalikor));

			GameEventMgr.AddHandler(njiedi, GameLivingEvent.Interact, new DOLEventHandler(TalkToNjiedi));
			GameEventMgr.AddHandler(njiedi, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToNjiedi));

			GameEventMgr.AddHandler(hyndla, GameLivingEvent.Interact, new DOLEventHandler(TalkToHyndla));
			GameEventMgr.AddHandler(hyndla, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToHyndla));

			GameEventMgr.AddHandler(askefruerTrainer, GameNPCEvent.OnAICallback, new DOLEventHandler(CheckNearAskefruerTrainer));

			/* Now we bring to dalikor the possibility to give this quest to players */
			dalikor.AddQuestToGive(typeof (StolenEggs));

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
			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));

			GameEventMgr.RemoveHandler(dalikor, GameLivingEvent.Interact, new DOLEventHandler(TalkToDalikor));
			GameEventMgr.RemoveHandler(dalikor, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToDalikor));

			GameEventMgr.RemoveHandler(njiedi, GameLivingEvent.Interact, new DOLEventHandler(TalkToNjiedi));
			GameEventMgr.RemoveHandler(njiedi, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToNjiedi));

			GameEventMgr.RemoveHandler(hyndla, GameLivingEvent.Interact, new DOLEventHandler(TalkToHyndla));
			GameEventMgr.RemoveHandler(hyndla, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToHyndla));

			GameEventMgr.RemoveHandler(askefruerTrainer, GameNPCEvent.OnAICallback, new DOLEventHandler(CheckNearAskefruerTrainer));

			/* Now we remove to dalikor the possibility to give this quest to players */
			dalikor.RemoveQuestToGive(typeof (StolenEggs));
		}

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToHyndla(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(dalikor.CanGiveQuest(typeof (StolenEggs), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			StolenEggs quest = player.IsDoingQuest(typeof (StolenEggs)) as StolenEggs;
			hyndla.TurnTo(player);
			if (e == GameLivingEvent.Interact)
			{
				if (quest != null)
				{
					hyndla.SayTo(player, "Greetings to you Viking of Midgard. How may I be of service to you?");
					hyndla.SayTo(player, "Dalikor must have sent you. Yes, I have recently heard of some suspicious activity north of Haggerfel. Passersby have reported seeing a pink colored creature with an egg of sorts. I have told Njiedi, but he is unable to leave his distraught [griffins].");
				}
			}
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				if (quest != null)
				{
					switch (wArgs.Text)
					{
						case "griffins":
							hyndla.SayTo(player, "The area location given to me was vague, but I can give you a general area. First travel towards Haggerfel. Once you reach it, head north-northeast on the road, past the two [tree stumps]. They will be on your right.");
							break;
						case "tree stumps":
							hyndla.SayTo(player, "Just down the road, still heading north-northeast, you will see a group of four very tall pine trees. The pink creature was seen in that vicinity. I wish I could give you more, but that's all I have. Good luck in finding this thing Eeinken.");
							if (quest.Step == 1)
							{
								quest.Step = 2;
								quest.initGrifflet();
								quest.grifflet.AddToWorld();
							}
							break;
					}
				}

			}
		}

		protected static void CheckNearAskefruerTrainer(DOLEvent e, object sender, EventArgs args)
		{
			GameNPC m_askefruerTrainer = (GameNPC) sender;

			// if princess is dead no ned to checks ...
			if (!m_askefruerTrainer.IsAlive || m_askefruerTrainer.ObjectState != GameObject.eObjectState.Active)
				return;

			foreach (GamePlayer player in m_askefruerTrainer.GetPlayersInRadius(1500))
			{
				StolenEggs quest = (StolenEggs) player.IsDoingQuest(typeof (StolenEggs));

				if (quest != null && !quest.askefruerGriffinHandlerAttackStarted && (quest.Step == 2 || quest.Step == 3))
				{
					quest.askefruerGriffinHandlerAttackStarted = true;

					SendSystemMessage(player, askefruerTrainer.GetName(0, true) + " says, \"You shall not take what is rightfully ours land-bound abomination!\"");
					IAggressiveBrain aggroBrain = m_askefruerTrainer.Brain as IAggressiveBrain;
					if (aggroBrain != null)
						aggroBrain.AddToAggroList(player, 70);

					quest.Step = 3;
					// if we find player doing quest stop looking for further ones ...
					break;
				}
			}
		}

		protected void deleteGrifflet()
		{
			if (grifflet != null)
			{
				GameEventMgr.RemoveHandler(grifflet, GameLivingEvent.Interact, new DOLEventHandler(TalkToGrifflet));
				grifflet.Delete();
				grifflet = null;
			}
		}

		protected void initGrifflet()
		{
			grifflet = new GameNPC();

			grifflet.Model = 1236;
			grifflet.Name = "Grifflet";
			grifflet.GuildName = "Part of " + m_questPlayer.GetName(0, false) + "'s " + questTitle + " Quest";
			grifflet.Flags ^= (uint)GameNPC.eFlags.PEACE;
			grifflet.CurrentRegionID = askefruerTrainer.CurrentRegionID;
			grifflet.Size = 20;
			grifflet.Level = 3;
			grifflet.X = askefruerTrainer.X + Util.Random(-150, 150);
			grifflet.Y = askefruerTrainer.Y + Util.Random(-150, 150);
			grifflet.Z = askefruerTrainer.Z;
			grifflet.Heading = 93;
			grifflet.MaxSpeedBase = 200;

			StandardMobBrain brain = new StandardMobBrain();
			brain.AggroLevel = 0;
			brain.AggroRange = 0;
			grifflet.SetOwnBrain(brain);

			//You don't have to store the created mob in the db if you don't want,
			//it will be recreated each time it is not found, just comment the following
			//line if you rather not modify your database
			//dragonflyHatchling.SaveIntoDatabase();                            

			GameEventMgr.AddHandler(grifflet, GameLivingEvent.Interact, new DOLEventHandler(TalkToGrifflet));

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

			if(dalikor.CanGiveQuest(typeof (StolenEggs), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			StolenEggs quest = player.IsDoingQuest(typeof (StolenEggs)) as StolenEggs;

			dalikor.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				//We check if the player is already doing the quest                
				if (quest == null)
				{
					//Player is not doing the quest...
					dalikor.SayTo(player, "Welcome back my friend. We seem to have another Askefruer situation on our hands. The elders don't want to take the guards away from their stations to help, so the task has fallen to you again. Let me [explain] the situation.");
					return;
				}
				else
				{
					if (quest.Step == 7)
					{
						dalikor.SayTo(player, "Ah, recruit, you've returned. I take it you were [successful] in your mission?");
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
						case "explain":
							dalikor.SayTo(player, "About a day ago, Njiedi was monitoring the recent clutch that was layed by his mated pair of griffins. He is hoping to have a few strong riders out of the group when he noticed one of the eggs had gone [missing].");
							break;
						case "missing":
							dalikor.SayTo(player, "He wasn't immediately alarmed, as sometimes eggs have a habit of rolling out of the nest. He looked around the hatchery but could not find the missing egg. When the parents of the clutch came in, the female went to the nest and started to [panic].");
							break;
						case "panic":
							dalikor.SayTo(player, "She too noticed that one of the eggs was missing. She began to fly around the hatchery and could not be calmed by Njiedi or even her mate. That's when Njiedi noticed a small area of the netting around the hatchery was [cut].");
							break;
						case "cut":
							dalikor.SayTo(player, "The opening was very small and it was too far away from the nest to be the work of a person. There was a piece of torn wing on the opening. When Njiedi brought it to me, I recognized it immediately as part of an [Askefruer] wing.");
							break;
						case "Askefruer":
							dalikor.SayTo(player, "I am too busy dealing with the elders to find where this Askefruer has taken the egg, but I know Njiedi and his mated griffins are eager to have the egg returned. If it is out of the nest too long, the baby inside will die. Will you [assist] them?");
							break;

							//If the player offered his "help", we send the quest dialog now!
						case "assist":
							player.Out.SendQuestSubscribeCommand(dalikor, QuestMgr.GetIDForQuestType(typeof(StolenEggs)), "Will you find out where the griffin egg has gone?");
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "successful":
							dalikor.SayTo(player, "Excellent! I heard the egg had already hatched, amazing! I hope the poor thing wasn't hurt too badly. Now it is back in the loving care of its parents, so it should be able to heal emotionally and physically. But I think you [deserve] something.");
							break;

						case "deserve":
							dalikor.SayTo(player, "Here you are my friend. I know it's not much, but a little coin in one's purse never hurts. It means a lot Njiedi, as well as me, that you have so selflessly helped in this situation. That little grifflet would have died without your help. Thank you.");
							if (quest.Step == 7)
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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(StolenEggs)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToGrifflet(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(dalikor.CanGiveQuest(typeof (StolenEggs), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			StolenEggs quest = player.IsDoingQuest(typeof (StolenEggs)) as StolenEggs;

			if (e == GameLivingEvent.Interact)
			{
				if (quest != null && quest.grifflet == sender && quest.Step == 4)
				{
					SendSystemMessage(player, "The grifflet hums quitely.");

					quest.grifflet.MaxSpeedBase = player.MaxSpeedBase;
					quest.grifflet.Follow(player, 30, 2000);
					quest.Step = 5;
					return;
				}
			}
		}

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToNjiedi(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(dalikor.CanGiveQuest(typeof (StolenEggs), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			StolenEggs quest = player.IsDoingQuest(typeof (StolenEggs)) as StolenEggs;

			njiedi.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					if (quest.Step == 5)
					{
						njiedi.SayTo(player, "Oh thank you Eeinken for bringing back the grifflet. I know his parents will be relieved he's returned safe and sound. How did you manage to get him back? Did the Askefruer use anything? A chain? A bridle?");

						if (quest.grifflet != null)
						{
							quest.grifflet.StopFollow();
							quest.deleteGrifflet();
						}
					}
					else if (quest.Step == 6)
					{
						njiedi.SayTo(player, "I know this isn't much, but I used to do a bit of adventuring in my time. I'm sure you'll be able to use this. Now, I think you should return to Master Frederick and let him or her know what's going on.");
						if (player.HasAbilityToUseItem(recruitsVest))
							GiveItem(njiedi, player, recruitsVest);
						else
							GiveItem(njiedi, player, recruitsQuiltedVest);
						quest.Step = 7;
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
					switch (wArgs.Text)
					{
						case "appreciation":
							SendReply(player, "I know this isn't much, but I used to do a bit of adventuring in my time. I'm sure you'll be able to use this. Now, I think you should return to Dalikor and let him know what's going on.");
							if (quest.Step == 6)
							{
								if (player.HasAbilityToUseItem(recruitsVest))
									GiveItem(njiedi, player, recruitsVest);
								else
									GiveItem(njiedi, player, recruitsQuiltedVest);
								quest.Step = 7;
							}
							break;
					}
				}
			}
		}

		protected static void PlayerEnterWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			StolenEggs quest = player.IsDoingQuest(typeof (StolenEggs)) as StolenEggs;
			if (quest != null)
			{
				GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
				if (quest.Step == 2)
				{
					quest.initGrifflet();
					quest.grifflet.AddToWorld();
				}
			}
		}

		protected static void PlayerLeftWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			GameEventMgr.RemoveHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
			StolenEggs quest = player.IsDoingQuest(typeof (StolenEggs)) as StolenEggs;
			if (quest != null)
			{
				quest.deleteGrifflet();
			}
		}

		/// <summary>
		/// This method checks if a player qualifies for this quest
		/// </summary>
		/// <returns>true if qualified, false if not</returns>
		public override bool CheckQuestQualification(GamePlayer player)
		{
			// if the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(typeof (StolenEggs)) != null)
				return true;

			// This checks below are only performed is player isn't doing quest already
			if (!CheckPartAccessible(player, typeof (StolenEggs)))
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
			StolenEggs quest = player.IsDoingQuest(typeof (StolenEggs)) as StolenEggs;

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
			if(dalikor.CanGiveQuest(typeof (StolenEggs), player)  <= 0)
				return;

			StolenEggs quest = player.IsDoingQuest(typeof (StolenEggs)) as StolenEggs;

			if (quest != null)
				return;

			if (response == 0x00)
			{
				SendReply(player, "Oh well, if you change your mind, please come back!");
			}
			else
			{
				//Check if we can add the quest!
				if (!dalikor.GiveQuest(typeof (StolenEggs), player, 1))
					return;

				GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));

				SendReply(player, "Thank you Eeinken. I want you to speak with Viking Hyndla near the Jordheim gates. Several passersby have reported seeing strange creatures on the sides of the roads as they traveled here. Ask her if she can give you a specific location. Thank you again Eeinken.");
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
						return "[Step #1] Find Viking Hyndla near the gates of Jordheim. Ask her if she has seen or heard of any suspicious [Askefruer] behavior.";
					case 2:
						return "[Step #2] Head towards Haggerfel. Travel north-northeast down the road from Haggerfel until you see two tree stumps. Look for the four tree groves. That is where the creature is hiding.";
					case 3:
						return "[Step #3] Defeat the Askefruer trainer!";
					case 4:
						return "[Step #4] You may now interact with the grifflet. To interact with him, right click on him.";
					case 5:
						return "[Step #5] Take the grifflet back to Griffin Handler Njiedi near the gates of Jordheim. Hand him the whip when he asks. If the grifflet gets lost, proceed to Griffin Handler Njiedi. The grifflet will find his way home.";
					case 6:
						return "[Step #6] Speak with Griffin Handler Njiedi.";
					case 7:
						return "[Step #7] Return to Dalikor at the guard tower near Mularn. Tell him you [successfully] returned the grifflet to Njiedi.";
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player==null || player.IsDoingQuest(typeof (StolenEggs)) == null)
				return;

			if (Step == 3 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;

				if (gArgs.Target == askefruerTrainer)
				{
					SendSystemMessage("You slay the creature and pluck a whip from the Askefruer trainer's hands.");
					GiveItem(gArgs.Target, player, trainerWhip);
					Step = 4;
					return;
				}
			}

			if (Step == 5 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == njiedi.Name && gArgs.Item.Id_nb == trainerWhip.Id_nb)
				{
					njiedi.SayTo(player, "A whip?! This is outrageous! I see they were just trying to torture him. These Askefruer are truly malicious creatures. I hope you wipe them out one day Eeinken. Here, take this as a sign of my [appreciation] for the return of the little one.");
					RemoveItem(njiedi, player, trainerWhip);
					Step = 6;
					return;
				}
			}

		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, trainerWhip, false);
			RemoveItem(m_questPlayer, dustyOldMap, false);

			if (m_questPlayer.HasAbilityToUseItem(recruitsVest))
				RemoveItem(m_questPlayer, recruitsVest, false);
			else
				RemoveItem(m_questPlayer, recruitsQuiltedVest, false);

			deleteGrifflet();

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
		}


		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			//Give reward to player here ...              
			m_questPlayer.GainExperience(507, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 7, Util.Random(50)), "You recieve {0} as a reward.");

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
		}

	}
}
