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
using DOL.AI.Brain;

namespace DOL.GS.PropertyCalc
{
	/// <summary>
	/// The Resistance Property calculator
	/// 
	/// Harcapped to 75.
	/// BaseBuffBonusCategory + SpecBuffBonusCategory is used for buffs cast on the living. (capped to 24)
	/// DebuffBonusCategory is used for debuffs clamped to 0 for player.
	/// AbilityBonusCategory is used for RA. capped to 50
	/// BuffBonusCategory4 is used for buffs that have no softcap
	/// BuffBonusMultCategory1 unused
	/// </summary>
	/// <author>Aredhel</author>	
	[PropertyCalculator(eProperty.Resist_First, eProperty.Resist_Last)]
	public class ResistCalculator : GenericCappedPropertyCalculator
	{

		public override int HardCap(GameLiving living)
		{
			return 75;
		}
		
		public override int AbilityCap(GameLiving living)
		{
			return 50;
		}

		public override int BaseCap(GameLiving living)
		{
			return 65;
		}
		
		public override int BuffCap(GameLiving living)
		{
			return 24;
		}
		
		public override int ItemCap(GameLiving living)
		{
			return (living.Level >> 2) + 1;
		}
		
		public override int DebuffCap(GameLiving living)
		{
			if (living is GamePlayer)
				return 0;
			
			return -25;
		}
		
		/// <summary>
		/// Resist base value lowered by debuff, clamped for player
		/// </summary>
		/// <param name="living"></param>
		/// <param name="property"></param>
		/// <returns></returns>
        public override int CalcValueBase(GameLiving living, eProperty property)
        {            
            int debuff = living.DebuffCategory[property];
            int racialBonus = SkillBase.GetRaceResist(living.Race, (eResist)property);
            
            int itemBonus = CalcValueFromItems(living, property);
            int buffBonus = CalcValueFromBuffs(living, property);
            
            // debuff = debuff Value / 2 + buff value / 2 : capped to debuff value (ex: 50%*1.25 on 24% = 62.5 => 62.5 / 2 + 24 / 2 = 43) 
            buffBonus -= Math.Min(debuff, ((Math.Abs(debuff) >> 1) + (buffBonus >> 1)));
            
            if (buffBonus < 0)
            {
                itemBonus += buffBonus;
                buffBonus = 0;
            }
            
            return Math.Max(DebuffCap(living) + racialBonus, Math.Min(BaseCap(living), itemBonus + buffBonus + racialBonus));
        }

        /// <summary>
        /// Calculate modified resists from items only. (override for item cap increase)
        /// </summary>
        /// <param name="living"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public override int CalcValueFromItems(GameLiving living, eProperty property)
        {
            return Math.Min(ItemCap(living) + GetItemBonusCapIncrease(living, property), living.ItemBonus[property]);
        }

        /// <summary>
        /// Returns the resist cap increase for the given living and the given
        /// resist type. It is hardcapped at 5% for the time being.
        /// </summary>
        /// <param name="living">The living the cap increase is to be determined for.</param>
        /// <param name="property">The resist type.</param>
        /// <returns></returns>
        public virtual int GetItemBonusCapIncrease(GameLiving living, eProperty property)
        {
            if (living == null)
            	return 0;
            
            return Math.Min(living.ItemBonus[(eProperty.ResCapBonus_First - eProperty.Resist_First + property)], 5);
        }
	}
	
	[PropertyCalculator(eProperty.Resist_Natural)]
	public class ResistNaturalCalculator : ResistCalculator
	{
		/// <summary>
        /// Returns the resist cap increase for the given living and the given
        /// resist type. It is hardcapped at 5% for the time being.
        /// </summary>
        /// <param name="living">The living the cap increase is to be determined for.</param>
        /// <param name="property">The resist type.</param>
        /// <returns></returns>
        public override int GetItemBonusCapIncrease(GameLiving living, eProperty property)
        {
            return 0;
        }

	}
}
