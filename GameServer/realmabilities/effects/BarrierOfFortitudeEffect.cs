using System;
using System.Collections.Generic;
using DOL.Events;

namespace DOL.GS.Effects
{
    /// <summary>
    /// Effect handler for Barrier Of Fortitude
    /// </summary>
    public class BarrierOfFortitudeEffect : StaticEffect
    {
        private const string DelveString = "Grants the group a melee absorption bonus (Does not stack with Soldier's Barricade or Bedazzling Aura).";
        private GamePlayer _player;
        private int _effectDuration;
        private RegionTimer _expireTimer;
        private int _value;

        /// <summary>
        /// Called when effect is to be started
        /// </summary>
        /// <param name="player">The player to start the effect for</param>
        /// <param name="duration">The effectduration in secounds</param>
        /// <param name="value">The percentage additional value for melee absorb</param>
        public void Start(GamePlayer player, int duration, int value)
        {
            _player = player;
            _effectDuration = duration;
            _value = value;

            if (player.TempProperties.getProperty(RealmAbilities.BarrierOfFortitudeAbility.BofBaSb, false))
            {
                return;
            }

            StartTimers();

            GameEventMgr.AddHandler(_player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));

            _player.AbilityBonus[(int)eProperty.ArmorAbsorption] += _value;

            _player.EffectList.Add(this);
            player.TempProperties.setProperty(RealmAbilities.BarrierOfFortitudeAbility.BofBaSb, true);
        }

        /// <summary>
        /// Called when a player leaves the game
        /// </summary>
        /// <param name="e">The event which was raised</param>
        /// <param name="sender">Sender of the event</param>
        /// <param name="args">EventArgs associated with the event</param>
        private static void PlayerLeftWorld(DOLEvent e, object sender, EventArgs args)
        {
            if (sender is GamePlayer player)
            {
                BarrierOfFortitudeEffect boFEffect = player.EffectList.GetOfType<BarrierOfFortitudeEffect>();
                boFEffect?.Cancel(false);
            }
        }

        /// <summary>
        /// Called when effect is to be cancelled
        /// </summary>
        /// <param name="playerCancel">Whether or not effect is player cancelled</param>
        public override void Cancel(bool playerCancel)
        {

            StopTimers();
            _player.AbilityBonus[(int)eProperty.ArmorAbsorption] -= _value;
            _player.EffectList.Remove(this);
            GameEventMgr.RemoveHandler(_player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
            _player.TempProperties.removeProperty(RealmAbilities.BarrierOfFortitudeAbility.BofBaSb);
        }

        /// <summary>
        /// Starts the timers for this effect
        /// </summary>
        private void StartTimers()
        {
            StopTimers();
            _expireTimer = new RegionTimer(_player, new RegionTimerCallback(ExpireCallback), _effectDuration * 1000);
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
        public override string Name => "Barrier of Fortitude";

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
        public override ushort Icon => 3015;

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
