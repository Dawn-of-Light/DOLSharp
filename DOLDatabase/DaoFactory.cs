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
using System.Collections.Generic;
using System.Xml;
using System.Reflection;
using DOL.Database.IDaos;

namespace DOL.Database
{
	/// <summary>
	/// Manages <see cref="IDataAccessObject"/>s. This class is thread safe.
	/// </summary>
	public class DaoFactory : IDaoFactory
	{
		/// <summary>
		/// Holds the table with all registered DAOs by DAO type.
		/// </summary>
		private readonly Dictionary<Type, IDataAccessObject>
			m_registeredDaos = new Dictionary<Type, IDataAccessObject>();

		/// <summary>
		/// constructor with cfg to configure it
		/// </summary>
		/// <param name="cfg"></param>
		public DaoFactory(DaoFactoryConfiguration cfg)
		{
			Configure(cfg);
		}

		/// <summary>
		/// configure with the XML
		/// </summary>
		/// <param name="cfg"></param>
		public void Configure(DaoFactoryConfiguration cfg)
		{ 
			foreach ( XmlTextReader reader in cfg.Config)
			{
				ParseConfig(reader);
			}
		}

		public void ParseConfig(XmlTextReader reader)
		{
			while (reader.Read())
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					if (reader.Name == "DaoList")
						continue;

					if (reader.Name == "RegisterDao")
					{
						IDataAccessObject dataAccessObject = null;
						string name = reader.GetAttribute("type");
						string dao = reader.GetAttribute("dao");
						Type interfaceType = Assembly.GetExecutingAssembly().GetType(name);
						Type daoType = Assembly.GetExecutingAssembly().GetType(dao);
						dataAccessObject = Activator.CreateInstance(daoType) as IDataAccessObject;
						//RegisterDao<interfaceType>(dataAccessObject);
						//TODO FIX ME HERE
					}
					else
					{
						//todo log error
					}
				}
				else
					continue;
			}
			reader.Close();
		}

		/// <summary>
		/// Registers the data access object and
		/// throws an exception if already registered.
		/// </summary>
		/// <typeparam name="T">The DAO type to register.</typeparam>
		/// <param name="dataAccessObject">The DAO to register.</param>
		/// <exception cref="ArgumentException">DAO type is already registered.</exception>
		/// <exception cref="ArgumentNullException">DAO object is null.</exception>
		public void RegisterDao<T>(IDataAccessObject dataAccessObject)
			where T : class, IDataAccessObject
		{
			if (dataAccessObject == null)
			{
				throw new ArgumentNullException("dataAccessObject", "DAO object cannot be null");
			}

			Type type = typeof (T);
			if (!type.IsAssignableFrom(dataAccessObject.GetType()))
			{
				throw new ArgumentException("The DAO "
				                            + dataAccessObject.GetType().FullName
				                            + " must derive from the specified type: "
				                            + type.FullName);
			}

			lock (m_registeredDaos)
			{
				if (m_registeredDaos.ContainsKey(type))
				{
					throw new ArgumentException("A DAO is already register for this type: "
					                            + type.FullName);
				}

				m_registeredDaos.Add(type, dataAccessObject);
			}
		}

		/// <summary>
		/// Pick the data access object of this type and return it.
		/// </summary>
		/// <typeparam name="T">The type of data object to register.</typeparam>
		/// <returns>The DAO object.</returns>
		/// <exception cref="Exception">The type is not registered.</exception>
		/// <exception cref="Exception">Registered DAO does not derive from <typeparamref name="T"/>.</exception>
		public T GetDao<T>()
			where T : class, IDataAccessObject
		{
			IDataAccessObject dataAccessObject;
			Type type = typeof (T);

			lock (m_registeredDaos)
			{
				if (!m_registeredDaos.TryGetValue(type, out dataAccessObject))
				{
					throw new Exception("No data access object for this type :"
					                    + type.ToString());
				}
			}

			if (dataAccessObject is T == false)
			{
				throw new Exception("The DAO "
				                    + dataAccessObject.GetType().FullName
				                    + " does not derive from type "
				                    + type.FullName);
			}

			return dataAccessObject as T;
		}

		/// <summary>
		/// Saves all data, synchronous.
		/// </summary>
		public void SaveAll()
		{
			IDataAccessObject[] daosCopy;
			lock (m_registeredDaos)
			{
				daosCopy = new IDataAccessObject[m_registeredDaos.Count];
				m_registeredDaos.Values.CopyTo(daosCopy, 0);
			}

			foreach (IDataAccessObject dataAccessObject in daosCopy)
			{
				dataAccessObject.SaveAll();
			}
		}
	}
}