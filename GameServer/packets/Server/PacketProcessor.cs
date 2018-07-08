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
using DOL.GS.ServerProperties;
using DOL.Network;
using log4net;
using Timer = System.Timers.Timer;
using System.Collections.Generic;

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
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Sync Lock Object
        /// </summary>
        private readonly object _syncLock = new object();

        /// <summary>
        /// Holds the current client for this processor
        /// </summary>
        private readonly GameClient _client;

        /// <summary>
        /// Stores all packet handlers found when searching the gameserver assembly
        /// </summary>
        private IPacketHandler[] _packetHandlers = new IPacketHandler[256];

        /// <summary>
        /// currently active packet handler
        /// </summary>
        private IPacketHandler _activePacketHandler;

        /// <summary>
        /// thread id of running packet handler
        /// </summary>
        private int _handlerThreadId;

        /// <summary>
        /// packet preprocessor that performs initial packet checks for this Packet Processor.
        /// </summary>
        private readonly PacketPreprocessing _packetPreprocessor;

        /// <summary>
        /// Constructs a new PacketProcessor
        /// </summary>
        /// <param name="client">The processor client</param>
        public PacketProcessor(GameClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _packetPreprocessor = new PacketPreprocessing();

            LoadPacketHandlers(client);

            _udpCounter = 0;

            // TODO set encoding based on client version in the future :)
            Encoding = new PacketEncoding168();
            _asyncUdpCallback = AsyncUdpSendCallback;
            _tcpSendBuffer = client.Server.AcquirePacketBuffer();
            _udpSendBuffer = client.Server.AcquirePacketBuffer();
        }

        /// <summary>
        /// The count of last sent/received packets to keep.
        /// </summary>
        private const int MaxLastPackets = 16;

        /// <summary>
        /// Holds the last sent/received packets.
        /// </summary>
        private readonly Queue<IPacket> _lastPackets = new Queue<IPacket>(MaxLastPackets);

        /// <summary>
        /// Saves the sent packet for debugging
        /// </summary>
        /// <param name="pak">The sent packet</param>
        private void SavePacket(IPacket pak)
        {
            lock (((ICollection)_lastPackets).SyncRoot)
            {
                while (_lastPackets.Count >= MaxLastPackets)
                {
                    _lastPackets.Dequeue();
                }

                _lastPackets.Enqueue(pak);
            }
        }

        /// <summary>
        /// Makes a copy of last sent/received packets.
        /// </summary>
        /// <returns></returns>
        public IPacket[] GetLastPackets()
        {
            lock (((ICollection)_lastPackets).SyncRoot)
            {
                return _lastPackets.ToArray();
            }
        }

        /// <summary>
        /// Gets the encoding for this processor
        /// </summary>
        public IPacketEncoding Encoding { get; }

        /// <summary>
        /// Caches packet handlers loaded for a given client version (in string format, used for namespace search).
        /// </summary>
        private static readonly Dictionary<string, IPacketHandler[]> CachedPacketHandlerSearchResults = new Dictionary<string, IPacketHandler[]>();
        /// <summary>
        /// Stores packet handler attributes for each version, required to load preprocessors.
        /// </summary>
        private static readonly Dictionary<string, List<PacketHandlerAttribute>> CachedPreprocessorSearchResults = new Dictionary<string, List<PacketHandlerAttribute>>();
        private static readonly object PacketHandlerCacheLock = new object();

        public virtual void LoadPacketHandlers(GameClient client)
        {
            string baseVersion = "v168";

            // String may seem cumbersome but I would like to leave the open of custom clients open without core modification (for this reason I cannot use eClientVersion).
            // Also I am merely reusing some already written search functionality, which searches a namespace and thus expects a string.
            LoadPacketHandlers(baseVersion, out _packetHandlers, out var attributes);

            // todo: load different handlers for cumulative client versions, overwriting duplicate entries in m_PacketHandlers with later version.

            // Add preprocessors for each packet handler
            foreach (PacketHandlerAttribute pha in attributes)
            {
                _packetPreprocessor.RegisterPacketDefinition(pha.Code, pha.PreprocessorID);
            }
        }

        /// <summary>
        /// Loads packet handlers to be used for handling incoming data from this game client.
        /// </summary>
        /// <param name="client"></param>
        public virtual void LoadPacketHandlers(string version, out IPacketHandler[] packetHandlers, out List<PacketHandlerAttribute> attributes)
        {
            packetHandlers = new IPacketHandler[256];
            attributes = new List<PacketHandlerAttribute>();

            Array.Clear(packetHandlers, 0, packetHandlers.Length);
            lock (PacketHandlerCacheLock)
            {
                if (!CachedPacketHandlerSearchResults.ContainsKey(version))
                {
                    int count = SearchAndAddPacketHandlers(version, Assembly.GetAssembly(typeof(GameServer)), packetHandlers);
                    if (Log.IsInfoEnabled)
                    {
                        Log.Info($"PacketProcessor: Loaded {count} handlers from GameServer Assembly!");
                    }

                    count = 0;
                    foreach (Assembly asm in ScriptMgr.Scripts)
                    {
                        count += SearchAndAddPacketHandlers(version, asm, packetHandlers);
                    }

                    if (Log.IsInfoEnabled)
                    {
                        Log.Info($"PacketProcessor: Loaded {count} handlers from Script Assemblys!");
                    }

                    // save search result for next login
                    CachedPacketHandlerSearchResults.Add(version, (IPacketHandler[])packetHandlers.Clone());
                }
                else
                {
                    packetHandlers = (IPacketHandler[])CachedPacketHandlerSearchResults[version].Clone();
                    int count = 0;
                    foreach (IPacketHandler ph in packetHandlers)
                    {
                        if (ph != null)
                        {
                            count++;
                        }
                    }

                    Log.Info($"PacketProcessor: Loaded {count} handlers from cache for version={version}!");
                }

                if (CachedPreprocessorSearchResults.ContainsKey(version))
                {
                    attributes = CachedPreprocessorSearchResults[version];
                }

                Log.Info($"PacketProcessor: Loaded {attributes.Count} preprocessors from cache for version={version}!");
            }
        }

        /// <summary>
        /// Registers a packet handler
        /// </summary>
        /// <param name="handler">The packet handler to register</param>
        /// <param name="packetCode">The packet ID to register it with</param>
        public void RegisterPacketHandler(int packetCode, IPacketHandler handler, IPacketHandler[] packetHandlers)
        {
            if (packetHandlers[packetCode] != null)
            {
                Log.Info($"Overwriting Client Packet Code {packetCode}, with handler {handler.GetType().FullName} in PacketProcessor");
            }

            packetHandlers[packetCode] = handler;
        }

        /// <summary>
        /// Searches an assembly for packet handlers
        /// </summary>
        /// <param name="version">namespace of packethandlers to search eg. 'v167'</param>
        /// <param name="assembly">Assembly to search</param>
        /// <returns>The number of handlers loaded</returns>
        protected int SearchAndAddPacketHandlers(string version, Assembly assembly, IPacketHandler[] packetHandlers)
        {
            int count = 0;

            // Walk through each type in the assembly
            foreach (Type type in assembly.GetTypes())
            {
                // Pick up a class
                if (type.IsClass != true)
                {
                    continue;
                }

                if (type.GetInterface("DOL.GS.PacketHandler.IPacketHandler") == null)
                {
                    continue;
                }

                if (type.Namespace != null && !type.Namespace.ToLower().EndsWith(version.ToLower()))
                {
                    continue;
                }

                var packethandlerattribs = (PacketHandlerAttribute[])type.GetCustomAttributes(typeof(PacketHandlerAttribute), true);
                if (packethandlerattribs.Length > 0)
                {
                    count++;
                    RegisterPacketHandler(packethandlerattribs[0].Code, (IPacketHandler)Activator.CreateInstance(type), packetHandlers);

                    if (!CachedPreprocessorSearchResults.ContainsKey(version))
                    {
                        CachedPreprocessorSearchResults.Add(version, new List<PacketHandlerAttribute>());
                    }

                    CachedPreprocessorSearchResults[version].Add(packethandlerattribs[0]);
                }
            }

            return count;
        }

        /// <summary>
        /// Called on client disconnect.
        /// </summary>
        public virtual void OnDisconnect()
        {
            byte[] tcp = _tcpSendBuffer;
            byte[] udp = _udpSendBuffer;
            _tcpSendBuffer = _udpSendBuffer = null;
            _client.Server.ReleasePacketBuffer(tcp);
            _client.Server.ReleasePacketBuffer(udp);
        }

        /// <summary>
        /// Holds the TCP send buffer
        /// </summary>
        private byte[] _tcpSendBuffer;

        /// <summary>
        /// The client TCP packet send queue
        /// </summary>
        private readonly Queue<byte[]> _tcpQueue = new Queue<byte[]>(256);

        /// <summary>
        /// Indicates whether data is currently being sent to the client
        /// </summary>
        private bool _sendingTcp;

        /// <summary>
        /// Sends a packet via TCP
        /// </summary>
        /// <param name="packet">The packet to be sent</param>
        public void SendTCP(GSTCPPacketOut packet)
        {
            // Fix the packet size
            packet.WritePacketLength();

            SavePacket(packet);

            // Get the packet buffer
            byte[] buf = packet.GetBuffer(); // packet.WritePacketLength sets the Capacity

            // Send the buffer
            SendTCP(buf);
        }

        /// <summary>
        /// Sends a packet via TCP
        /// </summary>
        /// <param name="buf">Buffer containing the data to be sent</param>
        public void SendTCP(byte[] buf)
        {
            if (_tcpSendBuffer == null)
            {
                return;
            }

            // Check if client is connected
            if (_client.Socket.Connected)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug(Marshal.ToHexDump($"<=== <{(_client.Account != null ? _client.Account.Name : _client.TcpEndpoint)}> Packet 0x{buf[2]:X2} (0x{buf[2] ^ 168:X2}) length: {buf.Length}", buf));
                }

                if (buf.Length > 2048)
                {
                    if (Log.IsErrorEnabled)
                    {
                        string desc = $"Sending packets longer than 2048 cause client to crash, check Log for stacktrace. Packet code: 0x{buf[2]:X2}, account: {(_client.Account != null ? _client.Account.Name : _client.TcpEndpoint)}, packet size: {buf.Length}.";
                        Log.Error($"{Marshal.ToHexDump(desc, buf)}\n{Environment.StackTrace}");

                        if (Properties.IGNORE_TOO_LONG_OUTCOMING_PACKET)
                        {
                            Log.Error("ALERT: Oversize packet detected and discarded.");
                            _client.Out.SendMessage("ALERT: Error sending an update to your client. Oversize packet detected and discarded. Please /report this issue!", eChatType.CT_Staff, eChatLoc.CL_SystemWindow);
                        }
                        else
                        {
                            GameServer.Instance.Disconnect(_client);
                        }

                        return;
                    }
                }

                // buf = m_encoding.EncryptPacket(buf, false);
                try
                {
                    Statistics.BytesOut += buf.Length;
                    Statistics.PacketsOut++;

                    lock (((ICollection)_tcpQueue).SyncRoot)
                    {
                        if (_sendingTcp)
                        {
                            _tcpQueue.Enqueue(buf);
                            return;
                        }

                        _sendingTcp = true;
                    }

                    Buffer.BlockCopy(buf, 0, _tcpSendBuffer, 0, buf.Length);

                    int start = Environment.TickCount;

                    _client.Socket.BeginSend(_tcpSendBuffer, 0, buf.Length, SocketFlags.None, AsyncTcpCallback, _client);

                    int took = Environment.TickCount - start;
                    if (took > 100 && Log.IsWarnEnabled)
                    {
                        Log.Warn($"SendTCP.BeginSend took {took}ms! (TCP to client: {_client})");
                    }
                }
                catch (Exception e)
                {
                    // assure that no exception is thrown into the upper layers and interrupt game loops!
                    if (Log.IsWarnEnabled)
                    {
                        Log.Warn($"It seems <{(_client.Account != null ? _client.Account.Name : "???")}> went linkdead. Closing connection. (SendTCP, {e.GetType()}: {e.Message})");
                    }

                    // DOLConsole.WriteWarning(e.ToString());
                    GameServer.Instance.Disconnect(_client);
                }
            }
        }

        /// <summary>
        /// Holds the TCP AsyncCallback delegate
        /// </summary>
        private static readonly AsyncCallback AsyncTcpCallback = AsyncTcpSendCallback;

        /// <summary>
        /// Callback method for async sends
        /// </summary>
        /// <param name="ar"></param>
        protected static void AsyncTcpSendCallback(IAsyncResult ar)
        {
            if (ar == null)
            {
                if (Log.IsErrorEnabled)
                {
                    Log.Error("AsyncSendCallback: ar == null");
                }

                return;
            }

            var client = (GameClient)ar.AsyncState;

            try
            {
                PacketProcessor pakProc = client.PacketProcessor;
                Queue<byte[]> q = pakProc._tcpQueue;

                if (client.IsConnected)
                {
                    client.Socket.EndSend(ar);
                }

                int count = 0;
                byte[] data = pakProc._tcpSendBuffer;

                if (data == null)
                {
                    return;
                }

                lock (((ICollection)q).SyncRoot)
                {
                    if (q.Count > 0)
                    {
                        count = CombinePackets(data, q, client);
                    }

                    if (count <= 0)
                    {
                        pakProc._sendingTcp = false;
                        return;
                    }
                }

                int start = Environment.TickCount;

                client.Socket.BeginSend(data, 0, count, SocketFlags.None, AsyncTcpCallback, client);

                int took = Environment.TickCount - start;
                if (took > 100 && Log.IsWarnEnabled)
                {
                    Log.Warn($"AsyncTcpSendCallback.BeginSend took {took}ms! (TCP to client: {client})");
                }
            }
            catch (ObjectDisposedException ex)
            {
                Log.Error("Packet processor: ObjectDisposedException", ex);
                GameServer.Instance.Disconnect(client);
            }
            catch (SocketException ex)
            {
                Log.Error("Packet processor: SocketException", ex);
                GameServer.Instance.Disconnect(client);
            }
            catch (Exception ex)
            {
                // assure that no exception is thrown into the upper layers and interrupt game loops!
                if (Log.IsErrorEnabled)
                {
                    Log.Error($"AsyncSendCallback. client: {client}", ex);
                }

                GameServer.Instance.Disconnect(client);
            }
        }

        /// <summary>
        /// Combines queued packets in one stream.
        /// </summary>
        /// <param name="buf">The target buffer.</param>
        /// <param name="q">The queued packets.</param>
        /// <param name="client">The client.</param>
        /// <returns>The count of bytes writen.</returns>
        private static int CombinePackets(byte[] buf, Queue<byte[]> q, GameClient client)
        {
            int i = 0;
            do
            {
                var pak = q.Peek();
                if (i + pak.Length > buf.Length)
                {
                    if (i == 0)
                    {
                        Log.WarnFormat($"packet size {pak.Length} > buf size {buf.Length}, ignored; client: {client}\n{Marshal.ToHexDump("packet data:", pak)}");
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
            SendTCP((byte[])packet.GetBuffer().Clone());
        }

        /// <summary>
        /// Holds the UDP send buffer
        /// </summary>
        private byte[] _udpSendBuffer;

        /// <summary>
        /// The client UDP packet send queue
        /// </summary>
        private readonly Queue<byte[]> _udpQueue = new Queue<byte[]>(256);

        /// <summary>
        /// This variable holds the current UDP counter for this sender
        /// </summary>
        private volatile ushort _udpCounter;

        /// <summary>
        /// Holds the async udp send callback delegate
        /// </summary>
        private readonly AsyncCallback _asyncUdpCallback;

        /// <summary>
        /// Indicates whether UDP data is currently being sent
        /// </summary>
        private bool _sendingUdp;

        /// <summary>
        /// Send the packet via UDP
        /// </summary>
        /// <param name="packet">Packet to be sent</param>
        /// <param name="isForced">Force UDP packet if <code>true</code>, else packet can be sent over TCP</param>
        public virtual void SendUDP(GSUDPPacketOut packet, bool isForced)
        {
            // Fix the packet size
            packet.WritePacketLength();

            SavePacket(packet);

            SendUDP(packet.GetBuffer(), isForced);
        }

        /// <summary>
        /// Send the packet via UDP
        /// </summary>
        /// <param name="buf">Packet to be sent</param>
        /// <param name="isForced">Force UDP packet if <code>true</code>, else packet can be sent over TCP</param>
        public void SendUDP(byte[] buf, bool isForced)
        {
            // No udp available, send via TCP instead!
            // bool flagLostUDP = false;
            if (_client.UdpEndPoint == null || !(isForced || _client.UdpConfirm))
            {
                var newbuf = new byte[buf.Length - 2];
                newbuf[0] = buf[0];
                newbuf[1] = buf[1];

                Buffer.BlockCopy(buf, 4, newbuf, 2, buf.Length - 4);
                SendTCP(newbuf);
                return;
            }

            if (_client.ClientState == GameClient.eClientState.Playing)
            {
                if ((DateTime.Now.Ticks - _client.UdpPingTime) > 500000000L) // really 24s, not 50s
                {
                    // flagLostUDP = true;
                    _client.UdpConfirm = false;
                }
            }

            if (_udpSendBuffer == null)
            {
                return;
            }

            // increase our UDP counter when it reaches 0xFFFF
            // and increases, it will automaticallys switch back to 0x00
            _udpCounter++;

            // fill the udpCounter
            buf[2] = (byte)(_udpCounter >> 8);
            buf[3] = (byte)_udpCounter;

            // buf = m_encoding.EncryptPacket(buf, true);
            Statistics.BytesOut += buf.Length;
            Statistics.PacketsOut++;

            lock (((ICollection)_udpQueue).SyncRoot)
            {
                if (_sendingUdp)
                {
                    _udpQueue.Enqueue(buf);
                    return;
                }

                _sendingUdp = true;
            }

            Buffer.BlockCopy(buf, 0, _udpSendBuffer, 0, buf.Length);

            try
            {
                GameServer.Instance.SendUDP(_udpSendBuffer, buf.Length, _client.UdpEndPoint, _asyncUdpCallback);
            }
            catch (Exception e)
            {
                int count = _udpQueue.Count;

                lock (_udpQueue)
                {
                    _udpQueue.Clear();

                    _sendingUdp = false;
                }

                if (Log.IsErrorEnabled)
                {
                    Log.ErrorFormat($"trying to send UDP ({count})", e);
                }
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
                var s = (Socket)ar.AsyncState;
                s.EndSendTo(ar);

                int count = 0;
                byte[] data = _udpSendBuffer;

                if (data == null)
                {
                    return;
                }

                lock (((ICollection)_udpQueue).SyncRoot)
                {
                    if (_udpQueue.Count > 0)
                    {
                        count = CombinePackets(data, _udpQueue, _client);
                    }

                    if (count <= 0)
                    {
                        _sendingUdp = false;
                        return;
                    }
                }

                int start = Environment.TickCount;

                GameServer.Instance.SendUDP(data, count, _client.UdpEndPoint, _asyncUdpCallback);

                int took = Environment.TickCount - start;
                if (took > 100 && Log.IsWarnEnabled)
                {
                    Log.WarnFormat($"AsyncUdpSendCallback.BeginSend took {took}ms! (TCP to client: {_client})");
                }
            }
            catch (Exception e)
            {
                int count = _udpQueue.Count;

                lock (((ICollection)_udpQueue).SyncRoot)
                {
                    _udpQueue.Clear();

                    _sendingUdp = false;
                }

                if (Log.IsErrorEnabled)
                {
                    Log.Error($"AsyncUdpSendCallback ({count})", e);
                }
            }
        }

        /// <summary>
        /// Send the UDP packet without changing any portion of the packet
        /// </summary>
        /// <param name="packet">Packet to be sent</param>
        public void SendUDPRaw(GSUDPPacketOut packet)
        {
            SendUDP((byte[])packet.GetBuffer().Clone(), false);
        }

        /// <summary>
        /// Called when the client receives bytes
        /// </summary>
        /// <param name="numBytes">The number of bytes received</param>
        public void ReceiveBytes(int numBytes)
        {
            lock (_syncLock)
            {
                byte[] buffer = _client.ReceiveBuffer;

                // End Offset of buffer
                int bufferSize = _client.ReceiveBufferOffset + numBytes;

                // Size < minimum
                if (bufferSize < GSPacketIn.HDR_SIZE)
                {
                    _client.ReceiveBufferOffset = bufferSize; // undo buffer read
                    return;
                }

                // Reset the offset
                _client.ReceiveBufferOffset = 0;

                // Current offset into the buffer
                int curOffset = 0;

                do
                {
                    int packetLength = (buffer[curOffset] << 8) + buffer[curOffset + 1] + GSPacketIn.HDR_SIZE;
                    int dataLeft = bufferSize - curOffset;

                    if (dataLeft < packetLength)
                    {
                        Buffer.BlockCopy(buffer, curOffset, buffer, 0, dataLeft);
                        _client.ReceiveBufferOffset = dataLeft;
                        break;
                    }

                    // ** commented out because this hasn't been used in forever and crutching
                    // ** to it only hurts performance in a design that needs to be reworked
                    // ** anyways.
                    // **                                                               - tobz
                    // var curPacket = new byte[packetLength];
                    // Buffer.BlockCopy(buffer, curOffset, curPacket, 0, packetLength);
                    // curPacket = m_encoding.DecryptPacket(buffer, false);
                    int packetEnd = curOffset + packetLength;

                    int calcCheck = CalculateChecksum(buffer, curOffset, packetLength - 2);
                    int pakCheck = (buffer[packetEnd - 2] << 8) | buffer[packetEnd - 1];

                    if (pakCheck != calcCheck)
                    {
                        if (Log.IsWarnEnabled)
                        {
                            Log.WarnFormat($"Bad TCP packet checksum (packet:0x{pakCheck:X4} calculated:0x{calcCheck:X4}) -> disconnecting\nclient: {_client}\ncurOffset={curOffset}; packetLength={packetLength}");
                        }

                        if (Log.IsInfoEnabled)
                        {
                            Log.Info("Last client sent/received packets (from older to newer):");

                            foreach (IPacket prevPak in GetLastPackets())
                            {
                                Log.Info(prevPak.ToHumanReadable());
                            }

                            Log.Info(Marshal.ToHexDump("Last Received Bytes : ", buffer));
                        }

                        _client.Disconnect();
                        return;
                    }

                    var pak = new GSPacketIn(packetLength - GSPacketIn.HDR_SIZE);
                    pak.Load(buffer, curOffset, packetLength);

                    try
                    {
                        HandlePacket(pak);
                    }
                    catch (Exception e)
                    {
                        if (Log.IsErrorEnabled)
                        {
                            Log.Error("HandlePacket(pak)", e);
                        }
                    }

                    curOffset += packetLength;
                } while (bufferSize - 1 > curOffset);

                if (bufferSize - 1 == curOffset)
                {
                    buffer[0] = buffer[curOffset];
                    _client.ReceiveBufferOffset = 1;
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
            int len = i + dataSize;

            while (i < len)
            {
                val1 += pak[i++];
                val2 += val1;
            }

            return (ushort)(val2 - ((val1 + val2) << 8));
        }

        public void HandlePacketTimeout(object sender, ElapsedEventArgs e)
        {
            string source = (_client.Account != null) ? _client.Account.Name : _client.TcpEndpoint;
            if (Log.IsErrorEnabled)
            {
                Log.Error($"Thread {_handlerThreadId} - Handler {_activePacketHandler.GetType()} takes too much time (>10000ms) <{source}> !");
            }
        }

#if LOGACTIVESTACKS
        /// <summary>
        /// Holds a list of all currently active handler threads!
        /// This list is updated in the HandlePacket method
        /// </summary>
        private static readonly Hashtable ActivePacketThreads = Hashtable.Synchronized(new Hashtable());
#endif

        /// <summary>
        /// Retrieves a textual description of all active packet handler thread stacks
        /// </summary>
        /// <returns>A string with the stacks</returns>
        public static string GetConnectionThreadpoolStacks()
        {
#if LOGACTIVESTACKS
            var builder = new StringBuilder();

            // When enumerating over a synchronized hashtable, we need to
            // lock it's syncroot! Only for reading, not for writing locking
            // is needed!
            lock (ActivePacketThreads.SyncRoot)
            {
                foreach (DictionaryEntry entry in ActivePacketThreads)
                {
                    try
                    {
                        var thread = (Thread)entry.Key;
                        var client = (GameClient)entry.Value;

                        // The use of the deprecated Suspend and Resume methods is necessary to get the StackTrace.
                        // Suspend/Resume are not being used for thread synchronization (very bad).
                        // It may be possible to get the StackTrace some other way, but this works for now
                        // So, the related warning is disabled
                        #pragma warning disable 0618
                        thread.Suspend();
                        StackTrace trace;
                        try
                        {
                            trace = new StackTrace(thread, true);
                        }
                        finally
                        {
                            thread.Resume();
                        }
                        #pragma warning restore 0618

                        builder.Append("Stack for thread from account: ");
                        if (client?.Account != null)
                        {
                            builder.Append(client.Account.Name);
                            if (client.Player != null)
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
                    catch (Exception e)
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
            if (packet == null || _client == null)
            {
                return;
            }

            int code = packet.ID;

            Statistics.BytesIn += packet.PacketSize;
            Statistics.PacketsIn++;

            SavePacket(packet);

            IPacketHandler packetHandler = null;
            if (code < _packetHandlers.Length)
            {
                packetHandler = _packetHandlers[code];
            }

            else if (Log.IsErrorEnabled)
            {
                Log.Error($"Received packet code is outside of m_packetHandlers array bounds! {_client}");
                Log.Error(Marshal.ToHexDump($"===> <{(_client.Account != null ? _client.Account.Name : _client.TcpEndpoint)}> Packet 0x{code:X2} (0x{code ^ 168:X2}) length: {packet.PacketSize} (ThreadId={Thread.CurrentThread.ManagedThreadId})", packet.ToArray()));
            }

            // make sure we can handle this packet at this stage
            var preprocess = _packetPreprocessor.CanProcessPacket(_client, packet);
            if (!preprocess)
            {
                // this packet can't be processed by this client right now, for whatever reason
                Log.Info($"PacketPreprocessor: Preprocessor prevents handling of a packet with packet.ID={packet.ID}");
                return;
            }

            if (packetHandler != null)
            {
                Timer monitorTimer = null;
                if (Log.IsDebugEnabled)
                {
                    try
                    {
                        monitorTimer = new Timer(10000);
                        _activePacketHandler = packetHandler;
                        _handlerThreadId = Thread.CurrentThread.ManagedThreadId;
                        monitorTimer.Elapsed += HandlePacketTimeout;
                        monitorTimer.Start();
                    }
                    catch (Exception e)
                    {
                        if (Log.IsErrorEnabled)
                        {
                            Log.Error("Starting packet monitor timer", e);
                        }

                        if (monitorTimer != null)
                        {
                            monitorTimer.Stop();
                            monitorTimer.Close();
                            monitorTimer = null;
                        }
                    }
                }

#if LOGACTIVESTACKS
                // Put the current thread into the active thread list!
                // No need to lock the hashtable since we created it
                // synchronized! One reader, multiple writers supported!
                ActivePacketThreads.Add(Thread.CurrentThread, _client);
#endif
                long start = Environment.TickCount;
                try
                {
                    packetHandler.HandlePacket(_client, packet);
                }
                catch (Exception e)
                {
                    if (Log.IsErrorEnabled)
                    {
                        string client = _client?.ToString() ?? "null";
                        Log.Error($"Error while processing packet (handler={packetHandler.GetType().FullName}  client: {client})", e);
                    }
                }
#if LOGACTIVESTACKS
                finally
                {
                    // Remove the thread from the active list after execution
                    // No need to lock the hashtable since we created it
                    // synchronized! One reader, multiple writers supported!
                    ActivePacketThreads.Remove(Thread.CurrentThread);
                }
#endif
                long timeUsed = Environment.TickCount - start;
                if (monitorTimer != null)
                {
                    monitorTimer.Stop();
                    monitorTimer.Close();
                }

                _activePacketHandler = null;
                if (timeUsed > 1000)
                {
                    string source = _client.Account != null ? _client.Account.Name : _client.TcpEndpoint;
                    if (Log.IsWarnEnabled)
                    {
                        Log.Warn($"({source}) Handle packet Thread {Thread.CurrentThread.ManagedThreadId} {packetHandler} took {timeUsed}ms!");
                    }
                }
            }
        }
    }
}
