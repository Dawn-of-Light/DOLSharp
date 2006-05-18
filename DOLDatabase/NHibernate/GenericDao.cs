using System;
using System.Collections.Generic;
using System.Text;
using DOL.Database.DataAccessInterfaces;

namespace DOL.Database.NHibernate
{
	/// <summary>
	/// Provides basic NHibernate DAO functionality.
	/// </summary>
	/// <typeparam name="TTransferObject">The transfer object type.</typeparam>
	/// <typeparam name="TPrimaryKey">The transfer object's primary key.</typeparam>
	public class GenericDao<TTransferObject, TPrimaryKey> : IGenericDao<TTransferObject, TPrimaryKey>
	{
		/// <summary>
		/// Holds the shared NHibernate state object.
		/// </summary>
		private readonly NHState m_state;

		/// <summary>
		/// Gets the shared NHibernate state object.
		/// </summary>
		/// <value>The state object.</value>
		protected NHState State
		{
			get { return m_state; }
		}

		/// <summary>
		/// Loads an object by primary key.
		/// </summary>
		/// <param name="key">The primary key.</param>
		/// <returns>Loaded object.</returns>
		public TTransferObject Load(TPrimaryKey key)
		{
			return (TTransferObject) State.FindObjectByKey(typeof(TTransferObject), key);
		}

		/// <summary>
		/// Inserts a new object into a database.
		/// </summary>
		/// <param name="obj">The object to insert.</param>
		public void Insert(TTransferObject obj)
		{
			State.AddNewObject(obj);
		}

		/// <summary>
		/// Deletes an object from a database.
		/// </summary>
		/// <param name="obj">The object to delete.</param>
		public void Delete(TTransferObject obj)
		{
			State.DeleteObject(obj);
		}

		/// <summary>
		/// Saves all data objects of <typeparamref name="TTransferObject"/> type, syncronous.
		/// </summary>
		public virtual void SaveAll()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GenericDao{TTransferObject, TPrimaryKey}"/> class.
		/// </summary>
		/// <param name="state">The state.</param>
		public GenericDao(NHState state)
		{
			m_state = state;
		}
	}
}
