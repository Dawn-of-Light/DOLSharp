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
		private struct PropEntry
		{
			public int key;
			public int value;
		}

		private int count = 0;

		private PropEntry[] m_entries;
		private readonly int[] m_staticArray;

		private const int REALLOCATE_COUNT = 5;

		public PropertyIndexer()
		{
		}


		public PropertyIndexer(int fixSize)
		{
			m_staticArray = new int[fixSize];
		}

		public int this[int index]
		{
			get
			{
				if (m_staticArray != null)
				{
					if (index < m_staticArray.Length)
					{
						return m_staticArray[index];
					}
					else
					{
						return 0;
					}
				}
				if (count == 0) return 0;
				lock (m_entries)
				{
					for (int i = 0; i < m_entries.Length; i++)
					{
						if (m_entries[i].key == index)
						{
							return m_entries[i].value;
						}
					}
				}
				return 0;
			}
			set
			{
				if (m_staticArray != null)
				{
					if (index < m_staticArray.Length)
					{
						m_staticArray[index] = value;
					}
					return;
				}

				lock (this)
				{

					// find entry
					int arrayIndex = -1;
					if (m_entries != null)
					{
						for (int i = 0; i < m_entries.Length; i++)
						{
							if (m_entries[i].key == index)
							{
								arrayIndex = i;
								break;
							}
						}
					}

					if (value == 0 && arrayIndex >= 0)
					{
						m_entries[arrayIndex].key = 0;
						m_entries[arrayIndex].value = 0;
						count--;
					}
					else
					{
						if (arrayIndex >= 0)
						{
							m_entries[arrayIndex].value = value;
						}
						else
						{
							if (m_entries == null)
							{
								m_entries = new PropEntry[REALLOCATE_COUNT];
								m_entries[0].key = index;
								m_entries[0].value = value;
								count++;
								return;
							}

							for (int i = 0; i < m_entries.Length; i++)
							{
								if (m_entries[i].key == 0)
								{
									m_entries[i].key = index;
									m_entries[i].value = value;
									count++;
									return;
								}
							}

							// reallocate
							PropEntry[] newdata = new PropEntry[m_entries.Length + REALLOCATE_COUNT];
							Array.Copy(m_entries, newdata, m_entries.Length);
							newdata[m_entries.Length].key = index;
							newdata[m_entries.Length].value = value;
							m_entries = newdata;
							count++;
						}
					}
				}
			}
		}
	}
}
