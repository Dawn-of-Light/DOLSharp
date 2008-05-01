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
	/// This class holds all possible database events.
	/// Only constants defined here!
	/// </summary>
	public class DatabaseEvent : DOLEvent
	{
		/// <summary>
		/// Constructs a new DatabaseEvent
		/// </summary>
		/// <param name="name">the name of the event</param>
		protected DatabaseEvent(string name) : base(name)
		{
		}

		/// <summary>
		/// The AccountCreated event is fired whenever an account is created.
		/// <seealso cref="AccountEventArgs"/>
		/// </summary>
		public static readonly DatabaseEvent AccountCreated = new DatabaseEvent("Database.AccountCreated");
		/// <summary>
		/// The CharacterCreated event is fired whenever a new character is created
		/// <seealso cref="CharacterEventArgs"/>
		/// </summary>
		public static readonly DatabaseEvent CharacterCreated = new DatabaseEvent("Database.CharacterCreated");
		/// <summary>
		/// The CharacterDeleted event is fired whenever an account is deleted
		/// <seealso cref="CharacterEventArgs"/>
		/// </summary>
		public static readonly DatabaseEvent CharacterDeleted = new DatabaseEvent("Database.CharacterDeleted");
		/// <summary>
		/// The NewsCreated event is fired whenever news is created
		/// </summary>
		public static readonly DatabaseEvent NewsCreated = new DatabaseEvent("Database.NewsCreated");
	}
}
