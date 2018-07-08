using System.Collections.Generic;

namespace DOL.GS.Effects
{
    /// <summary>
    /// Adrenaline Rush
    /// </summary>
    public class WhirlingStaffEffect : TimedEffect
    {
        public WhirlingStaffEffect()
            : base(6000)
        {
        }

        public override void Start(GameLiving target)
        {
            base.Start(target);
            if (target is GamePlayer player)
            {
                foreach (GamePlayer p in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    p.Out.SendSpellEffectAnimation(player, player, Icon, 0, false, 1);
                }
            }

            // target.IsDisarmed = true;
            target.DisarmedTime = target.CurrentRegion.Time + m_duration;
            target.StopAttack();
        }

        public override string Name => "Whirling Staff";

        public override ushort Icon => 3042;

        public int SpellEffectiveness => 100;

        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>
                {
                    "Disarms you for 6 seconds!"
                };

                return list;
            }
        }
    }
}