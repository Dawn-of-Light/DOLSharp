using DOL.GS;
using DOL.GS.Keeps;
using DOL.GS.Movement;

namespace DOL.AI.Brain
{
	/// <summary>
	/// Brain Class for Area Capture Guards
	/// </summary>
	public class KeepGuardBrain : StandardMobBrain
	{
		public GameKeepGuard guard;
		/// <summary>
		/// Constructor for the Brain setting default values
		/// </summary>
		public KeepGuardBrain()
			: base()
		{
			AggroLevel = 60;
			AggroRange = 1500;
		}

		/// <summary>
		/// Actions to be taken on each Think pulse
		/// </summary>
		public override void Think()
		{
			if (guard == null)
				guard = Body as GameKeepGuard;
			if (guard == null)
				Stop();
			CheckPlayerAggro();
			CheckNPCAggro();

			if ((guard is GuardArcher || guard is GuardLord))
			{
				if (guard.AttackState && guard.CanUseRanged)
				{
					guard.SwitchToRanged(guard.TargetObject);
				}
			}

			//switch attack target?
			GameLiving currentTarget = guard.TargetObject as GameLiving;
			GameLiving nextTarget = CalculateNextAttackTarget();
			if (nextTarget != null && nextTarget != currentTarget)
			{
				guard.StopAttack();
				guard.StartAttack(nextTarget);
			}

			//if we are not doing an action, let us see if we should move somewhere
			if (guard.CurrentSpellHandler == null && !guard.IsMoving && !guard.AttackState && !guard.InCombat)
			{
				if (guard.X != guard.SpawnX ||
					guard.Y != guard.SpawnY ||
					guard.Z != guard.SpawnZ)
					guard.WalkToSpawn();
			}
		}

		/// <summary>
		/// Check Area for Players to attack
		/// </summary>
		protected override void CheckPlayerAggro()
		{
			if (Body.AttackState || Body.CurrentSpellHandler != null)
				return;
			foreach (GamePlayer player in Body.GetPlayersInRadius((ushort)AggroRange))
			{
				if (GameServer.ServerRules.IsAllowedToAttack(Body, player, false))
				{
					if (KeepMgr.IsEnemy(Body as GameKeepGuard, player))
					{
						if (Body is GuardStealther == false && player.IsStealthed)
							continue;
						Body.StartAttack(player);
						AddToAggroList(player, player.EffectiveLevel << 1);
						return;
					}
				}
			}
		}

		/// <summary>
		/// Check area for NPCs to attack
		/// </summary>
		protected override void CheckNPCAggro()
		{
			if (Body.AttackState || Body.CurrentSpellHandler != null)
				return;
			foreach (GameNPC npc in Body.GetNPCsInRadius((ushort)AggroRange))
			{
				if (npc is GameKeepGuard) continue;
				if (GameServer.ServerRules.IsAllowedToAttack(Body, npc, false))
				{
					if (npc is GameSiegeWeapon) continue;
					if (KeepMgr.IsEnemy(Body as GameKeepGuard, npc))
					{
						Body.StartAttack(npc);
						AddToAggroList(npc, (npc.Level + 1) << 1);
						return;
					}
				}
			}
		}

		protected override void AttackMostWanted()
		{
			if (!IsActive)
				return;

			GameLiving target = CalculateNextAttackTarget();
			if (target != null)
			{
				if (!Body.AttackState || target != Body.TargetObject)
				{
					Body.StartAttack(target);
				}
			}
			else
			{
				if (Body.CurrentSpellHandler == null && !Body.IsMoving && !Body.AttackState && !Body.InCombat)
					Body.WalkToSpawn();
			}
		}

		public override int CalculateAggroLevelToTarget(GameLiving target)
		{
			if (KeepMgr.IsEnemy(Body, target))
				return AggroLevel;
			return 0;
		}
	}
}
