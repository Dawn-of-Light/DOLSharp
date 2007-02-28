using System;
using System.Collections;
using DOL.GS.PacketHandler;
using DOL.GS.SkillHandler;
using DOL.Events;

namespace DOL.GS.Effects
{
	/// <summary>
	/// 
	/// </summary>
	public class MasteryofConcentrationEffect : StaticEffect, IGameEffect
	{
		private const string m_delveString = "This ability allows a player to cast uninterrupted, even while sustaining attacks, through melee or spell for 30 seconds.";
		private GamePlayer m_player;
		//private Int64 m_startTick;
		private RegionTimer m_expireTimer;
		private UInt16 m_id;

		/// <summary>
		/// Default constructor for MasteryofConcentrationEffect
		/// </summary>
		public MasteryofConcentrationEffect()
		{
		}

		/// <summary>
		/// Called when effect is to be started
		/// </summary>
		/// <param name="player">The player to start the effect for</param>
		public void Start(GamePlayer player)
		{
			m_player = player;
			StartTimers();
			m_player.EffectList.Add(this);
			
		}

		/// <summary>
		/// Called when effect is to be cancelled
		/// </summary>
		/// <param name="playerCancel">Whether or not effect is player cancelled</param>
		public void Cancel(bool playerCancel)
		{
            //uncancable by player
            if (playerCancel)
                return;

            StopTimers();

			m_player.EffectList.Remove(this);
		}

		/// <summary>
		/// Starts the timers for this effect
		/// </summary>
		private void StartTimers()
		{
			StopTimers();
            m_expireTimer = new RegionTimer(m_player, new RegionTimerCallback(ExpiredCallback), RealmAbilities.MasteryofConcentrationAbility.Duration);
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
		private int ExpiredCallback(RegionTimer timer)
		{
			Cancel(false);

			return 0;
		}

		/// <summary>
		/// Name of the effect
		/// </summary>
		public string Name
		{
			get
			{
				return "Mastery of Concentration";
			}
		}

        /// <summary>
        /// Remaining time of the effect in milliseconds
        /// </summary>
        public Int32 RemainingTime
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
		public UInt16 Icon
		{
			get
			{
				return 3006;
			}
		}

		/// <summary>
		/// Unique ID for identification in the effect list
		/// </summary>
		public UInt16 InternalID
		{
			get
			{
				return m_id;
			}
			set
			{
				m_id = value;
			}
		}

		/// <summary>
		/// Delve information
		/// </summary>
		public IList DelveInfo
		{
			get
			{
				IList delveInfoList = new ArrayList(4);
				delveInfoList.Add(m_delveString);

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
