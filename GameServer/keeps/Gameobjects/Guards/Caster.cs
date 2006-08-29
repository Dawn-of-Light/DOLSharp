using DOL.GS;
using DOL.Events;
using System;
using DOL.AI.Brain;
using DOL.GS.Spells;
using DOL.GS.PacketHandler;

namespace DOL.GS.Keeps
{
	public class GuardCaster : GameKeepGuard
	{
		public override void StartAttack(GameObject attackTarget)
		{
			if (attackTarget is GameLiving == false)
				return;
			GameLiving target = attackTarget as GameLiving;
			if (target == null || target.IsAlive == false)
				return;

			if (CanUseRanged)
				StartSpellAttack(attackTarget);
			else base.StartAttack(attackTarget);
		}

		public override bool StartSpellAttack(GameObject attackTarget)
		{
			if (attackTarget is GameLiving == false)
				return false;

			GamePlayer LOSChecker = null;
			if (attackTarget is GamePlayer)
			{
				LOSChecker = attackTarget as GamePlayer;
			}
			else
			{
				foreach (GamePlayer player in this.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					LOSChecker = player;
					break;
				}
			}
			if (LOSChecker == null)
				return false;
			TargetObject = attackTarget;
			LOSChecker.Out.SendCheckLOS(this, attackTarget, new CheckLOSResponse(this.GuardStartSpellAttackCheckLOS));
			return true;
		}

		public void GuardStartSpellAttackCheckLOS(GamePlayer player, ushort response, ushort targetOID)
		{
			if ((response & 0x100) == 0x100)
			{
				SpellMgr.CastNukeSpell(this, TargetObject as GameLiving);
			}
		}

		public override void OnAfterSpellCastSequence(ISpellHandler handler)
		{
			base.OnAfterSpellCastSequence(handler);
			if (ObjectState != eObjectState.Active) return;
			if (!AttackState && CurrentSpellHandler == null && TargetObject != null)
			{
				if (TargetObject is GameLiving)
				{
					if ((TargetObject as GameLiving).IsAlive == false)
						return;
					if (!GameServer.ServerRules.IsAllowedToAttack(this, TargetObject as GameLiving, false))
						return;
				}
				StartAttack(TargetObject);
			}
		}
	}
}
