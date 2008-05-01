using System.Reflection;
using System.Collections;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Database;

namespace DOL.GS.RealmAbilities
{
    public class StrikePredictionAbility : TimedRealmAbility
    {
        public StrikePredictionAbility(DBAbility dba, int level) : base(dba, level) { }
        int m_range = 2000;
        int m_duration = 30;
        int m_value = 0;
        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;
            GamePlayer player = living as GamePlayer;
            if (player.EffectList.CountOfType(typeof(StrikePredictionEffect)) > 0)
            {
                player.Out.SendMessage("You already have an effect of that type!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
            }
            switch (Level)
            {
                case 1: m_value = 5; break;
                case 2: m_value = 10; break;
                case 3: m_value = 20; break;
                default: return;
            }
            DisableSkill(living);
            ArrayList targets = new ArrayList();
            if (player.Group == null)
                targets.Add(player);
            else
                foreach (GamePlayer grpMate in player.Group.GetPlayersInTheGroup())
                    if (WorldMgr.CheckDistance(grpMate, player, m_range) && grpMate.IsAlive)
                        targets.Add(grpMate);
            bool success;
            foreach (GamePlayer target in targets)
            {
                success = (target.EffectList.CountOfType(typeof(StrikePredictionEffect)) == 0);
                foreach (GamePlayer visPlayer in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                    visPlayer.Out.SendSpellEffectAnimation(player, target, 7037, 0, false, CastSuccess(success));
                if (success)
                    if (target != null)
                    {
                        new StrikePredictionEffect().Start(target, m_duration, m_value);
                    }
            }

        }
        private byte CastSuccess(bool suc)
        {
            if (suc)
                return 1;
            else
                return 0;
        }
        public override int GetReUseDelay(int level)
        {
            return 600;
        }
    }
}
