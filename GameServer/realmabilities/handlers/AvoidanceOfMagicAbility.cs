using System;
using DOL.Database;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Avoidance of Magic RA, reduces magical damage
	/// </summary>
	public class AvoidanceOfMagicAbility : RAPropertyEnhancer
	{
		public const string KEY = "Avoidance of Magic";

		public AvoidanceOfMagicAbility(DBAbility dba, int level)
			: base(dba, level, eProperty.Undefined)
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
				case 4: return 15;
				case 5: return 20;
				default: return 20;
			}
		}
	}
}