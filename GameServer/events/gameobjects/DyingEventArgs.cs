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
using DOL.GS;
namespace DOL.Events
{
	/// <summary>
	/// Holds the arguments for the Dying event of GameLivings
	/// </summary>
	public class DyingEventArgs : EventArgs
	{

		/// <summary>
		/// The killer
		/// </summary>
		private GameObject m_killer;

		private List<GamePlayer> m_playerKillers = null;

		/// <summary>
		/// Constructs a new Dying event args
		/// </summary>
		public DyingEventArgs(GameObject killer)
		{
			m_killer=killer;
		}

		public DyingEventArgs(GameObject killer, List<GamePlayer> playerKillers)
		{
			m_killer = killer;
			m_playerKillers = playerKillers;
		}

		/// <summary>
		/// Gets the killer
		/// </summary>
		public GameObject Killer
		{
			get { return m_killer; }
		}

		public List<GamePlayer> PlayerKillers
		{
			get { return m_playerKillers; }
		}
	}
}