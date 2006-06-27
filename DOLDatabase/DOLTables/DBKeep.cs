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
	/// DB Keep is database of keep
	/// </summary>
	[DataTable(TableName="Keep",PreCache=true) ]
	public class DBKeep : DataObject
	{
		static bool	m_autoSave;
		private string m_name;
		private int m_region;
		private int m_x;
		private int m_y;
		private int m_z;
		private int m_heading;
		private int m_realm;
		private int m_level;
		private int m_keepID;
		//private int m_level;
		private string m_guildName;
		private int m_albionDifficultyLevel;
		private int m_midgardDifficultyLevel;
		private int m_hiberniaDifficultyLevel;
		private int m_originalRealm;
		private int m_type;

		/// <summary>
		/// Create a keep row
		/// </summary>
		public DBKeep()
		{
			m_autoSave=false;
			m_name = "";
			m_albionDifficultyLevel = 1;
			m_midgardDifficultyLevel = 1;
			m_hiberniaDifficultyLevel = 1;
			m_type = 0; //by default melee
		}

		/// <summary>
		/// autosave this table
		/// </summary>
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
		
		/// <summary>
		/// Index of keep
		/// </summary>
		[PrimaryKey]
		public int KeepID
		{
			get
			{
				return m_keepID;
			}
			set
			{   
				Dirty = true;
				m_keepID = value;
			}
		}
		
		/// <summary>
		/// Name of keep
		/// </summary>
		[DataElement(AllowDbNull=false)]
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
		
		/// <summary>
		/// Region of keep
		/// </summary>
		[DataElement(AllowDbNull=false)]
		public int Region
		{
			get
			{
				return m_region;
			}
			set
			{
				Dirty = true;
				m_region = value;
			}
		}
		
		/// <summary>
		/// X position of keep
		/// </summary>
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

		/// <summary>
		/// Y position of keep
		/// </summary>
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

		/// <summary>
		/// Z position of keep
		/// </summary>
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

		/// <summary>
		/// heading of keep
		/// </summary>
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

		/// <summary>
		/// Realm of keep
		/// </summary>
		[DataElement(AllowDbNull=true)]
		public int Realm
		{
			get
			{
				return m_realm;
			}
			set
			{   
				Dirty = true;
				m_realm = value;
			}
		}
		
		/// <summary>
		/// Level of keep
		/// </summary>
		[DataElement(AllowDbNull=false)]
		public int Level
		{
			get
			{
				return m_level;
			}
			set
			{   
				Dirty = true;
				m_level = value;
			}
		}

		/// <summary>
		/// The guild chich claim this keep
		/// </summary>
		[DataElement(AllowDbNull=true)]
		public string ClaimedGuildName
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

		/// <summary>
		/// Albion difficulty level
		/// </summary>
		[DataElement(AllowDbNull=true)]
		public int AlbionDifficultyLevel
		{
			get
			{
				return m_albionDifficultyLevel;
			}
			set
			{   
				Dirty = true;
				m_albionDifficultyLevel = value;
			}
		}

		/// <summary>
		/// Midgard difficulty level
		/// </summary>
		[DataElement(AllowDbNull=true)]
		public int MidgardDifficultyLevel
		{
			get
			{
				return m_midgardDifficultyLevel;
			}
			set
			{   
				Dirty = true;
				m_midgardDifficultyLevel = value;
			}
		}

		/// <summary>
		/// Hibernia difficulty level
		/// </summary>
		[DataElement(AllowDbNull=true)]
		public int HiberniaDifficultyLevel
		{
			get
			{
				return m_hiberniaDifficultyLevel;
			}
			set
			{   
				Dirty = true;
				m_hiberniaDifficultyLevel = value;
			}
		}

		/// <summary>
		/// Realm at start
		/// </summary>
		[DataElement(AllowDbNull=true)]
		public int OriginalRealm
		{
			get
			{
				return m_originalRealm;
			}
			set
			{   
				Dirty = true;
				m_originalRealm = value;
			}
		}

		/// <summary>
		/// Keep type
		/// </summary>
		[DataElement(AllowDbNull=true)]
		public int KeepType
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
		
	}
}
