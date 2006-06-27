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
using System.Collections;
using System.Collections.Specialized;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace DOL.MPK
{
	/// <summary>
	/// Summary description for MPKDirectory.
	/// </summary>
	public class MPK
	{
		/// <summary>
		/// The magic at the top of the file
		/// </summary>
		public static readonly uint MAGIC = 0x4b41504d; //MPAK
		/// <summary>
		/// CRC32 of the deflated directory
		/// </summary>
		Crc32 m_crc = new Crc32();
		/// <summary>
		/// Compressed size of the directory section
		/// </summary>
		int m_sizeDir = 0;
		/// <summary>
		/// Compressed size of the name section
		/// </summary>
		int m_sizeName = 0;
		/// <summary>
		/// Number of files in the directory
		/// </summary>
		int m_numFiles = 0;
		/// <summary>
		/// Name of the archive
		/// </summary>
		string m_name = "";
		/// <summary>
		/// Holds all of the files in the MPK
		/// </summary>
		HybridDictionary m_files = new HybridDictionary();

		/// <summary>
		/// Delegate to be called when an invalid file was found inside the MPK
		/// </summary>
		public delegate void InvalidFileEventHandler(object sender, EventArgs e);

		/// <summary>
		/// The event to fire if an invalid file was found
		/// </summary>
		protected event InvalidFileEventHandler InvalidFile;

		/// <summary>
		/// Creates a new MPK file
		/// </summary>
		/// <param name="fname">The filename</param>
		/// <param name="create">if true, creates the file, else parses an existing file</param>
		public MPK(string fname, bool create)
		{
			if(!create)
			{
				Read(fname);
			}
			else
			{
				m_name = fname;
			}
		}

		/// <summary>
		/// Creates a new MPK file
		/// </summary>
		public MPK()
		{
		}

		/// <summary>
		/// The name of this file
		/// </summary>
		public string Name
		{
			get
			{
				return m_name;
			}
			set
			{
				m_name = value;
			}
		}

		/// <summary>
		/// The CRC of the MPK file
		/// </summary>
		public Crc32 CRC
		{
			get
			{
				return m_crc;
			}
		}

		/// <summary>
		/// The directory size of this MPK
		/// </summary>
		public int DirectorySize
		{
			get
			{
				return m_sizeDir;
			}
		}

		/// <summary>
		/// The filecount in this MPK
		/// </summary>
		public int Count
		{
			get
			{
				return m_files.Count;
			}
		}

		/// <summary>
		/// Gets a specific MPK file from this MPK
		/// </summary>
		public MPKFile this[string fname]
		{
			get
			{
				if(m_files.Contains(fname.ToLower()))
				{
					return (MPKFile)m_files[fname.ToLower()];
				}

				return null;
			}
		}

		/// <summary>
		/// Gets a list of all the files inside this MPK
		/// </summary>
		/// <returns>An IDictionaryEnumerator containing entries as filename, MPKFile pairs</returns>
		public IDictionaryEnumerator GetEnumerator()
		{
			return m_files.GetEnumerator();
		}

		/// <summary>
		/// Adds a file to the MPK
		/// </summary>
		/// <param name="file">The file to add</param>
		/// <returns>true if successfull, false if the file is already contained</returns>
		public bool AddFile(MPKFile file)
		{
			if(!m_files.Contains(file.Header.Name))
			{
				m_files.Add(file.Header.Name, file);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Removes a file from the MPK
		/// </summary>
		/// <param name="fname">The file to remove</param>
		/// <returns>true if the file was successfully removed, false if it wasn't in the MPK</returns>
		public bool RemoveFile(string fname)
		{
			if(m_files.Contains(fname.ToLower()))
			{
				m_files.Remove(fname.ToLower());
				return true;
			}

			return false;
		}

		/// <summary>
		/// Removes a file from the MPK
		/// </summary>
		/// <param name="file">The file to remove</param>
		/// <returns>true if the file was successfully removed, false if it wasn't in the MPK</returns>
		public bool RemoveFile(MPKFile file)
		{
			return RemoveFile(file.Header.Name);
		}

		/// <summary>
		/// Saves the MPK
		/// </summary>
		public void Save()
		{
			Write(this.Name);
		}

		/// <summary>
		/// Writes the MPK to a specific filename
		/// </summary>
		/// <param name="fname"></param>
		public void Write(string fname)
		{
			MemoryStream dirmem = new MemoryStream(m_files.Count * MPKFileHeader.MAX_SIZE);
			MemoryStream filemem = new MemoryStream();
			BinaryWriter wrtr = new BinaryWriter(filemem, System.Text.Encoding.UTF8);

			MPKFile[] files = new MPKFile[m_files.Count];
			int index = 0;
			uint offset = 0;
			uint diroffset = 0;

			foreach(MPKFile file in m_files.Values)
			{
				file.Header.DirectoryOffset = diroffset;
				file.Header.Offset = offset;
				files[index] = file;
				file.Header.Write(new BinaryWriter(dirmem, System.Text.Encoding.UTF8));
				offset += file.Header.Size;
				diroffset += file.Header.CompressedSize;
				index++;
			}

			Deflater def = new Deflater();
			def.SetInput(dirmem.GetBuffer(), 0, (int)dirmem.Position);
			def.Finish();
			byte[] dir = new byte[dirmem.Position];
			def.Deflate(dir);
			m_sizeDir = (int) def.TotalOut;
			def = new Deflater();

			m_crc.Reset();
			m_crc.Update(dir, 0, m_sizeDir);
			
			def.SetInput(System.Text.Encoding.UTF8.GetBytes(m_name));
			def.Finish();
			byte[] name = new byte[m_name.Length];
			def.Deflate(name);

			m_numFiles = m_files.Count;
			m_sizeName = (int) def.TotalOut;

			wrtr.Write((int)m_crc.Value);
			wrtr.Write(m_sizeDir);
			wrtr.Write(m_sizeName);
			wrtr.Write(m_numFiles);

			byte[] buf = new byte[16];
			Array.Copy(filemem.GetBuffer(), 0, buf, 0, 16);

			for(byte i = 0; i < 16; i++)
			{
				buf[i] ^= i;
			}

			using(wrtr = new BinaryWriter(File.Open(fname, FileMode.Create, FileAccess.Write), System.Text.Encoding.UTF8))
			{
				wrtr.Write(MAGIC);
				wrtr.Write((byte)2);
				wrtr.Write(buf, 0, 16);
				wrtr.Write(name, 0, m_sizeName);
				wrtr.Write(dir, 0, m_sizeDir);

				foreach(MPKFile file in files)
				{
					wrtr.Write(file.CompressedData);
				}

				wrtr.BaseStream.Flush();
				wrtr.Close();
			}
		}

		/// <summary>
		/// Extracts all files from this MPK into a directory
		/// </summary>
		/// <param name="dirname">The directory where to put the files</param>
		/// <param name="fname">The MPK file to extract</param>
		public void Extract(string dirname, string fname)
		{
			Read(fname);
			Extract(dirname);
		}

		/// <summary>
		/// Extracts all files from this MPK
		/// </summary>
		/// <param name="dirname">The directory where to put the files</param>
		public void Extract(string dirname)
		{
			if(!Directory.Exists(dirname))
			{
				Directory.CreateDirectory(dirname);
			}

			if(!dirname.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				dirname += Path.DirectorySeparatorChar;
			}

			foreach(MPKFile file in m_files.Values)
			{
				file.Save(dirname);
			}
		}
				
		/// <summary>
		/// Reads a MPK file
		/// </summary>
		/// <param name="fname">The MPK filename to read</param>
		public void Read(string fname)
		{
			using(BinaryReader rdr = new BinaryReader(File.OpenRead(fname), System.Text.Encoding.UTF8))
			{
				if(rdr.ReadUInt32() != MAGIC)
				{
					throw new Exception("Invalid MPK file");
				}

				rdr.ReadByte(); //always 2

				ReadArchive(rdr);
				rdr.Close();
			}
		}

		/// <summary>
		/// Reads a MPK from a binary reader
		/// </summary>
		/// <param name="rdr">The binary reader pointing to the MPK</param>
		protected void ReadArchive(BinaryReader rdr)
		{
			m_files.Clear();

			m_crc.Value = 0;
			m_sizeDir = 0;
			m_sizeName = 0;
			m_numFiles = 0;

			byte[] buf = new byte[16];
			rdr.Read(buf, 0, 16);

			for(byte i = 0; i < 16; ++i)
			{
				buf[i] ^= i;
			}

			m_crc.Value = (long)((buf[0] << 24) | (buf[1] << 16) | (buf[2] << 8) | buf[3]);
			m_sizeDir = ((buf[4] << 24) | (buf[5] << 16) | (buf[6] << 8) | buf[7]);
			m_sizeName = ((buf[8] << 24) | (buf[9] << 16) | (buf[10] << 8) | buf[11]);
			m_numFiles = ((buf[12] << 24) | (buf[13] << 16) | (buf[14] << 8) | buf[15]);

			buf = new byte[m_sizeName];
			rdr.Read(buf, 0, m_sizeName);
			
			Inflater inf = new Inflater();
			inf.SetInput(buf);
			buf = new byte[1024];
			inf.Inflate(buf);
			buf[inf.TotalOut] = 0;
			m_name = Marshal.ConvertToString(buf);
			long totalin = 0;
			buf = ReadDirectory(rdr, ref totalin);
			MemoryStream directory = new MemoryStream(buf);
				
			long pos = rdr.BaseStream.Position;
			long len = rdr.BaseStream.Seek(0, SeekOrigin.End);
			rdr.BaseStream.Position = pos;
			buf = new byte[len - pos];
			rdr.Read(buf, 0, buf.Length);
			MemoryStream files = new MemoryStream(buf);

			rdr.BaseStream.Position = pos - totalin;
			buf = new byte[totalin];
			rdr.Read(buf, 0, buf.Length);

			Crc32 crc = new Crc32();
			crc.Reset();
			crc.Update(buf);

			if(crc.Value != m_crc.Value)
			{
				throw new Exception("Invalid or corrupt MPK");
			}

			while(directory.Position < directory.Length && files.Position < files.Length)
			{
				crc.Reset();
				buf = new byte[MPKFileHeader.MAX_SIZE];
				directory.Read(buf, 0, MPKFileHeader.MAX_SIZE);
				MPKFileHeader hdr = new MPKFileHeader(new BinaryReader(new MemoryStream(buf), System.Text.Encoding.UTF8));

				byte[] compbuf = new byte[hdr.CompressedSize];
				files.Read(compbuf, 0, compbuf.Length);

				crc.Update(compbuf, 0, compbuf.Length);

				inf.Reset();
				inf.SetInput(compbuf, 0, compbuf.Length);
				buf = new byte[hdr.Size];
				inf.Inflate(buf, 0, buf.Length);

				MPKFile file = new MPKFile(compbuf, buf, hdr);
				

				if(crc.Value != hdr.CRC.Value)
				{
					OnInvalidFile(file);
					continue;
				}

				m_files.Add(hdr.Name.ToLower(), file);
			}
		}

		byte[] ReadDirectory(BinaryReader rdr, ref long totalin)
		{
			int totalout = 0;
			byte[] input = new byte[1024];
			byte[] output = new byte[1024];
			Inflater inf = new Inflater();
			totalin = 0;

			while(!inf.IsFinished)
			{
				while(inf.IsNeedingInput)
				{
					int count = 0;
					if((count = rdr.Read(input, 0, 1024)) <= 0)
					{
						throw new Exception("EOF");
					}

					inf.SetInput(input, 0, count);
					totalin += count;
				}

				if(totalout == output.Length)
				{
					byte[] newOutput = new byte[output.Length * 2];
					Array.Copy(output, newOutput, output.Length);
					output = newOutput;
				}

				totalout += inf.Inflate(output, totalout, output.Length - totalout);
			}

			byte[] final = new byte[totalout];
			Array.Copy(output, 0, final, 0, totalout);
			rdr.BaseStream.Position = rdr.BaseStream.Position - inf.RemainingInput;
			totalin -= inf.RemainingInput;
			return final;
		}

		/// <summary>
		/// Displays debug information about this MPK
		/// </summary>
		public void Display()
		{
			Console.WriteLine("**************************************************************************");
			Console.WriteLine(this.m_name);
			Console.WriteLine("{0} files", this.m_numFiles);
			Console.WriteLine("{0} actual files", m_files.Count);
			Console.WriteLine("**************************************************************************");
			foreach(MPKFile file in m_files.Values)
			{
				file.Display();
			}
		}

		void OnInvalidFile(MPKFile file)
		{
			if(InvalidFile != null)
			{
				InvalidFile(file, new EventArgs());
			}
		}

		/// <summary>
		/// Creates a new MPK file
		/// </summary>
		/// <param name="fname">The mpk filename</param>
		public void Create(string fname)
		{
			m_files.Clear();
			m_name = fname;
		}
	}
}
