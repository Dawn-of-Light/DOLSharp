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
using DOL.Events;
using DOL.GS.Spells;

namespace DOL.Events
{
	/// <summary>
	/// Provides a list of item rewards that the player chose from the 
	/// quest dialog.
	/// </summary>
	/// <author>Aredhel</author>
	class QuestRewardChosenEventArgs : EventArgs
	{
		private int m_questGiverID;
		private int m_questID;
		private int m_countChosen;
		private int[] m_itemsChosen;

		/// <summary>
		/// Constructs arguments for a QuestRewardChosen event.
		/// </summary>
		public QuestRewardChosenEventArgs(int questGiverID, int questID, int countChosen, int[] itemsChosen)
		{
			m_questGiverID = questGiverID;
			m_questID = questID;
			m_countChosen = countChosen;
			m_itemsChosen = itemsChosen;
		}

		/// <summary>
		/// ID of the NPC that gave the quest.
		/// </summary>
		public int QuestGiverID
		{
			get { return m_questGiverID; }
		}

		/// <summary>
		/// ID of the quest.
		/// </summary>
		public int QuestID
		{
			get { return m_questID; }
		}

		/// <summary>
		/// Number of rewards picked from the quest window.
		/// </summary>
		public int CountChosen
		{
			get { return m_countChosen; }
		}

		/// <summary>
		/// List of items (0-7) that were picked.
		/// </summary>
		public int[] ItemsChosen
		{
			get { return m_itemsChosen; }
		}
	}
}
