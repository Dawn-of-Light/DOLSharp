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
	/// The Spell Range bonus percent calculator
	/// 
	/// BuffBonusCategory1 unused
	/// BuffBonusCategory2 unused
	/// BuffBonusCategory3 is used for debuff
	/// BuffBonusCategory4 unused
	/// BuffBonusMultCategory1 unused
	/// </summary>
	 
	//Debuff Effectivness
	[PropertyCalculator(eProperty.DebuffEffectivness)]
	public class DebuffEffectivnessPercentCalculator : PropertyCalculator
	{
		public override int CalcValue(GameLiving living, eProperty property) 
		{
			return Math.Min( 25 , living.ItemBonus[(int)property] 
				- living.BuffBonusCategory3[(int)property]);
		}
	}

	//Buff Effectivness
	[PropertyCalculator(eProperty.BuffEffectiveness)]
	public class BuffEffectivenessPercentCalculator : PropertyCalculator
	{
		public override int CalcValue(GameLiving living, eProperty property) 
		{
			return Math.Min( 25 , living.ItemBonus[(int)property] 
				- living.BuffBonusCategory3[(int)property]);
		}
	}

	// Healing Effectivness
	[PropertyCalculator(eProperty.HealingEffectiveness)]
	public class HealingEffectivenessPercentCalculator : PropertyCalculator
	{
		public override int CalcValue(GameLiving living, eProperty property) 
		{
			int percent = living.BuffBonusCategory1[(int)property]
				- living.BuffBonusCategory3[(int)property]
				+ living.ItemBonus[(int)property];

			if (living is GamePlayer)
			{
				//Relic bonnus is calculated before ra bonus
				percent = (int)(percent * RelicMgr.GetRelicBonusModifier(living.Realm, eRelicType.Magic));
			}

			//hardcap at 25%
			return Math.Min(25, percent);
		}
	}


	//Cast Speed
	[PropertyCalculator(eProperty.CastingSpeed)]
	public class SpellCastSpeedPercentCalculator : PropertyCalculator
	{
		public override int CalcValue(GameLiving living, eProperty property) 
		{
			return Math.Min( 10 , living.ItemBonus[(int)property] 
				- living.BuffBonusCategory3[(int)property]);
		}
	}

	//Spell Duration
	[PropertyCalculator(eProperty.SpellDuration)]
	public class SpellDurationPercentCalculator : PropertyCalculator
	{
		public override int CalcValue(GameLiving living, eProperty property) 
		{
			return Math.Min( 25 , living.ItemBonus[(int)property] 
				- living.BuffBonusCategory3[(int)property]);
		}
	}

	//Spell Damage
	[PropertyCalculator(eProperty.SpellDamage)]
	public class SpellDamagePercentCalculator : PropertyCalculator
	{
		public override int CalcValue(GameLiving living, eProperty property) 
		{
			return Math.Min(10, living.BuffBonusCategory1[(int)property] 
				+ living.ItemBonus[(int)property]
				- living.BuffBonusCategory3[(int)property] 
				+ living.AbilityBonus[(int)property]);
		}
	}
}
