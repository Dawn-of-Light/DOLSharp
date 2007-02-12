using System;
using DOL.Database;
using DOL.GS.Effects;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Mastery of Concentration RA
	/// </summary>
	public class ShieldOfImmunityAbility : RR5RealmAbility
	{
		public ShieldOfImmunityAbility(DBAbility dba, int level) : base(dba, level) { }

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
				SendCasterSpellEffectAndCastMessage(player, 7048, true);
				ShieldOfImmunityEffect effect = new ShieldOfImmunityEffect();
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
			list.Add("Shield that absorbs 90% melee/archer damage for 20 seconds.");
			list.Add("");
			list.Add("Target: Self");
			list.Add("Duration: 20 sec");
			list.Add("Casting time: instant");
		}

	}
}