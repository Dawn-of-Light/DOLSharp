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
using DOL.AI;
using DOL.AI.Brain;
using DOL.GS.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using NHibernate.Expression;
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
	/* The first thing we do, is to declare the quest requirement
	* class linked with the new Quest. To do this, we derive 
	* from the abstract class AbstractQuestDescriptor
	*/
	public class IreFairyIreDescriptor : AbstractQuestDescriptor
	{
		/* This is the type of the quest class linked with 
		 * this requirement class, you must override the 
		 * base methid like that
		 */
		public override Type LinkedQuestType
		{
			get { return typeof(IreFairyIre); }
		}

		/* This value is used to retrieves the minimum level needed
		 *  to be able to make this quest. Override it only if you need, 
		 * the default value is 1
		 */
		public override int MinLevel
		{
			get { return 4; }
		}

		/* This value is used to retrieves how maximum level needed
		 * to be able to make this quest. Override it only if you need, 
		 * the default value is 50
		 */
		public override int MaxLevel
		{
			get { return 4; }
		}

		public override bool CheckQuestQualification(GamePlayer player)
		{
			// if the player is already doing the quest always return true !!!
			if (player.IsDoingQuest(LinkedQuestType) != null)
				return true;

			// This checks below are only performed is player isn't doing quest already	
			if (!BaseFrederickQuest.CheckPartAccessible(player, typeof(IreFairyIre)))
				return false;

			return base.CheckQuestQualification(player);
		}
	}

	/* The second thing we do, is to declare the class we create
	 * as Quest. We must make it persistant using attributes, to
	 * do this, we derive from the abstract class AbstractQuest
	 */
	[NHibernate.Mapping.Attributes.Subclass(NameType = typeof(IreFairyIre), ExtendsType = typeof(AbstractQuest))]
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

		private static GameMob masterFrederick = null;
		private static GameStableMaster colm = null;
		private static GameMob nob = null;
		private static GameMob haruld = null;

		private static GameMob fairyDragonflyHandler = null;
		private GameMob dragonflyHatchling = null;

		private static GenericItemTemplate dragonflyWhip = null;
		private static TorsoArmorTemplate recruitsVest = null;
		private static TorsoArmorTemplate recruitsQuiltedVest = null;

		private static Circle fairyDragonflyHandlerArea = null;
		
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
			if(masterFrederick == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
				return;
			}

			nob = ResearchQuestObject(typeof(GameMob), WorldMgr.GetRegion(1), eRealm.Albion, "Nob the Stableboy") as GameMob;
			if (nob == null)
			{
				nob = new GameMob();
				nob.Model = 9;
				nob.Name = "Nob the Stableboy";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + nob.Name + ", creating him ...");
				nob.GuildName = "Part of " + questTitle + " Quest";
				nob.Realm = (byte) eRealm.Albion;
				nob.Region = WorldMgr.GetRegion(1);

				nob.Size = 45;
				nob.Level = 4;
				nob.Position = new Point(573019, 504485, 2199);
				nob.Heading = 10;

				StandardMobBrain newBrain = new StandardMobBrain();
				newBrain.Body = nob;
				newBrain.AggroLevel = 100;
				newBrain.AggroRange = 0;
				nob.OwnBrain = newBrain;

				if(!nob.AddToWorld())
				{
					if (log.IsWarnEnabled)
						log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
					return;
				}

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(nob);
			}

			colm = ResearchQuestObject(typeof(GameStableMaster), WorldMgr.GetRegion(1), eRealm.Albion, "Dragonfly Handler Colm") as GameStableMaster;
			if (colm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Dragonfly Handler Colm, creating ...");
				colm = new GameStableMaster();
				colm.Model = 78;
				colm.Name = "Dragonfly Handler Colm";
				colm.GuildName = "Stable Master";
				colm.Realm = (byte) eRealm.Albion;
				colm.Region = WorldMgr.GetRegion(1);

				GameNpcInventory template = new GameNpcInventory();
				template.AddItem(eInventorySlot.TorsoArmor, new NPCArmor(81, 10, 0));
				template.AddItem(eInventorySlot.LegsArmor, new NPCArmor(82, 10, 0));
				template.AddItem(eInventorySlot.FeetArmor, new NPCArmor(84, 10, 0));
				template.AddItem(eInventorySlot.Cloak, new NPCEquipment(57, 32));
				colm.Inventory = template;

				colm.Size = 51;
				colm.Level = 50;
				colm.Position = new Point(562775, 512453, 2438);
				colm.Heading = 158;
				colm.MaxSpeedBase = 200;

				StandardMobBrain brain = new StandardMobBrain();
				brain.Body = colm;
				brain.AggroLevel = 0;
				brain.AggroRange = 0;
				colm.OwnBrain = brain;

				if(!colm.AddToWorld())
				{
					if (log.IsWarnEnabled)
						log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
					return;
				}

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database

				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(colm);
			}

			haruld = ResearchQuestObject(typeof(GameStableMaster), WorldMgr.GetRegion(1), eRealm.Albion, "Haruld") as GameStableMaster;
			if (haruld == null)
			{
				haruld = new GameStableMaster();
				haruld.Model = 9;
				haruld.Name = "Haruld";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + haruld.Name + ", creating ...");
				haruld.GuildName = "Stable Master";
				haruld.Realm = (byte) eRealm.Albion;
				haruld.Region = WorldMgr.GetRegion(1);
				haruld.Size = 49;
				haruld.Level = 4;

				haruld.Position = new Point(572479, 504410, 2184);
				haruld.Heading = 944;
				haruld.MaxSpeedBase = 100;

				StandardMobBrain brain = new StandardMobBrain();
				brain.Body = haruld;
				brain.AggroLevel = 0;
				brain.AggroRange = 0;
				haruld.OwnBrain = brain;

				if(!haruld.AddToWorld())
				{
					if (log.IsWarnEnabled)
						log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
					return;
				}

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database

				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(haruld);
			}

			fairyDragonflyHandler = ResearchQuestObject(typeof(GameMob), WorldMgr.GetRegion(1), eRealm.None, "Fairy Dragonfly Handler") as GameMob;
			if (fairyDragonflyHandler == null)
			{
				fairyDragonflyHandler = new GameMob();
				fairyDragonflyHandler.Name = "Fairy Dragonfly Handler";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + fairyDragonflyHandler.Name + ", creating ...");
				fairyDragonflyHandler.Position = new Point(575334, 506403, 2331);
				fairyDragonflyHandler.Heading = 114;
				fairyDragonflyHandler.Model = 603;
				fairyDragonflyHandler.GuildName = "Part of " + questTitle + " Quest";
				fairyDragonflyHandler.Realm = (byte) eRealm.None;
				fairyDragonflyHandler.Region = WorldMgr.GetRegion(1);

				fairyDragonflyHandler.Size = 49;
				fairyDragonflyHandler.Level = 3;

				fairyDragonflyHandler.RespawnInterval = -1; // autorespawn

				StandardMobBrain brain = new StandardMobBrain();
				brain.Body = fairyDragonflyHandler;
				brain.AggroLevel = 80;
				brain.AggroRange = 1000;
				fairyDragonflyHandler.OwnBrain = brain;

				if(!fairyDragonflyHandler.AddToWorld())
				{
					if (log.IsWarnEnabled)
						log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
					return;
				}

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database

				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(fairyDragonflyHandler);
			}

			#endregion

			#region defineItems

			dragonflyWhip = GameServer.Database.SelectObject(typeof (GenericItemTemplate), Expression.Eq("Name", "Dragonfly Whip")) as GenericItemTemplate;
			if (dragonflyWhip == null)
			{
				dragonflyWhip = new GenericItemTemplate();
				dragonflyWhip.Name = "Dragonfly Whip";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + dragonflyWhip.Name + " , creating it ...");

				dragonflyWhip.Weight = 15;
				dragonflyWhip.Model = 859;

				dragonflyWhip.IsDropable = false;
				dragonflyWhip.IsSaleable = false;
				dragonflyWhip.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(dragonflyWhip);
			}

			// item db check
			recruitsVest = GameServer.Database.SelectObject(typeof (TorsoArmorTemplate), Expression.Eq("Name", "Recruit's Studded Vest")) as TorsoArmorTemplate;
			if (recruitsVest == null)
			{
				recruitsVest = new TorsoArmorTemplate();
				recruitsVest.Name = "Recruit's Studded Vest";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsVest.Name + ", creating it ...");
				recruitsVest.Level = 8;

				recruitsVest.Weight = 60;
				recruitsVest.Model = 81; // studded vest

				recruitsVest.ArmorFactor = 12;
				recruitsVest.ArmorLevel = eArmorLevel.Medium;

				recruitsVest.Value = 900;

				recruitsVest.IsDropable = true;
				recruitsVest.IsSaleable = true;
				recruitsVest.IsTradable = true;
				recruitsVest.Color = 9; // red leather

				recruitsVest.Bonus = 5; // default bonus

				recruitsVest.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 4));
				recruitsVest.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 3));

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsVest);
			}

			// item db check
			recruitsQuiltedVest = GameServer.Database.SelectObject(typeof (TorsoArmorTemplate), Expression.Eq("Name", "Recruit's Quilted Vest")) as TorsoArmorTemplate;
			if (recruitsQuiltedVest == null)
			{
				recruitsQuiltedVest = new TorsoArmorTemplate();
				recruitsQuiltedVest.Name = "Recruit's Quilted Vest";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsQuiltedVest.Name + ", creating it ...");
				recruitsQuiltedVest.Level = 8;

				recruitsQuiltedVest.Weight = 20;
				recruitsQuiltedVest.Model = 151; // studded vest

				recruitsQuiltedVest.ArmorFactor = 6;
				recruitsQuiltedVest.ArmorLevel = eArmorLevel.VeryLow;

				recruitsQuiltedVest.Value = 900;

				recruitsQuiltedVest.IsDropable = true;
				recruitsQuiltedVest.IsSaleable = true;
				recruitsQuiltedVest.IsTradable = true;
				recruitsQuiltedVest.Color = 9; // red leather

				recruitsQuiltedVest.Bonus = 5; // default bonus

				recruitsQuiltedVest.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 4));
				recruitsQuiltedVest.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 3));

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

			fairyDragonflyHandlerArea = new Circle();
			fairyDragonflyHandlerArea.Description = "Fairy Dragonfly Handler Area";
			fairyDragonflyHandlerArea.IsBroadcastEnabled = false;
			fairyDragonflyHandlerArea.Radius = 1000;
			fairyDragonflyHandlerArea.RegionID = fairyDragonflyHandler.Region.RegionID;
			fairyDragonflyHandlerArea.X = fairyDragonflyHandler.Position.X;
			fairyDragonflyHandlerArea.Y = fairyDragonflyHandler.Position.Y;

			AreaMgr.RegisterArea(fairyDragonflyHandlerArea);

			GameEventMgr.AddHandler(AreaEvent.PlayerEnter, new DOLEventHandler(PlayerEnterFairyDragonflyHandlerArea));
			
			GameEventMgr.AddHandler(GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));

			GameEventMgr.AddHandler(masterFrederick, GameObjectEvent.Interact, new DOLEventHandler(TalkToMasterFrederick));
			GameEventMgr.AddHandler(masterFrederick, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterFrederick));

			GameEventMgr.AddHandler(colm, GameObjectEvent.Interact, new DOLEventHandler(TalkToColm));
			GameEventMgr.AddHandler(colm, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToColm));

			GameEventMgr.AddHandler(nob, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToNob));

			GameEventMgr.AddHandler(haruld, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToHaruld));

			/* Now we add some hooks to trigger the quest dialog reponse. */
			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogResponse));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogResponse));

			/* Now we bring to Ydenia the possibility to give this quest to players */
			QuestMgr.AddQuestDescriptor(masterFrederick, typeof(IreFairyIreDescriptor));

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

			AreaMgr.UnregisterArea(fairyDragonflyHandlerArea);

			GameEventMgr.RemoveHandler(AreaEvent.PlayerEnter, new DOLEventHandler(PlayerEnterFairyDragonflyHandlerArea));
			
			GameEventMgr.RemoveHandler(GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));

			GameEventMgr.RemoveHandler(masterFrederick, GameObjectEvent.Interact, new DOLEventHandler(TalkToMasterFrederick));
			GameEventMgr.RemoveHandler(masterFrederick, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterFrederick));

			GameEventMgr.RemoveHandler(colm, GameObjectEvent.Interact, new DOLEventHandler(TalkToColm));
			GameEventMgr.RemoveHandler(colm, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToColm));

			GameEventMgr.RemoveHandler(nob, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToNob));

			GameEventMgr.RemoveHandler(haruld, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToHaruld));

			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogResponse));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogResponse));

			/* Now we remove to masterFrederick the possibility to give this quest to players */
			QuestMgr.RemoveQuestDescriptor(masterFrederick, typeof(IreFairyIreDescriptor));
		}

		protected static void PlayerEnterFairyDragonflyHandlerArea(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = ((AreaEventArgs)args).GameObject as GamePlayer;

			// if princess is dead no need to checks ...
			if (fairyDragonflyHandler == null || !fairyDragonflyHandler.Alive || fairyDragonflyHandler.ObjectState != eObjectState.Active || !fairyDragonflyHandler.AttackState)
				return;

			IreFairyIre quest = (IreFairyIre) player.IsDoingQuest(typeof (IreFairyIre));
			if (quest == null) return;
			
			if(quest.Step == 3)
			{
				SendSystemMessage(player, "Fairy Dragonfly Handler says, \"No! I have been betrayed by loathsome insects! How can this be?\"");
				IAggressiveBrain aggroBrain = fairyDragonflyHandler.Brain as IAggressiveBrain;
				if (aggroBrain != null)
					aggroBrain.AddToAggroList(player, 70);
			}
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

			if (QuestMgr.CanGiveQuest(typeof(IreFairyIre), player, masterFrederick) <= 0)
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
								quest.ChangeQuestStep(3);
								quest.initDragonflyHatchling();
							}
							break;
					}
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

			if (QuestMgr.CanGiveQuest(typeof(IreFairyIre), player, masterFrederick) <= 0)
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
							quest.ChangeQuestStep(2);
							break;
					}
				}

			}
		}

		protected void deleteDragonflyHatchling()
		{
			if (dragonflyHatchling != null)
			{
				GameEventMgr.RemoveHandler(dragonflyHatchling, GameObjectEvent.Interact, new DOLEventHandler(TalkToDragonflyHatchling));
				dragonflyHatchling.StopFollow();
				dragonflyHatchling.RemoveFromWorld();
				dragonflyHatchling = null;
			}
		}

		protected void initDragonflyHatchling()
		{
			dragonflyHatchling = new GameMob();
			dragonflyHatchling.Model = 819;
			dragonflyHatchling.Name = "Dragonfly Hatchling";
			dragonflyHatchling.GuildName = "Part of " + questTitle + " Quest";
			dragonflyHatchling.Realm = (byte) eRealm.None;
			dragonflyHatchling.Region = WorldMgr.GetRegion(1);

			dragonflyHatchling.Size = 25;
			dragonflyHatchling.Level = 3;
			Point pos = fairyDragonflyHandler.Position;
			pos.X += Util.Random(-150, 150);
			pos.Y += Util.Random(-150, 150);
			dragonflyHatchling.Position = pos;
			dragonflyHatchling.Heading = 93;

			PeaceBrain brain = new PeaceBrain();
			brain.Body = dragonflyHatchling;
			dragonflyHatchling.OwnBrain = brain;

			dragonflyHatchling.AddToWorld();
                          
			GameEventMgr.AddHandler(dragonflyHatchling, GameObjectEvent.Interact, new DOLEventHandler(TalkToDragonflyHatchling));
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

			if (QuestMgr.CanGiveQuest(typeof(IreFairyIre), player, masterFrederick) <= 0)
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
					masterFrederick.SayTo(player, "Recruit "+player.Name+", these fairies are becoming a serious problem for Cotswold. They have been harassing the citizens here! Can you believe that? They are retaliating for us getting to their generals. They must be [stopped]!");
					return;
				}
				else
				{
					if (quest.Step == 7)
					{
						masterFrederick.SayTo(player, "You've returned "+player.Name+". Did you have any success in locating the [egg for Colm]?");
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
							QuestMgr.ProposeQuestToPlayer(typeof(IreFairyIre), "Will you go out and find where these eggs went?", player, masterFrederick);
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
							masterFrederick.SayTo(player, "I know you have done a great service for Colm, and I am happy for you for your continued selfless acts. Go now "+player.Name+". We will speak again soon.");
							if (quest.Step == 7)
							{
								quest.FinishQuest();
							}
							break;
					}
				}
			}
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

			if (QuestMgr.CanGiveQuest(typeof(IreFairyIre), player, masterFrederick) <= 0)
				return;

			//We also check if the player is already doing the quest
			IreFairyIre quest = player.IsDoingQuest(typeof (IreFairyIre)) as IreFairyIre;

			if (e == GameObjectEvent.Interact)
			{
				if (quest != null && quest.Step == 4)
				{
					SendSystemMessage(player, "The dragonfly hatchling hums quitely.");

					quest.dragonflyHatchling.MaxSpeedBase = player.MaxSpeedBase;
					quest.dragonflyHatchling.Follow(player, 30, 2000);
					quest.ChangeQuestStep(5);
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

			if (QuestMgr.CanGiveQuest(typeof(IreFairyIre), player, masterFrederick) <= 0)
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
						colm.SayTo(player, "Oh thank you "+player.Name+" for bringing back the baby. I know his parents will be relieved he's returned safe and sound. How did you manage to get him back? Did it use anything? A chain? A bridle?");

						if (quest.dragonflyHatchling != null)
						{
							quest.deleteDragonflyHatchling();
						}
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
								EquipableItem item = recruitsVest.CreateInstance() as EquipableItem;
								if(player.HasAbilityToUseItem(item))
								{
									GiveItemToPlayer(colm, item, player);
								}
								else
								{
									GiveItemToPlayer(colm, recruitsQuiltedVest.CreateInstance(), player);
								}
								
								quest.ChangeQuestStep(7);
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

		/* This is our callback hook that will be called when the player clicks
		 * on any button in the quest offer dialog. We check if he accepts or
		 * declines here...
		 */
		protected static void QuestDialogResponse(DOLEvent e, object sender, EventArgs args)
		{
			QuestEventArgs gArgs = args as QuestEventArgs;

			if (gArgs != null && gArgs.QuestType.Equals(typeof(IreFairyIre)))
			{
				GamePlayer player = gArgs.Player;
				if (player == null) return;

				if (e == GamePlayerEvent.AcceptQuest)
				{
					if (QuestMgr.GiveQuestToPlayer(typeof(IreFairyIre), player, gArgs.Source as GameNPC))
					{
						masterFrederick.SayTo(player, "Alright recruit, we have very few leads in regards to this egg napping, but I am of the opinion someone saw something. Haruld at the stable across the way may have seen something. There are a lot of fairies that reside near there. Go speak with him.");
					}
				}
				else if (e == GamePlayerEvent.DeclineQuest)
				{

					player.Out.SendMessage("Oh well, if you change your mind, please come back!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
				}
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
					default:
						return "[Step #" + Step + "] No Description entered for this step!";
				}
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
					GiveItemToPlayer(CreateQuestItem(dragonflyWhip));
					ChangeQuestStep(4);
					return;
				}
			}

			if (Step == 5 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target == colm && gArgs.Item.QuestName == Name && gArgs.Item.Name == dragonflyWhip.Name)
				{
					if (dragonflyHatchling != null)
					{
						deleteDragonflyHatchling();
					}

					colm.SayTo(player, "A whip?! This is outrageous! I see they were just trying to torture him. These Ire Fairies are truly malicious creatures. I hope you wipe them out one day "+player.Name+". Here, take this as a sign of my [appreciation] for the return of the little one.");
					RemoveItemFromPlayer(colm, gArgs.Item);
					ChangeQuestStep(6);
					return;
				}
			}
		}


		public override void FinishQuest()
		{
			//Give reward to player here ...              
			m_questPlayer.GainExperience(507, 0, 0, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 7, Util.Random(50)), "You recieve {0} as a reward.");
		
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...
		}
	}
}
