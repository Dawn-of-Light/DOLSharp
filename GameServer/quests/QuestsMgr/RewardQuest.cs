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
using System;
using System.Collections.Generic;
using DOL.Database;
using DOL.Events;
using DOL.Language;
using DOL.GS.PacketHandler;

namespace DOL.GS.Quests
{
    /// <summary>
    /// A quest type with basic and optional item rewards using
    /// the enhanced quest dialog.
    /// </summary>
    /// <author>Aredhel</author>
    public class RewardQuest : BaseQuest
    {
        public RewardQuest()
        {
            Rewards = new QuestRewards(this);
            Goals = new List<QuestGoal>();
        }

        /// <summary>
        /// Constructs a new RewardQuest.
        /// </summary>
        /// <param name="questingPlayer">The player doing this quest</param>
        public RewardQuest(GamePlayer questingPlayer)
            : this(questingPlayer, 1) { }

        /// <summary>
        /// Constructs a new RewardQuest.
        /// </summary>
        /// <param name="questingPlayer">The player doing this quest</param>
        /// <param name="step">The current step the player is on</param>
        public RewardQuest(GamePlayer questingPlayer,int step)
            : base(questingPlayer, step)
        {
            Rewards = new QuestRewards(this);
            Goals = new List<QuestGoal>();
        }

        /// <summary>
        /// Constructs a new RewardQuest from a database Object
        /// </summary>
        /// <param name="questingPlayer">The player doing the quest</param>
        /// <param name="dbQuest">The database object</param>
        public RewardQuest(GamePlayer questingPlayer, DBQuest dbQuest)
            : base(questingPlayer, dbQuest)
        {
            Rewards = new QuestRewards(this);
            Goals = new List<QuestGoal>();
        }

        /// <summary>
        /// Add a goal for this quest.
        /// </summary>
        /// <param name="description"></param>
        /// <param name="type"></param>
        /// <param name="targetNumber"></param>
        /// <param name="questItem"></param>
        protected QuestGoal AddGoal(string description, QuestGoal.GoalType type, int targetNumber, ItemTemplate questItem)
        {
            QuestGoal goal = new QuestGoal("none", this, description, type, Goals.Count + 1, targetNumber, questItem);
            Goals.Add(goal);
            return goal;
        }

        /// <summary>
        /// Add a goal for this quest and give it a unique identifier
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="description"></param>
        /// <param name="type"></param>
        /// <param name="targetNumber"></param>
        /// <param name="questItem"></param>
        /// <returns></returns>
        protected QuestGoal AddGoal(string id, string description, QuestGoal.GoalType type, int targetNumber, ItemTemplate questItem)
        {
            QuestGoal goal = new QuestGoal(id, this, description, type, Goals.Count + 1, targetNumber, questItem);
            Goals.Add(goal);
            return goal;
        }

        /// <summary>
        /// The NPC giving the quest.
        /// </summary>
        public GameNPC QuestGiver { get; set; }

        /// <summary>
        /// List of all goals for this quest
        /// </summary>
        public List<QuestGoal> Goals { get; }

        /// <summary>
        /// The rewards given on successful completion of this quest.
        /// </summary>
        public QuestRewards Rewards { get; set; }

        /// <summary>
        /// The fully-fledged story to the quest.
        /// </summary>
        public virtual string Story => "QUEST STORY UNDEFINED";

        /// <summary>
        /// A summary of the quest text.
        /// </summary>
        public virtual string Summary => "QUEST SUMMARY UNDEFINED";

        /// <summary>
        /// Text showing upon finishing the quest.
        /// </summary>
        public virtual string Conclusion => "QUEST CONCLUSION UNDEFINED";

        public override bool CheckQuestQualification(GamePlayer player)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// Offer this quest to a player.
        /// </summary>
        /// <param name="player"></param>
        public virtual void OfferQuest(GamePlayer player)
        {
            if (CheckQuestQualification(player))
            {
                OfferPlayer = player;
                player.Out.SendQuestOfferWindow(QuestGiver, player, this);
            }
        }

        /// <summary>
        /// Let player choose rewards (if any).
        /// </summary>
        /// <param name="player"></param>
        public virtual void ChooseRewards(GamePlayer player)
        {
            player.Out.SendQuestRewardWindow(QuestGiver, player, this);
        }

        /// <summary>
        /// Handles events.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
            base.Notify(e, sender, args);
            if (e == GamePlayerEvent.QuestRewardChosen)
            {
                if (!(args is QuestRewardChosenEventArgs rewardArgs))
                {
                    return;
                }

                // Check if this particular quest has been finished.
                if (QuestMgr.GetIDForQuestType(GetType()) != rewardArgs.QuestID)
                {
                    return;
                }

                for (int reward = 0; reward < rewardArgs.CountChosen; ++reward)
                {
                    Rewards.Choose(rewardArgs.ItemsChosen[reward]);
                }

                // k109: Handle the player not choosing a reward.
                if (Rewards.ChoiceOf > 0 && rewardArgs.CountChosen <= 0)
                {
                    QuestPlayer.Out.SendMessage(LanguageMgr.GetTranslation(QuestPlayer.Client, "RewardQuest.Notify"), eChatType.CT_System, eChatLoc.CL_ChatWindow);
                    return;
                }

                FinishQuest();
            }
        }

        /// <summary>
        /// Play a sound effect when player has acquired the quest.
        /// </summary>
        /// <param name="player"></param>
        public override void OnQuestAssigned(GamePlayer player)
        {
            player.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(player.Client.Account.Language, "RewardQuest.OnQuestAssigned", Name)), eChatType.CT_ScreenCenter, eChatLoc.CL_SystemWindow);
            player.Out.SendSoundEffect(7, 0, 0, 0, 0, 0);
        }

        /// <summary>
        /// Called when quest is finished, hands out rewards.
        /// </summary>
        public override void FinishQuest()
        {
            int inventorySpaceRequired = Rewards.BasicItems.Count + Rewards.ChosenItems.Count;

            if (QuestPlayer.Inventory.IsSlotsFree(inventorySpaceRequired, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
            {
                base.FinishQuest();
                QuestPlayer.Out.SendSoundEffect(11, 0, 0, 0, 0, 0);
                QuestPlayer.GainExperience(GameLiving.eXPSource.Quest, Rewards.Experience);
                QuestPlayer.AddMoney(Rewards.Money);
                InventoryLogging.LogInventoryAction($"(QUEST;{Name})", QuestPlayer, eInventoryActionType.Quest, Rewards.Money);
                if (Rewards.GiveBountyPoints > 0)
                {
                    QuestPlayer.GainBountyPoints(Rewards.GiveBountyPoints);
                }

                if (Rewards.GiveRealmPoints > 0)
                {
                    QuestPlayer.GainRealmPoints(Rewards.GiveRealmPoints);
                }

                foreach (ItemTemplate basicReward in Rewards.BasicItems)
                {
                    GiveItem(QuestPlayer, basicReward);
                }

                foreach (ItemTemplate optionalReward in Rewards.ChosenItems)
                {
                    GiveItem(QuestPlayer, optionalReward);
                }

                QuestPlayer.Out.SendNPCsQuestEffect(QuestGiver, QuestGiver.GetQuestIndicator(QuestPlayer));
            }
            else
            {
                QuestPlayer.Out.SendMessage($"Your inventory is full, you need {inventorySpaceRequired} free slot(s) to complete this quest.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                Rewards.ChosenItems.Clear();
            }
        }

        /// <summary>
        /// A single quest goal.
        /// </summary>
        public class QuestGoal
        {
            private readonly RewardQuest _quest;
            private readonly string _description;
            private readonly int _index;

            private int _current;
            private int _target;
            private ItemTemplate _questItem;

            public enum GoalType { KillTask = 3, ScoutMission = 5 };    // These are just a hunch for now.

            /// <summary>
            /// Constructs a new QuestGoal.
            /// </summary>
            /// <param name="id">An id used to fidn this quest goal.</param>
            /// <param name="quest">The quest this goal is a part of.</param>
            /// <param name="description">The description of the goal.</param>
            /// <param name="type"></param>
            /// <param name="index"></param>
            /// <param name="target"></param>
            public QuestGoal(string id, RewardQuest quest, string description, GoalType type, int index, int target, ItemTemplate questItem)
            {
                Id = id;
                _quest = quest;
                _description = description;
                Type = type;
                _index = index;
                _current = 0;
                _target = 0;
                Target = target;
                _questItem = questItem;
            }

            /// <summary>
            /// An id for this quest goal
            /// </summary>
            public string Id { get; }

            /// <summary>
            /// Ready-to-use description of the goal and its current status.
            /// </summary>
            public string Description
            {
                get
                {
                    if (_quest.QuestPlayer != null)
                    {
                        return string.Format(LanguageMgr.GetTranslation(_quest.QuestPlayer.Client, "RewardQuest.Description", _description, Current, Target));
                    }

                    return _description;
                }
            }

            /// <summary>
            /// The type of the goal, i.e. whether to scout or to kill things.
            /// </summary>
            public GoalType Type { get; }

            /// <summary>
            /// The quest item required for this goal.
            /// </summary>
            public ItemTemplate QuestItem
            {
                get => (Current > 0) ? _questItem : null;
                set => _questItem = value;
            }

            /// <summary>
            /// Current status of this goal.
            /// </summary>
            protected int Current
            {
                get
                {
                    if (_quest.QuestPlayer == null)
                    {
                        return _current;
                    }

                    string propertyValue = _quest.GetCustomProperty($"goal{_index}Current");
                    if (propertyValue == null)
                    {
                        Current = 0;
                        return Current;
                    }

                    return short.Parse(propertyValue);
                }

                set
                {
                    if (_quest.QuestPlayer == null)
                    {
                        _current = value;
                    }
                    else
                    {
                        _quest.SetCustomProperty($"goal{_index}Current", value.ToString());
                        _quest.SaveIntoDatabase();
                    }
                }
            }

            /// <summary>
            /// Target status of this goal.
            /// </summary>
            protected int Target
            {
                get
                {
                    if (_quest.QuestPlayer == null)
                    {
                        return _current;
                    }

                    string propertyValue = _quest.GetCustomProperty($"goal{_index}Target");
                    if (propertyValue == null)
                    {
                        Target = 0;
                        return Target;
                    }

                    return short.Parse(propertyValue);
                }

                set
                {
                    if (_quest.QuestPlayer == null)
                    {
                        _target = value;
                    }
                    else
                    {
                        _quest.SetCustomProperty($"goal{_index}Target", value.ToString());
                        _quest.SaveIntoDatabase();
                    }
                }
            }

            /// <summary>
            /// Whether or not the goal has been achieved yet.
            /// </summary>
            public bool IsAchieved => Current == Target;

            public void Advance()
            {
                if (Current < Target)
                {
                    Current++;
                    _quest.QuestPlayer.Out.SendMessage(Description, eChatType.CT_ScreenCenter,
                        eChatLoc.CL_SystemWindow);
                    _quest.QuestPlayer.Out.SendQuestUpdate(_quest);

                    // Check for updates
                    if (IsAchieved)
                    {
                        // check if all quest is achieved
                        bool done = true;
                        foreach (QuestGoal goal in _quest.Goals)
                        {
                            done &= goal.IsAchieved;
                        }

                        if (done && _quest.QuestGiver.IsWithinRadius(_quest.QuestPlayer, WorldMgr.VISIBILITY_DISTANCE))
                        {
                            _quest.QuestPlayer.Out.SendNPCsQuestEffect(_quest.QuestGiver, _quest.QuestGiver.GetQuestIndicator(_quest.QuestPlayer));
                        }
                    }
                }
            }

            /*
             * Not quite sure about the meaning of the following locations data,
             * but have to provide it for the quest update packet nonetheless.
             */

            public int ZoneId1 { get; } = 0;

            public int XOffset1 { get; } = 0;

            public int YOffset1 { get; } = 0;

            public int ZoneId2 { get; } = 0;

            public int XOffset2 { get; } = 0;

            public int YOffset2 { get; } = 0;
        }

        /// <summary>
        /// Class encapsulating the different types of rewards.
        /// </summary>
        public class QuestRewards
        {
            private readonly RewardQuest _quest;

            private int _moneyPercent;
            private long _experience;
            private int _choiceOf;

            public QuestRewards(RewardQuest quest)
            {
                _quest = quest;
                _moneyPercent = 0;
                _experience = 0;
                BasicItems = new List<ItemTemplate>();
                OptionalItems = new List<ItemTemplate>();
                _choiceOf = 0;
                ChosenItems = new List<ItemTemplate>();
                GiveBountyPoints = 0;
                GiveRealmPoints = 0;
                GiveGold = 0;
            }

            public int GiveGold { get; set; }

            public int GiveRealmPoints { get; set; }

            public int GiveBountyPoints { get; set; }

            /// <summary>
            /// The maximum amount of copper awarded for a quest with a
            /// particular level.
            /// </summary>
            private readonly long[] _maxCopperForLevel = {
                0,
                115,
                310,
                630,
                1015,
                1923,
                3106,
                4778,
                7490,
                11365,
                14700,      // level 10
                17897,
                21601,
                25359,
                31171,
                38322,
                47197,
                55945,
                67005,
                81063,
                92865,      // level 20
                109341,
                145712,
                153350,
                179629,
                208979,
                242462,
                273694,
                320555,
                375366,
                432613,     // level 30
                488446,
                556087,
                641920,
                735351,
                816085,
                935985,
                1059493,
                1142836,
                1282954,
                3000555,    // level 40
                5031641,
                5546643,
                5955362,
                6729142,
                7352371,
                8235521,
                9064368,
                10056130,
                11018817,
                11018817 // level 50, this appears to be the overall cap
            };

            /// <summary>
            /// Add a basic reward (up to a maximum of 8).
            /// </summary>
            /// <param name="reward"></param>
            public void AddBasicItem(ItemTemplate reward)
            {
                if (BasicItems.Count < 8)
                {
                    BasicItems.Add(reward);
                }
            }

            /// <summary>
            /// Add an optional reward (up to a maximum of 8).
            /// </summary>
            /// <param name="reward"></param>
            public void AddOptionalItem(ItemTemplate reward)
            {
                if (OptionalItems.Count < 8)
                {
                    OptionalItems.Add(reward);
                }
            }

            /// <summary>
            /// Pick an optional reward from the list.
            /// </summary>
            /// <param name="reward"></param>
            /// <returns></returns>
            public bool Choose(int reward)
            {
                if (reward > OptionalItems.Count)
                {
                    return false;
                }

                ChosenItems.Add(OptionalItems[reward]);
                return true;
            }

            /// <summary>
            /// Money awarded for completing this quest. This is a percentage
            /// of the maximum amount of money awarded for a quest with this level.
            /// This in turn means that there is a cap (100%) to earning money
            /// from quests.
            /// </summary>
            public int MoneyPercent
            {
                get => _moneyPercent;
                set { if (value >= 0 && value <= 100)
                    {
                        _moneyPercent = value;
                    }
                }
            }

            /// <summary>
            /// Money awarded for completing this quest. This is a flat value.
            /// You shouldn't be able to set this directly, because a level dependent
            /// cap applies to all money earned from quests.
            /// </summary>
            public long Money => (_maxCopperForLevel[_quest.Level] * MoneyPercent / 100) + (GiveGold * 10000);

            /// <summary>
            /// Experience awarded for completing this quest. This is a percentage
            /// of the amount of experience the questing player needs to get from
            /// their current level to the next level, not taking into account any
            /// experience the player already has gained towards the next level.
            /// Because this depends on the current level of the interacting player
            /// it doesn't make sense to change it from your scripts.
            /// </summary>
            public int ExperiencePercent(GamePlayer player)
            {
                int currentLevel = player.Level;
                if (currentLevel > player.MaxLevel)
                {
                    return 0;
                }

                long experienceToLevel = player.GetExperienceNeededForLevel(currentLevel + 1) -
                    player.GetExperienceNeededForLevel(currentLevel);

                return (int)((_experience * 100) / experienceToLevel);
            }

            /// <summary>
            /// Experience awarded for completing this quest. This is a flat value.
            /// </summary>
            public long Experience
            {
                get => _experience;
                set
                {
                    _experience = value;
                    if (_experience < 0)
                    {
                        _experience = 0;
                    }
                }
            }

            /// <summary>
            /// List of basic item rewards.
            /// </summary>
            public List<ItemTemplate> BasicItems { get; }

            /// <summary>
            /// List of optional item rewards.
            /// </summary>
            public List<ItemTemplate> OptionalItems { get; }

            /// <summary>
            /// List of optional rewards the player actually picked.
            /// </summary>
            public List<ItemTemplate> ChosenItems { get; }

            /// <summary>
            /// The number of items the player can choose from the optional
            /// item reward list.
            /// </summary>
            public int ChoiceOf
            {
                get => _choiceOf;
                set
                {
                    if (OptionalItems.Count > 0)
                    {
                        _choiceOf = Math.Min(Math.Max(1, value), OptionalItems.Count);
                    }
                }
            }
        }
    }
}
