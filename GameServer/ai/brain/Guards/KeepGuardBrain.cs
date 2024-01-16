using log4net;
using System.Reflection;
using DOL.GS;
using DOL.GS.Keeps;

namespace DOL.AI.Brain
{
	/// <summary>
	/// Brain Class for Area Capture Guards
	/// </summary>
	public class KeepGuardBrain : StandardMobBrain
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public GameKeepGuard guard;
		/// <summary>
		/// Constructor for the Brain setting default values
		/// </summary>
		public KeepGuardBrain()
			: base()
		{
			AggroLevel = 90;
			AggroRange = 1500;
		}

		public void SetAggression(int aggroLevel, int aggroRange)
		{
			AggroLevel = aggroLevel;
			AggroRange = aggroRange;
		}

		public override int ThinkInterval
		{
			get
			{
				return 1500;
			}
		}

		/// <summary>
		/// Actions to be taken on each Think pulse
		/// </summary>
		public override void Think()
		{
			if (guard == null)
				guard = Body as GameKeepGuard;
			if (guard == null)
			{
				Stop();
				return;
			}

			if ((guard is GuardArcher || guard is GuardStaticArcher || guard is GuardLord))
			{
				// Drop aggro and disengage if the target is out of range.
				if (Body.IsAttacking && Body.TargetObject is GameLiving living && Body.IsWithinRadius(Body.TargetObject, AggroRange, false) == false)
				{
					Body.StopAttack();
					RemoveFromAggroList(living);
					Body.TargetObject = null;
				}

				if (guard.AttackState && guard.CanUseRanged)
				{
					guard.SwitchToRanged(guard.TargetObject);
				}
			}

			//if we are not doing an action, let us see if we should move somewhere
			if (guard.CurrentSpellHandler == null && !guard.IsMoving && !guard.AttackState && !guard.InCombat)
			{
				// Tolakram - always clear the aggro list so if this is done by mistake the list will correctly re-fill on next think
				ClearAggroList();

				if (guard.Coordinate.DistanceTo(guard.SpawnPosition, ignoreZ: true) > 50)
				{
					guard.WalkToSpawn();
				}
			}
			//Eden - Portal Keeps Guards max distance
            if (guard.Level > 200 && guard.Coordinate.DistanceTo(guard.SpawnPosition) > 2000)
			{
				ClearAggroList();
				guard.WalkToSpawn();
			}
            else if (guard.InCombat == false && guard.Coordinate.DistanceTo(guard.SpawnPosition) > 6000)
			{
				ClearAggroList();
				guard.WalkToSpawn();
			}

			// We want guards to check aggro even when they are returning home, which StandardMobBrain does not, so add checks here
			if (guard.CurrentSpellHandler == null && !guard.AttackState && !guard.InCombat)
			{
				CheckPlayerAggro();
				CheckNPCAggro();

				if (HasAggro && Body.IsReturningHome)
				{
					Body.StopMoving();
					AttackMostWanted();
				}
			}

			base.Think();
		}

		/// <summary>
		/// Check Area for Players to attack
		/// </summary>
		protected override void CheckPlayerAggro()
		{
			if (HasAggro || Body.CurrentSpellHandler != null) return;

			foreach (GamePlayer player in Body.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
                if (player == null) continue;
                if (GameServer.ServerRules.IsAllowedToAttack(Body, player, true))
				{
                    if ( !Body.IsWithinRadius( player, AggroRange ) )
                        continue;
                    if ((Body as GameKeepGuard).Component != null && !GameServer.KeepManager.IsEnemy(Body as GameKeepGuard, player, true))
						continue;
					if (Body is GuardStealther == false && player.IsStealthed)
						continue;

					WarMapMgr.AddGroup(player.Position, player.Name, (byte)player.Realm);

					if (DOL.GS.ServerProperties.Properties.ENABLE_DEBUG)
					{
						Body.Say("Want to attack player " + player.Name);
					}

					AddToAggroList(player, 1);
					return;
				}
			}
		}

		/// <summary>
		/// Check area for NPCs to attack
		/// </summary>
		protected override void CheckNPCAggro()
		{
			if (HasAggro || Body.CurrentSpellHandler != null) return;

			foreach (GameNPC npc in Body.GetNPCsInRadius((ushort)AggroRange))
			{
				if (npc == null || npc.Brain == null || npc is GameKeepGuard || (npc.Brain as IControlledBrain) == null)
					continue;

				GamePlayer player = (npc.Brain as IControlledBrain).GetPlayerOwner();
				
				if (player == null)
					continue;

				if (GameServer.ServerRules.IsAllowedToAttack(Body, npc, true))
				{
					if ((Body as GameKeepGuard).Component != null && !GameServer.KeepManager.IsEnemy(Body as GameKeepGuard, player, true))
					{
						continue;
					}

					WarMapMgr.AddGroup(player.Position, player.Name, (byte)player.Realm);

					if (DOL.GS.ServerProperties.Properties.ENABLE_DEBUG)
					{
						Body.Say("Want to attack player " + player.Name + " pet " + npc.Name);
					}

					AddToAggroList(npc, 1);
					return;
				}
			}
		}

		public override int CalculateAggroLevelToTarget(GameLiving target)
		{
			GamePlayer checkPlayer = null;
			if (target is GameNPC && (target as GameNPC).Brain is IControlledBrain)
				checkPlayer = ((target as GameNPC).Brain as IControlledBrain).GetPlayerOwner();
			if (target is GamePlayer)
				checkPlayer = target as GamePlayer;
			if (checkPlayer == null)
				return 0;
			if (GameServer.KeepManager.IsEnemy(Body as GameKeepGuard, checkPlayer, true))
				return AggroLevel;
			return 0;
		}
		
		public override bool AggroLOS
		{
			get { return true; }
		}
	}
}
