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
using MySql.Data.Common;
using System.Collections;
using System.Threading;
using log4net;

namespace MySql.Data.MySqlClient
{
	/// <summary>
	/// Summary description for MySqlPool.
	/// </summary>
	internal sealed class MySqlPool
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private ArrayList				inUsePool;
		private ArrayList				idlePool;
		private MySqlConnectionString	settings;
		private int						minSize;
		private int						maxSize;

		public MySqlPool(MySqlConnectionString settings)
		{
			minSize = settings.MinPoolSize;
			maxSize = settings.MaxPoolSize;
			this.settings = settings;
			inUsePool =new ArrayList();
			idlePool = new ArrayList( settings.MinPoolSize );

			// prepopulate the idle pool to minSize
			for (int i=0; i < minSize; i++) 
				CreateNewPooledConnection();
		}

		public MySqlConnectionString	Settings 
		{
			get { return settings; }
			set { settings = value; }
		}

		private int CheckConnections() 
		{
			int freed = 0;
			lock (inUsePool.SyncRoot) 
			{
				for (int i=inUsePool.Count-1; i >= 0; i--) 
				{
					Driver d = (inUsePool[i] as Driver);
					if (! d.Ping()) 
					{
						inUsePool.RemoveAt(i);
						freed++;
					}
				}
			}
			return freed;
		}

		private Driver GetPooledConnection()
		{
			Driver driver = null;

			// if here are no idle connections and inUsePool is full, then we
			// check each of the inUsePool connections and make sure they are
			// actually in use.
			if (idlePool.Count == 0 && inUsePool.Count == maxSize) 
			{
				int freed = CheckConnections();

				// if we freed no connections, then we can't pull one so we return null
				if (0 == freed) return null;
			}

			// if we get here, then we have at least one connection in the idle pool
			lock (idlePool.SyncRoot) 
			{
				for (int i=idlePool.Count-1; i >=0; i--)
				{
					driver = (idlePool[i] as Driver);
					if ( driver.Ping() )
					{
						lock (inUsePool) 
						{
							inUsePool.Add( driver );
						}
						idlePool.RemoveAt( i );
						break;
					}
					else 
					{
						driver.SafeClose();
						idlePool.RemoveAt(i);
						driver = null;
					}
				}
			}

			if ( driver != null ) 
			{
				driver.Settings = settings;
				driver.Reset();
			}
			else if ((idlePool.Count+inUsePool.Count) < maxSize)
			{
				// if we couldn't get a pooled connection and there is still room
				// make a new one
				driver = CreateNewPooledConnection();
				if ( driver != null)
				{
					lock (idlePool.SyncRoot)
						lock (inUsePool.SyncRoot) 
						{
							idlePool.Remove( driver );
							inUsePool.Add( driver );
						}
				}
			}

			// make sure we stay at least minSize in our combined pools
			while ((idlePool.Count + inUsePool.Count) < minSize)
				CreateNewPooledConnection();

			return driver;
		}

		private Driver CreateNewPooledConnection()
		{
			lock(idlePool.SyncRoot) 
				lock (inUsePool.SyncRoot)
				{
					// first we check if we are allowed to create another
					if ((inUsePool.Count + idlePool.Count) == maxSize) return null;

					Driver driver = Driver.Create( settings );
					idlePool.Add( driver );
					return driver;
				}
		}

		public void ReleaseConnection( Driver driver )
		{
			lock (idlePool.SyncRoot)
				lock (inUsePool.SyncRoot) 
				{
					inUsePool.Remove( driver );
					if (driver.Settings.ConnectionLifetime != 0 && driver.IsTooOld())
						driver.Close();
					else
						idlePool.Add( driver );
				}
		}

		public Driver GetConnection() 
		{
			Driver driver = null;

			int start = Environment.TickCount;
			int ticks = settings.ConnectionTimeout * 1000;

			// wait timeOut seconds at most to get a connection
			while (driver == null && (Environment.TickCount - start) < ticks)
			{
				driver = GetPooledConnection();
				if (driver == null)
				{
					if(log.IsWarnEnabled)
						log.Warn("No pooled connections left, waiting... idle=" + idlePool.Count + " inUse=" + inUsePool.Count);
					Thread.Sleep(200); // wait for connection
				}
			}
					 
			// if pool size is at maximum, then we must have reached our timeout so we simply
			// throw our exception
			if (driver == null)
				throw new MySqlException("error connecting: Timeout expired.  The timeout period elapsed " + 
					"prior to obtaining a connection from the pool.  This may have occurred because all " +
					"pooled connections were in use and max pool size was reached.");

			return driver;
		}

	}
}
