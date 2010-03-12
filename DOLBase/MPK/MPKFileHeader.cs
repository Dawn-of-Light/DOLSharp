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
	/// Represents the header of an internal file in an MPK archive.
	/// </summary>
	public class MPKFileHeader
	{
		/// <summary>
		/// Maximum size of a file header
		/// </summary>
		public const int MaxSize = 0x11c;

		/// <summary>
		/// Compressed size of the file
		/// </summary>
		private uint _compressedSize;

		/// <summary>
		/// Checksum for the compressed file
		/// </summary>
		private Crc32 _crc = new Crc32();

		/// <summary>
		/// Offset of the file header in the directory memory space
		/// </summary>
		private uint _directoryOffset;

		/// <summary>
		/// Name of the file
		/// </summary>
		private string _name = "";

		/// <summary>
		/// Offset of the file in file memory space
		/// </summary>
		private uint _offset;

		/// <summary>
		/// Size of the uncompressed file
		/// </summary>
		private uint _uncompressedSize;

		/// <summary>
		/// Time the entry was created
		/// </summary>
		private uint _timestamp;

		/// <summary>
		/// Unknown, always 4?
		/// </summary>
		private int _unk = 4;

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
			get { return _name; }
			set
			{
				if (value.Length <= 256)
				{
					_name = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the timestamp of the MPK
		/// </summary>
		public uint TimeStamp
		{
			get { return _timestamp; }
			set { _timestamp = value; }
		}

		/// <summary>
		/// Gets or sets the size
		/// </summary>
		public uint UncompressedSize
		{
			get { return _uncompressedSize; }
			set { _uncompressedSize = value; }
		}

		/// <summary>
		/// Gets or sets the offset inside the MPK
		/// </summary>
		public uint Offset
		{
			get { return _offset; }
			set { _offset = value; }
		}

		/// <summary>
		/// Gets or sets the Directory offset inside the MPK
		/// </summary>
		public uint DirectoryOffset
		{
			get { return _directoryOffset; }
			set { _directoryOffset = value; }
		}

		/// <summary>
		/// Gets or sets the compressed size
		/// </summary>
		public uint CompressedSize
		{
			get { return _compressedSize; }
			set { _compressedSize = value; }
		}

		/// <summary>
		/// Gets or sets the CRC32 checksum of the MPK
		/// </summary>
		public Crc32 CRC
		{
			get { return _crc; }
			set { _crc = value; }
		}

		/// <summary>
		/// Writes a new MPK header into a file
		/// </summary>
		/// <param name="file"></param>
		public void Write(BinaryWriter file)
		{
			var name = new byte[256];

			byte[] buf = Encoding.UTF8.GetBytes(_name);
			Buffer.BlockCopy(buf, 0, name, 0, buf.Length);
			name[buf.Length] = 0;

			file.Write(name, 0, 256);
			file.Write(_timestamp);
			file.Write(_unk);
			file.Write(_offset);
			file.Write(_uncompressedSize);
			file.Write(_directoryOffset);
			file.Write(_compressedSize);
			file.Write((uint) _crc.Value);
		}

		/// <summary>
		/// Reads a new MPK header from a binary reader
		/// </summary>
		/// <param name="rdr">The binary reader pointing to the MPK header</param>
		public void Read(BinaryReader rdr)
		{
			var buf = new byte[256];
			rdr.Read(buf, 0, 256);

			_name = Marshal.ConvertToString(buf);
			_timestamp = rdr.ReadUInt32();
			_unk = rdr.ReadInt32();
			_offset = rdr.ReadUInt32();
			_uncompressedSize = rdr.ReadUInt32();
			_directoryOffset = rdr.ReadUInt32();
			_compressedSize = rdr.ReadUInt32();
			_crc.Value = rdr.ReadUInt32();
		}

		/// <summary>
		/// Displays debug information about this MPK file header
		/// </summary>
		public void Display()
		{
			Console.WriteLine("---------------------------------------------------------------------------");

			Console.WriteLine("Name: {0}", _name);
			Console.WriteLine("Offset: {0}", _offset);
			Console.WriteLine("Size: {0}", _uncompressedSize);
			Console.WriteLine("Directory Offset: {0}", _directoryOffset);
			Console.WriteLine("Compressed Size: {0}", _compressedSize);
			Console.WriteLine("CRC32: {0}", _crc.Value);
		}
	}
}