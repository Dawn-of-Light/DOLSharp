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
	/// The health regen rate calculator
	/// 
	/// BuffBonusCategory1 is used for all buffs
	/// BuffBonusCategory2 is used for all debuffs (positive values expected here)
	/// BuffBonusCategory3 unused
	/// BuffBonusCategory4 unused
	/// BuffBonusMultCategory1 unused
	/// </summary>
	[PropertyCalculator(eProperty.HealthRegenerationRate)]
	public class HealthRegenerationRateCalculator : PropertyCalculator
	{
		public HealthRegenerationRateCalculator() {}

		/// <summary>
		/// calculates the final property value
		/// </summary>
		/// <param name="living"></param>
		/// <param name="property"></param>
		/// <returns></returns>
		public override int CalcValue(GameLiving living, eProperty property)
		{
			//if (living is GameKeepDoor)
			//	return (int)(living.MaxHealth *0.05);//5% each time for keep door

			if (living.IsDiseased)
				return 0; // no HP regen if diseased

			double regen = living.Level * 0.15 + 3;
			if (living.Level > 15) 
			{
				regen += ((living.Level-15) * 0.2);
			}

			double decimals = regen - (int)regen;
			if (Util.ChanceDouble(decimals)) 
			{
				regen += 1;	// compensate int rounding error
			}
			
			int debuff = living.BuffBonusCategory2[(int)property];
			if (debuff < 0)
				debuff = -debuff;

			regen += living.BuffBonusCategory1[(int)property] - debuff;

			if(living is GameNPC) regen /= 2;

			if (regen < 1)
				regen = 1;

			return (int)regen;
		}
	}
}
