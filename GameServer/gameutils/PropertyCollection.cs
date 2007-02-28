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
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected readonly HybridDictionary m_props = new HybridDictionary(0);

		public PropertyCollection()
		{
		}

		/// <summary>
		/// retrieve bool property
		/// </summary>
		/// <param name="key">key</param>
		/// <param name="def">default value</param>
		/// <returns>value in properties or default value if not found</returns>
		public bool getProperty(string key, bool def)
		{
			object val = m_props[key];
			return (val is bool) ? (bool) val : def;
		}

		/// <summary>
		/// retrieve int property
		/// </summary>
		/// <param name="key">key</param>
		/// <param name="def">default value</param>
		/// <returns>value in properties or default value if not found</returns>
		public int getIntProperty(string key, int def)
		{
			object val = m_props[key];
			if (val == null)
				return def;
			if (val is int)
				return (int) val;
			if (log.IsWarnEnabled)
				log.Warn("Property " + key + " was requested as int but is " + val.GetType());
			return (int) val;
		}

		/// <summary>
		/// retrieve long property
		/// </summary>
		/// <param name="key">key</param>
		/// <param name="def">default value</param>
		/// <returns>value in properties or default value if not found</returns>
		public long getLongProperty(string key, long def)
		{
			object val = m_props[key];
			if (val == null)
				return def;
			if (val is long)
				return (long) val;
			if (log.IsWarnEnabled)
				log.Warn("Property " + key + " was requested as long but is " + val.GetType());
			return (long) val;
		}

		/// <summary>
		/// retrieve string property
		/// </summary>
		/// <param name="key">key</param>
		/// <param name="def">default value</param>
		/// <returns>value in properties or default value if not found</returns>
		public string getProperty(string key, string def)
		{
			object val = m_props[key];
			return (val is string) ? (string) val : def;
		}

		/// <summary>
		/// retrieve object property
		/// </summary>
		/// <param name="key">key</param>
		/// <param name="def">default value</param>
		/// <returns>value in properties or default value if not found</returns>
		public object getObjectProperty(string key, object def)
		{
			object val = m_props[key];
			return (val != null) ? val : def;
		}

		/// <summary>
		/// retrieve object property
		/// </summary>
		/// <param name="key">key</param>
		/// <param name="def">default value</param>
		/// <returns>value in properties or default value if not found</returns>
		public object getObjectProperty(object key, object def)
		{
			object val = m_props[key];
			return (val != null) ? val : def;
		}

		/// <summary>
		/// retrieve bool property but log if the property doesnt exist
		/// </summary>
		/// <param name="key">key</param>
		/// <param name="def">default value</param>
		/// <returns>value in properties or default value if not found</returns>
		public bool getRequiredProperty(string key, bool def)
		{
			object val = m_props[key];
			if (val == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Property '" + key + "' is required but not found, default value '" + def + "' is used.");
			}
			return (val is bool) ? (bool) val : def;
		}

		/// <summary>
		/// retrieve int property but log if the property doesnt exist
		/// </summary>
		/// <param name="key">key</param>
		/// <param name="def">default value</param>
		/// <returns>value in properties or default value if not found</returns>
		public int getRequiredProperty(string key, int def)
		{
			object val = m_props[key];
			if (val == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Property '" + key + "' is required but not found, default value '" + def + "' is used.");
			}
			return (val is int) ? (int) val : def;
		}

		/// <summary>
		/// retrieve string property but log if the property doesnt exist
		/// </summary>
		/// <param name="key">key</param>
		/// <param name="def">default value</param>
		/// <returns>value in properties or default value if not found</returns>
		public string getRequiredProperty(string key, string def)
		{
			object val = m_props[key];
			if (val == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Property '" + key + "' is required but not found, default value '" + def + "' is used.");
			}
			return (val is string) ? (string) val : def;
		}

		/// <summary>
		/// retrieve object property but log if the property doesnt exist
		/// </summary>
		/// <param name="key">key</param>
		/// <param name="def">default value</param>
		/// <returns>value in properties or default value if not found</returns>
		public object getRequiredProperty(string key, object def)
		{
			object val = m_props[key];
			if (val == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Property '" + key + "' is required but not found, default value '" + def + "' is used.");
				return def;
			}
			return val;
		}

		/// <summary>
		/// sets a property
		/// </summary>
		/// <param name="key">key</param>
		/// <param name="val">value</param>
		public void setProperty(object key, object val)
		{
			if (val == null)
			{
				m_props.Remove(key);
			}
			else
			{
				m_props[key] = val;
			}
		}

		/// <summary>
		/// removes a property
		/// </summary>
		/// <param name="key">key</param>
		public void removeProperty(object key)
		{
			m_props.Remove(key);
		}

		/// <summary>
		/// remove all properties
		/// </summary>
		public void RemoveAll()
		{
			m_props.Clear();
		}
	}
}