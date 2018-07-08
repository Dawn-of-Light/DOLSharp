using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;

namespace DOL.GS.RealmAbilities
{
    public class BadgeOfValorAbilityHandler : RR5RealmAbility
    {
        public BadgeOfValorAbilityHandler(DBAbility dba, int level) : base(dba, level) { }

        int m_reuseTimer = 900;

        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED))
            {
                return;
            }

            if (living.EffectList.CountOfType<BadgeOfValorEffect>() > 0)
            {
                (living as GamePlayer)?.Out.SendMessage("You already an effect of that type!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);

                return;
            }

            // send spelleffect
            foreach (GamePlayer visPlayer in living.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                visPlayer.Out.SendSpellEffectAnimation(living, living, 7057, 0, false, 0x01);
            }

            new BadgeOfValorEffect().Start(living);
            living.DisableSkill(this, m_reuseTimer * 1000);
        }
    }
}
