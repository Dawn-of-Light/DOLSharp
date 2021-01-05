using DOL.GS.PlayerClass;

namespace DOL.GS.Keeps
{
	public class GuardFighter : GameKeepGuard
	{
		protected override ICharacterClass GetClass()
		{
			if (ModelRealm == eRealm.Albion) return new ClassArmsman();
			else if (ModelRealm == eRealm.Midgard) return new ClassWarrior();
			else if (ModelRealm == eRealm.Hibernia) return new ClassHero();
			return new DefaultCharacterClass();
		}
	}
}
