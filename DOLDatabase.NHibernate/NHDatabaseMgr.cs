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
using System.Data;
using System.Reflection;
using System.Text;
using log4net;
using NHibernate.Dialect;
using NHibernate.Mapping;
using NHibernate.Tool.hbm2ddl;

namespace DOL.Database.NHibernate
{
	/// <summary>
	/// Implements NHibernate database methods.
	/// </summary>
	public class NHDatabaseMgr : DatabaseMgr
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Initializes a new instance of the <see cref="NHDatabaseMgr"/> class.
		/// </summary>
		/// <param name="param">The params.</param>
		/// <param name="state">The state.</param>
		public NHDatabaseMgr(IDictionary<string, string> param, NHState state) : base(param, state)
		{
		}

		/// <summary>
		/// Erases old database schemas and creates new ones.
		/// </summary>
		public override void CreateSchemas()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException("Database manager is disposed");
			}

			try
			{
				new SchemaExport(((NHState)m_state).Config)
					.Create(false, true);
			}
			catch (Exception e)
			{
				throw new DolDatabaseException("Creating schemas", e);
			}
		}

		/// <summary>
		/// Tests the current database structure against a NHibernate configuration
		/// </summary>
		public override IList<string> VerifySchemas()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException("Database manager is disposed");
			}

			try
			{
				IDbCommand statement = ((NHState)m_state).SessionFactory.ConnectionProvider.GetConnection().CreateCommand();
				statement.CommandType = CommandType.Text;
				statement.CommandText = "SHOW TABLES;";
				IDataReader reader;

				Dictionary<string, Dictionary<string, string>> tables = new Dictionary<string, Dictionary<string, string>>();
				using (reader = statement.ExecuteReader())
				while (reader.Read())
				{
					//Table names are case insensitive?
					string tablename = reader.GetString(0).ToLower();
					tables.Add(tablename, new Dictionary<string, string>());
				}


				List<string> errors = new List<string>();
				foreach (PersistentClass pClass in ((NHState)m_state).Config.ClassMappings)
				{
					string requiredTablename = pClass.Table.Name.ToLower();
					if (!tables.ContainsKey(requiredTablename))
					{
						String sentence = String.Format("Table: `{0}` is missing!", requiredTablename);
						if (!errors.Contains(sentence)) errors.Add(sentence);
					}
					else
					{
//						if (log.IsDebugEnabled)
//						{
//							log.DebugFormat("checking table '{0}'", requiredTablename);
//						}

						Dictionary<string, string> table = tables[requiredTablename];

						statement.CommandText = "SHOW COLUMNS IN `" + requiredTablename + "`;";
						
						using (reader = statement.ExecuteReader())
						while (reader.Read())
						{
							string field = reader.GetString(0).ToLower();
							string fieldType = reader.GetString(1);
//							if (log.IsDebugEnabled)
//							{
//								log.DebugFormat("  field '{0}'", field);
//							}
							
							if (!table.ContainsKey(field))
							{
								table.Add(field, fieldType);
							}
							else if (table[field] != fieldType)
							{
								throw new Exception("Different field types in table: '" + requiredTablename
								                    + "' field: '" + field
								                    + "' type1: '" + table[field]
								                    + "' type2: '" + fieldType + "'");
							}
						}

						foreach (Column requiredColumn in pClass.Table.ColumnCollection)
						{
							if (!table.ContainsKey(requiredColumn.Name.ToLower()))
							{
								String sentence = String.Format("Table: `{0}` Column: `{1}` Type: {2} is missing", requiredTablename, requiredColumn.Name, requiredColumn.GetSqlType(Dialect.GetDialect(((NHState)m_state).Config.Properties), null));
								if (!errors.Contains(sentence)) errors.Add(sentence);
							}
						}
					}
				}
				
				return errors.Count > 0 ? errors : null;
			}
			catch (Exception e)
			{
				throw new DolDatabaseException("Checking schemas", e);
			}
		}
	}
}
