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
 * Date:		20. 10. 2006	
 * Directory: /scripts/quests/albion/
 *
 * Description:
 * Brief Walkthrough:
 * 1) Speak with Palune at loc=24982,47250 Camelot Hills.
 * 2) Take the Enchanted Halberd to Guard Cynon, loc=29028,8615 Salisbury Plains, West Downs.
 * 3) Return to Palune and her the Enchanted Halberd when she asks for it to receive your reward. 
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
     * the name: DOL.GS.Quests.Albion.Disenchanted
     */

    public class Disenchanted : BaseQuest
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

        protected const string questTitle = "Disenchanted";
        protected const int minimumLevel = 7;
        protected const int maximumLevel = 10;

        /* 
         * Start NPC
         */
        private static GameNPC palune = null;

        /* 
        * NPCs
        */
        private static GameNPC guardCynon = null;


        /*
         * Item templates
         */
        private static ItemTemplate enchantedHalberd = null;

        /* We need to define the constructors from the base class here, else there might be problems
         * when loading this quest...
         */
        public Disenchanted()
            : base()
        {
        }

        public Disenchanted(GamePlayer questingPlayer)
            : this(questingPlayer, 1)
        {
        }

        public Disenchanted(GamePlayer questingPlayer, int step)
            : base(questingPlayer, step)
        {
        }

        public Disenchanted(GamePlayer questingPlayer, DBQuest dbQuest)
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

            GameNPC[] npcs = WorldMgr.GetNPCsByName("Palune", eRealm.Albion);
            if (npcs.Length == 0)
            {
                palune = new GameNPC();
                palune.Model = 81;
                palune.Name = "Palune";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + palune.Name + ", creating him ...");
                palune.GuildName = "Enchanter";
                palune.Realm = eRealm.Albion;
                palune.CurrentRegionID = 1;
                palune.Size = 52;
                palune.Level = 27;
                palune.X = 573735;
                palune.Y = 530508;
                palune.Z = 2957;
                palune.Heading = 3083;

                //You don't have to store the created mob in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    palune.SaveIntoDatabase();


                palune.AddToWorld();
            }
            else
                palune = npcs[0];

            npcs = WorldMgr.GetNPCsByName("Guard Cynon", eRealm.Albion);
            if (npcs.Length == 0)
            {
                guardCynon = new GameNPC();
                guardCynon.Model = 28;
                guardCynon.Name = "Guard Cynon";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + guardCynon.Name + ", creating him ...");
                guardCynon.GuildName = "Part of " + questTitle + " Quest";
                guardCynon.Realm = eRealm.Albion;
                guardCynon.CurrentRegionID = 1;
                guardCynon.Size = 51;
                guardCynon.Level = 25;
                guardCynon.X = 577920;
                guardCynon.Y = 557362;
                guardCynon.Z = 2159;
                guardCynon.Heading = 3982;
                guardCynon.MaxSpeedBase = 200;

                //You don't have to store the created mob in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    guardCynon.SaveIntoDatabase();


                guardCynon.AddToWorld();
            }
            else
                guardCynon = npcs[0];

            #endregion

            #region defineItems



            enchantedHalberd = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "enchanted_halberd");
            if (enchantedHalberd == null)
            {
                if (log.IsWarnEnabled)
                    log.Warn("Could not find Elder Wood, creating it ...");
                enchantedHalberd = new ItemTemplate();
                enchantedHalberd.Object_Type = 0;
                enchantedHalberd.TemplateID = "enchanted_halberd";
                enchantedHalberd.Name = "Enchanted Halberd";
                enchantedHalberd.Level = 1;
                enchantedHalberd.Model = 67;
                enchantedHalberd.IsDropable = false;
                enchantedHalberd.IsPickable = false;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(enchantedHalberd);
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

            GameEventMgr.AddHandler(palune, GameLivingEvent.Interact, new DOLEventHandler(TalkToPalune));
            GameEventMgr.AddHandler(palune, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToPalune));

            GameEventMgr.AddHandler(guardCynon, GameLivingEvent.Interact, new DOLEventHandler(TalkToGuardCynon));
            GameEventMgr.AddHandler(guardCynon, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToGuardCynon));

            /* Now we bring to palune the possibility to give this quest to players */
            palune.AddQuestToGive(typeof(Disenchanted));

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
            if (palune != null)
            {

                /* Removing hooks works just as adding them but instead of 
                 * AddHandler, we call RemoveHandler, the parameters stay the same
                 */

				GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
				GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

                GameEventMgr.RemoveHandler(palune, GameObjectEvent.Interact, new DOLEventHandler(TalkToPalune));
                GameEventMgr.RemoveHandler(palune, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToPalune));

                GameEventMgr.RemoveHandler(guardCynon, GameObjectEvent.Interact, new DOLEventHandler(TalkToGuardCynon));
                GameEventMgr.RemoveHandler(guardCynon, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToGuardCynon));

				/* Now we remove to arleighPenn the possibility to give this quest to players */
				palune.RemoveQuestToGive(typeof(Disenchanted));
            }
        }

        /* This is the method we declared as callback for the hooks we set to
         * palune. It will be called whenever a player right clicks on Sir Quait
         * or when he whispers something to him.
         */

        protected static void TalkToPalune(DOLEvent e, object sender, EventArgs args)
        {
            //We get the player from the event arguments and check if he qualifies		
            GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
            if (player == null)
                return;

            if (palune.CanGiveQuest(typeof(Disenchanted), player) <= 0)
                return;

            //We also check if the player is already doing the quest
            Disenchanted quest = player.IsDoingQuest(typeof(Disenchanted)) as Disenchanted;

            palune.TurnTo(player);
            //Did the player rightclick on palune?
            if (e == GameObjectEvent.Interact)
            {
                //We check if the player is already doing the quest
                if (quest != null)
                {
                    //If the player is already doing the quest, we look if he has the items:
                    if (quest.Step == 3)
                        palune.SayTo(player, "You've returned! Excell...hey, wait a minute. Why do you still have the halberd with you? Were you not able to find Guard Cynon or West [Downs]?");

                    return;
                }
                else
                {
                    //Player hasn't the quest:
                    palune.SayTo(player, "Greetings. I hope your day has gone better than mine. It seems the courier I normally use has fallen ill, and I've been left without anyone to deliver this equipment. I'm able to get some of the weapons and armor to my local [customers].");
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
                        case "customers":
                            palune.SayTo(player, "Unfortunately, I also have a number of deliveries to customers in other parts of the realm. I'd ask one of the squires or the stableboys to deliver some of these, but everyone seems to have their own [responsibilities].");
                            break;
                        case "responsibilities":
                            palune.SayTo(player, "If I paid you for your time, would you be willing to deliver a polearm I recently enchanted for a member of the Defenders of Albion?");
							player.Out.SendQuestSubscribeCommand(palune, QuestMgr.GetIDForQuestType(typeof(Disenchanted)), "Will you deliver the polearm for Palune? [Levels 7-10]");
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
                        case "Downs":
                            if (quest.Step == 3)
                            {
                                SendSystemMessage(player, "You tell Palune that you delivered the polearm to Guard Cynon, as asked, but he was not satisfied with it. You describe what happened when he tried to wield the weapon. Palune's face pales as you relate the story.");
                                palune.SayTo(player, "Oh my! I couldn't have done that, could I? Oh, no, what if I did put the wrong enchantment on it? I'll bet Guard Cynon is so angry at me...May I have the polearm back?");
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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(Disenchanted)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}


        protected static void TalkToGuardCynon (DOLEvent e, object sender, EventArgs args)
        {
            //We get the player from the event arguments 	
            GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
            if (player == null)
                return;

            //We also check if the player is already doing the quest
            Disenchanted quest = player.IsDoingQuest(typeof(Disenchanted)) as Disenchanted;
            if (quest == null)
                return;

            guardCynon.TurnTo(player);
            //Did the player rightclick on guardCynon?
            if (e == GameObjectEvent.Interact)
            {
                if (quest.Step == 1)
                    guardCynon.SayTo(player, "Good day, you must be the courier from Palune. I've been waiting for her to finish with that polearm for quite some time. May I have the halberd?");
                if (quest.Step == 2)
                    guardCynon.SayTo(player, "This halberd is talking to me! What kind of useless enchantment is this? I asked Palune to put a simple accuracy enchantment upon this weapon, not to make it [chatter] at me!");

                return;                
            }
            // The player whispered to Sir Quait (clicked on the text inside the [])
            else if (e == GameLivingEvent.WhisperReceive)
            {
                WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs)args;

                if (quest.Step == 2)
                {
                    switch (wArgs.Text)
                    {
                        case "chatter":
                            guardCynon.SayTo(player, "This is downright annoying, and worse, the weapon is useless to me! Take this back to Palune and let her know that if she doesn't correct this, I'll see to it that the Defenders of Albion find a new enchanter.");
                            GiveItem(guardCynon, player, enchantedHalberd);
                            quest.Step = 3;
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
            if (player.IsDoingQuest(typeof(Disenchanted)) != null)
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
            Disenchanted quest = player.IsDoingQuest(typeof(Disenchanted)) as Disenchanted;

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
            if (palune.CanGiveQuest(typeof(Disenchanted), player) <= 0)
                return;

            if (player.IsDoingQuest(typeof(Disenchanted)) != null)
                return;

            if (response == 0x00)
            {
                SendReply(player, "Oh well, if you change your mind, please come back!");
            }
            else
            {
                //Check if we can add the quest
                if (!palune.GiveQuest(typeof(Disenchanted), player, 1))
                    return;

                palune.SayTo(player, "Thank you so much for agreeing to help! Here's the halberd. Make sure you give it to Guard Cynon at West Downs and no one else. Guard Cynon would kill me if anything happened to this weapon.");
                palune.SayTo(player, "Go south across the bridge and continue to follow the road south into the Salisbury Plains. It will eventually turn west, and West Downs will be on the right side of the road. Be careful with that weapon!");
                
                GiveItem(palune, player, enchantedHalberd);
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
                        return "[Step #1] Travel east to the Prydwen Bridge, go south across the bridge and follow the road until you come to West Downs. Find Guard Cynon and tell him you have been asked to deliver his newly-enchanted polearm.";
                    case 2:
                        return "[Step #2] Continue talking to Guard Cynon.";
                    case 3:
                        return "[Step #2] Return to Prydwen Keep with the defective polearm and give it back to Palune when she asks for it.";

                }
                return base.Description;
            }
        }

        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = sender as GamePlayer;

            if (player == null)
                return;
            if (player.IsDoingQuest(typeof(Disenchanted)) == null)
                return;

            if (e == GamePlayerEvent.GiveItem)
            {
                GiveItemEventArgs gArgs = (GiveItemEventArgs)args;
                switch (Step)
                {
                    case 1:
                        if (gArgs.Target.Name == guardCynon.Name && gArgs.Item.TemplateID == enchantedHalberd.TemplateID)
                        {
                            RemoveItem(guardCynon, player, enchantedHalberd);
                            SendSystemMessage(player, "Guard Cynon accepts the polearm and inspects it closely, falling into a combat stance. Before he can swing the weapon, a disembodied voice strikes up a conversation with him. Cynon's eyes widen and his jaw drops.");
                            guardCynon.SayTo(player, "This halberd is talking to me! What kind of useless enchantment is this? I asked Palune to put a simple accuracy enchantment upon this weapon, not to make it [chatter] at me!");
                            Step = 2;
                        }
                        return;
                    case 3:
                        if (gArgs.Target.Name == palune.Name && gArgs.Item.TemplateID == enchantedHalberd.TemplateID)
                        {
                            RemoveItem(palune, player, enchantedHalberd);
                            palune.SayTo(player, "Thank you. Oh, I see what I did wrong now. He's right, I'm sure it was talking to him nonstop when he tried to use it. I'll have to find the time to fix it and someone else to deliver it. Well, thank you for bringing it back, and here's your payment.");
                            FinishQuest();
                        }
                        return;

                }
            }

        }

        public override void AbortQuest()
        {
            base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...
            RemoveItem(m_questPlayer, enchantedHalberd, false);
        }

        public override void FinishQuest()
        {
            base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

            //Give reward to player here ...
            m_questPlayer.GainExperience((long)((m_questPlayer.ExperienceForNextLevel - m_questPlayer.ExperienceForCurrentLevel) / 20), true);
            m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 1, 49 + Util.Random(50)), "You are awarded 1 silver and some copper!");

        }

    }
}
