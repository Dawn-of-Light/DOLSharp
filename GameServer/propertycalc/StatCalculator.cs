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
	/// The Character Stat calculator
	/// 
	/// BuffBonusCategory1 is used for all single stat buffs
	/// BuffBonusCategory2 is used for all dual stat buffs
	/// BuffBonusCategory3 is used for all debuffs (positive values expected here)
	/// BuffBonusCategory4 is used for all other uncapped modifications
	///                    category 4 kicks in at last
	/// BuffBonusMultCategory1 used after all buffs/debuffs
	/// </summary>
	/// <author>Aredhel</author>
	[PropertyCalculator(eProperty.Stat_First, eProperty.Stat_Last)]
	public class StatCalculator : PropertyCalculator
    {
        public StatCalculator() { }

        public override int CalcValue(GameLiving living, eProperty property)
        {
            int propertyIndex = (int)property;

            // Base stats/abilities/debuffs/death.

            int baseStat = living.GetBaseStat((eStat)property);
            int abilityBonus = living.AbilityBonus[propertyIndex];
            int debuff = living.DebuffCategory[propertyIndex];
			int deathConDebuff = 0;

            int itemBonus = CalcValueFromItems(living, property);
            int buffBonus = CalcValueFromBuffs(living, property);

			// Special cases:
			// 1) ManaStat (base stat + acuity, players only).
			// All caster classes benefit from item and ability acuity
			// bonuses, but only list casters can reap the benefits of
			// acuity buffs.
			// 2) Constitution lost at death, only affects players.

			if (living is GamePlayer)
			{
				GamePlayer player = living as GamePlayer;
				if (property == (eProperty)(player.CharacterClass.ManaStat))
					abilityBonus += player.AbilityBonus[(int)eProperty.Acuity];

				deathConDebuff = player.TotalConstitutionLostAtDeath;
			}

			// Apply debuffs, 100% effectiveness for player buffs, 50% effectiveness
			// for item and base stats

			int unbuffedBonus = baseStat + itemBonus;
			buffBonus -= Math.Abs(debuff);

			if (buffBonus < 0)
			{
				unbuffedBonus += buffBonus / 2;
				buffBonus = 0;
				if (unbuffedBonus < 0)
					unbuffedBonus = 0;
			}

			// Add up and apply any multiplicators.

			int stat = unbuffedBonus + buffBonus + abilityBonus;
			stat = (int)(stat * living.BuffBonusMultCategory1.Get((int)property));

			// Possibly apply constitution loss at death.

			stat -= (property == eProperty.Constitution)? deathConDebuff : 0;

			return Math.Max(1, stat);
        }

        /// <summary>
        /// Calculate modified bonuses from buffs only.
        /// </summary>
        /// <param name="living"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public override int CalcValueFromBuffs(GameLiving living, eProperty property)
        {
            if (living == null)
                return 0;

            int propertyIndex = (int)property;
            int baseBuffBonus = living.BaseBuffBonusCategory[propertyIndex];
            int specBuffBonus = living.SpecBuffBonusCategory[propertyIndex];

            if (living is GamePlayer)
            {
                GamePlayer player = living as GamePlayer;
                if (property == (eProperty)(player.CharacterClass.ManaStat))
                    if (player.CharacterClass.ClassType == eClassType.ListCaster)
                        specBuffBonus += player.BaseBuffBonusCategory[(int)eProperty.Acuity];
            }

            // Caps and cap increases. Only players actually have a buff bonus cap, 
            // pets don't.

            int baseBuffBonusCap = (living is GamePlayer) ? (int)(living.Level * 1.25) : Int16.MaxValue;
            int specBuffBonusCap = (living is GamePlayer) ? (int)(living.Level * 1.5 * 1.25) : Int16.MaxValue;
            
            // Apply soft caps.

            baseBuffBonus = Math.Min(baseBuffBonus, baseBuffBonusCap);
            specBuffBonus = Math.Min(specBuffBonus, specBuffBonusCap);

            return baseBuffBonus + specBuffBonus;
        }

        /// <summary>
        /// Calculate modified bonuses from items only.
        /// </summary>
        /// <param name="living"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public override int CalcValueFromItems(GameLiving living, eProperty property)
        {
            if (living == null)
                return 0;

            int itemBonus = living.ItemBonus[(int)property];
            int itemBonusCap = GetItemBonusCap(living, property);

            if (living is GamePlayer)
            {
                if (property == (eProperty)((living as GamePlayer).CharacterClass.ManaStat))
                    itemBonus += living.ItemBonus[(int)eProperty.Acuity];
            }

            int itemBonusCapIncrease = GetItemBonusCapIncrease(living, property);
            return Math.Min(itemBonus, itemBonusCap + itemBonusCapIncrease);
        }

        /// <summary>
        /// Returns the stat cap for this living and the given stat.
        /// </summary>
        /// <param name="living">The living the cap is to be determined for.</param>
        /// <param name="property">The stat.</param>
        /// <returns></returns>
        public static int GetItemBonusCap(GameLiving living, eProperty property)
        {
            if (living == null) return 0;
            return (int) (living.Level * 1.5);
        }

        /// <summary>
        /// Returns the stat cap increase for this living and the given stat.
        /// </summary>
        /// <param name="living">The living the cap increase is to be determined for.</param>
        /// <param name="property">The stat.</param>
        /// <returns></returns>
        public static int GetItemBonusCapIncrease(GameLiving living, eProperty property)
        {
            if (living == null) return 0;
            int itemBonusCapIncreaseCap = GetItemBonusCapIncreaseCap(living);
            int itemBonusCapIncrease = living.ItemBonus[(int)(eProperty.StatCapBonus_First - eProperty.Stat_First + property)];
            if (living is GamePlayer)
            {
                if (property == (eProperty)((living as GamePlayer).CharacterClass.ManaStat))
                    itemBonusCapIncrease += living.ItemBonus[(int)eProperty.AcuCapBonus];
            }

            return Math.Min(itemBonusCapIncrease, itemBonusCapIncreaseCap);
        }

        /// <summary>
        /// Returns the cap for stat cap increases.
        /// </summary>
        /// <param name="living">The living the value is to be determined for.</param>
        /// <returns>The cap increase cap for this living.</returns>
        public static int GetItemBonusCapIncreaseCap(GameLiving living)
        {
            if (living == null) return 0;
            return living.Level / 2 + 1;
        }
	}
}
