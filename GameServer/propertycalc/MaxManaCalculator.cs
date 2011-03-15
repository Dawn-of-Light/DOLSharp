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
	/// The Power Pool calculator
	/// 
	/// BuffBonusCategory1 unused
	/// BuffBonusCategory2 unused
	/// BuffBonusCategory3 unused
	/// BuffBonusCategory4 unused
	/// BuffBonusMultCategory1 unused
	/// </summary>
	[PropertyCalculator(eProperty.MaxMana)]
	public class MaxManaCalculator : PropertyCalculator
	{
		public MaxManaCalculator() {}

		public override int CalcValue(GameLiving living, eProperty property) 
		{
			if (living is GamePlayer) 
			{
				GamePlayer player = living as GamePlayer;
				eStat manaStat = player.CharacterClass.ManaStat;

				if (player.CharacterClass.ManaStat == eStat.UNDEFINED)
				{
					//Special handling for Vampiirs:
					/* There is no stat that affects the Vampiir's power pool or the damage done by its power based spells.
					 * The Vampiir is not a focus based class like, say, an Enchanter.
					 * The Vampiir is a lot more cut and dried than the typical casting class. 
					 * EDIT, 12/13/04 - I was told today that this answer is not entirely accurate.
					 * While there is no stat that affects the damage dealt (in the way that intelligence or piety affects how much damage a more traditional caster can do),
					 * the Vampiir's power pool capacity is intended to be increased as the Vampiir's strength increases.
					 * 
					 * This means that strength ONLY affects a Vampiir's mana pool
					 */
					if (player.CharacterClass.ID == (int)eCharacterClass.Vampiir)
					{
						manaStat = eStat.STR;
					}
					else if (player.Champion && player.ChampionLevel > 0)
					{
						player.CalculateMaxMana(player.Level, 0);
					}
					else
					{
						return 0;
					}
				}

				int manaBase = player.CalculateMaxMana(player.Level, player.GetModified((eProperty)manaStat));
				int itemBonus = living.ItemBonus[(int)property];
				int poolBonus = living.ItemBonus[(int)eProperty.PowerPool];
				int abilityBonus = living.AbilityBonus[(int)property]; 

				int itemCap = player.Level / 2 + 1;
				int poolCap = player.Level / 2;
				itemCap = itemCap + Math.Min(player.ItemBonus[(int)eProperty.PowerPoolCapBonus], itemCap);
				poolCap = poolCap + Math.Min(player.ItemBonus[(int)eProperty.PowerPoolCapBonus], player.Level);


				if (itemBonus > itemCap) {
					itemBonus = itemCap;
				}
				if (poolBonus > poolCap)
					poolBonus = poolCap;

				//Q: What exactly does the power pool % increase do?Does it increase the amount of power my cleric
				//can generate (like having higher piety)? Or, like the dex cap increase, do I have to put spellcraft points into power to make it worth anything?
				//A: I’m better off quoting Balance Boy directly here: ” Power pool is affected by
				//your acuity stat, +power bonus, the Ethereal Bond Realm ability, and your level.
				//The resulting power pool is adjusted by your power pool % increase bonus.
				return (int)(manaBase + itemBonus + abilityBonus + (manaBase + itemBonus + abilityBonus) * poolBonus * 0.01); 
			}
			else 
			{
				return 1000000;	// default
			}
		}
	}
}
