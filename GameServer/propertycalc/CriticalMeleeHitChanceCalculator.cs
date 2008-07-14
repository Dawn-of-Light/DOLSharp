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
    using DOL.GS.Effects;

    namespace DOL.GS.PropertyCalc
    {
       /// <summary>
       /// The critical hit chance calculator. Returns 0 .. 100 chance.
       ///
       /// BuffBonusCategory1 unused
       /// BuffBonusCategory2 unused
       /// BuffBonusCategory3 unused
       /// BuffBonusCategory4 for uncapped realm ability bonus
       /// BuffBonusMultCategory1 unused
       ///
       /// Crit propability is capped to 50% except for berserk
       /// </summary>
       [PropertyCalculator(eProperty.CriticalMeleeHitChance)]
       public class CriticalMeleeHitChanceCalculator : PropertyCalculator
       {
          public CriticalMeleeHitChanceCalculator() { }

          public override int CalcValue(GameLiving living, eProperty property)
          {
             // no berserk for ranged weapons
             IGameEffect berserk = living.EffectList.GetOfType(typeof(BerserkEffect));
             if (berserk != null)
             {
                return 100;
             }

             // base 10% chance of critical for all with melee weapons plus ra bonus
             int chance = 10 + living.BuffBonusCategory4[(int)property] + living.AbilityBonus[(int)property];

             if (living is GameNPC && (living as GameNPC).Brain is AI.Brain.IControlledBrain)
             {
                GamePlayer player = ((living as GameNPC).Brain as AI.Brain.IControlledBrain).GetPlayerOwner();
                if (player != null)
                {
                   RealmAbilities.WildMinionAbility ab = player.GetAbility(typeof(RealmAbilities.WildMinionAbility)) as RealmAbilities.WildMinionAbility;
                   chance += ab.Amount;
                   if (living is NecromancerPet)
                   {
                      RealmAbilities.MasteryOfPain mop = player.GetAbility(typeof(RealmAbilities.MasteryOfPain)) as RealmAbilities.MasteryOfPain;
                      chance += mop.Amount;
                   }
                }
             }

             //50% hardcap
             return Math.Min(chance, 50);
          }
       }
    }
