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
			int chance = living.AbilityBonus[(int)property];

			if (living is GamePlayer player)
			{
				if (player.CharacterClass.ClassType == eClassType.ListCaster)
					chance += 10;
			}
			else if (living is NecromancerPet petNecro)
			{
				if (petNecro.Brain is IControlledBrain brainNecro && brainNecro.GetPlayerOwner() is GamePlayer necro
					&& necro.GetAbility<RealmAbilities.WildPowerAbility>() is RealmAbilities.WildPowerAbility raWP)
					chance += raWP.Amount;
			}
			else if (living is GamePet pet)
			{
				if (ServerProperties.Properties.EXPAND_WILD_MINION
					&& pet.Brain is IControlledBrain brainPet && brainPet.GetPlayerOwner() is GamePlayer playerOwner
					&& playerOwner.GetAbility<RealmAbilities.WildMinionAbility>() is RealmAbilities.WildMinionAbility raWM)
					chance += raWM.Amount;
			}
			
			return Math.Min(chance, 50);
		}
	}
}
