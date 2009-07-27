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
		public static double XP_RATE;

		/// <summary>
		/// RvR Zones XP Rate
		/// </summary>
		[ServerProperty("rvr_zones_xp_rate", "The RvR zones Experience Points Rate Modifier", 1.0)]
		public static double RvR_XP_RATE;

		/// <summary>
		/// The Realm Points Rate
		/// </summary>
		[ServerProperty("rp_rate", "The Realm Points Rate Modifier - Edit this to change the rate at which you gain realm points e.g 1.5 is 50% more 2.0 is twice the amount (100%) 0.5 is half the amount (50%)", 1.0)]
		public static double RP_RATE;

		/// <summary>
		/// The Bounty Points Rate
		/// </summary>
		[ServerProperty("bp_rate", "The Bounty Points Rate Modifier - Edit this to change the rate at which you gain bounty points e.g 1.5 is 50% more 2.0 is twice the amount (100%) 0.5 is half the amount (50%)", 1.0)]
		public static double BP_RATE;

		/// <summary>
		/// The Server Message of the Day
		/// </summary>
		[ServerProperty("motd", "The Server Message of the Day - Edit this to set what is displayed when a level 2+ character enters the game for the first time, set to \"\" for nothing", "Welcome to a Dawn of Light server, please edit this MOTD")]
		public static string MOTD;

		/// <summary>
		/// The damage players do against monsters
		/// </summary>
		[ServerProperty("pve_damage", "The PvE Damage Modifier - Edit this to change the amount of damage done when fighting mobs e.g 1.5 is 50% more damage 2.0 is twice the damage (100%) 0.5 is half the damage (50%)", 1.0)]
		public static double PVE_DAMAGE;

		/// <summary>
		/// The percent per con difference (-1 = blue, 0 = yellow, 1 = OJ, 2 = red ...) subtracted to hitchance for spells in PVE.  0 is none, 5 is 5% per con, etc.  Default is 10%
		/// </summary>
		[ServerProperty("pve_spell_conhitpercent", "The percent per con (1 = OJ, 2 = red ...) subtracted to hitchance for spells in PVE  Must be >= 0.  0 is none, 5 is 5% per level, etc.  Default is 10%", (uint)10)]
		public static uint PVE_SPELL_CONHITPERCENT;

		/// <summary>
		/// The damage players do against players
		/// </summary>
		[ServerProperty("pvp_damage", "The PvP Damage Modifier - Edit this to change the amountof damage done when fighting players e.g 1.5 is 50% more damage 2.0 is twice the damage (100%) 0.5 is half the damage (50%)", 1.0)]
		public static double PVP_DAMAGE;

		/// <summary>
		/// The message players get when they enter the game past level 1
		/// </summary>
		[ServerProperty("starting_msg", "The Starting Mesage - Edit this to set what is displayed when a level 1 character enters the game for the first time, set to \"\" for nothing", "Welcome for your first time to a Dawn of Light server, please edit this Starter Message")]
		public static string STARTING_MSG;

		/// <summary>
		/// The amount of copper a player starts with
		/// </summary>
		[ServerProperty("starting_money", "Starting Money - Edit this to change the amount in copper of money new characters start the game with, max 214 plat", 0)]
		public static long STARTING_MONEY;

		/// <summary>
		/// The level of experience a player should start with
		/// </summary>
		[ServerProperty("starting_level", "Starting Level - Edit this to set which levels experience a new player start the game with", 1)]
		public static int STARTING_LEVEL;

		/// <summary>
		/// The message players get when they enter the game at level 1
		/// </summary>
		[ServerProperty("starting_realm_level", "Starting Realm level - Edit this to set which realm level a new player starts the game with", 0)]
		public static int STARTING_REALM_LEVEL;

		/// <summary>
		/// The a starting guild should be used
		/// </summary>
		[ServerProperty("starting_guild", "Starter Guild - Edit this to change the starter guild options, values True,False", true)]
		public static bool STARTING_GUILD;

		/// <summary>
		/// The crafting speed modifier
		/// </summary>
		[ServerProperty("crafting_speed", "Crafting Speed Modifier - Edit this to change the speed at which you craft e.g 1.5 is 50% faster 2.0 is twice as fast (100%) 0.5 is half the speed (50%)", 1.0)]
		public static double CRAFTING_SPEED;

		/// <summary>
		/// The money drop modifier
		/// </summary>
		[ServerProperty("money_drop", "Money Drop Modifier - Edit this to change the amount of money which is dropped e.g 1.5 is 50% more 2.0 is twice the amount (100%) 0.5 is half the amount (50%)", 1.0)]
		public static double MONEY_DROP;

		/// <summary>
		/// The broadcast type
		/// </summary>
		[ServerProperty("broadcast_type", "Broadcast Type - Edit this to change what /b does, values 0 = disabled, 1 = area, 2 = visibility distance, 3 = zone, 4 = region, 5 = realm, 6 = server", 1)]
		public static int BROADCAST_TYPE;

		/// <summary>
		/// The max number of guilds in an alliance
		/// </summary>
		[ServerProperty("alliance_max", "Max Guilds In Alliance - Edit this to change the maximum number of guilds in an alliance -1 = unlimited, 0=disable alliances", -1)]
		public static int ALLIANCE_MAX;

		/// <summary>
		/// The number of players needed for claiming
		/// </summary>
		[ServerProperty("claim_num", "Players Needed For Claim - Edit this to change the amount of players required to claim a keep, towers are half this amount", 8)]
		public static int CLAIM_NUM;

		/// <summary>
		/// The number of players needed to form a guild
		/// </summary>
		[ServerProperty("guild_num", "Players Needed For Guild Form - Edit this to change the amount of players required to form a guild", 8)]
		public static readonly int GUILD_NUM;

		/// <summary>
		/// If the server should only accept connections from staff
		/// </summary>
		[ServerProperty("staff_login", "Staff Login Only - Edit this to set weather you wish staff to be the only ones allowed to log in values True,False", false)]
		public static bool STAFF_LOGIN;

		/// <summary>
		/// The max number of players on the server
		/// </summary>
		[ServerProperty("max_players", "Max Players - Edit this to set the maximum players allowed to connect at the same time set 0 for unlimited", 0)]
		public static int MAX_PLAYERS;

		/// <summary>
		/// The time until a player is worth rps again after death
		/// </summary>
		[ServerProperty("rp_worth_seconds", "Realm Points Worth Seconds - Edit this to change how many seconds until a player is worth RPs again after being killed ", 300)]
		public static readonly int RP_WORTH_SECONDS;

		/// <summary>
		/// The minimum client version required to connect
		/// </summary>
		[ServerProperty("client_version_min", "Minimum Client Version - Edit this to change which client version at the least have to be used: -1 = any, 1.80 = 180", -1)]
		public static int CLIENT_VERSION_MIN;

		/// <summary>
		/// The maximum client version required to connect
		/// </summary>
		[ServerProperty("client_version_max", "Maximum Client Version - Edit this to change which client version at the most have to be used: -1 = any, 1.80 = 180", -1)]
		public static int CLIENT_VERSION_MAX;

		/// <summary>
		/// Should the server load quests
		/// </summary>
		[ServerProperty("load_quests", "Should the server load quests, values True,False", true)]
		public static readonly bool LOAD_QUESTS;

		/// <summary>
		/// Should the server log trades
		/// </summary>
		[ServerProperty("log_trades", "Should the server log all trades a player makes, values True,False", false)]
		public static bool LOG_TRADES;

		/// <summary>
		/// What class should the server use for players
		/// </summary>
		[ServerProperty("player_class", "What class should the server use for players", "DOL.GS.GamePlayer")]
		public static readonly string PLAYER_CLASS;

		/// <summary>
		/// What is the maximum client type allowed to connect
		/// </summary>
		[ServerProperty("client_type_max", "What is the maximum client type allowed to connect", -1)]
		public static int CLIENT_TYPE_MAX;

		/// <summary>
		/// Disable minotaurs from being created
		/// </summary>
		[ServerProperty("disable_minotaurs", "Disable minotaurs from being created", false)]
		public static bool DISABLE_MINOTAURS;
		
		/// <summary>
		/// Disable si race from being created
		/// </summary>
		[ServerProperty("disable_si_race", "Disable si race from being created", false)]
		public static bool DISABLE_SI_RACE;
		
		/// <summary>
		/// Disable toa race from being created
		/// </summary>
		[ServerProperty("disable_toa_race", "Disable toa race from being created", false)]
		public static bool DISABLE_TOA_RACE;

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
		/// A serialised list of RegionIDs that will load objects
		/// </summary>
		[ServerProperty("debug_load_regions", "a serialised list of region IDs that will load objects, separated by ; (leave this blank to load all regions normally)", "")]
		public static readonly string DEBUG_LOAD_REGIONS;

		/// <summary>
		/// A serialised list of disabled expansion IDs
		/// </summary>
		[ServerProperty("disabled_expansions", "a serialised list of disabled expansions IDs, expansion IDs are client type seperated by ;", "")]
		public static readonly string DISABLED_EXPANSIONS;

		/// <summary>
		/// Should the server disable the tutorial zone
		/// </summary>
		[ServerProperty("disable_tutorial", "should the server disable the tutorial zone", false)]
		public static bool DISABLE_TUTORIAL;

		/// <summary>
		/// Should users be able to create characters in all realms using the same account
		/// </summary>
		[ServerProperty("allow_all_realms", "should we allow characters to be created on all realms using a single account", false)]
		public static bool ALLOW_ALL_REALMS;

		/// <summary>
		/// Should users be allowed to create catacombs classes
		/// </summary>
		[ServerProperty("disable_catacombs_classes", "should we disable catacombs classes", false)]
		public static bool DISABLE_CATACOMBS_CLASSES;

		/// <summary>
		/// Days before your elligable for a free level
		/// </summary>
		[ServerProperty("freelevel_days", "days before your elligable for a free level, use -1 to deactivate", 7)]
		public static int FREELEVEL_DAYS;

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
		public static int STATSAVE_INTERVAL;

		/// <summary>
		/// Anon Modifier
		/// </summary>
		[ServerProperty("anon_modifier", "Various modifying options for anon, 0 = default, 1 = /who shows player but as ANON, -1 = disabled", 0)]
		public static int ANON_MODIFIER;

		/// <summary>
		/// Buff Range, 0 for unlimited
		/// </summary>
		[ServerProperty("buff_range", "The range that concentration buffs can last from the owner before it expires.  0 for unlimited.", 0)]
		public static int BUFF_RANGE;

		/// <summary>
		/// Disable Bug Reports
		/// </summary>
		[ServerProperty("disable_bug_reports", "Set to true to disable bug reporting, and false to enable bug reporting", true)]
		public static bool DISABLE_BUG_REPORTS;

		/// <summary>
		/// Use Custom Start Locations
		/// </summary>
		[ServerProperty("use_custom_start_locations", "Set to true if you will use another script to set your start locations", false)]
		public static readonly bool USE_CUSTOM_START_LOCATIONS;

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
		/// Death Messages All Realms
		/// </summary>
		[ServerProperty("death_messages_all_realms", "Set to true if you want all realms to see other realms death and kill messages", false)]
		public static bool DEATH_MESSAGES_ALL_REALMS;

		/// <summary>
		/// Log Email Addresses
		/// </summary>
		[ServerProperty("log_email_addresses", "set to the email addresses you want logs automatically emailed to, multiple addresses seperate with ;", "")]
		public static readonly string LOG_EMAIL_ADDRESSES;

		/// <summary>
		/// Bug Report Email Addresses
		/// </summary>
		[ServerProperty("bug_report_email_addresses", "set to the email addresses you want bug reports sent to (bug reports will only send if the user has set an email address for his account, multiple addresses seperate with ;", "")]
		public static readonly string BUG_REPORT_EMAIL_ADDRESSES;

		/// <summary>
		/// Allow Cata Slash Level
		/// </summary>
		[ServerProperty("allow_cata_slash_level", "Allow catacombs classes to use /level command", false)]
		public static bool ALLOW_CATA_SLASH_LEVEL;

		/// <summary>
		/// Allow Roam
		/// </summary>
		[ServerProperty("allow_roam", "Allow mobs to roam on the server", true)]
		public static bool ALLOW_ROAM;

		/// <summary>
		/// Allow Maulers
		/// </summary>
		[ServerProperty("allow_maulers", "Allow maulers to be created on the server", false)]
		public static readonly bool ALLOW_MAULERS;

		/// <summary>
		/// Log All GM commands
		/// </summary>
		[ServerProperty("log_all_gm_commands", "Log all GM commands on the server", false)]
		public static readonly bool LOG_ALL_GM_COMMANDS;

		/// <summary>
		/// Ban Hackers
		/// </summary>
		[ServerProperty("ban_hackers", "Should we ban hackers, if set to true, bans will be done, if set to false, kicks will be done", false)]
		public static readonly bool BAN_HACKERS;

		/// <summary>
		/// Allow all realms access to DF
		/// </summary>
		[ServerProperty("allow_all_realms_df", "Should we allow all realms access to DF", false)]
		public static readonly bool ALLOW_ALL_REALMS_DF;

		/// <summary>
		/// Is the database translated
		/// </summary>
		[ServerProperty("db_language", "What language is the DB", "EN")]
		public static readonly string DB_LANGUAGE;

		/// <summary>
		/// Max camp bonus
		/// </summary>
		[ServerProperty("max_camp_bonus", "Max camp bonus", 2.0)]
		public static double MAX_CAMP_BONUS;

		/// <summary>
		/// Disable Instances
		/// </summary>
		[ServerProperty("disable_instances", "Enable or disable instances on the server", false)]
		public static readonly bool DISABLE_INSTANCES;

		/// <summary>
		/// Health Regen Rate
		/// </summary>
		[ServerProperty("health_regen_rate", "Health regen rate", 1.0)]
		public static double HEALTH_REGEN_RATE;

		/// <summary>
		/// Health Regen Rate
		/// </summary>
		[ServerProperty("endurance_regen_rate", "Endurance regen rate", 1.0)]
		public static double ENDURANCE_REGEN_RATE;

		/// <summary>
		/// Health Regen Rate
		/// </summary>
		[ServerProperty("mana_regen_rate", "Mana regen rate", 1.0)]
		public static double MANA_REGEN_RATE;

		/// <summary>
		/// Load Hookpoints
		/// </summary>
		[ServerProperty("load_hookpoints", "Load keep hookpoints", true)]
		public static readonly bool LOAD_HOOKPOINTS;

		/// <summary>
		/// Load Keeps
		/// </summary>
		[ServerProperty("load_keeps", "Load keeps", true)]
		public static readonly bool LOAD_KEEPS;


		/// <summary>
		/// Save QuestItems into Database
		/// </summary>
		[ServerProperty("save_questitems_into_database", "set false if you don't want this", true)]
		public static bool SAVE_QUESTITEMS_INTO_DATABASE;

		/// <summary>
		/// Crafting skill gain bonus in capital cities
		/// </summary>
		[ServerProperty("capital_city_crafting_skill_gain_bonus", "Crafting skill gain bonus % in capital cities; 5 = 5%", 5)]
		public static int CAPITAL_CITY_CRAFTING_SKILL_GAIN_BONUS;

		/// <summary>
		/// Crafting speed bonus in capital cities
		/// </summary>
		[ServerProperty("capital_city_crafting_speed_bonus", "Crafting speed bonus in capital cities; 2 = 2x, 3 = 3x, ..., 1 = standard", 1.0)]
		public static double CAPITAL_CITY_CRAFTING_SPEED_BONUS;

		/// <summary>
		/// Allow Bounty Points to be gained in Battlegrounds
		/// </summary>
		[ServerProperty("allow_bps_in_bgs", "Allow bounty points to be gained in battlegrounds", false)]
		public static bool ALLOW_BPS_IN_BGS;

        /// <summary>
        /// Sets the Cap for Player Turrets
        /// </summary>
        [ServerProperty("turret_player_cap_count", "Sets the cap of turrets for a Player", 5)]
        public static int TURRET_PLAYER_CAP_COUNT;

        /// <summary>
        /// Sets the Area Cap for Turrets
        /// </summary>
        [ServerProperty("turret_area_cap_count", "Sets the cap of the Area for turrets", 10)]
        public static int TURRET_AREA_CAP_COUNT;

        /// <summary>
        /// Sets the Circle of the Area to check for Turrets
        /// </summary>
        [ServerProperty("turret_area_cap_radius", "Sets the Radius which is checked for the turretareacap", 1000)]
        public static int TURRET_AREA_CAP_RADIUS;

		[ServerProperty("theurgist_pet_cap", "Sets the maximum number of pets a Theurgist can summon", 16)]
		public static int THEURGIST_PET_CAP;

        /// <summary>
        /// This specifies the max amount of people in one battlegroup.
        /// </summary>
        [ServerProperty("battlegroup_max_member", "Max number of members allowed in a battlegroup.", 64)]
        public static int BATTLEGROUP_MAX_MEMBER;
        
        /// <summary>
        /// This if the server battleground zones are open to players
        /// </summary>
        [ServerProperty("bg_zones_open", "Can the players teleport to battleground", true)]
        public static bool BG_ZONES_OPENED;
        
        /// <summary>
        /// This enables or disables new guild dues. Live standard is 2% dues
        /// </summary>
        [ServerProperty("new_guild_dues", "Guild dues can be set from 1-100% if enabled, or standard 2% if not", false)]
        public static bool NEW_GUILD_DUES;

        /// <summary>
        /// Sets the max allowed items inside/outside a house.
        /// If Outdoor is increased past 30, they vanish. It seems to be hardcoded in client
        /// </summary>
        [ServerProperty("max_indoor_house_items", "Max number of items allowed inside a players house.", 40)]
        public static int MAX_INDOOR_HOUSE_ITEMS;
        [ServerProperty("max_outdoor_house_items", "Max number of items allowed in a players garden.", 30)]
        public static int MAX_OUTDOOR_HOUSE_ITEMS;
        [ServerProperty("indoor_items_depend_on_size", "If true the max number of allowed House indoor items are set like live (40, 60, 80, 100)", true)]
        public static bool INDOOR_ITEMS_DEPEND_ON_SIZE;


		[ServerProperty("housing_rent_cottage", "Rent price for a cottage.", 200000L)]
		public static long HOUSING_RENT_COTTAGE;

		[ServerProperty("housing_rent_house", "Rent price for a house.", 350000L)]
		public static long HOUSING_RENT_HOUSE;

		[ServerProperty("housing_rent_villa", "Rent price for a villa.", 600000L)]
		public static long HOUSING_RENT_VILLA;

		[ServerProperty("housing_rent_mansion", "Rent price for a mansion.", 1000000L)]
		public static long HOUSING_RENT_MANSION;

		[ServerProperty("housing_lot_price_start", "Starting lot price before per hour reductions", 95L * 1000L * 100L * 100L)] // 95p
		public static long HOUSING_LOT_PRICE_START;

		[ServerProperty("housing_lot_price_per_hour", "Lot price reduction per hour.", (long)(1.2 * 1000 * 100 * 100))] // 1.2p
		public static long HOUSING_LOT_PRICE_PER_HOUR;

		[ServerProperty("housing_lot_price_minimum", "Minimum lot price.", 300L * 100L * 100L)] // 300g
		public static long HOUSING_LOT_PRICE_MINIMUM;

		[ServerProperty("housing_debug_allow_multiple", "Allow the player to own more than 1 house.  This breaks some housing commands.", false)]
		public static bool HOUSING_DEBUG_ALLOW_MULTIPLE;


		[ServerProperty("statprint_frequency", "How often (milliseconds) should statistics be printed on the server console.", 30000)]
		public static int STATPRINT_FREQUENCY;


		[ServerProperty("world", "world_item_decay_time", "How long (milliseconds) will an item dropped on the ground stay in the world.", (uint)180000)]
		public static uint WORLD_ITEM_DECAY_TIME;

		[ServerProperty("world", "world_pickup_distance", "How far before you can no longer pick up an object (loot for example).", 256)]
		public static int WORLD_PICKUP_DISTANCE;

		[ServerProperty("world", "world_day_increment", "Day Increment (0 to 512, default is 24).  Larger increments make shorter days.", (uint)24)]
		public static uint WORLD_DAY_INCREMENT;

		[ServerProperty("world", "world_npc_update_interval", "How often (milliseconds) will npc's broadcast updates to the clients. Minimum allowed = 1000 (1 second).", (uint)30000)]
		public static uint WORLD_NPC_UPDATE_INTERVAL;

		[ServerProperty("world", "world_player_update_interval", "How often (milliseconds) will players be checked for updates. Minimum allowed = 100 (100 milliseconds).", (uint)300)]
		public static uint WORLD_PLAYER_UPDATE_INTERVAL;

		[ServerProperty("weather_check_interval", "How often (milliseconds) will weather be checked for a chance to start a storm.", 5 * 60 * 1000)]
		public static int WEATHER_CHECK_INTERVAL;

		[ServerProperty("weather_chance", "What is the chance of starting a storm.", 5)]
		public static int WEATHER_CHANCE;

		[ServerProperty("weather_log_events", "Should weather events be shown in the log (and on the console).", true)]
		public static bool WEATHER_LOG_EVENTS;



        /// <summary>
        /// This is to set the baseHP For NPCs
        /// </summary>
        [ServerProperty("npc", "gamenpc_base_hp", "GameNPC's base HP * level", 500)]
        public static int GAMENPC_BASE_HP;

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
		public static readonly string GAMENPC_DEFAULT_CLASSTYPE;

		/// <summary>
        /// This will Allow/Disallow dual loggins
        /// </summary>
        [ServerProperty("allow_dual_logins", "Disable to disallow players to connect with more than 1 account at a time.", true)]
        public static bool ALLOW_DUAL_LOGINS;

		/// <summary>
		/// Minimum privilege level to be able to enter Atlantis through teleporters.
		/// 1 = anyone can enter Atlantis,
		/// 2 = only GMs and admins can enter Atlantis,
		/// 3 = only admins can enter Atlantis.
		/// </summary>
		[ServerProperty("atlantis_teleport_plvl", "Set the minimum privilege level required to enter Atlantis zones.", 2)]
		public static int ATLANTIS_TELEPORT_PLVL;

		/// <summary>
		/// Sets the disabled commands for the server split by ;
		/// </summary>
		[ServerProperty("disabled_commands", "Sets the disabled commands for the server split by ;, example /realm;/toon;/quit", "")]
		public static readonly string DISABLED_COMMANDS;

		/// <summary>
		/// Do we allow guild members from other realms
		/// </summary>
		[ServerProperty("allow_cross_realm_guilds","Do we allow guild members from other realms?", false)]
		public static bool ALLOW_CROSS_REALM_GUILDS;

		/// <summary>
		/// Do we want to allow items to be equipped regardless of realm?
		/// </summary>
		[ServerProperty("allow_cross_realm_items", "Do we want to allow items to be equipped regardless of realm?", false)]
		public static bool ALLOW_CROSS_REALM_ITEMS;

		/// <summary>
		/// What level should /level bring you to?
		/// </summary>
		[ServerProperty("slash_level_target", "What level should /level bring you to? ", 20)]
		public static int SLASH_LEVEL_TARGET;

		/// <summary>
		/// What level should you have on your account to be able to use /level?
		/// </summary>
		[ServerProperty("slash_level_requirement", "What level should you have on your account be able to use /level?", 50)]
		public static int SLASH_LEVEL_REQUIREMENT;

		/// <summary>
		/// How many players are required on the relic pad to trigger the pillar?
		/// </summary>
		[ServerProperty("relic_players_required_on_pad", "How many players are required on the relic pad to trigger the pillar?", 16)]
		public static int RELIC_PLAYERS_REQUIRED_ON_PAD;

        /// <summary>
        /// What levels did we allow a DOL respec ? serialized
        /// </summary>
        [ServerProperty("give_dol_respec_at_level", "What levels does we give a DOL respec? (serialized)", "0")]
        public static string GIVE_DOL_RESPEC_AT_LEVEL;

		/// <summary>
		/// How many things do we allow guilds to claim?
		/// </summary>
		[ServerProperty("guilds_claim_limit", "How many things do we allow guilds to claim?", 1)]
		public static int GUILDS_CLAIM_LIMIT;

        /// <summary>
        /// The level keeps return to when claimed.
        /// </summary>
        [ServerProperty("keeps", "starting_keep_level", "The default level of a keep when captured that it reverts to. Set to 0 for typical new frontiers, though Dinberg would suggest a value of 1!", 0)]
        public static int STARTING_KEEP_LEVEL;

        /// <summary>
        /// Record news in database
        /// </summary>
        [ServerProperty("record_news", "Record News in database?", true)]
        public static bool RECORD_NEWS;

		/// <summary>
		/// Whether to use the sync timer utility or not
		/// </summary>
		[ServerProperty("use_sync_timer", "Shall we use the sync timers utility?", true)]
		public static readonly bool USE_SYNC_UTILITY;

		/// <summary>
		/// Ignore too long outcoming packet or not
		/// </summary>
		[ServerProperty("ignore_too_long_outcoming_packet", "Shall we ignore too long outcoming packet ?", false)]
		public static bool IGNORE_TOO_LONG_OUTCOMING_PACKET;

		
		/// <summary>
		/// Epic encounters strength: 100 is 100% base strength
		/// </summary>
		[ServerProperty("set_difficulty_on_epic_encounters", "Tune encounters taggued <Epic Encounter>. 0 means auto adaptative, others values are % of the initial difficulty (100%=initial difficulty)", 100)]
		public static int SET_DIFFICULTY_ON_EPIC_ENCOUNTERS;


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
		/// Multiplier used to add or subtract RP worth based on keep/tower level difference from 50. 
		/// </summary>
		[ServerProperty("keeps", "keep_rp_multiplier", "Integer multiplier used to increase/decrease RP worth based on keep/tower level difference from 50.", 0)]
		public static int KEEP_RP_MULTIPLIER;

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
		/// Multiplier used to determine keep guard levels.  This is applied to the bonus level (usually 4) and added after balance adjustments.
		/// </summary>
		[ServerProperty("keeps", "tower_guard_level_multiplier", "Multiplier used to determine tower guard levels.  This is applied to the bonus level (usually 4) and added after balance adjustments.", 1.0)]
		public static double TOWER_GUARD_LEVEL_MULTIPLIER;




		/// <summary>
		/// Allow or disallow /irc in RvR zones, while allowed in pve zone
		/// </summary>
		[ServerProperty("allow_irc_in_rvr", "Allow players to send/receive irc when in RvR zone", true)]
		public static bool ALLOW_IRC_IN_RVR;

        /// <summary>
        /// Ignore too long outcoming packet or not
        /// </summary>
        [ServerProperty("enable_minotaur_relics", "Shall we enable Minotaur Relics ?", false)]
        public static readonly bool ENABLE_MINOTAUR_RELICS;

        /// <summary>
        /// Enable WarMap manager
        /// </summary>
        [ServerProperty("enable_warmapmgr", "Shall we enable the WarMap manager ?", false)]
        public static bool ENABLE_WARMAPMGR;

        /// <summary>
		/// Keeps to load. 0 for Old Keeps, 1 for new keeps, 2 for both.
        /// </summary>
        [ServerProperty("use_new_keeps", "Keeps to load. 0 for Old Keeps, 1 for new keeps, 2 for both.", 2)]
        public static readonly int USE_NEW_KEEPS;

        /// <summary>
        /// Perform checklos on client with each mob
        /// </summary>
        [ServerProperty("always_check_los", "Perform a LoS check before aggroing. This can involve a huge lag, handle with care!", false)]
        public static bool ALWAYS_CHECK_LOS;
		
        /// <summary>
		/// Account bombing prevention
		/// </summary>
		[ServerProperty("time_between_account_creation", "The time in minutes between 2 accounts creation. This avoid account bombing with dynamic ip. 0 to disable", 2)]
		public static int TIME_BETWEEN_ACCOUNT_CREATION;
        
        /// <summary>
        /// PvP Immunity Timer - Killed by Player
        /// </summary>
        [ServerProperty("Timer_Killed_By_Player", "Immunity Timer When player killed in PvP, in seconds", 120)] //2 min default
        public static int TIMER_KILLED_BY_PLAYER;

        /// <summary>
        /// PvP Immunity Timer - Killed by Mobs
        /// </summary>
        [ServerProperty("Timer_Killed_By_Mob", "Immunity Timer When player killed in PvP, in seconds", 30)] //30 seconds default
        public static int TIMER_KILLED_BY_MOB;

        /// <summary>
        /// PvP Immunity Timer - Region Changed (Enter World Timer Divided by 3 of this)
        /// </summary>
        [ServerProperty("Timer_Region_Changed", "Immunity Timer when player changes regions, in seconds", 30)] //30 seconds default
        public static int TIMER_REGION_CHANGED;

        /// <summary>
        /// Time after a relic lost in nature is returning to his ReturnRelicPad pad
        /// </summary>
        [ServerProperty("Relic_Return_Time", "A lost relic will automatically returns to its defined point, in seconds", 20*60)] //20 mins default
        public static int RELIC_RETURN_TIME;

		/// <summary>
		/// Xp Cap for a player.  Given in percent of level.  Default is 125%
		/// </summary>
		[ServerProperty("XP_Cap_Percent", "Maximum XP a player can earn given in percent of their level. Default is 125%", 125)]
		public static int XP_CAP_PERCENT;

		/// <summary>
		/// Xp Cap for a player in a group.  Given in percent of level.  Default is 125%
		/// </summary>
		[ServerProperty("XP_Group_Cap_Percent", "Maximum XP a player can earn while in a group, given in percent of their level. Default is 125%", 125)]
		public static int XP_GROUP_CAP_PERCENT;

		/// <summary>
		/// Xp Cap for a player vs player kill.  Given in percent of level.  Default is 125%
		/// </summary>
		[ServerProperty("XP_PVP_Cap_Percent", "Maximum XP a player can earn killing another player, given in percent of their level. Default is 125%", 125)]
		public static int XP_PVP_CAP_PERCENT;

		/// <summary>
		/// Hardcap XP a player can earn after all other adjustments are applied.  Given in percent of level, default is 500%  There is no live value that corresponds to this cap.
		/// </summary>
		[ServerProperty("XP_HardCap_Percent", "Hardcap XP a player can earn after all other adjustments are applied. Given in percent of their level. Default is 500%", 500)]
		public static int XP_HARDCAP_PERCENT;


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
				property.Area = attrib.Area;
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
			catch (Exception e)
			{
				log.Error("Exception in ServerProperties Load: ", e);
                log.Error("Trying to load " + key + " value is " + property.Value);
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
		        f.SetValue(null, Load(attrib));
			}
		}

		/// <summary>
		/// Refreshes the server properties from the DB
		/// </summary>
		public static void Refresh()
		{
			log.Info("Refreshing server properties!");
			Init(typeof(Properties));
		}
	}
}
