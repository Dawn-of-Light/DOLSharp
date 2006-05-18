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
	/// <summary>
	/// Table of BindPoint where player pop when they die and released
	/// </summary>
	public class DbBindPoint
	{
		//This needs to be uint and ushort!
		private int		m_id;
		private int		m_xpos;
		private int		m_ypos;
		private int		m_zpos;
		private int		m_region;
		private int		m_radius;
		private int		m_realm;

		/// <summary>
		/// The unique id of this bind point
		/// </summary>
		public int BindPointID
		{
			get
			{
				return m_id;
			}
			set
			{
				m_id=value;
			}
		}

		/// <summary>
		/// The X position of bind
		/// </summary>
		public int X
		{
			get
			{
				return m_xpos;
			}
			set
			{
				m_xpos=value;
			}
		}

		/// <summary>
		/// The Y position of bind
		/// </summary>
		public int Y
		{
			get
			{
				return m_ypos;
			}
			set
			{
				m_ypos=value;
			}
		}

		/// <summary>
		/// The Z position of bind
		/// </summary>
		public int Z
		{
			get
			{
				return m_zpos;
			}
			set
			{
				m_zpos=value;
			}
		}

		/// <summary>
		/// The radius of bind
		/// </summary>
		public int Radius
		{
			get
			{
				return m_radius;
			}
			set
			{
				m_radius=value;
			}
		}

		/// <summary>
		/// The region of bind
		/// </summary>
		public int Region
		{
			get
			{
				return m_region;
			}
			set
			{
				m_region=value;
			}
		}

		/// <summary>
		/// The realm of this bind
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
	}
}
