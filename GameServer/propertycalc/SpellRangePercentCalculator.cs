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
	/// The Spell Range bonus percent calculator
	/// 
	/// BuffBonusCategory1 unused
	/// BuffBonusCategory2 is used for buff (like cloudsong/zahur/etc..)
	/// BuffBonusCategory3 is used for debuff
	/// BuffBonusCategory4 unused
	/// BuffBonusMultCategory1 unused
	/// 
	/// [Freya] Nidel: Item are cap to 10%, buff increase cap to 15% (10% item, 5% buff)
	/// http://www.youtube.com/watch?v=XcETvw5ge3s
	/// </summary>
	[PropertyCalculator(eProperty.SpellRange)]
	public class SpellRangePercentCalculator : PropertyCalculator
	{
		public override int CalcValue(GameLiving living, eProperty property) 
		{
			int debuff = living.DebuffCategory[(int)property];
			if(debuff > 0)
			{
				GameSpellEffect nsreduction = SpellHandler.FindEffectOnTarget(living, "NearsightReduction");
				if(nsreduction!=null) debuff *= (int)(1.00 - nsreduction.Spell.Value * 0.01);
			}
			int buff = CalcValueFromBuffs(living, property);
		    int item = CalcValueFromItems(living, property);
		    return Math.Max(0, 100 + (buff + item) - debuff);
		}

        public override int CalcValueFromBuffs(GameLiving living, eProperty property)
        {
            return Math.Min(5, living.SpecBuffBonusCategory[(int) property]);
        }

        public override int CalcValueFromItems(GameLiving living, eProperty property)
        {
            return Math.Min(10, living.ItemBonus[(int)property]);
        }
	}
}
