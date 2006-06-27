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
	/// Holds the arguments for the TakeDamage event of GameObjects
	/// </summary>
	public class TakeDamageEventArgs : EventArgs
	{
		private GameLiving m_damageSource;
		private eDamageType m_damageType;
		private int m_damageAmount;
		private int m_criticalAmount;

		/// <summary>
		/// Constructs new TakeDamageEventArgs
		/// </summary>
		/// <param name="damageSource">The damage source</param>
		/// <param name="damageType">The damage type</param>
		/// <param name="damageAmount">The damage amount</param>
		/// <param name="criticalAmount">The critical damage amount</param>
		public TakeDamageEventArgs(GameLiving damageSource, eDamageType damageType, int damageAmount, int criticalAmount)
		{
			m_damageSource = damageSource;
			m_damageType = damageType;
			m_damageAmount = damageAmount;
			m_criticalAmount = criticalAmount;
		}

		/// <summary>
		/// Gets the damage source
		/// </summary>
		public GameLiving DamageSource
		{
			get { return m_damageSource; }
		}

		/// <summary>
		/// Gets the damage type
		/// </summary>
		public eDamageType DamageType
		{
			get { return m_damageType; }
		}

		/// <summary>
		/// Gets the damage amount
		/// </summary>
		public int DamageAmount
		{
			get { return m_damageAmount; }
		}

		/// <summary>
		/// Gets the critical damage amount
		/// </summary>
		public int CriticalAmount
		{
			get { return m_criticalAmount; }
		}
	}
}
