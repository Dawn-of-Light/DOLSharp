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
	/// The Max HP calculator
	///
	/// BuffBonusCategory1 is used for absolute HP buffs
	/// BuffBonusCategory2 unused
	/// BuffBonusCategory3 unused
	/// BuffBonusCategory4 unused
	/// BuffBonusMultCategory1 unused
	/// </summary>
	[PropertyCalculator(eProperty.MaxHealth)]
	public class MaxHealthCalculator : PropertyCalculator
	{
		public override int CalcValue(GameLiving living, eProperty property)
		{
			if (living is GamePlayer)
			{
				GamePlayer player = living as GamePlayer;
				int hpBase = player.CalculateMaxHealth(player.Level, player.GetModified(eProperty.Constitution));
				int buffBonus = living.BuffBonusCategory1[(int)property];
				int itemBonus = living.ItemBonus[(int)property];
				int cap = Math.Max(player.Level * 4, 20) + // at least 20
						  Math.Min(living.ItemBonus[(int)eProperty.MaxHealthCapBonus], player.Level * 4);	
				itemBonus = Math.Min(itemBonus, cap);

				return Math.Max(hpBase + itemBonus + buffBonus, 1); // at least 1
			}
			/*else if ( living is GameKeepComponent )
			{
				GameKeepComponent keepcomp = living as GameKeepComponent;
				return (keepcomp.Keep.Level+1)*30000;
			}
			else if ( living is GameKeepDoor )
			{
				GameKeepDoor keepdoor = living as GameKeepDoor;
				return (keepdoor.Keep.Level+1)*10000;//todo : use material too to calculate maxhealth
			}*/
			else
			{
				if (living.Level<10)
				{
					return living.Level * 20 + 20 + living.BuffBonusCategory1[(int)property];	// default
				}
				else
				{
					// approx to original formula, thx to mathematica :)
					int hp = (int)(50 + 11*living.Level + 0.548331 * living.Level * living.Level) + living.BuffBonusCategory1[(int)property];
					if (living.Level < 25)
					{
						hp += 20;
					}
					return hp;
				}
			}
		}
	}
}
