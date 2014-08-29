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

				// no spec AF buff cap (capped at *12 to prevent bugs, max heretic AF spec buff)
				af = Math.Min((int)(living.Level * 12), living.SpecBuffBonusCategory[property]);
				// debuff
				af -= living.DebuffCategory[property];
				// TrialsOfAtlantis af bonus
				af += Math.Min(living.Level, living.ItemBonus[property]);
				// uncapped category
				af += living.BuffBonusCategory4[property];

				return af;
			}
			else if (living is GameKeepDoor || living is GameKeepComponent)
			{
				GameKeepComponent component = null;
				if (living is GameKeepDoor)
					component = (living as GameKeepDoor).Component;
				if (living is GameKeepComponent)
					component = living as GameKeepComponent;

				int amount = component.AbstractKeep.BaseLevel;
				if (component.AbstractKeep is GameKeep)
					return amount;
				else return amount / 2;
			}
			else
			{
				return (int)((1 + (living.Level / 170.0)) * (living.Level << 1) * 4.67)
				+ living.BaseBuffBonusCategory[property]
				+ living.SpecBuffBonusCategory[property]
				- living.DebuffCategory[property]
				+ living.BuffBonusCategory4[property];
			}
		}
	}
}
