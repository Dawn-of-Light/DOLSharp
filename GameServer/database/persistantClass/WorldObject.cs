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
	/// Objects as Tables, Lights, Bags non static in Game.
	/// </summary>
	public class WorldObject
	{
		private int			m_id;
		private string		m_type;
		private string		m_name;
		private int			m_x;
		private int			m_y;
		private int			m_z;
		private int			m_heading;
		private int			m_model;
		private int			m_region;
		private int			m_emblem;

		
		public WorldObject()
		{
			m_type = "DOL.GS.GameItem";
		}

		public int WorldObjectID
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
	}
}
