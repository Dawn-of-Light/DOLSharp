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


/// <summary>
///  Enum and constants related to DOLBase
/// </summary>

namespace DOL
{
	/// <summary>
	/// This class is handling DOLBase Contants
	/// </summary>
	public static class Constants
	{
		/// <summary>
		/// Send buffer size
		/// </summary>
		public const int SEND_BUFF_SIZE = 16 * 1024;
	}
	
	/// <summary>
	/// Status of the GameServer
	/// </summary>
	public enum eGameServerStatus
	{
		/// <summary>
		/// Server is open for connections
		/// </summary>
		GSS_Open = 0,
		/// <summary>
		/// Server is closed and won't accept connections
		/// </summary>
		GSS_Closed,
		/// <summary>
		/// Server is down
		/// </summary>
		GSS_Down,
		/// <summary>
		/// Server is full, no more connections accepted
		/// </summary>
		GSS_Full,
		/// <summary>
		/// Unknown server status
		/// </summary>
		GSS_Unknown,
		/// <summary>
		/// Server is banned for the user
		/// </summary>
		GSS_Banned,
		/// <summary>
		/// User is not invited
		/// </summary>
		GSS_NotInvited,
		/// <summary>
		/// The count of server stati
		/// </summary>
		_GSS_Count,
	}

	/// <summary>
	/// The different game server types
	/// </summary>
	public enum eGameServerType : int
	{
		/// <summary>
		/// Normal server
		/// </summary>
		GST_Normal = 0,
		/// <summary>
		/// Test server
		/// </summary>
		GST_Test = 1,
		/// <summary>
		/// Player vs Player
		/// </summary>
		GST_PvP = 2,
		/// <summary>
		/// Player vs Monsters
		/// </summary>
		GST_PvE = 3,
		/// <summary>
		/// Roleplaying server
		/// </summary>
		GST_Roleplay = 4,
		/// <summary>
		/// Casual server
		/// </summary>
		GST_Casual = 5,
		/// <summary>
		/// Unknown server type
		/// </summary>
		GST_Unknown = 6,
		/// <summary>
		/// The count of server types
		/// </summary>
		_GST_Count = 7,
	}
}
