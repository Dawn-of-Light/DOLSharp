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
	public class Mob
	{
		private int			m_id;
		private string		m_type;
		private string		m_name;
		private string		m_guild;		
		private int			m_x;
		private int			m_y;
		private int			m_z;
		private int			m_speed;
		private int			m_heading;
		private int			m_region;
		private int			m_model;
		private byte		m_size;
		private byte		m_level;
		private byte		m_realm;
		private byte		m_flags;
		private int			m_aggrolevel;
		private int			m_aggrorange;
		private int			m_meleeDamageType;
		private int			m_respawnInterval;
		private int			m_faction;

		private string		m_equipmentTemplateID;

		private string		m_itemsListTemplateID;

		
		public Mob()
		{
			m_type = "DOL.GS.GameMob";
			m_equipmentTemplateID = "";
			m_meleeDamageType = 2; // slash by default
			m_respawnInterval = -1; // randow respawn by default
		}

		public int MobID
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

		public string ClassType
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
				
		public string Guild
		{
			get
			{
				return m_guild;
			}
			set
			{
				m_guild = value;
			}
		}

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
		
		public int Speed
		{
			get
			{
				return m_speed;
			}
			set
			{   
				m_speed = value;
			}
		}

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
		
		public int Model
		{
			get
			{
				return m_model;
			}
			set
			{   
				m_model = value;
			}
		}
		
		public byte Size
		{
			get
			{
				return m_size;
			}
			set
			{   
				m_size = value;
			}
		}
		
		public byte Level
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
		
		public byte Realm
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
				
		public string EquipmentTemplateID
		{
			get
			{
				return m_equipmentTemplateID;
			}
			set
			{   
				m_equipmentTemplateID = value;
			}
		}

		public string ItemsListTemplateID
		{
			get
			{
				return m_itemsListTemplateID;
			}
			set
			{   
				m_itemsListTemplateID = value;
			}
		}

		public byte Flags
		{
			get
			{
				return m_flags;
			}
			set
			{   
				m_flags = value;
			}
		}

		public int AggroLevel
		{
			get	
			{ 
				return m_aggrolevel;
			}
			set 
			{ 
				m_aggrolevel = value;	
			}
		}

		public int AggroRange
		{
			get	
			{ 
				return m_aggrorange;
			}
			set 
			{ 
				m_aggrorange = value;	
			}
		}

		public int MeleeDamageType
		{
			get 
			{ 
				return m_meleeDamageType; 
			}
			set 
			{ 
				m_meleeDamageType = value; 
			}
		}

		public int RespawnInterval
		{
			get
			{
				return m_respawnInterval;
			}
			set
			{ 
				m_respawnInterval = value; 
			}
		}

		public int FactionID
		{
			get
			{ 
				return m_faction; 
			}
			set 
			{ 
				m_faction = value; 
			}
		}
	}
}

