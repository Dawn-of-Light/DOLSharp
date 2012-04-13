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

namespace DOL
{
	namespace Database
	{
		[DataTable(TableName = "InventoryBackup")]
		public class InventoryBackup : InventoryItem
		{
			string m_deletedOwnerName = "";
			private DateTime m_deleteDate;

			public InventoryBackup()
				: base()
			{
				m_deleteDate = DateTime.Now;
			}

			public InventoryBackup(DOLCharactersBackup deleted, InventoryItem inventory)
				: base()
			{
				DeletedOwnerName = deleted.Name;
				DeleteDate = DateTime.Now;
				AllowAdd = true;

				m_ownerID = deleted.ObjectId;
				m_itemplate_id = inventory.ITemplate_Id;
				m_utemplate_id = inventory.UTemplate_Id;
				m_color = inventory.Color;
				m_extension = inventory.Extension;
				m_slot_pos = inventory.SlotPosition;
				m_count = inventory.Count;
				m_creator = inventory.Creator;
				m_iscrafted = inventory.IsCrafted;
				m_sellPrice = inventory.SellPrice;
				m_condition = inventory.Condition;
				m_durability = inventory.Durability;
				m_emblem = inventory.Emblem;
				m_charges = inventory.Charges;
				m_charges1 = inventory.Charges1;
				m_poisonCharges = inventory.PoisonCharges;
				m_poisonMaxCharges = inventory.PoisonMaxCharges;
				m_poisonSpellID = inventory.PoisonSpellID;
				m_experience = inventory.Experience;
				m_ownerLot = inventory.OwnerLot;
			}

            /// <summary>
            /// Name of the character - indexed but not unique for backups
            /// </summary>
            [DataElement(AllowDbNull = false, Index = true)]
            public string DeletedOwnerName
            {
                get
                {
                    return m_deletedOwnerName;
                }
                set
                {
                    Dirty = true;
                    m_deletedOwnerName = value;
                }
            }

			/// <summary>
			/// The deletion date of this character
			/// </summary>
			[DataElement(AllowDbNull = false)]
			public DateTime DeleteDate
			{
				get
				{
					return m_deleteDate;
				}
				set
				{
					Dirty = true;
					m_deleteDate = value;
				}
			}
		}
	}
}
