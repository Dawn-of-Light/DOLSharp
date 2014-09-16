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

using DOL.Database;
using DOL.Database.Attributes;

namespace DOL
{
	namespace Database
	{
		/// <summary>
		/// Account table
		/// </summary>
		[DataTable(TableName = "PlayerXEffect")]
		public class PlayerXEffect : DataObject
		{
			private string m_charid;
			private string m_effecttype;
			private bool m_ishandler;
			private int m_duration;
			private int m_var1;
			private int m_var2;
			private int m_var3;
			private int m_var4;
			private int m_var5;
			private int m_var6;
			private string m_spellLine;

			public PlayerXEffect()
			{
			}

			[DataElement(AllowDbNull = true)]
			public bool IsHandler
			{
				get
				{
					return m_ishandler;
				}
				set
				{
					Dirty = true;
					m_ishandler = value;
				}
			}
			[DataElement(AllowDbNull = true)]
			public int Var6
			{
				get
				{
					return m_var6;
				}
				set
				{
					Dirty = true;
					m_var6 = value;
				}
			}
			[DataElement(AllowDbNull = true)]
			public int Var5
			{
				get
				{
					return m_var5;
				}
				set
				{
					Dirty = true;
					m_var5 = value;
				}
			}
			[DataElement(AllowDbNull = true)]
			public int Var4
			{
				get
				{
					return m_var4;
				}
				set
				{
					Dirty = true;
					m_var4 = value;
				}
			}
			[DataElement(AllowDbNull = true)]
			public int Var3
			{
				get
				{
					return m_var3;
				}
				set
				{
					Dirty = true;
					m_var3 = value;
				}
			}
			[DataElement(AllowDbNull = true)]
			public int Var2
			{
				get
				{
					return m_var2;
				}
				set
				{
					Dirty = true;
					m_var2 = value;
				}
			}
			[DataElement(AllowDbNull = true)]
			public int Var1
			{
				get
				{
					return m_var1;
				}
				set
				{
					Dirty = true;
					m_var1 = value;
				}
			}
			[DataElement(AllowDbNull = true)]
			public int Duration
			{
				get
				{
					return m_duration;
				}
				set
				{
					Dirty = true;
					m_duration = value;
				}
			}


			[DataElement(AllowDbNull = true)]
			public string EffectType
			{
				get { return m_effecttype; }
				set
				{
					Dirty = true;
					m_effecttype = value;
				}
			}

			[DataElement(AllowDbNull = true)]
			public string SpellLine
			{
				get { return m_spellLine; }
				set
				{
					Dirty = true;
					m_spellLine = value;
				}
			}

			[DataElement(AllowDbNull = true, Index = true)]
			public string ChardID
			{
				get
				{
					return m_charid;
				}
				set
				{
					Dirty = true;
					m_charid = value;
				}
			}

		}
	}
}