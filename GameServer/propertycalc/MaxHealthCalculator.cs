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
				int buffBonus = living.BuffBonusCategory1[(int)property];
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
				return (keepdoor.Component.Keep.EffectiveLevel(keepdoor.Component.Keep.Level) + 1) * keepdoor.Component.Keep.BaseLevel * 200;
				//todo : use material too to calculate maxhealth
			}
			else if (living is GameNPC)
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
                    //Fix for theurg and animist pets
                    if (((GameNPC)living).HealthMultiplicator)
                        return hp / 2;
                    else
					    return hp;
				}
			}
            else
            {
                if (living.Level < 10)
                {
                    return living.Level * 20 + 20 + living.BuffBonusCategory1[(int)property];	// default
                }
                else
                {
                    // approx to original formula, thx to mathematica :)
                    int hp = (int)(50 + 11 * living.Level + 0.548331 * living.Level * living.Level) + living.BuffBonusCategory1[(int)property];
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
