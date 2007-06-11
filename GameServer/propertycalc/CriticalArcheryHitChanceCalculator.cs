using System;

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
			// base 10% chance of critical for all with ranged weapons plus ra bonus
			return 10 + living.BuffBonusCategory4[(int)property] + living.AbilityBonus[(int)property];
		}
	}
}
