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

namespace DOL.GS
{
	/// <summary>
	/// Description résumée de GameMoney.
	/// </summary>
	public class GameMoney : GameObjectTimed
	{
		#region Value

		/// <summary>
		/// The value of copper in this coinbag
		/// </summary>
		protected long m_copperValue;

		/// <summary>
		/// Gets/Sets the total copper value of this bag
		/// </summary>
		public long TotalCopper
		{
			get { return m_copperValue; }
			set { m_copperValue = value; }
		}
		#endregion

		#region RemoveDelay

		/// <summary>
		/// Gets the delay in gameticks after which this object is removed
		/// </summary>
		public override int RemoveDelay
		{
			get { return 120000; }
		}

		#endregion
	}
}
