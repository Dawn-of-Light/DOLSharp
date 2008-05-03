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
	/// Holds the arguments for the ReceiveMoney event of GameLivings
	/// </summary>
	public class ReceiveMoneyEventArgs : EventArgs
	{
		private GameLiving source;
		private GameLiving target;
		private long copperValue;

		/// <summary>
		/// Constructs new ReceiveMoneyEventArgs
		/// </summary>
		/// <param name="source">the source of the money</param>
		/// <param name="target">the target of the money</param>
		/// <param name="copperValue">the money value</param>
		public ReceiveMoneyEventArgs(GameLiving source, GameLiving target, long copperValue)
		{
			this.source = source;
			this.target = target;
			this.copperValue = copperValue;
		}

		/// <summary>
		/// Gets the GameLiving who spent the money
		/// </summary>
		public GameLiving Source
		{
			get { return source; }
		}

		/// <summary>
		/// Gets the GameLivng who receives the money
		/// </summary>
		public GameLiving Target
		{
			get { return target; }
		}

		/// <summary>
		/// Gets the value of the money
		/// </summary>
		public long CopperValue
		{
			get { return copperValue; }
		}
	}
}
