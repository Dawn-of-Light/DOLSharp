using System;
using DOL.Database;
using DOL.GS.Effects;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Sputins Legacy Realm Ability
	/// </summary>
	public class SputinsLegacyAbility : RR5RealmAbility
	{
		public SputinsLegacyAbility(DBAbility dba, int level) : base(dba, level) { }

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
				SendCasterSpellEffectAndCastMessage(player, 7070, true);
				SputinsLegacyEffect effect = new SputinsLegacyEffect();
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
			list.Add("The Healer won't die for 30sec.");
			list.Add("");
			list.Add("Target: Self");
			list.Add("Duration: 30 sec");
			list.Add("Casting time: instant");
		}

	}
}