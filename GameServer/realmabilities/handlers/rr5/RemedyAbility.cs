using System;
using System.Collections.Generic;
using DOL.Database;
using DOL.GS.Effects;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Remedy Realm Ability
	/// </summary>
	public class RemedyAbility : RR5RealmAbility
	{
		public RemedyAbility(DBAbility dba, int level) : base(dba, level) { }

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
				SendCasterSpellEffectAndCastMessage(player, 7060, true);
				RemedyEffect effect = new RemedyEffect();
				effect.Start(player);
			}
			DisableSkill(living);
		}

		public override int GetReUseDelay(int level)
		{
			return 300;
		}

		public override void AddEffectsInfo(IList<string> list)
		{
			list.Add("Gives you immunity to weapon poisons for 1 minute. This spell wont purge already received poisons!");
			list.Add("This spell costs 10% of your HP. These will be regained by the end of the effect.");
			list.Add("");
			list.Add("Target: Self");
			list.Add("Duration: 60 sec");
			list.Add("Casting time: instant");
		}

	}
}