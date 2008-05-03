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
 */

using System;
using System.Reflection;
using DOL.Database2;
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
     * the name: DOL.GS.Quests.Albion.BoarStew
     */

    public class BoarStew : BaseQuest
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

        protected const string questTitle = "Boar Stew";
        protected const int minimumLevel = 19;
        protected const int maximumLevel = 22;

        /* 
         * Start NPC
         */
        private static GameNPC 	masterGerol = null;


        /*
         * Item templates
         */
        private static ItemTemplate  boarCarcass = null;

        /* We need to define the constructors from the base class here, else there might be problems
         * when loading this quest...
         */
        public BoarStew()
            : base()
        {
        }

        public BoarStew(GamePlayer questingPlayer)
            : this(questingPlayer, 1)
        {
        }

        public BoarStew(GamePlayer questingPlayer, int step)
            : base(questingPlayer, step)
        {
        }

        public BoarStew(GamePlayer questingPlayer, DBQuest dbQuest)
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

            GameNPC[] npcs = WorldMgr.GetNPCsByName("Master Gerol", eRealm.Albion);
            if (npcs.Length == 0)
            {
                masterGerol = new GameNPC();
                masterGerol.Model = 10;
                masterGerol.Name = "Master Gerol";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + masterGerol.Name + ", creating him ...");
                masterGerol.GuildName = "Healer";
                masterGerol.Realm = eRealm.Albion;
                masterGerol.CurrentRegionID = 1;
                masterGerol.Size = 45;
                masterGerol.Level = 20;
                masterGerol.X = 578668;
                masterGerol.Y = 556823;
                masterGerol.Z = 2184;
                masterGerol.Heading = 79;
                masterGerol.MaxSpeedBase = 200;

                //You don't have to store the created mob in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    masterGerol.SaveIntoDatabase();


                masterGerol.AddToWorld();
            }
            else
                masterGerol = npcs[0];

            

            #endregion

            #region defineItems



            boarCarcass = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "huge_boar_carcass");
            if (boarCarcass == null)
            {
                if (log.IsWarnEnabled)
                    log.Warn("Could not find Elder Wood, creating it ...");
                boarCarcass = new ItemTemplate();
                boarCarcass.Object_Type = 0;
                boarCarcass.Id_nb = "huge_boar_carcass";
                boarCarcass.Name = "Huge Boar Carcass";
                boarCarcass.Level = 1;
                boarCarcass.Model = 540;
                boarCarcass.IsDropable = false;
                boarCarcass.IsPickable = false;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(boarCarcass);
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

            GameEventMgr.AddHandler(masterGerol, GameLivingEvent.Interact, new DOLEventHandler(TalkToMasterGerol));
            GameEventMgr.AddHandler(masterGerol, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterGerol));


            /* Now we bring to masterGerol the possibility to give this quest to players */
            masterGerol.AddQuestToGive(typeof(BoarStew));

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
            if (masterGerol != null)
            {

                /* Removing hooks works just as adding them but instead of 
                 * AddHandler, we call RemoveHandler, the parameters stay the same
                 */

				GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
				GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

                GameEventMgr.RemoveHandler(masterGerol, GameObjectEvent.Interact, new DOLEventHandler(TalkToMasterGerol));
                GameEventMgr.RemoveHandler(masterGerol, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterGerol));

				/* Now we remove to arleighPenn the possibility to give this quest to players */
				masterGerol.RemoveQuestToGive(typeof(BoarStew));
            }
        }

        /* This is the method we declared as callback for the hooks we set to
         * masterGerol. It will be called whenever a player right clicks on Sir Quait
         * or when he whispers something to him.
         */

        protected static void TalkToMasterGerol(DOLEvent e, object sender, EventArgs args)
        {
            //We get the player from the event arguments and check if he qualifies		
            GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
            if (player == null)
                return;

            if (masterGerol.CanGiveQuest(typeof(BoarStew), player) <= 0)
                return;

            //We also check if the player is already doing the quest
            BoarStew quest = player.IsDoingQuest(typeof(BoarStew)) as BoarStew;

            masterGerol.TurnTo(player);
            //Did the player rightclick on masterGerol?
            if (e == GameObjectEvent.Interact)
            {
                if (quest == null)
                {
                    //Player hasn't the quest:
                    masterGerol.SayTo(player, "Good day, my friend. I am Gerol, Master and Healer of horses. The guards stop here frequently for my services. I'm no miracle worker, but I suppose if you're hurt, I could patch you up a bit, as well.");
                    masterGerol.SayTo(player, "Goodness, more new faces in town. Are you here to help me with my wonderful [stew]?");
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
                        case "stew":
                            masterGerol.SayTo(player, "Oh yes! It is my speciality and one of the key components to helping people out when they are sick. Everyone knows that my wonderful stew brings along a speedy recovery. I am running low on the main [ingredient], do you think you could help me out?");
                            break;
                        case "ingredient":
							player.Out.SendQuestSubscribeCommand(masterGerol, QuestMgr.GetIDForQuestType(typeof(BoarStew)), "Will you help Master Gerol? [Levels 19-22]");
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
                    }
                }

            }
        }

		protected static void SubscribeQuest(DOLEvent e, object sender, EventArgs args)
		{
			QuestEventArgs qargs = args as QuestEventArgs;
			if (qargs == null)
				return;

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(BoarStew)))
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
            if (player.IsDoingQuest(typeof(BoarStew)) != null)
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
            BoarStew quest = player.IsDoingQuest(typeof(BoarStew)) as BoarStew;

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
            if (masterGerol.CanGiveQuest(typeof(BoarStew), player) <= 0)
                return;

            if (player.IsDoingQuest(typeof(BoarStew)) != null)
                return;

            if (response == 0x00)
            {
                SendReply(player, "Oh well, if you change your mind, please come back!");
            }
            else
            {
                //Check if we can add the quest
                if (!masterGerol.GiveQuest(typeof(BoarStew), player, 1))
                    return;

                masterGerol.SayTo(player, "Seek out a huge boar in Salisbury Plains. Slay it and return the carcass.");

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
                        return "[Step #1] Seek out a huge boar in Salisbury Plains. Slay it and return the carcass to Master Gerol.";
                    case 2:
                        return "[Step #2] Return to Master Gerol in West Downs with the boar carcass.";
                    
                }
                return base.Description;
            }
        }

        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = sender as GamePlayer;

            if (player == null)
                return;
            if (player.IsDoingQuest(typeof(BoarStew)) == null)
                return;

            if (e == GameLivingEvent.EnemyKilled)
            {
                if (Step == 1)
                {
                    EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
                    if ((gArgs.Target.Name == "Huge Boar") || (gArgs.Target.Name == "huge boar"))
                    {
                        if (Util.Chance(50))
                        {
                            player.Out.SendDialogBox(eDialogCode.SimpleWarning, 0x00, 0x00, 0x00, 0x00, eDialogType.Ok, true, "You receive the Huge Boar Carcass from the huge boar! \nYour journal has been updated.");
                            boarCarcass.Name = gArgs.Target.GetName(1, true) + " carcass ";
                            GiveItem(player, boarCarcass);
                            Step = 2;
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
                    case 2:
                        if (gArgs.Target.Name == masterGerol.Name && gArgs.Item.Id_nb == boarCarcass.Id_nb)
                        {
                            RemoveItem(masterGerol, player, boarCarcass);
                            masterGerol.SayTo(player, "Wow, this has got to be the largest boar I have seen yet! Here, have some coins for payment of your task, as well as the knowledge you are helping out the health of Albion.");
                            FinishQuest();
                        }
                        return;                    

                }
            }

        }

        public override void AbortQuest()
        {
            base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

            RemoveItem(m_questPlayer, boarCarcass, false);
        }

        public override void FinishQuest()
        {
            base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

            //Give reward to player here ...
            m_questPlayer.GainExperience((long)((m_questPlayer.ExperienceForNextLevel - m_questPlayer.ExperienceForCurrentLevel) / 9.42), true);
            m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 9, 30 + Util.Random(50)), "You are awarded 9 silver and some copper!");

        }

    }
}
