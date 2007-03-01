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
 * 2) Go speak with Haruld at loc=23573,21096 Camelot Hills and type /whisper fairy. 
 * 3) Go into the stable and talk with Nob the Stableboy, loc= loc=24155,21157 Camelot Hills, and type /whisper Ire Fairies. 
 * 4) Run to loc=26470,23075 Camelot Hills. You will get a pop-up box telling you "This looks to be the Ire Fairies Grove." A Fairy Dragonfly Handler will appear along with a dragonfly hatchling, both will attack you, though the dragonfly hatchling will do you no harm damage wise. The Fairy Dragonfly Handler con'd blue to a level 3. Kill the Fairy Dragonfly Handler. 
 * 5) Right click on the Dragonfly Hatchling and run him back to Dragonfly Handler Colm, loc=14040,29170 Camelot Hills (Cotswold), hand Colm the Ire Dragonfly Whip for an item reward. 
 * 6) Return
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

	public class IreFairyIre : BaseFrederickQuest
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

		protected const string questTitle = "Ire Fairy Ire";
		protected const int minimumLevel = 4;
		protected const int maximumLevel = 4;

		private static GameNPC masterFrederick = null;
		private static GameStableMaster colm = null;
		private static GameNPC nob = null;
		private static GameNPC haruld = null;

		private static GameNPC fairyDragonflyHandler = null;
		private GameNPC dragonflyHatchling = null;

		private bool fairyDragonflyHandlerAttackStarted = false;

		private static ItemTemplate dragonflyWhip = null;
		private static ItemTemplate dustyOldMap = null;
		private static ItemTemplate recruitsVest = null;
		private static ItemTemplate recruitsQuiltedVest = null;


		/* We need to define the constructors from the base class here, else there might be problems
		 * when loading this quest...
		 */
		public IreFairyIre() : base()
		{
		}

		public IreFairyIre(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public IreFairyIre(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public IreFairyIre(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Nob the Stableboy", eRealm.Albion);
			if (npcs.Length == 0)
			{
				nob = new GameNPC();
				nob.Model = 9;
				nob.Name = "Nob the Stableboy";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + nob.Name + ", creating him ...");
				nob.GuildName = "Part of " + questTitle + " Quest";
				nob.Realm = (byte) eRealm.Albion;
				nob.CurrentRegionID = 1;

				nob.Size = 45;
				nob.Level = 4;
				nob.X = 573019;
				nob.Y = 504485;
				nob.Z = 2199;
				nob.Heading = 10;

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					nob.SaveIntoDatabase();

				nob.AddToWorld();
			}
			else
				nob = npcs[0];

			npcs = (GameNPC[]) WorldMgr.GetObjectsByName("Dragonfly Handler Colm", eRealm.Albion, typeof (GameStableMaster));
			if (npcs.Length == 0)
			{
				colm = new GameStableMaster();
				colm.Model = 78;
				colm.Name = "Dragonfly Handler Colm";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + colm.Name + ", creating ...");
				colm.GuildName = "Stable Master";
				colm.Realm = (byte) eRealm.Albion;
				colm.CurrentRegionID = 1;
				colm.Size = 51;
				colm.Level = 50;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 81, 10);
				template.AddNPCEquipment(eInventorySlot.LegsArmor, 82, 10);
				template.AddNPCEquipment(eInventorySlot.FeetArmor, 84, 10);
				template.AddNPCEquipment(eInventorySlot.Cloak, 57, 32);
				colm.Inventory = template.CloseTemplate();

//				colm.AddNPCEquipment(Slot.TORSO, 81, 10, 0, 0);
//				colm.AddNPCEquipment(Slot.LEGS, 82, 10, 0, 0);
//				colm.AddNPCEquipment(Slot.FEET, 84, 10, 0, 0);
//				colm.AddNPCEquipment(Slot.CLOAK, 57, 32, 0, 0);

				colm.X = 562775;
				colm.Y = 512453;
				colm.Z = 2438;
				colm.Heading = 158;
				colm.MaxSpeedBase = 200;

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 0;
				colm.SetOwnBrain(brain);

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					colm.SaveIntoDatabase();
				colm.AddToWorld();
			}
			else
			{
				colm = npcs[0] as GameStableMaster;
			}

			npcs = WorldMgr.GetNPCsByName("Haruld", eRealm.Albion);
			if (npcs.Length == 0 || !(npcs[0] is GameStableMaster))
			{
				haruld = new GameStableMaster();
				haruld.Model = 9;
				haruld.Name = "Haruld";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + haruld.Name + ", creating ...");
				haruld.GuildName = "Stable Master";
				haruld.Realm = (byte) eRealm.Albion;
				haruld.CurrentRegionID = 1;
				haruld.Size = 49;
				haruld.Level = 4;

				haruld.X = 572479;
				haruld.Y = 504410;
				haruld.Z = 2184;
				haruld.Heading = 944;
				haruld.MaxSpeedBase = 100;

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 0;
				haruld.SetOwnBrain(brain);

				haruld.EquipmentTemplateID = "11701337";

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					haruld.SaveIntoDatabase();
				haruld.AddToWorld();
			}
			else
			{
				haruld = npcs[0] as GameStableMaster;
			}

			npcs = WorldMgr.GetNPCsByName("Fairy Dragonfly Handler", eRealm.None);
			if (npcs.Length == 0)
			{
				fairyDragonflyHandler = new GameNPC();
				fairyDragonflyHandler.Name = "Fairy Dragonfly Handler";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + fairyDragonflyHandler.Name + ", creating ...");
				fairyDragonflyHandler.X = 575334;
				fairyDragonflyHandler.Y = 506403;
				fairyDragonflyHandler.Z = 2331;
				fairyDragonflyHandler.Heading = 114;
				fairyDragonflyHandler.Model = 603;
				fairyDragonflyHandler.GuildName = "Part of " + questTitle + " Quest";
				fairyDragonflyHandler.Realm = (byte) eRealm.None;
				fairyDragonflyHandler.CurrentRegionID = 1;
				fairyDragonflyHandler.Size = 49;
				fairyDragonflyHandler.Level = 3;

				// Leave at default values to be one the save side ...
				//fairyDragonflyHandler.AggroLevel = 80;
				//fairyDragonflyHandler.AggroRange = 1000;

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					fairyDragonflyHandler.SaveIntoDatabase();

				fairyDragonflyHandler.AddToWorld();
			}
			else
				fairyDragonflyHandler = (GameNPC) npcs[0];

			#endregion

			#region defineItems

			dragonflyWhip = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "dragonfly_whip");
			if (dragonflyWhip == null)
			{
				dragonflyWhip = new ItemTemplate();
				dragonflyWhip.Name = "Dragonfly Whip";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + dragonflyWhip.Name + " , creating it ...");

				dragonflyWhip.Weight = 15;
				dragonflyWhip.Model = 859;

				dragonflyWhip.Object_Type = (int) eObjectType.GenericItem;

				dragonflyWhip.Id_nb = "dragonfly_whip";
				dragonflyWhip.IsPickable = true;
				dragonflyWhip.IsDropable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(dragonflyWhip);
			}

			// item db check
			recruitsVest = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "recruits_studded_vest");
			if (recruitsVest == null)
			{
				recruitsVest = new ItemTemplate();
				recruitsVest.Name = "Recruit's Studded Vest";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsVest.Name + ", creating it ...");
				recruitsVest.Level = 8;

				recruitsVest.Weight = 60;
				recruitsVest.Model = 81; // studded vest

				recruitsVest.DPS_AF = 12; // Armour
				recruitsVest.SPD_ABS = 19; // Absorption

				recruitsVest.Object_Type = (int) eObjectType.Studded;
				recruitsVest.Item_Type = (int) eEquipmentItems.TORSO;
				recruitsVest.Id_nb = "recruits_studded_vest";
				recruitsVest.Gold = 0;
				recruitsVest.Silver = 9;
				recruitsVest.Copper = 0;
				recruitsVest.IsPickable = true;
				recruitsVest.IsDropable = true;
				recruitsVest.Color = 9; // red leather

				recruitsVest.Bonus = 5; // default bonus

				recruitsVest.Bonus1 = 4;
				recruitsVest.Bonus1Type = (int) eStat.STR;

				recruitsVest.Bonus2 = 3;
				recruitsVest.Bonus2Type = (int) eStat.CON;

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
			recruitsQuiltedVest = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "recruits_quilted_vest");
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
				recruitsQuiltedVest.Color = 9; // red leather

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

			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			//We want to be notified whenever a player enters the world            
			GameEventMgr.AddHandler(GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));

			GameEventMgr.AddHandler(masterFrederick, GameLivingEvent.Interact, new DOLEventHandler(TalkToMasterFrederick));
			GameEventMgr.AddHandler(masterFrederick, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterFrederick));

			GameEventMgr.AddHandler(colm, GameLivingEvent.Interact, new DOLEventHandler(TalkToColm));
			GameEventMgr.AddHandler(colm, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToColm));

			GameEventMgr.AddHandler(nob, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToNob));

			GameEventMgr.AddHandler(haruld, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToHaruld));

			GameEventMgr.AddHandler(fairyDragonflyHandler, GameNPCEvent.OnAICallback, new DOLEventHandler(CheckNearFairyDragonflyHandler));

			/* Now we bring to masterFrederick the possibility to give this quest to players */
			masterFrederick.AddQuestToGive(typeof (IreFairyIre));

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

			GameEventMgr.RemoveHandler(GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));

			GameEventMgr.RemoveHandler(masterFrederick, GameLivingEvent.Interact, new DOLEventHandler(TalkToMasterFrederick));
			GameEventMgr.RemoveHandler(masterFrederick, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterFrederick));

			GameEventMgr.RemoveHandler(colm, GameLivingEvent.Interact, new DOLEventHandler(TalkToColm));
			GameEventMgr.RemoveHandler(colm, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToColm));

			GameEventMgr.RemoveHandler(nob, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToNob));

			GameEventMgr.RemoveHandler(haruld, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToHaruld));

			GameEventMgr.RemoveHandler(fairyDragonflyHandler, GameNPCEvent.OnAICallback, new DOLEventHandler(CheckNearFairyDragonflyHandler));
			
			/* Now we remove to masterFrederick the possibility to give this quest to players */
			masterFrederick.RemoveQuestToGive(typeof (IreFairyIre));
		}

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToNob(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(masterFrederick.CanGiveQuest(typeof (IreFairyIre), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			IreFairyIre quest = player.IsDoingQuest(typeof (IreFairyIre)) as IreFairyIre;
			nob.TurnTo(player);
			if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				if (quest != null)
				{
					switch (wArgs.Text)
					{
						case "fairy":
							nob.SayTo(player, "Ire fairies? Hrm...Now that you mention it, I do remember seeing some fairies heading off to a small grove behind the stable last night. There were a solid handful of them. They were carrying [something], I don't know what it was.");
							break;
						case "something":
							nob.SayTo(player, "It looked heavy for them, whatever it was. Just head southeast from here and you'll find the grove. I hope you find what you're looking for.");
							if (quest.Step == 2)
							{
								quest.Step = 3;
								quest.initDragonflyHatchling();
								quest.dragonflyHatchling.AddToWorld();
							}
							break;
					}
				}

			}
		}

		protected static void CheckNearFairyDragonflyHandler(DOLEvent e, object sender, EventArgs args)
		{
			GameNPC fairyDragonflyHandler = (GameNPC) sender;

			// if princess is dead no ned to checks ...
			if (!fairyDragonflyHandler.IsAlive || fairyDragonflyHandler.ObjectState != GameObject.eObjectState.Active)
				return;

			foreach (GamePlayer player in fairyDragonflyHandler.GetPlayersInRadius(1500))
			{
				IreFairyIre quest = (IreFairyIre) player.IsDoingQuest(typeof (IreFairyIre));

				if (quest != null && !quest.fairyDragonflyHandlerAttackStarted && quest.Step == 3)
				{
					quest.fairyDragonflyHandlerAttackStarted = true;

					SendSystemMessage(player, "Fairy Dragonfly Handler says, \"No! I have been betrayed by loathsome insects! How can this be?\"");
					IAggressiveBrain aggroBrain = fairyDragonflyHandler.Brain as IAggressiveBrain;
					if (aggroBrain != null)
						aggroBrain.AddToAggroList(player, 70);

					// if we find player doing quest stop looking for further ones ...
					break;
				}
			}
		}

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToHaruld(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(masterFrederick.CanGiveQuest(typeof (IreFairyIre), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			IreFairyIre quest = player.IsDoingQuest(typeof (IreFairyIre)) as IreFairyIre;

			if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				if (quest != null && quest.Step == 1)
				{
					haruld.TurnTo(player);
					switch (wArgs.Text)
					{
						case "fairy":
							haruld.SayTo(player, "Fairies? Bah, I don't care one lick about those creatures. Why don't you go speak to Nob in the stable there? He knows all about those nasty things.");
							quest.Step = 2;
							break;
					}
				}

			}
		}

		protected void deleteDragonflyHatchling()
		{
			if (dragonflyHatchling != null)
			{
				GameEventMgr.RemoveHandler(dragonflyHatchling, GameLivingEvent.Interact, new DOLEventHandler(TalkToDragonflyHatchling));
				dragonflyHatchling.Delete();
				dragonflyHatchling = null;
			}
		}

		protected void initDragonflyHatchling()
		{
			dragonflyHatchling = new GameNPC();

			dragonflyHatchling.Model = 819;
			dragonflyHatchling.Name = "Dragonfly Hatchling";
			dragonflyHatchling.GuildName = "Part of " + questTitle + " Quest";
			dragonflyHatchling.Flags ^= (uint)GameNPC.eFlags.PEACE;
			dragonflyHatchling.CurrentRegionID = 1;
			dragonflyHatchling.Size = 25;
			dragonflyHatchling.Level = 3;
			dragonflyHatchling.X = fairyDragonflyHandler.X + Util.Random(-150, 150);
			dragonflyHatchling.Y = fairyDragonflyHandler.Y + Util.Random(-150, 150);
			dragonflyHatchling.Z = fairyDragonflyHandler.Z;
			dragonflyHatchling.Heading = 93;
			dragonflyHatchling.MaxSpeedBase = 200;

			StandardMobBrain brain = new StandardMobBrain();
			brain.AggroLevel = 0;
			brain.AggroRange = 0;
			dragonflyHatchling.SetOwnBrain(brain);

			//You don't have to store the created mob in the db if you don't want,
			//it will be recreated each time it is not found, just comment the following
			//line if you rather not modify your database
			//dragonflyHatchling.SaveIntoDatabase();                            

			GameEventMgr.AddHandler(dragonflyHatchling, GameLivingEvent.Interact, new DOLEventHandler(TalkToDragonflyHatchling));

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

			if(masterFrederick.CanGiveQuest(typeof (IreFairyIre), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			IreFairyIre quest = player.IsDoingQuest(typeof (IreFairyIre)) as IreFairyIre;

			masterFrederick.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				//We check if the player is already doing the quest                
				if (quest == null)
				{
					//Player is not doing the quest...
					masterFrederick.SayTo(player, "Recruit Vinde, these fairies are becoming a serious problem for Cotswold. They have been harassing the citizens here! Can you believe that? They are retaliating for us getting to their generals. They must be [stopped]!");
					return;
				}
				else
				{
					if (quest.Step == 7)
					{
						masterFrederick.SayTo(player, "You've returned Vinde. Did you have any success in locating the [egg for Colm]?");
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
						case "stopped":
							masterFrederick.SayTo(player, "Colm, the dragonfly handler, told me the other day one of his dragonfly eggs had gone missing. He suspected the Ire Fairies took it. Since there were no footprints, I'm inclined to [believe] him.");
							break;
						case "believe":
							masterFrederick.SayTo(player, "I want you to find where this egg has gone, bring it back and destroy the fairies who took it. We can't have them taking our dragonflies and torturing them for their own evil designs! What do you say? Are you [ready]?");
							break;

							//If the player offered his "help", we send the quest dialog now!
						case "ready":
							player.Out.SendQuestSubscribeCommand(masterFrederick, QuestMgr.GetIDForQuestType(typeof(IreFairyIre)), "Will you go out and find where these eggs went?");
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "egg for Colm":
							masterFrederick.SayTo(player, "Ah, so the little thing hatched did it? Well, no matter. Excellent job recruit! For that, you shall be rewarded! Here, take [this].");
							break;

						case "this":
							masterFrederick.SayTo(player, "I know you have done a great service for Colm, and I am happy for you for your continued selfless acts. Go now Vinde. We will speak again soon.");
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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(IreFairyIre)))
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

		protected static void TalkToDragonflyHatchling(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(masterFrederick.CanGiveQuest(typeof (IreFairyIre), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			IreFairyIre quest = player.IsDoingQuest(typeof (IreFairyIre)) as IreFairyIre;

			if (e == GameLivingEvent.Interact)
			{
				if (quest != null && quest.dragonflyHatchling == sender && quest.Step == 4)
				{
					SendSystemMessage(player, "The dragonfly hatchling hums quitely.");

					quest.dragonflyHatchling.MaxSpeedBase = player.MaxSpeedBase;
					quest.dragonflyHatchling.Follow(player, 30, 2000);
					quest.Step = 5;
					return;
				}
			}
		}

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToColm(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(masterFrederick.CanGiveQuest(typeof (IreFairyIre), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			IreFairyIre quest = player.IsDoingQuest(typeof (IreFairyIre)) as IreFairyIre;

			colm.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					if (quest.Step == 5)
					{
						colm.SayTo(player, "Oh thank you Vinde for bringing back the baby. I know his parents will be relieved he's returned safe and sound. How did you manage to get him back? Did it use anything? A chain? A bridle?");

						if (quest.dragonflyHatchling != null)
						{
							quest.dragonflyHatchling.StopFollow();
							quest.deleteDragonflyHatchling();
						}
					}
					else if (quest.Step == 6)
					{
						colm.SayTo(player, "I know this isn't much, but I used to do a bit of adventuring in my time. I'm sure you'll be able to use this. Now, I think you should return to Master Frederick and let him or her know what's going on.");

						if (player.HasAbilityToUseItem(recruitsVest))
							GiveItem(colm, player, recruitsVest);
						else
							GiveItem(colm, player, recruitsQuiltedVest);

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
							colm.SayTo(player, "I know this isn't much, but I used to do a bit of adventuring in my time. I'm sure you'll be able to use this. Now, I think you should return to your trainer and let him or her know what's going on.");
							if (quest.Step == 6)
							{
								if (player.HasAbilityToUseItem(recruitsVest))
									GiveItem(colm, player, recruitsVest);
								else
									GiveItem(colm, player, recruitsQuiltedVest);
								quest.Step = 7;
							}
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

			IreFairyIre quest = player.IsDoingQuest(typeof (IreFairyIre)) as IreFairyIre;
			if (quest != null)
			{
				quest.deleteDragonflyHatchling();
			}
		}

		/// <summary>
		/// This method checks if a player qualifies for this quest
		/// </summary>
		/// <returns>true if qualified, false if not</returns>
		public override bool CheckQuestQualification(GamePlayer player)
		{
			// if the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(typeof (IreFairyIre)) != null)
				return true;

			// This checks below are only performed is player isn't doing quest already			

			if (!CheckPartAccessible(player, typeof (IreFairyIre)))
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
			IreFairyIre quest = player.IsDoingQuest(typeof (IreFairyIre)) as IreFairyIre;

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
			if(masterFrederick.CanGiveQuest(typeof (IreFairyIre), player)  <= 0)
				return;

			IreFairyIre quest = player.IsDoingQuest(typeof (IreFairyIre)) as IreFairyIre;

			if (quest != null)
				return;

			if (response == 0x00)
			{
				SendReply(player, "Oh well, if you change your mind, please come back!");
			}
			else
			{
				//Check if we can add the quest!
				if (!masterFrederick.GiveQuest(typeof (IreFairyIre), player, 1))
					return;

				masterFrederick.SayTo(player, "Alright recruit, we have very few leads in regards to this egg napping, but I am of the opinion someone saw something. Haruld at the stable across the way may have seen something. There are a lot of fairies that reside near there. Go speak with him.");
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
						return "[Step #1] Find Haruld the stable master at the stables east of Cotswold. Ask him if he has seen or heard of any suspicious [fairy] behavior. To ask him a question, type \"/whisper fairy\".";
					case 2:
						return "[Step #2] Haruld has told you to go inside and speak with Nob. Ask him if he has seen or heard any [Ire Fairies]. To ask Nob a question, type \"/whisper fairy\".";
					case 3:
						return "[Step #3] Nob has told you to head southeast from the stable towards the small grove of trees. Be careful, there may be Ire Fairies around.";
					case 4:
						return "[Step #4] You must now interact with the hatchling. See if it will follow you back to Colm in Cotswold. To interact with the hatchling, right click on it.";
					case 5:
						return "[Step #5] Take the Hatchling to Colm in Cotswold. If he stops following you, don't worry. He can now find his way home.";
					case 6:
						return "[Step #6] Ask Colm if he has something to show his [appreciation] for your efforts.";
					case 7:
						return "[Step #7] Return to Master Frederick at the guard tower in Cotswold. Tell him that you have returned the hatchling to Colm.";
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player==null || player.IsDoingQuest(typeof (IreFairyIre)) == null)
				return;

			if (Step == 3 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;

				if (gArgs.Target == fairyDragonflyHandler)
				{
					SendSystemMessage("You slay the creature and pluck a whip from the Ire Fairy trainer's hands.");
					GiveItem(gArgs.Target, player, dragonflyWhip);
					Step = 4;
					return;
				}
			}


			if (Step == 5 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == colm.Name && gArgs.Item.Id_nb == dragonflyWhip.Id_nb)
				{
					colm.SayTo(player, "A whip?! This is outrageous! I see they were just trying to torture him. These Ire Fairies are truly malicious creatures. I hope you wipe them out one day Vinde. Here, take this as a sign of my [appreciation] for the return of the little one.");
					RemoveItem(colm, player, dragonflyWhip);
					Step = 6;
					return;
				}
			}

		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, dragonflyWhip, false);
			RemoveItem(m_questPlayer, dustyOldMap, false);

			if (m_questPlayer.HasAbilityToUseItem(recruitsVest))
				RemoveItem(m_questPlayer, recruitsVest, false);
			else
				RemoveItem(m_questPlayer, recruitsQuiltedVest, false);

			deleteDragonflyHatchling();
		}


		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			//Give reward to player here ...              
			m_questPlayer.GainExperience(507, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 7, Util.Random(50)), "You recieve {0} as a reward.");
		}

	}
}
