using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using DOL.Database;

namespace DOL.GS
{
	public enum AuditType
	{
		Account,
		Character,
		Chat
	}

	public enum AuditSubtype
	{
		AccountCreate,
		AccountFailedLogin,
		AccountSuccessfulLogin,
		AccountLogout,
		AccountPasswordChange,
		AccountEmailChange,
		AccountDelete,
		CharacterCreate,
		CharacterDelete,
		CharacterRename,
		CharacterLogin,
		CharacterLogout,
		PublicChat,
		PrivateChat
	}

	/// <summary>
	/// Manages all audit entries.
	/// </summary>
	/// <remarks>
	/// An audit entry can include events like an account log in, or a private chat message, or character 
	/// deletion.  The idea is to track certain events to have an audit log to be able to go back and see 
	/// exactly when, and in what order, events occured.
	/// </remarks>
	public static class AuditMgr
	{
		private const int EventsPerInsertLimit = 1000; // 1000 events max per insert query
		private const int PushUpdatesInterval = 5*1000; // 5 seconds (time in milliseconds)
		private static readonly Timer PushTimer;
		private static List<AuditEntry> _queuedAuditEntries;
		private static SpinWaitLock _updateLock;

		static AuditMgr()
		{
			_queuedAuditEntries = new List<AuditEntry>();

			PushTimer = new Timer(PushUpdatesInterval);
			PushTimer.Elapsed += OnPushTimerElapsed;
			PushTimer.AutoReset = false;

			// tobz: this is not ready for prime-time yet.
			// PushTimer.Start();

			_updateLock = new SpinWaitLock();
		}

		private static void OnPushTimerElapsed(object sender, ElapsedEventArgs e)
		{
			// push out the updates.
			// todo: do these as a batch operation to speed it up/make it more efficient

			List<AuditEntry> oldQueue;

			// swap out the queue
			_updateLock.Enter();

			try
			{
				// grab the old queue
				oldQueue = _queuedAuditEntries;

				// swap in a new queue
				_queuedAuditEntries = new List<AuditEntry>();
			}
			finally
			{
				_updateLock.Exit();
			}

			// here we're doing some custom query generation to batch insert all of these entries.
			// normally this would be something that goes in the DB layer, but since we don't want
			// to push hundreds of rows at a time, trying to do reflection on all of them before
			// insertion, we instead hard-code it here.  not the cleanest way but this will change
			// with a switch to a better DB layer.

			StringBuilder queryBuilder = GetEntryQueryBuilder();
			int currentQueryCount = 0;

			foreach (AuditEntry entry in oldQueue)
			{
				// we limit the number of items per insert query.
				if (currentQueryCount > EventsPerInsertLimit)
				{
					// reset item count
					currentQueryCount = 0;

					// close query, remove previous `,` and add `;`
					queryBuilder.Remove(queryBuilder.Length - 1, 1);
					queryBuilder.Append(';');

					// build query string and execute
					string queryString = queryBuilder.ToString();

					GameServer.Database.ExecuteNonQuery(queryString);

					// get new query builder
					queryBuilder = GetEntryQueryBuilder();
				}

				// add entry to the query
				queryBuilder.AppendFormat("({0},{1},{2},{3},{4},{5},{6},{7}),", entry.ObjectId, entry.AuditTime,
				                          entry.AccountID, entry.RemoteHost, entry.AuditType, entry.AuditSubtype,
				                          entry.OldValue, entry.NewValue);

				currentQueryCount++;
			}

			// close query, remove previous `,` and add `;`
			queryBuilder.Remove(queryBuilder.Length - 1, 1);
			queryBuilder.Append(';');

			// build query string and execute
			string entryQuery = queryBuilder.ToString();

			GameServer.Database.ExecuteNonQuery(entryQuery);

			// restart timer
			PushTimer.Start();
		}

		private static StringBuilder GetEntryQueryBuilder()
		{
			var queryBuilder = new StringBuilder();

			// generate insert query
			queryBuilder.Append("INSERT INTO ");

			// get proper table name
			string tableName = ObjectDatabase.GetTableOrViewName(typeof (AuditEntry));

			if (string.IsNullOrEmpty(tableName))
			{
				// this should never, ever happen.
				throw new DatabaseException("Audit table does not exist!");
			}

			queryBuilder.Append(tableName);
			queryBuilder.Append(
				" (`AuditEntry_ID`,`AuditTime`,`AccountID`,`RemoteHost`,`AuditType`,`AuditSubtype`,`OldValue`,`NewValue`) VALUES");

			return queryBuilder;
		}

		public static void AddAuditEntry(int type, int subType, string oldValue, string newValue)
		{
            if (!ServerProperties.Properties.ENABLE_AUDIT_LOG)
                return;

			// create the transaction
			var transactionHistory = new AuditEntry
			                         	{
			                         		AuditTime = DateTime.Now,
			                         		AuditType = type,
			                         		AuditSubtype = subType,
			                         		OldValue = oldValue,
			                         		NewValue = newValue
			                         	};

			_updateLock.Enter();

			try
			{
				// add it to the queue
				_queuedAuditEntries.Add(transactionHistory);
			}
			finally
			{
				_updateLock.Exit();
			}
		}

		public static void AddAuditEntry(AuditType type, AuditSubtype subType, string oldValue, string newValue)
		{
			AddAuditEntry((int) type, (int) subType, oldValue, newValue);
		}

		public static void AddAuditEntry(GameClient client, int type, int subType, string oldValue, string newValue)
		{
            if(!ServerProperties.Properties.ENABLE_AUDIT_LOG)
                return;

			// create the transaction
			var transactionHistory = new AuditEntry
			                         	{
			                         		AuditTime = DateTime.Now,
			                         		AuditType = type,
			                         		AuditSubtype = subType,
			                         		OldValue = oldValue,
			                         		NewValue = newValue
			                         	};

			// make sure account isn't null (no idea why it'd be)
			if (client.Account != null)
			{
				transactionHistory.AccountID = client.Account.ObjectId;
			}

			// set the remote host
			transactionHistory.RemoteHost = client.TcpEndpointAddress;

			_updateLock.Enter();

			try
			{
				// add it to the queue
				_queuedAuditEntries.Add(transactionHistory);
			}
			finally
			{
				_updateLock.Exit();
			}
		}

		public static void AddAuditEntry(GameClient client, AuditType type, AuditSubtype subType)
		{
			AddAuditEntry(client, (int) type, (int) subType, "", "");
		}

		public static void AddAuditEntry(GameClient client, AuditType type, AuditSubtype subType, string oldValue,
		                                 string newValue)
		{
			AddAuditEntry(client, (int) type, (int) subType, oldValue, newValue);
		}

		public static void AddAuditEntry(GamePlayer player, int type, int subType, string oldValue, string newValue)
		{
			AddAuditEntry(player.Client, type, subType, oldValue, newValue);
		}

		public static void AddAuditEntry(GamePlayer player, AuditType type, AuditSubtype subType, string oldValue,
		                                 string newValue)
		{
			AddAuditEntry(player.Client, type, subType, oldValue, newValue);
		}
	}
}