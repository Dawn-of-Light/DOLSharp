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
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using log4net;

namespace DOL.Network
{
	/// <summary>
	/// Base class for connected clients
	/// </summary>
	public class BaseClient
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Holds the receive callback delegate
		/// </summary>
		protected static readonly AsyncCallback ReceiveCallback = OnReceiveHandler;

		/// <summary>
		/// Packet buffer, holds incoming packet data
		/// </summary>
		protected byte[] _pbuf;

		/// <summary>
		/// Current offset into the buffer
		/// </summary>
		protected int _pBufEnd;

		/// <summary>
		/// Socket that holds the client connection
		/// </summary>
		protected Socket _sock;

		/// <summary>
		/// Pointer to the server the client is connected to
		/// </summary>
		protected BaseServer _srvr;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="srvr">Pointer to the server the client is connected to</param>
		public BaseClient(BaseServer srvr)
		{
			_srvr = srvr;

			if (srvr != null)
				_pbuf = srvr.AcquirePacketBuffer();

			_pBufEnd = 0;
		}

		/// <summary>
		/// Gets the server that the client is connected to
		/// </summary>
		public BaseServer Server
		{
			get { return _srvr; }
		}

		/// <summary>
		/// Gets or sets the socket the client is using
		/// </summary>
		public Socket Socket
		{
			get { return _sock; }
			set { _sock = value; }
		}

		/// <summary>
		/// Gets the packet buffer for the client
		/// </summary>
		public byte[] PacketBuf
		{
			get { return _pbuf; }
		}

		/// <summary>
		/// Gets or sets the offset into the receive buffer
		/// </summary>
		public int PacketBufSize
		{
			get { return _pBufEnd; }
			set { _pBufEnd = value; }
		}

		/// <summary>
		/// Gets the client's TCP endpoint address string, if connected
		/// </summary>
		public string TcpEndpointAddress
		{
			get
			{
				Socket s = _sock;
				if (s != null && s.Connected && s.RemoteEndPoint != null)
					return ((IPEndPoint) s.RemoteEndPoint).Address.ToString();

				return "not connected";
			}
		}

		/// <summary>
		/// Gets the client's TCP endpoint string, if connected
		/// </summary>
		public string TcpEndpoint
		{
			get
			{
				Socket s = _sock;
				if (s != null && s.Connected && s.RemoteEndPoint != null)
					return s.RemoteEndPoint.ToString();

				return "not connected";
			}
		}

		/// <summary>
		/// Called when data has been received from the connection
		/// </summary>
		/// <param name="numBytes">Number of bytes received in _pbuf</param>
		protected virtual void OnReceive(int numBytes)
		{
		}

		/// <summary>
		/// Called after the client connection has been accepted
		/// </summary>
		public virtual void OnConnect()
		{
		}

		/// <summary>
		/// Called right after the client has been disconnected
		/// </summary>
		public virtual void OnDisconnect()
		{
		}

		/// <summary>
		/// Tells the client to begin receiving data
		/// </summary>
		public void BeginReceive()
		{
			if (_sock != null && _sock.Connected)
			{
				int bufSize = _pbuf.Length;

				if (_pBufEnd >= bufSize) //Do we have space to receive?
				{
					if (Log.IsErrorEnabled)
					{
						Log.Error(TcpEndpoint + " disconnected because of buffer overflow!");
						Log.Error("_pBufEnd=" + _pBufEnd + "; buf size=" + bufSize);
						Log.Error(_pbuf);
					}

					_srvr.Disconnect(this);
				}
				else
				{
					_sock.BeginReceive(_pbuf, _pBufEnd, bufSize - _pBufEnd, SocketFlags.None, ReceiveCallback, this);
				}
			}
		}

		/// <summary>
		/// Called when a client has received data or the connection has been closed
		/// </summary>
		/// <param name="ar">Results of the receive operation</param>
		protected static void OnReceiveHandler(IAsyncResult ar)
		{
			if (ar == null)
				return;

			BaseClient baseClient = null;

			try
			{
				baseClient = (BaseClient) ar.AsyncState;
				int numBytes = baseClient.Socket.EndReceive(ar);

				if (numBytes > 0)
				{
					baseClient.OnReceive(numBytes);
					baseClient.BeginReceive();
				}
				else
				{
					if (Log.IsDebugEnabled)
						Log.Debug("Disconnecting client (" + baseClient.TcpEndpoint + "), received bytes=" + numBytes);

					baseClient._srvr.Disconnect(baseClient);
				}
			}
			catch (ObjectDisposedException)
			{
				if (baseClient != null)
					baseClient._srvr.Disconnect(baseClient);
			}
			catch (SocketException e)
			{
				if (baseClient != null)
				{
					if (Log.IsInfoEnabled)
						Log.Info(string.Format("{0}  {1}", baseClient.TcpEndpoint, e.Message));

					baseClient._srvr.Disconnect(baseClient);
				}
			}
			catch (Exception e)
			{
				if (Log.IsErrorEnabled)
					Log.Error("OnReceiveHandler", e);

				if (baseClient != null)
					baseClient._srvr.Disconnect(baseClient);
			}
		}

		/// <summary>
		/// Closes the client connection
		/// </summary>
		public void CloseConnections()
		{
			if (_sock != null)
			{
				try
				{
					_sock.Shutdown(SocketShutdown.Send);
				}
				catch
				{
				}

				try
				{
					_sock.Close();
				}
				catch
				{
				}
			}

			byte[] buff = _pbuf;
			if (buff != null)
			{
				_pbuf = null;
				_srvr.ReleasePacketBuffer(buff);
			}
		}

		/// <summary>
		/// Closes the client connection
		/// </summary>
		public void Disconnect()
		{
			try
			{
				_srvr.Disconnect(this);
			}
			catch (Exception e)
			{
				if (Log.IsErrorEnabled)
					Log.Error("Exception", e);
			}
		}
	}
}
