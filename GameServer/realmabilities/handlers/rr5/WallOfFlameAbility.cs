using System;
using System.Collections;
using System.Reflection;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;
using DOL.Database2;

namespace DOL.GS.RealmAbilities
{
	public class WallOfFlameAbility : RR5RealmAbility
	{
		public WallOfFlameAbility(DBAbility dba, int level) : base(dba, level) { }

		private int dmgValue = 400; // 400 Dmg
		private uint duration = 15; // 15 Sec duration

		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

			base.Execute(living);

			GamePlayer player = living as GamePlayer;
			if (player == null)
				return;

			if (player.IsMoving)
			{
				player.Out.SendMessage("You must be standing still to use this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			foreach (GamePlayer i_player in player.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
			{
				if (i_player == player) i_player.Out.SendMessage("You cast a Wall of Flame!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				else i_player.Out.SendMessage(player.Name + " casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				i_player.Out.SendSpellCastAnimation(player, 7028, 20);
			}

			Statics.WallOfFlameBase wof = new Statics.WallOfFlameBase(dmgValue);
			Point3D targetSpot = new Point3D(player.X, player.Y, player.Z);
			wof.CreateStatic(player, targetSpot, duration, 3, 150);

			DisableSkill(living);
			player.StopCurrentSpellcast();

		}


		public override int GetReUseDelay(int level)
		{
			return 600;
		}
	}
}