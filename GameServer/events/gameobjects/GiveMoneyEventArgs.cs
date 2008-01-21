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
using DOL.Database2;
using DOL.GS;

namespace DOL.Events
{
	/// <summary>
	/// Holds the arguments for the GiveMoney event of GamePlayer
	/// </summary>
	public class GiveMoneyEventArgs : EventArgs
	{

		private GamePlayer m_source;
		private GameObject m_target;
		private long m_money;

		/// <summary>
		/// Constructs a new GiveMoneyEventArgs
		/// </summary>
		/// <param name="source">the source that is saying something</param>
		/// <param name="target">the target that listened to the say</param>
		/// <param name="money">amount of money being given</param>
		public GiveMoneyEventArgs(GamePlayer source, GameObject target, long money)
		{
			m_source = source;
			m_target = target;
			m_money = money;
		}

		/// <summary>
		/// Gets the GamePlayer source
		/// </summary>
		public GamePlayer Source
		{
			get { return m_source; }
		}
		
		/// <summary>
		/// Gets the GameLiving target
		/// </summary>
		public GameObject Target
		{
			get { return m_target; }
		}

		/// <summary>
		/// Gets the amount of money being moved
		/// </summary>
		public long Money
		{
			get { return m_money; }
		}
	}
}
