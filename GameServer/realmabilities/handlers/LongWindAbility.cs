using System;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.GS.SkillHandler;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Long Wind
	/// </summary>
	public class LongWindAbility : RAPropertyEnhancer
	{
		public LongWindAbility(DBAbility dba, int level) : base(dba, level, eProperty.Undefined) { }

		public const string KEY = "Long Wind";

		public override int GetAmountForLevel(int level)
		{
			return level;
		}
	}
}