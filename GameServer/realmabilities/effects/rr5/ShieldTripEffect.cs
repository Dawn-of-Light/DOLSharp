using System;
using System.Collections.Generic;
using DOL.Events;

namespace DOL.GS.Effects
{
    /// <summary>
    /// Adrenaline Rush
    /// </summary>
    public class ShieldTripDisarmEffect : TimedEffect
    {

        public ShieldTripDisarmEffect()
            : base(15000)
        {
        }

        public override void Start(GameLiving target)
        {
            base.Start(target);

            // target.IsDisarmed = true;
            target.DisarmedTime = target.CurrentRegion.Time + m_duration;
            target.StopAttack();
        }

        public override string Name => "Shield Trip";

        public override ushort Icon { get; } = 3045;

        public int SpellEffectiveness => 100;

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

    public class ShieldTripRootEffect : TimedEffect
    {
        private GameLiving _owner;

        public ShieldTripRootEffect()
            : base(10000)
        {
        }

        public override void Start(GameLiving target)
        {
            base.Start(target);
            target.BuffBonusMultCategory1.Set((int)eProperty.MaxSpeed, this, 1.0 - 99 * 0.01);
            _owner = target;
            GameEventMgr.AddHandler(target, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttacked));
            if (_owner is GamePlayer player)
            {
                player.Out.SendUpdateMaxSpeed();
            }
            else
            {
                _owner.CurrentSpeed = _owner.MaxSpeed;
            }
        }

        public override void Stop()
        {
            _owner.BuffBonusMultCategory1.Remove((int)eProperty.MaxSpeed, this);
            GameEventMgr.RemoveHandler(_owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttacked));
            if (_owner is GamePlayer player)
            {
                player.Out.SendUpdateMaxSpeed();
            }
            else
            {
                _owner.CurrentSpeed = _owner.MaxSpeed;
            }

            base.Stop();
        }

        protected virtual void OnAttacked(DOLEvent e, object sender, EventArgs arguments)
        {
            if (!(arguments is AttackedByEnemyEventArgs attackArgs))
            {
                return;
            }

            switch (attackArgs.AttackData.AttackResult)
            {
                case GameLiving.eAttackResult.HitStyle:
                case GameLiving.eAttackResult.HitUnstyled:
                    Stop();
                    break;
            }
        }

        public override string Name => "Shield Trip";

        public override ushort Icon => 7046;

        public int SpellEffectiveness => 0;

        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>
                {
                    "Root Effect"
                };

                return list;
            }
        }
    }
}