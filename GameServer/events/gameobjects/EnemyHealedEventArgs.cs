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
	/// Holds the arguments for the EnemyHealed event of GameLivings
	/// </summary>
	public class EnemyHealedEventArgs : EventArgs
	{
		private readonly GameLiving m_enemy;
		private readonly GameObject m_healSource;
		private readonly GameLiving.eHealthChangeType m_changeType;
		private readonly int m_healAmount;

		/// <summary>
		/// Constructs new EnemyHealedEventArgs
		/// </summary>
		/// <param name="enemy">The healed enemy</param>
		/// <param name="healSource">The heal source</param>
		/// <param name="changeType">The health change type</param>
		/// <param name="healAmount">The heal amount</param>
		public EnemyHealedEventArgs(GameLiving enemy, GameObject healSource, GameLiving.eHealthChangeType changeType, int healAmount)
		{
			m_enemy = enemy;
			m_healSource = healSource;
			m_changeType = changeType;
			m_healAmount = healAmount;
		}

		/// <summary>
		/// Gets the healed enemy
		/// </summary>
		public GameLiving Enemy
		{
			get { return m_enemy; }
		}

		/// <summary>
		/// Gets the heal source
		/// </summary>
		public GameObject HealSource
		{
			get { return m_healSource; }
		}

		/// <summary>
		/// Gets the health change type
		/// </summary>
		public GameLiving.eHealthChangeType ChangeType
		{
			get { return m_changeType; }
		}

		/// <summary>
		/// Gets the heal amount
		/// </summary>
		public int HealAmount
		{
			get { return m_healAmount; }
		}
	}
}
