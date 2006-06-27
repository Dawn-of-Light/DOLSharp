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
using DOL.GS.Database;
using DOL.GS;

namespace DOL.Events
{
	/// <summary>
	/// Holds the arguments for the ReceiveItem event of GameLivings
	/// </summary>
	public class ReceiveItemEventArgs : EventArgs
	{
		private GameLiving source;
		private GameLiving target;
		private GenericItem item;

		/// <summary>
		/// Constructs new ReceiveItemEventArgs
		/// </summary>
		/// <param name="source">the source of the item</param>
		/// <param name="target">the target of the item</param>
		/// <param name="item">the item to transfer</param>
		public ReceiveItemEventArgs(GameLiving source, GameLiving target, GenericItem item)
		{
			this.source = source;
			this.target = target;
			this.item = item;
		}

		/// <summary>
		/// Gets the GameLiving who gives the item
		/// </summary>
		public GameLiving Source
		{
			get { return source; }
		}

		/// <summary>
		/// Gets the GameLiving who receives the item
		/// </summary>
		public GameLiving Target
		{
			get { return target; }
		}

		/// <summary>
		/// Gets the item to transfer
		/// </summary>
		public GenericItem Item
		{
			get { return item; }
		}
	}
}
