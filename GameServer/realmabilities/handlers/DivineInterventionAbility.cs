using DOL.GS.PacketHandler;
using DOL.Database;

namespace DOL.GS.RealmAbilities
{
    public class DivineInterventionAbility : TimedRealmAbility
    {
        public DivineInterventionAbility(DBAbility dba, int level) : base(dba, level) { }

        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED))
            {
                return;
            }

            if (living is GamePlayer player)
            {
                Group playerGroup = player.Group;

                if (playerGroup == null)
                {
                    player.Out.SendMessage("You must be in a group to use this ability!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    return;
                }

                int poolValue = 0;

                if (ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
                {
                    switch (Level)
                    {
                        case 1: poolValue = 1000; break;
                        case 2: poolValue = 1500; break;
                        case 3: poolValue = 2000; break;
                        case 4: poolValue = 2500; break;
                        case 5: poolValue = 3000; break;
                    }
                }
                else
                {
                    switch (Level)
                    {
                        case 1: poolValue = 1000; break;
                        case 2: poolValue = 2000; break;
                        case 3: poolValue = 3000; break;
                    }
                }

                foreach (GamePlayer groupMember in playerGroup.GetPlayersInTheGroup())
                {
                    DivineInterventionEffect diEffect = groupMember.EffectList.GetOfType<DivineInterventionEffect>();
                    if (diEffect != null)
                    {
                        player.Out.SendMessage("You are already protected by a pool of healing", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                        return;
                    }
                }

                DisableSkill(living);
                new DivineInterventionEffect(poolValue).Start(player);
            }
        }

        public override int GetReUseDelay(int level)
        {
            return 600;
        }
    }
}
