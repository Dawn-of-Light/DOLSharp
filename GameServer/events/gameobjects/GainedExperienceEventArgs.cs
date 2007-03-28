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
using DOL.GS;

namespace DOL.Events
{
	/// <summary>
	/// Holds the arguments for the GainedExperience event of GameLivings
	/// </summary>
	public class GainedExperienceEventArgs : EventArgs
	{
		private long m_expBase;
		private long m_expCampBonus;
		private long m_expGroupBonus;
		private long m_expOutpostBonus;
		private bool m_sendMessage;
		private bool m_allowMultiply;

		/// <summary>
		/// Constructs new ReceiveMoneyEventArgs
		/// </summary>
		/// <param name="expBase">the amount of base exp gained</param>
		/// <param name="expCampBonus">camp bonus to exp gained</param>
		/// <param name="expGroupBonus">group bonus to exp gained</param>
		/// <param name="sendMessage">send experience gained messages</param>
		public GainedExperienceEventArgs(long expBase, long expCampBonus, long expGroupBonus, long expOutpostBonus, bool sendMessage, bool allowMultiply)
		{
			m_expBase = expBase;
			m_expCampBonus = expCampBonus;
			m_expGroupBonus = expGroupBonus;
			m_expOutpostBonus = expOutpostBonus;
			m_sendMessage = sendMessage;
			m_allowMultiply = allowMultiply;
		}

		/// <summary>
		/// Gets the amount of base experience gained
		/// </summary>
		public long ExpBase
		{
			get { return m_expBase; }
		}

		/// <summary>
		/// Gets the amount of camp bonus experience gained
		/// </summary>
		public long ExpCampBonus
		{
			get { return m_expCampBonus; }
		}

		/// <summary>
		/// Gets the amount of group bonus experience gained
		/// </summary>
		public long ExpGroupBonus
		{
			get { return m_expGroupBonus; }
		}

		/// <summary>
		/// gets the amount of outpost bonus experience gained
		/// </summary>
		public long ExpOutpostBonus
		{
			get { return m_expOutpostBonus; }
		}

		/// <summary>
		/// True if experience gain message was sent
		/// </summary>
		public bool SendMessage
		{
			get { return m_sendMessage; }
		}

		/// <summary>
		/// is the experience allowed to be multiplied
		/// </summary>
		public bool AllowMultiply
		{
			get { return m_allowMultiply; }
		}
	}
}
