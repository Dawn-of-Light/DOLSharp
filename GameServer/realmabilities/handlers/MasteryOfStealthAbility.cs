using System;
using System.Collections;
using DOL.Database2;

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
				case 1: return 75;
				case 2: return 175;
				case 3: return 300;
				case 4: return 450;
				case 5: return 625;
				default: return 75;
			}
		}

		public static double GetSpeedBonusForLevel(int level)
		{
			switch (level)
			{
				case 1: return 0.05;
				case 2: return 0.10;
				case 3: return 0.15;
				case 4: return 0.20;
				case 5: return 0.25;
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