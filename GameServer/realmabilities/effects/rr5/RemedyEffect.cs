using System.Collections.Generic;

namespace DOL.GS.Effects
{
    /// <summary>
    /// Adrenaline Rush
    /// </summary>
    public class RemedyEffect : TimedEffect
    {
        public RemedyEffect()
            : base(60000)
        {
        }

        private GameLiving _owner;
        private int _healthdrain;

        public override void Start(GameLiving target)
        {
            base.Start(target);
            _owner = target;
            if (target is GamePlayer player)
            {
                foreach (GamePlayer p in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    p.Out.SendSpellEffectAnimation(player, player, Icon, 0, false, 1);
                }
            }

            _healthdrain = (int)(target.MaxHealth * 0.1);
            if (target.Health <= _healthdrain)
            {
                return;
            }

            target.TakeDamage(target, eDamageType.Body, _healthdrain, 0);
        }

        public override string Name => "Remedy";

        public override ushort Icon => 3059;

        public override void Stop()
        {
            if (!_owner.IsAlive)
            {
                base.Stop();
                return;
            }

            _owner.Health += _healthdrain;
            base.Stop();
        }

        public int SpellEffectiveness => 100;

        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>
                {
                    "For 60 seconds you're immune to all weapon poisons"
                };

                return list;
            }
        }
    }
}