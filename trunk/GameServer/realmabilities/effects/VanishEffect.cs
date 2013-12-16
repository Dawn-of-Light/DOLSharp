using System.Collections;
using System.Collections.Generic;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Vanish
	/// </summary>
	public class VanishEffect : TimedEffect
	{
		public const string VANISH_BLOCK_ATTACK_TIME_KEY = "vanish_no_attack";

		double m_speedBonus;
		int m_countdown;
		RegionTimer m_countDownTimer = null;
		RegionTimer m_removeTimer = null;

		public VanishEffect(int duration, double speedBonus)
			: base(duration)
		{
			m_speedBonus = speedBonus;
			m_countdown = (duration + 500) / 1000;
		}

		public override void Start(GameLiving target)
		{
			base.Start(target);
			GamePlayer player = target as GamePlayer;
			player.StopAttack();
			player.Stealth(true);
			player.Out.SendUpdateMaxSpeed();
			m_countDownTimer = new RegionTimer(player, new RegionTimerCallback(CountDown));
			m_countDownTimer.Start(1);
			player.TempProperties.setProperty(VANISH_BLOCK_ATTACK_TIME_KEY, player.CurrentRegion.Time + 30000);
			m_removeTimer = new RegionTimer(player, new RegionTimerCallback(RemoveAttackBlock));
			m_removeTimer.Start(30000);
		}

		public int RemoveAttackBlock(RegionTimer timer)
		{
			GamePlayer player = timer.Owner as GamePlayer;
			if (player != null)
				player.TempProperties.removeProperty(VANISH_BLOCK_ATTACK_TIME_KEY);
			return 0;
		}

		public override void Stop()
		{
			base.Stop();
			GamePlayer player = Owner as GamePlayer;
			player.Out.SendUpdateMaxSpeed();
			if (m_countDownTimer != null)
			{
				m_countDownTimer.Stop();
				m_countDownTimer = null;
			}
		}

		public int CountDown(RegionTimer timer)
		{
			if (m_countdown > 0)
			{
				((GamePlayer)Owner).Out.SendMessage("You are hidden for " + m_countdown + " more seconds!", eChatType.CT_SpellPulse, eChatLoc.CL_SystemWindow);
				m_countdown--;
				return 1000;
			}
			return 0;
		}

		public double SpeedBonus { get { return m_speedBonus; } }

		public override string Name { get { return "Vanish"; } }

		public override ushort Icon { get { return 3019; } }

		public override IList<string> DelveInfo
		{
			get
			{
				var list = new List<string>();
				list.Add("Vanish effect");
				return list;
			}
		}
	}
}