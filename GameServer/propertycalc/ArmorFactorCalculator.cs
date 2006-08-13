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

using DOL.GS.Keeps;

namespace DOL.GS.PropertyCalc
{
	/// <summary>
	/// The Armor Factor calculator
	///
	/// BuffBonusCategory1 is used for base buffs directly in player.GetArmorAF because it must be capped by item AF cap
	/// BuffBonusCategory2 is used for spec buffs, level*1.875 cap for players
	/// BuffBonusCategory3 is used for debuff, uncapped
	/// BuffBonusCategory4 is used for buffs, uncapped
	/// BuffBonusMultCategory1 unused
	/// ItemBonus is used for players TOA bonuse, living.Level cap
	/// </summary>
	[PropertyCalculator(eProperty.ArmorFactor)]
	public class ArmorFactorCalculator : PropertyCalculator
	{
		public override int CalcValue(GameLiving living, eProperty property)
		{
			if (living is GamePlayer)
			{
				int af;

				// 1.5*1.25 spec line buff cap
				af = Math.Min((int)(living.Level * 1.875), living.BuffBonusCategory2[(int)property]);
				// debuff
				af -= living.BuffBonusCategory3[(int)property];
				// ToA af bonus
				af += Math.Min(living.Level, living.ItemBonus[(int)property]);
				// uncapped category
				af += living.BuffBonusCategory4[(int)property];

				return af;
			}
			else if (living is GameKeepDoor)
			{
				GameKeepDoor keepdoor = living as GameKeepDoor;
				return 2000;
			}
			else
			{
				return (int)( (1+(living.Level/170.0))*(living.Level<<1)*4.67 )
				+living.BuffBonusCategory2[(int)property]
				-living.BuffBonusCategory3[(int)property]
				+living.BuffBonusCategory4[(int)property];
			}
		}
	}
}
