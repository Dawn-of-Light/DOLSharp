using System.Collections.Generic;

namespace DOL.GS.Effects
{
    /// <summary>
    /// Mastery of Concentration
    /// </summary>
    public class RuneOfUtterAgilityEffect : TimedEffect
    {
        private GameLiving _owner;

        public RuneOfUtterAgilityEffect()
            : base(15000)
        {
        }

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

                player.BuffBonusCategory4[(int)eProperty.EvadeChance] += 90;
            }
        }

        public override void Stop()
        {
            if (_owner is GamePlayer player)
            {
                player.BuffBonusCategory4[(int)eProperty.EvadeChance] -= 90;
            }

            base.Stop();
        }

        public override string Name => "Rune Of Utter Agility";

        public override ushort Icon => 3073;

        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>
                {
                    "Increases your evade chance up to 90% for 30 seconds."
                };

                return list;
            }
        }
    }
}