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
	/// Holds the arguments for the PlayerPromoted event of GameMerchants
	/// </summary>
	public class PlayerPromotedEventArgs : EventArgs
	{
		private GamePlayer player;
		private ICharacterClass oldClass;

		/// <summary>
		/// Constructs a new PlayerPromoted event argument class
		/// </summary>
		/// <param name="player">the player that was promoted</param>
		/// <param name="oldClass">the player old class</param>
		public PlayerPromotedEventArgs(GamePlayer player, ICharacterClass oldClass)
		{
			this.player = player;
			this.oldClass = oldClass;
		}

		/// <summary>
		/// Gets the player that was promoted
		/// </summary>
		public GamePlayer Player
		{
			get { return player; }
		}

		/// <summary>
		/// Gets the class player was using before promotion
		/// </summary>
		public ICharacterClass OldClass
		{
			get { return oldClass; }
		}
	}
}
