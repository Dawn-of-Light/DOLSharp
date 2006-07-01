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
		protected string	m_templateID;
		protected int		m_slot;
		protected int		m_model;
		protected int		m_color;
		protected int		m_effect;
		protected int		m_extension; 

		static bool			m_autoSave;

		public NPCEquipment()
		{
			m_autoSave=false;
		}

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

		[DataElement(AllowDbNull=false)]
		public int Slot
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

		[DataElement(AllowDbNull=true)]
		public int Color
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

		[DataElement(AllowDbNull=true)]
		public int Effect
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

		[DataElement(AllowDbNull = true)]
		public int Extension
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