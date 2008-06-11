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
	/// The Focus Level calculator
	///
	/// BuffBonusCategory1 is used for buffs, uncapped
	/// BuffBonusCategory2 unused
	/// BuffBonusCategory3 unused
	/// BuffBonusCategory4 unused
	/// BuffBonusMultCategory1 unused
	/// </summary>
	[PropertyCalculator(eProperty.Focus_Darkness, eProperty.Focus_Matter)]
	[PropertyCalculator(eProperty.Focus_Mind, eProperty.Focus_Arboreal)]
	[PropertyCalculator(eProperty.Focus_EtherealShriek, eProperty.Focus_Witchcraft)]
	public class FocusLevelCalculator : PropertyCalculator
	{
		public FocusLevelCalculator() { }

		public override int CalcValue(GameLiving living, eProperty property)
		{
			if (living is GamePlayer)
			{
				int itemBonus = living.ItemBonus[(int)property];
				int focusLevel = living.BaseBuffBonusCategory[(int)property];
				if (SkillBase.CheckPropertyType(property, ePropertyType.Focus)
				 && ((GamePlayer)living).CharacterClass.ClassType == eClassType.ListCaster)
				{
					focusLevel += living.BaseBuffBonusCategory[(int)eProperty.AllFocusLevels];
					itemBonus = Math.Max(itemBonus, living.ItemBonus[(int)eProperty.AllFocusLevels]);
				}
				return focusLevel + Math.Min(50, itemBonus);
			}
			else
			{
				// TODO other living types
			}
			return 0;
		}
	}
}
