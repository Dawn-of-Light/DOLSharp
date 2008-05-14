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

using DOL.Database2;


namespace DOL
{
	namespace Database2
	{
		[Serializable]//TableName = "DBHousePermissions")]
		public class DBHousePermissions : DatabaseObject
		{
			//important data
			private int m_housenumber;
			private int m_permLevel;
			private byte m_enter;
			private byte m_vault1;
			private byte m_vault2;
			private byte m_vault3;
			private byte m_vault4;
			private byte m_appearance;
			private byte m_interior;
			private byte m_garden;
			private byte m_banish;
			private byte m_useMerchant;
			private byte m_tools;
			private byte m_bind;
			private byte m_merchant;
			private byte m_payRent;

			static bool m_autoSave;


            public DBHousePermissions(int n, int lvl)
                : base()
			{
				m_autoSave = false;
				HouseNumber = n;
				PermLevel = lvl;
			}

			public DBHousePermissions()
			{
				m_autoSave = false;
			}



			
			public int PermLevel
			{
				get
				{
					return m_permLevel;
				}
				set
				{
					m_Dirty = true;
					m_permLevel = value;
				}
			}

			
			public int HouseNumber
			{
				get
				{
					return m_housenumber;
				}
				set
				{
					m_Dirty = true;
					m_housenumber = value;
				}
			}

			
			public byte Enter
			{
				get
				{
					return m_enter;
				}
				set
				{
					m_Dirty = true;
					m_enter = value;
				}
			}
			
			public byte Vault1
			{
				get
				{
					return m_vault1;
				}
				set
				{
					m_Dirty = true;
					m_vault1 = value;
				}
			}
			
			public byte Vault2
			{
				get
				{
					return m_vault2;
				}
				set
				{
					m_Dirty = true;
					m_vault2 = value;
				}
			}
			
			public byte Vault3
			{
				get
				{
					return m_vault3;
				}
				set
				{
					m_Dirty = true;
					m_vault3 = value;
				}
			}
			
			public byte Vault4
			{
				get
				{
					return m_vault4;
				}
				set
				{
					m_Dirty = true;
					m_vault4 = value;
				}
			}
			
			public byte Appearance
			{
				get
				{
					return m_appearance;
				}
				set
				{
					m_Dirty = true;
					m_appearance = value;
				}
			}
			
			public byte Interior
			{
				get
				{
					return m_interior;
				}
				set
				{
					m_Dirty = true;
					m_interior = value;
				}
			}
			
			public byte Garden
			{
				get
				{
					return m_garden;
				}
				set
				{
					m_Dirty = true;
					m_garden = value;
				}
			}
			
			public byte Banish
			{
				get
				{
					return m_banish;
				}
				set
				{
					m_Dirty = true;
					m_banish = value;
				}
			}
			
			public byte UseMerchant
			{
				get
				{
					return m_useMerchant;
				}
				set
				{
					m_Dirty = true;
					m_useMerchant = value;
				}
			}
			
			public byte Tools
			{
				get
				{
					return m_tools;
				}
				set
				{
					m_Dirty = true;
					m_tools = value;
				}
			}
			
			public byte Bind
			{
				get
				{
					return m_bind;
				}
				set
				{
					m_Dirty = true;
					m_bind = value;
				}
			}
			
			public byte Merchant
			{
				get
				{
					return m_merchant;
				}
				set
				{
					m_Dirty = true;
					m_merchant = value;
				}
			}
			
			public byte PayRent
			{
				get
				{
					return m_payRent;
				}
				set
				{
					m_Dirty = true;
					m_payRent = value;
				}
			}
		}
	}
}
