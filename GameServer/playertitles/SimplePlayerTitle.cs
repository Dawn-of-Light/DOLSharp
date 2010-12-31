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

namespace DOL.GS.PlayerTitles
{
	/// <summary>
	/// Base abstract class for simple player titles.
	/// </summary>
	public abstract class SimplePlayerTitle : IPlayerTitle
	{
		/// <summary>
		/// The title description, shown in "Titles" window.
		/// </summary>
		/// <param name="player">The title owner.</param>
		/// <returns>The title description.</returns>
		public abstract string GetDescription(GamePlayer player);

		/// <summary>
		/// The title value, shown over player's head.
		/// </summary>
		/// <param name="player">The title owner.</param>
		/// <returns>The title value.</returns>
		public abstract string GetValue(GamePlayer player);

		/// <summary>
		/// Checks whether this title can be changed by the player.
		/// </summary>
		/// <param name="player">The title owner.</param>
		/// <returns>True if player can not change the title.</returns>
		public virtual bool IsForced(GamePlayer player)
		{
			return false;
		}

		/// <summary>
		/// Verify whether the player is suitable for this title.
		/// </summary>
		/// <param name="player">The player to check.</param>
		/// <returns>true if the player is suitable for this title.</returns>
		public abstract bool IsSuitable(GamePlayer player);

		/// <summary>
		/// Callback for when player gains this title.
		/// </summary>
		/// <param name="player">The player that gained a title.</param>
		public virtual void OnTitleGained(GamePlayer player)
		{
		}

		/// <summary>
		/// Callback for when player loose this title.
		/// </summary>
		/// <param name="player">The player that lost a title.</param>
		public virtual void OnTitleLost(GamePlayer player)
		{
		}
	}
}
