using DOL.AI.Brain;
using DOL.GS.ServerProperties;
using DOL.Language;

namespace DOL.GS.Keeps
{
	public class GuardHealer : GameKeepGuard
	{
		protected override CharacterClass GetClass()
		{
			if (ModelRealm == eRealm.Albion) return CharacterClass.Cleric;
			else if (ModelRealm == eRealm.Midgard) return CharacterClass.Healer;
			else if (ModelRealm == eRealm.Hibernia) return CharacterClass.Druid;
			return CharacterClass.None;
		}

		protected override void SetBlockEvadeParryChance()
		{
			base.SetBlockEvadeParryChance();
			BlockChance = 5;
		}

		protected override KeepGuardBrain GetBrain() => new HealerBrain();

		protected override void SetName()
		{
			switch (ModelRealm)
			{
				case eRealm.None:
				case eRealm.Albion:
					Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.Cleric");
					break;
				case eRealm.Midgard:
					Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.Healer");
					break;
				case eRealm.Hibernia:
					Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.Druid");
					break;
			}

			if (Realm == eRealm.None)
			{
				Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.Renegade", Name);
			}
		}
	}
}
