using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using DOL.Database.Attributes;
using DOL.Database.Connection;
using DOL.Database.UniqueID;
using DataTable=System.Data.DataTable;

namespace DOL.Database.Handlers
{
	public class XMLObjectDatabase : ObjectDatabase
	{
		public XMLObjectDatabase(DataConnection connection)
			: base(connection)
		{
		}

		#region Public API

		/// <summary>
		/// Adds a new object to the database.
		/// </summary>
		/// <param name="dataObject">the object to add to the database</param>
		/// <returns>true if the object was added successfully; false otherwise</returns>
		protected override bool AddObjectImpl(DataObject dataObject)
		{
			DataTable table = null;
			DataRow row = null;

			try
			{
				string tableName = dataObject.TableName;

				DataSet dataset = GetDataSet(tableName);
				table = dataset.Tables[tableName];

				lock (dataset) // lock dataset before making any changes to it
				{
					row = table.NewRow();
					FillRowWithObject(dataObject, row);
					table.Rows.Add(row);
				}

				if (dataObject.ObjectId == null)
					dataObject.ObjectId = IDGenerator.GenerateID();

				dataObject.ObjectId = row[tableName + "_ID"].ToString();

				dataObject.Dirty = false;
				dataObject.IsValid = true;
				dataObject.IsDeleted = false;

				return true;
			}

			catch (Exception e)
			{
				if (Log.IsErrorEnabled)
					Log.Error("AddNewObject", e);

				if (row != null && table != null)
				{
					table.Rows.Remove(row);
				}

				throw new DatabaseException("Adding Databaseobject failed !", e);
			}
		}

		/// <summary>
		/// Persists an object to the database.
		/// </summary>
		/// <param name="dataObject">the object to save to the database</param>
		protected override void SaveObjectImpl(DataObject dataObject)
		{
			try
			{
				if (dataObject.Dirty == false)
					return;

				DataSet dataset = GetDataSet(dataObject.TableName);

				lock (dataset) // lock the dataset before changing any values!
				{
					DataRow row = FindRowByKey(dataObject);
					if (row == null)
					{
						throw new DatabaseException("Saving Databaseobject failed (Keyvalue Changed ?)!");
					}

					FillRowWithObject(dataObject, row);
				}

				dataObject.Dirty = false;
				dataObject.IsValid = true;

				return;
			}
			catch (Exception e)
			{
				throw new DatabaseException("Saving Database object failed!", e);
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
				DataSet dataset = GetDataSet(dataObject.TableName);
				if (dataset == null)
				{
					throw new DatabaseException("Deleting Databaseobject failed, no dataset!");
				}

				lock (dataset) // lock dataset before making changes to it
				{
					DataRow row = FindRowByKey(dataObject);
					if (row == null)
					{
						throw new DatabaseException("Deleting Databaseobject failed !");
					}

					row.Delete();
				}

				dataObject.IsValid = false;

				DeleteFromCache(dataObject.TableName, dataObject);
				DeleteObjectRelations(dataObject);

				dataObject.IsDeleted = true;
			}
			catch (Exception e)
			{
				throw new DatabaseException("Deleting Databaseobject failed !", e);
			}
		}

		/// <summary>
		/// Gets the number of objects in a given table in the database based on a given set of criteria. (where clause)
		/// </summary>
		/// <param name="objectType">the type of objects to count</param>
		/// <param name="where">the where clause to filter object count on</param>
		/// <returns>a positive integer representing the number of objects that matched the given criteria; zero if no such objects existed</returns>
		protected override int GetObjectCountImpl<TObject>(string whereExpression)
		{
			string tableName = GetTableOrViewName(typeof(TObject));

			DataSet ds = GetDataSet(tableName);
			if (ds != null)
			{
				DataTable table = ds.Tables[tableName];

				DataRow[] rows = table.Select(whereExpression);

				return rows.Length;
			}

			return 0;
		}


		protected override DataObject FindObjectByKeyImpl(Type objectType, object key)
		{
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
			var ret = (TObject)Activator.CreateInstance(typeof(TObject));
			string tableName = ret.TableName;

			DataTable table = GetDataSet(tableName).Tables[tableName];
			DataRow row = table.Rows.Find(key);
			if (row != null)
			{
				FillObjectWithRow(ref ret, row, false);
				return ret;
			}

			return null;
		}

		/// <summary>
		/// Selects objects from a given table in the database based on a given set of criteria. (where clause)
		/// This is SLOW SLOW SLOW.  You should cache these results rather than reading from the XML file each time.
		/// </summary>
		/// <param name="objectType">the type of objects to retrieve</param>
		/// <param name="whereClause">the where clause to filter object selection on</param>
		/// <returns>an array of <see cref="DataObject" /> instances representing the selected objects that matched the given criteria</returns>
		protected override DataObject[] SelectObjectsImpl(Type objectType, string statement, Transaction.IsolationLevel isolation)
		{

			string tableName = GetTableOrViewName(objectType);

			if (Log.IsDebugEnabled)
				Log.Debug("1. select objects " + tableName + " " + statement);

			DataSet ds = GetDataSet(tableName);
			if (ds != null)
			{
				Connection.LoadDataSet(tableName, ds);
				DataTable table = ds.Tables[tableName];

				if (Log.IsDebugEnabled)
					Log.Debug("2. " + tableName + " " + statement);

				DataRow[] rows = table.Select(statement);

				if (Log.IsDebugEnabled)
					Log.Debug("3. " + tableName + " " + statement);

				int count = rows.Length;

				var objs = (DataObject[])Array.CreateInstance(objectType, count);
				for (int i = 0; i < count; i++)
				{
					var remap = (DataObject)(Activator.CreateInstance(objectType));
					FillObjectWithRow(ref remap, rows[i], false);
					objs[i] = remap;
				}

				if (Log.IsDebugEnabled)
					Log.Debug("4. select objects " + tableName + " " + statement);

				return objs;
			}

			return (DataObject[])Array.CreateInstance(objectType, 0);
		}


		/// <summary>
		/// Selects objects from a given table in the database based on a given set of criteria. (where clause)
		/// This is SLOW SLOW SLOW.  You should cache these results rather than reading from the XML file each time.
		/// </summary>
		/// <param name="objectType">the type of objects to retrieve</param>
		/// <param name="whereClause">the where clause to filter object selection on</param>
		/// <returns>an array of <see cref="DataObject" /> instances representing the selected objects that matched the given criteria</returns>
		protected override IList<TObject> SelectObjectsImpl<TObject>(string statement, Transaction.IsolationLevel isolation)
		{
			string tableName = GetTableOrViewName(typeof(TObject));

			if (Log.IsDebugEnabled)
				Log.Debug("1. select objects " + tableName + " " + statement);

			DataSet ds = GetDataSet(tableName);
			if (ds != null)
			{
				Connection.LoadDataSet(tableName, ds);
				DataTable table = ds.Tables[tableName];

				if (Log.IsDebugEnabled)
					Log.Debug("2. " + tableName + " " + statement);

				DataRow[] rows = table.Select(statement);

				if (Log.IsDebugEnabled)
					Log.Debug("3. " + tableName + " " + statement);

				int count = rows.Length;

				var objs = new List<TObject>(count);
				for (int i = 0; i < count; i++)
				{
					var remap = (TObject)(Activator.CreateInstance(typeof(TObject)));
					FillObjectWithRow(ref remap, rows[i], false);

					objs.Add(remap);
				}

				if (Log.IsDebugEnabled)
					Log.Debug("4. select objects " + tableName + " " + statement);

				return objs;
			}

			return new List<TObject>();
		}


		/// <summary>
		/// Selects all objects from a given table in the database.
		/// This is SLOW SLOW SLOW.  You should cache these results rather than reading from the XML file each time.
		/// </summary>
		/// <param name="objectType">the type of objects to retrieve</param>
		/// <returns>an array of <see cref="DataObject" /> instances representing the selected objects</returns>
		protected override IList<TObject> SelectAllObjectsImpl<TObject>(Transaction.IsolationLevel isolation)
		{
			string tableName = DataObject.GetTableName(typeof(TObject));

			if (Log.IsDebugEnabled)
				Log.Debug("1. select all " + tableName);

			DataSet ds = GetDataSet(tableName);

			if (ds != null)
			{
				Connection.LoadDataSet(tableName, ds);
				DataTable table = ds.Tables[tableName];
				DataRow[] rows = table.Select();

				int count = rows.Length;
				//Create an array of our destination objects
				var objs = new List<TObject>(count);

				for (int i = 0; i < count; i++)
				{
					var remap = (TObject)(Activator.CreateInstance(typeof(TObject)));
					FillObjectWithRow(ref remap, rows[i], false);

					objs.Add(remap);
				}

				if (Log.IsDebugEnabled)
					Log.Debug("2. select all " + tableName);

				return objs;
			}

			return new List<TObject>();
		}

		protected override bool ExecuteNonQueryImpl(string rawQuery)
		{
			if (Log.IsDebugEnabled)
					Log.Debug("Raw query execution not supported in XML mode.");

			return true;
		}

		#endregion
	}
}
