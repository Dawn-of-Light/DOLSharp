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

using System.Collections;
using System.Data;
using DOL.Database.Cache;

namespace DOL.Database
{
	/// <summary>
	/// Zusammenfassung für DataTableHandler.
	/// </summary>
	public class DataTableHandler
	{
		private readonly ICache _cache;
		private readonly DataSet _dset;
		private readonly Hashtable _precache;
		private bool _hasRelations;

		/// <summary>
		/// The Constructor
		/// </summary>
		/// <param name="dataSet"></param>
		public DataTableHandler(DataSet dataSet)
		{
			_cache = new SimpleCache();
			_precache = new Hashtable();
			_dset = dataSet;
			_hasRelations = false;
		}

		/// <summary>
		/// Has Relations
		/// </summary>
		public bool HasRelations
		{
			get { return _hasRelations; }
			set { _hasRelations = false; }
		}

		/// <summary>
		/// Cache
		/// </summary>
		public ICache Cache
		{
			get { return _cache; }
		}

		/// <summary>
		/// DataSet
		/// </summary>
		public DataSet DataSet
		{
			get { return _dset; }
		}

		/// <summary>
		/// Uses Precaching
		/// </summary>
		public bool UsesPreCaching { get; set; }

		/// <summary>
		/// Set Cache Object
		/// </summary>
		/// <param name="key">The key object</param>
		/// <param name="obj">The value DataObject</param>
		public void SetCacheObject(object key, DataObject obj)
		{
			_cache[key] = obj;
		}

		/// <summary>
		/// Get Cache Object
		/// </summary>
		/// <param name="key">The key object</param>
		/// <returns>The value DataObject</returns>
		public DataObject GetCacheObject(object key)
		{
			return _cache[key] as DataObject;
		}

		/// <summary>
		/// Set Pre-Cached Object
		/// </summary>
		/// <param name="key">The key object</param>
		/// <param name="obj">The value DataObject</param>
		public void SetPreCachedObject(object key, DataObject obj)
		{
			_precache[key] = obj;
		}

		/// <summary>
		/// Get Pre-Cached Object
		/// </summary>
		/// <param name="key">The key object</param>
		/// <returns>The value DataObject</returns>
		public DataObject GetPreCachedObject(object key)
		{
			return _precache[key] as DataObject;
		}
	}
}