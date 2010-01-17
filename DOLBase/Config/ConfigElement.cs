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

namespace DOL.Config
{
	/// <summary>
	/// This class represents a configuration element
	/// It can hold one value and/or several children elements
	/// </summary>
	public class ConfigElement
	{
		/// <summary>
		/// All the children elements
		/// </summary>
		protected Hashtable m_children = new Hashtable();

		/// <summary>
		/// The parent element of this element
		/// </summary>
		protected ConfigElement m_parent;

		/// <summary>
		/// The value of this element
		/// </summary>
		protected string m_value;

		/// <summary>
		/// Constructs a new config element with the given parent.
		/// </summary>
		/// <param name="parent">The parent element of the newly created element</param>
		public ConfigElement(ConfigElement parent)
		{
			m_parent = parent;
		}

		/// <summary>
		/// Returns the child element with the specified key
		/// </summary>
		public ConfigElement this[string key]
		{
			get
			{
				lock (m_children) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
				{
					if (!m_children.Contains(key))
					{
						m_children.Add(key, GetNewConfigElement(this));
					}
				}
				return (ConfigElement) m_children[key];
			}
			set
			{
				lock (m_children) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
				{
					m_children[key] = value;
				}
			}
		}

		/// <summary>
		/// Gets the parent element of this config element
		/// </summary>
		public ConfigElement Parent
		{
			get { return m_parent; }
		}

		/// <summary>
		/// Returns if this element has children
		/// </summary>
		public bool HasChildren
		{
			get { return m_children.Count > 0; }
		}

		/// <summary>
		/// Returns a Hashtable with the children of this element
		/// </summary>
		public Hashtable Children
		{
			get { return m_children; }
		}

		/// <summary>
		/// Creates and returns a new configuration element.
		/// Can be used to create own configuration elements by
		/// overwriting this method
		/// </summary>
		/// <param name="parent">The parent element of the newly created element</param>
		/// <returns>The newly created config element</returns>
		protected virtual ConfigElement GetNewConfigElement(ConfigElement parent)
		{
			return new ConfigElement(parent);
		}

		/// <summary>
		/// Gets the value of this config element as string
		/// </summary>
		/// <returns>The string representing the value of this element</returns>
		public string GetString()
		{
			return m_value;
		}

		/// <summary>
		/// Gets the value of this config element as string
		/// and if no value is set returns the default value
		/// </summary>
		/// <param name="defaultValue">The default to return in case no value is set</param>
		/// <returns>The value of this element or the given default value if no value is set</returns>
		public string GetString(string defaultValue)
		{
			return m_value != null ? m_value : defaultValue;
		}

		/// <summary>
		/// Gets the value of this config element as integer
		/// </summary>
		/// <returns>The integer representing the value of this element</returns>
		public int GetInt()
		{
			return int.Parse(m_value);
		}

		/// <summary>
		/// Gets the value of this config element integer
		/// and if no value is set returns the default value
		/// </summary>
		/// <param name="defaultValue">The default to return in case no value is set</param>
		/// <returns>The value of this element or the given default value if no value is set</returns>
		public int GetInt(int defaultValue)
		{
			return m_value != null ? int.Parse(m_value) : defaultValue;
		}

		/// <summary>
		/// Gets the value of this config element as long
		/// </summary>
		/// <returns>The long representing the value of this element</returns>
		public long GetLong()
		{
			return long.Parse(m_value);
		}

		/// <summary>
		/// Gets the value of this config element as long
		/// and if no value is set returns the default value
		/// </summary>
		/// <param name="defaultValue">The default to return in case no value is set</param>
		/// <returns>The value of this element or the given default value if no value is set</returns>
		public long GetLong(long defaultValue)
		{
			return m_value != null ? long.Parse(m_value) : defaultValue;
		}

		/// <summary>
		/// Gets the value of this config element as boolean
		/// </summary>
		/// <returns>The boolean representing the value of this element</returns>
		public bool GetBoolean()
		{
			return bool.Parse(m_value);
		}

		/// <summary>
		/// Gets the value of this config element as boolean
		/// and if no value is set returns the default value
		/// </summary>
		/// <param name="defaultValue">The default to return in case no value is set</param>
		/// <returns>The value of this element or the given default value if no value is set</returns>
		public bool GetBoolean(bool defaultValue)
		{
			return m_value != null ? bool.Parse(m_value) : defaultValue;
		}

		/// <summary>
		/// Sets the value of this config element
		/// </summary>
		/// <param name="value">The value for element</param>
		public void Set(object value)
		{
			m_value = value.ToString();
		}
	}
}
