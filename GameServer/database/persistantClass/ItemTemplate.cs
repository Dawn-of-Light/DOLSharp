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
	public class ItemTemplate
	{
		protected string m_id;
		protected string m_name;
		protected int m_level;
		protected int m_durability;
		protected int m_maxdurability;
		protected int m_condition;
		protected int m_maxcondition;
		protected int m_quality;
		protected int m_maxquality;
		protected int m_dps_af;
		protected int m_spd_abs;
		protected int m_hand;
		protected int m_type_damage;
		protected int m_object_type;
		protected int m_item_type;
		protected int m_color;
		protected int m_emblem;
		protected int m_effect;
		protected int m_weight;
		protected int m_model;
		protected byte m_modelExtension;
		protected int m_bonus;
		protected int m_bonus1;
		protected int m_bonus2;
		protected int m_bonus3;
		protected int m_bonus4;
		protected int m_bonus5;
		protected int m_extrabonus;
		protected int m_bonusType;
		protected int m_bonus1Type;
		protected int m_bonus2Type;
		protected int m_bonus3Type;
		protected int m_bonus4Type;
		protected int m_bonus5Type;
		protected int m_extrabonusType;
		protected short m_gold;
		protected byte m_silver;
		protected byte m_copper;
		protected bool m_isDropable;
		protected bool m_isPickable;
		protected int m_maxCount;
		protected int m_packSize;
		protected int m_charges;
		protected int m_maxCharges;
		protected int m_spellID;
		protected int m_procSpellID;
		protected int m_realm;
        
		public ItemTemplate()
		{
			m_id = "XXX_useless_junk_XXX";
            m_name = "Some usless junk";
            m_level = 0;
            m_durability = 1;
            m_maxdurability = 1;
            m_condition = 1;
            m_maxcondition = 1;
            m_quality = 1;
            m_maxquality = 1;
            m_dps_af = 0;
			m_spd_abs = 0;
			m_hand = 0;
			m_type_damage = 0;
			m_object_type = 0;
			m_item_type = 0;
			m_color = 0;
			m_emblem = 0;
			m_effect = 0;
			m_weight = 0;
			m_model = 488; //bag
			m_modelExtension = 0;
			m_bonus = 0;
			m_bonus1 = 0;
			m_bonus2 = 0;
			m_bonus3 = 0;
			m_bonus4 = 0;
			m_bonus5 = 0;
			m_extrabonus = 0;
			m_bonusType = 0;
			m_bonus1Type = 0;
			m_bonus2Type = 0;
			m_bonus3Type = 0;
			m_bonus4Type = 0;
			m_bonus5Type = 0;
			m_extrabonusType = 0;
			m_gold = 0;
			m_silver = 0;
			m_copper = 0;
			m_isDropable = true;
			m_isPickable = true;
			m_maxCount = 1;
			m_packSize = 1;
			m_charges = 0;
			m_maxCharges = 0;
			m_spellID = 0;//when no spell link to item
			m_procSpellID = 0;
		}

		public string ItemTemplateID
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

		public string Name
		{
			get
			{
				return m_name;
			}
			set
			{
				m_name = value;
			}
		}
	
		public int Level
		{
			get
			{
				return m_level;
			}
			set
			{
				m_level = value;
			}
		}
		
		public int Durability
		{
			get
			{
				return m_durability;
			}
			set
			{
				m_durability = value;
			}
		}
		
		public byte DurabilityPercent
		{
			get
			{
				return (byte)((m_maxdurability>0) ? m_durability * 100 / m_maxdurability : 0);
			}
		}

		public int MaxDurability
		{
			get
			{
				return m_maxdurability;
			}
			set
			{
				m_maxdurability = value;
			}
		}
		
		public int Condition
		{
			get
			{
				return m_condition;
			}
			set
			{
				m_condition = value;
			}
		}
		
		public byte ConditionPercent
		{
			get
			{
				return (byte)Math.Round((m_maxcondition>0) ? (double)m_condition / m_maxcondition * 100 : 0);
			}
		}
		
		public int MaxCondition
		{
			get
			{
				return m_maxcondition;
			}
			set
			{
				m_maxcondition = value;
			}
		}
		
		public int Quality
		{
			get
			{
				return m_quality;
			}
			set
			{
				m_quality = value;
			}
		}		
		
		public int MaxQuality
		{
			get
			{
				return m_maxquality;
			}
			set
			{
				m_maxquality = value;
			}
		}
		
		public int DPS_AF
		{
			get
			{
				return m_dps_af;
			}
			set
			{
				m_dps_af = value;
			}
		}
		
		public int SPD_ABS
		{
			get
			{
				return m_spd_abs;
			}
			set
			{
				m_spd_abs = value;
			}
		}
		
		public int Hand
		{
			get
			{
				return m_hand;
			}
			set
			{
				m_hand = value;
			}
		}
		
		public int Type_Damage
		{
			get
			{
				return m_type_damage;
			}
			set
			{
				m_type_damage = value;
			}
		}
		
		public int Object_Type
		{
			get
			{
				return m_object_type;
			}
			set
			{
				m_object_type = value;
			}
		}
		
		public int Item_Type
		{
			get
			{
				return m_item_type;
			}
			set
			{
				m_item_type = value;
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
		
		public int Emblem
		{
			get
			{
				return m_emblem;
			}
			set
			{
				m_emblem = value;
			}
		}
		
		public int Effect
		{
			get
			{
				return m_effect;
			}
			set
			{
				m_effect = value;
			}
		}
		
		public int Weight
		{
			get
			{
				return m_weight;
			}
			set
			{
				m_weight = value;
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

		public byte ModelExtension
		{
			get
			{
				return m_modelExtension;
			}
			set
			{
				m_modelExtension = value;
			}
		}
		
		public int Bonus
		{
			get
			{
				return m_bonus;
			}
			set
			{
				m_bonus = value;
			}
		}
		
		public int Bonus1
		{
			get
			{
				return m_bonus1;
			}
			set
			{
				m_bonus1 = value;
			}
		}
		
		public int Bonus2
		{
			get
			{
				return m_bonus2;
			}
			set
			{
				m_bonus2 = value;
			}
		}
		
		public int Bonus3
		{
			get
			{
				return m_bonus3;
			}
			set
			{
				m_bonus3 = value;
			}
		}
		
		public int Bonus4
		{
			get
			{
				return m_bonus4;
			}
			set
			{
				m_bonus4 = value;
			}
		}
		
		public int Bonus5
		{
			get
			{
				return m_bonus5;
			}
			set
			{
				m_bonus5 = value;
			}
		}
		
		public int ExtraBonus
		{
			get
			{
				return m_extrabonus;
			}
			set
			{
				m_extrabonus = value;
			}
		}
		
		public int Bonus1Type
		{
			get
			{
				return m_bonus1Type;
			}
			set
			{
				m_bonus1Type = value;
			}
		}
		
		public int Bonus2Type
		{
			get
			{
				return m_bonus2Type;
			}
			set
			{
				m_bonus2Type = value;
			}
		}
		public int Bonus3Type
		{
			get
			{
				return m_bonus3Type;
			}
			set
			{
				m_bonus3Type = value;
			}
		}
		public int Bonus4Type
		{
			get
			{
				return m_bonus4Type;
			}
			set
			{
				m_bonus4Type = value;
			}
		}
		public int Bonus5Type
		{
			get
			{
				return m_bonus5Type;
			}
			set
			{
				m_bonus5Type = value;
			}
		}
		
		public int ExtraBonusType
		{
			get
			{
				return m_extrabonusType;
			}
			set
			{
				m_extrabonusType = value;
			}
		}
		
		public bool IsPickable
		{
			get
			{
				return m_isPickable;
			}
			set
			{   
				m_isPickable = value;
			}
		}

		public bool IsDropable
		{
			get
			{
				return m_isDropable;
			}
			set
			{   
				m_isDropable = value;
			}
		}
		
		public long Value
		{
			get {	
				return (((0*1000L+0)*1000L+Gold)*100L+Silver)*100L+Copper;	
			}
		}
		
		public short Gold
		{
			get
			{
				return m_gold;
			}
			set
			{   
				m_gold = value;
			}
		}
		
		public byte Silver
		{
			get
			{
				return m_silver;
			}
			set
			{   
				m_silver = value;
			}
		}

		public byte Copper
		{
			get
			{
				return m_copper;
			}
			set
			{   
				m_copper = value;
			}
		}

		/// <summary>
		/// Max amount allowed in one stack
		/// </summary>
		public int MaxCount
		{
			get 
			{
				return m_maxCount;
			}
			set
			{
				m_maxCount = value;
			}
		}

		public bool IsStackable
		{
			get
			{
				return m_maxCount > 1;
			}
		}

		/// <summary>
		/// Amount of items sold at once
		/// </summary>
		public int PackSize
		{
			get
			{ 
				return m_packSize;
			}
			set
			{
				m_packSize = value;
			}
		}

		/// <summary>
		/// Charge of item when he have some charge of a spell
		/// </summary>
		public int Charges
		{
			get 
			{ 
				return m_charges; 
			}
			set
			{
				m_charges = value;
			}
		}

		/// <summary>
		/// Max charge of item when he have some charge of a spell
		/// </summary>
		public int MaxCharges
		{
			get 
			{
				return m_maxCharges; 
			}
			set
			{
				m_maxCharges = value;
			}
		}

		/// <summary>
		/// Spell id for items with charge
		/// </summary>
		public int SpellID
		{
			get 
			{ 
				return m_spellID;
			}
			set
			{
				m_spellID = value;
			}
		}

		/// <summary>
		/// ProcSpell id for items
		/// </summary>
		public int ProcSpellID
		{
			get 
			{ 
				return m_procSpellID;
			}
			set
			{
				m_procSpellID = value;
			}
		}

		public int Realm
		{
			get
			{ 
				return m_realm;
			}
			set
			{
				m_realm = value;
			}
		}

		public virtual bool IsMagical
		{
			get
			{
				return
					(Bonus1 != 0 && Bonus1Type != 0) ||
					(Bonus2 != 0 && Bonus2Type != 0) ||
					(Bonus3 != 0 && Bonus3Type != 0) ||
					(Bonus4 != 0 && Bonus4Type != 0) ||
					(Bonus5 != 0 && Bonus5Type != 0) ||
					(ExtraBonus != 0 && ExtraBonusType != 0);
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
			if(article == 0)
			{
				if(firstLetterUppercase)
					return "The "+Name;
				else
					return "the "+Name;
			}
			else
			{
				// if first letter is a vowel
				if(m_vowels.IndexOf(Name[0]) != -1)
				{
					if(firstLetterUppercase)
						return "An "+Name;
					else
						return "an "+Name;
				}
				else
				{
					if(firstLetterUppercase)
						return "A "+Name;
					else
						return "a "+Name;
				}
			}
		}
	}
}