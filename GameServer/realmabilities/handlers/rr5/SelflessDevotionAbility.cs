using System;
using DOL.Database;
using DOL.GS.Effects;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Mastery of Concentration RA
	/// </summary>
	public class SelflessDevotionAbility : RR5RealmAbility
	{
		public SelflessDevotionAbility(DBAbility dba, int level) : base(dba, level) { }

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
				SendCasterSpellEffectAndCastMessage(player, 7039, true);
				SelflessDevotionEffect effect = new SelflessDevotionEffect();
				effect.Start(player);
			}
			DisableSkill(living);
		}

		public override int GetReUseDelay(int level)
		{
			return 900;
		}

		public override void AddEffectsInfo(System.Collections.IList list)
		{
			list.Add("Triples the effect of the paladin healing chant for 1 minute on all groupmates excluding the Paladin himself.");
			list.Add("");
			list.Add("Target: Self");
			list.Add("Duration: 60 sec");
			list.Add("Casting time: instant");
		}

	}
}