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

using DOL.Database.Attributes;

namespace DOL.Database
{
	/// <summary>
	/// Npc Table Holding spawn points of Non-Playable Livings
	/// </summary>
	[DataTable(TableName = "npc")]
	public class Npc : DataObject
	{
		private long m_npcID;
		
		/// <summary>
		/// Mob Primary Key Auto Increment
		/// </summary>
		[PrimaryKey(AutoIncrement = true)]
		public long NpcID {
			get { return m_npcID; }
			set { m_npcID = value; }
		}
		
		private string m_name;
		
		/// <summary>
		/// Mob's Name
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true, Varchar = 100)]
		public string Name {
			get { return m_name; }
			set { m_name = value; Dirty = true; }
		}
		
		private byte m_level;
		
		/// <summary>
		/// Mob's Level
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public byte Level {
			get { return m_level; }
			set { m_level = value; Dirty = true; }
		}
		
		private uint m_x;
		
		/// <summary>
		/// Mob's X position
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public uint X {
			get { return m_x; }
			set { m_x = value; Dirty = true; }
		}
		
		private uint m_y;
		
		/// <summary>
		/// Mob's Y position
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public uint Y {
			get { return m_y; }
			set { m_y = value; Dirty = true; }
		}
		
		private uint m_z;
		
		/// <summary>
		/// Mob's Z position
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public uint Z {
			get { return m_z; }
			set { m_z = value; Dirty = true; }
		}
		
		private ushort m_heading;
		
		/// <summary>
		/// Mob's spawn heading.
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public ushort Heading {
			get { return m_heading; }
			set { m_heading = value; Dirty = true; }
		}
		
		private ushort m_region;
		
		/// <summary>
		/// Mob's region spawn
		/// </summary>
		[DataElement(AllowDbNull = false, IndexColumns = "X,Y,Z")]
		public ushort Region {
			get { return m_region; }
			set { m_region = value; Dirty = true; }
		}

		private bool m_swimming;
		
		/// <summary>
		/// Mob's swimming state
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public bool Swimming {
			get { return m_swimming; }
			set { m_swimming = value; Dirty = true; }
		}
		
		private bool m_flying;
		
		/// <summary>
		/// Mob's Flying state
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public bool Flying {
			get { return m_flying; }
			set { m_flying = value; Dirty = true; }
		}
		
		private string m_spotID;
		
		/// <summary>
		/// Mob's Spot ID to attach additional Data
		/// </summary>
		[DataElement(AllowDbNull = true, Varchar = 150, Index = true)]
		public string SpotID {
			get { return m_spotID; }
			set { m_spotID = value; Dirty = true; }
		}
		
		private string m_packageID;
		
		/// <summary>
		/// Mob's Package ID for export/import.
		/// </summary>
		[DataElement(AllowDbNull = true, Varchar = 255)]
		public string PackageID {
			get { return m_packageID; }
			set { m_packageID = value; Dirty = true; }
		}

		/// <summary>
		/// Cross BaseTemplate Link (n,n)
		/// </summary>
		[Relation(LocalField = "SpotID", RemoteField = "SpotID", AutoLoad = true, AutoDelete = false)]
		public NpcXBaseTemplate[] BaseTemplates;
				
		/// <summary>
		/// Link to Aggro Table (n,1)
		/// </summary>
		[Relation(LocalField = "SpotID", RemoteField = "SpotID", AutoLoad = true, AutoDelete = false)]
		public NpcXAggro AggroLink;
		
		/// <summary>
		/// Link to Merchant Table (n, 1)
		/// </summary>
		[Relation(LocalField = "SpotID", RemoteField = "SpotID", AutoLoad = true, AutoDelete = false)]
		public NpcXMerchantItem MerchantItem;
		
		/// <summary>
		/// Cross Equipment Link (n,n)
		/// </summary>
		[Relation(LocalField = "SpotID", RemoteField = "SpotID", AutoLoad = true, AutoDelete = false)]
		public NpcXEquipment[] Equipment;
		
		/// <summary>
		/// Link to Npc Type Table (n, 1)
		/// </summary>
		[Relation(LocalField = "SpotID", RemoteField = "SpotID", AutoLoad = true, AutoDelete = false)]
		public NpcXType TypeLink;
		
		/// <summary>
		/// Link to Npc Spell Table (n, 1)
		/// </summary>
		[Relation(LocalField = "SpotID", RemoteField = "SpotID", AutoLoad = true, AutoDelete = false)]
		public NpcXSpell SpellLink;
		
		/// <summary>
		/// Link to Npc Style Table (n, 1)
		/// </summary>
		[Relation(LocalField = "SpotID", RemoteField = "SpotID", AutoLoad = true, AutoDelete = false)]
		public NpcXStyle StyleLink;
		
		/// <summary>
		/// Link to Npc Ability Table (n, 1)
		/// </summary>
		[Relation(LocalField = "SpotID", RemoteField = "SpotID", AutoLoad = true, AutoDelete = false)]
		public NpcXAbility AbilityLink;
		
		/// <summary>
		/// Link to Npc Stat Table (n, 1)
		/// </summary>
		[Relation(LocalField = "SpotID", RemoteField = "SpotID", AutoLoad = true, AutoDelete = false)]
		public NpcXStat StatLink;
		
		/// <summary>
		/// Default Constructor
		/// </summary>
		public Npc()
		{
			m_swimming = false;
			m_flying = false;
		}
	}
}
