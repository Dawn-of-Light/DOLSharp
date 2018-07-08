using System.Collections;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Database;

namespace DOL.GS.RealmAbilities
{
    public class StrikePredictionAbility : TimedRealmAbility
    {
        public StrikePredictionAbility(DBAbility dba, int level) : base(dba, level) { }

        private int _range = 2000;
        private int _duration = 30;
        private int _value;

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

            if (player.EffectList.CountOfType<StrikePredictionEffect>() > 0)
            {
                player.Out.SendMessage("You already have an effect of that type!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
            }

            if (ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
            {
                switch (Level)
                {
                    case 1: _value = 5; break;
                    case 2: _value = 7; break;
                    case 3: _value = 10; break;
                    case 4: _value = 15; break;
                    case 5: _value = 20; break;
                    default: return;
                }
            }
            else
            {
                switch (Level)
                {
                    case 1: _value = 5; break;
                    case 2: _value = 10; break;
                    case 3: _value = 20; break;
                    default: return;
                }
            }

            DisableSkill(living);
            ArrayList targets = new ArrayList();
            if (player.Group == null)
            {
                targets.Add(player);
            }
            else
            {
                foreach (GamePlayer grpMate in player.Group.GetPlayersInTheGroup())
                {
                    if (grpMate.IsWithinRadius(player, _range) && grpMate.IsAlive)
                    {
                        targets.Add(grpMate);
                    }
                }
            }

            foreach (GamePlayer target in targets)
            {
                var success = target.EffectList.CountOfType<StrikePredictionEffect>() == 0;
                foreach (GamePlayer visPlayer in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    visPlayer.Out.SendSpellEffectAnimation(player, target, 7037, 0, false, CastSuccess(success));
                }

                if (success)
                {
                    new StrikePredictionEffect().Start(target, _duration, _value);
                }
            }
        }

        private byte CastSuccess(bool suc)
        {
            if (suc)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public override int GetReUseDelay(int level)
        {
            return 600;
        }
    }
}
