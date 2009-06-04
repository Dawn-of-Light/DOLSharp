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
using System.Collections.Generic;
using log4net;
using System.Reflection;
using System.Timers;

namespace DOL.Database
{
	/// <summary>
	/// The InventoryItem table holds all values from the
	/// ItemTemplate table and also some more values that
	/// are neccessary to store the inventory position
	/// </summary>
	[DataTable(TableName = "InventoryItem")]
	public class InventoryItem : ItemTemplate
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected string m_ownerID;
		protected int m_slot_pos;
		protected string craftername;
		protected int m_canUseAgainIn;
        protected long item_exp;
		private int m_cooldown;
		private DateTime m_lastUsedDateTime;

		/// <summary>
		/// The count of items (for stack!)
		/// </summary>
		protected int m_count;
		static bool m_autoSave;
        protected string m_internalID;
		protected int m_sellPrice;
        protected ushort m_ownerLot;

		public InventoryItem()
			: base()
		{
			m_autoSave = false;
			m_id_nb = "default";
			m_count = 1;
            m_internalID = this.ObjectId;
			m_sellPrice = 0;
		}

		/// <summary>
		/// Creates a new Inventoryitem based on the given ItemTemplate
		/// </summary>
		/// <param name="itemTemplate"></param>
		public InventoryItem(ItemTemplate itemTemplate)
			: this()
		{
			CopyFrom(itemTemplate);
            m_internalID = this.ObjectId;
        }

		/// <summary>
		/// Creates a new Inventoryitem based on the given ItemTemplate
		/// </summary>
		/// <param name="inventoryItem"></param>
		public InventoryItem(InventoryItem inventoryItem)
			: this()
		{
			CopyFrom(inventoryItem);
            m_internalID = this.ObjectId;
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

		[DataElement(AllowDbNull = false)]
		public override string Id_nb
		{
			get
			{
				return m_id_nb;
			}
			set
			{
				Dirty = true;
				m_id_nb = value;
			}
		}

		[DataElement(AllowDbNull = false, Index = true)]
		public string OwnerID
		{
			get
			{
				return m_ownerID;
			}
			set
			{
				Dirty = true;
				m_ownerID = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int Count
		{
			get
			{
				return m_count;
			}
			set
			{
				Dirty = true;
				m_count = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int SlotPosition
		{
			get
			{
				return m_slot_pos;
			}
			set
			{
				Dirty = true;
				m_slot_pos = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int SellPrice
		{
			get
			{
				return m_sellPrice;
			}
			set
			{
				Dirty = true;
				m_sellPrice = value;
			}
		}

        [DataElement(AllowDbNull = true)]
        public ushort OwnerLot
        {
            get
            {
                return m_ownerLot;
            }
            set
            {
                Dirty = true;
                m_ownerLot = value;
            }
        }

		[DataElement(AllowDbNull = true)]
		public string CrafterName
		{
			get
			{
				return craftername;
			}
			set
			{
				Dirty = true;
				craftername = value;
			}
		}

		/// <summary>
		/// Internal use only!
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public int Cooldown
		{
			get { return CanUseAgainIn; }
			set { m_cooldown = value; }
		}

		/// <summary>
		/// Internal use only!
		/// </summary>
		public void SetCooldown()
		{
			CanUseAgainIn = m_cooldown;
		}

		/// <summary>
		/// When this item can be used again (in seconds).
		/// </summary>
		public int CanUseAgainIn
		{
			get 
			{
				try
				{
					TimeSpan elapsed = DateTime.Now.Subtract(m_lastUsedDateTime);
					TimeSpan reuse = new TimeSpan(0, 0, CanUseEvery);
					return (reuse.CompareTo(elapsed) < 0) 
						? 0 
						: CanUseEvery - elapsed.Seconds - 60 * elapsed.Minutes - 3600 * elapsed.Hours;
				}
				catch (ArgumentOutOfRangeException)
				{
					return 0;
				}
			}
			set
			{
				m_lastUsedDateTime = DateTime.Now.AddSeconds(value - CanUseEvery);
				Dirty = true;
			}
		}

        [DataElement(AllowDbNull = false)]
        public virtual long Experience
        {
            get { return item_exp; }
            set
            {
                Dirty = true;
                item_exp = value;
            }
        }

		/// <summary>
		/// Whether to save this object or not.
		/// </summary>
		public override bool Dirty
		{
			get
			{
				// Items with reuse timers will ALWAYS be saved.

				return (base.Dirty || CanUseEvery > 0);
			}
			set
			{
				base.Dirty = value;
			}
		}

        /// <summary>
        /// Repair cost for this item in the current state.
        /// </summary>
        public virtual long RepairCost
        {
            get
            {
                return ((MaxCondition - Condition) * Value) / MaxCondition;
            }
        }

        public void CopyFrom(InventoryItem template)
		{
			CopyFrom((ItemTemplate)template);
			OwnerID = template.OwnerID;
			Count = template.Count;
			SlotPosition = template.SlotPosition;
			CrafterName = template.CrafterName;
			Experience = template.Experience;
			//CopyFrom((ItemTemplate)template);
		}

		public void CopyFrom(ItemTemplate template)
		{
			OwnerID = null;
			SlotPosition = 0;
			Bonus = template.Bonus;
			Bonus1 = template.Bonus1;
			Bonus2 = template.Bonus2;
			Bonus3 = template.Bonus3;
			Bonus4 = template.Bonus4;
			Bonus5 = template.Bonus5;
			Bonus6 = template.Bonus6;
			Bonus7 = template.Bonus7;
			Bonus8 = template.Bonus8;
			Bonus9 = template.Bonus9;
			Bonus10 = template.Bonus10;
			Color = template.Color;
			Condition = template.Condition;
			DPS_AF = template.DPS_AF;
			Durability = template.Durability;
			Effect = template.Effect;
			Emblem = template.Emblem;
			ExtraBonus = template.ExtraBonus;
			Hand = template.Hand;
			Id_nb = template.Id_nb;
			IsDropable = template.IsDropable;
			IsPickable = template.IsPickable;
			IsTradable = template.IsTradable;
			CanDropAsLoot = template.CanDropAsLoot;
			MaxCount = template.MaxCount;
			PackSize = template.PackSize;
			Item_Type = template.Item_Type;
			Level = template.Level;
			MaxCondition = template.MaxCondition;
			MaxDurability = template.MaxDurability;
			Model = template.Model;
			Extension = template.Extension;
			Name = template.Name;
			Object_Type = template.Object_Type;
			Quality = template.Quality;
			SPD_ABS = template.SPD_ABS;
			Type_Damage = template.Type_Damage;
			Weight = template.Weight;
			Platinum = template.Platinum;
			Gold = template.Gold;
			Silver = template.Silver;
			Copper = template.Copper;
			Bonus1Type = template.Bonus1Type;
			Bonus2Type = template.Bonus2Type;
			Bonus3Type = template.Bonus3Type;
			Bonus4Type = template.Bonus4Type;
			Bonus5Type = template.Bonus5Type;
			Bonus6Type = template.Bonus6Type;
			Bonus7Type = template.Bonus7Type;
			Bonus8Type = template.Bonus8Type;
			Bonus9Type = template.Bonus9Type;
			Bonus10Type = template.Bonus10Type;
			ExtraBonusType = template.ExtraBonusType;
			Charges = template.Charges;
			MaxCharges = template.MaxCharges;
			Charges1 = template.Charges1;
			MaxCharges1 = template.MaxCharges1;
			SpellID = template.SpellID;
			SpellID1 = template.SpellID1;
			ProcSpellID = template.ProcSpellID;
			ProcSpellID1 = template.ProcSpellID1;
			PoisonSpellID = template.PoisonSpellID;
			PoisonCharges = template.PoisonCharges;
			PoisonMaxCharges = template.PoisonMaxCharges;
			Realm = template.Realm;
			AllowedClasses = template.AllowedClasses;
			CanUseEvery = template.CanUseEvery;
		}
	}
}