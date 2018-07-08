using System;
using System.Collections.Generic;

namespace DOL.GS.Effects
{
    /// <summary>
    /// Effect handler for Juggernaut
    /// </summary>
    public class JuggernautEffect : StaticEffect
    {
        private const string DelveString = "Increases the effective level of the pet by the listed number (capped at level 70).";
        private const int JuggernautCapEffect = 70;
        private GameNPC _living;
        private int _effectDuration;
        private RegionTimer _expireTimer;
        private byte _value;
        private int _growSize = 15;

        /// <summary>
        /// Called when effect is to be started
        /// </summary>
        /// <param name="player">The player to start the effect for</param>
        /// <param name="duration">The effectduration in secounds</param>
        /// <param name="value">The increment of effective level</param>
        public void Start(GameLiving living, int duration, byte value)
        {
            if (living.ControlledBrain == null)
            {
                return;
            }

            _living = living.ControlledBrain.Body;
            _effectDuration = duration;
            _value = value;

            StartTimers();

            _living.Size += (byte)_growSize;
            _living.Level = (byte)Math.Min(_living.Level + _value, JuggernautCapEffect);
            _living.EffectList.Add(this);
        }

        /// <summary>
        /// Called when effect is to be cancelled
        /// </summary>
        /// <param name="playerCancel">Whether or not effect is player cancelled</param>
        public override void Cancel(bool playerCancel)
        {
            StopTimers();
            _living.Size -= (byte)_growSize;
            _living.Level -= _value;
            _living.EffectList.Remove(this);
        }

        /// <summary>
        /// Starts the timers for this effect
        /// </summary>
        private void StartTimers()
        {
            StopTimers();
            _expireTimer = new RegionTimer(_living, new RegionTimerCallback(ExpireCallback), _effectDuration * 1000);
        }

        /// <summary>
        /// Stops the timers for this effect
        /// </summary>
        private void StopTimers()
        {

            if (_expireTimer != null)
            {
                _expireTimer.Stop();
                _expireTimer = null;
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
        public override string Name => "Juggernaut";

        /// <summary>
        /// Remaining time of the effect in milliseconds
        /// </summary>
        public override int RemainingTime
        {
            get
            {
                RegionTimer timer = _expireTimer;
                if (timer == null || !timer.IsAlive)
                {
                    return 0;
                }

                return timer.TimeUntilElapsed;
            }
        }

        /// <summary>
        /// Icon ID
        /// </summary>
        public override ushort Icon => 3030;

        /// <summary>
        /// Delve information
        /// </summary>
        public override IList<string> DelveInfo
        {
            get
            {
                var delveInfoList = new List<string>
                {
                    DelveString,
                    " ",
                    $"Value: {_value}%"
                };

                int seconds = RemainingTime / 1000;
                if (seconds > 0)
                {
                    delveInfoList.Add(" ");
                    delveInfoList.Add($"- {seconds} seconds remaining.");
                }

                return delveInfoList;
            }
        }
    }
}