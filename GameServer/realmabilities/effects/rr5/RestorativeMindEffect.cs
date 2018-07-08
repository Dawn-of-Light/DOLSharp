using System.Collections.Generic;

namespace DOL.GS.Effects
{
    /// <summary>
    /// Mastery of Concentration
    /// </summary>
    public class RestorativeMindEffect : TimedEffect
    {

        private GamePlayer _playerOwner;
        private RegionTimer _tickTimer;

        public RestorativeMindEffect()
            : base(30000)
        {
        }

        public override void Start(GameLiving target)
        {
            base.Start(target);
            if (target is GamePlayer player)
            {
                _playerOwner = player;
                foreach (GamePlayer p in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    p.Out.SendSpellEffectAnimation(player, player, Icon, 0, false, 1);
                }

                HealTarget();
                StartTimer();
            }
        }

        public override void Stop()
        {
            if (_tickTimer.IsAlive)
            {
                _tickTimer.Stop();
            }

            base.Stop();
        }

        private void HealTarget()
        {
            if (_playerOwner != null)
            {
                int healthtick = (int)(_playerOwner.MaxHealth * 0.05);
                int manatick = (int)(_playerOwner.MaxMana * 0.05);
                int endutick = (int)(_playerOwner.MaxEndurance * 0.05);
                if (!_playerOwner.IsAlive)
                {
                    Stop();
                }

                int modendu = _playerOwner.MaxEndurance - _playerOwner.Endurance;
                if (modendu > endutick)
                {
                    modendu = endutick;
                }

                _playerOwner.Endurance += modendu;
                int modheal = _playerOwner.MaxHealth - _playerOwner.Health;
                if (modheal > healthtick)
                {
                    modheal = healthtick;
                }

                _playerOwner.Health += modheal;
                int modmana = _playerOwner.MaxMana - _playerOwner.Mana;
                if (modmana > manatick)
                {
                    modmana = manatick;
                }

                _playerOwner.Mana += modmana;
            }
        }

        private int onTick(RegionTimer timer)
        {
            HealTarget();
            return 3000;
        }

        private void StartTimer()
        {
            _tickTimer = new RegionTimer(_playerOwner)
            {
                Callback = new RegionTimerCallback(onTick)
            };

            _tickTimer.Start(3000);
        }

        public override string Name => "Restorative Mind";

        public override ushort Icon => 3070;

        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>
                {
                    "Heals you for 5% mana/endu/hits each tick (3 seconds)"
                };

                return list;
            }
        }
    }
}