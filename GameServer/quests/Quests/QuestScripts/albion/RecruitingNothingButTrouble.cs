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
* Directory: /scripts/quests/albion/
* 
* Compiled on SVN 905
* 
* Description: The "Recruiting Nothing But Trouble" quest, mimics live US servers.
 */
using System;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Quests.Albion
{
    public class RecruitingNothingButTrouble : RewardQuest
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected const string questTitle = "Recruiting Nothing But Trouble";
        protected const int minimumLevel = 2;
        protected const int maximumLevel = 5;

        private static GameNPC Rheda = null;
        private QuestGoal BanditRecruitsKilled;

        private static ItemTemplate RecruitsShortSword = null;
        private static ItemTemplate RecruitsDirk = null;
        private static ItemTemplate RecruitsMace = null;
        private static ItemTemplate RecruitsStaff = null;

        public RecruitingNothingButTrouble()
            : base()
        {
            Init();
        }

        public RecruitingNothingButTrouble(GamePlayer questingPlayer)
            : this(questingPlayer, 1)
        {
        }

        public RecruitingNothingButTrouble(GamePlayer questingPlayer, int step)
            : base(questingPlayer, step)
        {
            Init();
        }

        public RecruitingNothingButTrouble(GamePlayer questingPlayer, DBQuest dbQuest)
            : base(questingPlayer, dbQuest)
        {
            Init();
        }

        private void Init()
        {
            #region defineItems
			RecruitsShortSword = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "k109_recruits_short_sword");
            RecruitsDirk = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "k109_recruits_dirk");
            RecruitsMace = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "k109_recruits_mace");
            RecruitsStaff = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "k109_recruits_staff");
            #endregion

            ItemTemplate rhedasword = new ItemTemplate();
            rhedasword.Weight = 0;
            rhedasword.Condition = 50000;
            rhedasword.MaxCondition = 50000;
            rhedasword.Model = 3;
            rhedasword.Extension = 1;
            rhedasword.Name = "Rheda's Sword";

            Level = 2;
            QuestGiver = Rheda;
            Rewards.Experience = 90;
            Rewards.MoneyPercent = 100;
            Rewards.AddOptionalItem(RecruitsShortSword);
            Rewards.AddOptionalItem(RecruitsDirk);
            Rewards.AddOptionalItem(RecruitsMace);
            Rewards.AddOptionalItem(RecruitsStaff);
            Rewards.ChoiceOf = 1;

            BanditRecruitsKilled = AddGoal("Kill one bandit recruit", QuestGoal.GoalType.KillTask, 1, rhedasword);
        }

        [ScriptLoadedEvent]
        public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
        {
            if (!ServerProperties.Properties.LOAD_QUESTS)
                return;
            if (log.IsInfoEnabled)
                log.Info("Quest \"" + questTitle + "\" initializing ...");


            #region defineNPCS

            GameNPC[] npcs = WorldMgr.GetNPCsByName("Rheda", eRealm.Albion);

            if (npcs.Length == 0)
            {
                Rheda = new GameNPC();
                Rheda.Model = 6;
                Rheda.Name = "Rheda";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + Rheda.Name + ", creating him ...");
                //k109: My preference, no guildname for quest NPCs.  Uncomment if you like that...
                //Rheda.GuildName = "Part of " + questTitle + " Quest";
                Rheda.Realm = eRealm.Albion;
                Rheda.CurrentRegionID = 1;

                GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
                template.AddNPCEquipment(eInventorySlot.HandsArmor, 80);
                template.AddNPCEquipment(eInventorySlot.FeetArmor, 54);
                template.AddNPCEquipment(eInventorySlot.TorsoArmor, 51);
                template.AddNPCEquipment(eInventorySlot.LegsArmor, 52);
                template.AddNPCEquipment(eInventorySlot.ArmsArmor, 53);
                Rheda.Inventory = template.CloseTemplate();
                Rheda.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

                Rheda.Size = 52;
                Rheda.Level = 25;
                Rheda.X = 559712;
                Rheda.Y = 513513;
                Rheda.Z = 2428;
                Rheda.Heading = 3822;

                if (SAVE_INTO_DATABASE)
                    Rheda.SaveIntoDatabase();

                Rheda.AddToWorld();
            }
            else
                Rheda = npcs[0];

            #endregion

            GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
            GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

            GameEventMgr.AddHandler(Rheda, GameLivingEvent.Interact, new DOLEventHandler(TalkToRheda));
            GameEventMgr.AddHandler(Rheda, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToRheda));

            Rheda.AddQuestToGive(typeof(RecruitingNothingButTrouble));

            if (log.IsInfoEnabled)
                log.Info("Quest \"" + questTitle + "\" initialized");
        }

        [ScriptUnloadedEvent]
        public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
        {
            if (Rheda == null)
                return;

            GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
            GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

            GameEventMgr.RemoveHandler(Rheda, GameObjectEvent.Interact, new DOLEventHandler(TalkToRheda));
            GameEventMgr.RemoveHandler(Rheda, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToRheda));

            Rheda.RemoveQuestToGive(typeof(RecruitingNothingButTrouble));
        }

        protected static void TalkToRheda(DOLEvent e, object sender, EventArgs args)
        {
            //We get the player from the event arguments and check if he qualifies		
            GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
            if (player == null)
                return;

            if (Rheda.CanGiveQuest(typeof(RecruitingNothingButTrouble), player) <= 0)
                return;


            RecruitingNothingButTrouble quest = player.IsDoingQuest(typeof(RecruitingNothingButTrouble)) as RecruitingNothingButTrouble;
            Rheda.TurnTo(player);

            if (e == GameObjectEvent.Interact)
            {
                if (quest == null)
                {
                    quest = new RecruitingNothingButTrouble();
                    quest.QuestGiver = Rheda;
                    quest.OfferQuest(player);
                }
                else
                {
                    if (quest.Step == 1 && quest.BanditRecruitsKilled.IsAchieved)
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

            if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(RecruitingNothingButTrouble)))
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
            if (player.IsDoingQuest(typeof(RecruitingNothingButTrouble)) != null)
                return true;

            // This checks below are only performed is player isn't doing quest already

            if (player.Level < minimumLevel || player.Level > maximumLevel)
                return false;

            return true;
        }

        private static void CheckPlayerAbortQuest(GamePlayer player, byte response)
        {
            RecruitingNothingButTrouble quest = player.IsDoingQuest(typeof(RecruitingNothingButTrouble)) as RecruitingNothingButTrouble;

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

            if (Rheda.CanGiveQuest(typeof(RecruitingNothingButTrouble), player) <= 0)
                return;

            if (player.IsDoingQuest(typeof(RecruitingNothingButTrouble)) != null)
                return;

            if (response == 0x00)
            {
                // Player declined, don't do anything.
            }
            else
            {
                // Player accepted, let's try to give him the quest.

                if (!Rheda.GiveQuest(typeof(RecruitingNothingButTrouble), player, 1))
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
                String desc = "I remember what it was like to be a reckless youth. I think we all had our moments when we were younger. I try to tolerate the antics of out local mischievous youth the best I can. Adults that encourage that kind of behavior, however, really upsets me. Youth are impressionable by nature, and adults that take advantage of that need to be taught a lesson.\n\n"
                    + "Take the bandits in the forest, for example. Most of them are a bunch of young thugs with nothing better to do, without an apprenticeship to keep them out of trouble. A few of them, though, are young adults who recruit the teens of our town, for the sole purpose of causing trouble. Just last week, some of them vandalized the rear of Elvar's smithy with brownie's blood. And, one of them stole one of my best swords!\n\n"
                    + "Something needs to be done about them. I know Sir Dorian is coordinating efforts to flush them out of the forest; perhaps you can do something to help?\n\n";
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
                return "One of the bandit recruits still has Rheda's sword. Defeat bandit recruits until you find her sword. Return to Rheda in Cotswold once you find it.";
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
                text += "Well I'll be!  You actually managed to get my sword back!  I must admit I thought I would never see it again.  Please accept this modest reward as payment for your services.";
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
            if (player.IsDoingQuest(typeof(RecruitingNothingButTrouble)) == null)
                return;


            if (Step == 1 && e == GameLivingEvent.EnemyKilled)
            {
                EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
                if (gArgs.Target.Name.IndexOf("bandit recruit") >= 0)
                {
                    if (!BanditRecruitsKilled.IsAchieved)
                    {
                        BanditRecruitsKilled.Advance();
                        return;
                    }
                }
            }
        }

        public override void AbortQuest()
        {
            base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

        }
    }
}
