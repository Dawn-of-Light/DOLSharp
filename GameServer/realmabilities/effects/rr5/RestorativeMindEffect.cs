using System;
using System.Collections;
using System.Collections.Generic;
using DOL.GS.PacketHandler;
using DOL.GS.RealmAbilities;
using DOL.GS;

namespace DOL.GS.Effects
{
	/// <summary>
	/// Mastery of Concentration
	/// </summary>
	public class RestorativeMindEffect : TimedEffect
	{

		private GamePlayer m_playerOwner;
		private RegionTimer m_tickTimer;


		public RestorativeMindEffect()
			: base(30000)
		{

		}

		public override void Start(GameLiving target)
		{
			base.Start(target);
			GamePlayer player = target as GamePlayer;
			if (player != null)
			{
				m_playerOwner = player;
				foreach (GamePlayer p in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					p.Out.SendSpellEffectAnimation(player, player, Icon, 0, false, 1);
				}

				healTarget();
				startTimer();
			}
		}

		public override void Stop()
		{
			if (m_tickTimer.IsAlive)
				m_tickTimer.Stop();
			base.Stop();
		}

		private void healTarget()
		{
			if (m_playerOwner != null)
			{
				int healthtick = (int)(m_playerOwner.MaxHealth * 0.05);
				int manatick = (int)(m_playerOwner.MaxMana * 0.05);
				int endutick = (int)(m_playerOwner.MaxEndurance * 0.05);
				if (!m_playerOwner.IsAlive)
					Stop();
				int modendu = m_playerOwner.MaxEndurance - m_playerOwner.Endurance;
				if (modendu > endutick)
					modendu = endutick;
				m_playerOwner.Endurance += modendu;
				int modheal = m_playerOwner.MaxHealth - m_playerOwner.Health;
				if (modheal > healthtick)
					modheal = healthtick;
				m_playerOwner.Health += modheal;
				int modmana = m_playerOwner.MaxMana - m_playerOwner.Mana;
				if (modmana > manatick)
					modmana = manatick;
				m_playerOwner.Mana += modmana;
			}
		}

		private int onTick(RegionTimer timer)
		{
			healTarget();
			return 3000;
		}

		private void startTimer()
		{
			m_tickTimer = new RegionTimer(m_playerOwner);
			m_tickTimer.Callback = new RegionTimerCallback(onTick);
			m_tickTimer.Start(3000);

		}

		public override string Name { get { return "Restorative Mind"; } }

		public override ushort Icon { get { return 3070; } }



		public override IList<string> DelveInfo
		{
			get
			{
				var list = new List<string>();
				list.Add("Heals you for 5% mana/endu/hits each tick (3 seconds)");
				return list;
			}
		}
	}
}