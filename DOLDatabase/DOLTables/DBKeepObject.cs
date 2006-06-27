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
	/// DBKeepObject is database of all object of keep like lord, guard, bannier
	/// </summary>
	[DataTable(TableName="KeepObject",PreCache=true) ]
	public class DBKeepObject : DataObject
	{
		static bool	m_autoSave;
		private int m_keepID;
		private string m_name;
		private int m_model;
		private string m_equipmentID;//only for guard
		private int m_baselevel;
		private int m_x;
		private int m_y;
		private int m_z;
		private int m_realm;
		private int m_heading;
		private string m_class;
		private int m_keepType;

		/// <summary>
		/// Create keep object
		/// </summary>
		public DBKeepObject()
		{
			m_autoSave=false;
		}

		/// <summary>
		/// Autosave table
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
		/// Name of keep object
		/// </summary>
		[DataElement(AllowDbNull=true)]
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
		/// equipment keep object when guard
		/// </summary>
		[DataElement(AllowDbNull=true)]
		public string EquipmentID
		{
			get
			{
				return m_equipmentID;
			}
			set
			{   
				Dirty = true;
				m_equipmentID = value;
			}
		}
		
		/// <summary>
		/// Keep owner of object
		/// </summary>
		[DataElement(AllowDbNull=true, Index=true)]
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
		/// Level of keep
		/// </summary>
		[DataElement(AllowDbNull=true)]
		public int BaseLevel
		{
			get
			{
				return m_baselevel;
			}
			set
			{   
				Dirty = true;
				m_baselevel = value;
			}
		}
			
		/// <summary>
		/// X position keep object
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
		/// Y position keep object
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
		/// Z position keep object
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
		/// Heading of keep object
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
		/// Realm of keep object
		/// with that we can have a totaly different list of object when own by midgard or albion, for example
		/// </summary>
		[DataElement(AllowDbNull=false, Index=true)]
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
		/// Model of keep object
		/// </summary>
		[DataElement(AllowDbNull=false)]
		public int Model
		{
			get
			{
				return m_model;
			}
			set
			{
				Dirty = true;
				m_model = value;
			}
		}

		/// <summary>
		/// class type to invoke
		/// </summary>
		[DataElement(AllowDbNull=false)]
		public string ClassType
		{
			get
			{
				return m_class;
			}
			set
			{
				Dirty = true;
				m_class = value;
			}
		}

		/// <summary>
		/// the keep type because with type we have different list of guard
		/// </summary>
		[DataElement(AllowDbNull=false)]
		public int KeepType
		{
			get
			{
				return m_keepType;
			}
			set
			{
				Dirty = true;
				m_keepType = value;
			}
		}
	}
}
