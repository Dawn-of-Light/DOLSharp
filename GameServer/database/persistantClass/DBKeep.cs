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
using System.Collections;

namespace DOL.GS.Database
{
	/// <summary>
	/// DB Keep is database of keep
	/// </summary>
	public class DBKeep
	{
		private int m_id;
		
		private string m_name;
		private int m_region;
		private int m_x;
		private int m_y;
		private int m_z;
		private int m_heading;
		private int m_realm;
		private int m_level;
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
			m_name = "";
			m_albionDifficultyLevel = 1;
			m_midgardDifficultyLevel = 1;
			m_hiberniaDifficultyLevel = 1;
			m_type = 0; //by default melee
		}
		
		/// <summary>
		/// Index of keep
		/// </summary>
		public int KeepID
		{
			get
			{
				return m_id;
			}
			set
			{   
				m_id = value;
			}
		}
		
		/// <summary>
		/// Name of keep
		/// </summary>
		public string Name
		{
			get
			{
				return m_name;
			}
			set
			{   
				m_name = value;
			}
		}
		
		/// <summary>
		/// Region of keep
		/// </summary>
		public int Region
		{
			get
			{
				return m_region;
			}
			set
			{
				m_region = value;
			}
		}
		
		/// <summary>
		/// X position of keep
		/// </summary>
		public int X
		{
			get
			{
				return m_x;
			}
			set
			{
				m_x = value;
			}
		}

		/// <summary>
		/// Y position of keep
		/// </summary>
		public int Y
		{
			get
			{
				return m_y;
			}
			set
			{
				m_y = value;
			}
		}

		/// <summary>
		/// Z position of keep
		/// </summary>
		public int Z
		{
			get
			{
				return m_z;
			}
			set
			{
				m_z = value;
			}
		}

		/// <summary>
		/// heading of keep
		/// </summary>
		public int Heading
		{
			get
			{
				return m_heading;
			}
			set
			{
				m_heading = value;
			}
		}

		/// <summary>
		/// Realm of keep
		/// </summary>
		public int Realm
		{
			get
			{
				return m_realm;
			}
			set
			{   
				m_realm = value;
			}
		}
		
		/// <summary>
		/// Level of keep
		/// </summary>
		public int Level
		{
			get
			{
				return m_level;
			}
			set
			{   
				m_level = value;
			}
		}

		/// <summary>
		/// The guild chich claim this keep
		/// </summary>
		public string ClaimedGuildName
		{
			get
			{
				return m_guildName;
			}
			set
			{   
				m_guildName = value;
			}
		}

		/// <summary>
		/// Albion difficulty level
		/// </summary>
		public int AlbionDifficultyLevel
		{
			get
			{
				return m_albionDifficultyLevel;
			}
			set
			{   
				m_albionDifficultyLevel = value;
			}
		}

		/// <summary>
		/// Midgard difficulty level
		/// </summary>
		public int MidgardDifficultyLevel
		{
			get
			{
				return m_midgardDifficultyLevel;
			}
			set
			{   
				m_midgardDifficultyLevel = value;
			}
		}

		/// <summary>
		/// Hibernia difficulty level
		/// </summary>
		public int HiberniaDifficultyLevel
		{
			get
			{
				return m_hiberniaDifficultyLevel;
			}
			set
			{   
				m_hiberniaDifficultyLevel = value;
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
				m_originalRealm = value;
			}
		}

		/// <summary>
		/// Keep type
		/// </summary>
		public int KeepType
		{
			get
			{
				return m_type;
			}
			set
			{   
				m_type = value;
			}
		}
	}
}
