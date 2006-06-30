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

namespace MySql.Data.MySqlClient
{
	/// <summary>
	/// Summary description for MySqlPoolManager.
	/// </summary>
	internal sealed class MySqlPoolManager
	{
		private static Hashtable	pools;

		public MySqlPoolManager() 
		{
		}

		/// <summary>
		/// 
		/// </summary>
		private static void Initialize()
		{
			pools = new Hashtable();
		}

		public static Driver GetConnection(MySqlConnectionString settings) 
		{
			// make sure the manager is initialized
			if (MySqlPoolManager.pools == null)
				MySqlPoolManager.Initialize();

			string text = settings.GetConnectionString(true);

			lock( pools.SyncRoot ) 
			{
				MySqlPool pool;
				if (!pools.Contains( text )) 
				{
					pool = new MySqlPool( settings );
					pools.Add( text, pool );
				}
				else 
				{
					pool = (pools[text] as MySqlPool);
					pool.Settings = settings;
				}

				return pool.GetConnection();
			}
		}

		public static void ReleaseConnection( Driver driver )
		{
			lock (pools.SyncRoot) 
			{
				string key = driver.Settings.GetConnectionString(true);
				MySqlPool pool = (MySqlPool)pools[ key ];
				if (pool == null)
					throw new MySqlException("Pooling exception: Unable to find original pool for connection");
				pool.ReleaseConnection( driver );
			}
		}
	}
}
