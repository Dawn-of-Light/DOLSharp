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

using System.Reflection;
using DOL.Database;
using log4net;

namespace DOL.GS.ServerProperties
{
	/// <summary>
	/// The class which defines the AbstractServerProperty all Server Properties must inherit from this class
	/// </summary>
	public abstract class AbstractServerProperty : IServerProperty
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		protected static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected string m_key;

		protected ServerProperty m_property;
		protected ServerPropertyAttribute m_attribute;

		/// <summary>
		/// The property key
		/// </summary>
		public string Key
		{
			get { return m_key; }
		}

		/// <summary>
		/// The constructor
		/// </summary>
		/// <param name="a"></param>
		public AbstractServerProperty(ServerPropertyAttribute a)
		{
			m_attribute = a;
		}

		/// <summary>
		/// Change the property
		/// </summary>
		/// <param name="newValue">The new value</param>
		public virtual void Change(string newValue)
		{
			m_property.Value = newValue;
			GameServer.Database.SaveObject(m_property);
		}

		/// <summary>
		/// Method to load the property
		/// </summary>
		public virtual void Load()
		{
			m_property = GameServer.Database.SelectObject(typeof(ServerProperty), "`Key` = '" + m_attribute.Key + "'") as ServerProperty;
			if (m_property == null)
			{
				log.Debug(m_attribute.Key + " not found in the db, adding default" + m_attribute.DefaultValue);
				m_property = new ServerProperty();
				m_property.DefaultValue = m_attribute.DefaultValue;
				m_property.Description = m_attribute.Description;
				m_property.Key = m_attribute.Key;
				m_property.Value = m_attribute.DefaultValue;
				GameServer.Database.AddNewObject(m_property);
			}
			log.Debug(m_property.Key + " loaded with value " + m_property.Value);
		}
	}
}
