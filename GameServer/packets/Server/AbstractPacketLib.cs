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
using System.Reflection;
using log4net;

namespace DOL.GS.PacketHandler
{
    public abstract class AbstractPacketLib
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The GameClient of this PacketLib
        /// </summary>
        protected GameClient GameClient { get; }

        /// <summary>
        /// Constructs a new PacketLib
        /// </summary>
        /// <param name="client">the gameclient this lib is associated with</param>
        protected AbstractPacketLib(GameClient client)
        {
            GameClient = client;
        }

        /// <summary>
        /// Retrieves the packet code depending on client version
        /// </summary>
        /// <param name="packetCode"></param>
        /// <returns></returns>
        public virtual byte GetPacketCode(eServerPackets packetCode)
        {
            return (byte)packetCode;
        }

        /// <summary>
        /// Sends a packet via TCP
        /// </summary>
        /// <param name="packet">The packet to be sent</param>
        public void SendTCP(GSTCPPacketOut packet)
        {
            GameClient.PacketProcessor.SendTCP(packet);
        }

        /// <summary>
        /// Sends a packet via TCP
        /// </summary>
        /// <param name="buf">Buffer containing the data to be sent</param>
        public void SendTCP(byte[] buf)
        {
            GameClient.PacketProcessor.SendTCP(buf);
        }

        /// <summary>
        /// Send the packet via TCP without changing any portion of the packet
        /// </summary>
        /// <param name="packet">Packet to send</param>
        public void SendTCPRaw(GSTCPPacketOut packet)
        {
            GameClient.PacketProcessor.SendTCPRaw(packet);
        }

        /// <summary>
        /// Send the packet via UDP
        /// </summary>
        /// <param name="packet">Packet to be sent</param>
        public virtual void SendUDP(GSUDPPacketOut packet)
        {
            SendUDP(packet, false);
        }

        /// <summary>
        /// Send the packet via UDP
        /// </summary>
        /// <param name="packet">Packet to be sent</param>
        /// <param name="isForced">Force UDP packet if <code>true</code>, else packet can be sent over TCP</param>
        public virtual void SendUDP(GSUDPPacketOut packet, bool isForced)
        {
            GameClient.PacketProcessor.SendUDP(packet, isForced);
        }

        /// <summary>
        /// Send the packet via UDP
        /// </summary>
        /// <param name="buf">Packet to be sent</param>
        public void SendUDP(byte[] buf)
        {
            GameClient.PacketProcessor.SendUDP(buf, false);
        }

        /// <summary>
        /// Send the UDP packet without changing any portion of the packet
        /// </summary>
        /// <param name="packet">Packet to be sent</param>
        public void SendUDPRaw(GSUDPPacketOut packet)
        {
            GameClient.PacketProcessor.SendUDPRaw(packet);
        }

        /// <summary>
        /// Finds and creates packetlib for specified raw version.
        /// </summary>
        /// <param name="rawVersion">The version number sent by the client.</param>
        /// <param name="client">The client for which to create packet lib.</param>
        /// <param name="version">The client version of packetlib.</param>
        /// <returns>null if not found or new packetlib instance.</returns>
        public static IPacketLib CreatePacketLibForVersion(int rawVersion, GameClient client, out GameClient.eClientVersion version)
        {
            foreach (Type t in ScriptMgr.GetDerivedClasses(typeof(IPacketLib)))
            {
                foreach (PacketLibAttribute attr in t.GetCustomAttributes(typeof(PacketLibAttribute), false))
                {
                    if (attr.RawVersion == rawVersion)
                    {
                        try
                        {
                            IPacketLib lib = (IPacketLib)Activator.CreateInstance(t, client);
                            version = attr.ClientVersion;
                            return lib;
                        }
                        catch (Exception e)
                        {
                            if (Log.IsErrorEnabled)
                            {
                                Log.Error($"error creating packetlib ({t.FullName}) for raw version {rawVersion}", e);
                            }
                        }
                    }
                }
            }

            version = GameClient.eClientVersion.VersionUnknown;
            return null;
        }

        /// <summary>
        /// Return the msb or lsb byte used the server versionning
        /// eg: 199 -> 1.99; 1100 -> 1.100
        /// </summary>
        /// <param name="version"></param>
        /// <param name="isMsb"></param>
        /// <returns></returns>
        public static byte ParseVersion(int version, bool isMsb)
        {
            int cteVersion = 100;
            if (version > 199)
            {
                cteVersion = 1000;
            }

            if (isMsb)
            {
                return (byte)(version / cteVersion);
            }

            return (byte)((version % cteVersion) / 10);
        }
    }
}
