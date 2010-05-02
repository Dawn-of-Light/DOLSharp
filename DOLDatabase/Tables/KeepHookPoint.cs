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

using DOL.Database;
using DOL.Database.Attributes;

namespace DOL.Database
{
	/// <summary>
	/// keep hook point in DB
	/// </summary>
	/// 
	[DataTable(TableName="KeepHookPoint")]
	public class DBKeepHookPoint : DataObject
	{
		private int m_x;
		private int m_y;
		private int m_z;
		private int m_heading;
		private int m_keepComponentSkinID;
		private int m_hookPointID;
		private int m_height;

		public DBKeepHookPoint()
		{
		}
		
		/// <summary>
		/// Hook Point
		/// </summary>
		[DataElement(AllowDbNull=false,Unique=false)]
		public int HookPointID
		{
			get
			{
				return m_hookPointID;
			}
			set
			{
				Dirty = true;
				m_hookPointID = value;
			}
		}

		/// <summary>
		/// skin of component with hookpoint is linked
		/// </summary>
		[DataElement(AllowDbNull=false,Unique=false)]
		public int KeepComponentSkinID
		{
			get
			{
				return m_keepComponentSkinID;
			}
			set
			{
				Dirty = true;
				m_keepComponentSkinID = value;
			}
		}
		
		/// <summary>
		/// Z position of door
		/// </summary>
		[DataElement(AllowDbNull = true)]
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
		/// Y position of door
		/// </summary>
		[DataElement(AllowDbNull = true)]
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
		/// X position of door
		/// </summary>
		[DataElement(AllowDbNull = true)]
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
		/// Heading of door
		/// </summary>
		[DataElement(AllowDbNull = true)]
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
		/// Height of door
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int Height
		{
			set
			{
				Dirty = true;
				m_height=value;
			}
			get{return m_height;}
		}
	}
}
