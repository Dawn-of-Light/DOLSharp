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
using System.Reflection;
using DOL.Database.Attributes;

using log4net;

namespace DOL.Database
{
	[DataTable(TableName = "ItemTemplate", PreCache = true)]
	public class ItemTemplate : DataObject
		
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		public static string m_blankItem = "Item_Template";
		
		protected string m_id_nb;
		protected string m_name;
		protected int m_level;
		
		// dur_con
		protected int m_durability;
		protected int m_condition;
		protected int m_maxdurability;
		protected int m_maxcondition;
		protected int m_quality;
		protected int m_weight;
		protected bool m_isIndestructible;
		protected bool m_isNotLosingDur;

		// weapon/armor
		protected int m_dps_af;
		protected int m_spd_abs;
		protected int m_hand;
		protected int m_type_damage;
		protected int m_object_type;
		protected int m_item_type;
		
		// apparence
		protected int m_color;
		protected int m_emblem;
		protected int m_effect;
		protected int m_model;
		protected byte m_extension;
		
		// bonuses
		protected int m_bonus;
		protected int m_bonus1;
		protected int m_bonus2;
		protected int m_bonus3;
		protected int m_bonus4;
		protected int m_bonus5;
		protected int m_bonus6;
		protected int m_bonus7;
		protected int m_bonus8;
		protected int m_bonus9;
		protected int m_bonus10;
		protected int m_extrabonus;
		protected int m_bonusType;
		protected int m_bonus1Type;
		protected int m_bonus2Type;
		protected int m_bonus3Type;
		protected int m_bonus4Type;
		protected int m_bonus5Type;
		protected int m_bonus6Type;
		protected int m_bonus7Type;
		protected int m_bonus8Type;
		protected int m_bonus9Type;
		protected int m_bonus10Type;
		protected int m_extrabonusType;
		
		// money
		protected long m_Price;
		
		// properties
		protected bool m_isDropable;
		protected bool m_isPickable;
		protected bool m_isTradable;
		protected bool m_canDropAsLoot;
		
		// stack
		protected int m_maxCount;
		protected int m_packSize;
		
		// proc & charges
		protected int m_spellID;
		protected int m_procSpellID;
		protected int m_maxCharges;
		protected int m_charges;
		protected int m_spellID1;
		protected int m_procSpellID1;
		protected int m_charges1;
		protected int m_maxCharges1;
		protected byte m_procChance;
		protected int m_poisonSpellID;
		protected int m_poisonMaxCharges;
		protected int m_poisonCharges;
		
		protected int m_realm;
		protected string m_allowedClasses;
		protected int m_canUseEvery;
		protected int m_flags;
		protected int m_bonusLevel;
		protected int m_levelRequirement;
		protected string m_description;
		
		protected string m_packageID;
		protected string m_classType;

		public ItemTemplate()
		{
			m_id_nb = m_blankItem;
			m_name = "(blank item)";
			m_level = 0;
			m_durability = m_maxdurability = 1;
			m_condition  = m_maxcondition = 1;
			m_quality = 1;
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
			m_extension = 0;
			m_bonus = 0;
			m_bonus1 = 0;
			m_bonus2 = 0;
			m_bonus3 = 0;
			m_bonus4 = 0;
			m_bonus5 = 0;
			m_bonus6 = 0;
			m_bonus7 = 0;
			m_bonus8 = 0;
			m_bonus9 = 0;
			m_bonus10 = 0;
			m_extrabonus = 0;
			m_bonusType = 0;
			m_bonus1Type = 0;
			m_bonus2Type = 0;
			m_bonus3Type = 0;
			m_bonus4Type = 0;
			m_bonus5Type = 0;
			m_bonus6Type = 0;
			m_bonus7Type = 0;
			m_bonus8Type = 0;
			m_bonus9Type = 0;
			m_bonus10Type = 0;
			m_extrabonusType = 0;
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
			m_procChance = 0;
			m_charges1 = 0;
			m_maxCharges1 = 0;
			m_poisonCharges = 0;
			m_poisonMaxCharges = 0;
			m_poisonSpellID = 0;
			m_realm = 0;
			m_allowedClasses = "0";
			m_flags = 0;
			m_bonusLevel = 0;
			m_levelRequirement = 0;
			m_description = "";
			m_packageID = "";
			m_classType = "";
		}

		public ItemTemplate(ItemTemplate template)
		{
			m_id_nb = template.Id_nb;
			Name = template.Name;
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
			Object_Type = template.Object_Type;
			Quality = template.Quality;
			SPD_ABS = template.SPD_ABS;
			Type_Damage = template.Type_Damage;
			Weight = template.Weight;
			Price = template.Price;
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
			ProcChance = template.ProcChance;
			PoisonSpellID = template.PoisonSpellID;
			PoisonCharges = template.PoisonCharges;
			PoisonMaxCharges = template.PoisonMaxCharges;
			Realm = template.Realm;
			AllowedClasses = template.AllowedClasses;
			CanUseEvery = template.CanUseEvery;
			Flags = template.Flags;
			BonusLevel = template.BonusLevel;
			LevelRequirement = template.LevelRequirement;
			Description = template.Description;
			IsIndestructible = template.IsIndestructible;
			IsNotLosingDur = template.IsNotLosingDur;
			PackageID = template.PackageID;
			ClassType = template.ClassType;
		}

		[PrimaryKey]
		public virtual string Id_nb
		{
            get { return m_id_nb; }
			set
			{
				m_id_nb = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public string Name
		{
            get { return m_name; }
			set
			{
                Dirty = true;
				m_name = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public int Level
		{
            get { return m_level; }
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
				if (m_durability==0)
					return m_maxdurability;
				else
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
            get { return m_maxdurability; }
            set
            {
                Dirty = true;
                m_maxdurability = value;
            }
        }

        /// <summary>
        /// Your item will not lose dur over repairs
        /// </summary>
        [DataElement(AllowDbNull = true)]
        public bool IsNotLosingDur
        {
            get { return m_isNotLosingDur; }
            set
            {
                Dirty = true;
                m_isNotLosingDur = value;
            }
        }

		[DataElement(AllowDbNull = true)]
		public int Condition
		{
			get
			{
				if (m_condition==0)
					return m_maxcondition;
				else
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
            get { return m_maxcondition; }
			set
			{
                Dirty = true;
				m_maxcondition = value;
			}
		}
		
		[DataElement(AllowDbNull = true)]
		public int Quality
		{
            get { return m_quality; }
			set
			{
                Dirty = true;
				m_quality = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int DPS_AF
		{
            get { return m_dps_af; }
			set
			{
                Dirty = true;
				m_dps_af = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int SPD_ABS
		{
            get { return m_spd_abs; }
			set
			{
                Dirty = true;
				m_spd_abs = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int Hand
		{
            get { return m_hand; }
			set
			{
                Dirty = true;
				m_hand = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int Type_Damage
		{
            get { return m_type_damage; }
			set
			{
                Dirty = true;
				m_type_damage = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int Object_Type
		{
            get { return m_object_type; }
			set
			{
                Dirty = true;
				m_object_type = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int Item_Type
		{
            get { return m_item_type; }
			set
			{
                Dirty = true;
				m_item_type = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int Color
		{
            get { return m_color; }
			set
			{
                Dirty = true;
				m_color = value;
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
		public int Effect
		{
            get { return m_effect; }
			set
			{
                Dirty = true;
				m_effect = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int Weight
		{
            get { return m_weight; }
			set
			{
                Dirty = true;
				m_weight = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public int Model
		{
            get { return m_model; }
			set
			{
                Dirty = true;
				m_model = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public byte Extension
		{
            get { return m_extension; }
			set
			{
                Dirty = true;
				m_extension = value;
			}
		}

        #region Bonuses

        [DataElement(AllowDbNull = true)]
		public int Bonus
		{
            get { return m_bonus; }
			set
			{
                Dirty = true;
				m_bonus = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public virtual int Bonus1
		{
            get { return m_bonus1; }
			set
			{
                Dirty = true;
				m_bonus1 = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public virtual int Bonus2
		{
            get { return m_bonus2; }
			set
			{
                Dirty = true;
				m_bonus2 = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public virtual int Bonus3
		{
            get { return m_bonus3; }
			set
			{
                Dirty = true;
				m_bonus3 = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public virtual int Bonus4
		{
            get { return m_bonus4; }
			set
			{
                Dirty = true;
				m_bonus4 = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public virtual int Bonus5
		{
            get { return m_bonus5; }
			set
			{
                Dirty = true;
				m_bonus5 = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public virtual int Bonus6
		{
            get { return m_bonus6; }
			set
			{
                Dirty = true;
				m_bonus6 = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public virtual int Bonus7
		{
            get { return m_bonus7; }
			set
			{
                Dirty = true;
				m_bonus7 = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public virtual int Bonus8
		{
            get { return m_bonus8; }
			set
			{
                Dirty = true;
				m_bonus8 = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public virtual int Bonus9
		{
            get { return m_bonus9; }
			set
			{
                Dirty = true;
				m_bonus9 = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public virtual int Bonus10
		{
            get { return m_bonus10; }
			set
			{
                Dirty = true;
				m_bonus10 = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int ExtraBonus
		{
            get { return m_extrabonus; }
			set
			{
                Dirty = true;
				m_extrabonus = value;
			}
		}

        #endregion

        #region BonusTypes

        [DataElement(AllowDbNull = true)]
		public virtual int Bonus1Type
		{
            get { return m_bonus1Type; }
			set
			{
                Dirty = true;
				m_bonus1Type = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public virtual int Bonus2Type
		{
            get { return m_bonus2Type; }
			set
			{
                Dirty = true;
				m_bonus2Type = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public virtual int Bonus3Type
		{
            get { return m_bonus3Type; }
			set
			{
                Dirty = true;
				m_bonus3Type = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public virtual int Bonus4Type
		{
            get { return m_bonus4Type; }
			set
			{
                Dirty = true;
				m_bonus4Type = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public virtual int Bonus5Type
		{
            get { return m_bonus5Type; }
			set
			{
                Dirty = true;
				m_bonus5Type = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public virtual int Bonus6Type
		{
            get { return m_bonus6Type; }
			set
			{
                Dirty = true;
				m_bonus6Type = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public virtual int Bonus7Type
		{
            get { return m_bonus7Type; }
			set
			{
                Dirty = true;
				m_bonus7Type = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public virtual int Bonus8Type
		{
            get { return m_bonus8Type; }
            set
			{
                Dirty = true;
				m_bonus8Type = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public virtual int Bonus9Type
		{
            get { return m_bonus9Type; }
			set
			{
                Dirty = true;
				m_bonus9Type = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public virtual int Bonus10Type
		{
            get { return m_bonus10Type; }
			set
			{
                Dirty = true;
				m_bonus10Type = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int ExtraBonusType
		{
            get { return m_extrabonusType; }
			set
			{
                Dirty = true;
				m_extrabonusType = value;
			}
		}

        #endregion

        #region Charges

        [DataElement(AllowDbNull = false)]
        public int CanUseEvery
        {
            get { return m_canUseEvery; }
            set
            {
                Dirty = true;
                m_canUseEvery = value;
            }
        }

        #region first charges

        /// <summary>
        /// Spell id for items with charge
        /// </summary>
        [DataElement(AllowDbNull = true)]
        public virtual int SpellID
        {
            get { return m_spellID; }
            set
            {
                Dirty = true;
                m_spellID = value;
            }
        }

        /// <summary>
        /// Charge of item when he have some charge of a spell
        /// </summary>
        [DataElement(AllowDbNull = true)]
        public int Charges
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
        public int MaxCharges
        {
            get { return m_maxCharges; }
            set
            {
                Dirty = true;
                m_maxCharges = value;
            }
        }

        #endregion
        #region second charges

        /// <summary>
        /// Spell id for items with charge
        /// </summary>
        [DataElement(AllowDbNull = true)]
        public virtual int SpellID1
        {
            get { return m_spellID1; }
            set
            {
                Dirty = true;
                m_spellID1 = value;
            }
        }

        [DataElement(AllowDbNull = true)]
        public int Charges1
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
        public int MaxCharges1
        {
            get { return m_maxCharges1; }
            set
            {
                Dirty = true;
                m_maxCharges1 = value;
            }
        }

        #endregion

        #endregion

        #region Procs

        [DataElement(AllowDbNull = true)]
        public virtual byte ProcChance
        {
            get { return m_procChance; }
            set
            {
                Dirty = true;
                m_procChance = value;
            }
        }

        #region first proc

        /// <summary>
        /// ProcSpell id for items
        /// </summary>
        [DataElement(AllowDbNull = true)]
        public virtual int ProcSpellID
        {
            get { return m_procSpellID; }
            set
            {
                Dirty = true;
                m_procSpellID = value;
            }
        }

        #endregion
        #region second proc

        [DataElement(AllowDbNull = true)]
        public virtual int ProcSpellID1
        {
            get { return m_procSpellID1; }
            set
            {
                Dirty = true;
                m_procSpellID1 = value;
            }
        }

        #endregion

        #endregion

        #region Poison

        [DataElement(AllowDbNull = true)]
        public int PoisonSpellID
        {
            get { return m_poisonSpellID; }
            set
            {
                Dirty = true;
                m_poisonSpellID = value;
            }
        }

        [DataElement(AllowDbNull = true)]
        public int PoisonCharges
        {
            get { return m_poisonCharges; }
            set
            {
                Dirty = true;
                m_poisonCharges = value;
            }
        }

        [DataElement(AllowDbNull = true)]
        public int PoisonMaxCharges
        {
            get { return m_poisonMaxCharges; }
            set
            {
                Dirty = true;
                m_poisonMaxCharges = value;
            }
        }

        #endregion

        [DataElement(AllowDbNull = true)]
		public bool IsPickable
		{
            get { return m_isPickable; }
			set
			{
                Dirty = true;
				m_isPickable = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public bool IsDropable
		{
            get { return m_isDropable; }
			set
			{
                Dirty = true;
				m_isDropable = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public bool CanDropAsLoot
		{
            get { return m_canDropAsLoot; }
			set
			{
                Dirty = true;
				m_canDropAsLoot = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public bool IsTradable
		{
            get { return m_isTradable; }
			set
			{
                Dirty = true;
				m_isTradable = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public long Price
		{
            get { return m_Price; }
			set
			{
                Dirty = true;
				m_Price = value;
			}
		}

		/// <summary>
		/// Max amount allowed in one stack
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int MaxCount
		{
			get { return m_maxCount; }
            set {
                Dirty = true;
                m_maxCount = value;
            }
		}

		public bool IsStackable
		{
            get { return m_maxCount > 1; }
		}

		/// <summary>
		/// Your item cannot be shift + d
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public bool IsIndestructible {
			get { return m_isIndestructible; }
			set {
                Dirty = true;
                m_isIndestructible = value;
            }
		}
		
		/// <summary>
		/// Amount of items sold at once
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int PackSize
		{
			get { return m_packSize; }
			set	{
                Dirty = true;
                m_packSize = value;
            }
		}

		[DataElement(AllowDbNull = true)]
		public int Realm
		{
			get { return m_realm; }
            set
            {
                Dirty = true;
                m_realm = value;
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
                Dirty = true;
				m_allowedClasses = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int Flags
		{
            get { return this.m_flags; }
			set
			{
                Dirty = true;
				this.m_flags = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int BonusLevel
		{
            get { return this.m_bonusLevel; }
			set
			{
                Dirty = true;
				this.m_bonusLevel = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int LevelRequirement
		{
            get { return this.m_levelRequirement; }
			set
			{
                Dirty = true;
				this.m_levelRequirement = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public string PackageID
		{
            get { return this.m_packageID; }
			set
			{
                Dirty = true;
				this.m_packageID = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public string Description
		{
            get { return this.m_description; }
			set
			{
                Dirty = true;
				this.m_description = value;
			}
		}


		[DataElement(AllowDbNull = true, Varchar = 255)]
		public string ClassType
		{
            get { return this.m_classType; }
			set
			{
                Dirty = true;
				this.m_classType = value;
			}
		}

		// Various Methods		
		public virtual byte BaseDurabilityPercent
		{
			get
			{
				return (byte)((MaxDurability > 0) ?Durability * 100 / MaxDurability : 0);
			}
		}

		public virtual byte BaseConditionPercent
		{
			get	{
				return (byte)Math.Round((MaxCondition > 0) ? (double)Condition / MaxCondition * 100 : 0);
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
					(Bonus6 != 0 && Bonus6Type != 0) ||
					(Bonus7 != 0 && Bonus7Type != 0) ||
					(Bonus8 != 0 && Bonus8Type != 0) ||
					(Bonus9 != 0 && Bonus9Type != 0) ||
					(Bonus10 != 0 && Bonus10Type != 0) ||
					(ExtraBonus != 0 && ExtraBonusType != 0);
			}
		}

		private const string m_vowels = "aeuio";
		/// <summary>
		/// Returns name with article for nouns
		/// </summary>
		/// <param name="article">0=definite, 1=indefinite</param>
		/// <param name="firstLetterUppercase">Forces the first letter of the returned string to be uppercase</param>
		/// <returns>name of this object (includes article if needed)</returns>
		public virtual string GetName(int article, bool firstLetterUppercase)
		{
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

		/// <summary>
		/// Get the bonus amount.
		/// </summary>
		/// <param name="bonusID"></param>
		/// <returns></returns>
		public int GetBonusAmount(ArtifactBonus.ID bonusID)
		{
			switch ((int)bonusID)
			{
				case 0:
					return Bonus1;
				case 1:
					return Bonus2;
				case 2:
					return Bonus3;
				case 3:
					return Bonus4;
				case 4:
					return Bonus5;
				case 5:
					return Bonus6;
				case 6:
					return Bonus7;
				case 7:
					return Bonus8;
				case 8:
					return Bonus9;
				case 9:
					return Bonus10;
			}

			return 0;
		}

		/// <summary>
		/// Get the bonus type.
		/// </summary>
		/// <param name="bonusID"></param>
		/// <returns></returns>
		public int GetBonusType(ArtifactBonus.ID bonusID)
		{
			switch ((int)bonusID)
			{
				case 0:
					return Bonus1Type;
				case 1:
					return Bonus2Type;
				case 2:
					return Bonus3Type;
				case 3:
					return Bonus4Type;
				case 4:
					return Bonus5Type;
				case 5:
					return Bonus6Type;
				case 6:
					return Bonus7Type;
				case 7:
					return Bonus8Type;
				case 8:
					return Bonus9Type;
				case 9:
					return Bonus10Type;
				case 10:
					return SpellID;
				case 11:
					return SpellID1;
				case 12:
					return ProcSpellID;
				case 13:
					return ProcSpellID1;
			}

			return 0;
		}

		/// <summary>
		/// Set the bonus amount.
		/// </summary>
		/// <param name="bonusID"></param>
		/// <returns></returns>
		public void SetBonusAmount(ArtifactBonus.ID bonusID, int bonusAmount)
		{
			switch ((int)bonusID)
			{
				case 0:
					Bonus1 = bonusAmount;
					break;
				case 1:
					Bonus2 = bonusAmount;
					break;
				case 2:
					Bonus3 = bonusAmount;
					break;
				case 3:
					Bonus4 = bonusAmount;
					break;
				case 4:
					Bonus5 = bonusAmount;
					break;
				case 5:
					Bonus6 = bonusAmount;
					break;
				case 6:
					Bonus7 = bonusAmount;
					break;
				case 7:
					Bonus8 = bonusAmount;
					break;
				case 8:
					Bonus9 = bonusAmount;
					break;
				case 9:
					Bonus10 = bonusAmount;
					break;
			}
		}

		/// <summary>
		/// Set the bonus type.
		/// </summary>
		/// <param name="bonusID"></param>
		/// <returns></returns>
		public void SetBonusType(ArtifactBonus.ID bonusID, int bonusType)
		{
			switch ((int)bonusID)
			{
				case 0:
					Bonus1Type = bonusType;
					break;
				case 1:
					Bonus2Type = bonusType;
					break;
				case 2:
					Bonus3Type = bonusType;
					break;
				case 3:
					Bonus4Type = bonusType;
					break;
				case 4:
					Bonus5Type = bonusType;
					break;
				case 5:
					Bonus6Type = bonusType;
					break;
				case 6:
					Bonus7Type = bonusType;
					break;
				case 7:
					Bonus8Type = bonusType;
					break;
				case 8:
					Bonus9Type = bonusType;
					break;
				case 9:
					Bonus10Type = bonusType;
					break;
				case 10:
					SpellID = bonusType;
					break;
				case 11:
					SpellID1 = bonusType;
					break;
				case 12:
					ProcSpellID = bonusType;
					break;
				case 13:
					ProcSpellID1 = bonusType;
					break;
			}
		}
	}
}
