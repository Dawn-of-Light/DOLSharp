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

namespace DOL.GS.PacketHandler
{
	/// <summary>
	/// Denotes a class as a packet lib.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple=true, Inherited=false)]
	public class PacketLibAttribute : Attribute
	{
		/// <summary>
		/// Stores version Id sent by the client.
		/// </summary>
		int m_rawVersion;
		/// <summary>
		/// PacketLib client version.
		/// </summary>
		GameClient.eClientVersion m_clientVersion;

		/// <summary>
		/// Constructs a new PacketLibAttribute.
		/// </summary>
		/// <param name="rawVersion">The version Id sent by the client.</param>
		/// <param name="clientVersion">PacketLib client version.</param>
		public PacketLibAttribute(int rawVersion, GameClient.eClientVersion clientVersion)
		{
			m_rawVersion = rawVersion;
			m_clientVersion = clientVersion;
		}

		/// <summary>
		/// Gets version Id sent by the client.
		/// </summary>
		public int RawVersion
		{
			get { return m_rawVersion; }
		}

		/// <summary>
		/// Gets the client version for which PacketLib is built.
		/// </summary>
		public GameClient.eClientVersion ClientVersion
		{
			get { return m_clientVersion; }
		}
	}
}
