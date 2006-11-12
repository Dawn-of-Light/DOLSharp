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
 * Author:		Cletus
 * Date:		23. 10. 2006	
 * Directory: /scripts/quests/albion/
 *
 * Description:
 * Speak to Atheleys Sy'Lian. He is the keep healer and is found inside the central building in Prydwen Keep.
 * 
 */

using System;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.AI.Brain;
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
     * This quest for example will be stored in the database with
     * the name: DOL.GS.Quests.Albion.BreakingTheBandits
     */

    public class BreakingTheBandits : BaseQuest
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
         * We store our two mobs as static variables, since we need them 
         */

        protected const string questTitle = "Breaking the Bandits";
        protected const int minimumLevel = 8;
        protected const int maximumLevel = 11;

        /* 
         * Start NPC
         */
        private static GameNPC atheleys = null;

        private static GameNPC mostram = null;

       
        /* We need to define the constructors from the base class here, else there might be problems
         * when loading this quest...
         */
        public BreakingTheBandits()
            : base()
        {
        }

        public BreakingTheBandits(GamePlayer questingPlayer)
            : this(questingPlayer, 1)
        {
        }

        public BreakingTheBandits(GamePlayer questingPlayer, int step)
            : base(questingPlayer, step)
        {
        }

        public BreakingTheBandits(GamePlayer questingPlayer, DBQuest dbQuest)
            : base(questingPlayer, dbQuest)
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
         * want. We will do it the standard way here ... and make Sir Quait wail
         * a bit about the loss of his sword! 
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

            GameNPC[] npcs = WorldMgr.GetNPCsByName("Atheleys Sy'Lian", eRealm.Albion);

            /* Whops, if the npcs array length is 0 then no Sir Quait exists in
                * this users Mob Database, so we simply create one ;-)
                * else we take the existing one. And if more than one exist, we take
                * the first ...
                */
            if (npcs.Length == 0)
            {
                atheleys = new GameMob();
                atheleys.Model = 87;
                atheleys.Name = "Atheleys Sy'Lian";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + atheleys.Name + ", creating him ...");
                atheleys.Realm = (byte)eRealm.Albion;
                atheleys.CurrentRegionID = 1;
                atheleys.Size = 50;
                atheleys.Level = 30;
                atheleys.X = 574375;
                atheleys.Y = 530243;
                atheleys.Z = 2906;
                atheleys.Heading = 1922;

                //You don't have to store the created mob in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    atheleys.SaveIntoDatabase();


                atheleys.AddToWorld();

            }
            else
                atheleys = npcs[0];

            npcs = WorldMgr.GetNPCsByName("Mostram", eRealm.None);


            if (npcs.Length == 0)
            {
                //TODO insert proper attributes 
                mostram = new GameMob();
                mostram.Model = 18;
                mostram.Name = "Mostram";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find Mostram, creating him ...");
                mostram.GuildName = "";
                mostram.Realm = (byte)eRealm.None;
                mostram.CurrentRegionID = 1;
                mostram.Size = 52;
                mostram.Level = 9;
                mostram.X = 574338;
                mostram.Y = 536865;
                mostram.Z = 2361;
                mostram.Heading = 57;

                StandardMobBrain brain = new StandardMobBrain();
                brain.AggroLevel = 0;
                brain.AggroRange = 500;
                mostram.SetOwnBrain(brain);

                //You don't have to store the created mob in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    mostram.SaveIntoDatabase();


                mostram.AddToWorld();

            }
            else
                mostram = npcs[0];


            #endregion

            

            /* Now we add some hooks to the Sir Quait we found.
				* Actually, we want to know when a player interacts with him.
				* So, we hook the right-click (interact) and the whisper method
				* of Sir Quait and set the callback method to the "TalkToXXX"
				* method. This means, the "TalkToXXX" method is called whenever
				* a player right clicks on him or when he whispers to him.
				*/
            GameEventMgr.AddHandler(atheleys, GameLivingEvent.Interact, new DOLEventHandler(TalkToAtheleys));
            GameEventMgr.AddHandler(atheleys, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToAtheleys));

            /* Now we bring to atheleys the possibility to give this quest to players */
            atheleys.AddQuestToGive(typeof(BreakingTheBandits));

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
            /* If atheleys has not been initialized, then we don't have to remove any
             * hooks from him ;-)
             */
            if (atheleys == null)
                return;

            /* Removing hooks works just as adding them but instead of 
             * AddHandler, we call RemoveHandler, the parameters stay the same
             */
            GameEventMgr.RemoveHandler(atheleys, GameObjectEvent.Interact, new DOLEventHandler(TalkToAtheleys));
            GameEventMgr.RemoveHandler(atheleys, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToAtheleys));

            /* Now we remove to atheleys the possibility to give this quest to players */
            atheleys.RemoveQuestToGive(typeof(BreakingTheBandits));
        }

        /* This is the method we declared as callback for the hooks we set to
         * Sir Quait. It will be called whenever a player right clicks on Sir Quait
         * or when he whispers something to him.
         */

        protected static void TalkToAtheleys(DOLEvent e, object sender, EventArgs args)
        {
            //We get the player from the event arguments and check if he qualifies		
            GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
            if (player == null)
                return;

            if (atheleys.CanGiveQuest(typeof(BreakingTheBandits), player) <= 0)
                return;

            //We also check if the player is already doing the quest
            BreakingTheBandits quest = player.IsDoingQuest(typeof(BreakingTheBandits)) as BreakingTheBandits;

            atheleys.TurnTo(player);
            //Did the player rightclick on atheleys?
            if (e == GameObjectEvent.Interact)
            {
                //We check if the player is already doing the quest
                if (quest != null)
                {
                    if (quest.Step == 1)
                        atheleys.SayTo(player, "I'm glad to hear you're willing to answer your realm's call for help. Albion will remember your service. The local bandit leader is a fellow by the name of [Mostram].");
                    else if (quest.Step == 3)
                        atheleys.SayTo(player, "Welcome back, "+player.Name+". Word travels quickly in these parts, and I have heard of your success. Some of the bandits have even started to retreat to their camps in the northeast. You've [done well].");
                }
                else
                {
                    //Player hasn't the quest:
                    atheleys.SayTo(player, "Good day to you mighty. I am Atheleys Sy'Lian from the royal court at Avalon. I was sent here to speak with Lord Prydwen about the problem that he has been experiencing lately with the [bandits] that roam the hillsides.");
                    return;
                }
            }
            // The player whispered to Sir Jerem (clicked on the text inside the [])
            else if (e == GameLivingEvent.WhisperReceive)
            {
                WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs)args;

                //We also check if the player is already doing the quest
                if (quest == null)
                {
                    switch (wArgs.Text)
                    {
                        case "bandits":
                            atheleys.SayTo(player, "Since the death of our beloved King Arthur, the bandits of Camelot Hills, and indeed, all of Albion, have grown quite bold. They roam the roads in packs, harassing travelers and disrupting the peace. They must be [stopped].");
                            break;
                        case "stopped":
                            atheleys.SayTo(player, "Alas, Captain Bonswell has but limited resources to address this problem. I do not blame him, nor do I envy him. His is not an easy task. While he deals with threats from abroad and Morgana's minions, I have decided to take on the bandit [problem].");
                            break;
                        case "problem":
                            atheleys.SayTo(player, "The first step in my plan is to eliminate the leader of a group of bandits operating just outside of Prydwen Keep. Will you assist me in this task?.");
                            player.Out.SendCustomDialog("Will you help Atheleys Sy'Lian in her campaign against the bandits? [Levels 8-11]", new CustomDialogResponse(CheckPlayerAcceptQuest));
                            break;
                    }
                }
                else
                {
                    switch (wArgs.Text)
                    {
                        case "abort":
                            player.Out.SendCustomDialog("Do you really want to abort this quest?", new CustomDialogResponse(CheckPlayerAbortQuest));
                            break;
                        case "Mostram":
                            if (quest.Step == 1)
                            {
                                atheleys.SayTo(player, "From what we've been able to find out, Mostram works from the fields south of the keep and across the river. Slay him and return to me when the deed is done.");
                                quest.Step = 2;
                            }
                            break;
                        case "done well":
                            if (quest.Step == 3)
                                atheleys.SayTo(player, "Now we must show the bandits that this is only the beginning of a long campaign. I've already begun planning our next moves to rid Camelot Hills of the bandit problem. Perhaps we can work together again in the [future].");
                            break;
                        case "future":
                            if (quest.Step == 3) 
                            {
                                atheleys.SayTo(player, "It will be some time before my next plan is ready. For now, though, please take this money as payment for your services.");
                                quest.FinishQuest();
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
            if (player.IsDoingQuest(typeof(BreakingTheBandits)) != null)
                return true;

            // This checks below are only performed is player isn't doing quest already

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
            BreakingTheBandits quest = player.IsDoingQuest(typeof(BreakingTheBandits)) as BreakingTheBandits;

            if (quest == null)
                return;

            if (response == 0x00)
            {
                SendSystemMessage(player, "Good, now go out there and finish your work!");
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
            if (atheleys.CanGiveQuest(typeof(BreakingTheBandits), player) <= 0)
                return;

            if (player.IsDoingQuest(typeof(BreakingTheBandits)) != null)
                return;

            if (response == 0x00)
            {
                SendReply(player, "Oh well, if you change your mind, please come back!");
            }
            else
            {
                //Check if we can add the quest
                if (!atheleys.GiveQuest(typeof(BreakingTheBandits), player, 1))
                    return;

                atheleys.SayTo(player, "I'm glad to hear you're willing to answer your realm's call for help. Albion will remember your service. The local bandit leader is a fellow by the name of [Mostram].");
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
                        return "[Step #1] Continue speaking with Atheleys Sy'Lian.";
                    case 2:
                        return "[Step #2] Venture out from the walls of Prydwen Keep and cross the river to the south. Search for the bandit named Mostram and kill him.";
                    case 3:
                        return "[Step #3] Now that you have slain Mostram, return to Atheleys Sy'Lian at Prydwen Keep, to the north.";
                    
                }
                return base.Description;
            }
        }

        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = sender as GamePlayer;

            if (player == null)
                return;
            if (player.IsDoingQuest(typeof(BreakingTheBandits)) == null)
                return;


            if (Step == 2 && e == GameLivingEvent.EnemyKilled)
            {
                EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
                if ((gArgs.Target.Name.IndexOf("Mostram") >= 0) || (gArgs.Target.Name.IndexOf("mostram") >= 0))
                {
                    Step = 3;                    
                    player.Out.SendDialogBox(eDialogCode.SimpleWarning, 0x00, 0x00, 0x00, 0x00, eDialogType.Ok, true, "You killed Mostram! Your journal has been updated.");
                    return;
                }
            }            
        }

        public override void AbortQuest()
        {
            base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

        }

        public override void FinishQuest()
        {
            base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

            //Give reward to player here ...
            m_questPlayer.GainExperience((long)((m_questPlayer.ExperienceForNextLevel - m_questPlayer.ExperienceForCurrentLevel) / 20), 0, 0, true);
            m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 1, 30 + Util.Random(50)), "You recieve {0} for your service.");

        }

    }
}
