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
 * Date:		19. 10. 2006	
 * Directory: /scripts/quests/albion/
 *
 * Description:
 * Here follows a new quest in Albion Camelot, as per patch 1.76, available in Prydwen Keep. 
 * This particular quest is intended as a low-level replacement for the old Camdene's Components.
 * 
 * Speak to Arleigh Penn, a dye merchant, who can be found amongst all the merchants 
 * around the back of the central building.
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
     * the name: DOL.GS.Quests.Albion.ArleighsAssistant
     */

    public class ArleighsAssistant : BaseQuest
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

        protected const string questTitle = "Arleigh's Assistant";
        protected const int minimumLevel = 6;
        protected const int maximumLevel = 50;

        /* 
         * Start NPC
         */
        private static GameNPC arleighPenn = null;

        /*
         * Item templates
         */
        private static ItemTemplate spritelingToes = null;
        private static ItemTemplate wormRot = null;
        private static ItemTemplate snakeSkin = null;

        /* We need to define the constructors from the base class here, else there might be problems
         * when loading this quest...
         */
        public ArleighsAssistant()
            : base()
        {
        }

        public ArleighsAssistant(GamePlayer questingPlayer)
            : this(questingPlayer, 1)
        {
        }

        public ArleighsAssistant(GamePlayer questingPlayer, int step)
            : base(questingPlayer, step)
        {
        }

        public ArleighsAssistant(GamePlayer questingPlayer, DBQuest dbQuest)
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

            GameNPC[] npcs = WorldMgr.GetNPCsByName("Arleigh Penn", eRealm.Albion);

            /* Whops, if the npcs array length is 0 then no Sir Quait exists in
                * this users Mob Database, so we simply create one ;-)
                * else we take the existing one. And if more than one exist, we take
                * the first ...
                */
            if (npcs.Length == 0)
            {
                  arleighPenn = new GameNPC();
                  arleighPenn.Model = 8;
                  arleighPenn.Name = "Arleigh Penn";
                  if (log.IsWarnEnabled)
                      log.Warn("Could not find " + arleighPenn.Name + ", creating him ...");
                  arleighPenn.GuildName = "Dye Merchant";
                  arleighPenn.Realm = (byte)eRealm.Albion;
                  arleighPenn.CurrentRegionID = 1;
                  arleighPenn.Size = 51;
                  arleighPenn.Level = 15;
                  arleighPenn.X = 574559;
                  arleighPenn.Y = 531482;
                  arleighPenn.Z = 2896;
                  arleighPenn.Heading = 2468;

                  //You don't have to store the created mob in the db if you don't want,
                  //it will be recreated each time it is not found, just comment the following
                  //line if you rather not modify your database
                  if (SAVE_INTO_DATABASE)
                      arleighPenn.SaveIntoDatabase();


                  arleighPenn.AddToWorld();
                 
            }
            else
                arleighPenn = npcs[0];
                
            
            
            #endregion

            #region defineItems



            spritelingToes = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "spriteling_toes");
            if (spritelingToes == null)
             {
                 if (log.IsWarnEnabled)
                     log.Warn("Could not find Spriteling Toes, creating it ...");
                 spritelingToes = new ItemTemplate();
                 spritelingToes.Object_Type = 0;
                 spritelingToes.Id_nb = "spriteling_toes";
                 spritelingToes.Name = "Spriteling Toes";
                 spritelingToes.Level = 1;
                 spritelingToes.Model = 508; 
                 spritelingToes.IsDropable = false;
                 spritelingToes.IsPickable = false;
                 spritelingToes.Weight = 2;

                 //You don't have to store the created item in the db if you don't want,
                 //it will be recreated each time it is not found, just comment the following
                 //line if you rather not modify your database
                 if (SAVE_INTO_DATABASE)
                     GameServer.Database.AddNewObject(spritelingToes);
             }

             wormRot = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "worm_rot");
             if (wormRot == null)
             {
                 if (log.IsWarnEnabled)
                     log.Warn("Could not find Worm Rot, creating it ...");
                 wormRot = new ItemTemplate();
                 wormRot.Object_Type = 0;
                 wormRot.Id_nb = "worm_rot";
                 wormRot.Name = "Worm Rot";
                 wormRot.Level = 1;
                 spritelingToes.Model = 102; //TODO Model-ID 
                 wormRot.IsDropable = false;
                 wormRot.IsPickable = false;

                 //You don't have to store the created item in the db if you don't want,
                 //it will be recreated each time it is not found, just comment the following
                 //line if you rather not modify your database
                 if (SAVE_INTO_DATABASE)
                     GameServer.Database.AddNewObject(wormRot);
             }

             snakeSkin = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "green_skin");
             if (snakeSkin == null)
             {
                 if (log.IsWarnEnabled)
                     log.Warn("Could not find Green Skin, creating it ...");
                 snakeSkin = new ItemTemplate();
                 snakeSkin.Object_Type = 0;
                 snakeSkin.Id_nb = "green_skin";
                 snakeSkin.Name = "Green Skin";
                 snakeSkin.Level = 1;
                 snakeSkin.Model = 107; 
                 snakeSkin.IsDropable = false;
                 snakeSkin.IsPickable = false;
                 snakeSkin.Weight = 3;

                 //You don't have to store the created item in the db if you don't want,
                 //it will be recreated each time it is not found, just comment the following
                 //line if you rather not modify your database
                 if (SAVE_INTO_DATABASE)
                     GameServer.Database.AddNewObject(snakeSkin);
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

            GameEventMgr.AddHandler(arleighPenn, GameLivingEvent.Interact, new DOLEventHandler(TalkToArleighPenn));
            GameEventMgr.AddHandler(arleighPenn, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToArleighPenn));

            /* Now we bring to arleighPenn the possibility to give this quest to players */
            arleighPenn.AddQuestToGive(typeof(ArleighsAssistant));

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
            /* If arleighPenn has not been initialized, then we don't have to remove any
             * hooks from him ;-)
             */
            if (arleighPenn == null)
                return;

            /* Removing hooks works just as adding them but instead of 
             * AddHandler, we call RemoveHandler, the parameters stay the same
             */

			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

            GameEventMgr.RemoveHandler(arleighPenn, GameObjectEvent.Interact, new DOLEventHandler(TalkToArleighPenn));
            GameEventMgr.RemoveHandler(arleighPenn, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToArleighPenn));

            /* Now we remove to arleighPenn the possibility to give this quest to players */
            arleighPenn.RemoveQuestToGive(typeof(ArleighsAssistant));
        }

        /* This is the method we declared as callback for the hooks we set to
         * Sir Quait. It will be called whenever a player right clicks on Sir Quait
         * or when he whispers something to him.
         */

        protected static void TalkToArleighPenn(DOLEvent e, object sender, EventArgs args)
        {
            //We get the player from the event arguments and check if he qualifies		
            GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
            if (player == null)
                return;

            if (arleighPenn.CanGiveQuest(typeof(ArleighsAssistant), player) <= 0)
                return;

            //We also check if the player is already doing the quest
            ArleighsAssistant quest = player.IsDoingQuest(typeof(ArleighsAssistant)) as ArleighsAssistant;

            arleighPenn.TurnTo(player);
            //Did the player rightclick on arleighPenn?
            if (e == GameObjectEvent.Interact)
            {
                //We check if the player is already doing the quest
                if (quest != null)
                {
                    //If the player is already doing the quest, we look if he has the items:
                    if ((quest.Step == 4) || (quest.Step == 5))
                    {
                        arleighPenn.SayTo(player, "Welcome back, my friend " + player.Name + ". I can tell already you've had luck finding the items I requested. How about you hand over those spriteling toes? Arthw is getting very low on the light green dye.");
                        quest.Step = 5;
                    }
                    if (quest.Step == 6)
                        arleighPenn.SayTo(player, "Now, do you have the worm rot for me? I'm getting very low on dark gray dye.");
                    if (quest.Step == 7)
                        arleighPenn.SayTo(player, "Now for the final item, the green skin.");
                    return;
                }
                else
                {
                    SendSystemMessage(player, "Arleigh does a little dance of celebration.");
                    //Player hasn't the quest:
                    arleighPenn.SayTo(player, "Greetings and salutations, my fine friend. I am the one, the only, Arleigh, dyemaster extraordinaire! I have just finished developing a vast collection of [new dyes] through my refinement of the arts!");
                    return;
                }
            }
            // The player whispered to Sir Quait (clicked on the text inside the [])
            else if (e == GameLivingEvent.WhisperReceive)
            {
                WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs)args;

                //We also check if the player is already doing the quest
                if (quest == null)
                {
                    switch (wArgs.Text)
                    {
                        case "new dyes":
                            arleighPenn.SayTo(player, "Yes, new dyes! Tired of that old orange, that boring blue, that tired worn-out green? Well, take a look at what my friend Arthw and I have! We [carry the finest] selection of dyes in the land.");
                            break;
                        case "carry the finest":
                            arleighPenn.SayTo(player, "No one else in this area carries dyes as fine as ours! You can believe me, I am an honest man. In fact, I will prove how honest I am to you. I will give you a dye free if you help me with [a small problem].");
                            break;
                        case "a small problem":
                            arleighPenn.SayTo(player, "When I was gathering materials to create these new, exciting dyes, I only gathered enough to create a test lot. That lot is almost gone, and Arthw and I have been so busy that neither of us has had time to go collecting. If you would be so kind as to gather a few things for me, I will give you a pot of dye free of charge, plus throw in a few coins for your trouble. Does that [sound good] to you?");
                            break;
                        //If the player offered his "help", we send the quest dialog now!
                        case "sound good":
							player.Out.SendQuestSubscribeCommand(arleighPenn, QuestMgr.GetIDForQuestType(typeof(ArleighsAssistant)), "Will you help Arleigh find the components he needs for new dyes?");
                            break;
                    }
                }
                else
                {
                    switch (wArgs.Text)
                    {
                        case "abort":
                            player.Out.SendCustomDialog("Do you really want to abort this quest, \nall items gained during quest will be lost?", new CustomDialogResponse(CheckPlayerAbortQuest));
                            break;
                        case "list them":
                            arleighPenn.SayTo(player, "I need spriteling toes from a river spriteling. When you dry them out, grind them up, and mix them with other components I shall keep secret, they make a wonderful light green dye that Arthw sells. Now, for the [next item].");
                            break;
                        case "next item":
                            arleighPenn.SayTo(player, "The dark gray dye I make comes from none other than worm rot. Can you believe it? I have developed a special process that extracts and purifies the color for a true dark gray. The worm rot can be found on undead fildhs, what else? Once you have the spriteling toes and the worm rot, search for an [emerald snake].");
                            break;
                        case "emerald snake":
                            arleighPenn.SayTo(player, "These snakes are born green and spend all day in the green grass. I need some of that snake skin. When it is refined, it makes a fantastic royal green color. There are your three items. Return to me when you have them all!.");
                            SendSystemMessage(player, "Arleigh bows to you.");
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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(ArleighsAssistant)))
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
            if (player.IsDoingQuest(typeof(ArleighsAssistant)) != null)
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
            ArleighsAssistant quest = player.IsDoingQuest(typeof(ArleighsAssistant)) as ArleighsAssistant;

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
            if (arleighPenn.CanGiveQuest(typeof(ArleighsAssistant), player) <= 0)
                return;

            if (player.IsDoingQuest(typeof(ArleighsAssistant)) != null)
                return;

            if (response == 0x00)
            {
                SendReply(player, "Oh well, if you change your mind, please come back!");
            }
            else
            {
                //Check if we can add the quest
                if (!arleighPenn.GiveQuest(typeof(ArleighsAssistant), player, 1))
                    return;

                arleighPenn.SayTo(player, "Wonderful! Now I only need three things really simple things to make these dyes. Each one can be found around here. Shall I [list them] for you?");
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
                        return "[Step #1] Search the river banks in Camelot Hills for a river spriteling. Get its toes for Arleigh.";
                    case 2:
                        return "[Step #2] Find an undead fildh at the circle of stones near Darkness Falls. Get some worm rot from it for Arleigh.";
                    case 3:
                        return "[Step #3] Get some green skin from an emerald snake. They can be found in the grass one the other side of Salisbury Bridge.";
                    case 4:
                        return "[Step #4] Return to Dyemaster Arleigh Penn in Prydwen Keep. Take the spriteling toes, worm rot, and green skin with you.";
                    case 5: 
                        return "[Step #5] Turn in the spriteling toes to Arleigh.";
                    case 6:
                        return "[Step #6] Turn in the worm rot to Arleigh.";
                    case 7:
                        return "[Step #7] Turn in the green skin to Arleigh.";
                    }
                return base.Description;
            }
        }

        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = sender as GamePlayer;

            if (player == null)
                return;
            if (player.IsDoingQuest(typeof(ArleighsAssistant)) == null)
                return;

            
            if (Step == 1 && e == GameLivingEvent.EnemyKilled)
            {
                EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
                if (gArgs.Target.Name.IndexOf("spriteling") >= 0)
                {
                    SendSystemMessage("You receive the Spriteling Toes from the river spriteling!");
                    spritelingToes.Name = gArgs.Target.GetName(1, true) + " toe";
                    GiveItem(player, spritelingToes);
                    Step = 2;
                    return;
                }
            }
            else if (Step == 2 && e == GameLivingEvent.EnemyKilled)
            {
                EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
                if (gArgs.Target.Name.IndexOf("undead filidh") >= 0)
                {
                    SendSystemMessage("You receive Worm Rot from the undead filidh!");
                    GiveItem(player, wormRot);
                    Step = 3;
                    return;
                }
            }
            else if (Step == 3 && e == GameLivingEvent.EnemyKilled)
            {
                EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
                if (gArgs.Target.Name.IndexOf("emerald snake") >= 0)
                {
                    SendSystemMessage("You receive the Green Skin from the emerald snake!");
                    snakeSkin.Name = gArgs.Target.GetName(1, true) + " Skin";
                    GiveItem(player, snakeSkin);
                    Step = 4;
                    return;
                }
            }
            else if (Step == 5 && e == GamePlayerEvent.GiveItem)
            {
                GiveItemEventArgs gArgs = (GiveItemEventArgs)args;
                if (gArgs.Target.Name == arleighPenn.Name && gArgs.Item.Id_nb == spritelingToes.Id_nb)
                {
                    RemoveItem(arleighPenn, player, spritelingToes);
                    arleighPenn.SayTo(player, "Ah, these toes are perfect, and quite large too! Arthw will be able to make this into quite a bit of dye.");
                    SendSystemMessage("Arleigh hands the spriteling toes over to Arthw, who begins to prepare them.");
                    arleighPenn.SayTo(player, "He'll get to work on that right now. Now, do you have the worm rot for me? I'm getting very low on dark gray dye.");
                    Step = 6;
                    return;
                }
            }
            else if (Step == 6 && e == GamePlayerEvent.GiveItem)
            {
                GiveItemEventArgs gArgs = (GiveItemEventArgs)args;
                if (gArgs.Target.Name == arleighPenn.Name && gArgs.Item.Id_nb == wormRot.Id_nb)
                {
                    RemoveItem(arleighPenn, player, wormRot);
                    arleighPenn.SayTo(player, "Oh, this is superb rot! It will make the most brilliant shade of gray. I am very pleased! Now for the final item, the green skin.");
                    Step = 7;
                    return;
                }
            }
            else if (Step == 7 && e == GamePlayerEvent.GiveItem)
            {
                GiveItemEventArgs gArgs = (GiveItemEventArgs)args;
                if (gArgs.Target.Name == arleighPenn.Name && gArgs.Item.Id_nb == snakeSkin.Id_nb)
                {
                    RemoveItem(arleighPenn, player, snakeSkin);
                    arleighPenn.SayTo(player, "Excellent! This skin will keep me in business for quite a while. I am very pleased with your work. Now, here is the reward I promised. I hope the dye meets with your approval. I worked very hard to prepare it especially for you! Fare thee well, " + player.Name + "!");
                    Step = 7;
                    FinishQuest();
                    return;
                }
            }
        }

        public override void AbortQuest()
        {
            base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

            RemoveItem(m_questPlayer, spritelingToes, false);
            RemoveItem(m_questPlayer, wormRot, false);
            RemoveItem(m_questPlayer, snakeSkin, false);
        }

        private static String getRandomDyeColor()
        {
            int number = Util.Random(9);
            switch (number)
            { 
                case 0:
                    return "blue";
                case 1:
                    return "brown";
                case 2:
                    return "gray";
                case 3:
                    return "green";
                case 4:
                    return "orange";
                case 5:
                    return "purple";
                case 6:
                    return "red";
                case 7:
                    return "teal";
                case 8:
                    return "turquoise";
                case 9:
                    return "yellow";
                default: return "";
            }

        }

        public override void FinishQuest()
        {
            base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

            //Give reward to player here ...
            ItemTemplate dye = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "light_" + getRandomDyeColor() + "_cloth_dye");
            if (dye != null)
                GiveItem(arleighPenn, m_questPlayer, dye);

            m_questPlayer.GainExperience(2560, 0, 0, true);
            m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 0, 30 + Util.Random(50)), "You recieve {0} for your service.");

        }

    }
}
