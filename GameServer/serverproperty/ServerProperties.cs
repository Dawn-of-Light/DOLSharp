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
using System.Collections;
using System;
using System.Reflection;
using DOL.Database;
using DOL.GS.Scripts;
using log4net;

namespace DOL.GS.ServerProperties
{
	/// <summary>
	/// Convience class to access server properties
	/// </summary>
	public class Properties
	{
		/// <summary>
		/// The Experience Rate
		/// </summary>
		public static double XP_RATE
		{
			get { return XPRateServerProperty.Value; }
		}

		/// <summary>
		/// The Realm Points Rate
		/// </summary>
		public static double RP_RATE
		{
			get { return RPRateServerProperty.Value; }
		}

		/// <summary>
		/// The Server Message of the Day
		/// </summary>
		public static string MOTD
		{
			get { return MOTDServerProperty.Value; }
		}

		/// <summary>
		/// The damage players do against monsters
		/// </summary>
		public static double PVE_DAMAGE
		{
			get { return PvEDamageServerProperty.Value; }
		}

		/// <summary>
		/// The damage players do against players
		/// </summary>
		public static double PVP_DAMAGE
		{
			get { return PvPDamageServerProperty.Value; }
		}

		/// <summary>
		/// The message players get when they enter the game past level 1
		/// </summary>
		public static string STARTING_MSG
		{
			get { return StartingMsgServerProperty.Value; }
		}

		/// <summary>
		/// The amount of copper a player starts with
		/// </summary>
		public static long STARTING_MONEY
		{
			get { return StartingMoneyServerProperty.Value; }
		}

		/// <summary>
		/// The level of experience a player should start with
		/// </summary>
		public static byte STARTING_LEVEL
		{
			get { return StartingLevelProperty.Value; }
		}

		/// <summary>
		/// The message players get when they enter the game at level 1
		/// </summary>
		public static byte STARTING_REALM_LEVEL
		{
			get { return StartingRealmLevelServerProperty.Value; }
		}

		/// <summary>
		/// The a starting guild should be used
		/// </summary>
		public static bool STARTING_GUILD
		{
			get { return StartingGuildServerProperty.Value; }
		}

		/// <summary>
		/// The crafting speed modifier
		/// </summary>
		public static double CRAFTING_SPEED
		{
			get { return CraftingSpeedServerProperty.Value; }
		}

		/// <summary>
		/// The money drop modifier
		/// </summary>
		public static double MONEY_DROP
		{
			get { return MoneyDropServerProperty.Value; }
		}

		/// <summary>
		/// The broadcast type
		/// </summary>
		public static byte BROADCAST_TYPE
		{
			get { return BroadcastTypeServerProperty.Value; }
		}

		/// <summary>
		/// The max number of guilds in an alliance
		/// </summary>
		public static int ALLIANCE_MAX
		{
			get { return AllianceMaxServerProperty.Value; }
		}

		/// <summary>
		/// The number of players needed for claiming
		/// </summary>
		public static byte CLAIM_NUM
		{
			get { return ClaimNumServerProperty.Value; }
		}

		/// <summary>
		/// The number of players needed to form a guild
		/// </summary>
		public static byte GUILD_NUM
		{
			get { return GuildNumServerProperty.Value; }
		}

		/// <summary>
		/// If the server should only accept connections from staff
		/// </summary>
		public static bool STAFF_LOGIN
		{
			get { return StaffLoginServerProperty.Value; }
		}

		/// <summary>
		/// The max number of players on the server
		/// </summary>
		public static int MAX_PLAYERS
		{
			get { return MaxPlayersServerProperty.Value; }
		}

		/// <summary>
		/// The time until a player is worth rps again after death
		/// </summary>
		public static int RP_WORTH_SECONDS
		{
			get { return RPWorthSecondsServerProperty.Value; }
		}
	}
}
