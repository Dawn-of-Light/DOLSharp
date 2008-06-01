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
	/// The Melee Speed bonus percent calculator
	///
	/// BuffBonusCategory1 is used for buffs
	/// BuffBonusCategory2 unused
	/// BuffBonusCategory3 is used for debuff
	/// BuffBonusCategory4 unused
	/// BuffBonusMultCategory1 unused
	/// </summary>
	[PropertyCalculator(eProperty.MeleeSpeed)]
	public class MeleeSpeedPercentCalculator : PropertyCalculator
	{
		public override int CalcValue(GameLiving living, eProperty property)
		{
			return Math.Max(1, 100
				-living.BuffBonusCategory1[(int)property] // less is faster = buff
				+living.DebuffCategory[(int)property] // more is slower = debuff
				-Math.Min(10, living.ItemBonus[(int)property])); // http://www.camelotherald.com/more/1325.shtml
		}
	}
}
