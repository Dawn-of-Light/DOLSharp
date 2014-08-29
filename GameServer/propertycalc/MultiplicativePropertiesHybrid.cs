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
	/// Implements multiplicative properties using Concurrent Collections
	/// </summary>
	public sealed class MultiplicativePropertiesHybrid : IMultiplicativeProperties
	{
		// Dictionary for Properties and linked value.
		private ConcurrentDictionary<eProperty, ConcurrentDictionary<object, double>> m_propertiesDict = new ConcurrentDictionary<eProperty, ConcurrentDictionary<object, double>>();

		/// <summary>
		/// Adds new value, if key exists value will be overwriten
		/// </summary>
		/// <param name="index">The property index</param>
		/// <param name="key">The key used to remove value later</param>
		/// <param name="value">The value added</param>
		public void Set(eProperty index, object key, double ratio)
		{
			if (!m_propertiesDict.ContainsKey(index))
			{
				m_propertiesDict.TryAdd(index, new ConcurrentDictionary<object, double>());
			}
			
			if (!m_propertiesDict[index].ContainsKey(key))
			{
				m_propertiesDict[index].TryAdd(key, ratio);
			}
			else
			{
				m_propertiesDict[index][key] = ratio;
			}
		}

		/// <summary>
		/// Removes stored value
		/// </summary>
		/// <param name="index">The property index</param>
		/// <param name="key">The key use to add the value</param>
		public void Remove(eProperty index, object key)
		{
			// Check if index and key exists
			if (m_propertiesDict.ContainsKey(index) && m_propertiesDict[index].ContainsKey(key))
			{
				double dummy;
				m_propertiesDict[index].TryRemove(key, out dummy);
				
				// empty the dictionary
				if (m_propertiesDict[index].Count < 1)
				{
					ConcurrentDictionary<object, double> rem;
					m_propertiesDict.TryRemove(index, out rem);
				}
			}
		}

		/// <summary>
		/// Gets the property value
		/// </summary>
		/// <param name="index">The property index</param>
		/// <returns>The property value (1.0 = 100%)</returns>
		public double Get(eProperty index)
		{
			// if property exist calc ratio.
			if (m_propertiesDict.ContainsKey(index))
			{
				double result = 1.0;
				
				foreach (double val in m_propertiesDict[index].Values)
				{
					double ratio = val;
					result *= ratio;
				}
				
				return result;
			}
			
			// default return 1.0
			return 1.0;
		}
	}
}
