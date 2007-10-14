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
	/// The Character Stat calculator
	/// 
	/// BuffBonusCategory1 is used for all single stat buffs
	/// BuffBonusCategory2 is used for all dual stat buffs
	/// BuffBonusCategory3 is used for all debuffs (positive values expected here)
	/// BuffBonusCategory4 is used for all other uncapped modifications
	///                    category 4 kicks in at last
	/// BuffBonusMultCategory1 used after all buffs/debuffs
	/// </summary>
	[PropertyCalculator(eProperty.Stat_First, eProperty.Stat_Last)]
	public class StatCalculator : PropertyCalculator
	{
		public StatCalculator() { }

		public override int CalcValue(GameLiving living, eProperty property)
		{
			if (living is GamePlayer)
			{
				GamePlayer player = living as GamePlayer;
				int baseStat = player.GetBaseStat((eStat)property);
				int itemBonus = living.ItemBonus[(int)property];
				int abilityBonus = living.AbilityBonus[(int)property];
				int singleStatBonus = living.BuffBonusCategory1[(int)property];
				int dualStatBonus = living.BuffBonusCategory2[(int)property];
				int debuff = living.BuffBonusCategory3[(int)property];
				int acuityBonus = 0;

				int itemcap = (int)(living.Level * 1.5);
				int singlecap = (int)(living.Level * 1.25);
				int dualcap = (int)(living.Level * 1.25 * 1.5);
				int overcapMax = living.Level / 2 + 1;
				int overcapbonus = living.ItemBonus[(int)(eProperty.StatCapBonus_First - eProperty.Stat_First + property)];

				if (property == (eProperty)player.CharacterClass.ManaStat)
				{
					overcapbonus += living.ItemBonus[(int)eProperty.AcuCapBonus];
					itemBonus += living.ItemBonus[(int)eProperty.Acuity];
					abilityBonus += living.AbilityBonus[(int)eProperty.Acuity];
					if (player.CharacterClass.ClassType == eClassType.ListCaster)
						acuityBonus = living.BuffBonusCategory1[(int)eProperty.Acuity];
				}

				overcapbonus = Math.Min(overcapbonus, overcapMax);
				if (itemBonus > itemcap + overcapbonus)
				{
					itemBonus = itemcap + overcapbonus;
				}
				if (singleStatBonus > singlecap)
				{
					singleStatBonus = singlecap;
				}
				if (dualStatBonus > dualcap)
				{
					dualStatBonus = dualcap;
				}
				if (debuff < 0)
				{
					debuff = -debuff;
				}
				int stat = singleStatBonus + dualStatBonus - debuff + acuityBonus + living.BuffBonusCategory4[(int)property];
				//50% debuff effectiveness for item and base stats
				if (stat < 0)
					stat >>= 1;
				stat += baseStat + itemBonus + abilityBonus;
				stat = (int)(stat * living.BuffBonusMultCategory1.Get((int)property));

				if (property == eProperty.Constitution && player.TotalConstitutionLostAtDeath > 0)
				{
					stat -= player.TotalConstitutionLostAtDeath;
				}

				if (stat < 1) stat = 1;	// at least 1

				return stat;
			}
			else
			{
                GameNPC npc = living as GameNPC;
                int baseStat = npc.GetBaseStat((eStat)property);
                int itemBonus = living.ItemBonus[(int)property];
                int abilityBonus = living.AbilityBonus[(int)property];
                int singleStatBonus = living.BuffBonusCategory1[(int)property];
                int dualStatBonus = living.BuffBonusCategory2[(int)property];
                int debuff = living.BuffBonusCategory3[(int)property];
                int acuityBonus = 0;

                int itemcap = (int)(living.Level * 1.5);
                int singlecap = (int)(living.Level * 1.25);
                int dualcap = (int)(living.Level * 1.25 * 1.5);
                int overcapMax = living.Level / 2 + 1;
                int overcapbonus = living.ItemBonus[(int)(eProperty.StatCapBonus_First - eProperty.Stat_First + property)];

                if (property == (eProperty)npc.Intelligence)
                {
                    overcapbonus += living.ItemBonus[(int)eProperty.AcuCapBonus];
                    itemBonus += living.ItemBonus[(int)eProperty.Acuity];
                    abilityBonus += living.AbilityBonus[(int)eProperty.Acuity];
                    //if (player.CharacterClass.ClassType == eClassType.ListCaster)
                       // acuityBonus = living.BuffBonusCategory1[(int)eProperty.Acuity];
                }

                overcapbonus = Math.Min(overcapbonus, overcapMax);
                if (itemBonus > itemcap + overcapbonus)
                {
                    itemBonus = itemcap + overcapbonus;
                }
                if (singleStatBonus > singlecap)
                {
                    singleStatBonus = singlecap;
                }
                if (dualStatBonus > dualcap)
                {
                    dualStatBonus = dualcap;
                }
                if (debuff < 0)
                {
                    debuff = -debuff;
                }
                int stat = singleStatBonus + dualStatBonus - debuff + acuityBonus + living.BuffBonusCategory4[(int)property];
                //50% debuff effectiveness for item and base stats
                if (stat < 0)
                    stat >>= 1;
                stat += baseStat + itemBonus + abilityBonus;
                stat = (int)(stat * living.BuffBonusMultCategory1.Get((int)property));

                if (stat < 1) stat = 1;	// at least 1

                return stat;
            }
		}
	}
}
