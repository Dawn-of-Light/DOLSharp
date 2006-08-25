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
	public struct ItemEntity
	{
		private int m_id;
		private byte m_armorFactor;
		private byte m_armorLevel;
		private int m_bonus;
		private byte m_bonusType;
		private byte m_charge;
		private byte m_chargeEffectType;
		private int m_chargeSpellId;
		private double m_condition;
		private int m_count;
		private string m_crafterName;
		private byte m_damage;
		private byte m_damagePerSecond;
		private byte m_damageType;
		private byte m_durability;
		private string m_genericItemType;
		private int m_glowEffect;
		private byte m_handNeeded;
		private int m_heading;
		private bool m_isDropable;
		private bool m_isSaleable;
		private bool m_isTradable;
		private byte m_level;
		private byte m_materialLevel;
		private byte m_maxCharge;
		private int m_maxCount;
		private int m_model;
		private byte m_modelExtension;
		private string m_name;
		private int m_or1;
		private byte m_precision;
		private byte m_procEffectType;
		private int m_procSpellId;
		private int m_quality;
		private string m_questName;
		private byte m_range;
		private byte m_realm;
		private int m_region;
		private byte m_respecType;
		private byte m_size;
		private int m_slotPosition;
		private int m_speed;
		private int m_spellId;
		private int m_tripPathId;
		private byte m_type;
		private long m_value;
		private int m_weaponRange;
		private int m_weight;
		private int m_x;
		private int m_y;
		private int m_z;

		public int Id
		{
			get { return m_id; }
			set { m_id = value; }
		}
		public byte ArmorFactor
		{
			get { return m_armorFactor; }
			set { m_armorFactor = value; }
		}
		public byte ArmorLevel
		{
			get { return m_armorLevel; }
			set { m_armorLevel = value; }
		}
		public int Bonus
		{
			get { return m_bonus; }
			set { m_bonus = value; }
		}
		public byte BonusType
		{
			get { return m_bonusType; }
			set { m_bonusType = value; }
		}
		public byte Charge
		{
			get { return m_charge; }
			set { m_charge = value; }
		}
		public byte ChargeEffectType
		{
			get { return m_chargeEffectType; }
			set { m_chargeEffectType = value; }
		}
		public int ChargeSpellId
		{
			get { return m_chargeSpellId; }
			set { m_chargeSpellId = value; }
		}
		public double Condition
		{
			get { return m_condition; }
			set { m_condition = value; }
		}
		public int Count
		{
			get { return m_count; }
			set { m_count = value; }
		}
		public string CrafterName
		{
			get { return m_crafterName; }
			set { m_crafterName = value; }
		}
		public byte Damage
		{
			get { return m_damage; }
			set { m_damage = value; }
		}
		public byte DamagePerSecond
		{
			get { return m_damagePerSecond; }
			set { m_damagePerSecond = value; }
		}
		public byte DamageType
		{
			get { return m_damageType; }
			set { m_damageType = value; }
		}
		public byte Durability
		{
			get { return m_durability; }
			set { m_durability = value; }
		}
		public string GenericItemType
		{
			get { return m_genericItemType; }
			set { m_genericItemType = value; }
		}
		public int GlowEffect
		{
			get { return m_glowEffect; }
			set { m_glowEffect = value; }
		}
		public byte HandNeeded
		{
			get { return m_handNeeded; }
			set { m_handNeeded = value; }
		}
		public int Heading
		{
			get { return m_heading; }
			set { m_heading = value; }
		}
		public bool IsDropable
		{
			get { return m_isDropable; }
			set { m_isDropable = value; }
		}
		public bool IsSaleable
		{
			get { return m_isSaleable; }
			set { m_isSaleable = value; }
		}
		public bool IsTradable
		{
			get { return m_isTradable; }
			set { m_isTradable = value; }
		}
		public byte Level
		{
			get { return m_level; }
			set { m_level = value; }
		}
		public byte MaterialLevel
		{
			get { return m_materialLevel; }
			set { m_materialLevel = value; }
		}
		public byte MaxCharge
		{
			get { return m_maxCharge; }
			set { m_maxCharge = value; }
		}
		public int MaxCount
		{
			get { return m_maxCount; }
			set { m_maxCount = value; }
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
		public string Name
		{
			get { return m_name; }
			set { m_name = value; }
		}
		public int or1
		{
			get { return m_or1; }
			set { m_or1 = value; }
		}
		public byte Precision
		{
			get { return m_precision; }
			set { m_precision = value; }
		}
		public byte ProcEffectType
		{
			get { return m_procEffectType; }
			set { m_procEffectType = value; }
		}
		public int ProcSpellId
		{
			get { return m_procSpellId; }
			set { m_procSpellId = value; }
		}
		public int Quality
		{
			get { return m_quality; }
			set { m_quality = value; }
		}
		public string QuestName
		{
			get { return m_questName; }
			set { m_questName = value; }
		}
		public byte Range
		{
			get { return m_range; }
			set { m_range = value; }
		}
		public byte Realm
		{
			get { return m_realm; }
			set { m_realm = value; }
		}
		public int Region
		{
			get { return m_region; }
			set { m_region = value; }
		}
		public byte RespecType
		{
			get { return m_respecType; }
			set { m_respecType = value; }
		}
		public byte Size
		{
			get { return m_size; }
			set { m_size = value; }
		}
		public int SlotPosition
		{
			get { return m_slotPosition; }
			set { m_slotPosition = value; }
		}
		public int Speed
		{
			get { return m_speed; }
			set { m_speed = value; }
		}
		public int SpellId
		{
			get { return m_spellId; }
			set { m_spellId = value; }
		}
		public int TripPathId
		{
			get { return m_tripPathId; }
			set { m_tripPathId = value; }
		}
		public byte Type
		{
			get { return m_type; }
			set { m_type = value; }
		}
		public long Value
		{
			get { return m_value; }
			set { m_value = value; }
		}
		public int WeaponRange
		{
			get { return m_weaponRange; }
			set { m_weaponRange = value; }
		}
		public int Weight
		{
			get { return m_weight; }
			set { m_weight = value; }
		}
		public int X
		{
			get { return m_x; }
			set { m_x = value; }
		}
		public int Y
		{
			get { return m_y; }
			set { m_y = value; }
		}
		public int Z
		{
			get { return m_z; }
			set { m_z = value; }
		}
	}
}
