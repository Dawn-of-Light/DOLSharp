using System;
using DOL.Database2;

namespace DOL.GS.RealmAbilities
{
	public class WildMinionAbility : RAPropertyEnhancer
	{
		public WildMinionAbility(DBAbility dba, int level)
			: base(dba, level, eProperty.Undefined)
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
}