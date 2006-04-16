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

namespace DOL
{
	namespace GS
	{
		/// <summary>
		/// Game server specific packet
		/// </summary>
		public class GSPacketIn : PacketIn
		{
			/// <summary>
			/// Defines a logger for this class.
			/// </summary>
			private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

			/// <summary>
			/// Header size including checksum at the end of the packet
			/// </summary>
			public const ushort HDR_SIZE = 12;
			/// <summary>
			/// Session ID
			/// </summary>
			protected ushort m_sessionID;
			/// <summary>
			/// Packet size
			/// </summary>
			protected ushort m_psize;
			/// <summary>
			/// Packet sequence (ordering)
			/// </summary>
			protected ushort m_sequence;
			/// <summary>
			/// Packet ID
			/// </summary>
			protected ushort m_id;
			/// <summary>
			/// Packet parameter
			/// </summary>
			protected ushort m_parameter;

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="pak">Base packet to wrap</param>
			public GSPacketIn(PacketIn pak):base(10)
			{
				m_psize = pak.ReadShort();
				m_sequence = pak.ReadShort();
				m_sessionID = pak.ReadShort();
				m_parameter = pak.ReadShort();
				m_id = pak.ReadShort();

				byte[] buf = new byte[m_psize];
				pak.Read(buf, 0, m_psize);
			
				pak.Skip(2); //Skip the checksum bytes

				SetLength(m_psize);
				Position = 0;
				Write(buf, 0, m_psize);
				Position = 0;
			}

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="size">Size of the internal buffer</param>
			public GSPacketIn(int size) : base(size)
			{
			}

			/// <summary>
			/// Dumps the packet data into the log
			/// </summary>
			public void LogDump()
			{
				if (log.IsDebugEnabled)
					log.Debug(Marshal.ToHexDump(ToString(), ToArray()));
			}

			/// <summary>
			/// Gets the session id
			/// </summary>
			public ushort SessionID
			{
				get { return m_sessionID; }
			}

			/// <summary>
			/// Gets the packet size
			/// </summary>
			public ushort PacketSize
			{
				get { return (ushort)(m_psize + HDR_SIZE); }
			}

			/// <summary>
			/// Gets the size of the data portion of the packet
			/// </summary>
			public ushort DataSize
			{
				get { return m_psize; }
			}

			/// <summary>
			/// Gets the sequence of the packet
			/// </summary>
			public ushort Sequence
			{
				get { return m_sequence; }
			}

			/// <summary>
			/// Gets the packet ID
			/// </summary>
			public ushort ID
			{
				get { return m_id; }
			}

			/// <summary>
			/// Gets the packet parameter
			/// </summary>
			public ushort Parameter
			{
				get { return m_parameter; }
			}

			/// <summary>
			/// Loads the specified count of bytes from another buffer
			/// </summary>
			/// <param name="buf">The buffer to load the data from</param>
			/// <param name="count">The count of packet bytes</param>
			public virtual void Load(byte[] buf, int count)
			{
				m_psize     = (ushort)((buf[0] << 8) | buf[1]);
				m_sequence  = (ushort)((buf[2] << 8) | buf[3]);
				m_sessionID = (ushort)((buf[4] << 8) | buf[5]);
				m_parameter = (ushort)((buf[6] << 8) | buf[7]);
				m_id        = (ushort)((buf[8] << 8) | buf[9]);
				Position = 0;
				Write(buf, 10, count - HDR_SIZE);
				SetLength(count - HDR_SIZE);
				Position = 0;
			}

			/// <summary>
			/// Info about the packet
			/// </summary>
			/// <returns></returns>
			public override string ToString()
			{
				return
					GetType().Name + ": Size=" + m_psize + " Sequence=0x" + m_sequence.ToString("X4") + " Session=" + m_sessionID +
					" Parameter=" + m_parameter + " ID=0x" + m_id.ToString("X2");
			}
		}
	}
}