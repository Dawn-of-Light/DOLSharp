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
* Author: k109
* 
* Date: 12/5/07	
* Directory: /scripts/quests/hibernia/
* 
* Compiled on SVN 911
* 
* Description: The "Search For Knowledge" quest, mimics live US servers.
 */
using System;
using System.Reflection;
using DOL.Database2;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Quests.Hibernia
{
    public class SearchForKnowledge : RewardQuest
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected const string questTitle = "Search For Knowledge";
        protected const int minimumLevel = 1;
        protected const int maximumLevel = 5;

        private static GameNPC Blercyn = null;
        private static GameNPC Epona = null;

        private QuestGoal eponasletter;

        private static ItemTemplate LetterToEpona = null;
        private static ItemTemplate RecruitsCloak = null;

        public SearchForKnowledge()
            : base()
        {
            Init();
        }

        public SearchForKnowledge(GamePlayer questingPlayer)
            : this(questingPlayer, 1)
        {
        }

        public SearchForKnowledge(GamePlayer questingPlayer, int step)
            : base(questingPlayer, step)
        {
            Init();
        }

        public SearchForKnowledge(GamePlayer questingPlayer, DBQuest dbQuest)
            : base(questingPlayer, dbQuest)
        {
            Init();
        }

        private void Init()
        {
            #region defineItems

            // item db check
            RecruitsCloak = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "k109_recruits_cloak");
            if (RecruitsCloak == null)
            {
                RecruitsCloak = new ItemTemplate();
                RecruitsCloak.Name = "Recruit's Cloak";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + RecruitsCloak.Name + ", creating it ...");

                RecruitsCloak.Level = 5;
                RecruitsCloak.Weight = 3;
                RecruitsCloak.Model = 443;
                RecruitsCloak.Color = 30;

                RecruitsCloak.Object_Type = (int)eObjectType.Magical;
                RecruitsCloak.Item_Type = (int)eEquipmentItems.CLOAK;
                RecruitsCloak.Id_nb = "k109_recruits_cloak";
                RecruitsCloak.Gold = 0;
                RecruitsCloak.Silver = 0;
                RecruitsCloak.Copper = 0;
                RecruitsCloak.IsPickable = true;
                RecruitsCloak.IsDropable = false; // can't be sold to merchand

                RecruitsCloak.Bonus1 = 6;
                RecruitsCloak.Bonus1Type = (int)eProperty.MaxHealth;

                RecruitsCloak.Quality = 100;
                RecruitsCloak.Condition = 50000;
                RecruitsCloak.MaxCondition = 50000;
                RecruitsCloak.Durability = 50000;
                RecruitsCloak.MaxDurability = 50000;

                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(RecruitsCloak);
            }
            LetterToEpona = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "letter_to_epona");
            if (LetterToEpona == null)
            {
                LetterToEpona = new ItemTemplate();
                LetterToEpona.Weight = 0;
                LetterToEpona.Condition = 50000;
                LetterToEpona.MaxCondition = 50000;
                LetterToEpona.Model = 499;
                LetterToEpona.Extension = 1;
                LetterToEpona.Name = "Letter To Epona";
				LetterToEpona.Id_nb = "letter_to_epona";

                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(LetterToEpona);
            }
            #endregion
            Level = 1;
            QuestGiver = Blercyn;
            Rewards.Experience = 22;
            Rewards.MoneyPercent = 100;
            Rewards.AddBasicItem(RecruitsCloak);
            Rewards.ChoiceOf = 1;

            eponasletter = AddGoal("Take the message to Epona", QuestGoal.GoalType.ScoutMission, 1, LetterToEpona);
        }

        [ScriptLoadedEvent]
        public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
        {
            if (!ServerProperties.Properties.LOAD_QUESTS)
                return;
            if (log.IsInfoEnabled)
                log.Info("Quest \"" + questTitle + "\" initializing ...");


            #region defineNPCS

            GameNPC[] npcs = WorldMgr.GetNPCsByName("Blercyn", eRealm.Hibernia);

            /* Whops, if the npcs array length is 0 then no npc exists in
                * this users Mob GS, so we simply create one ;-)
                * else we take the existing one. And if more than one exist, we take
                * the first ...
                */
            if (npcs.Length == 0)
            {
                Blercyn = new GameNPC();
                Blercyn.Model = 700;
                Blercyn.Name = "Blercyn";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + Blercyn.Name + ", creating him ...");
                //Blercyn.GuildName = "Part of " + questTitle + " Quest";
                Blercyn.Realm = eRealm.Hibernia;
                Blercyn.CurrentRegionID = 200;

                GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
                template.AddNPCEquipment(eInventorySlot.TorsoArmor, 58);
                Blercyn.Inventory = template.CloseTemplate();
                Blercyn.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

                Blercyn.Size = 50;
                Blercyn.Level = 50;
                Blercyn.X = 348614;
                Blercyn.Y = 492141;
                Blercyn.Z = 5199;
                Blercyn.Heading = 1539;

                //You don't have to store the created mob in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database

                if (SAVE_INTO_DATABASE)
                    Blercyn.SaveIntoDatabase();

                Blercyn.AddToWorld();
            }
            else
                Blercyn = npcs[0];

            //Pompin The Crier
            npcs = WorldMgr.GetNPCsByName("Epona", eRealm.Hibernia);
            if (npcs.Length == 0)
            {
                Epona = new GameNPC();
                Epona.Model = 10;
                Epona.Name = "Epona";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + Epona.Name + ", creating him ...");
                //Blercyn.GuildName = "Part of " + questTitle + " Quest";
                Epona.Realm = eRealm.Hibernia;
                Epona.CurrentRegionID = 200;

                GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
                template.AddNPCEquipment(eInventorySlot.TorsoArmor, 58);
                Epona.Inventory = template.CloseTemplate();
                Epona.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

                Epona.Size = 50;
                Epona.Level = 50;
                Epona.X = 347606;
                Epona.Y = 490658;
                Epona.Z = 5227;
                Epona.Heading = 1342;

                //You don't have to store the created mob in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database

                if (SAVE_INTO_DATABASE)
                    Epona.SaveIntoDatabase();

                Epona.AddToWorld();
            }
            else
                Epona = npcs[0];

            #endregion

            GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
            GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

            GameEventMgr.AddHandler(Blercyn, GameLivingEvent.Interact, new DOLEventHandler(TalkToBlercyn));
            GameEventMgr.AddHandler(Blercyn, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToBlercyn));

            GameEventMgr.AddHandler(Epona, GameLivingEvent.Interact, new DOLEventHandler(TalkToEpona));
            GameEventMgr.AddHandler(Epona, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToEpona));

            Blercyn.AddQuestToGive(typeof(SearchForKnowledge));

            if (log.IsInfoEnabled)
                log.Info("Quest \"" + questTitle + "\" initialized");
        }

        [ScriptUnloadedEvent]
        public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
        {
            if (Blercyn == null)
                return;

            GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
            GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

            GameEventMgr.RemoveHandler(Blercyn, GameObjectEvent.Interact, new DOLEventHandler(TalkToBlercyn));
            GameEventMgr.RemoveHandler(Blercyn, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToBlercyn));

            GameEventMgr.RemoveHandler(Epona, GameLivingEvent.Interact, new DOLEventHandler(TalkToEpona));
            GameEventMgr.RemoveHandler(Epona, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToEpona));

            Blercyn.RemoveQuestToGive(typeof(SearchForKnowledge));
        }

        protected static void TalkToBlercyn(DOLEvent e, object sender, EventArgs args)
        {
            //We get the player from the event arguments and check if he qualifies		
            GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
            if (player == null)
                return;

            if (Blercyn.CanGiveQuest(typeof(SearchForKnowledge), player) <= 0)
                return;


            SearchForKnowledge quest = player.IsDoingQuest(typeof(SearchForKnowledge)) as SearchForKnowledge;
            Blercyn.TurnTo(player);

            if (e == GameObjectEvent.Interact)
            {
                if (quest == null)
                {
                    quest = new SearchForKnowledge();
                    quest.QuestGiver = Blercyn;
                    quest.OfferQuest(player);
                }

            }
        }

        protected static void TalkToEpona(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
            if (player == null)
                return;

            SearchForKnowledge quest = player.IsDoingQuest(typeof(SearchForKnowledge)) as SearchForKnowledge;
            Epona.TurnTo(player);
            if (e == GameObjectEvent.Interact)
            {
                if (quest == null)
                {
                    Epona.SayTo(player, "You look a bit frazzled, adventurer.  Shall I make you whole again?");
                }
                else
                {
                    if (quest.Step == 1)
                    {
                        quest.QuestGiver = Epona;
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

            if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(SearchForKnowledge)))
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
            if (player.IsDoingQuest(typeof(SearchForKnowledge)) != null)
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
            SearchForKnowledge quest = player.IsDoingQuest(typeof(SearchForKnowledge)) as SearchForKnowledge;

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

            if (Blercyn.CanGiveQuest(typeof(SearchForKnowledge), player) <= 0)
                return;

            if (player.IsDoingQuest(typeof(SearchForKnowledge)) != null)
                return;

            if (response == 0x00)
            {
                // Player declined, don't do anything.
            }
            else
            {
                // Player accepted, let's try to give him the quest.
                if (!Blercyn.GiveQuest(typeof(SearchForKnowledge), player, 1))
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
                String desc = "Blessings upon you and welcome to Mag Mell.  Times are changing and so is the majetic Hibernia that we once knew."
                    + "The intensity of the realm war against Albion and Midgard has brought much concern to our leaders."
                    + "King Lug has returned from the Veil and taken his throne in Tir Na Nog once again."
                    + "Even Fagan has been uneasy about the safety of his people here in Mag Mell.\n\n"
                    + "The realm war has taken most of the garrison that guarded our small town.  When the sentinels had patrolled with regular numbers they had their hands full with Siabra and other creatures."
                    + "Now they can barely handle patrolling the town perimeter with their skeletal force.  I have a message for our local healer, Epona."
                    + "The realm war is taking a toll on our brave men and women fighting in the frontlines and the Path of Elders informed us that they are recruiting as many Healers as they can find for the frontiers."
                    + "I need you to deliver Epona a message informing her of this news.  She may have a few tasks for you.";
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
                return "Listen to what Blercyn has to say about the current state of Mag Mell and Hibernia.  When your trainer has finished seek out Epona and deliver the letter.";
            }
        }

        /// <summary>
        /// Text showing upon finishing the quest.
        /// </summary>
        public override String Conclusion
        {
            get
            {
                String text = String.Format("Thank you, {0}.",QuestPlayer.CharacterClass.Name);
                text += "I'll send a summons to my apprentices in Lough Gur.  The war is depleting our resources and we've much to do on all fronts.  I certainly have more than my share of work.  Let me know if you would like to help out.";
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
    }
}
