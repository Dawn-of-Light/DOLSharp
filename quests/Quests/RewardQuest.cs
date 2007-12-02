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
using DOL.Database;
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
		private QuestRewards m_rewards;

		public RewardQuest() : base()
		{
			m_rewards = new QuestRewards();
		}

		/// <summary>
		/// Constructs a new RewardQuest.
		/// </summary>
		/// <param name="questingPlayer">The player doing this quest</param>
		public RewardQuest(GamePlayer questingPlayer) 
			: this(questingPlayer, 1)
		{
		}

		/// <summary>
		/// Constructs a new RewardQuest.
		/// </summary>
		/// <param name="questingPlayer">The player doing this quest</param>
		/// <param name="step">The current step the player is on</param>
		public RewardQuest(GamePlayer questingPlayer,int step) 
			: base(questingPlayer, step)
		{
			m_rewards = new QuestRewards();
		}

		/// <summary>
		/// Constructs a new RewardQuest from a database Object
		/// </summary>
		/// <param name="questingPlayer">The player doing the quest</param>
		/// <param name="dbQuest">The database object</param>
		public RewardQuest(GamePlayer questingPlayer, DBQuest dbQuest) 
			: base(questingPlayer, dbQuest)
		{
			m_rewards = new QuestRewards();
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
		/// The quest goal.
		/// </summary>
		public virtual String Goal
		{
			get { return "QUEST GOAL UNDEFINED"; }
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
		/// Player is getting closer to the quest goal.
		/// </summary>
		/// <param name="player"></param>
		protected virtual void GoalAdvance(GamePlayer player)
		{
			player.Out.SendMessage(Goal, eChatType.CT_ScreenCenter, eChatLoc.CL_SystemWindow);
			player.Out.SendQuestUpdate(this);
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

				for (int reward = 0; reward < rewardArgs.CountChosen; ++reward)
					Rewards.ChooseReward(reward);

				FinishQuest();
			}
		}

		/// <summary>
		/// Called when quest is finished, hands out rewards.
		/// </summary>
		public override void FinishQuest()
		{
			base.FinishQuest();

			QuestPlayer.GainExperience(Rewards.XP);
			QuestPlayer.ReceiveMoney(QuestGiver, Rewards.Money);
			foreach (ItemTemplate basicReward in Rewards.BasicItemRewards)
				GiveItem(QuestPlayer, basicReward);
			foreach (ItemTemplate optionalReward in Rewards.ChosenItemRewards)
				GiveItem(QuestPlayer, optionalReward);
		}

		/// <summary>
		/// Class encapsulating the different types of rewards.
		/// </summary>
		public class QuestRewards
		{
			private long m_moneyReward, m_xpReward;
			private List<ItemTemplate> m_basicItemRewards, m_optionalItemRewards;
			private int m_choiceOf;
			private List<ItemTemplate> m_chosenItemRewards;
			public QuestRewards()
			{
				m_moneyReward = 0;
				m_xpReward = 0;
				m_basicItemRewards = new List<ItemTemplate>();
				m_optionalItemRewards = new List<ItemTemplate>();
				m_choiceOf = 0;
				m_chosenItemRewards = new List<ItemTemplate>();
			}

			/// <summary>
			/// Add a basic reward (up to a maximum of 8).
			/// </summary>
			/// <param name="reward"></param>
			public void AddBasicReward(ItemTemplate reward)
			{
				if (m_basicItemRewards.Count < 8)
					m_basicItemRewards.Add(reward);
			}

			/// <summary>
			/// Add an optional reward (up to a maximum of 8).
			/// </summary>
			/// <param name="reward"></param>
			public void AddOptionalReward(ItemTemplate reward)
			{
				if (m_optionalItemRewards.Count < 8)
					m_optionalItemRewards.Add(reward);
			}

			/// <summary>
			/// Pick an optional reward from the list.
			/// </summary>
			/// <param name="reward"></param>
			/// <returns></returns>
			public bool ChooseReward(int reward)
			{
				if (reward > m_optionalItemRewards.Count)
					return false;

				m_chosenItemRewards.Add(m_optionalItemRewards[reward]);
				return true;
			}

			/// <summary>
			/// Money awarded for completing this quest.
			/// </summary>
			public long Money
			{
				get { return m_moneyReward; }
				set { m_moneyReward = value; }
			}

			/// <summary>
			/// XP awarded for completing this quest.
			/// </summary>
			public long XP
			{
				get { return m_xpReward; }
				set { m_xpReward = value; }
			}

			/// <summary>
			/// List of basic item rewards.
			/// </summary>
			public List<ItemTemplate> BasicItemRewards
			{
				get { return m_basicItemRewards; }
			}

			/// <summary>
			/// List of optional item rewards.
			/// </summary>
			public List<ItemTemplate> OptionalItemRewards
			{
				get { return m_optionalItemRewards; }
			}

			/// <summary>
			/// List of optional rewards the player actually picked.
			/// </summary>
			public List<ItemTemplate> ChosenItemRewards
			{
				get { return m_chosenItemRewards; }
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
					if (m_optionalItemRewards.Count > 0)
						m_choiceOf = Math.Min(Math.Max(1, value), m_optionalItemRewards.Count);
				}
			}
		}
	}
}
