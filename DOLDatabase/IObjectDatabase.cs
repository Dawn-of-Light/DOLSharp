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
		/// <summary>
		/// Insert a new DataObject into the database and save it
		/// </summary>
		/// <param name="dataObject">DataObject to Add into database</param>
		/// <returns>True if the DataObject was added.</returns>
		bool AddObject(DataObject dataObject);

		/// <summary>
		/// Save a DataObject to database if saving is allowed and object is dirty
		/// </summary>
		/// <param name="dataObject">DataObject to Save in database</param>
		/// <returns>True if the DataObject was saved.</returns>
		bool SaveObject(DataObject dataObject);

		/// <summary>
		/// Delete a DataObject from database if deletion is allowed
		/// </summary>
		/// <param name="dataObject">DataObject to Delete from database</param>
		/// <returns>True if the DataObject was deleted.</returns>
		bool DeleteObject(DataObject dataObject);

		TObject FindObjectByKey<TObject>(object key)
			where TObject : DataObject;

		TObject SelectObject<TObject>(string whereExpression)
			where TObject : DataObject;

		TObject SelectObject<TObject>(string whereExpression, Transaction.IsolationLevel isolation)
			where TObject : DataObject;

		IList<TObject> SelectObjects<TObject>(string whereExpression)
			where TObject : DataObject;

		IList<TObject> SelectObjects<TObject>(string whereExpression, Transaction.IsolationLevel isolation)
			where TObject : DataObject;

		IList<TObject> SelectAllObjects<TObject>()
			where TObject : DataObject;

		IList<TObject> SelectAllObjects<TObject>(Transaction.IsolationLevel isolation)
			where TObject : DataObject;

		int GetObjectCount<TObject>()
			where TObject : DataObject;

		int GetObjectCount<TObject>(string whereExpression)
			where TObject : DataObject;

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
	}
}
