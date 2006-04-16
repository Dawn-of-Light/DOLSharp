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
	/// This class holds all possible script events.
	/// Only constants defined here!
	/// </summary>
	public class ScriptEvent : DOLEvent
	{
		/// <summary>
		/// Constructs a new ScriptEvent
		/// </summary>
		/// <param name="name">the event name</param>
		protected ScriptEvent(string name) : base(name)
		{
		}

		/// <summary>
		/// The Loaded event is fired whenever the scripts have loaded
		/// </summary>
		public static readonly ScriptEvent Loaded = new ScriptEvent("Script.Loaded");
		/// <summary>
		/// The Unloaded event is fired whenever the scripts should unload
		/// </summary>
		public static readonly ScriptEvent Unloaded = new ScriptEvent("Script.Unloaded");
	}
}
