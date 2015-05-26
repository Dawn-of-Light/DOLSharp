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

using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Static Extension Class for Spell Messages System.
	/// </summary>
	public static class SpellMessagesHelper
	{
		/// <summary>
		/// Sends a message to the caster, if the caster is a controlled
		/// creature, to the player instead (only spell hit and resisted
		/// messages).
		/// </summary>
		/// <param name="handler"></param>
		/// <param name="type"></param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void MessageToCaster(this SpellHandler handler, eChatType type, string format, params object[] args)
		{
			handler.MessageToCaster(string.Format(format, args), type);
		}
		
		/// <summary>
		/// Sends a message to a living
		/// </summary>
		/// <param name="handler"></param>
		/// <param name="living"></param>
		/// <param name="type"></param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void MessageToLiving(this SpellHandler handler, GameLiving living, eChatType type, string format, params object[] args)
		{
			handler.MessageToLiving(living, string.Format(format, args), type);
		}

	}
}
