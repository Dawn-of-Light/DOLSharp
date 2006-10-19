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

namespace DOL.GS.ServerProperties
{
	/// <summary>
	/// Class to access the keys for server properties
	/// </summary>
	public class ServerPropertyConstants
	{
		/// <summary>
		/// The key for Experience Rate
		/// </summary>
		public const string XP_RATE = "xp_rate";
		/// <summary>
		/// The key for the Realm Points Rate
		/// </summary>
		public const string RP_RATE = "rp_rate";
		/// <summary>
		/// The key for the Server Message of the Day
		/// </summary>
		public const string MOTD = "motd";
		/// <summary>
		/// The key for the damage players do against monsters
		/// </summary>
		public const string PVE_DAMAGE = "pve_damage";
		/// <summary>
		/// The key for the damage players do against players
		/// </summary>
		public const string PVP_DAMAGE = "pvp_damage";
		/// <summary>
		/// The key for the message players get when they enter the game past level 1
		/// </summary>
		public const string STARTING_MSG = "starting_msg";
		/// <summary>
		/// The key for the amount of copper a player starts with
		/// </summary>
		public const string STARTING_MONEY = "starting_money";
		/// <summary>
		/// The key for the level of experience a player should start with
		/// </summary>
		public const string STARTING_LEVEL = "starting_lvl";
		/// <summary>
		/// The key for the message players get when they enter the game at level 1
		/// </summary>
		public const string STARTING_REALM_LEVEL = "starting_realm_lvl";
		/// <summary>
		/// The key for if a starting guild should be used
		/// </summary>
		public const string STARTING_GUILD = "starting_guild";
		/// <summary>
		/// The key for the crafting speed modifier
		/// </summary>
		public const string CRAFTING_SPEED = "crafting_speed";
		/// <summary>
		/// The key for the money drop modifier
		/// </summary>
		public const string MONEY_DROP = "money_drop";
		/// <summary>
		/// The key for the broadcast type
		/// </summary>
		public const string BROADCAST_TYPE = "broadcast_type";
		/// <summary>
		/// The key for the max number of guilds in an alliance
		/// </summary>
		public const string ALLIANCE_MAX = "alliance_max";
		/// <summary>
		/// The key for the number of players needed for claiming
		/// </summary>
		public const string CLAIM_NUM = "claim_num";
		/// <summary>
		/// The key for the number of players needed to form a guild
		/// </summary>
		public const string GUILD_NUM = "guild_num";
		/// <summary>
		/// The key for if the server should only accept connections from staff
		/// </summary>
		public const string STAFF_LOGIN = "staff_login";
		/// <summary>
		/// The key for the max number of players on the server
		/// </summary>
		public const string MAX_PLAYERS = "max_players";
		/// <summary>
		/// The key for the time until a player is worth rps again after death
		/// </summary>
		public const string RP_WORTH_SECONDS = "rp_worth_seconds";
		/// <summary>
		/// The key for listing the minimum client version required to connect
		/// </summary>
		public const string CLIENT_VERSION_MIN = "client_version_min";
		/// <summary>
		/// The key for listing the maximum client version required to connect
		/// </summary>
		public const string CLIENT_VERSION_MAX = "client_version_max";
		/// <summary>
		/// The key for whether the server should load quests
		/// </summary>
		public const string LOAD_QUESTS = "load_quests";
	}
}
