using System;
using System.Collections;
using DOL.GS.PacketHandler;
using DOL.GS.SkillHandler;
using DOL.GS.PropertyCalc;
using DOL.Events;
using System.Collections.Generic;

namespace DOL.GS.Effects
{
    /// <summary>
    /// Effect handler for Barrier Of Fortitude
    /// </summary>
	public class StrikePredictionEffect : StaticEffect, IGameEffect
    {
        private const String m_delveString = "Grants all group members a chance to evade all melee and arrow attacks for 30 seconds.";
        private GamePlayer m_player;
        private Int32 m_effectDuration;
        private RegionTimer m_expireTimer;
        private int m_value;

        /// <summary>
        /// Called when effect is to be started
        /// </summary>
        /// <param name="player">The player to start the effect for</param>
        /// <param name="duration">The effectduration in secounds</param>
        /// <param name="value">The percentage additional value for melee absorb</param>
        public void Start(GamePlayer player, int duration, int value)
        {
            m_player = player;
            m_effectDuration = duration;
            m_value = value;

            StartTimers();

            GameEventMgr.AddHandler(m_player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
            m_player.AbilityBonus[(int)eProperty.EvadeChance] += m_value;

            m_player.EffectList.Add(this);
        }

        /// <summary>
        /// Called when a player leaves the game
        /// </summary>
        /// <param name="e">The event which was raised</param>
        /// <param name="sender">Sender of the event</param>
        /// <param name="args">EventArgs associated with the event</param>
        private static void PlayerLeftWorld(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = (GamePlayer)sender;

            StrikePredictionEffect SPEffect = (StrikePredictionEffect)player.EffectList.GetOfType(typeof(StrikePredictionEffect));
            if (SPEffect != null)
            {
                SPEffect.Cancel(false);
            }
        }

        /// <summary>
        /// Called when effect is to be cancelled
        /// </summary>
        /// <param name="playerCancel">Whether or not effect is player cancelled</param>
        public override void Cancel(bool playerCancel)
        {
            StopTimers();
            m_player.AbilityBonus[(int)eProperty.EvadeChance] -= m_value;
            m_player.EffectList.Remove(this);
            GameEventMgr.RemoveHandler(m_player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
        }

        /// <summary>
        /// Starts the timers for this effect
        /// </summary>
        private void StartTimers()
        {
            StopTimers();
            m_expireTimer = new RegionTimer(m_player, new RegionTimerCallback(ExpireCallback), m_effectDuration * 1000);
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
                return "Strike Prediction";
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
                return 3036;
            }
        }

        /// <summary>
        /// Delve information
        /// </summary>
        public override IList<string> DelveInfo
        {
            get
            {
				var delveInfoList = new List<string>();
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