using System;
namespace DOL.GS.PropertyCalc
{
	[PropertyCalculator(eProperty.WeaponSkill)]
	public class WeaponSkillPercentCalculator : PropertyCalculator
	{
		public override int CalcValue(GameLiving living, eProperty property)
		{
			double percent = 100
			+ living.BuffBonusCategory1[(int)property] // enchance the weaponskill
			- living.BuffBonusCategory2[(int)property] // enchance the weaponskill
				//hotfix for poisons where both debuff components have same value
			- (int)(living.BuffBonusCategory3[(int)property] / 5.4) // reduce
		   + living.ItemBonus[(int)property];
			return (int)Math.Max(1, percent);
		}
	}
}
