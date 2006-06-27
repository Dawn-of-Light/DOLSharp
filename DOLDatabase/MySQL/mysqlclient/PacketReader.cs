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

namespace MySql.Data.MySqlClient
{
	/// <summary>
	/// Summary description for PacketWriter.
	/// </summary>
	internal class PacketReader
	{
		public static int		NULL_LEN=-1;
		private Stream			stream;
		private long			packetLength;
		private bool			isLastPacket;
		private NativeDriver	driver;
		private int				firstByte;
		private byte[]			buffer;
		private Encoding		encoding;
		private long			bytesLeft;

		public PacketReader(Stream stream, NativeDriver driver) 
		{
			packetLength = -1;
			isLastPacket = false;
			this.driver = driver;
			this.stream = stream;
			firstByte = -1;
			buffer = new byte[1024];
			encoding = driver.Encoding;
		}

		#region Properties

		public MySql.Data.Common.DBVersion Version 
		{
			get { return driver.Version; }
		}

		public int Length 
		{
			get { return (int)packetLength; }
		}

		public Encoding Encoding 
		{
			get { return encoding; }
			set { encoding = value; }
		}

		public Stream Stream 
		{
			get { return stream; }
			set { stream = value; }
		}

		public bool IsLastPacket 
		{
			get { return isLastPacket; }
		}

		public bool HasMoreData 
		{
			get { return bytesLeft > 0; }
		}

//		public bool IsLoadDataLocal 
//		{
//			get { return firstByte == 254 && packetLength >= 9; }
//		}

#endregion

		private void ReadHeader() 
		{
			int b1 = stream.ReadByte();
			int b2 = stream.ReadByte();
			int b3 = stream.ReadByte();
			int seq = stream.ReadByte();
			if (b1 == -1 || b2 == -1 || b3 == -1 || seq == -1)
				throw new MySqlException( "Connection unexpectedly terminated", true, null );

			packetLength = b1 + (b2 << 8) + (b3 << 16);
			bytesLeft = packetLength;

			(driver as NativeDriver).SequenceByte = (byte)(seq + 1);
			if (packetLength > 0) 
			{
				firstByte = stream.ReadByte();
				if (firstByte == -1)
					throw new MySqlException( "Connection unexpectedly terminated", true, null );
			}

			encoding = driver.Encoding;
			isLastPacket = (packetLength < 9 && firstByte == 0xfe);
			if (! isLastPacket)
				CheckForError();
		}

		public void OpenPacket() 
		{
			// if we have not read all of the last packet, then read it off now
			// we use skip so we don't create unneccessary buffers
			while (bytesLeft > 0 || packetLength == driver.MaxSinglePacket ) 
			{
				if (bytesLeft == 0 && packetLength == driver.MaxSinglePacket)
					ReadHeader();
				Skip( bytesLeft );
			}

			ReadHeader();
		}

		public bool ReadOk() 
		{
			OpenPacket();
			bool isOk = firstByte == 0 && packetLength > 1;
			Skip( packetLength ) ;
			return isOk;
		}

		private void CheckForError() 
		{
			if (firstByte == 0xff)
			{
				ReadByte();
				int errorCode = ReadInteger(2);
				string msg = ReadString();
				throw new MySqlException( msg, errorCode );
			}

		}

		public void Skip( long count ) 
		{
			while (count > 0) 
			{
				int cntRead = Read( ref buffer, 0, count > buffer.Length ? buffer.Length : count );
				count -= cntRead;
			}
		}

		public int ReadByte() 
		{
			bytesLeft --;
			
			if (firstByte != -1) 
			{
				int b = firstByte;
				firstByte = -1;
				return b;
			}

			int theByte = stream.ReadByte();
			if (theByte == -1)
				throw new MySqlException( "Connection unexpectedly terminated", true, null );
			return theByte;
		}

		public long GetFieldLength() 
		{
			byte c  = (byte)ReadByte();

			switch(c) 
			{
				case 251 : return (long)NULL_LEN; 
				case 252 : return (long)ReadInteger(2);
				case 253 : return (long)ReadInteger(3);
				case 254 : return (long)ReadInteger(8);
				default  : return c;
			}
		}

		public int ReadNBytes()
		{
			byte c = (byte)ReadByte();
			if (c < 1 || c > 4) throw new MySqlException("Unexpected byte count received");
			return ReadInteger((int)c);
		}

		public int Read(ref byte[] buffer, long pos, long len) 
		{
			if (buffer == null || buffer.Length < len)
				buffer = new byte[ pos + len ];

			try 
			{
				long totalLen = len;
				while (len > 0) 
				{
					// if we are in a multipacket, then read the next section
					if (bytesLeft == 0 && packetLength == driver.MaxSinglePacket)
						ReadHeader();

					if (firstByte != -1) 
					{
						buffer[pos++] = (byte)ReadByte();
						len --;
					}
					else 
					{
						int lenToRead = (int)Math.Min( len, bytesLeft );
						int count = stream.Read(buffer, (int)pos, lenToRead);
						if (count == 0)
							throw new MySqlException( "Connection unexpectedly terminated", true, null );

						len -= count;
						pos += count;
						bytesLeft -= count;
					}
				}

				return (int)totalLen;
			}
			catch (Exception ex) 
			{
				Logger.LogException(ex) ;
				throw new MySqlException("Connection unexpectedly terminated", true, ex);
			}
		}

		public ulong ReadLong( int numbytes ) 
		{
			ulong val = 0;
			int raise = 1;
			for (int x=0; x < numbytes; x++)
			{
				int b = ReadByte();
				val += (ulong)(b*raise);
				raise *= 256;
			}
			return val;
		}

		public int ReadInteger(int numbytes)
		{
			return (int)ReadLong( numbytes );
		}

		public string ReadString() 
		{
			MemoryStream ms = new MemoryStream();

			while (HasMoreData)
			{
				int b = ReadByte();
				if (b == 0) break;
				ms.WriteByte((byte)b);
			}

			return Encoding.GetString( ms.GetBuffer(), 0, (int)ms.Length );
		}

		public int ReadPackedInteger()
		{
			byte c  = (byte)ReadByte();

			switch(c) 
			{
				case 251 : return NULL_LEN; 
				case 252 : return ReadInteger(2);
				case 253 : return ReadInteger(3);
				case 254 : return ReadInteger(4);
				default  : return c;
			}
		}

		public string ReadString( long length ) 
		{
			if (length == 0) return String.Empty;

			byte[] myBuff = buffer;
			if (length > myBuff.Length)
				myBuff = new byte[ length ];

			Read( ref myBuff, 0, length );
			return Encoding.GetString( myBuff, 0, (int)length );
		}

		public string ReadLenString()
		{
			long len = ReadPackedInteger();
			return ReadString( len );
		}

	}
}
