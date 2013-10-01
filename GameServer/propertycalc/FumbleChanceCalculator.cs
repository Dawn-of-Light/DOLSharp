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
	/// The fumble chance calculator. Returns 0 .. 1000 chance.
	/// 
	/// BuffBonusCategory1 unused
	/// BuffBonusCategory2 unused
	/// BuffBonusCategory3 is used for debuff
	/// BuffBonusCategory4 unused
	/// BuffBonusMultCategory1 unused
	/// </summary>
	[PropertyCalculator(eProperty.FumbleChance)]
	public class FumbleChanceCalculator : PropertyCalculator
	{
		public override int CalcValue(GameLiving living, eProperty property)
		{
			// 5% level 1, 0.1% level 50
			return Math.Max(51 - living.Level, (10 * living.DebuffCategory[(int)property]) + (10 * living.AbilityBonus[(int)property]));
		}
	}
	[PropertyCalculator(eProperty.SpellFumbleChance)]
	public class SpellFumbleChanceCalculator : PropertyCalculator
	{
		public override int CalcValue(GameLiving living, eProperty property)
		{
			return Math.Min(100, living.DebuffCategory[(int)property]);
		}
	}
}
