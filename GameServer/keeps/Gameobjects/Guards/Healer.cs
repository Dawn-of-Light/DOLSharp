using DOL.GS.PlayerClass;

namespace DOL.GS.Keeps
{
	public class GuardHealer : GameKeepGuard
	{
		protected override ICharacterClass GetClass()
		{
			if (ModelRealm == eRealm.Albion) return new ClassCleric();
			else if (ModelRealm == eRealm.Midgard) return new ClassHealer();
			else if (ModelRealm == eRealm.Hibernia) return new ClassDruid();
			return new DefaultCharacterClass();
		}
	}
}
