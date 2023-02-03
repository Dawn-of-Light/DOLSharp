using DOL.GS.ServerProperties;
using DOL.Language;

namespace DOL.GS.Keeps
{
	public class GuardArcher : GameKeepGuard
	{
		public override int AttackRangeDistance
		{
			get
			{
				return 2100;
			}
		}

		protected override CharacterClass GetClass()
		{
			if (ModelRealm == eRealm.Albion) return CharacterClass.Scout;
			else if (ModelRealm == eRealm.Midgard) return CharacterClass.Hunter;
			else if (ModelRealm == eRealm.Hibernia) return CharacterClass.Ranger;
			return CharacterClass.None;
		}

		protected override void SetBlockEvadeParryChance()
		{
			base.SetBlockEvadeParryChance();
			if (ModelRealm == eRealm.Albion)
			{
				BlockChance = 10;
				EvadeChance = 5;
			}
			else
			{
				EvadeChance = 15;
			}

		}

		protected override void SetName()
		{
			switch (ModelRealm)
			{
				case eRealm.None:
				case eRealm.Albion:
					if (IsPortalKeepGuard)
						Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.BowmanCommander");
					else Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.Scout");
					break;
				case eRealm.Midgard:
					if (IsPortalKeepGuard)
						Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.NordicHunter");
					else Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.Hunter");
					break;
				case eRealm.Hibernia:
					if (IsPortalKeepGuard)
						Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.MasterRanger");
					else Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.Ranger");
					break;
			}

			if (Realm == eRealm.None)
			{
				Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.Renegade", Name);
			}
		}
	}
}
