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
using System.Data;
using System.Reflection;
using log4net;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Expression;
using NHibernate.Mapping;
using NHibernate.Dialect;
using NHibernate.Tool.hbm2ddl;

namespace DOL.Database
{
	/// <summary>
	/// Description résumée de Database.
	/// </summary>
	public class NHDatabase
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
 
		/// <summary>
		/// The database session factory
		/// </summary>
		protected ISessionFactory m_databaseSessionFactory = null;

		/// <summary>
		/// The database configuration (hold all tables structures ect ...)
		/// </summary>
		protected Configuration m_databaseConfiguration = null;

        #region singleton pattern
        static readonly NHDatabase instance=new NHDatabase();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static NHDatabase()
        {
        }

        NHDatabase()
        {
        }

        public static NHDatabase Instance
        {
            get
            {
                return instance;
            }
        }
        #endregion

        /// <summary>
        /// initialize the db layer
        /// </summary>
        /// <param name="configurationFile">the config file for core</param>
        /// <param name="assemblies">the assembly for scripts</param>
		public void Init(string configurationFile,ArrayList assemblies)
		{
			if (log.IsInfoEnabled)
				log.Info("Loading server core database mapping ...");

			m_databaseConfiguration = new Configuration();
			m_databaseConfiguration.Configure(configurationFile);

			if (log.IsInfoEnabled)
				log.Info("Loading scripts directory database mapping attributes ...");
	
			System.IO.MemoryStream stream = new System.IO.MemoryStream();
			NHibernate.Mapping.Attributes.HbmSerializer.Default.Validate = true;
            foreach (Assembly asm in assemblies)
            {
                NHibernate.Mapping.Attributes.HbmSerializer.Default.Serialize(stream, asm);   // here we load all mapping attributes of the GameServerScripts assembly
            }
			if(stream.Length > 0)
			{
				stream.Position = 0;
				m_databaseConfiguration.AddInputStream(stream);
			}
			stream.Close(); 

			if (log.IsInfoEnabled)
				log.Info("Creating database connection and instanciating caches ...");
	
			m_databaseSessionFactory = m_databaseConfiguration.BuildSessionFactory();
		}

		/// <summary>
		/// Erase and recreate all needed database table in the db
		/// </summary>
		/// <param name="configurationFile">The mapping configuration to use</param>
		public void CreateDatabaseStructure(string configurationFile)
		{
			if(m_databaseConfiguration != null)
				(new SchemaExport (m_databaseConfiguration)).Create(true,true);
		}

		/// <summary>
		/// Test the current database structure against a NHibernate configuration
		/// </summary>
		/// <param name="configurationFile">The mapping configuration to use</param>
		/// <param name="errors">A list that will be filled with error description</param>
		public bool TestDatabaseStructure(string configurationFile, IList errors)
		{
			if(m_databaseConfiguration != null)
			{
				IDbCommand statement = m_databaseSessionFactory.ConnectionProvider.GetConnection().CreateCommand();
				statement.CommandType = CommandType.Text;
				statement.CommandText = "SHOW TABLES;";
				IDataReader reader = statement.ExecuteReader();
			
				bool dbOK = true;

				Hashtable tables = new Hashtable();
				while(reader.Read()) 
				{   
					//Table names are case insensitive?
					string tablename = reader.GetString(0).ToLower();
					tables[tablename] = new Hashtable();
				}
				reader.Close();

				foreach(PersistentClass pClass in m_databaseConfiguration.ClassMappings)
				{
					string requiredTablename = pClass.Table.Name.ToLower();
					if(!tables.ContainsKey(requiredTablename))
					{
						String sentence = String.Format("Table: `{0}` is missing!", requiredTablename);
						if(!errors.Contains(sentence)) errors.Add(sentence);
						dbOK = false;
					}
					else
					{
						Hashtable table = (Hashtable) tables[requiredTablename];

						statement.CommandText = "SHOW COLUMNS IN `"+requiredTablename+"`;";
						reader = statement.ExecuteReader();
					
						while(reader.Read())
						{
							table[reader.GetString(0).ToLower()] = reader.GetString(1);
						}
						reader.Close();
					
						foreach(Column requiredColumn in pClass.Table.ColumnCollection)
						{
							if(!table.ContainsKey(requiredColumn.Name.ToLower()))
							{
								String sentence = String.Format("Table: `{0}` Column: `{1}` Type: {2} is missing", requiredTablename, requiredColumn.Name, requiredColumn.GetSqlType(Dialect.GetDialect(m_databaseConfiguration.Properties), null));
								if(!errors.Contains(sentence)) errors.Add(sentence);
								dbOK = false;
							}
						}	
					}
				}
				return dbOK;
			}
			return true;
		}

		/// <summary>
		/// Only open a database session in order to have a direct access
		/// </summary>
		public ISession OpenSession()
		{
			try
			{
				return m_databaseSessionFactory.OpenSession();
			}
			catch(Exception e)
			{
				throw e;
			}
		}

		/// <summary>
		/// Close a session
		/// </summary>
		public void CloseSession(ISession session)
		{
			try
			{
				if (session != null)
				{
					session.Close();
				}
			}
			catch(Exception e)
			{
				throw e;
			}
		} 

		/// <summary>
		/// Add a new persistent object to the database
		/// </summary>
		public void AddNewObject(object o)
		{
			ITransaction transaction = null;
			ISession session = null;
			try
			{
				session = m_databaseSessionFactory.OpenSession();
				transaction = session.BeginTransaction();
				session.Save(o);
				transaction.Commit();
			}
			catch(Exception e)
			{
				transaction.Rollback();
				throw e;
			}
			finally
			{
				session.Close();
			}
		} 

		/// <summary>
		/// Delete a persistent object from the database
		/// </summary>
		public void DeleteObject(object o)
		{
			ITransaction transaction = null;
			ISession session = null;
			try
			{
				session = m_databaseSessionFactory.OpenSession();
				transaction = session.BeginTransaction();
				session.Delete(o);
				transaction.Commit();
			}
			catch(Exception e)
			{
				transaction.Rollback();
				throw e;
			}
			finally
			{
				session.Close();
			}
		} 

		/// <summary>
		/// Updata a persistant object
		/// </summary>
		public void SaveObject(object o)
		{
			ITransaction transaction = null;
			ISession session = null;
			try
			{
				session = m_databaseSessionFactory.OpenSession();
				transaction = session.BeginTransaction();
				session.Update(o);
				transaction.Commit();
			}
			catch(Exception e)
			{
				transaction.Rollback();
				throw e;
			}
			finally
			{
				session.Close();
			}
		}

		/// <summary>
		/// Find a persistant object using its primary key
		/// </summary>
		public object FindObjectByKey(Type type, object key)
		{
			ISession session = null;
			try
			{
				session = m_databaseSessionFactory.OpenSession();
				return session.Get(type, key);
			}
			catch(Exception e)
			{
				throw e;
			}
			finally
			{
				session.Close();
			}
		} 

		/// <summary>
		/// Find all persistant object with the given type
		/// </summary>
		public IList  SelectAllObjects(Type type)
		{
			ISession session = null;
			try
			{
				session = m_databaseSessionFactory.OpenSession();
				return session.CreateCriteria(type).List();
			}
			catch(Exception e)
			{
				throw e;
			}
			finally
			{
				session.Close();
			} 
		}

		/// <summary>
		/// Find all persistant object using Criterion
		/// </summary>
		public IList SelectObjects(Type objectType, ICriterion criterion)
		{
			return SelectObjects(objectType, criterion, null);
		}

		/// <summary>
		/// Find all persistant object using Criterion and sort it
		/// </summary>
		public IList SelectObjects(Type objectType, ICriterion criterion, Order order)
		{
			ISession session = null;
			try
			{	
				session = m_databaseSessionFactory.OpenSession();
				ICriteria criteria = session.CreateCriteria(objectType);
				criteria.Add(criterion);
				if(order != null) criteria.AddOrder(order);
				return criteria.List();
			}
			catch (Exception e)
			{
				throw e;
			}
			finally
			{
				session.Close();
			}
		}

		/// <summary>
		/// Find all persistant object using the HQL query
		/// </summary>
		public IList SelectObjects(string query)
		{
			ISession session = null;
			try
			{
				session = m_databaseSessionFactory.OpenSession();
				return session.CreateQuery(query).List();
			}
			catch(Exception e)
			{
				throw e;
			}
			finally
			{
				session.Close();
			} 
		}

		/// <summary>
		/// Find only one persistant object using the Criterion
		/// </summary>
		public object SelectObject(Type objectType, ICriterion criterion)
		{
			ISession session = null;
			try
			{
				session = m_databaseSessionFactory.OpenSession();
				ICriteria criteria = session.CreateCriteria(objectType);
				criteria.Add(criterion);
				return criteria.UniqueResult();
			}
			catch (Exception e)
			{
				throw e;
			}
			finally
			{
				session.Close();
			}
		}

		/// <summary>
		/// Find only one persistant object using the HQL query
		/// </summary>
		public object SelectObject(string query)
		{
			ISession session = null;
			try
			{
				session = m_databaseSessionFactory.OpenSession();
				return session.CreateQuery(query).UniqueResult();
			}
			catch(Exception e)
			{
				throw e;
			}
			finally
			{
				session.Close();
			} 
		}

		/// <summary>
		/// Load a lazy collection of a specific object
		/// </summary>
		/// <param name="obj">The object owning the lazy component to load</param>
		/// <param name="component">The component to load, it can be a collection</param>
		public void LoadLazyComponent(object obj, object component)
		{
			ISession session = null;
			try
			{
				session = m_databaseSessionFactory.OpenSession();
				session.Lock(obj, NHibernate.LockMode.None);
				NHibernateUtil.Initialize(component);
			}
			catch(Exception e)
			{
				throw e;
			}
			finally
			{
				session.Close();
			} 
		}

		/// <summary>
		/// Find a persistant object using its primary key
		/// </summary>
		public int GetObjectCount(Type type)
		{
			ISession session = null;
			try
			{
				session = m_databaseSessionFactory.OpenSession();
				return (int) session.CreateQuery("SELECT COUNT(*) FROM "+type).UniqueResult();
			}
			catch(Exception e)
			{
				throw e;
			}
			finally
			{
				session.Close();
			}
		}
	}
}
