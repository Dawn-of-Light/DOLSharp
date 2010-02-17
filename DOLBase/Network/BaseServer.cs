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
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using DOL.Config;
using log4net;

namespace DOL.Network
{
	/// <summary>
	/// Base class for a server using overlapped socket IO.
	/// </summary>
	public class BaseServer
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Holds the async accept callback delegate
		/// </summary>
		private readonly AsyncCallback _asyncAcceptCallback;

		/// <summary>
		/// Hash table of clients
		/// </summary>
		protected readonly Dictionary<BaseClient, BaseClient> _clients = new Dictionary<BaseClient, BaseClient>();

		/// <summary>
		/// The configuration of this server
		/// </summary>
		protected BaseServerConfiguration _config;

		/// <summary>
		/// Socket that receives connections
		/// </summary>
		protected Socket _listen;

		/// <summary>
		/// Constructor that takes a server configuration as parameter
		/// </summary>
		/// <param name="config">The configuraion for the server</param>
		protected BaseServer(BaseServerConfiguration config)
		{
			if (config == null)
				throw new ArgumentNullException("config");

			_config = config;
			_asyncAcceptCallback = new AsyncCallback(AcceptCallback);
		}

		/// <summary>
		/// Retrieves the server configuration
		/// </summary>
		public virtual BaseServerConfiguration Configuration
		{
			get { return _config; }
		}

		/// <summary>
		/// Returns the number of clients currently connected to the server
		/// </summary>
		public int ClientCount
		{
			get { return _clients.Count; }
		}

		/// <summary>
		/// Creates a new client object
		/// </summary>
		/// <returns>A new client object</returns>
		protected virtual BaseClient GetNewClient()
		{
			return new BaseClient(this);
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
		protected virtual bool InitSocket()
		{
			try
			{
				_listen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				_listen.Bind(new IPEndPoint(_config.IP, _config.Port));
			}
			catch (Exception e)
			{
				if (Log.IsErrorEnabled)
					Log.Error("InitSocket", e);

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
			/*if(Configuration.EnableUPNP)
			{
				try
				{
					UPnPNat nat = new UPnPNat();
					ArrayList list = new ArrayList();
					foreach(PortMappingInfo info in nat.PortMappings)
					{
						list.Add(info);
					}
					if(Log.IsDebugEnabled)
					{
						Log.Debug("Current UPnP mappings:");
						foreach(PortMappingInfo info in list)
						{
							Log.DebugFormat("({0}) {1} - {2} -> {3}:{4}({5})", info.Enabled ? "(Enabled)" : "(Disabled)", info.Description, info.ExternalPort, info.InternalHostName, info.InternalPort, info.Protocol);
						}
					}
					IPAddress localAddr = Configuration.IP;
					string address = "";
					if(localAddr.ToString() == IPAddress.Any.ToString())
					{
						IPAddress[] addr = Dns.GetHostByName(Dns.GetHostName()).AddressList;
						address = addr[0].ToString();
					}
					else
					{
						address = Configuration.IP.ToString();
					}

					PortMappingInfo pmiUdp = new PortMappingInfo("DOL UDP", "UDP", address, Configuration.UDPPort, Configuration.UDPPort, true);
					PortMappingInfo pmiTcp = new PortMappingInfo("DOL TCP", "TCP", address, Configuration.Port, Configuration.Port, true);
					nat.AddPortMapping(pmiUdp);
					nat.AddPortMapping(pmiTcp);

					if(Configuration.DetectRegionIP)
					{
						try
						{
							Configuration.RegionIP = nat.PortMappings[0].ExternalIPAddress;
							if(Log.IsDebugEnabled)
								Log.Debug("Found the RegionIP: " + Configuration.RegionIP);
						}
						catch(Exception)
						{
							if(Log.IsDebugEnabled)
								Log.Debug("Unable to detect the RegionIP, It is possible that no mappings exist yet");
						}
					}
				}
				catch(Exception)
				{
					if(Log.IsDebugEnabled)
						Log.Debug("Unable to access the UPnP Internet Gateway Device");
				}
			}*/
			//Test if we have a valid port yet
			//if not try  binding.
			if (_listen == null && !InitSocket())
				return false;

			try
			{
				_listen.Listen(100);
				_listen.BeginAccept(_asyncAcceptCallback, this);

				if (Log.IsDebugEnabled)
					Log.Debug("Server is now listening to incoming connections!");
			}
			catch (Exception e)
			{
				if (Log.IsErrorEnabled)
					Log.Error("Start", e);

				if (_listen != null)
					_listen.Close();

				return false;
			}

			return true;
		}

		/// <summary>
		/// Called when a client is trying to connect to the server
		/// </summary>
		/// <param name="ar">Async result of the operation</param>
		private void AcceptCallback(IAsyncResult ar)
		{
			Socket sock = null;

			try
			{
				if (_listen == null)
					return;

				sock = _listen.EndAccept(ar);

				sock.SendBufferSize = Constants.SendBufferSize;
				sock.ReceiveBufferSize = Constants.ReceiveBufferSize;
				sock.NoDelay = Constants.UseNoDelay;

				BaseClient baseClient = null;

				try
				{
					if (Log.IsInfoEnabled)
					{
						string ip = sock.Connected ? sock.RemoteEndPoint.ToString() : "socket disconnected";
						Log.Info("Incoming connection from " + ip);
					}

					baseClient = GetNewClient();
					baseClient.Socket = sock;

					lock (_clients)
						_clients.Add(baseClient, baseClient);

					baseClient.OnConnect();
					baseClient.BeginReceive();
				}
				catch (SocketException)
				{
					if (baseClient != null)
						Disconnect(baseClient);
				}
				catch (Exception e)
				{
					if (Log.IsErrorEnabled)
						Log.Error("Client creation", e);

					if (baseClient != null)
						Disconnect(baseClient);
				}
			}
			catch
			{
				if (sock != null) // don't leave the socket open on exception
				{
					try
					{
						sock.Close();
					}
					catch
					{
					}
				}
			}
			finally
			{
				if (_listen != null)
				{
					_listen.BeginAccept(_asyncAcceptCallback, this);
				}
			}
		}

		/// <summary>
		/// Stops the server
		/// </summary>
		public virtual void Stop()
		{
			if (Log.IsDebugEnabled)
				Log.Debug("Stopping server! - Entering method");

			/*if(Configuration.EnableUPNP)
			{
				try
				{
					if(Log.IsDebugEnabled)
						Log.Debug("Removing UPnP Mappings");
					UPnPNat nat = new UPnPNat();
					PortMappingInfo pmiUDP = new PortMappingInfo("UDP", Configuration.UDPPort);
					PortMappingInfo pmiTCP = new PortMappingInfo("TCP", Configuration.Port);
					nat.RemovePortMapping(pmiUDP);
					nat.RemovePortMapping(pmiTCP);
				}
				catch(Exception ex)
				{
					if(Log.IsDebugEnabled)
						Log.Debug("Failed to rmeove UPnP Mappings", ex);
				}
			}*/

			try
			{
				if (_listen != null)
				{
					Socket socket = _listen;
					_listen = null;
					socket.Close();

					if (Log.IsDebugEnabled)
						Log.Debug("Server is no longer listening for incoming connections!");
				}
			}
			catch (Exception e)
			{
				if (Log.IsErrorEnabled)
					Log.Error("Stop", e);
			}

			if (_clients != null)
			{
				lock (_clients)
				{
					try
					{
						foreach (var clientPair in _clients)
						{
							clientPair.Key.CloseConnections();
						}

						if (Log.IsDebugEnabled)
							Log.Debug("Stopping server! - Cleaning up client list!");

						_clients.Clear();
					}
					catch (Exception e)
					{
						if (Log.IsErrorEnabled)
							Log.Error("Stop", e);
					}
				}
			}
			if (Log.IsDebugEnabled)
				Log.Debug("Stopping server! - End of method!");
		}

		/// <summary>
		/// Disconnects a client
		/// </summary>
		/// <param name="baseClient">Client to be disconnected</param>
		/// <returns>True if the client was disconnected, false if it doesn't exist</returns>
		public virtual bool Disconnect(BaseClient baseClient)
		{
			lock (_clients)
			{
				if (!_clients.ContainsKey(baseClient))
					return false;

				_clients.Remove(baseClient);
			}

			try
			{
				baseClient.OnDisconnect();
				baseClient.CloseConnections();
			}
			catch (Exception e)
			{
				if (Log.IsErrorEnabled)
					Log.Error("Exception", e);

				return false;
			}

			return true;
		}
	}
}