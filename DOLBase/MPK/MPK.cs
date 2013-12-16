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
using System.Collections.Generic;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace DOL.MPK
{
	/// <summary>
	/// Handles the reading and writing to MPK files.
	/// </summary>
	public class MPK
	{
		/// <summary>
		/// The magic at the top of the file
		/// </summary>
		private const uint Magic = 0x4b41504d; //MPAK

		/// <summary>
		/// CRC32 of the deflated directory
		/// </summary>
		private readonly Crc32 _crc = new Crc32();

		/// <summary>
		/// Holds all of the files in the MPK
		/// </summary>
		private readonly Dictionary<string, MPKFile> _files = new Dictionary<string, MPKFile>();

		/// <summary>
		/// Name of the archive
		/// </summary>
		private string _name = "";

		/// <summary>
		/// Number of files in the directory
		/// </summary>
		private int _numFiles;

		/// <summary>
		/// Compressed size of the directory section
		/// </summary>
		private int _sizeDir;

		/// <summary>
		/// Compressed size of the name section
		/// </summary>
		private int _sizeName;

		/// <summary>
		/// Creates a new MPK file
		/// </summary>
		/// <param name="fname">The filename</param>
		/// <param name="create">if true, creates the file, else parses an existing file</param>
		public MPK(string fname, bool create)
		{
			if (!create)
			{
				Read(fname);
			}
			else
			{
				_name = fname;
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
			get { return _name; }
			set { _name = value; }
		}

		/// <summary>
		/// The CRC of the MPK file
		/// </summary>
		public Crc32 CRC
		{
			get { return _crc; }
		}

		/// <summary>
		/// The directory size of this MPK
		/// </summary>
		public int DirectorySize
		{
			get { return _sizeDir; }
		}

		/// <summary>
		/// The filecount in this MPK
		/// </summary>
		public int Count
		{
			get { return _files.Count; }
		}

		/// <summary>
		/// Gets a specific MPK file from this MPK
		/// </summary>
		public MPKFile this[string fname]
		{
			get
			{
				if (_files.ContainsKey(fname.ToLower()))
				{
					return _files[fname.ToLower()];
				}

				return null;
			}
		}

		/// <summary>
		/// The event to fire if an invalid file was found
		/// </summary>
		public event EventHandler InvalidFile;

		/// <summary>
		/// Gets a list of all the files inside this MPK
		/// </summary>
		/// <returns>An IDictionaryEnumerator containing entries as filename, MPKFile pairs</returns>
		public IEnumerator<KeyValuePair<string, MPKFile>> GetEnumerator()
		{
			return _files.GetEnumerator();
		}

		/// <summary>
		/// Adds a file to the MPK
		/// </summary>
		/// <param name="file">The file to add</param>
		/// <returns>true if successfull, false if the file is already contained</returns>
		public bool AddFile(MPKFile file)
		{
			if (!_files.ContainsKey(file.Header.Name))
			{
				_files.Add(file.Header.Name, file);
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
			if (_files.ContainsKey(fname.ToLower()))
			{
				_files.Remove(fname.ToLower());
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
			Write(Name);
		}

		/// <summary>
		/// Writes the MPK to a specific filename
		/// </summary>
		/// <param name="fname"></param>
		public void Write(string fname)
		{
			Deflater def;
			byte[] buf, dir, name;
			var files = new MPKFile[_files.Count];

			using (var dirmem = new MemoryStream(_files.Count*MPKFileHeader.MaxSize))
			{
				int index = 0;
				uint offset = 0;
				uint diroffset = 0;

				foreach (MPKFile file in _files.Values)
				{
					file.Header.DirectoryOffset = diroffset;
					file.Header.Offset = offset;
					files[index] = file;

					using (var wrtr = new BinaryWriter(dirmem, Encoding.UTF8))
					{
						file.Header.Write(wrtr);
					}

					offset += file.Header.UncompressedSize;
					diroffset += file.Header.CompressedSize;
					index++;
				}

				def = new Deflater();
				def.SetInput(dirmem.GetBuffer(), 0, (int) dirmem.Position);
				def.Finish();

				dir = new byte[dirmem.Position];
				def.Deflate(dir);
				_sizeDir = (int) def.TotalOut;
			}

			def = new Deflater();

			_crc.Reset();
			_crc.Update(dir, 0, _sizeDir);

			def.SetInput(Encoding.UTF8.GetBytes(_name));
			def.Finish();

			name = new byte[_name.Length];
			def.Deflate(name);

			_numFiles = _files.Count;
			_sizeName = (int) def.TotalOut;

			using (var filemem = new MemoryStream())
			{
				using (var wrtr = new BinaryWriter(filemem, Encoding.UTF8))
				{
					wrtr.Write((int) _crc.Value);
					wrtr.Write(_sizeDir);
					wrtr.Write(_sizeName);
					wrtr.Write(_numFiles);

					buf = new byte[16];
					Buffer.BlockCopy(filemem.GetBuffer(), 0, buf, 0, 16);

					for (byte i = 0; i < 16; i++)
					{
						buf[i] ^= i;
					}
				}
			}

			using (FileStream fileStream = File.Open(fname, FileMode.Create, FileAccess.Write))
			{
				using (var wrtr = new BinaryWriter(fileStream, Encoding.UTF8))
				{
					wrtr.Write(Magic);
					wrtr.Write((byte) 2);
					wrtr.Write(buf, 0, 16);
					wrtr.Write(name, 0, _sizeName);
					wrtr.Write(dir, 0, _sizeDir);

					foreach (MPKFile file in files)
					{
						wrtr.Write(file.CompressedData);
					}
				}
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
			if (!Directory.Exists(dirname))
			{
				Directory.CreateDirectory(dirname);
			}

			if (!dirname.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				dirname += Path.DirectorySeparatorChar;
			}

			foreach (MPKFile file in _files.Values)
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
			using (var rdr = new BinaryReader(File.OpenRead(fname), Encoding.UTF8))
			{
				if (rdr.ReadUInt32() != Magic)
				{
					throw new Exception("Invalid MPK file");
				}

				rdr.ReadByte(); //always 2

				ReadArchive(rdr);
			}
		}

		/// <summary>
		/// Reads a MPK from a binary reader
		/// </summary>
		/// <param name="rdr">The binary reader pointing to the MPK</param>
		private void ReadArchive(BinaryReader rdr)
		{
			_files.Clear();

			_crc.Value = 0;
			_sizeDir = 0;
			_sizeName = 0;
			_numFiles = 0;

			var buf = new byte[16];
			rdr.Read(buf, 0, 16);

			for (byte i = 0; i < 16; ++i)
			{
				buf[i] ^= i;
			}

			_crc.Value = ((buf[0] << 24) | (buf[1] << 16) | (buf[2] << 8) | buf[3]);
			_sizeDir = ((buf[4] << 24) | (buf[5] << 16) | (buf[6] << 8) | buf[7]);
			_sizeName = ((buf[8] << 24) | (buf[9] << 16) | (buf[10] << 8) | buf[11]);
			_numFiles = ((buf[12] << 24) | (buf[13] << 16) | (buf[14] << 8) | buf[15]);

			buf = new byte[_sizeName];
			rdr.Read(buf, 0, _sizeName);

			var inf = new Inflater();
			inf.SetInput(buf);
			buf = new byte[1024];
			inf.Inflate(buf);
			buf[inf.TotalOut] = 0;

			_name = Marshal.ConvertToString(buf);

			long totalin = 0;
			buf = ReadDirectory(rdr, ref totalin);

			using (var directory = new MemoryStream(buf))
			{
				long pos = rdr.BaseStream.Position;
				long len = rdr.BaseStream.Seek(0, SeekOrigin.End);
				rdr.BaseStream.Position = pos;

				buf = new byte[len - pos];
				rdr.Read(buf, 0, buf.Length);

				using (var files = new MemoryStream(buf))
				{
					rdr.BaseStream.Position = pos - totalin;
					buf = new byte[totalin];
					rdr.Read(buf, 0, buf.Length);

					var crc = new Crc32();
					crc.Reset();
					crc.Update(buf);

					if (crc.Value != _crc.Value)
					{
						throw new Exception("Invalid or corrupt MPK");
					}

					while (directory.Position < directory.Length && files.Position < files.Length)
					{
						crc.Reset();

						buf = new byte[MPKFileHeader.MaxSize];
						directory.Read(buf, 0, MPKFileHeader.MaxSize);

						MPKFileHeader hdr;

						using (var hdrStream = new MemoryStream(buf))
						{
							using (var hdrRdr = new BinaryReader(hdrStream, Encoding.UTF8))
							{
								hdr = new MPKFileHeader(hdrRdr);
							}
						}

						var compbuf = new byte[hdr.CompressedSize];
						files.Read(compbuf, 0, compbuf.Length);

						crc.Update(compbuf, 0, compbuf.Length);

						inf.Reset();
						inf.SetInput(compbuf, 0, compbuf.Length);
						buf = new byte[hdr.UncompressedSize];
						inf.Inflate(buf, 0, buf.Length);

						var file = new MPKFile(compbuf, buf, hdr);

						if (crc.Value != hdr.CRC.Value)
						{
							OnInvalidFile(file);
							continue;
						}

						_files.Add(hdr.Name.ToLower(), file);
					}
				}
			}
		}

		private static byte[] ReadDirectory(BinaryReader rdr, ref long totalin)
		{
			int totalout = 0;
			var input = new byte[1024];
			var output = new byte[1024];
			var inf = new Inflater();
			totalin = 0;

			while (!inf.IsFinished)
			{
				while (inf.IsNeedingInput)
				{
					int count;
					if ((count = rdr.Read(input, 0, 1024)) <= 0)
					{
						throw new Exception("EOF");
					}

					inf.SetInput(input, 0, count);
					totalin += count;
				}

				if (totalout == output.Length)
				{
					var newOutput = new byte[output.Length*2];
					Buffer.BlockCopy(output, 0, newOutput, 0, output.Length);
					output = newOutput;
				}

				totalout += inf.Inflate(output, totalout, output.Length - totalout);
			}

			var final = new byte[totalout];
			Buffer.BlockCopy(output, 0, final, 0, totalout);
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
			Console.WriteLine(_name);
			Console.WriteLine("{0} files", _numFiles);
			Console.WriteLine("{0} actual files", _files.Count);
			Console.WriteLine("**************************************************************************");
			foreach (MPKFile file in _files.Values)
			{
				file.Display();
			}
		}

		private void OnInvalidFile(MPKFile file)
		{
			if (InvalidFile != null)
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
			_files.Clear();
			_name = fname;
		}
	}
}