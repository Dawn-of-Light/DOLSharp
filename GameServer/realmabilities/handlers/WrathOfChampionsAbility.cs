using DOL.GS.PacketHandler;
using DOL.Database;
using DOL.GS.Spells;

namespace DOL.GS.RealmAbilities
{
    public class WrathofChampionsAbility : TimedRealmAbility
    {
        public WrathofChampionsAbility(DBAbility dba, int level) : base(dba, level) { }

        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED))
            {
                return;
            }

            if (!(living is GamePlayer caster))
            {
                return;
            }

            int dmgValue = 0;
            if (ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
            {
                switch (Level)
                {
                    case 1: dmgValue = 200; break;
                    case 2: dmgValue = 350; break;
                    case 3: dmgValue = 500; break;
                    case 4: dmgValue = 625; break;
                    case 5: dmgValue = 750; break;
                }
            }
            else
            {
                switch (Level)
                {
                    case 1: dmgValue = 200; break;
                    case 2: dmgValue = 500; break;
                    case 3: dmgValue = 750; break;
                }
            }

            // send cast messages
            foreach (GamePlayer iPlayer in caster.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
            {
                if (iPlayer == caster)
                {
                    iPlayer.MessageToSelf($"You cast {Name}!", eChatType.CT_Spell);
                }
                else
                {
                    iPlayer.MessageFromArea(caster, $"{caster.Name} casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                }
            }

            // deal damage to npcs
            foreach (GameNPC mob in caster.GetNPCsInRadius(200))
            {
                if (GameServer.ServerRules.IsAllowedToAttack(caster, mob, true) == false)
                {
                    continue;
                }

                mob.TakeDamage(caster, eDamageType.Spirit, dmgValue, 0);
                caster.Out.SendMessage($"You hit the {mob.Name} for {dmgValue} damage.", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                foreach (GamePlayer player2 in caster.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    player2.Out.SendSpellCastAnimation(caster, 4468, 0);
                    player2.Out.SendSpellEffectAnimation(caster, mob, 4468, 0, false, 1);
                }
            }

            // deal damage to players
            foreach (GamePlayer tPlayer in caster.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                if (GameServer.ServerRules.IsAllowedToAttack(caster, tPlayer, true) == false)
                {
                    continue;
                }

                // Check to see if the player is phaseshifted
                var phaseshift = SpellHandler.FindEffectOnTarget(tPlayer, "Phaseshift");
                if (phaseshift != null)
                {
                    caster.Out.SendMessage($"{tPlayer.Name} is Phaseshifted and can\'t be effected by this Spell!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    continue;
                }

                if (!caster.IsWithinRadius(tPlayer, 200))
                {
                    continue;
                }

                tPlayer.TakeDamage(caster, eDamageType.Spirit, dmgValue, 0);

                // send a message
                caster.Out.SendMessage($"You hit {tPlayer.Name} for {dmgValue} damage.", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                tPlayer.Out.SendMessage($"{caster.Name} hits you for {dmgValue} damage.", eChatType.CT_YouWereHit, eChatLoc.CL_SystemWindow);

                foreach (GamePlayer nPlayer in tPlayer.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    nPlayer.Out.SendSpellCastAnimation(caster, 4468, 0);
                    nPlayer.Out.SendSpellEffectAnimation(caster, tPlayer, 4468, 0, false, 1);
                }
            }

            DisableSkill(living);
            caster.LastAttackTickPvP = caster.CurrentRegion.Time;
        }

        public override int GetReUseDelay(int level)
        {
            return 600;
        }
    }
}
