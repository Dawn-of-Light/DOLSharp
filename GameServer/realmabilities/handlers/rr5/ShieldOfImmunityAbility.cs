using System.Collections.Generic;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;

namespace DOL.GS.RealmAbilities
{
    /// <summary>
    /// Mastery of Concentration RA
    /// </summary>
    public class ShieldOfImmunityAbility : RR5RealmAbility
    {
        public ShieldOfImmunityAbility(DBAbility dba, int level) : base(dba, level) { }

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

            if (!(living is GamePlayer player))
            {
                return;
            }

            // Check for MoC on the Sorceror: he cannot cast RA5L when the other is up
            MasteryofConcentrationEffect ra5L = null;
            lock (player.EffectList)
            {
                foreach (IGameEffect effect in player.EffectList)
                {
                    if (effect is MasteryofConcentrationEffect)
                    {
                        ra5L = effect as MasteryofConcentrationEffect;
                        break;
                    }
                }
            }

            if (ra5L != null)
            {
                player.Out.SendMessage("You cannot currently use this ability", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                return;
            }

            SendCasterSpellEffectAndCastMessage(player, 7048, true);
            ShieldOfImmunityEffect raEffect = new ShieldOfImmunityEffect();
            raEffect.Start(player);

            DisableSkill(living);
        }

        public override int GetReUseDelay(int level)
        {
            return 900;
        }

        public override void AddEffectsInfo(IList<string> list)
        {
            list.Add("Shield that absorbs 90% melee/archer damage for 20 seconds.");
            list.Add(string.Empty);
            list.Add("Target: Self");
            list.Add("Duration: 20 sec");
            list.Add("Casting time: instant");
        }
    }
}