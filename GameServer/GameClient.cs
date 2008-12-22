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
using System.Text;
using System.Threading;
using DOL.Database;
using System.Net;
using System.Reflection;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL
{
	namespace GS
	{
		/// <summary>
		/// Represents a single connection to the game server
		/// </summary>
		public class GameClient : ClientBase
		{
			/// <summary>
			/// Defines a logger for this class.
			/// </summary>
			private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

			/// <summary>
			/// Current state of the client
			/// </summary>
			public enum eClientState
			{
				NotConnected = 0x00,
				Connecting = 0x01,
				CharScreen = 0x02,
				WorldEnter = 0x03,
				Playing = 0x04,
				Linkdead = 0x05,
				Disconnected = 0x06,
			};

			/// <summary>
			/// This variable holds the UDP endpoint of this client
			/// </summary>
			protected IPEndPoint m_udpEndpoint;
			/// <summary>
			/// Holds the current clientstate
			/// </summary>
			protected volatile eClientState m_clientState = eClientState.NotConnected;
			/// <summary>
			/// This variable holds all info about the active player
			/// </summary>
			protected GamePlayer m_player;
			/// <summary>
			/// This variable holds the active charindex
			/// </summary>
			protected int m_activeCharIndex;
			/// <summary>
			/// This variable holds the accountdata
			/// </summary>
			protected Account m_account;
			/// <summary>
			/// Holds the time of the last ping
			/// </summary>
			protected long m_pingTime = DateTime.Now.Ticks;	// give ping time on creation
			/// <summary>
			/// This variable holds the sessionid
			/// </summary>
			protected int m_sessionID = 0;
			/// <summary>
			/// Holds the time of the last UDP ping
			/// </summary>
			protected string m_localIP = "";
			/// <summary>
			/// Holds the time of the last UDP ping
			/// </summary>
			protected long m_udpPingTime = DateTime.Now.Ticks;
			/// <summary>
			/// This variable holds the UDP endpoint of this client
			/// </summary>
			protected volatile bool	m_udpConfirm;
			/// <summary>
			/// Constructor for a game client
			/// </summary>
			/// <param name="srvr">The server that's communicating with this client</param>
			public GameClient(BaseServer srvr)
				: base(srvr)
			{
				m_clientVersion = eClientVersion.VersionNotChecked;
				m_player = null;
				m_activeCharIndex = -1; //No character loaded yet!
			}

			/// <summary>
			/// Called when a packet has been received.
			/// </summary>
			/// <param name="num_bytes">The number of bytes received</param>
			/// <remarks>This function parses the incoming data into individual packets and then calls the appropriate handler.</remarks>
			public override void OnRecv(int num_bytes)
			{
				//This is the first received packet ...
				if (Version == eClientVersion.VersionNotChecked)
				{
					//Disconnect if the packet seems wrong
					if (num_bytes < 17) // 17 is correct bytes count for 0xF4 packet
					{
						if (log.IsWarnEnabled)
						{
							log.WarnFormat("Disconnected {0} in login phase because wrong packet size {1}", TcpEndpoint, num_bytes);
							log.Warn("num_bytes=" + num_bytes);
							log.Warn(Marshal.ToHexDump("packet buffer:", m_pbuf, 0, num_bytes));
						}
						GameServer.Instance.Disconnect(this);
						return;
					}

					//Currently, the version is sent with the first packet, no
					//matter what packet code it is
					int version = (m_pbuf[12] * 100) + (m_pbuf[13] * 10) + m_pbuf[14];

					eClientVersion ver;
					IPacketLib lib = AbstractPacketLib.CreatePacketLibForVersion(version, this, out ver);

					if (lib == null)
					{
						Version = eClientVersion.VersionUnknown;
						if (log.IsErrorEnabled)
							log.Error(TcpEndpoint + " Client Version " + version + " not handled in this server!");
						GameServer.Instance.Disconnect(this);
					}
					else
					{
						Version = ver;
						Out = lib;
						PacketProcessor = new PacketProcessor(this);
					}
				}

				if (Version != eClientVersion.VersionUnknown)
				{
					m_packetProcessor.ReceiveBytes(num_bytes);
				}
			}

			/// <summary>
			/// Called when this client has been disconnected
			/// </summary>
			public override void OnDisconnect()
			{
				try
				{
					if (PacketProcessor != null)
						PacketProcessor.OnDisconnect();

					//If we went linkdead and we were inside the game
					//we don't let the player disappear!
					if (ClientState == eClientState.Playing)
					{
						OnLinkdeath();
						return;
					}
					else if (ClientState == eClientState.WorldEnter && Player != null)
					{
						Player.SaveIntoDatabase();
					}
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error("OnDisconnect", e);
				}

				// Make sure the client is diconnected even on errors
				Quit();
			}

			/// <summary>
			/// Called when this client has connected
			/// </summary>
			public override void OnConnect()
			{
				GameEventMgr.Notify(GameClientEvent.Connected, this);
			}

			/// <summary>
			/// Loads a player from the DB
			/// </summary>
			/// <param name="accountindex">Index of the character within the account</param>
			public void LoadPlayer(int accountindex)
			{
				m_activeCharIndex = accountindex;
				GamePlayer player = null;
				Character car = m_account.Characters[m_activeCharIndex];

				Assembly gasm = Assembly.GetAssembly(typeof(GameServer));
				String playerClass;

				switch ((eCharacterClass)(m_account.Characters[m_activeCharIndex].Class))
				{
					case eCharacterClass.Disciple:
					case eCharacterClass.Necromancer:
						playerClass = "DOL.GS.GameNecromancer";
						break;
					default:
						playerClass = ServerProperties.Properties.PLAYER_CLASS;
						break;
				}

				try
				{
					player = (GamePlayer)gasm.CreateInstance(playerClass, false, BindingFlags.CreateInstance, null, new object[] { this, m_account.Characters[m_activeCharIndex] }, null, null);
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error("LoadPlayer", e);
				}
				if (player == null)
				{
					foreach (Assembly asm in ScriptMgr.Scripts)
					{
						try
						{
							player = (GamePlayer)asm.CreateInstance(playerClass, false, BindingFlags.CreateInstance, null, new object[] { this, m_account.Characters[m_activeCharIndex] }, null, null);
						}
						catch (Exception e)
						{
							if (log.IsErrorEnabled)
								log.Error("LoadPlayer", e);
						}
						if (player != null)
							break;
					}
				}
				if (player == null)
				{
					switch ((eCharacterClass)(m_account.Characters[m_activeCharIndex].Class))
					{
						case eCharacterClass.Disciple:
						case eCharacterClass.Necromancer:
							player = new GameNecromancer(this, m_account.Characters[m_activeCharIndex]);
							break;
						default:
							player = new GamePlayer(this, m_account.Characters[m_activeCharIndex]);
							break;
					}
				}
				Thread.MemoryBarrier();
				Player = player;
			}

			/// <summary>
			/// Saves a player to the DB
			/// </summary>
			public void SavePlayer()
			{
				try
				{
					if (m_activeCharIndex != -1 && m_player != null)
					{
						m_player.SaveIntoDatabase();
					}
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error("SavePlayer", e);
				}
			}

			/// <summary>
			/// Called when a player goes linkdead
			/// </summary>
			protected void OnLinkdeath()
			{
				if (log.IsDebugEnabled)
					log.Debug("Linkdeath called (" + Account.Name + ")  client state=" + ClientState);

				//If we have no sessionid we simply disconnect
				GamePlayer curPlayer = Player;
				if (m_sessionID == 0 || curPlayer == null)
				{
					Quit();
				}
				else
				{
					ClientState = eClientState.Linkdead;
					// If we have a good sessionid, we won't remove the client yet!
					// OnLinkdeath() can start a timer to remove the client "a bit later"
					curPlayer.OnLinkdeath();
				}
			}


			/// <summary>
			/// Quits a client from the world
			/// </summary>
			protected internal void Quit()
			{
				lock (this)
				{
					try
					{
						eClientState oldClientState = ClientState;
						if (m_sessionID != 0)
						{
							if (oldClientState == eClientState.Playing || oldClientState == eClientState.WorldEnter || oldClientState == eClientState.Linkdead)
							{
								try
								{
									if (Player != null)
										Player.Quit(true); //calls delete
									//m_player.Delete(true);
								}
								catch (Exception e)
								{
									log.Error("player cleanup on client quit", e);
								}
							}
							try
							{
								//Now free our objid and sessionid again
								WorldMgr.RemoveClient(this); //calls RemoveSessionID -> player.Delete
							}
							catch (Exception e)
							{
								log.Error("client cleanup on quit", e);
							}
						}
						ClientState = eClientState.Disconnected;
						Player = null;

						GameEventMgr.Notify(GameClientEvent.Disconnected, this);

						if (Account != null)
						{
							if (log.IsInfoEnabled)
							{
								if (m_udpEndpoint != null)
									log.Info("(" + m_udpEndpoint.Address.ToString() + ") " + Account.Name + " just disconnected!");
								else
									log.Info("(" + TcpEndpoint + ") " + Account.Name + " just disconnected!");
							}
						}
					}
					catch (Exception e)
					{
						if (log.IsErrorEnabled)
							log.Error("Quit", e);
					}
				}
			}

			/// <summary>
			/// UDP address for this client
			/// </summary>
			public IPEndPoint UDPEndPoint
			{
				get { return m_udpEndpoint; }
				set { m_udpEndpoint = value; }
			}

			/// <summary>
			/// Gets or sets the client state
			/// </summary>
			public eClientState ClientState
			{
				get { return m_clientState; }
				set
				{
					eClientState oldState = m_clientState;

					// refresh ping timeouts immediately when we change into playing state or charscreen
					if ((oldState != eClientState.Playing && value == eClientState.Playing) ||
						(oldState != eClientState.CharScreen && value == eClientState.CharScreen))
					{
						PingTime = DateTime.Now.Ticks;
					}

					m_clientState = value;
					GameEventMgr.Notify(GameClientEvent.StateChanged, this);
					//DOLConsole.WriteSystem("New State="+value.ToString());
				}
			}

			/// <summary>
			/// Gets whether or not the client is playing
			/// </summary>
			public bool IsPlaying
			{
				get
				{
					//Linkdead players also count as playing :)
					return m_clientState == eClientState.Playing || m_clientState == eClientState.Linkdead;
				}
			}

			/// <summary>
			/// Gets or sets the account being used by this client
			/// </summary>
			public Account Account
			{
				get { return m_account; }
				set
				{
					m_account = value;
					GameEventMgr.Notify(GameClientEvent.AccountLoaded, this);
				}
			}

			/// <summary>
			/// Gets or sets the player this client is using
			/// </summary>
			public GamePlayer Player
			{
				get { return m_player; }
				set
				{
					GamePlayer oldPlayer = Interlocked.Exchange(ref m_player, value);
					if (oldPlayer != null)
					{
						oldPlayer.CleanupOnDisconnect();
						oldPlayer.Delete();
					}
					GameEventMgr.Notify(GameClientEvent.PlayerLoaded, this); // hmm seems not right
				}
			}

			/// <summary>
			/// Gets or sets the character index for the player currently being used
			/// </summary>
			public int ActiveCharIndex
			{
				get { return m_activeCharIndex; }
				set { m_activeCharIndex = value; }
			}

			/// <summary>
			/// Gets or sets the session ID for this client
			/// </summary>
			public int SessionID
			{
				get { return m_sessionID; }
				internal set { m_sessionID = value; }
			}

			/// <summary>
			/// Gets/Sets the time of last ping packet
			/// </summary>
			public long PingTime
			{
				get { return m_pingTime; }
				set { m_pingTime = value; }
			}

			/// <summary>
			/// UDP address for this client
			/// </summary>
			public string LocalIP
			{
				get { return m_localIP; }
				set { m_localIP = value; }
			}

			/// <summary>
			/// Gets/Sets the time of last UDP ping packet
			/// </summary>
			public long UDPPingTime
			{
				get { return m_udpPingTime; }
				set { m_udpPingTime = value; }
			}

			/// <summary>
			/// UDP confirm flag from this client
			/// </summary>
			public bool UDPConfirm
			{
				get { return m_udpConfirm; }
				set { m_udpConfirm = value; }
			}

			/// <summary>
			/// The packetsender of this client
			/// </summary>
			protected IPacketLib m_packetLib;

			/// <summary>
			/// Gets or sets the packet sender
			/// </summary>
			public IPacketLib Out
			{
				get { return m_packetLib; }
				set { m_packetLib = value; }
			}

			/// <summary>
			/// The packetreceiver of this client
			/// </summary>
			protected PacketProcessor m_packetProcessor;

			/// <summary>
			/// Gets or Sets the packet receiver
			/// </summary>
			public PacketProcessor PacketProcessor
			{
				get { return m_packetProcessor; }
				set { m_packetProcessor = value; }
			}

			/// <summary>
			/// the version enum
			/// </summary>
			public enum eClientVersion : int
			{
				VersionNotChecked = -1,
				VersionUnknown = 0,
				_FirstVersion = 168,
				Version168 = 168,
				Version169 = 169,
				Version170 = 170,
				Version171 = 171,
				Version172 = 172,
				Version173 = 173,
				Version174 = 174,
				Version175 = 175,
				Version176 = 176,
				Version177 = 177,
				Version178 = 178,
				Version179 = 179,
				Version180 = 180,
				Version181 = 181,
				Version182 = 182,
				Version183 = 183,
				Version184 = 184,
				Version185 = 185,
				Version186 = 186,
				Version187 = 187,
				Version188 = 188,
				Version189 = 189,
				Version190 = 190,
				Version191 = 191,
				Version192 = 192,
				Version193 = 193,
				Version194 = 194,
				Version195 = 195,
				Version196 = 196,
				_LastVersion = 196,
			}
			protected eClientVersion m_clientVersion;
			/// <summary>
			/// the version of this client
			/// </summary>
			public eClientVersion Version
			{
				get { return m_clientVersion; }
				set { m_clientVersion = value; }
			}
			/// <summary>
			/// The client software type enum
			/// </summary>
			public enum eClientType : int
			{
				Unknown = -1,
				Classic = 1,
				ShroudedIsles = 2,
				TrialsOfAtlantis = 3,
				Catacombs = 4,
				DarknessRising = 5,
				LabyrinthOfTheMinotaur = 6,
			}
			/// <summary>
			/// The client addons enum
			/// </summary>
			[Flags]
			public enum eClientAddons : int
			{
				bit4 = 0x10,
				bit5 = 0x20,
				Foundations = 0x40,
				NewFrontiers = 0x80,
			}
			/// <summary>
			/// Holds client software type
			/// </summary>
			protected eClientType m_clientType = eClientType.Unknown;
			/// <summary>
			/// Holds installed client addons
			/// </summary>
			protected eClientAddons m_clientAddons;
			/// <summary>
			/// Gets/sets client software type (classic/SI/ToA/Catacombs)
			/// </summary>
			public eClientType ClientType
			{
				get { return m_clientType; }
				set { m_clientType = value; }
			}
			/// <summary>
			/// Gets/sets installed client addons (housing/new frontiers)
			/// </summary>
			public eClientAddons ClientAddons
			{
				get { return m_clientAddons; }
				set { m_clientAddons = value; }
			}

			/// <summary>
			/// Returns short informations about the client
			/// </summary>
			/// <returns></returns>
			public override string ToString()
			{
				return new StringBuilder(128)
					.Append(Version.ToString())
					.Append(" pakLib:").Append(Out == null ? "(null)" : Out.GetType().FullName)
					.Append(" type:").Append(ClientType)
					.Append('(').Append((eClientType)ClientType).Append(')')
					.Append(" addons:").Append(ClientAddons.ToString("G"))
					.Append(" state:").Append(ClientState.ToString())
					.Append(" IP:").Append(TcpEndpoint)
					.Append(" session:").Append(SessionID)
					.Append(" acc:").Append(Account == null ? "null" : Account.Name)
					.Append(" char:").Append(Player == null ? "null" : Player.Name)
					.Append(" class:").Append(Player == null ? "null" : Player.CharacterClass.ID.ToString())
					.ToString();
			}
		}
	}
}
