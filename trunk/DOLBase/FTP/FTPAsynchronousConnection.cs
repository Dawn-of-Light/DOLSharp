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
using System.Collections.Generic;
using System.IO;
using System.Threading;

//Written by the DotNetFTPClient team: http://www.sourceforge.net/projects/dotnetftpclient

namespace DOL.FTP
{
	/// <summary>
	/// Summary description for FTPAsynchronousConnection.
	/// </summary>
	public class FTPAsynchronousConnection : FTPConnection
	{
		private readonly Queue<string> _deleteFileQueue;
		private readonly Queue<FileTransferStruct> _getFileTransfersQueue;
		private readonly Queue<string> _makeDirQueue;
		private readonly Queue<string> _removeDirQueue;
		private readonly Queue<FileTransferStruct> _sendFileTransfersQueue;
		private readonly Queue<string> _setCurrentDirectoryQueue;

		/// <summary>
		/// Creates a new asynchronous FTP connection
		/// </summary>
		public FTPAsynchronousConnection()
		{
			_sendFileTransfersQueue = new Queue<FileTransferStruct>();
			_getFileTransfersQueue = new Queue<FileTransferStruct>();
			_deleteFileQueue = new Queue<string>();
			_setCurrentDirectoryQueue = new Queue<string>();
			_makeDirQueue = new Queue<string>();
			_removeDirQueue = new Queue<string>();
		}

		/// <summary>
		/// Retrieves a remote file
		/// </summary>
		/// <param name="remoteFileName">The remote filename</param>
		/// <param name="type">The transfer type</param>
		public override void GetFile(string remoteFileName, FTPFileTransferType type)
		{
			GetFile(remoteFileName, Path.GetFileName(remoteFileName), type);
		}

		/// <summary>
		/// Retrieves a remote file
		/// </summary>
		/// <param name="remoteFileName">The remote filename</param>
		/// <param name="localFileName">The local filename</param>
		/// <param name="type">The transfer type</param>
		public override void GetFile(string remoteFileName, string localFileName, FTPFileTransferType type)
		{
			var ftStruct = new FileTransferStruct
			               	{
			               		LocalFileName = localFileName,
			               		RemoteFileName = remoteFileName,
			               		Type = type
			               	};

			_getFileTransfersQueue.Enqueue(ftStruct);

			ThreadPool.QueueUserWorkItem(GetFileFromQueue);
		}

		private void GetFileFromQueue(object state)
		{
			FileTransferStruct ftStruct = _getFileTransfersQueue.Dequeue();
			base.GetFile(ftStruct.RemoteFileName, ftStruct.LocalFileName, ftStruct.Type);
		}

		/// <summary>
		/// Sends a file to the remote host
		/// </summary>
		/// <param name="localFileName">The local filename</param>
		/// <param name="type">The transfer type</param>
		public override void SendFile(string localFileName, FTPFileTransferType type)
		{
			SendFile(localFileName, Path.GetFileName(localFileName), type);
		}

		/// <summary>
		/// Sends a file to the remote host
		/// </summary>
		/// <param name="localFileName">The local filename</param>
		/// <param name="remoteFileName">The remote filename</param>
		/// <param name="type">The transfer type</param>
		public override void SendFile(string localFileName, string remoteFileName, FTPFileTransferType type)
		{
			var ftStruct = new FileTransferStruct
			               	{
			               		LocalFileName = localFileName,
			               		RemoteFileName = remoteFileName,
			               		Type = type
			               	};

			_sendFileTransfersQueue.Enqueue(ftStruct);

			ThreadPool.QueueUserWorkItem(SendFileFromQueue);
		}

		private void SendFileFromQueue(object state)
		{
			FileTransferStruct ftStruct = _sendFileTransfersQueue.Dequeue();
			base.SendFile(ftStruct.LocalFileName, ftStruct.RemoteFileName, ftStruct.Type);
		}

		/// <summary>
		/// Deletes a remote file
		/// </summary>
		/// <param name="remoteFileName">The remote filename</param>
		public override void DeleteFile(string remoteFileName)
		{
			_deleteFileQueue.Enqueue(remoteFileName);

			ThreadPool.QueueUserWorkItem(DeleteFileFromQueue);
		}

		private void DeleteFileFromQueue(object state)
		{
			base.DeleteFile(_deleteFileQueue.Dequeue());
		}

		/// <summary>
		/// Sets the current remote directory
		/// </summary>
		/// <param name="remotePath">The remote path to set</param>
		public override void SetCurrentDirectory(string remotePath)
		{
			_setCurrentDirectoryQueue.Enqueue(remotePath);

			ThreadPool.QueueUserWorkItem(SetCurrentDirectoryFromQueue);
		}

		private void SetCurrentDirectoryFromQueue(object state)
		{
			base.SetCurrentDirectory(_setCurrentDirectoryQueue.Dequeue());
		}

		/// <summary>
		/// Creates a directory on the remote server
		/// </summary>
		/// <param name="directoryName">The directory name to create</param>
		public override void CreateDirectory(string directoryName)
		{
			_makeDirQueue.Enqueue(directoryName);

			ThreadPool.QueueUserWorkItem(MakeDirFromQueue);
		}

		private void MakeDirFromQueue(object state)
		{
			base.CreateDirectory(_makeDirQueue.Dequeue());
		}

		/// <summary>
		/// Removes a remote directory
		/// </summary>
		/// <param name="directoryName">The directory name to remove</param>
		public override void RemoveDirectory(string directoryName)
		{
			_removeDirQueue.Enqueue(directoryName);

			ThreadPool.QueueUserWorkItem(RemoveDirFromQueue);
		}

		private void RemoveDirFromQueue(object state)
		{
			base.RemoveDirectory(_removeDirQueue.Dequeue());
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