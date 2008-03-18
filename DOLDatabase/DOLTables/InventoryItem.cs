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
	public class InventoryItem : DataObject
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private string m_ownerID;
		private string m_templateID;
		private byte m_count;
		private int m_slot_pos;
		private string m_description;
		private int m_durability;
		private int m_condition;
		private byte m_color;
		private int m_emblem;
		private byte m_effect;
		protected long item_exp;
		private int m_cooldown;
		protected long m_sellPrice;
		private string m_poisonTemplateID;
		private byte m_poisonCharges;
		private byte m_quality;
		private byte m_bonus;
		private byte m_extension;
		private byte m_charges;
		private byte m_charges1;
		private byte m_maxCharges;
		private string m_name;
		private ushort m_model;

		//internal
		static bool m_autoSave = false;
		private DateTime m_lastUsedDateTime;
		private ItemTemplate m_template = null;

		public InventoryItem()
			: base()
		{
		}
		/// <summary>
		/// Creates a new Inventoryitem based on the given ItemTemplate
		/// </summary>
		/// <param name="itemTemplate"></param>
		public InventoryItem(ItemTemplate itemTemplate)
			: base()
		{
			m_templateID = itemTemplate.TemplateID;
			m_template = itemTemplate;
			m_name = itemTemplate.Name;
			m_model = itemTemplate.Model;
			m_maxCharges = itemTemplate.MaxCharges;
			m_quality = itemTemplate.Quality;
			m_condition = itemTemplate.MaxCondition;
			m_durability = itemTemplate.MaxDurability;
			m_description = itemTemplate.Description;
		}

		public InventoryItem(ItemTemplate itemTemplate, byte amount)
			: this(itemTemplate)
		{
			if (IsStackable)
				m_count = amount;
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
		public string TemplateID
		{
			get
			{
				return m_templateID;
			}
			set
			{
				Dirty = true;
				m_templateID = value;
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
		public byte Count
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
		public long SellPrice
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
		public string Description
		{
			get
			{
				return m_description;
			}
			set
			{
				Dirty = true;
				m_description = value;
			}
		}

		/// <summary>
		/// Item Name
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string Name
		{
			get { return m_name; }
			set
			{
				Dirty = true;
				m_name = value;
			}
		}

		/// <summary>
		/// Item Model
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public ushort Model
		{
			get { return m_model; }
			set
			{
				Dirty = true;
				m_model = value;
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
					TimeSpan reuse = new TimeSpan(0, 0, Template.CanUseEvery);
					return (reuse.CompareTo(elapsed) < 0)
						? 0
						: Template.CanUseEvery - elapsed.Seconds - 60 * elapsed.Minutes - 3600 * elapsed.Hours;
				}
				catch (ArgumentOutOfRangeException)
				{
					return 0;
				}
			}
			set
			{
				m_lastUsedDateTime = DateTime.Now.AddSeconds(value - Template.CanUseEvery);
				Dirty = true;
			}
		}

		[DataElement(AllowDbNull = true)]
		public virtual long Experience
		{
			get { return item_exp; }
			set
			{
				Dirty = true;
				item_exp = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public string PoisonTemplateID
		{
			get { return m_poisonTemplateID; }
			set
			{
				Dirty = true;
				m_poisonTemplateID = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public byte PoisonCharges
		{
			get { return m_poisonCharges; }
			set
			{
				Dirty = true;
				m_poisonCharges = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public byte Quality
		{
			get { return m_quality; }
			set
			{
				Dirty = true;
				m_quality = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public byte Bonus
		{
			get { return m_bonus; }
			set
			{
				Dirty = true;
				m_bonus = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int Condition
		{
			get { return m_condition; }
			set
			{
				Dirty = true;
				m_condition = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int Durability
		{
			get { return m_durability; }
			set
			{
				Dirty = true;
				m_durability = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int Emblem
		{
			get { return m_emblem; }
			set
			{
				Dirty = true;
				m_emblem = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public byte Color
		{
			get { return m_color; }
			set
			{
				Dirty = true;
				m_color = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public byte Effect
		{
			get { return m_effect; }
			set
			{
				Dirty = true;
				m_effect = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public byte Extension
		{
			get
			{
				if (Template != null)
				{
					if (m_extension > Template.Extension)
						return m_extension;
					else return Template.Extension;
				}
				else return m_extension;
			}
			set
			{
				Dirty = true;
				m_extension = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public byte Charges
		{
			get { return m_charges; }
			set
			{
				Dirty = true;
				m_charges = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public byte Charges1
		{
			get { return m_charges1; }
			set
			{
				Dirty = true;
				m_charges1 = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public byte MaxCharges
		{
			get { return m_maxCharges; }
			set
			{
				Dirty = true;
				m_maxCharges = value;
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

				return (base.Dirty || Template.CanUseEvery > 0);
			}
			set
			{
				base.Dirty = value;
			}
		}

		#region Properties we refer to itemtemplate for
		public byte Item_Type
		{
			get { return Template.Item_Type; }
		}

		public byte Hand
		{
			get { return Template.Hand; }
			set { Template.Hand = value; }
		}

		public byte SPD_ABS
		{
			get { return Template.SPD_ABS; }
			set { Template.SPD_ABS = value; }
		}

		public byte DPS_AF
		{
			get { return Template.DPS_AF; }
			set { Template.DPS_AF = value; }
		}

		public ushort ProcSpellID
		{
			get { return Template.ProcSpellID; }
			set { Template.ProcSpellID = value; }
		}

		public ushort ProcSpellID1
		{
			get { return Template.ProcSpellID1; }
			set { Template.ProcSpellID1 = value; }
		}

		public byte Object_Type
		{
			get { return Template.Object_Type; }
			set { Template.Object_Type = value; }
		}

		public byte Type_Damage
		{
			get { return Template.Type_Damage; }
		}

		public byte PackSize
		{
			get { return Template.PackSize; }
		}

		public byte Level
		{
			get { return Template.Level; }
		}

		public int MaxCondition
		{
			get { return Template.MaxCondition; }
		}

		public int MaxDurability
		{
			get { return Template.MaxDurability; }
		}

		public byte Weight
		{
			get { return (byte)(Template.Weight * m_count); }
		}

		public byte DurabilityPercent
		{
			get
			{
				return (byte)((MaxDurability > 0) ? m_durability * 100 / MaxDurability : 0);
			}
		}

		public byte ConditionPercent
		{
			get
			{
				return (byte)Math.Round((MaxCondition > 0) ? (double)m_condition / MaxCondition * 100 : 0);
			}
		}

		public byte Realm
		{
			get { return Template.Realm; }
		}

		public byte MaxCount
		{
			get { return Template.MaxCount; }
		}

		public bool IsDropable
		{
			get { return Template.IsDropable; }
		}

		public bool IsPickable
		{
			get { return Template.IsPickable; }
		}

		public bool IsTradable
		{
			get { return Template.IsTradable; }
		}

		public ushort SpellID
		{
			get { return Template.SpellID; }
			set { Template.SpellID = value; }
		}

		public ushort SpellID1
		{
			get { return Template.SpellID1; }
			set { Template.SpellID1 = value; }
		}

		public byte MaxCharges1
		{
			get { return Template.MaxCharges1; }
		}

		public virtual bool IsMagical
		{
			get { return MagicalBonuses.Length > 0; }
		}

		public string AllowedClasses
		{
			get { return Template.AllowedClasses; }
		}

		public bool IsStackable
		{
			get
			{
				return MaxCount > 1;
			}
		}

		public long Value
		{
			get
			{
				return (((0 * 1000L + Template.Platinum) * 1000L + Template.Gold) * 100L + Template.Silver) * 100L + Template.Copper;
			}
		}

		private const string m_vowels = "aeuio";

		/// <summary>
		/// Returns name with article for nouns
		/// </summary>
		/// <param name="article">0=definite, 1=indefinite</param>
		/// <param name="firstLetterUppercase"></param>
		/// <returns>name of this object (includes article if needed)</returns>
		public virtual string GetName(int article, bool firstLetterUppercase)
		{
			/*			if(char.IsUpper(Name[0]))
						{
							// proper name
							if(firstLetterUppercase) return "The "+Name; else return "the "+Name;
						}
						else // common noun*/
			if (article == 0)
			{
				if (firstLetterUppercase)
					return "The " + Name;
				else
					return "the " + Name;
			}
			else
			{
				// if first letter is a vowel
				if (m_vowels.IndexOf(Name[0]) != -1)
				{
					if (firstLetterUppercase)
						return "An " + Name;
					else
						return "an " + Name;
				}
				else
				{
					if (firstLetterUppercase)
						return "A " + Name;
					else
						return "a " + Name;
				}
			}
		}

		#endregion
		/// <summary>
		/// Template this inventoryitem is based on
		/// </summary>
		[Relation(LocalField = "TemplateID", RemoteField = "TemplateID", AutoLoad = true, AutoDelete = false)]
		public ItemTemplate Template
		{
			get { return m_template; }
			set { m_template = value; }
		}

		/// <summary>
		/// Template poison is based on
		/// </summary>
		[Relation(LocalField = "PoisonTemplateID", RemoteField = "TemplateID", AutoLoad = true, AutoDelete = false)]
		public ItemTemplate PoisonTemplate;

		/// <summary>
		/// Item specific bonuses from ROG or SpellCrafting
		/// </summary>
		[Relation(LocalField = "InventoryItem_ID", RemoteField = "ItemID", AutoLoad = true, AutoDelete = true)]
		protected ItemBonus[] ItemSpecificBonuses = new ItemBonus[0];

		/// <summary>
		/// Magical bonuses for item, if no specific item bonuses are set, we use the template bonuses
		/// </summary>
		public virtual ItemBonus[] MagicalBonuses
		{
			get
			{
				if (ItemSpecificBonuses.Length > 0)
					return ItemSpecificBonuses;
				return Template.MagicalBonuses;
			}
		}

		public ItemBonus AddBonus(byte type, int amount)
		{
			ItemBonus bonus = new ItemBonus(ObjectId, type, amount);
			MagicalBonuses.SetValue(bonus, MagicalBonuses.Length - 1);
			return bonus;
		}
	}
}