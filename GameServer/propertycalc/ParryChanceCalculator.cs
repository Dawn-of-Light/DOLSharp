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
using DOL.AI.Brain;

namespace DOL.GS.PropertyCalc
{
	/// <summary>
	/// The parry chance calculator. Returns 0 .. 1000 chance.
	/// 
	/// BuffBonusCategory1 unused
	/// BuffBonusCategory2 unused
	/// BuffBonusCategory3 unused
	/// BuffBonusCategory4 unused
	/// BuffBonusMultCategory1 unused
	/// </summary>
	[PropertyCalculator(eProperty.ParryChance)]
	public class ParryChanceCalculator : PropertyCalculator
	{
		public override int CalcValue(GameLiving living, eProperty property)
		{
			
			GamePlayer player = living as GamePlayer;
			if (player != null)
			{
				int buff = player.BaseBuffBonusCategory[(int)property] * 10
				+ player.SpecBuffBonusCategory[(int)property] * 10
				- player.DebuffCategory[(int)property] * 10
				+ player.BuffBonusCategory4[(int)property] * 10
				+ player.AbilityBonus[(int)property] * 10;
				int parrySpec = 0;
				if (player.HasSpecialization(Specs.Parry))
				{					
					parrySpec = (player.Dexterity * 2 - 100) / 4 + (player.GetModifiedSpecLevel(Specs.Parry) - 1) * (10 / 2) + 50;
				}
                if (parrySpec > 500)
                {
                    parrySpec = 500;
                }
				return parrySpec + buff;
			}
			NecromancerPet pet = living as NecromancerPet;
			if (pet != null)
			{
				IControlledBrain brain = pet.Brain as IControlledBrain;
				if (brain != null)
				{
					int buff = pet.BaseBuffBonusCategory[(int)property] * 10
					+ pet.SpecBuffBonusCategory[(int)property] * 10
					- pet.DebuffCategory[(int)property] * 10
					+ pet.BuffBonusCategory4[(int)property] * 10
					+ pet.AbilityBonus[(int)property] * 10
					+ (pet.GetModified(eProperty.Dexterity) * 2 - 100) / 4
					+ pet.ParryChance * 10;
					return buff;
				}
			}			
			GameNPC npc = living as GameNPC;
			if (npc != null)
			{
				return npc.ParryChance * 10;
			}

			return 0;
		}
	}
}
