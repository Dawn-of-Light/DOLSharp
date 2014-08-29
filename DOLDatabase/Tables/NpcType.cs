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
	/// Npc Type allow to set base behavior of mob, Realm, Class/Brain, Race / Faction, BodyType for Charm
	/// </summary>
	[DataTable(TableName="npc_type")]
	public class NpcType : DataObject
	{
		private long m_npc_typeID;
		
		/// <summary>
		/// Npc Type Primary Key Auto Increment
		/// </summary>
		[PrimaryKey(AutoIncrement = true)]
		public long Npc_typeID {
			get { return m_npc_typeID; }
			set { m_npc_typeID = value; }
		}
		
		private string m_typeID;
		
		/// <summary>
		/// Type Name for easy Reference
		/// </summary>
		[DataElement(AllowDbNull = false, Unique = true, Varchar = 150)]
		public string TypeID {
			get { return m_typeID; }
			set { m_typeID = value; Dirty = true; }
		}
		
		private bool m_peace;
		
		/// <summary>
		/// Force peace, default to true
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public bool Peace {
			get { return m_peace; }
			set { m_peace = value; Dirty = true; }
		}
		
		private byte m_realm;
		
		/// <summary>
		/// Mob Realm, default to 0 = Mob
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public byte Realm {
			get { return m_realm; }
			set { m_realm = value; Dirty = true; }
		}
		
		private ushort m_respawnTime;
		
		/// <summary>
		/// Mob Respawn Time (in seconds), null = instant, 0 = auto 
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public ushort RespawnTime {
			get { return m_respawnTime; }
			set { m_respawnTime = value; }
		}
		
		private ushort m_maxSpeed;
		
		/// <summary>
		/// Mob MaxSpeed
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public ushort MaxSpeed {
			get { return m_maxSpeed; }
			set { m_maxSpeed = value; Dirty = true; }
		}
		
		private string m_classType;
		
		/// <summary>
		/// Mob DOL ClassType (default to DOL.GS.GameNPC)
		/// </summary>
		[DataElement(AllowDbNull = true, Varchar = 255)]
		public string ClassType {
			get { return m_classType; }
			set { m_classType = value; Dirty = true; }
		}
		
		private string m_classBrain;
		
		/// <summary>
		/// Mob DOL Brain Type (default to No Brain)
		/// </summary>
		[DataElement(AllowDbNull = true, Varchar = 255)]
		public string ClassBrain {
			get { return m_classBrain; }
			set { m_classBrain = value; Dirty = true; }
		}
		
		private byte m_bodyType;
		
		/// <summary>
		/// Mob Body Type for Charm Check (null = not charmable)
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public byte BodyType {
			get { return m_bodyType; }
			set { m_bodyType = value; Dirty = true; }
		}
		
		private int m_raceID;
		
		/// <summary>
		/// Link to Race ID
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int RaceID {
			get { return m_raceID; }
			set { m_raceID = value; Dirty = true; }
		}
		
		private int m_factionID;
		
		/// <summary>
		/// Link to Faction ID
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int FactionID {
			get { return m_factionID; }
			set { m_factionID = value; Dirty = true; }
		}
		
		private string m_packageID;
		
		/// <summary>
		/// Package ID for import / export
		/// </summary>
		[DataElement(AllowDbNull = true, Varchar = 255)]
		public string PackageID {
			get { return m_packageID; }
			set { m_packageID = value; Dirty = true; }
		}
		
		/// <summary>
		/// Relation to Race Table (n,1)
		/// </summary>
		[Relation(LocalField = "RaceID", RemoteField = "ID", AutoLoad = true, AutoDelete = false)]
		public Race NpcRace;
		
		/// <summary>
		/// Relation to Faction Table (n, 1)
		/// </summary>
		[Relation(LocalField = "FactionID", RemoteField = "ID", AutoLoad = true, AutoDelete = false)]
		public DBFaction NpcFaction;
		
		/// <summary>
		/// Default Constructor
		/// </summary>
		public NpcType()
		{
			m_peace = true;
			m_realm = 0;
		}
	}
}
