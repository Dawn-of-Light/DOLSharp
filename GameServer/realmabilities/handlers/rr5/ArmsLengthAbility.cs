using System;
using DOL.Database;
using DOL.GS.Effects;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Arms Length Realm Ability
	/// </summary>
	public class ArmsLengthAbility : RR5RealmAbility
	{
		public ArmsLengthAbility(DBAbility dba, int level) : base(dba, level) { }

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
				SendCasterSpellEffectAndCastMessage(player, 7068, true);
				ArmsLengthEffect effect = new ArmsLengthEffect();
				effect.Start(player);
			}
			DisableSkill(living);
		}

		public override int GetReUseDelay(int level)
		{
			return 600;
		}

		public override void AddEffectsInfo(System.Collections.IList list)
		{
			list.Add("10 second unbreakable burst of extreme speed.");
			list.Add("");
			list.Add("Target: Self");
			list.Add("Duration: 10 sec");
			list.Add("Casting time: instant");
		}

	}
}