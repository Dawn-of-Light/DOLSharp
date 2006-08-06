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

namespace DOL.GS.ServerProperties
{
	/// <summary>
	/// The server property attribute
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class ServerPropertyAttribute : Attribute
	{
		private string m_key;
		private string m_description;
		private string m_defaultValue;

		/// <summary>
		/// The constructor
		/// </summary>
		/// <param name="key">The property key</param>
		/// <param name="description">The property description</param>
		/// <param name="defaultValue">The property default value</param>
		public ServerPropertyAttribute(string key, string description, string defaultValue)
		{
			m_key = key;
			m_description = description;
			m_defaultValue = defaultValue;
		}

		/// <summary>
		/// The property key
		/// </summary>
		public string Key
		{
			get
			{
				return m_key;
			}
		}

		/// <summary>
		/// The property description
		/// </summary>
		public string Description
		{
			get
			{
				return m_description;
			}
		}

		/// <summary>
		/// The property default value
		/// </summary>
		public string DefaultValue
		{
			get
			{
				return m_defaultValue;
			}
		}
	}
}