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
	/// BuffBonusCategory1 is used for buffs cast on the living.
	/// BuffBonusCategory2 is used for modified damage only (no full resist)
	/// BuffBonusCategory3 is used for debuffs
	/// BuffBonusCategory4 is used for buffs that have no softcap
	/// BuffBonusMultCategory1 unused
	/// </summary>
	/// <author>Aredhel</author>	
	[PropertyCalculator(eProperty.Resist_First, eProperty.Resist_Last)]
	public class ResistCalculator : PropertyCalculator
	{
		public ResistCalculator() { }

        /// <summary>
        /// Calculate the actual resist amount for the given living and the given
        /// resist type, applying all possible caps and cap increases.
        /// </summary>
        /// <param name="living">The living the resist amount is to be determined for.</param>
        /// <param name="property">The resist type.</param>
        /// <returns>The actual resist amount.</returns>
        public override int CalcValue(GameLiving living, eProperty property)
        {
            int propertyIndex = (int)property;

            // Abilities/racials/debuffs.

            int debuff = living.DebuffCategory[propertyIndex];
			int abilityBonus = living.AbilityBonus[propertyIndex];
            int racialBonus = (living is GamePlayer)
                ? SkillBase.GetRaceResist((eRace)((living as GamePlayer).Race), (eResist)property)
                : 0;

            // Items and buffs.

            int itemBonus = CalcValueFromItems(living, property);
            int buffBonus = CalcValueFromBuffs(living, property);

            // Apply debuffs. 100% Effectiveness for player buffs, but only 50%
            // effectiveness for item bonuses.

            buffBonus -= Math.Abs(debuff);

            if (buffBonus < 0)
            {
                itemBonus += buffBonus / 2;
                buffBonus = 0;
                if (itemBonus < 0)
                    itemBonus = 0;
            }

            // Add up and apply hardcap.

            return Math.Min(itemBonus + buffBonus + abilityBonus + racialBonus, HardCap);
        }

        /// <summary>
        /// Calculate modified resists from buffs only.
        /// </summary>
        /// <param name="living"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public override int CalcValueFromBuffs(GameLiving living, eProperty property)
        {
            int buffBonus = living.BuffBonusCategory1[(int)property]
				+ living.BuffBonusCategory4[(int)property];
            return Math.Min(buffBonus, BuffBonusCap);
        }

        /// <summary>
        /// Calculate modified resists from items only.
        /// </summary>
        /// <param name="living"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public override int CalcValueFromItems(GameLiving living, eProperty property)
        {
            int itemBonus = living.ItemBonus[(int)property];

            // Item bonus cap and cap increase from Mythirians.

            int itemBonusCap = living.Level / 2 + 1;
            int itemBonusCapIncrease = GetItemBonusCapIncrease(living, property);
            return Math.Min(itemBonus, itemBonusCap + itemBonusCapIncrease);
        }

        /// <summary>
        /// Returns the resist cap increase for the given living and the given
        /// resist type. It is hardcapped at 5% for the time being.
        /// </summary>
        /// <param name="living">The living the cap increase is to be determined for.</param>
        /// <param name="property">The resist type.</param>
        /// <returns></returns>
        public static int GetItemBonusCapIncrease(GameLiving living, eProperty property)
        {
            if (living == null) return 0;
            return Math.Min(living.ItemBonus[(int)(eProperty.ResCapBonus_First - eProperty.Resist_First + property)], 5);
        }

        /// <summary>
        /// Cap for player cast resist buffs.
        /// </summary>
        public static int BuffBonusCap
        {
            get { return 24; }
        }

        /// <summary>
        /// Hard cap for resists.
        /// </summary>
        public static int HardCap
        {
            get { return 90; }
        }
	}
}
