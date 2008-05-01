using System;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.GS.SkillHandler;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Toughness
	/// </summary>
	public class ToughnessAbility : RAPropertyEnhancer
	{
		public ToughnessAbility(DBAbility dba, int level) : base(dba, level, eProperty.MaxHealth) { }

		public override int GetAmountForLevel(int level)
		{
			switch (level)
			{
				case 1: return 25;
				case 2: return 75;
				case 3: return 150;
				case 4: return 250;
				case 5: return 400;
				default: return 0;
			}
		}

		public override bool CheckRequirement(GamePlayer player)
		{
			return player.Level >= 40;
		}
	}
}