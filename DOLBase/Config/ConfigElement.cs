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

namespace DOL.Config
{
	/// <summary>
	/// This class represents a single element in a configuration file.
	/// </summary>
	/// <remarks>This element could be a parent container for multiple child elements.</remarks>
	public class ConfigElement
	{
		/// <summary>
		/// The child elements of this element.
		/// </summary>
		private readonly Dictionary<string, ConfigElement> _children = new Dictionary<string, ConfigElement>();

		/// <summary>
		/// The parent element of this element.
		/// </summary>
		private readonly ConfigElement _parent;

		/// <summary>
		/// The value of this element.
		/// </summary>
		private string _value;

		/// <summary>
		/// Constructs a new config element with the given parent.
		/// </summary>
		/// <param name="parent">the parent element of the newly created element</param>
		public ConfigElement(ConfigElement parent)
		{
			_parent = parent;
			_value = "";
		}

		/// <summary>
		/// Returns the child element with the specified key.
		/// </summary>
		public ConfigElement this[string key]
		{
			get
			{
				lock (_children) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
				{
					if (!_children.ContainsKey(key))
					{
						_children.Add(key, GetNewConfigElement(this));
					}
				}

				return _children[key];
			}
			set
			{
				lock (_children) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
				{
					_children[key] = value;
				}
			}
		}

		/// <summary>
		/// Gets the parent element of this config element.
		/// </summary>
		public ConfigElement Parent
		{
			get { return _parent; }
		}

		/// <summary>
		/// Whether or not this element has child elements.
		/// </summary>
		public bool HasChildren
		{
			get { return _children.Count > 0; }
		}

		/// <summary>
		/// Returns a dictionary with the children of this element.
		/// </summary>
		public Dictionary<string, ConfigElement> Children
		{
			get { return _children; }
		}

		/// <summary>
		/// Creates and returns a new configuration element.
		/// </summary>
		/// <param name="parent">the parent element of the newly created element</param>
		/// <returns>the newly created config element</returns>
		private static ConfigElement GetNewConfigElement(ConfigElement parent)
		{
			return new ConfigElement(parent);
		}

		/// <summary>
		/// Gets the value of this this configuration element as a string.
		/// </summary>
		/// <returns>the string representing the value of this element</returns>
		public string GetString()
		{
			return _value ?? "";
		}

		/// <summary>
		/// Gets the value of this this configuration element as a string.
		/// </summary>
		/// <param name="defaultValue">the default to return in case no value is set</param>
		/// <returns>the value of this element or the default value</returns>
		public string GetString(string defaultValue)
		{
			return _value ?? defaultValue;
		}

		/// <summary>
		/// Gets the value of this this configuration element as an integer.
		/// </summary>
		/// <returns>the integer representation of the value of this element</returns>
		public int GetInt()
		{
			return int.Parse(_value);
		}

		/// <summary>
		/// Gets the value of this this configuration element as an integer.
		/// </summary>
		/// <param name="defaultValue">the default to return in case no value is set</param>
		/// <returns>the integer representation of the value of this element or the default value</returns>
		public int GetInt(int defaultValue)
		{
			return _value != null ? int.Parse(_value) : defaultValue;
		}

		/// <summary>
		/// Gets the value of this this configuration element as a long.
		/// </summary>
		/// <returns>the long representation of the value of this element</returns>
		public long GetLong()
		{
			return long.Parse(_value);
		}

		/// <summary>
		/// Gets the value of this this configuration element as a long.
		/// </summary>
		/// <param name="defaultValue">the default to return in case no value is set</param>
		/// <returns> the long representation of the value of this element or the default value</returns>
		public long GetLong(long defaultValue)
		{
			return _value != null ? long.Parse(_value) : defaultValue;
		}

		/// <summary>
		/// Gets the value of this this configuration element as a boolean.
		/// </summary>
		/// <returns>the boolean representation of the value of this element</returns>
		public bool GetBoolean()
		{
			return bool.Parse(_value);
		}

		/// <summary>
		/// Gets the value of this this configuration element as a boolean.
		/// </summary>
		/// <param name="defaultValue">the default to return in case no value is set</param>
		/// <returns>the boolean representation of the value of this element or the default value</returns>
		public bool GetBoolean(bool defaultValue)
		{
			return _value != null ? bool.Parse(_value) : defaultValue;
		}

		/// <summary>
		/// Sets the value for this configuration element.
		/// </summary>
		/// <param name="value">the value for element</param>
		public void Set(object value)
		{
			if (value == null)
				value = "";

			_value = value.ToString();
		}
	}
}