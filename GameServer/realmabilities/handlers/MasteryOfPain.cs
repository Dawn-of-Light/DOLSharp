using DOL.Database;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Mastery of Pain ability
	/// </summary>
	public class MasteryOfPain : RAPropertyEnhancer
	{
		public MasteryOfPain(DBAbility dba, int level)
			: base(dba, level, new eProperty[] { eProperty.CriticalMeleeHitChance })
		{
		}

		protected override string ValueUnit { get { return "%"; } }

		public override int GetAmountForLevel(int level)
		{
			if (level < 1) return 0;
			if (ServerProperties.Properties.USE_NEW_PASSIVES_RAS_SCALING)
			{
				switch (level)
				{
						case 1: return 3;
						case 2: return 6;
						case 3: return 9;
						case 4: return 13;
						case 5: return 17;
						case 6: return 22;
						case 7: return 27;
						case 8: return 33;
						case 9: return 39;
						default: return 39;
				}
			}
			else
			{
				switch (level)
				{
						case 1: return 3;
						case 2: return 9;
						case 3: return 17;
						case 4: return 27;
						case 5: return 39;
						default: return 39;
				}
			}
		}
	}

	/// <summary>
	/// Mastery of Parry ability
	/// </summary>
	public class MasteryOfParrying : RAPropertyEnhancer
	{
		public MasteryOfParrying(DBAbility dba, int level)
			: base(dba, level, eProperty.ParryChance)
		{
		}

		protected override string ValueUnit { get { return "%"; } }

		public override int GetAmountForLevel(int level)
		{
			if (level < 1) return 0;
			if (ServerProperties.Properties.USE_NEW_PASSIVES_RAS_SCALING)
			{
				switch (level)
				{
						case 1: return 2;
						case 2: return 4;
						case 3: return 6;
						case 4: return 9;
						case 5: return 12;
						case 6: return 15;
						case 7: return 18;
						case 8: return 21;
						case 9: return 25;
						default: return 25;
				}
			}
			else
			{
				switch (level)
				{
						case 1: return 2;
						case 2: return 5;
						case 3: return 10;
						case 4: return 16;
						case 5: return 23;
						default: return 23;
				}
			}
		}
	}

	/// <summary>
	/// Mastery of Blocking ability
	/// </summary>
	public class MasteryOfBlocking : RAPropertyEnhancer
	{
		public MasteryOfBlocking(DBAbility dba, int level)
			: base(dba, level, eProperty.BlockChance)
		{
		}

		protected override string ValueUnit { get { return "%"; } }

		public override int GetAmountForLevel(int level)
		{
			if (level < 1) return 0;
			if (ServerProperties.Properties.USE_NEW_PASSIVES_RAS_SCALING)
			{
				switch (level)
				{
						case 1: return 2;
						case 2: return 4;
						case 3: return 6;
						case 4: return 9;
						case 5: return 12;
						case 6: return 15;
						case 7: return 18;
						case 8: return 21;
						case 9: return 25;
						default: return 25;
				}
			}
			else
			{
				switch (level)
				{
						case 1: return 2;
						case 2: return 5;
						case 3: return 10;
						case 4: return 16;
						case 5: return 23;
						default: return 23;
				}
			}
		}
	}
}
