using System;
using System.Collections;
using DOL.GS.PacketHandler;
using DOL.GS.SkillHandler;
using DOL.GS.PropertyCalc;
using DOL.Events;

namespace DOL.GS.Effects
{
	/// <summary>
	/// Effect handler for Barrier Of Fortitude
	/// </summary>
	public class JuggernautEffect : StaticEffect, IGameEffect
	{
		private const String m_delveString = "Increases the effective level of the pet by the listed number (capped at level 70).";
		private GameNPC m_living;
		private Int32 m_effectDuration;
		private RegionTimer m_expireTimer;
		private byte m_value;
		private int m_growSize = 15;



		/// <summary>
		/// Default constructor for AmelioratingMelodiesEffect
		/// </summary>
		public JuggernautEffect()
		{

		}

		/// <summary>
		/// Called when effect is to be started
		/// </summary>
		/// <param name="player">The player to start the effect for</param>
		/// <param name="duration">The effectduration in secounds</param>
		/// <param name="value">The increment of effective level</param>
		public void Start(GamePlayer player, int duration, byte value)
		{
			if (player.ControlledNpc == null)
				return;
			m_living = player.ControlledNpc.Body;
			m_effectDuration = duration;
			m_value = value;


			StartTimers();

			m_living.Size += (byte)m_growSize;
			m_living.Level += m_value;
			m_living.EffectList.Add(this);

		}

		/// <summary>
		/// Called when effect is to be cancelled
		/// </summary>
		/// <param name="playerCancel">Whether or not effect is player cancelled</param>
		public override void Cancel(bool playerCancel)
		{
			StopTimers();
			m_living.Size -= (byte)m_growSize;
			m_living.Level -= m_value;
			m_living.EffectList.Remove(this);
		}

		/// <summary>
		/// Starts the timers for this effect
		/// </summary>
		private void StartTimers()
		{
			StopTimers();
			m_expireTimer = new RegionTimer(m_living, new RegionTimerCallback(ExpireCallback), m_effectDuration * 1000);
		}

		/// <summary>
		/// Stops the timers for this effect
		/// </summary>
		private void StopTimers()
		{

			if (m_expireTimer != null)
			{
				m_expireTimer.Stop();
				m_expireTimer = null;
			}
		}

		/// <summary>
		/// The callback for when the effect expires
		/// </summary>
		/// <param name="timer">The ObjectTimerCallback object</param>
		private int ExpireCallback(RegionTimer timer)
		{
			Cancel(false);

			return 0;
		}


		/// <summary>
		/// Name of the effect
		/// </summary>
		public override string Name
		{
			get
			{
				return "Juggernaut";
			}
		}

		/// <summary>
		/// Remaining time of the effect in milliseconds
		/// </summary>
		public override Int32 RemainingTime
		{
			get
			{
				RegionTimer timer = m_expireTimer;
				if (timer == null || !timer.IsAlive)
					return 0;
				return timer.TimeUntilElapsed;
			}
		}

		/// <summary>
		/// Icon ID
		/// </summary>
		public override UInt16 Icon
		{
			get
			{
				return 3030;
			}
		}

		/// <summary>
		/// Delve information
		/// </summary>
		public override IList DelveInfo
		{
			get
			{
				IList delveInfoList = new ArrayList(10);
				delveInfoList.Add(m_delveString);
				delveInfoList.Add(" ");
				delveInfoList.Add("Value: " + m_value + "%");

				int seconds = (int)(RemainingTime / 1000);
				if (seconds > 0)
				{
					delveInfoList.Add(" ");
					delveInfoList.Add("- " + seconds + " seconds remaining.");
				}

				return delveInfoList;
			}
		}
	}
}