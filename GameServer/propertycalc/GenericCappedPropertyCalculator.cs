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

namespace DOL.GS.PropertyCalc
{
	/// <summary>
	/// Generic Property Calculator for common Bonus behavior
	/// 
	/// Total value is HardCapped
	/// ItemBonus is used for item (item capped)
	/// BuffBonusCategory1 is used for buffs (buff Capped)
	/// BuffBonusCategory2 spec/short buff (buff Capped)
	/// BuffBonusCategory3 is used for debuffs (debuff Capped, Full Effect on Buff, Half Effect on remaining)
	/// AbilityBonus is used for Ability (Ability Capped)
	/// BuffBonusCategory4 for special effect (HardCapped)
	/// BuffBonusMultCategory1 unused
	/// </summary>
	public abstract class GenericCappedPropertyCalculator : PropertyCalculator
	{
		/// <summary>
		/// HardCap for Total Value (Ability+Other+Base-Debuff)
		/// </summary>
		public abstract int HardCap(GameLiving living);
		
		/// <summary>
		/// Cap For Item Bonuses
		/// </summary>
		public abstract int ItemCap(GameLiving living);

		/// <summary>
		/// Cap For Buff Bonuses
		/// </summary>
		public abstract int BuffCap(GameLiving living);
		
		/// <summary>
		/// Cap For Ability Bonuses
		/// </summary>
		public abstract int AbilityCap(GameLiving living);
		
		/// <summary>
		/// Cap For Base Item + Buff Bonuses
		/// </summary>
		public abstract int BaseCap(GameLiving living);
		
		/// <summary>
		/// Cap For Debuff (Minimal Total Value / Base Value)
		/// </summary>
		public abstract int DebuffCap(GameLiving living);
		
		/// <summary>
		/// Total Capped
		/// </summary>
		/// <param name="living"></param>
		/// <param name="property"></param>
		public override int CalcValue(GameLiving living, eProperty property)
		{
			int baseValue = CalcValueBase(living, property);
			int ability = living.AbilityBonus[property];
			int other = living.BuffBonusCategory4[property];
			
			// Ability + Base HardCapped, Other Uncapped
			return Math.Min(HardCap(living), Math.Min(AbilityCap(living), baseValue + ability) + other);
		}
		
		/// <summary>
		/// Total Base (without ability or other)
		/// </summary>
		/// <param name="living"></param>
		/// <param name="property"></param>
		/// <returns></returns>
		public override int CalcValueBase(GameLiving living, eProperty property)
		{
            int debuff = living.DebuffCategory[property];
            int itemBonus = CalcValueFromItems(living, property);
            int buffBonus = CalcValueFromBuffs(living, property);
            
            // Full effect on buff
            buffBonus -= debuff;
            
            // Half Effect on Item
            if (buffBonus < 0)
            {
                itemBonus += buffBonus / 2;
                buffBonus = 0;
            }
            
            // Clamp Between Debuff Cap and Base Cap
            return Math.Max(DebuffCap(living), Math.Min(BaseCap(living), itemBonus + buffBonus));
		}

		/// <summary>
		/// Total From Buffs
		/// </summary>
		/// <param name="living"></param>
		/// <param name="property"></param>
		public override int CalcValueFromBuffs(GameLiving living, eProperty property)
		{
            int buffBonus = living.BaseBuffBonusCategory[property]
            	+ living.SpecBuffBonusCategory[property];
            
            // Buff Capped
            return Math.Min(BuffCap(living), buffBonus);
		}

        /// <summary>
        /// Total From Items (can be used for specific debuff targetting items)
        /// </summary>
        /// <param name="living"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public override int CalcValueFromItems(GameLiving living, eProperty property)
        {
            int itemBonus = living.ItemBonus[property];

            // Item Capped
            return Math.Min(ItemCap(living), itemBonus);
        }
	}
}
