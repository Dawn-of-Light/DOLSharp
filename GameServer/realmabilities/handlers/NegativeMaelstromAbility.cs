using System;
using System.Collections;
using System.Reflection;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;
using DOL.Database;
using DOL.GS.Geometry;

namespace DOL.GS.RealmAbilities
{
	public class NegativeMaelstromAbility : TimedRealmAbility
	{
        public NegativeMaelstromAbility(DBAbility dba, int level) : base(dba, level) { }
		private int dmgValue;
		private uint duration;
		private GamePlayer player;
        private const string IS_CASTING = "isCasting";
        private const string NM_CAST_SUCCESS = "NMCasting";

        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;
            GamePlayer caster = living as GamePlayer;
			if (caster.IsMoving)
			{
				caster.Out.SendMessage("You must be standing still to use this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

            if ( caster.GroundTargetPosition == Position.Nowhere || caster.Coordinate.DistanceTo(caster.GroundTargetPosition) > 1500)
            {
				caster.Out.SendMessage("You groundtarget is too far away to use this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
            }
            if (caster.TempProperties.getProperty(IS_CASTING, false))
            {
                caster.Out.SendMessage("You are already casting an ability.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }
            this.player = caster;
            if (caster.AttackState) 
            {
                caster.StopAttack();
            }
            caster.StopCurrentSpellcast();

            if(ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
            {
	            switch (Level)
				{
					case 1: dmgValue = 175; break;
					case 2: dmgValue = 260; break;
					case 3: dmgValue = 350; break;
					case 4: dmgValue = 425; break;
					case 5: dmgValue = 500; break;
					default: return;
				}
            }
            else
            {
	            switch (Level)
				{
					case 1: dmgValue = 120; break;
					case 2: dmgValue = 240; break;
					case 3: dmgValue = 360; break;
					default: return;
				}
            }
            
			duration = 30;
			foreach (GamePlayer i_player in caster.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
			{
				if (i_player == caster)
				{
					i_player.MessageToSelf("You cast " + this.Name + "!", eChatType.CT_Spell);
				}
				else
				{
					i_player.MessageFromArea(caster, caster.Name + " casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				}

				i_player.Out.SendSpellCastAnimation(caster, 7027, 20);
			}
            caster.TempProperties.setProperty(IS_CASTING, true);
            caster.TempProperties.setProperty(NM_CAST_SUCCESS, true);
            GameEventMgr.AddHandler(caster, GamePlayerEvent.Moving, new DOLEventHandler(CastInterrupted));
            GameEventMgr.AddHandler(caster, GamePlayerEvent.AttackFinished, new DOLEventHandler(CastInterrupted));
            GameEventMgr.AddHandler(caster, GamePlayerEvent.Dying, new DOLEventHandler(CastInterrupted));
            if (caster != null)
            {
                new RegionTimer(caster, new RegionTimerCallback(EndCast), 2000);
            }
		}
		protected virtual int EndCast(RegionTimer timer)
		{
            bool castWasSuccess = player.TempProperties.getProperty(NM_CAST_SUCCESS, false);
            player.TempProperties.removeProperty(IS_CASTING);
            GameEventMgr.RemoveHandler(player, GamePlayerEvent.Moving, new DOLEventHandler(CastInterrupted));
            GameEventMgr.RemoveHandler(player, GamePlayerEvent.AttackFinished, new DOLEventHandler(CastInterrupted));
            GameEventMgr.RemoveHandler(player, GamePlayerEvent.Dying, new DOLEventHandler(CastInterrupted));
            if (player.IsMezzed || player.IsStunned || player.IsSitting)
                return 0;
            if (!castWasSuccess)
                return 0;
			Statics.NegativeMaelstromBase nm = new Statics.NegativeMaelstromBase(dmgValue);
			nm.CreateStatic(player, player.GroundTargetPosition.Coordinate, duration, 5, 350);
            DisableSkill(player); 
			timer.Stop();
			timer = null;
			return 0;
		}
        private void CastInterrupted(DOLEvent e, object sender, EventArgs arguments) 
        {
            AttackFinishedEventArgs attackFinished = arguments as AttackFinishedEventArgs;
            if (attackFinished != null && attackFinished.AttackData.Attacker != sender)
                return;
            player.TempProperties.setProperty(NM_CAST_SUCCESS, false);
            foreach (GamePlayer i_player in player.GetPlayersInRadius(WorldMgr.INFO_DISTANCE)) {
                i_player.Out.SendInterruptAnimation(player);
            }
        }
        public override int GetReUseDelay(int level)
        {
            return 900;
        }
	}
}
