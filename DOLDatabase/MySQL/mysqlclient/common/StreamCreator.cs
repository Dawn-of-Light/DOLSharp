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
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Threading;
using MySql.Data.MySqlClient;
#if __MonoCS__ 
using Mono.Posix;
#endif

namespace MySql.Data.Common
{
	/// <summary>
	/// Summary description for StreamCreator.
	/// </summary>
	internal class StreamCreator
	{
		private const uint FIONBIO = 0x8004667e;
		string				hostList;
		int					port;
		string				pipeName;
		int					timeOut;

		public StreamCreator( string hosts, int port, string pipeName)
		{
			hostList = hosts;
			if (hostList == null || hostList.Length == 0)
				hostList = "localhost";
			this.port = port;
			this.pipeName = pipeName;
		}

		public Stream GetStream(int timeOut) 
		{
			this.timeOut = timeOut;

			if (hostList.StartsWith("/"))
				return CreateSocketStream(null, 0, true);

			string [] dnsHosts = hostList.Split('&');
			ArrayList ipAddresses = new ArrayList();
			ArrayList hostNames = new ArrayList();

			//
			// Each host name specified may contain multiple IP addresses
			// Lets look at the DNS entries for each host name
			foreach (string h in dnsHosts)
			{
				IPHostEntry hostAddress = Dns.GetHostByName(h);
				foreach (IPAddress addr in hostAddress.AddressList)
				{
					ipAddresses.Add( addr );
					hostNames.Add( hostAddress.HostName );
				}
			}

			System.Random random = new Random((int)DateTime.Now.Ticks);
			int index = random.Next(ipAddresses.Count);

			bool usePipe = (pipeName != null && pipeName.Length != 0);
			Stream stream = null;
			for (int i=0; i < ipAddresses.Count; i++)
			{
				if ( pipeName != null )
					stream = CreateNamedPipeStream( (string)hostNames[index] );
				else
					stream = CreateSocketStream( (IPAddress)ipAddresses[index], port, false );
				if (stream != null) return stream;

				index++;
				if (index == ipAddresses.Count) index = 0;
			}

			return stream;
		}

		private Stream CreateNamedPipeStream( string hostname ) 
		{
			string pipePath;
			if (0 == String.Compare(hostname, "localhost", true))
				pipePath = @"\\.\pipe\" + pipeName;
			else
				pipePath = String.Format(@"\\{0}\pipe\{1}", hostname.ToString(), pipeName);
			return new NamedPipeStream(pipePath, FileAccess.ReadWrite);
		}

		private Stream CreateSocketStream(IPAddress ip, int port, bool unix) 
		{
			SocketStream ss = null;
			try
			{
				//
				// Lets try to connect
				EndPoint endPoint;
#if __MonoCS__ && !WINDOWS
				if (unix)
					endPoint = new UnixEndPoint(hostList[0]);
				else
#else
				endPoint = 	new IPEndPoint(ip, port);
				if (unix)
					throw new PlatformNotSupportedException(
						Resources.GetString("UnixSocketsNotSupported"));
#endif

				ss = unix ? 
					new SocketStream(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP) :
					new SocketStream(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				ss.Connect(endPoint, timeOut);
				ss.Socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1);
			}
			catch (ArgumentOutOfRangeException are)
			{
				Logger.LogException(are);
				ss = null;
			}
			catch (SocketException se) 
			{
				Logger.LogException(se);
				ss = null;
			}
			catch (ObjectDisposedException ode) 
			{
				Logger.LogException(ode);
				ss = null;
			}
			catch (MySqlException me)
			{
				Logger.LogException(me);
				ss = null;
			}
			return ss;
		}

	}
}
