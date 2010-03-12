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
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace DOL.MPK
{
	/// <summary>
	/// Represents a file stored in an MPK archive.
	/// </summary>
	public class MPKFile
	{
		private byte[] _buf;
		private byte[] _compBuf;
		private MPKFileHeader _hdr = new MPKFileHeader();

		/// <summary>
		/// Constructs a new MPK file entry
		/// </summary>
		/// <param name="compData">The compressed data of this file entry</param>
		/// <param name="data">The uncompressed data of this file entry</param>
		/// <param name="hdr">The file entry header</param>
		public MPKFile(byte[] compData, byte[] data, MPKFileHeader hdr)
		{
			_compBuf = compData;
			_buf = data;
			_hdr = hdr;
		}

		/// <summary>
		/// Creates a new MPK file entry
		/// </summary>
		/// <param name="fname">The file name of the MPK file entry</param>
		public MPKFile(string fname)
		{
			Load(fname);
		}

		/// <summary>
		/// Gets the MPK header
		/// </summary>
		public MPKFileHeader Header
		{
			get { return _hdr; }
		}

		/// <summary>
		/// Gets the unencrypted Data in the MPK
		/// </summary>
		public byte[] Data
		{
			get
			{
				var buf = new byte[_buf.Length];
				Buffer.BlockCopy(_buf, 0, buf, 0, buf.Length);

				return buf;
			}
		}

		/// <summary>
		/// Gets the compressed data in the MPK
		/// </summary>
		public byte[] CompressedData
		{
			get
			{
				var buf = new byte[_hdr.CompressedSize];
				Buffer.BlockCopy(_compBuf, 0, buf, 0, buf.Length);

				return buf;
			}
		}

		/// <summary>
		/// Displays header information of this MPK file entry
		/// </summary>
		public void Display()
		{
			_hdr.Display();
		}

		/// <summary>
		/// Loads a MPK file
		/// </summary>
		/// <param name="fname">The filename to load</param>
		public void Load(string fname)
		{
			var fi = new FileInfo(fname);

			if (!fi.Exists)
			{
				throw new FileNotFoundException("File does not exist", fname);
			}

			using (FileStream file = fi.OpenRead())
			{
				_buf = new byte[fi.Length];
				file.Read(_buf, 0, _buf.Length);
			}

			_hdr = new MPKFileHeader { Name = fname, UncompressedSize = (uint)fi.Length, TimeStamp = (uint)DateTime.Now.ToFileTime() };

			var def = new Deflater();

			def.SetInput(_buf, 0, _buf.Length);
			def.Finish();

			// create temporary buffer
			var tempbuffer = new byte[_buf.Length + _buf.Length / 5];
			_hdr.CompressedSize = (uint)def.Deflate(tempbuffer, 0, tempbuffer.Length);

			_compBuf = new byte[_hdr.CompressedSize];
			Buffer.BlockCopy(tempbuffer, 0, _compBuf, 0, (int)_hdr.CompressedSize);


			var crc = new Crc32();
			crc.Update(_compBuf, 0, (int)_hdr.CompressedSize);

			_hdr.CRC = crc;
		}

		/// <summary>
		/// Saves an MPK file
		/// </summary>
		/// <param name="dir">The directory where to save the file</param>
		public void Save(string dir)
		{
			if (!dir.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				dir += Path.DirectorySeparatorChar;
			}

			using (var writer = new BinaryWriter(File.Create(dir + _hdr.Name), Encoding.UTF8))
			{
				writer.Write(_buf);
			}
		}
	}
}