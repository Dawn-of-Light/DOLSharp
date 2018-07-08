using System;
using DOL.GS.PacketHandler;
using DOL.Events;
using DOL.Database;

namespace DOL.GS.RealmAbilities
{
    public class NegativeMaelstromAbility : TimedRealmAbility
    {
        public NegativeMaelstromAbility(DBAbility dba, int level) : base(dba, level) { }

        private int _dmgValue;
        private uint _duration;
        private GamePlayer _player;
        private const string IsCasting = "isCasting";
        private const string NmCastSuccess = "NMCasting";

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

            if (caster.GroundTarget == null || !caster.IsWithinRadius(caster.GroundTarget, 1500))
            {
                caster.Out.SendMessage("You groundtarget is too far away to use this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            if (caster.TempProperties.getProperty(IsCasting, false))
            {
                caster.Out.SendMessage("You are already casting an ability.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
                    case 1: _dmgValue = 175; break;
                    case 2: _dmgValue = 260; break;
                    case 3: _dmgValue = 350; break;
                    case 4: _dmgValue = 425; break;
                    case 5: _dmgValue = 500; break;
                    default: return;
                }
            }
            else
            {
                switch (Level)
                {
                    case 1: _dmgValue = 120; break;
                    case 2: _dmgValue = 240; break;
                    case 3: _dmgValue = 360; break;
                    default: return;
                }
            }

            _duration = 30;
            foreach (GamePlayer iPlayer in caster.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
            {
                if (iPlayer == caster)
                {
                    iPlayer.MessageToSelf("You cast " + Name + "!", eChatType.CT_Spell);
                }
                else
                {
                    iPlayer.MessageFromArea(caster, caster.Name + " casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                }

                iPlayer.Out.SendSpellCastAnimation(caster, 7027, 20);
            }

            caster.TempProperties.setProperty(IsCasting, true);
            caster.TempProperties.setProperty(NmCastSuccess, true);
            GameEventMgr.AddHandler(caster, GameLivingEvent.Moving, new DOLEventHandler(CastInterrupted));
            GameEventMgr.AddHandler(caster, GameLivingEvent.AttackFinished, new DOLEventHandler(CastInterrupted));
            GameEventMgr.AddHandler(caster, GameLivingEvent.Dying, new DOLEventHandler(CastInterrupted));
            new RegionTimer(caster, new RegionTimerCallback(EndCast), 2000);
        }

        protected virtual int EndCast(RegionTimer timer)
        {
            bool castWasSuccess = _player.TempProperties.getProperty(NmCastSuccess, false);
            _player.TempProperties.removeProperty(IsCasting);
            GameEventMgr.RemoveHandler(_player, GameLivingEvent.Moving, new DOLEventHandler(CastInterrupted));
            GameEventMgr.RemoveHandler(_player, GameLivingEvent.AttackFinished, new DOLEventHandler(CastInterrupted));
            GameEventMgr.RemoveHandler(_player, GameLivingEvent.Dying, new DOLEventHandler(CastInterrupted));
            if (_player.IsMezzed || _player.IsStunned || _player.IsSitting)
            {
                return 0;
            }

            if (!castWasSuccess)
            {
                return 0;
            }

            Statics.NegativeMaelstromBase nm = new Statics.NegativeMaelstromBase(_dmgValue);
            nm.CreateStatic(_player, _player.GroundTarget, _duration, 5, 350);
            DisableSkill(_player);
            timer.Stop();
            return 0;
        }

        private void CastInterrupted(DOLEvent e, object sender, EventArgs arguments)
        {
            if (arguments is AttackFinishedEventArgs attackFinished && attackFinished.AttackData.Attacker != sender)
            {
                return;
            }

            _player.TempProperties.setProperty(NmCastSuccess, false);
            foreach (GamePlayer iPlayer in _player.GetPlayersInRadius(WorldMgr.INFO_DISTANCE)) {
                iPlayer.Out.SendInterruptAnimation(_player);
            }
        }

        public override int GetReUseDelay(int level)
        {
            return 900;
        }
    }
}
