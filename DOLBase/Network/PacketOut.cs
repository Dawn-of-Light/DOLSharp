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
using System.IO;

namespace DOL.Network
{
	/// <summary>
	/// Writes primitives data types to an underlying stream.
	/// </summary>
	public class PacketOut : MemoryStream, IPacket
	{
		/// <summary>
		/// Default Constructor
		/// </summary>
		protected PacketOut()
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="size">Size of the internal buffer</param>
		public PacketOut(int size) : base(size)
		{
		}

		#region IPacket Members

		/// <summary>
		/// Generates a human-readable dump of the packet contents.
		/// </summary>
		/// <returns>a string representing the packet contents in hexadecimal</returns>
		public string ToHumanReadable()
		{
			return Marshal.ToHexDump(ToString(), ToArray());
		}

		#endregion

		/// <summary>
		/// Writes a 2 byte (short) value to the stream in network byte order
		/// </summary>
		/// <param name="val">Value to write</param>
		public void WriteShort(ushort val)
		{
			WriteByte((byte) (val >> 8));
			WriteByte((byte) (val & 0xff));
		}

		/// <summary>
		/// Writes a 2 byte (short) value to the stream in host byte order
		/// </summary>
		/// <param name="val">Value to write</param>
		public void WriteShortLowEndian(ushort val)
		{
			WriteByte((byte) (val & 0xff));
			WriteByte((byte) (val >> 8));
		}

		/// <summary>
		/// Writes a 4 byte value to the stream in host byte order
		/// </summary>
		/// <param name="val">Value to write</param>
		public void WriteInt(uint val)
		{
			WriteByte((byte) (val >> 24));
			WriteByte((byte) ((val >> 16) & 0xff));
			WriteByte((byte) ((val & 0xffff) >> 8));
			WriteByte((byte) ((val & 0xffff) & 0xff));
		}

		/// <summary>
		/// Writes a 4 byte value to the stream in host byte order
		/// </summary>
		/// <param name="val">Value to write</param>
		public void WriteIntLowEndian(uint val)
		{
			WriteByte((byte) ((val & 0xffff) & 0xff));
			WriteByte((byte) ((val & 0xffff) >> 8));
			WriteByte((byte) ((val >> 16) & 0xff));
			WriteByte((byte) (val >> 24));
		}

		/// <summary>
		/// Writes a 8 byte value to the stream in host byte order
		/// </summary>
		/// <param name="val">Value to write</param>
		public void WriteLong(ulong val)
		{
			WriteByte((byte) (val >> 56));
			WriteByte((byte) ((val >> 48) & 0xff));
			WriteByte((byte) ((val >> 40) & 0xff));
			WriteByte((byte) ((val >> 32) & 0xff));
			WriteByte((byte) ((val >> 24) & 0xff));
			WriteByte((byte) ((val >> 16) & 0xff));
			WriteByte((byte) ((val >> 8) & 0xff));
			WriteByte((byte) (val & 0xff));
		}

		/// <summary>
		/// Writes a 8 byte value to the stream in host byte order
		/// </summary>
		/// <param name="val">Value to write</param>
		public void WriteLongLowEndian(ulong val)
		{
			WriteByte((byte) (val & 0xff));
			WriteByte((byte) ((val >> 8) & 0xff));
			WriteByte((byte) ((val >> 16) & 0xff));
			WriteByte((byte) ((val >> 24) & 0xff));
			WriteByte((byte) ((val >> 32) & 0xff));
			WriteByte((byte) ((val >> 40) & 0xff));
			WriteByte((byte) ((val >> 48) & 0xff));
			WriteByte((byte) (val >> 56));
		}

		/// <summary>
		/// writes a float value to low endian used in 1.124 packets
		/// </summary>
		public void WriteFloatLowEndian(float val)
		{
			var bytes = BitConverter.GetBytes(val);
			Write(bytes, 0, bytes.Length);
		}
		
		/// <summary>
		/// Calculates the checksum for the internal buffer
		/// </summary>
		/// <returns>The checksum of the internal buffer</returns>
		public virtual byte GetChecksum()
		{
			byte val = 0;
			byte[] buf = GetBuffer();

			for (int i = 0; i < Position - 6; ++i)
			{
				val += buf[i + 8];
			}

			return val;
		}

		/// <summary>
		/// Writes the supplied value to the stream for a specified number of bytes
		/// </summary>
		/// <param name="val">Value to write</param>
		/// <param name="num">Number of bytes to write</param>
		public void Fill(byte val, int num)
		{
			for (int i = 0; i < num; ++i)
			{
				WriteByte(val);
			}
		}

		/// <summary>
		/// Writes the length of the patcket at the beginning of the stream
		/// </summary>
		/// <returns>Length of the packet</returns>
		public virtual ushort WritePacketLength()
		{
			Position = 0;

			WriteShort((ushort) (Length - 3));

			Capacity = (int) Length;

			return (ushort) (Length - 3);
		}

		/// <summary>
		/// Writes a pascal style string to the stream
		/// </summary>
		/// <param name="str">String to write</param>
		public void WritePascalString(string str)
		{
			if (str == null || str.Length <= 0)
			{
				WriteByte(0);
				return;
			}

			byte[] bytes = Constants.DefaultEncoding.GetBytes(str);
			WriteByte((byte) bytes.Length);
			Write(bytes, 0, bytes.Length);
		}

		public void WritePascalStringIntLE(string str)
		{
			if (str == null || str.Length <= 0)
			{
				WriteIntLowEndian(0);
				return;
			}

			byte[] bytes = Constants.DefaultEncoding.GetBytes(str);
			WriteIntLowEndian((uint)bytes.Length + 1);
			Write(bytes, 0, bytes.Length);
			WriteByte(0);
		}

		/// <summary>
		/// Writes a C-style string to the stream
		/// </summary>
		/// <param name="str">String to write</param>
		public void WriteString(string str)
		{
			WriteStringBytes(str);
			WriteByte(0x0);
		}


		/// <summary>
		/// Writes exactly the bytes from the string without any trailing 0
		/// </summary>
		/// <param name="str">the string to write</param>
		public void WriteStringBytes(string str)
		{
			if (str.Length <= 0)
				return;

			byte[] bytes = Constants.DefaultEncoding.GetBytes(str);
			Write(bytes, 0, bytes.Length);
		}

		/// <summary>
		/// Writes up to maxlen bytes to the stream from the supplied string
		/// </summary>
		/// <param name="str">String to write</param>
		/// <param name="maxlen">Maximum number of bytes to be written</param>
		public void WriteString(string str, int maxlen)
		{
			if (str.Length <= 0)
				return;

			byte[] bytes = Constants.DefaultEncoding.GetBytes(str);
			Write(bytes, 0, bytes.Length < maxlen ? bytes.Length : maxlen);
		}

		/// <summary>
		/// Writes len number of bytes from str to the stream
		/// </summary>
		/// <param name="str">String to write</param>
		/// <param name="len">Number of bytes to write</param>
		public void FillString(string str, int len)
		{
			long pos = Position;

			Fill(0x0, len);

			if (str == null)
				return;

			Position = pos;

			if (str.Length <= 0)
			{
				Position = pos + len;
				return;
			}

			byte[] bytes = Constants.DefaultEncoding.GetBytes(str);
			Write(bytes, 0, len > bytes.Length ? bytes.Length : len);
			Position = pos + len;
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>		
		public override string ToString()
		{
			return GetType().Name;
		}
	}
}
