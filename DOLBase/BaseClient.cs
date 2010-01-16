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

namespace DOL
{
	/// <summary>
	/// Base class for connected clients
	/// </summary>
	public class BaseClient
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Holds the receive callback delegate
		/// </summary>
		protected static readonly AsyncCallback m_recvCallback = RecvCallback;

		/// <summary>
		/// Packet buffer, holds incoming packet data
		/// </summary>
		protected byte[] m_pbuf;

		/// <summary>
		/// Current offset into the buffer
		/// </summary>
		protected int m_pBufEnd;

		/// <summary>
		/// Socket that holds the client connection
		/// </summary>
		protected Socket m_sock;

		/// <summary>
		/// Pointer to the server the client is connected to
		/// </summary>
		protected BaseServer m_srvr;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="srvr">Pointer to the server the client is connected to</param>
		public BaseClient(BaseServer srvr)
		{
			m_srvr = srvr;

			if (srvr != null)
				m_pbuf = srvr.AcquirePacketBuffer();

			m_pBufEnd = 0;
		}

		/// <summary>
		/// Gets the server that the client is connected to
		/// </summary>
		public BaseServer Server
		{
			get { return m_srvr; }
		}

		/// <summary>
		/// Gets or sets the socket the client is using
		/// </summary>
		public Socket Socket
		{
			get { return m_sock; }
			set { m_sock = value; }
		}

		/// <summary>
		/// Gets the packet buffer for the client
		/// </summary>
		public byte[] PacketBuf
		{
			get { return m_pbuf; }
		}

		/// <summary>
		/// Gets or sets the offset into the receive buffer
		/// </summary>
		public int PacketBufSize
		{
			get { return m_pBufEnd; }
			set { m_pBufEnd = value; }
		}

		/// <summary>
		/// Gets the client's TCP endpoint address string, if connected
		/// </summary>
		public string TcpEndpointAddress
		{
			get
			{
				Socket s = m_sock;
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
				Socket s = m_sock;
				if (s != null && s.Connected && s.RemoteEndPoint != null)
					return s.RemoteEndPoint.ToString();

				return "not connected";
			}
		}

		/// <summary>
		/// Called when data has been received from the connection
		/// </summary>
		/// <param name="numBytes">Number of bytes received in m_pbuf</param>
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
			if (m_sock != null && m_sock.Connected)
			{
				int bufSize = m_pbuf.Length;

				if (m_pBufEnd >= bufSize) //Do we have space to receive?
				{
					if (log.IsErrorEnabled)
					{
						log.Error(TcpEndpoint + " disconnected because of buffer overflow!");
						log.Error("m_pBufEnd=" + m_pBufEnd + "; buf size=" + bufSize);
						log.Error(m_pbuf);
					}

					m_srvr.Disconnect(this);
				}
				else
				{
					m_sock.BeginReceive(m_pbuf, m_pBufEnd, bufSize - m_pBufEnd, SocketFlags.None, m_recvCallback, this);
				}
			}
		}

		/// <summary>
		/// Called when a client has received data or the connection has been closed
		/// </summary>
		/// <param name="ar">Results of the receive operation</param>
		protected static void RecvCallback(IAsyncResult ar)
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
					if (log.IsDebugEnabled)
						log.Debug("Disconnecting client (" + baseClient.TcpEndpoint + "), received bytes=" + numBytes);

					baseClient.m_srvr.Disconnect(baseClient);
				}
			}
			catch (ObjectDisposedException)
			{
				if (baseClient != null)
					baseClient.m_srvr.Disconnect(baseClient);
			}
			catch (SocketException e)
			{
				if (baseClient != null)
				{
					if (log.IsInfoEnabled)
						log.Info(string.Format("{0}  {1}", baseClient.TcpEndpoint, e.Message));

					baseClient.m_srvr.Disconnect(baseClient);
				}
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("RecvCallback", e);

				if (baseClient != null)
					baseClient.m_srvr.Disconnect(baseClient);
			}
		}

		/// <summary>
		/// Closes the client connection
		/// </summary>
		public void CloseConnections()
		{
			if (m_sock != null)
			{
				try
				{
					m_sock.Shutdown(SocketShutdown.Send);
				}
				catch
				{
				}
				try
				{
					m_sock.Close();
				}
				catch
				{
				}
			}

			byte[] buff = m_pbuf;
			if (buff != null)
			{
				m_pbuf = null;
				m_srvr.ReleasePacketBuffer(buff);
			}
		}

		/// <summary>
		/// Closes the client connection
		/// </summary>
		public void Disconnect()
		{
			try
			{
				m_srvr.Disconnect(this);
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("Exception", e);
			}
		}
	}
}
