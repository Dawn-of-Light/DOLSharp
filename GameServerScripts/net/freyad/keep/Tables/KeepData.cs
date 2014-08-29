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

namespace net.freyad.keep
{
	/// <summary>
	/// Keep Data table templated by Keep Positions using TemplateName.
	/// </summary>
	[DataTable(TableName="keepdata")]
	public class KeepData : DataObject
	{
		private long m_keepDataID;
		
		/// <summary>
		/// Keep Data Primary Key Auto Increment
		/// </summary>
		[PrimaryKey(AutoIncrement = true)]
		public long KeepDataID {
			get { return m_keepDataID; }
			set { m_keepDataID = value; }
		}
		
		private ushort m_keepID;
		
		/// <summary>
		/// Keep ID sent to client, Must be unique for each Region where the keep is located !
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true)]
		public ushort KeepID {
			get { return m_keepID; }
			set { m_keepID = value; Dirty = true; }
		}
		private string m_name;
		
		/// <summary>
		/// Keep Name displayed in Area and Player Selection.
		/// </summary>
		[DataElement(AllowDbNull = false, Varchar = 150, Index = true)]
		public string Name {
			get { return m_name; }
			set { m_name = value; Dirty = true; }
		}

		private string m_templateName;
		
		/// <summary>
		/// Keep Template Name, used to Link Hookpoints, Guards, Components, Doors, etc...
		/// </summary>
		[DataElement(AllowDbNull = false, Varchar = 150, Index = true)]
		public string TemplateName {
			get { return m_templateName; }
			set { m_templateName = value; Dirty = true; }
		}
		
		private string m_rulesType;
		
		/// <summary>
		/// Keep rules types, using a pre-defined set of rules : 
		/// null/default = timer based reseting keep with default RvR behavior
		/// rvr = standard rvr ruleset including level raising, balancing, guard respawn etc...
		/// decaying = decaying keep returning to owner after some kind of scripted fight.
		/// destroyed = fully destroyed keep with standard guards and capture
		/// destroynocapt = fully destroyed deserted keep
		/// portalkeep = fully leveled uncapturable undestroyable keep
		/// </summary>
		[DataElement(AllowDbNull = false, Varchar = 150, Index = true)]
		public string RulesType {
			get { return m_rulesType; }
			set { m_rulesType = value; Dirty = true; }
		}
		
		private byte m_owningRealm;
		
		/// <summary>
		/// Realm Currently Owning this Keep, 0 for no owner or "mob owner"
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true)]
		public byte OwningRealm {
			get { return m_owningRealm; }
			set { m_owningRealm = value; Dirty = true; }
		}
		
		private byte m_originalRealm;
		
		/// <summary>
		/// Realm originally owning this Keep
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true)]
		public byte OriginalRealm {
			get { return m_originalRealm; }
			set { m_originalRealm = value; Dirty = true; }
		}

		private byte m_mapIndex;
		
		/// <summary>
		/// Map index for warmap displaying.
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public byte MapIndex {
			get { return m_mapIndex; }
			set { m_mapIndex = value; Dirty = true; }
		}
		
		private byte m_keepIndex;
		
		/// <summary>
		/// Keep Index for Warmap displaying.
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public byte KeepIndex {
			get { return m_keepIndex; }
			set { m_keepIndex = value; Dirty = true; }
		}
		
		private byte m_towerIndex;
		
		/// <summary>
		/// Tower Index for Warmap displaying
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public byte TowerIndex {
			get { return m_towerIndex; }
			set { m_towerIndex = value; Dirty = true; }
		}

		private string m_claimedName;
		
		/// <summary>
		/// Claimed by this "Name"
		/// </summary>
		[DataElement(AllowDbNull = true, Varchar = 150, Index = true)]
		public string ClaimedName {
			get { return m_claimedName; }
			set { m_claimedName = value; Dirty = true; }
		}
		
		private int m_x;
		
		/// <summary>
		/// X position of this keep
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true)]
		public int X {
			get { return m_x; }
			set { m_x = value; Dirty = true; }
		}
		
		private int m_y;
		
		/// <summary>
		/// Y posiiton of this Keep
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true)]
		public int Y {
			get { return m_y; }
			set { m_y = value; Dirty = true; }
		}
		private int m_z;
		
		/// <summary>
		/// Z position of this keep
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true)]
		public int Z {
			get { return m_z; }
			set { m_z = value; Dirty = true; }
		}
		
		private ushort m_heading;
		
		/// <summary>
		/// Heading of this Keep
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public ushort Heading {
			get { return m_heading; }
			set { m_heading = value; Dirty = true; }
		}
		
		private int m_region;
		
		/// <summary>
		/// Region where this keep is spawned.
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true)]
		public int Region {
			get { return m_region; }
			set { m_region = value; Dirty = true; }
		}
		
		private byte m_baseLevel;
		
		/// <summary>
		/// Base Level of this keep 
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public byte BaseLevel {
			get { return m_baseLevel; }
			set { m_baseLevel = value; Dirty = true; }
		}
		
		private byte m_claimedBaseLevel;
		
		/// <summary>
		/// Base Level when Claimed
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public byte ClaimedBaseLevel {
			get { return m_claimedBaseLevel; }
			set { m_claimedBaseLevel = value; Dirty = true; }
		}
		
		private byte m_maxLevel;
		
		/// <summary>
		/// Max Level of this Keep when unclaimed.
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public byte MaxLevel {
			get { return m_maxLevel; }
			set { m_maxLevel = value; Dirty = true; }
		}
		
		private byte m_maxClaimedLevel;
		
		/// <summary>
		/// Max Level of this Keep when Claimed.
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public byte MaxClaimedLevel {
			get { return m_maxClaimedLevel; }
			set { m_maxClaimedLevel = value; Dirty = true; }
		}
		
		private byte m_playerBaseLevel;
		
		/// <summary>
		/// Base player Level which will face this Keep
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public byte PlayerBaseLevel {
			get { return m_playerBaseLevel; }
			set { m_playerBaseLevel = value; Dirty = true; }
		}
		
		private byte m_playerMaxLevel;
		
		/// <summary>
		/// Max player Level which will face this Keep (clamp guard and other NPC levels...)
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public byte PlayerMaxLevel {
			get { return m_playerMaxLevel; }
			set { m_playerMaxLevel = value; Dirty = true; }
		}

		private byte m_currentLevel;
		
		/// <summary>
		/// Current Keep Level
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public byte CurrentLevel {
			get { return m_currentLevel; }
			set { m_currentLevel = value; Dirty = true; }
		}
		
		
		private bool m_enabled;
		
		/// <summary>
		/// Is this Keep Enabled ?
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true)]
		public bool Enabled {
			get { return m_enabled; }
			set { m_enabled = value; Dirty = true; }
		}
		/*
		/// <summary>
		/// Last time this record was updated.
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true)]
		public DateTime Updated {
			get { return DateTime.UtcNow; }
			set { Dirty = true; }
		}*/

		/// <summary>
		/// Cross Relation to Keep Positions
		/// </summary>
		[Relation(LocalField = "TemplateName", RemoteField = "TemplateName", AutoLoad = true, AutoDelete = false)]
		public KeepDataXKeepDataPosition[] Template;

		/// <summary>
		/// 1,n Relation to Keep Component
		/// </summary>
		[Relation(LocalField = "TemplateName", RemoteField = "TemplateName", AutoLoad = true, AutoDelete = false)]
		public KeepDataComponent[] Component;
		
		/// <summary>
		/// Default Constructror
		/// </summary>
		public KeepData()
		{
		}
	}
}
