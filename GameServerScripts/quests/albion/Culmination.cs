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
using NHibernate.Expression;
using NHibernate.Mapping.Attributes;
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
    public class CulminationDescriptor : AbstractQuestDescriptor
    {
        /* This is the type of the quest class linked with 
         * this requirement class, you must override the 
         * base methid like that
         */
        public override Type LinkedQuestType
        {
            get { return typeof(Culmination); }
        }

        /* This value is used to retrieves the minimum level needed
         *  to be able to make this quest. Override it only if you need, 
         * the default value is 1
         */
        public override int MinLevel
        {
            get { return 5; }
        }

        /* This value is used to retrieves how maximum level needed
         * to be able to make this quest. Override it only if you need, 
         * the default value is 50
         */
        public override int MaxLevel
        {
            get { return 10; }
        }

        public override bool CheckQuestQualification(GamePlayer player)
        {
			// if the player is already doing the quest always return true !!!
			if (player.IsDoingQuest(LinkedQuestType) != null)
				return true;

            // This checks below are only performed is player isn't doing quest already
            if (!BaseFrederickQuest.CheckPartAccessible(player, typeof(Culmination)))
                return false;

            return base.CheckQuestQualification(player);
        }
    }

    /* The second thing we do, is to declare the class we create
     * as Quest. We must make it persistant using attributes, to
     * do this, we derive from the abstract class AbstractQuest
     */
    [Subclass(NameType = typeof(Culmination), ExtendsType = typeof(AbstractQuest))]
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

		private static GameMob masterFrederick = null;

		private GameMob dunwynClone = null;

		private GameMob[] recruits = new GameMob[4];

		private static GameMob queenTatiana = null;
		private static GameMob[] fairySorceress = new GameMob[4];

		private static GenericItemTemplate queenTatianasHead = null;

		private static HandsArmorTemplate recruitsGauntlets = null;
		private static HandsArmorTemplate recruitsGloves = null;
		private static JewelTemplate recruitsJewel = null;
		private static JewelTemplate recruitsJewelCloth = null;

		private static BracerTemplate recruitsBracer = null;


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

			queenTatiana = ResearchQuestObject(typeof(GameMob), WorldMgr.GetRegion(1), eRealm.Albion, "Queen Tatiana") as GameMob;
			if (queenTatiana == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Queen Tatiana, creating ...");
				queenTatiana = new GameMob();

				queenTatiana.Name = "Queen Tatiana";
				queenTatiana.Position = new Point(558500, 533042, 2573);
				queenTatiana.Heading = 174;
				queenTatiana.Model = 603;
				queenTatiana.GuildName = "Part of " + questTitle + " Quest";
				queenTatiana.Realm = (byte) eRealm.None;
				queenTatiana.Region = WorldMgr.GetRegion(1);
				queenTatiana.Size = 49;
				queenTatiana.Level = 5;

				queenTatiana.RespawnInterval = -1; //auto respawn

				StandardMobBrain brain = new StandardMobBrain();
				brain.Body = queenTatiana;
				brain.AggroLevel = 50;
				brain.AggroRange = 600;
				queenTatiana.OwnBrain = brain;

				if(!queenTatiana.AddToWorld())
				{
					if (log.IsWarnEnabled)
						log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
					return;
				}

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(queenTatiana);
			}

			int counter = 0;
			foreach (GameMob mob in queenTatiana.GetInRadius(typeof(GameMob), 500))
			{
				if (mob.Name == "ire fairy sorceress")
				{
					fairySorceress[counter] = mob;
					counter++;
				}
				if (counter == fairySorceress.Length) break;
			}

			for (int i = 0; i < fairySorceress.Length; i++)
			{
				if (fairySorceress[i] == null)
				{
					if (log.IsWarnEnabled)
						log.Warn("Could not find ire fairy sorceress, creating ...");
					fairySorceress[i] = new GameMob();
					fairySorceress[i].Model = 603; // //819;
					fairySorceress[i].Name = "ire fairy sorceress";
					fairySorceress[i].GuildName = "Part of " + questTitle + " Quest";
					fairySorceress[i].Realm = (byte) eRealm.None;
					fairySorceress[i].Region = WorldMgr.GetRegion(1);
					fairySorceress[i].Size = 35;
					fairySorceress[i].Level = 5;
					Point pos = queenTatiana.Position;
					pos.X += Util.Random(30, 150);
					pos.Y += Util.Random(30, 150);
					fairySorceress[i].Position = pos;
					fairySorceress[i].Heading = 93;

					fairySorceress[i].RespawnInterval = -1; //aurespawn

					StandardMobBrain brain = new StandardMobBrain();
					brain.Body = fairySorceress[i];
					brain.AggroLevel = 50;
					brain.AggroRange = 600;
					fairySorceress[i].OwnBrain = brain;

					
					
					if(!fairySorceress[i].AddToWorld())
					{
						if (log.IsWarnEnabled)
							log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
						return;
					}

					//You don't have to store the created mob in the db if you don't want,
					//it will be recreated each time it is not found, just comment the following
					//line if you rather not modify your database
					if (SAVE_INTO_DATABASE)
						GameServer.Database.AddNewObject(fairySorceress[i]);
				}
			}

			#endregion

			#region defineItems

			queenTatianasHead = GameServer.Database.SelectObject(typeof (GenericItemTemplate), Expression.Eq("Name", "Queen Tatiana's Head")) as GenericItemTemplate;
			if (queenTatianasHead == null)
			{
				queenTatianasHead = new GenericItemTemplate();
				queenTatianasHead.Name = "Queen Tatiana's Head";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + queenTatianasHead.Name + " , creating it ...");

				queenTatianasHead.Weight = 15;
				queenTatianasHead.Model = 503;

				queenTatianasHead.IsDropable = false;
                queenTatianasHead.IsSaleable = false;
                queenTatianasHead.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(queenTatianasHead);
			}

			// item db check
			recruitsGauntlets = GameServer.Database.SelectObject(typeof (HandsArmorTemplate), Expression.Eq("Name", "Recruit's Studded Gauntles")) as HandsArmorTemplate;
			if (recruitsGauntlets == null)
			{
				recruitsGauntlets = new HandsArmorTemplate();
				recruitsGauntlets.Name = "Recruit's Studded Gauntles";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsGauntlets.Name + ", creating it ...");
				recruitsGauntlets.Level = 8;

				recruitsGauntlets.Weight = 24;
				recruitsGauntlets.Model = 80;

                recruitsGauntlets.ArmorFactor = 14;
                recruitsGauntlets.ArmorLevel = eArmorLevel.Medium;

				recruitsGauntlets.Value = 900;

                recruitsGauntlets.IsDropable = true;
                recruitsGauntlets.IsSaleable = true;
                recruitsGauntlets.IsTradable = true;
				recruitsGauntlets.Color = 9; // red leather

				recruitsGauntlets.Bonus = 5; // default bonus

                recruitsGauntlets.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 4));
                recruitsGauntlets.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 3));

				recruitsGauntlets.Quality = 100;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsGauntlets);
			}

			// item db check
			recruitsGloves = GameServer.Database.SelectObject(typeof (HandsArmorTemplate), Expression.Eq("Name", "Recruit's Quilted Gloves")) as HandsArmorTemplate;
			if (recruitsGloves == null)
			{
				recruitsGloves = new HandsArmorTemplate();
				recruitsGloves.Name = "Recruit's Quilted Gloves";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsGloves.Name + ", creating it ...");
				recruitsGloves.Level = 8;

				recruitsGloves.Weight = 8;
				recruitsGloves.Model = 154;

                recruitsGloves.ArmorFactor = 7;
                recruitsGloves.ArmorLevel = eArmorLevel.VeryLow;

                recruitsGloves.Value = 900;

				recruitsGloves.IsDropable = true;
                recruitsGloves.IsSaleable = true;
                recruitsGloves.IsTradable = true;

				recruitsGloves.Color = 27; // red leather

				recruitsGloves.Bonus = 5; // default bonus

                recruitsGloves.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 4));
                recruitsGloves.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 3));

				recruitsGloves.Quality = 100;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsGloves);
			}

			recruitsJewel = GameServer.Database.SelectObject(typeof (JewelTemplate), Expression.Eq("Name", "Recruit's Cloudy Jewel")) as JewelTemplate;
			if (recruitsJewel == null)
			{
				recruitsJewel = new JewelTemplate();
				recruitsJewel.Name = "Recruit's Cloudy Jewel";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsJewel.Name + ", creating it ...");
				recruitsJewel.Level = 7;

				recruitsJewel.Weight = 2;
				recruitsJewel.Model = 110;

                recruitsJewel.Value = 900;

				recruitsJewel.IsDropable = true;
                recruitsJewel.IsSaleable = true;
                recruitsJewel.IsTradable = true;

				recruitsJewel.Bonus = 5; // default bonus

                recruitsJewel.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 6));

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsJewel);
			}

			recruitsJewelCloth = GameServer.Database.SelectObject(typeof (JewelTemplate), Expression.Eq("Name", "Recruit's Cloudy Jewel")) as JewelTemplate;
			if (recruitsJewelCloth == null)
			{
				recruitsJewelCloth = new JewelTemplate();
				recruitsJewelCloth.Name = "Recruit's Cloudy Jewel";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsJewelCloth.Name + ", creating it ...");
				recruitsJewelCloth.Level = 7;

				recruitsJewelCloth.Weight = 2;
				recruitsJewelCloth.Model = 110;

				recruitsJewelCloth.Value = 900;
                recruitsJewelCloth.IsDropable = true;
                recruitsJewelCloth.IsSaleable = true;
                recruitsJewelCloth.IsTradable = true;

				recruitsJewelCloth.Bonus = 5; // default bonus

                recruitsJewelCloth.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 4));
                recruitsJewelCloth.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 3));
                recruitsJewelCloth.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 1));

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsJewelCloth);
			}

			recruitsBracer = GameServer.Database.SelectObject(typeof (BracerTemplate), Expression.Eq("Name", "Recruit's Golden Bracer")) as BracerTemplate;
			if (recruitsBracer == null)
			{
				recruitsBracer = new BracerTemplate();
				recruitsBracer.Name = "Recruit's Golden Bracer";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsBracer.Name + ", creating it ...");
				recruitsBracer.Level = 7;

				recruitsBracer.Weight = 2;
				recruitsBracer.Model = 121;

				 recruitsBracer.Value = 900;

				recruitsBracer.IsDropable = true;
                recruitsBracer.IsSaleable = true;
                recruitsBracer.IsTradable = true;

				recruitsBracer.Bonus = 5; // default bonus

                recruitsBracer.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 4));
                recruitsBracer.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 3));
                recruitsBracer.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 1));

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

			GameEventMgr.AddHandler(masterFrederick, GameObjectEvent.Interact, new DOLEventHandler(TalkToMasterFrederick));
			GameEventMgr.AddHandler(masterFrederick, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterFrederick));

			GameEventMgr.AddHandler(queenTatiana, GameNPCEvent.OnAICallback, new DOLEventHandler(CheckNearQueenTatiana));

			/* Now we add some hooks to trigger the quest dialog reponse. */
			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogResponse));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogResponse));

			/* Now we bring to masterFrederick the possibility to give this quest to players */
            QuestMgr.AddQuestDescriptor(masterFrederick, typeof(CulminationDescriptor));

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

			GameEventMgr.RemoveHandler(masterFrederick, GameObjectEvent.Interact, new DOLEventHandler(TalkToMasterFrederick));
			GameEventMgr.RemoveHandler(masterFrederick, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterFrederick));

			GameEventMgr.RemoveHandler(queenTatiana, GameNPCEvent.OnAICallback, new DOLEventHandler(CheckNearQueenTatiana));

			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogResponse));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogResponse));

			/* Now we remove to masterFrederick the possibility to give this quest to players */
            QuestMgr.RemoveQuestDescriptor(masterFrederick, typeof(CulminationDescriptor));
		}

		protected static void CheckNearQueenTatiana(DOLEvent e, object sender, EventArgs args)
		{
			GameMob queenTatiana = (GameMob) sender;

			// if princess is dead no ned to checks ...
			if (!queenTatiana.Alive || queenTatiana.ObjectState != eObjectState.Active)
				return;

			foreach (GamePlayer player in queenTatiana.GetInRadius(typeof(GamePlayer), 1000))
			{
				Culmination quest = (Culmination) player.IsDoingQuest(typeof (Culmination));

				if (quest != null && !queenTatiana.AttackState && quest.Step == 2)
				{
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

            if (QuestMgr.CanGiveQuest(typeof(Culmination), player, masterFrederick) <= 0)
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
						masterFrederick.SayTo(player, "You've returned "+player.Name+". That can only mean that you were successful in your battle with the fairies! Please, show me whatever proof you have that the fairies are finally gone.");
					}
					else if (quest.Step == 5)
					{
						masterFrederick.SayTo(player, "Wonderful! Now I know Cotswold will be safe, thanks in no small part to you, Recruit "+player.Name+". Excellent work. Cotswold is forever in your debt. I have a [reward] for you. I hope you have some use for it.");
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
							QuestMgr.ProposeQuestToPlayer(typeof(Culmination), "Are you ready to take part in this monumental battle for the good of Cotswold?", player, masterFrederick);
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "reward":
							masterFrederick.SayTo(player, "Here are a few things to help you start off your life as a great adventurer. Be safe and well "+player.Name+". You have now grown beyond my teachings. If you wish to continue questings, speak with the Town Criers and with your trainer. Cotswold, and I, thank you again for your assistance.");
							if (quest.Step == 5)
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

		protected static void TalkToMasterDunwyn(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

            if (QuestMgr.CanGiveQuest(typeof(Culmination), player, masterFrederick) <= 0)
				return;

			//We also check if the player is already doing the quest
			Culmination quest = player.IsDoingQuest(typeof (Culmination)) as Culmination;

			if (e == GameObjectEvent.Interact)
			{
				if (quest != null && quest.dunwynClone != null && quest.dunwynClone == sender)
				{
					quest.dunwynClone.TurnTo(player);

					if (quest.Step <= 1)
					{
						quest.dunwynClone.SayTo(player, "What? Who's that? Can't you leave an old man in peace?");
						quest.dunwynClone.SayTo(player, ""+player.Name+", how good to see you again. I see that the fairy problem is slightly [larger] than when I left.");
					}
					else if (quest.Step == 2)
					{
						quest.dunwynClone.SayTo(player, "Go now and kill their queen, so that Cotswold is at ease.");
						foreach (GameMob recruit in quest.recruits)
						{
							recruit.Follow(player, 50 + Util.Random(100), 4000);
						}
					}
					else if (quest.Step == 3)
					{
						quest.dunwynClone.SayTo(player, "Good job again "+player.Name+". Now, I will be taking these recruits back to Avalon Marsh with me. There is much to do there. Good luck to you "+player.Name+". I wish you well in your future endeavors.");
						quest.ResetMasterDunwyn();
						quest.ChangeQuestStep(4);
						if (player.PlayerGroup != null)
						{
							foreach (GamePlayer groupMember in player.PlayerGroup.GetPlayersInTheGroup())
							{
								Culmination memberQuest = groupMember.IsDoingQuest(typeof (Culmination)) as Culmination;
								// we found another groupmember doing the same quest...
								if (memberQuest != null && memberQuest.Step == 3)
								{
									memberQuest.ChangeQuestStep(4);
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
							quest.dunwynClone.SayTo(player, "Surely you haven't already forgotten about your great battle with Obera, have you? You're far too young to be forgetting things "+player.Name+", but I digress. You must make your way into the camp and slay [Queen Tatiana].");
							break;
						case "Queen Tatiana":
							quest.dunwynClone.SayTo(player, "She is easy enough to spot, for her colors differ from the other fairies around her. Good luck "+player.Name+".");
							if (quest.Step == 1)
							{
								quest.ChangeQuestStep(2);
								if (player.PlayerGroup != null)
								{
									foreach (GamePlayer groupMember in player.PlayerGroup.GetPlayersInTheGroup())
									{
										Culmination memberQuest = groupMember.IsDoingQuest(typeof (Culmination)) as Culmination;
										// we found another groupmember doing the same quest...
										if (memberQuest != null && memberQuest.Step == 1)
										{
											memberQuest.ChangeQuestStep(2);
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

		protected virtual void ResetMasterDunwyn()
		{
			if (dunwynClone != null && (dunwynClone.Alive || dunwynClone.ObjectState == eObjectState.Active))
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
				dunwynClone.RemoveFromWorld();
				GameEventMgr.RemoveHandler(dunwynClone, GameObjectEvent.Interact, new DOLEventHandler(TalkToMasterDunwyn));
				GameEventMgr.RemoveHandler(dunwynClone, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterDunwyn));
			}

			for (int i = 0; i < recruits.Length; i++)
			{
				if (recruits[i] != null)
					recruits[i].RemoveFromWorld();
			}

			return 0;
		}

		protected void CreateDunwynClone()
		{
			GameNpcInventory template;
            Zone zone = WorldMgr.GetRegion(1).GetZone(1);
			if (dunwynClone == null)
			{
				dunwynClone = new GameMob();
				dunwynClone.Name = "Master Dunwyn";
				dunwynClone.Model = 9;
				dunwynClone.GuildName = "Part of " + questTitle + " Quest";
				dunwynClone.Realm = (byte) eRealm.Albion;
				dunwynClone.Region = WorldMgr.GetRegion(1);
				dunwynClone.Size = 50;
				dunwynClone.Level = 14;

				dunwynClone.Position = zone.ToRegionPosition(
					new Point(
						8602 + Util.Random(-150, 150),
						47193 + Util.Random(-150, 150),
						2409));
				dunwynClone.Heading = 342;

				template = new GameNpcInventory();
				template.AddItem(eInventorySlot.TorsoArmor, new NPCArmor(798));
				template.AddItem(eInventorySlot.RightHandWeapon, new NPCWeapon(19));
				dunwynClone.Inventory = template;
				dunwynClone.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				StandardMobBrain brain = new StandardMobBrain();
				brain.Body = dunwynClone;
				brain.AggroLevel = 0;
				brain.AggroRange = 0;
				dunwynClone.OwnBrain = brain;

				dunwynClone.AddToWorld();

				GameEventMgr.AddHandler(dunwynClone, GameObjectEvent.Interact, new DOLEventHandler(TalkToMasterDunwyn));
				GameEventMgr.AddHandler(dunwynClone, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterDunwyn));
			}
			else
			{
				dunwynClone.MoveTo(WorldMgr.GetRegion(1), new Point(567604, 509619, 2813), 3292);
			}


			foreach (GamePlayer visPlayer in dunwynClone.GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
			{
				visPlayer.Out.SendEmoteAnimation(dunwynClone, eEmote.Bind);
			}


			for (int i = 0; i < recruits.Length; i++)
			{
				recruits[i] = new GameMob();

				recruits[i].Name = "Recruit";

				recruits[i].GuildName = "Part of " + questTitle + " Quest";
				recruits[i].Realm = (byte) eRealm.Albion;
				recruits[i].Region = WorldMgr.GetRegion(1);

				recruits[i].Size = 50;
				recruits[i].Level = 6;
				recruits[i].Position = zone.ToRegionPosition(
					new Point(
						8602 + Util.Random(-150, 150),
						47193 + Util.Random(-150, 150),
						2409));
				recruits[i].Heading = 187;

				StandardMobBrain brain = new StandardMobBrain();
				brain.Body = recruits[i];
				brain.AggroLevel = 0;
				brain.AggroRange = 0;
				recruits[i].OwnBrain = brain;
			}

			recruits[0].Name = "Recruit Armsman McTavish";
			recruits[0].Model = 40;
			template = new GameNpcInventory();
			template.AddItem(eInventorySlot.TwoHandWeapon, new NPCWeapon(69));
			template.AddItem(eInventorySlot.TorsoArmor, new NPCArmor(46));
			template.AddItem(eInventorySlot.LegsArmor, new NPCArmor(47));
			template.AddItem(eInventorySlot.FeetArmor, new NPCArmor(50));
			template.AddItem(eInventorySlot.ArmsArmor, new NPCArmor(48));
			template.AddItem(eInventorySlot.HandsArmor, new NPCArmor(49));
			recruits[0].Inventory = template;
			recruits[0].SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);

			recruits[1].Name = "Recruit Paladin Andral";
			recruits[1].Model = 41;
			template = new GameNpcInventory();
			template.AddItem(eInventorySlot.TwoHandWeapon, new NPCWeapon(6));
			template.AddItem(eInventorySlot.TorsoArmor, new NPCArmor(41));
			template.AddItem(eInventorySlot.LegsArmor, new NPCArmor(42));
			template.AddItem(eInventorySlot.FeetArmor, new NPCArmor(45));
			template.AddItem(eInventorySlot.ArmsArmor, new NPCArmor(43));
			template.AddItem(eInventorySlot.HandsArmor, new NPCArmor(44));
			recruits[1].Inventory = template;
			recruits[1].SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);

			recruits[2].Name = "Recruit Scout Gillman";
			recruits[2].Model = 32;
			template = new GameNpcInventory();
			template.AddItem(eInventorySlot.RightHandWeapon, new NPCWeapon(4));
			template.AddItem(eInventorySlot.TorsoArmor, new NPCArmor(36));
			template.AddItem(eInventorySlot.LegsArmor, new NPCArmor(37));
			recruits[2].Inventory = template;
			recruits[2].SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

			recruits[3].Name = "Recruit Scout Stuart";
			recruits[3].Model = 32;
			template = new GameNpcInventory();
			template.AddItem(eInventorySlot.RightHandWeapon, new NPCWeapon(5));
			template.AddItem(eInventorySlot.TorsoArmor, new NPCArmor(36));
			template.AddItem(eInventorySlot.LegsArmor, new NPCArmor(37));
			recruits[3].Inventory = template;
			recruits[3].SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

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

		/* This is our callback hook that will be called when the player clicks
		 * on any button in the quest offer dialog. We check if he accepts or
		 * declines here...
		 */
		protected static void QuestDialogResponse(DOLEvent e, object sender, EventArgs args)
		{
			QuestEventArgs gArgs = args as QuestEventArgs;

			if (gArgs != null && gArgs.QuestType.Equals(typeof(Culmination)))
			{
				GamePlayer player = gArgs.Player;
				if (player == null) return;

				if (e == GamePlayerEvent.AcceptQuest)
				{
					if (QuestMgr.GiveQuestToPlayer(typeof(Culmination), player, gArgs.Source as GameNPC))
					{
						GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));

						masterFrederick.SayTo(player, "Remember, Master Dunwyn is in the woods south of the bridge to Camelot. He is near the fairy's village. Be safe!");

						bool dunwynCloneCreated = false;
						if (player.PlayerGroup != null)
						{
							foreach (GamePlayer groupMember in player.PlayerGroup.GetPlayersInTheGroup())
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
							Culmination quest = player.IsDoingQuest(typeof (Culmination)) as Culmination;
							if(quest != null) quest.CreateDunwynClone();
						}
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
						return "[Step #1] Travel across the bridge to Camelot and turn south. Follow the inside of the woods until you find Master Dunwyn, down beyond the gates to the houses.";
					case 2:
						return "[Step #2] Find and kill Queen, Tatiana.";
					case 3:
						return "[Step #3] When the fighting is over, speak with Master Dunwyn.";
					case 4:
						return "[Step #4] Return to Master Frederick in Cotswold. Hand him the head of Queen Tatiana as evidence that the fairy threat has now been extinguished.";
					case 5:
						return "[Step #5] Wait for Master Frederick to reward you. If he stops speaking with you, ask about a [reward] for your time and effort.";
                    default:
                        return "[Step #" + Step + "] No Description entered for this step!";
				}
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

				if (gArgs.Target == queenTatiana)
				{
					SendSystemMessage("You slay the queen and take her head as proof.");
					GiveItemToPlayer(CreateQuestItem(queenTatianasHead));
					ChangeQuestStep(3);
					return;
				}
			}

			if (Step == 4 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target == masterFrederick && gArgs.Item.Name == queenTatianasHead.Name)
				{
					masterFrederick.SayTo(m_questPlayer, "Wonderful! Now I know Cotswold will be safe, thanks in no small part to you, Recruit "+player.Name+". Excellent work. Cotswold is forever in your debt. I have a [reward] for you. I hope you have some use for it.");
					RemoveItemFromPlayer(masterFrederick, gArgs.Item);
					ChangeQuestStep(5);
					return;
				}
			}
		}

		public override void FinishQuest()
		{
			// make sure to clean up, should be needed , but just to make certain
			ResetMasterDunwyn();

			//Give reward to player here ...              
			m_questPlayer.GainExperience(1012, 0, 0, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 9, Util.Random(50)), "You recieve {0} as a reward.");
			if (m_questPlayer.HasAbilityToUseItem(recruitsGauntlets.CreateInstance() as EquipableItem))
			{
				GiveItemToPlayer(masterFrederick, recruitsGauntlets.CreateInstance());
				GiveItemToPlayer(masterFrederick, recruitsJewel.CreateInstance());
			}
			else
			{
				GiveItemToPlayer(masterFrederick, recruitsGloves.CreateInstance());
				GiveItemToPlayer(masterFrederick, recruitsJewelCloth.CreateInstance());
			}

			GiveItemToPlayer(masterFrederick, recruitsBracer.CreateInstance());

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
		
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...
		}
	}
}
