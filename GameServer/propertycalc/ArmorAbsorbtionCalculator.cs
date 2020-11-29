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
	[PropertyCalculator(eProperty.ArmorAbsorption)]
	public class ArmorAbsorptionCalculator : PropertyCalculator
	{
		public override int CalcValue(GameLiving living, eProperty property)
		{
			if (living is GameNPC)
			{
				return CalculateNPCMeleeAbsorb(living, property);
			}
			return GetEffectiveMeleeAbsorptionBonus(living, property);
		}

		private int GetEffectiveMeleeAbsorptionBonus(GameLiving living, eProperty property)
		{
			int buffBonus = living.BaseBuffBonusCategory[property];
			int debuffMalus = Math.Abs(living.DebuffCategory[property]);
			int itemBonus = living.ItemBonus[property];
			int abilityBonus = living.AbilityBonus[property];
			int hardCap = 50;
			return Math.Min(hardCap, (buffBonus - debuffMalus + itemBonus + abilityBonus));
		}

		private int CalculateNPCMeleeAbsorb(GameLiving living, eProperty property)
		{
			double absorbBonus = GetEffectiveMeleeAbsorptionBonus(living, property) / 100.0;

			double debuffBuffRatio = 2;

			double constitutionPerAbsorptionPercent = 4;
			double baseConstitutionPerAbsorptionPercent = 12; //kept for DB legacy reasons
			var constitutionBuffBonus = living.BaseBuffBonusCategory[eProperty.Constitution] + living.SpecBuffBonusCategory[eProperty.Constitution];
			var constitutionDebuffMalus = Math.Abs(living.DebuffCategory[eProperty.Constitution] + living.SpecDebuffCategory[eProperty.Constitution]);
			double constitutionAbsorb = 0;
			//simulate old behavior for base constitution
			double baseConstitutionAbsorb = (living.GetBaseStat((eStat)eProperty.Constitution) - 60) / baseConstitutionPerAbsorptionPercent / 100.0;
			double consitutionBuffAbsorb = (constitutionBuffBonus - constitutionDebuffMalus * debuffBuffRatio) / constitutionPerAbsorptionPercent / 100;
			constitutionAbsorb += baseConstitutionAbsorb + consitutionBuffAbsorb;

			//Note: On Live SpecAFBuffs do nothing => Cap to Live baseAF cap;
			double afPerAbsorptionPercent = 6;
			double liveBaseAFcap = 150 * 1.25 * 1.25;
			double afBuffBonus = Math.Min(liveBaseAFcap, living.BaseBuffBonusCategory[eProperty.ArmorFactor] + living.SpecBuffBonusCategory[eProperty.ArmorFactor]);
			double afDebuffMalus = Math.Abs(living.DebuffCategory[eProperty.ArmorFactor] + living.SpecDebuffCategory[eProperty.ArmorFactor]);
			double afBuffAbsorb = (afBuffBonus - afDebuffMalus * debuffBuffRatio) / afPerAbsorptionPercent / 100;

			double baseAbsorb = 0;
			if (living.Level >= 30) baseAbsorb = 0.27;
			else if (living.Level >= 20) baseAbsorb = 0.19;
			else if (living.Level >= 10) baseAbsorb = 0.10;

			double absorb = 1 - (1 - absorbBonus) * (1 - baseAbsorb) * (1 - constitutionAbsorb) * (1 - afBuffAbsorb);
			return (int)(100 * absorb);
		}
	}
}
