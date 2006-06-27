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
	public class MerchantItem
	{
		private int			m_id;
		private string		m_item_list_ID;
		private string		m_id_nb;
		private int			m_page_number;
		private int			m_slot_pos;


		public int MerchantItemID
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

		public string ItemListID
		{
			get
			{
				return m_item_list_ID;
			}
			set
			{
				m_item_list_ID = value;
			}
		}

		public string ItemTemplateID
		{
			get
			{
				return m_id_nb;
			}
			set
			{
				m_id_nb = value;
			}
		}

		public int PageNumber
		{
			get
			{
				return m_page_number;
			}
			set
			{
				m_page_number = value;
			}
		}

		public int SlotPosition
		{
			get
			{
				return m_slot_pos;
			}
			set
			{
				m_slot_pos = value;
			}
		}
	}
}
