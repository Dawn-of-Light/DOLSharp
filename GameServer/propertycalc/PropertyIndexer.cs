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

namespace DOL.GS.PropertyCalc
{
	/// <summary>
	/// helper class for memory efficient usage of property fields
	/// it keeps integer values indexed by integer keys
	/// </summary>
	public sealed class PropertyIndexer : IPropertyIndexer
	{
		private readonly ReaderWriterDictionary<int, int> m_propDict;
		
		public PropertyIndexer()
		{
			m_propDict = new ReaderWriterDictionary<int, int>();
		}
		
		public PropertyIndexer(int fixSize)
		{
			m_propDict = new ReaderWriterDictionary<int, int>(fixSize);
		}
		
		public int this[int index]
		{
			get
			{
				try
				{
					return m_propDict[index];
				}
				catch
				{
					return 0;
				}
				
			}
			set
			{
				try
				{
					m_propDict[index] = value;
				}
				catch
				{
					// Warn Property is wrong.
				}
			}
		}
		
		public int this[eProperty index]
		{
			get
			{
				return this[(int)index];
			}
			set
			{
				this[(int)index] = value;
			}
		}
	}
}
