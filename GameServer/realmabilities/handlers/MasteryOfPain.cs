using DOL.Database;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Mastery of Pain ability
	/// </summary>
	public class MasteryOfPain : RAPropertyEnhancer
	{
		public MasteryOfPain(DBAbility dba, int level)
			: base(dba, level, eProperty.CriticalMeleeHitChance)
		{
		}

		protected override string ValueUnit { get { return "%"; } }

		public override int GetAmountForLevel(int level)
		{
			if (level < 1) return 0;
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

	/// <summary>
	/// Mastery of Parry ability
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