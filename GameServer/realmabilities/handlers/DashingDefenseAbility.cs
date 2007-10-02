using System.Reflection;
using System.Collections;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Database;

namespace DOL.GS.RealmAbilities
{
    public class DashingDefenseAbility : TimedRealmAbility
    {
        public DashingDefenseAbility(DBAbility dba, int level) : base(dba, level) { }

        public const string Dashing = "Dashing";

        //private RegionTimer m_expireTimer;
        int m_duration = 1;
        int m_range = 1000;
        //private GamePlayer m_player;

        public override void Execute(GameLiving living)
        {
            GamePlayer player = living as GamePlayer;
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;
            if (player.TempProperties.getProperty(Dashing, false))
            {
                player.Out.SendMessage("You already an effect of that type!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                return;
            }

            switch (Level)
            {
                case 1: m_duration = 10; break;
                case 2: m_duration = 30; break;
                case 3: m_duration = 60; break;
                default: return;
            }

            DisableSkill(living);

            ArrayList targets = new ArrayList();
            if (player.PlayerGroup == null)
                {
                    player.Out.SendMessage("You must be in a group to use this ability!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    return;
                }
            else foreach (GamePlayer grpMate in player.PlayerGroup.GetPlayersInTheGroup())
                    if (WorldMgr.CheckDistance(grpMate, player, m_range) && grpMate.IsAlive)
                        targets.Add(grpMate);

            bool success;
            foreach (GamePlayer target in targets)
            {
                //send spelleffect
                if (!target.IsAlive) continue;
                success = !target.TempProperties.getProperty(Dashing, false);
                if (success)
                    if (target != null && target != player)
                    {
                        new DashingDefenseEffect().Start(player, target, m_duration);
                    }
            }

        }

        public override int GetReUseDelay(int level)
        {
            return 420;
        }
    }
}
