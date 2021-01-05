using DOL.GS.PlayerClass;

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
	}
}
