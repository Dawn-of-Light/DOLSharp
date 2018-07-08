using System;
using System.Collections.Generic;
using DOL.Events;

namespace DOL.GS.Effects
{
    /// <summary>
    /// Mastery of Concentration
    /// </summary>
    public class EntwiningSnakesEffect : TimedEffect
    {
        private GameLiving _owner;

        public EntwiningSnakesEffect()
            : base(20000)
        {
        }

        public override void Start(GameLiving target)
        {
            base.Start(target);
            target.BuffBonusMultCategory1.Set((int)eProperty.MaxSpeed, this, 1.0 - 50 * 0.01);
            _owner = target;
            GamePlayer player = _owner as GamePlayer;
            GameEventMgr.AddHandler(target, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttacked));
            if (player != null)
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
            base.Stop();
            GamePlayer player = _owner as GamePlayer;
            GameEventMgr.RemoveHandler(_owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttacked));
            if (player != null)
            {
                player.Out.SendUpdateMaxSpeed();
            }
            else if (_owner.CurrentSpeed > _owner.MaxSpeed)
            {
                _owner.CurrentSpeed = _owner.MaxSpeed;
            }
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

        public override string Name => "Entwining Snakes";

        public override ushort Icon => 3071;

        public int SpellEffectiveness => 0;

        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>
                {
                    "A breakable 50 % snare with 20 seconds duration"
                };

                return list;
            }
        }
    }
}