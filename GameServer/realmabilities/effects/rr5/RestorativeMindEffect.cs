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

		private GamePlayer owner;
		private RegionTimer ticktimer;


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
				owner = player;
				foreach (GamePlayer p in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					p.Out.SendSpellEffectAnimation(player, player, Icon, 0, false, 1);
				}
			}

			healTarget();
			startTimer();

		}

		public override void Stop()
		{
			if (ticktimer.IsAlive)
				ticktimer.Stop();
			base.Stop();
		}

		private void healTarget()
		{
			int healthtick = (int)(owner.MaxHealth * 0.05);
			int manatick = (int)(owner.MaxMana * 0.05);
			int endutick = (int)(owner.MaxEndurance * 0.05);
			if (!owner.IsAlive)
				Stop();
			int modendu = owner.MaxEndurance - owner.Endurance;
			if (modendu > endutick)
				modendu = endutick;
			owner.Endurance += modendu;
			int modheal = owner.MaxHealth - owner.Health;
			if (modheal > healthtick)
				modheal = healthtick;
			owner.Health += modheal;
			int modmana = owner.MaxMana - owner.Mana;
			if (modmana > manatick)
				modmana = manatick;
			owner.Mana += modmana;

		}

		private int onTick(RegionTimer timer)
		{
			healTarget();
			return 3000;
		}

		private void startTimer()
		{
			ticktimer = new RegionTimer(owner);
			ticktimer.Callback = new RegionTimerCallback(onTick);
			ticktimer.Start(3000);

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