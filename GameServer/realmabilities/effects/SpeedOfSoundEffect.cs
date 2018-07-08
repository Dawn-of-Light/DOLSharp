using System;
using DOL.Events;
using System.Collections.Generic;

namespace DOL.GS.Effects
{
    /// <summary>
    /// Effect handler for Barrier Of Fortitude
    /// </summary>
    public class SpeedOfSoundEffect : TimedEffect
    {
        public SpeedOfSoundEffect(int duration)
            : base(duration)
        { }

        private readonly DOLEventHandler _attackFinished = new DOLEventHandler(AttackFinished);

        /// <summary>
        /// Called when effect is to be started
        /// </summary>
        /// <param name="living">The living to start the effect for</param>
        public override void Start(GameLiving living)
        {
            base.Start(living);
            living.TempProperties.setProperty("Charging", true);
            GameEventMgr.AddHandler(living, GameLivingEvent.AttackFinished, _attackFinished);
            GameEventMgr.AddHandler(living, GameLivingEvent.CastFinished, _attackFinished);
            living.BuffBonusMultCategory1.Set((int)eProperty.MaxSpeed, this, PropertyCalc.MaxSpeedCalculator.Speed4);
            (living as GamePlayer)?.Out.SendUpdateMaxSpeed();
        }

        /// <summary>
        /// Called when the effectowner attacked an enemy
        /// </summary>
        /// <param name="e">The event which was raised</param>
        /// <param name="sender">Sender of the event</param>
        /// <param name="args">EventArgs associated with the event</param>
        private static void AttackFinished(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = (GamePlayer)sender;
            if (e == GameLivingEvent.CastFinished)
            {
                CastingEventArgs cfea = args as CastingEventArgs;

                if (cfea.SpellHandler.Caster != player)
                {
                    return;
                }

                // cancel if the effectowner casts a non-positive spell
                if (!cfea.SpellHandler.HasPositiveEffect)
                {
                    SpeedOfSoundEffect effect = player.EffectList.GetOfType<SpeedOfSoundEffect>();
                    effect?.Cancel(false);
                }
            }
            else if (e == GameLivingEvent.AttackFinished)
            {
                if (!(args is AttackFinishedEventArgs afargs))
                {
                    return;
                }

                if (afargs.AttackData.Attacker != player)
                {
                    return;
                }

                switch (afargs.AttackData.AttackResult)
                {
                    case GameLiving.eAttackResult.HitStyle:
                    case GameLiving.eAttackResult.HitUnstyled:
                    case GameLiving.eAttackResult.Blocked:
                    case GameLiving.eAttackResult.Evaded:
                    case GameLiving.eAttackResult.Fumbled:
                    case GameLiving.eAttackResult.Missed:
                    case GameLiving.eAttackResult.Parried:
                        SpeedOfSoundEffect effect = player.EffectList.GetOfType<SpeedOfSoundEffect>();
                        effect?.Cancel(false);

                        break;
                }
            }
        }

        public override void Stop()
        {
            base.Stop();
            m_owner.TempProperties.removeProperty("Charging");
            m_owner.BuffBonusMultCategory1.Remove((int)eProperty.MaxSpeed, this);
            if (m_owner is GamePlayer player)
            {
                player.Out.SendUpdateMaxSpeed();
            }

            GameEventMgr.RemoveHandler(m_owner, GameLivingEvent.AttackFinished, _attackFinished);
            GameEventMgr.RemoveHandler(m_owner, GameLivingEvent.CastFinished, _attackFinished);
        }

        /// <summary>
        /// Name of the effect
        /// </summary>
        public override string Name => "Speed of Sound";

        /// <summary>
        /// Icon ID
        /// </summary>
        public override ushort Icon => 3020;

        /// <summary>
        /// Delve information
        /// </summary>
        public override IList<string> DelveInfo
        {
            get
            {
                var delveInfoList = new List<string>
                {
                    "Gives immunity to stun/snare/root and mesmerize spells and provides unbreakeable speed.",
                    " "
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