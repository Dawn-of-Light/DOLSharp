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

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace DOL.GS
{
    /// <summary>
    /// ReaderWriterHashSet is a ISet implementaiton with ReaderWriterLockSlim for concurrent acess.
    /// This Collection supports Multiple Reader but only one writer. Enumerator is a Snapshot of Collection.
    /// You can use TryXXX Method to prevent race conditions between read checks and write lock acquirement.
    /// </summary>
    public class ReaderWriterHashSet<T> : ISet<T>
	{
		private readonly HashSet<T> m_set;
		private readonly ReaderWriterLockSlim m_rwLock = new ReaderWriterLockSlim();
		
		public ReaderWriterHashSet()
		{
			m_set = new HashSet<T>();
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
				newList = m_set.ToArray();
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
		
		void ICollection<T>.Add(T item)
		{
			Add(item);
		}
		
		public void Clear()
		{
			m_rwLock.EnterWriteLock();
			try
			{
				m_set.Clear();
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
				contains = m_set.Contains(item);
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
				m_set.CopyTo(array, arrayIndex);
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
				removed = m_set.Remove(item);
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
					count = m_set.Count;
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
					readOnly = ((ICollection<T>)m_set).IsReadOnly;
				}
				finally
				{
					m_rwLock.ExitReadLock();
				}
				return readOnly;
			}
		}
		
		#endregion
		
		#region Implementation of ISet<T>
		
		public bool Add(T item)
		{
			bool retval;
			m_rwLock.EnterWriteLock();
			try
			{
				retval = m_set.Add(item);
			}
			finally
			{
				m_rwLock.ExitWriteLock();
			}
			return retval;
		}
		
		public void UnionWith(IEnumerable<T> other)
		{
			m_rwLock.EnterWriteLock();
			try
			{
				m_set.UnionWith(other);
			}
			finally
			{
				m_rwLock.ExitWriteLock();
			}
		}

		public void IntersectWith(IEnumerable<T> other)
		{
			m_rwLock.EnterWriteLock();
			try
			{
				m_set.IntersectWith(other);
			}
			finally
			{
				m_rwLock.ExitWriteLock();
			}
		}

		public void ExceptWith(IEnumerable<T> other)
		{
			m_rwLock.EnterWriteLock();
			try
			{
				m_set.ExceptWith(other);
			}
			finally
			{
				m_rwLock.ExitWriteLock();
			}
		}

		public void SymmetricExceptWith(IEnumerable<T> other)
		{
			m_rwLock.EnterWriteLock();
			try
			{
				m_set.SymmetricExceptWith(other);
			}
			finally
			{
				m_rwLock.ExitWriteLock();
			}
		}

		public bool IsSubsetOf(IEnumerable<T> other)
		{
			bool retval;
			m_rwLock.EnterReadLock();
			try
			{
				retval = m_set.IsSubsetOf(other);
			}
			finally
			{
				m_rwLock.ExitReadLock();
			}
			return retval;
		}

		public bool IsSupersetOf(IEnumerable<T> other)
		{
			bool retval;
			m_rwLock.EnterReadLock();
			try
			{
				retval = m_set.IsSupersetOf(other);
			}
			finally
			{
				m_rwLock.ExitReadLock();
			}
			return retval;
		}

		public bool IsProperSupersetOf(IEnumerable<T> other)
		{
			bool retval;
			m_rwLock.EnterReadLock();
			try
			{
				retval = m_set.IsProperSupersetOf(other);
			}
			finally
			{
				m_rwLock.ExitReadLock();
			}
			return retval;
		}

		public bool IsProperSubsetOf(IEnumerable<T> other)
		{
			bool retval;
			m_rwLock.EnterReadLock();
			try
			{
				retval = m_set.IsProperSubsetOf(other);
			}
			finally
			{
				m_rwLock.ExitReadLock();
			}
			return retval;
		}

		public bool Overlaps(IEnumerable<T> other)
		{
			bool retval;
			m_rwLock.EnterReadLock();
			try
			{
				retval = m_set.Overlaps(other);
			}
			finally
			{
				m_rwLock.ExitReadLock();
			}
			return retval;
		}

		public bool SetEquals(IEnumerable<T> other)
		{
			bool retval;
			m_rwLock.EnterReadLock();
			try
			{
				retval = m_set.SetEquals(other);
			}
			finally
			{
				m_rwLock.ExitReadLock();
			}
			return retval;
		}
		#endregion
	}
}
