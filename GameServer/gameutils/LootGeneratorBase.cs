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
using DOL.Database;

namespace DOL.GS
{
	/// <summary>
	/// Base for all loot generators
	/// </summary>
	public class LootGeneratorBase : ILootGenerator
	{
		protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		protected int m_exclusivePriority = 0;

		public LootGeneratorBase()
		{
		}

		public int ExclusivePriority
		{
			get{ return m_exclusivePriority; }
			set{ m_exclusivePriority = value; }
		}

		public virtual void Refresh(GameNPC mob)
		{
		}

		/// <summary>
		/// Generate loot for given mob
		/// </summary>
		/// <param name="mob"></param>
		/// <param name="killer"></param>
		/// <returns></returns>
		public virtual LootList GenerateLoot(GameNPC mob, GameObject killer)
		{
			LootList loot = new LootList();
			return loot;
		}
	}
}
