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
 * Author:		k109
 * 
 * Date:		12/5/07	
 * Directory: /scripts/quests/hibernia/
 *
 * Description:
 *  Brief Walkthrough: 
 * 1) Talk with Josson
 * 2) Slay a Orchard nipper and retrieve it's wing.  You will receive some xp, copper and the weapon of your choice.
 */
using System;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Quests.Hibernia
{
    public class MagicalBacklash : RewardQuest
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected const string questTitle = "Magical Backlash";
        protected const int minimumLevel = 2;
        protected const int maximumLevel = 5;

        private static GameNPC Josson = null;
        private QuestGoal OrchardNipperKilled;

        private static ItemTemplate RecruitsShortSword = null;
        private static ItemTemplate RecruitsDirk = null;
        private static ItemTemplate RecruitsClub = null;
        private static ItemTemplate RecruitsStaff = null;

        public MagicalBacklash()
            : base()
        {
            Init();
        }

        public MagicalBacklash(GamePlayer questingPlayer)
            : this(questingPlayer, 1)
        {
        }

        public MagicalBacklash(GamePlayer questingPlayer, int step)
            : base(questingPlayer, step)
        {
            Init();
        }

        public MagicalBacklash(GamePlayer questingPlayer, DBQuest dbQuest)
            : base(questingPlayer, dbQuest)
        {
            Init();
        }

        private void Init()
        {
            #region defineItems
            RecruitsShortSword = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "recruits_short_sword_hib");
            RecruitsDirk = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "recruits_dirk");
            RecruitsClub = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "recruits_club");
            RecruitsStaff = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "recruits_staff");
            #endregion

            ItemTemplate nipperwing = new ItemTemplate();
            nipperwing.Weight = 0;
            nipperwing.Condition = 50000;
            nipperwing.MaxCondition = 50000;
            nipperwing.Model = 551;
            nipperwing.Extension = 1;
            nipperwing.Name = "Orchard Nipper Wing";

            Level = 2;
            QuestGiver = Josson;
            Rewards.Experience = 90;
            Rewards.MoneyPercent = 20;
            Rewards.AddOptionalItem(RecruitsShortSword);
            Rewards.AddOptionalItem(RecruitsDirk);
            Rewards.AddOptionalItem(RecruitsClub);
            Rewards.AddOptionalItem(RecruitsStaff);
            Rewards.ChoiceOf = 1;

            OrchardNipperKilled = AddGoal("Kill orchard nippers for wings.", QuestGoal.GoalType.KillTask, 1, nipperwing);

        }

        [ScriptLoadedEvent]
        public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
        {
            if (!ServerProperties.Properties.LOAD_QUESTS)
                return;
            if (log.IsInfoEnabled)
                log.Info("Quest \"" + questTitle + "\" initializing ...");


            #region defineNPCS

            GameNPC[] npcs = WorldMgr.GetNPCsByName("Josson", eRealm.Hibernia);

            if (npcs.Length == 0)
            {
                Josson = new GameNPC();
                Josson.Model = 382;
                Josson.Name = "Josson";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + Josson.Name + ", creating him ...");
                //k109: My preference, no guildname for quest NPCs.  Uncomment if you like that...
                //Josson.GuildName = "Part of " + questTitle + " Quest";
                Josson.Realm = eRealm.Hibernia;
                Josson.CurrentRegionID = 200;

                GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
                template.AddNPCEquipment(eInventorySlot.HandsArmor, 386);   //Slot 22
                template.AddNPCEquipment(eInventorySlot.HeadArmor, 835);    //Slot 21
                template.AddNPCEquipment(eInventorySlot.FeetArmor, 387);     //Slot 23
                template.AddNPCEquipment(eInventorySlot.TorsoArmor, 383);    //Slot 25
                template.AddNPCEquipment(eInventorySlot.LegsArmor, 384);    //Slot 27
                template.AddNPCEquipment(eInventorySlot.ArmsArmor, 385);    //Slot 28
                Josson.Inventory = template.CloseTemplate();
                Josson.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

                Josson.Size = 48;
                Josson.Level = 50;
                Josson.X = 346627;
                Josson.Y = 491453;
                Josson.Z = 5247;
                Josson.Heading = 2946;

                if (SAVE_INTO_DATABASE)
                    Josson.SaveIntoDatabase();

                Josson.AddToWorld();
            }
            else
                Josson = npcs[0];

            #endregion

            GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
            GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

            GameEventMgr.AddHandler(Josson, GameLivingEvent.Interact, new DOLEventHandler(TalkToJosson));
            GameEventMgr.AddHandler(Josson, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToJosson));

            Josson.AddQuestToGive(typeof(MagicalBacklash));

            if (log.IsInfoEnabled)
                log.Info("Quest \"" + questTitle + "\" initialized");
        }

        [ScriptUnloadedEvent]
        public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
        {
            if (Josson == null)
                return;

            GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
            GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

            GameEventMgr.RemoveHandler(Josson, GameObjectEvent.Interact, new DOLEventHandler(TalkToJosson));
            GameEventMgr.RemoveHandler(Josson, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToJosson));

            Josson.RemoveQuestToGive(typeof(MagicalBacklash));
        }

        protected static void TalkToJosson(DOLEvent e, object sender, EventArgs args)
        {
            //We get the player from the event arguments and check if he qualifies		
            GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
            if (player == null)
                return;

            if (Josson.CanGiveQuest(typeof(MagicalBacklash), player) <= 0)
                return;


            MagicalBacklash quest = player.IsDoingQuest(typeof(MagicalBacklash)) as MagicalBacklash;
            Josson.TurnTo(player);

            if (e == GameObjectEvent.Interact)
            {
                if (quest == null)
                {
                    quest = new MagicalBacklash();
                    quest.QuestGiver = Josson;
                    quest.OfferQuest(player);
                }
                else
                {
                    if (quest.Step == 1 && quest.OrchardNipperKilled.IsAchieved)
                    {
                        quest.ChooseRewards(player);
                    }
                }
            }
        }

        /// <summary>
        /// Callback for player accept/decline action.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected static void SubscribeQuest(DOLEvent e, object sender, EventArgs args)
        {
            QuestEventArgs qargs = args as QuestEventArgs;
            if (qargs == null)
                return;

            if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(MagicalBacklash)))
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
            if (player.IsDoingQuest(typeof(MagicalBacklash)) != null)
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
            MagicalBacklash quest = player.IsDoingQuest(typeof(MagicalBacklash)) as MagicalBacklash;

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
            // We recheck the qualification, because we don't talk to players
            // who are not doing the quest.

            if (Josson.CanGiveQuest(typeof(MagicalBacklash), player) <= 0)
                return;

            if (player.IsDoingQuest(typeof(MagicalBacklash)) != null)
                return;

            if (response == 0x00)
            {
                // Player declined, don't do anything.
            }
            else
            {
                // Player accepted, let's try to give him the quest.

                if (!Josson.GiveQuest(typeof(MagicalBacklash), player, 1))
                    return;

            }
        }

        /// <summary>
        /// The quest title.
        /// </summary>
        public override string Name
        {
            get { return questTitle; }
        }

        /// <summary>
        /// The text for individual quest steps as shown in the journal.
        /// </summary>

        public override string Description
        {
            get
            {
                switch (Step)
                {

                    case 1:
                        return Summary;
                    default:
                        return "No Queststep Description available.";
                }
            }
        }

        /// <summary>
        /// The fully-fledged story to the quest.
        /// </summary>
        public override string Story
        {
            get
            {
                String desc = "Have you come in for safety? Living in a land full of magic makes you more aware of your surroundings, but that same magic has caused us problems. Mag Mell isn't the peaceful village it once was. We're plagued with one problem after the next, some of which were cause by carelessness.\n\n"
                    + "Once, while toying with unrefined magic, those careless Shar tore a hole in the Veil leading into Hibernia. The overload of magical energy turned many of our creatures mad. Little was left untainted by this magical backlash. Even the magic of the burrows and mounds shifted.\n\n"
                    + "They say the Cursed Burrow dwellers are venturing beyond their domain. Taskmaster Sevinia, our local dungeon scholar, asked me to slay some of the orchard nippers near the lair. I'm sorry to say I'll not go near it. The Cursed Burrow terrifies me, but I can't back out of a community duty, and I can't have her, Fagan, or anyone else thinking I'm gutless or that I'm shirking responsibility.\n\n";
                return desc;
            }
        }

        /// <summary>
        /// A summary of the quest's story.
        /// </summary>
        public override string Summary
        {
            get
            {
                return "Find the Cursed Burrow and slay orchard nippers near the entrance until you receive a set of wings. Return to Josson one you collect a set of Orchard Nipper wings to show proof that you completed the task.";
            }
        }

        /// <summary>
        /// Text showing upon finishing the quest.
        /// </summary>
        public override String Conclusion
        {
            get
            {
                String text = String.Format("");
                text += "Thank you.  I couldn't bear the thought of going near that place.  Your much braver then I'll ever be.  You should go speak with some of the other locals.  I'm sure they could use your help.";
                return text;
            }
        }

        /// <summary>
        /// The level of the quest as it shows in the journal.
        /// </summary>
        public override int Level
        {
            get
            {
                return 1;
            }
        }

        /// <summary>
        /// Handles quest events.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
            base.Notify(e, sender, args);
            GamePlayer player = sender as GamePlayer;

            if (player == null)
                return;
            if (player.IsDoingQuest(typeof(MagicalBacklash)) == null)
                return;


            if (Step == 1 && e == GameLivingEvent.EnemyKilled)
            {
                EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
                if (gArgs.Target.Name.IndexOf("orchard nipper") >= 0)
                {
                    if (!OrchardNipperKilled.IsAchieved)
                    {
                        OrchardNipperKilled.Advance();
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
            base.FinishQuest();
        }
    }
}
