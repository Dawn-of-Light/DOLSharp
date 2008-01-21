using System;
using DOL.Database2;
using DOL.GS.Effects;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Arms Length Realm Ability
	/// </summary>
	public class DreamweaverAbility : RR5RealmAbility
	{
		public DreamweaverAbility(DBAbility dba, int level) : base(dba, level) { }

		/// <summary>
		/// Action
		/// </summary>
		/// <param></param>
		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

			GamePlayer player = living as GamePlayer;
			if (player != null)
			{
				SendCasterSpellEffectAndCastMessage(player, 7052, true);
				DreamweaverEffect effect = new DreamweaverEffect();
				effect.Start(player);
			}
			DisableSkill(living);
		}

		public override int GetReUseDelay(int level)
		{
			return 420;
		}

		public override void AddEffectsInfo(System.Collections.IList list)
		{
			list.Add("Dreamweaver.");
			list.Add("");
			list.Add("Target: Self");
			list.Add("Duration: 5 min");
			list.Add("Casting time: instant");
		}

	}
}