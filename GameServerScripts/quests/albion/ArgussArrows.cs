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
 * Argus Bowman can be found in Ludlow Villiage located in Black Mountains South, loc=47236,45863,2251 dir=142.
 * 
 * Travel to Black Mtns. South: loc=54469,46373,2824 dir=179, approach the crest of the hill carefully 
 * as there are cutpurses mixed in with the young cutpurse's. Cutpurse's tend to con yellow to orange 
 * to a lvl 4, while young cutpurse's will con blue... both are high agro and will call for help. 
 * Pull a young cutpurse down the hill a ways and kill him. A "Bunch of Ludlow Magical Wood" will appear 
 * in your inventory upon his death.
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
     * the name: DOL.GS.Quests.Albion.ArgussArrows
     */

    public class ArgussArrows : BaseQuest
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

        protected const string questTitle = "Argus's Arrows";
        protected const int minimumLevel = 4;
        protected const int maximumLevel = 4;

        /* 
         * Start NPC
         */
        private static GameNPC argusBowman = null;

        /*
         * Item templates
         */
        private static ItemTemplate magicalWood = null;
        private static ItemTemplate dullBlackGem = null;

        /* We need to define the constructors from the base class here, else there might be problems
         * when loading this quest...
         */
        public ArgussArrows()
            : base()
        {
        }

        public ArgussArrows(GamePlayer questingPlayer)
            : this(questingPlayer, 1)
        {
        }

        public ArgussArrows(GamePlayer questingPlayer, int step)
            : base(questingPlayer, step)
        {
        }

        public ArgussArrows(GamePlayer questingPlayer, DBQuest dbQuest)
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

            GameNPC[] npcs = WorldMgr.GetNPCsByName("Argus Bowman", eRealm.Albion);

            /* Whops, if the npcs array length is 0 then no Sir Quait exists in
                * this users Mob Database, so we simply create one ;-)
                * else we take the existing one. And if more than one exist, we take
                * the first ...
                */
            if (npcs.Length == 0)
            {
                argusBowman = new GameMob();
                argusBowman.Model = 40;
                argusBowman.Name = "Argus Bowman";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + argusBowman.Name + ", creating him ...");
                argusBowman.GuildName = "Weapon Merchant";
                argusBowman.Realm = (byte)eRealm.Albion;
                argusBowman.CurrentRegionID = 1;
                argusBowman.Size = 50;
                argusBowman.Level = 18;
                argusBowman.X = 530594;
                argusBowman.Y = 480120;
                argusBowman.Z = 2251;
                argusBowman.Heading = 1627;

                //You don't have to store the created mob in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    argusBowman.SaveIntoDatabase();


                argusBowman.AddToWorld();

            }
            else
                argusBowman = npcs[0];


            #endregion

            #region defineItems



            magicalWood = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "ludlow_magical_wood");
            if (magicalWood == null)
            {
                if (log.IsWarnEnabled)
                    log.Warn("Could not find Ludlow Magical Wood, creating it ...");
                magicalWood = new ItemTemplate();
                magicalWood.Object_Type = 0;
                magicalWood.Id_nb = "ludlow_magical_wood";
                magicalWood.Name = "Ludlow Magical Wood";
                magicalWood.Level = 1;
                magicalWood.Model = 520;
                magicalWood.IsDropable = false;
                magicalWood.IsPickable = false;
                magicalWood.Weight = 5;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(magicalWood);
            }

            dullBlackGem = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "dull_black_gem");
            if (dullBlackGem == null)
            {
                dullBlackGem = new ItemTemplate();
                dullBlackGem.Name = "Dull Black Gem";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + dullBlackGem.Name + ", creating it ...");
                dullBlackGem.Level = 6;

                dullBlackGem.Weight = 3;
                dullBlackGem.Model = 118;

                dullBlackGem.Object_Type = (int)eObjectType.Magical;
                dullBlackGem.Item_Type = (int)eEquipmentItems.JEWEL;
                dullBlackGem.Id_nb = "dull_black_gem";
                dullBlackGem.Gold = 0;
                dullBlackGem.Silver = 9;
                dullBlackGem.Copper = 0;
                dullBlackGem.IsPickable = true;
                dullBlackGem.IsDropable = true;

                dullBlackGem.Bonus = 5; // default bonus

                dullBlackGem.Bonus1 = 6;
                dullBlackGem.Bonus1Type = (int)eStat.DEX;

                dullBlackGem.Bonus2 = 1;
                dullBlackGem.Bonus2Type = (int)eResist.Spirit;

                dullBlackGem.Quality = 100;
                dullBlackGem.MaxQuality = 100;
                dullBlackGem.Condition = 1000;
                dullBlackGem.MaxCondition = 1000;
                dullBlackGem.Durability = 1000;
                dullBlackGem.MaxDurability = 1000;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(dullBlackGem);
            }

            #endregion

            /* Now we add some hooks to the Sir Quait we found.
				* Actually, we want to know when a player interacts with him.
				* So, we hook the right-click (interact) and the whisper method
				* of Sir Quait and set the callback method to the "TalkToXXX"
				* method. This means, the "TalkToXXX" method is called whenever
				* a player right clicks on him or when he whispers to him.
				*/
            GameEventMgr.AddHandler(argusBowman, GameLivingEvent.Interact, new DOLEventHandler(TalkToArgusBowman));
            GameEventMgr.AddHandler(argusBowman, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToArgusBowman));

            /* Now we bring to argusBowman the possibility to give this quest to players */
            argusBowman.AddQuestToGive(typeof(ArgussArrows));

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
            /* If argusBowman has not been initialized, then we don't have to remove any
             * hooks from him ;-)
             */
            if (argusBowman == null)
                return;

            /* Removing hooks works just as adding them but instead of 
             * AddHandler, we call RemoveHandler, the parameters stay the same
             */
            GameEventMgr.RemoveHandler(argusBowman, GameObjectEvent.Interact, new DOLEventHandler(TalkToArgusBowman));
            GameEventMgr.RemoveHandler(argusBowman, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToArgusBowman));

            /* Now we remove to argusBowman the possibility to give this quest to players */
            argusBowman.RemoveQuestToGive(typeof(ArgussArrows));
        }

        /* This is the method we declared as callback for the hooks we set to
         * Sir Quait. It will be called whenever a player right clicks on Sir Quait
         * or when he whispers something to him.
         */

        protected static void TalkToArgusBowman(DOLEvent e, object sender, EventArgs args)
        {
            //We get the player from the event arguments and check if he qualifies		
            GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
            if (player == null)
                return;

            if (argusBowman.CanGiveQuest(typeof(ArgussArrows), player) <= 0)
                return;

            //We also check if the player is already doing the quest
            ArgussArrows quest = player.IsDoingQuest(typeof(ArgussArrows)) as ArgussArrows;

            argusBowman.TurnTo(player);
            //Did the player rightclick on argusBowman?
            if (e == GameObjectEvent.Interact)
            {
                //We check if the player is already doing the quest
                if (quest != null)
                {
                    if (quest.Step == 3)
                        argusBowman.SayTo(player, "Welcome back. Did you have any luck?");
                    else if (quest.Step == 4)
                        argusBowman.SayTo(player, "Excellent! Did you get anymore wood?");

                    else if (quest.Step == 5)
                        argusBowman.SayTo(player, "Excellent! This will do nicely. Now wait, I have a [reward] for you somewhere around here.");
                    return;
                }
                else
                {
                    //Player hasn't the quest:
                    argusBowman.SayTo(player, "Welcome to my shop friend. I don't suppose you're here about the [job], are you?");
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
                        case "job":
                            argusBowman.SayTo(player, "Oh, I thought maybe all the money I paid to the criers had paid off. Anyhow, let me [tell] you about the job I have.");
                            break;
                        case "tell":
                            argusBowman.SayTo(player, "I am on the verge of making a new type of arrow, lighter, faster and more deadly than any other arrow ever made. The only problem is that I can't get a hold of anymore of the wood I need. You see, it's [magical].");
                            break;
                        case "magical":
                            argusBowman.SayTo(player, "The young cutpurses on the south-east hill are cutting down the last of the magical trees that reside around these parts. Ludlow didn't even know they were magical until a [sample] was brought in by another adventurer.");
                            break;
                        case "sample":
                            argusBowman.SayTo(player, "I whittled on it for a while and made it into an arrow. I shot it and it went so far, I was, frankly, astonished. I ran towards where I had lost sight of it and finally found it. Normally, you can't recover arrows, but this one was [perfect].");
                            break;
                        case "perfect":
                            argusBowman.SayTo(player, "It wasn't bent or cracked or anything! I guess now you know why I want more of that wood! So, if you go and kill a few of those cutpurses and get me a good solid stack of the wood, I'll reward you. You [up] for it?");
                            break;
                        case "up":
                            player.Out.SendCustomDialog("Will you help Argus in his quest to make a new type of arrow? [Level 4] ", new CustomDialogResponse(CheckPlayerAcceptQuest));
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
                        case "reward":
                            if (quest.Step == 5)
                            {
                                argusBowman.SayTo(player, "Ah yes. For the brave adventurer, I have this jewel I got in trade one time. I have never had a use for it, so I think you'll find it more useful than I. Good luck my friend. I'll be sure to let you know when my arrows are available.");
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
            if (player.IsDoingQuest(typeof(ArgussArrows)) != null)
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
            ArgussArrows quest = player.IsDoingQuest(typeof(ArgussArrows)) as ArgussArrows;

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
            if (argusBowman.CanGiveQuest(typeof(ArgussArrows), player) <= 0)
                return;

            if (player.IsDoingQuest(typeof(ArgussArrows)) != null)
                return;

            if (response == 0x00)
            {
                SendReply(player, "Oh well, if you change your mind, please come back!");
            }
            else
            {
                //Check if we can add the quest
                if (!argusBowman.GiveQuest(typeof(ArgussArrows), player, 1))
                    return;

                argusBowman.SayTo(player, "Great! Now, just kill two of the young cutpurses and bring me back any of the wood they have. Good luck friend.");
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
                        return "[Step #1] Search south east of Ludlow on the hill near the villiage for the young cutpurses. Slay them for pieces of the magical wood they might have.";
                    case 2:
                        return "[Step #2] You do not yet have enough wood for Argus. Slay one more young cutpurse for the wood.";
                    case 3:
                        return "[Step #3] Return to Argus with the two pieces of the magical wood. He lives in Ludlow.";
                    case 4:
                        return "[Step #3] Return to Argus with the two pieces of the magical wood. He lives in Ludlow.";
                    case 5:
                        return "[Step #3] Return to Argus with the two pieces of the magical wood. He lives in Ludlow.";


                }
                return base.Description;
            }
        }

        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = sender as GamePlayer;

            if (player == null)
                return;
            if (player.IsDoingQuest(typeof(ArgussArrows)) == null)
                return;


            if (Step == 1 && e == GameLivingEvent.EnemyKilled)
            {
                EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
                if (gArgs.Target.Name.IndexOf("young cutpurse") >= 0)
                {
                    if (Util.Chance(50))
                    {
                        player.Out.SendDialogBox(eDialogCode.SimpleWarning, 0x00, 0x00, 0x00, 0x00, eDialogType.Ok, true, "You receive a Bunch of Ludlow Magical Wood from the young cutpurse! \nYour journal has been updated.");
                        GiveItem(player, magicalWood);
                        Step = 2;
                    }
                    return;                   
                }
            }
            else if (Step == 2 && e == GameLivingEvent.EnemyKilled)
            {
                EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
                if (gArgs.Target.Name.IndexOf("young cutpurse") >= 0)
                {
                    if (Util.Chance(50))
                    {
                        player.Out.SendDialogBox(eDialogCode.SimpleWarning, 0x00, 0x00, 0x00, 0x00, eDialogType.Ok, true, "You receive a Bunch of Ludlow Magical Wood from the young cutpurse! \nYour journal has been updated.");
                        GiveItem(player, magicalWood);
                        Step = 3;
                    }
                    return;
                }
            }
            else if (Step == 3 && e == GamePlayerEvent.GiveItem)
            {
                GiveItemEventArgs gArgs = (GiveItemEventArgs)args;
                if (gArgs.Target.Name == argusBowman.Name && gArgs.Item.Id_nb == magicalWood.Id_nb)
                {
                    RemoveItem(argusBowman, player, magicalWood);
                    argusBowman.SayTo(player, "Excellent! Did you get anymore wood?");
                    Step = 4;
                    return;
                }
            }
            else if (Step == 4 && e == GamePlayerEvent.GiveItem)
            {
                GiveItemEventArgs gArgs = (GiveItemEventArgs)args;
                if (gArgs.Target.Name == argusBowman.Name && gArgs.Item.Id_nb == magicalWood.Id_nb)
                {
                    RemoveItem(argusBowman, player, magicalWood);
                    argusBowman.SayTo(player, "Excellent! This will do nicely. Now wait, I have a [reward] for you somewhere around here.");
                    Step = 5;
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
            GiveItem(argusBowman, m_questPlayer, dullBlackGem);

            m_questPlayer.GainExperience(160, 0, 0, true);
            m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 10, 30 + Util.Random(50)), "You are awarded 10 silver and some copper!");

        }

    }
}
