using System;
using System.Collections.Generic;
using DOL.Events;

namespace DOL.GS.Effects
{
    /// <summary>
    /// Adrenaline Rush
    /// </summary>
    public class RetributionOfTheFaithfulStunEffect : TimedEffect
    {
        public RetributionOfTheFaithfulStunEffect()
            : base(3000)
        { }

        private GameLiving _owner;

        public override void Start(GameLiving target)
        {
            base.Start(target);
            _owner = target;
            foreach (GamePlayer p in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                p.Out.SendSpellEffectAnimation(target, target, 7042, 0, false, 1);
            }

            _owner.IsStunned = true;
            _owner.StopAttack();
            _owner.StopCurrentSpellcast();
            _owner.DisableTurning(true);
            if (_owner is GamePlayer player)
            {
                player.Out.SendUpdateMaxSpeed();
            }
            else if (_owner.CurrentSpeed > _owner.MaxSpeed)
            {
                _owner.CurrentSpeed = _owner.MaxSpeed;
            }
        }

        public override string Name => "Retribution Of The Faithful";

        public override ushort Icon => 3041;

        public override void Stop()
        {
            _owner.IsStunned = false;
            _owner.DisableTurning(false);
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

        public int SpellEffectiveness => 100;

        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>
                {
                    "Stuns you for the brief duration of 3 seconds"
                };

                return list;
            }
        }
    }

    public class RetributionOfTheFaithfulEffect : TimedEffect
    {

        public RetributionOfTheFaithfulEffect()
            : base(30000)
        {
        }

        private GameLiving _owner;

        public override void Start(GameLiving target)
        {
            base.Start(target);
            _owner = target;
            if (target is GamePlayer player)
            {
                foreach (GamePlayer p in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    p.Out.SendSpellEffectAnimation(player, player, 7042, 0, false, 1);
                }
            }

            GameEventMgr.AddHandler(target, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
        }

        private void OnAttack(DOLEvent e, object sender, EventArgs arguments)
        {
            if (!(arguments is AttackedByEnemyEventArgs args))
            {
                return;
            }

            if (args.AttackData == null)
            {
                return;
            }

            if (!args.AttackData.IsMeleeAttack)
            {
                return;
            }

            // FIXME: [WARN] this has been commented out, it should be handled somewhere
            if (args.AttackData.Attacker.EffectList.GetOfType<ChargeEffect>() != null || args.AttackData.Attacker.TempProperties.getProperty("Charging", false))
            {
                return;
            }

            if (!_owner.IsWithinRadius(args.AttackData.Attacker, 300))
            {
                return;
            }

            if (Util.Chance(50))
            {
                RetributionOfTheFaithfulStunEffect effect = new RetributionOfTheFaithfulStunEffect();
                effect.Start(args.AttackData.Attacker);
            }
        }

        public override string Name => "Retribution Of The Faithful";

        public override ushort Icon => 3041;

        public override void Stop()
        {
            GameEventMgr.RemoveHandler(_owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
            base.Stop();
        }

        public int SpellEffectiveness => 100;

        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>
                {
                    "30 second buff that has a 50% chance to proc a 3 second (duration undiminished by resists) stun on any melee attack on the cleric."
                };

                return list;
            }
        }
    }
}
