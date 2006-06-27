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
using System.IO;

namespace DOL
{
	/// <summary>
	/// Class reads data from incoming incoming packets
	/// </summary>
	public class PacketIn : MemoryStream
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public PacketIn() : base()
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="size">Size of the internal buffer</param>
		public PacketIn(int size) : base(size)
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="buf">Buffer containing packet data to read from</param>
		public PacketIn(byte[] buf) : base(buf)
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="buf">Buffer containing packet data to read from</param>
		/// <param name="canwrite">True if writing to the buffer is allowed</param>
		public PacketIn(byte[] buf, bool canwrite) : base(buf, canwrite)
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="buf">Buffer containing packet data to read from</param>
		/// <param name="start">Starting index into buf</param>
		/// <param name="size">Number of bytes to read from buf</param>
		public PacketIn(byte[] buf, int start, int size) : base(buf, start, size)
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="buf">Buffer containing packet data to read from</param>
		/// <param name="start">Starting index into buf</param>
		/// <param name="size">Number of bytes to read from buf</param>
		/// <param name="canwrite">True if writing to the buffer is allowed</param>
		public PacketIn(byte[] buf, int start, int size, bool canwrite) : base(buf, start, size, canwrite)
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="buf">Buffer containing packet data to read from</param>
		/// <param name="start">Starting index into buf</param>
		/// <param name="size">Number of bytes to read from buf</param>
		/// <param name="canwrite">True if writing to the buffer is allowed</param>
		/// <param name="getbuf">True if you can retrieve a copy of the internal buffer</param>
		public PacketIn(byte[] buf, int start, int size, bool canwrite, bool getbuf) : base(buf, start, size, canwrite, getbuf)
		{
		}

		/// <summary>
		/// Reads in 2 bytes and converts it from network to host byte order
		/// </summary>
		/// <returns>A 2 byte (short) value</returns>
		public virtual ushort ReadShort()
		{
			byte v1 = (byte)ReadByte();
			byte v2 = (byte)ReadByte();
			return Marshal.ConvertToUInt16(v1, v2);
		}

		/// <summary>
		/// Reads in 2 bytes
		/// </summary>
		/// <returns>A 2 byte (short) value in network byte order</returns>
		public virtual ushort ReadShortLowEndian()
		{
			byte v1 = (byte) ReadByte();
			byte v2 = (byte) ReadByte();
			return Marshal.ConvertToUInt16(v2, v1);
		}

		/// <summary>
		/// Reads in 4 bytes and converts it from network to host byte order
		/// </summary>
		/// <returns>A 4 byte value</returns>
		public virtual uint ReadInt()
		{
			byte v1 = (byte) ReadByte();
			byte v2 = (byte) ReadByte();
			byte v3 = (byte) ReadByte();
			byte v4 = (byte) ReadByte();
			return Marshal.ConvertToUInt32(v1, v2, v3, v4);
		}

		/// <summary>
		/// Skips 'num' bytes ahead in the stream
		/// </summary>
		/// <param name="num">Number of bytes to skip ahead</param>
		public void Skip(long num)
		{
			Seek(num, SeekOrigin.Current);
		}

		/// <summary>
		/// Reads a null-terminated string from the stream
		/// </summary>
		/// <param name="maxlen">Maximum number of bytes to read in</param>
		/// <returns>A string of maxlen or less</returns>
		public virtual string ReadString(int maxlen)
		{
			byte[] buf = new byte[maxlen];
			Read(buf, 0, maxlen);
			return Marshal.ConvertToString(buf);
		}

		/// <summary>
		/// Reads in a pascal style string
		/// </summary>
		/// <returns>A string from the stream</returns>
		public virtual string ReadPascalString()
		{
			int size = ReadByte();
			return ReadString(size);
		}
	}
}