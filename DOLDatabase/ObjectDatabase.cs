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
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Reflection;
using DataTable = System.Data.DataTable;

using DOL.Database.Attributes;
using DOL.Database.Cache;
using DOL.Database.Connection;
using DOL.Database.Handlers;

using log4net;

namespace DOL.Database
{
	/// <summary>
	/// Database to to full Dokumentation
	/// </summary>
	public abstract class ObjectDatabase : IObjectDatabase
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Number Format Info to Use for Database
		/// </summary>
		protected static readonly NumberFormatInfo Nfi = new CultureInfo("en-US", false).NumberFormat;
		
		// TODO Remove
		private readonly Dictionary<Type, ConstructorInfo> ConstructorByFieldType = new Dictionary<Type, ConstructorInfo>();
		private readonly Dictionary<Type, MemberInfo[]> MemberInfoCache = new Dictionary<Type, MemberInfo[]>();
		private readonly Dictionary<MemberInfo, Relation[]> RelationAttributes = new Dictionary<MemberInfo, Relation[]>();

		/// <summary>
		/// Data Table Handlers for this Database Handler
		/// </summary>
		protected readonly Dictionary<string, DataTableHandler> TableDatasets = new Dictionary<string, DataTableHandler>();

		/// <summary>
		/// Connection String for this Database
		/// </summary>
		protected string ConnectionString { get; set; }
		
		/// <summary>
		/// Creates a new Instance of <see cref="ObjectDatabase"/>
		/// </summary>
		/// <param name="ConnectionString">Database Connection String</param>
		protected ObjectDatabase(string ConnectionString)
		{
			this.ConnectionString = ConnectionString;
		}

		#region Data tables

		protected DataSet GetDataSet(string tableName)
		{
			if (!TableDatasets.ContainsKey(tableName))
				return null;

			return TableDatasets[tableName].DataSet;
		}

		protected void FillObjectWithRow<TObject>(ref TObject dataObject, DataRow row, bool reload)
			where TObject : DataObject
		{
			bool relation = false;

			string tableName = dataObject.TableName;
			Type myType = dataObject.GetType();
			string id = row[tableName + "_ID"].ToString();

			MemberInfo[] myMembers = myType.GetMembers();
			dataObject.ObjectId = id;

			for (int i = 0; i < myMembers.Length; i++)
			{
				object[] myAttributes = GetRelationAttributes(myMembers[i]);

				if (myAttributes.Length > 0)
				{
					//if(myAttributes[0] is Attributes.Relation)
					//{
					relation = true;
					//}
				}
				else
				{
					object[] keyAttrib = myMembers[i].GetCustomAttributes(typeof(PrimaryKey), true);
					myAttributes = myMembers[i].GetCustomAttributes(typeof(DataElement), true);
					if (myAttributes.Length > 0 || keyAttrib.Length > 0)
					{
						object val = row[myMembers[i].Name];
						if (val != null && !val.GetType().IsInstanceOfType(DBNull.Value))
						{
							if (myMembers[i] is PropertyInfo)
							{
								((PropertyInfo)myMembers[i]).SetValue(dataObject, val, null);
							}
							if (myMembers[i] is FieldInfo)
							{
								((FieldInfo)myMembers[i]).SetValue(dataObject, val);
							}
						}
					}
				}
			}

			dataObject.Dirty = false;


			if (relation)
			{
				FillLazyObjectRelations(dataObject, true);
			}

			dataObject.IsPersisted = true;
		}

		protected void FillRowWithObject(DataObject dataObject, DataRow row)
		{
			bool relation = false;

			Type myType = dataObject.GetType();

			row[dataObject.TableName + "_ID"] = dataObject.ObjectId;

			MemberInfo[] myMembers = myType.GetMembers();

			for (int i = 0; i < myMembers.Length; i++)
			{
				object[] myAttributes = GetRelationAttributes(myMembers[i]);
				object val = null;

				if (myAttributes.Length > 0)
				{
					relation = true;
				}
				else
				{
					myAttributes = myMembers[i].GetCustomAttributes(typeof(DataElement), true);
					object[] keyAttrib = myMembers[i].GetCustomAttributes(typeof(PrimaryKey), true);

					if (myAttributes.Length > 0 || keyAttrib.Length > 0)
					{
						if (myMembers[i] is PropertyInfo)
						{
							val = ((PropertyInfo)myMembers[i]).GetValue(dataObject, null);
						}
						if (myMembers[i] is FieldInfo)
						{
							val = ((FieldInfo)myMembers[i]).GetValue(dataObject);
						}
						if (val != null)
						{
							row[myMembers[i].Name] = val;
						}
					}
				}
				//}
			}

			if (relation)
			{
				SaveObjectRelations(dataObject);
			}
		}

		protected DataRow FindRowByKey(DataObject dataObject)
		{
			DataRow row;

			string tableName = dataObject.TableName;


			DataTable table = GetDataSet(tableName).Tables[tableName];

			Type myType = dataObject.GetType();

			string key = table.PrimaryKey[0].ColumnName;

			if (key.Equals(tableName + "_ID"))
				row = table.Rows.Find(dataObject.ObjectId);
			else
			{
				MemberInfo[] keymember = myType.GetMember(key);

				object val = null;

				if (keymember[0] is PropertyInfo)
					val = ((PropertyInfo)keymember[0]).GetValue(dataObject, null);
				if (keymember[0] is FieldInfo)
					val = ((FieldInfo)keymember[0]).GetValue(dataObject);

				if (val != null)
					row = table.Rows.Find(val);
				else
					return null;
			}

			return row;
		}

		#endregion

		#region Public API

		/// <summary>
		/// insert a new object into the db and save it
		/// </summary>
		/// <param name="dataObject"></param>
		public bool AddObject(DataObject dataObject)
		{
			if (dataObject.AllowAdd)
			{
				return AddObjectImpl(dataObject);
			}
			
			if (Log.IsWarnEnabled)
				Log.WarnFormat("AddObject called on DataObject when AllowAdd is False: {0} : {1}", dataObject.TableName, dataObject.ObjectId);
			
			return false;
		}

		/// <summary>
		/// Saves an object to db if saving is allowed and object is dirty
		/// </summary>
		/// <param name="dataObject"></param>
		public bool SaveObject(DataObject dataObject)
		{
			if (dataObject.Dirty)
			{
				return SaveObjectImpl(dataObject);
			}

			return true;
		}

		/// <summary>
		/// delete object from db
		/// </summary>
		/// <param name="dataObject"></param>
		public bool DeleteObject(DataObject dataObject)
		{
			if (dataObject.AllowDelete)
			{
				return DeleteObjectImpl(dataObject);
			}

			return false;
		}

		public int GetObjectCount<TObject>()
			where TObject : DataObject
		{
			return GetObjectCount<TObject>("");
		}

		public int GetObjectCount<TObject>(string whereExpression)
			where TObject : DataObject
		{
			return GetObjectCountImpl<TObject>(whereExpression);
		}

		public TObject FindObjectByKey<TObject>(object key)
			where TObject : DataObject
		{
			var dataObject = FindObjectByKeyImpl<TObject>(key);

			return dataObject;
		}


		/// <summary>
		/// Selects a single object, if more than
		/// one exist, the first is returned
		/// </summary>
		/// <param name="objectType">the type of the object</param>
		/// <param name="statement">the select statement</param>
		/// <returns>the object or null if none found</returns>
		public TObject SelectObject<TObject>(string whereExpression)
			where TObject : DataObject
		{
			return SelectObject<TObject>(whereExpression, Transaction.IsolationLevel.DEFAULT);
		}

		/// <summary>
		/// Selects a single object, if more than
		/// one exist, the first is returned
		/// </summary>
		/// <typeparam name="TObject"></typeparam>
		/// <param name="whereExpression"></param>
		/// <param name="isolation"></param>
		/// <returns></returns>
		public TObject SelectObject<TObject>(string whereExpression, Transaction.IsolationLevel isolation)
			where TObject : DataObject
		{
			var objs = SelectObjects<TObject>(whereExpression, isolation);

			if (objs.Count > 0)
				return objs[0];

			return null;
		}

		public IList<TObject> SelectObjects<TObject>(string whereExpression)
			where TObject : DataObject
		{
			return SelectObjects<TObject>(whereExpression, Transaction.IsolationLevel.DEFAULT);
		}

		public IList<TObject> SelectObjects<TObject>(string whereExpression, Transaction.IsolationLevel isolation)
			where TObject : DataObject
		{
			var dataObjects = SelectObjectsImpl<TObject>(whereExpression, isolation);

			return dataObjects ?? new List<TObject>();
		}

		public IList<TObject> SelectAllObjects<TObject>()
			where TObject : DataObject
		{
			return SelectAllObjects<TObject>(Transaction.IsolationLevel.DEFAULT);
		}

		public IList<TObject> SelectAllObjects<TObject>(Transaction.IsolationLevel isolation)
			where TObject : DataObject
		{
			var dataObjects = SelectAllObjectsImpl<TObject>(isolation);

			return dataObjects ?? new List<TObject>();
		}

		/// <summary>
		/// Register Data Object Type if not already Registered
		/// </summary>
		/// <param name="objType">DataObject Type</param>
		public virtual void RegisterDataObject(Type objType)
		{
			var tableName = AttributesUtils.GetTableOrViewName(objType);
			if (TableDatasets.ContainsKey(tableName))
				return;
			
			var dataTableHandler = new DataTableHandler(objType);
			TableDatasets.Add(tableName, dataTableHandler);

			// TODO get rid of this or implement it properly
			//if (dth.UsesPreCaching && Connection.IsSQLConnection)
			//{
			//    // not useful for xml connection
			//    if (Log.IsDebugEnabled)
			//        Log.Debug("Precaching of " + table.TableName + "...");

			//    var objects = SQLSelectObjects<TObject>("");

			//    object key;
			//    for (int i = 0; i < objects.Length; i++)
			//    {
			//        key = null;
			//        if (primaryIndexMember == null)
			//        {
			//            key = objects[i].ObjectId;
			//        }
			//        else
			//        {
			//            if (primaryIndexMember is PropertyInfo)
			//            {
			//                key = ((PropertyInfo) primaryIndexMember).GetValue(objects[i], null);
			//            }
			//            else if (primaryIndexMember is FieldInfo)
			//            {
			//                key = ((FieldInfo) primaryIndexMember).GetValue(objects[i]);
			//            }
			//        }
			//        if (key != null)
			//        {
			//            dth.SetPreCachedObject(key, objects[i]);
			//        }
			//        else
			//        {
			//            if (Log.IsErrorEnabled)
			//                Log.Error("Primary key is null! " + ((primaryIndexMember != null) ? primaryIndexMember.Name : ""));
			//        }
			//    }

			//    if (Log.IsDebugEnabled)
			//        Log.Debug("Precaching of " + table.TableName + " finished!");
			//}
		}

		/// <summary>
		/// escape the strange character from string
		/// </summary>
		/// <param name="rawInput">the string</param>
		/// <returns>the string with escaped character</returns>
		public abstract string Escape(string rawInput);
		
		/// <summary>
		/// Execute a Raw Non-Query on the Database
		/// </summary>
		/// <param name="rawQuery">Raw Command</param>
		/// <returns>True if the Command succeeded</returns>
		public virtual bool ExecuteNonQuery(string rawQuery)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Implementation

		/// <summary>
		/// Adds a new object to the database.
		/// </summary>
		/// <param name="dataObject">the object to add to the database</param>
		/// <returns>true if the object was added successfully; false otherwise</returns>
		protected abstract bool AddObjectImpl(DataObject dataObject);

		/// <summary>
		/// Persists an object to the database.
		/// </summary>
		/// <param name="dataObject">the object to save to the database</param>
		protected abstract bool SaveObjectImpl(DataObject dataObject);

		/// <summary>
		/// Deletes an object from the database.
		/// </summary>
		/// <param name="dataObject">the object to delete from the database</param>
		protected abstract bool DeleteObjectImpl(DataObject dataObject);

		/// <summary>
		/// Finds an object in the database by primary key.
		/// </summary>
		/// <typeparam name="TObject">the type of objects to retrieve</typeparam>
		/// <param name="key">the value of the primary key to search for</param>
		/// <returns>a <see cref="DataObject" /> instance representing a row with the given primary key value; null if the key value does not exist</returns>
		protected abstract TObject FindObjectByKeyImpl<TObject>(object key)
			where TObject : DataObject;

		/// <summary>
		/// Finds an object in the database by primary key.
		/// Uses cache if available
		/// </summary>
		/// <param name="objectType"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		protected abstract DataObject FindObjectByKeyImpl(Type objectType, object key);

		/// <summary>
		/// Selects objects from a given table in the database based on a given set of criteria. (where clause)
		/// </summary>
		/// <param name="objectType"></param>
		/// <param name="whereClause"></param>
		/// <param name="isolation"></param>
		/// <returns></returns>
		protected abstract DataObject[] SelectObjectsImpl(Type objectType, string whereClause, Transaction.IsolationLevel isolation);

		/// <summary>
		/// Selects objects from a given table in the database based on a given set of criteria. (where clause)
		/// </summary>
		/// <typeparam name="TObject">the type of objects to retrieve</typeparam>
		/// <param name="whereClause">the where clause to filter object selection on</param>
		/// <returns>an array of <see cref="DataObject" /> instances representing the selected objects that matched the given criteria</returns>
		/// <returns></returns>
		protected abstract IList<TObject> SelectObjectsImpl<TObject>(string whereClause, Transaction.IsolationLevel isolation)
			where TObject : DataObject;

		/// <summary>
		/// Selects all objects from a given table in the database.
		/// </summary>
		/// <typeparam name="TObject">the type of objects to retrieve</typeparam>
		/// <returns>an array of <see cref="DataObject" /> instances representing the selected objects</returns>
		/// <returns></returns>
		protected abstract IList<TObject> SelectAllObjectsImpl<TObject>(Transaction.IsolationLevel isolation)
			where TObject : DataObject;

		/// <summary>
		/// Gets the number of objects in a given table in the database based on a given set of criteria. (where clause)
		/// </summary>
		/// <typeparam name="TObject">the type of objects to retrieve</typeparam>
		/// <param name="whereExpression">the where clause to filter object count on</param>
		/// <returns>a positive integer representing the number of objects that matched the given criteria; zero if no such objects existed</returns>
		protected abstract int GetObjectCountImpl<TObject>(string whereExpression)
			where TObject : DataObject;

		#endregion

		#region Relations

		public void FillObjectRelations(DataObject dataObject)
		{
			FillLazyObjectRelations(dataObject, false);
		}

		protected void SaveObjectRelations(DataObject dataObject)
		{
			try
			{
				object val;

				Type myType = dataObject.GetType();

				MemberInfo[] myMembers = myType.GetMembers();

				for (int i = 0; i < myMembers.Length; i++)
				{
					Relation[] myAttributes = GetRelationAttributes(myMembers[i]);
					if (myAttributes.Length > 0)
					{
						//if(myAttributes[0] is Attributes.Relation)
						//{
						bool array = false;

						Type type;

						if (myMembers[i] is PropertyInfo)
							type = ((PropertyInfo)myMembers[i]).PropertyType;
						else
							type = ((FieldInfo)myMembers[i]).FieldType;

						if (type.HasElementType)
						{
							type = type.GetElementType();
							array = true;
						}

						val = null;

						if (array)
						{
							if (myMembers[i] is PropertyInfo)
							{
								val = ((PropertyInfo)myMembers[i]).GetValue(dataObject, null);
							}
							if (myMembers[i] is FieldInfo)
							{
								val = ((FieldInfo)myMembers[i]).GetValue(dataObject);
							}
							if (val is Array)
							{
								var a = val as Array;

								foreach (object o in a)
								{
									if (o is DataObject)
										SaveObject(o as DataObject);
								}
							}
							else
							{
								if (val is DataObject)
									SaveObject(val as DataObject);
							}
						}
						else
						{
							if (myMembers[i] is PropertyInfo)
								val = ((PropertyInfo)myMembers[i]).GetValue(dataObject, null);
							if (myMembers[i] is FieldInfo)
								val = ((FieldInfo)myMembers[i]).GetValue(dataObject);
							if (val != null && val is DataObject)
								SaveObject(val as DataObject);
						}
					}
					//}
				}
			}
			catch (Exception e)
			{
				throw new DatabaseException("Saving Relations failed !", e);
			}
		}

		protected void DeleteObjectRelations(DataObject dataObject)
		{
			try
			{
				object val;

				Type myType = dataObject.GetType();

				MemberInfo[] myMembers = myType.GetMembers();

				for (int i = 0; i < myMembers.Length; i++)
				{
					Relation[] myAttributes = GetRelationAttributes(myMembers[i]);
					if (myAttributes.Length > 0)
					{
						//if(myAttributes[0] is Attributes.Relation)
						//{
						if (myAttributes[0].AutoDelete == false)
							continue;

						bool array = false;

						Type type;

						if (myMembers[i] is PropertyInfo)
							type = ((PropertyInfo)myMembers[i]).PropertyType;
						else
							type = ((FieldInfo)myMembers[i]).FieldType;

						if (type.HasElementType)
						{
							type = type.GetElementType();
							array = true;
						}

						val = null;

						if (array)
						{
							if (myMembers[i] is PropertyInfo)
							{
								val = ((PropertyInfo)myMembers[i]).GetValue(dataObject, null);
							}
							if (myMembers[i] is FieldInfo)
							{
								val = ((FieldInfo)myMembers[i]).GetValue(dataObject);
							}
							if (val is Array)
							{
								var a = val as Array;

								foreach (object o in a)
								{
									if (o is DataObject)
										DeleteObject(o as DataObject);
								}
							}
							else
							{
								if (val is DataObject)
									DeleteObject(val as DataObject);
							}
						}
						else
						{
							if (myMembers[i] is PropertyInfo)
								val = ((PropertyInfo)myMembers[i]).GetValue(dataObject, null);
							if (myMembers[i] is FieldInfo)
								val = ((FieldInfo)myMembers[i]).GetValue(dataObject);
							if (val != null && val is DataObject)
								DeleteObject(val as DataObject);
						}
						//}
					}
				}
			}
			catch (Exception e)
			{
				throw new DatabaseException("Resolving Relations failed !", e);
			}
		}

		protected void FillLazyObjectRelations(DataObject dataObject, bool autoload)
		{
			try
			{
				var dataObjectType = dataObject.GetType();

				MemberInfo[] myMembers;
				if (!MemberInfoCache.TryGetValue(dataObjectType, out myMembers))
				{
					myMembers = dataObjectType.GetMembers();
					MemberInfoCache[dataObjectType] = myMembers;
				}

				for (int i = 0; i < myMembers.Length; i++)
				{
					Relation[] myAttributes = GetRelationAttributes(myMembers[i]);

					if (myAttributes.Length > 0)
					{
						Relation rel = myAttributes[0];

						if ((rel.AutoLoad == false) && autoload)
							continue;

						bool isArray = false;
						Type remoteType;
						DataObject[] elements;

						string local = rel.LocalField;
						string remote = rel.RemoteField;

						if (myMembers[i] is PropertyInfo)
						{
							remoteType = ((PropertyInfo)myMembers[i]).PropertyType;
						}
						else
						{
							remoteType = ((FieldInfo) myMembers[i]).FieldType;
						}

						if (remoteType.HasElementType)
						{
							remoteType = remoteType.GetElementType();
							isArray = true;
						}

						PropertyInfo prop = dataObjectType.GetProperty(local);
						FieldInfo field = dataObjectType.GetField(local);

						object val = 0;

						if (prop != null)
						{
							val = prop.GetValue(dataObject, null);
						}
						if (field != null)
						{
							val = field.GetValue(dataObject);
						}

						if (val != null && val.ToString() != string.Empty)
						{
							if (AttributesUtils.GetPreCachedFlag(remoteType))
							{
								elements = new DataObject[1];
								elements[0] = FindObjectByKeyImpl(remoteType, val);
							}
							else
							{
								elements = SelectObjectsImpl(remoteType, remote + " = '" + Escape(val.ToString()) + "'", Transaction.IsolationLevel.DEFAULT);
							}

							if ((elements != null) && (elements.Length > 0))
							{
								if (isArray)
								{
									if (myMembers[i] is PropertyInfo)
									{
										((PropertyInfo) myMembers[i]).SetValue(dataObject, elements, null);
									}
									if (myMembers[i] is FieldInfo)
									{
										var currentField = (FieldInfo) myMembers[i];
										ConstructorInfo constructor;
										if (!ConstructorByFieldType.TryGetValue(currentField.FieldType, out constructor))
										{
											constructor = currentField.FieldType.GetConstructor(new[] {typeof (int)});
											ConstructorByFieldType[currentField.FieldType] = constructor;
										}

										object elementHolder = constructor.Invoke(new object[] {elements.Length});
										var elementArray = (object[]) elementHolder;

										for (int m = 0; m < elementArray.Length; m++)
										{
											elementArray[m] = elements[m];
										}

										currentField.SetValue(dataObject, elementArray);
									}
								}
								else
								{
									if (myMembers[i] is PropertyInfo)
									{
										((PropertyInfo) myMembers[i]).SetValue(dataObject, elements[0], null);
									}
									if (myMembers[i] is FieldInfo)
									{
										((FieldInfo) myMembers[i]).SetValue(dataObject, elements[0]);
									}
								}
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				throw new DatabaseException("Resolving Relations for " + dataObject.TableName + " failed!", e);
			}
		}

		#endregion

		#region Cache

		protected void DeleteFromCache(string tableName, DataObject obj)
		{
			DataTableHandler handler = TableDatasets[tableName];
			handler.SetCacheObject(obj.ObjectId, null);
		}

		/// <summary>
		/// Selects object from the db and updates or adds entry in the pre-cache
		/// </summary>
		/// <param name="objectType"></param>
		/// <param name="key"></param>
		public bool UpdateInCache<TObject>(object key)
			where TObject : DataObject
		{
			MemberInfo[] members = typeof(TObject).GetMembers();
			var ret = (TObject)Activator.CreateInstance(typeof(TObject));

			string tableName = ret.TableName;
			DataTableHandler dth = TableDatasets[tableName];
			string whereClause = null;

			if (!dth.UsesPreCaching || key == null)
				return false;

			// Escape PK value
			key = Escape(key.ToString());

			for (int i = 0; i < members.Length; i++)
			{
				object[] keyAttrib = members[i].GetCustomAttributes(typeof(PrimaryKey), true);
				if (keyAttrib.Length > 0)
				{
					whereClause = "`" + members[i].Name + "` = '" + key + "'";
					break;
				}
			}

			if (whereClause == null)
			{
				whereClause = "`" + ret.TableName + "_ID` = '" + key + "'";
			}

			var objs = SelectObjects<TObject>(whereClause);
			if (objs.Count > 0)
			{
				dth.SetPreCachedObject(key, objs[0]);
				return true;
			}

			return false;
		}

		protected void ReloadCache(string tableName)
		{
			DataTableHandler handler = TableDatasets[tableName];

			ICache cache = handler.Cache;

			foreach (object o in cache.Keys)
			{
				ReloadObject(cache[o] as DataObject);
			}
		}

		#endregion

		#region Helpers

		protected Relation[] GetRelationAttributes(MemberInfo info)
		{
			Relation[] rel;
			if (RelationAttributes.TryGetValue(info, out rel))
				return rel;

			rel = (Relation[])info.GetCustomAttributes(typeof(Relation), true);
			RelationAttributes[info] = rel;

			return rel;
		}

		private DataObject ReloadObject(DataObject dataObject)
		{
			try
			{
				if (dataObject == null)
					return null;

				DataObject ret = dataObject;

				DataRow row = FindRowByKey(ret);

				if (row == null)
					throw new DatabaseException("Reloading Databaseobject failed (Keyvalue Changed ?)!");

				FillObjectWithRow(ref ret, row, true);

				dataObject.Dirty = false;
				dataObject.IsPersisted = true;

				return ret;
			}
			catch (Exception e)
			{
				throw new DatabaseException("Reloading Databaseobject failed !", e);
			}
		}

		#endregion

		#region Factory

		public static IObjectDatabase GetObjectDatabase(ConnectionType connectionType, string connectionString)
		{
			if (connectionType == ConnectionType.DATABASE_MYSQL)
				return new MySQLObjectDatabase(connectionString);
			if (connectionType == ConnectionType.DATABASE_SQLITE)
				return new SQLiteObjectDatabase(connectionString);

			return null;
		}

		#endregion
	}
}