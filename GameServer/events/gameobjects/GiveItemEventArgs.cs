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
using DOL.Database;
using DOL.GS;

namespace DOL.Events
{
	/// <summary>
	/// Holds the arguments for the GiveItem event of GamePlayer
	/// </summary>
	public class GiveItemEventArgs : EventArgs
	{

		private GamePlayer source;
		private GameObject target;
		private InventoryItem item;

		/// <summary>
		/// Constructs a new SayReceiveEventArgs
		/// </summary>
		/// <param name="source">the source that is saying something</param>
		/// <param name="target">the target that listened to the say</param>
		/// <param name="item">the item being given</param>
		public GiveItemEventArgs(GamePlayer source, GameObject target, InventoryItem item)
		{
			this.source = source;
			this.target = target;
			this.item = item;
		}

		/// <summary>
		/// Gets the GamePlayer source who was saying something
		/// </summary>
		public GamePlayer Source
		{
			get { return source; }
		}
		
		/// <summary>
		/// Gets the GameLiving target who listened to the say
		/// </summary>
		public GameObject Target
		{
			get { return target; }
		}

		/// <summary>
		/// Gets the item being moved
		/// </summary>
		public InventoryItem Item
		{
			get { return item; }
		}
	}
}
