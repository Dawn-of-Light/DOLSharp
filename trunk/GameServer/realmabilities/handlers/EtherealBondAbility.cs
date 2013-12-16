using System;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.GS.SkillHandler;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Ethereal Bond
	/// </summary>
	public class EtherealBondAbility : RAPropertyEnhancer
	{
		public EtherealBondAbility(DBAbility dba, int level) : base(dba, level, eProperty.MaxMana) { }

		public override int GetAmountForLevel(int level)
		{
			if (level < 1) return 0;
			if (ServerProperties.Properties.USE_NEW_PASSIVES_RAS_SCALING)
			{
				switch (level)
				{
						case 1: return 15;
						case 2: return 25;
						case 3: return 40;
						case 4: return 55;
						case 5: return 75;
						case 6: return 100;
						case 7: return 130;
						case 8: return 165;
						case 9: return 200;
						default: return 200;
				}
			}
			else
			{
				switch (level)
				{
						case 1: return 20;
						case 2: return 50;
						case 3: return 80;
						case 4: return 130;
						case 5: return 200;
						default: return 0;
				}
			}
		}
	}
}