using System;
using DOL.Database2;
using DOL.GS.Effects;

namespace DOL.GS.RealmAbilities
{
    /// <summary>
    /// Arms Length Realm Ability
    /// </summary>
    public class AllureOfDeathAbility : RR5RealmAbility
    {
        public AllureOfDeathAbility(DBAbility dba, int level) : base(dba, level) { }

        /// <summary>
        /// Action
        /// </summary>
        /// <param name="living"></param>
        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;



            GamePlayer player = living as GamePlayer;
            if (player != null)
            {
                SendCasterSpellEffectAndCastMessage(player, 7076, true);
                AllureofDeathEffect effect = new AllureofDeathEffect();
                effect.Start(player);
            }
            DisableSkill(living);
        }

        public override int GetReUseDelay(int level)
        {
            return 420;
        }

        public override void AddEffectsInfo(System.Collections.IList list)
        {
            list.Add("Allure of Death.");
            list.Add("");
            list.Add("Target: Self");
            list.Add("Duration: 60 seconds");
            list.Add("Casting time: instant");
        }

    }
}