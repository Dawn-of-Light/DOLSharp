using System.Collections.Generic;
using DOL.Database;
using DOL.GS.Effects;

namespace DOL.GS.RealmAbilities
{
    /// <summary>
    /// Mastery of Concentration RA
    /// </summary>
    public class RuneOfUtterAgilityAbility : RR5RealmAbility
    {
        public RuneOfUtterAgilityAbility(DBAbility dba, int level) : base(dba, level) { }

        /// <summary>
        /// Action
        /// </summary>
        /// <param name="living"></param>
        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED))
            {
                return;
            }

            if (living is GamePlayer player)
            {
                SendCasterSpellEffectAndCastMessage(player, 7074, true);
                RuneOfUtterAgilityEffect effect = new RuneOfUtterAgilityEffect();
                effect.Start(player);
            }

            DisableSkill(living);
        }

        public override int GetReUseDelay(int level)
        {
            return 600;
        }

        public override void AddEffectsInfo(IList<string> list)
        {
            list.Add("Runemaster gets a 90% chance to evade all melee attacks (regardless of direction) for 15 seconds.");
            list.Add(string.Empty);
            list.Add("Target: Self");
            list.Add("Duration: 15 sec");
            list.Add("Casting time: instant");
        }
    }
}