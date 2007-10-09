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
using DOL.GS.Spells;
using DOL.GS.Effects;

namespace DOL.GS.PropertyCalc
{
	/// <summary>
	/// The Archery Range bonus percent calculator
	///
	/// BuffBonusCategory1 unused
	/// BuffBonusCategory2 unused
	/// BuffBonusCategory3 is used for debuff
	/// BuffBonusCategory4 unused
	/// BuffBonusMultCategory1 unused
	/// </summary>
	[PropertyCalculator(eProperty.ArcheryRange)]
	public class ArcheryRangePercentCalculator : PropertyCalculator
	{
		public override int CalcValue(GameLiving living, eProperty property)
		{
			int debuff = living.BuffBonusCategory3[(int)property];
			if(debuff > 0)
			{
				GameSpellEffect nsreduction = SpellHandler.FindEffectOnTarget(living, "NearsightReduction");
				if(nsreduction!=null) debuff *= (int)(1.00 - nsreduction.Spell.Value * 0.01);
			}
			
			int item = Math.Max(0, 100
				- debuff
				+ Math.Min(10, living.ItemBonus[(int)property]));// http://www.camelotherald.com/more/1325.shtml

			int ra = 0;
			if (living.RangeAttackType == GameLiving.eRangeAttackType.Long)
			{
				ra = 50;
				IGameEffect effect = living.EffectList.GetOfType(typeof(TrueshotEffect));
				if (effect != null)
					effect.Cancel(false);
			}

			return item + ra;
		}
	}
}
