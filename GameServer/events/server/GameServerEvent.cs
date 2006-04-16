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
	/// This class holds all possible server events.
	/// Only constants defined here!
	/// </summary>
	public class GameServerEvent : DOLEvent
	{
		/// <summary>
		/// Constructs a new GameServerEvent
		/// </summary>
		/// <param name="name">the name of the event</param>
		protected GameServerEvent(string name) : base(name)
		{
		}
		/// <summary>
		/// The Started event is fired whenever the GameServer has finished startup
		/// </summary>
		public static readonly GameServerEvent Started = new GameServerEvent("Server.Started");
		/// <summary>
		/// The Stopped event is fired whenever the GameServer is stopping
		/// </summary>
		public static readonly GameServerEvent Stopped = new GameServerEvent("Server.Stopped");
	}
}
