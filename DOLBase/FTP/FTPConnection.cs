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
using System.Net;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Collections;
using System.Data;
using System.Threading;

//Written by the DotNetFTPClient team: http://www.sourceforge.net/projects/dotnetftpclient
namespace DOL.FTP
{
	/// <summary>
	/// Summary description for FTPConnection.
	/// </summary>

	public class FTPConnection
	{		
		private TcpClient mTCPClient;
		private String mRemoteHost;
		private int mRemotePort;
		private static int BLOCK_SIZE = 512;
		private static int DEFAULT_REMOTE_PORT = 21;
		private static int DATA_PORT_RANGE_FROM = 1500;
		private static int DATA_PORT_RANGE_TO = 65000;
		private FTPMode mMode;
		private int mActiveConnectionsCount;

		private ArrayList mMessageList = new ArrayList();
		private bool mLogMessages;

		/// <summary>
		/// Creates a new ftp connection
		/// </summary>
		public FTPConnection()
		{
			mActiveConnectionsCount = 0;
			mMode = FTPMode.Active;
			mLogMessages = false;
		}

		/// <summary>
		/// The message list containing all the remote messages
		/// </summary>
		public ArrayList MessageList
		{
			get
			{
				return mMessageList;
			}
		}


		/// <summary>
		/// Sets or gets if messages should be logged
		/// </summary>
		public bool LogMessages
		{
			get
			{
				return mLogMessages;
			}

			set
			{
				if(!value)
				{
					mMessageList = new ArrayList();
				}

				mLogMessages = value;
			}
		}

		/// <summary>
		/// Opens a new ftp connection
		/// </summary>
		/// <param name="pRemoteHost">The remote hostname</param>
		/// <param name="pUser">The remote user</param>
		/// <param name="pPassword">The remote password</param>
		public virtual void Open(string pRemoteHost, string pUser, string pPassword)
		{
			Open(pRemoteHost, DEFAULT_REMOTE_PORT, pUser, pPassword, FTPMode.Active);
		}

		/// <summary>
		/// Opens a new ftp connection
		/// </summary>
		/// <param name="pRemoteHost">The remote hostname</param>
		/// <param name="pUser">The remote user</param>
		/// <param name="pPassword">The remote password</param>
		/// <param name="pMode">The ftp mode</param>
		public virtual void Open(string pRemoteHost, string pUser, string pPassword, FTPMode pMode)
		{
			Open(pRemoteHost, DEFAULT_REMOTE_PORT, pUser, pPassword, pMode);
		}

		/// <summary>
		/// Opens a new ftp connection
		/// </summary>
		/// <param name="pRemoteHost">The remote hostname</param>
		/// <param name="pRemotePort">The remote port</param>
		/// <param name="pUser">The remote user</param>
		/// <param name="pPassword">The remote password</param>
		public virtual void Open(string pRemoteHost, int pRemotePort, string pUser, string pPassword)
		{
			Open(pRemoteHost, pRemotePort, pUser, pPassword, FTPMode.Active);
		}
		
		/// <summary>
		/// Opens a new ftp connection
		/// </summary>
		/// <param name="pRemoteHost">The remote hostname</param>
		/// <param name="pRemotePort">The remote port</param>
		/// <param name="pUser">The remote user</param>
		/// <param name="pPassword">The remote password</param>
		/// <param name="pMode">The ftp mode</param>
		public virtual void Open(string pRemoteHost, int pRemotePort, string pUser, string pPassword, FTPMode pMode)
		{
			ArrayList aTempMessageList = new ArrayList();
			int aReturnValue;

			mMode = pMode;
			mTCPClient = new TcpClient();
			mRemoteHost = pRemoteHost;
			mRemotePort = pRemotePort;

			//CONNECT
			try
			{
				mTCPClient.Connect(mRemoteHost, mRemotePort);
			}
			catch(Exception)
			{
				throw new IOException("Couldn't connect to remote server");
			}
			aTempMessageList = Read();
			aReturnValue = GetMessageReturnValue((string)aTempMessageList[0]);
			if(aReturnValue != 220)
			{
				Close();
				throw new Exception((string)aTempMessageList[0]);
			}

			//SEND USER
			aTempMessageList = SendCommand("USER " + pUser);
			aReturnValue = GetMessageReturnValue((string)aTempMessageList[0]);
			if(!(aReturnValue == 331 || aReturnValue == 202))
			{
				Close();
				throw new Exception((string)aTempMessageList[0]);
			}

			//SEND PASSWORD
			if(aReturnValue == 331)
			{
				aTempMessageList = SendCommand("PASS " + pPassword);
				aReturnValue = GetMessageReturnValue((string)aTempMessageList[0]);
				if(!(aReturnValue == 230 || aReturnValue == 202))
				{
					Close();
					throw new Exception((string)aTempMessageList[0]);
				}
			}
		}

		/// <summary>
		/// Closes a connection to the remote server
		/// </summary>
		public virtual void Close()
		{
			if( mTCPClient != null )
			{
				SendCommand("QUIT");
				mTCPClient.Close();
			}
		}

		/// <summary>
		/// Returns a list of remote directories
		/// </summary>
		/// <param name="pMask">The mask for the query</param>
		/// <returns>An ArrayList of directories</returns>
		public ArrayList Dir(String pMask)
		{
			ArrayList aTmpList = Dir();

			DataTable aTable = new DataTable();
			aTable.Columns.Add("Name");
			for(int i = 0; i < aTmpList.Count; i++)
			{
				DataRow aRow = aTable.NewRow();
				aRow["Name"] = (string)aTmpList[i];
				aTable.Rows.Add(aRow);
			}

			DataRow [] aRowList = aTable.Select("Name LIKE '" + pMask + "'", "", DataViewRowState.CurrentRows);
			aTmpList = new ArrayList();
			for(int i = 0; i < aRowList.Length; i++)
			{
				aTmpList.Add((string)aRowList[i]["Name"]);
			}

			return aTmpList;
		}
		
		/// <summary>
		/// Reads the remote directory
		/// </summary>
		/// <returns>An ArrayList with the remote directory contents</returns>
		public ArrayList Dir()
		{
			LockTcpClient();
			TcpListener aListner = null;
			TcpClient aClient = null;
			NetworkStream aNetworkStream = null;
			ArrayList aTempMessageList = new ArrayList();
			int aReturnValue = 0;
			string aReturnValueMessage = "";
			ArrayList aFileList = new ArrayList();

			SetTransferType(FTPFileTransferType.ASCII);

			if(mMode == FTPMode.Active)
			{
				aListner = CreateDataListner();
				aListner.Start();
			}
			else
			{
				aClient = CreateDataClient();
			}

			aTempMessageList = new ArrayList();
			aTempMessageList = SendCommand("NLST");
			aReturnValue = GetMessageReturnValue((string)aTempMessageList[0]);
			if(!(aReturnValue == 150 || aReturnValue == 125))
			{
				throw new Exception((string)aTempMessageList[0]);
			}

			if(mMode == FTPMode.Active)
			{
				aClient = aListner.AcceptTcpClient();
			}
			aNetworkStream = aClient.GetStream();

			aFileList = ReadLines(aNetworkStream);

			if(aTempMessageList.Count == 1)
			{
				aTempMessageList = Read();
				aReturnValue = GetMessageReturnValue((string)aTempMessageList[0]);
				aReturnValueMessage = (string)aTempMessageList[0];
			}
			else
			{
				aReturnValue = GetMessageReturnValue((string)aTempMessageList[1]);
				aReturnValueMessage = (string)aTempMessageList[1];
			}

			if(!(aReturnValue == 226))
			{
				throw new Exception(aReturnValueMessage);
			}

			aNetworkStream.Close();
			aClient.Close();

			if(mMode == FTPMode.Active)
			{
				aListner.Stop();
			}
			UnlockTcpClient();
			return aFileList;
		}

		/// <summary>
		/// Sends a stream to a remote file
		/// </summary>
		/// <param name="pStream">The stream to send</param>
		/// <param name="pRemoteFileName">The remote file name</param>
		/// <param name="pType">The transfer type</param>
		public void SendStream(Stream pStream, string pRemoteFileName, FTPFileTransferType pType)
		{
			LockTcpClient();
			TcpListener aListner = null;
			TcpClient aClient = null;
			NetworkStream aNetworkStream = null;
			ArrayList aTempMessageList = new ArrayList();
			int aReturnValue = 0;
			string aReturnValueMessage = "";
			aTempMessageList = new ArrayList();

			SetTransferType(pType);

			if(mMode == FTPMode.Active)
			{
				aListner = CreateDataListner();
				aListner.Start();
			}
			else
			{
				aClient = CreateDataClient();
			}

			aTempMessageList = SendCommand("STOR " + pRemoteFileName);
			aReturnValue = GetMessageReturnValue((string)aTempMessageList[0]);
			if(!(aReturnValue == 150 || aReturnValue == 125))
			{
				throw new Exception((string)aTempMessageList[0]);
			}

			if(mMode == FTPMode.Active)
			{
				aClient = aListner.AcceptTcpClient();
			}

			aNetworkStream = aClient.GetStream();

			Byte[] aBuffer = new Byte[BLOCK_SIZE];
			int iBytes = 0;
			int iTotalBytes = 0;

			while(iTotalBytes < pStream.Length)
			{
				iBytes = pStream.Read(aBuffer, 0, BLOCK_SIZE);
				iTotalBytes = iTotalBytes + iBytes;
				aNetworkStream.Write(aBuffer, 0, iBytes);
			}

			pStream.Close();

			aNetworkStream.Close();
			aClient.Close();

			if(mMode == FTPMode.Active)
			{
				aListner.Stop();
			}

			if(aTempMessageList.Count == 1)
			{
				aTempMessageList = Read();
				aReturnValue = GetMessageReturnValue((string)aTempMessageList[0]);
				aReturnValueMessage = (string)aTempMessageList[0];
			}
			else
			{
				aReturnValue = GetMessageReturnValue((string)aTempMessageList[1]);
				aReturnValueMessage = (string)aTempMessageList[1];
			}

			if(!(aReturnValue == 226))
			{
				throw new Exception(aReturnValueMessage);
			}
			UnlockTcpClient();
		}

		/// <summary>
		/// Sends a file to the remote server
		/// </summary>
		/// <param name="pLocalFileName">The local filename</param>
		/// <param name="pType">The transfer type</param>
		public virtual void SendFile(string pLocalFileName, FTPFileTransferType pType)
		{
			SendFile(pLocalFileName, Path.GetFileName(pLocalFileName), pType);
		}

		/// <summary>
		/// Sends a file to the remote server
		/// </summary>
		/// <param name="pLocalFileName">The local filename</param>
		/// <param name="pRemoteFileName">The remote filename</param>
		/// <param name="pType">The transfer type</param>
		public virtual void SendFile(string pLocalFileName, string pRemoteFileName, FTPFileTransferType pType)
		{
			using (FileStream file = new FileStream(pLocalFileName,FileMode.Open))
			{
				SendStream(file, pRemoteFileName, pType);
			}
		}

		/// <summary>
		/// Connects a stream to remote file
		/// </summary>
		/// <param name="pRemoteFileName">The remote file name</param>
		/// <param name="pStream">The stream to connect to the remote file</param>
		/// <param name="pType">The transfer type</param>
		public void GetStream(string pRemoteFileName, Stream pStream, FTPFileTransferType pType)
		{
			LockTcpClient();
			TcpListener aListner = null;
			TcpClient aClient = null;
			NetworkStream aNetworkStream = null;
			ArrayList aTempMessageList = new ArrayList();
			int aReturnValue = 0;
			string aReturnValueMessage = "";

			SetTransferType(pType);

			if(mMode == FTPMode.Active)
			{
				aListner = CreateDataListner();
				aListner.Start();
			}
			else
			{
				aClient = CreateDataClient();
			}

			aTempMessageList = new ArrayList();
			aTempMessageList = SendCommand("RETR " + pRemoteFileName);
			aReturnValue = GetMessageReturnValue((string)aTempMessageList[0]);
			if(!(aReturnValue == 150 || aReturnValue == 125))
			{
				throw new Exception((string)aTempMessageList[0]);
			}

			if(mMode == FTPMode.Active)
			{
				aClient = aListner.AcceptTcpClient();
			}

			aNetworkStream = aClient.GetStream();

			Byte[] aBuffer = new Byte[BLOCK_SIZE];
			int iBytes = 0;

			bool bRead = true;
			while(bRead)
			{
				iBytes = aNetworkStream.Read(aBuffer, 0, aBuffer.Length);
				pStream.Write(aBuffer, 0, iBytes);
				if(iBytes == 0)
				{
					bRead = false;
				}
			}

			pStream.Close();

			aNetworkStream.Close();
			aClient.Close();

			if(mMode == FTPMode.Active)
			{
				aListner.Stop();
			}

			if(aTempMessageList.Count == 1)
			{
				aTempMessageList = Read();
				aReturnValue = GetMessageReturnValue((string)aTempMessageList[0]);
				aReturnValueMessage = (string)aTempMessageList[0];
			}
			else
			{
				aReturnValue = GetMessageReturnValue((string)aTempMessageList[1]);
				aReturnValueMessage = (string)aTempMessageList[1];
			}

			if(!(aReturnValue == 226))
			{
				throw new Exception(aReturnValueMessage);
			}

			UnlockTcpClient();
		}
		
		/// <summary>
		/// Retrieves a remote file
		/// </summary>
		/// <param name="pRemoteFileName">The remote file name</param>
		/// <param name="pType">The transfer type</param>
		public virtual void GetFile(string pRemoteFileName, FTPFileTransferType pType)
		{
			GetFile(pRemoteFileName, Path.GetFileName(pRemoteFileName), pType);
		}

		/// <summary>
		/// Retrieves a remote file
		/// </summary>
		/// <param name="pRemoteFileName">The remote file name</param>
		/// <param name="pLocalFileName">The local file name</param>
		/// <param name="pType">The transfer type</param>
		public virtual void GetFile(string pRemoteFileName, string pLocalFileName, FTPFileTransferType pType)
		{
			GetStream(pRemoteFileName, new FileStream(pLocalFileName,FileMode.Create), pType);
		}
		
		/// <summary>
		/// Deletes a remote file
		/// </summary>
		/// <param name="pRemoteFileName">The remote filename</param>
		public virtual void DeleteFile(String pRemoteFileName)
		{
			lock(mTCPClient)
			{
				ArrayList aTempMessageList = new ArrayList();
				int aReturnValue = 0;
				aTempMessageList = SendCommand("DELE " + pRemoteFileName);
				aReturnValue = GetMessageReturnValue((string)aTempMessageList[0]);
				if(aReturnValue != 250)
				{
					throw new Exception((string)aTempMessageList[0]);
				}
			}
		}

		/// <summary>
		/// Sets the remote directory
		/// </summary>
		/// <param name="pRemotePath">The remote path to set</param>
		public virtual void SetCurrentDirectory(String pRemotePath)
		{
			LockTcpClient();

			ArrayList aTempMessageList = new ArrayList();
			int aReturnValue = 0;
			aTempMessageList = SendCommand("CWD " + pRemotePath);
			aReturnValue = GetMessageReturnValue((string)aTempMessageList[0]);
			if(aReturnValue != 250)
			{
				throw new Exception((string)aTempMessageList[0]);
			}
			UnlockTcpClient();
		}
		
		private void SetTransferType(FTPFileTransferType pType)
		{
			switch (pType)
			{
				case FTPFileTransferType.ASCII:
					SetMode("TYPE A");
					break;
				case FTPFileTransferType.Binary:
					SetMode("TYPE I");
					break;
				default:
					throw new Exception("Invalid File Transfer Type");
			}
		}

		private void SetMode(string pMode)
		{
			LockTcpClient();

			ArrayList aTempMessageList = new ArrayList();
			int aReturnValue = 0;
			aTempMessageList = SendCommand(pMode);
			aReturnValue = GetMessageReturnValue((string)aTempMessageList[0]);
			if(aReturnValue != 200)
			{
				throw new Exception((string)aTempMessageList[0]);
			}
			UnlockTcpClient();
		}
		
		private TcpListener CreateDataListner()
		{
			int aPort = GetPortNumber();
			SetDataPort(aPort);
			IPAddress ipAddress = Dns.Resolve("localhost").AddressList[0];

			TcpListener aListner = new TcpListener(ipAddress, aPort);
			return aListner; 
		}
		
		private TcpClient CreateDataClient()
		{
			int aPort = GetPortNumber();

			IPEndPoint ep = new 
				IPEndPoint(GetLocalAddressList()[0], aPort);

			TcpClient aClient = new TcpClient();

			aClient.Connect(ep);

			return aClient;
		}
		
		private void SetDataPort(int pPortNumber)
		{
			LockTcpClient();

			ArrayList aTempMessageList = new ArrayList();
			int aReturnValue = 0;
			int iPortHigh = pPortNumber >> 8;
			int iPortLow = pPortNumber & 255;

			aTempMessageList = SendCommand("PORT " 
				+ GetLocalAddressList()[0].ToString().Replace(".", ",")
				+ "," + iPortHigh.ToString() + "," + iPortLow);

			aReturnValue = GetMessageReturnValue((string)aTempMessageList[0]);
			if(aReturnValue != 200)
			{
				throw new Exception((string)aTempMessageList[0]);
			}
			UnlockTcpClient();

		}

		/// <summary>
		/// Creates a remote directory
		/// </summary>
		/// <param name="pDirectoryName">The remote directory to create</param>
		public virtual void MakeDir(string pDirectoryName)
		{
			LockTcpClient();

			ArrayList aTempMessageList = new ArrayList();
			int aReturnValue = 0;

			aTempMessageList = SendCommand("MKD " + pDirectoryName);
			aReturnValue = GetMessageReturnValue((string)aTempMessageList[0]);
			if(aReturnValue != 257)
			{
				throw new Exception((string)aTempMessageList[0]);
			}

			UnlockTcpClient();
		}

		/// <summary>
		/// Removes a remote directory
		/// </summary>
		/// <param name="pDirectoryName">The remote directory to remove</param>
		public virtual void RemoveDir(string pDirectoryName)
		{
			LockTcpClient();

			ArrayList aTempMessageList = new ArrayList();
			int aReturnValue = 0;

			aTempMessageList = SendCommand("RMD " + pDirectoryName);
			aReturnValue = GetMessageReturnValue((string)aTempMessageList[0]);
			if(aReturnValue != 250)
			{
				throw new Exception((string)aTempMessageList[0]);
			}

			UnlockTcpClient();
		}

		/// <summary>
		/// Sends a specific command to the remote server
		/// </summary>
		/// <param name="pCommand">The command name</param>
		/// <returns>An array containing the response</returns>
		public ArrayList SendCommand(String pCommand)
		{
			while(mActiveConnectionsCount!=0)
			{
				Thread.Sleep(100);
			}
			
			mActiveConnectionsCount++;

			Byte[] cmdBytes = Encoding.ASCII.GetBytes((pCommand+"\r\n").ToCharArray());
			NetworkStream aStream = mTCPClient.GetStream();
			aStream.Write(cmdBytes, 0, cmdBytes.Length);

			mActiveConnectionsCount--;

			return Read();
		}

		private ArrayList Read ()
		{
			NetworkStream aStream = mTCPClient.GetStream();
			ArrayList aMessageList = new ArrayList();
			ArrayList aTempMessage = ReadLines(aStream);
			if(aTempMessage.Count > 0)
			{
				while(((string)aTempMessage[aTempMessage.Count - 1]).Substring(3, 1) == "-")
				{
					aMessageList.AddRange(aTempMessage);
					aTempMessage = ReadLines(aStream);
				}
				aMessageList.AddRange(aTempMessage);
			}

			AddMessagesToMessageList(aMessageList);

			return aMessageList;
		}
		
		private ArrayList ReadLines(NetworkStream pStream)
		{
			ArrayList aMessageList = new ArrayList();
			char[] seperator = {'\n'};
			char[] toRemove = {'\r'};
			Byte[] aBuffer = new Byte[BLOCK_SIZE];
			int bytes = 0;
			string tmpMes = "";
			bool bRead = true;

			while(bRead)
			{
				bytes = pStream.Read(aBuffer, 0, aBuffer.Length);
				tmpMes += Encoding.ASCII.GetString(aBuffer, 0, bytes);
				if(bytes < aBuffer.Length)
				{
					bRead = false;
				}
			}

			string[] mess = tmpMes.Split(seperator);
			for (int i = 0; i < mess.Length; i++)
			{
				if(mess[i].Length > 0)
				{
					aMessageList.Add(mess[i].Trim(toRemove));
				}
			}

			return aMessageList;
		}

		private int GetMessageReturnValue(string pMessage)
		{
			return int.Parse(pMessage.Substring(0, 3));
		}

		private int GetPortNumber()
		{
			LockTcpClient();
			int iPort = 0;
			switch (mMode)
			{
				case FTPMode.Active:
					Random rnd = new Random((int)DateTime.Now.Ticks);
					iPort = DATA_PORT_RANGE_FROM + rnd.Next(DATA_PORT_RANGE_TO - DATA_PORT_RANGE_FROM);
					break;
				case FTPMode.Passive:
					ArrayList aTempMessageList = new ArrayList();
					int aReturnValue = 0;
					aTempMessageList = SendCommand("PASV");
					aReturnValue = GetMessageReturnValue((string)aTempMessageList[0]);
					if(aReturnValue != 227)
					{
						if(((string)aTempMessageList[0]).Length > 4)
						{
							throw new Exception((string)aTempMessageList[0]);
						}
						else
						{
							throw new Exception((string)aTempMessageList[0] + " Passive Mode not implemented");
						}
					}
					string aMessage = (string)aTempMessageList[0];
					int iIndex1 = aMessage.IndexOf(",", 0);
					int iIndex2 = aMessage.IndexOf(",", iIndex1 + 1);
					int iIndex3 = aMessage.IndexOf(",", iIndex2 + 1);
					int iIndex4 = aMessage.IndexOf(",", iIndex3 + 1);
					int iIndex5 = aMessage.IndexOf(",", iIndex4 + 1);
					int iIndex6 = aMessage.IndexOf(")", iIndex5 + 1);
					iPort = 256 * int.Parse(aMessage.Substring(iIndex4 + 1, iIndex5 - iIndex4 - 1)) + int.Parse(aMessage.Substring(iIndex5 + 1, iIndex6 - iIndex5 - 1));
					break;
			}
			UnlockTcpClient();
			return iPort;
		}

		private void AddMessagesToMessageList(ArrayList mMessages)
		{
			if(mLogMessages)
			{
				mMessageList.AddRange(mMessages);
			}
		}

		private IPAddress[] GetLocalAddressList()
		{
			return Dns.Resolve(Dns.GetHostName()).AddressList;
		}

		private void LockTcpClient()
		{
			System.Threading.Monitor.Enter(mTCPClient);
		}

		private void UnlockTcpClient()
		{
			System.Threading.Monitor.Exit(mTCPClient);
		}
	}
}
