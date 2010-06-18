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

namespace DOL.GS.PacketHandler
{
	/// <summary>
	/// Type of packet handler
	/// </summary>
	public enum PacketHandlerType
	{
		TCP = 0x01,
		UDP = 0x02
	}

	/// <summary>
	/// Denotes a class as a packet handler
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class PacketHandlerAttribute : Attribute 
	{
		/// <summary>
		/// Type of packet handler
		/// </summary>
		protected PacketHandlerType m_type;
		/// <summary>
		/// Packet ID to handle
		/// </summary>
		protected int m_code;
		/// <summary>
		/// Description of the packet handler
		/// </summary>
		protected string m_desc;

		/// <summary>
		/// Holds the ID of the preprocessor to use for this packet.
		/// </summary>
		protected int m_preprocessorId;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="type">Type of packet to handle</param>
		/// <param name="code">ID of the packet to handle</param>
		/// <param name="desc">Description of the packet handler</param>
		public PacketHandlerAttribute(PacketHandlerType type, int code, string desc)
		{
			m_type = type;
			m_code = code;
			m_desc = desc;
			m_preprocessorId = (int)ClientStatus.None;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="type">Type of packet to handle</param>
		/// <param name="code">ID of the packet to handle</param>
		/// <param name="desc">Description of the packet handler</param>
		/// <param name="preprocessorId">ID of the preprocessor to use for this packet</param>
		public PacketHandlerAttribute(PacketHandlerType type, int code, string desc, int preprocessorId)
		{
			m_type = type;
			m_code = code;
			m_desc = desc;
			m_preprocessorId = preprocessorId;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="type">Type of packet to handle</param>
		/// <param name="code">ID of the packet to handle</param>
		/// <param name="desc">Description of the packet handler</param>
		/// <param name="preprocessorId">ID of the preprocessor to use for this packet</param>
		public PacketHandlerAttribute(PacketHandlerType type, int code, string desc, ClientStatus preprocessorId)
		{
			m_type = type;
			m_code = code;
			m_desc = desc;
			m_preprocessorId = (int) preprocessorId;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="type">Type of packet to handle</param>
		/// <param name="code">ID of the packet to handle</param>
		/// <param name="desc">Description of the packet handler</param>
		/// <param name="preprocessorId">ID of the preprocessor to use for this packet</param>
		public PacketHandlerAttribute(PacketHandlerType type, eClientPackets code, ClientStatus preprocessorId)
		{
			m_type = type;
			m_code = (int)code;
			m_desc = "";
			m_preprocessorId = (int)preprocessorId;
		}

		/// <summary>
		/// Gets the packet type
		/// </summary>
		public PacketHandlerType Type
		{
			get
			{
				return m_type;
			}
		}

		/// <summary>
		/// Gets the packet ID that is handled
		/// </summary>
		public int Code 
		{
			get
			{
				return m_code;
			}
		}

		/// <summary>
		/// Gets the description of the packet handler
		/// </summary>
		public string Description
		{
			get
			{
				return m_desc;
			}
		}

		/// <summary>
		/// Gets the preprocessor ID associated with this packet.
		/// </summary>
		public int PreprocessorID
		{
			get
			{
				return m_preprocessorId;
			}
		}
	}
}
