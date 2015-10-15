
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
using System.Globalization;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using DOL.Database;
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
		/// Init the properties
		/// </summary>
		public static void InitProperties()
		{
			var propDict = AllDomainProperties;
			foreach (var prop in propDict)
			{
				Load(prop.Value.Item1, prop.Value.Item2, prop.Value.Item3);
			}
			
			// Refresh static dict values for display
			AllCurrentProperties = propDict.ToDictionary(k => k.Key, v => v.Value.Item2.GetValue(null));
		}

		// Properties: feel free to improve the SPs in the
		// categories below, or extend the category list.

		#region SYSTEM / DEBUG
		/// <summary>
		/// Enable Debug mode - used to alter some features during server startup to make debugging easier
		/// Can be changed while server is running but may require restart to enable all debug features
		/// </summary>
		[ServerProperty("system", "enable_debug", "Enable Debug mode? Used to alter some features during server startup to make debugging easier", false)]
		public static bool ENABLE_DEBUG;

		/// <summary>
		/// Whether to use the sync timer utility or not
		/// </summary>
		[ServerProperty("system", "use_sync_timer", "Shall we use the sync timers utility?", true)]
		public static bool USE_SYNC_UTILITY;

		/// <summary>
		/// Ignore too long outcoming packet or not
		/// </summary>
		[ServerProperty("system", "ignore_too_long_outcoming_packet", "Shall we ignore too long outcoming packet ?", false)]
		public static bool IGNORE_TOO_LONG_OUTCOMING_PACKET;

		/// <summary>
		/// Maximum length for reward quest description text to prevent client crashes
		/// </summary>
		[ServerProperty("system", "max_rewardquest_description_length", "Maximum length for reward quest description text to prevent client crashes.", 1000)]
		public static int MAX_REWARDQUEST_DESCRIPTION_LENGTH;

		/// <summary>
		/// If the server should only accept connections from staff
		/// </summary>
		[ServerProperty("system", "staff_login", "Staff Login Only - Edit this to set weather you wish staff to be the only ones allowed to Log in values True,False", false)]
		public static bool STAFF_LOGIN;

		/// <summary>
		/// The minimum client version required to connect
		/// </summary>
		[ServerProperty("system", "client_version_min", "Minimum Client Version - Edit this to change which client version at the least have to be used: -1 = any, 1.80 = 180", -1)]
		public static int CLIENT_VERSION_MIN;

		/// <summary>
		/// What is the maximum client type allowed to connect
		/// </summary>
		[ServerProperty("system", "client_type_max", "What is the maximum client type allowed to connect", -1)]
		public static int CLIENT_TYPE_MAX;

		/// <summary>
		/// The maximum client version required to connect
		/// </summary>
		[ServerProperty("system", "client_version_max", "Maximum Client Version - Edit this to change which client version at the most have to be used: -1 = any, 1.80 = 180", -1)]
		public static int CLIENT_VERSION_MAX;

		/// <summary>
		/// Should the server load quests
		/// </summary>
		[ServerProperty("system", "load_quests", "Should the server load quests, values True,False", true)]
		public static bool LOAD_QUESTS;

		/// <summary>
		/// Should the server load Buff Tokens
		/// </summary>
		[ServerProperty("system", "load_buff_tokens", "Should the server load buff tokens (npc and items), values True,False", true)]
		public static bool LOAD_BUFF_TOKENS;

		/// <summary>
		/// Should the server load Arrow Summoning items
		/// </summary>
		[ServerProperty("system", "load_arrow_summoning", "Should the server load Arrow Summoning items, values True,False", true)]
		public static bool LOAD_ARROW_SUMMONING;

		/// <summary>
		/// Should the server load Housing items
		/// </summary>
		[ServerProperty("system", "load_housing_items", "Should the server load Housing items, values True,False", true)]
		public static bool LOAD_HOUSING_ITEMS;

		/// <summary>
		/// Should the server load Housing NPC
		/// </summary>
		[ServerProperty("system", "load_housing_npc", "Should the server load Housing npc, values True,False", true)]
		public static bool LOAD_HOUSING_NPC;

		/// <summary>
		/// Disable Bug Reports
		/// </summary>
		[ServerProperty("system", "disable_bug_reports", "Set to true to disable bug reporting, and false to enable bug reporting", true)]
		public static bool DISABLE_BUG_REPORTS;
		/// <summary>
		/// Max bug repots in Queue
		/// </summary>
		[ServerProperty("system", "max_bugreport_queue", "Maximum number of bug reports allowed in queue.  0 to disabled max check", 100)]
		public static int MAX_BUGREPORT_QUEUE;

		/// <summary>
		/// The max number of players on the server
		/// </summary>
		[ServerProperty("system", "max_players", "Max Players - Edit this to set the maximum players allowed to connect at the same time set 0 for unlimited", 0)]
		public static int MAX_PLAYERS;

		/// <summary>
		/// What class should the server use for players
		/// </summary>
		[ServerProperty("system", "player_class", "What class should the server use for players", "DOL.GS.GamePlayer")]
		public static string PLAYER_CLASS;

		/// <summary>
		/// A serialised list of RegionIDs that will load objects
		/// </summary>
		[ServerProperty("system", "debug_load_regions", "Serialized list of region IDs that will load objects, separated by semi-colon (leave this blank to load all regions normally)", "")]
		public static string DEBUG_LOAD_REGIONS;

		/// <summary>
		/// A serialised list of disabled expansion IDs
		/// </summary>
		[ServerProperty("system", "disabled_expansions", "Serialized list of disabled expansions IDs, expansion IDs are client type seperated by ;", "")]
		public static string DISABLED_EXPANSIONS;

		/// <summary>
		/// Server Language
		/// </summary>
		[ServerProperty("system", "server_language", "Language of your server. It can be EN, FR or DE.", "EN")]
		public static string SERV_LANGUAGE;

		/// <summary>
		/// allow_change_language
		/// </summary>
		[ServerProperty("system", "allow_change_language", "Should we allow clients to change their language ?", false)]
		public static bool ALLOW_CHANGE_LANGUAGE;

		/// <summary>
		/// Allow player to change their character face after creation through customizing ?
		/// </summary>
		[ServerProperty("system", "allow_customize_face_after_creation", "Allow player to change their character face after creation through customizing ?", true)]
		public static bool ALLOW_CUSTOMIZE_FACE_AFTER_CREATION;

		/// <summary>
		/// Allow player to change their character starting stats after creation through customizing ?
		/// </summary>
		[ServerProperty("system", "allow_customize_stats_after_creation", "Allow player to change their character starting stats after creation through customizing ?", true)]
		public static bool ALLOW_CUSTOMIZE_STATS_AFTER_CREATION;

		/// <summary>
		/// StatSave Interval
		/// </summary>
		[ServerProperty("system", "statsave_interval", "Interval between each DB Stats store in minutes. -1 for deactivated.", -1)]
		public static int STATSAVE_INTERVAL;

		/// <summary>
		/// Bug Report Email Addresses
		/// </summary>
		[ServerProperty("system", "bug_report_email_addresses", "set to the email addresses you want bug reports sent to (bug reports will only send if the user has set an email address for his account, multiple addresses seperate with ;", "")]
		public static string BUG_REPORT_EMAIL_ADDRESSES;

		/// <summary>
		/// Ban Hackers
		/// </summary>
		[ServerProperty("system", "ban_hackers", "Should we ban hackers, if set to true, bans will be done, if set to false, kicks will be done", false)]
		public static bool BAN_HACKERS;

		/// <summary>
		/// Is the database translated
		/// </summary>
		[ServerProperty("system", "db_language", "What language is the DB", "EN")]
		public static string DB_LANGUAGE;

		[ServerProperty("system", "statprint_frequency", "How often (milliseconds) should statistics be printed on the server console.", 30000)]
		public static int STATPRINT_FREQUENCY;

		/// <summary>
		/// Should users be able to create characters in all realms using the same account
		/// </summary>
		[ServerProperty("system", "allow_all_realms", "should we allow characters to be created on all realms using a single account", false)]
		public static bool ALLOW_ALL_REALMS;

		/// <summary>
		/// This will Allow/Disallow dual loggins
		/// </summary>
		[ServerProperty("system", "allow_dual_logins", "Disable to disallow players to connect with more than 1 account at a time.", true)]
		public static bool ALLOW_DUAL_LOGINS;

		/// <summary>
		/// The emote delay
		/// </summary>
		[ServerProperty("system", "emote_delay", "The emote delay, default 3000ms before another emote.", 3000)]
		public static int EMOTE_DELAY;

		/// <summary>
		/// Command spam protection delay
		/// </summary>
		[ServerProperty("system", "command_spam_delay", "The spam delay, default 1500ms before the same command can be issued again.", 1500)]
		public static int COMMAND_SPAM_DELAY;

		/// <summary>
		/// This specifies the max number of inventory items to send in an update packet
		/// </summary>
		[ServerProperty("system", "max_items_per_packet", "Max number of inventory items sent per packet.", 30)]
		public static int MAX_ITEMS_PER_PACKET;

		/// <summary>
		/// Number of times speed hack detected before banning.  Must be multiples of 5 (20, 25, 30, etc)
		/// </summary>
		[ServerProperty("system", "speedhack_tolerance", "Number of times speed hack detected before banning.  Multiples of 5 (20, 25, 30, etc)", 20)]
		public static int SPEEDHACK_TOLERANCE;

		/// <summary>
		/// Turn on move detect
		/// </summary>
		[ServerProperty("system", "enable_movedetect", "Should the move detect code be enabled to kick possible movement hackers?", false)]
		public static bool ENABLE_MOVEDETECT;

		/// <summary>
		/// Coords per second tolerance before player is identified as a hacker?
		/// </summary>
		[ServerProperty("system", "cps_tolerance", "Coords per second tolerance before player is identified as a hacker?", 1000)]
		public static int CPS_TOLERANCE;

		/// <summary>
		/// Time tolerance before player is identified as move hacker
		/// </summary>
		[ServerProperty("system", "cps_time_tolerance", "Time tolerance for CPS before player is identified as a move hacker?", 200)]
		public static int CPS_TIME_TOLERANCE;

		/// <summary>
		/// Z distance tolerance before player is identified as a jump hacker
		/// </summary>
		[ServerProperty("system", "jump_tolerance", "Z distance tolerance before player is identified as a jump hacker?", 200)]
		public static int JUMP_TOLERANCE;

		/// <summary>
		/// Display centered screen messages if a player enters an area.
		/// </summary>
		[ServerProperty("system", "display_area_enter_screen_desc", "Display centered screen messages if a player enters an area.", false)]
		public static bool DISPLAY_AREA_ENTER_SCREEN_DESC;

		/// <summary>
		/// Whether or not to enable the audit log
		/// </summary>
		[ServerProperty("system", "enable_audit_log", "Whether or not to enable the audit log", false)]
		public static bool ENABLE_AUDIT_LOG;
		
		/// <summary>
		/// Enable a periodic server shutdown. If you run your server into a batch loop, this performs a restart.
		/// </summary>
		[ServerProperty("system", "hours_uptime_between_shutdown", "Hours between a scheduled server shutdown (-1 = no scheduled restart)", -1)]
		public static int HOURS_UPTIME_BETWEEN_SHUTDOWN;
		
		/// <summary>
		/// Use the NPC Guild Scripts
		/// </summary>
		[ServerProperty("system", "use_npcguildscripts", "Use the NPC Guild Scripts", true)]
		public static bool USE_NPCGUILDSCRIPTS;

		#endregion

		#region LOGGING
		/// <summary>
		/// Turn on logging of player vs player kills
		/// </summary>
		[ServerProperty("system", "log_pvp_kills", "Turn on logging of pvp kills?", false)]
		public static bool LOG_PVP_KILLS;

		/// <summary>
		/// Log All GM commands
		/// </summary>
		[ServerProperty("system", "log_all_gm_commands", "Log all GM commands on the server", false)]
		public static bool LOG_ALL_GM_COMMANDS;

		/// <summary>
		/// Should the server Log trades
		/// </summary>
		[ServerProperty("system", "log_trades", "Should the server Log all trades a player makes, values True,False", false)]
		public static bool LOG_TRADES;

		/// <summary>
		/// Log Email Addresses
		/// </summary>
		[ServerProperty("system", "log_email_addresses", "set to the email addresses you want logs automatically emailed to, multiple addresses seperate with ;", "")]
		public static string LOG_EMAIL_ADDRESSES;

		/// <summary>
		/// Enable inventory logging (trade, loot, buy, sell, quests,...)
		/// </summary>
		[ServerProperty("log", "log_inventory", "Enable inventory logging (trade, loot, buy, sell, quests,...)", false)]
		public static bool LOG_INVENTORY;

		/// <summary>
		/// Enable trade logging in inventory log (log_inventory must be enabled)
		/// </summary>
		[ServerProperty("log", "log_inventory_trade", "Enable trade logging in inventory log (log_inventory must be enabled)", true)]
		public static bool LOG_INVENTORY_TRADE;

		/// <summary>
		/// Enable craft logging in inventory log (log_inventory must be enabled)
		/// </summary>
		[ServerProperty("log", "log_inventory_craft", "Enable craft logging in inventory log (log_inventory must be enabled)", true)]
		public static bool LOG_INVENTORY_CRAFT;

		/// <summary>
		/// Enable loot logging in inventory log (log_inventory must be enabled)
		/// </summary>
		[ServerProperty("log", "log_inventory_loot", "Enable loot logging in inventory log (log_inventory must be enabled)", true)]
		public static bool LOG_INVENTORY_LOOT;

		/// <summary>
		/// Enable quest logging in inventory log (log_inventory must be enabled)
		/// </summary>
		[ServerProperty("log", "log_inventory_quest", "Enable quest logging in inventory log (log_inventory must be enabled)", true)]
		public static bool LOG_INVENTORY_QUEST;

		/// <summary>
		/// Enable merchant logging in inventory log (log_inventory must be enabled)
		/// </summary>
		[ServerProperty("log", "log_inventory_merchant", "Enable merchant logging in inventory log (log_inventory must be enabled)", true)]
		public static bool LOG_INVENTORY_MERCHANT;

		/// <summary>
		/// Enable other logging in inventory log (log_inventory must be enabled)
		/// </summary>
		[ServerProperty("log", "log_inventory_other", "Enable other logging in inventory log (log_inventory must be enabled)", true)]
		public static bool LOG_INVENTORY_OTHER;
		#endregion

		#region SERVER

        /// <summary>
        /// Enable integrated serverlistupdate script?
        /// </summary>
        [ServerProperty("server", "enable_serverlistupdate", "Enable in-built serverlistupdate script?", false)]
        public static bool USE_SERVER_LIST_UPDATE_INTEGRATED;

        /// <summary>
        /// The username for server list update.
        /// </summary>
        [ServerProperty("server", "serverlistupdate_user", "Username for serverlistupdate.", "")]
        public static string SERVER_LIST_UPDATE_USER;

        /// <summary>
        /// The password for server list update.
        /// </summary>
        [ServerProperty("server", "serverlistupdate_password", "Password for serverlistupdate.", "")]
        public static string SERVER_LIST_UPDATE_PASS;

		/// <summary>
		/// Post 1.108 Passive RA 9-Tiers
		/// </summary>
		[ServerProperty("server", "use_new_passives_ras_scaling", "Use new passives realmabilities scaling (1.108+) ?", false)]
		public static bool USE_NEW_PASSIVES_RAS_SCALING;

		/// <summary>
		/// Post 1.108 Active RA 5-Tiers
		/// </summary>
		[ServerProperty("server", "use_new_actives_ras_scaling", "Use new actives realmabilities (5-Tiers) scaling (1.108+) ?", false)]
		public static bool USE_NEW_ACTIVES_RAS_SCALING;

		/// <summary>
		/// Use pre 1.105 train or livelike
		/// </summary>
		[ServerProperty("server", "custom_train", "Train is custom pre-1.105 one ? (false set it to livelike 1.105+)", true)]
		public static bool CUSTOM_TRAIN;

		/// <summary>
		/// Record news in database
		/// </summary>
		[ServerProperty("server", "record_news", "Record News in database?", true)]
		public static bool RECORD_NEWS;

		/// <summary>
		/// The Server Message of the Day
		/// </summary>
		[ServerProperty("server", "motd", "The Server Message of the Day - Edit this to set what is displayed when a level 2+ character enters the game for the first time, set to \"\" for nothing", "Welcome to a Dawn of Light server, please edit this MOTD")]
		public static string MOTD;

		/// <summary>
		/// The message players get when they enter the game past level 1
		/// </summary>
		[ServerProperty("server", "starting_msg", "The Starting Mesage - Edit this to set what is displayed when a level 1 character enters the game for the first time, set to \"\" for nothing", "Welcome for your first time to a Dawn of Light server, please edit this Starter Message")]
		public static string STARTING_MSG;

		/// <summary>
		/// The broadcast type
		/// </summary>
		[ServerProperty("server", "broadcast_type", "Broadcast Type - Edit this to change what /b does, values 0 = disabled, 1 = area, 2 = visibility distance, 3 = zone, 4 = region, 5 = realm, 6 = server", 1)]
		public static int BROADCAST_TYPE;

		/// <summary>
		/// Anon Modifier
		/// </summary>
		[ServerProperty("server", "anon_modifier", "Various modifying options for anon, 0 = default, 1 = /who shows player but as ANON, -1 = disabled", 0)]
		public static int ANON_MODIFIER;

		/// <summary>
		/// Should the server load the example scripts
		/// </summary>
		[ServerProperty("server", "load_examples", "Should the server load the example scripts", true)]
		public static bool LOAD_EXAMPLES;

		/// <summary>
		/// Death Messages All Realms
		/// </summary>
		[ServerProperty("server", "death_messages_all_realms", "Set to true if you want all realms to see other realms death and kill messages", false)]
		public static bool DEATH_MESSAGES_ALL_REALMS;

		/// <summary>
		/// Disable Instances
		/// </summary>
		[ServerProperty("server", "disable_instances", "Enable or disable instances on the server", false)]
		public static bool DISABLE_INSTANCES;

		/// <summary>
		/// Save QuestItems into Database
		/// </summary>
		[ServerProperty("server", "save_quest_mobs_into_database", "set false if you don't want this", true)]
		public static bool SAVE_QUEST_MOBS_INTO_DATABASE;

		/// <summary>
		/// This specifies the max amount of people in one battlegroup.
		/// </summary>
		[ServerProperty("server", "battlegroup_max_member", "Max number of members allowed in a battlegroup.", 64)]
		public static int BATTLEGROUP_MAX_MEMBER;

		/// <summary>
		///  This specifies the max amount of people in one group.
		/// </summary>
		[ServerProperty("server", "group_max_member", "Max number of members allowed in a group.", 8)]
		public static int GROUP_MAX_MEMBER;

		/// <summary>
		/// Sets the disabled commands for the server split by ;
		/// </summary>
		[ServerProperty("server", "disabled_commands", "Serialized list of disabled commands separated by semi-colon, example /realm;/toon;/quit", "")]
		public static string DISABLED_COMMANDS;

		/// <summary>
		/// Disable Appeal System
		/// </summary>
		[ServerProperty("server", "disable_appeal_system", "Disable the /Appeal System", false)]
		public static bool DISABLE_APPEALSYSTEM;

		/// <summary>
		/// Use Database Language datas instead of files (if empty = build the table from files)
		/// </summary>
		[ServerProperty("server", "use_dblanguage", "Use Database Language datas instead of files (if empty = build the table from files)", false)]
		public static bool USE_DBLANGUAGE;

		/// <summary>
		/// Update existing rows within the LanguageSystem table from language files.
		/// </summary>
		[ServerProperty("server", "update_existing_db_system_sentences_from_files", "Update existing rows within the LanguageSystem table from language files.", false)]
		public static bool UPDATE_EXISTING_DB_SYSTEM_SENTENCES_FROM_FILES;

		/// <summary>
		/// Set the maximum number of objects allowed in a region.  Smaller numbers offer better performance.  This is used to allocate arrays for both Regions and GamePlayers
		/// </summary>
		[ServerProperty("server", "region_max_objects", "Set the maximum number of objects allowed in a region.  Smaller numbers offer better performance.  This can't be changed while the server is running. (256 - 65535)", (ushort)30000)]
		public static ushort REGION_MAX_OBJECTS;

		/// <summary>
		/// Show logins
		/// </summary>
		[ServerProperty("server", "show_logins", "Show login messages when players log in and out of game?", true)]
		public static bool SHOW_LOGINS;

		/// <summary>
		/// Show logins channel
		/// </summary>
		[ServerProperty("server", "show_logins_channel", "What channel should be used for login messages? See eChatType, default is System.", (byte)0)]
		public static byte SHOW_LOGINS_CHANNEL;

		/// <summary>
		/// Enable PvE Speed
		/// </summary>
		[ServerProperty("server", "enable_pve_speed", "Set to true if you wish to enable the extra 25% increase to speed when not in combat or an RvR zone", true)]
		public static bool ENABLE_PVE_SPEED;

		/// <summary>
		/// Enable Encumberance Speed loss
		/// </summary>
		[ServerProperty("server", "enable_encumberance_speed_loss", "Set to true if you wish to enable the encumberance speed loss", true)]
		public static bool ENABLE_ENCUMBERANCE_SPEED_LOSS;

		/// <summary>
		/// Property to enable "forced" Tooltip send when Update are made to player skills, or player effects.
		/// </summary>
		[ServerProperty("server", "use_new_tooltip_forcedupdate", "Set to true if you wish to enable the new 1.110+ Tooltip Forced update each time the server send a skill to a new client.", true)]
		public static bool USE_NEW_TOOLTIP_FORCEDUPDATE;

		#endregion

		#region WORLD
		/// <summary>
		/// Epic encounters strength: 100 is 100% base strength
		/// </summary>
		[ServerProperty("world", "set_difficulty_on_epic_encounters", "Tune encounters taggued <Epic Encounter>. 0 means auto adaptative, others values are % of the initial difficulty (100%=initial difficulty)", 100)]
		public static int SET_DIFFICULTY_ON_EPIC_ENCOUNTERS;

		/// <summary>
		/// A serialised list of disabled RegionIDs
		/// </summary>
		[ServerProperty("world", "disabled_regions", "Serialized list of disabled region IDs, separated by semi-colon or a range with a dash (ie 1-5;7;9)", "")]
		public static string DISABLED_REGIONS;

		/// <summary>
		/// Should the server disable the tutorial zone
		/// </summary>
		[ServerProperty("world", "disable_tutorial", "should the server disable the tutorial zone", false)]
		public static bool DISABLE_TUTORIAL;

		[ServerProperty("world", "world_item_decay_time", "How long (milliseconds) will an item dropped on the ground stay in the world.", (uint)180000)]
		public static uint WORLD_ITEM_DECAY_TIME;

		[ServerProperty("world", "world_pickup_distance", "How far before you can no longer pick up an object (loot for example).", 256)]
		public static int WORLD_PICKUP_DISTANCE;

		[ServerProperty("world", "world_day_increment", "Day Increment (0 to 512, default is 24).  Larger increments make shorter days.", (uint)24)]
		public static uint WORLD_DAY_INCREMENT;

		[ServerProperty("world", "world_npc_update_interval", "How often (milliseconds) will npc's broadcast updates to the clients. Minimum allowed = 1000 (1 second). 0 will disable this update.", (uint)10000)]
		public static uint WORLD_NPC_UPDATE_INTERVAL;

		[ServerProperty("world", "world_object_update_interval", "How often (milliseconds) will objects (static, housing, doors, broadcast updates to the clients. Minimum allowed = 10000 (10 seconds). 0 will disable this update.", (uint)30000)]
		public static uint WORLD_OBJECT_UPDATE_INTERVAL;

		[ServerProperty("world", "world_playertoplayer_update_interval", "How often (milliseconds) will other players packet be broadcasted again to the clients. Minimum allowed = 1000 (1 seconds). 0 will disable this update.", (uint)1000)]
		public static uint WORLD_PLAYERTOPLAYER_UPDATE_INTERVAL;

		[ServerProperty("world", "world_player_update_interval", "How often (milliseconds) will players be checked for updates. Minimum allowed = 100 (100 milliseconds).", (uint)300)]
		public static uint WORLD_PLAYER_UPDATE_INTERVAL;

		[ServerProperty("world", "weather_check_interval", "How often (milliseconds) will weather be checked for a chance to start a storm.", 5 * 60 * 1000)]
		public static int WEATHER_CHECK_INTERVAL;

		[ServerProperty("world", "weather_chance", "What is the chance of starting a storm.", 5)]
		public static int WEATHER_CHANCE;

		[ServerProperty("world", "weather_log_events", "Should weather events be shown in the Log (and on the console).", true)]
		public static bool WEATHER_LOG_EVENTS;

		/// <summary>
		/// Perform checklos on client with each mob
		/// </summary>
		[ServerProperty("world", "always_check_los", "Perform a LoS check before aggroing. This can involve a huge lag, handle with care!", false)]
		public static bool ALWAYS_CHECK_LOS;

		/// <summary>
		/// Perform checklos on client with each mob
		/// </summary>
		[ServerProperty("world", "check_los_during_cast", "Perform a LOS check during a spell cast.", true)]
		public static bool CHECK_LOS_DURING_CAST;

		/// <summary>
		/// Perform LOS check between controlled NPC's and players
		/// </summary>
		[ServerProperty("world", "always_check_pet_los", "Should we perform LOS checks between controlled NPC's and players?", false)]
		public static bool ALWAYS_CHECK_PET_LOS;

		/// <summary>
		/// LOS check frequency; how often are we allowed to check LOS on the same player (seconds)
		/// </summary>
		[ServerProperty("world", "los_player_check_frequency", "How often are we allowed to check LOS on the same player (seconds)", (ushort)5)]
		public static ushort LOS_PLAYER_CHECK_FREQUENCY;
	
		/// <summary>
		/// HPs gained per champion's level
		/// </summary>
		[ServerProperty("world", "hps_per_championlevel", "The amount of extra HPs gained each time you reach a new Champion's Level", 40)]
		public static int HPS_PER_CHAMPIONLEVEL;

		/// <summary>
		/// Time player must wait after failed task
		/// </summary>
		[ServerProperty("world", "task_pause_ticks", "Time player must wait after failed task check to get new chance for a task, in milliseconds", 5 * 60 * 1000)]
		public static int TASK_PAUSE_TICKS;

		/// <summary>
		/// Should we handle tasks with items
		/// </summary>
		[ServerProperty("world", "task_give_random_item", "Task is also rewarded with ROG ?", false)]
		public static bool TASK_GIVE_RANDOM_ITEM;

		/// <summary>
		/// Should we enable Zone Bonuses?
		/// </summary>
		[ServerProperty("world", "enable_zone_bonuses", "Are Zone Bonuses Enabled?", false)]
		public static bool ENABLE_ZONE_BONUSES;

		/// <summary>
		/// List of ZoneId where personnal mount is allowed
		/// </summary>
		[ServerProperty("world", "allow_personnal_mount_in_regions", "CSV Regions where player mount is allowed", "")]
		public static string ALLOW_PERSONNAL_MOUNT_IN_REGIONS;

		/// <summary>
		/// Immunity Timer length when a player logs into game or zones into a new region, in seconds
		/// </summary>
		[ServerProperty("world", "timer_player_init", "Immunity Timer length when a player logs into game or zones into a new region, in seconds", 15)]
		public static int TIMER_PLAYER_INIT;

		/// <summary>
		/// Display the zonepoint with a choosen model
		/// </summary>
		[ServerProperty("world", "zonepoint_npctemplate", "Display the zonepoint with the following npctemplate. 0 for no display", 0)]
		public static int ZONEPOINT_NPCTEMPLATE;

		/// <summary>
		/// Line of Sight Manager Enable
		/// </summary>
		[ServerProperty("world", "losmgr_enable", "Enable the Line of Sight Manager where overriden methods are implemented.", false)]
		public static bool LOSMGR_ENABLE;

		/// <summary>
		/// Line of Sight Manager Debug Level
		/// </summary>
		[ServerProperty("world", "losmgr_debug_level", "Set the Level of Debug for the Line of Sight (LoS) Manager (do not set level 3 in production), 0 = no debug, 1 = info, 2 = warn, 3 = debug.", 0)]
		public static int LOSMGR_DEBUG_LEVEL;

		/// <summary>
		/// Line of Sight Manager Cleanup Frequency
		/// </summary>
		[ServerProperty("world", "losmgr_cleanup_frequency", "How fast should the Line of Sight (LoS) Manager clean up its data (in milliseconds), don't get under 30000 ms, raise cleanup count if you put high number here.", 120000)]
		public static int LOSMGR_CLEANUP_FREQUENCY;

		/// <summary>
		/// Line of Sight Manager number of entries to Cleanup
		/// </summary>
		[ServerProperty("world", "losmgr_cleanup_entries", "Number of Entries cleaned from Line of Sight (LoS) Manager each Cleanup ticks, if you need to go above 10 000 entries consider using a shorter cleanup frequency.", 1000)]
		public static int LOSMGR_CLEANUP_ENTRIES;

		/// <summary>
		/// Line of Sight Manager Query Timeout
		/// </summary>
		[ServerProperty("world", "losmgr_query_timeout", "Timeout (in milliseconds) until Line of Sight (LoS) Manager tries to resend a LoS check, -1 to disable (don't get under 100ms except for Local Network).", 300)]
		public static int LOSMGR_QUERY_TIMEOUT;

		/// <summary>
		/// Line of Sight Manager PvP Cache Timeout
		/// </summary>
		[ServerProperty("world", "losmgr_player_vs_player_cache_timeout", "Set Timeout (in milliseconds) for PvP Line of Sight (LoS) Manager cache data, 0 to disable.", 200)]
		public static int LOSMGR_PLAYER_VS_PLAYER_CACHE_TIMEOUT;

		/// <summary>
		/// Line of Sight Manager PvE Cache Timeout
		/// </summary>
		[ServerProperty("world", "losmgr_player_vs_environment_cache_timeout", "Set Timeout (in milliseconds) for PvE Line of Sight (LoS) Manager cache data, 0 to disable. (Should be slightly above Brain Think timer to have any effect)", 1500)]
		public static int LOSMGR_PLAYER_VS_ENVIRONMENT_CACHE_TIMEOUT;

		/// <summary>
		/// Line of Sight Manager EvE Cache Timeout
		/// </summary>
		[ServerProperty("world", "losmgr_environment_vs_environment_cache_timeout", "Set Timeout (in milliseconds) for EvE Line of Sight (LoS) Manager cache data, 0 to disable. (Should be at least 2 times PvE cache to lower LoS Queries)", 5000)]
		public static int LOSMGR_ENVIRONMENT_VS_ENVIRONMENT_CACHE_TIMEOUT;

		/// <summary>
		/// Line of Sight Manager Player Check Frequency
		/// </summary>
		[ServerProperty("world", "losmgr_player_check_frequency", "Line of Sight (LoS) Manager will try to reduce player queries to this frequency (in millisecond), raise this if player are experiencing lags.", 100)]
		public static int LOSMGR_PLAYER_CHECK_FREQUENCY;

		/// <summary>
		/// Line of Sight Manager PvP threshold
		/// </summary>
		[ServerProperty("world", "losmgr_player_vs_player_range_threshold", "Line of Sight (LoS) Manager won't check LoS for players within this range. (Should be low to prevent abuses)", 32)]
		public static int LOSMGR_PLAYER_VS_PLAYER_RANGE_THRESHOLD;

		/// <summary>
		/// Line of Sight Manager PvE threshold
		/// </summary>
		[ServerProperty("world", "losmgr_player_vs_environment_range_threshold", "Line of Sight (LoS) Manager won't check LoS for mobs and NPC within this range. (Shouldn't be under default to prevent LoS flood)", 125)]
		public static int LOSMGR_PLAYER_VS_ENVIRONMENT_RANGE_THRESHOLD;

		/// <summary>
		/// Line of Sight Manager EvE threshold
		/// </summary>
		[ServerProperty("world", "losmgr_environment__vs_environment_range_threshold", "Line of Sight (LoS) Manager won't check LoS for Mobs vs Mobs within this range. (Shouldn't be under PvE Threshold)", 350)]
		public static int LOSMGR_ENVIRONMENT_VS_ENVIRONMENT_RANGE_THRESHOLD;

		/// <summary>
		/// Line of Sight Manager EvE Contamination
		/// </summary>
		[ServerProperty("world", "losmgr_max_contamination_radius", "Line of Sight (LoS) Manager will update LoS of Mobs/NPCs in this range of the checker if target is a Mob, and highest value used for contamination LoS updates.", 350)]
		public static int LOSMGR_MAX_CONTAMINATION_RADIUS;

		/// <summary>
		/// Line of Sight Manager PvE Contamination
		/// </summary>
		[ServerProperty("world", "losmgr_npc_contamination_radius", "Line of Sight (LoS) Manager will update LoS of Mobs/NPCs in this range of the checker, and highest value used for Player contamination LoS updates.", 250)]
		public static int LOSMGR_NPC_CONTAMINATION_RADIUS;

		/// <summary>
		/// Line of Sight Manager Pets Contamination
		/// </summary>
		[ServerProperty("world", "losmgr_pet_contamination_radius", "Line of Sight (LoS) Manager will update LoS of Player's Pet in this range of the checker, keep value low to prevent abuses.", 50)]
		public static int LOSMGR_PET_CONTAMINATION_RADIUS;

		/// <summary>
		/// Line of Sight Manager Player Contamination
		/// </summary>
		[ServerProperty("world", "losmgr_players_contamination_radius", "Line of Sight (LoS) Manager will update LoS of Players in this range of the checker, should be disabled to prevent PvP abuses.", 0)]
		public static int LOSMGR_PLAYER_CONTAMINATION_RADIUS;

		/// <summary>
		/// Line of Sight Manager Guards Contamination
		/// </summary>
		[ServerProperty("world", "losmgr_guard_contamination_radius", "Line of Sight (LoS) Manager will update LoS of Keep Guards in this range of the checker, raising this value will pack more Guards when Aggroing.", 200)]
		public static int LOSMGR_GUARD_CONTAMINATION_RADIUS;

		/// <summary>
		/// Line of Sight Manager Contamination Z-Factor in range checks
		/// </summary>
		[ServerProperty("world", "losmgr_contamination_zfactor", "Line of Sight (LoS) Manager Contamination will use this to lower or raise the Z checks when updating LoS checks. 0 = Z must be exact, 1 = Z range is radius.", 0.5)]
		public static double LOSMGR_CONTAMINATION_ZFACTOR;

		#endregion

		#region RATES
		/// <summary>
		/// Xp Cap for a player.  Given in percent of level.  Default is 125%
		/// </summary>
		[ServerProperty("rates", "XP_Cap_Percent", "Maximum XP a player can earn given in percent of their level. Default is 125%", 125)]
		public static int XP_CAP_PERCENT;

		/// <summary>
		/// Xp Cap for a player in a group.  Given in percent of level.  Default is 125%
		/// </summary>
		[ServerProperty("rates", "XP_Group_Cap_Percent", "Maximum XP a player can earn while in a group, given in percent of their level. Default is 125%", 125)]
		public static int XP_GROUP_CAP_PERCENT;

		/// <summary>
		/// Xp Cap for a player vs player kill.  Given in percent of level.  Default is 125%
		/// </summary>
		[ServerProperty("rates", "XP_PVP_Cap_Percent", "Maximum XP a player can earn killing another player, given in percent of their level. Default is 125%", 125)]
		public static int XP_PVP_CAP_PERCENT;

		/// <summary>
		/// Hardcap XP a player can earn after all other adjustments are applied.  Given in percent of level, default is 500%  There is no live value that corresponds to this cap.
		/// </summary>
		[ServerProperty("rates", "XP_HardCap_Percent", "Hardcap XP a player can earn after all other adjustments are applied. Given in percent of their level. Default is 500%", 500)]
		public static int XP_HARDCAP_PERCENT;

		/// <summary>
		/// The Experience Rate
		/// </summary>
		[ServerProperty("rates", "xp_rate", "The Experience Points Rate Modifier - Edit this to change the rate at which you gain experience points e.g 1.5 is 50% more 2.0 is twice the amount (100%) 0.5 is half the amount (50%)", 1.0)]
		public static double XP_RATE;

		/// <summary>
		/// The CL Experience Rate
		/// </summary>
		[ServerProperty("rates", "cl_xp_rate", "The Champion Level Experience Points Rate Modifier - Edit this to change the rate at which you gain CL experience points e.g 1.5 is 50% more 2.0 is twice the amount (100%) 0.5 is half the amount (50%)", 1.0)]
		public static double CL_XP_RATE;

		/// <summary>
		/// RvR Zones XP Rate
		/// </summary>
		[ServerProperty("rates", "rvr_zones_xp_rate", "The RvR zones Experience Points Rate Modifier", 1.0)]
		public static double RvR_XP_RATE;

		/// <summary>
		/// The Realm Points Rate
		/// </summary>
		[ServerProperty("rates", "rp_rate", "The Realm Points Rate Modifier - Edit this to change the rate at which you gain realm points e.g 1.5 is 50% more 2.0 is twice the amount (100%) 0.5 is half the amount (50%)", 1.0)]
		public static double RP_RATE;

		/// <summary>
		/// The Bounty Points Rate
		/// </summary>
		[ServerProperty("rates", "bp_rate", "The Bounty Points Rate Modifier - Edit this to change the rate at which you gain bounty points e.g 1.5 is 50% more 2.0 is twice the amount (100%) 0.5 is half the amount (50%)", 1.0)]
		public static double BP_RATE;

		/// <summary>
		/// The damage players do against monsters with melee
		/// </summary>
		[ServerProperty("rates", "pve_melee_damage", "The PvE Melee Damage Modifier - Edit this to change the amount of melee damage done when fighting mobs e.g 1.5 is 50% more damage 2.0 is twice the damage (100%) 0.5 is half the damage (50%)", 1.0)]
		public static double PVE_MELEE_DAMAGE;

		/// <summary>
		/// The damage players do against monsters with spells
		/// </summary>
		[ServerProperty("rates", "pve_spell_damage", "The PvE Spell Damage Modifier - Edit this to change the amount of spell damage done when fighting mobs e.g 1.5 is 50% more damage 2.0 is twice the damage (100%) 0.5 is half the damage (50%)", 1.0)]
		public static double PVE_SPELL_DAMAGE;

		/// <summary>
		/// The percent per con difference (-1 = blue, 0 = yellow, 1 = OJ, 2 = red ...) subtracted to hitchance for spells in PVE.  0 is none, 5 is 5% per con, etc.  Default is 10%
		/// </summary>
		[ServerProperty("rates", "pve_spell_conhitpercent", "The percent per con (1 = OJ, 2 = red ...) subtracted to hitchance for spells in PVE  Must be >= 0.  0 is none, 5 is 5% per level, etc.  Default is 10%", (uint)10)]
		public static uint PVE_SPELL_CONHITPERCENT;

		/// <summary>
		/// The damage players do against players with melee
		/// </summary>
		[ServerProperty("rates", "pvp_melee_damage", "The PvP Melee Damage Modifier - Edit this to change the amount of melee damage done when fighting players e.g 1.5 is 50% more damage 2.0 is twice the damage (100%) 0.5 is half the damage (50%)", 1.0)]
		public static double PVP_MELEE_DAMAGE;

		/// <summary>
		/// The damage players do against players with spells
		/// </summary>
		[ServerProperty("rates", "pvp_spell_damage", "The PvP Spell Damage Modifier - Edit this to change the amount of spell damage done when fighting players e.g 1.5 is 50% more damage 2.0 is twice the damage (100%) 0.5 is half the damage (50%)", 1.0)]
		public static double PVP_SPELL_DAMAGE;

		/// <summary>
		/// The % value of gainrps when heal a players recently damaged in rvr.
		/// </summary>
		[ServerProperty("rates", "heal_pvp_damage_value_rp", "How many % of heal final value is obtained in rps?", 8)]
		public static int HEAL_PVP_DAMAGE_VALUE_RP;

		/// <summary>
		/// The highest possible Block Rate against an Enemy (Hard Cap)
		/// </summary>
		[ServerProperty("rates", "block_cap", "Block Rate Cap Modifier - Edit this to change the highest possible block rate against an enemy (Hard Cap) in game e.g .60 = 60%", 0.60)]
		public static double BLOCK_CAP;

		///<summary>
		/// The highest possible Evade Rate against an Enemy (Hard Cap)
		/// </summary>
		[ServerProperty("rates", "evade_cap", "Evade Rate Cap Modifier - Edit this to change the highest possible evade rate against an enemy (Hard Cap) in game e.g .50 = 50%", 0.50)]
		public static double EVADE_CAP;

		///<summary>
		///The highest possible Parry Rate against an Enemy (Hard Cap)
		/// </summary>
		[ServerProperty("rates", "parry_cap", "Parry Rate Cap Modifier - Edit this to change the highest possible parry rate against an enemy (Hard Cap) in game e.g .50 = 50%", 0.50)]
		public static double PARRY_CAP;

		/// <summary>
		/// Critical strike opening style effectiveness.  Increase this to make CS styles BS, BSII and Perf Artery more effective
		/// </summary>
		[ServerProperty("rates", "cs_opening_effectiveness", "Critical strike opening style effectiveness.  Increase this to make CS styles BS, BSII and Perf Artery more effective", 1.0)]
		public static double CS_OPENING_EFFECTIVENESS;

		/// <summary>
		/// The money drop modifier
		/// </summary>
		[ServerProperty("rates", "money_drop", "Money Drop Modifier - Edit this to change the amount of money which is dropped e.g 1.5 is 50% more 2.0 is twice the amount (100%) 0.5 is half the amount (50%)", 1.0)]
		public static double MONEY_DROP;

		/// <summary>
		/// The time until a player is worth rps again after death
		/// </summary>
		[ServerProperty("rates", "rp_worth_seconds", "Realm Points Worth Seconds - Edit this to change how many seconds until a player is worth RPs again after being killed ", 300)]
		public static int RP_WORTH_SECONDS;

		/// <summary>
		/// Health Regen Rate
		/// </summary>
		[ServerProperty("rates", "health_regen_rate", "Health regen rate", 1.0)]
		public static double HEALTH_REGEN_RATE;

		/// <summary>
		/// Health Regen Rate
		/// </summary>
		[ServerProperty("rates", "endurance_regen_rate", "Endurance regen rate", 1.0)]
		public static double ENDURANCE_REGEN_RATE;

		/// <summary>
		/// Health Regen Rate
		/// </summary>
		[ServerProperty("rates", "mana_regen_rate", "Mana regen rate", 1.0)]
		public static double MANA_REGEN_RATE;

		/// <summary>
		/// Items sell ratio
		/// </summary>
		[ServerProperty("rates", "item_sell_ratio", "Merchants are buying items at the % of initial value", 50)]
		public static int ITEM_SELL_RATIO;

		/// <summary>
		/// Chance for condition loss on weapons and armor
		/// </summary>
		[ServerProperty("rates", "item_condition_loss_chance", "What chance does armor or weapon have to lose condition?", 5)]
		public static int ITEM_CONDITION_LOSS_CHANCE;

		/// <summary>
		/// Under level 35 mount speed, live like = 135
		/// </summary>
		[ServerProperty("rates", "mount_under_level_35_speed", "What is the speed of player controlled mounts under level 35?", (short)135)]
		public static short MOUNT_UNDER_LEVEL_35_SPEED;

		/// <summary>
		/// Over level 35 mount speed, live like = 145
		/// </summary>
		[ServerProperty("rates", "mount_over_level_35_speed", "What is the speed of player controlled mounts over level 35?", (short)145)]
		public static short MOUNT_OVER_LEVEL_35_SPEED;

		/// <summary>
		/// Relic Bonus Modifier
		/// </summary>
		[ServerProperty("rates", "relic_owning_bonus", "Relic Owning Bonus in percent per relic (default 10%) in effect when owning enemy relic", (short)10)]
		public static short RELIC_OWNING_BONUS;		

		#endregion

		#region NPCs
		/// <summary>
		/// Base Value to use when auto-setting STR stat.
		/// </summary>
		[ServerProperty("npc", "mob_autoset_str_base", "Base Value to use when auto-setting STR stat. ", 30.0)]
		public static double MOB_AUTOSET_STR_BASE;

		/// <summary>
		/// Multiplier to use when auto-setting STR stat.
		/// </summary>
		[ServerProperty("npc", "mob_autoset_str_multiplier", "Multiplier to use when auto-setting STR stat. ", 1.0)]
		public static double MOB_AUTOSET_STR_MULTIPLIER;

		/// <summary>
		/// Base Value to use when auto-setting CON stat.
		/// </summary>
		[ServerProperty("npc", "mob_autoset_con_base", "Base Value to use when auto-setting CON stat. ", 30.0)]
		public static double MOB_AUTOSET_CON_BASE;

		/// <summary>
		/// Multiplier to use when auto-setting CON stat.
		/// </summary>
		[ServerProperty("npc", "mob_autoset_con_multiplier", "Multiplier to use when auto-setting CON stat. ", 1.0)]
		public static double MOB_AUTOSET_CON_MULTIPLIER;

		/// <summary>
		/// Base Value to use when auto-setting QUI stat.
		/// </summary>
		[ServerProperty("npc", "mob_autoset_qui_base", "Base Value to use when auto-setting qui stat. ", 30.0)]
		public static double MOB_AUTOSET_QUI_BASE;

		/// <summary>
		/// Multiplier to use when auto-setting QUI stat.
		/// </summary>
		[ServerProperty("npc", "mob_autoset_qui_multiplier", "Multiplier to use when auto-setting QUI stat. ", 1.0)]
		public static double MOB_AUTOSET_QUI_MULTIPLIER;

		/// <summary>
		/// Base Value to use when auto-setting DEX stat.
		/// </summary>
		[ServerProperty("npc", "mob_autoset_dex_base", "Base Value to use when auto-setting DEX stat. ", 30.0)]
		public static double MOB_AUTOSET_DEX_BASE;

		/// <summary>
		/// Multiplier to use when auto-setting DEX stat.
		/// </summary>
		[ServerProperty("npc", "mob_autoset_dex_multiplier", "Multiplier to use when auto-setting DEX stat. ", 1.0)]
		public static double MOB_AUTOSET_DEX_MULTIPLIER;

		/// <summary>
		/// Base Value to use when auto-setting pet STR stat.
		/// </summary>
		[ServerProperty("npc", "pet_autoset_str_base", "Base Value to use when auto-setting Pet STR stat. ", 20.0)]
		public static double PET_AUTOSET_STR_BASE;

		/// <summary>
		/// Multiplier to use when auto-setting pet STR stat.
		/// </summary>
		[ServerProperty("npc", "pet_autoset_str_multiplier", "Multiplier to use when auto-setting Pet STR stat. ", 6.0)]
		public static double PET_AUTOSET_STR_MULTIPLIER;
		
		/// Base Value to use when auto-setting pet CON stat.
		/// </summary>
		[ServerProperty("npc", "pet_autoset_con_base", "Base Value to use when auto-setting Pet CON stat. ", 30.0)]
		public static double PET_AUTOSET_CON_BASE;

		/// <summary>
		/// Multiplier to use when auto-setting pet CON stat.
		/// </summary>
		[ServerProperty("npc", "pet_autoset_con_multiplier", "Multiplier to use when auto-setting Pet CON stat. ", 1.0)]
		public static double PET_AUTOSET_CON_MULTIPLIER;

		/// Base Value to use when auto-setting Pet DEX stat.
		/// </summary>
		[ServerProperty("npc", "pet_autoset_dex_base", "Base Value to use when auto-setting Pet DEX stat. ", 30.0)]
		public static double PET_AUTOSET_DEX_BASE;

		/// <summary>
		/// Multiplier to use when auto-setting pet DEX stat.
		/// </summary>
		[ServerProperty("npc", "pet_autoset_dex_multiplier", "Multiplier to use when auto-setting Pet DEX stat. ", 1.0)]
		public static double PET_AUTOSET_DEX_MULTIPLIER;

		/// Base Value to use when auto-setting Pet QUI stat.
		/// </summary>
		[ServerProperty("npc", "pet_autoset_qui_base", "Base Value to use when auto-setting Pet QUI stat. ", 30.0)]
		public static double PET_AUTOSET_QUI_BASE;

		/// <summary>
		/// Multiplier to use when auto-setting pet QUI stat.
		/// </summary>
		[ServerProperty("npc", "pet_autoset_qui_multiplier", "Multiplier to use when auto-setting Pet QUI stat. ", 1.0)]
		public static double PET_AUTOSET_QUI_MULTIPLIER;

		/// <summary>
		/// How often should pets think?  Default 1500 or 1.5 seconds
		/// </summary>
		[ServerProperty("npc", "pet_think_interval", "How often should pets think?  Default 1500 (1.5 seconds)", 1500)]
		public static int PET_THINK_INTERVAL;

		/// <summary>
		/// What level to start increasing mob damage
		/// </summary>
		[ServerProperty("npc", "mob_damage_increase_startlevel", "What level to start increasing mob damage.", 30)]
		public static int MOB_DAMAGE_INCREASE_STARTLEVEL;

		/// <summary>
		/// How much damage to increase per level
		/// </summary>
		[ServerProperty("npc", "mob_damage_increase_perlevel", "How much damage to increase per level", 0.0)]
		public static double MOB_DAMAGE_INCREASE_PERLEVEL;

		/// <summary>
		/// Minimum respawn time for npc's without a set respawninterval
		/// </summary>
		[ServerProperty("npc", "npc_min_respawn_interval", "Minimum respawn time, in minutes, for npc's without a set respawninterval", 5)]
		public static int NPC_MIN_RESPAWN_INTERVAL;

		/// <summary>
		/// Allow Roam
		/// </summary>
		[ServerProperty("npc", "allow_roam", "Allow mobs to roam on the server", true)]
		public static bool ALLOW_ROAM;

		/// <summary>
		/// This is to set the baseHP For NPCs
		/// </summary>
		[ServerProperty("npc", "gamenpc_base_hp", "GameNPC's base HP * level", 500)]
		public static int GAMENPC_BASE_HP;

		/// <summary>
		/// How many hitpoints per point of CON above gamenpc_base_con should an NPC gain.
		/// This modification is applied prior to any buffs
		/// </summary>
		[ServerProperty("npc", "gamenpc_hp_gain_per_con", "How many hitpoints per point of CON above gamenpc_base_con should an NPC gain", 2)]
		public static int GAMENPC_HP_GAIN_PER_CON;

		/// <summary>
		/// What is the base contitution for npc's
		/// </summary>
		[ServerProperty("npc", "gamenpc_base_con", "GameNPC's base Constitution", 30)]
		public static int GAMENPC_BASE_CON;

		/// <summary>
		/// Chance for NPC to random walk. Default is 20
		/// </summary>
		[ServerProperty("npc", "gamenpc_randomwalk_chance", "Chance for NPC to random walk. Default is 20", 20)]
		public static int GAMENPC_RANDOMWALK_CHANCE;

		/// <summary>
		/// How often, in milliseconds, to check follow distance.  Lower numbers make NPC follow closer but increase load on server.
		/// </summary>
		[ServerProperty("npc", "gamenpc_followcheck_time", "How often, in milliseconds, to check follow distance. Lower numbers make NPC follow closer but increase load on server.", 500)]
		public static int GAMENPC_FOLLOWCHECK_TIME;

		/// <summary>
		/// Override the classtype of any npc with a classtype of DOL.GS.GameNPC
		/// </summary>
		[ServerProperty("npc", "gamenpc_default_classtype", "Change the classtype of any npc of classtype DOL.GS.GameNPC to this.", "DOL.GS.GameNPC")]
		public static string GAMENPC_DEFAULT_CLASSTYPE;

		/// <summary>
		/// Chances for npc (including pet) to style (chance is calculated randomly according to this value + the number of style the NPC own)
		/// </summary>
		[ServerProperty("npc", "gamenpc_chances_to_style", "Change the chance to fire a style for a mob or a pet", 20)]
		public static int GAMENPC_CHANCES_TO_STYLE;

		/// <summary>
		/// Chances for npc (including pet) to cast (chance is calculated randomly according to this value + the number of spells the NPC own)
		/// </summary>
		[ServerProperty("npc", "gamenpc_chances_to_cast", "Change the chance to cast a spell for a mob or a pet", 25)]
		public static int GAMENPC_CHANCES_TO_CAST;

		#endregion

		#region PVP / RVR
		/// <summary>
		/// Grace period in minutes to allow relog near enemy structure after link death
		/// </summary>
		[ServerProperty("pvp", "RvRLinkDeathRelogGracePeriod", "The Grace Period in minutes, to allow to relog near enemy structure after a link death.", "20")]
		public static string RVR_LINK_DEATH_RELOG_GRACE_PERIOD;

		/// <summary>
		/// PvP Immunity Timer - Killed by Mobs
		/// </summary>
		[ServerProperty("pvp", "Timer_Killed_By_Mob", "Immunity Timer When player killed in PvP, in seconds", 30)] //30 seconds default
		public static int TIMER_KILLED_BY_MOB;

		/// <summary>
		/// PvP Immunity Timer - Killed by Player
		/// </summary>
		[ServerProperty("pvp", "Timer_Killed_By_Player", "Immunity Timer When player killed in PvP, in seconds", 120)] //2 min default
		public static int TIMER_KILLED_BY_PLAYER;

		/// <summary>
		/// PvP Immunity Timer - Region Changed
		/// </summary>
		[ServerProperty("pvp", "Timer_Region_Changed", "Immunity Timer when player changes regions, in seconds", 30)] //30 seconds default
		public static int TIMER_REGION_CHANGED;

		/// <summary>
		/// PvP Immunity Timer - Game Entered
		/// </summary>
		[ServerProperty("pvp", "Timer_Game_Entered", "Immunity Timer when player enters the game, in seconds", 10)] //10 seconds default
		public static int TIMER_GAME_ENTERED;

		/// <summary>
		/// PvP Immunity Timer - Teleport
		/// </summary>
		[ServerProperty("pvp", "Timer_PvP_Teleport", "Immunity Timer when player teleports within the same region, in seconds", 30)] //30 seconds default
		public static int TIMER_PVP_TELEPORT;

		/// <summary>
		/// Time after a relic lost in nature is returning to his ReturnRelicPad pad
		/// </summary>
		[ServerProperty("pvp", "Relic_Return_Time", "A lost relic will automatically returns to its defined point, in seconds", 20 * 60)] //20 mins default
		public static int RELIC_RETURN_TIME;

		/// <summary>
		/// Allow all realms access to DF
		/// </summary>
		[ServerProperty("pvp", "allow_all_realms_df", "Should we allow all realms access to DF", false)]
		public static bool ALLOW_ALL_REALMS_DF;

		/// <summary>
		/// Allow Bounty Points to be gained in Battlegrounds
		/// </summary>
		[ServerProperty("pvp", "allow_bps_in_bgs", "Allow bounty points to be gained in battlegrounds", false)]
		public static bool ALLOW_BPS_IN_BGS;

		/// <summary>
		/// This if the server battleground zones are open to players
		/// </summary>
		[ServerProperty("pvp", "bg_zones_open", "Can the players teleport to battleground", true)]
		public static bool BG_ZONES_OPENED;

		/// <summary>
		/// Message to display to player if BG zones are closed
		/// </summary>
		[ServerProperty("pvp", "bg_zones_closed_message", "Message to display to player if BG zones are closed", "The battlegrounds are not open on this server.")]
		public static string BG_ZONES_CLOSED_MESSAGE;

		/// <summary>
		/// How many players are required on the relic pad to trigger the pillar?
		/// </summary>
		[ServerProperty("pvp", "relic_players_required_on_pad", "How many players are required on the relic pad to trigger the pillar?", 16)]
		public static int RELIC_PLAYERS_REQUIRED_ON_PAD;

		/// <summary>
		/// Ignore too long outcoming packet or not
		/// </summary>
		[ServerProperty("pvp", "enable_minotaur_relics", "Shall we enable Minotaur Relics ?", false)]
		public static bool ENABLE_MINOTAUR_RELICS;

		/// <summary>
		/// Enable WarMap manager
		/// </summary>
		[ServerProperty("pvp", "enable_warmapmgr", "Shall we enable the WarMap manager ?", false)]
		public static bool ENABLE_WARMAPMGR;
		#endregion

		#region KEEPS
		/// <summary>
		/// Number of seconds between allowed LOS checks for keep guards
		/// </summary>
		[ServerProperty("keeps", "keep_guard_los_check_time", "Number of seconds between allowed LOS checks for keep guards", 5)]
		public static int KEEP_GUARD_LOS_CHECK_TIME;

		/// <summary>
		/// The number of players needed for claiming
		/// </summary>
		[ServerProperty("keeps", "claim_num", "Players Needed For Claim - Edit this to change the amount of players required to claim a keep, towers are half this amount", 8)]
		public static int CLAIM_NUM;

		/// <summary>
		/// Use Keep Balancing
		/// </summary>
		[ServerProperty("keeps", "use_keep_balancing", "Set to true if you want keeps to be higher level in NF the less you have, and lower level the more you have", false)]
		public static bool USE_KEEP_BALANCING;

		/// <summary>
		/// Use Live Keep Bonuses
		/// </summary>
		[ServerProperty("keeps", "use_live_keep_bonuses", "Set to true if you want to use the live keeps bonuses, for example 3% extra xp", false)]
		public static bool USE_LIVE_KEEP_BONUSES;

		/// <summary>
		/// Use Supply Chain
		/// </summary>
		[ServerProperty("keeps", "use_supply_chain", "Set to true if you want to use the live supply chain for keep teleporting, set to false to allow teleporting to any keep that your realm controls (and towers)", false)]
		public static bool USE_SUPPLY_CHAIN;

		/// <summary>
		/// Load Hookpoints
		/// </summary>
		[ServerProperty("keeps", "load_hookpoints", "Load keep hookpoints", true)]
		public static bool LOAD_HOOKPOINTS;

		/// <summary>
		/// Load Keeps
		/// </summary>
		[ServerProperty("keeps", "load_keeps", "Load keeps", true)]
		public static bool LOAD_KEEPS;

		/// <summary>
		/// The level keeps start at when not claimed - please note only levels 4 and 5 are supported correctly at this time
		/// </summary>
		[ServerProperty("keeps", "starting_keep_level", "The level an unclaimed keep starts at.", 4)]
		public static int STARTING_KEEP_LEVEL;

		/// <summary>
		/// The level keeps start at when claimed - please note only levels 4 and 5 are supported correctly at this time
		/// </summary>
		[ServerProperty("keeps", "starting_keep_claim_level", "The level a claimed keep starts at.", 5)]
		public static int STARTING_KEEP_CLAIM_LEVEL;

		/// <summary>
		/// The maximum keep level - please note only levels 4 and 5 are supported correctly at this time
		/// </summary>
		[ServerProperty("keeps", "max_keep_level", "The maximum keep level.", 5)]
		public static int MAX_KEEP_LEVEL;

		/// <summary>
		/// Enable the keep upgrade timer to slowly raise keep levels
		/// </summary>
		[ServerProperty("keeps", "enable_keep_upgrade_timer", "Enable the keep upgrade timer to slowly raise keep levels?", false)]
		public static bool ENABLE_KEEP_UPGRADE_TIMER;

		/// <summary>
		/// Define toughness for keep and tower walls: 100 is 100% player's damages inflicted.
		/// </summary>
		[ServerProperty("keeps", "set_structures_toughness", "This value is % of total damages inflicted to walls. (100=full damages)", 100)]
		public static int SET_STRUCTURES_TOUGHNESS;

		/// <summary>
		/// Define toughness for keep doors: 100 is 100% player's damages inflicted.
		/// </summary>
		[ServerProperty("keeps", "set_keep_door_toughness", "This value is % of total damages inflicted to level 1 door. (100=full damages)", 100)]
		public static int SET_KEEP_DOOR_TOUGHNESS;

		/// <summary>
		/// Define toughness for tower doors: 100 is 100% player's damages inflicted.
		/// </summary>
		[ServerProperty("keeps", "set_tower_door_toughness", "This value is % of total damages inflicted to level 1 door. (100=full damages)", 100)]
		public static int SET_TOWER_DOOR_TOUGHNESS;

		/// <summary>
		/// Allow player pets to attack keep walls
		/// </summary>
		[ServerProperty("keeps", "structures_allowpetattack", "Allow player pets to attack keep and tower walls?", true)]
		public static bool STRUCTURES_ALLOWPETATTACK;

		/// <summary>
		/// Allow player pets to attack keep and tower doors
		/// </summary>
		[ServerProperty("keeps", "doors_allowpetattack", "Allow player pets to attack keep and tower doors?", true)]
		public static bool DOORS_ALLOWPETATTACK;

		/// <summary>
		/// Multiplier used in determining RP reward for claiming towers.
		/// </summary>
		[ServerProperty("keeps", "tower_rp_claim_multiplier", "Integer multiplier used in determining RP reward for claiming towers.", 100)]
		public static int TOWER_RP_CLAIM_MULTIPLIER;

		/// <summary>
		/// Multiplier used in determining RP reward for keeps.
		/// </summary>
		[ServerProperty("keeps", "keep_rp_claim_multiplier", "Integer multiplier used in determining RP reward for claiming keeps.", 1000)]
		public static int KEEP_RP_CLAIM_MULTIPLIER;

		/// <summary>
		/// Turn on logging of keep captures
		/// </summary>
		[ServerProperty("keeps", "log_keep_captures", "Turn on logging of keep captures?", false)]
		public static bool LOG_KEEP_CAPTURES;

		/// <summary>
		/// Base RP value of a keep
		/// </summary>
		[ServerProperty("keeps", "keep_rp_base", "Base RP value of a keep", 4500)]
		public static int KEEP_RP_BASE;

		/// <summary>
		/// Base RP value of a tower
		/// </summary>
		[ServerProperty("keeps", "tower_rp_base", "Base RP value of a tower", 500)]
		public static int TOWER_RP_BASE;

		/// <summary>
		/// The number of seconds from last kill the this lord is worth no RP
		/// </summary>
		[ServerProperty("keeps", "lord_rp_worth_seconds", "The number of seconds from last kill the this lord is worth no RP.", 300)]
		public static int LORD_RP_WORTH_SECONDS;

		/// <summary>
		/// Multiplier used to add or subtract RP worth based on keep level difference from 50.
		/// </summary>
		[ServerProperty("keeps", "keep_rp_multiplier", "Integer multiplier used to increase/decrease RP worth based on keep level difference from 50.", 50)]
		public static int KEEP_RP_MULTIPLIER;

		/// <summary>
		/// Multiplier used to add or subtract RP worth based on tower level difference from 50.
		/// </summary>
		[ServerProperty("keeps", "tower_rp_multiplier", "Integer multiplier used to increase/decrease RP worth based on tower level difference from 50.", 50)]
		public static int TOWER_RP_MULTIPLIER;

		/// <summary>
		/// Multiplier used to add or subtract RP worth based on upgrade level > 1
		/// </summary>
		[ServerProperty("keeps", "upgrade_multiplier", "Integer multiplier used to increase/decrease RP worth based on upgrade level (0..10)", 100)]
		public static int UPGRADE_MULTIPLIER;

		/// <summary>
		/// Multiplier used to determine keep level changes when balancing.  For each keep above or below normal multiply by this.
		/// </summary>
		[ServerProperty("keeps", "keep_balance_multiplier", "Multiplier used to determine keep level changes when balancing.  For each keep above or below normal multiply by this.", 1.0)]
		public static double KEEP_BALANCE_MULTIPLIER;

		/// <summary>
		/// Multiplier used to determine keep level changes when balancing.  For each keep above or below normal multiply by this.
		/// </summary>
		[ServerProperty("keeps", "tower_balance_multiplier", "Multiplier used to determine tower level changes when balancing.  For each keep above or below normal multiply by this.", 0.15)]
		public static double TOWER_BALANCE_MULTIPLIER;

		/// <summary>
		/// Balance Towers and Keeps separately
		/// </summary>
		[ServerProperty("keeps", "balance_towers_separate", "Balance Towers and Keeps separately?", true)]
		public static bool BALANCE_TOWERS_SEPARATE;

		/// <summary>
		/// Multiplier used to determine keep guard levels.  This is applied to the bonus level (usually 4) and added after balance adjustments.
		/// </summary>
		[ServerProperty("keeps", "keep_guard_level_multiplier", "Multiplier used to determine keep guard levels.  This is applied to the bonus level (usually 4) and added after balance adjustments.", 1.6)]
		public static double KEEP_GUARD_LEVEL_MULTIPLIER;

		/// <summary>
		/// Modifier used to adjust damage for pets on keep components
		/// </summary>
		[ServerProperty("keeps", "pet_damage_multiplier", "Modifier used to adjust damage for pets classes.", 1.0)]
		public static double PET_DAMAGE_MULTIPLIER;

		/// <summary>
		/// Modifier used to adjust damage for pet spam classes (currently animist and theurgist) on keep components
		/// </summary>
		[ServerProperty("keeps", "pet_spam_damage_multiplier", "Modifier used to adjust damage for pet spam classes (currently animist and theurgist).", 1.0)]
		public static double PET_SPAM_DAMAGE_MULTIPLIER;

		/// <summary>
		/// Multiplier used to determine keep guard levels.  This is applied to the bonus level (usually 4) and added after balance adjustments.
		/// </summary>
		[ServerProperty("keeps", "tower_guard_level_multiplier", "Multiplier used to determine tower guard levels.  This is applied to the bonus level (usually 4) and added after balance adjustments.", 1.0)]
		public static double TOWER_GUARD_LEVEL_MULTIPLIER;

        /// <summary>
        /// Keeps to load. 0 for Old Keeps, 1 for new keeps, 2 for both.
        /// </summary>
        [ServerProperty("keeps", "use_new_keeps", "Appearance Keeps Components to load. 0 for Old Appearance Keeps Components, 1 for New Appearance Keeps Components. 2 is no longer used but load 0 for compatibility.", 0)]
        public static int USE_NEW_KEEPS;

		/// <summary>
		/// Should guards loaded from db be equipped by Keepsystem? (false=load equipment from db)
		/// </summary>
		[ServerProperty("keeps", "autoequip_guards_loaded_from_db", "Should guards loaded from db be equipped by Keepsystem? (false=load equipment from db)", true)]
		public static bool AUTOEQUIP_GUARDS_LOADED_FROM_DB;

		/// <summary>
		/// Should guards loaded from db be modeled by Keepsystem? (false=load from db)
		/// </summary>
		[ServerProperty("keeps", "automodel_guards_loaded_from_db", "Should guards loaded from db be modeled by Keepsystem? (false=load from db)", true)]
		public static bool AUTOMODEL_GUARDS_LOADED_FROM_DB;

		/// <summary>
		/// Are unclaimed keeps considered the enemy in PvP mode?
		/// </summary>
		[ServerProperty("keeps", "pvp_unclaimed_keeps_enemy", "Are unclaimed keeps considered the enemy in PvP mode?", false)]
		public static bool PVP_UNCLAIMED_KEEPS_ENEMY;

		/// <summary>
		/// Should players that log near enemy keeps be teleported to a safe area when logging in?
		/// </summary>
		[ServerProperty("keeps", "teleport_login_near_enemy_keep", "Should players that log near enemy keeps be teleported to a safe area?", true)]
		public static bool TELEPORT_LOGIN_NEAR_ENEMY_KEEP;

		/// <summary>
		/// Should players that exceed BG level cap be moved out of BG when logging in?
		/// </summary>
		[ServerProperty("keeps", "teleport_login_bg_level_exceeded", "Should players that exceed BG level cap be moved out of BG when logging in?", true)]
		public static bool TELEPORT_LOGIN_BG_LEVEL_EXCEEDED;

        /// <summary>
        /// Do you allowed player to climb towers?
        /// </summary>
        [ServerProperty("keeps", "allow_tower_climb", "Do you allowed player to climb towers? Set True for yes, False for not.", false)]
        public static bool ALLOW_TOWER_CLIMB;


		#endregion

		#region PVE / TOA
		/// <summary>
		/// Adjustment to missrate per number of attackers
		/// </summary>
		[ServerProperty("pve", "missrate_reduction_per_attackers", "Adjustment to missrate per number of attackers", 0)]
		public static int MISSRATE_REDUCTION_PER_ATTACKERS;

		/// <summary>
		/// Spell damage reduction multiplier based on hitchance below 55%.
		/// Default is 4.3, which will produce the minimum 1 damage at a 33% chance to hit
		/// Lower numbers reduce damage reduction
		/// </summary>
		[ServerProperty("pve", "spell_hitchance_damage_reduction_multiplier", "Spell damage reduction multiplier based on hitchance if < 55%. Lower numbers reduce damage reduction.", 4.3)]
		public static double SPELL_HITCHANCE_DAMAGE_REDUCTION_MULTIPLIER;

		/// <summary>
		/// TOA Artifact XP rate
		/// </summary>
		[ServerProperty("pve", "artifact_xp_rate", "Adjust the rate at which all artifacts gain xp.  Higher numbers mean slower XP gain. XP / this = result", 350)]
		public static int ARTIFACT_XP_RATE;

		/// <summary>
		/// TOA Scroll drop rate
		/// </summary>
		[ServerProperty("pve", "scroll_drop_rate", "Adjust the drop rate (percent chance) for scrolls.", 25)]
		public static int SCROLL_DROP_RATE;

		/// <summary>
		/// Max camp bonus
		/// </summary>
		[ServerProperty("pve", "max_camp_bonus", "Max camp bonus, 0.55 = 55%", 0.55)]
		public static double MAX_CAMP_BONUS;

		/// <summary>
		/// Minimum privilege level to be able to enter Atlantis through teleporters.
		/// </summary>
		[ServerProperty("pve", "atlantis_teleport_plvl", "Set the minimum privilege level required to enter Atlantis zones.", 2)]
		public static int ATLANTIS_TELEPORT_PLVL;
		
		/// <summary>
		/// Time Before Adventure Wings Instances Destroy when Empty
		/// </summary>
		[ServerProperty("pve", "adventurewing_time_to_destroy", "Set the time before Instanced Adventure Wings (Catacombs) are destroy when empty (in minutes).", 5)]
		public static int ADVENTUREWING_TIME_TO_DESTROY;

		/// <summary>
		/// Aurulite Loot Generator Drop Base Chance
		/// </summary>
		[ServerProperty("pve", "lootgenerator_aurulite_base_chance", "Base chance for dropping Aurulite using Loot Generator.", 10)]
		public static int LOOTGENERATOR_AURULITE_BASE_CHANCE;

		/// <summary>
		/// Aurulite Loot Generator Amount Ratio
		/// </summary>
		[ServerProperty("pve", "lootgenerator_aurulite_amount_ratio", "Modify the final count of Aurulite Loot Generator drop. (TotalCount * lootgenerator_aurulite_amount_ratio)", 0.5)]
		public static double LOOTGENERATOR_AURULITE_AMOUNT_RATIO;
		
		/// <summary>
		/// Aurulite Loot Generator Named Boost Count
		/// </summary>
		[ServerProperty("pve", "lootgenerator_aurulite_named_count", "Increase count of Aurulite Loot Generator drop for Named mobs. (count * lootgenerator_aurulite_named_count)", 1.5)]
		public static double LOOTGENERATOR_AURULITE_NAMED_COUNT;
				
		/// <summary>
		/// Atlantean Glass Loot Generator Drop Base Chance
		/// </summary>
		[ServerProperty("pve", "lootgenerator_atlanteanglass_base_chance", "Base chance for dropping Atlantean Glass using Loot Generator.", 20)]
		public static int LOOTGENERATOR_ATLANTEANGLASS_BASE_CHANCE;
		
		/// <summary>
		/// Atlantean Glass Loot Generator Named Boost Count
		/// </summary>
		[ServerProperty("pve", "lootgenerator_atlanteanglass_named_count", "Increase count of Atlantean Glass Loot Generator drop for Named mobs. (count * lootgenerator_atlanteanglass_named_count)", 1.5)]
		public static double LOOTGENERATOR_ATLANTEANGLASS_NAMED_COUNT;
		
		/// <summary>
		/// Dragon Scales Loot Generator Drop Base Chance
		/// </summary>
		[ServerProperty("pve", "lootgenerator_dragonscales_base_chance", "Base chance for dropping Dragon Scales using Loot Generator.", 15)]
		public static int LOOTGENERATOR_DRAGONSCALES_BASE_CHANCE;
		
		/// <summary>
		/// Dragon Scales Loot Generator Named Boost Count
		/// </summary>
		[ServerProperty("pve", "lootgenerator_dragonscales_named_count", "Increase count of Dragon Scales Loot Generator drop for Named mobs. (count * lootgenerator_dragonscales_named_count)", 1.5)]
		public static double LOOTGENERATOR_DRAGONSCALES_NAMED_COUNT;		
		#endregion

		#region HOUSING
		/// <summary>
		/// Maximum number of houses supported on this server.  Limits the size of the housing array used for updates
		/// </summary>
		[ServerProperty("housing", "max_num_houses", "Max number of houses supported on this server.", 5000)]
		public static int MAX_NUM_HOUSES;

		/// <summary>
		/// The starting NPCTemplate ID to use for housing NPC's
		/// </summary>
		[ServerProperty("housing", "housing_starting_npctemplate_id", "The starting NPCTemplate ID to use for housing NPC's", 500)]
		public static int HOUSING_STARTING_NPCTEMPLATE_ID;

		/// <summary>
		/// Sets the max allowed items inside a house.
		/// </summary>
		[ServerProperty("housing", "max_indoor_house_items", "Max number of items allowed inside a players house.", 40)]
		public static int MAX_INDOOR_HOUSE_ITEMS;

		/// <summary>
		/// Max outdoor items.  If Outdoor is increased past 30 they vanish. It seems to be hardcoded in client
		/// </summary>
		[ServerProperty("housing", "max_outdoor_house_items", "Max number of items allowed in a players garden.", 30)]
		public static int MAX_OUTDOOR_HOUSE_ITEMS;

		[ServerProperty("housing", "indoor_items_depend_on_size", "If true the max number of allowed House indoor items are set like live (40, 60, 80, 100)", true)]
		public static bool INDOOR_ITEMS_DEPEND_ON_SIZE;

		[ServerProperty("housing", "housing_rent_cottage", "Rent price for a cottage.", 20L * 100L * 100L)] // 20g
		public static long HOUSING_RENT_COTTAGE;

		[ServerProperty("housing", "housing_rent_house", "Rent price for a house.", 35L * 100L * 100L)] // 35g
		public static long HOUSING_RENT_HOUSE;

		[ServerProperty("housing", "housing_rent_villa", "Rent price for a villa.", 60L * 100L * 100L)] // 60g
		public static long HOUSING_RENT_VILLA;

		[ServerProperty("housing", "housing_rent_mansion", "Rent price for a mansion.", 100L * 100L * 100L)] // 100g
		public static long HOUSING_RENT_MANSION;

		[ServerProperty("housing", "housing_lot_price_start", "Starting lot price before per hour reductions", 95L * 1000L * 100L * 100L)] // 95p
		public static long HOUSING_LOT_PRICE_START;

		[ServerProperty("housing", "housing_lot_price_per_hour", "Lot price reduction per hour.", (long)(1.2 * 1000 * 100 * 100))] // 1.2p
		public static long HOUSING_LOT_PRICE_PER_HOUR;

		[ServerProperty("housing", "housing_lot_price_minimum", "Minimum lot price.", 300L * 100L * 100L)] // 300g
		public static long HOUSING_LOT_PRICE_MINIMUM;

		/// <summary>
		/// How often, in days, is rent due?  0 for never, negative for testing repossession
		/// </summary>
		[ServerProperty("housing", "rent_due_days", "How often, in days, is rent due?  0 for never, negative for testing repossession.", 7)]
		public static int RENT_DUE_DAYS;

		/// <summary>
		/// How often, in minutes, do we check for rent?
		/// </summary>
		[ServerProperty("housing", "rent_check_interval", "How often, in minutes, do we check for rent?", 120)]
		public static int RENT_CHECK_INTERVAL;

		/// <summary>
		/// How many rent payments can be stored in the lockbox?
		/// </summary>
		[ServerProperty("housing", "rent_lockbox_payments", "How many rent payments can be stored in the lockbox?", 4)]
		public static int RENT_LOCKBOX_PAYMENTS;

		/// <summary>
		/// The worth of 1 (one) bounty point in gold (e.g. 1 bp = 1g -> 10000, 1bp = 10g -> 100000)
		/// </summary>
		[ServerProperty("housing", "rent_bounty_point_to_gold", "The worth of 1 (one) bounty point in gold (e.g. 1 bp = 1g -> 10000, 1bp = 10g -> 100000)", 10000)]
		public static long RENT_BOUNTY_POINT_TO_GOLD;

		/// <summary>
		/// Do housing consignment merchants use BP instead of money?
		/// </summary>
		[ServerProperty("housing", "consignment_use_bp", "If true the housing consignment merchants use BP instead of money.", false)]
		public static bool CONSIGNMENT_USE_BP;

		/// <summary>
		/// Enable consignment merchants and market cache
		/// </summary>
		[ServerProperty("housing", "market_enabled", "If true the market explorers are enabled and the cache is initialized on server start.", true)]
		public static bool MARKET_ENABLED;

		/// <summary>
		/// Enable consignment merchants and market cache
		/// </summary>
		[ServerProperty("housing", "market_enable", "If true the market explorers are enabled and the cache is initialized on server start.", true)]
		public static bool MARKET_ENABLE;

		/// <summary>
		/// Enable logging of all market activity
		/// </summary>
		[ServerProperty("housing", "market_enable_log", "Enable debug logging of all market activity", true)]
		public static bool MARKET_ENABLE_LOG;

		/// <summary>
		/// What is the additional fee (%) charged to players using the market explorer?
		/// </summary>
		[ServerProperty("housing", "market_fee_percent", "What is the additional fee (%) charged to players using the market explorer?", 20)]
		public static int MARKET_FEE_PERCENT;

		/// <summary>
		/// How many items can the market search return?
		/// </summary>
		[ServerProperty("housing", "market_search_limit", "How many items can the market search return?", 300)]
		public static int MARKET_SEARCH_LIMIT;

		#endregion

		#region CLASSES

		/// <summary>
		/// Disable some classes from being created
		/// </summary>
		[ServerProperty("classes", "disabled_classes", "Serialized list of disabled classes, separated by semi-colon or a range with a dash (ie 1-5;7;9)", "")]
		public static string DISABLED_CLASSES;

		/// <summary>
		/// Disable some races from being created
		/// </summary>
		[ServerProperty("classes", "disabled_races", "Serialized list of disabled races, separated by semi-colon or a range with a dash (ie 1-5;7;9)", "")]
		public static string DISABLED_RACES;

		/// <summary>
		/// Days before your elligable for a free level in Albion
		/// </summary>
		[ServerProperty("classes", "freelevel_days_albion", "days before your elligable for a free level in Albion, use -1 to deactivate", 7)]
		public static int FREELEVEL_DAYS_ALBION;
		
		/// <summary>
		/// Days before your elligable for a free level in Midgard
		/// </summary>
		[ServerProperty("classes", "freelevel_days_midgard", "days before your elligable for a free level in Midgard, use -1 to deactivate", 7)]
		public static int FREELEVEL_DAYS_MIDGARD;

		/// <summary>
		/// Days before your elligable for a free level in Hibernia
		/// </summary>
		[ServerProperty("classes", "freelevel_days_hibernia", "days before your elligable for a free level in Hibernia, use -1 to deactivate", 7)]
		public static int FREELEVEL_DAYS_HIBERNIA;

		/// <summary>
		/// Buff Range, 0 for unlimited
		/// </summary>
		[ServerProperty("classes", "buff_range", "The range that concentration buffs can last from the owner before it expires.  0 for unlimited.", 0)]
		public static int BUFF_RANGE;

		/// <summary>
		/// Allow Cata Slash Level
		/// </summary>
		[ServerProperty("classes", "allow_cata_slash_level", "Allow catacombs classes to use /level command", false)]
		public static bool ALLOW_CATA_SLASH_LEVEL;

		/// <summary>
		/// Sets the Cap for Player Turrets
		/// </summary>
		[ServerProperty("classes", "turret_player_cap_count", "Sets the cap of turrets for a Player", 5)]
		public static int TURRET_PLAYER_CAP_COUNT;

		/// <summary>
		/// Sets the Area Cap for Turrets
		/// </summary>
		[ServerProperty("classes", "turret_area_cap_count", "Sets the cap of the Area for turrets", 10)]
		public static int TURRET_AREA_CAP_COUNT;

		/// <summary>
		/// Sets the Circle of the Area to check for Turrets
		/// </summary>
		[ServerProperty("classes", "turret_area_cap_radius", "Sets the Radius which is checked for the turretareacap", 1000)]
		public static int TURRET_AREA_CAP_RADIUS;

		[ServerProperty("classes", "theurgist_pet_cap", "Sets the maximum number of pets a Theurgist can summon", 16)]
		public static int THEURGIST_PET_CAP;

		/// <summary>
		/// Do we want to allow items to be equipped regardless of realm?
		/// </summary>
		[ServerProperty("classes", "allow_cross_realm_items", "Do we want to allow items to be equipped regardless of realm?", false)]
		public static bool ALLOW_CROSS_REALM_ITEMS;

		/// <summary>
		/// What level should /level bring you to? 0 to disable
		/// </summary>
		[ServerProperty("classes", "slash_level_target", "What level should /level bring you to? 0 is disabled.", 0)]
		public static int SLASH_LEVEL_TARGET;

		/// <summary>
		/// What level should you have on your account to be able to use /level?
		/// </summary>
		[ServerProperty("classes", "slash_level_requirement", "What level should you have on your account be able to use /level?", 50)]
		public static int SLASH_LEVEL_REQUIREMENT;

		/// <summary>
		/// Should the server start characters as Base Class?
		/// </summary>
		[ServerProperty("classes", "allow_old_archery", "Should we allow archers to be able to use arrows from their quiver?", false)]
		public static bool ALLOW_OLD_ARCHERY;


		#endregion

		#region SPELLS

		/// <summary>
		/// Spells-related properties
		/// </summary>
		[ServerProperty("spells", "spell_interrupt_duration", "", 4500)]
		public static int SPELL_INTERRUPT_DURATION;

		[ServerProperty("spells", "spell_interrupt_recast", "", 2000)]
		public static int SPELL_INTERRUPT_RECAST;

		[ServerProperty("spells", "spell_interrupt_again", "", 100)]
		public static int SPELL_INTERRUPT_AGAIN;

		[ServerProperty("spells", "spell_interrupt_maxstagelength", "Max length of stage 1 and 3, 1000 = 1 second", 1500)]
		public static int SPELL_INTERRUPT_MAXSTAGELENGTH;
		
		[ServerProperty("spells", "spell_charm_named_check", "Prevents charm spell to work on Named Mobs, 0 = disable, 1 = enable", 1)]
		public static int SPELL_CHARM_NAMED_CHECK;

		#endregion

		#region GUILDS / ALLIANCES
		/// <summary>
		/// The max number of guilds in an alliance
		/// </summary>
		[ServerProperty("guild", "alliance_max", "Max Guilds In Alliance - Edit this to change the maximum number of guilds in an alliance -1 = unlimited, 0=disable alliances", -1)]
		public static int ALLIANCE_MAX;

		/// <summary>
		/// The number of players needed to form a guild
		/// </summary>
		[ServerProperty("guild", "guild_num", "Players Needed For Guild Form - Edit this to change the amount of players required to form a guild", 8)]
		public static int GUILD_NUM;

		/// <summary>
		/// This enables or disables new guild dues. Live standard is 2% dues
		/// </summary>
		[ServerProperty("guild", "new_guild_dues", "Guild dues can be set from 1-100% if enabled, or standard 2% if not", false)]
		public static bool NEW_GUILD_DUES;

		/// <summary>
		/// Do we allow guild members from other realms
		/// </summary>
		[ServerProperty("guild", "allow_cross_realm_guilds", "Do we allow guild members from other realms?", false)]
		public static bool ALLOW_CROSS_REALM_GUILDS;

		/// <summary>
		/// How many things do we allow guilds to claim?
		/// </summary>
		[ServerProperty("guild", "guilds_claim_limit", "How many things do we allow guilds to claim?", 1)]
		public static int GUILDS_CLAIM_LIMIT;

		/// <summary>
		/// Guild Crafting Buff bonus amount
		/// </summary>
		[ServerProperty("guild", "guild_buff_crafting", "Percent speed gain for the guild crafting buff?", (ushort)5)]
		public static ushort GUILD_BUFF_CRAFTING;

		/// <summary>
		/// Guild XP Buff bonus amount
		/// </summary>
		[ServerProperty("guild", "guild_buff_xp", "Extra XP gain percent for the guild PvE XP buff?", (ushort)5)]
		public static ushort GUILD_BUFF_XP;

		/// <summary>
		/// Guild RP Buff bonus amount
		/// </summary>
		[ServerProperty("guild", "guild_buff_rp", "Extra RP gain percent for the guild RP buff?", (ushort)2)]
		public static ushort GUILD_BUFF_RP;

		/// <summary>
		/// Guild BP Buff bonus amount -  this is not available on live, disabled by default
		/// </summary>
		[ServerProperty("guild", "guild_buff_bp", "Extra BP gain percent for the guild BP buff?", (ushort)0)]
		public static ushort GUILD_BUFF_BP;

		/// <summary>
		/// Guild artifact XP Buff bonus amount
		/// </summary>
		[ServerProperty("guild", "guild_buff_artifact_xp", "Extra artifact XP gain percent for the guild artifact XP buff?", (ushort)5)]
		public static ushort GUILD_BUFF_ARTIFACT_XP;

		/// <summary>
		/// Guild masterlevel XP Buff bonus amount
		/// </summary>
		[ServerProperty("guild", "guild_buff_masterlevel_xp", "Extra masterlevel XP gain percent for the guild masterlevel XP buff?", (ushort)20)]
		public static ushort GUILD_BUFF_MASTERLEVEL_XP;

		/// <summary>
		/// How much merit to reward guild when dragon is killed, if any.
		/// </summary>
		[ServerProperty("guild", "guild_merit_on_dragon_kill", "How much merit to reward guild when dragon is killed, if any.", (ushort)0)]
		public static ushort GUILD_MERIT_ON_DRAGON_KILL;

		/// <summary>
		/// When a banner is lost to the enemy how long is the wait before purchase is allowed?  In Minutes.
		/// </summary>
		[ServerProperty("guild", "guild_banner_lost_time", "When a banner is lost to the enemy how many minutes is the wait before purchase is allowed?", (ushort)1440)]
		public static ushort GUILD_BANNER_LOST_TIME;



		#endregion

		#region CRAFT / SALVAGE
		/// <summary>
		/// The crafting speed modifier
		/// </summary>
		[ServerProperty("craft", "crafting_speed", "Crafting Speed Modifier - Edit this to change the speed at which you craft e.g 1.5 is 50% faster 2.0 is twice as fast (100%) 0.5 is half the speed (50%)", 1.0)]
		public static double CRAFTING_SPEED;

		/// <summary>
		/// Crafting skill gain bonus in capital cities
		/// </summary>
		[ServerProperty("craft", "capital_city_crafting_skill_gain_bonus", "Crafting skill gain bonus % in capital cities; 5 = 5%", 5)]
		public static int CAPITAL_CITY_CRAFTING_SKILL_GAIN_BONUS;

		/// <summary>
		/// Crafting speed bonus in capital cities
		/// </summary>
		[ServerProperty("craft", "capital_city_crafting_speed_bonus", "Crafting speed bonus in capital cities; 2 = 2x, 3 = 3x, ..., 1 = standard", 1.0)]
		public static double CAPITAL_CITY_CRAFTING_SPEED_BONUS;

		/// <summary>
		/// Allow any realm to craft items with a realm of 0 (no realm)
		/// </summary>
		[ServerProperty("craft", "allow_craft_norealm_items", "Allow any realm to craft items with 0 (no) realm.", false)]
		public static bool ALLOW_CRAFT_NOREALM_ITEMS;

		/// <summary>
		/// Use salvage per realm and get back material to use in chars realm
		/// </summary>
		[ServerProperty("salvage", "use_salvage_per_realm", "Enable to get back material to use in chars realm. Disable to get back the same material in all realms.", false)]
		public static bool USE_SALVAGE_PER_REALM;

		/// <summary>
		/// Use salvage per realm and get back material to use in chars realm
		/// </summary>
		[ServerProperty("salvage", "use_new_salvage", "Enable to use a new system calcul of salvage count based on object_type.", false)]
		public static bool USE_NEW_SALVAGE;

		#endregion

		#region ACCOUNT
		/// <summary>
		/// Allow auto-account creation  This is also set in serverconfig.xml and must be enabled for this property to work.
		/// </summary>
		[ServerProperty("account", "allow_auto_account_creation", "Allow auto-account creation  This is also set in serverconfig.xml and must be enabled for this property to work.", true)]
		public static bool ALLOW_AUTO_ACCOUNT_CREATION;

		/// <summary>
		/// Account bombing prevention
		/// </summary>
		[ServerProperty("account", "time_between_account_creation", "The time in minutes between 2 accounts creation. This avoid account bombing with dynamic ip. 0 to disable", 0)]
		public static int TIME_BETWEEN_ACCOUNT_CREATION;

		/// <summary>
		/// Account IP bombing prevention
		/// </summary>
		[ServerProperty("account", "time_between_account_creation_sameip", "The time in minutes between accounts creation from the same ip after 2 creations.", 15)]
		public static int TIME_BETWEEN_ACCOUNT_CREATION_SAMEIP;

		/// <summary>
		/// Total number of account allowed for the same IP
		/// </summary>
		[ServerProperty("account", "total_accounts_allowed_sameip", "Total number of account allowed for the same IP", 20)]
		public static int TOTAL_ACCOUNTS_ALLOWED_SAMEIP;

		/// <summary>
		/// Should we backup deleted characters and not delete associated content?
		/// </summary>
		[ServerProperty("account", "backup_deleted_characters", "Should we backup deleted characters and not delete associated content?", true)]
		public static bool BACKUP_DELETED_CHARACTERS;


		#endregion

		public static IDictionary<string, object> AllCurrentProperties
		{
			get; private set;
		}
		
		/// <summary>
		/// Get a Dictionary that tracks all Properties by Key String
		/// Returns the ServerPropertyAttribute, the Static Field with current Value, and the according DataObject
		/// Create a default dataObject if value wasn't found in Database
		/// </summary>
		public static IDictionary<string, Tuple<ServerPropertyAttribute, FieldInfo, ServerProperty>> AllDomainProperties
		{
			get
			{
				var result = new Dictionary<string, Tuple<ServerPropertyAttribute, FieldInfo, ServerProperty>>();
				var allProperties = GameServer.Database.SelectAllObjects<ServerProperty>();
				
				foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
				{
					foreach (Type type in asm.GetTypes())
					{
						foreach (FieldInfo field in type.GetFields())
						{
							// Properties are Static
							if (!field.IsStatic)
								continue;
							
							// Properties shoud contain a property attribute
							object[] attribs = field.GetCustomAttributes(typeof(ServerPropertyAttribute), false);
							if (attribs.Length == 0)
								continue;
							
							ServerPropertyAttribute att = (ServerPropertyAttribute)attribs[0];
							
							ServerProperty serverProp = allProperties.Where(p => p.Key.Equals(att.Key, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
							
							if (serverProp == null)
							{
								// Init DB Object
								serverProp = new ServerProperty();
								serverProp.Category = att.Category;
								serverProp.Key = att.Key;
								serverProp.Description = att.Description;
								if (att.DefaultValue is double)
								{
									CultureInfo myCIintl = new CultureInfo("en-US", false);
									IFormatProvider provider = myCIintl.NumberFormat;
									serverProp.DefaultValue = ((double)att.DefaultValue).ToString(provider);
								}
								else
								{
									serverProp.DefaultValue = att.DefaultValue.ToString();
								}
								serverProp.Value = serverProp.DefaultValue;
							}
							
							result[att.Key] = new Tuple<ServerPropertyAttribute, FieldInfo, ServerProperty>(att, field, serverProp);
						}
					}
				}

				return result;
			}
		}
		
		/// <summary>
		/// This method loads the property from the database and returns
		/// the value of the property as strongly typed object based on the
		/// type of the default value
		/// </summary>
		/// <param name="attrib">The attribute</param>
		/// <returns>The real property value</returns>
		public static void Load(ServerPropertyAttribute attrib, FieldInfo field, ServerProperty prop)
		{
			string key = attrib.Key;
			
			// Not Added to database...
			if (!prop.IsPersisted)
			{
				GameServer.Database.AddObject(prop);
				log.DebugFormat("Cannot find server property {0} creating it", key);
			}
			
			log.DebugFormat("Loading {0} Value is {1}", key, prop.Value);
			
			try
			{
				if (field.IsInitOnly)
					log.WarnFormat("Property {0} is ReadOnly, Value won't be changed - {1} !", key, field.GetValue(null));
				
				//we do this because we need "1.0" to be considered double sometimes its "1,0" in other countries
				CultureInfo myCIintl = new CultureInfo("en-US", false);
				IFormatProvider provider = myCIintl.NumberFormat;
				field.SetValue(null, Convert.ChangeType(prop.Value, attrib.DefaultValue.GetType(), provider));
			}
			catch (Exception e)
			{
				log.ErrorFormat("Exception in ServerProperties Load: {0}", e);
				log.ErrorFormat("Trying to load {0} value is {1}", key, prop.Value);
			}
		}
		
		/// <summary>
		/// Refreshes the server properties from the DB
		/// </summary>
		public static void Refresh()
		{
			log.Info("Refreshing server properties...");
			InitProperties();
		}
	}
}