using System;
using DOL.Database2;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.GS.SkillHandler;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Determination
	/// </summary>
	public class DeterminationAbility : RAPropertyEnhancer
	{
		public static eProperty[] properties = new eProperty[] 
{
	eProperty.MesmerizeDuration,
	eProperty.StunDuration,
	eProperty.SpeedDecreaseDuration,
};
		public DeterminationAbility(DBAbility dba, int level) : base(dba, level, properties) { }

		protected override string ValueUnit { get { return "%"; } }

		public override int GetAmountForLevel(int level)
		{
			if (level < 1) return 0;
			int amount = 0;
			if (level >= 1)
				amount += 4;
			if (level >= 2)
				amount += 4;
			if (level >= 3)
				amount += 12;
			if (level >= 4)
				amount += 18;
			if (level >= 5)
				amount += 17;
			return amount;
		}
	}
}