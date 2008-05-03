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
using System.Text;
using DOL.Database2;
using DOL.GS.PacketHandler;
using DOL.Events;

namespace DOL.GS.Quests
{
	/// <summary>
	/// A quest type with basic and optional item rewards using
	/// the enhanced quest dialog.
	/// </summary>
	/// <author>Aredhel</author>
	public class RewardQuest : BaseQuest
	{
		private GameNPC m_questGiver;
		private List<QuestGoal> m_goals;
		private QuestRewards m_rewards;

		public RewardQuest() : base()
		{
			m_rewards = new QuestRewards(this);
			m_goals = new List<QuestGoal>();
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
			m_rewards = new QuestRewards(this);
			m_goals = new List<QuestGoal>();
		}

		/// <summary>
		/// Constructs a new RewardQuest from a database Object
		/// </summary>
		/// <param name="questingPlayer">The player doing the quest</param>
		/// <param name="dbQuest">The database object</param>
		public RewardQuest(GamePlayer questingPlayer, DBQuest dbQuest) 
			: base(questingPlayer, dbQuest)
		{
			m_rewards = new QuestRewards(this);
			m_goals = new List<QuestGoal>();
		}

		/// <summary>
		/// Add a goal for this quest.
		/// </summary>
		/// <param name="description"></param>
		/// <param name="type"></param>
		/// <param name="targetNumber"></param>
		/// <param name="questItem"></param>
		protected QuestGoal AddGoal(String description, QuestGoal.GoalType type, int targetNumber,
			ItemTemplate questItem)
		{
			QuestGoal goal = new QuestGoal(this, description, type, m_goals.Count + 1, targetNumber,
				questItem);
			m_goals.Add(goal);
			return goal;
		}

		/// <summary>
		/// The NPC giving the quest.
		/// </summary>
		public GameNPC QuestGiver
		{
			get { return m_questGiver; }
			set { m_questGiver = value; }
		}

		/// <summary>
		/// List of all goals for this quest
		/// </summary>
		public List<QuestGoal> Goals
		{
			get { return m_goals; }
		}

		/// <summary>
		/// The rewards given on successful completion of this quest.
		/// </summary>
		public QuestRewards Rewards
		{
			get { return m_rewards; }
			set { m_rewards = value; }
		}

		/// <summary>
		/// The fully-fledged story to the quest.
		/// </summary>
		public virtual String Story
		{
			get { return "QUEST STORY UNDEFINED"; }
		}

		/// <summary>
		/// A summary of the quest text.
		/// </summary>
		public virtual String Summary
		{
			get { return "QUEST SUMMARY UNDEFINED"; }
		}

		/// <summary>
		/// Text showing upon finishing the quest.
		/// </summary>
		public virtual String Conclusion
		{
			get { return "QUEST CONCLUSION UNDEFINED"; }
		}

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
				player.Out.SendQuestOfferWindow(QuestGiver, player, this);
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
				QuestRewardChosenEventArgs rewardArgs = args as QuestRewardChosenEventArgs;
				if (rewardArgs == null)
					return;

				// Check if this particular quest has been finished.

				if (QuestMgr.GetIDForQuestType(this.GetType()) != rewardArgs.QuestID)
					return;

				for (int reward = 0; reward < rewardArgs.CountChosen; ++reward)
					Rewards.Choose(rewardArgs.ItemsChosen[reward]);

                //k109: Handle the player not choosing a reward.
                if (Rewards.ChoiceOf > 0 && rewardArgs.CountChosen <= 0)
                {
                    QuestPlayer.Out.SendMessage("You must choose a reward!", eChatType.CT_System, eChatLoc.CL_ChatWindow);
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
			player.Out.SendMessage(String.Format("You have acquired the {0} quest.",
				Name), eChatType.CT_ScreenCenter, eChatLoc.CL_SystemWindow);
			player.Out.SendSoundEffect(7, 0, 0, 0, 0, 0);
		}

		/// <summary>
		/// Called when quest is finished, hands out rewards.
		/// </summary>
		public override void FinishQuest()
		{
			base.FinishQuest();
			QuestPlayer.Out.SendSoundEffect(11, 0, 0, 0, 0, 0);
			QuestPlayer.GainExperience(Rewards.Experience);
            //k109: Could not get ReceiveMoney to work...trying AddMoney...
            QuestPlayer.AddMoney(Rewards.Money);
            //QuestPlayer.ReceiveMoney(QuestGiver, Rewards.Money);
			foreach (ItemTemplate basicReward in Rewards.BasicItems)
				GiveItem(QuestPlayer, basicReward);
			foreach (ItemTemplate optionalReward in Rewards.ChosenItems)
				GiveItem(QuestPlayer, optionalReward);
			QuestPlayer.Out.SendNPCsQuestEffect(QuestGiver, QuestGiver.CanGiveOneQuest(QuestPlayer));
		}

		/// <summary>
		/// A single quest goal.
		/// </summary>
		public class QuestGoal
		{
			private RewardQuest m_quest;
			private String m_description;
			private int m_index;
			private int m_current, m_target;
			private int m_zoneID1 = 0, m_xOffset1 = 0, m_yOffset1 = 0;
			private int m_zoneID2 = 0, m_xOffset2 = 0, m_yOffset2 = 0;
			private GoalType m_goalType;
			private ItemTemplate m_questItem = null;

			public enum GoalType { KillTask = 3, ScoutMission = 5 };	// These are just a hunch for now.

			/// <summary>
			/// Constructs a new QuestGoal.
			/// </summary>
			/// <param name="quest">The quest this goal is a part of.</param>
			/// <param name="description">The description of the goal.</param>
			/// <param name="type"></param>
			/// <param name="index"></param>
			/// <param name="target"></param>
			public QuestGoal(RewardQuest quest, String description, GoalType type, int index, int target,
				ItemTemplate questItem)
			{
				m_quest = quest;
				m_description = description;
				m_goalType = type;
				m_index = index;
				m_current = 0;
				m_target = 0;
				Target = target;
				m_questItem = questItem;
			}

			/// <summary>
			/// Ready-to-use description of the goal and its current status.
			/// </summary>
			public String Description
			{
				get { return String.Format("Quest Goal : {0} ({1}/{2})", m_description, Current, Target); }
			}

			/// <summary>
			/// The type of the goal, i.e. whether to scout or to kill things.
			/// </summary>
			public GoalType Type
			{
				get { return m_goalType; }
			}

			/// <summary>
			/// The quest item required for this goal.
			/// </summary>
			public ItemTemplate QuestItem
			{
				get { return (Current > 0) ? m_questItem : null; }
				set { m_questItem = value; }
			}

			/// <summary>
			/// Current status of this goal.
			/// </summary>
			protected int Current
			{
				get 
				{
					if (m_quest.QuestPlayer == null)
						return m_current;
					String propertyValue = m_quest.GetCustomProperty(String.Format("goal{0}Current", m_index));
					if (propertyValue == null)
					{
						Current = 0;
						return Current;
					}
					return Int16.Parse(propertyValue); 
				}
				set 
				{
					if (m_quest.QuestPlayer == null)
						m_current = value;
					else
					{
						m_quest.SetCustomProperty(String.Format("goal{0}Current", m_index), value.ToString());
						m_quest.SaveIntoDatabase();
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
					if (m_quest.QuestPlayer == null)
						return m_current;
					String propertyValue = m_quest.GetCustomProperty(String.Format("goal{0}Target", m_index));
					if (propertyValue == null)
					{
						Target = 0;
						return Target;
					}
					return Int16.Parse(propertyValue); 
				}
				set 
				{
					if (m_quest.QuestPlayer == null)
						m_target = value;
					else
					{
						m_quest.SetCustomProperty(String.Format("goal{0}Target", m_index), value.ToString());
						m_quest.SaveIntoDatabase();
					}
				}
			}

			/// <summary>
			/// Whether or not the goal has been achieved yet.
			/// </summary>
			public bool IsAchieved
			{
				get { return (Current == Target); }
			}

			public void Advance()
			{
				if (Current < Target)
				{
					Current++;
					m_quest.QuestPlayer.Out.SendMessage(Description, eChatType.CT_ScreenCenter, 
						eChatLoc.CL_SystemWindow);
					m_quest.QuestPlayer.Out.SendQuestUpdate(m_quest);
				}
			}

			/*
			 * Not quite sure about the meaning of the following locations data,
			 * but have to provide it for the quest update packet nonetheless.
			 */

			public int ZoneID1
			{
				get { return m_zoneID1; }
			}

			public int XOffset1
			{
				get { return m_xOffset1; }
			}

			public int YOffset1
			{
				get { return m_yOffset1; }
			}

			public int ZoneID2
			{
				get { return m_zoneID2; }
			}

			public int XOffset2
			{
				get { return m_xOffset2; }
			}

			public int YOffset2
			{
				get { return m_yOffset2; }
			}
		}

		/// <summary>
		/// Class encapsulating the different types of rewards.
		/// </summary>
		public class QuestRewards
		{
			private RewardQuest m_quest;
			private int m_moneyPercent;
			private long m_experience;
			private List<ItemTemplate> m_basicItems, m_optionalItems;
			private int m_choiceOf;
			private List<ItemTemplate> m_chosenItems;

			/// <summary>
			/// The maximum amount of copper awarded for a quest with a
			/// particular level.
			/// </summary>
			private long[] m_maxCopperForLevel = {
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
				14700,		// level 10
				17897,
				21601,
				25359,
				31171,
				38322,
				47197,
				55945,
				67005,
				81063,
				92865,		// level 20
				109341,
				145712,
				153350,
				179629,
				208979,
				242462,
				273694,
				320555,
				375366,
				432613,		// level 30
				488446,
				556087,
				641920,
				735351,
				816085,
				935985,
				1059493,
				1142836,
				1282954,
				3000555,	// level 40
				5031641,
				5546643,
				5955362,
				6729142,
				7352371,
				8235521,
				9064368,
				10056130,
				11018817,
				11018817	// level 50, this appears to be the overall cap
			};

			public QuestRewards(RewardQuest quest)
			{
				m_quest = quest;
				m_moneyPercent = 0;
				m_experience = 0;
				m_basicItems = new List<ItemTemplate>();
				m_optionalItems = new List<ItemTemplate>();
				m_choiceOf = 0;
				m_chosenItems = new List<ItemTemplate>();
			}

			/// <summary>
			/// Add a basic reward (up to a maximum of 8).
			/// </summary>
			/// <param name="reward"></param>
			public void AddBasicItem(ItemTemplate reward)
			{
				if (m_basicItems.Count < 8)
					m_basicItems.Add(reward);
			}

			/// <summary>
			/// Add an optional reward (up to a maximum of 8).
			/// </summary>
			/// <param name="reward"></param>
			public void AddOptionalItem(ItemTemplate reward)
			{
				if (m_optionalItems.Count < 8)
					m_optionalItems.Add(reward);
			}

			/// <summary>
			/// Pick an optional reward from the list.
			/// </summary>
			/// <param name="reward"></param>
			/// <returns></returns>
			public bool Choose(int reward)
			{
				if (reward > m_optionalItems.Count)
					return false;

				m_chosenItems.Add(m_optionalItems[reward]);
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
				get { return m_moneyPercent; }
				set { if (value >= 0 && value <= 100) m_moneyPercent = value; }
			}

			/// <summary>
			/// Money awarded for completing this quest. This is a flat value.
			/// You shouldn't be able to set this directly, because a level dependent
			/// cap applies to all money earned from quests.
			/// </summary>
			public long Money
			{
				get { return (long)((m_maxCopperForLevel[m_quest.Level] * MoneyPercent / 100)); }
			}

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
				if (currentLevel > 50)
					return 0;
				long experienceToLevel = GamePlayer.XPLevel[currentLevel] -
					GamePlayer.XPLevel[currentLevel-1];
				return (int)((m_experience * 100) / experienceToLevel);
			}

			/// <summary>
			/// Experience awarded for completing this quest. This is a flat value.
			/// </summary>
			public long Experience
			{
				get { return m_experience; }
				set
                {
                    m_experience = value;
                    if (m_experience < 0)
                        m_experience = 0;
                }
			}

			/// <summary>
			/// List of basic item rewards.
			/// </summary>
			public List<ItemTemplate> BasicItems
			{
				get { return m_basicItems; }
			}

			/// <summary>
			/// List of optional item rewards.
			/// </summary>
			public List<ItemTemplate> OptionalItems
			{
				get { return m_optionalItems; }
			}

			/// <summary>
			/// List of optional rewards the player actually picked.
			/// </summary>
			public List<ItemTemplate> ChosenItems
			{
				get { return m_chosenItems; }
			}

			/// <summary>
			/// The number of items the player can choose from the optional
			/// item reward list.
			/// </summary>
			public int ChoiceOf
			{
				get { return m_choiceOf; }
				set 
				{
					if (m_optionalItems.Count > 0)
						m_choiceOf = Math.Min(Math.Max(1, value), m_optionalItems.Count);
				}
			}
		}
	}
}
