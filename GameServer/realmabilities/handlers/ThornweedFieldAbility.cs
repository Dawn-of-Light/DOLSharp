using System;
using System.Collections;
using System.Reflection;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;
using DOL.Database;

namespace DOL.GS.RealmAbilities
{
	public class ThornweedFieldAbility : TimedRealmAbility
	{
		public ThornweedFieldAbility(DBAbility dba, int level) : base(dba, level) { }
		private int dmgValue;
		private uint duration;
		private GamePlayer player;

		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

			GamePlayer player = living as GamePlayer;
			if (player == null)
				return;

			if (player.IsMoving)
			{
				player.Out.SendMessage("You must be standing still to use this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (player.GroundTarget == null )
            {
                player.Out.SendMessage( "You must set a ground target to use this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow );
                return;
            }
            else if(!player.IsWithinRadius( player.GroundTarget, 1500 ))
			{
				player.Out.SendMessage("You ground target is too far away to use this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
				case 1: dmgValue = 25; duration = 10; break;
				case 2: dmgValue = 100; duration = 20; break;
				case 3: dmgValue = 250; duration = 30; break;
				default: return;
			}
			foreach (GamePlayer i_player in player.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
			{
				if (i_player == player) i_player.Out.SendMessage("You cast " + this.Name + "!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				else i_player.Out.SendMessage(player.Name + " casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);

				i_player.Out.SendSpellCastAnimation(player, 7028, 20);
			}

			if (player.RealmAbilityCastTimer != null)
			{
				player.RealmAbilityCastTimer.Stop();
				player.RealmAbilityCastTimer = null;
				player.Out.SendMessage("You cancel your Spell!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
			}

			player.RealmAbilityCastTimer = new RegionTimer(player);
			player.RealmAbilityCastTimer.Callback = new RegionTimerCallback(EndCast);
			player.RealmAbilityCastTimer.Start(2000);
		}

		protected virtual int EndCast(RegionTimer timer)
		{
			if (player.IsMezzed || player.IsStunned || player.IsSitting)
				return 0;
			Statics.ThornweedFieldBase twf = new Statics.ThornweedFieldBase(dmgValue);
			twf.CreateStatic(player, player.GroundTarget, duration, 3, 500);
			DisableSkill(player);
			timer.Stop();
			timer = null;
			return 0;
		}

		public override int GetReUseDelay(int level)
		{
			return 600;
		}
	}
}
