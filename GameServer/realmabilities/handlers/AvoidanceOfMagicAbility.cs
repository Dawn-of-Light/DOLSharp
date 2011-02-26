using System;
using DOL.Database;
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
			if (ServerProperties.Properties.USE_NEW_PASSIVES_RAS_SCALING)
			{
				switch (level)
				{
						case 1: return 2;
						case 2: return 3;
						case 3: return 5;
						case 4: return 7;
						case 5: return 10;
						case 6: return 12;
						case 7: return 15;
						case 8: return 17;
						case 9: return 20;
						default: return 20;
				}
			}
			else
			{
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
			if (ServerProperties.Properties.USE_NEW_PASSIVES_RAS_SCALING)
			{
				switch (level)
				{
						case 1: return 2;
						case 2: return 4;
						case 3: return 6;
						case 4: return 9;
						case 5: return 12;
						case 6: return 16;
						case 7: return 20;
						case 8: return 25;
						case 9: return 30;
						default: return 30;
				}
			}
			else
			{
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
}