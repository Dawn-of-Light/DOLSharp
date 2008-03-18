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
using System.Collections;
using System.Collections.Generic;
using log4net;
using System.Reflection;

namespace DOL.Database
{
	[DataTable(TableName = "ItemTemplate")]
	public class ItemTemplate : DataObject
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected string m_templateID;
		protected string m_name;
		protected byte m_level;
		protected int m_durability;
		protected int m_maxdurability;
		protected int m_condition;
		protected int m_maxcondition;
		protected byte m_quality;
		protected byte m_dps_af;
		protected byte m_spd_abs;
		protected byte m_hand;
		protected byte m_type_damage;
		protected byte m_object_type;
		protected byte m_item_type;
		protected byte m_color;
		protected byte m_effect;
		protected byte m_weight;
		protected ushort m_model;
		protected byte m_extension;
		protected byte m_bonus;
		protected short m_platinum;
		protected short m_gold;
		protected byte m_silver;
		protected byte m_copper;
		protected bool m_isDropable;
		protected bool m_isPickable;
		protected bool m_isTradable;
		protected bool m_canDropAsLoot;
		protected byte m_maxCount;
		protected byte m_packSize;
		protected ushort m_spellID;
		protected ushort m_procSpellID;
		protected byte m_maxCharges;
		protected byte m_charges;
		protected ushort m_spellID1;
		protected ushort m_procSpellID1;
		protected byte m_charges1;
		protected byte m_maxCharges1;
		protected byte m_realm;
		private string m_allowedClasses = "";
		private string m_description = "";
		private int m_canUseEvery;

		//internal
		static bool m_autoSave;

		public ItemTemplate()
		{
			m_templateID = "XXX_useless_junk_XXX";
			m_name = "Some usless junk";
			m_level = 0;
			m_durability = 1;
			m_maxdurability = 1;
			m_condition = 1;
			m_maxcondition = 1;
			m_quality = 1;
			m_dps_af = 0;
			m_spd_abs = 0;
			m_hand = 0;
			m_type_damage = 0;
			m_object_type = 0;
			m_item_type = 0;
			m_color = 0;
			m_effect = 0;
			m_weight = 0;
			m_model = 488; //bag
			m_extension = 0;
			m_bonus = 0;
			m_platinum = 0;
			m_gold = 0;
			m_silver = 0;
			m_copper = 0;
			m_isDropable = true;
			m_isPickable = true;
			m_isTradable = true;
			m_canDropAsLoot = true;
			m_maxCount = 1;
			m_packSize = 1;
			m_charges = 0;
			m_maxCharges = 0;
			m_spellID = 0;//when no spell link to item
			m_spellID1 = 0;
			m_procSpellID = 0;
			m_procSpellID1 = 0;
			m_charges1 = 0;
			m_maxCharges1 = 0;
			m_realm = 0;
			m_autoSave = false;
		}

		[PrimaryKey]
		public virtual string TemplateID
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
		[DataElement(AllowDbNull = false)]
		public byte Level
		{
			get
			{
				return m_level;
			}
			set
			{
				Dirty = true;
				m_level = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public int Durability
		{
			get
			{
				return m_durability;
			}
			set
			{
				Dirty = true;
				m_durability = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int MaxDurability
		{
			get
			{
				return m_maxdurability;
			}
			set
			{
				Dirty = true;
				m_maxdurability = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public int Condition
		{
			get
			{
				return m_condition;
			}
			set
			{
				Dirty = true;
				m_condition = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public int MaxCondition
		{
			get
			{
				return m_maxcondition;
			}
			set
			{
				Dirty = true;
				m_maxcondition = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public byte Quality
		{
			get
			{
				return m_quality;
			}
			set
			{
				Dirty = true;
				m_quality = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public byte DPS_AF
		{
			get
			{
				return m_dps_af;
			}
			set
			{
				Dirty = true;
				m_dps_af = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public byte SPD_ABS
		{
			get
			{
				return m_spd_abs;
			}
			set
			{
				Dirty = true;
				m_spd_abs = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public byte Hand
		{
			get
			{
				return m_hand;
			}
			set
			{
				Dirty = true;
				m_hand = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public byte Type_Damage
		{
			get
			{
				return m_type_damage;
			}
			set
			{
				Dirty = true;
				m_type_damage = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public byte Object_Type
		{
			get
			{
				return m_object_type;
			}
			set
			{
				Dirty = true;
				m_object_type = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public byte Item_Type
		{
			get
			{
				return m_item_type;
			}
			set
			{
				Dirty = true;
				m_item_type = value;
			}
		}
		[DataElement(AllowDbNull = true)]
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
		[DataElement(AllowDbNull = true)]
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
		[DataElement(AllowDbNull = true)]
		public byte Weight
		{
			get
			{
				return m_weight;
			}
			set
			{
				Dirty = true;
				m_weight = value;
			}
		}
		[DataElement(AllowDbNull = false)]
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
		[DataElement(AllowDbNull = true)]
		public byte Bonus
		{
			get
			{
				return m_bonus;
			}
			set
			{
				Dirty = true;
				m_bonus = value;
			}
		}
		
		[DataElement(AllowDbNull = true)]
		public bool IsPickable
		{
			get
			{
				return m_isPickable;
			}
			set
			{
				Dirty = true;
				m_isPickable = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public bool IsDropable
		{
			get
			{
				return m_isDropable;
			}
			set
			{
				Dirty = true;
				m_isDropable = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public bool CanDropAsLoot
		{
			get
			{
				return m_canDropAsLoot;
			}
			set
			{
				Dirty = true;
				m_canDropAsLoot = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public bool IsTradable
		{
			get
			{
				return m_isTradable;
			}
			set
			{
				Dirty = true;
				m_isTradable = value;
			}
		}

		public long Value
		{
			get
			{
				return (((0 * 1000L + Platinum) * 1000L + Gold) * 100L + Silver) * 100L + Copper;
			}
		}

		[DataElement(AllowDbNull = true)]
		public short Platinum
		{
			get
			{
				return m_platinum;
			}
			set
			{
				Dirty = true;
				m_platinum = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public short Gold
		{
			get
			{
				return m_gold;
			}
			set
			{
				Dirty = true;
				m_gold = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public byte Silver
		{
			get
			{
				return m_silver;
			}
			set
			{
				Dirty = true;
				m_silver = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public byte Copper
		{
			get
			{
				return m_copper;
			}
			set
			{
				Dirty = true;
				m_copper = value;
			}
		}

		/// <summary>
		/// Max amount allowed in one stack
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public byte MaxCount
		{
			get { return m_maxCount; }
			set
			{
				Dirty = true;
				m_maxCount = value;
			}
		}

		/// <summary>
		/// Amount of items sold at once
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public byte PackSize
		{
			get { return m_packSize; }
			set
			{
				Dirty = true;
				m_packSize = value;
			}
		}

		/// <summary>
		/// Charge of item when he have some charge of a spell
		/// </summary>
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

		/// <summary>
		/// Max charge of item when he have some charge of a spell
		/// </summary>
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

		/// <summary>
		/// Max charge of item when he have some charge of a spell
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public byte MaxCharges1
		{
			get { return m_maxCharges1; }
			set
			{
				Dirty = true;
				m_maxCharges1 = value;
			}
		}

		/// <summary>
		/// Spell id for items with charge
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public virtual ushort SpellID
		{
			get { return m_spellID; }
			set
			{
				Dirty = true;
				m_spellID = value;
			}
		}

		/// <summary>
		/// Spell id for items with charge
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public virtual ushort SpellID1
		{
			get { return m_spellID1; }
			set
			{
				Dirty = true;
				m_spellID1 = value;
			}
		}

		/// <summary>
		/// ProcSpell id for items
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public virtual ushort ProcSpellID
		{
			get { return m_procSpellID; }
			set
			{
				Dirty = true;
				m_procSpellID = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public virtual ushort ProcSpellID1
		{
			get { return m_procSpellID1; }
			set
			{
				Dirty = true;
				m_procSpellID1 = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public byte Realm
		{
			get { return m_realm; }
			set
			{
				m_realm = value;
				Dirty = true;
			}
		}

		/// <summary>
		/// the serialized allowed classes of item
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public string AllowedClasses
		{
			get { return m_allowedClasses; }
			set
			{
				m_allowedClasses = value;
				Dirty = true;
			}
		}

		[DataElement(AllowDbNull = false)]
		public int CanUseEvery
		{
			get { return m_canUseEvery; }
			set
			{
				m_canUseEvery = value;
				Dirty = true;
			}
		}

		[DataElement(AllowDbNull = true)]
		public string Description
		{
			get { return m_description; }
			set
			{
				m_description = value;
				Dirty = true;
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

		/// <summary>
		/// Item specific bonuses from ROG or SpellCrafting
		/// </summary>
		[Relation(LocalField = "TemplateID", RemoteField = "ItemID", AutoLoad = true, AutoDelete = true)]
		public ItemBonus[] MagicalBonuses = new ItemBonus[0];

		public ItemBonus AddBonus(byte type, int amount)
		{
			List<ItemBonus> list = new List<ItemBonus>(MagicalBonuses);
			ItemBonus bonus = new ItemBonus(TemplateID, type, amount);
			list.Add(bonus);
			MagicalBonuses = list.ToArray();
			return bonus;
		}
	}
}
