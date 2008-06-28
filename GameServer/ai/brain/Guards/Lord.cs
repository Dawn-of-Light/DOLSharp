using DOL.GS;
using DOL.GS.Keeps;

namespace DOL.AI.Brain
{
	/// <summary>
	/// The Brain for the Area Lord
	/// </summary>
	public class LordBrain : KeepGuardBrain
	{
		public LordBrain() : base()
		{
		}

		public override void Think()
		{
			if (Body != null && Body.Spells.Count == 0)
			{
				switch (Body.Realm)
				{
					case eRealm.None:
					case eRealm.Albion:
						Body.Spells.Add(SpellMgr.AlbLordHealSpell);
						break;
					case eRealm.Midgard:
						Body.Spells.Add(SpellMgr.MidLordHealSpell);
						break;
					case eRealm.Hibernia:
						Body.Spells.Add(SpellMgr.HibLordHealSpell);
						break;
				}
			}
			base.Think();
		}
	}
}
