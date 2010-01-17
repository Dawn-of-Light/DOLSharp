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
using System.Collections;
using System.IO;
using System.Threading;
using System.Timers;
using Timer=System.Timers.Timer;

//Written by the DotNetFTPClient team: http://www.sourceforge.net/projects/dotnetftpclient

namespace DOL.FTP
{
	/// <summary>
	/// Summary description for FTPAsynchronousConnection.
	/// </summary>
	public class FTPAsynchronousConnection : FTPConnection
	{
		private readonly Queue mDeleteFileQueue;
		private readonly Queue mGetFileTransfersQueue;
		private readonly Queue mMakeDirQueue;
		private readonly Queue mRemoveDirQueue;
		private readonly Queue mSendFileTransfersQueue;
		private readonly Queue mSetCurrentDirectoryQueue;
		private readonly ArrayList mThreadPool;
		private readonly Timer mTimer;

		/// <summary>
		/// Creates a new asynchronous FTP connection
		/// </summary>
		public FTPAsynchronousConnection()
		{
			mThreadPool = new ArrayList();
			mSendFileTransfersQueue = new Queue();
			mGetFileTransfersQueue = new Queue();
			mDeleteFileQueue = new Queue();
			mSetCurrentDirectoryQueue = new Queue();
			mMakeDirQueue = new Queue();
			mRemoveDirQueue = new Queue();
			mTimer = new Timer(100);
			mTimer.Elapsed += ManageThreads;
			mTimer.Start();
		}

		/// <summary>
		/// Opens a new FTP connection to a remote host
		/// </summary>
		/// <param name="pRemoteHost">The remote host address</param>
		/// <param name="pUser">The remote username</param>
		/// <param name="pPassword">The remote password</param>
		public override void Open(string pRemoteHost, string pUser, string pPassword)
		{
			base.Open(pRemoteHost, pUser, pPassword);
		}

		/// <summary>
		/// Opens a new FTP connection to a remote host
		/// </summary>
		/// <param name="pRemoteHost">The remote host address</param>
		/// <param name="pUser">The remote username</param>
		/// <param name="pPassword">The remote password</param>
		/// <param name="pMode">The ftp mode</param>
		public override void Open(string pRemoteHost, string pUser, string pPassword, FTPMode pMode)
		{
			base.Open(pRemoteHost, pUser, pPassword, pMode);
		}

		/// <summary>
		/// Opens a new FTP connection to a remote host
		/// </summary>
		/// <param name="pRemoteHost">The remote host address</param>
		/// <param name="pRemotePort">The remote port</param>
		/// <param name="pUser">The remote username</param>
		/// <param name="pPassword">The remote password</param>
		public override void Open(string pRemoteHost, int pRemotePort, string pUser, string pPassword)
		{
			base.Open(pRemoteHost, pRemotePort, pUser, pPassword);
		}

		/// <summary>
		/// Opens a new FTP connection to a remote host
		/// </summary>
		/// <param name="pRemoteHost">The remote host address</param>
		/// <param name="pRemotePort">The remote port</param>
		/// <param name="pUser">The remote username</param>
		/// <param name="pPassword">The remote password</param>
		/// <param name="pMode">The ftp mode</param>
		public override void Open(string pRemoteHost, int pRemotePort, string pUser, string pPassword, FTPMode pMode)
		{
			base.Open(pRemoteHost, pRemotePort, pUser, pPassword, pMode);
		}

		private Thread CreateGetFileThread(string pRemoteFileName, string pLocalFileName, FTPFileTransferType pType)
		{
			var aFT = new FileTransferStruct();
			aFT.LocalFileName = pLocalFileName;
			aFT.RemoteFileName = pRemoteFileName;
			aFT.Type = pType;
			mGetFileTransfersQueue.Enqueue(aFT);

			var aThread = new Thread(GetFileFromQueue);
			aThread.Name = "GetFileFromQueue " + pRemoteFileName + ", " + pLocalFileName + ", " + pType;
			;
			return aThread;
		}

		/// <summary>
		/// Retrieves a remote file
		/// </summary>
		/// <param name="pRemoteFileName">The remote filename</param>
		/// <param name="pType">The transfer type</param>
		public override void GetFile(string pRemoteFileName, FTPFileTransferType pType)
		{
			GetFile(pRemoteFileName, Path.GetFileName(pRemoteFileName), pType);
		}

		/// <summary>
		/// Retrieves a remote file
		/// </summary>
		/// <param name="pRemoteFileName">The remote filename</param>
		/// <param name="pLocalFileName">The local filename</param>
		/// <param name="pType">The transfer type</param>
		public override void GetFile(string pRemoteFileName, string pLocalFileName, FTPFileTransferType pType)
		{
			EnqueueThread(CreateGetFileThread(pRemoteFileName, pLocalFileName, pType));
		}

		private void GetFileFromQueue()
		{
			var aFT = (FileTransferStruct) mGetFileTransfersQueue.Dequeue();
			base.GetFile(aFT.RemoteFileName, aFT.LocalFileName, aFT.Type);
		}

		private Thread CreateSendFileThread(string pLocalFileName, string pRemoteFileName, FTPFileTransferType pType)
		{
			var aFT = new FileTransferStruct();
			aFT.LocalFileName = pLocalFileName;
			aFT.RemoteFileName = pRemoteFileName;
			aFT.Type = pType;
			mSendFileTransfersQueue.Enqueue(aFT);

			var aThread = new Thread(SendFileFromQueue);
			aThread.Name = "GetFileFromQueue " + pLocalFileName + ", " + pRemoteFileName + ", " + pType;
			;
			return aThread;
		}

		/// <summary>
		/// Sends a file to the remote host
		/// </summary>
		/// <param name="pLocalFileName">The local filename</param>
		/// <param name="pType">The transfer type</param>
		public override void SendFile(string pLocalFileName, FTPFileTransferType pType)
		{
			SendFile(pLocalFileName, Path.GetFileName(pLocalFileName), pType);
		}

		/// <summary>
		/// Sends a file to the remote host
		/// </summary>
		/// <param name="pLocalFileName">The local filename</param>
		/// <param name="pRemoteFileName">The remote filename</param>
		/// <param name="pType">The transfer type</param>
		public override void SendFile(string pLocalFileName, string pRemoteFileName, FTPFileTransferType pType)
		{
			EnqueueThread(CreateSendFileThread(pLocalFileName, pRemoteFileName, pType));
		}

		private void SendFileFromQueue()
		{
			var aFT = (FileTransferStruct) mSendFileTransfersQueue.Dequeue();
			base.SendFile(aFT.LocalFileName, aFT.RemoteFileName, aFT.Type);
		}

		/// <summary>
		/// Deletes a remote file
		/// </summary>
		/// <param name="pRemoteFileName">The remote filename</param>
		public override void DeleteFile(String pRemoteFileName)
		{
			EnqueueThread(CreateDeleteFileThread(pRemoteFileName));
		}

		private Thread CreateDeleteFileThread(String pRemoteFileName)
		{
			mDeleteFileQueue.Enqueue(pRemoteFileName);

			var aThread = new Thread(DeleteFileFromQueue);
			aThread.Name = "DeleteFileFromQueue " + pRemoteFileName;
			return aThread;
		}

		private void DeleteFileFromQueue()
		{
			base.DeleteFile((string) mDeleteFileQueue.Dequeue());
		}

		/// <summary>
		/// Sets the current remote directory
		/// </summary>
		/// <param name="pRemotePath">The remote path to set</param>
		public override void SetCurrentDirectory(String pRemotePath)
		{
			EnqueueThread(CreateSetCurrentDirectoryThread(pRemotePath));
		}

		private Thread CreateSetCurrentDirectoryThread(String pRemotePath)
		{
			mSetCurrentDirectoryQueue.Enqueue(pRemotePath);

			var aThread = new Thread(SetCurrentDirectoryFromQueue);
			aThread.Name = "SetCurrentDirectoryFromQueue " + pRemotePath;
			return aThread;
		}

		private void SetCurrentDirectoryFromQueue()
		{
			base.SetCurrentDirectory((string) mSetCurrentDirectoryQueue.Dequeue());
		}

		/// <summary>
		/// Creates a directory on the remote server
		/// </summary>
		/// <param name="pDirectoryName">The directory name to create</param>
		public override void MakeDir(string pDirectoryName)
		{
			EnqueueThread(CreateMakeDirFromQueueThread(pDirectoryName));
		}

		private Thread CreateMakeDirFromQueueThread(string pDirectoryName)
		{
			mMakeDirQueue.Enqueue(pDirectoryName);

			var aThread = new Thread(MakeDirFromQueue);
			aThread.Name = "MakeDirFromQueue " + pDirectoryName;
			return aThread;
		}

		private void MakeDirFromQueue()
		{
			base.MakeDir((String) mMakeDirQueue.Dequeue());
		}

		/// <summary>
		/// Removes a remote directory
		/// </summary>
		/// <param name="pDirectoryName">The directory name to remove</param>
		public override void RemoveDir(string pDirectoryName)
		{
			EnqueueThread(CreateRemoveDirFromQueue(pDirectoryName));
		}

		private Thread CreateRemoveDirFromQueue(string pDirectoryName)
		{
			mRemoveDirQueue.Enqueue(pDirectoryName);

			var aThread = new Thread(RemoveDirFromQueue);
			aThread.Name = "RemoveDirFromQueue " + pDirectoryName;
			return aThread;
		}

		private void RemoveDirFromQueue()
		{
			base.RemoveDir((String) mRemoveDirQueue.Dequeue());
		}

		/// <summary>
		/// Closes the FTP connection to the remote server
		/// </summary>
		public override void Close()
		{
			WaitAllThreads();
			base.Close();
		}

		private void ManageThreads(Object state, ElapsedEventArgs e)
		{
			Thread aThread;
			try
			{
				LockThreadPool();
				aThread = PeekThread();
				if (aThread != null)
				{
					switch (aThread.ThreadState)
					{
						case ThreadState.Unstarted:
							LockThreadPool();
							aThread.Start();
							UnlockThreadPool();
							break;
						case ThreadState.Stopped:
							LockThreadPool();
							DequeueThread();
							UnlockThreadPool();
							break;
					}
				}
				UnlockThreadPool();
			}
			catch (Exception)
			{
				UnlockThreadPool();
			}
		}

		private void WaitAllThreads()
		{
			while (mThreadPool.Count != 0)
			{
				Thread.Sleep(100);
			}
		}

		private void EnqueueThread(Thread aThread)
		{
			LockThreadPool();
			mThreadPool.Add(aThread);
			UnlockThreadPool();
		}

		private Thread DequeueThread()
		{
			Thread aThread;
			LockThreadPool();
			aThread = (Thread) mThreadPool[0];
			mThreadPool.RemoveAt(0);
			UnlockThreadPool();
			return aThread;
		}

		private Thread PeekThread()
		{
			Thread aThread = null;
			LockThreadPool();
			if (mThreadPool.Count > 0)
			{
				aThread = (Thread) mThreadPool[0];
			}
			UnlockThreadPool();
			return aThread;
		}

		private void LockThreadPool()
		{
			Monitor.Enter(mThreadPool);
		}

		private void UnlockThreadPool()
		{
			Monitor.Exit(mThreadPool);
		}

		#region Nested type: FileTransferStruct

		private struct FileTransferStruct
		{
			public string LocalFileName;
			public string RemoteFileName;
			public FTPFileTransferType Type;
		}

		#endregion
	}
}
