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
	/// Base class representing a game client.
	/// </summary>
	public class BaseClient
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// The callback when the client receives data.
		/// </summary>
		private static readonly AsyncCallback ReceiveCallback = OnReceiveHandler;

		/// <summary>
		/// Receive buffer for the client.
		/// </summary>
		protected byte[] _pBuf;

		/// <summary>
		/// The current offset into the receive buffer.
		/// </summary>
		protected int _pBufOffset;

		/// <summary>
		/// The socket for the client's connection to the server.
		/// </summary>
		protected Socket _socket;

		/// <summary>
		/// The current server instance that is servicing this client.
		/// </summary>
		protected BaseServer _srvr;

		/// <summary>
		/// Creates a new client.
		/// </summary>
		/// <param name="srvr">the server that is servicing this client</param>
		public BaseClient(BaseServer srvr)
		{
			_srvr = srvr;

			if (srvr != null)
				_pBuf = srvr.AcquirePacketBuffer();

			_pBufOffset = 0;
		}

		/// <summary>
		/// Gets the current server instance that is servicing this client.
		/// </summary>
		public BaseServer Server
		{
			get { return _srvr; }
		}

		/// <summary>
		/// Gets/sets the socket for the client's connection to the server.
		/// </summary>
		public Socket Socket
		{
			get { return _socket; }
			set { _socket = value; }
		}

		/// <summary>
		/// Gets the receive buffer for the client.
		/// </summary>
		public byte[] ReceiveBuffer
		{
			get { return _pBuf; }
		}

		/// <summary>
		/// Gets/sets the offset into the receive buffer.
		/// </summary>
		public int ReceiveBufferOffset
		{
			get { return _pBufOffset; }
			set { _pBufOffset = value; }
		}

		/// <summary>
		/// Gets the client's TCP endpoint address, if connected.
		/// </summary>
		public string TcpEndpointAddress
		{
			get
			{
				Socket s = _socket;
				if (s != null && s.Connected && s.RemoteEndPoint != null)
					return ((IPEndPoint) s.RemoteEndPoint).Address.ToString();

				return "not connected";
			}
		}

		/// <summary>
		/// Gets the client's TCP endpoint, if connected.
		/// </summary>
		public string TcpEndpoint
		{
			get
			{
				Socket s = _socket;
				if (s != null && s.Connected && s.RemoteEndPoint != null)
					return s.RemoteEndPoint.ToString();

				return "not connected";
			}
		}

		/// <summary>
		/// Called when the client has received data.
		/// </summary>
		/// <param name="numBytes">number of bytes received in _pBuf</param>
		protected virtual void OnReceive(int numBytes)
		{
		}

		/// <summary>
		/// Called after the client connection has been accepted.
		/// </summary>
		public virtual void OnConnect()
		{
		}

		/// <summary>
		/// Called right after the client has been disconnected.
		/// </summary>
		public virtual void OnDisconnect()
		{
		}

		/// <summary>
		/// Starts listening for incoming data.
		/// </summary>
		public void BeginReceive()
		{
			if (_socket != null && _socket.Connected)
			{
				int bufSize = _pBuf.Length;

				if (_pBufOffset >= bufSize) //Do we have space to receive?
				{
					if (Log.IsErrorEnabled)
					{
						Log.Error(TcpEndpoint + " disconnected because of buffer overflow!");
						Log.Error("_pBufOffset=" + _pBufOffset + "; buf size=" + bufSize);
						Log.Error(_pBuf);
					}

					_srvr.Disconnect(this);
				}
				else
				{
					_socket.BeginReceive(_pBuf, _pBufOffset, bufSize - _pBufOffset, SocketFlags.None, ReceiveCallback, this);
				}
			}
		}

		/// <summary>
		/// Called when the client has received data or the connection has been closed.
		/// </summary>
		/// <param name="ar">the async operation result object from the receive operation</param>
		private static void OnReceiveHandler(IAsyncResult ar)
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
		/// Closes the client connection.
		/// </summary>
		public void CloseConnections()
		{
			if (_socket != null)
			{
				try
				{
					_socket.Shutdown(SocketShutdown.Send);
				}
				catch
				{
				}

				try
				{
					_socket.Close();
				}
				catch
				{
				}
			}

			byte[] buff = _pBuf;
			if (buff != null)
			{
				_pBuf = null;
				_srvr.ReleasePacketBuffer(buff);
			}
		}

		/// <summary>
		/// Closes the client connection.
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