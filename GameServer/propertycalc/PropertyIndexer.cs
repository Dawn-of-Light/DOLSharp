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
using System.Collections.Concurrent;

namespace DOL.GS.PropertyCalc
{
	/// <summary>
	/// helper class for memory efficient usage of property fields
	/// it keeps integer values indexed by eProperty keys
	/// </summary>
	public sealed class PropertyIndexer : IPropertyIndexer
	{
		private ConcurrentDictionary<eProperty, int> m_propertyDict;

		public PropertyIndexer()
		{
		}


		public PropertyIndexer(int fixSize)
		{
			m_propertyDict = new ConcurrentDictionary<eProperty, int>();
		}

		public int this[eProperty index]
		{
			get
			{
				// Dict is available
				if (m_propertyDict != null)
				{
					if (m_propertyDict.ContainsKey(index))
					{
						// property is set return value
						return m_propertyDict[index];
					}
				}
				
				return 0;
			}
			set
			{
				if (m_propertyDict == null)
				{
					m_propertyDict = new ConcurrentDictionary<eProperty, int>();
				}
				
				if (m_propertyDict != null)
				{
					if (m_propertyDict.ContainsKey(index))
					{
						m_propertyDict[index] = value;
					}
					else
					{
						m_propertyDict.TryAdd(index, value);
					}
				}
			}
		}
		
	}
}
