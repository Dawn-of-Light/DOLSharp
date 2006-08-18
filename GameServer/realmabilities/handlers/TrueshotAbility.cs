using System;
using DOL.Database;
using DOL.GS.Effects;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Trueshot RA, grants 50% more range on next archery attack
	/// </summary>
	public class TrueshotAbility : TimedRealmAbility
	{
		public TrueshotAbility(DBAbility dba, int level) : base(dba, level) { }

		/// <summary>
		/// Action
		/// </summary>
		/// <param name="living"></param>
		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;
			GamePlayer player = living as GamePlayer;
			if (player != null)
			{
				TrueshotEffect effect = new TrueshotEffect();
				effect.Start(player);
			}
			DisableSkill(living);
		}

		public override int GetReUseDelay(int level)
		{
			switch (level)
			{
				case 1: return 600;
				case 2: return 180;
				case 3: return 30;
			}
			return 600;
		}
	}
}