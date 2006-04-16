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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using DOL.Events;
using DOL.GS.Database;
using DOL.GS.JumpPoints;
using DOL.GS.PacketHandler;
using DOL.GS.PlayerTitles;
using DOL.GS.Scripts;
using DOL.GS.ServerRules;
using DOL.GS.SpawnGenerators;
using DOL.Config;
using log4net;
using log4net.Config;
using log4net.Core;

namespace DOL
{
	namespace GS
	{
		/// <summary>
		/// Class encapsulates all game server functionality
		/// </summary>
		public class GameServer : BaseServer
		{
			/// <summary>
			/// Defines a logger for this class.
			/// </summary>
			private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

			#region Variables

			/// <summary>
			/// The textwrite for log operations
			/// </summary>
			protected ILog m_gmLog;

			/// <summary>
			/// The textwrite for log operations
			/// </summary>
			protected ILog m_cheatLog;

			/// <summary>
			/// Minute conversion from milliseconds
			/// </summary>
			protected const int MINUTE_CONV = 60000;

			/// <summary>
			/// Socket that listens for UDP packets
			/// </summary>
			protected Socket m_udplisten;

			/// <summary>
			/// Receive buffer for UDP
			/// </summary>
			protected byte[] m_pudpbuf;

			/// <summary>
			/// Maximum UDP buffer size
			/// </summary>
			protected const int MAX_UDPBUF = 4096;

			/// <summary>
			/// Database instance
			/// </summary>
			protected ObjectDatabase m_database = null;

			/// <summary>
			/// Holds instance of current server rules
			/// </summary>
			protected IServerRules m_serverRules = null;

			/// <summary>
			/// World save timer
			/// </summary>
			protected Timer m_timer;

			/// <summary>
			/// Game server status variable
			/// </summary>
			protected eGameServerStatus m_status;

			/// <summary>
			/// The instance!
			/// </summary>
			protected static GameServer m_instance;

			#endregion

			#region Properties
			
			/// <summary>
			/// Returns the instance
			/// </summary>
			public static GameServer Instance
			{
				get { return m_instance; }
			}

			/// <summary>
			/// Retrieves the server configuration
			/// </summary>
			public virtual new GameServerConfiguration Configuration
			{
				get { return (GameServerConfiguration)m_config; }
			}
			
			/// <summary>
			/// Gets the server status
			/// </summary>
			public eGameServerStatus ServerStatus
			{
				get { return m_status; }
			}

			/// <summary>
			/// Gets the current rules used by server
			/// </summary>
			public static IServerRules ServerRules
			{
				get
				{
					if (Instance.m_serverRules == null)
						Instance.m_serverRules = ScriptMgr.CreateServerRules(Instance.Configuration.ServerType);
					return Instance.m_serverRules;
				}
			}

			/// <summary>
			/// Gets the database instance
			/// </summary>
			public static ObjectDatabase Database
			{
				get { return Instance.m_database; }
			}

			/// <summary>
			/// Gets or sets the world save interval
			/// </summary>
			public int SaveInterval
			{
				get { return Configuration.DBSaveInterval; }
				set
				{
					Configuration.DBSaveInterval = value;
					if (m_timer != null)
						m_timer.Change(value*MINUTE_CONV, Timeout.Infinite);
				}
			}

			/// <summary>
			/// True if the server is listening
			/// </summary>
			public bool IsRunning
			{
				get
				{
					return m_listen != null;
				}
			}

			#endregion

			#region CreateInstance

			/// <summary>
			/// Creates the gameserver instance
			/// </summary>
			/// <param name="config"></param>
			public static void CreateInstance(GameServerConfiguration config)
			{
				//Only one intance
				if(Instance==null) m_instance = new GameServer(config);
			}

			#endregion

			#region UDP 

			/// <summary>
			/// Gets the UDP Socket of this server instance
			/// </summary>
			protected Socket UDPSocket
			{
				get
				{
					return m_udplisten;
				}
			}

			/// <summary>
			/// Gets the UDP buffer of this server instance
			/// </summary>
			protected byte[] UDPBuffer
			{
				get
				{
					return m_pudpbuf;
				}
			}

			/// <summary>
			/// Starts the udp listening
			/// </summary>
			/// <returns>true if successfull</returns>
			protected bool StartUDP()
			{
				try
				{
					m_pudpbuf = new byte[MAX_UDPBUF];
					m_udpReceiveCallback = new AsyncCallback(RecvFromCallback);
					m_udpSendCallback = new AsyncCallback(SendToCallback);

					//open our udp socket
					m_udplisten = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
					m_udplisten.Bind(new IPEndPoint(Configuration.UDPIp, Configuration.UDPPort));
					EndPoint tempRemoteEP = new IPEndPoint(IPAddress.Any, 0);
					m_udplisten.BeginReceiveFrom(m_pudpbuf, 0, MAX_UDPBUF, SocketFlags.None, ref tempRemoteEP, m_udpReceiveCallback, this);
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error("StartUDP", e);
					return false;
				}

				return true;
			}

			/// <summary>
			/// Holds udp receive callback delegate
			/// </summary>
			protected AsyncCallback m_udpReceiveCallback;

			/// <summary>
			/// UDP event handler. Called when a UDP packet is waiting to be read
			/// </summary>
			/// <param name="ar"></param>
			protected void RecvFromCallback(IAsyncResult ar)
			{
				if (ar == null) return;
				GameServer server = (GameServer) (ar.AsyncState);
				Socket s = server.UDPSocket;
				GameClient client = null;

				if (s != null)
				{
					//Creates a temporary EndPoint to pass to EndReceiveFrom.
					EndPoint tempRemoteEP = new IPEndPoint(IPAddress.Any, 0);
					try
					{
						// Handle the packet
						int read = s.EndReceiveFrom(ar, ref tempRemoteEP);
						if (read == 0)
						{
							log.Debug("UDP received bytes = 0");
						}
						else
						{
							int pakCheck = (server.UDPBuffer[read - 2] << 8) | server.UDPBuffer[read - 1];
							int calcCheck = PacketProcessor.CalculateChecksum(server.UDPBuffer, 0, read - 2);
							if (calcCheck != pakCheck)
							{
								if (log.IsWarnEnabled)
									log.WarnFormat("Bad UDP packet checksum (packet:0x{0:X4} calculated:0x{1:X4}) -> ignored", pakCheck, calcCheck);
								if (log.IsDebugEnabled)
									log.Debug(Marshal.ToHexDump("UDP buffer dump, received "+read+"bytes", server.UDPBuffer));
							}
							else
							{
								IPEndPoint sender = (IPEndPoint) (tempRemoteEP);

								GSPacketIn pakin = new GSPacketIn(read-GSPacketIn.HDR_SIZE);
								pakin.Load(server.UDPBuffer, read);

								client = WorldMgr.GetClientFromID(pakin.SessionID);

								if (client != null)
								{
									//If this is the first message from the client, we
									//save the endpoint!
									if (client.UDPEndPoint == null)
									{
										client.UDPEndPoint = sender;
									}
									//Only handle the packet if it comes from a valid client
									if (client.UDPEndPoint.Equals(sender))
									{
										client.PacketProcessor.HandlePacket(pakin);
									}
								}
								else if (log.IsErrorEnabled)
								{
									log.Error(string.Format("Got an UDP packet from invalid client id or ip: client id = {0}, ip = {1},  code = {2:x2}", pakin.SessionID, sender.ToString(), pakin.ID));
								}
							}
						}
					}
					catch (SocketException)
					{
					}
					catch (ObjectDisposedException)
					{
					}
					catch (Exception e)
					{
						if (log.IsErrorEnabled)
							log.Error("RecvFromCallback", e);
					}
					//Get the next message
					//Even if we have errors, we need to continue with UDP
					tempRemoteEP = new IPEndPoint(IPAddress.Any, 0);
					Thread.MemoryBarrier();
					try
					{
						s.BeginReceiveFrom(server.UDPBuffer, 0, MAX_UDPBUF, SocketFlags.None, ref tempRemoteEP, m_udpReceiveCallback, server);
					}
					catch (SocketException)
					{
					}
					catch (ObjectDisposedException)
					{
					}
					catch (Exception e)
					{
						if (log.IsErrorEnabled)
							log.Error("UDP Recv", e);
					}
				}
			}

			/// <summary>
			/// Holds the async UDP send callback
			/// </summary>
			protected AsyncCallback m_udpSendCallback;

			/// <summary>
			/// Sends a UDP packet
			/// </summary>
			/// <param name="bytes">Packet to be sent</param>
			/// <param name="count">The count of bytes to send</param>
			/// <param name="clientEndpoint">Address of receiving client</param>
			public void SendUDP(byte[] bytes, int count, EndPoint clientEndpoint)
			{
				SendUDP(bytes, count, clientEndpoint, null);
			}
			
			/// <summary>
			/// Sends a UDP packet
			/// </summary>
			/// <param name="bytes">Packet to be sent</param>
			/// <param name="count">The count of bytes to send</param>
			/// <param name="clientEndpoint">Address of receiving client</param>
			public void SendUDP(byte[] bytes, int count, EndPoint clientEndpoint, AsyncCallback callback)
			{
				try
				{
					int start = Environment.TickCount;

					m_udplisten.BeginSendTo(bytes, 0, count, SocketFlags.None, clientEndpoint, callback, m_udplisten);

					int took = Environment.TickCount - start;
					if (took > 100 && log.IsWarnEnabled)
						log.WarnFormat("m_udplisten.BeginSendTo took {0}ms! (UDP to {1})", took, clientEndpoint.ToString());
				}
				catch (SocketException)
				{
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error("SendUDP", e);
				}
			}

			/// <summary>
			/// Callback function for UDP sends
			/// </summary>
			/// <param name="ar">Asynchronous result of this operation</param>
			protected void SendToCallback(IAsyncResult ar)
			{
				if (ar == null) return;
				try
				{
					Socket s = (Socket) (ar.AsyncState);
					s.EndSendTo(ar);
				}
				catch (ObjectDisposedException)
				{
				}
				catch (SocketException)
				{
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error("SendToCallback", e);
				}
			}

			#endregion

			#region Start

			/// <summary>
			/// Starts the server
			/// </summary>
			/// <returns>True if the server was successfully started</returns>
			public override bool Start()
			{	
				m_status = eGameServerStatus.GSS_Closed;
				//Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
				AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

				//---------------------------------------------------------------
				//Try to init log4net (must be do first)
				if(!InitComponent(InitLog4Net(), "InitLog4Net()"))
					return false;
	
				if (log.IsDebugEnabled)
				{
					log.Debug("Current directory is: "+Directory.GetCurrentDirectory());
					log.Debug("Gameserver root directory is: "+Configuration.RootDirectory);
					log.Debug("Changing directory to root directory");
				}				
				Directory.SetCurrentDirectory(Configuration.RootDirectory);

				//---------------------------------------------------------------
				//Try to init the server port
				if (!InitComponent(InitSocket(), "InitSocket()"))
					return false;

				//---------------------------------------------------------------
				//Packet buffers
				if (!InitComponent(AllocatePacketBuffers(), "AllocatePacketBuffers()"))
					return false;

				//---------------------------------------------------------------
				//Try to start the udp port
				if (!InitComponent(StartUDP(), "StartUDP()"))
					return false;

				//---------------------------------------------------------------
				//Try to compile the Scripts
				if (!InitComponent(RecompileScripts(), "Script compilation"))
					return false;

				//---------------------------------------------------------------
				//Try to initialize the database after the script compilation
				if (!InitComponent(InitDB(), "Database Initialization"))
					return false;

				//---------------------------------------------------------------
				//Try to initialize the script components
				if (!InitComponent(StartScriptComponents(), "Script components"))
					return false;

				//---------------------------------------------------------------
				//Try to init the RSA key
				/* No Cryptlib currently
				if (log.IsInfoEnabled)
					log.Info("Generating RSA key, may take a minute, please wait...");
				if (!InitComponent(CryptLib168.GenerateRSAKey(), "RSA key generation"))
					return false;
				*/

				//---------------------------------------------------------------
				//Try to start the Language Manager
				if (!InitComponent(LanguageMgr.Initialize(), "Multi Language Initialization"))
					return false;

				//---------------------------------------------------------------
				//Try to start the Npc Templates Manager
				if (!InitComponent(NPCInventoryMgr.LoadAllNPCInventorys(), "NPCInventoryMgr"))
					return false;


				//---------------------------------------------------------------
				//Load all faction managers
				if (!InitComponent(FactionMgr.LoadAllFactions(), "Faction Managers"))
					return false;

				//---------------------------------------------------------------
				//Load all calculators
				if (!InitComponent(GameLiving.LoadCalculators(), "GameLiving.LoadCalculators()"))
					return false;

				//---------------------------------------------------------------
				//Try to initialize the WorldMgr
				if (!InitComponent(WorldMgr.Start(), "World Manager / Region Managers Initialization"))
					return false;
				
				//---------------------------------------------------------------
				//Load the house manager
				//if (!InitComponent(HouseMgr.Start(), "House Manager"))
				//	return false;

				//---------------------------------------------------------------
				//Load the area manager
				if (!InitComponent(AreaMgr.LoadAllAreas(), "AreaMgr"))
					return false;

				//---------------------------------------------------------------
				//Load the jump point manager
				if (!InitComponent(JumpPointMgr.LoadAllJumpPoints(), "JumpPointMgr"))
					return false;

				//---------------------------------------------------------------
				//Enable PlayerSave timer now
				if (m_timer != null)
				{
					m_timer.Change(Timeout.Infinite, Timeout.Infinite);
					m_timer.Dispose();
				}
				m_timer = new Timer(new TimerCallback(SaveTimerProc), null, SaveInterval*MINUTE_CONV, Timeout.Infinite);
				if (log.IsInfoEnabled)
					log.Info("Player save timer: true");
				 
				//---------------------------------------------------------------
				//Load all guilds
				if (!InitComponent(GuildMgr.LoadAllGuilds(), "GuildMgr"))
					return false;
				
				//---------------------------------------------------------------
				//Load the keep manager
				//if (!InitComponent(KeepMgr.Load(), "Keep Manager"))
				//	return false;

				//---------------------------------------------------------------
				//Load all weather managers
				if (!InitComponent(WeatherMgr.Load(), "Weather Managers"))
					return false;

				//---------------------------------------------------------------
				//Load all crafting managers
				if (!InitComponent(CraftingMgr.Init(), "Crafting Managers"))
					return false;

				//---------------------------------------------------------------
				//Load player titles manager
				if (!InitComponent(PlayerTitleMgr.Init(), "Player Titles Manager"))
					return false;

                //---------------------------------------------------------------
				//Load player titles manager
                if (!InitComponent(SpawnGeneratorMgr.LoadAllSpawnGenerator(), "Spawn Generator Manager"))
					return false;
                
				//---------------------------------------------------------------
				//Notify our scripts that everything went fine!
				GameEventMgr.Notify(ScriptEvent.Loaded);

				//---------------------------------------------------------------
				//Notify everyone that the server is now started!
				GameEventMgr.Notify(GameServerEvent.Started, this);

				//---------------------------------------------------------------
				//Try to start the base server (open server port for connections)
				if (!InitComponent(base.Start(), "base.Start()"))
					return false;

				GC.Collect(GC.MaxGeneration);

				//---------------------------------------------------------------
				//Open the server, players can now connect!
				m_status = eGameServerStatus.GSS_Open;

				if (log.IsInfoEnabled)
					log.Info("GameServer is now open for connections!");

				//INIT WAS FINE!
				return true;
			}

			/// <summary>
			/// Logs unhandled exceptions
			/// </summary>
			/// <param name="sender"></param>
			/// <param name="e"></param>
			private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
			{
				log.Fatal("Unhandled exception!\n" + e.ExceptionObject.ToString());
				if (e.IsTerminating)
					LogManager.Shutdown();
			}

			/// <summary>
			/// Recomiples the scripts dll
			/// </summary>
			/// <returns></returns>
			public bool RecompileScripts()
			{
				string scriptDirectory = Configuration.RootDirectory + Path.DirectorySeparatorChar + "scripts";
				if(!Directory.Exists(scriptDirectory))
					Directory.CreateDirectory(scriptDirectory);

				string[] parameters = Configuration.ScriptAssemblies.Split(',');
				return ScriptMgr.CompileScripts(false, scriptDirectory, Configuration.ScriptCompilationTarget, parameters);
			}

			/// <summary>
			/// Initialize all script components
			/// </summary>
			/// <returns>true if successfull, false if not</returns>
			protected bool StartScriptComponents()
			{
				try
				{
					//---------------------------------------------------------------
					//Create the server rules
					m_serverRules = ScriptMgr.CreateServerRules(Configuration.ServerType);
					if (log.IsInfoEnabled)
						log.Info("Server rules: true");

					//---------------------------------------------------------------
					//Load the skills
					SkillBase.LoadSkills();
					if (log.IsInfoEnabled)
						log.Info("Loading skills: true");

					//---------------------------------------------------------------
					//Register all event handlers
					ArrayList scripts = new ArrayList(ScriptMgr.Scripts);
					scripts.Insert(0, typeof(GameServer).Assembly);
					foreach (Assembly asm in scripts)
					{
						GameEventMgr.RegisterGlobalEvents(asm, typeof (GameServerStartedEventAttribute), GameServerEvent.Started);
						GameEventMgr.RegisterGlobalEvents(asm, typeof (GameServerStoppedEventAttribute), GameServerEvent.Stopped);
						GameEventMgr.RegisterGlobalEvents(asm, typeof (ScriptLoadedEventAttribute), ScriptEvent.Loaded);
						GameEventMgr.RegisterGlobalEvents(asm, typeof (ScriptUnloadedEventAttribute), ScriptEvent.Unloaded);
					}
					if (log.IsInfoEnabled)
						log.Info("Registering global event handlers: true");
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error("StartScriptComponents", e);
					return false;
				}
				//---------------------------------------------------------------
				return true;
			}

			/// <summary>
			/// Prints out some text info on component initialisation
			/// and stops the server again if the component failed
			/// </summary>
			/// <param name="componentInitState">The state</param>
			/// <param name="text">The text to print</param>
			/// <returns>false if startup should be interrupted</returns>
			protected bool InitComponent(bool componentInitState, string text)
			{
				if (log.IsInfoEnabled)
					log.Info(text + ": " + componentInitState);
				return componentInitState;
			}

			#endregion

			#region Stop

			/// <summary>
			/// Stops the server, disconnects all clients, and writes the database to disk
			/// </summary>
			public override void Stop()
			{
				//Stop new clients from logging in
				m_status = eGameServerStatus.GSS_Closed;

				log.Info("GameServer.Stop() - enter method");

				if (log.IsWarnEnabled)
				{
					string stacks = PacketProcessor.GetConnectionThreadpoolStacks();
					if (stacks.Length > 0)
					{
						log.Warn("Packet processor thread stacks:");
						log.Warn(stacks);
					}
				}

				//Notify our scripthandlers
				GameEventMgr.Notify(ScriptEvent.Unloaded);

				//Notify of the global server stop event
				//We notify before we shutdown the database
				//so that event handlers can use the datbase too
				GameEventMgr.Notify(GameServerEvent.Stopped, this);
				GameEventMgr.RemoveAllHandlers(true);

				//Stop the World Save timer
				if (m_timer != null)
				{
					m_timer.Change(Timeout.Infinite, Timeout.Infinite);
					m_timer.Dispose();
					m_timer = null;
				}

				//Stop the base server
				base.Stop();

				//Close the UDP connection
				if (m_udplisten != null)
				{
					m_udplisten.Close();
					m_udplisten = null;
				}

				//unload all weatherMgr
				WeatherMgr.Unload();

				//Save all players
				SaveTimerProc(null);

				WorldMgr.Stop();

				m_serverRules = null;

				Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

				if (log.IsInfoEnabled)
					log.Info("Server Stopped");
			}

			#endregion

			#region Packet buffer pool
			
			/// <summary>
			/// The size of all packet buffers.
			/// </summary>
			private const int BUF_SIZE = 2048;
			
			/// <summary>
			/// Holds all packet buffers.
			/// </summary>
			private Queue m_packetBufPool;
			
			/// <summary>
			/// Allocates all packet buffers.
			/// </summary>
			/// <returns>success</returns>
			private bool AllocatePacketBuffers()
			{
				int count = Configuration.MaxClientCount * 3;
				count += Math.Max(10*3, count*3/8);
				m_packetBufPool = new Queue(count);
				for (int i = 0; i < count; i++)
				{
					m_packetBufPool.Enqueue(new byte[BUF_SIZE]);
				}
				if (log.IsDebugEnabled)
					log.DebugFormat("allocated packet buffers: {0}", count.ToString());
				return true;
			}
			
			/// <summary>
			/// Gets the count of packet buffers in the pool.
			/// </summary>
			public int PacketPoolSize
			{
				get { return m_packetBufPool.Count; }
			}
			
			/// <summary>
			/// Gets packet buffer from the pool.
			/// </summary>
			/// <returns>byte array that will be used as packet buffer.</returns>
			public override byte[] AcquirePacketBuffer()
			{
				lock (m_packetBufPool.SyncRoot)
				{
					if (m_packetBufPool.Count > 0)
						return (byte[]) m_packetBufPool.Dequeue();
				}
				log.Warn("packet buffer pool is empty!");
				return new byte[BUF_SIZE];
			}
			
			/// <summary>
			/// Releases previously acquired packet buffer.
			/// </summary>
			/// <param name="buf">The released buf</param>
			public override void ReleasePacketBuffer(byte[] buf)
			{
				if (buf == null || GC.GetGeneration(buf) < GC.MaxGeneration)
					return;
				lock (m_packetBufPool.SyncRoot)
				{
					m_packetBufPool.Enqueue(buf);
				}
			}

			#endregion
			
			#region Client

			/// <summary>
			/// Creates a new client
			/// </summary>
			/// <returns>An instance of a new client</returns>
			protected override ClientBase GetNewClient()
			{
				GameClient client = new GameClient(this);
				GameEventMgr.Notify(GameClientEvent.Created, client);
				return client;
			}

			#endregion

			#region Log4Net

			/// <summary>
			/// Initializes Log4Net
			/// </summary>
			/// <returns>True if the Log4Net was successfully initialized</returns>
			public bool InitLog4Net()
			{
				//Try to find the log.config file, if it doesn't exist we create it
				FileInfo logConfig = new FileInfo(Configuration.LogConfigFile);
				if(!logConfig.Exists)
				{
					ResourceUtil.ExtractResource(logConfig.Name,logConfig.FullName);
				}
				//Configure and watch the config file
				XmlConfigurator.ConfigureAndWatch(logConfig);
				
				m_gmLog = LogManager.GetLogger(Configuration.GMActionsLoggerName);
				m_cheatLog = LogManager.GetLogger(Configuration.CheatLoggerName);

				return true;
			}

			/// <summary>
			/// Writes a line to the gm log file
			/// </summary>
			/// <param name="text">the text to log</param>
			public void LogGMAction(string text)
			{
				m_gmLog.Logger.Log(typeof(GameServer), Level.Alert, text, null);
			}

			/// <summary>
			/// Writes a line to the cheat log file
			/// </summary>
			/// <param name="text">the text to log</param>
			public void LogCheatAction(string text)
			{
				m_cheatLog.Logger.Log(typeof(GameServer), Level.Alert, text, null);
			}

			#endregion

			#region Database

			/// <summary>
			/// Initializes the database
			/// </summary>
			/// <returns>True if the database was successfully initialized</returns>
			public bool InitDB()
			{
				if (m_database == null)
				{
					try
					{
						if (log.IsInfoEnabled)
							log.Info("Loading database configuration ...");
			
						//Try to find the databaseconfig file, if it doesn't exist we create it
						if(!File.Exists(Configuration.DatabaseConfigFile))
						{
							ResourceUtil.ExtractResource(new FileInfo(Configuration.DatabaseConfigFile).Name, Configuration.DatabaseConfigFile);
						}

						if(!Configuration.UseReflectionOptimizer) NHibernate.Cfg.Environment.UseReflectionOptimizer = false;

						m_database = new ObjectDatabase(Configuration.DatabaseConfigFile); // Open the connection to the database
					}
					catch (Exception e)
					{
						if (log.IsErrorEnabled)
							log.Error("Error connecting to database", e);
						return false;
					}

					try
					{
						if(Configuration.DBAutoCreate)
						{
							if (log.IsInfoEnabled)
								log.Info("Creating database tables ...");
			
							m_database.CreateDatabaseStructure(Configuration.DatabaseConfigFile);
						}
						else
						{
							if (log.IsInfoEnabled)
								log.Info("Testing actual database structure ...");
			
							// Test the database structure
							ArrayList errors = new ArrayList();
							if(!m_database.TestDatabaseStructure(Configuration.DatabaseConfigFile, errors))
							{
								if (log.IsErrorEnabled)
								{
									log.Error("Wrong database structure found :");
						
									int i = 0;
									foreach(string msg in errors)
									{
										i++;
										log.Error(String.Format("{0,3}) {1}",i,msg));
									}
								}
								return false;
							}
						}
					}
					catch (Exception e)
					{
						if (log.IsErrorEnabled)
							log.Error("Error checking database structure", e);
						return false;
					}

				}
				return true;
			}

			/// <summary>
			/// Function called at X interval to write the database to disk
			/// </summary>
			/// <param name="sender">Object that generated the event</param>
			protected void SaveTimerProc(object sender)
			{
				try
				{
					int startTick = System.Environment.TickCount;
					if (log.IsInfoEnabled)
						log.Info("Saving players ...");
					if (log.IsDebugEnabled)
						log.Debug("Save ThreadId=" + AppDomain.GetCurrentThreadId());
					int saveCount = 0;
					
					ThreadPriority oldprio = Thread.CurrentThread.Priority;
					Thread.CurrentThread.Priority = ThreadPriority.Lowest;

					//Only save the players (+inventory + quests ect ...), NOT other objects!
					saveCount = WorldMgr.SavePlayers();

					Thread.CurrentThread.Priority = oldprio;
					
					if (log.IsInfoEnabled)
						log.Info("Saving database complete!");
					startTick = System.Environment.TickCount - startTick;
					if (log.IsInfoEnabled)
						log.Info("Saved " + saveCount + " players in " + startTick + "ms");
				}
				catch (Exception e1)
				{
					if (log.IsErrorEnabled)
						log.Error("SaveTimerProc", e1);
				}
				finally
				{
					if (m_timer != null)
						m_timer.Change(SaveInterval*MINUTE_CONV, Timeout.Infinite);
				}
			}

			#endregion

			#region Constructors
	
			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="config">A valid game server configuration</param>
			protected GameServer(GameServerConfiguration config) : base(config)
			{
			}
			#endregion

		}
	}
}