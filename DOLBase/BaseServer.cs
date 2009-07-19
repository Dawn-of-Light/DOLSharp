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
using System.Collections.Specialized;
using System.Net;
using System.Net.Sockets;
using log4net;
/*using DOL.NatTraversal.Interop;
using DOL.NatTraversal;*/

namespace DOL
{
	/// <summary>
	/// The status of the gameserver
	/// </summary>
	public enum eGameServerStatus
	{
		/// <summary>
		/// Server is open for connections
		/// </summary>
		GSS_Open = 0,
		/// <summary>
		/// Server is closed and won't accept connections
		/// </summary>
		GSS_Closed,
		/// <summary>
		/// Server is down
		/// </summary>
		GSS_Down,
		/// <summary>
		/// Server is full, no more connections accepted
		/// </summary>
		GSS_Full,
		/// <summary>
		/// Unknown server status
		/// </summary>
		GSS_Unknown,
		/// <summary>
		/// Server is banned for the user
		/// </summary>
		GSS_Banned,
		/// <summary>
		/// User is not invited
		/// </summary>
		GSS_NotInvited,
		/// <summary>
		/// The count of server stati
		/// </summary>
		_GSS_Count,
	}

	/// <summary>
	/// The different game server types
	/// </summary>
	public enum eGameServerType : int
	{
		/// <summary>
		/// Normal server
		/// </summary>
		GST_Normal = 0,
		/// <summary>
		/// Test server
		/// </summary>
		GST_Test = 1,
		/// <summary>
		/// Player vs Player
		/// </summary>
		GST_PvP = 2,
		/// <summary>
		/// Player vs Monsters
		/// </summary>
		GST_PvE = 3,
		/// <summary>
		/// Roleplaying server
		/// </summary>
		GST_Roleplay = 4,
		/// <summary>
		/// Casual server
		/// </summary>
		GST_Casual = 5,
		/// <summary>
		/// Unknown server type
		/// </summary>
		GST_Unknown = 6,
		/// <summary>
		/// The count of server types
		/// </summary>
		_GST_Count = 7,
	}

	/// <summary>
	/// Base class for a server using overlapped socket IO
	/// </summary>
	public class BaseServer
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly int SEND_BUFF_SIZE = 16 * 1024;

		/// <summary>
		/// Hash table of clients
		/// </summary>
		protected readonly HybridDictionary m_clients = new HybridDictionary();

		/// <summary>
		/// Socket that receives connections
		/// </summary>
		protected Socket m_listen;

		/// <summary>
		/// The configuration of this server
		/// </summary>
		protected BaseServerConfiguration m_config;

		/// <summary>
		/// Default Constructor
		/// </summary>
		protected BaseServer() : this(new BaseServerConfiguration())
		{
		}

		/// <summary>
		/// Constructor that takes a server configuration as parameter
		/// </summary>
		/// <param name="config">The configuraion for the server</param>
		protected BaseServer(BaseServerConfiguration config)
		{
			if(config == null)
				throw new ArgumentNullException("config");
			
			m_config = config;
			m_asyncAcceptCallback = new AsyncCallback(AcceptCallback);
		}

		/// <summary>
		/// Retrieves the server configuration
		/// </summary>
		public virtual BaseServerConfiguration Configuration
		{
			get
			{
				return m_config;
			}
		}

		/// <summary>
		/// Returns the number of clients currently connected to the server
		/// </summary>
		public int ClientCount
		{
			get { return m_clients.Count; }
		}

		/// <summary>
		/// Creates a new client object
		/// </summary>
		/// <returns>A new client object</returns>
		protected virtual ClientBase GetNewClient()
		{
			return new ClientBase(this);
		}
		
		/// <summary>
		/// Used to get packet buffer.
		/// </summary>
		/// <returns>byte array that will be used as packet buffer.</returns>
		public virtual byte[] AcquirePacketBuffer()
		{
			return new byte[2048];
		}
		
		/// <summary>
		/// Releases previously acquired packet buffer.
		/// </summary>
		/// <param name="buf"></param>
		public virtual void ReleasePacketBuffer(byte[] buf)
		{
		}

		/// <summary>
		/// Initializes and binds the socket, doesn't listen yet!
		/// </summary>
		/// <returns>true if bound</returns>
		public virtual bool InitSocket()
		{
			try
			{
				m_listen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				m_listen.Bind(new IPEndPoint(m_config.Ip, m_config.Port));
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("InitSocket", e);

				return false;
			}
			return true;
		}

		/// <summary>
		/// Starts the server
		/// </summary>
		/// <returns>True if the server was successfully started</returns>
		public virtual bool Start()
		{
			/*if(Configuration.EnableUPnP)
			{
				try
				{
					UPnPNat nat = new UPnPNat();
					ArrayList list = new ArrayList();
					foreach(PortMappingInfo info in nat.PortMappings)
					{
						list.Add(info);
					}
					if(log.IsDebugEnabled)
					{
						log.Debug("Current UPnP mappings:");
						foreach(PortMappingInfo info in list)
						{
							log.DebugFormat("({0}) {1} - {2} -> {3}:{4}({5})", info.Enabled ? "(Enabled)" : "(Disabled)", info.Description, info.ExternalPort, info.InternalHostName, info.InternalPort, info.Protocol);
						}
					}
					IPAddress localAddr = Configuration.Ip;
					string address = "";
					if(localAddr.ToString() == IPAddress.Any.ToString())
					{
						IPAddress[] addr = Dns.GetHostByName(Dns.GetHostName()).AddressList;
						address = addr[0].ToString();
					}
					else
					{
						address = Configuration.Ip.ToString();
					}

					PortMappingInfo pmiUdp = new PortMappingInfo("DOL UDP", "UDP", address, Configuration.UDPPort, Configuration.UDPPort, true);
					PortMappingInfo pmiTcp = new PortMappingInfo("DOL TCP", "TCP", address, Configuration.Port, Configuration.Port, true);
					nat.AddPortMapping(pmiUdp);
					nat.AddPortMapping(pmiTcp);

					if(Configuration.DetectRegionIP)
					{
						try
						{
							Configuration.RegionIp = nat.PortMappings[0].ExternalIPAddress;
							if(log.IsDebugEnabled)
								log.Debug("Found the RegionIP: " + Configuration.RegionIp);
						}
						catch(Exception)
						{
							if(log.IsDebugEnabled)
								log.Debug("Unable to detect the RegionIP, It is possible that no mappings exist yet");
						}
					}
				}
				catch(Exception)
				{
					if(log.IsDebugEnabled)
						log.Debug("Unable to access the UPnP Internet Gateway Device");
				}
			}*/
			//Test if we have a valid port yet
			//if not try  binding.
			if(m_listen == null && !InitSocket())
				return false;

			try
			{
				m_listen.Listen(100);
				m_listen.BeginAccept(m_asyncAcceptCallback, this);

				if (log.IsDebugEnabled)
					log.Debug("Server is now listening to incoming connections!");
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("Start", e);
				if (m_listen != null)
					m_listen.Close();
				return false;
			}
			return true;
		}

		/// <summary>
		/// Holds the async accept callback delegate
		/// </summary>
		private readonly AsyncCallback m_asyncAcceptCallback;

		/// <summary>
		/// Called when a client is trying to connect to the server
		/// </summary>
		/// <param name="ar">Async result of the operation</param>
		private void AcceptCallback(IAsyncResult ar)
		{
			Socket sock = null;
			try
			{
				if (m_listen == null)
				{
					return;
				}
				sock = m_listen.EndAccept(ar);
				sock.SendBufferSize = SEND_BUFF_SIZE;

				ClientBase client = null;
				try
				{
					if (log.IsInfoEnabled)
					{
						string ip = sock.Connected ? sock.RemoteEndPoint.ToString() : "socket disconnected";
						log.Info("Incoming connection from " + ip);
					}

					client = GetNewClient();
					client.Socket = sock;

					lock (m_clients.SyncRoot)
						m_clients.Add(client, client);

					client.OnConnect();
					client.BeginRecv();
				}
				catch (SocketException)
				{
					if (client != null)
						Disconnect(client);
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error("Client creation", e);

					if (client != null)
						Disconnect(client);
				}
			}
			catch
			{
				if (sock != null) // don't leave the socket open on exception
					try { sock.Close(); }
					catch { }
			}
			finally
			{
				if (m_listen != null)
				{
					m_listen.BeginAccept(m_asyncAcceptCallback, this);
				}
			}
		}

		/// <summary>
		/// Stops the server
		/// </summary>
		public virtual void Stop()
		{
			if(log.IsDebugEnabled)
				log.Debug("Stopping server! - Entering method");
			
			/*if(Configuration.EnableUPnP)
			{
				try
				{
					if(log.IsDebugEnabled)
						log.Debug("Removing UPnP Mappings");
					UPnPNat nat = new UPnPNat();
					PortMappingInfo pmiUDP = new PortMappingInfo("UDP", Configuration.UDPPort);
					PortMappingInfo pmiTCP = new PortMappingInfo("TCP", Configuration.Port);
					nat.RemovePortMapping(pmiUDP);
					nat.RemovePortMapping(pmiTCP);
				}
				catch(Exception ex)
				{
					if(log.IsDebugEnabled)
						log.Debug("Failed to rmeove UPnP Mappings", ex);
				}
			}*/

			try
			{
				if (m_listen != null)
				{
					Socket socket = m_listen;
					m_listen = null;
					socket.Close();
					if (log.IsDebugEnabled)
						log.Debug("Server is no longer listening for incoming connections!");
				}
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("Stop", e);
			}
			
			if(m_clients != null)
			{
				lock (m_clients.SyncRoot)
				{
					try
					{
						IDictionaryEnumerator iter = m_clients.GetEnumerator();
						while (iter.MoveNext())
						{
							ClientBase client = (ClientBase) iter.Key;
							client.CloseConnections();
						}

						if(log.IsDebugEnabled)
							log.Debug("Stopping server! - Cleaning up client list!");

						m_clients.Clear();
					}
					catch(Exception e)
					{
						if(log.IsErrorEnabled)
							log.Error("Stop",e);
					}
				}
			}
			if(log.IsDebugEnabled)
				log.Debug("Stopping server! - End of method!");
		}

		/// <summary>
		/// Disconnects a client
		/// </summary>
		/// <param name="client">Client to be disconnected</param>
		/// <returns>True if the client was disconnected, false if it doesn't exist</returns>
		public virtual bool Disconnect(ClientBase client)
		{
			lock (m_clients.SyncRoot)
			{
				if (!m_clients.Contains(client))
					return false;
				m_clients.Remove(client);
			}

			try
			{
				client.OnDisconnect();
			}
			catch(Exception e)
			{
				if(log.IsErrorEnabled)
					log.Error("Exception", e);
			}

			try
			{
				client.CloseConnections();
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("Exception", e);
			}
			return true;
		}
	}
}