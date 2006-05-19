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
using System.Reflection;
using DOL.Database.DataAccessInterfaces;
using log4net;

namespace DOL.Database.NHibernate
{
	/// <summary>
	/// Provides basic NHibernate DAO functionality.
	/// </summary>
	/// <typeparam name="TTransferObject">The transfer object type.</typeparam>
	/// <typeparam name="TPrimaryKey">The transfer object's primary key.</typeparam>
	public class GenericDao<TTransferObject, TPrimaryKey> : IGenericDao<TTransferObject, TPrimaryKey>
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Holds the shared NHibernate state object.
		/// </summary>
		private readonly NHState m_state;

		/// <summary>
		/// Gets the shared NHibernate state object.
		/// </summary>
		/// <value>The state object.</value>
		protected NHState State
		{
			get { return m_state; }
		}

		/// <summary>
		/// Loads an object by primary key.
		/// </summary>
		/// <param name="key">The primary key.</param>
		/// <returns>Loaded object.</returns>
		public TTransferObject Load(TPrimaryKey key)
		{
			return (TTransferObject) State.FindObjectByKey(typeof(TTransferObject), key);
		}

		/// <summary>
		/// Inserts a new object into a database.
		/// </summary>
		/// <param name="obj">The object to insert.</param>
		public void Insert(TTransferObject obj)
		{
			State.AddNewObject(obj);
		}

		/// <summary>
		/// Deletes an object from a database.
		/// </summary>
		/// <param name="obj">The object to delete.</param>
		public void Delete(TTransferObject obj)
		{
			State.DeleteObject(obj);
		}

		/// <summary>
		/// Saves all data objects of <typeparamref name="TTransferObject"/> type, syncronous.
		/// </summary>
		public virtual void SaveAll()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GenericDao{TTransferObject, TPrimaryKey}"/> class.
		/// </summary>
		/// <param name="state">The state.</param>
		public GenericDao(NHState state)
		{
			m_state = state;
			log.WarnFormat("Hi from {0}!", GetType().FullName);
		}
	}
}
