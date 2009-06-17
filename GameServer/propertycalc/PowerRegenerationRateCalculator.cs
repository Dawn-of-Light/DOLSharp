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
	/// The power regen rate calculator
	/// 
	/// BuffBonusCategory1 is used for all buffs
	/// BuffBonusCategory2 is used for all debuffs (positive values expected here)
	/// BuffBonusCategory3 unused
	/// BuffBonusCategory4 unused
	/// BuffBonusMultCategory1 unused
	/// </summary>
	[PropertyCalculator(eProperty.PowerRegenerationRate)]
	public class PowerRegenerationRateCalculator : PropertyCalculator
	{
		public PowerRegenerationRateCalculator() {}

		public override int CalcValue(GameLiving living, eProperty property) 
		{
			/* PATCH 1.87 COMBAT AND REGENERATION
			  - While in combat, health and power regeneration ticks will happen twice as often.
    		  - Each tick of health and power is now twice as effective.
              - All health and power regeneration aids are now twice as effective.
             */

			double regen = 15; // tested by SAND

			// tolakram - there is no difference per tic between combat and non combat

			if (regen != 0 && ServerProperties.Properties.MANA_REGEN_RATE != 1)
				regen *= ServerProperties.Properties.MANA_REGEN_RATE;

			double decimals = regen - (int)regen;
			if (Util.ChanceDouble(decimals)) 
			{
				regen += 1;	// compensate int rounding error
			}

			int debuff = living.SpecBuffBonusCategory[(int)property];
			if (debuff < 0)
				debuff = -debuff;

			regen += living.BaseBuffBonusCategory[(int)property] + living.ItemBonus[(int)property] - debuff;

			if (regen < 1)
				regen = 1;

			return (int)regen;
		}
	}
}
