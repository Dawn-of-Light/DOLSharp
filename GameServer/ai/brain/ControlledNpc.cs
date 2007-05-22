/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
using System;
using System.Reflection;
using DOL.Events;
using DOL.GS;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.AI.Brain
{
	/// <summary>
	/// A brain that can be controlled
	/// </summary>
	public class ControlledNpc : StandardMobBrain, IControlledBrain
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static readonly short MIN_OWNER_FOLLOW_DIST = 128;
		public static readonly short MAX_OWNER_FOLLOW_DIST = short.MaxValue;
		public static readonly short MIN_ENEMY_FOLLOW_DIST = 90;
		public static readonly short MAX_ENEMY_FOLLOW_DIST = 512;

		private int m_tempX = 0;
		private int m_tempY = 0;
		private int m_tempZ = 0;

		/// <summary>
		/// Holds the controlling player of this brain
		/// </summary>
		private readonly GamePlayer m_owner;

		/// <summary>
		/// Holds the walk state of the brain
		/// </summary>
		private eWalkState m_walkState;

		/// <summary>
		/// Holds the aggression level of the brain
		/// </summary>
		private eAggressionState m_aggressionState;

		/// <summary>
		/// Constructs new controlled npc brain
		/// </summary>
		/// <param name="owner"></param>
		public ControlledNpc(GamePlayer owner)
			: base()
		{
			if (owner == null)
				throw new ArgumentNullException("owner");

			m_owner = owner;
			m_aggressionState = eAggressionState.Defensive;
			m_walkState = eWalkState.Follow;
			m_aggroLevel = 99;
			m_aggroMaxRange = 1500;
		}
		
		/// <summary>
		/// Checks if this NPC is a permanent/charmed or timed pet
		/// </summary>
		/*public bool CanReceiveOrder
		{
			get
			{
				DOL.GS.Effects.GameSpellEffect summon = DOL.GS.Spells.SpellHandler.FindEffectOnTarget(this.Body, "Summon");
				if (summon == null)
					return true;
				else
					return (summon.Duration >= 65535);
			}
		}*/

		/// <summary>
		/// The number of seconds/10 this brain will stay active even when no player is close
		/// Overriden. Returns int.MaxValue
		/// </summary>
		protected override int NoPlayersStopDelay
		{
			get { return int.MaxValue; }
		}

		/// <summary>
		/// The interval for thinking, 1.5 seconds
		/// </summary>
		public override int ThinkInterval
		{
			get { return 1500; }
		}

		#region Control

		/// <summary>
		/// Gets the controlling owner of the brain
		/// </summary>
		public GamePlayer Owner
		{
			get { return m_owner; }
		}

		/// <summary>
		/// Gets or sets the walk state of the brain
		/// </summary>
		public eWalkState WalkState
		{
			get { return m_walkState; }
			set
			{
				m_walkState = value;
				UpdatePetWindow();
			}
		}

		/// <summary>
		/// Gets or sets the aggression state of the brain
		/// </summary>
		public eAggressionState AggressionState
		{
			get { return m_aggressionState; }
			set
			{
				m_aggressionState = value;
				m_orderAttackTarget = null;
				if (m_aggressionState == eAggressionState.Passive)
				{
					ClearAggroList();
					Body.StopAttack();
					Body.TargetObject = null;
					if (WalkState == eWalkState.Follow)
						FollowOwner();
					else if (m_tempX > 0 && m_tempY > 0 && m_tempZ > 0)
						Body.WalkTo(m_tempX, m_tempY, m_tempZ, Body.MaxSpeed);
				}
				AttackMostWanted();
				UpdatePetWindow();
			}
		}

		/// <summary>
		/// Attack the target on command
		/// </summary>
		/// <param name="target"></param>
		public void Attack(GameObject target)
		{
			if (AggressionState == eAggressionState.Passive)
				AggressionState = eAggressionState.Defensive;
			m_orderAttackTarget = target as GameLiving;
			AttackMostWanted();
		}

		/// <summary>
		/// Follow the target on command
		/// </summary>
		/// <param name="target"></param>
		public void Follow(GameObject target)
		{
			WalkState = eWalkState.Follow;
			Body.Follow(target, MIN_OWNER_FOLLOW_DIST, MAX_OWNER_FOLLOW_DIST);
		}

		/// <summary>
		/// Stay at current position on command
		/// </summary>
		public void Stay()
		{
			m_tempX = Body.X;
			m_tempY = Body.Y;
			m_tempZ = Body.Z;
			WalkState = eWalkState.Stay;
			Body.StopFollow();
		}

		/// <summary>
		/// Go to owner on command
		/// </summary>
		public void ComeHere()
		{
			m_tempX = Body.X;
			m_tempY = Body.Y;
			m_tempZ = Body.Z;
			WalkState = eWalkState.ComeHere;
			Body.StopFollow();
			Body.WalkTo(Owner, Body.MaxSpeed);
		}

		/// <summary>
		/// Go to targets location on command
		/// </summary>
		/// <param name="target"></param>
		public void Goto(GameObject target)
		{
			m_tempX = Body.X;
			m_tempY = Body.Y;
			m_tempZ = Body.Z;
			WalkState = eWalkState.GoTarget;
			Body.StopFollow();
			Body.WalkTo(target, Body.MaxSpeed);
		}

		/// <summary>
		/// Updates the pet window
		/// </summary>
		public void UpdatePetWindow()
		{
			m_owner.Out.SendPetWindow(m_body, ePetWindowAction.Update, m_aggressionState, m_walkState);
		}

		/// <summary>
		/// Start following the owner
		/// </summary>
		protected void FollowOwner()
		{
			Body.StopAttack();
			Body.Follow(Owner, MIN_OWNER_FOLLOW_DIST, MAX_OWNER_FOLLOW_DIST);
		}

		#endregion

		#region AI

		/// <summary>
		/// The attack target ordered by the owner
		/// </summary>
		protected GameLiving m_orderAttackTarget;

		/// <summary>
		/// Starts the brain thinking and resets the inactivity countdown
		/// </summary>
		/// <returns>true if started</returns>
		public override bool Start()
		{
			if (!base.Start()) return false;
			if (WalkState == eWalkState.Follow)
				FollowOwner();
			GameEventMgr.AddHandler(Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnOwnerAttacked));
			return true;
		}

		/// <summary>
		/// Stops the brain thinking
		/// </summary>
		/// <returns>true if stopped</returns>
		public override bool Stop()
		{
			if (!base.Stop()) return false;
			GameEventMgr.RemoveHandler(Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnOwnerAttacked));
			Owner.CommandNpcRelease();
			return true;
		}

		/// <summary>
		/// Do the mob AI
		/// </summary>
		public override void Think()
		{
			if (!Owner.CurrentUpdateArray[Body.ObjectID - 1])
			{
				Owner.Out.SendObjectUpdate(Body);
				Owner.CurrentUpdateArray[Body.ObjectID - 1] = true;
			}

			if (!WorldMgr.CheckDistance(Body, Owner, MAX_OWNER_FOLLOW_DIST))
				Owner.CommandNpcRelease();

			if (!Body.IsCasting && Body.Spells.Count > 0)
				CheckSpells();

			if (AggressionState == eAggressionState.Aggressive)
			{
				CheckPlayerAggro();
				CheckNPCAggro();
			}
		}

		/// <summary>
		/// Receives all messages of the body
		/// </summary>
		/// <param name="e">The event received</param>
		/// <param name="sender">The event sender</param>
		/// <param name="args">The event arguments</param>
		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			base.Notify(e, sender, args);

			if (!IsActive) return;

			if (sender == Body)
			{
				if (e == GameNPCEvent.FollowLostTarget)
				{
					return;
				}
			}
		}

		/// <summary>
		/// Lost follow target event
		/// </summary>
		/// <param name="target"></param>
		protected override void OnFollowLostTarget(GameObject target)
		{
			if (target == Owner)
			{
				Owner.CommandNpcRelease();
				return;
			}

			FollowOwner();
		}

		/// <summary>
		/// Add living to the aggrolist
		/// aggroamount can be negative to lower amount of aggro		
		/// </summary>
		/// <param name="living"></param>
		/// <param name="aggroamount"></param>
		public override void AddToAggroList(GameLiving living, int aggroamount)
		{
			if (living == Owner)
				return;
			base.AddToAggroList(living, aggroamount);
		}

		/// <summary>
		/// Returns the best target to attack
		/// </summary>
		/// <returns>the best target</returns>
		protected override GameLiving CalculateNextAttackTarget()
		{
			if (AggressionState == eAggressionState.Passive)
				return null;
			if (m_orderAttackTarget != null)
			{
				if (m_orderAttackTarget.IsAlive && m_orderAttackTarget.ObjectState == GameObject.eObjectState.Active)
					return m_orderAttackTarget;
				m_orderAttackTarget = null;
			}

			return base.CalculateNextAttackTarget();
		}

		/// <summary>
		/// Selects and attacks the next target or does nothing
		/// </summary>
		protected override void AttackMostWanted()
		{
			if (!IsActive)
				return;
			GameLiving target = CalculateNextAttackTarget();

			if (target != null)
			{
				if (!Body.AttackState || target != Body.TargetObject)
				{
					if (!Body.StartSpellAttack(target))
						Body.StartAttack(target);
				}
			}
			else
			{
				if (Body.AttackState)
					Body.StopAttack();

				if (WalkState == eWalkState.Follow)
					FollowOwner();
				else if (m_tempX > 0 && m_tempY > 0 && m_tempZ > 0)
					Body.WalkTo(m_tempX, m_tempY, m_tempZ, Body.MaxSpeed);
			}
		}

		/// <summary>
		/// Owner attacked event
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="arguments"></param>
		protected virtual void OnOwnerAttacked(DOLEvent e, object sender, EventArgs arguments)
		{
			// theurgist pets don't help their owner
			if (Owner.CharacterClass.ID == (int)eCharacterClass.Theurgist)
				return;

			AttackedByEnemyEventArgs args = arguments as AttackedByEnemyEventArgs;
			if (args == null) return;
			if (args.AttackData.Target is GamePlayer && (args.AttackData.Target as GamePlayer).ControlledNpc != this)
				return;
			// react only on these attack results
			switch (args.AttackData.AttackResult)
			{
				case GameLiving.eAttackResult.Blocked:
				case GameLiving.eAttackResult.Evaded:
				case GameLiving.eAttackResult.Fumbled:
				case GameLiving.eAttackResult.HitStyle:
				case GameLiving.eAttackResult.HitUnstyled:
				case GameLiving.eAttackResult.Missed:
				case GameLiving.eAttackResult.Parried:
					AddToAggroList(args.AttackData.Attacker, args.AttackData.Attacker.EffectiveLevel + args.AttackData.Damage + args.AttackData.CriticalDamage);
					break;
			}
		}

		protected override void BringFriends(AttackData ad)
		{
			// don't
		}

		#endregion
	}
}
