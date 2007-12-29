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
 * Speak to Brother Maynard. He is the keep healer and is found inside the central building in Prydwen Keep.
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
     * the name: DOL.GS.Quests.Albion.BoulderlingBalm
     */

    public class BoulderlingBalm : BaseQuest
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

        protected const string questTitle = "Boulderling Balm";
        protected const int minimumLevel = 10;
        protected const int maximumLevel = 13;

        /* 
         * Start NPC
         */
        private static GameNPC brotherMaynard = null;


        /*
         * Item templates
         */
        private static ItemTemplate boulderlingRemains = null;

        /* We need to define the constructors from the base class here, else there might be problems
         * when loading this quest...
         */
        public BoulderlingBalm()
            : base()
        {
        }

        public BoulderlingBalm(GamePlayer questingPlayer)
            : this(questingPlayer, 1)
        {
        }

        public BoulderlingBalm(GamePlayer questingPlayer, int step)
            : base(questingPlayer, step)
        {
        }

        public BoulderlingBalm(GamePlayer questingPlayer, DBQuest dbQuest)
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

            GameNPC[] npcs = WorldMgr.GetNPCsByName("Brother Maynard", eRealm.Albion);

            /* Whops, if the npcs array length is 0 then no Sir Quait exists in
                * this users Mob Database, so we simply create one ;-)
                * else we take the existing one. And if more than one exist, we take
                * the first ...
                */
            if (npcs.Length == 0)
            {
                brotherMaynard = new GameHealer();
                brotherMaynard.Model = 34;
                brotherMaynard.Name = "Brother Maynard";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + brotherMaynard.Name + ", creating him ...");
                brotherMaynard.GuildName = "Healer";
                brotherMaynard.Realm = eRealm.Albion;
                brotherMaynard.CurrentRegionID = 1;
                brotherMaynard.Size = 52;
                brotherMaynard.Level = 27;
                brotherMaynard.X = 574190;
                brotherMaynard.Y = 530721;
                brotherMaynard.Z = 2896;
                brotherMaynard.Heading = 1251;

                //You don't have to store the created mob in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    brotherMaynard.SaveIntoDatabase();


                brotherMaynard.AddToWorld();

            }
            else
                brotherMaynard = npcs[0];

            


            #endregion

            #region defineItems



            boulderlingRemains = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "boulderling_remains");
            if (boulderlingRemains == null)
            {
                if (log.IsWarnEnabled)
                    log.Warn("Could not find Ludlow Magical Wood, creating it ...");
                boulderlingRemains = new ItemTemplate();
                boulderlingRemains.Object_Type = 0;
                boulderlingRemains.Id_nb = "boulderling_remains";
                boulderlingRemains.Name = "Boulderling Remains";
                boulderlingRemains.Level = 1;
                boulderlingRemains.Model = 110;
                boulderlingRemains.IsDropable = false;
                boulderlingRemains.IsPickable = false;
                boulderlingRemains.Weight = 4;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(boulderlingRemains);
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

            GameEventMgr.AddHandler(brotherMaynard, GameLivingEvent.Interact, new DOLEventHandler(TalkToBrotherMaynard));
            GameEventMgr.AddHandler(brotherMaynard, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToBrotherMaynard));

            /* Now we bring to brotherMaynard the possibility to give this quest to players */
            brotherMaynard.AddQuestToGive(typeof(BoulderlingBalm));

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
            /* If brotherMaynard has not been initialized, then we don't have to remove any
             * hooks from him ;-)
             */
            if (brotherMaynard == null)
                return;

            /* Removing hooks works just as adding them but instead of 
             * AddHandler, we call RemoveHandler, the parameters stay the same
             */

			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

            GameEventMgr.RemoveHandler(brotherMaynard, GameObjectEvent.Interact, new DOLEventHandler(TalkToBrotherMaynard));
            GameEventMgr.RemoveHandler(brotherMaynard, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToBrotherMaynard));

            /* Now we remove to brotherMaynard the possibility to give this quest to players */
            brotherMaynard.RemoveQuestToGive(typeof(BoulderlingBalm));
        }

        /* This is the method we declared as callback for the hooks we set to
         * Sir Quait. It will be called whenever a player right clicks on Sir Quait
         * or when he whispers something to him.
         */

        protected static void TalkToBrotherMaynard(DOLEvent e, object sender, EventArgs args)
        {
            //We get the player from the event arguments and check if he qualifies		
            GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
            if (player == null)
                return;

            if (brotherMaynard.CanGiveQuest(typeof(BoulderlingBalm), player) <= 0)
                return;

            //We also check if the player is already doing the quest
            BoulderlingBalm quest = player.IsDoingQuest(typeof(BoulderlingBalm)) as BoulderlingBalm;

            brotherMaynard.TurnTo(player);
            //Did the player rightclick on brotherMaynard?
            if (e == GameObjectEvent.Interact)
            {
                //We check if the player is already doing the quest
                if (quest != null)
                {
                    if (quest.Step == 1)
                        brotherMaynard.SayTo(player, "Thank you for agreeing to help. The main ingredient in my balm is a powder made from the crushed remains of boulderlings. The remains of two boulderlings should supply me for quite [some time].");
                    else if (quest.Step == 4)
                        brotherMaynard.SayTo(player, "You've returned! I trust you were able to get the boulderling remains without much trouble? Please hand me the first set of remains.");

                    else if (quest.Step == 5)
                        brotherMaynard.SayTo(player, "May I have the second set of remains?");
                    return;
                }
                else
                {
                    //Player hasn't the quest:
                    brotherMaynard.SayTo(player, "Good day to you, friend. Have you come to seek healing or to offer aid to the [Church]?");
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
                        case "Church":
                            brotherMaynard.SayTo(player, "I'm not necessarily talking about punishing the wicked or slaying the enemies of the pious. Small acts of charity and service can make a difference, too. I leave combat to the Church's [paladins].");
                            break;
                        case "paladins":
                            brotherMaynard.SayTo(player, "Most believe that it is more glorious to take up the sword in the service of faith than to offer assistance to those in need. My calling is that of a healer, and I am grateful for the chance to use my skill in service to a greater [cause].");
                            break;
                        case "cause":
                            brotherMaynard.SayTo(player, "It is easy to forget that with no healers, no armor and weaponsmiths, there would be no paladins. I serve those who make the blades, the armor, the arrows and bows that our knights use in [battle].");
                            break;
                        case "battle":
                            brotherMaynard.SayTo(player, "A dedicated craftsman's hands taked a beating, and over the years, I've learned to make a balm that soothes them. I go hrough it rather quickly and I'm almost out of ingredients. Perhaps you would help me restock?");
							player.Out.SendQuestSubscribeCommand(brotherMaynard, QuestMgr.GetIDForQuestType(typeof(BoulderlingBalm)), "Will you gather the ingredients for Brother Maynard's special balm? [Levels 10-13]");
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
                        case "some time":
                            if (quest.Step == 1)
                            {
                                brotherMaynard.SayTo(player, "To find the boulderlings, travel east to Prydwen Bridge and cross it. The boulderlings will be located on a hill just to the east of the road. Good luck, " + player.CharacterClass.Name + ".");
                                quest.Step = 2;
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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(BoulderlingBalm)))
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
            if (player.IsDoingQuest(typeof(BoulderlingBalm)) != null)
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
            BoulderlingBalm quest = player.IsDoingQuest(typeof(BoulderlingBalm)) as BoulderlingBalm;

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
            if (brotherMaynard.CanGiveQuest(typeof(BoulderlingBalm), player) <= 0)
                return;

            if (player.IsDoingQuest(typeof(BoulderlingBalm)) != null)
                return;

            if (response == 0x00)
            {
                SendReply(player, "Oh well, if you change your mind, please come back!");
            }
            else
            {
                //Check if we can add the quest
                if (!brotherMaynard.GiveQuest(typeof(BoulderlingBalm), player, 1))
                    return;

                brotherMaynard.SayTo(player, "Thank you for agreeing to help. The main ingredient in my balm is a powder made from the crushed remains of boulderlings. The remains of two boulderlings should supply me for quite [some time].");
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
                        return "[Step #1] Continue speaking with Brother Maynard. If he stops talking o you, remind him that he needs to spend [some time] giving you directions.";
                    case 2:
                        return "[Step #2] Brother Maynard wants you to collect the remains of two boulderlings. To find the boulderlings, travel east from Prydwen keep to the bridge and cross it. The boulderlings will be on a hill to the east.";
                    case 3:
                        return "[Step #3] Brother Maynard wants you to collect the remains of one more boulderling. To find the boulderlings, travel east from Prydwen keep to the bridge and cross it. The boulderlings will be on a hill to the east.";
                    case 4:
                        return "[Step #4] Now that you have all the boulderling remains Brother Maynard needs, return to Prydwen Keep and speak with Maynard. ";
                    case 5:
                        return "[Step #5] Hand Brother Maynard the second set of boulderling remains.";


                }
                return base.Description;
            }
        }

        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = sender as GamePlayer;

            if (player == null)
                return;
            if (player.IsDoingQuest(typeof(BoulderlingBalm)) == null)
                return;


            if (Step == 2 && e == GameLivingEvent.EnemyKilled)
            {
                EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
                if ((gArgs.Target.Name.IndexOf("Boulderling") >= 0) || (gArgs.Target.Name.IndexOf("boulderling") >= 0))
                {
                    if (Util.Chance(50))
                    {
                        player.Out.SendDialogBox(eDialogCode.SimpleWarning, 0x00, 0x00, 0x00, 0x00, eDialogType.Ok, true, "You collect the boulderling's remains and place them in your pack. Your journal has been updated.");
                        GiveItem(player, boulderlingRemains);
                        Step = 3;
                    }
                    return;
                }
            }
            else if (Step == 3 && e == GameLivingEvent.EnemyKilled)
            {
                EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
                if ((gArgs.Target.Name.IndexOf("Boulderling") >= 0) || (gArgs.Target.Name.IndexOf("boulderling") >= 0))
                {
                    if (Util.Chance(50))
                    {
                        player.Out.SendDialogBox(eDialogCode.SimpleWarning, 0x00, 0x00, 0x00, 0x00, eDialogType.Ok, true, "You collect the boulderling's remains and place them in your pack. Your journal has been updated.");
                        GiveItem(player, boulderlingRemains);
                        Step = 4;
                    }
                    return;
                }
            }
            else if (Step == 4 && e == GamePlayerEvent.GiveItem)
            {
                GiveItemEventArgs gArgs = (GiveItemEventArgs)args;
                if (gArgs.Target.Name == brotherMaynard.Name && gArgs.Item.Id_nb == boulderlingRemains.Id_nb)
                {
                    RemoveItem(brotherMaynard, player, boulderlingRemains);
                    brotherMaynard.SayTo(player, "Ah, yes, these will provide quite a lot of powder. May I have the second set of remains?");
                    Step = 5;
                    return;
                }
            }
            else if (Step == 5 && e == GamePlayerEvent.GiveItem)
            {
                GiveItemEventArgs gArgs = (GiveItemEventArgs)args;
                if (gArgs.Target.Name == brotherMaynard.Name && gArgs.Item.Id_nb == boulderlingRemains.Id_nb)
                {
                    RemoveItem(brotherMaynard, player, boulderlingRemains);
                    brotherMaynard.SayTo(player, "Thank you, "+player.Name+". I should be able to make a lot of hand blam from these remains. It's going to take quite some time to grind them down, so I'd better get to work. Please accept this money as a reward for your help.");
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
            m_questPlayer.GainExperience((long)((m_questPlayer.ExperienceForNextLevel - m_questPlayer.ExperienceForCurrentLevel) / 20), true);
            m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 2, 49 + Util.Random(50)), "You are awarded 2 silver and some copper!");

        }

    }
}
