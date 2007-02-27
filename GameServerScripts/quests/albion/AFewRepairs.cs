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
 * Visit Brice Yarley at their farm in Western Cornwall (loc=18.0K/49.0K)
 * Go kill 2 Elder Beech. The journal describes their position fairly accurately.
 * Return to Patrick Yarley with the 2 pieces of Elder Wood.
 * Go kill a Moor Boogey and then return with the Teeth. 
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
     * the name: DOL.GS.Quests.Albion.AFewRepairs
     */

    public class AFewRepairs : BaseQuest
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

        protected const string questTitle = "A Few Repairs";
        protected const int minimumLevel = 35;
        protected const int maximumLevel = 37;

        /* 
         * Start NPC
         */
        private static GameNPC briceYarley = null;

        /*
         * NPCs
         */
        private static GameNPC patrickYarley = null;

        /*
         * Item templates
         */
        private static ItemTemplate elderWood = null;
        private static ItemTemplate boogeyTeeth = null;

        /* We need to define the constructors from the base class here, else there might be problems
         * when loading this quest...
         */
        public AFewRepairs()
            : base()
        {
        }

        public AFewRepairs(GamePlayer questingPlayer)
            : this(questingPlayer, 1)
        {
        }

        public AFewRepairs(GamePlayer questingPlayer, int step)
            : base(questingPlayer, step)
        {
        }

        public AFewRepairs(GamePlayer questingPlayer, DBQuest dbQuest)
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

            GameNPC[] npcs = WorldMgr.GetNPCsByName("Brice Yarley", eRealm.Albion);
            if (npcs.Length == 0)
            {
                briceYarley = new GameNPC();
                briceYarley.Model = 10;
                briceYarley.Name = "Brice Yarley";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + briceYarley.Name + ", creating him ...");
                briceYarley.Realm = (byte)eRealm.Albion;
                briceYarley.CurrentRegionID = 1;
                briceYarley.Size = 51;
                briceYarley.Level = 43;
                briceYarley.X = 370236;
                briceYarley.Y = 679755;
                briceYarley.Z = 5558;
                briceYarley.Heading = 2468;
                briceYarley.MaxSpeedBase = 191;

                //You don't have to store the created mob in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    briceYarley.SaveIntoDatabase();


                briceYarley.AddToWorld();
            }
            else
                briceYarley = npcs[0];

            npcs = WorldMgr.GetNPCsByName("Patrick Yarley", eRealm.Albion);
            if (npcs.Length == 0)
            {
                patrickYarley = new GameNPC();
                patrickYarley.Model = 9;
                patrickYarley.Name = "Patrick Yarley";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + patrickYarley.Name + ", creating him ...");
                patrickYarley.Realm = (byte)eRealm.Albion;
                patrickYarley.CurrentRegionID = 1;
                patrickYarley.Size = 51;
                patrickYarley.Level = 43;
                patrickYarley.X = 371752;
                patrickYarley.Y = 680486;
                patrickYarley.Z = 5595;
                patrickYarley.Heading = 0;
                patrickYarley.MaxSpeedBase = 200;

                //You don't have to store the created mob in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    patrickYarley.SaveIntoDatabase();


                patrickYarley.AddToWorld();
            }
            else
                patrickYarley = npcs[0];



            #endregion

            #region defineItems



            elderWood = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "elder_wood");
            if (elderWood == null)
            {
                if (log.IsWarnEnabled)
                    log.Warn("Could not find Elder Wood, creating it ...");
                elderWood = new ItemTemplate();
                elderWood.Object_Type = 0;
                elderWood.Id_nb = "elder_wood";
                elderWood.Name = "Elder Wood";
                elderWood.Level = 1;
                elderWood.Model = 520; 
                elderWood.IsDropable = false;
                elderWood.IsPickable = false;
                elderWood.Weight = 5;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(elderWood);
            }

            boogeyTeeth = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "boogey_teeth");
            if (boogeyTeeth == null)
            {
                if (log.IsWarnEnabled)
                    log.Warn("Could not find Boogey Teeth, creating it ...");
                boogeyTeeth = new ItemTemplate();
                boogeyTeeth.Object_Type = 0;
                boogeyTeeth.Id_nb = "boogey_teeth";
                boogeyTeeth.Name = "Boogey Teeth";
                boogeyTeeth.Level = 1;
                boogeyTeeth.Model = 106; 
                boogeyTeeth.IsDropable = false;
                boogeyTeeth.IsPickable = false;
                boogeyTeeth.Weight = 4;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(boogeyTeeth);
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

            GameEventMgr.AddHandler(briceYarley, GameLivingEvent.Interact, new DOLEventHandler(TalkToBriceYarley));
            GameEventMgr.AddHandler(briceYarley, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToBriceYarley));

            GameEventMgr.AddHandler(patrickYarley, GameLivingEvent.Interact, new DOLEventHandler(TalkToPatrickYarley));
            GameEventMgr.AddHandler(patrickYarley, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToPatrickYarley));


            
            /* Now we bring to briceYarley the possibility to give this quest to players */
            briceYarley.AddQuestToGive(typeof(AFewRepairs));

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
            if (briceYarley != null)
            {

                /* Removing hooks works just as adding them but instead of 
                 * AddHandler, we call RemoveHandler, the parameters stay the same
                 */
				GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
				GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

                GameEventMgr.RemoveHandler(briceYarley, GameObjectEvent.Interact, new DOLEventHandler(TalkToBriceYarley));
                GameEventMgr.RemoveHandler(briceYarley, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToBriceYarley));

				/* Now we remove to arleighPenn the possibility to give this quest to players */
				briceYarley.RemoveQuestToGive(typeof(AFewRepairs));
            }
            if (patrickYarley != null)
            {
                GameEventMgr.RemoveHandler(patrickYarley, GameObjectEvent.Interact, new DOLEventHandler(TalkToPatrickYarley));
                GameEventMgr.RemoveHandler(patrickYarley, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToPatrickYarley));
            }
        }

        /* This is the method we declared as callback for the hooks we set to
         * briceYarley. It will be called whenever a player right clicks on Sir Quait
         * or when he whispers something to him.
         */

        protected static void TalkToBriceYarley(DOLEvent e, object sender, EventArgs args)
        {
            //We get the player from the event arguments and check if he qualifies		
            GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
            if (player == null)
                return;

            if (briceYarley.CanGiveQuest(typeof(AFewRepairs), player) <= 0)
                return;

            //We also check if the player is already doing the quest
            AFewRepairs quest = player.IsDoingQuest(typeof(AFewRepairs)) as AFewRepairs;

            briceYarley.TurnTo(player);
            //Did the player rightclick on briceYarley?
            if (e == GameObjectEvent.Interact)
            {
                //We check if the player is already doing the quest
                if (quest != null)
                {
                    //If the player is already doing the quest, we look if he has the items:
                    if (quest.Step == 1)
                        briceYarley.SayTo(player, "There are some [elder beech] creatures to the north just inside the tree line beyond the fallen tower. If you cut down a few of them they should yield enough wood for the project.");
                    
                    return;
                }
                else
                {
                    //Player hasn't the quest:
                    briceYarley.SayTo(player, "Welcome to Cornwall, "+player.CharacterClass.Name+". My family has a bit of a [problem] and we would be grateful if you could lend a hand.");
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
                        case "problem":
                            briceYarley.SayTo(player, "We Yarleys raise these giant rooters which we bring to markets all across Albion. Perhaps you have tried some of our sausages at Ye Mug in Camelot? Well anyway we do a pretty good business here and the hours aren't too bad. It is a satisfying life but one that does not leave much time for [mastering the arts] of magic or swordplay.");
                            break;
                        case "mastering the arts":
                            briceYarley.SayTo(player, "Raising your livestock a stone's throw from a dungeon of undead and other unsavory critters has its drawbacks as you might imagine. We are constantly on the lookout for [moor boogeys] that seem to have a genuine dislike for our rooters. Now our animals can hold their own in a fight, but we have enough to do around here without having to heal the hogs every other day.");
                            break;
                        case "moor boogeys":
                            briceYarley.SayTo(player, "We have tried to kill off these pests but there always seems to be more of them. We [maintain this fence] to keep rooters in and the boogeys out.");
                            break;
                        case "maintain this fence":
                            briceYarley.SayTo(player, "This old barrier is in need of some repair but we are a bit low on lumber at the moment. Would you be interested in [gathering] some supplies for our fence?");
                            break;
                        //If the player offered his "help", we send the quest dialog now!
                        case "gathering":
							player.Out.SendQuestSubscribeCommand(briceYarley, QuestMgr.GetIDForQuestType(typeof(AFewRepairs)), "Will you help the Yarleys with a few repairs? [Levels 35-37]");
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
                        case "elder beech":
                            if (quest.Step == 1)
                            {
                                briceYarley.SayTo(player, "Bring the lumber to my brother, Patrick. You will find him along the fence to the east. I will make sure he knows to expect your return.");
                                quest.Step = 2;
                            }
                            break;
                    }
                }

            }
        }

        /* This is the method we declared as callback for the hooks we set to
         * patrickYarley. It will be called whenever a player right clicks on Sir Quait
         * or when he whispers something to him.
         */

        protected static void TalkToPatrickYarley(DOLEvent e, object sender, EventArgs args)
        {
            //We get the player from the event arguments and check if he qualifies		
            GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
            if (player == null)
                return;

            AFewRepairs quest = player.IsDoingQuest(typeof(AFewRepairs)) as AFewRepairs;
            if (quest == null)
                return;
            patrickYarley.TurnTo(player);
            //Did the player rightclick on patrickYarley?
            if (e == GameObjectEvent.Interact)
            {
                //If the player is already doing the quest, we look if he has the items:
                if (quest.Step == 4)
                    patrickYarley.SayTo(player, "That was quick. Do you have the wood that my brother requested? Please give it to me now.");
                if (quest.Step == 5)
                    patrickYarley.SayTo(player, "Were you able to get any more wood?");
                if (quest.Step == 6)
                    patrickYarley.SayTo(player, "That is more than enough wood. There is one other thing I will need to complete this project. Would you be able to help out with this [last errand]?");
                if ((quest.Step == 7) || (quest.Step == 8))
                    patrickYarley.SayTo(player, "Do you have the boogey teeth?");
                return;
            } 
            // The player whispered to Sir Quait (clicked on the text inside the [])
            else if (e == GameLivingEvent.WhisperReceive)
            {
                WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs)args;

                switch (wArgs.Text)
                {
                    case "last errand":
                        if (quest.Step == 6)
                            patrickYarley.SayTo(player, "The last thing I need is something to hold the boards in place. You may find this ironic but the best place to find these spikes is from the very creatures we are trying to keep out with this fence. It turns out that their long teeth make excellent nails for holding the elder wood in place. If you can slay one of these moor boogeys then it should provide enough for the job at hand. Return to me with these teeth and your task will be [complete].");
                        break;
                    case "complete":
                        if (quest.Step == 6)
                        {
                            patrickYarley.SayTo(player, "You may find the moor boogeys roaming the hill to the east.");
                            quest.Step = 7;
                        }
                            break;
                }

            }
        }

		protected static void SubscribeQuest(DOLEvent e, object sender, EventArgs args)
		{
			QuestEventArgs qargs = args as QuestEventArgs;
			if (qargs == null)
				return;

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(AFewRepairs)))
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
            if (player.IsDoingQuest(typeof(AFewRepairs)) != null)
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
            AFewRepairs quest = player.IsDoingQuest(typeof(AFewRepairs)) as AFewRepairs;

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
            if (briceYarley.CanGiveQuest(typeof(AFewRepairs), player) <= 0)
                return;

            if (player.IsDoingQuest(typeof(AFewRepairs)) != null)
                return;

            if (response == 0x00)
            {
                SendReply(player, "Oh well, if you change your mind, please come back!");
            }
            else
            {
                //Check if we can add the quest
                if (!briceYarley.GiveQuest(typeof(AFewRepairs), player, 1))
                    return;

                briceYarley.SayTo(player, "Excellent, there are some [elder beech] creatures to the north just inside the tree line beyond the fallen tower. If you cut down a few of them they should yield enough wood for the project.");
                
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
                        return "[Step #1] Continue talking to Brice Yarley.";
                    case 2:
                        return "[Step #2] There are some elder beeches to the north just inside the tree line beyond the fallen tower. Slay a few of them to gather the wood that you need.";
                    case 3:
                        return "[Step #3] Slay one more elder beech. It can be found in the woods to the north of the Yarley's farm.";
                    case 4:
                        return "[Step #4] Take the elder wood to Patrick Yarley. You can find him along the fence in Western Cornwall.";
                    case 5:
                        return "[Step #5] Give the rest of the Elder Wood to Patrick Yarley.";
                    case 6:
                        return "[Step #6] Ask Patrick Yarley about a [last errand].";
                    case 7:
                        return "[Step #7] Find a moor boogey on the hill to the east of the Yarley farm and collect its teeth.";
                    case 8:
                        return "[Step #8] Return to Patrick Yarley with the Boogey Teeth.";
                }
                return base.Description;
            }
        }

        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = sender as GamePlayer;

            if (player == null)
                return;
            if (player.IsDoingQuest(typeof(AFewRepairs)) == null)
                return;

            if (e == GameLivingEvent.EnemyKilled)
            {
                if (Step == 2)
                {
                    EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
                    if (gArgs.Target.Name == "elder beech")
                    {
                        if (Util.Chance(50))
                        {
                            player.Out.SendDialogBox(eDialogCode.SimpleWarning, 0x00, 0x00, 0x00, 0x00, eDialogType.Ok, true, "You receive the Elder Wood from the elder beech! \nYour journal has been updated.");
                            elderWood.Name = gArgs.Target.GetName(1, true) + " wood";
                            GiveItem(player, elderWood);
                            Step = 3;
                        }
                        return;
                    }
                }
                else if (Step == 3)
                {
                    EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
                    if (gArgs.Target.Name == "elder beech")
                    {
                        if (Util.Chance(50))
                        {
                            player.Out.SendDialogBox(eDialogCode.SimpleWarning, 0x00, 0x00, 0x00, 0x00, eDialogType.Ok, true, "You receive the Elder Wood from the elder beech! \nYour journal has been updated.");
                            elderWood.Name = gArgs.Target.GetName(1, true) + " wood";
                            GiveItem(player, elderWood);
                            Step = 4;
                        }
                        return;
                    }
                }
                else if (Step == 7)
                {
                    EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
                    if (gArgs.Target.Name == "moor boogey")
                    {
                        if (Util.Chance(50))
                        {
                            player.Out.SendDialogBox(eDialogCode.SimpleWarning, 0x00, 0x00, 0x00, 0x00, eDialogType.Ok, true, "You receive the Boogey Teeth from the moor boogey! \nYour journal has been updated.");
                            boogeyTeeth.Name = gArgs.Target.GetName(1, true) + " teeth";
                            GiveItem(player, boogeyTeeth);
                            Step = 8;
                        }
                        return;
                    }
                }
            }

            else if (e == GamePlayerEvent.GiveItem)
            {
                GiveItemEventArgs gArgs = (GiveItemEventArgs)args;
                switch (Step)
                {
                    case 4:
                        if (gArgs.Target.Name == patrickYarley.Name && gArgs.Item.Id_nb == elderWood.Id_nb)
                        {
                            RemoveItem(patrickYarley, player, elderWood);
                            patrickYarley.SayTo(player, "This should do the trick. Were you able to get any more wood?");
                            Step = 5;
                        }
                        return;
                    case 5:
                        if (gArgs.Target.Name == patrickYarley.Name && gArgs.Item.Id_nb == elderWood.Id_nb)
                        {
                            RemoveItem(patrickYarley, player, elderWood);
                            patrickYarley.SayTo(player, "That is more than enough wood. There is one other thing I will need to complete this project. Would you be able to help out with this [last errand]?");
                            Step = 6;
                        }
                        return;
                    case 8:
                        if (gArgs.Target.Name == patrickYarley.Name && gArgs.Item.Id_nb == boogeyTeeth.Id_nb)
                        {
                            RemoveItem(patrickYarley, player, boogeyTeeth);
                            patrickYarley.SayTo(player, "I will get started on the repairs immediately. Here is your reward for a job well done.");
                            FinishQuest();
                        }
                        return;

                }
            }
            
        }

        public override void AbortQuest()
        {
            base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

            RemoveItem(m_questPlayer, elderWood, false);
            RemoveItem(m_questPlayer, elderWood, false);
            RemoveItem(m_questPlayer, boogeyTeeth, false);
        }

        public override void FinishQuest()
        {
            base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

            //Give reward to player here ...
            m_questPlayer.GainExperience((long)((m_questPlayer.ExperienceForNextLevel - m_questPlayer.ExperienceForCurrentLevel) / 6.57), 0, 0, true);
            m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 81, 30 + Util.Random(60)), "You are awarded 81 silver and some copper!");

        }

    }
}
