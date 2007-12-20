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
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// The GameClient of this PacketLib
		/// </summary>
		protected readonly GameClient m_gameClient;

		/// <summary>
		/// Constructs a new PacketLib
		/// </summary>
		/// <param name="client">the gameclient this lib is associated with</param>
		public AbstractPacketLib(GameClient client)
		{
			m_gameClient = client;
		}

		/// <summary>
		/// Retrieves the packet code depending on client version
		/// </summary>
		/// <param name="packetCode"></param>
		/// <returns></returns>
		public virtual byte GetPacketCode(ePackets packetCode)
		{
			return (byte)packetCode;
		}

		/// <summary>
		/// Sends a packet via TCP
		/// </summary>
		/// <param name="packet">The packet to be sent</param>
		public void SendTCP(GSTCPPacketOut packet)
		{
			m_gameClient.PacketProcessor.SendTCP(packet);
		}

		/// <summary>
		/// Sends a packet via TCP
		/// </summary>
		/// <param name="buf">Buffer containing the data to be sent</param>
		public void SendTCP(byte[] buf)
		{	
			m_gameClient.PacketProcessor.SendTCP(buf);
		}

		/// <summary>
		/// Send the packet via TCP without changing any portion of the packet
		/// </summary>
		/// <param name="packet">Packet to send</param>
		public void SendTCPRaw(GSTCPPacketOut packet)
		{
			m_gameClient.PacketProcessor.SendTCPRaw(packet);
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
			m_gameClient.PacketProcessor.SendUDP(packet, isForced);
		}

		/// <summary>
		/// Send the packet via UDP
		/// </summary>
		/// <param name="buf">Packet to be sent</param>
		public void SendUDP(byte[] buf)
		{
			m_gameClient.PacketProcessor.SendUDP(buf, false);
		}

		/// <summary>
		/// Send the UDP packet without changing any portion of the packet
		/// </summary>
		/// <param name="packet">Packet to be sent</param>
		public void SendUDPRaw(GSUDPPacketOut packet)
		{
			m_gameClient.PacketProcessor.SendUDPRaw(packet);
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
			foreach (Type t in ScriptMgr.GetDerivedClasses(typeof (IPacketLib)))
			{
				foreach (PacketLibAttribute attr in t.GetCustomAttributes(typeof (PacketLibAttribute), false))
				{
					if (attr.RawVersion == rawVersion)
					{
						try
						{
							IPacketLib lib = (IPacketLib) Activator.CreateInstance(t, new object[] {client});
							version = attr.ClientVersion;
							return lib;
						}
						catch (Exception e)
						{
							if (log.IsErrorEnabled)
								log.Error("error creating packetlib (" + t.FullName + ") for raw version " + rawVersion, e);
						}
					}
				}
			}
			
			version = GameClient.eClientVersion.VersionUnknown;
			return null;
		}
	}
}
