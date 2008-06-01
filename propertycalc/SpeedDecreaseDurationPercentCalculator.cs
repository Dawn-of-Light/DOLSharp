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
using DOL.GS.RealmAbilities;

namespace DOL.GS.PropertyCalc
{
	/// <summary>
	/// The melee damage bonus percent calculator
	/// 
	/// BuffBonusCategory1 is used for buffs
	/// BuffBonusCategory2 unused
	/// BuffBonusCategory3 is used for debuff
	/// BuffBonusCategory4 unused
	/// BuffBonusMultCategory1 unused
	/// </summary>
	[PropertyCalculator(eProperty.SpeedDecreaseDuration)]
	public class SpeedDecreaseDurationPercentCalculator : PropertyCalculator
	{
		public override int CalcValue(GameLiving living, eProperty property) 
		{
			int percent = 100
				-living.BuffBonusCategory1[(int)property] // buff reduce the duration
				+living.DebuffCategory[(int)property]
				-living.ItemBonus[(int)property]
				-living.AbilityBonus[(int)property];

			if (living.HasAbility(Abilities.Stoicism))
				percent -= 25;

			return Math.Max(1, percent);
		}
	}
}
