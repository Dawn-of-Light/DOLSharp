using System;
using System.Collections;
using System.Collections.Generic;
using DOL.Database;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Mastery of Stealth RA
	/// </summary>
	public class MasteryOfStealthAbility : RAPropertyEnhancer
	{
		public MasteryOfStealthAbility(DBAbility dba, int level)
			: base(dba, level, eProperty.Undefined)
		{
		}

		public override int GetAmountForLevel(int level)
		{
			if (level < 1) return 0;
			if (ServerProperties.Properties.USE_NEW_PASSIVES_RAS_SCALING)
			{
				switch (level)
				{
						case 1: return 75;
						case 2: return 125;
						case 3: return 175;
						case 4: return 235;
						case 5: return 300;
						case 6: return 375;
						case 7: return 450;
						case 8: return 535;
						case 9: return 625;
						default: return 625;
				}
			}
			else
			{
				switch (level)
				{
						case 1: return 50;
						case 2: return 100;
						case 3: return 200;
						case 4: return 325;
						case 5: return 475;
						default: return 50;
				}
			}
		}

		public static double GetSpeedBonusForLevel(int level)
		{
			if (ServerProperties.Properties.USE_NEW_PASSIVES_RAS_SCALING)
			{
				switch (level)
				{
						case 1: return 0.10;
						case 2: return 0.15;
						case 3: return 0.20;
						case 4: return 0.25;
						case 5: return 0.30;
						case 6: return 0.35;
						case 7: return 0.40;
						case 8: return 0.45;
						case 9: return 0.50;
						default: return 0.50;
				}
			}
			else
			{
				switch (level)
				{
						case 1: return 0.10;
						case 2: return 0.20;
						case 3: return 0.30;
						case 4: return 0.40;
						case 5: return 0.50;
						default: return 0;
				}
			}
		}

		public override IList<string> DelveInfo
		{
			get
			{
				var list = new List<string>();
				list.Add(m_description);
				list.Add("");
				for (int i = 1; i <= MaxLevel; i++)
				{
					list.Add("Level " + i + ": Amount: " + Level * 5 + "% / " + GetAmountForLevel(i));
				}
				return list;
			}
		}
	}
}