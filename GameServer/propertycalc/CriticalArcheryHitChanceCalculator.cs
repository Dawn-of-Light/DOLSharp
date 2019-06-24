using System;
using DOL.AI.Brain;

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
	/// </summary>
	[PropertyCalculator(eProperty.CriticalArcheryHitChance)]
	public class CriticalArcheryHitChanceCalculator : PropertyCalculator
	{
		public CriticalArcheryHitChanceCalculator() {}

		public override int CalcValue(GameLiving living, eProperty property) 
		{
			int chance = living.BuffBonusCategory4[(int)property] + living.AbilityBonus[(int)property];

			if (living is GamePet gamePet)
			{
				if (ServerProperties.Properties.EXPAND_WILD_MINION && gamePet.Brain is IControlledBrain playerBrain
					&& playerBrain.GetPlayerOwner() is GamePlayer player
					&& player.GetAbility<RealmAbilities.WildMinionAbility>() is RealmAbilities.WildMinionAbility ab)
					chance += ab.Amount;
			}
			else // not a pet
				chance += 10;

			return chance;
		}
	}
}
