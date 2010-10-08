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
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.Language;
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

        protected static string questTitle = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.QuestTitle");

		protected const int minimumLevel = 4;
		protected const int maximumLevel = 4;

		private static GameNPC dalikor = null;
		private static GameStableMaster njiedi = null;
		private static GameNPC hyndla = null;

		private static GameNPC askefruerTrainer = null;
		private GameNPC grifflet = null;

		private bool askefruerGriffinHandlerAttackStarted = false;

		private static ItemTemplate trainerWhip = null;
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

            GameNPC[] npcs = WorldMgr.GetNPCsByName(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.NPCVikingHyndla"), eRealm.Midgard);
            if (npcs.Length == 0)
			{
				hyndla = new GameNPC();
				hyndla.Model = 9;
                hyndla.Name = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.NPCVikingHyndla");
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

            npcs = (GameNPC[])WorldMgr.GetObjectsByName(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.NPCGriffinHandlerNjiedi"), eRealm.Midgard, typeof(GameStableMaster));

			if (npcs.Length == 0)
			{
				njiedi = new GameStableMaster();
				njiedi.Model = 158;
                njiedi.Name = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.NPCGriffinHandlerNjiedi");
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

//			npcs = WorldMgr.GetNPCsByName("Askefruer Trainer", eRealm.None);
            npcs = WorldMgr.GetNPCsByName(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.NPCAskefruerTrainer"), eRealm.None);
            if (npcs.Length == 0)
			{
				askefruerTrainer = new GameNPC();

                askefruerTrainer.Name = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.NPCAskefruerTrainer");
                askefruerTrainer.X = GameLocation.ConvertLocalXToGlobalX(54739, 100);
				askefruerTrainer.Y = GameLocation.ConvertLocalYToGlobalY(18264, 100);
				askefruerTrainer.Z = 5195;
				askefruerTrainer.Heading = 79;
				askefruerTrainer.Model = 678;
				//askefruerTrainer.GuildName = "Part of " + questTitle + " Quest";
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

			trainerWhip = GameServer.Database.FindObjectByKey<ItemTemplate>("askefruer_whip");
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

				GameServer.Database.AddObject(trainerWhip);
			}

			// item db check
			recruitsVest = GameServer.Database.FindObjectByKey<ItemTemplate>("recruits_studded_vest_mid");
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
				recruitsVest.Price = Money.GetMoney(0,0,0,9,0);
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

				GameServer.Database.AddObject(recruitsVest);
			}

			// item db check
			recruitsQuiltedVest = GameServer.Database.FindObjectByKey<ItemTemplate>("recruits_quilted_vest");
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
				recruitsQuiltedVest.Price = Money.GetMoney(0,0,0,9,0);
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

				GameServer.Database.AddObject(recruitsQuiltedVest);
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
                    hyndla.SayTo(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.TalkToHyndla.Talk1", player.CharacterClass.Name));
                    hyndla.SayTo(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.TalkToHyndla.Talk2"));
                }
			}
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				if (quest != null)
				{
                    if (wArgs.Text == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.TalkToHyndla.Whisper1"))
                    {
                        hyndla.SayTo(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.TalkToHyndla.Talk3"));
                    }
                    else if (wArgs.Text == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.TalkToHyndla.Whisper2"))
                    {
                        hyndla.SayTo(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.TalkToHyndla.Talk4", player.Name));
                        if (quest.Step == 1)
                        {
                            quest.Step = 2;
                            quest.initGrifflet();
                            quest.grifflet.AddToWorld();
                        }
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

                    IOldAggressiveBrain aggroBrain = m_askefruerTrainer.Brain as IOldAggressiveBrain;
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
            grifflet.Name = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.InitGrifflet.NPCGrifflet");
            //grifflet.GuildName = "Part of " + m_questPlayer.GetName(0, false) + "'s " + questTitle + " Quest";
			grifflet.Flags ^= GameNPC.eFlags.PEACE;
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
            //grifflet.SaveIntoDatabase();                            

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
                    dalikor.SayTo(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.TalkToDalikor.Talk1"));
                    return;
				}
				else
				{
					if (quest.Step == 7)
					{
                        dalikor.SayTo(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.TalkToDalikor.Talk2"));
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
                    if (wArgs.Text == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.TalkToDalikor.Whisper1"))
                    {
                        dalikor.SayTo(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.TalkToDalikor.Talk3"));
                    }
                    else if (wArgs.Text == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.TalkToDalikor.Whisper2"))
                    {
                        dalikor.SayTo(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.TalkToDalikor.Talk4"));
                    }
                    else if (wArgs.Text == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.TalkToDalikor.Whisper3"))
                    {
                        dalikor.SayTo(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.TalkToDalikor.Talk5"));
                    }
                    else if (wArgs.Text == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.TalkToDalikor.Whisper4"))
                    {
                        dalikor.SayTo(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.TalkToDalikor.Talk6"));
                    }
                    else if (wArgs.Text == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.TalkToDalikor.Whisper5"))
                    {
                        dalikor.SayTo(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.TalkToDalikor.Talk7"));
                    }
                    else if (wArgs.Text == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.TalkToDalikor.Whisper6"))
                    {
                        //If the player offered his "help", we send the quest dialog now!
                        player.Out.SendQuestSubscribeCommand(dalikor, QuestMgr.GetIDForQuestType(typeof(StolenEggs)), LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.TalkToDalikor.OfferQuest"));
                    }
				}
				else
				{
                    if (wArgs.Text == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.TalkToDalikor.Whisper7"))
                    {
						dalikor.SayTo(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.TalkToDalikor.Talk8"));
                    }
                    else if (wArgs.Text == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.TalkToDalikor.Whisper8"))
                    {
						dalikor.SayTo(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.TalkToDalikor.Talk9"));
						if (quest.Step == 7)
						{
							quest.FinishQuest();
						}
                    }
                    else if (wArgs.Text == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.TalkToDalikor.Whisper9"))
                    {
						player.Out.SendCustomDialog(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.TalkToDalikor.AbortQuest"), new CustomDialogResponse(CheckPlayerAbortQuest));
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
                    SendSystemMessage(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.TalkToGrifflet.Talk1"));
					quest.grifflet.MaxSpeedBase = player.MaxSpeedBase;
                    quest.grifflet.Realm = eRealm.Midgard;
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
                        njiedi.SayTo(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.TalkToNjiedi.Talk1", player.Name));

						if (quest.grifflet != null)
						{
							quest.grifflet.StopFollowing();
							quest.deleteGrifflet();
						}
					}
					else if (quest.Step == 6)
					{
                        njiedi.SayTo(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.TalkToNjiedi.Talk2"));
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
                    if (wArgs.Text == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.TalkToNjiedi.Whisper1"))
                    {
                        njiedi.SayTo(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.TalkToNjiedi.Talk2"));
                        if (quest.Step == 6)
                        {
                            if (player.HasAbilityToUseItem(recruitsVest))
                                GiveItem(njiedi, player, recruitsVest);
                            else
                                GiveItem(njiedi, player, recruitsQuiltedVest);
                            quest.Step = 7;
                        }
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
                SendSystemMessage(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.CheckPlayerAbortQuest.Text1"));
            }
			else
			{
                SendSystemMessage(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.CheckPlayerAbortQuest.Text2", questTitle));
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
                dalikor.SayTo(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.CheckPlayerAcceptQuest.Text1"));
            }
			else
			{
				//Check if we can add the quest!
				if (!dalikor.GiveQuest(typeof (StolenEggs), player, 1))
					return;

                GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
                dalikor.SayTo(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.CheckPlayerAcceptQuest.Text2", player.Name));
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
                        return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.Descriptiont.Text1");
                    case 2:
                        return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.Descriptiont.Text2");
                    case 3:
                        return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.Descriptiont.Text3");
                    case 4:
                        return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.Descriptiont.Text4");
                    case 5:
                        return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.Descriptiont.Text5");
                    case 6:
                        return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.Descriptiont.Text6");
                    case 7:
                        return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.Descriptiont.Text7");
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
                    SendSystemMessage(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.Notify.Text1", askefruerTrainer.GetName(0, true)));
                    SendSystemMessage(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.Notify.Text2"));
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
                    njiedi.SayTo(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.Notify.Text3", player.Name));
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
			m_questPlayer.GainExperience(GameLiving.eXPSource.Quest, 507, true);
            m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 7, Util.Random(50)), LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.StolenEggs.FinishQuest.Text1"));

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
		}
	}
}
