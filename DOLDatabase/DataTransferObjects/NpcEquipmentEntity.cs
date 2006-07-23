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

namespace DOL.Database.DataTransferObjects
{
	[Serializable]
	public struct NpcEquipmentEntity
	{
		private int m_id;
		private int m_glowEffect;
		private int m_inventory;
		private int m_model;
		private byte m_modelExtension;
		private string m_nPCEquipmentType;
		private int m_or1;
		private int m_slotPosition;

		public int Id
		{
			get { return m_id; }
			set { m_id = value; }
		}
		public int GlowEffect
		{
			get { return m_glowEffect; }
			set { m_glowEffect = value; }
		}
		public int Inventory
		{
			get { return m_inventory; }
			set { m_inventory = value; }
		}
		public int Model
		{
			get { return m_model; }
			set { m_model = value; }
		}
		public byte ModelExtension
		{
			get { return m_modelExtension; }
			set { m_modelExtension = value; }
		}
		public string NPCEquipmentType
		{
			get { return m_nPCEquipmentType; }
			set { m_nPCEquipmentType = value; }
		}
		public int or1
		{
			get { return m_or1; }
			set { m_or1 = value; }
		}
		public int SlotPosition
		{
			get { return m_slotPosition; }
			set { m_slotPosition = value; }
		}
	}
}
