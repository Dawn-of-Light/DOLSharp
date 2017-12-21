using DOL.GS;
using DOL.GS.Keeps;

namespace DOL.AI.Brain
{
	/// <summary>
	/// The Brain for the Area Lord
	/// </summary>
	public class LordBrain : KeepGuardBrain
	{
		private long m_nextCallForHelpTime = 0; // Used to limit how often keep lords call for help
	
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

		/// <summary>
		/// Bring all alive keep guards to defend the lord
		/// </summary>
		/// <param name="attackData">The data associated with the puller's attack.</param>
		protected override void BringFriends(AttackData ad)
		{
			long currenttime = DateTime.UtcNow.Ticks;
			if (m_nextCallForHelpTime < currenttime && Body is GuardLord lord)
			{
				// Don't call for help more than once every minute
				m_nextCallForHelpTime = currenttime + TimeSpan.TicksPerMinute;

				int iGuardsResponding = 0;
				foreach (GameKeepGuard guard in lord.Component.Keep.Guards.Values)
					if (guard != null && guard.IsAlive && guard.IsAvailable && !(guard is FrontierHastener))
					{
						iGuardsResponding++;
						guard.Follow(ad.Target, GameNPC.STICKMINIMUMRANGE, int.MaxValue);
					}

				string sMessage = $"{lord.Name} bellows for assistance ";
				if (iGuardsResponding == 0)
					sMessage += "but no guards respond!";
				else
					sMessage += $"and {iGuardsResponding} guards respond!";

				foreach (GamePlayer player in lord.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE, true))
					if (player != null)
						ChatUtil.SendErrorMessage(player, sMessage);
			}
		}
	}// LordBrain
}
