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

namespace DOL.GS.Collections
{
	/// <summary>
	/// List optimized for unique reference type objects.
	/// Do not use with value types and strings.
	/// </summary>
	public class DynamicList
	{
		/// <summary>
		/// Holds all objects stored in the list.
		/// </summary>
		private object[] m_objects;
		/// <summary>
		/// The count of objects in the list.
		/// </summary>
		private int m_count;

		/// <summary>
		/// Constructs a new instance with default capacity.
		/// </summary>
		public DynamicList()
		{
			m_objects = new object[16];
		}

		/// <summary>
		/// Constructs a new instance with specified capacity.
		/// </summary>
		/// <param name="capacity">The initial list capacity.</param>
		public DynamicList(int capacity)
		{
			m_objects = new object[capacity];
		}
		
		/// <summary>
		/// Gets the capacity of collection.
		/// </summary>
		public int Capacity
		{
			get { return m_objects.Length; }
		}

		/// <summary>
		/// Gets the count of objects in collection.
		/// </summary>
		public int Count
		{
			get { return m_count; }
		}

		/// <summary>
		/// Gets the objects buffer.
		/// </summary>
		public object[] Objects
		{
			get { return m_objects; }
		}

		/// <summary>
		/// Adds an object to collection.
		/// </summary>
		/// <param name="obj">The object to add.</param>
		public void Add(object obj)
		{
			int count = m_count;
			if (count++ == m_objects.Length)
				EnsureCapacity(count);
			m_objects[m_count] = obj;
			m_count = count;
		}
		
		/// <summary>
		/// Resizes the list if not big enough.
		/// </summary>
		/// <param name="min"></param>
		private void EnsureCapacity(int min)
		{
			int cap = m_objects.Length;
			if (cap < min)
			{
				cap = (cap == 0 ? 16 : cap*2);
				if (cap < min)
					cap = min;
			
				object[] objs = new object[cap];
				Array.Copy(m_objects, 0, objs, 0, m_count);
				m_objects = objs;
			}
		}
		
		/// <summary>
		/// Checks whether object is already in collection.
		/// </summary>
		/// <param name="obj">Object to look for.</param>
		/// <returns>True if found.</returns>
		public bool Contains(object obj)
		{
			for(int i = 0 ; i < m_count ; i++)
			{
				if (m_objects[i] == obj) return true;
			}
			return false;
		}

		/// <summary>
		/// Makes a copy of current list to provided array.
		/// </summary>
		/// <param name="objs">Target array.</param>
		public void CopyTo(object[] objs)
		{
			if (objs == null)
				throw new ArgumentNullException("objs");
			Array.Copy(m_objects, 0, objs, 0, m_count);
		}
	}
}
