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

namespace DOL.GS
{
	/// <summary>
	/// Marks a class as a guild wide npc script
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class NPCGuildScriptAttribute : Attribute
	{
		string m_guild;
		eRealm m_realm;

		/// <summary>
		/// constructs new attribute
		/// </summary>
		/// <param name="guildname">name of the npc guild to that the script has to be applied</param>
		/// <param name="realm">valid realm for the script</param>
		public NPCGuildScriptAttribute(string guildname, eRealm realm)
		{
			m_guild = guildname;
			m_realm = realm;
		}

		/// <summary>
		/// constructs new attribute
		/// </summary>
		/// <param name="guildname">name of the npc guild to that the script has to be applied</param>
		public NPCGuildScriptAttribute(string guildname)
		{
			m_guild = guildname;
			m_realm = eRealm.None;
		}

		/// <summary>
		/// npc guild
		/// </summary>
		public string GuildName {
			get { return m_guild; }
		}

		/// <summary>
		/// valid realm for this script
		/// </summary>
		public eRealm Realm {
			get { return m_realm; }
		}
	}
}
