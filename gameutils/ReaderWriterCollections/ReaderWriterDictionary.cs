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
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace DOL.GS
{
	/// <summary>
	/// ReaderWriterDictionary is a IDictionary implementaiton with ReaderWriterLockSlim for concurrent acess.
	/// This Collection supports Multiple Reader but only one writer. Enumerator is a Snapshot of Collection.
	/// You can use TryXXX Method to prevent race conditions between read checks and write lock acquirement.
	/// </summary>
	public class ReaderWriterDictionary<TKey, TValue> : IDictionary<TKey, TValue>
	{
		private readonly IDictionary<TKey, TValue> m_dictionary;
		private readonly ReaderWriterLockSlim m_rwLock = new ReaderWriterLockSlim();
		
		public ReaderWriterDictionary()
		{
			m_dictionary = new Dictionary<TKey, TValue>();
		}
		
		public ReaderWriterDictionary(int capacity)
		{
			m_dictionary = new Dictionary<TKey, TValue>(capacity);
		}
		
		public ReaderWriterDictionary(IDictionary<TKey, TValue> collection)
		{
			m_dictionary = new Dictionary<TKey, TValue>(collection);
		}
		
		#region implementation of IEnumerator
		/// <summary>
		/// Get an enumerator over collection Snapshot.
		/// </summary>
		/// <returns></returns>
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			m_rwLock.EnterReadLock();
			IEnumerable<KeyValuePair<TKey, TValue>> copy = null;
			try
			{
				copy = m_dictionary.ToArray();
			}
			finally
			{
				m_rwLock.ExitReadLock();
			}
			
			if (copy != null)
				return copy.GetEnumerator();
			
			return default(IEnumerator<KeyValuePair<TKey, TValue>>);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
	        return GetEnumerator();
	    }		
		#endregion
		
		#region implementation of ICollection<KeyValuePair<TKey, TValue>>
		public int Count
		{
			get
			{
				m_rwLock.EnterReadLock();
				int count = 0;
				try
				{
					count = m_dictionary.Count;
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
				m_rwLock.EnterReadLock();
				bool readOnly = false;
				try
				{
					readOnly = m_dictionary.IsReadOnly;
				}
				finally
				{
					m_rwLock.ExitReadLock();
				}
				
				return readOnly;
			}
		}

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
		{
			m_rwLock.EnterReadLock();
			try
			{
				m_dictionary.CopyTo(array, index);
			}
			finally
			{
				m_rwLock.ExitReadLock();
			}
		}
		
		public void Add(KeyValuePair<TKey, TValue> item)
		{
			m_rwLock.EnterWriteLock();
			try
			{
				m_dictionary.Add(item);
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
				m_dictionary.Clear();
			}
			finally
			{
				m_rwLock.ExitWriteLock();
			}
		}
		
		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			bool contains = false;
			m_rwLock.EnterReadLock();
			try
			{
				contains = m_dictionary.Contains(item);
			}
			finally
			{
				m_rwLock.ExitReadLock();
			}
			return contains;
		}
				
		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			bool removed;
			m_rwLock.EnterWriteLock();
			try
			{
				removed = m_dictionary.Remove(item);
			}
			finally
			{
				m_rwLock.ExitWriteLock();
			}
			
			return removed;
		}

		#endregion
		
		#region implementation of IDictionary<TKey, TValue>
		
		public TValue this[TKey key]
		{
			get
			{
				m_rwLock.EnterReadLock();
				TValue el = default(TValue);
				try
				{
					el = m_dictionary[key];
				}
				finally
				{
					m_rwLock.ExitReadLock();
				}
				return el;
			}
			set
			{
				m_rwLock.EnterWriteLock();
				try
				{
					m_dictionary[key] = value;
				}
				finally
				{
					m_rwLock.ExitWriteLock();
				}
			}
		}
		
		public ICollection<TKey> Keys
		{
			get
			{
				m_rwLock.EnterReadLock();
				ICollection<TKey> result = default(ICollection<TKey>);
				try
				{
					result = m_dictionary.Keys.ToArray();
				}
				finally
				{
					m_rwLock.ExitReadLock();
				}
				
				return result;
			}
		}
		
		public ICollection<TValue> Values
		{
			get
			{
				m_rwLock.EnterReadLock();
				ICollection<TValue> result = default(ICollection<TValue>);
				try
				{
					result = m_dictionary.Values.ToArray();
				}
				finally
				{
					m_rwLock.ExitReadLock();
				}
				return result;
			}
		}
		
		public bool ContainsKey(TKey key)
		{
			m_rwLock.EnterReadLock();
			bool contains = false;
			try
			{
				contains = m_dictionary.ContainsKey(key);
			}
			finally
			{
				m_rwLock.ExitReadLock();
			}
			
			return contains;
		}
		
		public void Add(TKey key, TValue value)
		{
			m_rwLock.EnterWriteLock();
			try
			{
				m_dictionary.Add(key, value);
			}
			finally
			{
				m_rwLock.ExitWriteLock();
			}
		}
		
		public bool Remove(TKey key)
		{
			m_rwLock.EnterWriteLock();
			bool removed = false;
			try
			{
				removed = m_dictionary.Remove(key);
			}
			finally
			{
				m_rwLock.ExitWriteLock();
			}
			return removed;
		}
		
		public bool TryGetValue(TKey key, out TValue value)
		{
			m_rwLock.EnterReadLock();
			bool found = false;
			try
			{
				found = m_dictionary.TryGetValue(key, out value);
			}
			finally
			{
				m_rwLock.ExitReadLock();
			}
						
			return found;
		}
		#endregion

		public bool AddOrReplace(TKey key, TValue val)
		{
			m_rwLock.EnterWriteLock();
			bool replaced = false;
			try
			{
				if (!m_dictionary.ContainsKey(key))
				{
					m_dictionary.Add(key, val);
				}
				else
				{
					m_dictionary[key] = val;
					replaced = true;
				}
			}
			finally
			{
				m_rwLock.ExitWriteLock();
			}
			
			return replaced;
		}
		
		public bool AddIfNotExists(TKey key, TValue val)
		{
			m_rwLock.EnterUpgradeableReadLock();
			bool added = false;
			try
			{
				if (!m_dictionary.ContainsKey(key))
				{
					m_rwLock.EnterWriteLock();
					try
					{
						m_dictionary.Add(key, val);
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
		
		public bool UpdateIfExists(TKey key, TValue val)
		{
			m_rwLock.EnterUpgradeableReadLock();
			bool replaced = false;
			try
			{
				if (m_dictionary.ContainsKey(key))
				{
					m_rwLock.EnterWriteLock();
					try
					{
						m_dictionary[key] = val;
						replaced = true;
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
			
			return replaced;
		}

		public bool TryRemove(TKey key, out TValue val)
		{
			m_rwLock.EnterUpgradeableReadLock();
			bool removed = false;
			val = default(TValue);
			try
			{
				if (m_dictionary.ContainsKey(key))
				{
					val = m_dictionary[key];
					m_rwLock.EnterWriteLock();
					try
					{
						removed = m_dictionary.Remove(key);
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

		
		public void FreezeWhile(Action<IDictionary<TKey, TValue>> method)
		{
			m_rwLock.EnterWriteLock();
			try
			{
				method(m_dictionary);
			}
			finally
			{
				m_rwLock.ExitWriteLock();
			}
		}
		
		public N FreezeWhile<N>(Func<IDictionary<TKey, TValue>, N> func)
		{
			m_rwLock.EnterWriteLock();
			try
			{
				return func(m_dictionary);
			}
			finally
			{
				m_rwLock.ExitWriteLock();
			}
		}
	}
}
