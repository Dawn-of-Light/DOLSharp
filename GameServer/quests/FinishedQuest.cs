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

namespace DOL.GS.Quests
{
	/// <summary>
	/// Description résumée de FinishedQuest.
	/// </summary>
	public class FinishedQuest
	{
		#region Declaration
		/// <summary>
		/// The unique id of this quest finished
		/// </summary>
		protected Type m_finishedQuestType;

		/// <summary>
		/// How much time the player has finished the quest
		/// </summary>
		protected byte m_count;

		/// <summary>
		/// Gets or Sets the type of this quest finished
		/// </summary>
		public Type FinishedQuestType
		{
			get { return m_finishedQuestType; }
			set { m_finishedQuestType = value; }
		}

		/// <summary>
		/// Gets or sets the player doing the quest
		/// </summary>
		public byte Count
		{
			get	{ return m_count; }
			set	{ m_count = value; }
		}
		#endregion
	}
}
