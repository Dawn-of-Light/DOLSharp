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

		[DataTable(TableName = "DBHouseCharsXPerms")]
		public class DBHouseCharsXPerms : DataObject
		{
			//important data
			private int m_housenumber;
			private byte m_type;
			private string m_name;
			private int m_permLevel;
            private int m_slot;

			static bool m_autoSave;



			public DBHouseCharsXPerms()
			{
				m_autoSave = false;
			}


			[DataElement(AllowDbNull = false)]
			public int HouseNumber
			{
				get
				{
					return m_housenumber;
				}
				set
				{
					Dirty = true;
					m_housenumber = value;
				}
			}
			[DataElement(AllowDbNull = false)]
			public byte Type
			{
				get
				{
					return m_type;
				}
				set
				{
					Dirty = true;
					m_type = value;
				}
			}
			[DataElement(AllowDbNull = false)]
			public string Name
			{
				get
				{
					return m_name;
				}
				set
				{
					Dirty = true;
					m_name = value;
				}
			}
			[DataElement(AllowDbNull = false)]
			public int PermLevel
			{
				get
				{
					return m_permLevel;
				}
				set
				{
					Dirty = true;
					m_permLevel = value;
				}
			}

            [DataElement(AllowDbNull = false)]
            public int Slot
            {
                get
                {
                    return m_slot;
                }
                set
                {
                    Dirty = true;
                    m_slot = value;
                }
            }

			

			override public bool AutoSave
			{
				get
				{
					return m_autoSave;
				}
				set
				{
					m_autoSave = value;
				}
			}
		}
	}
}
