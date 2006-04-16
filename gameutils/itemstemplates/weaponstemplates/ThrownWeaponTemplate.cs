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
using System.Collections;
using System;
using System.Reflection;
using DOL.GS.Database;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Summary description for a ThrownWeaponTemplate
	/// </summary> 
	public class ThrownWeaponTemplate : RangedWeaponTemplate, IStackableItemTemplate
	{
		#region Declaraction
		/// <summary>
		/// The max count of the template
		/// </summary>
		protected int m_maxCount;

		/// <summary>
		/// The number of instance to buy
		/// </summary>
		protected int m_packSize;

		#endregion

		#region Get and Set
		/// <summary>
		/// Gets or sets how much items could be stack in the same inventory slot
		/// </summary>
		public int MaxCount
		{
			get { return m_maxCount; }
			set	{ m_maxCount = value; }
		}

		/// <summary>
		/// Gets or sets how much items will be buy
		/// </summary>
		public int PackSize
		{
			get { return m_packSize; }
			set	{ m_packSize = value; }
		}

		#endregion

		/// <summary>
		/// Gets the object type of the template (for test use class type instead of this propriety)
		/// </summary>
		public override eObjectType ObjectType
		{
			get { return eObjectType.ThrownWeapon; }
		}

		/// <summary>
		/// Create a object usable by players using this template
		/// </summary>
		public override GenericItem CreateInstance()
		{
			ThrownWeapon item = new ThrownWeapon();
			item.Name = m_name;
			item.Level = m_level;
			item.Weight = m_weight;
			item.Value = m_value;
			item.Realm = m_realm;
			item.Model = m_model;
			item.IsSaleable = m_isSaleable;
			item.IsTradable = m_isTradable;
			item.IsDropable = m_isDropable;
			item.QuestName = "";
			item.CrafterName = "";
			item.Quality = m_quality;
			item.Bonus = m_bonus;
			item.Durability = m_durability;
			item.Condition = m_condition;
			item.MaterialLevel = m_materialLevel;
			item.ProcSpellID = m_procSpellID;
			item.ProcEffectType = m_procEffectType;
			item.ChargeSpellID = m_chargeSpellID;
			item.ChargeEffectType = m_chargeEffectType;
			item.Charge = m_charge;
			item.MaxCharge = m_maxCharge;
			item.AllowedClass = (Iesi.Collections.ISet)AllowedClass.Clone();
			item.MagicalBonus = (Iesi.Collections.ISet)MagicalBonus.Clone();
			item.Color = m_color;
			item.DamagePerSecond = m_damagePerSecond;
			item.Speed = m_speed;
			item.DamageType = m_damageType;
			item.HandNeeded = m_handNeeded;
			item.GlowEffect = m_glowEffect;
			item.WeaponRange = m_weaponRange;

			item.MaxCount = m_maxCount;
			item.Count = m_packSize;

			return item;
		}
	}
}
