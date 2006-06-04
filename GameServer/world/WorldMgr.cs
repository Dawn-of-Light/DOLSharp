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
using DOL.GS.Database;
using DOL.GS.PacketHandler;
using DOL.GS.Utils;
using log4net;
using NHibernate.Expression;
using DOL.Config;
using Timer=System.Threading.Timer;

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

		#region All distance constant

		/// <summary>
		/// Holds the distance which player get experience from a living object
		/// </summary>
		public static readonly ushort MAX_EXPFORKILL_DISTANCE = 16384;
		/// <summary>
		/// Is the distance a whisper can be heard
		/// </summary>
		public static readonly uint WHISPER_DISTANCE = 512; // tested
		/// <summary>
		/// Is the distance a say is broadcast
		/// </summary>
		public static readonly ushort SAY_DISTANCE = 512; // tested
		/// <summary>
		/// Is the distance info messages are broadcast (player attacks, spell cast, player stunned/rooted/mezzed, loot dropped)
		/// </summary>
		public static readonly ushort INFO_DISTANCE = 512; // tested for player attacks, think it's same for rest
		/// <summary>
		/// Is the distance a death message is broadcast when player dies
		/// </summary>
		public static readonly ushort DEATH_MESSAGE_DISTANCE = ushort.MaxValue; // unknown
		/// <summary>
		/// Is the distance a yell is broadcast
		/// </summary>
		public static readonly ushort YELL_DISTANCE = 1024; // tested
		/// <summary>
		/// Is the distance at which livings can give a item
		/// </summary>
		public static readonly ushort GIVE_ITEM_DISTANCE = 128;  // tested
		/// <summary>
		/// Is the distance at which livings can interact
		/// </summary>
		public static readonly ushort INTERACT_DISTANCE = 192;  // tested
		/// <summary>
		/// Is the distance an player can see
		/// </summary>
		public static readonly ushort VISIBILITY_DISTANCE = 3600;
		/// <summary>
		/// Is the square distance a player can see
		/// </summary>
		public static readonly int VISIBILITY_SQUARE_DISTANCE = 12960000;
		/// <summary>
		/// Holds the distance at which objects are updated
		/// </summary>
		public static readonly ushort OBJ_UPDATE_DISTANCE = 4096;
		/// <summary>
		/// How close a player can be to pick up loot
		/// </summary>
		public static readonly ushort PICKUP_DISTANCE = 256;

		#endregion

		#region TimeManagers
		/// <summary>
		/// Holds all region timers
		/// </summary>
		private static GameTimer.TimeManager[] m_regionTimeManagers;

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
		#endregion

		#region Start / stop

		/// <summary>
		/// Initializes the WorldMgr. This function must be called
		/// before the WorldMgr can be used!
		/// </summary>
		public static bool Start()
		{
			try
			{
				m_regions = new Hashtable();
				m_clients = new GameClient[GameServer.Instance.Configuration.MaxClientCount];
				
				log.Debug(GC.GetTotalMemory(true)/1000+"kb - w1");

				int cpuCount = GameServer.Instance.Configuration.CpuCount;
				if (cpuCount < 1)
					cpuCount = 1;

				GameTimer.TimeManager[] timers = new GameTimer.TimeManager[cpuCount];
				for (int i = 0; i < cpuCount; i++)
				{
					timers[i] = new GameTimer.TimeManager("RegionTime" + (i+1).ToString());
				}
				m_regionTimeManagers = timers;

				log.Debug("Loading regions and zones from database...");

				long objects = 0;
			
				IList regions = GameServer.Database.SelectAllObjects(typeof(Region));
				if(regions.Count <= 0)
				{
					if (log.IsErrorEnabled)
					{
						log.Error("No regions found in the database, WorldMgr.Start() abort !");
					}

					return false;
				}

				foreach(Region currentRegion in regions)
				{
					m_regions.Add(currentRegion.RegionID, currentRegion);

					currentRegion.TimeManager = timers[FastMath.Abs(m_regions.Count%(cpuCount*2)-cpuCount)%cpuCount];

					int objectsInRegion = 0;
					foreach(PersistantGameObject obj in GameServer.Database.SelectObjects(typeof(PersistantGameObject), Expression.Eq("RegionID", currentRegion.RegionID)))
					{
						if(obj is GamePlayer) continue;
						if(obj is IDoor) DoorMgr.AddDoor((IDoor)obj);

						obj.AddToWorld();

						objectsInRegion++;
					}

					objects += objectsInRegion;

					if (log.IsDebugEnabled)
						log.Debug("Registered region: " + currentRegion.Description + " ("+currentRegion.RegionID+") GameObjects count : "+objectsInRegion);
				}

				if (log.IsInfoEnabled)
				{
					log.Info("Total Region: " + m_regions.Count);
					log.Info("Total GameObjects: " + objects);
				}

				log.Debug(GC.GetTotalMemory(true)/1000+"kb - w2");
				
				if (log.IsDebugEnabled)
					log.Debug("Starting region managers...");

				foreach (Region reg in m_regions.Values)
				{
					reg.StartRegionMgr();
				}
				
				m_GameObjectUpdateThread = new Thread(new ThreadStart(GameObjectUpdateThreadStart));
				m_GameObjectUpdateThread.Name = "NpcUpdate";
				m_GameObjectUpdateThread.IsBackground = true;
				m_GameObjectUpdateThread.Start();

				m_dayIncrement = 24;
				m_dayStartTick = Environment.TickCount - (int) (DAY/m_dayIncrement/2); // set start time to 12am
				m_dayResetTimer = new Timer(new TimerCallback(DayReset), null, DAY/m_dayIncrement/2, DAY/m_dayIncrement);

				m_pingCheckTimer = new Timer(new TimerCallback(PingCheck), null, 10*1000, 0); // every 10s a chec

				if (log.IsDebugEnabled)
					log.Debug("Region managers started.");
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
		/// Cleans up and stops all the RegionMgr tasks inside
		/// the regions.
		/// </summary>
		public static void Stop()
		{
			try
			{
				if (log.IsDebugEnabled)
					log.Debug("Stopping region managers...");
				
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
				if (m_GameObjectUpdateThread != null)
				{
					m_GameObjectUpdateThread.Abort();
					m_GameObjectUpdateThread = null;
				}

				if(m_regions != null)
				{
					foreach (Region reg in m_regions.Values)
					{
						reg.StopRegionMgr();
					}
				}

				if (log.IsDebugEnabled)
					log.Debug("Region managers stopped.");
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("Exit", e);
			}
		}

		#endregion

		#region PingCheck

		/// <summary>
		/// Ping timeout definition in seconds
		/// </summary>
		public static readonly long PING_TIMEOUT = 360; // 6 min default ping timeout (ticks are 100 nano seconds)
		
		/// <summary>
		/// Timer for ping timeout checks
		/// </summary>
		private static Timer m_pingCheckTimer;

		/// <summary>
		/// perform the ping timeout check and disconnect clients that timed out
		/// </summary>
		/// <param name="sender"></param>
		private static void PingCheck(object sender)
		{
			try
			{
				lock (m_clients.SyncRoot)
				{
					foreach (GameClient client in m_clients)
					{
						if(client == null) continue;
						try
						{
							// check ping timeout if we are in charscreen or in playing state
							if (client.ClientState == GameClient.eClientState.CharScreen ||
								client.ClientState == GameClient.eClientState.Playing)
							{
								if (client.PingTime + PING_TIMEOUT*1000*1000*10 < DateTime.Now.Ticks)
								{
									if (log.IsWarnEnabled)
										log.Warn("Ping timeout for client " + client.Account.AccountName);
									GameServer.Instance.Disconnect(client);
								}
							}
							else
							{
								// in all other cases client gets 10min to get wether in charscreen or playing state					
								if (client.PingTime + 10*60*10000000L < DateTime.Now.Ticks)
								{
									if (log.IsWarnEnabled)
										log.Warn("Hard timeout for client " + client.Account.AccountName + " (" + client.ClientState + ")");
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
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("PingCheck callback", e);
			}
			finally
			{
				m_pingCheckTimer.Change(10*1000, Timeout.Infinite);
			}
		}

		#endregion

		#region Game days / time

		/// <summary>
		/// This constant defines the day constant
		/// </summary>
		private static readonly int DAY = 77760000;

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
		public static void StartDay(uint dayInc, uint dayStart)
		{
			m_dayIncrement = dayInc;
			m_dayStartTick = Environment.TickCount - (int) (dayStart/m_dayIncrement); // set start time to ...
			m_dayResetTimer.Change((DAY - dayStart)/m_dayIncrement, Timeout.Infinite);
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
			long curTime = diff*m_dayIncrement;
			return (uint) (curTime%DAY);
		}

		/// <summary>
		/// Returns the day increment
		/// </summary>
		/// <returns>the day increment</returns>
		public static uint GetDayIncrement()
		{
			return m_dayIncrement;
		}

		#endregion

		#region GameObject update thread

		/// <summary>
		/// How often players are updated
		/// </summary>
		public static readonly int PLAYER_UPDATE_TIME = 300;

		/// <summary>
		/// This thread is used to update the NPCs around a player
		/// as fast as possible
		/// </summary>
		private static Thread m_GameObjectUpdateThread;

		/// <summary>
		/// Gets the npc update thread stacktrace
		/// </summary>
		/// <returns></returns>
		public static StackTrace GetNpcUpdateStacktrace()
		{
			return Util.GetThreadStack(m_GameObjectUpdateThread);
		}

		/// <summary>
		/// This thread updates the NPCs around the player at very short
		/// intervalls! But since the update is very quick the thread will
		/// sleep most of the time!
		/// </summary>
		private static void GameObjectUpdateThreadStart()
		{
			bool running = true;
			if (log.IsDebugEnabled)
				log.Debug("GameObjectUpdateThread ThreadId=" + AppDomain.GetCurrentThreadId());
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
								log.Error("account has no active player but is playing, disconnecting! => " + client.Account.AccountName);
							GameServer.Instance.Disconnect(client);
							Thread.Sleep(200);
						}
						if (client.ClientState != GameClient.eClientState.Playing)
							continue;
						if (player.ObjectState != eObjectState.Active)
							continue;

						if ((uint)Environment.TickCount - player.LastNPCUpdate > PLAYER_UPDATE_TIME)
						{
							BitArray carray = player.CurrentUpdateArray;
							BitArray narray = player.NewUpdateArray;
							narray.SetAll(false);

							foreach (IMovingGameObject movingObject in player.GetInRadius(typeof(IMovingGameObject), VISIBILITY_DISTANCE))
							{
								GeometryEngineNode movingNode = (GeometryEngineNode)movingObject;
								if(movingNode.ObjectState != eObjectState.Active) continue;

								try
								{
									narray[movingNode.ObjectID-1] = true;
									if ((uint)Environment.TickCount - movingObject.LastUpdateTickCount > 30000)
									{
										movingObject.BroadcastUpdate();
									}
									else if (carray[movingNode.ObjectID-1] == false)
									{
										client.Out.SendNPCUpdate((GameObject)movingObject);
									}
								}
								catch (Exception e)
								{
									if (log.IsErrorEnabled)
										log.Error("MovingObject update: " + e.GetType().FullName + " (" + movingNode.ToString() + ")", e);
								}
							}
							
							player.SwitchUpdateArrays();
							player.LastNPCUpdate = (uint)Environment.TickCount;
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

		#endregion
		
		#region GameClient methods

		/// <summary>
		/// This array holds all gameclients connected to the game
		/// </summary>
		private static GameClient[] m_clients;

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
				{
					if (m_clients[i] == null)
					{
						m_clients[i] = obj;
						obj.SessionID = i + 1;
						return i + 1;
					}
				}
			}
			return -1;
		}

		/// <summary>
		/// Removes a GameClient and free's it's ID again!
		/// </summary>
		/// <param name="entry">The GameClient to be removed</param>
		public static void RemoveClient(GameClient entry)
		{
			if (entry == null) return;

			int sessionid = entry.SessionID;

			if (sessionid > 0)
			{
				lock (m_clients.SyncRoot)
				{
					m_clients[sessionid - 1] = null;
				}
			}
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
		/// Finds a GameClient by the AccountName
		/// </summary>
		/// <param name="accountName">AccountName to search</param>
		/// <returns>The found GameClient or null</returns>
		public static GameClient GetClientByAccountName(string accountName)
		{
			accountName = accountName.ToLower();
			lock (m_clients.SyncRoot)
			{
				foreach (GameClient client in m_clients)
				{
					if (client != null && client.Account.AccountName.ToLower() == accountName)
					{
						return client;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Find a GameClient by the Player's name. Allways return exact match if exist
		/// else try to guess the name.
		/// </summary>
		/// <param name="playerName">Name to search</param>
		/// <param name="exactMatch">true if AccountName match exactly</param>
		/// <returns>The found GameClient or null</returns>
		public static GameClient GetClientByPlayerName(string playerName, bool exactMatch)
		{
			GameClient guessedClient = null;
			string compareName = playerName.ToLower();
			
			lock (m_clients.SyncRoot)
			{
				foreach (GameClient client in m_clients)
				{
					if (client != null
						&& client.IsPlaying
						&& client.Player != null
						&& client.Player.ObjectState == eObjectState.Active)
					{
						// try exact match in case player with "abcde" name is before "abc" in list and user typed "abc"
						if (0 == string.Compare(client.Player.Name, compareName, true)) // exact match case insensitive compare
						{
							return client;
						}

						// try to guess the name
						if (!exactMatch && client.Player.Name.ToLower().StartsWith(compareName))
						{
							guessedClient = client;
						}
					}
				}
			}

			return guessedClient;
		}

		/// <summary>
		/// Gets a copy of all playing clients
		/// </summary>
		/// <returns>ArrayList of playing GameClients</returns>
		public static ArrayList GetAllPlayingClients()
		{
			ArrayList targetClients = new ArrayList(GameServer.Instance.Configuration.MaxClientCount);
			lock (m_clients.SyncRoot)
			{
				foreach (GameClient client in m_clients)
				{
					if (client != null
						&& client.IsPlaying
						&& client.Player != null
						&& client.Player.ObjectState == eObjectState.Active)
						targetClients.Add(client);
				}
			}
			return targetClients;
		}

		/// <summary>
		/// Gets the count of connected clients.
		/// </summary>
		/// <returns>The count of connected clients.</returns>
		public static int GetConnectedClientsCount()
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
		/// Returns an array of GameClients currently playing from a specific realm
		/// </summary>
		/// <param name="realmID">ID of Realm (1=Alb, 2=Mid, 3=Hib)</param>
		/// <returns>An ArrayList of clients</returns>
		public static ArrayList GetClientsOfRealm(int realmID)
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
							&& client.Player.ObjectState == eObjectState.Active
							&& client.Player.Realm == realmID)
							targetClients.Add(client);
					}
				}
			}
			return targetClients;
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
							&& client.Player.ObjectState == eObjectState.Active
							&& client.Player.Region.RegionID == regionID)
							targetClients.Add(client);
					}
				}
			}
			return targetClients;
		}

		/// <summary>
		/// Saves all players into the database.
		/// </summary>
		/// <returns>The count of players saved</returns>
		public static int SavePlayers()
		{
			if(m_clients == null) return 0;

			GameClient[] clientsCopy = null;
			lock (m_clients.SyncRoot)
			{
				clientsCopy = (GameClient[]) m_clients.Clone();
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
		#endregion

		#region Regions methods

		/// <summary>
		/// This hashtable holds all regions in the world
		/// </summary>
		private static Hashtable m_regions;

		/// <summary>
		/// Fetch a Region by it's ID
		/// </summary>
		/// <param name="regionID">ID to search</param>
		/// <returns>Region or null if not found</returns>
		public static Region GetRegion(int regionID)
		{
			return (Region) m_regions[regionID];
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
					Region reg = (Region) iter.Value;
					regs[i].id = (ushort)reg.RegionID;
					regs[i].ip = ip;
					regs[i].toPort = port;
					regs[i].name = "Region"+reg.RegionID;
					regs[i].fromPort = port;
					regs[i].expansion = reg.Expansion;

					++i;
				}
			}

			return regs;
		}
		#endregion
	}
}