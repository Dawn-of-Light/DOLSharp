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
namespace DOL.Database2
{
	/// <summary>
	/// DB relic is database of relic
	/// </summary>
    [Serializable]//TableName = "Relic")]
	public class DBRelic : DatabaseObject
	{
		static bool m_autoSave;
		private int m_relicID;
		private int m_region;
		private int m_x;
		private int m_y;
		private int m_z;
		private int m_heading;
		private int m_realm;
		private int m_originalRealm;
		private int m_type;



		/// <summary>
		/// Create a relic row
		/// </summary>
		public DBRelic()
		{
			m_autoSave = true;
		}


		/// <summary>
		/// Index of relic
		/// </summary>
		////[PrimaryKey]
		public int RelicID
		{
			get
			{
				return m_relicID;
			}
			set
			{
				m_Dirty = true;
				m_relicID = value;
			}
		}

		/// <summary>
		/// Region of relic
		/// </summary>
		
		public int Region
		{
			get
			{
				return m_region;
			}
			set
			{
				m_Dirty = true;
				m_region = value;
			}
		}

		/// <summary>
		/// X position of relic
		/// </summary>
		
		public int X
		{
			get
			{
				return m_x;
			}
			set
			{
				m_Dirty = true;
				m_x = value;
			}
		}

		/// <summary>
		/// Y position of relic
		/// </summary>
		
		public int Y
		{
			get
			{
				return m_y;
			}
			set
			{
				m_Dirty = true;
				m_y = value;
			}
		}

		/// <summary>
		/// Z position of relic
		/// </summary>
		
		public int Z
		{
			get
			{
				return m_z;
			}
			set
			{
				m_Dirty = true;
				m_z = value;
			}
		}

		/// <summary>
		/// heading of relic
		/// </summary>
		
		public int Heading
		{
			get
			{
				return m_heading;
			}
			set
			{
				m_Dirty = true;
				m_heading = value;
			}
		}

		/// <summary>
		/// Realm of relic
		/// </summary>
		
		public int Realm
		{
			get
			{
				return m_realm;
			}
			set
			{
				m_Dirty = true;
				m_realm = value;
			}
		}


		/// <summary>
		/// Realm at start
		/// </summary>
		
		public int OriginalRealm
		{
			get
			{
				return m_originalRealm;
			}
			set
			{
				m_Dirty = true;
				m_originalRealm = value;
			}
		}

		/// <summary>
		/// relic type, 0 is melee, 1 is magic
		/// </summary>
		
		public int relicType
		{
			get
			{
				return m_type;
			}
			set
			{
				m_Dirty = true;
				m_type = value;
			}
		}
	}
}
