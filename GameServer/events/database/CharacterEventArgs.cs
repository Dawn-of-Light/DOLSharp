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
	/// Holds the arguments for the character events
	/// </summary>
	public class CharacterEventArgs : EventArgs
	{
		/// <summary>
		/// Holds the target character for this event
		/// </summary>
		private Character m_character;

		/// <summary>
		/// Holds the character's creation client for this event
		/// </summary>
		private GameClient m_client;
		
		/// <summary>
        /// Constructs a new event argument class for the
        /// character events 
		/// </summary>
		/// <param name="character"></param>
		/// <param name="client"></param>
		public CharacterEventArgs(Character character, GameClient client)
		{
			m_character = character;
			m_client = client;
		}

		/// <summary>
		/// Gets the character for this event
		/// </summary>
		public Character Character
		{
			get
			{
				return m_character;
			}
		}

		/// <summary>
		/// Gets the client for this event
		/// </summary>
		public GameClient GameClient
		{
			get
			{
				return m_client;
			}
		}
	}
}
