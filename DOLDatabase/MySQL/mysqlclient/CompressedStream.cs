// Copyright (C) 2004-2005 MySQL AB
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License version 2 as published by
// the Free Software Foundation
//
// There are special exceptions to the terms and conditions of the GPL 
// as it is applied to this software. View the full text of the 
// exception in file EXCEPTIONS in the directory of this software 
// distribution.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using MySql.Data.Common;

namespace MySql.Data.MySqlClient
{
	/// <summary>
	/// Summary description for CompressedStream.
	/// </summary>
	internal class CompressedStream : Stream
	{
		// writing fields
		private	Stream					baseStream;
		private MemoryStream			cache;
		private int						numWritten;
		private int						expecting;

		// reading fields
		private byte[]					buffer;
		private	int						index;

		public CompressedStream( Stream baseStream )
		{
			this.baseStream = baseStream;
			cache = new MemoryStream();

			buffer = new byte[0];
		}

		#region Properties

		//TODO: remove comment
/*		public Stream BaseStream 
		{
			get { return baseStream; }
		}
*/
		public override bool CanRead
		{
			get	{ return baseStream.CanRead; }
		}

		public override bool CanWrite
		{
			get	{ return baseStream.CanWrite; }
		}

		public override bool CanSeek
		{
			get	{ return baseStream.CanSeek; }
		}

		public override long Length
		{
			get { return baseStream.Length; }
		}

		public override long Position
		{
			get	{ return baseStream.Position; }
			set	{ baseStream.Position = value; }
		}
		#endregion

		public override void Close()
		{
			baseStream.Close();
			base.Close ();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException(Resources.GetString("CSNoSetLength"));
		}

		public override int ReadByte()
		{
			EnsureData(1);

			return (int)buffer[index++];
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException("buffer", 
					Resources.GetString("BufferCannotBeNull"));
			if (offset < 0 || offset >= buffer.Length)
				throw new ArgumentOutOfRangeException("offset", 
					Resources.GetString("OffsetMustBeValid"));
			if ((offset + count) > buffer.Length)
				throw new ArgumentException(Resources.GetString("BufferNotLargeEnough"),
					"buffer");

			EnsureData(count);

			Array.Copy(this.buffer, index, buffer, offset, count);
			index += count;

			return count;
		}

		private byte[] CompressData( byte[] buff, int offset, int count )
		{
			MemoryStream ms = new MemoryStream();
			DeflaterOutputStream	deflater;
			deflater = new DeflaterOutputStream( ms );

			byte[] cacheBuff = cache.GetBuffer();

			byte seq = cacheBuff[3];
			cacheBuff[3] = 0;

			deflater.Write( cacheBuff, 0, (int)cache.Length );
			if ( count > 0 )
				deflater.Write( buff, offset, count );
			deflater.Finish();

			cacheBuff[3] = seq;

			long unCompLen = cache.Length + count;

			if ( ms.Length >= unCompLen )
				return null;
			return ms.ToArray();
		}

		public override void Flush() 
		{
			baseStream.Flush();
		}

		private bool InputDone() 
		{
			// if we have not done so yet, see if we can calculate how many bytes we are expecting
			if (expecting == 0)
			{
				byte[] buf = cache.GetBuffer();
				expecting = buf[0] + (buf[1] << 8) + (buf[2] << 16);
			}
			return numWritten == (expecting+4);
		}

		private void FlushData(byte[] buff, int offset, int count)
		{
			// if we have already flushed, then just return
			if (cache.Length < 4) return;

			if (! InputDone()) return;

			byte[] compressedData = CompressData(buff, offset, count);

			int comp_len = compressedData == null ? numWritten : (int)compressedData.Length;
			int ucomp_len = compressedData == null ? 0 : numWritten;
			byte[] cacheBuff = cache.GetBuffer();

			baseStream.WriteByte( (byte)(comp_len & 0xff) );
			baseStream.WriteByte( (byte)((comp_len >> 8) & 0xff) );
			baseStream.WriteByte( (byte)((comp_len >> 16) & 0Xff) );
			baseStream.WriteByte( cacheBuff[3] );
			baseStream.WriteByte( (byte)(ucomp_len & 0xff) );
			baseStream.WriteByte( (byte)((ucomp_len >> 8) & 0xff) );
			baseStream.WriteByte( (byte)((ucomp_len >> 16) & 0Xff) );

			if (ucomp_len == 0) 
			{
				cacheBuff[3] = 0;

				baseStream.Write( cacheBuff, 0, (int)cache.Length );

				if (count > 0) 
					baseStream.Write( buff, offset, count );
			}
			else
				baseStream.Write( compressedData, 0, compressedData.Length );

			baseStream.Flush();

			cache.SetLength(0);
			expecting = numWritten = 0;
		}



		public override void WriteByte(byte value)
		{
			cache.WriteByte( value );
			numWritten++;
			FlushData( null, 0, 0);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			numWritten += count;
			if (! InputDone())
				cache.Write( buffer, offset, count );
			else
				FlushData( buffer, offset, count );
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return baseStream.Seek( offset, origin );
		}

		private static void ReadBuffer(Stream s, byte[] buf, int offset, int length)
		{
			while (length > 0)
			{
				int amountRead = s.Read(buf, offset, length);
				if (amountRead == 0)
				throw new MySqlException("Unexpected end of data encountered");
				length -= amountRead;
				offset += amountRead;
			}
		}

		private void ReadCompressedBuffer( byte[] buf, int index, int compLen, int unCompLen )
		{
			byte[] compBuf = new byte[ compLen ];
			ReadBuffer( baseStream, compBuf, 0, compLen );
			Inflater i = new Inflater();
			i.SetInput( compBuf, 0, compLen );
			int count = i.Inflate( buf, index, unCompLen );
		}

		private void ReadNextPacket()
		{
			// read off the uncompressed and compressed lengths
			byte b1 = (byte)baseStream.ReadByte();
			byte b2 = (byte)baseStream.ReadByte();
			byte b3 = (byte)baseStream.ReadByte();
			int compressedLen = b1 + (b2 << 8) + (b3 << 16);

//			int compressedLen = baseStream.ReadByte() + (baseStream.ReadByte() << 8) + 
//				(baseStream.ReadByte() << 16);
			byte seq = (byte)baseStream.ReadByte();
			int unCompressedLen = baseStream.ReadByte() + (baseStream.ReadByte() << 8) + 
				(baseStream.ReadByte() << 16);

			// if the data is in fact compressed, then uncompress it
			byte[] unCompressedBuffer = null;
			if (unCompressedLen > 0) 
			{
				unCompressedBuffer = new byte[ unCompressedLen ];
				ReadCompressedBuffer( unCompressedBuffer, 0, compressedLen, unCompressedLen );
			}
			else 
			{
				unCompressedBuffer = new byte[ compressedLen ];
				ReadBuffer( baseStream, unCompressedBuffer, 0, compressedLen );
			}

			// now join this buffer to our existing one
			int left = buffer.Length - index;
			byte[] newBuffer = new byte[ left + unCompressedBuffer.Length ];

			int newIndex = 0;
			// first copy in the rest of the original
			for (int i=index; i < buffer.Length; i++)
				newBuffer[newIndex++] = buffer[i];
			unCompressedBuffer.CopyTo( newBuffer, newIndex );
			buffer = newBuffer;
			index = 0;
		}

		private void EnsureData( int size )
		{
			while ((buffer.Length - index) < size)
				ReadNextPacket();
		}
	}
}
