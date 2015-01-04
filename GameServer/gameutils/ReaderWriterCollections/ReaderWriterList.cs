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
using System.Collections.Generic;
using System.Threading;

namespace DOL.GS
{
	/// <summary>
	/// ReaderWriterList is a IList implementaiton with ReaderWriterLockSlim for concurrent acess.
	/// This Collection supports Multiple Reader but only one writer. Enumerator is a Snapshot of Collection.
	/// You can use TryXXX Method to prevent race conditions between read checks and write lock acquirement.
	/// </summary>
	public class ReaderWriterList<T> : IList<T>
	{
		private readonly List<T> m_list;
		private readonly ReaderWriterLockSlim m_rwLock = new ReaderWriterLockSlim();
		
		public ReaderWriterList()
		{
			m_list = new List<T>();
		}
		
		public ReaderWriterList(int capacity)
		{
			m_list = new List<T>(capacity);
		}
		
		public ReaderWriterList(ICollection<T> collection)
		{
			m_list = new List<T>(collection);
		}

		#region Implementation of IEnumerable
		/// <summary>
		/// Returns an Enumerator on a Snapshot Collection
		/// </summary>
		/// <returns></returns>
		public IEnumerator<T> GetEnumerator()
		{
			m_rwLock.EnterReadLock();
			IEnumerable<T> newList = null;
			try
			{
				newList = m_list.ToArray();
			}
			finally
			{
				m_rwLock.ExitReadLock();
			}
			
			if (newList != null)
				return newList.GetEnumerator();
			
			return default(IEnumerator<T>);

		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
	        return GetEnumerator();
	    }
		#endregion    

		#region Implementation of ICollection<T>
		
		public void Add(T item)
		{
			m_rwLock.EnterWriteLock();
			try
			{
				m_list.Add(item);
			}
			finally
			{
				m_rwLock.ExitWriteLock();
			}
		}
		
		public void Clear()
		{
			m_rwLock.EnterWriteLock();
			try
			{
				m_list.Clear();
			}
			finally
			{
				m_rwLock.ExitWriteLock();
			}
		}
		
		public bool Contains(T item)
		{
			bool contains = false;
			m_rwLock.EnterReadLock();
			try
			{
				contains = m_list.Contains(item);
			}
			finally
			{
				m_rwLock.ExitReadLock();
			}
			return contains;
		}
		
		public void CopyTo(T[] array, int arrayIndex)
		{
			m_rwLock.EnterReadLock();
			try
			{
				m_list.CopyTo(array, arrayIndex);
			}
			finally
			{
				m_rwLock.ExitReadLock();
			}
		}
		
		public bool Remove(T item)
		{
			bool removed = false;
			m_rwLock.EnterWriteLock();
			try
			{
				removed = m_list.Remove(item);
			}
			finally
			{
				m_rwLock.ExitWriteLock();
			}
			return removed;
		}
		
		public int Count
		{
			get
			{
				int count = 0;
				m_rwLock.EnterReadLock();
				try
				{
					count = m_list.Count;
				}
				finally
				{
					m_rwLock.ExitReadLock();
				}
				return count;
			}
		}
		
		public bool IsReadOnly
		{
			get
			{
				bool readOnly = false;
				m_rwLock.EnterReadLock();
				try
				{
					readOnly = ((ICollection<T>)m_list).IsReadOnly;
				}
				finally
				{
					m_rwLock.ExitReadLock();
				}
				return readOnly;
			}
		}
		
		#endregion
		
		#region Implementation of IList<T>
		
		public int IndexOf(T item)
		{
			int index = -1;
			m_rwLock.EnterReadLock();
			try
			{
				index = m_list.IndexOf(item);
			}
			finally
			{
				m_rwLock.ExitReadLock();
			}
			return index;
		}
		
		public void Insert(int index, T item)
		{
			m_rwLock.EnterWriteLock();
			try
			{
				m_list.Insert(index, item);
			}
			finally
			{
				m_rwLock.ExitWriteLock();
			}
		}
		
		public void RemoveAt(int index)
		{
			m_rwLock.EnterWriteLock();
			try
			{
				m_list.RemoveAt(index);
			}
			finally
			{
				m_rwLock.ExitWriteLock();
			}
		}
		
		public T this[int index]
		{
			get
			{
				T ret = default(T);
				m_rwLock.EnterReadLock();
				try
				{
					ret = m_list[index];
				}
				finally
				{
					m_rwLock.ExitReadLock();
				}
				return ret;
			}
			set
			{
				m_rwLock.EnterWriteLock();
				try
				{
					m_list[index] = value;
				}
				finally
				{
					m_rwLock.ExitWriteLock();
				}
			}
		}
		
		#endregion
		
		public bool AddOrReplace(T item)
		{
			bool replaced = false;
			m_rwLock.EnterWriteLock();
			try
			{
				int index = m_list.IndexOf(item);
				if (index < 0)
				{
					// add
					m_list.Add(item);
				}
				else
				{
					// replace
					m_list[index] = item;
					replaced = true;
				}
					
			}
			finally
			{
				m_rwLock.ExitWriteLock();
			}
			return replaced;
		}
		
		public bool AddIfNotExists(T item)
		{
			bool added = false;
			m_rwLock.EnterUpgradeableReadLock();
			try
			{
				if (!m_list.Contains(item))
				{
					m_rwLock.EnterWriteLock();
					try
					{
						m_list.Add(item);
						added = true;
					}
					finally
					{
						m_rwLock.ExitWriteLock();
					}
				}
			}
			finally
			{
				m_rwLock.ExitUpgradeableReadLock();
			}
			return added;
		}
		
		public bool TryRemove(T item)
		{
			bool removed = false;
			m_rwLock.EnterUpgradeableReadLock();
			try
			{
				if (m_list.Contains(item))
				{
					m_rwLock.EnterWriteLock();
					try
					{
						m_list.Remove(item);
						removed = true;
					}
					finally
					{
						m_rwLock.ExitWriteLock();
					}
				}
			}
			finally
			{
				m_rwLock.ExitUpgradeableReadLock();
			}
			return removed;
		}
		
		public bool TryRemoveAt(int index)
		{
			bool removed = false;
			m_rwLock.EnterUpgradeableReadLock();
			try
			{
				if (m_list.Count > index)
				{
					m_rwLock.EnterWriteLock();
					try
					{
						m_list.RemoveAt(index);
						removed = true;
					}
					finally
					{
						m_rwLock.ExitWriteLock();
					}
				}
			}
			finally
			{
				m_rwLock.ExitUpgradeableReadLock();
			}
			return removed;
		}
		
		public bool TryInsert(int index, T item)
		{
			bool inserted = false;
			m_rwLock.EnterUpgradeableReadLock();
			try
			{
				if (m_list.Count > index)
				{
					m_rwLock.EnterWriteLock();
					try
					{
						m_list.Insert(index, item);
						inserted = true;
					}
					finally
					{
						m_rwLock.ExitWriteLock();
					}
				}
			}
			finally
			{
				m_rwLock.ExitUpgradeableReadLock();
			}
			return inserted;
		}
		
		public void FreezeWhile(Action<List<T>> method)
		{
			m_rwLock.EnterWriteLock();
			try
			{
				method(m_list);
			}
			finally
			{
				m_rwLock.ExitWriteLock();
			}
		}
		
		public N FreezeWhile<N>(Func<List<T>, N> func)
		{
			m_rwLock.EnterWriteLock();
			try
			{
				return func(m_list);
			}
			finally
			{
				m_rwLock.ExitWriteLock();
			}
		}
		
		public int FindIndex(Predicate<T> match)
		{
			m_rwLock.EnterReadLock();
			int index = -1;
			try
			{
				index = m_list.FindIndex(match);
			}
			finally
			{
				m_rwLock.ExitReadLock();
			}
			
			return index;
		}
	}
}
