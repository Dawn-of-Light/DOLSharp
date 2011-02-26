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
			if (level < 1) return 0;
            if (ServerProperties.Properties.USE_NEW_PASSIVES_RAS_SCALING)
            {     
                switch (level)
                {
                    case 1: return 25;
                    case 2: return 50;
                    case 3: return 75;
                    case 4: return 100;
                    case 5: return 150;
                    case 6: return 200;
                    case 7: return 250;
                    case 8: return 325;
                    case 9: return 400;
                    default: return 400;
                }
            }
            else
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
		}

		public override bool CheckRequirement(GamePlayer player)
		{
			return player.Level >= 40;
		}
	}
}