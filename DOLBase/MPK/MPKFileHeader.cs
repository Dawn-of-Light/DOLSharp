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
using System.Text;
using ICSharpCode.SharpZipLib.Checksums;

namespace DOL.MPK
{
	/// <summary>
	/// Summary description for MPKFileHeader.
	/// </summary>
	public class MPKFileHeader
	{
		/// <summary>
		/// Maximum size of a file header
		/// </summary>
		public static readonly int MAX_SIZE = 0x11c;

		/// <summary>
		/// Compressed size of the file
		/// </summary>
		private uint m_compSize;

		/// <summary>
		/// Checksum for the compressed file
		/// </summary>
		private Crc32 m_crc = new Crc32();

		/// <summary>
		/// Offset of the file header in the directory memory space
		/// </summary>
		private uint m_dirOff;

		/// <summary>
		/// Name of the file
		/// </summary>
		private string m_name = "";

		/// <summary>
		/// Offset of the file in file memory space
		/// </summary>
		private uint m_off;

		/// <summary>
		/// Size of the uncompressed file
		/// </summary>
		private uint m_size;

		/// <summary>
		/// Time the entry was created
		/// </summary>
		private uint m_time;

		/// <summary>
		/// Unknown, always 4?
		/// </summary>
		private int m_unk = 4;

		/// <summary>
		/// Creates a new MPK file header
		/// </summary>
		public MPKFileHeader()
		{
		}

		/// <summary>
		/// Creates a new MPK file header
		/// </summary>
		/// <param name="rdr">The binary reader pointing to the MPK header</param>
		public MPKFileHeader(BinaryReader rdr)
		{
			Read(rdr);
		}

		/// <summary>
		/// Gets or sets the name
		/// </summary>
		public string Name
		{
			get { return m_name; }
			set
			{
				if (value.Length <= 256)
				{
					m_name = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the timestamp of the MPK
		/// </summary>
		public uint TimeStamp
		{
			get { return m_time; }
			set { m_time = value; }
		}

		/// <summary>
		/// Gets or sets the size
		/// </summary>
		public uint Size
		{
			get { return m_size; }
			set { m_size = value; }
		}

		/// <summary>
		/// Gets or sets the offset inside the MPK
		/// </summary>
		public uint Offset
		{
			get { return m_off; }
			set { m_off = value; }
		}

		/// <summary>
		/// Gets or sets the Directory offset inside the MPK
		/// </summary>
		public uint DirectoryOffset
		{
			get { return m_dirOff; }
			set { m_dirOff = value; }
		}

		/// <summary>
		/// Gets or sets the compressed size
		/// </summary>
		public uint CompressedSize
		{
			get { return m_compSize; }
			set { m_compSize = value; }
		}

		/// <summary>
		/// Gets or sets the CRC32 checksum of the MPK
		/// </summary>
		public Crc32 CRC
		{
			get { return m_crc; }
			set { m_crc = value; }
		}

		/// <summary>
		/// Writes a new MPK header into a file
		/// </summary>
		/// <param name="file"></param>
		public void Write(BinaryWriter file)
		{
			var name = new byte[256];

			byte[] buf = Encoding.UTF8.GetBytes(m_name);
			buf.CopyTo(name, 0);
			name[buf.Length] = 0;

			file.Write(name, 0, 256);
			file.Write(m_time);
			file.Write(m_unk);
			file.Write(m_off);
			file.Write(m_size);
			file.Write(m_dirOff);
			file.Write(m_compSize);
			file.Write((uint) m_crc.Value);
		}

		/// <summary>
		/// Reads a new MPK header from a binary reader
		/// </summary>
		/// <param name="rdr">The binary reader pointing to the MPK header</param>
		public void Read(BinaryReader rdr)
		{
			var buf = new byte[256];
			rdr.Read(buf, 0, 256);
			m_name = Marshal.ConvertToString(buf);
			m_time = rdr.ReadUInt32();
			m_unk = rdr.ReadInt32();
			m_off = rdr.ReadUInt32();
			m_size = rdr.ReadUInt32();
			m_dirOff = rdr.ReadUInt32();
			m_compSize = rdr.ReadUInt32();
			m_crc.Value = rdr.ReadUInt32();
		}

		/// <summary>
		/// Displays debug information about this MPK file header
		/// </summary>
		public void Display()
		{
			Console.WriteLine("---------------------------------------------------------------------------");

			Console.WriteLine("Name: {0}", m_name);
			Console.WriteLine("Offset: {0}", m_off);
			Console.WriteLine("Size: {0}", m_size);
			Console.WriteLine("Directory Offset: {0}", m_dirOff);
			Console.WriteLine("Compressed Size: {0}", m_compSize);
			Console.WriteLine("CRC32: {0}", m_crc.Value);
		}
	}
}