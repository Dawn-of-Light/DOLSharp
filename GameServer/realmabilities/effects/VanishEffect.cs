using System.Collections;

using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Vanish
	/// </summary>
	public class VanishEffect : TimedEffect
	{
		double m_speedBonus;
		int m_countdown;
		RegionTimer m_countDownTimer = null;
		
		public VanishEffect(int duration, double speedBonus) : base(duration)
		{
			m_speedBonus = speedBonus;
			m_countdown = (duration+500)/1000;
		}

		public override void Start(GameLiving target)
		{
			base.Start(target);
			GamePlayer player = target as GamePlayer;
			player.Stealth(true);
			player.Out.SendUpdateMaxSpeed();
			m_countDownTimer = new RegionTimer(player, new RegionTimerCallback(CountDown));
			m_countDownTimer.Start(1);
		}
		
		public override void Stop()
		{
			base.Stop();
			GamePlayer player = Owner as GamePlayer;
			player.Out.SendUpdateMaxSpeed();
			if (m_countDownTimer != null) {
				m_countDownTimer.Stop();
				m_countDownTimer = null;
			}
		}
		
		public int CountDown(RegionTimer timer)
		{
			if (m_countdown > 0) {
				((GamePlayer)Owner).Out.SendMessage("You are hidden for "+m_countdown+" more seconds!", eChatType.CT_SpellPulse, eChatLoc.CL_SystemWindow);
				m_countdown--;
				return 1000;
			}
			return 0;
		}

		public double SpeedBonus { get { return m_speedBonus; } }

		public override string Name { get { return "Vanish"; } }

		public override ushort Icon { get { return 3019; } }

		public override System.Collections.IList DelveInfo
		{
			get
			{
				ArrayList list = new ArrayList();
				list.Add("Vanish effect");
				return list;
			}
		}
	}
}