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
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;

namespace MySql.Data.MySqlClient
{
	/// <summary>
	/// Summary description for SharedMemoryStream.
	/// </summary>
	internal class SharedMemoryStream : Stream
	{
		private string			memoryName;
		private AutoResetEvent	serverRead;
		private AutoResetEvent	serverWrote;
		private AutoResetEvent	clientRead;
		private AutoResetEvent	clientWrote;
		private IntPtr			dataMap;
		private IntPtr			dataView;
		private int				bytesLeft;
		private int				position;
		private int				connectNumber;

		private uint	EVENT_ALL_ACCESS = 0x001F0003;
		private uint	FILE_MAP_WRITE = 0x2;
		private int		BUFFERLENGTH = 16004;

		public SharedMemoryStream(string memName)
		{
			memoryName = memName;
		}

		public void Open(int timeOut)
		{
			GetConnectNumber(timeOut);
			SetupEvents();
		}

		public override void Close() 
		{
			UnmapViewOfFile( dataView );
			CloseHandle( dataMap );
		}

		private void GetConnectNumber(int timeOut)
		{
			AutoResetEvent connectRequest = new AutoResetEvent(false);
			connectRequest.Handle = OpenEvent( EVENT_ALL_ACCESS, false, 
				memoryName + "_" + "CONNECT_REQUEST" );

			AutoResetEvent connectAnswer = new AutoResetEvent(false);
			connectAnswer.Handle = OpenEvent( EVENT_ALL_ACCESS, false, 
				memoryName + "_" + "CONNECT_ANSWER" );

			IntPtr connectFileMap = OpenFileMapping( FILE_MAP_WRITE, false,
				memoryName + "_" + "CONNECT_DATA" );
			IntPtr connectView = MapViewOfFile( connectFileMap, FILE_MAP_WRITE,
				0, 0, (UIntPtr)4 );

			// now start the connection
			if (! connectRequest.Set())
				throw new MySqlException( "Failed to open shared memory connection " );

			connectAnswer.WaitOne( timeOut*1000, false );

			connectNumber = Marshal.ReadInt32( connectView );
		}

		private void SetupEvents()
		{
			string dataMemoryName = memoryName + "_" + connectNumber;
			dataMap = OpenFileMapping( FILE_MAP_WRITE, false, 
				dataMemoryName + "_DATA" );
			dataView = MapViewOfFile( dataMap, FILE_MAP_WRITE, 0, 0, (UIntPtr)BUFFERLENGTH );

			serverWrote = new AutoResetEvent(false);
			serverWrote.Handle = OpenEvent( EVENT_ALL_ACCESS, false, 
				dataMemoryName + "_SERVER_WROTE" );

			serverRead = new AutoResetEvent(false);
			serverRead.Handle = OpenEvent( EVENT_ALL_ACCESS, false, 
				dataMemoryName + "_SERVER_READ" );

			clientWrote = new AutoResetEvent(false);
			clientWrote.Handle = OpenEvent( EVENT_ALL_ACCESS, false, 
				dataMemoryName + "_CLIENT_WROTE" );

			clientRead = new AutoResetEvent(false);
			clientRead.Handle = OpenEvent( EVENT_ALL_ACCESS, false, 
				dataMemoryName + "_CLIENT_READ" );

			// tell the server we are ready
			serverRead.Set();
		}

		#region Properties
		public override bool CanRead
		{
			get	{ return true; }
		}

		public override bool CanSeek
		{
			get { return false; }
		}

		public override bool CanWrite
		{
			get { return true; }
		}

		public override long Length
		{
			get { throw new NotSupportedException("SharedMemoryStream does not support seeking - length"); }
		}

		public override long Position
		{
			get { throw new NotSupportedException("SharedMemoryStream does not support seeking - postition"); }
			set	{}
		}

		#endregion

		public override void Flush()
		{
			FlushViewOfFile( dataView, 0 );
		}

		public bool IsClosed() 
		{
			try 
			{
				dataView = MapViewOfFile( dataMap, FILE_MAP_WRITE, 0, 0, (UIntPtr)BUFFERLENGTH );
				if (dataView == IntPtr.Zero) return true;
				return false;
			}
			catch (Exception) 
			{
				return true;
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			while (bytesLeft == 0)
			{
				while (! serverWrote.WaitOne(500, false)) 
				{
					if (IsClosed()) return 0;
				}

				bytesLeft = Marshal.ReadInt32( dataView );
				position = 4;
			}

			int len = Math.Min( count, bytesLeft );
			long baseMem = dataView.ToInt64() + position;

			for (int i=0; i < len; i++, position++)
				buffer[offset+i] = Marshal.ReadByte( (IntPtr)( baseMem + i ) );

			bytesLeft -= len;

			if ( bytesLeft == 0)
				clientRead.Set();

			return len;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException("SharedMemoryStream does not support seeking");
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			int leftToDo = count;
			int buffPos = offset;

			while (leftToDo > 0)
			{
				if (! serverRead.WaitOne()) 
					throw new MySqlException("Writing to shared memory failed");

				int bytesToDo = Math.Min( leftToDo, BUFFERLENGTH );

				long baseMem = dataView.ToInt64() + 4;
				Marshal.WriteInt32( dataView, bytesToDo );
				for (int i=0; i < bytesToDo; i++, buffPos++)
					Marshal.WriteByte( (IntPtr)(baseMem + i), buffer[ buffPos ] );
				leftToDo -= bytesToDo;
				if (! clientWrote.Set())
					throw new MySqlException("Writing to shared memory failed");
			}
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException("SharedMemoryStream does not support seeking");
		}



#region Imports
		[DllImport("kernel32.dll")]
		static extern IntPtr OpenEvent(uint dwDesiredAccess, bool bInheritHandle,
			string lpName);

		[DllImport("kernel32.dll")]
		static extern bool SetEvent(IntPtr hEvent);

		[DllImport("kernel32.dll")]
		static extern IntPtr OpenFileMapping(uint dwDesiredAccess, bool bInheritHandle,
			string lpName);

		[DllImport("kernel32.dll")]
		static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, uint
			dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow,
			UIntPtr dwNumberOfBytesToMap);

		[DllImport("kernel32.dll")]
		static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

		[DllImport("kernel32.dll", SetLastError=true)]
		static extern int CloseHandle(IntPtr hObject);

		[DllImport("kernel32.dll", SetLastError=true)]
		static extern int FlushViewOfFile( IntPtr address, uint numBytes );

#endregion




	}
}
