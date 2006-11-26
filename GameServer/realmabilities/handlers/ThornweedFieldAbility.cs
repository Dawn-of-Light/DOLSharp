using System;
using System.Collections;
using System.Reflection;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.GS.Scripts;
using DOL.Events;
using DOL.Database;

namespace DOL.GS.RealmAbilities
{
	public class ThornweedFieldAbility : TimedRealmAbility
	{
        public ThornweedFieldAbility(DBAbility dba, int level) : base(dba, level) { }
		private const string IS_CASTING = "isCasting";
		private const string TWF_CAST_SUCCESS = "TWFCasting";
		private int dmgValue;
		private uint duration;
		private GamePlayer player;
		
		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;
            
			GamePlayer player = living as GamePlayer;
			if (player.IsMoving)
			{
				player.Out.SendMessage("You must be standing still to use this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}            
			if (player.GroundTarget == null || WorldMgr.GetDistance(player, player.GroundTarget.X, player.GroundTarget.Y, player.GroundTarget.Z) >1500)
            {
				player.Out.SendMessage("You groundtarget is too far away to use this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
            if (player.TempProperties.getProperty(IS_CASTING, false)) 
            {
                player.Out.SendMessage("You are already casting an ability.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            } 
            this.player = player;
            if (player.AttackState) 
            {
                player.StopAttack();
            }
            player.StopCurrentSpellcast();
			switch (Level)
			{
				case 1: dmgValue = 25; duration = 10;  break;
				case 2: dmgValue = 100; duration = 20; break;
				case 3: dmgValue = 250; duration = 30; break;
				default: return;
			}
            if (player != null)
            {
                foreach (GamePlayer i_player in player.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
                {
                    if (i_player == player) i_player.Out.SendMessage("You cast " + this.Name + "!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                    else i_player.Out.SendMessage(player.Name + " casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);

                    i_player.Out.SendSpellCastAnimation(player, 7028, 20);
                }
                player.TempProperties.setProperty(IS_CASTING, true);
                player.TempProperties.setProperty(TWF_CAST_SUCCESS, true);
                GameEventMgr.AddHandler(player, GamePlayerEvent.Moving, new DOLEventHandler(CastInterrupted));
                GameEventMgr.AddHandler(player, GamePlayerEvent.AttackFinished, new DOLEventHandler(CastInterrupted));
                GameEventMgr.AddHandler(player, GamePlayerEvent.Dying, new DOLEventHandler(CastInterrupted));
                new RegionTimer(player, new RegionTimerCallback(EndCast), 2000);
            }
		}
		protected virtual int EndCast(RegionTimer timer)
		{
			bool castWasSuccess = player.TempProperties.getProperty(TWF_CAST_SUCCESS, false);
			player.TempProperties.removeProperty(IS_CASTING);
			GameEventMgr.RemoveHandler(player, GamePlayerEvent.Moving , new DOLEventHandler(CastInterrupted));
			GameEventMgr.RemoveHandler(player, GamePlayerEvent.AttackFinished, new DOLEventHandler(CastInterrupted));
			GameEventMgr.RemoveHandler(player, GamePlayerEvent.Dying, new DOLEventHandler(CastInterrupted));	
			if (player.IsMezzed || player.IsStunned || player.IsSitting)
				return 0;
			if (!castWasSuccess)
				return 0;
			Statics.ThornweedFieldBase twf = new Statics.ThornweedFieldBase(dmgValue);
			twf.CreateStatic(player,player.GroundTarget,duration,3,500);
            DisableSkill(player);
			timer.Stop();
			timer = null;
			return 0;
		}
		private void CastInterrupted (DOLEvent e, object sender, EventArgs arguments)
        {
            AttackFinishedEventArgs attackFinished = arguments as AttackFinishedEventArgs;
            if (attackFinished != null && attackFinished.AttackData.Attacker != sender)
                return;
			player.TempProperties.setProperty(TWF_CAST_SUCCESS,false);
			foreach (GamePlayer i_player in player.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
			{
				i_player.Out.SendInterruptAnimation(player);
			}
		}
        public override int GetReUseDelay(int level)
        {
            return 600;
        }
	}
}
