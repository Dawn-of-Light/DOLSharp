using DOL.GS.PacketHandler;
using DOL.Database;

namespace DOL.GS.RealmAbilities
{
    public class ThornweedFieldAbility : TimedRealmAbility
    {
        public ThornweedFieldAbility(DBAbility dba, int level) : base(dba, level) { }

        private int _dmgValue;
        private uint _duration;
        private GamePlayer _player;

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

            if (caster.IsMoving)
            {
                caster.Out.SendMessage("You must be standing still to use this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            if (caster.GroundTarget == null)
            {
                caster.Out.SendMessage("You must set a ground target to use this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            if (!caster.IsWithinRadius(caster.GroundTarget, 1500))
            {
                caster.Out.SendMessage("Your ground target is too far away to use this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            _player = caster;
            if (caster.AttackState)
            {
                caster.StopAttack();
            }

            caster.StopCurrentSpellcast();

            if (ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
            {
                switch (Level)
                {
                    case 1: _dmgValue = 25; _duration = 10; break;
                    case 2: _dmgValue = 50; _duration = 15; break;
                    case 3: _dmgValue = 100; _duration = 20; break;
                    case 4: _dmgValue = 175; _duration = 25; break;
                    case 5: _dmgValue = 250; _duration = 30; break;
                    default: return;
                }
            }
            else
            {
                switch (Level)
                {
                    case 1: _dmgValue = 25; _duration = 10; break;
                    case 2: _dmgValue = 100; _duration = 20; break;
                    case 3: _dmgValue = 250; _duration = 30; break;
                    default: return;
                }
            }

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

                iPlayer.Out.SendSpellCastAnimation(caster, 7028, 20);
            }

            if (caster.RealmAbilityCastTimer != null)
            {
                caster.RealmAbilityCastTimer.Stop();
                caster.RealmAbilityCastTimer = null;
                caster.Out.SendMessage("You cancel your Spell!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
            }

            caster.RealmAbilityCastTimer = new RegionTimer(caster)
            {
                Callback = new RegionTimerCallback(EndCast)
            };

            caster.RealmAbilityCastTimer.Start(2000);
        }

        protected virtual int EndCast(RegionTimer timer)
        {
            if (_player.IsMezzed || _player.IsStunned || _player.IsSitting)
            {
                return 0;
            }

            Statics.ThornweedFieldBase twf = new Statics.ThornweedFieldBase(_dmgValue);
            twf.CreateStatic(_player, _player.GroundTarget, _duration, 3, 500);
            DisableSkill(_player);
            timer.Stop();
            return 0;
        }

        public override int GetReUseDelay(int level)
        {
            return 600;
        }
    }
}
