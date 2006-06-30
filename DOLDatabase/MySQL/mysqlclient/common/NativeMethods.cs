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
using System.Threading;
using System.Runtime.InteropServices;

namespace MySql.Data.Common
{
	class NativeMethods
	{
		// Keep the compiler from generating a default ctor
		private NativeMethods() 
		{
		}

		[StructLayout(LayoutKind.Sequential)]
		public class SecurityAttributes 
		{
			public SecurityAttributes() 
			{
				Length = Marshal.SizeOf(typeof(SecurityAttributes));
			}
			public int Length;
			public IntPtr securityDescriptor = IntPtr.Zero;
			public bool inheritHandle;
		}

		[DllImport("Kernel32")]
		static extern public int CreateFile(String fileName,
			uint desiredAccess,
			uint shareMode, 
			SecurityAttributes securityAttributes,
			uint creationDisposition,
			uint flagsAndAttributes,
			uint templateFile);

		[return:MarshalAs(UnmanagedType.Bool)]
		[DllImport("kernel32.dll", EntryPoint="PeekNamedPipe", SetLastError=true)]
		static extern public bool PeekNamedPipe(IntPtr handle,
			byte[] buffer, 
			uint nBufferSize, 
			ref uint bytesRead,
			ref uint bytesAvail, 
			ref uint BytesLeftThisMessage);

		[return:MarshalAs(UnmanagedType.Bool)]
		[DllImport("kernel32.dll", SetLastError=true)]
		static extern internal bool ReadFile(IntPtr hFile, [Out] byte[] lpBuffer, uint nNumberOfBytesToRead,
			out uint lpNumberOfBytesRead, IntPtr lpOverlapped);

		[return:MarshalAs(UnmanagedType.Bool)]
		[DllImport("Kernel32")]
		static extern internal bool WriteFile(IntPtr hFile, [In]byte[] buffer,
			uint numberOfBytesToWrite, out uint numberOfBytesWritten,
			IntPtr lpOverlapped);

		[return:MarshalAs(UnmanagedType.Bool)]
		[DllImport("kernel32.dll", SetLastError=true)]
		public static extern bool CloseHandle(IntPtr handle);

		[return:MarshalAs(UnmanagedType.Bool)]
		[DllImport("kernel32.dll", SetLastError=true)]
		public static extern bool FlushFileBuffers(IntPtr handle);

		//Constants for dwDesiredAccess:
		public const UInt32 GENERIC_READ = 0x80000000;
		public const UInt32 GENERIC_WRITE = 0x40000000;

		//Constants for return value:
		public const Int32 INVALIDpipeHandle_VALUE = -1;

		//Constants for dwFlagsAndAttributes:
		public const UInt32 FILE_FLAG_OVERLAPPED = 0x40000000;
		public const UInt32 FILE_FLAG_NO_BUFFERING = 0x20000000;

		//Constants for dwCreationDisposition:
		public const UInt32 OPEN_EXISTING = 3;

		#region Winsock functions

		// SOcket routines
		[DllImport("ws2_32.dll", SetLastError=true)]
		static extern public IntPtr socket(int af, int type, int protocol);

		[DllImport("ws2_32.dll", SetLastError=true)]
		static extern public int ioctlsocket(IntPtr socket, uint cmd, ref UInt32 arg);

		[DllImport("ws2_32.dll", SetLastError=true)]
		public static extern int WSAIoctl(IntPtr s, uint dwIoControlCode, byte[] inBuffer, uint cbInBuffer,
			byte[] outBuffer, uint cbOutBuffer, IntPtr lpcbBytesReturned, IntPtr lpOverlapped,
			IntPtr lpCompletionRoutine);

		[DllImport("ws2_32.dll", SetLastError=true)]
		static extern public int WSAGetLastError();

		[DllImport("ws2_32.dll", SetLastError=true)]
		static extern public int connect(IntPtr socket, byte[] addr, int addrlen);

		[DllImport("ws2_32.dll", SetLastError=true)]
		static extern public int recv(IntPtr socket, byte[] buff, int len, int flags);

		[DllImport("ws2_32.Dll", SetLastError=true)]
		static extern public int send(IntPtr socket, byte[] buff, int len, int flags);

		#endregion

	}
}
