using System.Collections.Generic;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.RealmAbilities
{
    /// <summary>
    /// Ameliorating Melodies
    /// </summary>
    public class AmelioratingMelodiesEffect : TimedEffect
    {
        /// <summary>
        /// The countdown value. If this value is 0, the effect vanishes
        /// </summary>
        int _countdown;

        /// <summary>
        /// The number of hit points healed each tick
        /// </summary>
        readonly int _heal;

        /// <summary>
        /// Max healing range
        /// </summary>
        int _range;

        /// <summary>
        /// The rgion timer
        /// </summary>
        RegionTimer _countDownTimer;

        /// <summary>
        /// Ameliorating Melodies
        /// </summary>
        /// <param name="heal">Delve value hit points per tick"</param>
        public AmelioratingMelodiesEffect(int heal)
            : base(30000)
        {
            _heal = heal;
            _range = 2000;
            _countdown = 10;
        }

        /// <summary>
        /// Starts the effect
        /// </summary>
        /// <param name="target">The player of this effect</param>
        public override void Start(GameLiving target)
        {
            base.Start(target);
            if (!(target is GamePlayer player))
            {
                return;
            }

            player.EffectList.Add(this);
            _range = (int)(2000 * (player.GetModified(eProperty.SpellRange) * 0.01));
            _countDownTimer = new RegionTimer(player, new RegionTimerCallback(CountDown));
            _countDownTimer.Start(1);
        }

        /// <summary>
        /// Stops the effect
        /// </summary>
        public override void Stop()
        {
            base.Stop();
            Owner.EffectList.Remove(this);
            if (_countDownTimer != null)
            {
                _countDownTimer.Stop();
                _countDownTimer = null;
            }
        }

        /// <summary>
        /// Timer callback
        /// </summary>
        /// <param name="timer">The region timer</param>
        public int CountDown(RegionTimer timer)
        {
            if (_countdown > 0)
            {
                _countdown--;
                if (!(Owner is GamePlayer player))
                {
                    return 0;
                }

                if (player.Group == null)
                {
                    return 3000;
                }

                foreach (GamePlayer p in player.Group.GetPlayersInTheGroup())
                {
                    if ((p != player) && p.Health < p.MaxHealth && player.IsWithinRadius(p, _range) && p.IsAlive)
                    {
                        if (player.IsStealthed)
                        {
                            player.Stealth(false);
                        }

                        int heal = _heal;
                        if (p.Health + heal > p.MaxHealth)
                        {
                            heal = p.MaxHealth - p.Health;
                        }

                        p.ChangeHealth(player, GameLiving.eHealthChangeType.Regenerate, heal);
                        player.Out.SendMessage($"You heal {p.Name} for {heal} hit points.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                        p.Out.SendMessage($"{player.Name} heals you for {heal} hit points.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                    }
                }

                return 3000;
            }

            return 0;
        }

        /// <summary>
        /// Name of the effect
        /// </summary>
        public override string Name => "Ameliorating Melodies";

        /// <summary>
        /// Icon of the effect
        /// </summary>
        public override ushort Icon => 3021;

        /// <summary>
        /// Delve information
        /// </summary>
        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>(8)
                {
                    "Ameliorating Melodies",
                    " ",
                    $"Value: {_heal} / tick",
                    "Target: Group",
                    $"Range: {_range}",
                    $"Duration: 30 s ({(_countdown * 3)} s remaining)"
                };

                return list;
            }
        }
    }
}