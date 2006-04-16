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
	/// Holds the arguments for the GainedRealmPoints event of GameLivings
	/// </summary>
	public class GainedRealmPointsEventArgs : EventArgs
	{
		private long m_realmPoints;

		/// <summary>
		/// Constructs new GainedRealmPointsEventArgs
		/// </summary>
		/// <param name="realmPoints">the amount of realm points gained</param>
		public GainedRealmPointsEventArgs(long realmPoints)
		{
			m_realmPoints = realmPoints;
		}

		/// <summary>
		/// Gets the amount of realm points gained
		/// </summary>
		public long RealmPoints
		{
			get { return m_realmPoints; }
		}
	}
}
