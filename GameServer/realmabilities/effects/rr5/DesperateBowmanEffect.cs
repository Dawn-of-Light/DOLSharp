using System.Collections.Generic;

namespace DOL.GS.Effects
{
    /// <summary>
    /// Adrenaline Rush
    /// </summary>
    public class DesperateBowmanDisarmEffect : TimedEffect
    {
        public DesperateBowmanDisarmEffect()
            : base(15000)
        {
        }

        public override void Start(GameLiving target)
        {
            base.Start(target);
            target.DisarmedTime = target.CurrentRegion.Time + m_duration;
            target.SilencedTime = target.CurrentRegion.Time + m_duration;
            target.StopAttack();
            target.StopCurrentSpellcast();
        }

        public override string Name => "Desperate Bowman";

        public override ushort Icon => 3060;

        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>
                {
                    "Disarms you for 15 seconds!"
                };

                return list;
            }
        }
    }

    public class DesperateBowmanStunEffect : TimedEffect
    {
        public DesperateBowmanStunEffect()
            : base(5000)
        {
        }

        public override void Start(GameLiving target)
        {
            base.Start(target);
            target.IsStunned = true;
            target.StopAttack();
            target.StopCurrentSpellcast();
            target.DisableTurning(true);
            (target as GamePlayer)?.Out.SendUpdateMaxSpeed();
        }

        public override void Stop()
        {
            base.Stop();
            m_owner.IsStunned = false;
            m_owner.DisableTurning(false);
            (m_owner as GamePlayer)?.Out.SendUpdateMaxSpeed();
        }

        public override string Name => "Desperate Bowman";

        public override ushort Icon => 3060;

        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>
                {
                    "Stun Effect"
                };

                return list;
            }
        }
    }
}