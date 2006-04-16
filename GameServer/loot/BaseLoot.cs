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
using System.Collections;
using System.Text;
using DOL.GS.Database;

namespace DOL.GS.Loot
{
	/// <summary>
	/// The base class for all different loot type
	/// </summary>
	public abstract class BaseLoot : ILoot
	{	
		#region Declaration

		/// <summary>
		/// The unique db id of this loot
		/// </summary>	
		protected int m_id;	
	
		/// <summary>
		/// Returns the unique ID of this loot
		/// </summary>
		public int LootID
		{
			get { return m_id; }
			set { m_id = value; }
		}

		/// <summary>
		/// The chance to see this loot on the ground (0 - 1000)
		/// </summary>	
		protected int m_chance;	
	
		/// <summary>
		/// Get / set the chances to see this loot on the ground (0 - 1000)
		/// </summary>
		public int Chance
		{
			get { return m_chance; }
			set { m_chance = value; }
		}
		#endregion

		/// <summary>
		/// This method is used to return the object to add to the world base on this loot
		/// </summary>	
		public abstract GameObjectTimed GetLoot(GameMob killedMob, GameLiving killer);

		/// <summary>
		/// Returns the string representation of the object
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return new StringBuilder(128)
				.Append(GetType().FullName)
				.Append(" LootID=").Append(LootID)
				.Append(" chance=").Append(Chance)
				.ToString();
		}
	}
}
