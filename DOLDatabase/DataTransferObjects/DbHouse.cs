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

namespace DOL.Database.DataTransferObjects
{
	public class DbHouse
	{
		private int m_housenumber;
		private int m_x;
		private int m_y;
		private int m_z;
		private int m_regionid;
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
		private string m_ownerids;


		public int HouseNumber
		{
			get
			{
				return m_housenumber;
			}
			set
			{
				m_housenumber = value;
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
		
		public int RegionID
		{
			get
			{
				return m_regionid;
			}
			set
			{
				m_regionid = value;
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
		
		public DateTime CreationTime
		{
			get
			{
				return m_creationtime;
			}
			set
			{
				m_creationtime = value;
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
		
		public int Emblem
		{
			get
			{
				return m_emblem;
			}
			set
			{
				m_emblem = value;
			}
		}
		
		public int PorchRoofColor
		{
			get 
			{ 
				return m_porchroofcolor;  
			} 
			set 
			{ 
				m_porchroofcolor = value; 
			}
		}
		
		public int PorchMaterial
		{
			get 
			{ 
				return m_porchmaterial;  
			} 
			set 
			{ 
				m_porchmaterial = value; 
			}
		}
		
		public int RoofMaterial
		{
			get 
			{ 
				return m_roofmaterial;  
			} 
			set 
			{ 
				m_roofmaterial = value; 
			}
		}	
		
		public int DoorMaterial
		{
			get 
			{ 
				return m_doormaterial;  
			} 
			set 
			{ 
				m_doormaterial = value; 
			}
		}
		
		public int WallMaterial
		{
			get 
			{ 
				return m_wallmaterial;  
			} 
			set 
			{ 
				m_wallmaterial = value; 
			}
		}
		
		public int TrussMaterial
		{
			get 
			{ 
				return m_trussmaterial;  
			} 
			set 
			{ 
				m_trussmaterial = value; 
			}
		}
		
		public int WindowMaterial
		{
			get 
			{ 
				return m_windowmaterial;  	
			} 
			set 
			{ 
				m_windowmaterial = value; 
			}
		}
		
		public int Rug1Color
		{
			get 
			{ 
				return m_rug1color;  	
			} 
			set 
			{ 
				m_rug1color = value; 
			}
		}
		
		public int Rug2Color
		{
			get 
			{ 
				return m_rug2color;  	
			} 
			set 
			{ 
				m_rug2color = value; 
			}
		}
		
		public int Rug3Color
		{
			get 
			{ 
				return m_rug3color;  	
			} 
			set 
			{ 
				m_rug3color = value; 
			}
		}
		
		public int Rug4Color
		{
			get 
			{ 
				return m_rug4color;  	
			} 
			set 
			{ 
				m_rug4color = value; 
			}
		}
		
		public bool IndoorGuildBanner
		{
			get 
			{ 
				return m_indoorguildbanner;  	
			} 
			set 
			{ 
				m_indoorguildbanner = value; 
			}
		}
		
		public bool IndoorGuildShield
		{
			get 
			{ 
				return m_indoorguildshield;  	
			} 
			set 
			{ 
				m_indoorguildshield = value; 
			}
		}
		
		public bool OutdoorGuildBanner
		{
			get 
			{ 
				return m_outdoorguildbanner;  	
			} 
			set 
			{ 
				m_outdoorguildbanner = value; 
			}
		}
		
		public bool OutdoorGuildShield
		{
			get 
			{ 
				return m_outdoorguildshield;  	
			} 
			set 
			{ 
				m_outdoorguildshield = value; 
			}
		}
		
		public bool Porch
		{
			get 
			{ 
				return m_porch;  	
			} 
			set 
			{ 
				m_porch = value; 
			}
		}

		public string OwnerIDs
		{
			get
			{
				return m_ownerids;
			}
			set
			{
				m_ownerids = value;
			}
		}
	}
}
