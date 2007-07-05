using System;
using System.Collections;
using DOL.GS.PacketHandler;
using DOL.GS.RealmAbilities;

namespace DOL.GS.Effects
{
	/// <summary>
	/// Adrenaline Rush
	/// </summary>
	public class FerociousWillEffect : TimedEffect
	{

		private RegionTimer ticktimer;
		private int absorb = 0;
		private int m_currentBonus = 0;

		public FerociousWillEffect()
			: base(30000)
		{
			
		}

		private GameLiving owner;

		public override void Start(GameLiving target)
		{
			base.Start(target);
			owner = target;
			GamePlayer player = target as GamePlayer;
			if (player != null)
			{
				foreach (GamePlayer p in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					p.Out.SendSpellEffectAnimation(player, player, Icon, 0, false, 1);
				}
			}
			m_currentBonus = 0;
			ticktimer = new RegionTimer(target);
			ticktimer.Callback = new RegionTimerCallback(OnTick);
			ticktimer.Start(5000);


		}

		private int OnTick(RegionTimer timer)
		{
			if (m_currentBonus >= 25)
			{
				owner.BuffBonusCategory1[(int)eProperty.ArmorAbsorbtion] -= m_currentBonus;
				m_currentBonus += 5;
				owner.BuffBonusCategory1[(int)eProperty.ArmorAbsorbtion] += m_currentBonus;
				return 5000;
			}
			return 0;

		}

		public override string Name { get { return "Ferocious Will"; } }

		public override ushort Icon { get { return 7065; } }

		public override void Stop()
		{
			owner.BuffBonusCategory1[(int)eProperty.ArmorAbsorbtion] -= m_currentBonus;
			base.Stop();
		}

		public int SpellEffectiveness
		{
			get { return 100; }
		}

		public override System.Collections.IList DelveInfo
		{
			get
			{
				ArrayList list = new ArrayList();
				list.Add("Gives the zerker an ABS buff that ticks up by 5% every 5 seconds for a max of 25% at 25 seconds. Lasts 30 seconds total.");
				return list;
			}
		}
	}
}