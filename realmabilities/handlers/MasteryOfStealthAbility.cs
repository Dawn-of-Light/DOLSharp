using System;
using System.Collections;
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

		public static double GetSpeedBonusForLevel(int level)
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

		public override System.Collections.IList DelveInfo
		{
			get
			{
				ArrayList list = new ArrayList();
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