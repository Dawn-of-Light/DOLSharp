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
        /// Constructor
        /// </summary>
        /// <param name="type">Type of packet to handle</param>
        /// <param name="code">ID of the packet to handle</param>
        /// <param name="desc">Description of the packet handler</param>
        public PacketHandlerAttribute(PacketHandlerType type, int code, string desc)
        {
            Type = type;
            Code = code;
            Description = desc;
            PreprocessorID = (int)eClientStatus.None;
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
            Type = type;
            Code = code;
            Description = desc;
            PreprocessorID = preprocessorId;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type of packet to handle</param>
        /// <param name="code">ID of the packet to handle</param>
        /// <param name="desc">Description of the packet handler</param>
        /// <param name="preprocessorId">ID of the preprocessor to use for this packet</param>
        public PacketHandlerAttribute(PacketHandlerType type, int code, string desc, eClientStatus preprocessorId)
        {
            Type = type;
            Code = code;
            Description = desc;
            PreprocessorID = (int)preprocessorId;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type of packet to handle</param>
        /// <param name="code">ID of the packet to handle</param>
        /// <param name="desc">Description of the packet handler</param>
        /// <param name="preprocessorId">ID of the preprocessor to use for this packet</param>
        public PacketHandlerAttribute(PacketHandlerType type, eClientPackets code, eClientStatus preprocessorId)
        {
            Type = type;
            Code = (int)code;
            Description = string.Empty;
            PreprocessorID = (int)preprocessorId;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type of packet to handle</param>
        /// <param name="code">ID of the packet to handle</param>
        /// <param name="desc">Description of the packet handler</param>
        /// <param name="preprocessorId">ID of the preprocessor to use for this packet</param>
        public PacketHandlerAttribute(PacketHandlerType type, eClientPackets code, string desc, eClientStatus preprocessorId)
        {
            Type = type;
            Code = (int)code;
            Description = desc;
            PreprocessorID = (int)preprocessorId;
        }

        /// <summary>
        /// Gets the packet type
        /// </summary>
        public PacketHandlerType Type { get; }

        /// <summary>
        /// Gets the packet ID that is handled
        /// </summary>
        public int Code { get; }

        /// <summary>
        /// Gets the description of the packet handler
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the preprocessor ID associated with this packet.
        /// </summary>
        public int PreprocessorID { get; }
    }
}
