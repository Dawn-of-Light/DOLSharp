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
using System.Data;
using System.Collections;

using DOL.Database.Cache;

namespace DOL.Database
{
	/// <summary>
	/// Zusammenfassung für DataTableHandler.
	/// </summary>
	public class DataTableHandler
	{
		ICache cache;
		Hashtable precache;
		bool usesPrecaching;
		DataSet dset;
		bool hasRelations;

		/// <summary>
		/// The Constructor
		/// </summary>
		/// <param name="dataSet"></param>
		public DataTableHandler(DataSet dataSet)
		{
			cache = new SimpleCache();
			precache = new Hashtable();
			dset = dataSet;
			hasRelations = false;
		}

		/// <summary>
		/// Has Relations
		/// </summary>
		public bool HasRelations
		{
			get
			{
				return hasRelations;
			}
			set
			{
				hasRelations = false;
			}
		}

		/// <summary>
		/// Cache
		/// </summary>
		public ICache Cache
		{
			get
			{
				return cache;
			}
		}
	
		/// <summary>
		/// DataSet
		/// </summary>
		public DataSet DataSet
		{
			get
			{
				return dset;
			}
		}

		/// <summary>
		/// Uses Precaching
		/// </summary>
		public bool UsesPreCaching
		{
			get
			{
				return usesPrecaching;
			}
			set
			{
				usesPrecaching = value;
			}
		}

		/// <summary>
		/// Set Cache Object
		/// </summary>
		/// <param name="Key">The key object</param>
		/// <param name="Obj">The value DataObject</param>
		public void SetCacheObject(object Key, DataObject Obj)
		{
			cache[Key] = Obj;
		}

		/// <summary>
		/// Get Cache Object
		/// </summary>
		/// <param name="Key">The key object</param>
		/// <returns>The value DataObject</returns>
		public DataObject GetCacheObject(object Key)
		{
			return cache[Key] as DataObject;
		}

		/// <summary>
		/// Set Pre-Cached Object
		/// </summary>
		/// <param name="key">The key object</param>
		/// <param name="obj">The value DataObject</param>
		public void SetPreCachedObject(object key, DataObject obj)
		{
			precache[key] = obj;
		}

		/// <summary>
		/// Get Pre-Cached Object
		/// </summary>
		/// <param name="key">The key object</param>
		/// <returns>The value DataObject</returns>
		public DataObject GetPreCachedObject(object key)
		{
			return precache[key] as DataObject;
		}
	}
}
