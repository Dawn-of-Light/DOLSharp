using System.Collections.Generic;
using DOL.Database;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.RealmAbilities
{
    /// <summary>
    /// Arms Length Realm Ability
    /// </summary>
    public class ArmsLengthAbility : RR5RealmAbility
    {
        public ArmsLengthAbility(DBAbility dba, int level) : base(dba, level) { }

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
                if (player.TempProperties.getProperty("Charging", false)
                    || player.EffectList.CountOfType(typeof(SpeedOfSoundEffect), typeof(ArmsLengthEffect), typeof(ChargeEffect)) > 0)
                {
                    player.Out.SendMessage("You already an effect of that type!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    return;
                }

                GameSpellEffect speed = Spells.SpellHandler.FindEffectOnTarget(player, "SpeedEnhancement");
                speed?.Cancel(false);

                new ArmsLengthEffect().Start(player);
                SendCasterSpellEffectAndCastMessage(player, 7068, true);
            }

            DisableSkill(living);
        }

        public override int GetReUseDelay(int level)
        {
            return 600;
        }

        public override void AddEffectsInfo(IList<string> list)
        {
            list.Add("10 second unbreakable burst of extreme speed.");
            list.Add(string.Empty);
            list.Add("Target: Self");
            list.Add("Duration: 10 sec");
            list.Add("Casting time: instant");
        }
    }
}
