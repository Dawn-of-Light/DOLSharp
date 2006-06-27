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
using System.Collections.Specialized;
using System.Reflection;
using System.Text;
using DOL.GS.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.Scripts;
using log4net;

namespace DOL.GS.Quests
{
	/// <summary>
	/// Declares the abstract quest class from which all user created
	/// quests must derive!
	/// </summary>
	public abstract class AbstractQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region Declaraction

		/// <summary>
		/// The unique active quest identifier in the db
		/// </summary>
		protected int m_id;

		/// <summary>
		/// The actual quest step
		/// </summary>
		protected byte m_step;

		/// <summary>
		/// The player doing the quest
		/// </summary>
		protected GamePlayer m_questPlayer;

		/// <summary>
		/// Gets or sets the unique active quest identifier in the db
		/// </summary>
		public int AbstractQuestID
		{
			get	{ return m_id; }
			set	{ m_id = value; }
		}

		/// <summary>
		/// Gets or Sets the current step of the quest.
		/// </summary>
		public virtual byte Step
		{
			get { return m_step; }
			set { m_step = value; }
		}

		/// <summary>
		/// Gets or sets the player doing the quest
		/// </summary>
		public GamePlayer QuestPlayer
		{
			get	{ return m_questPlayer; }
			set	{ m_questPlayer = value; }
		}

		/// <summary>
		/// Retrieves the name of the quest
		/// </summary>
		public abstract string Name
		{
			get;
		}

		/// <summary>
		/// Retrieves the description for the current quest step
		/// </summary>
		public abstract string Description
		{
			get;
		}

		#endregion

		#region Function
		/// <summary>
		/// Change the Quest Step, it will change the 
		/// description and also update the player quest list!
		/// </summary>
		public virtual void ChangeQuestStep(byte newStep)
		{
			m_step = newStep; 
			m_questPlayer.Out.SendQuestUpdate(this);
		}

		/// <summary>
		/// Called to finish the quest.
		/// Should be overridden and some rewards given etc.
		/// </summary>
		public virtual void FinishQuest()
		{
			m_step = 0; // tag this quest as finished
			m_questPlayer.Out.SendQuestListUpdate();
			m_questPlayer.Out.SendMessage("You finish the "+Name+" quest!",eChatType.CT_System,eChatLoc.CL_SystemWindow);

			m_questPlayer.ActiveQuests.Remove(this);
			if(AbstractQuestID != 0) GameServer.Database.DeleteObject(this);
			
			bool isNewQuestFinished = true;
			foreach(FinishedQuest quest in m_questPlayer.FinishedQuests)
			{
				if(quest.FinishedQuestType.Equals(GetType()))
				{
					quest.Count ++;
					isNewQuestFinished = false;
					break;
				}
			}
			
			if(isNewQuestFinished)
			{
				FinishedQuest newFinishedQuest = new FinishedQuest();
				newFinishedQuest.FinishedQuestType = GetType();
				newFinishedQuest.Count = 1;
				m_questPlayer.FinishedQuests.Add(newFinishedQuest);
			}

			//Update all npc in the visibility range
			foreach (GameNPC npc in m_questPlayer.GetNPCsInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				m_questPlayer.Out.SendNPCsQuestEffect(npc, QuestMgr.CanGiveOneQuest(npc, m_questPlayer));
			}
		}

		/// <summary>
		/// Called to abort the quest and remove it from the database!
		/// </summary>
		public virtual void CheckPlayerAbortQuest(GamePlayer player, byte response)
		{
			if(response == 0x01)
			{
				AbortQuest();
			}
		}

		/// <summary>
		/// Aborts the quest and removes it from the database
		/// </summary>
		public virtual void AbortQuest()
		{
			m_step = 0; // tag this quest as finished
			m_questPlayer.Out.SendQuestListUpdate();
			m_questPlayer.Out.SendMessage("You abort the " + Name + " quest!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

			m_questPlayer.ActiveQuests.Remove(this);
			if (AbstractQuestID != 0) GameServer.Database.DeleteObject(this);

			//Update all npc in the visibility range
			foreach (GameNPC npc in m_questPlayer.GetNPCsInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				m_questPlayer.Out.SendNPCsQuestEffect(npc, QuestMgr.CanGiveOneQuest(npc, m_questPlayer));
			}
		}

		/// <summary>
		/// This method needs to be implemented in each quest.
		/// It is the core of the quest. The global event hook of the GamePlayer.
		/// This method will be called whenever a GamePlayer with this quest
		/// fires ANY event!
		/// </summary>
		/// <param name="e">The event type</param>
		/// <param name="sender">The sender of the event</param>
		/// <param name="args">The event arguments</param>
		public abstract void Notify(DOLEvent e, object sender, EventArgs args);

		#endregion

	}
}