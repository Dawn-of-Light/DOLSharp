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
 * 1) Travel to loc=41211,50221 Vale of Mularn to speak with Dalikor 
 * 2) Go to loc= 45394,39768 Vale of Mularn and speak with MasterBriedi. 
 * 3) Kill Queen Vuuna, center of the Askefruer village, loc=47071,38934 Vale of Mularn. 
 * 4) Speak with MasterBried. 
 * 5) Go back to Dalikor and hand him Queen Vuuna's Head for your rewards.
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

	public class Culmination : BaseDalikorQuest
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

		protected const string questTitle = "Culmination (Mid)";
		protected const int minimumLevel = 5;
		protected const int maximumLevel = 5;

		private static GameNPC dalikor = null;

		private GameNPC briediClone = null;

		private GameNPC[] recruits = new GameNPC[4];

		private static GameMob queenVuuna = null;
		private static GameMob[] askefruerSorceress = new GameMob[4];

		private static ItemTemplate queenVuunaHead = null;

		private bool queenVuunaAttackStarted = false;

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

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Queen Vuuna", eRealm.None);
			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Queen Vuuna, creating ...");
				queenVuuna = new GameMob();

				queenVuuna.Name = "Queen Vuuna";
				queenVuuna.X = GameLocation.ConvertLocalXToGlobalX(47071, 100);
				queenVuuna.Y = GameLocation.ConvertLocalYToGlobalY(38934, 100);
				queenVuuna.Z = 4747;
				queenVuuna.Heading = 50;
				queenVuuna.Model = 678;
				queenVuuna.GuildName = "Part of " + questTitle + " Quest";
				queenVuuna.Realm = (byte) eRealm.None;
				queenVuuna.CurrentRegionID = 100;
				queenVuuna.Size = 49;
				queenVuuna.Level = 5;

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 30;
				brain.AggroRange = 600;
				queenVuuna.SetOwnBrain(brain);

				if (SAVE_INTO_DATABASE)
					queenVuuna.SaveIntoDatabase();

				queenVuuna.AddToWorld();
			}
			else
			{
				queenVuuna = (GameMob) npcs[0];
			}

			int counter = 0;
			foreach (GameNPC npc in queenVuuna.GetNPCsInRadius(500))
			{
				if (npc.Name == "askefruer sorceress")
				{
					askefruerSorceress[counter] = (GameMob) npc;
					counter++;
				}
				if (counter == askefruerSorceress.Length)
					break;
			}

			for (int i = 0; i < askefruerSorceress.Length; i++)
			{
				if (askefruerSorceress[i] == null)
				{
					if (log.IsWarnEnabled)
						log.Warn("Could not find askefruer sorceress, creating ...");
					askefruerSorceress[i] = new GameMob();
					askefruerSorceress[i].Model = 678; // //819;
					askefruerSorceress[i].Name = "askefruer sorceress";
					askefruerSorceress[i].GuildName = "Part of " + questTitle + " Quest";
					askefruerSorceress[i].Realm = (byte) eRealm.None;
					askefruerSorceress[i].CurrentRegionID = 100;
					askefruerSorceress[i].Size = 35;
					askefruerSorceress[i].Level = 5;
					askefruerSorceress[i].X = queenVuuna.X + Util.Random(30, 150);
					askefruerSorceress[i].Y = queenVuuna.Y + Util.Random(30, 150);
					askefruerSorceress[i].Z = queenVuuna.Z;

					StandardMobBrain brain = new StandardMobBrain();
					brain.AggroLevel = 30;
					brain.AggroRange = 600;
					askefruerSorceress[i].SetOwnBrain(brain);

					askefruerSorceress[i].Heading = 93;

					//You don't have to store the created mob in the db if you don't want,
					//it will be recreated each time it is not found, just comment the following
					//line if you rather not modify your database
					if (SAVE_INTO_DATABASE)
						askefruerSorceress[i].SaveIntoDatabase();
					askefruerSorceress[i].AddToWorld();
				}
			}

			#endregion

			#region defineItems

			queenVuunaHead = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "queen_vuuna_head");
			if (queenVuunaHead == null)
			{
				queenVuunaHead = new ItemTemplate();
				queenVuunaHead.Name = "Queen Vuuna's Head";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + queenVuunaHead.Name + " , creating it ...");

				queenVuunaHead.Weight = 15;
				queenVuunaHead.Model = 503;

				queenVuunaHead.Object_Type = (int) eObjectType.GenericItem;

				queenVuunaHead.Id_nb = "queen_vuuna_head";
				queenVuunaHead.IsPickable = true;
				queenVuunaHead.IsDropable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(queenVuunaHead);
			}

			// item db check
			recruitsGauntlets = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "recruits_studded_gauntlets_mid");
			if (recruitsGauntlets == null)
			{
				recruitsGauntlets = new ItemTemplate();
				recruitsGauntlets.Name = "Recruit's Studded Gauntles (Mid)";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsGauntlets.Name + ", creating it ...");
				recruitsGauntlets.Level = 8;

				recruitsGauntlets.Weight = 24;
				recruitsGauntlets.Model = 80;

				recruitsGauntlets.DPS_AF = 14; // Armour
				recruitsGauntlets.SPD_ABS = 19; // Absorption

				recruitsGauntlets.Object_Type = (int) eObjectType.Studded;
				recruitsGauntlets.Item_Type = (int) eEquipmentItems.HAND;
				recruitsGauntlets.Id_nb = "recruits_studded_gauntlets_mid";
				recruitsGauntlets.Gold = 0;
				recruitsGauntlets.Silver = 9;
				recruitsGauntlets.Copper = 0;
				recruitsGauntlets.IsPickable = true;
				recruitsGauntlets.IsDropable = true;
				recruitsGauntlets.Color = 36; // blue leather

				recruitsGauntlets.Bonus = 5; // default bonus

				recruitsGauntlets.Bonus1 = 4;
				recruitsGauntlets.Bonus1Type = (int) eStat.STR;

				recruitsGauntlets.Bonus2 = 3;
				recruitsGauntlets.Bonus2Type = (int) eStat.DEX;

				recruitsGauntlets.Quality = 100;
				recruitsGauntlets.MaxQuality = 100;
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
				recruitsGloves.Color = 36; // red cloth

				recruitsGloves.Bonus = 5; // default bonus

				recruitsGloves.Bonus1 = 4;
				recruitsGloves.Bonus1Type = (int) eStat.INT;

				recruitsGloves.Bonus2 = 3;
				recruitsGloves.Bonus2Type = (int) eStat.DEX;

				recruitsGloves.Quality = 100;
				recruitsGloves.MaxQuality = 100;
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

			recruitsJewel = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "recruits_tarnished_bauble");
			if (recruitsJewel == null)
			{
				recruitsJewel = new ItemTemplate();
				recruitsJewel.Name = "Recruit's Tarnished Bauble";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsJewel.Name + ", creating it ...");
				recruitsJewel.Level = 7;

				recruitsJewel.Weight = 2;
				recruitsJewel.Model = 110;

				recruitsJewel.Object_Type = (int) eObjectType.Magical;
				recruitsJewel.Item_Type = (int) eEquipmentItems.JEWEL;
				recruitsJewel.Id_nb = "recruits_tarnished_bauble";
				recruitsJewel.Gold = 0;
				recruitsJewel.Silver = 9;
				recruitsJewel.Copper = 0;
				recruitsJewel.IsPickable = true;
				recruitsJewel.IsDropable = true;

				recruitsJewel.Bonus = 5; // default bonus

				recruitsJewel.Bonus1 = 4;
				recruitsJewel.Bonus1Type = (int) eStat.CON;

				recruitsJewel.Bonus2 = 12;
				recruitsJewel.Bonus2Type = (int) eProperty.MaxHealth;

				recruitsJewel.Quality = 100;
				recruitsJewel.MaxQuality = 100;
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
				recruitsJewelCloth.MaxQuality = 100;
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
				recruitsBracer.MaxQuality = 100;
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
			//We want to be notified whenever a player enters the world            
			GameEventMgr.AddHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));

			GameEventMgr.AddHandler(dalikor, GameLivingEvent.Interact, new DOLEventHandler(TalkToDalikor));
			GameEventMgr.AddHandler(dalikor, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToDalikor));

			GameEventMgr.AddHandler(queenVuuna, GameNPCEvent.OnAICallback, new DOLEventHandler(CheckNearQueenVuuna));

			/* Now we bring to Dalikor the possibility to give this quest to players */
			dalikor.AddQuestToGive(typeof (Culmination));

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

			GameEventMgr.RemoveHandler(queenVuuna, GameNPCEvent.OnAICallback, new DOLEventHandler(CheckNearQueenVuuna));

			/* Now we remove to dalikor the possibility to give this quest to players */
			dalikor.RemoveQuestToGive(typeof (Culmination));
		}

		protected static void CheckNearQueenVuuna(DOLEvent e, object sender, EventArgs args)
		{
			GameMob queenTatiana = (GameMob) sender;

			// if princess is dead no ned to checks ...
			if (!queenTatiana.Alive || queenTatiana.ObjectState != GameObject.eObjectState.Active)
				return;

			foreach (GamePlayer player in queenTatiana.GetPlayersInRadius(1000))
			{
				Culmination quest = (Culmination) player.IsDoingQuest(typeof (Culmination));

				if (quest != null && !quest.queenVuunaAttackStarted && quest.Step == 2)
				{
					quest.queenVuunaAttackStarted = true;

					SendSystemMessage(player, "There they are. You take care of the queen I'll deal with the fairy sorcesses littleone.");
					IAggressiveBrain aggroBrain = queenTatiana.Brain as IAggressiveBrain;
					if (aggroBrain != null)
						aggroBrain.AddToAggroList(player, 70);

					for (int i = 0; i < askefruerSorceress.Length; i++)
					{
						if (quest.recruits[i] != null)
						{
							aggroBrain = quest.recruits[i].Brain as IAggressiveBrain;
							if (aggroBrain != null)
								aggroBrain.AddToAggroList(askefruerSorceress[i], 50);
						}

						aggroBrain = askefruerSorceress[i].Brain as IAggressiveBrain;
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

		protected static void TalkToDalikor(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(dalikor.CanGiveQuest(typeof (Culmination), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Culmination quest = player.IsDoingQuest(typeof (Culmination)) as Culmination;

			dalikor.TurnTo(player);

			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...
					dalikor.SayTo(player, "Hello again Eeinken. No doubt you have seen the Askefruer village northeast of here. They have been amassing as many Askfruer as they can, calling them from all over Midgard. We are dealing with a very large [problem].");
					return;
				}
				else
				{
					if (quest.Step == 1)
					{
						dalikor.SayTo(player, "Alright my friend. Here is what you must do. You must travel northeast from this tower towards the road to Haggerfel. Follow the road a little ways. You will see the Askefruer village on your right. Master Briedi is waiting for you there. Good luck.");

					}
					else if (quest.Step == 4)
					{
						dalikor.SayTo(player, "Welcome back Eeinken. Where is Master Briedi? Don'T tell me he [returned to Gotar] again without visiting his old friend?");
					}
					else if (quest.Step == 5)
					{
						dalikor.SayTo(player, "I see. Excellent work recruit. Now, I have a [reward] for you.");
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
						case "problem":
							dalikor.SayTo(player, "I have already contacted Master Briedi and a few other former recruits to help with the situation. There are many Askefruer, but the task of slaying their [queen] has fallen to you.");
							break;
						case "queen":
							dalikor.SayTo(player, "This is a very important task I have set before you recruit Eeinken. Will you [take on] this burden and help Mularn?");
							break;
							//If the player offered his "help", we send the quest dialog now!
						case "take on":
							player.Out.SendCustomDialog("Are you ready to take part in this monumental battle for the good of Mularn?", new CustomDialogResponse(CheckPlayerAcceptQuest));
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "returned to Gotar":
							dalikor.SayTo(player, "Ah, yes. I thought he would. He likes it there, for some reason. I'm hoping that this Askefruer threat is no more so I may go to visit with him. Tell me recruit, were you able to slay the queen?");
							break;

						case "reward":
							dalikor.SayTo(player, "The Council of Elders has authorized me to give you these two items as compensation for all the work you have done for us in this matter. I believe that now Eeinken, you have gone beyond my skills as a trainer. Always remember your training, it will come in more useful than you know. Now, return to your trainer, I'm sure they are anxious to assist you as your make your way in Midgard.");
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

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToMasterBriedi(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(dalikor.CanGiveQuest(typeof (Culmination), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Culmination quest = player.IsDoingQuest(typeof (Culmination)) as Culmination;

			if (e == GameLivingEvent.Interact)
			{
				if (quest != null && quest.briediClone != null && quest.briediClone == sender)
				{
					quest.briediClone.TurnTo(player);

					if (quest.Step <= 1)
					{
						quest.briediClone.SayTo(player, "Ah, Eeinken, the one who always scares me. Yes, quite a pickle we're in this time, don't you agree? But don't worry, I have a [plan].");
					}
					else if (quest.Step == 2)
					{
						quest.briediClone.SayTo(player, "Go now and kill their queen, so that Mularn is at ease.");
						foreach (GameMob recruit in quest.recruits)
						{
							recruit.Follow(player, 50 + Util.Random(100), 4000);
						}
					}
					else if (quest.Step == 3)
					{
						quest.briediClone.SayTo(player, "Good fight Eeinken, good fight. Now that my work here is done, these fine recruits and I shall be heading back to Gotar where they can get some more training. Please tell Dalikor I returned to Gotar won't you? Be safe!");
						quest.ResetMasterBriedi();
						quest.Step = 4;
						if (player.PlayerGroup != null)
						{
							foreach (GamePlayer groupMember in player.PlayerGroup.GetPlayersInTheGroup())
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
				if (quest != null && quest.briediClone == sender)
				{
					quest.briediClone.TurnTo(player);
					//Do some small talk :)
					switch (wArgs.Text)
					{
						case "plan":
							quest.briediClone.SayTo(player, "Remember last time, with Princess Aiyr? Surely you remember, you can't be that old, can you? These fresh recruits and I will stave off the army so you can concentrate on the Queen. [Got that]?");
							break;

						case "Got that":
							quest.briediClone.SayTo(player, "Good. Now, wait for her, she is tricky, but big, you should be able to spot her easily. Remember, let the recruits and I take care of the rest of them.");
							if (quest.Step == 1)
							{
								quest.briediClone.SayTo(player, "Attack!");
								quest.Step = 2;
								if (player.PlayerGroup != null)
								{
									foreach (GamePlayer groupMember in player.PlayerGroup.GetPlayersInTheGroup())
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

							foreach (GameMob recruit in quest.recruits)
							{
								recruit.Follow(player, 50 + Util.Random(100), 4000);
							}
							break;
					}
				}
			}
		}

		protected virtual void ResetMasterBriedi()
		{
			if (briediClone != null && (briediClone.Alive || briediClone.ObjectState == GameObject.eObjectState.Active))
			{
				m_animSpellObjectQueue.Enqueue(briediClone);
				m_animSpellTeleportTimerQueue.Enqueue(new RegionTimer(briediClone, new RegionTimerCallback(MakeAnimSpellSequence), 4000));

				m_animEmoteObjectQueue.Enqueue(briediClone);
				m_animEmoteTeleportTimerQueue.Enqueue(new RegionTimer(briediClone, new RegionTimerCallback(MakeAnimEmoteSequence), 5000));

				for (int i = 0; i < recruits.Length; i++)
				{
					if (recruits[i] != null)
					{
						m_animEmoteObjectQueue.Enqueue(recruits[i]);
						m_animEmoteTeleportTimerQueue.Enqueue(new RegionTimer(recruits[i], new RegionTimerCallback(MakeAnimEmoteSequence), 4500));
					}
				}
			}

			if (briediClone != null)
			{
				RegionTimer castTimer = new RegionTimer(briediClone, new RegionTimerCallback(DeleteBriediClone), 6000);
			}
		}

		protected virtual int DeleteBriediClone(RegionTimer callingTimer)
		{
			if (briediClone != null)
			{
				briediClone.Delete();
				GameEventMgr.RemoveHandler(briediClone, GameLivingEvent.Interact, new DOLEventHandler(TalkToMasterBriedi));
				GameEventMgr.RemoveHandler(briediClone, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterBriedi));
			}

			for (int i = 0; i < recruits.Length; i++)
			{
				if (recruits[i] != null)
					recruits[i].Delete();
			}

			return 0;
		}

		protected void CreateBriediClone()
		{
			GameNpcInventoryTemplate template;
			if (briediClone == null)
			{
				briediClone = new GameMob();
				briediClone.Model = 157;
				briediClone.Name = "Master Briedi";
				briediClone.GuildName = "Part of " + questTitle + " Quest";
				briediClone.Realm = (byte) eRealm.Midgard;
				briediClone.CurrentRegionID = 100;

				briediClone.Size = 50;
				briediClone.Level = 45;
				briediClone.X = GameLocation.ConvertLocalXToGlobalX(45394, 100);
				briediClone.Y = GameLocation.ConvertLocalYToGlobalY(39768, 100);
				briediClone.Z = 4709;
				briediClone.Heading = 107;

				template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 348);
				template.AddNPCEquipment(eInventorySlot.LegsArmor, 349);
				template.AddNPCEquipment(eInventorySlot.ArmsArmor, 350);
				template.AddNPCEquipment(eInventorySlot.HandsArmor, 351);
				template.AddNPCEquipment(eInventorySlot.FeetArmor, 352);
				template.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 640);
				briediClone.Inventory = template.CloseTemplate();
				briediClone.SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);

//				briediClone.AddNPCEquipment((byte) eEquipmentItems.TORSO, 348, 0, 0, 0);
//				briediClone.AddNPCEquipment((byte) eEquipmentItems.LEGS, 349, 0, 0, 0);
//				briediClone.AddNPCEquipment((byte) eEquipmentItems.ARMS, 350, 0, 0, 0);
//				briediClone.AddNPCEquipment((byte) eEquipmentItems.HAND, 351, 0, 0, 0);
//				briediClone.AddNPCEquipment((byte) eEquipmentItems.FEET, 352, 0, 0, 0);
//				briediClone.AddNPCEquipment((byte) eEquipmentItems.TWO_HANDED, 640, 0, 0, 0);

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 0;
				briediClone.SetOwnBrain(brain);

				briediClone.AddToWorld();

				GameEventMgr.AddHandler(briediClone, GameLivingEvent.Interact, new DOLEventHandler(TalkToMasterBriedi));
				GameEventMgr.AddHandler(briediClone, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterBriedi));
			}
			else
			{
				briediClone.MoveTo(100, GameLocation.ConvertLocalXToGlobalX(45394, 100), GameLocation.ConvertLocalYToGlobalY(39768, 100), 4709, 107);
			}


			foreach (GamePlayer visPlayer in briediClone.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				visPlayer.Out.SendEmoteAnimation(briediClone, eEmote.Bind);
			}


			for (int i = 0; i < recruits.Length; i++)
			{
				recruits[i] = new GameMob();

				recruits[i].Name = "Recruit";

				recruits[i].GuildName = "Part of " + questTitle + " Quest";
				recruits[i].Realm = (byte) eRealm.Midgard;
				recruits[i].CurrentRegionID = briediClone.CurrentRegionID;

				recruits[i].Size = 50;
				recruits[i].Level = 6;
				recruits[i].X = briediClone.X + Util.Random(-150, 150);
				recruits[i].Y = briediClone.Y + Util.Random(-150, 150);

				recruits[i].Z = briediClone.Z;
				recruits[i].Heading = 187;

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 0;
				recruits[i].SetOwnBrain(brain);

			}

			recruits[0].Name = "Recruit Hietan";
			recruits[0].Model = 189;
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

			recruits[1].Name = "Recruit Iduki";
			recruits[1].Model = 190;
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

			recruits[2].Name = "Recruit Odigi";
			recruits[2].Model = 774;
			template = new GameNpcInventoryTemplate();
			template.AddNPCEquipment(eInventorySlot.RightHandWeapon, 4);
			template.AddNPCEquipment(eInventorySlot.TorsoArmor, 36);
			template.AddNPCEquipment(eInventorySlot.LegsArmor, 37);
			recruits[2].Inventory = template.CloseTemplate();
			recruits[2].SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

//			recruits[2].AddNPCEquipment((byte) eEquipmentItems.RIGHT_HAND, 4, 0, 0, 0);
//			recruits[2].AddNPCEquipment((byte) eEquipmentItems.TORSO, 36, 0, 0, 0);
//			recruits[2].AddNPCEquipment((byte) eEquipmentItems.LEGS, 37, 0, 0, 0);

			recruits[3].Name = "Recruit Thulder";
			recruits[3].Model = 775;
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

				quest.ResetMasterBriedi();
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
					quest.CreateBriediClone();
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
			if(dalikor.CanGiveQuest(typeof (Culmination), player)  <= 0)
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
				if (!dalikor.GiveQuest(typeof (Culmination), player, 1))
					return;

				GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));

				dalikor.SayTo(player, "Alright my friend. Here is what you must do. You must travel northeast from this tower towards the road to Haggerfel. Follow the road a little ways. You will see the Askefruer village on your right. Master Briedi is waiting for you there. Good luck.");


				bool briediCloneCreated = false;
				if (player.PlayerGroup != null)
				{
					foreach (GamePlayer groupMember in player.PlayerGroup.GetPlayersInTheGroup())
					{
						Culmination memberQuest = groupMember.IsDoingQuest(typeof (Culmination)) as Culmination;
						// we found another groupmember doing the same quest...
						if (memberQuest != null && memberQuest.briediClone != null)
						{
							briediCloneCreated = true;
							break;
						}
					}
				}

				if (!briediCloneCreated)
				{
					quest = player.IsDoingQuest(typeof (Culmination)) as Culmination;
					if(quest != null) quest.CreateBriediClone();
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
						return "[Step #1] Make your way northeast from Dalikor and take the road towards Haggerfel. You will see Master Briedi and his troups off to the right of the road.";
					case 2:
						return "[Step #2] Find and kill Queen Vuuna.";
					case 3:
						return "[Step #3] Speak with Master Briedi when the fighting is over.";
					case 4:
						return "[Step #4] Return to Dalikor at the guard town near Mularn. Hand him the head of Queen Vuuna as evidence that the Askefuer threat has now been extinguished. Tell him Master Briedi has [returned to Gotar].";
					case 5:
						return "[Step #5] Wait for Dalikor to reward you. If he stops speaking with you, ask about a [reward] for your time and effort.";
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

				if (gArgs.Target.Name == queenVuuna.Name)
				{
					SendSystemMessage("You slay the queen and take her head as proof.");
					GiveItem(gArgs.Target, player, queenVuunaHead);
					Step = 3;
					return;
				}
			}

			if (Step == 4 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == dalikor.Name && gArgs.Item.Id_nb == queenVuunaHead.Id_nb)
				{
					dalikor.SayTo(player, "I see. Excellent work recruit. Now, I have a [reward] for you.");
					RemoveItem(dalikor, player, queenVuunaHead);
					Step = 5;
					return;
				}
			}
		}

		public override void AbortQuest()
		{
			ResetMasterBriedi();

			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, queenVuunaHead, false);

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			// make sure to clean up, should be needed , but just to make certain
			ResetMasterBriedi();
			//Give reward to player here ...              
			m_questPlayer.GainExperience(1012, 0, 0, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 9, Util.Random(50)), "You recieve {0} as a reward.");
			if (m_questPlayer.HasAbilityToUseItem(recruitsGauntlets))
			{
				GiveItem(dalikor, m_questPlayer, recruitsGauntlets);
				GiveItem(dalikor, m_questPlayer, recruitsJewel);
			}
			else
			{
				GiveItem(dalikor, m_questPlayer, recruitsGloves);
				GiveItem(dalikor, m_questPlayer, recruitsJewelCloth);
			}
			GiveItem(dalikor, m_questPlayer, recruitsBracer);

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
		}

	}
}
