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
using System.Collections.Specialized;
using System.Reflection;

using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Holds properties of different types
	/// </summary>
	public class PropertyCollection
	{
		/// <summary>
		/// Define a logger for this class.
		/// </summary>
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Container of properties
		/// </summary>
		private readonly HybridDictionary _props = new HybridDictionary();

		/// <summary>
		/// Retrieve a property
		/// </summary>
		/// <param name="key">key</param>
		/// <param name="def">default value</param>
		/// <param name="loggued">loggued if the value is not found</param>
		/// <returns>value in properties or default value if not found</returns>
		public T getProperty<T>(object key)
		{
			return getProperty<T>(key, default(T));
		}
		public T getProperty<T>(object key, T def)
		{
			return getProperty<T>(key, def, false);
		}
		public T getProperty<T>(object key, T def, bool loggued)
		{
			object val;

			lock (_props)
				val = _props[key];

			if (loggued)
			{
				if (val == null)
				{
					if ( Log.IsWarnEnabled)
						Log.Warn("Property '" + key + "' is required but not found, default value '" + def + "' is used.");
					
					return def;
				}
			}
			
			if (val is T)
				return (T)val;
			
			return def;
		}
		
		/// <summary>
		/// Set a property
		/// </summary>
		/// <param name="key">key</param>
		/// <param name="val">value</param>
		public void setProperty(object key, object val)
		{
			lock (_props)
			{
				if (val == null)
				{
					_props.Remove(key);
				}
				else
				{
					_props[key] = val;
				}
			}
		}

		/// <summary>
		/// Remove a property
		/// </summary>
		/// <param name="key">key</param>
		public void removeProperty(object key)
		{
			lock (_props)
			{
				_props.Remove(key);
			}
		}

		/// <summary>
		/// List all properties
		/// </summary>
		/// <returns></returns>
		public List<string> getAllProperties()
		{
			var temp = new List<string>();

			lock (_props)
			{
				foreach (string key in _props.Keys)
					temp.Add(key);
			}

			return temp;
		}

		/// <summary>
		/// Remove all properties
		/// </summary>
		public void removeAllProperties()
		{
			lock (_props)
			{
				_props.Clear();
			}
		}
	}
}