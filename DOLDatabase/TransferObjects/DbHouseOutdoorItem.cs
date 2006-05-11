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

namespace DOL.Database.TransferObjects
{
	public class DbHouseOutdoorItem
	{
		private int m_id;	
		private int m_housenumber;
		private int m_model;
		private int m_position;
		private int m_rotation;
		private string m_baseitemid;

		public int HouseOutdoorItemID
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
		
		public int Position
		{
			get
			{
				return m_position;
			}
			set
			{
				m_position = value;
			}
		}
		
		public int Rotation
		{
			get
			{
				return m_rotation;
			}
			set
			{
				m_rotation = value;
			}
		}
		
		public string BaseItemID
		{
			get
			{
				return m_baseitemid;
			}
			set
			{
				m_baseitemid = value;
			}
		}
	}
}
