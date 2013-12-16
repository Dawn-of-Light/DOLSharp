using System;
using DOL.Database;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Serenity realm ability
	/// </summary>
	public class SerenityAbility : RAPropertyEnhancer
	{
		public SerenityAbility(DBAbility dba, int level) : base(dba, level, eProperty.PowerRegenerationRate) { }

		public override int GetAmountForLevel(int level)
		{
			if (level < 1) return 0;
			switch (level)
			{
				case 1: return 1;
				case 2: return 2;
				case 3: return 3;
				case 4: return 5;
				case 5: return 7;
				default: return 7;
			}
		}

		public override int CostForUpgrade(int level)
		{
			switch (level)
			{
				case 0: return 1;
				case 1: return 3;
				case 2: return 6;
				case 3: return 10;
				case 4: return 14;
				default: return 1000;
			}
		}

		public override bool CheckRequirement(GamePlayer player)
		{
			return Level <= 5;
		}

		public override int MaxLevel
		{
			get
			{
				return 5;
			}
		}
	}
}