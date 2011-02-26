using System;
using DOL.Database;
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
            if (ServerProperties.Properties.USE_NEW_PASSIVES_RAS_SCALING)
            {
                if (level < 1) return 0;
                switch (level)
                {
                    case 1: return 10;
                    case 2: return 20;
                    case 3: return 30;
                    case 4: return 40;
                    case 5: return 50;
                    case 6: return 60;
                    case 7: return 70;
                    case 8: return 80;
                    case 9: return 90;
                    default: return 90;
                }
            }
            else
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
}