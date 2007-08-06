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
using DOL.Database.Attributes;

namespace DOL.Database
{
	/// <summary>
	/// Table of BindPoint where player pop when they die and released
	/// </summary>
	[DataTable(TableName="bind_point")]
	public class BindPoint : DataObject
	{
		//This needs to be uint and ushort!
		private int	m_xpos;
		private int	m_ypos;
		private int	m_zpos;
		private int		m_region;
		private ushort	m_radius;
		private int		m_realm;

		/// <summary>
		/// Create a bind point
		/// </summary>
		public BindPoint()
		{
		}

		/// <summary>
		/// The X position of bind
		/// </summary>
		[DataElement(AllowDbNull=false)]
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
		[DataElement(AllowDbNull=false)]
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
		[DataElement(AllowDbNull=false)]
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
		[DataElement(AllowDbNull=false)]
		public ushort Radius
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
		[DataElement(AllowDbNull=false)]
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
		[DataElement(AllowDbNull=true)]
		public int Realm
		{
			get { return m_realm; }
			set { m_realm = value; }
		}

		/// <summary>
		/// autosave this bind or not
		/// </summary>
		override public bool AutoSave
		{
			get
			{
				return false;
			}
			set
			{
			}
		}
	}
}
