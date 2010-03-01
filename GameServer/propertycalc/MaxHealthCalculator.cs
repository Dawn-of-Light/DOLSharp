using System;

using DOL.GS.Keeps;

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
				int buffBonus = living.BaseBuffBonusCategory[(int)property];
				if (buffBonus < 0) buffBonus = (int)((1 + (buffBonus / -100.0)) * hpBase)-hpBase;
				int itemBonus = living.ItemBonus[(int)property];
				int cap = Math.Max(player.Level * 4, 20) + // at least 20
						  Math.Min(living.ItemBonus[(int)eProperty.MaxHealthCapBonus], player.Level * 4);	
				itemBonus = Math.Min(itemBonus, cap);
                if (player.HasAbility(Abilities.ScarsOfBattle) && player.Level >= 40)
                {
                    int levelbonus = Math.Min(player.Level - 40, 10);
                    hpBase = (int)(hpBase * (100 + levelbonus) * 0.01);
                }
				int abilityBonus = living.AbilityBonus[(int)property];

				return Math.Max(hpBase + itemBonus + buffBonus + abilityBonus, 1); // at least 1
			}
			else if ( living is GameKeepComponent )
			{
				GameKeepComponent keepcomp = living as GameKeepComponent;
				return (keepcomp.Keep.EffectiveLevel(keepcomp.Keep.Level) + 1) * keepcomp.Keep.BaseLevel * 200;
			}
			else if ( living is GameKeepDoor )
			{
				GameKeepDoor keepdoor = living as GameKeepDoor;

				if (keepdoor.Component != null && keepdoor.Component.Keep != null)
				{
					return (keepdoor.Component.Keep.EffectiveLevel(keepdoor.Component.Keep.Level) + 1) * keepdoor.Component.Keep.BaseLevel * 200;
				}

				return 0;

				//todo : use material too to calculate maxhealth
			}
			else if (living is GameNPC)
			{
				int hp = 0;

				if (living.Level<10)
				{
					hp = living.Level * 20 + 20 + living.BaseBuffBonusCategory[(int)property];	// default
				}
				else
				{
					// approx to original formula, thx to mathematica :)
					hp = (int)(50 + 11*living.Level + 0.548331 * living.Level * living.Level) + living.BaseBuffBonusCategory[(int)property];
					if (living.Level < 25)
						hp += 20;
				}

				int basecon = (living as GameNPC).Constitution;
				int conmod = 20; // at level 50 +75 con ~= +300 hit points

				// first adjust hitpoints based on base CON

				if (basecon != ServerProperties.Properties.GAMENPC_BASE_CON)
				{
					hp = Math.Max(1, hp + ((basecon - ServerProperties.Properties.GAMENPC_BASE_CON) * ServerProperties.Properties.GAMENPC_HP_GAIN_PER_CON));
				}

				// Now adjust for buffs

				// adjust hit points based on constitution difference from base con
				// modified from http://www.btinternet.com/~challand/hp_calculator.htm
				int conhp = hp + (conmod * living.Level * (living.GetModified(eProperty.Constitution) - basecon) / 250);

				// 50% buff / debuff cap
				if (conhp > hp * 1.5)
					conhp = (int)(hp * 1.5);
				else if (conhp < hp / 2)
					conhp = hp / 2;

				return conhp;
			}
            else
            {
                if (living.Level < 10)
                {
                    return living.Level * 20 + 20 + living.BaseBuffBonusCategory[(int)property];	// default
                }
                else
                {
                    // approx to original formula, thx to mathematica :)
                    int hp = (int)(50 + 11 * living.Level + 0.548331 * living.Level * living.Level) + living.BaseBuffBonusCategory[(int)property];
                    if (living.Level < 25)
                    {
                        hp += 20;
                    }
                    return hp;
                }
            }
		}

        /// <summary>
        /// Returns the hits cap for this living.
        /// </summary>
        /// <param name="living">The living the cap is to be determined for.</param>
        /// <returns></returns>
        public static int GetItemBonusCap(GameLiving living)
        {
            if (living == null) return 0;
            return living.Level * 4;
        }

        /// <summary>
        /// Returns the hits cap increase for the this living.
        /// </summary>
        /// <param name="living">The living the cap increase is to be determined for.</param>
        /// <returns></returns>
        public static int GetItemBonusCapIncrease(GameLiving living)
        {
            if (living == null) return 0;
            int itemBonusCapIncreaseCap = GetItemBonusCapIncreaseCap(living);
            int itemBonusCapIncrease = living.ItemBonus[(int)(eProperty.MaxHealthCapBonus)];
            return Math.Min(itemBonusCapIncrease, itemBonusCapIncreaseCap);
        }

        /// <summary>
        /// Returns the cap for hits cap increase for this living.
        /// </summary>
        /// <param name="living">The living the value is to be determined for.</param>
        /// <returns>The cap increase cap for this living.</returns>
        public static int GetItemBonusCapIncreaseCap(GameLiving living)
        {
            if (living == null) return 0;
            return living.Level * 4;
        }
	}
}
