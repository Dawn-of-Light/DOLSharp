using System;
using System.Collections.Generic;
using DOL.Events;
using DOL.GS.Effects;

namespace DOL.GS.RealmAbilities
{
    /// <summary>
    /// Effect handler for Adrenaline Rush
    /// </summary>
    public class AdrenalineRushEffect : TimedEffect
    {
        private readonly int _value;

        /// <summary>
        /// Default constructor for AdrenalineRushEffect
        /// </summary>
        public AdrenalineRushEffect(int duration, int value)
            : base(duration)
        {
            _value = value;
        }

        /// <summary>
        /// Called when effect is to be started
        /// </summary>
        /// <param name="living">The living to start the effect for</param>
        public override void Start(GameLiving living)
        {
            base.Start(living);

            if (living is GamePlayer)
            {
                GameEventMgr.AddHandler(living, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
            }

            living.AbilityBonus[(int)eProperty.MeleeDamage] += _value;
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
                AdrenalineRushEffect spEffect = player.EffectList.GetOfType<AdrenalineRushEffect>();
                spEffect?.Cancel(false);
            }
        }

        public override void Stop()
        {
            base.Stop();
            m_owner.AbilityBonus[(int)eProperty.MeleeDamage] -= _value;
            if (m_owner is GamePlayer)
            {
                GameEventMgr.RemoveHandler(m_owner, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
            }
        }

        /// <summary>
        /// Name of the effect
        /// </summary>
        public override string Name => "Adrenaline Rush";

        /// <summary>
        /// Icon ID
        /// </summary>
        public override ushort Icon => 3001;

        /// <summary>
        /// Delve information
        /// </summary>
        public override IList<string> DelveInfo
        {
            get
            {
                var delveInfoList = new List<string>(8)
                {
                    "Doubles the base melee damage for 20 seconds.",
                    " ",
                    "Value: " + _value + "%"
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