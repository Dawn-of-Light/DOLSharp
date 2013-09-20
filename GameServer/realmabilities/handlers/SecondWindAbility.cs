using System;
using System.Collections.Generic;
using DOL.Database;
using DOL.Language;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Second Wind, restores 100% endu
	/// </summary>
	public class SecondWindAbility : TimedRealmAbility
	{
		public SecondWindAbility(DBAbility dba, int level) : base(dba, level) { }

		/// <summary>
		/// Action
		/// </summary>
		/// <param name="living"></param>
		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

			int regged = living.ChangeEndurance(living, GameLiving.eEnduranceChangeType.Spell, living.MaxEndurance);

			SendCasterSpellEffectAndCastMessage(living, 7003, regged > 0);

			if (regged > 0) DisableSkill(living);
		}

		public override int GetReUseDelay(int level)
		{
			if(ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
			{
				switch (level)
				{
					case 1: return 900;
					case 2: return 600;
					case 3: return 300;
					case 4: return 210;
					case 5: return 120;
				}
			}
			else
			{
				switch (level)
				{
					case 1: return 900;
					case 2: return 300;
					case 3: return 120;
				}
			}
			return 900;
		}

        public override void AddEffectsInfo(IList<string> list)
        {
        	//TODO Translate
        	if(ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
        	{
	            list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "SecondWindAbility.AddEffectsInfo.Info1"));
	            list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "SecondWindAbility.AddEffectsInfo.Info4"));
	            list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "SecondWindAbility.AddEffectsInfo.Info5"));
        	}
        	else
        	{
	            list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "SecondWindAbility.AddEffectsInfo.Info1"));
	            list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "SecondWindAbility.AddEffectsInfo.Info4"));
	            list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "SecondWindAbility.AddEffectsInfo.Info5"));
        	}
        }

	}
}