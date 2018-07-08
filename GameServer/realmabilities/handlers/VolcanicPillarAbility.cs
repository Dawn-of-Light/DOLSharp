using DOL.GS.PacketHandler;
using DOL.Database;

namespace DOL.GS.RealmAbilities
{
    public class VolcanicPillarAbility : TimedRealmAbility
    {
        public VolcanicPillarAbility(DBAbility dba, int level) : base(dba, level) { }

        private int _dmgValue;
        private GamePlayer _caster;

        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED))
            {
                return;
            }

            _caster = living as GamePlayer;
            if (_caster == null)
            {
                return;
            }

            if (_caster.TargetObject == null)
            {
                _caster.Out.SendMessage("You need a target for this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                _caster.DisableSkill(this, 3 * 1000);
                return;
            }

            if (!_caster.IsWithinRadius(_caster.TargetObject, (int)(1500 * _caster.GetModified(eProperty.SpellRange) * 0.01)))
            {
                _caster.Out.SendMessage($"{_caster.TargetObject} is too far away.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                return;
            }

            if (ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
            {
                switch (Level)
                {
                    case 1: _dmgValue = 200; break;
                    case 2: _dmgValue = 350; break;
                    case 3: _dmgValue = 500; break;
                    case 4: _dmgValue = 625; break;
                    case 5: _dmgValue = 750; break;
                    default: return;
                }
            }
            else
            {
                switch (Level)
                {
                    case 1: _dmgValue = 200; break;
                    case 2: _dmgValue = 500; break;
                    case 3: _dmgValue = 750; break;
                    default: return;
                }
            }

            foreach (GamePlayer iPlayer in _caster.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
            {
                if (iPlayer == _caster)
                {
                    iPlayer.MessageToSelf($"You cast {Name}!", eChatType.CT_Spell);
                }
                else
                {
                    iPlayer.MessageFromArea(_caster, $"{_caster.Name} casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                }

                iPlayer.Out.SendSpellCastAnimation(_caster, 7025, 20);
            }

            if (_caster.RealmAbilityCastTimer != null)
            {
                _caster.RealmAbilityCastTimer.Stop();
                _caster.RealmAbilityCastTimer = null;
                _caster.Out.SendMessage("You cancel your Spell!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
            }

            _caster.RealmAbilityCastTimer = new RegionTimer(_caster)
            {
                Callback = new RegionTimerCallback(EndCast)
            };

            _caster.RealmAbilityCastTimer.Start(2000);
        }

        protected virtual int EndCast(RegionTimer timer)
        {
            if (_caster.TargetObject == null)
            {
                _caster.Out.SendMessage("You need a target for this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                _caster.DisableSkill(this, 3 * 1000);
                return 0;
            }

            if (!_caster.IsWithinRadius(_caster.TargetObject, (int)(1500 * _caster.GetModified(eProperty.SpellRange) * 0.01)))
            {
                _caster.Out.SendMessage(_caster.TargetObject + " is too far away.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                return 0;
            }

            foreach (GamePlayer player in _caster.TargetObject.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                player.Out.SendSpellEffectAnimation(_caster, _caster.TargetObject as GameLiving, 7025, 0, false, 1);
            }

            foreach (GameNPC mob in _caster.TargetObject.GetNPCsInRadius(500))
            {
                if (!GameServer.ServerRules.IsAllowedToAttack(_caster, mob, true))
                {
                    continue;
                }

                mob.TakeDamage(_caster, eDamageType.Heat, _dmgValue, 0);
                _caster.Out.SendMessage($"You hit the {mob.Name} for {_dmgValue} damage.", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                foreach (GamePlayer player2 in _caster.TargetObject.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    player2.Out.SendSpellEffectAnimation(_caster, mob, 7025, 0, false, 1);
                }
            }

            foreach (GamePlayer aeplayer in _caster.TargetObject.GetPlayersInRadius(500))
            {
                if (!GameServer.ServerRules.IsAllowedToAttack(_caster, aeplayer, true))
                {
                    continue;
                }

                aeplayer.TakeDamage(_caster, eDamageType.Heat, _dmgValue, 0);
                _caster.Out.SendMessage($"You hit {aeplayer.Name} for {_dmgValue} damage.", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                aeplayer.Out.SendMessage($"{_caster.Name} hits you for {_dmgValue} damage.", eChatType.CT_YouWereHit, eChatLoc.CL_SystemWindow);
                foreach (GamePlayer player3 in _caster.TargetObject.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    player3.Out.SendSpellEffectAnimation(_caster, aeplayer, 7025, 0, false, 1);
                }
            }

            DisableSkill(_caster);
            timer.Stop();
            return 0;
        }

        public override int GetReUseDelay(int level)
        {
            return 900;
        }
    }
}
