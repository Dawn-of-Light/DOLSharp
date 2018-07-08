using System.Collections.Generic;

namespace DOL.GS.Effects
{
    /// <summary>
    /// Adrenaline Rush
    /// </summary>
    public class FerociousWillEffect : TimedEffect
    {

        private int m_currentBonus = 25;

        public FerociousWillEffect()
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
                    p.Out.SendSpellEffectAnimation(player, player, Icon, 0, false, 1);
                }
            }

            _owner.BaseBuffBonusCategory[(int)eProperty.ArmorAbsorption] += m_currentBonus;
        }

        public override string Name => "Ferocious Will";

        public override ushort Icon => 3064;

        public override void Stop()
        {
            _owner.BaseBuffBonusCategory[(int)eProperty.ArmorAbsorption] -= m_currentBonus;
            base.Stop();
        }

        public int SpellEffectiveness => 100;

        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>
                {
                    "Gives the Berzerker an ABS buff of 25%. Lasts 30 seconds total."
                };

                return list;
            }
        }
    }
}
