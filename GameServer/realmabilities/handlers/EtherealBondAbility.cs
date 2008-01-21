using System;
using DOL.Database2;
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
			switch (level)
			{
				case 1: return 15;
				case 2: return 40;
				case 3: return 75;
				case 4: return 130;
				case 5: return 200;
				default: return 0;
			}
		}
	}
}