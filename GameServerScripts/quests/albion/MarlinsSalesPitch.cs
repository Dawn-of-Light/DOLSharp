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
 * Speak to Marlin Thuler, the instrument merchant, who can be found in the loft of the stables in West Downs. 
 * 
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
     * the name: DOL.GS.Quests.Albion.MarlinsSalesPitch
     */

    public class MarlinsSalesPitch : BaseQuest
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

        protected const string questTitle = "Marlin's Sales Pitch";
        protected const int minimumLevel = 17;
        protected const int maximumLevel = 20;

        /* 
         * Start NPC
         */
        private static GameNPC marlinThuler = null;

        /*
         * Item templates
         */
        private static ItemTemplate drum = null;

        /* We need to define the constructors from the base class here, else there might be problems
         * when loading this quest...
         */
        public MarlinsSalesPitch()
            : base()
        {
        }

        public MarlinsSalesPitch(GamePlayer questingPlayer)
            : this(questingPlayer, 1)
        {
        }

        public MarlinsSalesPitch(GamePlayer questingPlayer, int step)
            : base(questingPlayer, step)
        {
        }

        public MarlinsSalesPitch(GamePlayer questingPlayer, DBQuest dbQuest)
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

            GameNPC[] npcs = WorldMgr.GetNPCsByName("Marlin Thuler", eRealm.Albion);

            /* Whops, if the npcs array length is 0 then no Sir Quait exists in
                * this users Mob Database, so we simply create one ;-)
                * else we take the existing one. And if more than one exist, we take
                * the first ...
                */
            if (npcs.Length == 0)
            {
                marlinThuler = new GameNPC();
                marlinThuler.Model = 960;
                marlinThuler.Name = "Marlin Thuler";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + marlinThuler.Name + ", creating him ...");
                marlinThuler.GuildName = "Instrument Merchant";
                marlinThuler.Realm = (byte)eRealm.Albion;
                marlinThuler.CurrentRegionID = 1;
                marlinThuler.Size = 52;
                marlinThuler.Level = 40;
                marlinThuler.X = 578189;
                marlinThuler.Y = 557031;
                marlinThuler.Z = 2340;
                marlinThuler.Heading = 1513;

                //You don't have to store the created mob in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    marlinThuler.SaveIntoDatabase();


                marlinThuler.AddToWorld();

            }
            else
                marlinThuler = npcs[0];


            #endregion

            #region defineItems



            drum = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "finely_crafted_drum");
            if (drum == null)
            {
                if (log.IsWarnEnabled)
                    log.Warn("Could not find Finely Crafted Drum from a wolf cub, creating it ...");
                drum = new ItemTemplate();
                drum.Object_Type = 0;
                drum.Id_nb = "finely_crafted_drum";
                drum.Name = "Finely Crafted Drum";
                drum.Level = 1;
                drum.Model = 2977;
                drum.IsDropable = false;
                drum.IsPickable = false;
                drum.Weight = 15;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(drum);
            }


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

            GameEventMgr.AddHandler(marlinThuler, GameLivingEvent.Interact, new DOLEventHandler(TalkToMarlinThuler));
            GameEventMgr.AddHandler(marlinThuler, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMarlinThuler));

            /* Now we bring to marlinThuler the possibility to give this quest to players */
            marlinThuler.AddQuestToGive(typeof(MarlinsSalesPitch));

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
            /* If marlinThuler has not been initialized, then we don't have to remove any
             * hooks from him ;-)
             */
            if (marlinThuler == null)
                return;

            /* Removing hooks works just as adding them but instead of 
             * AddHandler, we call RemoveHandler, the parameters stay the same
             */

			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

            GameEventMgr.RemoveHandler(marlinThuler, GameObjectEvent.Interact, new DOLEventHandler(TalkToMarlinThuler));
            GameEventMgr.RemoveHandler(marlinThuler, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMarlinThuler));

            /* Now we remove to marlinThuler the possibility to give this quest to players */
            marlinThuler.RemoveQuestToGive(typeof(MarlinsSalesPitch));
        }

        /* This is the method we declared as callback for the hooks we set to
         * Sir Quait. It will be called whenever a player right clicks on Sir Quait
         * or when he whispers something to him.
         */

        protected static void TalkToMarlinThuler(DOLEvent e, object sender, EventArgs args)
        {
            //We get the player from the event arguments and check if he qualifies		
            GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
            if (player == null)
                return;

            if (marlinThuler.CanGiveQuest(typeof(MarlinsSalesPitch), player) <= 0)
                return;

            //We also check if the player is already doing the quest
            MarlinsSalesPitch quest = player.IsDoingQuest(typeof(MarlinsSalesPitch)) as MarlinsSalesPitch;

            marlinThuler.TurnTo(player);
            //Did the player rightclick on marlinThuler?
            if (e == GameObjectEvent.Interact)
            {
                //We check if the player is already doing the quest
                if (quest != null)
                {
                    if (quest.Step == 1)
                        marlinThuler.SayTo(player, "I'm glad to hear you're willing to answer your realm's call for help. Albion will remember your service. The local bandit leader is a fellow by the name of [Mostram].");
                    else if (quest.Step == 3)
                        marlinThuler.SayTo(player, "Welcome back, " + player.Name + ". Word travels quickly in these parts, and I have heard of your success. Some of the bandits have even started to retreat to their camps in the northeast. You've [done well].");
                }
                else
                {
                    //Player hasn't the quest:
                    marlinThuler.SayTo(player, "Oh woe is me! I have no idea how I can possibly hope to go on with [business] now? Please, you must help me!");
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
                        case "business":
                            marlinThuler.SayTo(player, "Those tomb raider scoundrels have stolen my favorite drum again. I use it to demonstrate the wondrous sound my instruments can produce! Never has a customer heard that beautiful sound and failed to buy something from me. Without that drum, I'm ruined! Will you get my [drum] back for me? I will reward you handsomely, I swear it.");
                            break;
                        case "drum":
							player.Out.SendQuestSubscribeCommand(marlinThuler, QuestMgr.GetIDForQuestType(typeof(MarlinsSalesPitch)), "Will you help Marlin retrieve his lost drum? [Levels 17-20]");
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
                                marlinThuler.SayTo(player, "From what we've been able to find out, Mostram works from the fields south of the keep and across the river. Slay him and return to me when the deed is done.");
                                quest.Step = 2;
                            }
                            break;
                        case "done well":
                            if (quest.Step == 3)
                                marlinThuler.SayTo(player, "Now we must show the bandits that this is only the beginning of a long campaign. I've already begun planning our next moves to rid Camelot Hills of the bandit problem. Perhaps we can work together again in the [future].");
                            break;
                        case "future":
                            if (quest.Step == 3)
                            {
                                marlinThuler.SayTo(player, "It will be some time before my next plan is ready. For now, though, please take this money as payment for your services.");
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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(MarlinsSalesPitch)))
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
            if (player.IsDoingQuest(typeof(MarlinsSalesPitch)) != null)
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
            MarlinsSalesPitch quest = player.IsDoingQuest(typeof(MarlinsSalesPitch)) as MarlinsSalesPitch;

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
            if (marlinThuler.CanGiveQuest(typeof(MarlinsSalesPitch), player) <= 0)
                return;

            if (player.IsDoingQuest(typeof(MarlinsSalesPitch)) != null)
                return;

            if (response == 0x00)
            {
                SendReply(player, "Oh well, if you change your mind, please come back!");
            }
            else
            {
                //Check if we can add the quest
                if (!marlinThuler.GiveQuest(typeof(MarlinsSalesPitch), player, 1))
                    return;

                marlinThuler.SayTo(player, "Oh thank you! You can find the tomb raider scouts that took my drum east of here, across the road and just down a steep hill. They like to meet near a circular set of stones. Find the one that has my drum and return it to me, I beg of you.");
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
                        return "[Step #1] Search to the East of West Downs for the Tomb Raider Scouts and retrieve Marlin's drum. You may have to kill more than one to find the culprit.";
                    case 2:
                        return "[Step #2] Return the drum to Marlin in West Downs and receive your reward!";
                   
                }
                return base.Description;
            }
        }

        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = sender as GamePlayer;

            if (player == null)
                return;
            if (player.IsDoingQuest(typeof(MarlinsSalesPitch)) == null)
                return;


            if (Step == 1 && e == GameLivingEvent.EnemyKilled)
            {
                EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
                if ((gArgs.Target.Name.IndexOf("Tomb Raider Digger") >= 0) || (gArgs.Target.Name.IndexOf("Tomb Raider Scout") >= 0) || (gArgs.Target.Name.IndexOf("tomb raider digger") >= 0) || (gArgs.Target.Name.IndexOf("tomb raider scout") >= 0))
                {
                    if (Util.Chance(20))
                    {
                        SendSystemMessage("The tomb raider staggers away before falling over, lifeless.");
                    
                        player.Out.SendDialogBox(eDialogCode.SimpleWarning, 0x00, 0x00, 0x00, 0x00, eDialogType.Ok, true, "The tomb raider scout drops a beautifully crafted drum as he falls to the ground. Your journal has been updated.");
                        GiveItem(player, drum);
                        Step = 2;
                    } return;
                }
            }
            else if (Step == 2 && e == GamePlayerEvent.GiveItem)
            {
                GiveItemEventArgs gArgs = (GiveItemEventArgs)args;
                if (gArgs.Target.Name == marlinThuler.Name && gArgs.Item.Id_nb == drum.Id_nb)
                {
                    RemoveItem(marlinThuler, player, drum);
                    marlinThuler.SayTo(player, "Hallelujah! You found my drum, thank you! I will never be able to fully repay you, but perhaps these coins will at least start to show my appreciation for your service.");
                    FinishQuest();
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
            m_questPlayer.GainExperience((long)((m_questPlayer.ExperienceForNextLevel - m_questPlayer.ExperienceForCurrentLevel) / 9), true);
            m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 11, 49 + Util.Random(50)), "You are awarded 11 silver and some copper!");

        }

    }
}
