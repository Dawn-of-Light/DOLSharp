using System;
using System.Collections.Generic;
using DOL.Database;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.SkillHandler;
using DOL.Language;

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

		public override void AddEffectsInfo(IList<string> list)
		{
            list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "ConcentrationAbility.AddEffectsInfo.Info1"));
            list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "ConcentrationAbility.AddEffectsInfo.Info2"));
            list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "ConcentrationAbility.AddEffectsInfo.Info3"));
            list.Add("");
            list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "ConcentrationAbility.AddEffectsInfo.Info4"));
            list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "ConcentrationAbility.AddEffectsInfo.Info5"));
		}
	}
}