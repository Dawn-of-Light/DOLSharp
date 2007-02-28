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
using System.Text;
using MySql.Data.Common;

namespace MySql.Data.MySqlClient
{
	/// <summary>
	/// Summary description for PacketWriter.
	/// </summary>
	internal class PacketWriter
	{
		private Stream			stream;
		private MemoryStream	buffStream;
		private Stream			nativeStream;
		private Encoding		encoding;
		private NativeDriver	driver;
		private long			leftThisPacket=0;
		private long 			leftToWrite = 0;
		private DBVersion		version;

		public PacketWriter() 
		{
			buffStream = new MemoryStream();
			Buffering = true;
		}

		public PacketWriter( Stream stream, NativeDriver driver ) : this()
		{
			this.driver = driver;
			nativeStream = stream;
			if (stream is CompressedStream)
				Buffering = true;
			else
				Buffering = false;
		}

		public DBVersion Version 
		{
			get { return version; }
			set { version = value; }
		}

		public NativeDriver Driver 
		{
			get { return driver; }
			set { driver = value; }
		}

		public bool Buffering
		{
			get { return stream is MemoryStream; }
			set { stream = value ? buffStream : nativeStream; }
		}

		public Stream Stream 
		{
			get { return stream; }
		}

		public Encoding Encoding 
		{ 
			get { return encoding; }
			set { encoding = value; }
		}

		private void WriteStartBlock( long len ) 
		{
			long maxSinglePacket = driver.MaxSinglePacket;
			if (stream is CompressedStream)
				maxSinglePacket -= 4;

			leftThisPacket = Math.Min( maxSinglePacket, len );

			stream.WriteByte( (byte)(leftThisPacket & 0xff) );
			stream.WriteByte( (byte)((leftThisPacket >> 8) & 0xff) );
			stream.WriteByte( (byte)((leftThisPacket >> 16) & 0xff) );
			stream.WriteByte( driver.SequenceByte++ );
		}


		public void StartPacket( long len ) 
		{
			Buffering = (len == 0);
			if (Buffering) 
			{
				buffStream.SetLength(0);
			}
			else 
			{
				WriteStartBlock( len );
				leftToWrite = len;
			}
		}

		private void FlushBuffer() 
		{
			long len = buffStream.Length;
			byte[] bytes = buffStream.GetBuffer();
			long pos = 0;

			while (len > 0) 
			{
				int toWrite = Math.Min( driver.MaxSinglePacket, (int)len );

				nativeStream.WriteByte( (byte)(toWrite & 0xff) );
				nativeStream.WriteByte( (byte)((toWrite >> 8) & 0xff) );
				nativeStream.WriteByte( (byte)((toWrite >> 16) & 0xff) );
				nativeStream.WriteByte( driver.SequenceByte++ );
				nativeStream.Write( bytes, (int)pos, toWrite );
				nativeStream.Flush();
				len -= toWrite;
				pos += toWrite;
			}
			Buffering = true;
		}

		public void Flush() 
		{
			if (Buffering) 
			{
				if (buffStream.Length > driver.MaxPacketSize)
					throw new MySqlException("Packet size too large.  This MySQL server cannot accept rows larger than " + 
						driver.MaxPacketSize + " bytes.");
					
				FlushBuffer();
			}

			nativeStream.Flush();
		}

		public void WriteByte( byte b ) 
		{
			stream.WriteByte( b );
			leftToWrite --;
			leftThisPacket --;
		}

		public void Write( byte[] buffer ) 
		{
			Write( buffer, 0, buffer.Length );
		}

		public void WriteChunk( byte[] buf, int offset, int length ) 
		{
			while (length > 0) 
			{
				long toWrite = Math.Min( leftThisPacket, Math.Min( (long)length, leftToWrite  ) );
				stream.Write(buf, (int)offset, (int)toWrite);
				stream.Flush();
				leftThisPacket -= toWrite;
				offset += (int)toWrite;
				leftToWrite -= toWrite;
				if (leftThisPacket == 0 && leftToWrite > 0)
					WriteStartBlock( leftToWrite );
				length -= (int)toWrite;
			}
		}

		public void Write( byte[] buffer, int offset, int count ) 
		{
			try 
			{
				if (! Buffering)
					WriteChunk( buffer, offset, count );
				else
					stream.Write(buffer, offset, count);
			}
			catch (Exception ex)
			{
				Logger.LogException(ex);
				throw new MySqlException("Unable to write to stream", true, ex);
			}
		}

		public void WriteLength(long length) 
		{
			if (length < 251)
				WriteByte( (byte)length );
			else if ( length < 65536L )
			{
				WriteByte( 252 );
				WriteInteger( length, 2 );
			}
			else if ( length < 16777216L )
			{
				WriteByte( 253 );
				WriteInteger( length, 3 );
			}
			else 
			{
				WriteByte( 254 );
				WriteInteger( length, 4 );
			}
		}

		/// <summary>
		/// WriteInteger
		/// </summary>
		/// <param name="v"></param>
		/// <param name="numbytes"></param>
		public void WriteInteger( long v, int numbytes )
		{
			long val = v;

			if (numbytes < 1 || numbytes > 4) 
				throw new ArgumentOutOfRangeException("Wrong byte count for WriteInteger");

			for (int x=0; x < numbytes; x++)
			{
				stream.WriteByte( (byte)(val&0xff) );
				val >>= 8;
			}
			leftToWrite -= numbytes;
		}

		public void WriteLenString( string s )
		{
			byte[] bytes = encoding.GetBytes(s);
			WriteLength( bytes.Length );
			stream.Write( bytes, 0,bytes.Length );
		}

		public void WriteString(string v )
		{
			WriteStringNoNull( v );
			stream.WriteByte(0);
		}
 
		public void WriteStringNoNull(string v )
		{
			byte[] bytes = encoding.GetBytes(v);
			stream.Write(bytes, 0, bytes.Length);
		}

	}
}
