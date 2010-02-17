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
using System.Text;

namespace DOL
{
	/// <summary>
	/// Provides basic functionality to convert data types.
	/// </summary>
	public static class Marshal
	{
		/// <summary>
		/// Reads a null-terminated string from a byte array.
		/// </summary>
		/// <param name="cstyle">the bytes</param>
		/// <returns>the string</returns>
		public static string ConvertToString(byte[] cstyle)
		{
			if (cstyle == null)
				return null;

			for (int i = 0; i < cstyle.Length; i++)
			{
				if (cstyle[i] == 0)
					return Encoding.Default.GetString(cstyle, 0, i);
			}
			return Encoding.Default.GetString(cstyle);
		}

		/// <summary>
		/// Converts 4 bytes to an integer value
		/// in high to low order
		/// </summary>
		/// <param name="val">the bytes</param>
		/// <returns>the integer value</returns>
		public static int ConvertToInt32(byte[] val)
		{
			return ConvertToInt32(val, 0);
		}

		/// <summary>
		/// Converts 4 bytes to an integer value
		/// in high to low order
		/// </summary>
		/// <param name="val">the bytes</param>
		/// <param name="startIndex">where to read the values from</param>
		/// <returns>the integer value</returns>
		public static int ConvertToInt32(byte[] val, int startIndex)
		{
			return ConvertToInt32(val[startIndex], val[startIndex + 1], val[startIndex + 2], val[startIndex + 3]);
		}

		/// <summary>
		/// Converts 4 bytes to an integer value
		/// in high to low order
		/// </summary>
		/// <param name="v1">the first bytes</param>
		/// <param name="v2">the second bytes</param>
		/// <param name="v3">the third bytes</param>
		/// <param name="v4">the fourth bytes</param>
		/// <returns>the integer value</returns>
		public static int ConvertToInt32(byte v1, byte v2, byte v3, byte v4)
		{
			return ((v1 << 24) | (v2 << 16) | (v3 << 8) | v4);
		}

		/// <summary>
		/// Converts 4 bytes to an unsigned integer value
		/// in high to low order
		/// </summary>
		/// <param name="val">the bytes</param>
		/// <returns>the integer value</returns>
		public static uint ConvertToUInt32(byte[] val)
		{
			return ConvertToUInt32(val, 0);
		}

		/// <summary>
		/// Converts 4 bytes to an unsigned integer value
		/// in high to low order
		/// </summary>
		/// <param name="val">the bytes</param>
		/// <param name="startIndex">where to read the values from</param>
		/// <returns>the integer value</returns>
		public static uint ConvertToUInt32(byte[] val, int startIndex)
		{
			return ConvertToUInt32(val[startIndex], val[startIndex + 1], val[startIndex + 2], val[startIndex + 3]);
		}

		/// <summary>
		/// Converts 4 bytes to an unsigned integer value
		/// in high to low order
		/// </summary>
		/// <param name="v1">the first bytes</param>
		/// <param name="v2">the second bytes</param>
		/// <param name="v3">the third bytes</param>
		/// <param name="v4">the fourth bytes</param>
		/// <returns>the integer value</returns>
		public static uint ConvertToUInt32(byte v1, byte v2, byte v3, byte v4)
		{
			return (uint) ((v1 << 24) | (v2 << 16) | (v3 << 8) | v4);
		}

		/// <summary>
		/// Converts 2 bytes to an short value
		/// in high to low order
		/// </summary>
		/// <param name="val">the bytes</param>
		/// <returns>the integer value</returns>
		public static short ConvertToInt16(byte[] val)
		{
			return ConvertToInt16(val, 0);
		}

		/// <summary>
		/// Converts 2 bytes to an short value
		/// in high to low order
		/// </summary>
		/// <param name="val">the bytes</param>
		/// <param name="startIndex">where to read the values from</param>
		/// <returns>the integer value</returns>
		public static short ConvertToInt16(byte[] val, int startIndex)
		{
			return ConvertToInt16(val[startIndex], val[startIndex + 1]);
		}

		/// <summary>
		/// Converts 2 bytes to an short value
		/// in high to low order
		/// </summary>
		/// <param name="v1">the first bytes</param>
		/// <param name="v2">the second bytes</param>
		/// <returns>the integer value</returns>
		public static short ConvertToInt16(byte v1, byte v2)
		{
			return (short) ((v1 << 8) | v2);
		}

		/// <summary>
		/// Converts 2 bytes to an unsigned short value
		/// in high to low order
		/// </summary>
		/// <param name="val">the bytes</param>
		/// <returns>the integer value</returns>
		public static ushort ConvertToUInt16(byte[] val)
		{
			return ConvertToUInt16(val, 0);
		}

		/// <summary>
		/// Converts 2 bytes to an unsigned short value
		/// in high to low order
		/// </summary>
		/// <param name="val">the bytes</param>
		/// <param name="startIndex">where to read the values from</param>
		/// <returns>the integer value</returns>
		public static ushort ConvertToUInt16(byte[] val, int startIndex)
		{
			return ConvertToUInt16(val[startIndex], val[startIndex + 1]);
		}

		/// <summary>
		/// Converts 2 bytes to an integer value
		/// in high to low order
		/// </summary>
		/// <param name="v1">the first bytes</param>
		/// <param name="v2">the second bytes</param>
		/// <returns>the integer value</returns>
		public static ushort ConvertToUInt16(byte v1, byte v2)
		{
			return (ushort) (v2 | (v1 << 8));
		}

		/// <summary>
		/// Converts a byte array into a hex dump
		/// </summary>
		/// <param name="description">Dump description</param>
		/// <param name="dump">byte array</param>
		/// <returns>the converted hex dump</returns>
		public static string ToHexDump(string description, byte[] dump)
		{
			return ToHexDump(description, dump, 0, dump.Length);
		}

		/// <summary>
		/// Converts a byte array into a hex dump
		/// </summary>
		/// <param name="description">Dump description</param>
		/// <param name="dump">byte array</param>
		/// <param name="start">dump start offset</param>
		/// <param name="count">dump bytes count</param>
		/// <returns>the converted hex dump</returns>
		public static string ToHexDump(string description, byte[] dump, int start, int count)
		{
			var hexDump = new StringBuilder();
			if (description != null)
			{
				hexDump.Append(description).Append("\n");
			}
			int end = start + count;
			for (int i = start; i < end; i += 16)
			{
				var text = new StringBuilder();
				var hex = new StringBuilder();
				hex.Append(i.ToString("X4"));
				hex.Append(": ");

				for (int j = 0; j < 16; j++)
				{
					if (j + i < end)
					{
						byte val = dump[j + i];
						hex.Append(dump[j + i].ToString("X2"));
						hex.Append(" ");
						if (val >= 32 && val <= 127)
						{
							text.Append((char) val);
						}
						else
						{
							text.Append(".");
						}
					}
					else
					{
						hex.Append("   ");
						text.Append(" ");
					}
				}
				hex.Append("  ");
				hex.Append(text.ToString());
				hex.Append('\n');
				hexDump.Append(hex.ToString());
			}
			return hexDump.ToString();
		}
	}
}