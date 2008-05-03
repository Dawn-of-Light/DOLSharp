using System;
using System.Collections;
using DOL.Database2;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Falcon's Eye RA
	/// </summary>
	public class FalconsEyeAbility : RAPropertyEnhancer
	{
		public FalconsEyeAbility(DBAbility dba, int level)
			: base(dba, level, eProperty.CriticalArcheryHitChance)
		{
		}

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