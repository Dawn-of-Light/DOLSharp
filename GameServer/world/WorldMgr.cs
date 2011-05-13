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
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.Utils;
using log4net;
using DOL.Config;
using Timer=System.Threading.Timer;
using System.Collections.Generic;
using DOL.GS.Housing;

namespace DOL.GS
{
	/// <summary>
	/// The WorldMgr is used to retrieve information and objects from
	/// the world. It contains lots of functions that can be used. It
	/// is a static class.
	/// </summary>
	public sealed class WorldMgr
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Ping timeout definition in seconds
		/// </summary>
		public const long PING_TIMEOUT = 360; // 6 min default ping timeout (ticks are 100 nano seconds)
		/// <summary>
		/// Holds the distance which player get experience from a living object
		/// </summary>
		public const int MAX_EXPFORKILL_DISTANCE = 16384;
		/// <summary>
		/// Is the distance a whisper can be heard
		/// </summary>
		public const int WHISPER_DISTANCE = 512; // tested
		/// <summary>
		/// Is the distance a say is broadcast
		/// </summary>
		public const int SAY_DISTANCE = 512; // tested
		/// <summary>
		/// Is the distance info messages are broadcast (player attacks, spell cast, player stunned/rooted/mezzed, loot dropped)
		/// </summary>
		public const int INFO_DISTANCE = 512; // tested for player attacks, think it's same for rest
		/// <summary>
		/// Is the distance a death message is broadcast when player dies
		/// </summary>
		public const ushort DEATH_MESSAGE_DISTANCE = ushort.MaxValue; // unknown
		/// <summary>
		/// Is the distance a yell is broadcast
		/// </summary>
		public const int YELL_DISTANCE = 1024; // tested
		/// <summary>
		/// Is the distance at which livings can give a item
		/// </summary>
		public const int GIVE_ITEM_DISTANCE = 128;  // tested
		/// <summary>
		/// Is the distance at which livings can interact
		/// </summary>
		public const int INTERACT_DISTANCE = 192;  // tested
		/// <summary>
		/// Is the distance an player can see
		/// </summary>
		public const int VISIBILITY_DISTANCE = 3600;
		/// <summary>
		/// Moving greater than this distance requires the player to do a full world refresh
		/// </summary>
		public const int REFRESH_DISTANCE = 1000;
		/// <summary>
		/// Is the square distance a player can see
		/// </summary>
		public const int VISIBILITY_SQUARE_DISTANCE = 12960000;
		/// <summary>
		/// Holds the distance at which objects are updated
		/// </summary>
		public const int OBJ_UPDATE_DISTANCE = 4096;

		/// <summary>
		/// This will store available teleport destinations as read from the 'teleport' table.  These are
		/// stored in a dictionary of dictionaries because the name of the teleport destination (the
		/// 'TeleportID' field from the table) does not have to be unique across realms.  Duplicate
		/// 'TeleportID' fields are permitted so long as the 'Realm' field is different for each.
		/// </summary>
		private static Dictionary<eRealm, Dictionary<string, Teleport>> m_teleportLocations;
		private static object m_syncTeleport = new object();

		// this is used to hold the player ids with timestamp of ld, that ld near an enemy keep structure, to allow grace period relog
		public static Dictionary<string, DateTime> RvRLinkDeadPlayers = new Dictionary<string, DateTime>();

		/// <summary>
		/// Returns the teleport given an ID and a realm
		/// </summary>
		/// <param name="realm">
		/// The home realm identifier of the NPC doing the teleporting.  Whether or not a teleport is
		/// permitted is determined by the home realm of the teleporter NPC, not the home realm of
		/// the player who is teleporting.  A teleport will be allowed so long as the 'Realm' field in
		/// the 'teleport' table matches the 'Realm' field for the teleporter's record in the 'mob' table.
		/// For example, a Lurikeen teleporter with a 'mob' table entry that has the Realm field set to 3
		/// (Hibernia), will happily teleport an Albion player to Jordheim so long as the Jordheim record
		/// in the 'teleport' table is also tagged as Realm 3.  So, the Realm field in the 'teleport'
		/// table is not the realm of the destination, but the realm of the NPCs that are allowed to
		/// teleport a player to that location.
		/// </param>
		/// <param name="teleportKey">Composite key into teleport dictionary.</param>
		/// <returns></returns>
		public static Teleport GetTeleportLocation(eRealm realm, String teleportKey)
		{
			lock (m_syncTeleport)
			{
				return (m_teleportLocations.ContainsKey(realm)) ?
					(m_teleportLocations[realm].ContainsKey(teleportKey) ?
					 m_teleportLocations[realm][teleportKey] :
					 null) :
					null;
			}
		}

		/// <summary>
		/// Add a new teleport destination (used by /teleport add).
		/// </summary>
		/// <param name="teleport"></param>
		/// <returns></returns>
		public static bool AddTeleportLocation(Teleport teleport)
		{
			eRealm realm = (eRealm)teleport.Realm;
			String teleportKey = String.Format("{0}:{1}", teleport.Type, teleport.TeleportID);

			lock (m_syncTeleport)
			{
				Dictionary<String, Teleport> teleports = null;
				if (m_teleportLocations.ContainsKey(realm))
				{
					if (m_teleportLocations[realm].ContainsKey(teleportKey))
						return false;   // Double entry.

					teleports = m_teleportLocations[realm];
				}

				if (teleports == null)
				{
					teleports = new Dictionary<String, Teleport>();
					m_teleportLocations.Add(realm, teleports);
				}

				teleports.Add(teleportKey, teleport);
				return true;
			}
		}

		/// <summary>
		/// This hashtable holds all regions in the world
		/// </summary>
		private static readonly Hashtable m_regions = new Hashtable();

		public static Hashtable Regions
		{
			get { return m_regions; }
		}

		/// <summary>
		/// This hashtable holds all zones in the world, for easy access
		/// </summary>
		private static readonly Hashtable m_zones = new Hashtable();

		public static Hashtable Zones
		{
			get { return m_zones; }
		}

		/// <summary>
		/// This array holds all gameclients connected to the game
		/// </summary>
		private static GameClient[] m_clients = new GameClient[0];

		/// <summary>
		/// Timer for ping timeout checks
		/// </summary>
		private static Timer m_pingCheckTimer;

		/// <summary>
		/// This thread is used to update the NPCs around a player
		/// as fast as possible
		/// </summary>
		private static Thread m_WorldUpdateThread;

		/// <summary>
		/// This constant defines the day constant
		/// </summary>
		private const int DAY = 77760000;

		/// <summary>
		/// This holds the tick when the day started
		/// </summary>
		private static int m_dayStartTick;

		/// <summary>
		/// This holds the speed of our days
		/// </summary>
		private static uint m_dayIncrement;

		/// <summary>
		/// A timer that will send the daytime to all playing
		/// clients after a certain intervall;
		/// </summary>
		private static Timer m_dayResetTimer;

		/// <summary>
		/// Region ID INI field
		/// </summary>
		private const string ENTRY_REG_ID = "id";
		/// <summary>
		/// Region IP INI field
		/// </summary>
		private const string ENTRY_REG_IP = "ip";
		/// <summary>
		/// Region port INI field
		/// </summary>
		private const string ENTRY_REG_PORT = "port";
		/// <summary>
		/// Region description INI field
		/// </summary>
		private const string ENTRY_REG_DESC = "description";
		/// <summary>
		/// Region diving enable INI field
		/// </summary>
		private const string ENTRY_REG_DIVING_ENABLE = "isDivingEnabled";
		/// <summary>
		/// Region diving enable INI field
		/// </summary>
		private const string ENTRY_REG_HOUSING_ENABLE = "isHousingEnabled";
		/// <summary>
		/// Region water level INI field
		/// </summary>
		private const string ENTRY_REG_WATER_LEVEL = "waterLevel";
		/// <summary>
		/// Region expansion INI field
		/// </summary>
		private const string ENTRY_REG_EXPANSION = "expansion";

		/// <summary>
		/// Zone ID INI field
		/// </summary>
		private const string ENTRY_ZONE_ZONEID = "zoneID";
		/// <summary>
		/// Zone region INI field
		/// </summary>
		private const string ENTRY_ZONE_REGIONID = "regionID";
		/// <summary>
		/// Zone description INI field
		/// </summary>
		private const string ENTRY_ZONE_DESC = "description";
		/// <summary>
		/// Zone X offset INI field
		/// </summary>
		private const string ENTRY_ZONE_OFFX = "offsetx";
		/// <summary>
		/// Zone Y offset INI field
		/// </summary>
		private const string ENTRY_ZONE_OFFY = "offsety";
		/// <summary>
		/// Zone width INI field
		/// </summary>
		private const string ENTRY_ZONE_WIDTH = "width";
		/// <summary>
		/// Zone height INI field
		/// </summary>
		private const string ENTRY_ZONE_HEIGHT = "height";
		/// <summary>
		/// Zone water level INI field
		/// </summary>
		private const string ENTRY_ZONE_WATER_LEVEL = "waterlevel";
		
		/// <summary>
		/// Does this zone contain Lava
		/// </summary>
		private const string ENTRY_ZONE_LAVA = "IsLava";

		/// <summary>
		/// Relocation threads for relocation of zones
		/// </summary>
		private static Thread m_relocationThread;

		/// <summary>
		/// Holds all region timers
		/// </summary>
		private static GameTimer.TimeManager[] m_regionTimeManagers;

		/// <summary>
		/// Initializes the most important things that is needed for some code
		/// </summary>
		/// <param name="regionsData">The loaded regions data</param>
		public static bool EarlyInit(out RegionData[] regionsData)
		{
			
			log.Debug(GC.GetTotalMemory(true) / 1000 + "kb - w1");

			lock (m_regions.SyncRoot)
				m_regions.Clear();
			lock (m_zones.SyncRoot)
				m_zones.Clear();

			//If the files are missing this method
			//creates the default values
			CheckRegionAndZoneConfigs();
			XMLConfigFile zoneCfg = XMLConfigFile.ParseXMLFile(new FileInfo(GameServer.Instance.Configuration.ZoneConfigFile)); ;
			
			XMLConfigFile regionCfg = XMLConfigFile.ParseXMLFile(new FileInfo(GameServer.Instance.Configuration.RegionConfigFile));

			if (log.IsDebugEnabled)
			{
				log.Debug(string.Format("{0} blocks read from {1}", regionCfg.Children.Count, GameServer.Instance.Configuration.RegionConfigFile));
				log.Debug(string.Format("{0} blocks read from {1}", zoneCfg.Children.Count, GameServer.Instance.Configuration.ZoneConfigFile));
			}

			#region Instances

			//Dinberg: We now need to save regionData, indexed by regionID, for instances.
			//The information generated here is oddly ordered by number of mbos in the region,
			//so I'm contriving to generate this list myself.
			m_regionData = new Hashtable();

			//We also will need to store zones, because we need at least one zone per region - hence
			//we will create zones inside our instances or the player gets banned by anti-telehack scripts.
			m_zonesData = new Dictionary<ushort, List<ZoneData>>();

			#endregion

			log.Info(LoadTeleports());

			// sort the regions by mob count

			log.Debug("loading mobs from DB...");

			var mobList = new List<Mob>();
			if (ServerProperties.Properties.DEBUG_LOAD_REGIONS != string.Empty)
			{
				foreach (string loadRegion in ServerProperties.Properties.DEBUG_LOAD_REGIONS.SplitCSV(true))
				{
					mobList.AddRange(GameServer.Database.SelectObjects<Mob>("region = " + loadRegion));
				}
			}
			else
			{
				mobList.AddRange(GameServer.Database.SelectAllObjects<Mob>());
			}

			var mobsByRegionId = new Dictionary<ushort, List<Mob>>(512);
			foreach (Mob mob in mobList)
			{
				List<Mob> list;

				if (!mobsByRegionId.TryGetValue(mob.Region, out list))
				{
					list = new List<Mob>(1024);
					mobsByRegionId.Add(mob.Region, list);
				}

				list.Add(mob);
			}

            if (GameServer.Database.GetObjectCount<DBRegions>() < regionCfg.Children.Count)
            {
                foreach (var entry in regionCfg.Children)
                {
                    ConfigElement config = entry.Value;

                    DBRegions dbRegion = GameServer.Database.FindObjectByKey<DBRegions>(config[ENTRY_REG_ID].GetInt());
                    if (dbRegion == null)
                    {
                        dbRegion = new DBRegions();

                        dbRegion.RegionID = (ushort)config[ENTRY_REG_ID].GetInt();
                        dbRegion.Name = entry.Key;
                        dbRegion.Description = config[ENTRY_REG_DESC].GetString();
                        dbRegion.IP = config[ENTRY_REG_IP].GetString();
                        dbRegion.Port = (ushort)config[ENTRY_REG_PORT].GetInt();
                        dbRegion.Expansion = config[ENTRY_REG_EXPANSION].GetInt();
                        dbRegion.HousingEnabled = config[ENTRY_REG_HOUSING_ENABLE].GetBoolean(false);
                        dbRegion.DivingEnabled = config[ENTRY_REG_DIVING_ENABLE].GetBoolean(false);
                        dbRegion.WaterLevel = config[ENTRY_REG_WATER_LEVEL].GetInt();

                        log.Debug(string.Format("Region {0} was not found in the database. Added!", dbRegion.RegionID));
                        GameServer.Database.AddObject(dbRegion);
                    }
                }
            }

			var regions = new List<RegionData>(512);
            foreach (DBRegions dbRegion in GameServer.Database.SelectAllObjects<DBRegions>())
            {
                RegionData data = new RegionData();

                data.Id = dbRegion.RegionID;
                data.Name = dbRegion.Name;
                data.Description = dbRegion.Description;
                data.Ip = dbRegion.IP;
                data.Port = dbRegion.Port;
                data.Expansion = dbRegion.Expansion;
                data.HousingEnabled = dbRegion.HousingEnabled;
                data.DivingEnabled = dbRegion.DivingEnabled;
                data.WaterLevel = dbRegion.WaterLevel;

                List<Mob> mobs;

                if (!mobsByRegionId.TryGetValue(data.Id, out mobs))
                    data.Mobs = new Mob[0];
                else
                    data.Mobs = mobs.ToArray();

                regions.Add(data);

                //Dinberg - save the data by ID.
                if (m_regionData.ContainsKey(data.Id))
                    log.Error("Duplicate key in region table - " + data.Id + ", EarlyInit in WorldMgr failed.");
                else
                    m_regionData.Add(data.Id, data);
            }

			regions.Sort();

			log.Debug(GC.GetTotalMemory(true) / 1000 + "kb - w2");

			int cpuCount = GameServer.Instance.Configuration.CpuCount;
			if (cpuCount < 1)
				cpuCount = 1;

			GameTimer.TimeManager[] timers = new GameTimer.TimeManager[cpuCount];
			for (int i = 0; i < cpuCount; i++)
			{
				timers[i] = new GameTimer.TimeManager("RegionTime" + (i + 1).ToString());
			}

			m_regionTimeManagers = timers;

			for (int i = 0; i < regions.Count; i++)
			{
				RegionData region = (RegionData)regions[i];
				RegisterRegion(timers[FastMath.Abs(i % (cpuCount * 2) - cpuCount) % cpuCount], region);
			}

			log.Debug(GC.GetTotalMemory(true) / 1000 + "kb - w3");

			//stephenxpimentel - changed to SQL.
			if (GameServer.Database.GetObjectCount<Zones>() < zoneCfg.Children.Count)
			{
				foreach (var entry in zoneCfg.Children)
				{
					//string name = (string) entry.Key;
					ConfigElement config = entry.Value;

					Zones checkZone = GameServer.Database.FindObjectByKey<Zones>(config[ENTRY_ZONE_ZONEID].GetInt());
					if (checkZone == null)
					{
						Zones dbZone = new Zones();
						dbZone.Height = config[ENTRY_ZONE_HEIGHT].GetInt(0);
						dbZone.Width = config[ENTRY_ZONE_WIDTH].GetInt(0);
						dbZone.OffsetY = config[ENTRY_ZONE_OFFY].GetInt(0);
						dbZone.OffsetX = config[ENTRY_ZONE_OFFX].GetInt(0);
						dbZone.Name = config[ENTRY_ZONE_DESC].GetString("");
						dbZone.RegionID = (ushort)config[ENTRY_ZONE_REGIONID].GetInt(0);
						dbZone.ZoneID = config[ENTRY_ZONE_ZONEID].GetInt(0);
						dbZone.WaterLevel = config[ENTRY_ZONE_WATER_LEVEL].GetInt(0);
						dbZone.DivingFlag = 0;

						if (config[ENTRY_ZONE_LAVA].GetInt(0) != 0)
						{
							dbZone.IsLava = true;
						}
						else
						{
							dbZone.IsLava = false;
						}

						log.Debug(string.Format("Zone {0} was not found in the database. Added!", dbZone.ZoneID));
						GameServer.Database.AddObject(dbZone);
					}
				}
			}			

			foreach (Zones dbZone in GameServer.Database.SelectAllObjects<Zones>())
			{
				ZoneData zoneData = new ZoneData();
				zoneData.Height = (byte)dbZone.Height;
				zoneData.Width = (byte)dbZone.Width;
				zoneData.OffY = (byte)dbZone.OffsetY;
				zoneData.OffX = (byte)dbZone.OffsetX;
				zoneData.Description = dbZone.Name;
				zoneData.RegionID = dbZone.RegionID;
				zoneData.ZoneID = (ushort)dbZone.ZoneID;
				zoneData.WaterLevel = dbZone.WaterLevel;
				zoneData.DivingFlag = dbZone.DivingFlag;
				zoneData.IsLava = dbZone.IsLava;
				RegisterZone(zoneData, zoneData.ZoneID, zoneData.RegionID, zoneData.Description,
				             dbZone.Experience, dbZone.Realmpoints, dbZone.Bountypoints, dbZone.Coin);

				//Save the zonedata.
				if (!m_zonesData.ContainsKey(zoneData.RegionID))
					m_zonesData.Add(zoneData.RegionID, new List<ZoneData>());

				m_zonesData[zoneData.RegionID].Add(zoneData);

			}

			log.Debug(GC.GetTotalMemory(true) / 1000 + "kb - w4");

			regionsData = regions.ToArray();
			return true;
		}


		/// <summary>
		/// Load available teleport locations.
		/// </summary>
		public static string LoadTeleports()
		{
			var objs = GameServer.Database.SelectAllObjects<Teleport>();
			m_teleportLocations = new Dictionary<eRealm, Dictionary<string, Teleport>>();
			int[] numTeleports = new int[3];
			foreach (Teleport teleport in objs)
			{
				Dictionary<string, Teleport> teleportList;
				if (m_teleportLocations.ContainsKey((eRealm)teleport.Realm))
					teleportList = m_teleportLocations[(eRealm)teleport.Realm];
				else
				{
					teleportList = new Dictionary<string, Teleport>();
					m_teleportLocations.Add((eRealm)teleport.Realm, teleportList);
				}
				String teleportKey = String.Format("{0}:{1}", teleport.Type, teleport.TeleportID);
				if (teleportList.ContainsKey(teleportKey))
				{
					log.Error("WorldMgr.EarlyInit teleporters - Cannot add " + teleportKey + " already exists");
					continue;
				}
				teleportList.Add(teleportKey, teleport);
				if (teleport.Realm >= 1 && teleport.Realm <= 3)
					numTeleports[teleport.Realm - 1]++;
			}

			objs = null;

			return String.Format("Loaded {0} Albion, {1} Midgard and {2} Hibernia teleport locations", numTeleports[0], numTeleports[1], numTeleports[2]);
		}


		/// <summary>
		/// Initializes the WorldMgr. This function must be called
		/// before the WorldMgr can be used!
		/// </summary>
		public static bool Init(RegionData[] regionsData)
		{
			try
			{
				m_clients = new GameClient[GameServer.Instance.Configuration.MaxClientCount];

				LootMgr.Init();

				long mobs = 0;
				long merchants = 0;
				long items = 0;
				long bindpoints = 0;
				foreach (RegionData data in regionsData)
				{
					Region reg = (Region)m_regions[data.Id];
					reg.LoadFromDatabase(data.Mobs, ref mobs, ref merchants, ref items, ref bindpoints);
				}

				if (log.IsInfoEnabled)
				{
					log.Info("Total Mobs: " + mobs);
					log.Info("Total Merchants: " + merchants);
					log.Info("Total Items: " + items);
					log.Info("Total Bind Points: " + bindpoints);
				}

				m_WorldUpdateThread = new Thread(new ThreadStart(WorldUpdateThreadStart));
				m_WorldUpdateThread.Priority = ThreadPriority.AboveNormal;
				m_WorldUpdateThread.Name = "NpcUpdate";
				m_WorldUpdateThread.IsBackground = true;
				m_WorldUpdateThread.Start();

				m_dayIncrement = Math.Max(0, Math.Min(1000, ServerProperties.Properties.WORLD_DAY_INCREMENT)); // increments > 1000 do not render smoothly on clients
				m_dayStartTick = Environment.TickCount - (int)(DAY / Math.Max(1, m_dayIncrement) / 2); // set start time to 12pm
				m_dayResetTimer = new Timer(new TimerCallback(DayReset), null, DAY / Math.Max(1, m_dayIncrement) / 2, DAY / Math.Max(1, m_dayIncrement));

				m_pingCheckTimer = new Timer(new TimerCallback(PingCheck), null, 10 * 1000, 0); // every 10s a check

				m_relocationThread = new Thread(new ThreadStart(RelocateRegions));
				m_relocationThread.Name = "RelocateReg";
				m_relocationThread.IsBackground = true;
				m_relocationThread.Start();
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("Init", e);
				return false;
			}
			return true;
		}

		/// <summary>
		/// Gets all region time managers
		/// </summary>
		/// <returns>A copy of region time managers array</returns>
		public static GameTimer.TimeManager[] GetRegionTimeManagers()
		{
			GameTimer.TimeManager[] timers = m_regionTimeManagers;
			if (timers == null) return new GameTimer.TimeManager[0];
			return (GameTimer.TimeManager[])timers.Clone();
		}

		/// <summary>
		/// perform the ping timeout check and disconnect clients that timed out
		/// </summary>
		/// <param name="sender"></param>
		private static void PingCheck(object sender)
		{
			try
			{
				foreach (GameClient client in GetAllClients())
				{
					try
					{
						// check ping timeout if we are in charscreen or in playing state
						if (client.ClientState == GameClient.eClientState.CharScreen ||
						    client.ClientState == GameClient.eClientState.Playing)
						{
							if (client.PingTime + PING_TIMEOUT * 1000 * 1000 * 10 < DateTime.Now.Ticks)
							{
								if (log.IsWarnEnabled)
									log.Warn("Ping timeout for client " + client.Account.Name);
								GameServer.Instance.Disconnect(client);
							}
						}
						else
						{
							// in all other cases client gets 10min to get wether in charscreen or playing state
							if (client.PingTime + 10 * 60 * 10000000L < DateTime.Now.Ticks)
							{
								if (log.IsWarnEnabled)
									log.Warn("Hard timeout for client " + client.Account.Name + " (" + client.ClientState + ")");
								GameServer.Instance.Disconnect(client);
							}
						}
					}
					catch (Exception ex)
					{
						if (log.IsErrorEnabled)
							log.Error("PingCheck", ex);
					}
				}
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("PingCheck callback", e);
			}
			finally
			{
				m_pingCheckTimer.Change(10 * 1000, Timeout.Infinite);
			}
		}

		/// <summary>
		/// Gets the RelocateRegions() thread stacktrace
		/// </summary>
		/// <returns></returns>
		public static StackTrace GetRelocateRegionsStacktrace()
		{
			return Util.GetThreadStack(m_relocationThread);
		}

		private static void RelocateRegions()
		{
			log.InfoFormat("started RelocateRegions() thread ID:{0}", Thread.CurrentThread.ManagedThreadId);
			while (m_relocationThread != null && m_relocationThread.IsAlive)
			{
				try
				{
					Thread.Sleep(200); // check every 200ms for needed relocs
					int start = Environment.TickCount;

					Hashtable regionsClone = (Hashtable)m_regions.Clone();

					foreach (Region region in regionsClone.Values)
					{
						if (region.NumPlayers > 0 && (region.LastRelocation + Zone.MAX_REFRESH_INTERVAL) * 10 * 1000 < DateTime.Now.Ticks)
						{
							region.Relocate();
						}
					}
					int took = Environment.TickCount - start;
					if (took > 500)
					{
						if (log.IsWarnEnabled)
							log.WarnFormat("RelocateRegions() took {0}ms", took);
					}
				}
				catch (ThreadAbortException)
				{
					//On Threadabort exit!
					return;
				}
				catch (ThreadInterruptedException)
				{
					//On sleep interrupt do nothing
				}
				catch (Exception e)
				{
					log.Error(e.ToString());
				}
			}
			log.InfoFormat("stopped RelocateRegions() thread ID:{0}", Thread.CurrentThread.ManagedThreadId);
		}

		/// <summary>
		/// This timer callback resets the day on all clients
		/// </summary>
		/// <param name="sender"></param>
		private static void DayReset(object sender)
		{
			m_dayStartTick = Environment.TickCount;
			foreach (GameClient client in GetAllPlayingClients())
			{
				if (client.Player != null && client.Player.CurrentRegion != null && client.Player.CurrentRegion.UseTimeManager)
				{
					client.Out.SendTime();
				}
			}
		}

		/// <summary>
		/// Starts a new day with a certain increment
		/// </summary>
		/// <param name="dayInc"></param>
		/// <param name="dayStart"></param>
		public static void StartDay(uint dayInc, uint dayStart)
		{
			m_dayIncrement = dayInc;

			if (m_dayIncrement == 0)
			{
				// day should stand still so pause the timer
				m_dayStartTick = (int)(dayStart);
				m_dayResetTimer.Change(Timeout.Infinite, Timeout.Infinite);
			}
			else
			{
				m_dayStartTick = Environment.TickCount - (int)(dayStart / m_dayIncrement); // set start time to ...
				m_dayResetTimer.Change((DAY - dayStart) / m_dayIncrement, Timeout.Infinite);
			}

			foreach (GameClient client in GetAllPlayingClients())
			{
				if (client.Player != null && client.Player.CurrentRegion != null && client.Player.CurrentRegion.UseTimeManager)
				{
					client.Out.SendTime();
				}
			}
		}


		/// <summary>
		/// Gets the game time for a players current region
		/// </summary>
		/// <param name="client"></param>
		/// <returns></returns>
		public static uint GetCurrentGameTime(GamePlayer player)
		{
			if (player.CurrentRegion != null)
				return player.CurrentRegion.GameTime;

			return GetCurrentGameTime();
		}

		/// <summary>
		/// Gets the current game time
		/// </summary>
		/// <returns>current time</returns>
		public static uint GetCurrentGameTime()
		{
			if (m_dayIncrement == 0)
			{
				return (uint)m_dayStartTick;
			}
			else
			{
				long diff = Environment.TickCount - m_dayStartTick;
				long curTime = diff * m_dayIncrement;
				return (uint)(curTime % DAY);
			}
		}

		/// <summary>
		/// Returns the day increment
		/// </summary>
		/// <returns>the day increment</returns>
		public static uint GetDayIncrement(GamePlayer player)
		{
			if (player.CurrentRegion != null)
				return player.CurrentRegion.DayIncrement;

			return GetDayIncrement();
		}


		public static uint GetDayIncrement()
		{
			return m_dayIncrement;
		}

		/// <summary>
		/// Gets the world update thread stacktrace
		/// </summary>
		/// <returns></returns>
		public static StackTrace GetWorldUpdateStacktrace()
		{
			return Util.GetThreadStack(m_WorldUpdateThread);
		}

		private static uint m_lastWorldObjectUpdateTick = 0;

		/// <summary>
		/// This thread updates the NPCs and objects around the player at very short
		/// intervalls! But since the update is very quick the thread will
		/// sleep most of the time!
		/// </summary>
		private static void WorldUpdateThreadStart()
		{
			bool running = true;
			if (log.IsDebugEnabled)
			{
				log.Debug("NPCUpdateThread ThreadId=" + Thread.CurrentThread.ManagedThreadId);
			}
			while (running)
			{
				try
				{
					int start = Environment.TickCount;
					for (int i = 0; i < m_clients.Length; i++)
					{
						GameClient client = m_clients[i];
						if (client == null)
							continue;

						GamePlayer player = client.Player;
						if (client.ClientState == GameClient.eClientState.Playing && player == null)
						{
							if (log.IsErrorEnabled)
								log.Error("account has no active player but is playing, disconnecting! => " + client.Account.Name);
							GameServer.Instance.Disconnect(client);
							continue;
						}

						if (client.ClientState != GameClient.eClientState.Playing)
							continue;
						if (player.ObjectState != GameObject.eObjectState.Active)
							continue;

						if (Environment.TickCount - player.LastWorldUpdate > (int)(ServerProperties.Properties.WORLD_PLAYER_UPDATE_INTERVAL >= 100 ? ServerProperties.Properties.WORLD_PLAYER_UPDATE_INTERVAL : 100))
						{
							BitArray carray = player.CurrentUpdateArray;
							BitArray narray = player.NewUpdateArray;
							narray.SetAll(false);

							int npcsUpdated = 0, objectsUpdated = 0, doorsUpdated = 0, housesUpdated = 0;

							lock (player.CurrentRegion.ObjectsSyncLock)
							{
								foreach (GameNPC npc in player.GetNPCsInRadius(VISIBILITY_DISTANCE))
								{
									try
									{
										if (npc == null) continue;
										narray[npc.ObjectID - 1] = true;
										if ((uint)Environment.TickCount - npc.LastUpdateTickCount > (ServerProperties.Properties.WORLD_NPC_UPDATE_INTERVAL >= 1000 ? ServerProperties.Properties.WORLD_NPC_UPDATE_INTERVAL : 1000))
										{
											npc.BroadcastUpdate();
											npcsUpdated++;
										}
										else if (carray[npc.ObjectID - 1] == false)
										{
											client.Out.SendObjectUpdate(npc);
											npcsUpdated++;
										}
									}
									catch (Exception e)
									{
										if (log.IsErrorEnabled)
											log.Error("NPC update: " + e.GetType().FullName + " (" + npc.ToString() + ")", e);
									}
								}

								// Broadcast updates of all non-npc objects around this player
								if (ServerProperties.Properties.WORLD_OBJECT_UPDATE_INTERVAL > 0 && (uint)Environment.TickCount - m_lastWorldObjectUpdateTick > (ServerProperties.Properties.WORLD_OBJECT_UPDATE_INTERVAL >= 10000 ? ServerProperties.Properties.WORLD_OBJECT_UPDATE_INTERVAL : 10000))
								{
									foreach (GameStaticItem item in client.Player.GetItemsInRadius(WorldMgr.OBJ_UPDATE_DISTANCE))
									{
										client.Out.SendObjectCreate(item);
										objectsUpdated++;
									}

									foreach (IDoor door in client.Player.GetDoorsInRadius(WorldMgr.OBJ_UPDATE_DISTANCE))
									{
										client.Player.SendDoorUpdate(door);
										doorsUpdated++;
									}

									//housing
									if (client.Player.CurrentRegion.HousingEnabled)
									{
										if (client.Player.HousingUpdateArray == null)
										{
											client.Player.HousingUpdateArray = new BitArray(ServerProperties.Properties.MAX_NUM_HOUSES, false);
										}

										var houses = HouseMgr.GetHouses(client.Player.CurrentRegionID);
										if (houses != null)
										{
											foreach (House house in houses.Values)
											{
												if (house.UniqueID < client.Player.HousingUpdateArray.Length)
												{
													if (client.Player.IsWithinRadius(house, HousingConstants.HouseViewingDistance))
													{
														if (!client.Player.HousingUpdateArray[house.UniqueID])
														{
															client.Out.SendHouse(house);
															client.Out.SendGarden(house);

															if (house.IsOccupied)
															{
																client.Out.SendHouseOccupied(house, true);
															}

															client.Player.HousingUpdateArray[house.UniqueID] = true;
															housesUpdated++;
														}
													}
													else
													{
														client.Player.HousingUpdateArray[house.UniqueID] = false;
													}
												}
											}
										}
									}
									else if (client.Player.HousingUpdateArray != null)
									{
										client.Player.HousingUpdateArray = null;
									}

									m_lastWorldObjectUpdateTick = (uint)Environment.TickCount;
								}
							}

							player.SwitchUpdateArrays();
							player.LastWorldUpdate = Environment.TickCount;

							// log.DebugFormat("Player {0} world update: {1} npcs, {2} objects, {3} doors, and {4} houses in {5}ms", player.Name, npcsUpdated, objectsUpdated, doorsUpdated, housesUpdated, Environment.TickCount - start);
						}
					}

					int took = Environment.TickCount - start;
					if (took > 500)
					{
						if (log.IsWarnEnabled)
							log.WarnFormat("NPC update took {0}ms", took);
					}

					Thread.Sleep(50);
				}
				catch (ThreadAbortException)
				{
					if (log.IsDebugEnabled)
						log.Debug("NPC Update Thread stopping...");
					running = false;
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error("Error in NPC Update Thread!", e);
				}
			}
		}

		/// <summary>
		/// Cleans up and stops all the RegionMgr tasks inside
		/// the regions.
		/// </summary>
		public static void Exit()
		{
			try
			{
				if (m_pingCheckTimer != null)
				{
					m_pingCheckTimer.Dispose();
					m_pingCheckTimer = null;
				}
				if (m_dayResetTimer != null)
				{
					m_dayResetTimer.Dispose();
					m_dayResetTimer = null;
				}
				if (m_WorldUpdateThread != null)
				{
					m_WorldUpdateThread.Abort();
					m_WorldUpdateThread = null;
				}
				if (m_relocationThread != null)
				{
					m_relocationThread.Abort();
					m_relocationThread = null;
				}

				//Stop all mobMgrs
				StopRegionMgrs();
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("Exit", e);
			}
		}

		/// <summary>
		/// Creates and adds a new region to the WorldMgr
		/// </summary>
		/// <param name="time">Time manager for the region</param>
		/// <param name="data">The region data</param>
		/// <returns>Registered region</returns>
		public static Region RegisterRegion(GameTimer.TimeManager time, RegionData data)
		{
			Region region = new Region(time, data);
			lock (m_regions.SyncRoot)
			{
				m_regions.Add(data.Id, region);
			}
			return region;
		}

		/// <summary>
		/// Creates an array of region entries
		/// </summary>
		/// <returns>An array of regions available on the server</returns>
		public static RegionEntry[] GetRegionList()
		{
			RegionEntry[] regs = null;

			lock (m_regions.SyncRoot)
			{
				regs = new RegionEntry[m_regions.Count];

				IDictionaryEnumerator iter = m_regions.GetEnumerator();

				string port = string.Format("{0:D00000}", GameServer.Instance.Configuration.RegionPort);
				string ip = GameServer.Instance.Configuration.RegionIP.ToString();

				int i = 0;

				while (iter.MoveNext())
				{
					Region reg = (Region)iter.Value;
					//Dinberg - we want to use the skinID, not the ID, for regions like this.
					regs[i].id = reg.ID; // reg.Skin;// reg.ID; //Dinberg - changed it back.
					regs[i].ip = ip;
					regs[i].toPort = port;
					regs[i].name = reg.Name;
					regs[i].fromPort = port;
					regs[i].expansion = reg.Expansion;

					++i;
				}
			}

			return regs;
		}

		/// <summary>
		/// Returns all the regions of the world
		/// </summary>
		/// <returns></returns>
		public static ICollection GetAllRegions()
		{
			return m_regions.Values;
		}

		/// <summary>
		/// Registers a Zone into a Region
		/// </summary>
		public static void RegisterZone(ZoneData zoneData, ushort zoneID, ushort regionID, string zoneName, int xpBonus, int rpBonus, int bpBonus, int coinBonus)
		{
			Region region = GetRegion(regionID);
			if (region == null)
			{
				if (log.IsWarnEnabled)
				{
					log.Warn("Could not find Region " + regionID + " for Zone " + zoneData.Description);
				}
				return;
			}
			
			// Making an assumption that a zone waterlevel of 0 means it is not set and we should use the regions waterlevel - Tolakram
			if (zoneData.WaterLevel == 0)
			{
				zoneData.WaterLevel = region.WaterLevel;
			}

			bool isDivingEnabled = region.IsRegionDivingEnabled;

			if (zoneData.DivingFlag == 1)
				isDivingEnabled = true;
			else if (zoneData.DivingFlag == 2)
				isDivingEnabled = false;
			
			Zone zone = new Zone(region,
			                     zoneID,
			                     zoneName,
			                     zoneData.OffX * 8192,
			                     zoneData.OffY * 8192,
			                     zoneData.Width * 8192,
			                     zoneData.Height * 8192,
			                     zoneData.ZoneID,
			                     isDivingEnabled,
			                     zoneData.WaterLevel,
			                     zoneData.IsLava,
			                     xpBonus,
			                     rpBonus,
			                     bpBonus,
			                     coinBonus);

			//Dinberg:Instances
			//ZoneID will always be constant as last parameter, because ZoneSkinID will effectively be a bluff, to remember
			//the original region that spawned this one!

			/*reg,
                    zoneID,
                    desc,
                    offx * 8192,
                    offy * 8192,
                    width * 8192,
                    height * 8192);*/

			lock (region.Zones.SyncRoot)
			{
				region.Zones.Add(zone);
			}
			lock (m_zones.SyncRoot)
			{
				m_zones.Add(zoneID, zone);
			}

			log.Info("Added a zone, " + zoneData.Description + ", to region " + region.Name);
		}

		/// <summary>
		/// Starts all RegionMgrs inside the Regions
		/// </summary>
		/// <returns>true</returns>
		public static bool StartRegionMgrs()
		{
			lock (m_regions.SyncRoot)
			{
				foreach (Region reg in m_regions.Values)
					reg.StartRegionMgr();
			}
			return true;
		}

		/// <summary>
		/// Stops all Regionmgrs inside the Regions
		/// </summary>
		public static void StopRegionMgrs()
		{
			if (log.IsDebugEnabled)
				log.Debug("Stopping region managers...");
			lock (m_regions.SyncRoot)
			{
				foreach (Region reg in m_regions.Values)
				{
					//DOLConsole.WriteLine("Stopping region manager for region "+reg.Description+"...");
					reg.StopRegionMgr();
				}
			}
			if (log.IsDebugEnabled)
				log.Debug("Region managers stopped.");
		}

		/// <summary>
		/// Fetch a Region by it's ID
		/// </summary>
		/// <param name="regionID">ID to search</param>
		/// <returns>Region or null if not found</returns>
		public static Region GetRegion(ushort regionID)
		{
			return (Region)m_regions[regionID];
		}

		public static ushort m_lastZoneError = 0;

		/// <summary>
		/// Gets a Zone object by it's ID
		/// </summary>
		/// <param name="zoneID">the zoneID</param>
		/// <returns>the zone object or null</returns>
		public static Zone GetZone(ushort zoneID)
		{
			if (!m_zones.Contains(zoneID))
			{
				if (m_lastZoneError != zoneID)
				{
					log.Error("Trying to access inexistent ZoneID " + zoneID + " " + Environment.StackTrace);
					m_lastZoneError = zoneID;
				}

				return null;
			}

			return (Zone)m_zones[zoneID];
		}


		/// <summary>
		/// Creates a new SessionID for a GameClient object
		/// </summary>
		/// <param name="obj">The GameClient for which we need an ID</param>
		/// <returns>The new ID or -1 if none free</returns>
		public static int CreateSessionID(GameClient obj)
		{
			lock (m_clients.SyncRoot)
			{
				for (int i = 0; i < m_clients.Length; i++)
					if (m_clients[i] == null)
				{
					m_clients[i] = obj;
					obj.SessionID = i + 1;
					return i + 1;
				}
			}
			return -1;
		}

		/// <summary>
		/// Searches for all objects from a specific region
		/// </summary>
		/// <param name="regionID">The region to search</param>
		/// <param name="objectType">The type of the object you search</param>
		/// <returns>All objects with the specified parameters</returns>
		public static GameObject[] GetobjectsFromRegion( ushort regionID, Type objectType)
		{
			Region reg = (Region)m_regions[regionID];
			if (reg == null)
				return new GameObject[0];
			GameObject[] objs = reg.Objects;
			ArrayList returnObjs = new ArrayList();
			for (int i = 0; i < objs.Length; i++)
			{
				GameObject obj = objs[i];
				if (obj != null && objectType.IsInstanceOfType(obj))
					returnObjs.Add(obj);
			}
			return (GameObject[])returnObjs.ToArray(objectType);
		}
		
		/// <summary>
		/// Searches for all GameStaticItem from a specific region
		/// </summary>
		/// <param name="regionID">The region to search</param>
		/// <returns>All NPCs with the specified parameters</returns>
		public static GameStaticItem[] GetStaticItemFromRegion(ushort regionID)
		{
			return (GameStaticItem[])GetobjectsFromRegion(regionID, typeof(GameStaticItem));
		}

		/// <summary>
		/// Searches for all objects with the given name, from a specific region and realm
		/// </summary>
		/// <param name="name">The name of the object to search</param>
		/// <param name="regionID">The region to search</param>
		/// <param name="realm">The realm of the object we search!</param>
		/// <param name="objectType">The type of the object you search</param>
		/// <returns>All objects with the specified parameters</returns>
		public static GameObject[] GetObjectsByNameFromRegion(string name, ushort regionID, eRealm realm, Type objectType)
		{
			Region reg = (Region)m_regions[regionID];
			if (reg == null)
				return new GameObject[0];
			GameObject[] objs = reg.Objects;
			ArrayList returnObjs = new ArrayList();
			for (int i = 0; i < objs.Length; i++)
			{
				GameObject obj = objs[i];
				if (obj != null && objectType.IsInstanceOfType(obj) && obj.Realm == realm && obj.Name == name)
					returnObjs.Add(obj);
			}
			return (GameObject[])returnObjs.ToArray(objectType);
		}

		//Added by Dinberg, i want to know if we should so freely enumerate reg.Objects?
		/// <summary>
		/// Returns the npcs in a given region
		/// </summary>
		/// <returns></returns>
		public static GameNPC[] GetNPCsFromRegion(ushort regionID)
		{
			Region reg = (Region)m_regions[regionID];
			if (reg == null)
				return new GameNPC[0];
			GameObject[] objs = reg.Objects;
			ArrayList returnObjs = new ArrayList();
			for (int i = 0; i < objs.Length; i++)
			{
				GameNPC obj = objs[i] as GameNPC;
				if (obj != null)
					returnObjs.Add(obj);
			}
			return (GameNPC[])returnObjs.ToArray(typeof(GameNPC));
		}

		/// <summary>
		/// Searches for all objects with the given name and realm in ALL regions!
		/// </summary>
		/// <param name="name">The name of the object to search</param>
		/// <param name="realm">The realm of the object we search!</param>
		/// <param name="objectType">The type of the object you search</param>
		/// <returns>All objects with the specified parameters</returns>b
		public static GameObject[] GetObjectsByName(string name, eRealm realm, Type objectType)
		{
			ArrayList returnObjs = new ArrayList();
			foreach (Region reg in m_regions.Values)
				returnObjs.AddRange(GetObjectsByNameFromRegion(name, reg.ID, realm, objectType));
			return (GameObject[])returnObjs.ToArray(objectType);
		}

		/// <summary>
		/// Searches for all NPCs with the given name, from a specific region and realm
		/// </summary>
		/// <param name="name">The name of the object to search</param>
		/// <param name="regionID">The region to search</param>
		/// <param name="realm">The realm of the object we search!</param>
		/// <returns>All NPCs with the specified parameters</returns>
		public static GameNPC[] GetNPCsByNameFromRegion(string name, ushort regionID, eRealm realm)
		{
			return (GameNPC[])GetObjectsByNameFromRegion(name, regionID, realm, typeof(GameNPC));
		}

		/// <summary>
		/// Searches for all NPCs with the given name and realm in ALL regions!
		/// </summary>
		/// <param name="name">The name of the object to search</param>
		/// <param name="realm">The realm of the object we search!</param>
		/// <returns>All NPCs with the specified parameters</returns>b
		public static GameNPC[] GetNPCsByName(string name, eRealm realm)
		{
			return (GameNPC[])GetObjectsByName(name, realm, typeof(GameNPC));
		}

		/// <summary>
		/// Searches for all NPCs with the given guild and realm in ALL regions!
		/// </summary>
		/// <param name="guild">The guild name for the npc</param>
		/// <param name="realm">The realm of the npc</param>
		/// <returns>A collection of NPCs which match the result</returns>
		public static List<GameNPC> GetNPCsByGuild(string guild, eRealm realm)
		{
			List<GameNPC> returnNPCs = new List<GameNPC>();
			foreach (Region r in m_regions.Values)
			{
				foreach (GameObject obj in r.Objects)
				{
					GameNPC npc = obj as GameNPC;
					if (npc == null)
						continue;
					if (npc.Realm == realm && npc.GuildName == guild)
						returnNPCs.Add(npc);
				}
			}
			return returnNPCs;
		}

		/// <summary>
		/// Searches for all NPCs with the given type and realm in ALL regions!
		/// </summary>
		/// <param name="type"></param>
		/// <param name="realm"></param>
		/// <returns></returns>
		public static List<GameNPC> GetNPCsByType(Type type, eRealm realm)
		{
			List<GameNPC> returnNPCs = new List<GameNPC>();
			foreach (Region r in m_regions.Values)
			{
				foreach (GameObject obj in r.Objects)
				{
					GameNPC npc = obj as GameNPC;
					if (npc == null)
						continue;
					if (npc.Realm == realm && type.IsInstanceOfType(npc))
						returnNPCs.Add(npc);
				}
			}
			return returnNPCs;
		}

		/// <summary>
		/// Searches for all NPCs with the given type and realm in a specific region
		/// </summary>
		/// <param name="type"></param>
		/// <param name="realm"></param>
		/// <returns></returns>
		public static List<GameNPC> GetNPCsByType(Type type, eRealm realm, ushort region)
		{
			List<GameNPC> returnNPCs = new List<GameNPC>();
			Region r = GetRegion(region);
			if (r == null)
				return returnNPCs;
			foreach (GameObject obj in r.Objects)
			{
				GameNPC npc = obj as GameNPC;
				if (npc == null)
					continue;
				if (npc.Realm == realm && type.IsInstanceOfType(npc))
					returnNPCs.Add(npc);
			}
			return returnNPCs;
		}

		/// <summary>
		/// Fetch a GameClient based on it's ID
		/// </summary>
		/// <param name="id">ID to search</param>
		/// <returns>The found GameClient or null if not found</returns>
		public static GameClient GetClientFromID(int id)
		{
			int i = id;
			if (i <= 0 || i > m_clients.Length)
				return null;
			return m_clients[i - 1];
		}

		/// <summary>
		/// Removes a GameClient and free's it's ID again!
		/// </summary>
		/// <param name="entry">The GameClient to be removed</param>
		public static void RemoveClient(GameClient entry)
		{
			if (entry == null)
				return;
			int sessionid = -1;
			int i = 1;
			lock (m_clients.SyncRoot)
			{
				foreach (GameClient client in m_clients)
				{
					if (client == entry)
					{
						sessionid = i;
						break;
					}
					i++;
				}
			}

			// do NOT remove sessionid in lock of clients
			// or a deadlock can occur under certain circumstances!
			if (sessionid > 0)
			{
				RemoveSessionID(sessionid);
			}
		}

		/// <summary>
		/// Removes a GameClient based on it's ID
		/// </summary>
		/// <param name="id">The SessionID to free</param>
		public static void RemoveSessionID(int id)
		{
			GameClient client;
			lock (m_clients.SyncRoot)
			{
				client = m_clients[id - 1];
				m_clients[id - 1] = null;
			}
			if (client == null)
				return;
			if (client.Player == null)
				return;
			//client.Player.RemoveFromWorld();
			client.Player.Delete();
			return;
		}

		//Various functions to get a list of players/mobs/items
		#region getdistance
		/// <summary>
		/// Get's the distance of two GameObjects
		/// </summary>
		/// <param name="obj1">Object1</param>
		/// <param name="obj2">Object2</param>
		/// <returns>The distance in units or -1 if they are not the same Region</returns>
		[Obsolete( "Use Point3D.GetDistance" )]
		public static int GetDistance( GameObject obj1, GameObject obj2 )
		{
			if ( obj1 == null || obj2 == null || obj1.CurrentRegion != obj2.CurrentRegion )
				return -1;
			return GetDistance( obj1.X, obj1.Y, obj1.Z, obj2.X, obj2.Y, obj2.Z );
		}

		/// <summary>
		/// Get's the distance of two GameObjects
		/// </summary>
		/// <param name="obj1">Object1</param>
		/// <param name="obj2">Object2</param>
		/// <param name="zfactor">Factor for Z distance use lower 0..1 to lower Z influence</param>
		/// <returns>The distance in units or -1 if they are not the same Region</returns>
		[Obsolete( "Use Point3D.GetDistance" )]
		public static int GetDistance( GameObject obj1, GameObject obj2, double zfactor )
		{
			if ( obj1 == null || obj2 == null || obj1.CurrentRegion != obj2.CurrentRegion )
				return -1;
			return GetDistance( obj1.X, obj1.Y, obj1.Z, obj2.X, obj2.Y, obj2.Z, zfactor );
		}

		/// <summary>
		/// Gets the distance of two arbitary points in space
		/// </summary>
		/// <param name="x1">X of Point1</param>
		/// <param name="y1">Y of Point1</param>
		/// <param name="z1">Z of Point1</param>
		/// <param name="x2">X of Point2</param>
		/// <param name="y2">Y of Point2</param>
		/// <param name="z2">Z of Point2</param>
		/// <returns>The distance</returns>
		[Obsolete( "Use Point3D.GetDistance" )]
		public static int GetDistance( int x1, int y1, int z1, int x2, int y2, int z2 )
		{
			long xdiff = (long)x1 - x2;
			long ydiff = (long)y1 - y2;

			long zdiff = (long)z1 - z2;
			return (int)Math.Sqrt( xdiff * xdiff + ydiff * ydiff + zdiff * zdiff );
		}

		/// <summary>
		/// Gets the distance of two arbitary points in space
		/// </summary>
		/// <param name="x1">X of Point1</param>
		/// <param name="y1">Y of Point1</param>
		/// <param name="z1">Z of Point1</param>
		/// <param name="x2">X of Point2</param>
		/// <param name="y2">Y of Point2</param>
		/// <param name="z2">Z of Point2</param>
		/// <param name="zfactor">Factor for Z distance use lower 0..1 to lower Z influence</param>
		/// <returns>The distance</returns>
		[Obsolete( "Use Point3D.GetDistance" )]
		public static int GetDistance( int x1, int y1, int z1, int x2, int y2, int z2, double zfactor )
		{
			long xdiff = (long)x1 - x2;
			long ydiff = (long)y1 - y2;

			long zdiff = (long)( ( z1 - z2 ) * zfactor );
			return (int)Math.Sqrt( xdiff * xdiff + ydiff * ydiff + zdiff * zdiff );
		}

		/// <summary>
		/// Gets the distance of an Object to an arbitary point
		/// </summary>
		/// <param name="obj">GameObject used as Point1</param>
		/// <param name="x">X of Point2</param>
		/// <param name="y">Y of Point2</param>
		/// <param name="z">Z of Point2</param>
		/// <returns>The distance</returns>
		[Obsolete( "Use Point3D.GetDistance" )]
		public static int GetDistance( GameObject obj, int x, int y, int z )
		{
			return GetDistance( obj.X, obj.Y, obj.Z, x, y, z );
		}
		#endregion get distance

		#region check distance
		[Obsolete( "Use Point3D.IsWithinRadius" )]
		public static bool CheckDistance(int x1, int y1, int z1, int x2, int y2, int z2, int radius)
		{
			return CheckSquareDistance(x1, y1, z1, x2, y2, z2, radius * radius);
		}
		[Obsolete( "Use Point3D.IsWithinRadius" )]
		public static bool CheckDistance( IPoint3D obj, IPoint3D obj2, int radius )
		{
			return CheckDistance(obj.X, obj.Y, obj.Z, obj2.X, obj2.Y, obj2.Z, radius);
		}
		[Obsolete( "Use Point3D.IsWithinRadius" )]
		public static bool CheckDistance( GameObject obj, int x2, int y2, int z2, int radius )
		{
			return CheckDistance(obj.X, obj.Y, obj.Z, x2, y2, z2, radius);
		}
		[Obsolete( "Use Point3D.IsWithinRadius" )]
		public static bool CheckDistance( GameObject obj, GameObject obj2, int radius )
		{
			if (obj == null || obj2 == null)
				return false;
			if (obj.CurrentRegion != obj2.CurrentRegion)
				return false;
			return CheckDistance(obj.X, obj.Y, obj.Z, obj2.X, obj2.Y, obj2.Z, radius);
		}
		#endregion
		#region check square distance
		[Obsolete( "Use Point3D.IsWithinRadius" )]
		private static bool CheckSquareDistance( int x1, int y1, int z1, int x2, int y2, int z2, int squareRadius )
		{
			long xdiff = (long)x1 - x2;
			long ydiff = (long)y1 - y2;
			long zdiff = (long)z1 - z2;
			return (xdiff * xdiff + ydiff * ydiff + zdiff * zdiff <= squareRadius);
		}
		[Obsolete( "Use Point3D.IsWithinRadius" )]
		private static bool CheckSquareDistance( IPoint3D obj, IPoint3D obj2, int squareRadius )
		{
			return CheckSquareDistance(obj.X, obj.Y, obj.Z, obj2.X, obj2.Y, obj2.Z, squareRadius);
		}
		[Obsolete( "Use Point3D.IsWithinRadius" )]
		private static bool CheckSquareDistance( GameObject obj, int x2, int y2, int z2, int squareRadius )
		{
			return CheckSquareDistance(obj.X, obj.Y, obj.Z, x2, y2, z2, squareRadius);
		}
		[Obsolete( "Use Point3D.IsWithinRadius" )]
		private static bool CheckSquareDistance( GameObject obj, GameObject obj2, int squareRadius )
		{
			if (obj.CurrentRegion != obj2.CurrentRegion)
				return false;
			return CheckSquareDistance(obj.X, obj.Y, obj.Z, obj2.X, obj2.Y, obj2.Z, squareRadius);
		}
		#endregion

		/// <summary>
		/// Returns the number of playing Clients inside a realm
		/// </summary>
		/// <param name="realmID">ID of Realm (1=Alb, 2=Mid, 3=Hib)</param>
		/// <returns>Client count of that realm</returns>
		public static int GetClientsOfRealmCount(eRealm realm)
		{
			int count = 0;
			lock (m_clients.SyncRoot)
			{
				foreach (GameClient client in m_clients)
				{
					if (client != null)
					{
						if (client.IsPlaying
						    && client.Player != null
						    && client.Player.ObjectState == GameObject.eObjectState.Active
						    && client.Player.Realm == realm)
							count++;
					}
				}
			}
			return count;
		}

		/// <summary>
		/// Returns an array of GameClients currently playing from a specific realm
		/// </summary>
		/// <param name="realmID">ID of Realm (1=Alb, 2=Mid, 3=Hib)</param>
		/// <returns>An ArrayList of clients</returns>
		public static IList<GameClient> GetClientsOfRealm(eRealm realm)
		{
			var targetClients = new List<GameClient>();

			lock (m_clients.SyncRoot)
			{
				foreach (GameClient client in m_clients)
				{
					if (client != null)
					{
						if (client.IsPlaying
						    && client.Player != null
						    && client.Player.ObjectState == GameObject.eObjectState.Active
						    && client.Player.Realm == realm)
							targetClients.Add(client);
					}
				}
			}

			return targetClients;
		}

		/// <summary>
		/// Returns the number of playing Clients in a certain Region
		/// </summary>
		/// <param name="regionID">The ID of the Region</param>
		/// <returns>Number of playing Clients in that Region</returns>
		public static int GetClientsOfRegionCount(ushort regionID)
		{
			int count = 0;
			lock (m_clients.SyncRoot)
			{
				foreach (GameClient client in m_clients)
				{
					if (client != null)
					{
						if (client.IsPlaying
						    && client.Player != null
						    && client.Player.ObjectState == GameObject.eObjectState.Active
						    && client.Player.CurrentRegionID == regionID)
							count++;
					}
				}
			}
			return count;
		}

		/// <summary>
		/// Returns the number of playing Clients in a certain Region
		/// </summary>
		/// <param name="regionID">The ID of the Region</param>
		/// <param name="realm">The realm of clients to check</param>
		/// <returns>Number of playing Clients in that Region</returns>
		public static int GetClientsOfRegionCount(ushort regionID, eRealm realm)
		{
			int count = 0;
			lock (m_clients.SyncRoot)
			{
				foreach (GameClient client in m_clients)
				{
					if (client != null)
					{
						if (client.IsPlaying
						    && client.Player != null
						    && client.Player.ObjectState == GameObject.eObjectState.Active
						    && client.Player.CurrentRegionID == regionID
						    && client.Player.Realm == realm)
							count++;
					}
				}
			}
			return count;
		}

		/// <summary>
		/// Returns a list of playing clients inside a region
		/// </summary>
		/// <param name="regionID">The ID of the Region</param>
		/// <returns>Array of GameClients from that Region</returns>
		public static IList<GameClient> GetClientsOfRegion(ushort regionID)
		{
			var targetClients = new  List<GameClient>();

			lock (m_clients.SyncRoot)
			{
				foreach (GameClient client in m_clients)
				{
					if (client != null)
					{
						if (client.IsPlaying
						    && client.Player != null
						    && client.Player.ObjectState == GameObject.eObjectState.Active
						    && client.Player.CurrentRegionID == regionID)
							targetClients.Add(client);
					}
				}
			}

			return targetClients;
		}

		/// <summary>
		/// Find a GameClient by the Player's ID
		/// Case-insensitive, make sure you use returned Player.Name instead of what player typed.
		/// </summary>
		/// <param name="playerID">ID to search</param>
		/// <param name="exactMatch">true if AccountName match exactly</param>
		/// <param name="activeRequired"></param>
		/// <returns>The found GameClient or null</returns>
		public static GameClient GetClientByPlayerID(string playerID, bool exactMatch, bool activeRequired)
		{
			foreach (GameClient client in WorldMgr.GetAllPlayingClients())
			{
				if (client.Player.InternalID == playerID)
					return client;
			}
			return null;
		}
		
		/// <summary>
		/// Finds a GameClient by the AccountName
		/// </summary>
		/// <param name="accountName">AccountName to search</param>
		/// <param name="exactMatch">true if AccountName match exactly</param>
		/// <returns>The found GameClient or null</returns>
		public static GameClient GetClientByAccountName(string accountName, bool exactMatch)
		{
			accountName = accountName.ToLower();
			lock (m_clients.SyncRoot)
			{
				foreach (GameClient client in m_clients)
				{
					if (client != null)
					{
						if ((exactMatch && client.Account.Name.ToLower() == accountName)
						    || (!exactMatch && client.Account.Name.ToLower().StartsWith(accountName)))
						{
							return client;
						}
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Find a GameClient by the Player's name
		/// Case-insensitive, make sure you use returned Player.Name instead of what player typed.
		/// </summary>
		/// <param name="playerName">Name to search</param>
		/// <param name="exactMatch">true if AccountName match exactly</param>
		/// <param name="activeRequired"></param>
		/// <returns>The found GameClient or null</returns>
		public static GameClient GetClientByPlayerName(string playerName, bool exactMatch, bool activeRequired)
		{
			if (exactMatch)
			{
				return GetClientByPlayerNameAndRealm(playerName, 0, activeRequired);
			}
			else
			{
				int x = 0;
				return GuessClientByPlayerNameAndRealm(playerName, 0, activeRequired, out x);
			}
		}

		/// <summary>
		/// Find a GameClient by the Player's name.
		/// Case-insensitive now, make sure you use returned Player.Name instead of what player typed.
		/// </summary>
		/// <param name="playerName">Name to search</param>
		/// <param name="realmID">search in: 0=all realms or player.Realm</param>
		/// <param name="activeRequired"></param>
		/// <returns>The found GameClient or null</returns>
		public static GameClient GetClientByPlayerNameAndRealm(string playerName, eRealm realm, bool activeRequired)
		{
			lock (m_clients.SyncRoot)
			{
				foreach (GameClient client in m_clients)
				{
					if (client != null && client.Player != null && (realm == eRealm.None || client.Player.Realm == realm))
					{
						if (activeRequired && (!client.IsPlaying || client.Player.ObjectState != GameObject.eObjectState.Active))
							continue;
						if (0 == string.Compare(client.Player.Name, playerName, true)) // case insensitive comapre
						{
							return client;
						}
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Guess a GameClient by first letters of Player's name
		/// Case-insensitive, make sure you use returned Player.Name instead of what player typed.
		/// </summary>
		/// <param name="playerName">Name to search</param>
		/// <param name="realmID">search in: 0=all realms or player.Realm</param>
		/// <param name="result">returns: 1=no name found, 2=name is not unique, 3=exact match, 4=guessed name</param>
		/// <param name="activeRequired"></param>
		/// <returns>The found GameClient or null</returns>
		public static GameClient GuessClientByPlayerNameAndRealm(string playerName, eRealm realm, bool activeRequired, out int result)
		{
			// first try exact match in case player with "abcde" name is
			// before "abc" in list and user typed "abc"
			GameClient guessedClient = GetClientByPlayerNameAndRealm(playerName, realm, activeRequired);
			if (guessedClient != null)
			{
				result = 3; // exact match
				return guessedClient;
			}

			// now trying to guess
			string compareName = playerName.ToLower();
			result = 1; // no name found
			lock (m_clients.SyncRoot)
			{
				foreach (GameClient client in m_clients)
				{
					if (client != null && client.Player != null)
					{
						if (activeRequired && (!client.IsPlaying || client.Player.ObjectState != GameObject.eObjectState.Active))
							continue;
						if (realm == eRealm.None || client.Player.Realm == realm)
						{
							if (client.Player.Name.ToLower().StartsWith(compareName))
							{
								if (result == 4) // keep looking to be sure that name is unique
								{
									result = 2; // name not unique
									break;
								}
								else
								{
									result = 4; // guessed name
									guessedClient = client;
								}
							}
						}
					}
				}
			}
			return guessedClient;
		}

		/// <summary>
		/// Find a GameClient by the Player's name from a specific region
		/// </summary>
		/// <param name="playerName">Name to search</param>
		/// <param name="regionID">Region ID of region to search through</param>
		/// <param name="exactMatch">true if the Name must match exactly</param>
		/// <param name="activeRequired"></param>
		/// <returns>The first found GameClient or null</returns>
		public static GameClient GetClientByPlayerNameFromRegion(string playerName, ushort regionID, bool exactMatch, bool activeRequired)
		{
			GameClient client = GetClientByPlayerName(playerName, exactMatch, activeRequired);
			if (client == null || client.Player.CurrentRegionID != regionID)
				return null;
			return client;
		}

		/// <summary>
		/// Gets a copy of all playing clients
		/// </summary>
		/// <returns>ArrayList of playing GameClients</returns>
		public static IList<GameClient> GetAllPlayingClients()
		{
			var targetClients = new List<GameClient>();

			lock (m_clients.SyncRoot)
			{
				foreach (GameClient client in m_clients)
				{
					if (client != null
					    && client.IsPlaying
					    && client.Player != null
					    && client.Player.ObjectState == GameObject.eObjectState.Active)
						targetClients.Add(client);
				}
			}
			return targetClients;
		}

		/// <summary>
		/// Returns the number of all playing clients
		/// </summary>
		/// <returns>Count of all playing clients</returns>
		public static int GetAllPlayingClientsCount()
		{
			int count = 0;
			lock (m_clients.SyncRoot)
			{
				foreach (GameClient client in m_clients)
				{
					if (client != null
					    && client.IsPlaying
					    && client.Player != null
					    && client.Player.ObjectState == GameObject.eObjectState.Active)
						count++;
				}
			}
			return count;
		}

		/// <summary>
		/// Gets a copy of ALL clients no matter at what state they are
		/// </summary>
		/// <returns>ArrayList of GameClients</returns>
		public static IList<GameClient> GetAllClients()
		{
			var targetClients = new List<GameClient>();

			lock (m_clients.SyncRoot)
			{
				foreach (GameClient client in m_clients)
				{
					if (client != null)
						targetClients.Add(client);
				}
			}

			return targetClients;
		}

		/// <summary>
		/// Gets a count of ALL clients no matter at what state they are
		/// </summary>
		/// <returns>ArrayList of GameClients</returns>
		public static int GetAllClientsCount()
		{
			int count = 0;
			lock (m_clients.SyncRoot)
			{
				foreach (GameClient client in m_clients)
				{
					if (client != null)
						count++;
				}
			}
			return count;
		}

		/// <summary>
		/// Fetch an Object from a specific Region by it's ID
		/// </summary>
		/// <param name="regionID">Region ID of Region to search through</param>
		/// <param name="oID">Object ID to search</param>
		/// <returns>GameObject found in the Region or null</returns>
		public static GameObject GetObjectByIDFromRegion(ushort regionID, ushort oID)
		{
			Region reg = GetRegion(regionID);
			if (reg == null)
				return null;
			return reg.GetObject(oID);
		}

		/// <summary>
		/// Fetch an Object of specific type from a specific Region
		/// </summary>
		/// <param name="regionID">Region ID of Regin to search through</param>
		/// <param name="oID">Object ID to search</param>
		/// <param name="type">Type of Object to search</param>
		/// <returns>GameObject of specific type or null if not found</returns>
		public static GameObject GetObjectTypeByIDFromRegion(ushort regionID, ushort oID, Type type)
		{
			GameObject obj = GetObjectByIDFromRegion(regionID, oID);
			if (obj == null || !type.IsInstanceOfType(obj))
				return null;
			return obj;
		}

		/// <summary>
		/// Returns an IEnumerator of GamePlayers that are close to a certain
		/// spot in the region
		/// </summary>
		/// <param name="regionid">Region to search</param>
		/// <param name="x">X inside region</param>
		/// <param name="y">Y inside region</param>
		/// <param name="z">Z inside region</param>
		/// <param name="withDistance">Wether or not to return the objects with distance</param>
		/// <param name="radiusToCheck">Radius to sarch for GameClients</param>
		/// <returns>IEnumerator that can be used to go through all players</returns>
		public static IEnumerable GetPlayersCloseToSpot(ushort regionid, int x, int y, int z, ushort radiusToCheck, bool withDistance)
		{
			Region reg = GetRegion(regionid);
			if (reg == null)
				return new Region.EmptyEnumerator();
			return reg.GetPlayersInRadius(x, y, z, radiusToCheck, withDistance, false);
		}

		/// <summary>
		/// Returns an IEnumerator of GamePlayers that are close to a certain
		/// spot in the region
		/// </summary>
		/// <param name="location">the game location to search from</param>
		/// <param name="radiusToCheck">Radius to sarch for GameClients</param>
		/// <returns>IEnumerator that can be used to go through all players</returns>
		public static IEnumerable GetPlayersCloseToSpot(IGameLocation location, ushort radiusToCheck)
		{
			return GetPlayersCloseToSpot(location.RegionID, location.X, location.Y, location.Z, radiusToCheck, false);
		}

		/// <summary>
		/// Returns an IEnumerator of GamePlayers that are close to a certain
		/// spot in the region
		/// </summary>
		/// <param name="regionid">Region to search</param>
		/// <param name="point">the 3D point to search from</param>
		/// <param name="radiusToCheck">Radius to sarch for GameClients</param>
		/// <returns>IEnumerator that can be used to go through all players</returns>
		public static IEnumerable GetPlayersCloseToSpot(ushort regionid, IPoint3D point, ushort radiusToCheck)
		{
			return GetPlayersCloseToSpot(regionid, point.X, point.Y, point.Z, radiusToCheck, false);
		}

		/// <summary>
		/// Returns an IEnumerator of GamePlayers that are close to a certain
		/// spot in the region
		/// </summary>
		/// <param name="regionid">Region to search</param>
		/// <param name="x">X inside region</param>
		/// <param name="y">Y inside region</param>
		/// <param name="z">Z inside region</param>
		/// <param name="radiusToCheck">Radius to sarch for GameClients</param>
		/// <returns>IEnumerator that can be used to go through all players</returns>
		public static IEnumerable GetPlayersCloseToSpot(ushort regionid, int x, int y, int z, ushort radiusToCheck)
		{
			return GetPlayersCloseToSpot(regionid, x, y, z, radiusToCheck, false);
		}

		/// <summary>
		/// Returns an IEnumerator of GameNPCs that are close to a certain
		/// spot in the region
		/// </summary>
		/// <param name="regionid">Region to search</param>
		/// <param name="x">X inside region</param>
		/// <param name="y">Y inside region</param>
		/// <param name="z">Z inside region</param>
		/// <param name="radiusToCheck">Radius to sarch for GameNPCs</param>
		/// <param name="withDistance">Wether or not to return the objects with distance</param>
		/// <returns>IEnumerator that can be used to go through all NPCs</returns>
		public static IEnumerable GetNPCsCloseToSpot(ushort regionid, int x, int y, int z, ushort radiusToCheck, bool withDistance)
		{
			Region reg = GetRegion(regionid);
			if (reg == null)
				return new Region.EmptyEnumerator();
			return reg.GetNPCsInRadius(x, y, z, radiusToCheck, withDistance, false);
		}

		/// <summary>
		/// Returns an IEnumerator of GameNPCs that are close to a certain
		/// spot in the region
		/// </summary>
		/// <param name="regionid">Region to search</param>
		/// <param name="x">X inside region</param>
		/// <param name="y">Y inside region</param>
		/// <param name="z">Z inside region</param>
		/// <param name="radiusToCheck">Radius to sarch for GameNPCs</param>
		/// <returns>IEnumerator that can be used to go through all NPCs</returns>
		public static IEnumerable GetNPCsCloseToSpot(ushort regionid, int x, int y, int z, ushort radiusToCheck)
		{
			return GetNPCsCloseToSpot(regionid, x, y, z, radiusToCheck, false);
		}

		/// <summary>
		/// Returns an IEnumerator of GameItems that are close to a certain
		/// spot in the region
		/// </summary>
		/// <param name="regionid">Region to search</param>
		/// <param name="x">X inside region</param>
		/// <param name="y">Y inside region</param>
		/// <param name="z">Z inside region</param>
		/// <param name="radiusToCheck">Radius to sarch for GameItems</param>
		/// <param name="withDistance">Wether or not to return the objects with distance</param>
		/// <returns>IEnumerator that can be used to go through all items</returns>
		public static IEnumerable GetItemsCloseToSpot(ushort regionid, int x, int y, int z, ushort radiusToCheck, bool withDistance)
		{
			Region reg = GetRegion(regionid);
			if (reg == null)
				return new Region.EmptyEnumerator();

			return reg.GetItemsInRadius(x, y, z, radiusToCheck, withDistance);
		}

		private static void CheckRegionAndZoneConfigs()
		{
			FileInfo regionConfigFile = new FileInfo(GameServer.Instance.Configuration.RegionConfigFile);
			FileInfo zoneConfigFile = new FileInfo(GameServer.Instance.Configuration.ZoneConfigFile);
			if (!regionConfigFile.Exists)
				ResourceUtil.ExtractResource(regionConfigFile.Name, regionConfigFile.FullName);
			if (!zoneConfigFile.Exists)
				ResourceUtil.ExtractResource(zoneConfigFile.Name, zoneConfigFile.FullName);
		}

		/// <summary>
		/// Saves all players into the database.
		/// </summary>
		/// <returns>The count of players saved</returns>
		public static int SavePlayers()
		{
			GameClient[] clientsCopy = null;
			lock (m_clients.SyncRoot)
			{
				clientsCopy = (GameClient[])m_clients.Clone();
			}

			int savedCount = 0;
			foreach (GameClient client in clientsCopy)
			{
				if (client != null)
				{
					client.SavePlayer();
					savedCount++;
					//Relinquis our remaining thread time here after each save
					Thread.Sleep(0);
				}
			}
			return savedCount;
		}

		#region Instances

		//Dinberg: We must now store the region data here. This is incase admins wish to create instances
		//that require information from regions Data, like instance of underwater ToA areas as a prime
		//example!
		/// <summary>
		/// Stores the region Data parsed from the regions xml file.
		/// </summary>
		private static Hashtable m_regionData;

		public static Hashtable RegionData
		{
			get { return m_regionData; }
		}

		/// <summary>
		/// Stores the zone data parsed from the zones file by RegionID.
		/// </summary>
		private static Dictionary<ushort, List<ZoneData>> m_zonesData;

		public static Dictionary<ushort, List<ZoneData>> ZonesData
		{
			get { return m_zonesData; }
		}


		/// <summary>
		/// Creates a new instance, with the given 'skin' (the regionID to display client side).
		/// </summary>
		/// <param name="skinID"></param>
		/// <returns></returns>
		public static BaseInstance CreateInstance(ushort skinID, Type instanceType)
		{
			return CreateInstance(0, skinID, instanceType);
		}

		/// <summary>
		/// Where do we start looking for an instance id from if none is requested?
		/// </summary>
		public const int DEFAULT_VALUE_FOR_INSTANCE_ID_SEARCH_START = 1000;


		/// <summary>
		/// Tries to create an instance with the suggested ID and a given 'skin' (the regionID to display client side).
		/// </summary>
		/// <param name="requestedID">0 for random</param>
		/// <param name="skinID"></param>
		/// <param name="instanceType"></param>
		/// <returns></returns>
		public static BaseInstance CreateInstance(ushort requestedID, ushort skinID, Type instanceType)
		{
			//TODO: Typeof field so TaskDungeonInstance, QuestInstance etc can be created.
			if ((instanceType.IsSubclassOf(typeof(BaseInstance)) || instanceType == typeof(BaseInstance)) == false)
			{
				log.Error("Invalid type given for instance creation: " + instanceType + ". Returning null instance now.");
				return null;
			}

			BaseInstance instance = null;

			//To create the instance, we need to select the region relevant to the SkinID.
			RegionData data = (RegionData)m_regionData[skinID];

			if (data == null)
			{
				log.Warn("Data for region " + skinID + " not found on instance create!");
				return null;
			}

			//Having selected the ID, we must select a time manager.
			//For now, for simplicity so we can get the system running we will just take the first TimeManager
			//in our list. This is because I don't want to risk causing an error by sharing an important resource
			//like the TimeManager until I get some good testing and ensure work thus far is clean.

			//Later, we will share the resources over the different threads.

			GameTimer.TimeManager time = m_regionTimeManagers[0];

			if (time == null)
			{
				log.Warn("TimeManager not found on instance create!");
				return null;
			}

			//I've placed constructor info outside of the lock, to prevent a time delay on parallel threads.
			ConstructorInfo info = instanceType.GetConstructor(new Type[] { typeof(ushort), typeof(GameTimer.TimeManager), typeof(RegionData)});

			if (info == null)
			{
				log.Error("Classtype " + instanceType + " did not have a cosntructor that matched the requirement!");
				return null;
			}

			bool RequestedAnID = requestedID == 0 ? false : true;

			ushort ID = requestedID;

			//Get the unique ID for this instance or try to create an instance at the requested ID

			//We need to keep the lock over this whole area until we have successfully inserted the instance,
			//incase a parallel thread also receives a request to create an instance. We cant have the two colliding!
			//If they did, one instance generation would fail.

			//I'm welcome to suggestions on how to improve this
			//              -Dinberg.
			lock (m_regions.SyncRoot)
			{
				if (!RequestedAnID)
				{
					ID = DEFAULT_VALUE_FOR_INSTANCE_ID_SEARCH_START;
					while (ID < ushort.MaxValue)
					{
						//Look for a space in the regions table...
						if (!m_regions.ContainsKey(ID))
							break;

						//If no space here, no worries - move quickly to the next ID and continue.
						ID++;
					}
				}
				else if (m_regions.ContainsKey(ID))
				{
					// requested ID is in use
					return null;
				}

				//In the unlikely event of 65535 regions, I'd still like to be warned!
				if (ID == ushort.MaxValue)
				{
					log.Warn("ID was ushort.MaxValue - Region Table is full upon instance creation! Aborting now.");
					return null;
				}

				//Having selected the data we need, create the Instance.
				try
				{
					instance = (BaseInstance)info.Invoke(new object[] { ID, time, data });//new Instance(ID, time, data);
					m_regions.Add(ID, instance);
				}
				catch (Exception e)
				{
					log.Error("Error on instance creation - " + e.Message + e.StackTrace);
					return null;
				}
			}

			// But its not over there. We need to put a zone into the instance.

			List<ZoneData> list = null;

			if (m_zonesData.ContainsKey(data.Id))
			{
				list = m_zonesData[data.Id];
			}

			if (list == null)
			{
				log.Warn("No zones found for given skinID on instance creation, " + skinID);
				return null;
			}

			ushort zoneID = 0;

			foreach (ZoneData dat in list)
			{
				//we need to get an id for each one.
				lock (m_zones.SyncRoot)
				{
					while (m_zones.ContainsKey(zoneID))
						zoneID++;

					if (zoneID == ushort.MaxValue)
						log.Error("Zone limit reached in instance creation!");

					//create a zone of this ID.
					RegisterZone(dat, zoneID, ID, dat.Description + " (Instance)", 0, 0, 0, 0);
				}
			}

			// Start the instance and execute any final startup tasks
			instance.Start();

			return instance;
		}

		/// <summary>
		/// Removes the given instance from the server.
		/// </summary>
		/// <param name="instance"></param>
		public static void RemoveInstance(BaseInstance instance)
		{
			//Remove the region
			lock (m_regions.SyncRoot)
			{
				m_regions.Remove(instance.ID);
			}

			//Remove zones
			lock (m_zones.SyncRoot)
			{
				foreach (Zone zn in instance.Zones)
				{
					m_zones.Remove(zn.ID);
				}
			}

			instance.OnCollapse();

			//Destroy the region once and for all.
			instance = null;
		}


		#endregion
	}
}
