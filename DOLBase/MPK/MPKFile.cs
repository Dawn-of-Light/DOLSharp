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
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace DOL.MPK
{
	/// <summary>
	/// Summary description for MPKFile.
	/// </summary>
	public class MPKFile
	{
		MPKFileHeader m_hdr = new MPKFileHeader();
		byte[] m_buf = null;
		byte[] m_compBuf = null;

		/// <summary>
		/// Constructs a new MPK file entry
		/// </summary>
		/// <param name="compData">The compressed data of this file entry</param>
		/// <param name="data">The uncompressed data of this file entry</param>
		/// <param name="hdr">The file entry header</param>
		public MPKFile(byte[] compData, byte[] data, MPKFileHeader hdr)
		{
			m_compBuf = compData;
			m_buf = data;
			m_hdr = hdr;
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
		/// Displays header information of this MPK file entry
		/// </summary>
		public void Display()
		{
			m_hdr.Display();
		}

		/// <summary>
		/// Loads a MPK file
		/// </summary>
		/// <param name="fname">The filename to load</param>
		public void Load(string fname)
		{
			FileInfo fi = new FileInfo(fname);

			if(!fi.Exists)
			{
				throw new System.IO.FileNotFoundException("File does not exist", fname);
			}

			using(FileStream file = fi.OpenRead())
			{
				m_buf = new byte[fi.Length];
				file.Read(m_buf, 0, m_buf.Length);
				file.Close();
			}

			m_hdr = new MPKFileHeader();
			m_hdr.Name = fname;
			m_hdr.Size = (uint)fi.Length;
			m_hdr.TimeStamp = (uint)DateTime.Now.ToFileTime();

			Deflater def = new Deflater();

			def.SetInput(m_buf, 0, m_buf.Length);
			def.Finish();

			// create temporary buffer
			byte[] tempbuffer = new byte[m_buf.Length+m_buf.Length/5];
			m_hdr.CompressedSize = (uint)def.Deflate(tempbuffer, 0, tempbuffer.Length);
			
			m_compBuf = new byte[m_hdr.CompressedSize];
			Array.Copy(tempbuffer,0,m_compBuf,0,m_hdr.CompressedSize);


			Crc32 crc = new Crc32();
			crc.Update(m_compBuf, 0, (int)m_hdr.CompressedSize);
			m_hdr.CRC = crc;
		}

		/// <summary>
		/// Saves an MPK file
		/// </summary>
		/// <param name="dir">The directory where to save the file</param>
		public void Save(string dir)
		{
			if(!dir.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				dir += Path.DirectorySeparatorChar;
			}

			using(BinaryWriter writer = new BinaryWriter(File.Create(dir + m_hdr.Name), System.Text.Encoding.UTF8))
			{
				writer.Write(m_buf);
				writer.Flush();
				writer.Close();
			}
		}

		/// <summary>
		/// Gets the MPK header
		/// </summary>
		public MPKFileHeader Header
		{
			get
			{
				return m_hdr;
			}
		}

		/// <summary>
		/// Gets the unencrypted Data in the MPK
		/// </summary>
		public byte[] Data
		{
			get
			{
				byte[] buf = new byte[m_buf.Length];
				m_buf.CopyTo(buf, 0);
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
				byte[] buf = new byte[m_hdr.CompressedSize];
				m_compBuf.CopyTo(buf, 0);
				return buf;
			}
		}
	}
}
