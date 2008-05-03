using System;
using DOL.Database2;
using DOL.GS.PropertyCalc;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Avoidance of Magic RA, reduces magical damage
	/// </summary>
	public class AvoidanceOfMagicAbility : RAPropertyEnhancer
	{
		/// <summary>
		/// The list of properties this RA affects
		/// </summary>
		public static eProperty[] properties = new eProperty[] 
{
	eProperty.Resist_Body,
	eProperty.Resist_Cold,
	eProperty.Resist_Energy,
	eProperty.Resist_Heat,
	eProperty.Resist_Matter,
	eProperty.Resist_Spirit,
};

		public AvoidanceOfMagicAbility(DBAbility dba, int level)
			: base(dba, level, properties)
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

	/// <summary>
	/// Physical Defence RA, reduces melee damage
	/// </summary>
	public class PhysicalDefenceAbility : RAPropertyEnhancer
	{
		/// <summary>
		/// The list of properties this RA affects
		/// </summary>
		public static eProperty[] properties = new eProperty[] 
{
	eProperty.Resist_Crush,
	eProperty.Resist_Slash,
	eProperty.Resist_Thrust,
};

		public PhysicalDefenceAbility(DBAbility dba, int level)
			: base(dba, level, properties)
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
				case 3: return 12;
				case 4: return 19;
				case 5: return 28;
				default: return 28;
			}
		}
	}
}