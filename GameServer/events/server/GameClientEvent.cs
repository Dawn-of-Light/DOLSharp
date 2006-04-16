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
namespace DOL.Events
{
	/// <summary>
	/// This class holds all possible GameClient events.
	/// Only constants defined here!
	/// </summary>
	public class GameClientEvent : DOLEvent
	{
		/// <summary>
		/// Constructs a new GameClientEvent
		/// </summary>
		/// <param name="name">the name of the event</param>
		protected GameClientEvent(string name) : base(name)
		{
		}

		/// <summary>
		/// The Created event is fired whenever a GameClient is created
		/// </summary>
		public static readonly GameClientEvent Created = new GameClientEvent("GameClient.Created");
		/// <summary>
		/// The Connected event is fired whenever a GameClient has connected
		/// </summary>
		public static readonly GameClientEvent Connected = new GameClientEvent("GameClient.Connected");
		/// <summary>
		/// The Disconnected event is fired whenever a GameClient has disconnected
		/// </summary>
		public static readonly GameClientEvent Disconnected = new GameClientEvent("GameClient.Disconnected");
		/// <summary>
		/// The PlayerLoaded event is fired whenever a player is set for the GameClient
		/// </summary>
		public static readonly GameClientEvent PlayerLoaded = new GameClientEvent("GameClient.PlayerLoaded");
		/// <summary>
		/// The StateChanged event is fired whenever the GameClient's state changed
		/// </summary>
		public static readonly GameClientEvent StateChanged = new GameClientEvent("GameClient.StateChanged");
		/// <summary>
		/// The AccountLoaded event is fired whenever an account has been set for the GameClient
		/// </summary>
		public static readonly GameClientEvent AccountLoaded = new GameClientEvent("GameClient.AccountLoaded");
	}
}
