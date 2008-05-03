using System.Collections;
using System.Reflection;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;
using DOL.Database2;

namespace DOL.GS.RealmAbilities
{
	public class StaticTempestAbility : TimedRealmAbility
	{
        public StaticTempestAbility(DBAbility dba, int level) : base(dba, level) { }

		private int stunDuration;
		private uint duration;
		private GamePlayer player;
        public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;
			GamePlayer player = living as GamePlayer;
            if (player.TargetObject == null)
            {
                player.Out.SendMessage("You need a target for this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            if (!(player.TargetObject is GameLiving)
                || !GameServer.ServerRules.IsAllowedToAttack(player, (GameLiving)player.TargetObject, true))
            {
                player.Out.SendMessage("You cannot attack " + player.TargetObject.Name + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }
            if (!player.TargetInView)
            {
                player.Out.SendMessage("You cannot see " + player.TargetObject.Name + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }
            if (WorldMgr.GetDistance(player, player.TargetObject) > 1500)
            {
                player.Out.SendMessage("You target is too far away to use this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }
            this.player = player;
            switch (Level)
            {
                case 1: duration = 10; break;
                case 2: duration = 15; break;
                case 3: duration = 30; break;
                default: return;
            }
            stunDuration = 3;
            foreach (GamePlayer i_player in player.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
            {
                if (i_player == player) i_player.Out.SendMessage("You cast " + this.Name + "!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                else i_player.Out.SendMessage(player.Name + " casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
            }
            Statics.StaticTempestBase st = new Statics.StaticTempestBase(stunDuration);
            Point3D targetSpot = new Point3D(player.TargetObject.X, player.TargetObject.Y, player.TargetObject.Z);
            st.CreateStatic(player, targetSpot, duration, 5, 360);
            DisableSkill(living);
        }
        public override int GetReUseDelay(int level)
        {
            return 600;
        }
	}
}
