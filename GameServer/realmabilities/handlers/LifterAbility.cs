using System;
using DOL.Database2;
using DOL.GS.PropertyCalc;

namespace DOL.GS.RealmAbilities
{
	public class LifterAbility : RAPropertyEnhancer
	{
		public LifterAbility(DBAbility dba, int level)
			: base(dba, level, eProperty.Undefined)
		{
		}

		protected override string ValueUnit { get { return "%"; } }

		public override int GetAmountForLevel(int level)
		{
			switch (level)
			{
				case 1: return 10;
				case 2: return 25;
				case 3: return 45;
				case 4: return 70;
				case 5: return 95;
				default: return 0;
			}
		}
	}
}