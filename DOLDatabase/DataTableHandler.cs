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

		public DataTableHandler(DataSet dataSet)
		{
			cache = new SimpleCache();
			precache = new Hashtable();
			dset = dataSet;
			hasRelations = false;
		}

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

		public ICache Cache
		{
			get
			{
				return cache;
			}
		}
	
		public DataSet DataSet
		{
			get
			{
				return dset;
			}
		}

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

		public void SetCacheObject(object Key, DataObject Obj)
		{
			cache[Key] = Obj;
		}

		public DataObject GetCacheObject(object Key)
		{
			return cache[Key] as DataObject;
		}

		public void SetPreCachedObject(object key, DataObject obj)
		{
			precache[key] = obj;
		}

		public DataObject GetPreCachedObject(object key)
		{
			return precache[key] as DataObject;
		}
	}
}
