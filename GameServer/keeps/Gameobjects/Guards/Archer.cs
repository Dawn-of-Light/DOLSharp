using DOL.GS.PlayerClass;

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

		protected override ICharacterClass GetClass()
		{
			if (ModelRealm == eRealm.Albion) return new ClassScout();
			else if (ModelRealm == eRealm.Midgard) return new ClassHunter();
			else if (ModelRealm == eRealm.Hibernia) return new ClassRanger();
			return new DefaultCharacterClass();
		}
	}
}
