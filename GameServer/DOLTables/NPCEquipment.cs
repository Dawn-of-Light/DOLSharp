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
using DOL.Database2;


namespace DOL.Database2
{
	/// <summary>
	/// The NPCEqupment table holds standard equipment
	/// templates that npcs may wear!
	/// </summary>
	[Serializable]//TableName="NPCEquipment")]
	public class NPCEquipment : DatabaseObject
	{
		/// <summary>
		/// The Equipment Template ID
		/// </summary>
		protected UInt64	m_templateID;
		/// <summary>
		/// The Item Slot
		/// </summary>
		protected int		m_slot;
		/// <summary>
		/// The Item Model
		/// </summary>
		protected int		m_model;
		/// <summary>
		/// The Item Color
		/// </summary>
		protected int		m_color;
		/// <summary>
		/// The Item Effect
		/// </summary>
		protected int		m_effect;
		/// <summary>
		/// The Item Extension
		/// </summary>
		protected int		m_extension; 

		static bool			m_autoSave;

		/// <summary>
		/// The Constructor
		/// </summary>
        public NPCEquipment()
            : base()
		{
			m_autoSave=false;
		}


		/// <summary>
		/// Template ID
		/// </summary>
		//[DataElement(AllowDbNull=false)]
		public UInt64 TemplateID
		{
			get
			{
				return m_templateID;
			}
			set
			{
				m_Dirty = true;
				m_templateID=value;
			}
		}

		/// <summary>
		/// Slot
		/// </summary>
		//[DataElement(AllowDbNull=false)]
		public int Slot
		{
			get
			{
				return m_slot;
			}
			set
			{
				m_Dirty = true;
				m_slot = value;
			}
		}

		/// <summary>
		/// Model
		/// </summary>
		//[DataElement(AllowDbNull=false)]
		public int Model
		{
			get
			{
				return m_model;
			}
			set
			{
				m_Dirty = true;
				m_model = value;
			}
		}

		/// <summary>
		/// Color
		/// </summary>
		//[DataElement(AllowDbNull=true)]
		public int Color
		{
			get
			{
				return m_color;
			}
			set
			{
				m_Dirty = true;
				m_color = value;
			}
		}

		/// <summary>
		/// Effect
		/// </summary>
		//[DataElement(AllowDbNull=true)]
		public int Effect
		{
			get
			{
				return m_effect;
			}
			set
			{
				m_Dirty = true;
				m_effect = value;
			}
		}

		/// <summary>
		/// Extension
		/// </summary>
		//[DataElement(AllowDbNull=true)]
		public int Extension
		{
			get
			{
				return m_extension;
			}
			set
			{
				m_Dirty = true;
				m_extension = value;
			}
		} 
	}
}