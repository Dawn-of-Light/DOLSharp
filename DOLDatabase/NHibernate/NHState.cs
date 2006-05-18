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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using NHibernate;
using NHibernate.Expression;
using NHConfiguration=NHibernate.Cfg.Configuration;

namespace DOL.Database.NHibernate
{
	/// <summary>
	/// The helper state object shared by all NHibernate DAOs.
	/// </summary>
	public class NHState : IDisposable
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
 
		/// <summary>
		/// The database session factory.
		/// </summary>
		private readonly ISessionFactory m_sessionFactory;

		/// <summary>
		/// The database configuration.
		/// </summary>
		private readonly NHConfiguration m_config;

		/// <summary>
		/// Gets the session factory.
		/// </summary>
		/// <value>The session factory.</value>
		public ISessionFactory SessionFactory
		{
			get { return m_sessionFactory; }
		}

		/// <summary>
		/// Gets the config.
		/// </summary>
		/// <value>The config.</value>
		public NHConfiguration Config
		{
			get { return m_config; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NHState"/> class.
		/// </summary>
		/// <param name="param">The params.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="param"/> is null.</exception>
		public NHState(IDictionary<string, string> param)
		{
			if (param == null)
			{
				throw new ArgumentNullException("param", "params can't be null");
			}

			if (log.IsInfoEnabled)
				log.Info("Loading server core database mapping ...");

			string configFile = param["config"];
			m_config = new NHConfiguration();
			m_config.Configure(configFile);

			if (log.IsInfoEnabled)
				log.Info("Creating database connection and instanciating caches ...");

			m_sessionFactory = m_config.BuildSessionFactory();
			if (m_sessionFactory == null)
			{
				throw new ApplicationException("Failed to create session factory");
			}
		}

		/// <summary>
		/// Only open a database session in order to have a direct access
		/// </summary>
		public ISession OpenSession()
		{
			try
			{
				return m_sessionFactory.OpenSession();
			}
			catch(Exception e)
			{
				throw new DolDatabaseException("Opening session", e);
			}
		}

		/// <summary>
		/// Close a session
		/// </summary>
		public void CloseSession(ISession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException("session");
			}
			
			try
			{
				session.Close();
			}
			catch(Exception e)
			{
				throw new DolDatabaseException("Closing session", e);
			}
		} 

		/// <summary>
		/// Adds a new persistent object to the database.
		/// </summary>
		public void AddNewObject(object o)
		{
			if (o == null)
			{
				throw new ArgumentNullException("o");
			}
			
			ISession session = null;
			ITransaction transaction = null;
			try
			{
				session = m_sessionFactory.OpenSession();
				transaction = session.BeginTransaction();
				session.Save(o);
				transaction.Commit();
			}
			catch(Exception e)
			{
				if (transaction != null)
					transaction.Rollback();

				throw new DolDatabaseException("Adding object " + o.ToString(), e);
			}
			finally
			{
				if (session != null)
					session.Close();
			}
		} 

		/// <summary>
		/// Delete a persistent object from the database.
		/// </summary>
		public void DeleteObject(object o)
		{
			if (o == null)
			{
				throw new ArgumentNullException("o");
			}
			
			ISession session = null;
			ITransaction transaction = null;
			try
			{
				session = m_sessionFactory.OpenSession();
				transaction = session.BeginTransaction();
				session.Delete(o);
				transaction.Commit();
			}
			catch(Exception e)
			{
				if (transaction != null)
					transaction.Rollback();

				throw new DolDatabaseException("Deleting object " + o.ToString(), e);
			}
			finally
			{
				if (session != null)
					session.Close();
			}
		} 

		/// <summary>
		/// Updata a persistant object.
		/// </summary>
		public void SaveObject(object o)
		{
			if (o == null)
			{
				throw new ArgumentNullException("o");
			}
			
			ISession session = null;
			ITransaction transaction = null;
			try
			{
				session = m_sessionFactory.OpenSession();
				transaction = session.BeginTransaction();
				session.Update(o);
				transaction.Commit();
			}
			catch(Exception e)
			{
				if (transaction != null)
					transaction.Rollback();
				
				throw new DolDatabaseException("Saving object " + o.ToString(), e);
			}
			finally
			{
				if (session != null)
					session.Close();
			}
		}

		/// <summary>
		/// Find a persistant object using its primary key.
		/// </summary>
		public object FindObjectByKey(Type type, object key)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			
			ISession session = null;
			try
			{
				session = m_sessionFactory.OpenSession();
				return session.Get(type, key);
			}
			catch(Exception e)
			{
				throw new DolDatabaseException("Selecting type " + type.FullName + " by key " + key.ToString(), e);
			}
			finally
			{
				if (session != null)
					session.Close();
			}
		} 

		/// <summary>
		/// Find all persistant object with the given type.
		/// </summary>
		public IList  SelectAllObjects(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			
			ISession session = null;
			try
			{
				session = m_sessionFactory.OpenSession();
				return session.CreateCriteria(type).List();
			}
			catch(Exception e)
			{
				throw new DolDatabaseException("Selecting all " + type.FullName, e);
			}
			finally
			{
				if (session != null)
					session.Close();
			} 
		}

		/// <summary>
		/// Find all persistant object using Criterion.
		/// </summary>
		public IList SelectObjects(Type objectType, ICriterion criterion)
		{
			return SelectObjects(objectType, criterion, null);
		}

		/// <summary>
		/// Find all persistant object using Criterion and sort it.
		/// </summary>
		public IList SelectObjects(Type objectType, ICriterion criterion, Order order)
		{
			if (objectType == null)
			{
				throw new ArgumentNullException("objectType");
			}
			if (criterion == null)
			{
				throw new ArgumentNullException("criterion");
			}
			if (order == null)
			{
				throw new ArgumentNullException("order");
			}

			ISession session = null;
			try
			{	
				session = m_sessionFactory.OpenSession();
				ICriteria criteria = session.CreateCriteria(objectType);
				criteria.Add(criterion);
				if(order != null) criteria.AddOrder(order);
				return criteria.List();
			}
			catch (Exception e)
			{
				throw new DolDatabaseException(
					"Select objects type " + objectType.FullName + ", criterion " + criterion.ToString() + ", order " +
					order.ToString(), e);
			}
			finally
			{
				if (session != null)
					session.Close();
			}
		}

		/// <summary>
		/// Find all persistant object using the HQL query.
		/// </summary>
		/// <param name="query">The query.</param>
		/// <returns>Selected objects.</returns>
		public IList SelectObjects(string query)
		{
			if (query == null)
			{
				throw new ArgumentNullException("query");
			}
			
			ISession session = null;
			try
			{
				session = m_sessionFactory.OpenSession();
				return session.CreateQuery(query).List();
			}
			catch(Exception e)
			{
				throw new DolDatabaseException("Select query: " + query, e);
			}
			finally
			{
				if (session != null)
				{
					session.Close();
				}
			} 
		}

		/// <summary>
		/// Find only one persistant object using the Criterion
		/// </summary>
		public object SelectObject(Type objectType, ICriterion criterion)
		{
			if (objectType == null)
			{
				throw new ArgumentNullException("objectType");
			}
			if (criterion == null)
			{
				throw new ArgumentNullException("criterion");
			}

			ISession session = null;
			try
			{
				session = m_sessionFactory.OpenSession();
				ICriteria criteria = session.CreateCriteria(objectType);
				criteria.Add(criterion);
				return criteria.UniqueResult();
			}
			catch (Exception e)
			{
				throw new DolDatabaseException("Select object type " + objectType.FullName + " criterion " + criterion.ToString(), e);
			}
			finally
			{
				if (session != null)
				{
					session.Close();
				}
			}
		}

		/// <summary>
		/// Find only one persistant object using the HQL query.
		/// </summary>
		/// <returns>Selected object.</returns>
		public object SelectObject(string query)
		{
			if (query == null)
			{
				throw new ArgumentNullException("query");
			}

			ISession session = null;
			try
			{
				session = m_sessionFactory.OpenSession();
				return session.CreateQuery(query).UniqueResult();
			}
			catch(Exception e)
			{
				throw new DolDatabaseException("Select query: " + query, e);
			}
			finally
			{
				if (session != null)
				{
					session.Close();
				}
			} 
		}

		/// <summary>
		/// Load a lazy collection of a specific object.
		/// </summary>
		/// <param name="obj">The object owning the lazy component to load.</param>
		/// <param name="component">The component to load, it can be a collection.</param>
		public void LoadLazyComponent(object obj, object component)
		{
			ISession session = null;
			try
			{
				session = m_sessionFactory.OpenSession();
				session.Lock(obj, LockMode.None);
				NHibernateUtil.Initialize(component);
			}
			catch(Exception e)
			{
				throw new DolDatabaseException("LoadLazyComponent", e);
			}
			finally
			{
				if (session != null)
				{
					session.Close();
				}
			} 
		}

		/// <summary>
		/// Find a persistant object using its primary key.
		/// </summary>
		/// <returns>Objects count.</returns>
		public int GetObjectCount(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}

			ISession session = null;
			try
			{
				session = m_sessionFactory.OpenSession();
				return (int) session.CreateQuery("SELECT COUNT(*) FROM "+type).UniqueResult();
			}
			catch(Exception e)
			{
				throw new DolDatabaseException("Getting object count for type " + type.FullName, e);
			}
			finally
			{
				if (session != null)
				{
					session.Close();
				}
			}
		}

		///<summary>
		///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		///</summary>
		///<filterpriority>2</filterpriority>
		public void Dispose()
		{
			m_sessionFactory.Close();
		}
	}
}
