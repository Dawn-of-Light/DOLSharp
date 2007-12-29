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
		/// Is the square distance a player can see
		/// </summary>
		public const int VISIBILITY_SQUARE_DISTANCE = 12960000;
		/// <summary>
		/// Holds the distance at which objects are updated
		/// </summary>
		public const int OBJ_UPDATE_DISTANCE = 4096;
		/// <summary>
		/// How often players are updated
		/// </summary>
		public const int PLAYER_UPDATE_TIME = 300;
		/// <summary>
		/// How close a player can be to pick up loot
		/// </summary>
		public const int PICKUP_DISTANCE = 256;

		/// <summary>
		/// All available teleport locations.
		/// </summary>
		private static Dictionary<eRealm, List<Teleport>> m_teleportLocations;

		/// <summary>
		/// Returns a list of all teleport locations available for this realm.
		/// </summary>
		/// <param name="realm"></param>
		/// <returns></returns>
		public static List<Teleport> GetTeleportLocations(eRealm realm)
		{
			return (m_teleportLocations.ContainsKey(realm))
				? m_teleportLocations[realm]
				: null;
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
		private static Thread m_NPCUpdateThread;

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

			XMLConfigFile zoneCfg = XMLConfigFile.ParseXMLFile(new FileInfo(GameServer.Instance.Configuration.ZoneConfigFile));
			XMLConfigFile regionCfg = XMLConfigFile.ParseXMLFile(new FileInfo(GameServer.Instance.Configuration.RegionConfigFile));

			if (log.IsDebugEnabled)
			{
				log.Debug(string.Format("{0} blocks read from {1}", regionCfg.Children.Count, GameServer.Instance.Configuration.RegionConfigFile));
				log.Debug(string.Format("{0} blocks read from {1}", zoneCfg.Children.Count, GameServer.Instance.Configuration.ZoneConfigFile));
			}

			// Load available teleport locations.

			DataObject[] objs = GameServer.Database.SelectAllObjects(typeof(Teleport));
			m_teleportLocations = new Dictionary<eRealm, List<Teleport>>();
			int[] numTeleports = new int[3];
			foreach (Teleport teleport in objs)
			{
				List<Teleport> teleportList;
				if (m_teleportLocations.ContainsKey((eRealm)teleport.Realm))
					teleportList = m_teleportLocations[(eRealm)teleport.Realm];
				else
				{
					teleportList = new List<Teleport>();
					m_teleportLocations.Add((eRealm)teleport.Realm, teleportList);
				}
				teleportList.Add(teleport);
				if (teleport.Realm >= 1 && teleport.Realm <= 3)
					numTeleports[teleport.Realm - 1]++;
			}
			log.Info(String.Format("Loaded {0} Albion, {1} Midgard and {2} Hibernia teleport locations",
				numTeleports[0], numTeleports[1], numTeleports[2]));

			// sort the regions by mob count

			log.Debug("loading mobs from DB...");
			objs = GameServer.Database.SelectAllObjects(typeof(Mob));

			Hashtable mobsByRegionId = new Hashtable(512);
			foreach (Mob mob in objs)
			{
				ArrayList list = (ArrayList)mobsByRegionId[mob.Region];
				if (list == null)
				{
					list = new ArrayList(1024);
					mobsByRegionId.Add(mob.Region, list);
				}
				list.Add(mob);
			}
			objs = null;

			ArrayList regions = new ArrayList(512);
			foreach (DictionaryEntry entry in regionCfg.Children)
			{
				string name = (string)entry.Key;
				ConfigElement config = (ConfigElement)entry.Value;

				RegionData data = new RegionData();
				data.Id = (ushort)config[ENTRY_REG_ID].GetInt();
				data.Name = name;
				data.Description = config[ENTRY_REG_DESC].GetString();
				data.Ip = config[ENTRY_REG_IP].GetString();
				data.Port = (ushort)config[ENTRY_REG_PORT].GetInt();
				data.WaterLevel = config[ENTRY_REG_WATER_LEVEL].GetInt();
				data.DivingEnabled = config[ENTRY_REG_DIVING_ENABLE].GetBoolean(false);
				data.HousingEnabled = config[ENTRY_REG_HOUSING_ENABLE].GetBoolean(false);
				data.Expansion = config[ENTRY_REG_EXPANSION].GetInt();
				//				data.Mobs = (Mob[])GameServer.Database.SelectObjects(typeof(Mob), "Region = '" + data.Id + "'");
				ArrayList mobs = (ArrayList)mobsByRegionId[data.Id];
				if (mobs == null)
					data.Mobs = new Mob[0];
				else
					data.Mobs = (Mob[])mobs.ToArray(typeof(Mob));

				regions.Add(data);
			}

			regions.Sort();

			/////

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

			foreach (DictionaryEntry entry in zoneCfg.Children)
			{
				//string name = (string) entry.Key;
				ConfigElement config = (ConfigElement)entry.Value;

				RegisterZone(
					(ushort)config[ENTRY_ZONE_ZONEID].GetInt(),
					(ushort)config[ENTRY_ZONE_REGIONID].GetInt(),
					config[ENTRY_ZONE_DESC].GetString(),
					(byte)config[ENTRY_ZONE_OFFX].GetInt(),
					(byte)config[ENTRY_ZONE_OFFY].GetInt(),
					(byte)config[ENTRY_ZONE_WIDTH].GetInt(),
					(byte)config[ENTRY_ZONE_HEIGHT].GetInt());
			}

			log.Debug(GC.GetTotalMemory(true) / 1000 + "kb - w4");

			regionsData = (RegionData[])regions.ToArray(typeof(RegionData));

			return true;
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

				m_NPCUpdateThread = new Thread(new ThreadStart(NPCUpdateThreadStart));
				m_NPCUpdateThread.Name = "NpcUpdate";
				m_NPCUpdateThread.IsBackground = true;
				m_NPCUpdateThread.Start();

				m_dayIncrement = 24;
				m_dayStartTick = Environment.TickCount - (int)(DAY / m_dayIncrement / 2); // set start time to 12am
				m_dayResetTimer = new Timer(new TimerCallback(DayReset), null, DAY / m_dayIncrement / 2, DAY / m_dayIncrement);

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
					foreach (Region region in m_regions.Values)
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
				client.Out.SendTime();
		}

		/// <summary>
		/// Starts a new day with a certain increment
		/// </summary>
		/// <param name="dayInc"></param>
		/// <param name="dayStart"></param>
		public static void StartDay(uint dayInc, uint dayStart)
		{
			m_dayIncrement = dayInc;
			m_dayStartTick = Environment.TickCount - (int)(dayStart / m_dayIncrement); // set start time to ...
			m_dayResetTimer.Change((DAY - dayStart) / m_dayIncrement, Timeout.Infinite);
			foreach (GameClient client in GetAllPlayingClients())
				client.Out.SendTime();
		}

		/// <summary>
		/// Gets the current daytime
		/// </summary>
		/// <returns>current time</returns>
		public static uint GetCurrentDayTime()
		{
			long diff = Environment.TickCount - m_dayStartTick;
			long curTime = diff * m_dayIncrement;
			return (uint)(curTime % DAY);
		}

		/// <summary>
		/// Returns the day increment
		/// </summary>
		/// <returns>the day increment</returns>
		public static uint GetDayIncrement()
		{
			return m_dayIncrement;
		}

		/// <summary>
		/// Gets the npc update thread stacktrace
		/// </summary>
		/// <returns></returns>
		public static StackTrace GetNpcUpdateStacktrace()
		{
			return Util.GetThreadStack(m_NPCUpdateThread);
		}

		/// <summary>
		/// This thread updates the NPCs around the player at very short
		/// intervalls! But since the update is very quick the thread will
		/// sleep most of the time!
		/// </summary>
		private static void NPCUpdateThreadStart()
		{
			bool running = true;
			if (log.IsDebugEnabled)
				log.Debug("NPCUpdateThread ThreadId=" + Thread.CurrentThread.ManagedThreadId);
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
							Thread.Sleep(200);
						}
						if (client.ClientState != GameClient.eClientState.Playing)
							continue;
						if (player.ObjectState != GameObject.eObjectState.Active)
							continue;

						if (Environment.TickCount - player.LastNPCUpdate > PLAYER_UPDATE_TIME)
						{
							BitArray carray = player.CurrentUpdateArray;
							BitArray narray = player.NewUpdateArray;
							narray.SetAll(false);
							lock (player.CurrentRegion.ObjectsSyncLock)
							{
								foreach (GameNPC npc in player.GetNPCsInRadius(VISIBILITY_DISTANCE))
								{
									try
									{
										if (npc == null) continue;
										narray[npc.ObjectID - 1] = true;
										/*
										if(npc.IsMoving
											&& npc.IsOnTarget()
											&& !npc.HasArriveOnTargetHandlers()
											&& !npc.HasCloseToTargetHandlers())
										{
											npc.StopMoving();
										}
										else*/
										if ((uint)Environment.TickCount - npc.LastUpdateTickCount > 30000)
										{
											npc.BroadcastUpdate();
										}
										else if (carray[npc.ObjectID - 1] == false)
										{
											client.Out.SendObjectUpdate(npc);
										}
									}
									catch (Exception e)
									{
										if (log.IsErrorEnabled)
											log.Error("NPC update: " + e.GetType().FullName + " (" + npc.ToString() + ")", e);
									}
								}
							}
							player.SwitchUpdateArrays();
							player.LastNPCUpdate = Environment.TickCount;
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
				if (m_NPCUpdateThread != null)
				{
					m_NPCUpdateThread.Abort();
					m_NPCUpdateThread = null;
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
				string ip = GameServer.Instance.Configuration.RegionIp.ToString();

				int i = 0;

				while (iter.MoveNext())
				{
					Region reg = (Region)iter.Value;
					regs[i].id = reg.ID;
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
		/// Registers a Zone into a Region
		/// </summary>
		/// <param name="zoneID">ID of the Zone</param>
		/// <param name="regionID">ID of the Region the Zone belongs to</param>
		/// <param name="desc">Description of Zone</param>
		/// <param name="offx">X offset within Region</param>
		/// <param name="offy">Y offset within Region</param>
		/// <param name="width">Width of Zone</param>
		/// <param name="height">Height of Zone</param>
		public static void RegisterZone(ushort zoneID, ushort regionID, string desc, byte offx, byte offy, byte width, byte height)
		{
			Region reg = GetRegion(regionID);
			if (reg == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Region " + regionID + " for Zone " + desc);
				return;
			}
			Zone zone = new Zone(
				reg,
				zoneID,
				desc,
				offx * 8192,
				offy * 8192,
				width * 8192,
				height * 8192);

			lock (reg.Zones.SyncRoot)
			{
				reg.Zones.Add(zone);
			}
			lock (m_zones.SyncRoot)
			{
				m_zones.Add(zoneID, zone);
			}

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

		/// <summary>
		/// Gets a Zone object by it's ID
		/// </summary>
		/// <param name="zoneID">the zoneID</param>
		/// <returns>the zone object or null</returns>
		public static Zone GetZone(ushort zoneID)
		{
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
		public static int GetDistance(GameObject obj1, GameObject obj2)
		{
			if (obj1 == null || obj2 == null || obj1.CurrentRegion != obj2.CurrentRegion)
				return -1;
			return GetDistance(obj1.X, obj1.Y, obj1.Z, obj2.X, obj2.Y, obj2.Z);
		}

		/// <summary>
		/// Get's the distance of two GameObjects
		/// </summary>
		/// <param name="obj1">Object1</param>
		/// <param name="obj2">Object2</param>
		/// <param name="zfactor">Factor for Z distance use lower 0..1 to lower Z influence</param>
		/// <returns>The distance in units or -1 if they are not the same Region</returns>
		public static int GetDistance(GameObject obj1, GameObject obj2, double zfactor)
		{
			if (obj1 == null || obj2 == null || obj1.CurrentRegion != obj2.CurrentRegion)
				return -1;
			return GetDistance(obj1.X, obj1.Y, obj1.Z, obj2.X, obj2.Y, obj2.Z, zfactor);
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
		public static int GetDistance(int x1, int y1, int z1, int x2, int y2, int z2)
		{
			long xdiff = (long)x1 - x2;
			long ydiff = (long)y1 - y2;
			//SH: Removed Z checks when one of the two Z values is zero(on ground)
			if (z1 == 0 || z2 == 0)
				return (int)Math.Sqrt(xdiff * xdiff + ydiff * ydiff);

			long zdiff = (long)z1 - z2;
			return (int)Math.Sqrt(xdiff * xdiff + ydiff * ydiff + zdiff * zdiff);
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
		public static int GetDistance(int x1, int y1, int z1, int x2, int y2, int z2, double zfactor)
		{
			long xdiff = (long)x1 - x2;
			long ydiff = (long)y1 - y2;
			//SH: Removed Z checks when one of the two Z values is zero(on ground)
			if (z1 == 0 || z2 == 0)
				return (int)Math.Sqrt(xdiff * xdiff + ydiff * ydiff);

			long zdiff = (long)((z1 - z2) * zfactor);
			return (int)Math.Sqrt(xdiff * xdiff + ydiff * ydiff + zdiff * zdiff);
		}

		/// <summary>
		/// Gets the distance of an Object to an arbitary point
		/// </summary>
		/// <param name="obj">GameObject used as Point1</param>
		/// <param name="x">X of Point2</param>
		/// <param name="y">Y of Point2</param>
		/// <param name="z">Z of Point2</param>
		/// <returns>The distance</returns>
		public static int GetDistance(GameObject obj, int x, int y, int z)
		{
			return GetDistance(obj.X, obj.Y, obj.Z, x, y, z);
		}
		#endregion
		#region check distance
		public static bool CheckDistance(int x1, int y1, int z1, int x2, int y2, int z2, int radius)
		{
			return CheckSquareDistance(x1, y1, z1, x2, y2, z2, radius * radius);
		}
		public static bool CheckDistance(IPoint3D obj, IPoint3D obj2, int radius)
		{
			return CheckDistance(obj.X, obj.Y, obj.Z, obj2.X, obj2.Y, obj2.Z, radius);
		}
		public static bool CheckDistance(GameObject obj, int x2, int y2, int z2, int radius)
		{
			return CheckDistance(obj.X, obj.Y, obj.Z, x2, y2, z2, radius);
		}
		public static bool CheckDistance(GameObject obj, GameObject obj2, int radius)
		{
			if (obj == null || obj2 == null)
				return false;
			if (obj.CurrentRegion != obj2.CurrentRegion)
				return false;
			return CheckDistance(obj.X, obj.Y, obj.Z, obj2.X, obj2.Y, obj2.Z, radius);
		}
		#endregion
		#region check square distance
		public static bool CheckSquareDistance(int x1, int y1, int z1, int x2, int y2, int z2, int squareRadius)
		{
			long xdiff = (long)x1 - x2;
			long ydiff = (long)y1 - y2;
			//SH: Removed Z checks when one of the two Z values is zero(on ground)
			if (z1 == 0 || z2 == 0)
				return (xdiff * xdiff + ydiff * ydiff <= squareRadius);
			long zdiff = (long)z1 - z2;
			return (xdiff * xdiff + ydiff * ydiff + zdiff * zdiff <= squareRadius);
		}
		public static bool CheckSquareDistance(IPoint3D obj, IPoint3D obj2, int squareRadius)
		{
			return CheckSquareDistance(obj.X, obj.Y, obj.Z, obj2.X, obj2.Y, obj2.Z, squareRadius);
		}
		public static bool CheckSquareDistance(GameObject obj, int x2, int y2, int z2, int squareRadius)
		{
			return CheckSquareDistance(obj.X, obj.Y, obj.Z, x2, y2, z2, squareRadius);
		}
		public static bool CheckSquareDistance(GameObject obj, GameObject obj2, int squareRadius)
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
		public static ArrayList GetClientsOfRealm(eRealm realm)
		{
			ArrayList targetClients = new ArrayList();
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
		public static ArrayList GetClientsOfRegion(ushort regionID)
		{
			ArrayList targetClients = new ArrayList();
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
		public static ArrayList GetAllPlayingClients()
		{
			ArrayList targetClients = new ArrayList();
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
		public static ArrayList GetAllClients()
		{
			ArrayList targetClients = new ArrayList();
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
			return reg.GetPlayersInRadius(x, y, z, radiusToCheck, withDistance);
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
		/// Returns an IEnumerator of GamePlayers close to another GameObject
		/// </summary>
		/// <param name="obj">GameObject to check</param>
		/// <param name="radiusToCheck">Radius around GameObject to check for clients</param>
		/// <param name="withDistance">Wether or not to return the objects with distance</param>
		/// <returns>IEnumerator of players close to the target object</returns>
		[Obsolete("Use GameObject.GetPlayersInRadius instead!", false)]
		public static IEnumerable GetPlayersCloseToObject(GameObject obj, ushort radiusToCheck, bool withDistance)
		{
			return obj.CurrentRegion.GetPlayersInRadius(obj.X, obj.Y, obj.Z, radiusToCheck, withDistance);
		}

		/// <summary>
		/// Returns an IEnumerator of GamePlayers close to another GameObject
		/// </summary>
		/// <param name="obj">GameObject to check</param>
		/// <param name="radiusToCheck">Radius around GameObject to check for clients</param>
		/// <returns>IEnumerator of players close to the target object</returns>
		[Obsolete("Use GameObject.GetPlayersInRadius instead!", false)]
		public static IEnumerable GetPlayersCloseToObject(GameObject obj, ushort radiusToCheck)
		{
			return GetPlayersCloseToObject(obj, radiusToCheck, false);
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
			return reg.GetNPCsInRadius(x, y, z, radiusToCheck, withDistance);
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
		/// Returns an IEnumerator of GameNPCs close to another GameObject
		/// </summary>
		/// <param name="obj">GameObject to check</param>
		/// <param name="radiusToCheck">Radius around GameObject to check for NPCs</param>
		/// <param name="withDistance">Wether or not to return the objects with distance</param>
		/// <returns>IEnumerator of NPCs close to the target object</returns>
		[Obsolete("Use GameObject.GetNPCsInRadius instead!", false)]
		public static IEnumerable GetNPCsCloseToObject(GameObject obj, ushort radiusToCheck, bool withDistance)
		{
			return obj.CurrentRegion.GetNPCsInRadius(obj.X, obj.Y, obj.Z, radiusToCheck, withDistance);
		}

		/// <summary>
		/// Returns an IEnumerator of GameNPCs close to another GameObject
		/// </summary>
		/// <param name="obj">GameObject to check</param>
		/// <param name="radiusToCheck">Radius around GameObject to check for NPCs</param>
		/// <returns>IEnumerator of NPCs close to the target object</returns>
		[Obsolete("Use GameObject.GetNPCsInRadius instead!", false)]
		public static IEnumerable GetNPCsCloseToObject(GameObject obj, ushort radiusToCheck)
		{
			return GetNPCsCloseToObject(obj, radiusToCheck, false);
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

		/// <summary>
		/// Returns an IEnumerator of GameItems that are close to a certain
		/// spot in the region
		/// </summary>
		/// <param name="regionid">Region to search</param>
		/// <param name="x">X inside region</param>
		/// <param name="y">Y inside region</param>
		/// <param name="z">Z inside region</param>
		/// <param name="radiusToCheck">Radius to sarch for GameItems</param>
		/// <returns>IEnumerator that can be used to go through all items</returns>
		public static IEnumerable GetItemsCloseToSpot(ushort regionid, int x, int y, int z, ushort radiusToCheck)
		{
			return GetItemsCloseToSpot(regionid, x, y, z, radiusToCheck, false);
		}

		/// <summary>
		/// Returns an IEnumerator of GameItems close to another GameObject
		/// </summary>
		/// <param name="obj">GameObject to check</param>
		/// <param name="radiusToCheck">Radius around GameObject to check for items</param>
		/// <param name="withDistance">Wether or not to return the objects with distance</param>
		/// <returns>IEnumerator of items close to the target object</returns>
		[Obsolete("Use GameObject.GetItemsInRadius instead!", false)]
		public static IEnumerable GetItemsCloseToObject(GameObject obj, ushort radiusToCheck, bool withDistance)
		{
			return obj.CurrentRegion.GetItemsInRadius(obj.X, obj.Y, obj.Z, radiusToCheck, withDistance);
		}

		/// <summary>
		/// Returns an IEnumerator of GameItems close to another GameObject
		/// </summary>
		/// <param name="obj">GameObject to check</param>
		/// <param name="radiusToCheck">Radius around GameObject to check for items</param>
		/// <returns>IEnumerator of items close to the target object</returns>
		[Obsolete("Use GameObject.GetItemsInRadius instead!", false)]
		public static IEnumerable GetItemsCloseToObject(GameObject obj, ushort radiusToCheck)
		{
			return GetItemsCloseToObject(obj, radiusToCheck, false);
		}

		/// <summary>
		/// Saves the world to the DB
		/// </summary>
		public static void SaveToDatabase()
		{
			// region entries are not dynamically changed, so we can iterate without lock
			//lock(m_regions.SyncRoot)
			//{
			IDictionaryEnumerator iter = m_regions.GetEnumerator();
			/*
							using(WorldSaveProgress dlg = new WorldSaveProgress())
							{
								dlg.progBar.Minimum = 0;
								dlg.progBar.Minimum = m_regions.Count;
								dlg.progBar.Step = 1;
								dlg.lblTxt.Select(dlg.lblTxt.Text.Length-1, 0);

								dlg.Show();

								while(iter.MoveNext())
								{
									Region reg = (Region)iter.Value;

									reg.SaveToDatabase();
									dlg.progBar.PerformStep();
								}

								dlg.Close();
							}
							*/
			while (iter.MoveNext())
			{
				Region reg = (Region)iter.Value;
				if (log.IsDebugEnabled)
					log.Debug("saving region " + reg.Description + " " + reg.Name + " " + iter.Key);
				reg.SaveToDatabase();
				if (log.IsDebugEnabled)
					log.Debug("Saved region " + reg.Description);
				Thread.Sleep(0);
			}
			//}
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
	}
}
