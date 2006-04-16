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
using DOL.GS.Database;
using DOL.GS;

namespace DOL.GS.Loot
{
	/// <summary>
	/// List containing all loot drop for a given id
	/// </summary>
	public class LootList
	{
		/// <summary>
		/// The unique ID of the loot list
		/// </summary>
		protected int m_id;	
	
		/// <summary>
		/// Returns the unique ID of this loot list
		/// </summary>
		public int LootListID
		{
			get { return m_id; }
			set { m_id = value; }
		}

		/// <summary>
		/// Holds all the loots currently in this list
		/// </summary>
		protected Iesi.Collections.ISet m_allLoots;

		/// <summary>
		/// Gets or sets all the loots currently in this list
		/// </summary>
		public Iesi.Collections.ISet AllLoots
		{
			get
			{
				if(m_allLoots == null) m_allLoots = new Iesi.Collections.HybridSet();
				return m_allLoots;
			}
			set	{ m_allLoots = value; }
		}
	}
}