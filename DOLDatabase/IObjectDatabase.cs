using System;
using System.Collections.Generic;
using System.Text;

namespace DOL.Database
{
	public interface IObjectDatabase
	{
		bool AddObject(DataObject dataObject);

		void SaveObject(DataObject dataObject);

		void DeleteObject(DataObject dataObject);

		TObject FindObjectByKey<TObject>(object key)
			where TObject : DataObject;

		TObject SelectObject<TObject>(string whereExpression)
			where TObject : DataObject;

		TObject SelectObject<TObject>(string whereExpression, Transaction.IsloationLevel isolation)
			where TObject : DataObject;

		IList<TObject> SelectObjects<TObject>(string whereExpression)
			where TObject : DataObject;

		IList<TObject> SelectObjects<TObject>(string whereExpression, Transaction.IsloationLevel isolation)
			where TObject : DataObject;

		IList<TObject> SelectAllObjects<TObject>()
			where TObject : DataObject;

		IList<TObject> SelectAllObjects<TObject>(Transaction.IsloationLevel isolation)
			where TObject : DataObject;

		int GetObjectCount<TObject>()
			where TObject : DataObject;

		int GetObjectCount<TObject>(string whereExpression)
			where TObject : DataObject;

		void RegisterDataObject(Type dataObjectType);

		bool UpdateInCache<TObject>(object key)
			where TObject : DataObject;

		void FillObjectRelations(DataObject dataObject);

		string Escape(string rawInput);
	}
}
