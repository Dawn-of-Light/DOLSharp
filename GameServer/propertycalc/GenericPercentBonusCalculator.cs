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
	/// Generic Property Calculator for Percent Bonus behavior, no Negative Malus allowed
	/// 
	/// This unallow Debuff to go under 0 (only allow for debuff to reduce bonus values)
	/// 
	/// Total value is HardCapped to 99%
	/// ItemBonus is used for item (item capped)
	/// BuffBonusCategory1 is used for buffs (buff Capped)
	/// BuffBonusCategory2 spec/short buff (buff Capped)
	/// BuffBonusCategory3 is used for debuffs (debuff Capped to 0%, Full Effect on Buff, Half Effect on remaining)
	/// AbilityBonus is used for Ability (Harp Capped)
	/// BuffBonusCategory4 for special effect capped to HardCap
	/// BuffBonusMultCategory1 unused
	/// </summary>	
	public abstract class GenericPercentBonusCalculator : GenericPercentCalculator
	{
		/// <summary>
		/// Don't Allow for Negative Values
		/// </summary>
		/// <param name="living"></param>
		/// <returns></returns>
		public override int DebuffCap(GameLiving living)
		{
			return 0;
		}
	}
}
