using DOL.GS;
using DOL.GS.Keeps;
using System;

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
		
		private long m_nextCallForHelpTime = 0; // Used to limit how often keep lords call for help
		/// <summary>
		/// Bring all alive keep guards to defend the lord
		/// </summary>
		/// <param name="trigger">The entity which triggered the BAF.</param>
		protected override void BringFriends(GameLiving trigger)
		{
			if (GameServer.Instance.Configuration.ServerType == eGameServerType.GST_PvE)
			{
				GuardLord lord = Body as GuardLord;
				long currenttime = DateTime.UtcNow.Ticks;

				if (lord != null && m_nextCallForHelpTime < currenttime)
				{
					// Don't call for help more than once every minute
					m_nextCallForHelpTime = currenttime + TimeSpan.TicksPerMinute;

					int iGuardsResponding = 0;

					foreach (GameKeepGuard guard in lord.Component.Keep.Guards.Values)
						if (guard != null && guard.IsAlive && guard.IsAvailable)
							if (guard.AssistLord(lord))
								iGuardsResponding++;

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
			else
				base.BringFriends(trigger);
		}
	}// LordBrain
}
