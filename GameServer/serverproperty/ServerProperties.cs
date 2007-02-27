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
using System.Globalization;
using System;
using System.Reflection;
using DOL.Database;
using DOL.GS.Scripts;
using log4net;

namespace DOL.GS.ServerProperties
{
	/// <summary>
	/// The abstract ServerProperty class that also defines the
	/// static Init and Load methods for other properties that inherit
	/// </summary>
	public abstract class Properties
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Init the property
		/// </summary>
		static Properties()
		{
			Init(typeof(Properties));
		}

		/// <summary>
		/// The Experience Rate
		/// </summary>
		[ServerProperty("xp_rate", "The Experience Points Rate Modifier - Edit this to change the rate at which you gain experience points e.g 1.5 is 50% more 2.0 is twice the amount (100%) 0.5 is half the amount (50%)", 1.0)]
		public static readonly double XP_RATE;

		/// <summary>
		/// The Realm Points Rate
		/// </summary>
		[ServerProperty("rp_rate", "The Realm Points Rate Modifier - Edit this to change the rate at which you gain realm points e.g 1.5 is 50% more 2.0 is twice the amount (100%) 0.5 is half the amount (50%)", 1.0)]
		public static readonly double RP_RATE;

		/// <summary>
		/// The Bounty Points Rate
		/// </summary>
		[ServerProperty("bp_rate", "The Bounty Points Rate Modifier - Edit this to change the rate at which you gain bounty points e.g 1.5 is 50% more 2.0 is twice the amount (100%) 0.5 is half the amount (50%)", 1.0)]
		public static readonly double BP_RATE;

		/// <summary>
		/// The Server Message of the Day
		/// </summary>
		[ServerProperty("motd", "The Server Message of the Day - Edit this to set what is displayed when a level 2+ character enters the game for the first time, set to \"\" for nothing", "Welcome to a Dawn of Light server, please edit this MOTD")]
		public static readonly string MOTD;

		/// <summary>
		/// The damage players do against monsters
		/// </summary>
		[ServerProperty("pve_damage", "The PvE Damage Modifier - Edit this to change the amount of damage done when fighting mobs e.g 1.5 is 50% more damage 2.0 is twice the damage (100%) 0.5 is half the damage (50%)", 1.0)]
		public static readonly double PVE_DAMAGE;

		/// <summary>
		/// The damage players do against players
		/// </summary>
		[ServerProperty("pvp_damage", "The PvP Damage Modifier - Edit this to change the amountof damage done when fighting players e.g 1.5 is 50% more damage 2.0 is twice the damage (100%) 0.5 is half the damage (50%)", 1.0)]
		public static readonly double PVP_DAMAGE;

		/// <summary>
		/// The message players get when they enter the game past level 1
		/// </summary>
		[ServerProperty("starting_msg", "The Starting Mesage - Edit this to set what is displayed when a level 1 character enters the game for the first time, set to \"\" for nothing", "Welcome for your first time to a Dawn of Light server, please edit this Starter Message")]
		public static readonly string STARTING_MSG;

		/// <summary>
		/// The amount of copper a player starts with
		/// </summary>
		[ServerProperty("starting_money", "Starting Money - Edit this to change the amount in copper of money new characters start the game with, max 214 plat", 0)]
		public static readonly long STARTING_MONEY;

		/// <summary>
		/// The level of experience a player should start with
		/// </summary>
		[ServerProperty("starting_level", "Starting Level - Edit this to set which levels experience a new player start the game with", 1)]
		public static readonly int STARTING_LEVEL;

		/// <summary>
		/// The message players get when they enter the game at level 1
		/// </summary>
		[ServerProperty("starting_realm_level", "Starting Realm level - Edit this to set which realm level a new player starts the game with", 0)]
		public static readonly int STARTING_REALM_LEVEL;

		/// <summary>
		/// The a starting guild should be used
		/// </summary>
		[ServerProperty("starting_guild", "Starter Guild - Edit this to change the starter guild options, values True,False", true)]
		public static readonly bool STARTING_GUILD;

		/// <summary>
		/// The crafting speed modifier
		/// </summary>
		[ServerProperty("crafting_speed", "Crafting Speed Modifier - Edit this to change the speed at which you craft e.g 1.5 is 50% faster 2.0 is twice as fast (100%) 0.5 is half the speed (50%)", 1.0)]
		public static readonly double CRAFTING_SPEED;

		/// <summary>
		/// The money drop modifier
		/// </summary>
		[ServerProperty("money_drop", "Money Drop Modifier - Edit this to change the amount of money which is dropped e.g 1.5 is 50% more 2.0 is twice the amount (100%) 0.5 is half the amount (50%)", 1.0)]
		public static readonly double MONEY_DROP;

		/// <summary>
		/// The broadcast type
		/// </summary>
		[ServerProperty("broadcast_type", "Broadcast Type - Edit this to change what /b does, values 0 = disabled, 1 = area, 2 = visibility distance, 3 = zone, 4 = region, 5 = realm, 6 = server", 1)]
		public static readonly int BROADCAST_TYPE;

		/// <summary>
		/// The max number of guilds in an alliance
		/// </summary>
		[ServerProperty("alliance_max", "Max Guilds In Alliance - Edit this to change the maximum number of guilds in an alliance -1 = unlimited, 0=disable alliances", -1)]
		public static readonly int ALLIANCE_MAX;

		/// <summary>
		/// The number of players needed for claiming
		/// </summary>
		[ServerProperty("claim_num", "Players Needed For Claim - Edit this to change the amount of players required to claim a keep, towers are half this amount", 8)]
		public static readonly int CLAIM_NUM;

		/// <summary>
		/// The number of players needed to form a guild
		/// </summary>
		[ServerProperty("guild_num", "Players Needed For Guild Form - Edit this to change the amount of players required to form a guild", 8)]
		public static readonly int GUILD_NUM;

		/// <summary>
		/// If the server should only accept connections from staff
		/// </summary>
		[ServerProperty("staff_login", "Staff Login Only - Edit this to set weather you wish staff to be the only ones allowed to log in values True,False", false)]
		public static readonly bool STAFF_LOGIN;

		/// <summary>
		/// The max number of players on the server
		/// </summary>
		[ServerProperty("max_players", "Max Players - Edit this to set the maximum players allowed to connect at the same time set 0 for unlimited", 0)]
		public static readonly int MAX_PLAYERS;

		/// <summary>
		/// The time until a player is worth rps again after death
		/// </summary>
		[ServerProperty("rp_worth_seconds", "Realm Points Worth Seconds - Edit this to change how many seconds until a player is worth RPs again after being killed ", 300)]
		public static readonly int RP_WORTH_SECONDS;

		/// <summary>
		/// The minimum client version required to connect
		/// </summary>
		[ServerProperty("client_version_min", "Minimum Client Version - Edit this to change which client version at the least have to be used: -1 = any, 1.80 = 180", -1)]
		public static readonly int CLIENT_VERSION_MIN;

		/// <summary>
		/// The maximum client version required to connect
		/// </summary>
		[ServerProperty("client_version_max", "Maximum Client Version - Edit this to change which client version at the most have to be used: -1 = any, 1.80 = 180", -1)]
		public static readonly int CLIENT_VERSION_MAX;

		/// <summary>
		/// Should the server load quests
		/// </summary>
		[ServerProperty("load_quests", "Should the server load quests, values True,False", true)]
		public static readonly bool LOAD_QUESTS;

		/// <summary>
		/// Should the server log trades
		/// </summary>
		[ServerProperty("log_trades", "Should the server log all trades a player makes, values True,False", false)]
		public static readonly bool LOG_TRADES;

		/// <summary>
		/// What class should the server use for players
		/// </summary>
		[ServerProperty("player_class", "What class should the server use for players", "DOL.GS.GamePlayer")]
		public static readonly string PLAYER_CLASS;

		/// <summary>
		/// What is the maximum client type allowed to connect
		/// </summary>
		[ServerProperty("client_type_max", "What is the maximum client type allowed to connect", -1)]
		public static readonly int CLIENT_TYPE_MAX;

		/// <summary>
		/// Disable minotaurs from being created
		/// </summary>
		[ServerProperty("disable_minotaurs", "Disable minotaurs from being created", false)]
		public static readonly bool DISABLE_MINOTAURS;

		/// <summary>
		/// Should the server load the example scripts
		/// </summary>
		[ServerProperty("load_examples", "should the server load the example scripts", true)]
		public static readonly bool LOAD_EXAMPLES;
		
		/// <summary>
		/// A serialised list of disabled RegionIDs
		/// </summary>
		[ServerProperty("disabled_regions", "a serialised list of disabled region IDs seperated by ;", "")]
		public static readonly string DISABLED_REGIONS;

		/// <summary>
		/// A serialised list of disabled expansion IDs
		/// </summary>
		[ServerProperty("disabled_expansions", "a serialised list of disabled expansions IDs, expansion IDs are client type seperated by ;", "")]
		public static readonly string DISABLED_EXPANSIONS;

		/// <summary>
		/// Should the server disable the tutorial zone
		/// </summary>
		[ServerProperty("disable_tutorial", "should the server disable the tutorial zone", false)]
		public static readonly bool DISABLE_TUTORIAL;

		/// <summary>
		/// Should users be able to create characters in all realms using the same account
		/// </summary>
		[ServerProperty("allow_all_realms", "should we allow characters to be created on all realms using a single account", false)]
		public static readonly bool ALLOW_ALL_REALMS;

		/// <summary>
		/// Should users be allowed to create catacombs classes
		/// </summary>
		[ServerProperty("disable_catacombs_classes", "should we disable catacombs classes", false)]
		public static readonly bool DISABLE_CATACOMBS_CLASSES;

		/// <summary>
		/// Days before your elligable for a free level
		/// </summary>
		[ServerProperty("freelevel_days", "days before your elligable for a free level, use -1 to deactivate", 7)]
		public static readonly int FREELEVEL_DAYS;

		/// <summary>
		/// Server Language
		/// </summary>
		[ServerProperty("server_language", "Language of your server. It can be EN, FR or DE.", "EN")]
		public static readonly string SERV_LANGUAGE;

		/// <summary>
		/// allow_change_language
		/// </summary>
		[ServerProperty("allow_change_language", "Should we allow clients to change their language ?", false)]
		public static readonly bool ALLOW_CHANGE_LANGUAGE;

		/// <summary>
		/// StatSave Interval
		/// </summary>
		[ServerProperty("statsave_interval", "Interval between each DB Stats store in minutes. -1 for deactivated.", -1)]
		public static readonly int STATSAVE_INTERVAL;

		/// <summary>
		/// Anon Modifier
		/// </summary>
		[ServerProperty("anon_modifier", "Various modifying options for anon, 0 = default, 1 = /who shows player but as ANON, -1 = disabled", 0)]
		public static readonly int ANON_MODIFIER;

		/// <summary>
		/// Buff Range
		/// </summary>
		[ServerProperty("buff_range", "The range that concentration buffs can last from the owner before it expires", 0)]
		public static readonly int BUFF_RANGE;

		/// <summary>
		/// Disable Bug Reports
		/// </summary>
		[ServerProperty("disable_bug_reports", "Set to false to disable bug reporting, and true to enable bug reporting", false)]
		public static readonly bool DISABLE_BUG_REPORTS;
		
		/// <summary>
		/// This method loads the property from the database and returns
		/// the value of the property as strongly typed object based on the
		/// type of the default value
		/// </summary>
		/// <param name="attrib">The attribute</param>
		/// <returns>The real property value</returns>
		public static object Load(ServerPropertyAttribute attrib)
		{
			string key = attrib.Key;
			ServerProperty property = GameServer.Database.SelectObject(typeof(ServerProperty), "`Key` = '" + GameServer.Database.Escape(key) + "'") as ServerProperty;
			if (property == null)
			{
				property = new ServerProperty();
				property.Key = attrib.Key;
				property.Description = attrib.Description;
				property.DefaultValue = attrib.DefaultValue.ToString();
				property.Value = attrib.DefaultValue.ToString();
				GameServer.Database.AddNewObject(property);
				log.Debug("Cannot find server property " + key + " creating it");
			}
			log.Debug("Loading " + key + " Value is " + property.Value);
			try
			{
				//we do this because we need "1.0" to be considered double sometimes its "1,0" in other countries
				CultureInfo myCIintl = new CultureInfo("en-US", false);
				IFormatProvider provider = myCIintl.NumberFormat;
				return Convert.ChangeType(property.Value, attrib.DefaultValue.GetType(), provider);
			}
			catch(Exception e)
			{
				log.Error("Exception in ServerProperties Load: ", e);
				return null;
			}
		}

		/// <summary>
		/// This method is the key. It checks all fields of a specific type and
		/// if the field is a ServerProperty it loads the value from the database.
		/// </summary>
		/// <param name="type">The type to analyze</param>
		protected static void Init(Type type)
		{
			foreach (FieldInfo f in type.GetFields())
			{
				if (!f.IsStatic)
					continue;
				object[] attribs = f.GetCustomAttributes(typeof(ServerPropertyAttribute), false);
				if (attribs.Length == 0)
					continue;
				ServerPropertyAttribute attrib = (ServerPropertyAttribute)attribs[0];
				f.SetValue(null, Properties.Load(attrib));
			}
		}
	}
}
