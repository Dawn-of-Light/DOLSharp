using DOL.GS.ServerProperties;
using DOL.Language;

namespace DOL.GS.Keeps
{
	public class GuardFighter : GameKeepGuard
	{
		protected override ICharacterClass GetClass()
		{
			if (ModelRealm == eRealm.Albion) return CharacterClassBase.GetClass((int)eCharacterClass.Armsman);
			else if (ModelRealm == eRealm.Midgard) return CharacterClassBase.GetClass((int)eCharacterClass.Warrior);
			else if (ModelRealm == eRealm.Hibernia) return CharacterClassBase.GetClass((int)eCharacterClass.Hero);
			return new CharacterClassBase();
		}

		protected override void SetBlockEvadeParryChance()
		{
			base.SetBlockEvadeParryChance();
			BlockChance = 10;
			ParryChance = 10;

			if (ModelRealm != eRealm.Albion)
			{
				EvadeChance = 5;
				ParryChance = 5;
			}
		}

		protected override void SetName()
		{
			switch (ModelRealm)
			{
				case eRealm.None:
				case eRealm.Albion:
					if (IsPortalKeepGuard)
					{
						Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.KnightCommander");
					}
					else
					{
						if (Gender == eGender.Male)
							Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.Armsman");
						else Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.Armswoman");
					}
					break;
				case eRealm.Midgard:
					if (IsPortalKeepGuard)
						Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.NordicJarl");
					else Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.Huscarl");
					break;
				case eRealm.Hibernia:
					if (IsPortalKeepGuard)
						Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.Champion");
					else Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.Guardian");
					break;
			}

			if (Realm == eRealm.None)
			{
				Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.Renegade", Name);
			}
		}
	}
}
