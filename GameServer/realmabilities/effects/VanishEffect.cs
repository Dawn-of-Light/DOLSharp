using System.Collections.Generic;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.RealmAbilities
{
    /// <summary>
    /// Vanish
    /// </summary>
    public class VanishEffect : TimedEffect
    {
        public const string VANISH_BLOCK_ATTACK_TIME_KEY = "vanish_no_attack";

        private int _countdown;
        private RegionTimer _mCountDownTimer;
        private RegionTimer _mRemoveTimer;

        public VanishEffect(int duration, double speedBonus)
            : base(duration)
        {
            SpeedBonus = speedBonus;
            _countdown = (duration + 500) / 1000;
        }

        public override void Start(GameLiving target)
        {
            base.Start(target);
            if (target is GamePlayer player)
            {
                player.StopAttack();
                player.Stealth(true);
                player.Out.SendUpdateMaxSpeed();
                _mCountDownTimer = new RegionTimer(player, new RegionTimerCallback(CountDown));
                _mCountDownTimer.Start(1);
                player.TempProperties.setProperty(VANISH_BLOCK_ATTACK_TIME_KEY, player.CurrentRegion.Time + 30000);
                _mRemoveTimer = new RegionTimer(player, new RegionTimerCallback(RemoveAttackBlock));
            }
            _mRemoveTimer.Start(30000);
        }

        public int RemoveAttackBlock(RegionTimer timer)
        {
            if (timer.Owner is GamePlayer player)
            {
                player.TempProperties.removeProperty(VANISH_BLOCK_ATTACK_TIME_KEY);
            }

            return 0;
        }

        public override void Stop()
        {
            base.Stop();
            if (Owner is GamePlayer player)
            {
                player.Out.SendUpdateMaxSpeed();
            }

            if (_mCountDownTimer != null)
            {
                _mCountDownTimer.Stop();
                _mCountDownTimer = null;
            }
        }

        public int CountDown(RegionTimer timer)
        {
            if (_countdown > 0)
            {
                ((GamePlayer)Owner).Out.SendMessage($"You are hidden for {_countdown} more seconds!", eChatType.CT_SpellPulse, eChatLoc.CL_SystemWindow);
                _countdown--;
                return 1000;
            }

            return 0;
        }

        public double SpeedBonus { get; }

        public override string Name => "Vanish";

        public override ushort Icon => 3019;

        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>
                {
                    "Vanish effect"
                };

                return list;
            }
        }
    }
}