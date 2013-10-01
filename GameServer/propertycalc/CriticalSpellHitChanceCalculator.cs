using System;

namespace DOL.GS.PropertyCalc
{
	/// <summary>
	/// The critical hit chance calculator. Returns 0 .. 100 chance.
	/// 
	/// BuffBonusCategory1 unused
	/// BuffBonusCategory2 unused
	/// BuffBonusCategory3 unused
	/// BuffBonusCategory4 unused
	/// BuffBonusMultCategory1 unused
	/// AbilityBonus used
	/// </summary>
	[PropertyCalculator(eProperty.CriticalSpellHitChance)]
	public class CriticalSpellHitChanceCalculator : PropertyCalculator
	{
		public CriticalSpellHitChanceCalculator() {}

		public override int CalcValue(GameLiving living, eProperty property) 
		{
			int chance = 0;
			if (living is GamePlayer)
			{
				if ((living as GamePlayer).CharacterClass.ClassType == eClassType.ListCaster)
					chance += 10;
			}
			chance += living.AbilityBonus[(int)property];
			return Math.Min(chance, 50);
		}
	}
}
