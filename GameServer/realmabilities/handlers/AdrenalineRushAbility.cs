using System.Reflection;
using System.Collections;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Database;

namespace DOL.GS.RealmAbilities
{
    public class AdrenalineRushAbility : TimedRealmAbility
    {
        int m_duration = 20;
        int m_value = 100;

        public AdrenalineRushAbility(DBAbility dba, int level) : base(dba, level) { }

        public override void Execute(GameLiving living)
        {
            GamePlayer player = living as GamePlayer;
            if (CheckPreconditions(living, DEAD | SITTING | STEALTHED)) return;
            if (player.EffectList.CountOfType(typeof(AdrenalineRushEffect)) > 0)
            {
                player.Out.SendMessage("You already an effect of that type!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
            }
            SendCasterSpellEffectAndCastMessage(living, 7002, true);
            if (player != null)
            {
                new AdrenalineRushEffect().Start(player, m_duration, m_value);
            }
            DisableSkill(living);
        }
        public override int GetReUseDelay(int level)
        {
            switch (level)
            {
                case 1: return 1200;
                case 2: return 600;
                case 3: return 300;
            }
            return 600;
        }
    }
}
