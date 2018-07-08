using System.Collections;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Database;

namespace DOL.GS.RealmAbilities
{

    public class SoldiersBarricadeAbility : TimedRealmAbility
    {
        public SoldiersBarricadeAbility(DBAbility dba, int level) : base(dba, level) { }

        public const string BofBaSb = "RA_DAMAGE_DECREASE";

        private int _range = 1500;
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

            if (player.TempProperties.getProperty(BofBaSb, false))
            {
                player.Out.SendMessage("You already an effect of that type!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                return;
            }

            if (ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
            {
                switch (Level)
                {
                    case 1: _value = 5; break;
                    case 2: _value = 10; break;
                    case 3: _value = 15; break;
                    case 4: _value = 20; break;
                    case 5: _value = 30; break;
                    default: return;
                }
            }
            else
            {
                switch (Level)
                {
                    case 1: _value = 5; break;
                    case 2: _value = 15; break;
                    case 3: _value = 25; break;
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
                    if (player.IsWithinRadius(grpMate, _range) && grpMate.IsAlive)
                    {
                        targets.Add(grpMate);
                    }
                }
            }

            foreach (GamePlayer target in targets)
            {
                // send spelleffect
                if (!target.IsAlive)
                {
                    continue;
                }

                var success = !target.TempProperties.getProperty(BofBaSb, false);
                foreach (GamePlayer visPlayer in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    visPlayer.Out.SendSpellEffectAnimation(player, target, 7015, 0, false, System.Convert.ToByte(success));
                }

                if (success)
                {
                    new SoldiersBarricadeEffect().Start(target, _duration, _value);
                }
            }
        }

        public override int GetReUseDelay(int level)
        {
            return 600;
        }
    }
}
