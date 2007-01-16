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
	public class WrathofChampionsAbility : TimedRealmAbility
	{
        public WrathofChampionsAbility(DBAbility dba, int level) : base(dba, level) { }

		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

			GamePlayer player = living as GamePlayer;
			if (player == null)
				return;

			if (player.TargetObject == null)
			{
				player.Out.SendMessage("You need a target for this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (!(player.TargetObject is GameLiving) || !GameServer.ServerRules.IsAllowedToAttack(player, (GameLiving)player.TargetObject, true))
			{
				player.Out.SendMessage("You cannot attack " + player.TargetObject.Name + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (WorldMgr.GetDistance(player, player.TargetObject) > 200)
			{
				player.Out.SendMessage("You target is too far away to use this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			Int32 dmgValue = 0;
			switch (Level)
			{
				case 1: dmgValue = 200; break;
				case 2: dmgValue = 500; break;
				case 3: dmgValue = 750; break;
			}

			//send cast messages
			foreach (GamePlayer i_player in player.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
			{
				if (i_player == player) i_player.Out.SendMessage("You cast a Wrath of Champions Spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				else i_player.Out.SendMessage(player.Name + " casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
			}

			//deal damage to npcs
			foreach (GameNPC mob in player.GetNPCsInRadius(200))
			{
				if (GameServer.ServerRules.IsAllowedToAttack(player, mob, true) == false) continue;

				mob.TakeDamage(player, eDamageType.Spirit, dmgValue, 0);
				player.Out.SendMessage("You hit the " + mob.Name + " for " + dmgValue + " damage.", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
				foreach (GamePlayer player2 in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					player2.Out.SendSpellEffectAnimation(player, mob, 7024, 0, false, 1);
				}
			}

			//deal damage to players
			foreach (GamePlayer t_player in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				if (GameServer.ServerRules.IsAllowedToAttack(player, t_player, true) == false)
					continue;

				if (WorldMgr.GetDistance(player, t_player) > 200)
					continue;
				t_player.TakeDamage(player, eDamageType.Spirit, dmgValue, 0);

				// send a message

				foreach (GamePlayer n_player in t_player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					n_player.Out.SendSpellCastAnimation(player, 7024, 0);
					n_player.Out.SendSpellEffectAnimation(player, t_player, 7024, 0, false, 1);
				}
			}
			DisableSkill(living);
			player.LastAttackTick = player.CurrentRegion.Time;
		}

        public override int GetReUseDelay(int level)
        {
            return 600;
        }
	}
}
