using DOL.Database;

namespace DOL.GS.RealmAbilities
{
	public class DualThreatAbility : L3RAPropertyEnhancer
	{
		public DualThreatAbility(DBAbility dba, int level)
			: base(dba, level, new eProperty[] { eProperty.CriticalMeleeHitChance, eProperty.CriticalSpellHitChance, eProperty.CriticalHealHitChance})
		{
		}

		protected override string ValueUnit { get { return "%"; } }

		public override int GetAmountForLevel(int level)
		{
			if(ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
			{
				switch (level)
				{
					case 1: return 5;
					case 2: return 7;
					case 3: return 10;
					case 4: return 15;
					case 5: return 20;
					default: return 0;
				}
			}
			else
			{
				switch (level)
				{
					case 1: return 5;
					case 2: return 10;
					case 3: return 20;
					default: return 0;
				}
			}
		}
	}

	public class ReflexAttackAbility : L3RAPropertyEnhancer
	{
		public ReflexAttackAbility(DBAbility dba, int level)
			: base(dba, level, eProperty.Undefined)
		{
		}

		protected override string ValueUnit { get { return "%"; } }

		public override int GetAmountForLevel(int level)
		{
			if(ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
			{
				switch (level)
				{
					case 1: return 10;
					case 2: return 20;
					case 3: return 30;
					case 4: return 40;
					case 5: return 50;
					default: return 0;
				}
			}
			else
			{
				switch (level)
				{
					case 1: return 5;
					case 2: return 15;
					case 3: return 30;
					default: return 0;
				}
			}
		}
	}

	public class ViperAbility : L3RAPropertyEnhancer
	{
		public ViperAbility(DBAbility dba, int level)
			: base(dba, level, eProperty.Undefined)
		{
		}

		protected override string ValueUnit { get { return "%"; } }

		public override int GetAmountForLevel(int level)
		{
			if(ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
			{
				switch (level)
				{
					case 1: return 25;
					case 2: return 35;
					case 3: return 50;
					case 4: return 75;
					case 5: return 100;
					default: return 0;
				}
			}
			else
			{
				switch (level)
				{
					case 1: return 25;
					case 2: return 50;
					case 3: return 100;
					default: return 0;
				}
			}
		}
	}
}