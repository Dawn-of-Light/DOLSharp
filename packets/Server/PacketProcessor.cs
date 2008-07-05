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
#define LOGACTIVESTACKS

using System;
using System.Collections;
using System.Diagnostics;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Timers;
using DOL.Events;
using log4net;
using Timer = System.Timers.Timer;

namespace DOL.GS.PacketHandler
{
	/// <summary>
	/// This class handles the packets, receiving and sending
	/// </summary>
	public class PacketProcessor
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Holds the current client for this processor
		/// </summary>
		protected readonly GameClient m_client;

		/// <summary>
		/// Holds the encoding used to encrypt/decrypt the packets
		/// </summary>
		protected readonly IPacketEncoding m_encoding;

		/// <summary>
		/// Stores all packet handlers found when searching the gameserver assembly
		/// </summary>
		protected static readonly IPacketHandler[] m_packetHandlers = new IPacketHandler[256];

		/// <summary>
		/// currently active packet handler
		/// </summary>
		protected IPacketHandler m_activePacketHandler;

		/// <summary>
		/// thread id of running packet handler
		/// </summary>
		protected int m_handlerThreadID = 0;

		/// <summary>
		/// Constructs a new PacketProcessor
		/// </summary>
		/// <param name="client">The processor client</param>
		public PacketProcessor(GameClient client)
		{
			if (client == null)
				throw new ArgumentNullException("client");
			m_client = client;
			m_udpCounter = 0;
			//TODO set encoding based on client version in the future :)
			m_encoding = new PacketEncoding168();
			m_asyncUdpCallback = new AsyncCallback(AsyncUdpSendCallback);
			m_tcpSendBuffer = client.Server.AcquirePacketBuffer();
			m_udpSendBuffer = client.Server.AcquirePacketBuffer();
		}

		#region save last couple packets

		/// <summary>
		/// The count of last sent/received packets to keep.
		/// </summary>
		protected const int MAX_LAST_PACKETS = 16;

		/// <summary>
		/// Holds the last sent/received packets.
		/// </summary>
		protected readonly Queue m_lastPackets = new Queue(MAX_LAST_PACKETS);

//		/// <summary>
//		/// Holds the packets that were received last
//		/// </summary>
//		protected readonly Queue m_receivedPackets = new Queue(MAX_LAST_PACKETS);
//
//		/// <summary>
//		/// Holds the packets that were sent last
//		/// </summary>
//		protected readonly Queue m_sentPackets = new Queue(MAX_LAST_PACKETS);

		/// <summary>
		/// Saves the sent packet for debugging
		/// </summary>
		/// <param name="pak">The sent packet</param>
		protected void SaveSentPacket(PacketOut pak)
		{
			lock (m_lastPackets.SyncRoot)
			{
				while (m_lastPackets.Count >= MAX_LAST_PACKETS)
					m_lastPackets.Dequeue();
				m_lastPackets.Enqueue(pak);
			}
//			lock (m_sentPackets.SyncRoot)
//			{
//				while (m_sentPackets.Count >= MAX_LAST_PACKETS)
//					m_sentPackets.Dequeue();
//				m_sentPackets.Enqueue(pak);
//			}
		}

		/// <summary>
		/// Save the received packet for debugging
		/// </summary>
		/// <param name="pak">The received packet</param>
		protected void SaveReceivedPacket(GSPacketIn pak)
		{
			lock (m_lastPackets.SyncRoot)
			{
				while (m_lastPackets.Count >= MAX_LAST_PACKETS)
					m_lastPackets.Dequeue();
				m_lastPackets.Enqueue(pak);
			}
//			lock (m_receivedPackets.SyncRoot)
//			{
//				while (m_receivedPackets.Count >= MAX_LAST_PACKETS)
//					m_receivedPackets.Dequeue();
//				m_receivedPackets.Enqueue(pak);
//			}
		}

		/// <summary>
		/// Makes a copy of last sent/received packets.
		/// </summary>
		/// <returns></returns>
		public object[] GetLastPackets()
		{
			lock (m_lastPackets.SyncRoot)
			{
				object[] res = new object[m_lastPackets.Count];
				m_lastPackets.CopyTo(res, 0);
				return res;
			}
		}

//		/// <summary>
//		/// Makes a copy of last sent packets. Most recent packet has highest index.
//		/// </summary>
//		/// <returns>The copy of last sent packets</returns>
//		public PacketOut[] GetSentPackets()
//		{
//			lock (m_sentPackets.SyncRoot)
//			{
//				PacketOut[] sent = new PacketOut[m_sentPackets.Count];
//				m_sentPackets.CopyTo(sent, 0);
//				return sent;
//			}
//		}
//
//		/// <summary>
//		/// Makes a copy of last received packets. Most recent packet has highest index.
//		/// </summary>
//		/// <returns>The copy of last received packets</returns>
//		public GSPacketIn[] GetReceivedPackets()
//		{
//			lock (m_receivedPackets.SyncRoot)
//			{
//				GSPacketIn[] received = new GSPacketIn[m_receivedPackets.Count];
//				m_receivedPackets.CopyTo(received, 0);
//				return received;
//			}
//		}

		#endregion

		/// <summary>
		/// Gets the encoding for this processor
		/// </summary>
		public IPacketEncoding Encoding
		{
			get { return m_encoding; }
		}

		/// <summary>
		/// Callback function called when the scripts assembly has been compiled
		/// </summary>
		[ScriptLoadedEvent]
		public static void OnScriptCompiled(DOLEvent ev, object sender, EventArgs args)
		{
			Array.Clear(m_packetHandlers, 0, m_packetHandlers.Length);
			int count = SearchPacketHandlers("v168", Assembly.GetAssembly(typeof (GameServer)));
			if (log.IsInfoEnabled)
				log.Info("PacketProcessor: Loaded " + count + " handlers from GameServer Assembly!");

			count = 0;
			foreach (Assembly asm in ScriptMgr.Scripts)
			{
				count += SearchPacketHandlers("v168", asm);
			}
			if (log.IsInfoEnabled)
				log.Info("PacketProcessor: Loaded " + count + " handlers from Script Assemblys!");
		}

		/// <summary>
		/// Registers a packet handler
		/// </summary>
		/// <param name="handler">The packet handler to register</param>
		/// <param name="packetCode">The packet ID to register it with</param>
		public static void RegisterPacketHandler(int packetCode, IPacketHandler handler)
		{
			m_packetHandlers[packetCode] = handler;
		}

		/// <summary>
		/// Searches an assembly for packet handlers
		/// </summary>
		/// <param name="version">namespace of packethandlers to search eg. 'v167'</param>
		/// <param name="assembly">Assembly to search</param>
		/// <returns>The number of handlers loaded</returns>
		protected static int SearchPacketHandlers(string version, Assembly assembly)
		{
			int count = 0;

			// Walk through each type in the assembly
			foreach (Type type in assembly.GetTypes())
			{
				// Pick up a class
				if (type.IsClass != true)
					continue;
				if (type.GetInterface("DOL.GS.PacketHandler.IPacketHandler") == null)
					continue;
				if (!type.Namespace.ToLower().EndsWith(version.ToLower()))
					continue;
				PacketHandlerAttribute[] packethandlerattribs = (PacketHandlerAttribute[]) type.GetCustomAttributes(typeof (PacketHandlerAttribute), true);
				if (packethandlerattribs.Length > 0)
				{
					count++;
					RegisterPacketHandler(packethandlerattribs[0].Code, (IPacketHandler) Activator.CreateInstance(type));
				}
			}
			return count;
		}

		/// <summary>
		/// Called on client disconnect.
		/// </summary>
		public virtual void OnDisconnect()
		{
			byte[] tcp = m_tcpSendBuffer;
			byte[] udp = m_udpSendBuffer;
			m_tcpSendBuffer = m_udpSendBuffer = null;
			m_client.Server.ReleasePacketBuffer(tcp);
			m_client.Server.ReleasePacketBuffer(udp);
		}

		#region TCP

		/// <summary>
		/// Holds the TCP send buffer
		/// </summary>
		protected byte[] m_tcpSendBuffer;

		/// <summary>
		/// The client TCP packet send queue
		/// </summary>
		protected readonly Queue m_tcpQueue = new Queue(256);

		/// <summary>
		/// Indicates whether data is currently being sent to the client
		/// </summary>
		protected bool m_sendingTcp;

		/// <summary>
		/// Sends a packet via TCP
		/// </summary>
		/// <param name="packet">The packet to be sent</param>
		public void SendTCP(GSTCPPacketOut packet)
		{
			//Fix the packet size
			packet.WritePacketLength();
			SaveSentPacket(packet);
			//Get the packet buffer
			byte[] buf = packet.GetBuffer(); //packet.WritePacketLength sets the Capacity
			//Send the buffer
			SendTCP(buf);
		}

		/// <summary>
		/// Sends a packet via TCP
		/// </summary>
		/// <param name="buf">Buffer containing the data to be sent</param>
		public void SendTCP(byte[] buf)
		{
			if (m_tcpSendBuffer == null)
				return;

			//Check if client is connected
			if (m_client.Socket.Connected)
			{
				if (log.IsDebugEnabled)
					log.Debug(Marshal.ToHexDump(
						string.Format("<=== <{2}> Packet 0x{0:X2} (0x{1:X2}) length: {3}", buf[2], buf[2] ^ 168, (m_client.Account != null) ? m_client.Account.Name : m_client.TcpEndpoint, buf.Length),
						buf));

				if (buf.Length > 2048)
				{
					if (ServerProperties.Properties.IGNORE_TOO_LONG_OUTCOMING_PACKET)
						return;
					if (log.IsErrorEnabled)
					{
						string desc = String.Format("Sending packets longer than 2048 cause client to crash, check log for stacktrace. Packet code: 0x{0:X2}, account: {1}, packet size: {2}.",
							buf[2], (m_client.Account != null) ? m_client.Account.Name : m_client.TcpEndpoint, buf.Length);
						log.Error(Marshal.ToHexDump(desc, buf) + "\n" + Environment.StackTrace);
						GameServer.Instance.Disconnect(m_client);
						return;
					}
				}

				buf = m_encoding.EncryptPacket(buf, false);

				try
				{
					Statistics.BytesOut += buf.Length;
					Statistics.PacketsOut++;
					lock (m_tcpQueue.SyncRoot)
					{
						if (m_sendingTcp)
						{
							m_tcpQueue.Enqueue(buf);
							return;
						}
						else
						{
							m_sendingTcp = true;
						}
					}

					Buffer.BlockCopy(buf, 0, m_tcpSendBuffer, 0, buf.Length);

					int start = Environment.TickCount;

					m_client.Socket.BeginSend(m_tcpSendBuffer, 0, buf.Length, SocketFlags.None, m_asyncTcpCallback, m_client);

					int took = Environment.TickCount - start;
					if (took > 100 && log.IsWarnEnabled)
						log.WarnFormat("SendTCP.BeginSend took {0}ms! (TCP to client: {1})", took, m_client);
				}
				catch (Exception e)
				{
					// assure that no exception is thrown into the upper layers and interrupt game loops!
					if (log.IsWarnEnabled)
						log.Warn("It seems <" + ((m_client.Account != null) ? m_client.Account.Name : "???") + "> went linkdead. Closing connection. (SendTCP, " + e.GetType() + ": " + e.Message + ")");
					//DOLConsole.WriteWarning(e.ToString());
					GameServer.Instance.Disconnect(m_client);
				}
			}
		}

		/// <summary>
		/// Holds the TCP AsyncCallback delegate
		/// </summary>
		protected static readonly AsyncCallback m_asyncTcpCallback = new AsyncCallback(AsyncTcpSendCallback);

		/// <summary>
		/// Callback method for async sends
		/// </summary>
		/// <param name="ar"></param>
		protected static void AsyncTcpSendCallback(IAsyncResult ar)
		{
			if (ar == null)
			{
				if (log.IsErrorEnabled)
					log.Error("AsyncSendCallback: ar == null");
				return;
			}

			GameClient client = (GameClient) ar.AsyncState;

			try
			{
				PacketProcessor pakProc = client.PacketProcessor;
				Queue q = pakProc.m_tcpQueue;

				int sent = client.Socket.EndSend(ar);

				int count = 0;
				byte[] data = pakProc.m_tcpSendBuffer;

				if (data == null)
					return;

				lock (q.SyncRoot)
				{
					if (q.Count > 0)
					{
//						log.WarnFormat("async sent {0} bytes, sending queued packets count: {1}", sent, q.Count);
						count = CombinePackets(data, q, data.Length, client);
					}
					if (count <= 0)
					{
//						log.WarnFormat("async sent {0} bytes", sent);
						pakProc.m_sendingTcp = false;
						return;
					}
				}

				int start = Environment.TickCount;

				client.Socket.BeginSend(data, 0, count, SocketFlags.None, m_asyncTcpCallback, client);

				int took = Environment.TickCount - start;
				if (took > 100 && log.IsWarnEnabled)
					log.WarnFormat("AsyncTcpSendCallback.BeginSend took {0}ms! (TCP to client: {1})", took, client.ToString());
			}
			catch (ObjectDisposedException)
			{
				GameServer.Instance.Disconnect(client);
			}
			catch (SocketException)
			{
				GameServer.Instance.Disconnect(client);
			}
			catch (Exception e)
			{
				// assure that no exception is thrown into the upper layers and interrupt game loops!
				if (log.IsErrorEnabled)
					log.Error("AsyncSendCallback. client: " + client.ToString(), e);
				GameServer.Instance.Disconnect(client);
			}
		}

		/// <summary>
		/// Combines queued packets in one stream.
		/// </summary>
		/// <param name="buf">The target buffer.</param>
		/// <param name="q">The queued packets.</param>
		/// <param name="length">The max stream len.</param>
		/// <param name="client">The client.</param>
		/// <returns>The count of bytes writen.</returns>
		private static int CombinePackets(byte[] buf, Queue q, int length, GameClient client)
		{
			int i = 0;
			do
			{
				byte[] pak = (byte[]) q.Peek();
				if (i + pak.Length > buf.Length)
				{
					if (i == 0)
					{
						log.WarnFormat("packet size {0} > buf size {1}, ignored; client: {2}\n{3}", pak.Length, buf.Length, client, Marshal.ToHexDump("packet data:", pak));
						q.Dequeue();
						continue;
					}
					break;
				}
				Buffer.BlockCopy(pak, 0, buf, i, pak.Length);
				i += pak.Length;
				q.Dequeue();
			} while (q.Count > 0);

			return i;
		}

		/// <summary>
		/// Send the packet via TCP without changing any portion of the packet
		/// </summary>
		/// <param name="packet">Packet to send</param>
		public void SendTCPRaw(GSTCPPacketOut packet)
		{
			SendTCP((byte[]) packet.GetBuffer().Clone());
		}

		#endregion

		#region UDP

		/// <summary>
		/// Holds the UDP send buffer
		/// </summary>
		protected byte[] m_udpSendBuffer;

		/// <summary>
		/// The client UDP packet send queue
		/// </summary>
		protected readonly Queue m_udpQueue = new Queue(256);

		/// <summary>
		/// This variable holds the current UDP counter for this sender
		/// </summary>
		protected volatile ushort m_udpCounter;

		/// <summary>
		/// Holds the async udp send callback delegate
		/// </summary>
		private readonly AsyncCallback m_asyncUdpCallback;

		/// <summary>
		/// Indicates whether UDP data is currently being sent
		/// </summary>
		private bool m_sendingUdp;

		/// <summary>
		/// Send the packet via UDP
		/// </summary>
		/// <param name="packet">Packet to be sent</param>
		/// <param name="isForced">Force UDP packet if <code>true</code>, else packet can be sent over TCP</param>
		public virtual void SendUDP(GSUDPPacketOut packet, bool isForced)
		{
			//Fix the packet size
			packet.WritePacketLength();
			SaveSentPacket(packet);
			SendUDP(packet.GetBuffer(), isForced);
		}

		/// <summary>
		/// Send the packet via UDP
		/// </summary>
		/// <param name="buf">Packet to be sent</param>
		/// <param name="isForced">Force UDP packet if <code>true</code>, else packet can be sent over TCP</param>
		public void SendUDP(byte[] buf, bool isForced)
		{
//			log.FatalFormat("Send UDP: {0}", isForced);

			//No udp available, send via TCP instead!
			//bool flagLostUDP = false;
			if (m_client.UDPEndPoint == null || !(isForced || m_client.UDPConfirm))
			{
//				log.FatalFormat("UDP sent over TCP");
				//DOLConsole.WriteWarning("Trying to send UDP when UDP isn't initialized!");
				byte[] newbuf = new byte[buf.Length - 2];
				newbuf[0] = buf[0];
				newbuf[1] = buf[1];
				Array.Copy(buf, 4, newbuf, 2, buf.Length - 4);
				SendTCP(newbuf);
				return;
			}
			else if(m_client.ClientState == GameClient.eClientState.Playing)
			{
				if ((DateTime.Now.Ticks - m_client.UDPPingTime) > 500000000L) // really 24s, not 50s
				{
					//flagLostUDP = true;
					m_client.UDPConfirm = false;
				}
			}
			if (m_udpSendBuffer == null)
				return;

			//increase our UDP counter when it reaches 0xFFFF
			//and increases, it will automaticallys switch back to 0x00
			m_udpCounter++;

			//fill the udpCounter
			buf[2] = (byte) (m_udpCounter >> 8);
			buf[3] = (byte) m_udpCounter;
			buf = m_encoding.EncryptPacket(buf, true);

			Statistics.BytesOut += buf.Length;
			Statistics.PacketsOut++;

			lock (m_udpQueue.SyncRoot)
			{
				if (m_sendingUdp)
				{
					m_udpQueue.Enqueue(buf);
					return;
				}
				else
				{
					m_sendingUdp = true;
				}
			}

			Buffer.BlockCopy(buf, 0, m_udpSendBuffer, 0, buf.Length);

			try
			{
				GameServer.Instance.SendUDP(m_udpSendBuffer, buf.Length, m_client.UDPEndPoint, m_asyncUdpCallback);
			}
			catch (Exception e)
			{
				int count;
				lock (m_udpQueue.SyncRoot)
				{
					count = m_udpQueue.Count;
					m_udpQueue.Clear();
					m_sendingUdp = false;
				}
				if (log.IsErrorEnabled)
					log.ErrorFormat("trying to send UDP (" + count + ")", e);
			}
		}

		/// <summary>
		/// Finishes an asynchronous UDP transaction
		/// </summary>
		/// <param name="ar"></param>
		private void AsyncUdpSendCallback(IAsyncResult ar)
		{
			try
			{
				Socket s = (Socket) ar.AsyncState;
				int sent = s.EndSendTo(ar);

				int count = 0;
				byte[] data = m_udpSendBuffer;

				if (data == null)
					return;

				lock (m_udpQueue.SyncRoot)
				{
					if (m_udpQueue.Count > 0)
					{
//						log.WarnFormat("async UDP sent {0} bytes, sending queued packets count: {1}", sent, m_udpQueue.Count);
						count = CombinePackets(data, m_udpQueue, data.Length, m_client);
					}
					if (count <= 0)
					{
//						log.WarnFormat("async UDP sent {0} bytes", sent);
						m_sendingUdp = false;
						return;
					}
				}

				int start = Environment.TickCount;

				GameServer.Instance.SendUDP(data, count, m_client.UDPEndPoint, m_asyncUdpCallback);

				int took = Environment.TickCount - start;
				if (took > 100 && log.IsWarnEnabled)
					log.WarnFormat("AsyncUdpSendCallback.BeginSend took {0}ms! (TCP to client: {1})", took, m_client.ToString());

			}
			catch (Exception e)
			{
				int count;
				lock (m_udpQueue.SyncRoot)
				{
					count = m_udpQueue.Count;
					m_udpQueue.Clear();
					m_sendingUdp = false;
				}
				if (log.IsErrorEnabled)
					log.Error("AsyncUdpSendCallback (" + count + ")", e);
			}
		}

		/// <summary>
		/// Send the UDP packet without changing any portion of the packet
		/// </summary>
		/// <param name="packet">Packet to be sent</param>
		public void SendUDPRaw(GSUDPPacketOut packet)
		{
			SendUDP((byte[]) packet.GetBuffer().Clone(), false);
		}

		#endregion

		/// <summary>
		/// Called when the client receives bytes
		/// </summary>
		/// <param name="numBytes">The number of bytes received</param>
		public void ReceiveBytes(int numBytes)
		{
			lock (this)
			{
				byte[] buffer = m_client.PacketBuf;
				//End Offset of buffer
				int bufferSize = m_client.PacketBufSize + numBytes;
				//Size < minimum
				if (bufferSize < GSPacketIn.HDR_SIZE)
				{
					m_client.PacketBufSize = bufferSize; // undo buffer read
					return;
				}
				//Reset the offset
				m_client.PacketBufSize = 0;
				//Current offset into the buffer
				int curOffset = 0;

				do
				{
					int packetLength = (buffer[curOffset] << 8) + buffer[curOffset + 1] + GSPacketIn.HDR_SIZE;
					int dataLeft = bufferSize - curOffset;

					if (dataLeft < packetLength)
					{
						Array.Copy(buffer, curOffset, buffer, 0, dataLeft);
						m_client.PacketBufSize = dataLeft;
						break;
					}

					byte[] curPacket = new byte[packetLength];
					Array.Copy(buffer, curOffset, curPacket, 0, packetLength);

					//DOLConsole.Log("Encrypted Packet\n");
					//DOLConsole.LogDump(curPacket);

					//Decrypting can change the packet size!!!
					//DOLConsole.Log("Encrypted Packet:\n");
					//DOLConsole.LogDump(curPacket);
					curPacket = m_encoding.DecryptPacket(curPacket, false);
					//DOLConsole.Log("Decrypted Packet:\n");
					//DOLConsole.LogDump(curPacket);

					int calcCheck = CalculateChecksum(curPacket, 0, curPacket.Length - 2);
					int pakCheck = (curPacket[curPacket.Length - 2] << 8) | (curPacket[curPacket.Length - 1]);
					if (pakCheck != calcCheck)
					{
						if (log.IsWarnEnabled)
							log.WarnFormat("Bad TCP packet checksum (packet:0x{0:X4} calculated:0x{1:X4}) -> disconnecting\nclient: {2}\ncurOffset={3}; packetLength={4}",
							               pakCheck, calcCheck, m_client.ToString(), curOffset, packetLength);

						if (log.IsInfoEnabled)
						{
							log.Info("Last client sent/received packets (from older to newer):");
							foreach (object obj in GetLastPackets())
							{
								PacketOut outPak = obj as PacketOut;
								GSPacketIn inPak = obj as GSPacketIn;
								if (outPak != null)
								{
									log.Info(Marshal.ToHexDump(outPak.GetType().FullName + ":", outPak.GetBuffer()));
								}
								else if (inPak != null)
								{
									inPak.LogDump();
								}
								else
								{
									log.Info("Unknown packet type: " + obj.GetType().FullName);
								}
							}
						}

//						if (log.IsInfoEnabled)
//						{
//							log.InfoFormat("\n{0}\n{1}", Marshal.ToHexDump("packet buffer:", curPacket), Marshal.ToHexDump("client buffer:", buffer));
//							log.Info("Last Client => Server packets (from older to newer):");
//							foreach (GSPacketIn p in GetReceivedPackets())
//							{
//								log.Info(Marshal.ToHexDump(p.ToString() + ":", p.ToArray()));
//							}
//						}
						m_client.Disconnect();
						return;
					}

					GSPacketIn pak = new GSPacketIn(curPacket.Length - GSPacketIn.HDR_SIZE);
					pak.Load(curPacket, curPacket.Length);
					//DOLConsole.Log("Decrypted Packet\n");
					//pak.LogDump();

					try
					{
						HandlePacket(pak);
					}
					catch (Exception e)
					{
						if (log.IsErrorEnabled)
							log.Error("HandlePacket(pak)", e);
					}

					curOffset += packetLength;
				} while (bufferSize - 1 > curOffset);

				if (bufferSize - 1 == curOffset)
				{
					buffer[0] = buffer[curOffset];
					m_client.PacketBufSize = 1;
				}
			}
		}

		/// <summary>
		/// Calculates the packet checksum
		/// </summary>
		/// <param name="packet">The full received packet including checksum bytes</param>
		/// <param name="dataOffset">The offset of data for checksum calc in the buffer</param>
		/// <param name="dataSize">The size of data for checksum calc</param>
		/// <returns>The calculated checksum</returns>
		public static ushort CalculateChecksum(byte[] packet, int dataOffset, int dataSize)
		{
			byte[] pak = packet;
			byte val1 = 0x7E;
			byte val2 = 0x7E;
			int i = dataOffset;
			int len = i+dataSize;

			while (i < len)
			{
				val1 += pak[i++];
				val2 += val1;
			}

			return (ushort)(val2 - ((val1 + val2) << 8));
		}

		public void HandlePacketTimeout(object sender, ElapsedEventArgs e)
		{
			string source = ((m_client.Account != null) ? m_client.Account.Name : m_client.TcpEndpoint);
			if (log.IsErrorEnabled)
				log.Error("Thread " + m_handlerThreadID + " - Handler " + m_activePacketHandler.GetType().ToString() + " takes too much time (>10000ms) <" + source + "> " + "!");
		}

#if LOGACTIVESTACKS
		/// <summary>
		/// Holds a list of all currently active handler threads!
		/// This list is updated in the HandlePacket method
		/// </summary>
		public static Hashtable m_activePacketThreads = Hashtable.Synchronized(new Hashtable());
#endif
		/// <summary>
		/// Retrieves a textual description of all active packet handler thread stacks
		/// </summary>
		/// <returns>A string with the stacks</returns>
		public static string GetConnectionThreadpoolStacks()
		{
#if LOGACTIVESTACKS
			StringBuilder builder = new StringBuilder();
			//When enumerating over a synchronized hashtable, we need to
			//lock it's syncroot! Only for reading, not for writing locking
			//is needed!
			lock(m_activePacketThreads.SyncRoot)
			{
				foreach(DictionaryEntry entry in m_activePacketThreads)
				{
					try
					{
						Thread thread = (Thread) entry.Key;
						GameClient client = (GameClient) entry.Value;
						//Suspend the thread
						thread.Suspend();
						//Get the trace
						StackTrace trace = new StackTrace(thread, true);
						//Resume the thread
						thread.Resume();

						builder.Append("Stack for thread from account: ");
						if(client!=null && client.Account!=null)
						{
							builder.Append(client.Account.Name);
							if(client.Player!=null)
							{
								builder.Append(" (");
								builder.Append(client.Player.Name);
								builder.Append(")");
							}
						}
						else
						{
							builder.Append("null");
						}
						builder.Append("\n");
						builder.Append(Util.FormatStackTrace(trace));
						builder.Append("\n\n");
					}
					catch(Exception e)
					{
						builder.Append("Error getting stack for thread: ");
						builder.Append("\n");
						builder.Append(e);
						builder.Append("\n\n");
					}
				}
			}
			return builder.ToString();
#else
			return "LOGACTIVESTACKS is not defined in PacketProcessor";
#endif
		}



		public void HandlePacket(GSPacketIn packet)
		{
			if (packet == null || m_client == null)
				return;

			int code = packet.ID;

			Statistics.BytesIn += packet.PacketSize;
			Statistics.PacketsIn++;
			SaveReceivedPacket(packet);

			IPacketHandler packetHandler = null;
			if (code < m_packetHandlers.Length)
				packetHandler = m_packetHandlers[code];
			else if (log.IsErrorEnabled)
			{
				log.ErrorFormat("Received packet code is outside of m_packetHandlers array bounds! "+m_client.ToString());
				log.Error(Marshal.ToHexDump(
					String.Format("===> <{2}> Packet 0x{0:X2} (0x{1:X2}) length: {3} (ThreadId={4})", code, code ^ 168, (m_client.Account != null) ? m_client.Account.Name : m_client.TcpEndpoint, packet.PacketSize, Thread.CurrentThread.ManagedThreadId),
					packet.ToArray()));
			}

			//TODO remove debug output
			//if (packet.ID != (0x12^168) && packet.ID != (0x0b^168) && packet.ID != (0x01^168)) {
			if (log.IsDebugEnabled)
				log.Debug(Marshal.ToHexDump(
					String.Format("===> <{2}> Packet 0x{0:X2} (0x{1:X2}) length: {3} handled by {4} (ThreadId={5})", code, code ^ 168, (m_client.Account != null) ? m_client.Account.Name : m_client.TcpEndpoint, packet.PacketSize, packetHandler, Thread.CurrentThread.ManagedThreadId),
					packet.ToArray()));

			if (packetHandler != null)
			{
				Timer monitorTimer = null;
				if(log.IsDebugEnabled)
				{
					try
					{
						monitorTimer = new Timer(10000);
						m_activePacketHandler = packetHandler;
						m_handlerThreadID = Thread.CurrentThread.ManagedThreadId;
						monitorTimer.Elapsed += new ElapsedEventHandler(HandlePacketTimeout);
						monitorTimer.Start();
					}
					catch (Exception e)
					{
						if (log.IsErrorEnabled)
							log.Error("Starting packet monitor timer", e);

						if (monitorTimer != null)
						{
							monitorTimer.Stop();
							monitorTimer.Close();
							monitorTimer = null;
						}
					}
				}

#if LOGACTIVESTACKS
				//Put the current thread into the active thread list!
				//No need to lock the hashtable since we created it
				//synchronized! One reader, multiple writers supported!
				m_activePacketThreads.Add(Thread.CurrentThread, m_client);
#endif
				long start = Environment.TickCount;
				try
				{
					packetHandler.HandlePacket(m_client, packet);
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
					{
						string client = (m_client == null ? "null" : m_client.ToString());
						log.Error("Error while processing packet (handler="+packetHandler.GetType().FullName+"  client: " + client + ")", e);
					}
				}
#if LOGACTIVESTACKS
				finally
				{
					//Remove the thread from the active list after execution
					//No need to lock the hashtable since we created it
					//synchronized! One reader, multiple writers supported!
					m_activePacketThreads.Remove(Thread.CurrentThread);
				}
#endif
				long timeUsed = Environment.TickCount - start;
				if (monitorTimer != null) {
					monitorTimer.Stop();
					monitorTimer.Close();
				}
				m_activePacketHandler = null;
				if (timeUsed > 1000)
				{
					string source = ((m_client.Account != null) ? m_client.Account.Name : m_client.TcpEndpoint);
					if (log.IsWarnEnabled)
						log.Warn("(" + source + ") Handle packet Thread " + Thread.CurrentThread.ManagedThreadId + " " + packetHandler + " took " + timeUsed + "ms!");
				}
			}
		}
	}
}
