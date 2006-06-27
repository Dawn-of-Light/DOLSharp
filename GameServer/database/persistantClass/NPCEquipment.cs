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
	/// The NPCEqupment table holds standard equipment
	/// templates that npcs may wear!
	/// </summary>
	public class NPCEquipment
	{
		protected int		m_id;
		protected string	m_templateID;
		protected int		m_slot;
		protected int		m_model;
		protected int		m_color;
		protected int		m_value; // Glow effect for weapon and model extension for armor

		public int NPCEquipmentID
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

		public string TemplateID
		{
			get
			{
				return m_templateID;
			}
			set
			{
				m_templateID=value;
			}
		}

		public int Slot
		{
			get
			{
				return m_slot;
			}
			set
			{
				m_slot = value;
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

		public int Color
		{
			get
			{
				return m_color;
			}
			set
			{
				m_color = value;
			}
		}

		public int Value
		{
			get
			{
				return m_value;
			}
			set
			{
				m_value = value;
			}
		}
	}
}