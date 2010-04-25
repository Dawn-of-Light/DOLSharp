using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using DOL.Database.Attributes;
using DOL.Database.Connection;
using DOL.Database.UniqueID;
using MySql.Data.MySqlClient;
using MySql.Data.Types;

namespace DOL.Database.Handlers
{
	public class MySQLObjectDatabase : ObjectDatabase
	{
		public MySQLObjectDatabase(DataConnection connection)
			: base(connection)
		{
		}

		#region SQL implementation

		/// <summary>
		/// Adds a new object to the database.
		/// </summary>
		/// <param name="dataObject">the object to add to the database</param>
		/// <returns>true if the object was added successfully; false otherwise</returns>
		protected override bool AddObjectImpl(DataObject dataObject)
		{
			try
			{
				string tableName = dataObject.TableName;

				if (dataObject.ObjectId == null)
				{
					dataObject.ObjectId = IDGenerator.GenerateID();
				}

				var columns = new StringBuilder();
				var values = new StringBuilder();

				MemberInfo[] objMembers = dataObject.GetType().GetMembers();
				bool hasRelations = false;
				string dateFormat = Connection.GetDBDateFormat();

				columns.Append("`" + tableName + "_ID`");
				values.Append("'" + Escape(dataObject.ObjectId) + "'");

				for (int i = 0; i < objMembers.Length; i++)
				{
					if (!hasRelations)
					{
						object[] relAttrib = GetRelationAttributes(objMembers[i]);
						hasRelations = relAttrib.Length > 0;
					}
					object[] keyAttrib = objMembers[i].GetCustomAttributes(typeof(PrimaryKey), true);
					object[] attrib = objMembers[i].GetCustomAttributes(typeof(DataElement), true);
					if (attrib.Length > 0 || keyAttrib.Length > 0)
					{
						object val = null;
						if (objMembers[i] is PropertyInfo)
						{
							val = ((PropertyInfo)objMembers[i]).GetValue(dataObject, null);
						}
						else if (objMembers[i] is FieldInfo)
						{
							val = ((FieldInfo)objMembers[i]).GetValue(dataObject);
						}

						columns.Append(", ");
						values.Append(", ");
						columns.Append("`" + objMembers[i].Name + "`");

						if (val is bool)
						{
							val = ((bool)val) ? (byte)1 : (byte)0;
						}
						else if (val is DateTime)
						{
							val = ((DateTime)val).ToString(dateFormat);
						}
						else if (val is float)
						{
							val = ((float)val).ToString(Nfi);
						}
						else if (val is double)
						{
							val = ((double)val).ToString(Nfi);
						}
						else if (val is string)
						{
							val = Escape(val.ToString());
						}

						values.Append('\'');
						values.Append(val);
						values.Append('\'');
					}
				}

				string sql = "INSERT INTO `" + tableName + "` (" + columns + ") VALUES (" + values + ")";

				if (Log.IsDebugEnabled)
					Log.Debug(sql);

				int res = Connection.ExecuteNonQuery(sql);
				if (res == 0)
				{
					if (Log.IsErrorEnabled)
						Log.Error("Error adding object into " + dataObject.TableName + " ID=" + dataObject.ObjectId + "Query = " + sql);
					return false;
				}

				if (hasRelations)
				{
					SaveObjectRelations(dataObject);
				}

				dataObject.Dirty = false;
				dataObject.IsValid = true;

				return true;
			}
			catch (Exception e)
			{
				if (Log.IsErrorEnabled)
					Log.Error("Error while adding data object " + dataObject.TableName + " " + dataObject.ObjectId, e);
			}

			return false;
		}

		/// <summary>
		/// Persists an object to the database.
		/// </summary>
		/// <param name="dataObject">the object to save to the database</param>
		protected override void SaveObjectImpl(DataObject dataObject)
		{
			try
			{
				string tableName = dataObject.TableName;

				var sb = new StringBuilder("UPDATE `" + tableName + "` SET ");

				BindingInfo[] bindingInfo = GetBindingInfo(dataObject.GetType());
				bool hasRelations = false;
				bool first = true;
				string dateFormat = Connection.GetDBDateFormat();

				for (int i = 0; i < bindingInfo.Length; i++)
				{
					BindingInfo bind = bindingInfo[i];

					if (bind.ReadOnly)
					{
						continue;
					}

					if (!hasRelations)
					{
						hasRelations = bind.HasRelation;
					}
					if (!bind.HasRelation)
					{
						object val = null;
						if (bind.Member is PropertyInfo)
						{
							val = ((PropertyInfo)bind.Member).GetValue(dataObject, null);
						}
						else if (bind.Member is FieldInfo)
						{
							val = ((FieldInfo)bind.Member).GetValue(dataObject);
						}
						else
						{
							continue;
						}

						if (!first)
						{
							sb.Append(", ");
						}
						else
						{
							first = false;
						}

						if (val is bool)
						{
							val = ((bool)val) ? (byte)1 : (byte)0;
						}
						else if (val is DateTime)
						{
							val = ((DateTime)val).ToString(dateFormat);
						}
						else if (val is float)
						{
							val = ((float)val).ToString(Nfi);
						}
						else if (val is double)
						{
							val = ((double)val).ToString(Nfi);
						}
						else if (val is string)
						{
							val = Escape(val.ToString());
						}

						sb.Append("`" + bind.Member.Name + "` = ");
						sb.Append('\'');
						sb.Append(val);
						sb.Append('\'');
					}
				}

				sb.Append(" WHERE `" + tableName + "_ID` = '" + Escape(dataObject.ObjectId) + "'");

				string sql = sb.ToString();
				if (Log.IsDebugEnabled)
					Log.Debug(sql);

				int res = Connection.ExecuteNonQuery(sql);
				if (res == 0)
				{
					if (Log.IsErrorEnabled)
						Log.Error("Error modifying object " + dataObject.TableName + " ID=" + dataObject.ObjectId + " --- keyvalue changed? " + sql + " " + Environment.StackTrace);
					return;
				}

				if (hasRelations)
				{
					SaveObjectRelations(dataObject);
				}

				dataObject.Dirty = false;
				dataObject.IsValid = true;
			}
			catch (Exception e)
			{
				if (Log.IsErrorEnabled)
					Log.Error("Error while adding dataobject " + dataObject.TableName + " " + dataObject.ObjectId, e);
			}
		}

		/// <summary>
		/// Deletes an object from the database.
		/// </summary>
		/// <param name="dataObject">the object to delete from the database</param>
		protected override void DeleteObjectImpl(DataObject dataObject)
		{
			try
			{
				string sql = "DELETE FROM `" + dataObject.TableName + "` WHERE `" + dataObject.TableName + "_ID` = '" +
							 Escape(dataObject.ObjectId) + "'";

				if (Log.IsDebugEnabled)
					Log.Debug(sql);

				int res = Connection.ExecuteNonQuery(sql);
				if (res == 0)
				{
					if (Log.IsErrorEnabled)
						Log.Error("Deleting " + dataObject.TableName + " object failed! ID=" + dataObject.ObjectId + " " + Environment.StackTrace);
				}

				dataObject.IsValid = false;

				DeleteFromCache(dataObject.TableName, dataObject);
				DeleteObjectRelations(dataObject);
			}
			catch (Exception e)
			{
				throw new DatabaseException("Deleting Databaseobject failed !", e);
			}
		}


		protected override DataObject FindObjectByKeyImpl(Type objectType, object key)
		{
			MemberInfo[] members = objectType.GetMembers();
			var ret = Activator.CreateInstance(objectType) as DataObject;

			string tableName = ret.TableName;
			DataTableHandler dth = TableDatasets[tableName];
			string whereClause = null;

			if (dth.UsesPreCaching)
			{
				DataObject obj = dth.GetPreCachedObject(key);
				if (obj != null)
					return obj;
			}

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

			var objs = SelectObjectsImpl(objectType, whereClause, Transaction.IsloationLevel.DEFAULT);
			if (objs.Length > 0)
			{
				dth.SetPreCachedObject(key, objs[0]);
				return objs[0];
			}

			return null;
		}

		/// <summary>
		/// Finds an object in the database by primary key.
		/// </summary>
		/// <param name="objectType">the type of object to retrieve</param>
		/// <param name="key">the value of the primary key to search for</param>
		/// <returns>a <see cref="DataObject" /> instance representing a row with the given primary key value; null if the key value does not exist</returns>
		protected override TObject FindObjectByKeyImpl<TObject>(object key)
		{
			MemberInfo[] members = typeof(TObject).GetMembers();
			var ret = (TObject)Activator.CreateInstance(typeof(TObject));

			string tableName = ret.TableName;
			DataTableHandler dth = TableDatasets[tableName];
			string whereClause = null;

			if (dth.UsesPreCaching)
			{
				DataObject obj = dth.GetPreCachedObject(key);
				if (obj != null)
					return obj as TObject;
			}

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

			var objs = SelectObjectsImpl<TObject>(whereClause, Transaction.IsloationLevel.DEFAULT);
			if (objs.Count > 0)
			{
				dth.SetPreCachedObject(key, objs[0]);
				return objs[0];
			}

			return null;
		}

		/// <summary>
		/// Selects objects from a given table in the database based on a given set of criteria. (where clause)
		/// </summary>
		/// <param name="objectType">the type of objects to retrieve</param>
		/// <param name="whereClause">the where clause to filter object selection on</param>
		/// <returns>an array of <see cref="DataObject" /> instances representing the selected objects that matched the given criteria</returns>
		protected override DataObject[] SelectObjectsImpl(Type objectType, string whereClause, Transaction.IsloationLevel isolation)
		{
			string tableName = GetTableOrViewName(objectType);
			var dataObjects = new List<DataObject>(64);

			// build sql command
			var sb = new StringBuilder("SELECT `" + tableName + "_ID`, ");
			bool first = true;

			BindingInfo[] bindingInfo = GetBindingInfo(objectType);
			for (int i = 0; i < bindingInfo.Length; i++)
			{
				if (!bindingInfo[i].HasRelation)
				{
					if (!first)
					{
						sb.Append(", ");
					}
					else
					{
						first = false;
					}
					sb.Append("`" + bindingInfo[i].Member.Name + "`");
				}
			}

			sb.Append(" FROM `" + tableName + "`");

			if (whereClause != null && whereClause.Trim().Length > 0)
			{
				sb.Append(" WHERE " + whereClause);
			}

			string sql = sb.ToString();

			if (Log.IsDebugEnabled)
				Log.Debug("DataObject[] SelectObjectsImpl: " + sql);

			int objCount = 0;

			// read data and fill objects
			Connection.ExecuteSelect(sql, delegate(MySqlDataReader reader)
			{
				var data = new object[reader.FieldCount];
				while (reader.Read())
				{
					objCount++;

					reader.GetValues(data);
					var id = (string)data[0];

					// fill new data object
					var obj = Activator.CreateInstance(objectType) as DataObject;
					obj.ObjectId = id;

					bool hasRelations = false;
					int field = 1;
					// we can use hard index access because we iterate the same order here
					for (int i = 0; i < bindingInfo.Length; i++)
					{
						BindingInfo bind = bindingInfo[i];
						if (!hasRelations)
						{
							hasRelations = bind.HasRelation;
						}

						if (!bind.HasRelation)
						{
							object val = data[field++];
							if (val != null && !val.GetType().IsInstanceOfType(DBNull.Value))
							{
								if (bind.Member is PropertyInfo)
								{
									Type type = ((PropertyInfo)bind.Member).PropertyType;

									try
									{
										if (type == typeof(bool))
										{
											// special handling for bool
											((PropertyInfo)bind.Member).SetValue(obj,
																				  (val.ToString() == "0") ? false : true,
																				  null);
										}
										else if (type == typeof(DateTime))
										{
											// special handling for datetime
											if (val is MySqlDateTime)
											{
												((PropertyInfo)bind.Member).SetValue(obj,
																					  ((MySqlDateTime)val).GetDateTime(),
																					  null);
											}
											else
											{
												((PropertyInfo)bind.Member).SetValue(obj,
																					  ((IConvertible)val).ToDateTime(null),
																					  null);
											}
										}
										else
										{
											((PropertyInfo)bind.Member).SetValue(obj, val, null);
										}
									}
									catch (Exception e)
									{
										if (Log.IsErrorEnabled)
											Log.Error(
												tableName + ": " + bind.Member.Name + " = " + val.GetType().FullName +
												" doesnt fit to " + bind.Member.DeclaringType.FullName, e);
										continue;
									}
								}
								else if (bind.Member is FieldInfo)
								{
									((FieldInfo)bind.Member).SetValue(obj, val);
								}
							}
						}
					}

					dataObjects.Add(obj);
					obj.Dirty = false;

					if (hasRelations)
					{
						FillLazyObjectRelations(obj, true);
					}

					obj.IsValid = true;
				}
			}
			, isolation);

			return dataObjects.ToArray();
		}

		/// <summary>
		/// Selects objects from a given table in the database based on a given set of criteria. (where clause)
		/// </summary>
		/// <param name="objectType">the type of objects to retrieve</param>
		/// <param name="whereClause">the where clause to filter object selection on</param>
		/// <returns>an array of <see cref="DataObject" /> instances representing the selected objects that matched the given criteria</returns>
		protected override IList<TObject> SelectObjectsImpl<TObject>(string whereClause, Transaction.IsloationLevel isolation)
		{
			string tableName = GetTableOrViewName(typeof(TObject));
			var dataObjects = new List<TObject>(64);

			// build sql command
			var sb = new StringBuilder("SELECT `" + tableName + "_ID`, ");
			bool first = true;

			BindingInfo[] bindingInfo = GetBindingInfo(typeof(TObject));
			for (int i = 0; i < bindingInfo.Length; i++)
			{
				if (!bindingInfo[i].HasRelation)
				{
					if (!first)
					{
						sb.Append(", ");
					}
					else
					{
						first = false;
					}
					sb.Append("`" + bindingInfo[i].Member.Name + "`");
				}
			}

			sb.Append(" FROM `" + tableName + "`");

			if (whereClause != null && whereClause.Trim().Length > 0)
			{
				sb.Append(" WHERE " + whereClause);
			}

			string sql = sb.ToString();

			if (Log.IsDebugEnabled)
				Log.Debug("IList<TObject> SelectObjectsImpl: " + sql);

			// read data and fill objects
			Connection.ExecuteSelect(sql, delegate(MySqlDataReader reader)
											{
												var data = new object[reader.FieldCount];
												while (reader.Read())
												{
													reader.GetValues(data);
													var id = (string)data[0];

													// fill new data object
													var obj = Activator.CreateInstance(typeof(TObject)) as TObject;
													obj.ObjectId = id;

													bool hasRelations = false;
													int field = 1;
													// we can use hard index access because we iterate the same order here
													for (int i = 0; i < bindingInfo.Length; i++)
													{
														BindingInfo bind = bindingInfo[i];
														if (!hasRelations)
														{
															hasRelations = bind.HasRelation;
														}

														if (!bind.HasRelation)
														{
															object val = data[field++];
															if (val != null && !val.GetType().IsInstanceOfType(DBNull.Value))
															{
																if (bind.Member is PropertyInfo)
																{
																	Type type = ((PropertyInfo)bind.Member).PropertyType;

																	try
																	{
																		if (type == typeof(bool))
																		{
																			// special handling for bool
																			((PropertyInfo)bind.Member).SetValue(obj,
																												  (val.ToString() == "0") ? false : true,
																												  null);
																		}
																		else if (type == typeof(DateTime))
																		{
																			// special handling for datetime
																			if (val is MySqlDateTime)
																			{
																				((PropertyInfo)bind.Member).SetValue(obj,
																													  ((MySqlDateTime)val).GetDateTime(),
																													  null);
																			}
																			else
																			{
																				((PropertyInfo)bind.Member).SetValue(obj,
																													  ((IConvertible)val).ToDateTime(null),
																													  null);
																			}
																		}
																		else
																		{
																			((PropertyInfo)bind.Member).SetValue(obj, val, null);
																		}
																	}
																	catch (Exception e)
																	{
																		if (Log.IsErrorEnabled)
																			Log.Error(
																				tableName + ": " + bind.Member.Name + " = " + val.GetType().FullName +
																				" doesnt fit to " + bind.Member.DeclaringType.FullName, e);
																		continue;
																	}
																}
																else if (bind.Member is FieldInfo)
																{
																	((FieldInfo)bind.Member).SetValue(obj, val);
																}
															}
														}
													}

													dataObjects.Add(obj);
													obj.Dirty = false;

													if (hasRelations)
													{
														FillLazyObjectRelations(obj, true);
													}

													obj.IsValid = true;
												}
											}
				, isolation);

			return dataObjects.ToArray();
		}

		/// <summary>
		/// Selects all objects from a given table in the database.
		/// </summary>
		/// <param name="objectType">the type of objects to retrieve</param>
		/// <returns>an array of <see cref="DataObject" /> instances representing the selected objects</returns>
		protected override IList<TObject> SelectAllObjectsImpl<TObject>(Transaction.IsloationLevel isolation)
		{
			return SelectObjectsImpl<TObject>("", isolation);
		}

		/// <summary>
		/// Gets the number of objects in a given table in the database based on a given set of criteria. (where clause)
		/// </summary>
		/// <param name="objectType">the type of objects to count</param>
		/// <param name="where">the where clause to filter object count on</param>
		/// <returns>a positive integer representing the number of objects that matched the given criteria; zero if no such objects existed</returns>
		protected override int GetObjectCountImpl<TObject>(string where)
		{
			string tableName = GetTableOrViewName(typeof(TObject));

			if (Connection.IsSQLConnection)
			{
				string query = "SELECT COUNT(*) FROM " + tableName;
				if (where != "")
					query += " WHERE " + where;

				object count = Connection.ExecuteScalar(query);
				if (count is long)
					return (int)((long)count);

				return (int)count;
			}

			return 0;
		}

		#endregion
	}
}
