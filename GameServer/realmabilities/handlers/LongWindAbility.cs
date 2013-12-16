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

        protected override string ValueUnit { get { return "%"; } }

		public override int GetAmountForLevel(int level)
		{
            switch (level)
            {
                case 1: return 20;
                case 2: return 40;
                case 3: return 60;
                case 4: return 80;
                case 5: return 100;
                default: return 0;
            }
        }

		public override int CostForUpgrade(int level)
		{
			switch (level)
			{
				case 0: return 1;
				case 1: return 3;
				case 2: return 6;
				case 3: return 10;
				case 4: return 14;
				default: return 1000;
			}
		}

		public override bool CheckRequirement(GamePlayer player)
		{
			return Level <= 5;
		}

		public override int MaxLevel
		{
			get
			{
				return 5;
			}
		}
	}
}