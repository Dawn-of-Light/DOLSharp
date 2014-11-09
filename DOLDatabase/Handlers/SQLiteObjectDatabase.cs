﻿/*
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
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using System.Data;
using DOL.Database.Attributes;
using DOL.Database.Connection;
using DOL.Database.UniqueID;
using System.Data.SQLite;

namespace DOL.Database.Handlers
{
	public class SQLiteObjectDatabase : ObjectDatabase
	{				
		public SQLiteObjectDatabase(DataConnection connection)
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

				var columns = new StringBuilder();
				var values = new StringBuilder();

				MemberInfo[] objMembers = dataObject.GetType().GetMembers();
				bool hasRelations = false;
				bool usePrimary = false;
				object primaryKey = null;
				string primaryColumnName = "";
				bool firstColumn = true;
				string dateFormat = Connection.GetDBDateFormat();

				for (int i = 0; i < objMembers.Length; i++)
				{
					bool isPrimary = false;

					if (!hasRelations)
					{
						object[] relAttrib = GetRelationAttributes(objMembers[i]);
						hasRelations = relAttrib.Length > 0;
					}
					object[] keyAttrib = objMembers[i].GetCustomAttributes(typeof(PrimaryKey), true);
					object[] attrib = objMembers[i].GetCustomAttributes(typeof(DataElement), true);

					// if a primary key field is using auto increment then use it as the key instead of the tablename_id column
					if (keyAttrib.Length > 0 && (keyAttrib[0] as PrimaryKey).AutoIncrement)
					{
						usePrimary = true;
						primaryColumnName = objMembers[i].Name;
						isPrimary = true;
					}

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

						if (firstColumn == false)
						{
							columns.Append(", ");
							values.Append(", ");
						}

						columns.Append("`" + objMembers[i].Name + "`");

						firstColumn = false;

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
						
						if (isPrimary && ((val is int && (int)val == 0) || (val is long && (long)val == 0)))
						{
							values.Append("null");
						}
						else
						{
							values.Append('\'');
							values.Append(val);
							values.Append('\'');
						}

						if (isPrimary)
						{
							if (val is int)
							{
								primaryKey = Convert.ToInt32(val);
							}
							else if (val is long)
							{
								primaryKey = Convert.ToInt64(val);
							}
							else
							{
								if (Log.IsErrorEnabled)
									Log.Error("Error adding object into " + dataObject.TableName + ".  PrimaryKey with AutoIncrement must be of type int or long.");

								return false;
							}
						}
					}
				}

				if (usePrimary == false)
				{
					if (dataObject.ObjectId == null)
					{
						dataObject.ObjectId = IDGenerator.GenerateID();
					}

					// Add the silly Tablename_ID column
					columns.Insert(0, "`" + tableName + "_ID`, ");
					values.Insert(0, "'" + Escape(dataObject.ObjectId) + "', ");
				}

				string sql = "INSERT INTO `" + tableName + "` (" + columns + ") VALUES (" + values + ")";

				if (Log.IsDebugEnabled)
					Log.Debug(sql);

				if (usePrimary)
				{
					object objID = Connection.ExecuteScalar(sql + "; SELECT LAST_INSERT_ROWID();");
					
					object newID;
					bool newIDzero = false;
					bool error = false;
					
					if(primaryKey is int)
					{
						newID = Convert.ToInt32(objID);
						newIDzero = (int)newID == 0;
						error = newIDzero && (int)primaryKey == 0;
					}
					else
					{
						newID = Convert.ToInt64(objID);
						newIDzero = (long)newID == 0;
						error = newIDzero && (long)primaryKey == 0;
					}
					
					if (primaryKey == null || error)
					{
						if (Log.IsErrorEnabled)
							Log.Error("Error adding object into " + dataObject.TableName + " ID=" + objID + ", UsePrimary, Query = " + sql);
						return false;
					}
					else
					{
						if (newIDzero)
						{
							newID = Convert.ToInt64(primaryKey);
						}

						for (int i = 0; i < objMembers.Length; i++)
						{
							if (objMembers[i].Name == primaryColumnName)
							{
								if (objMembers[i] is PropertyInfo)
								{
									if (primaryKey is long)
									{
										((PropertyInfo)objMembers[i]).SetValue(dataObject, (long)newID, null);
									}
									else
									{
										((PropertyInfo)objMembers[i]).SetValue(dataObject, (int)newID, null);
									}
								}
								else if (objMembers[i] is FieldInfo)
								{
									if (primaryKey is long)
									{
										((FieldInfo)objMembers[i]).SetValue(dataObject, (long)newID);
									}
									else
									{
										((FieldInfo)objMembers[i]).SetValue(dataObject, (int)newID);
									}
								}

								break;
							}
						}
					}
				}
				else
				{
					int res = Connection.ExecuteNonQuery(sql);
					if (res == 0)
					{
						if (Log.IsErrorEnabled)
							Log.Error("Error adding object into " + dataObject.TableName + " ID=" + dataObject.ObjectId + "Query = " + sql);
						return false;
					}
				}


				if (hasRelations)
				{
					SaveObjectRelations(dataObject);
				}

				dataObject.Dirty = false;
				dataObject.IsPersisted = true;
				dataObject.IsDeleted = false;

				return true;
			}
			catch (Exception e)
			{
				if (Log.IsErrorEnabled)
					Log.Error("Error while adding data object: " + dataObject.ToString(), e);
			}

			return false;
		}

		/// <summary>
		/// Persists an object to the database.
		/// </summary>
		/// <param name="dataObject">the object to save to the database</param>
		protected override bool SaveObjectImpl(DataObject dataObject)
		{
			try
			{
				string tableName = dataObject.TableName;

				var sb = new StringBuilder("UPDATE `" + tableName + "` SET ");

				BindingInfo[] bindingInfo = GetBindingInfo(dataObject.GetType());
				bool hasRelations = false;
				bool first = true;
				string dateFormat = Connection.GetDBDateFormat();
				string primaryKeyColumn = string.Empty;
				object primaryKeyValue = null;

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

						if (bind.UsePrimaryKey)
						{
							primaryKeyColumn = bind.Member.Name;
							primaryKeyValue = val;
						}


					}
				}

				if (primaryKeyColumn != string.Empty)
				{
					sb.Append(" WHERE `" + primaryKeyColumn + "` = '" + primaryKeyValue + "'");
				}
				else
				{
					sb.Append(" WHERE `" + tableName + "_ID` = '" + Escape(dataObject.ObjectId) + "'");
				}

				string sql = sb.ToString();

				if (Log.IsDebugEnabled)
					Log.Debug(sql);

				int res = Connection.ExecuteNonQuery(sql);
				if (res == 0)
				{
					if (Log.IsErrorEnabled)
						Log.Error("Error modifying object " + dataObject.TableName + " ID=" + dataObject.ObjectId + " --- keyvalue changed? " + sql + " " + Environment.StackTrace);
					return false;
				}

				if (hasRelations)
				{
					SaveObjectRelations(dataObject);
				}

				dataObject.Dirty = false;
				dataObject.IsPersisted = true;
				return true;
			}
			catch (Exception e)
			{
				if (Log.IsErrorEnabled)
					Log.Error("Error while saving data object: " + dataObject.ToString(), e);
			}

			return false;
		}

		/// <summary>
		/// Deletes an object from the database.
		/// </summary>
		/// <param name="dataObject">the object to delete from the database</param>
		protected override bool DeleteObjectImpl(DataObject dataObject)
		{
			try
			{
				BindingInfo[] bindingInfo = GetBindingInfo(dataObject.GetType());

				string primaryKeyColumn = string.Empty;
				object primaryKeyValue = null;

				for (int i = 0; i < bindingInfo.Length; i++)
				{
					BindingInfo bind = bindingInfo[i];
					object val;

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

					if (bind.UsePrimaryKey)
					{
						primaryKeyColumn = bind.Member.Name;
						primaryKeyValue = val;
					}

				}

				string sql;

				if (primaryKeyColumn != string.Empty)
				{
					sql = "DELETE FROM `" + dataObject.TableName + "` WHERE `" + primaryKeyColumn + "` = '" + Escape(primaryKeyValue.ToString()) + "'";
				}
				else
				{
					sql = "DELETE FROM `" + dataObject.TableName + "` WHERE `" + dataObject.TableName + "_ID` = '" + Escape(dataObject.ObjectId) + "'";
				}

				if (Log.IsDebugEnabled)
					Log.Debug(sql);

				int res = Connection.ExecuteNonQuery(sql);
				if (res == 0)
				{
					if (Log.IsErrorEnabled)
						Log.Error("Deleting " + dataObject.ToString() + " object failed!" + " " + Environment.StackTrace);
				}

				dataObject.IsPersisted = false;

				DeleteFromCache(dataObject.TableName, dataObject);
				DeleteObjectRelations(dataObject);

				dataObject.IsDeleted = true;
				return true;
			}
			catch (Exception e)
			{
				throw new DatabaseException("Deleting DataObject " + dataObject.ToString() + " failed !", e);
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

			var objs = SelectObjectsImpl(objectType, whereClause, Transaction.IsolationLevel.DEFAULT);
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

			var objs = SelectObjectsImpl<TObject>(whereClause, Transaction.IsolationLevel.DEFAULT);
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
		protected override DataObject[] SelectObjectsImpl(Type objectType, string whereClause, Transaction.IsolationLevel isolation)
		{
			string tableName = GetTableOrViewName(objectType);
			var dataObjects = new List<DataObject>(64);

			// build sql command

			//var sb = new StringBuilder("SELECT `" + tableName + "_ID`, ");
			var sb = new StringBuilder("");
			bool first = true;
			bool usePrimaryKey = false;

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

					if (bindingInfo[i].UsePrimaryKey)
					{
						usePrimaryKey = true;
					}
				}
			}

			if (usePrimaryKey)
			{
				sb.Insert(0, "SELECT ");
			}
			else
			{
				sb.Insert(0, "SELECT `" + tableName + "_ID`, ");
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
			Connection.ExecuteSelect(sql, delegate(IDataReader reader)
			{
				var data = new object[reader.FieldCount];
				while (reader.Read())
				{
					objCount++;

					reader.GetValues(data);

					// fill new data object
					var obj = Activator.CreateInstance(objectType) as DataObject;
					
					int field = 0;
					if (usePrimaryKey == false)
					{
						// fill the silly TableName_ID field
						obj.ObjectId = (string)data[0];
						field = 1;
					}

					bool hasRelations = false;
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
										else if (type == typeof(System.UInt64))
										{
											((PropertyInfo)bind.Member).SetValue(obj, Convert.ToUInt64(val), null);
										}
										else if (type == typeof(System.UInt32))
										{
											((PropertyInfo)bind.Member).SetValue(obj, Convert.ToUInt32(val), null);
										}
										else if (type == typeof(System.Int32))
										{
											((PropertyInfo)bind.Member).SetValue(obj, Convert.ToInt32(val), null);
										}
										else if (type == typeof(System.UInt16))
										{
											((PropertyInfo)bind.Member).SetValue(obj, Convert.ToUInt16(val), null);
										}
										else if (type == typeof(System.Int16))
										{
											((PropertyInfo)bind.Member).SetValue(obj, Convert.ToInt16(val), null);
										}
										else if (type == typeof(System.SByte))
										{
											((PropertyInfo)bind.Member).SetValue(obj, Convert.ToSByte(val), null);
										}
										else if (type == typeof(System.Byte))
										{
											((PropertyInfo)bind.Member).SetValue(obj, Convert.ToByte(val), null);
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

					obj.IsPersisted = true;
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
		protected override IList<TObject> SelectObjectsImpl<TObject>(string whereClause, Transaction.IsolationLevel isolation)
		{
			string tableName = GetTableOrViewName(typeof(TObject));
			var dataObjects = new List<TObject>(64);

			// build sql command
			var sb = new StringBuilder("");
			bool first = true;

			bool usePrimaryKey = false;

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

					if (bindingInfo[i].UsePrimaryKey)
						usePrimaryKey = true;
				}
			}

			if (usePrimaryKey)
			{
				sb.Insert(0, "SELECT ");
			}
			else
			{
				sb.Insert(0, "SELECT `" + tableName + "_ID`, ");
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
			Connection.ExecuteSelect(sql, delegate(IDataReader reader)
											{
												var data = new object[reader.FieldCount];
												while (reader.Read())
												{
													reader.GetValues(data);
													var obj = Activator.CreateInstance(typeof(TObject)) as TObject;
													int field = 0;

													if (usePrimaryKey == false)
													{
														// fill the silly TableName_ID field
														obj.ObjectId = (string)data[0];
														field = 1;
													}

													bool hasRelations = false;
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
																		else if (type == typeof(System.UInt64))
																		{
																			((PropertyInfo)bind.Member).SetValue(obj, Convert.ToUInt64(val), null);
																		}
																		else if (type == typeof(System.UInt32))
																		{
																			((PropertyInfo)bind.Member).SetValue(obj, Convert.ToUInt32(val), null);
																		}
																		else if (type == typeof(System.Int32))
																		{
																			((PropertyInfo)bind.Member).SetValue(obj, Convert.ToInt32(val), null);
																		}
																		else if (type == typeof(System.UInt16))
																		{
																			((PropertyInfo)bind.Member).SetValue(obj, Convert.ToUInt16(val), null);
																		}
																		else if (type == typeof(System.Int16))
																		{
																			((PropertyInfo)bind.Member).SetValue(obj, Convert.ToInt16(val), null);
																		}
																		else if (type == typeof(System.SByte))
																		{
																			((PropertyInfo)bind.Member).SetValue(obj, Convert.ToSByte(val), null);
																		}
																		else if (type == typeof(System.Byte))
																		{
																			((PropertyInfo)bind.Member).SetValue(obj, Convert.ToByte(val), null);
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

													obj.IsPersisted = true;
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
		protected override IList<TObject> SelectAllObjectsImpl<TObject>(Transaction.IsolationLevel isolation)
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

		/// <summary>
		/// Executes a raw SQL query against the database.
		/// </summary>
		/// <param name="dataObject">the query to execute</param>
		/// <returns>true if the query was run successfully; false otherwise</returns>
		protected override bool ExecuteNonQueryImpl(string rawQuery)
		{
			try
			{
				if (Log.IsDebugEnabled)
					Log.Debug(rawQuery);

				int res = Connection.ExecuteNonQuery(rawQuery);
				if (res == 0)
				{
					if (Log.IsErrorEnabled)
						Log.Error("Error executing raw query: " + rawQuery);
					
					return false;
				}

				return true;
			}
			catch (Exception e)
			{
				if (Log.IsErrorEnabled)
					Log.Error("Error while executing raw query: " + rawQuery, e);
			}

			return false;
		}

		#endregion
		
		public override string Escape(string toEscape)
		{
			return toEscape.Replace("'", "''");
		}
	}
}
