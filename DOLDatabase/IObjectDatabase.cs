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

namespace DOL.Database
{
	/// <summary>
	/// Interface for Handling Object Database API
	/// </summary>
	public interface IObjectDatabase
	{
		#region Add Objects
		/// <summary>
		/// Insert a new DataObject into the database and save it
		/// </summary>
		/// <param name="dataObject">DataObject to Add into database</param>
		/// <returns>True if the DataObject was added.</returns>
		bool AddObject(DataObject dataObject);

		/// <summary>
		/// Insert new DataObjects into the database and save them
		/// </summary>
		/// <param name="dataObjects">DataObjects to Add into database</param>
		/// <returns>True if All DataObjects were added.</returns>
		bool AddObject(IEnumerable<DataObject> dataObjects);
		#endregion
		#region Save Objects
		/// <summary>
		/// Save a DataObject to database if saving is allowed and object is dirty
		/// </summary>
		/// <param name="dataObject">DataObject to Save in database</param>
		/// <returns>True if the DataObject was saved.</returns>
		bool SaveObject(DataObject dataObject);

		/// <summary>
		/// Save DataObjects to database if saving is allowed and object is dirty
		/// </summary>
		/// <param name="dataObjects">DataObjects to Save in database</param>
		/// <returns>True if All DataObjects were saved.</returns>
		bool SaveObject(IEnumerable<DataObject> dataObjects);
		#endregion
		#region Delete Objects
		/// <summary>
		/// Delete a DataObject from database if deletion is allowed
		/// </summary>
		/// <param name="dataObject">DataObject to Delete from database</param>
		/// <returns>True if the DataObject was deleted.</returns>
		bool DeleteObject(DataObject dataObject);
		
		/// <summary>
		/// Delete DataObjects from database if deletion is allowed
		/// </summary>
		/// <param name="dataObjects">DataObjects to Delete from database</param>
		/// <returns>True if All DataObjects were deleted.</returns>
		bool DeleteObject(IEnumerable<DataObject> dataObjects);
		#endregion

		#region Select By Key
		TObject FindObjectByKey<TObject>(object key)
			where TObject : DataObject;
		IEnumerable<TObject> FindObjectByKey<TObject>(IEnumerable<object> key)
			where TObject : DataObject;
		#endregion
		#region Select Where Clause With Parameters
		IEnumerable<IEnumerable<TObject>> SelectObject<TObject>(string whereExpression, IEnumerable<IEnumerable<KeyValuePair<string, object>>> parameters)
			where TObject : DataObject;
		IEnumerable<TObject> SelectObject<TObject>(string whereExpression, IEnumerable<KeyValuePair<string, object>> parameter)
			where TObject : DataObject;
		IEnumerable<TObject> SelectObject<TObject>(string whereExpression, KeyValuePair<string, object> param)
			where TObject : DataObject;
		#endregion
		#region Select Where Clause Without Parameter
		[Obsolete("Use Parametrized Select Queries for best perfomance")]
		TObject SelectObject<TObject>(string whereExpression)
			where TObject : DataObject;
		[Obsolete("Use Parametrized Select Queries for best perfomance")]
		IList<TObject> SelectObjects<TObject>(string whereExpression)
			where TObject : DataObject;
		[Obsolete("Use Parametrized Select Queries for best perfomance")]
		TObject SelectObject<TObject>(string whereExpression, Transaction.IsolationLevel isolation)
			where TObject : DataObject;
		[Obsolete("Use Parametrized Select Queries for best perfomance")]
		IList<TObject> SelectObjects<TObject>(string whereExpression, Transaction.IsolationLevel isolation)
			where TObject : DataObject;
		#endregion
		#region Select All Object
		IList<TObject> SelectAllObjects<TObject>()
			where TObject : DataObject;

		IList<TObject> SelectAllObjects<TObject>(Transaction.IsolationLevel isolation)
			where TObject : DataObject;
		#endregion
		#region Count Objects
		int GetObjectCount<TObject>()
			where TObject : DataObject;

		int GetObjectCount<TObject>(string whereExpression)
			where TObject : DataObject;
		#endregion
		#region Metadata Handlers
		/// <summary>
		/// Register Data Object Type if not already Registered
		/// </summary>
		/// <param name="dataObjectType">DataObject Type</param>
		void RegisterDataObject(Type dataObjectType);

		bool UpdateInCache<TObject>(object key)
			where TObject : DataObject;

		/// <summary>
		/// Populate or Refresh Object Relations
		/// </summary>
		/// <param name="dataObject">Object to Populate</param>
		void FillObjectRelations(DataObject dataObject);
		
		/// <summary>
		/// Populate or Refresh Objects Relations
		/// </summary>
		/// <param name="dataObject">DataObject to Populate</param>
		void FillObjectRelations(IEnumerable<DataObject> dataObject);
		#endregion
		#region Utils
		/// <summary>
		/// Escape wrong characters from string for Database Insertion
		/// </summary>
		/// <param name="rawInput">String to Escape</param>
		/// <returns>Escaped String</returns>
		string Escape(string rawInput);

		/// <summary>
		/// Execute a Raw Non-Query on the Database
		/// </summary>
		/// <param name="rawQuery">Raw Command</param>
		/// <returns>True if the Command succeeded</returns>
		[Obsolete("Raw Non-Query are SQL Only and don't use internal Object Database Implementations...")]
		bool ExecuteNonQuery(string rawQuery);
		#endregion
	}
}
