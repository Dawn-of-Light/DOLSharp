using System;
using DOL.Database2;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.GS.SkillHandler;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Concentration - refresh quickcast
	/// </summary>
	public class ConcentrationAbility : TimedRealmAbility
	{
		public ConcentrationAbility(DBAbility dba, int level) : base(dba, level) { }

		/// <summary>
		/// Action
		/// </summary>
		/// <param name="living"></param>
		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

			SendCasterSpellEffectAndCastMessage(living, 7006, true);

			GamePlayer player = living as GamePlayer;
			if (player != null)
			{
				player.TempProperties.setProperty(GamePlayer.QUICK_CAST_CHANGE_TICK, player.CurrentRegion.Time - QuickCastAbilityHandler.DISABLE_DURATION);
			}
			DisableSkill(living);
		}

		public override int GetReUseDelay(int level)
		{
			switch (level)
			{
				case 1: return 15 * 60;
				case 2: return 3 * 60;
				case 3: return 30;
			}
			return 0;
		}

		public override void AddEffectsInfo(System.Collections.IList list)
		{
			list.Add("Level 1: Value: 25%");
			list.Add("Level 2: Value: 60%");
			list.Add("Level 3: Value: 100%");
			list.Add("");
			list.Add("Target: Self");
			list.Add("Casting time: instant");
		}
	}
}