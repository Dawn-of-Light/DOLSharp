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
using DOL.Database;
using DOL.Database.Attributes;

namespace DOL.Database
{
	/// <summary>
	/// The NPCEqupment table holds standard equipment
	/// templates that npcs may wear!
	/// </summary>
	[DataTable(TableName="NPCEquipment")]
	public class NPCEquipment : DataObject
	{
		/// <summary>
		/// The Equipment Template ID
		/// </summary>
		protected string	m_templateID;
		/// <summary>
		/// The Item Slot
		/// </summary>
		protected byte		m_slot;
		/// <summary>
		/// The Item Model
		/// </summary>
		protected ushort		m_model;
		/// <summary>
		/// The Item Color
		/// </summary>
		protected byte		m_color;
		/// <summary>
		/// The Item Effect
		/// </summary>
		protected byte		m_effect;
		/// <summary>
		/// The Item Extension
		/// </summary>
		protected byte		m_extension; 

		static bool			m_autoSave;

		/// <summary>
		/// The Constructor
		/// </summary>
		public NPCEquipment()
		{
			m_autoSave=false;
		}

		/// <summary>
		/// Autosave in table
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
		/// Template ID
		/// </summary>
		[DataElement(AllowDbNull=false)]
		public string TemplateID
		{
			get
			{
				return m_templateID;
			}
			set
			{
				Dirty = true;
				m_templateID=value;
			}
		}

		/// <summary>
		/// Slot
		/// </summary>
		[DataElement(AllowDbNull=false)]
		public byte Slot
		{
			get
			{
				return m_slot;
			}
			set
			{
				Dirty = true;
				m_slot = value;
			}
		}

		/// <summary>
		/// Model
		/// </summary>
		[DataElement(AllowDbNull=false)]
		public ushort Model
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
		/// Color
		/// </summary>
		[DataElement(AllowDbNull=true)]
		public byte Color
		{
			get
			{
				return m_color;
			}
			set
			{
				Dirty = true;
				m_color = value;
			}
		}

		/// <summary>
		/// Effect
		/// </summary>
		[DataElement(AllowDbNull=true)]
		public byte Effect
		{
			get
			{
				return m_effect;
			}
			set
			{
				Dirty = true;
				m_effect = value;
			}
		}

		/// <summary>
		/// Extension
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public byte Extension
		{
			get
			{
				return m_extension;
			}
			set
			{
				Dirty = true;
				m_extension = value;
			}
		} 
	}
}