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
 * 2) Go to loc=8602,47193 Camelot Hills and speak with Master Dunwyn. 
 * 3) Kill Queen Tatiana, center of the Fairy village, loc=9636,49714 Camelot Hills. 
 * 4) Speak with Master Dunwyn. 
 * 5) Go back to Master Frederick and hand him Queen Tatiana's Head for your rewards.
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

	public class Culmination : BaseFrederickQuest
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

		protected const string questTitle = "Culmination";
		protected const int minimumLevel = 5;
		protected const int maximumLevel = 5;

		private static GameNPC masterFrederick = null;

		private GameNPC dunwynClone = null;

		private GameNPC[] recruits = new GameNPC[4];

		private static GameNPC queenTatiana = null;
		private static GameNPC[] fairySorceress = new GameNPC[4];

		private static ItemTemplate queenTatianasHead = null;

		private bool queenTatianaAttackStarted = false;

		private static ItemTemplate recruitsGauntlets = null;
		private static ItemTemplate recruitsGloves = null;
		private static ItemTemplate recruitsJewel = null;
		private static ItemTemplate recruitsJewelCloth = null;

		private static ItemTemplate recruitsBracer = null;


		/* We need to define the constructors from the base class here, else there might be problems
		 * when loading this quest...
		 */
		public Culmination() : base()
		{
		}

		public Culmination(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public Culmination(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public Culmination(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Queen Tatiana", eRealm.None);
			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Queen Tatiana, creating ...");
				queenTatiana = new GameNPC();

				queenTatiana.Name = "Queen Tatiana";
				queenTatiana.X = 558500;
				queenTatiana.Y = 533042;
				queenTatiana.Z = 2573;
				queenTatiana.Heading = 174;
				queenTatiana.Model = 603;
				queenTatiana.GuildName = "Part of " + questTitle + " Quest";
				queenTatiana.Realm = (byte) eRealm.None;
				queenTatiana.CurrentRegionID = 1;
				queenTatiana.Size = 49;
				queenTatiana.Level = 5;

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 30;
				brain.AggroRange = 600;
				queenTatiana.SetOwnBrain(brain);

				if (SAVE_INTO_DATABASE)
					queenTatiana.SaveIntoDatabase();

				queenTatiana.AddToWorld();
			}
			else
			{
				queenTatiana = (GameNPC) npcs[0];
			}

			int counter = 0;
			foreach (GameNPC npc in queenTatiana.GetNPCsInRadius(500))
			{
				if (npc.Name == "ire fairy sorceress")
				{
					fairySorceress[counter] = (GameNPC) npc;
					counter++;
				}
				if (counter == fairySorceress.Length)
					break;
			}

			for (int i = 0; i < fairySorceress.Length; i++)
			{
				if (fairySorceress[i] == null)
				{
					if (log.IsWarnEnabled)
						log.Warn("Could not find ire fairy sorceress, creating ...");
					fairySorceress[i] = new GameNPC();
					fairySorceress[i].Model = 603; // //819;
					fairySorceress[i].Name = "ire fairy sorceress";
					fairySorceress[i].GuildName = "Part of " + questTitle + " Quest";
					fairySorceress[i].Realm = (byte) eRealm.None;
					fairySorceress[i].CurrentRegionID = 1;
					fairySorceress[i].Size = 35;
					fairySorceress[i].Level = 5;
					fairySorceress[i].X = queenTatiana.X + Util.Random(30, 150);
					fairySorceress[i].Y = queenTatiana.Y + Util.Random(30, 150);
					fairySorceress[i].Z = queenTatiana.Z;

					StandardMobBrain brain = new StandardMobBrain();
					brain.AggroLevel = 30;
					brain.AggroRange = 600;
					fairySorceress[i].SetOwnBrain(brain);

					fairySorceress[i].Heading = 93;
					//fairySorceress[i].EquipmentTemplateID = 200276;                

					//You don't have to store the created mob in the db if you don't want,
					//it will be recreated each time it is not found, just comment the following
					//line if you rather not modify your database
					if (SAVE_INTO_DATABASE)
						fairySorceress[i].SaveIntoDatabase();
					fairySorceress[i].AddToWorld();
				}
			}

			#endregion

			#region defineItems

			queenTatianasHead = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "queen_tatianas_head");
			if (queenTatianasHead == null)
			{
				queenTatianasHead = new ItemTemplate();
				queenTatianasHead.Name = "Queen Tatiana's Head";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + queenTatianasHead.Name + " , creating it ...");

				queenTatianasHead.Weight = 15;
				queenTatianasHead.Model = 503;

				queenTatianasHead.Object_Type = (int) eObjectType.GenericItem;

				queenTatianasHead.Id_nb = "queen_tatianas_head";
				queenTatianasHead.IsPickable = true;
				queenTatianasHead.IsDropable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(queenTatianasHead);
			}

			// item db check
			recruitsGauntlets = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "recruits_studded_gauntlets");
			if (recruitsGauntlets == null)
			{
				recruitsGauntlets = new ItemTemplate();
				recruitsGauntlets.Name = "Recruit's Studded Gauntles";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsGauntlets.Name + ", creating it ...");
				recruitsGauntlets.Level = 8;

				recruitsGauntlets.Weight = 24;
				recruitsGauntlets.Model = 80;

				recruitsGauntlets.DPS_AF = 14; // Armour
				recruitsGauntlets.SPD_ABS = 19; // Absorption

				recruitsGauntlets.Object_Type = (int) eObjectType.Studded;
				recruitsGauntlets.Item_Type = (int) eEquipmentItems.HAND;
				recruitsGauntlets.Id_nb = "recruits_studded_gauntlets";
				recruitsGauntlets.Gold = 0;
				recruitsGauntlets.Silver = 9;
				recruitsGauntlets.Copper = 0;
				recruitsGauntlets.IsPickable = true;
				recruitsGauntlets.IsDropable = true;
				recruitsGauntlets.Color = 9; // red leather

				recruitsGauntlets.Bonus = 5; // default bonus

				recruitsGauntlets.Bonus1 = 4;
				recruitsGauntlets.Bonus1Type = (int) eStat.STR;

				recruitsGauntlets.Bonus2 = 3;
				recruitsGauntlets.Bonus2Type = (int) eStat.DEX;

				recruitsGauntlets.Quality = 100;
				recruitsGauntlets.Condition = 1000;
				recruitsGauntlets.MaxCondition = 1000;
				recruitsGauntlets.Durability = 1000;
				recruitsGauntlets.MaxDurability = 1000;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsGauntlets);
			}

			// item db check
			recruitsGloves = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "recruits_quilted_gloves");
			if (recruitsGloves == null)
			{
				recruitsGloves = new ItemTemplate();
				recruitsGloves.Name = "Recruit's Quilted Gloves";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsGloves.Name + ", creating it ...");
				recruitsGloves.Level = 8;

				recruitsGloves.Weight = 8;
				recruitsGloves.Model = 154;

				recruitsGloves.DPS_AF = 7; // Armour
				recruitsGloves.SPD_ABS = 0; // Absorption

				recruitsGloves.Object_Type = (int) eObjectType.Cloth;
				recruitsGloves.Item_Type = (int) eEquipmentItems.HAND;
				recruitsGloves.Id_nb = "recruits_quilted_gloves";
				recruitsGloves.Gold = 0;
				recruitsGloves.Silver = 9;
				recruitsGloves.Copper = 0;
				recruitsGloves.IsPickable = true;
				recruitsGloves.IsDropable = true;
				recruitsGloves.Color = 27; // red leather

				recruitsGloves.Bonus = 5; // default bonus

				recruitsGloves.Bonus1 = 4;
				recruitsGloves.Bonus1Type = (int) eStat.INT;

				recruitsGloves.Bonus2 = 3;
				recruitsGloves.Bonus2Type = (int) eStat.DEX;

				recruitsGloves.Quality = 100;
				recruitsGloves.Condition = 1000;
				recruitsGloves.MaxCondition = 1000;
				recruitsGloves.Durability = 1000;
				recruitsGloves.MaxDurability = 1000;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsGloves);
			}

			recruitsJewel = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "recruits_cloudy_jewel");
			if (recruitsJewel == null)
			{
				recruitsJewel = new ItemTemplate();
				recruitsJewel.Name = "Recruit's Cloudy Jewel";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsJewel.Name + ", creating it ...");
				recruitsJewel.Level = 7;

				recruitsJewel.Weight = 2;
				recruitsJewel.Model = 110;

				recruitsJewel.Object_Type = (int) eObjectType.Magical;
				recruitsJewel.Item_Type = (int) eEquipmentItems.JEWEL;
				recruitsJewel.Id_nb = "recruits_cloudy_jewel";
				recruitsJewel.Gold = 0;
				recruitsJewel.Silver = 9;
				recruitsJewel.Copper = 0;
				recruitsJewel.IsPickable = true;
				recruitsJewel.IsDropable = true;

				recruitsJewel.Bonus = 5; // default bonus

				recruitsJewel.Bonus1 = 6;
				recruitsJewel.Bonus1Type = (int) eStat.STR;

				recruitsJewel.Quality = 100;
				recruitsJewel.Condition = 1000;
				recruitsJewel.MaxCondition = 1000;
				recruitsJewel.Durability = 1000;
				recruitsJewel.MaxDurability = 1000;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsJewel);
			}

			recruitsJewelCloth = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "recruits_cloudy_jewel_cloth");
			if (recruitsJewelCloth == null)
			{
				recruitsJewelCloth = new ItemTemplate();
				recruitsJewelCloth.Name = "Recruit's Cloudy Jewel";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsJewelCloth.Name + ", creating it ...");
				recruitsJewelCloth.Level = 7;

				recruitsJewelCloth.Weight = 2;
				recruitsJewelCloth.Model = 110;

				recruitsJewelCloth.Object_Type = (int) eObjectType.Magical;
				recruitsJewelCloth.Item_Type = (int) eEquipmentItems.JEWEL;
				recruitsJewelCloth.Id_nb = "recruits_cloudy_jewel_cloth";
				recruitsJewelCloth.Gold = 0;
				recruitsJewelCloth.Silver = 9;
				recruitsJewelCloth.Copper = 0;
				recruitsJewelCloth.IsPickable = true;
				recruitsJewelCloth.IsDropable = true;

				recruitsJewelCloth.Bonus = 5; // default bonus

				recruitsJewelCloth.Bonus1 = 4;
				recruitsJewelCloth.Bonus1Type = (int) eStat.INT;

				recruitsJewelCloth.Bonus2 = 3;
				recruitsJewelCloth.Bonus2Type = (int) eStat.CON;

				recruitsJewelCloth.Bonus3 = 1;
				recruitsJewelCloth.Bonus3Type = (int) eResist.Body;

				recruitsJewelCloth.Quality = 100;
				recruitsJewelCloth.Condition = 1000;
				recruitsJewelCloth.MaxCondition = 1000;
				recruitsJewelCloth.Durability = 1000;
				recruitsJewelCloth.MaxDurability = 1000;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsJewelCloth);
			}

			recruitsBracer = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "recruits_golden_bracer");
			if (recruitsBracer == null)
			{
				recruitsBracer = new ItemTemplate();
				recruitsBracer.Name = "Recruit's Golden Bracer";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsBracer.Name + ", creating it ...");
				recruitsBracer.Level = 7;

				recruitsBracer.Weight = 2;
				recruitsBracer.Model = 121;

				recruitsBracer.Object_Type = (int) eObjectType.Magical;
				recruitsBracer.Item_Type = (int) eEquipmentItems.R_BRACER;
				recruitsBracer.Id_nb = "recruits_golden_bracer";
				recruitsBracer.Gold = 0;
				recruitsBracer.Silver = 9;
				recruitsBracer.Copper = 0;
				recruitsBracer.IsPickable = true;
				recruitsBracer.IsDropable = true;

				recruitsBracer.Bonus = 5; // default bonus

				recruitsBracer.Bonus1 = 4;
				recruitsBracer.Bonus1Type = (int) eStat.STR;

				recruitsBracer.Bonus2 = 3;
				recruitsBracer.Bonus2Type = (int) eStat.CON;

				recruitsBracer.Bonus3 = 1;
				recruitsBracer.Bonus3Type = (int) eResist.Body;

				recruitsBracer.Quality = 100;
				recruitsBracer.Condition = 1000;
				recruitsBracer.MaxCondition = 1000;
				recruitsBracer.Durability = 1000;
				recruitsBracer.MaxDurability = 1000;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsBracer);
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

			GameEventMgr.AddHandler(queenTatiana, GameNPCEvent.OnAICallback, new DOLEventHandler(CheckNearQueenTatiana));

			/* Now we bring to masterFrederick the possibility to give this quest to players */
			masterFrederick.AddQuestToGive(typeof (Culmination));			

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

			GameEventMgr.RemoveHandler(queenTatiana, GameNPCEvent.OnAICallback, new DOLEventHandler(CheckNearQueenTatiana));

			/* Now we remove to masterFrederick the possibility to give this quest to players */
			masterFrederick.RemoveQuestToGive(typeof (Culmination));
		}

		protected static void CheckNearQueenTatiana(DOLEvent e, object sender, EventArgs args)
		{
			GameNPC queenTatiana = (GameNPC) sender;

			// if princess is dead no ned to checks ...
			if (!queenTatiana.IsAlive || queenTatiana.ObjectState != GameObject.eObjectState.Active)
				return;

			foreach (GamePlayer player in queenTatiana.GetPlayersInRadius(1000))
			{
				Culmination quest = (Culmination) player.IsDoingQuest(typeof (Culmination));

				if (quest != null && !quest.queenTatianaAttackStarted && quest.Step == 2)
				{
					quest.queenTatianaAttackStarted = true;

					SendSystemMessage(player, "There they are. You take care of the queen I'll deal with the fairy sorcesses littleone.");
					IAggressiveBrain aggroBrain = queenTatiana.Brain as IAggressiveBrain;
					if (aggroBrain != null)
						aggroBrain.AddToAggroList(player, 70);

					for (int i = 0; i < fairySorceress.Length; i++)
					{
						if (quest.recruits[i] != null)
						{
							aggroBrain = quest.recruits[i].Brain as IAggressiveBrain;
							if (aggroBrain != null)
								aggroBrain.AddToAggroList(fairySorceress[i], 50);
						}

						aggroBrain = fairySorceress[i].Brain as IAggressiveBrain;
						if (aggroBrain != null)
							aggroBrain.AddToAggroList(quest.recruits[i], 50);
					}

					// if we find player doing quest stop looking for further ones ...
					break;
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

			if(masterFrederick.CanGiveQuest(typeof (Culmination), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Culmination quest = player.IsDoingQuest(typeof (Culmination)) as Culmination;

			masterFrederick.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...
					masterFrederick.SayTo(player, "Hello again recruit. It seems those fairies just will not give up. I don't know why they're so intent on harming anyone here in Cotswold. I mean, it's not like we did anything to [start] this fight.");
					return;
				}
				else
				{
					if (quest.Step == 1)
					{
						masterFrederick.SayTo(player, "Remember, Master Dunwyn is in the woods south of the bridge to Camelot. He is near the fairy's village. Be safe!");

					}
					else if (quest.Step == 4)
					{
						masterFrederick.SayTo(player, "You've returned Vinde. That can only mean that you were successful in your battle with the fairies! Please, show me whatever proof you have that the fairies are finally gone.");
					}
					else if (quest.Step == 5)
					{
						masterFrederick.SayTo(player, "Wonderful! Now I know Cotswold will be safe, thanks in no small part to you, Recruit Vinde. Excellent work. Cotswold is forever in your debt. I have a [reward] for you. I hope you have some use for it.");
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
						case "start":
							masterFrederick.SayTo(player, "Well, they are malevolent beings, but still. This is ridiculous! But, once again, here they are, trying to get us. Quite frankly, I think this is crazy. I mean, I want them gone, but I don't want them [extinct].");
							break;
						case "extinct":
							masterFrederick.SayTo(player, "But they leave us no choice. Now listen recruit. I have enlisted the aid of a few other recruits, as well as Master Dunwyn, to help deal with this problem. Make your way over the bridge to Camelot. Once you have crossed the bridge you will need to head south along the river bank. You will see Master Dunwyn and the other recruits waiting for you in the trees. Speak with Master Dunwyn when you arrive. Are you [ready] for this task?");
							break;
							//If the player offered his "help", we send the quest dialog now!
						case "ready":
							player.Out.SendQuestSubscribeCommand(masterFrederick, QuestMgr.GetIDForQuestType(typeof(Culmination)), "Are you ready to take part in this monumental battle for the good of Cotswold?");
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "reward":
							masterFrederick.SayTo(player, "Here are a few things to help you start off your life as a great adventurer. Be safe and well Vinde. You have now grown beyond my teachings. If you wish to continue questings, speak with the Town Criers and with your trainer. Cotswold, and I, thank you again for your assistance.");
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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(Culmination)))
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

		protected static void TalkToMasterDunwyn(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(masterFrederick.CanGiveQuest(typeof (Culmination), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Culmination quest = player.IsDoingQuest(typeof (Culmination)) as Culmination;

			if (e == GameLivingEvent.Interact)
			{
				if (quest != null && quest.dunwynClone != null && quest.dunwynClone == sender)
				{
					quest.dunwynClone.TurnTo(player);

					if (quest.Step <= 1)
					{
						quest.dunwynClone.SayTo(player, "What? Who's that? Can't you leave an old man in peace?");
						quest.dunwynClone.SayTo(player, "Vinde, how good to see you again. I see that the fairy problem is slightly [larger] than when I left.");
					}
					else if (quest.Step == 2)
					{
						quest.dunwynClone.SayTo(player, "Go now and kill their queen, so that Cotswold is at ease.");
						foreach (GameNPC recruit in quest.recruits)
						{
							recruit.Follow(player, 50 + Util.Random(100), 4000);
						}
					}
					else if (quest.Step == 3)
					{
						quest.dunwynClone.SayTo(player, "Good job again Vinde. Now, I will be taking these recruits back to Avalon Marsh with me. There is much to do there. Good luck to you Vinde. I wish you well in your future endeavors.");
						quest.ResetMasterDunwyn();
						quest.Step = 4;
						if (player.Group != null)
						{
							foreach (GamePlayer groupMember in player.Group.GetPlayersInTheGroup())
							{
								Culmination memberQuest = groupMember.IsDoingQuest(typeof (Culmination)) as Culmination;
								// we found another groupmember doing the same quest...
								if (memberQuest != null && memberQuest.Step == 3)
								{
									memberQuest.Step = 4;
								}
							}
						}
					}
					return;
				}
			} // The player whispered to NPC (clicked on the text inside the [])
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				if (quest != null && quest.dunwynClone == sender)
				{
					quest.dunwynClone.TurnTo(player);
					//Do some small talk :)
					switch (wArgs.Text)
					{
						case "larger":
							quest.dunwynClone.SayTo(player, "Here is the situation. The other recruits and I shall stave off the fairy populace while you deal with their Queen, Tatiana. Oh yes, they do have a queen. I am fairly certain she is none too happy with you for having [killed] her daughter.");
							break;

						case "killed":
							quest.dunwynClone.SayTo(player, "Surely you haven't already forgotten about your great battle with Obera, have you? You're far too young to be forgetting things Vinde, but I digress. You must make your way into the camp and slay [Queen Tatiana].");
							break;
						case "Queen Tatiana":
							quest.dunwynClone.SayTo(player, "She is easy enough to spot, for her colors differ from the other fairies around her. Good luck Vinde.");
							if (quest.Step == 1)
							{
								quest.Step = 2;
								if (player.Group != null)
								{
									foreach (GamePlayer groupMember in player.Group.GetPlayersInTheGroup())
									{
										Culmination memberQuest = groupMember.IsDoingQuest(typeof (Culmination)) as Culmination;
										// we found another groupmember doing the same quest...
										if (memberQuest != null && memberQuest.Step == 1)
										{
											memberQuest.Step = 2;
										}
									}
								}
							}

							foreach (GameNPC recruit in quest.recruits)
							{
								recruit.Follow(player, 50 + Util.Random(100), 4000);
							}
							break;
					}
				}
			}
		}

		protected virtual void ResetMasterDunwyn()
		{
			if (dunwynClone != null && (dunwynClone.IsAlive || dunwynClone.ObjectState == GameObject.eObjectState.Active))
			{
				m_animSpellObjectQueue.Enqueue(dunwynClone);
				m_animSpellTeleportTimerQueue.Enqueue(new RegionTimer(dunwynClone, new RegionTimerCallback(MakeAnimSpellSequence), 4000));

				m_animEmoteObjectQueue.Enqueue(dunwynClone);
				m_animEmoteTeleportTimerQueue.Enqueue(new RegionTimer(dunwynClone, new RegionTimerCallback(MakeAnimEmoteSequence), 5000));

				for (int i = 0; i < recruits.Length; i++)
				{
					if (recruits[i] != null)
					{
						m_animEmoteObjectQueue.Enqueue(recruits[i]);
						m_animEmoteTeleportTimerQueue.Enqueue(new RegionTimer(recruits[i], new RegionTimerCallback(MakeAnimEmoteSequence), 4500));
					}
				}
			}

			if (dunwynClone != null)
			{
				new RegionTimer(dunwynClone, new RegionTimerCallback(DeleteDunwynClone), 6000);
			}
		}

		protected virtual int DeleteDunwynClone(RegionTimer callingTimer)
		{
			if (dunwynClone != null)
			{
				dunwynClone.Delete();
				GameEventMgr.RemoveHandler(dunwynClone, GameLivingEvent.Interact, new DOLEventHandler(TalkToMasterDunwyn));
				GameEventMgr.RemoveHandler(dunwynClone, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterDunwyn));
			}

			for (int i = 0; i < recruits.Length; i++)
			{
				if (recruits[i] != null)
					recruits[i].Delete();
			}

			return 0;
		}

		protected void CreateDunwynClone()
		{
			GameNpcInventoryTemplate template;
			if (dunwynClone == null)
			{
				dunwynClone = new GameNPC();
				dunwynClone.Name = "Master Dunwyn";
				dunwynClone.Model = 9;
				dunwynClone.GuildName = "Part of " + questTitle + " Quest";
				dunwynClone.Realm = (byte) eRealm.Albion;
				dunwynClone.CurrentRegionID = 1;
				dunwynClone.Size = 50;
				dunwynClone.Level = 14;

				dunwynClone.X = GameLocation.ConvertLocalXToGlobalX(8602, 0) + Util.Random(-150, 150);
				dunwynClone.Y = GameLocation.ConvertLocalYToGlobalY(47193, 0) + Util.Random(-150, 150);
				dunwynClone.Z = 2409;
				dunwynClone.Heading = 342;

				template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 798);
				template.AddNPCEquipment(eInventorySlot.RightHandWeapon, 19);
				dunwynClone.Inventory = template.CloseTemplate();
				dunwynClone.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

//				dunwynClone.AddNPCEquipment((byte) eEquipmentItems.TORSO, 798, 0, 0, 0);
//				dunwynClone.AddNPCEquipment((byte) eEquipmentItems.RIGHT_HAND, 19, 0, 0, 0);

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 0;
				dunwynClone.SetOwnBrain(brain);

				dunwynClone.AddToWorld();

				GameEventMgr.AddHandler(dunwynClone, GameLivingEvent.Interact, new DOLEventHandler(TalkToMasterDunwyn));
				GameEventMgr.AddHandler(dunwynClone, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterDunwyn));
			}
			else
			{
				dunwynClone.MoveTo(1, 567604, 509619, 2813, 3292);
			}


			foreach (GamePlayer visPlayer in dunwynClone.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				visPlayer.Out.SendEmoteAnimation(dunwynClone, eEmote.Bind);
			}


			for (int i = 0; i < recruits.Length; i++)
			{
				recruits[i] = new GameNPC();

				recruits[i].Name = "Recruit";

				recruits[i].GuildName = "Part of " + questTitle + " Quest";
				recruits[i].Realm = (byte) eRealm.Albion;
				recruits[i].CurrentRegionID = 1;

				recruits[i].Size = 50;
				recruits[i].Level = 6;
				recruits[i].X = GameLocation.ConvertLocalXToGlobalX(8602, 0) + Util.Random(-150, 150);
				recruits[i].Y = GameLocation.ConvertLocalYToGlobalY(47193, 0) + Util.Random(-150, 150);

				recruits[i].Z = 2409;
				recruits[i].Heading = 187;

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 0;
				recruits[i].SetOwnBrain(brain);

			}

			recruits[0].Name = "Recruit Armsman McTavish";
			recruits[0].Model = 40;
			template = new GameNpcInventoryTemplate();
			template.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 69);
			template.AddNPCEquipment(eInventorySlot.TorsoArmor, 46);
			template.AddNPCEquipment(eInventorySlot.LegsArmor, 47);
			template.AddNPCEquipment(eInventorySlot.FeetArmor, 50);
			template.AddNPCEquipment(eInventorySlot.ArmsArmor, 48);
			template.AddNPCEquipment(eInventorySlot.HandsArmor, 49);
			recruits[0].Inventory = template.CloseTemplate();
			recruits[0].SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);

//			recruits[0].AddNPCEquipment((byte) eEquipmentItems.TWO_HANDED, 69, 0, 0, 0);
//			recruits[0].AddNPCEquipment((byte) eEquipmentItems.TORSO, 46, 0, 0, 0);
//			recruits[0].AddNPCEquipment((byte) eEquipmentItems.LEGS, 47, 0, 0, 0);
//			recruits[0].AddNPCEquipment((byte) eEquipmentItems.FEET, 50, 0, 0, 0);
//			recruits[0].AddNPCEquipment((byte) eEquipmentItems.ARMS, 48, 0, 0, 0);
//			recruits[0].AddNPCEquipment((byte) eEquipmentItems.HAND, 49, 0, 0, 0);

			recruits[1].Name = "Recruit Paladin Andral";
			recruits[1].Model = 41;
			template = new GameNpcInventoryTemplate();
			template.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 6);
			template.AddNPCEquipment(eInventorySlot.TorsoArmor, 41);
			template.AddNPCEquipment(eInventorySlot.LegsArmor, 42);
			template.AddNPCEquipment(eInventorySlot.FeetArmor, 45);
			template.AddNPCEquipment(eInventorySlot.ArmsArmor, 43);
			template.AddNPCEquipment(eInventorySlot.HandsArmor, 44);
			recruits[1].Inventory = template.CloseTemplate();
			recruits[1].SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);

//			recruits[1].AddNPCEquipment((byte) eEquipmentItems.TWO_HANDED, 6, 0, 0, 0);
//			recruits[1].AddNPCEquipment((byte) eEquipmentItems.TORSO, 41, 0, 0, 0);
//			recruits[1].AddNPCEquipment((byte) eEquipmentItems.LEGS, 42, 0, 0, 0);
//			recruits[1].AddNPCEquipment((byte) eEquipmentItems.FEET, 45, 0, 0, 0);
//			recruits[1].AddNPCEquipment((byte) eEquipmentItems.ARMS, 43, 0, 0, 0);
//			recruits[1].AddNPCEquipment((byte) eEquipmentItems.HAND, 44, 0, 0, 0);

			recruits[2].Name = "Recruit Scout Gillman";
			recruits[2].Model = 32;
			template = new GameNpcInventoryTemplate();
			template.AddNPCEquipment(eInventorySlot.RightHandWeapon, 4);
			template.AddNPCEquipment(eInventorySlot.TorsoArmor, 36);
			template.AddNPCEquipment(eInventorySlot.LegsArmor, 37);
			recruits[2].Inventory = template.CloseTemplate();
			recruits[2].SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

//			recruits[2].AddNPCEquipment((byte) eEquipmentItems.RIGHT_HAND, 4, 0, 0, 0);
//			recruits[2].AddNPCEquipment((byte) eEquipmentItems.TORSO, 36, 0, 0, 0);
//			recruits[2].AddNPCEquipment((byte) eEquipmentItems.LEGS, 37, 0, 0, 0);

			recruits[3].Name = "Recruit Scout Stuart";
			recruits[3].Model = 32;
			template = new GameNpcInventoryTemplate();
			template.AddNPCEquipment(eInventorySlot.RightHandWeapon, 5);
			template.AddNPCEquipment(eInventorySlot.TorsoArmor, 36);
			template.AddNPCEquipment(eInventorySlot.LegsArmor, 37);
			recruits[3].Inventory = template.CloseTemplate();
			recruits[3].SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

//			recruits[3].AddNPCEquipment((byte) eEquipmentItems.RIGHT_HAND, 5, 0, 0, 0);
//			recruits[3].AddNPCEquipment((byte) eEquipmentItems.TORSO, 36, 0, 0, 0);
//			recruits[3].AddNPCEquipment((byte) eEquipmentItems.LEGS, 37, 0, 0, 0);

			for (int i = 0; i < recruits.Length; i++)
			{
				recruits[i].AddToWorld();
			}

		}

		protected static void PlayerLeftWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			Culmination quest = player.IsDoingQuest(typeof (Culmination)) as Culmination;
			if (quest != null)
			{
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));

				quest.ResetMasterDunwyn();
			}
		}

		protected static void PlayerEnterWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			Culmination quest = player.IsDoingQuest(typeof (Culmination)) as Culmination;
			if (quest != null)
			{
				GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));

				if (quest.Step >= 1 && quest.Step <= 3)
				{
					quest.CreateDunwynClone();
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
			if (player.IsDoingQuest(typeof (Culmination)) != null)
				return true;

			// This checks below are only performed is player isn't doing quest already
			if (!CheckPartAccessible(player, typeof (Culmination)))
				return false;

			if (player.Level < minimumLevel || player.Level > maximumLevel)
				return false;

			return true;
		}

		/* This is our callback hook that will be called when the player clicks
		 * on any button in the quest offer dialog. We check if he accepts or
		 * declines here...
		 */

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			//We recheck the qualification, because we don't talk to players
			//who are not doing the quest
			if(masterFrederick.CanGiveQuest(typeof (Culmination), player)  <= 0)
				return;

			Culmination quest = player.IsDoingQuest(typeof (Culmination)) as Culmination;

			if (quest != null)
				return;

			if (response == 0x00)
			{
				SendReply(player, "Oh well, if you change your mind, please come back!");
			}
			else
			{
				//Check if we can add the quest!
				if (!masterFrederick.GiveQuest(typeof (Culmination), player, 1))
					return;

				GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));

				masterFrederick.SayTo(player, "Remember, Master Dunwyn is in the woods south of the bridge to Camelot. He is near the fairy's village. Be safe!");


				bool dunwynCloneCreated = false;
				if (player.Group != null)
				{
					foreach (GamePlayer groupMember in player.Group.GetPlayersInTheGroup())
					{
						Culmination memberQuest = groupMember.IsDoingQuest(typeof (Culmination)) as Culmination;
						// we found another groupmember doing the same quest...
						if (memberQuest != null && memberQuest.dunwynClone != null)
						{
							dunwynCloneCreated = true;
							break;
						}
					}
				}

				if (!dunwynCloneCreated)
				{
					quest = player.IsDoingQuest(typeof (Culmination)) as Culmination;
					if(quest != null) quest .CreateDunwynClone();
				}
			}
		}

		/* This is our callback hook that will be called when the player clicks
		 * on any button in the quest offer dialog. We check if he accepts or
		 * declines here...
		 */

		private static void CheckPlayerAbortQuest(GamePlayer player, byte response)
		{
			Culmination quest = player.IsDoingQuest(typeof (Culmination)) as Culmination;

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
						return "[Step #1] Travel across the bridge to Camelot and turn south. Follow the inside of the woods until you find Master Dunwyn, down beyond the gates to the houses.";
					case 2:
						return "[Step #2] Find and kill Queen, Tatiana.";
					case 3:
						return "[Step #3] When the fighting is over, speak with Master Dunwyn.";
					case 4:
						return "[Step #4] Return to Master Frederick in Cotswold. Hand him the head of Queen Tatiana as evidence that the fairy threat has now been extinguished.";
					case 5:
						return "[Step #5] Wait for Master Frederick to reward you. If he stops speaking with you, ask about a [reward] for your time and effort.";
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player==null || player.IsDoingQuest(typeof (Culmination)) == null)
				return;


			if (Step == 2 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;

				if (gArgs.Target.Name == queenTatiana.Name)
				{
					SendSystemMessage("You slay the queen and take her head as proof.");
					GiveItem(gArgs.Target, player, queenTatianasHead);
					Step = 3;
					return;
				}
			}

			if (Step == 4 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == masterFrederick.Name && gArgs.Item.Id_nb == queenTatianasHead.Id_nb)
				{
					masterFrederick.SayTo(m_questPlayer, "Wonderful! Now I know Cotswold will be safe, thanks in no small part to you, Recruit Vinde. Excellent work. Cotswold is forever in your debt. I have a [reward] for you. I hope you have some use for it.");
					RemoveItem(masterFrederick, player, queenTatianasHead);
					Step = 5;
					return;
				}
			}
		}

		public override void AbortQuest()
		{
			ResetMasterDunwyn();

			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, queenTatianasHead, false);

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			// make sure to clean up, should be needed , but just to make certain
			ResetMasterDunwyn();
			//Give reward to player here ...              
			m_questPlayer.GainExperience(1012, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 9, Util.Random(50)), "You recieve {0} as a reward.");
			if (m_questPlayer.HasAbilityToUseItem(recruitsGauntlets))
			{
				GiveItem(masterFrederick, m_questPlayer, recruitsGauntlets);
				GiveItem(masterFrederick, m_questPlayer, recruitsJewel);
			}
			else
			{
				GiveItem(masterFrederick, m_questPlayer, recruitsGloves);
				GiveItem(masterFrederick, m_questPlayer, recruitsJewelCloth);
			}

			GiveItem(masterFrederick, m_questPlayer, recruitsBracer);

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
		}

	}
}
