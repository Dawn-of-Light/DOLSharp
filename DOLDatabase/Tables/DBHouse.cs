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
		[DataTable(TableName="DBHouse")]
		public class DBHouse : DataObject
		{
			private int m_housenumber;
			private int m_x;
			private int m_y;
			private int m_z;
			private ushort m_regionid;
			private int m_heading;
			private string m_name;
			private int m_model;
			private DateTime m_creationtime;
			private int m_emblem;
			private int m_porchroofcolor;
			private int m_porchmaterial;
			private int m_roofmaterial;
			private int m_doormaterial;
			private int m_wallmaterial;
			private int m_trussmaterial;
			private int m_windowmaterial;
			private int m_rug1color;
			private int m_rug2color;
			private int m_rug3color;
			private int m_rug4color;
			private bool m_indoorguildbanner;
			private bool m_indoorguildshield;
			private bool m_outdoorguildshield;
			private bool m_outdoorguildbanner;
			private bool m_porch;
			private string m_ownerid;
			private DateTime m_lastpaid;
			private long m_keptmoney;
			private bool m_nopurge;
			private bool m_guildHouse;
			private string m_guildName;
			private bool m_hasConsignment;

			public DBHouse(){ }
			
			[PrimaryKey]
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
			[DataElement(AllowDbNull=false)]
			public int X
			{
				get
				{
					return m_x;
				}
				set
				{
					Dirty = true;
					m_x = value;
				}
			}
			[DataElement(AllowDbNull=false)]
			public int Y
			{
				get
				{
					return m_y;
				}
				set
				{
					Dirty = true;
					m_y = value;
				}
			}
			[DataElement(AllowDbNull=false)]
			public int Z
			{
				get
				{
					return m_z;
				}
				set
				{
					Dirty = true;
					m_z = value;
				}
			}
			[DataElement(AllowDbNull=false)]
			public ushort RegionID
			{
				get
				{
					return m_regionid;
				}
				set
				{
					Dirty = true;
					m_regionid = value;
				}
			}
			[DataElement(AllowDbNull=false)]
			public int Heading
			{
				get
				{
					return m_heading;
				}
				set
				{
					Dirty = true;
					m_heading = value;
				}
			}
			[DataElement(AllowDbNull=true)]
			public DateTime CreationTime
			{
				get
				{
					return m_creationtime;
				}
				set
				{
					Dirty = true;
					m_creationtime = value;
				}
			}
			[DataElement(AllowDbNull=true)]
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
			[DataElement(AllowDbNull=true)]
			public int Model
			{
				get
				{
					return m_model;
				}
				set
				{
					Dirty = true;
					m_model = value;
				}
			}
			[DataElement(AllowDbNull=true)]
			public int Emblem
			{
				get
				{
					return m_emblem;
				}
				set
				{
					Dirty = true;
					m_emblem = value;
				}
			}
			[DataElement(AllowDbNull=true)]
			public int PorchRoofColor
			{
				get
				{
					return m_porchroofcolor;
				}
				set
				{
					Dirty = true;
					m_porchroofcolor = value;
				}
			}
			[DataElement(AllowDbNull=true)]
			public int PorchMaterial
			{
				get
				{
					return m_porchmaterial;
				}
				set
				{
					Dirty = true;
					m_porchmaterial = value;
				}
			}
			[DataElement(AllowDbNull=true)]
			public int RoofMaterial
			{
				get
				{
					return m_roofmaterial;
				}
				set
				{
					Dirty = true;
					m_roofmaterial = value;
				}
			}
			[DataElement(AllowDbNull=true)]
			public int DoorMaterial
			{
				get
				{
					return m_doormaterial;
				}
				set
				{
					Dirty = true;
					m_doormaterial = value;
				}
			}
			[DataElement(AllowDbNull=true)]
			public int WallMaterial
			{
				get
				{
					return m_wallmaterial;
				}
				set
				{
					Dirty = true;
					m_wallmaterial = value;
				}
			}
			[DataElement(AllowDbNull=true)]
			public int TrussMaterial
			{
				get
				{
					return m_trussmaterial;
				}
				set
				{
					Dirty = true;
					m_trussmaterial = value;
				}
			}
			[DataElement(AllowDbNull=true)]
			public int WindowMaterial
			{
				get
				{
					return m_windowmaterial;
				}
				set
				{
					Dirty = true;
					m_windowmaterial = value;
				}
			}
			[DataElement(AllowDbNull=true)]
			public int Rug1Color
			{
				get
				{
					return m_rug1color;
				}
				set
				{
					Dirty = true;
					m_rug1color = value;
				}
			}
			[DataElement(AllowDbNull=true)]
			public int Rug2Color
			{
				get
				{
					return m_rug2color;
				}
				set
				{
					Dirty = true;
					m_rug2color = value;
				}
			}
			[DataElement(AllowDbNull=true)]
			public int Rug3Color
			{
				get
				{
					return m_rug3color;
				}
				set
				{
					Dirty = true;
					m_rug3color = value;
				}
			}
			[DataElement(AllowDbNull=true)]
			public int Rug4Color
			{
				get
				{
					return m_rug4color;
				}
				set
				{
					Dirty = true;
					m_rug4color = value;
				}
			}
			[DataElement(AllowDbNull=true)]
			public bool IndoorGuildBanner
			{
				get
				{
					return m_indoorguildbanner;
				}
				set
				{
					Dirty = true;
					m_indoorguildbanner = value;
				}
			}
			[DataElement(AllowDbNull=true)]
			public bool IndoorGuildShield
			{
				get
				{
					return m_indoorguildshield;
				}
				set
				{
					Dirty = true;
					m_indoorguildshield = value;
				}
			}
			[DataElement(AllowDbNull=true)]
			public bool OutdoorGuildBanner
			{
				get
				{
					return m_outdoorguildbanner;
				}
				set
				{
					Dirty = true;
					m_outdoorguildbanner = value;
				}
			}
			[DataElement(AllowDbNull=true)]
			public bool OutdoorGuildShield
			{
				get
				{
					return m_outdoorguildshield;
				}
				set
				{
					Dirty = true;
					m_outdoorguildshield = value;
				}
			}
			[DataElement(AllowDbNull=true)]
			public bool Porch
			{
				get
				{
					return m_porch;
				}
				set
				{
					Dirty = true;
					m_porch = value;
				}
			}

			[DataElement(AllowDbNull=true)]
			public string OwnerID
			{
				get
				{
					return m_ownerid;
				}
				set
				{
					Dirty = true;
					m_ownerid = value;
				}
			}

			[DataElement(AllowDbNull = true)]
			public DateTime LastPaid
			{
				get
				{
					return m_lastpaid;
				}
				set
				{
					Dirty = true;
					m_lastpaid = value;
				}
			}

			[DataElement(AllowDbNull = true)]
			public long KeptMoney
			{
				get
				{
					return m_keptmoney;
				}
				set
				{
					Dirty = true;
					m_keptmoney = value;
				}
			}

			[DataElement(AllowDbNull = true)]
			public bool NoPurge
			{
				get
				{
					return m_nopurge;
				}
				set
				{
					Dirty = true;
					m_nopurge = value;
				}
			}

			[DataElement(AllowDbNull = true)]
			public bool GuildHouse
			{
				get
				{
					return m_guildHouse;
				}
				set
				{
					Dirty = true;
					m_guildHouse = value;
				}
			}
			[DataElement(AllowDbNull = true)]
			public string GuildName
			{
				get
				{
					return m_guildName;
				}
				set
				{
					Dirty = true;
					m_guildName = value;
				}
			}

			[DataElement(AllowDbNull = true)]
			public bool HasConsignment
			{
				get
				{
					return m_hasConsignment;
				}
				set
				{
					Dirty = true;
					m_hasConsignment = value;
				}
			}
			
			//[Relation(LocalField = "HouseNumber", RemoteField = "HouseNumber", AutoLoad = true, AutoDelete=true)]
			//public DBHouseIndoorItem[] IndoorItems;

			//[Relation(LocalField = "HouseNumber", RemoteField = "HouseNumber", AutoLoad = true, AutoDelete=true)]
			//public DBHouseOutdoorItem[] OutdoorItems;
		}
	}
}
