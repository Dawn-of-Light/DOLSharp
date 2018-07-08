using System.Collections.Generic;

namespace DOL.GS.Effects
{
    /// <summary>
    /// Adrenaline Rush
    /// </summary>
    public class BlindingDustEffect : TimedEffect
    {

        public BlindingDustEffect()
            : base(15000)
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

            _owner.AbilityBonus[(int)eProperty.FumbleChance] += 25;
        }

        public override string Name => "Blinding Dust";

        public override ushort Icon => 3039;

        public override void Stop()
        {
            _owner.AbilityBonus[(int)eProperty.FumbleChance] -= 25;
            base.Stop();
        }

        public int SpellEffectiveness => 100;

        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>
                {
                    "Insta-cast PBAE Attack that causes the enemy to have a 25% chance to fumble melee/bow attacks for the next 15 seconds."
                };

                return list;
            }
        }
    }
}