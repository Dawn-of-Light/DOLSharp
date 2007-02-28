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
 */

using System;
using System.Reflection;
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
     * This quest for example will be stored in the database with
     * the name: DOL.GS.Quests.Albion.AMessageToTheManes
     */

    public class AMessageToTheManes : BaseQuest
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

        protected const string questTitle = "A Message to the Manes";
        protected const int minimumLevel = 9;
        protected const int maximumLevel = 12;

        /* 
         * Start NPC
         */
        private static GameNPC sirJerem = null;

        private short manesKilled = 0;

        /* We need to define the constructors from the base class here, else there might be problems
         * when loading this quest...
         */
        public AMessageToTheManes()
            : base()
        {
        }

        public AMessageToTheManes(GamePlayer questingPlayer)
            : this(questingPlayer, 1)
        {
        }

        public AMessageToTheManes(GamePlayer questingPlayer, int step)
            : base(questingPlayer, step)
        {
        }

        public AMessageToTheManes(GamePlayer questingPlayer, DBQuest dbQuest)
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

            GameNPC[] npcs = WorldMgr.GetNPCsByName("Sir Jerem", eRealm.Albion);

            /* Whops, if the npcs array length is 0 then no Sir Quait exists in
                * this users Mob Database, so we simply create one ;-)
                * else we take the existing one. And if more than one exist, we take
                * the first ...
                */
            if (npcs.Length == 0)
            {
                sirJerem = new GameNPC();
                sirJerem.Model = 254;
                sirJerem.Name = "Sir Jerem";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + sirJerem.Name + ", creating him ...");
                sirJerem.Realm = (byte)eRealm.Albion;
                sirJerem.CurrentRegionID = 1;
                sirJerem.Size = 49;
                sirJerem.Level = 38;
                sirJerem.X = 573815;
                sirJerem.Y = 530850;
                sirJerem.Z = 2933;
                sirJerem.Heading = 2685;

                //You don't have to store the created mob in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    sirJerem.SaveIntoDatabase();


                sirJerem.AddToWorld();

            }
            else
                sirJerem = npcs[0];


            #endregion


            /* Now we add some hooks to the Sir Quait we found.
				* Actually, we want to know when a player interacts with him.
				* So, we hook the right-click (interact) and the whisper method
				* of Sir Quait and set the callback method to the "TalkToXXX"
				* method. This means, the "TalkToXXX" method is called whenever
				* a player right clicks on him or when he whispers to him.
				*/
			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

            GameEventMgr.AddHandler(sirJerem, GameLivingEvent.Interact, new DOLEventHandler(TalkToSirJerem));
            GameEventMgr.AddHandler(sirJerem, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToSirJerem));

            /* Now we bring to sirJerem the possibility to give this quest to players */
            sirJerem.AddQuestToGive(typeof(AMessageToTheManes));

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
            /* If sirJerem has not been initialized, then we don't have to remove any
             * hooks from him ;-)
             */
            if (sirJerem == null)
                return;

            /* Removing hooks works just as adding them but instead of 
             * AddHandler, we call RemoveHandler, the parameters stay the same
             */
			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

            GameEventMgr.RemoveHandler(sirJerem, GameObjectEvent.Interact, new DOLEventHandler(TalkToSirJerem));
            GameEventMgr.RemoveHandler(sirJerem, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToSirJerem));

            /* Now we remove to sirJerem the possibility to give this quest to players */
            sirJerem.RemoveQuestToGive(typeof(AMessageToTheManes));
        }

        /* This is the method we declared as callback for the hooks we set to
         * Sir Quait. It will be called whenever a player right clicks on Sir Quait
         * or when he whispers something to him.
         */

        protected static void TalkToSirJerem(DOLEvent e, object sender, EventArgs args)
        {
            //We get the player from the event arguments and check if he qualifies		
            GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
            if (player == null)
                return;

            if (sirJerem.CanGiveQuest(typeof(AMessageToTheManes), player) <= 0)
                return;

            //We also check if the player is already doing the quest
            AMessageToTheManes quest = player.IsDoingQuest(typeof(AMessageToTheManes)) as AMessageToTheManes;

            sirJerem.TurnTo(player);
            //Did the player rightclick on sirJerem?
            if (e == GameObjectEvent.Interact)
            {
                //We check if the player is already doing the quest
                if (quest != null)
                {
                    if (quest.Step == 1)
                        sirJerem.SayTo(player, "It heartens me to know that I can count on your help. Because we cannot devote many resources to dealing with the demons, the first part of my plan calls for us to deliver a warning message to these [demons].");
                    else if (quest.Step == 3)
                        sirJerem.SayTo(player, "You've returned! I must admit, "+player.Name+". I was beginning to get a little worried. Were you able to kill the two Manes [demons]?");
                    return;
                }
                else
                {
                    //Player hasn't the quest:
                    sirJerem.SayTo(player, "Hello again, " + player.CharacterClass.Name + ". Pardon me for not taking note of your presence earlier, but I've been considering how best to approach a new problem. Albion has no lack of enemies, and we must be vigilant in our defence against [each].");
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
                        case "each":
                            sirJerem.SayTo(player, "The forces of Midgard, Hibernia, the demons of Darkness Falls, the Drakoran, and Morgana's twisted legions all crave the destruction of our kingdom. So far, we have managed to fend them off, but we cannot count on [luck] to guide us.");
                            break;
                        case "luck":
                            sirJerem.SayTo(player, "We have skilled soldirs and commanders, but if our enemies should decide to unite, I fear they would overrun us. The denizens of Darkness Falls have begun to venture beyond the confines of their dungeon. We must act to [contain] them.");
                            break;
                        case "contain":
							player.Out.SendQuestSubscribeCommand(sirJerem, QuestMgr.GetIDForQuestType(typeof(AMessageToTheManes)), "Will you help Sir Jerem dissuade the Manes demons from leaving Darkness Falls? [Levels 9-12]");
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
                        case "demons":
                            if (quest.Step == 1)
                                sirJerem.SayTo(player, "Our scouts have reported that two kinds of demons, the Grumoz and the Manes, have ventured outside the portal. Slay two of the Manes to show the demon princes that Albion will not suffer their presence in its [lands].");
                            else if (quest.Step == 3)
                            {
                                quest.SendSystemMessage("You tell Sir Jerem that you were successful in your mission.");
                                sirJerem.SayTo(player, "Well done! I'll have my scouts monitor the area around the portal in the coming days to see if your actions had the intended effect. I'd be a fool to believe they would give up easily, but at least they will know that we are aware of their [presence].");
                            }
                            break;
                        case "lands":
                            if (quest.Step == 1)
                            {
                                sirJerem.SayTo(player, "The Darkness Falls portal is to the east of Prydwen Keep, beyond the bridge. If you have trouble locating it, don't forget that it is marked on your map.");
                                quest.Step = 2;
                            }
                            break;
                        case "presence":
                            if (quest.Step == 3)
                            {
                                sirJerem.SayTo(player, "Thank you again for all your help. I wish I could offer you a greater payment for your services, but as it is, our gold is stretched thin. Please accept this money along with the thanks of the people of Prydwen Keep.");
                                quest.FinishQuest();
                            }
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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(AMessageToTheManes)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

        /// <summary>
        /// This method checks if a player qualifies for this quest
        /// </summary>
        /// <returns>true if qualified, false if not</returns>
        public override bool CheckQuestQualification(GamePlayer player)
        {
            // if the player is already doing the quest his level is no longer of relevance
            if (player.IsDoingQuest(typeof(AMessageToTheManes)) != null)
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
            AMessageToTheManes quest = player.IsDoingQuest(typeof(AMessageToTheManes)) as AMessageToTheManes;

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
            if (sirJerem.CanGiveQuest(typeof(AMessageToTheManes), player) <= 0)
                return;

            if (player.IsDoingQuest(typeof(AMessageToTheManes)) != null)
                return;

            if (response == 0x00)
            {
                SendReply(player, "Oh well, if you change your mind, please come back!");
            }
            else
            {
                //Check if we can add the quest
                if (!sirJerem.GiveQuest(typeof(AMessageToTheManes), player, 1))
                    return;

                sirJerem.SayTo(player, "It heartens me to know that I can count on your help. Because we cannot devote many resources to dealing with the demons, the first part of my plan calls for us to deliver a warning message to these [demons].");
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
                        return "[Step #1] Continue speaking to Sir Jerem about his plans to drive back the [demons] escaping from Darkness Falls.";
                    case 2:
                        return "[Step #2] Travel east from Prydwen Keep, past the bridge, to the Darkness Falls portal and kill two Manes demons. If you have trouble locating the portal, you can access your map by typing /map.";
                    case 3:
                        return "[Step #3] You've killed two Manes demons. Return to Sir Jerem at Prydwen Keep to the west and speak with him.";
                    
                }
                return base.Description;
            }
        }

        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = sender as GamePlayer;

            if (player == null)
                return;
            if (player.IsDoingQuest(typeof(AMessageToTheManes)) == null)
                return;


            if (Step == 2 && e == GameLivingEvent.EnemyKilled)
            {
                EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
                if ((gArgs.Target.Name.IndexOf("Manes Demon") >= 0) || (gArgs.Target.Name.IndexOf("manes demon") >= 0))
                {
                    if (manesKilled == 0)
                    {
                        SendSystemMessage("You've killed the first Manes demon.");
                        manesKilled++;
                        return;
                    }
                    else if (manesKilled == 1)
                    {
                        player.Out.SendDialogBox(eDialogCode.SimpleWarning, 0x00, 0x00, 0x00, 0x00, eDialogType.Ok, true, "You've killed two Manes demons! \nYour journal has been updated.");
                        Step = 3;
                        manesKilled++;
                        return;
                    }
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

            m_questPlayer.GainExperience((long)((m_questPlayer.ExperienceForNextLevel - m_questPlayer.ExperienceForCurrentLevel) / 8.98), 0, 0, true);
            m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 1, 49 + Util.Random(50)), "You are awarded 1 silver and some copper!");

        }

    }
}
