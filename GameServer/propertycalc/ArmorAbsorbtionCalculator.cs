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
	/// The Armor Absorbtion calculator
	/// 
	/// BuffBonusCategory1 is used for buffs, uncapped
	/// BuffBonusCategory2 unused
	/// BuffBonusCategory3 is used for debuffs
	/// BuffBonusCategory4 unused
	/// BuffBonusMultCategory1 unused
	/// </summary>
	[PropertyCalculator(eProperty.ArmorAbsorbtion)]
	public class ArmorAbsorbtionCalculator : PropertyCalculator
	{
		public override int CalcValue(GameLiving living, eProperty property)
		{
			int abs = living.BuffBonusCategory1[(int)property]
				- living.BuffBonusCategory3[(int)property]
				+ living.ItemBonus[(int)property]
				+ living.AbilityBonus[(int)property];

			if (living is GameNPC)
			{
				if (living.Level >= 30) abs += 27;
				else if (living.Level >= 20) abs += 19;
				else if (living.Level >= 10) abs += 10;

				//abs += (living.GetModified(eProperty.Constitution) 
				//    + living.GetModified(eProperty.Dexterity) - 100) / 400;

				abs += (living.GetModified(eProperty.Constitution)
					+ living.GetModified(eProperty.Dexterity) - 120) / 12;
			}
			return abs;
		}
	}
}
