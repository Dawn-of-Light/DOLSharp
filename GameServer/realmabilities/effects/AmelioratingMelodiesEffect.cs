using System.Collections;

using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Ameliorating Melodies
	/// </summary>
	public class AmelioratingMelodiesEffect : TimedEffect
	{
		/// <summary>
		/// The countdown value. If this value is 0, the effect vanishes
		/// </summary>
		int m_countdown;

		/// <summary>
		/// The number of hit points healed each tick
		/// </summary>
		int m_heal;

		/// <summary>
		/// Max healing range
		/// </summary>
		int m_range;

		/// <summary>
		/// The rgion timer
		/// </summary>
		RegionTimer m_countDownTimer = null;

		/// <summary>
		/// Ameliorating Melodies
		/// </summary>
		/// <param name="heal">Delve value hit points per tick"</param>
		public AmelioratingMelodiesEffect(int heal)
			: base(30000)
		{
			m_heal = heal;
			m_range = 2000;
			m_countdown = 10;
		}

		/// <summary>
		/// Starts the effect
		/// </summary>
		/// <param name="target">The player of this effect</param>
		public override void Start(GameLiving target)
		{
			base.Start(target);
			GamePlayer player = target as GamePlayer;
			if (player == null) return;
			m_range = (int)(2000 * player.GetModified(eProperty.SpellRange) * 0.01);
			m_heal = (int)(m_heal * player.GetModified(eProperty.HealingEffectiveness) * 0.01);
			m_countDownTimer = new RegionTimer(player, new RegionTimerCallback(CountDown));
			m_countDownTimer.Start(1);
		}

		/// <summary>
		/// Stops the effect
		/// </summary>
		public override void Stop()
		{
			base.Stop();
			if (m_countDownTimer != null)
			{
				m_countDownTimer.Stop();
				m_countDownTimer = null;
			}
		}

		/// <summary>
		/// Timer callback
		/// </summary>
		/// <param name="timer">The region timer</param>
		public int CountDown(RegionTimer timer)
		{
			if (m_countdown > 0)
			{
				m_countdown--;
				GamePlayer player = Owner as GamePlayer;
				if (player == null) return 0;
				if (player.PlayerGroup == null) return 3000;
				GamePlayer[] group = player.PlayerGroup.GetPlayersInTheGroup();
				foreach (GamePlayer groupmember in group)
				{
					if ((groupmember != player) && (groupmember.Health < groupmember.MaxHealth) && (WorldMgr.CheckDistance(player, groupmember, m_range)) && (groupmember.IsAlive))
					{
						if (player.IsStealthed)
						{
							player.Stealth(false);
						}

						int heal = m_heal;
						if (groupmember.Health + heal > groupmember.MaxHealth) heal = groupmember.MaxHealth - groupmember.Health;
						groupmember.ChangeHealth(player, GameLiving.eHealthChangeType.Regenerate, heal);
						player.Out.SendMessage("You heal " + groupmember.Name + " for " + heal.ToString() + " hit points.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
						groupmember.Out.SendMessage(player.Name + " heals you for " + heal.ToString() + " hit points.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
					}
				}
				return 3000;
			}
			return 0;
		}

		/// <summary>
		/// Name of the effect
		/// </summary>
		public override string Name { get { return "Ameliorating Melodies"; } }

		/// <summary>
		/// Icon of the effect
		/// </summary>
		public override ushort Icon { get { return 3021; } }

		/// <summary>
		/// Delve information
		/// </summary>
		public override System.Collections.IList DelveInfo
		{
			get
			{
				ArrayList list = new ArrayList();
				list.Add("Ameliorating Melodies");
				list.Add(" ");
				list.Add("Value: " + m_heal.ToString() + " / tick");
				list.Add("Target: Group");
				list.Add("Range: " + m_range.ToString());
				list.Add("Duration: 30 s (" + (m_countdown * 3).ToString() + " s remaining)");
				return list;
			}
		}
	}
}