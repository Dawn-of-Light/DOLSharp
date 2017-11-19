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
using System.Reflection;
using System.Linq;

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
		private readonly ReaderWriterDictionary<object, object> _props = new ReaderWriterDictionary<object, object>();

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

			bool exists = _props.TryGetValue(key, out val);

			if (loggued)
			{
				if (!exists)
				{
					if (Log.IsWarnEnabled)
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
			if (val == null)
			{
				object dummy;
				_props.TryRemove(key, out dummy);
			}
			else
			{
				_props[key] = val;
			}
		}

		/// <summary>
		/// Remove a property
		/// </summary>
		/// <param name="key">key</param>
		public void removeProperty(object key)
		{
			object dummy;
			_props.TryRemove(key, out dummy);
		}

		/// <summary>
		/// List all properties
		/// </summary>
		/// <returns></returns>
		public List<string> getAllProperties()
		{
			return _props.Keys.Cast<string>().ToList();
		}

		/// <summary>
		/// Remove all properties
		/// </summary>
		public void removeAllProperties()
		{
			_props.Clear();
		}
	}
}