using System.Collections;
using DOL.GS.PacketHandler;
using DOL.Database;
using DOL.GS.Spells;

namespace DOL.GS.RealmAbilities
{
    public class BedazzlingAuraAbility : TimedRealmAbility
    {
        public BedazzlingAuraAbility(DBAbility dba, int level) : base(dba, level) { }

        private const string BofBaSb = "RA_DAMAGE_DECREASE";

        private int _range = 1500;
        private int _value;

        public override void Execute(GameLiving living)
        {
            if (!(living is GamePlayer player))
            {
                return;
            }

            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED))
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
                    case 1: _value = 10; break;
                    case 2: _value = 15; break;
                    case 3: _value = 20; break;
                    case 4: _value = 30; break;
                    case 5: _value = 40; break;
                    default: return;
                }
            }
            else
            {
                switch (Level)
                {
                    case 1: _value = 10; break;
                    case 2: _value = 20; break;
                    case 3: _value = 40; break;
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
                foreach (GamePlayer p in player.Group.GetPlayersInTheGroup())
                {
                    if (player.IsWithinRadius(p, _range) && p.IsAlive)
                    {
                        targets.Add(p);
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

                if (target.CharacterClass.Name == "Vampiir" && SpellHandler.FindEffectOnTarget(target, "VampiirMagicResistance") != null)
                {
                    continue;
                }

                var success = !target.TempProperties.getProperty(BofBaSb, false);
                foreach (GamePlayer visPlayer in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    visPlayer.Out.SendSpellEffectAnimation(player, target, 7030, 0, false, CastSuccess(success));
                }

                if (success)
                {
                    new BedazzlingAuraEffect().Start(target, _value);
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
