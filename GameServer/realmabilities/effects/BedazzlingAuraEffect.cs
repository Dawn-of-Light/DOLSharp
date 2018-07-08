using System;
using System.Collections.Generic;
using DOL.Events;
using DOL.GS.Effects;

namespace DOL.GS.RealmAbilities
{
    /// <summary>
    /// Effect handler for Barrier Of Fortitude
    /// </summary>
    public class BedazzlingAuraEffect : TimedEffect
    {
        private int _value;

        /// <summary>
        /// Default constructor for AmelioratingMelodiesEffect
        /// </summary>
        public BedazzlingAuraEffect()
            : base(30000)
        {
        }

        /// <summary>
        /// Called when effect is to be started
        /// </summary>
        /// <param name="living"></param>
        /// <param name="value"></param>
        public void Start(GameLiving living, int value)
        {
            _value = value;

            if (living.TempProperties.getProperty(BarrierOfFortitudeAbility.BofBaSb, false))
            {
                return;
            }

            base.Start(living);

            living.AbilityBonus[(int)eProperty.Resist_Body] += _value;
            living.AbilityBonus[(int)eProperty.Resist_Cold] += _value;
            living.AbilityBonus[(int)eProperty.Resist_Energy] += _value;
            living.AbilityBonus[(int)eProperty.Resist_Heat] += _value;
            living.AbilityBonus[(int)eProperty.Resist_Matter] += _value;
            living.AbilityBonus[(int)eProperty.Resist_Spirit] += _value;

            if (living is GamePlayer player)
            {
                GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
                player.Out.SendCharResistsUpdate();
            }

            living.TempProperties.setProperty(BarrierOfFortitudeAbility.BofBaSb, true);
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

        public override void Stop()
        {
            base.Stop();

            m_owner.AbilityBonus[(int)eProperty.Resist_Body] -= _value;
            m_owner.AbilityBonus[(int)eProperty.Resist_Cold] -= _value;
            m_owner.AbilityBonus[(int)eProperty.Resist_Energy] -= _value;
            m_owner.AbilityBonus[(int)eProperty.Resist_Heat] -= _value;
            m_owner.AbilityBonus[(int)eProperty.Resist_Matter] -= _value;
            m_owner.AbilityBonus[(int)eProperty.Resist_Spirit] -= _value;
            if (m_owner is GamePlayer player)
            {
                player.Out.SendCharResistsUpdate();
                GameEventMgr.RemoveHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
            }

            m_owner.TempProperties.removeProperty(BarrierOfFortitudeAbility.BofBaSb);
        }

        /// <summary>
        /// Name of the effect
        /// </summary>
        public override string Name => "Bedazzling Aura";

        /// <summary>
        /// Icon ID
        /// </summary>
        public override ushort Icon => 3029;

        /// <summary>
        /// Delve information
        /// </summary>
        public override IList<string> DelveInfo
        {
            get
            {
                var delveInfoList = new List<string>(8)
                {
                    "Grants the group increased resistance to magical damage (Does not stack with Soldier's Barricade or Barrier of Fortitude).",
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
