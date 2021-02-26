using DOL.GS.PlayerClass;
using DOL.GS.ServerProperties;
using DOL.Language;

namespace DOL.GS.Keeps
{
	public class GuardStealther : GameKeepGuard
	{
        public GuardStealther() : base()
        {
            Flags = eFlags.STEALTH;
        }

		protected override ICharacterClass GetClass()
		{
			if (ModelRealm == eRealm.Albion) return new ClassInfiltrator();
			else if (ModelRealm == eRealm.Midgard) return new ClassShadowblade();
			else if (ModelRealm == eRealm.Hibernia) return new ClassNightshade();
			return new DefaultCharacterClass();
		}

		protected override void SetBlockEvadeParryChance()
		{
			base.SetBlockEvadeParryChance();
			EvadeChance = 30;
		}

		protected override void SetName()
		{
			switch (ModelRealm)
			{
				case eRealm.None:
				case eRealm.Albion:
					Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.Infiltrator");
					break;
				case eRealm.Midgard:
					Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.Shadowblade");
					break;
				case eRealm.Hibernia:
					Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.Nightshade");
					break;
			}

			if (Realm == eRealm.None)
			{
				Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.Renegade", Name);
			}
		}
	}
}
