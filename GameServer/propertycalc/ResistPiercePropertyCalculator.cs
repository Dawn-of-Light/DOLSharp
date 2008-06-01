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
	/// The resist pierce bonus calculator
	///
	/// BuffBonusCategory1 is used for buffs
	/// BuffBonusCategory2 unused
	/// BuffBonusCategory3 is used for debuff (never seen on live)
	/// BuffBonusCategory4 unused
	/// BuffBonusMultCategory1 unused
	/// </summary>
	[PropertyCalculator(eProperty.ResistPierce)]
	public class ResistPierceCalculator : PropertyCalculator
	{
		public override int CalcValue(GameLiving living, eProperty property)
		{
			// cap at living.level/5
			return Math.Min(Math.Max(1,living.Level/5),
				living.BuffBonusCategory1[(int)property]
				- living.DebuffCategory[(int)property]
				+ living.ItemBonus[(int)property]); 
			/*
			* 
			* Test Version 1.70v Release Notes June 1, 2004
			* NEW THINGS AND BUG FIXES
			* - Spell Piercing bonuses now correctly modify resistances downward instead of upward,
			* for direct damage spells, debuffs and damage over time spells.
			* A level 50 character can not have more than 10% Spell Piercing effect up at any one time 
			* (from items or spells); any Spell Piercing over 10% is ignored 
			* (previously Spell Piercing was "capped" at 25% for a level 50 character). 
			*/
			// http://www.camelotherald.com/more/1325.shtml
		}
	}
}
