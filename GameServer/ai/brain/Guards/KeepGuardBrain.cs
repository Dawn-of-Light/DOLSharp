using System;
using log4net;
using System.Reflection;
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

		/// <summary>
		/// Actions to be taken on each Think pulse
		/// </summary>
		public override void Think()
		{
			if (guard == null)
				guard = Body as GameKeepGuard;
			if (guard == null)
				Stop();

			if ((guard is GuardArcher || guard is GuardLord))
			{
				if (guard.AttackState && guard.CanUseRanged)
				{
					guard.SwitchToRanged(guard.TargetObject);
				}
			}

			//if we are not doing an action, let us see if we should move somewhere
			if (guard.CurrentSpellHandler == null && !guard.IsMoving && !guard.AttackState && !guard.InCombat)
			{
				if (guard.X != guard.SpawnPoint.X ||
					guard.Y != guard.SpawnPoint.Y ||
					guard.Z != guard.SpawnPoint.Z)
					guard.WalkToSpawn();
			}
			//Eden - Portal Keeps Guards max distance
            if ( guard.Level > 200 && !guard.IsWithinRadius( guard.SpawnPoint, 2000 ) )
			{
				ClearAggroList();
				guard.WalkToSpawn();
			}
			// other guards max distance
            else if ( !guard.InCombat && !guard.IsWithinRadius( guard.SpawnPoint, 6000 ) )
			{
				ClearAggroList();
				guard.WalkToSpawn();
			}
			base.Think();
		}

		/// <summary>
		/// Check Area for Players to attack
		/// </summary>
		protected override void CheckPlayerAggro()
		{
			if (Body is MissionMaster)
				return;
			if (Body.AttackState || Body.CurrentSpellHandler != null)
				return;
			foreach (GamePlayer player in Body.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
                if (player == null) continue;
                if (GameServer.ServerRules.IsAllowedToAttack(Body, player, false)) // using group check, feat PvP rules
				{
                    WarMapMgr.AddGroup((byte)player.CurrentZone.ID, player.X, player.Y, player.Name, (byte)player.Realm);

                    if ( !Body.IsWithinRadius( player, AggroRange ) )
                        continue;
                    if ((Body as GameKeepGuard).Component != null && !KeepMgr.IsEnemy(Body as GameKeepGuard, player, true))
						continue;
					if (Body is GuardStealther == false && player.IsStealthed)
						continue;
					Body.StartAttack(player);
					AddToAggroList(player, player.EffectiveLevel << 1);
					return;
				}
			}
		}

		/// <summary>
		/// Check area for NPCs to attack
		/// </summary>
		protected override void CheckNPCAggro()
		{
			if (Body is MissionMaster)
				return;
			if (Body.AttackState || Body.CurrentSpellHandler != null)
				return;
			foreach (GameNPC npc in Body.GetNPCsInRadius((ushort)AggroRange))
			{
				if (npc == null || npc.Brain == null || npc is GameKeepGuard || (npc.Brain as ControlledNpc == null) || !(npc.Brain is IControlledBrain))
					continue;

				GamePlayer player = (npc.Brain as ControlledNpc).GetPlayerOwner();
				
				if (player == null)
					continue;

				if (GameServer.ServerRules.IsAllowedToAttack(Body, npc, false))
				{
					if ((Body as GameKeepGuard).Component != null && !KeepMgr.IsEnemy(Body as GameKeepGuard, player, true))
						continue;
					Body.StartAttack(npc);
					AddToAggroList(npc, (npc.Level + 1) << 1);
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
			if (KeepMgr.IsEnemy(Body as GameKeepGuard, checkPlayer, true))
				return AggroLevel;
			return 0;
		}
		
		public override bool AggroLOS
		{
			get { return true; }
		}
	}
}
