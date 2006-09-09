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
using System.Threading;
using System.Xml;
using System.Reflection;
using DOL.Database.DataAccessInterfaces;
using log4net;

namespace DOL.Database
{
	/// <summary>
	/// Base database functionality. This class is thread safe.
	/// </summary>
	public class DatabaseMgr : IDatabaseMgr
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Holds the table with all registered DAOs by DAO type.
		/// </summary>
		private readonly Dictionary<Type, IDataAccessObject>
			m_registeredDaos = new Dictionary<Type, IDataAccessObject>();

		/// <summary>
		/// Holds the state object.
		/// </summary>
		protected readonly IDisposable m_state;

		/// <summary>
		/// Registers the specified <paramref name="dataAccessObject"/> as <paramref name="type"/> interface.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="dataAccessObject">The data access object.</param>
		protected void Register(Type type, IDataAccessObject dataAccessObject)
		{
			if (dataAccessObject == null)
			{
				throw new ArgumentNullException("dataAccessObject", "DAO object cannot be null");
			}

			if (!type.IsAssignableFrom(dataAccessObject.GetType()))
			{
				throw new ArgumentException("The DAO "
											+ dataAccessObject.GetType().FullName
											+ " must derive from the specified type: "
											+ type.FullName);
			}

#warning use ReaderWriterLock instead
			
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
		/// Registers a data access object.
		/// </summary>
		/// <typeparam name="T">The DAO type to register.</typeparam>
		/// <param name="dataAccessObject">The DAO to register.</param>
		/// <exception cref="ArgumentException">The DAO type <typeparamref name="T"/> is already registered.</exception>
		/// <exception cref="ArgumentNullException">The DAO is a null reference.</exception>
		/// <seealso cref="Register(Type, IDataAccessObject)"/>
		public virtual void Register<T>(IDataAccessObject dataAccessObject)
			where T : class, IDataAccessObject
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException("Database manager is disposed");
			}

			Register(typeof(T), dataAccessObject);
		}

		/// <summary>
		/// Creates an instance of <paramref name="dao"/> type with <see cref="m_state"/>
		/// as constructor param and registers it
		/// as <paramref name="daoInterface"/> type.
		/// </summary>
		/// <param name="daoInterface">The DAO interface to register.</param>
		/// <param name="dao">The DAO type to register.</param>
		/// <exception cref="ArgumentNullException">Thrown if either param is null.</exception>
		/// <exception cref="ArgumentException">
		/// Thrown either if <see cref="IDataAccessObject"/>
		/// is not assignable from <paramref name="daoInterface"/>, OR, 
		/// <paramref name="daoInterface"/> is not assignable from <paramref name="dao"/>.
		/// </exception>
		/// <seealso cref="Register(Type, IDataAccessObject)"/>
		public void Register(Type daoInterface, Type dao, IDictionary<string, string> param)
		{
#warning TODO: use factory instead of these 2 methods
			if (IsDisposed)
			{
				throw new ObjectDisposedException("Database manager is disposed");
			}

			if (daoInterface == null)
			{
				throw new ArgumentNullException("daoInterface");
			}
			if (dao == null)
			{
				throw new ArgumentNullException("dao");
			}
			if (!typeof(IDataAccessObject).IsAssignableFrom(daoInterface))
			{
				throw new ArgumentException("The DAO interface " + daoInterface.FullName + " must derive from " +
											typeof(IDataAccessObject).FullName, "daoInterface");
			}
			if (!daoInterface.IsAssignableFrom(dao))
			{
				throw new ArgumentException("The DAO " + dao.FullName + " must derive from " + daoInterface.FullName, "dao");
			}

			IDataAccessObject obj = (IDataAccessObject) Activator.CreateInstance(dao, m_state);
			Register(daoInterface, obj);
		}

		/// <summary>
		/// Gets the data access object of this type and return it.
		/// </summary>
		/// <typeparam name="T">The type of data object to register.</typeparam>
		/// <returns>The DAO object.</returns>
		/// <exception cref="Exception">The type is not registered.</exception>
		/// <exception cref="Exception">Registered DAO does not derive from <typeparamref name="T"/>.</exception>
		public virtual T Using<T>()
			where T : class, IDataAccessObject
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException("Database manager is disposed");
			}

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
		/// Saves all data in registered DAOs, synchronous.
		/// </summary>
		public virtual void SaveAll()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException("Database manager is disposed");
			}
			
			IDataAccessObject[] daosCopy;
			lock (m_registeredDaos)
			{
				daosCopy = new IDataAccessObject[m_registeredDaos.Count];
				m_registeredDaos.Values.CopyTo(daosCopy, 0);
			}

			foreach (IDataAccessObject dataAccessObject in daosCopy)
			{
				try
				{
					dataAccessObject.SaveAll();
				}
				catch (Exception e)
				{
					log.Error("Error saving " + dataAccessObject.GetType().FullName, e);
				}
			}
		}

		/// <summary>
		/// Erases old database schemas and creates new ones.
		/// </summary>
		public virtual void CreateSchemas()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException("Database manager is disposed");
			}
#warning route to all registered DAOs
		}

		/// <summary>
		/// Verifies database schemas.
		/// </summary>
		/// <returns>Null if no errors or a list with error descriptions.</returns>
		public virtual IList<string> VerifySchemas()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException("Database manager is disposed");
			}
			
			List<string> errors = new List<string>();
			lock (m_registeredDaos)
			{
				foreach (KeyValuePair<Type, IDataAccessObject> keyValuePair in m_registeredDaos)
				{
					IList<string> daoErrors = keyValuePair.Value.VerifySchema();
					if (daoErrors != null)
					{
						errors.Add("");
						errors.Add("Problems with schema of " + keyValuePair.Value.GetType().FullName);
						errors.AddRange(daoErrors);
					}
				}
			}
			return errors.Count > 0 ? errors : null;
		}

		#region IDispose methods

		/// <summary>
		/// Holds the count of times this instance has been disposed.
		/// </summary>
		private int m_disposed;

		/// <summary>
		/// Gets a value indicating whether this instance is disposed.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is disposed; otherwise, <c>false</c>.
		/// </value>
		public bool IsDisposed
		{
			get { return m_disposed != 0; }
		}

		///<summary>
		///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		///</summary>
		///<filterpriority>2</filterpriority>
		public void Dispose()
		{
			if (Interlocked.Increment(ref m_disposed) == 1)
			{
				Dispose(false);
			}
		}

		/// <summary>
		///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <param name="finalize">Set to <c>true</c> if called from finalizer.</param>
		protected virtual void Dispose(bool finalize)
		{
			SaveAll();
			if (m_state != null)
			{
				m_state.Dispose();
			}
		}

		#endregion
		
		/// <summary>
		/// Initializes a new instance of the <see cref="DatabaseMgr"/> class.
		/// </summary>
		public DatabaseMgr()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DatabaseMgr"/> class.
		/// </summary>
		/// <param name="param">The params.</param>
		/// <param name="state">The state.</param>
		public DatabaseMgr(IDictionary<string, string> param, IDisposable state)
		{
			m_state = state;
		}
	}
}
