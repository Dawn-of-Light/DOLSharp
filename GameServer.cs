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

using DOL.Database;
using DOL.Database.Attributes;
using DOL.Database.Connection;
using DOL.Events;
using DOL.GS.DatabaseConverters;
using DOL.GS.Housing;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;
using DOL.GS.PlayerTitles;
using DOL.GS.Scripts;
using DOL.GS.ServerRules;
using DOL.GS.ServerProperties;
using DOL.Config;
using DOL.Language;
using log4net;
using log4net.Config;
using log4net.Core;
using DOL.GS.Quests;
using DOL.GS.Behaviour;

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

			bool debugMemory = true;

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
			/// Holds the startSystemTick when server is up.
			/// </summary>
			protected int m_startTick = 0;

			/// <summary>
			/// Minute conversion from milliseconds
			/// </summary>
			protected const int MINUTE_CONV = 60000;

			/// <summary>
			/// Socket that listens for UDP packets
			/// </summary>
			protected Socket m_udpListen;

			/// <summary>
			/// Socket that sends UDP packets
			/// </summary>
			protected Socket m_udpOutSocket;

			/// <summary>
			/// Receive buffer for UDP
			/// </summary>
			protected byte[] m_udpBuf;

			/// <summary>
			/// Maximum UDP buffer size
			/// </summary>
			protected const int MAX_UDPBUF = 4096;

			/// <summary>
			/// Database instance
			/// </summary>
			protected ObjectDatabase m_database = null;

			/// <summary>
			/// Contains a list of invalid names
			/// </summary>
			protected ArrayList m_invalidNames = new ArrayList();

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
				get { return Configuration.SaveInterval; }
				set
				{
					Configuration.SaveInterval = value;
					if (m_timer != null)
						m_timer.Change(value * MINUTE_CONV, Timeout.Infinite);
				}
			}


			/// <summary>
			/// Gets an array of invalid player names
			/// </summary>
			public ArrayList InvalidNames
			{
				get { return m_invalidNames; }
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

			/// <summary>
			/// Gets the number of millisecounds elapsed since the GameServer started.
			/// </summary>
			public int TickCount
			{
				get
				{
					return System.Environment.TickCount - m_startTick;
				}
			}

			#endregion

			#region Initialization

			/// <summary>
			/// Creates the gameserver instance
			/// </summary>
			/// <param name="config"></param>
			public static void CreateInstance(GameServerConfiguration config)
			{
				//Only one intance
				if (Instance != null)
					return;

				//Try to find the log.config file, if it doesn't exist
				//we create it
				FileInfo logConfig = new FileInfo(config.LogConfigFile);
				if (!logConfig.Exists)
				{
					ResourceUtil.ExtractResource(logConfig.Name, logConfig.FullName);
				}
				//Configure and watch the config file
				XmlConfigurator.ConfigureAndWatch(logConfig);
				//Create the instance
				m_instance = new GameServer(config);
			}

			/// <summary>
			/// Loads an array of invalid names
			/// </summary>
			public void LoadInvalidNames()
			{
				try
				{
					m_invalidNames.Clear();

					if (File.Exists(Configuration.InvalidNamesFile))
					{
						using (StreamReader file = File.OpenText(Configuration.InvalidNamesFile))
						{
							string line = null;
							while ((line = file.ReadLine()) != null)
							{
								if (line[0] == '#')
								{
									continue;
								}

								m_invalidNames.Add(line.ToLower());
							}

							file.Close();
						}
					}
					else
					{
						using (StreamWriter file = File.CreateText(Configuration.InvalidNamesFile))
						{
							file.WriteLine("#This file contains invalid name segments.");
							file.WriteLine("#If a player's name contains any portion of a segment it is rejected.");
							file.WriteLine("#Example: if a segment is \"bob\" then the name PlayerBobIsCool would be rejected");
							file.WriteLine("#The # symbol at the beginning of a line means a comment and will not be read");
							file.Flush();
							file.Close();
						}
					}
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error("LoadInvalidNames", e);
				}
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
					return m_udpListen;
				}
			}

			/// <summary>
			/// Gets the UDP buffer of this server instance
			/// </summary>
			protected byte[] UDPBuffer
			{
				get
				{
					return m_udpBuf;
				}
			}

			/// <summary>
			/// Starts the udp listening
			/// </summary>
			/// <returns>true if successfull</returns>
			protected bool StartUDP()
			{
				bool ret = true;
				try
				{
					// Open our udp socket
					m_udpListen = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
					m_udpListen.Bind(new IPEndPoint(Configuration.UDPIp, Configuration.UDPPort));
					
					// Bind out UDP socket
					m_udpOutSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
					if (Configuration.UDPOutEndpoint != null)
					{
						m_udpOutSocket.Bind(Configuration.UDPOutEndpoint);
					}

					ret = BeginReceiveUDP(m_udpListen, this);
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error("StartUDP", e);
					ret = false;
				}

				return ret;
			}

			/// <summary>
			/// Holds udp receive callback delegate
			/// </summary>
			protected readonly AsyncCallback m_udpReceiveCallback;

			/// <summary>
			/// UDP event handler. Called when a UDP packet is waiting to be read
			/// </summary>
			/// <param name="ar"></param>
			protected void RecvFromCallback(IAsyncResult ar)
			{
				if (m_status != eGameServerStatus.GSS_Open)
					return;

				if (ar == null)
					return;

				GameServer server = (GameServer)(ar.AsyncState);
				Socket s = server.UDPSocket;
				GameClient client = null;

				if (s != null)
				{
					//Creates a temporary EndPoint to pass to EndReceiveFrom.
					EndPoint tempRemoteEP = new IPEndPoint(IPAddress.Any, 0);
					bool receiving = false;
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
									log.Debug(Marshal.ToHexDump("UDP buffer dump, received " + read + "bytes", server.UDPBuffer));
							}
							else
							{
								IPEndPoint sender = (IPEndPoint)(tempRemoteEP);

								GSPacketIn pakin = new GSPacketIn(read - GSPacketIn.HDR_SIZE);
								pakin.Load(server.UDPBuffer, read);

								//Get the next message
								BeginReceiveUDP(s, server);
								receiving = true;

								client = WorldMgr.GetClientFromID(pakin.SessionID);

								if (client != null)
								{
									//If this is the first message from the client, we
									//save the endpoint!
									if (client.UDPEndPoint == null)
									{
										client.UDPEndPoint = sender;
										client.UDPConfirm = false;
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
					finally
					{
						if (!receiving)
						{
							//Get the next message
							//Even if we have errors, we need to continue with UDP
							BeginReceiveUDP(s, server);
						}
					}
				}
			}

			/// <summary>
			/// Starts receiving UDP packets.
			/// </summary>
			/// <param name="s">Socket to receive packets.</param>
			/// <param name="server">Server instance used to receive packets.</param>
			private bool BeginReceiveUDP(Socket s, GameServer server)
			{
				bool ret = false;
				EndPoint tempRemoteEP;
				tempRemoteEP = new IPEndPoint(IPAddress.Any, 0);
				try
				{
					s.BeginReceiveFrom(server.UDPBuffer, 0, MAX_UDPBUF, SocketFlags.None, ref tempRemoteEP, m_udpReceiveCallback, server);
					ret = true;
				}
				catch (SocketException e)
				{
					log.Fatal(string.Format("Failed to resume receiving UDP packets. UDP is DEAD now. (code: {0}  socketCode: {1})", e.ErrorCode, e.SocketErrorCode), e);
				}
				catch (ObjectDisposedException e)
				{
					log.Fatal("Tried to start UDP. Object disposed.", e);
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error("UDP Recv", e);
				}

				return ret;
			}

			/// <summary>
			/// Holds the async UDP send callback
			/// </summary>
			protected readonly AsyncCallback m_udpSendCallback;

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
			/// <param name="callback"></param>
			public void SendUDP(byte[] bytes, int count, EndPoint clientEndpoint, AsyncCallback callback)
			{
				int start = Environment.TickCount;

				m_udpOutSocket.BeginSendTo(bytes, 0, count, SocketFlags.None, clientEndpoint, callback, m_udpOutSocket);

				int took = Environment.TickCount - start;
				if (took > 100 && log.IsWarnEnabled)
					log.WarnFormat("m_udpListen.BeginSendTo took {0}ms! (UDP to {1})", took, clientEndpoint.ToString());
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
					Socket s = (Socket)(ar.AsyncState);
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
				if (debugMemory)
					log.Debug("Starting Server, Memory is " + GC.GetTotalMemory(false) / 1024 / 1024);
				m_status = eGameServerStatus.GSS_Closed;
				Thread.CurrentThread.Priority = ThreadPriority.Normal;

				AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

				//---------------------------------------------------------------
				//Check and convert the database version if older that current
				if (!CheckDatabaseVersion())
					return false;

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
				//Try to init the RSA key
				/* No Cryptlib currently
				if (log.IsInfoEnabled)
					log.Info("Generating RSA key, may take a minute, please wait...");
				if (!InitComponent(CryptLib168.GenerateRSAKey(), "RSA key generation"))
					return false;
				*/

				//---------------------------------------------------------------
				//Try to start the Language Manager
				if (!InitComponent(DOL.Language.LanguageMgr.Init(), "Multi Language Initialization"))
					return false;

				//Init the mail manager
				InitComponent(Mail.MailMgr.Init(), "Mail Manager Initialization");


				//---------------------------------------------------------------
				//Try to initialize the WorldMgr in early state
				RegionData[] regionsData;
				if (!InitComponent(WorldMgr.EarlyInit(out regionsData), "World Manager PreInitialization"))
					return false;

				//---------------------------------------------------------------
				//Try to compile the Scripts
				if (!InitComponent(RecompileScripts(), "Script compilation"))
					return false;

				//---------------------------------------------------------------
				//Try to initialize the script components
				if (!InitComponent(StartScriptComponents(), "Script components"))
					return false;

				//---------------------------------------------------------------
				//Load all faction managers
				if (!InitComponent(FactionMgr.Init(), "Faction Managers"))
					return false;

				//---------------------------------------------------------------
				//Load all calculators
				if (!InitComponent(GameLiving.LoadCalculators(), "GameLiving.LoadCalculators()"))
					return false;

				//---------------------------------------------------------------
				//Try to start the Npc Templates Manager
				if (!InitComponent(NpcTemplateMgr.Init(), "Npc Templates Manager"))
					return false;

				//---------------------------------------------------------------
				//Load the house manager
				if (!InitComponent(HouseMgr.Start(), "House Manager"))
					return false;

				//---------------------------------------------------------------
				//Load the region managers
				if (!InitComponent(WorldMgr.StartRegionMgrs(), "Region Managers"))
					return false;

				//---------------------------------------------------------------
				//Load the area manager
				if (!InitComponent(AreaMgr.LoadAllAreas(), "Areas"))
					return false;

				//---------------------------------------------------------------
				//Enable Worldsave timer now
				if (m_timer != null)
				{
					m_timer.Change(Timeout.Infinite, Timeout.Infinite);
					m_timer.Dispose();
				}
				m_timer = new Timer(new TimerCallback(SaveTimerProc), null, SaveInterval * MINUTE_CONV, Timeout.Infinite);
				if (log.IsInfoEnabled)
					log.Info("World save timer: true");

				//---------------------------------------------------------------
				//Load all guilds
				if (!InitComponent(GuildMgr.LoadAllGuilds(), "Guild Manager"))
					return false;

				//---------------------------------------------------------------
				//Load the keep manager
				if (!InitComponent(KeepMgr.Load(), "Keep Manager"))
					return false;

				//---------------------------------------------------------------
				//Load the door manager
				if (!InitComponent(DoorMgr.Init(), "Door Manager"))
					return false;

				//---------------------------------------------------------------
				//Try to initialize the WorldMgr
				if (!InitComponent(WorldMgr.Init(regionsData), "World Manager Initialization"))
					return false;
				regionsData = null;

				//---------------------------------------------------------------
				//Load the relic manager
				if (!InitComponent(RelicMgr.Init(), "Relic Manager"))
					return false;

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
                //Load behaviour manager
                if (!InitComponent(BehaviourMgr.Init(), "Behaviour Manager"))
                    return false;

                //Load the quest managers if enabled
                if (ServerProperties.Properties.LOAD_QUESTS)
                {
                    if (!InitComponent(QuestMgr.Init(), "Quest Manager"))
                        return false;
                }
				//---------------------------------------------------------------
				//Notify our scripts that everything went fine!
				GameEventMgr.Notify(ScriptEvent.Loaded);

				//---------------------------------------------------------------
				//Set the GameServer StartTick
				m_startTick = System.Environment.TickCount;
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
				if (!Directory.Exists(scriptDirectory))
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
						GameEventMgr.RegisterGlobalEvents(asm, typeof(GameServerStartedEventAttribute), GameServerEvent.Started);
						GameEventMgr.RegisterGlobalEvents(asm, typeof(GameServerStoppedEventAttribute), GameServerEvent.Stopped);
						GameEventMgr.RegisterGlobalEvents(asm, typeof(ScriptLoadedEventAttribute), ScriptEvent.Loaded);
						GameEventMgr.RegisterGlobalEvents(asm, typeof(ScriptUnloadedEventAttribute), ScriptEvent.Unloaded);
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
			/// Checks and convers database to newer versions
			/// </summary>
			/// <returns>true if all went fine, false if errors</returns>
			protected virtual bool CheckDatabaseVersion()
			{
				const string versionKey = "DatabaseVersion";
				const string errorKey = "LastError";

				XMLConfigFile xmlConfig = new XMLConfigFile();
				FileInfo versionFile = new FileInfo("./config/DatabaseVersion.xml");
				int currentVersion = 1;

				try
				{
					log.Info("Checking database version...");

					if (versionFile.Exists)
					{
						xmlConfig = XMLConfigFile.ParseXMLFile(versionFile);
						currentVersion = xmlConfig[versionKey].GetInt();
					}

					if (currentVersion < 0)
						currentVersion = 0;

					SortedList convertersByVersion = new SortedList();
					foreach (Type type in typeof(GameServer).Assembly.GetTypes())
					{
						if (!type.IsClass) continue;
						if (!typeof(IDatabaseConverter).IsAssignableFrom(type)) continue;
						object[] attributes = type.GetCustomAttributes(typeof(DatabaseConverterAttribute), false);
						if (attributes.Length <= 0) continue;
						DatabaseConverterAttribute attr = (DatabaseConverterAttribute)attributes[0];

						if (convertersByVersion.ContainsKey(attr.TargetVersion))
						{
							log.ErrorFormat("{0}: converter to version {1} is already defined!", type.FullName, attr.TargetVersion);
							return false;
						}
						if (attr.TargetVersion < 1)
						{
							log.ErrorFormat("{0}: converter version is {1}, should be higher than zero!", type.FullName, attr.TargetVersion);
							return false;
						}
						object instance = Activator.CreateInstance(type);
						convertersByVersion.Add(attr.TargetVersion, instance);
					}

					int prevVersion = 1;
					foreach (DictionaryEntry entry in convertersByVersion)
					{
						IDatabaseConverter converter = (IDatabaseConverter)entry.Value;
						int version = (int)entry.Key;
						if (prevVersion + 1 != version)
						{
							log.ErrorFormat("{0}: gap between database converters (prev converter version = {1}, current converter version = {2})", converter.GetType().FullName, prevVersion, version);
							return false;
						}
						prevVersion = version;
					}

					for (int i = currentVersion + 1; ; i++)
					{
						IDatabaseConverter conv = (IDatabaseConverter)convertersByVersion[i];
						if (conv == null)
							break;

						log.InfoFormat("Converting database to version {0}...", i);
						conv.ConvertDatabase();

						xmlConfig[versionKey].Set(i);
						versionFile.Refresh();
						xmlConfig.Save(versionFile);
					}
					return true;
				}
				catch (Exception e)
				{
					if (currentVersion == -1)
						currentVersion = 1;
					log.Error("Error checking/converting database version:", e);
					xmlConfig[versionKey].Set(currentVersion);
					xmlConfig[errorKey].Set(e.ToString());
					versionFile.Refresh();
					xmlConfig.Save(versionFile);
					return false;
				}
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
				if (debugMemory)
					log.Debug("Start Memory " + text + ": " + GC.GetTotalMemory(false) / 1024 / 1024);
				if (log.IsInfoEnabled)
					log.Info(text + ": " + componentInitState);
				if (!componentInitState)
					Stop();
				if (debugMemory)
					log.Debug("Finish Memory " + text + ": " + GC.GetTotalMemory(false) / 1024 / 1024);
				return componentInitState;
			}

			#endregion

			#region Stop

			public void Close()
			{
				m_status = eGameServerStatus.GSS_Closed;
			}

			public void Open()
			{
				m_status = eGameServerStatus.GSS_Open;
			}

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
				if (m_udpListen != null)
				{
					m_udpListen.Close();
					m_udpListen = null;
				}
				if (m_udpOutSocket != null)
				{
					m_udpOutSocket.Close();
					m_udpOutSocket = null;
				}

				//Stop all mobMgrs
				WorldMgr.StopRegionMgrs();

				//unload all weatherMgr
				WeatherMgr.Unload();

				//Stop the WorldMgr, save all players
				//WorldMgr.SaveToDatabase();
				SaveTimerProc(null);

				WorldMgr.Exit();


				//Save the database
				if (m_database != null)
				{
					m_database.WriteDatabaseTables();
					//move inactive accounts, characters, quests TODO and inventoryitems to archive
					if (ServerProperties.Properties.USE_ARCHIVING)
						m_database.ArchiveTables();
				}

				m_serverRules = null;

				Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

				if (log.IsInfoEnabled)
					log.Info("Server Stopped");
				
				LogManager.Shutdown();
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
				count += Math.Max(10 * 3, count * 3 / 8);
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
						return (byte[])m_packetBufPool.Dequeue();
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
				client.UDPConfirm = false;
				return client;
			}

			#endregion

			#region Logging
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
					DataConnection con = new DataConnection(Configuration.DBType, Configuration.DBConnectionString);
					m_database = new ObjectDatabase(con);
					try
					{
						//We will search our assemblies for DataTables by reflection so 
						//it is not neccessary anymore to register new tables with the 
						//server, it is done automatically!
						foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
						{
							// Walk through each type in the assembly
							foreach (Type type in assembly.GetTypes())
							{
								// Pick up a class
								if (type.IsClass != true)
									continue;
								object[] attrib = type.GetCustomAttributes(typeof(DataTable), true);
								if (attrib.Length > 0)
								{
									if (log.IsInfoEnabled)
										log.Info("Registering table: " + type.FullName);
									m_database.RegisterDataObject(type);
								}
							}
						}
					}
					catch (DatabaseException e)
					{
						if (log.IsErrorEnabled)
							log.Error("Error registering Tables", e);
						return false;
					}

					try
					{
						m_database.LoadDatabaseTables();
					}
					catch (DatabaseException e)
					{
						if (log.IsErrorEnabled)
							log.Error("Error loading Database", e);
						return false;
					}
				}
				if (log.IsInfoEnabled)
					log.Info("Database Initialization: true");
				return true;
			}

			/// <summary>
			/// Writes the database to disk
			/// </summary>
			public void SaveDatabase()
			{
				if (m_database != null)
					m_database.WriteDatabaseTables();
			}

			/// <summary>
			/// Function called at X interval to write the database to disk
			/// </summary>
			/// <param name="sender">Object that generated the event</param>
			protected void SaveTimerProc(object sender)
			{
				try
				{
					int startTick = Environment.TickCount;
					if (log.IsInfoEnabled)
						log.Info("Saving database...");
					if (log.IsDebugEnabled)
						log.Debug("Save ThreadId=" + Thread.CurrentThread.ManagedThreadId);
					int saveCount = 0;
					if (m_database != null)
					{
						ThreadPriority oldprio = Thread.CurrentThread.Priority;
						Thread.CurrentThread.Priority = ThreadPriority.Lowest;

						//Only save the players, NOT any other object!
						saveCount = WorldMgr.SavePlayers();

						//The following line goes through EACH region and EACH object
						//is tested for savability. A real waste of time, so it is commented out
						//WorldMgr.SaveToDatabase();

						GuildMgr.SaveAllGuilds();

						FactionMgr.SaveAllAggroToFaction();

						m_database.WriteDatabaseTables();
						Thread.CurrentThread.Priority = oldprio;
					}
					if (log.IsInfoEnabled)
						log.Info("Saving database complete!");
					startTick = Environment.TickCount - startTick;
					if (log.IsInfoEnabled)
						log.Info("Saved all databases and " + saveCount + " players in " + startTick + "ms");
				}
				catch (Exception e1)
				{
					if (log.IsErrorEnabled)
						log.Error("SaveTimerProc", e1);
				}
				finally
				{
					if (m_timer != null)
						m_timer.Change(SaveInterval * MINUTE_CONV, Timeout.Infinite);
					DOL.Events.GameEventMgr.Notify(GameServerEvent.WorldSave);
				}
			}

			#endregion

			#region Constructors
			/// <summary>
			/// Default game server constructor
			/// </summary>
			protected GameServer()
				: this(new GameServerConfiguration())
			{
			}

			/// <summary>
			/// Constructor with a given configuration
			/// </summary>
			/// <param name="config">A valid game server configuration</param>
			protected GameServer(GameServerConfiguration config)
				: base(config)
			{
				

				m_gmLog = LogManager.GetLogger(Configuration.GMActionsLoggerName);
				m_cheatLog = LogManager.GetLogger(Configuration.CheatLoggerName);

				if (log.IsDebugEnabled)
				{
					log.Debug("Current directory is: " + Directory.GetCurrentDirectory());
					log.Debug("Gameserver root directory is: " + Configuration.RootDirectory);
					log.Debug("Changing directory to root directory");
				}
				Directory.SetCurrentDirectory(Configuration.RootDirectory);

				try
				{
					LoadInvalidNames();

					m_udpBuf = new byte[MAX_UDPBUF];
					m_udpReceiveCallback = new AsyncCallback(RecvFromCallback);
					m_udpSendCallback = new AsyncCallback(SendToCallback);

					if (!InitDB() || m_database == null)
					{
						if (log.IsErrorEnabled)
							log.Error("Could not initialize DB, please check path/connection string");
						throw new ApplicationException("DB initialization error");
					}

					if (log.IsInfoEnabled)
						log.Info("Game Server Initialization finished!");
				}
				catch (Exception e)
				{
					if (log.IsFatalEnabled)
						log.Fatal("GameServer initialization failed!", e);
					throw new ApplicationException("Fatal Error: Could not initialize Game Server", e);
				}
			}
			#endregion

		}
	}
}
