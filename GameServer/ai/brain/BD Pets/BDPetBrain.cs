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
using DOL.Events;
using DOL.GS;
using DOL.GS.Geometry;

namespace DOL.AI.Brain
{
	public abstract class BDPetBrain : ControlledNpcBrain
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		protected const int BASEFORMATIONDIST = 50;

		public BDPetBrain(GameLiving Owner) : base(Owner)
		{
			IsMainPet = false;
		}

		/// <summary>
		/// Find the player owner of the pets at the top of the tree
		/// </summary>
		/// <returns>Player owner at the top of the tree.  If there was no player, then return null.</returns>
		public override GamePlayer GetPlayerOwner()
		{
			GameNPC commanderOwner = (GameNPC)Owner;
			if (commanderOwner != null && commanderOwner.Brain is IControlledBrain)
			{
				GamePlayer playerOwner = (commanderOwner.Brain as IControlledBrain).Owner as GamePlayer;
				return playerOwner;
			}
			return null;
		}

		/// <summary>
		/// Are minions assisting the commander?
		/// </summary>
		public bool MinionsAssisting
		{ 
			get { return Owner is CommanderPet commander && commander.MinionsAssisting; } 
		}

		public override void SetAggressionState(eAggressionState state)
		{
			if (MinionsAssisting)
				base.SetAggressionState(state);
			else
				base.SetAggressionState(eAggressionState.Passive);

			// Attack immediately rather than waiting for the next Think()
			if (AggressionState != eAggressionState.Passive)
				Attack(Owner.TargetObject);
				
		}

		/// <summary>
		/// This method is called at the end of the attack sequence to
		/// notify objects if they have been attacked/hit by an attack
		/// </summary>
		/// <param name="ad">information about the attack</param>
		protected override void OnAttackedByEnemy(AttackData ad)
		{
			base.OnAttackedByEnemy(ad);

			// Get help from the commander and other minions
			if (ad.CausesCombat && Owner is GamePet own && own.Brain is CommanderBrain ownBrain)
				ownBrain.DefendMinion(ad.Attacker);
		}

		/// <summary>
		/// Updates the pet window
		/// </summary>
		public override void UpdatePetWindow() { }

		/// <summary>
		/// Stops the brain thinking
		/// </summary>
		/// <returns>true if stopped</returns>
		public override bool Stop()
		{
			if (!base.Stop()) return false;

			GameEventMgr.Notify(GameLivingEvent.PetReleased, Body);
			return true;
		}

		/// <summary>
		/// Start following the owner
		/// </summary>
		public override void FollowOwner()
		{
			Body.StopAttack();
			Body.Follow(Owner, MIN_OWNER_FOLLOW_DIST, MAX_OWNER_FOLLOW_DIST);
		}

		/// <summary>
		/// Checks for the formation position of the BD pet
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <returns></returns>
		public override bool CheckFormation(ref int x, ref int y, ref int z)
		{
			if (!Body.AttackState && Body.Attackers.Count == 0)
			{
				GameNPC commander = (GameNPC)Owner;
				var heading = commander.Orientation.InRadians;
                var commanderOrientation = commander.Orientation;
				//Get which place we should put minion
				int i = 0;
				//How much do we want to slide back and left/right
				for (; i < commander.ControlledNpcList.Length; i++)
				{
					if (commander.ControlledNpcList[i] == this)
						break;
				}
                var offset = Vector.Zero;
                var spacing = BASEFORMATIONDIST * commander.FormationSpacing;
                switch (commander.Formation)
                {
                    case GameNPC.eFormationType.Triangle:
                        switch (i)
                        {
                            case 0: offset = Vector.Create(0, -spacing); break;
                            case 1: offset = Vector.Create(spacing, -spacing * 2); break;
                            case 2: offset = Vector.Create(-spacing, -spacing * 2); break;
                        }
                        break;
                    case GameNPC.eFormationType.Line:
                        switch (i)
                        {
                            case 0: offset = Vector.Create(0, -spacing); break;
                            case 1: offset = Vector.Create(0, -spacing * 2); break;
                            case 2: offset = Vector.Create(0, -spacing * 3); break;
                        }
                        break;
                    case GameNPC.eFormationType.Protect:
                        switch (i)
                        {
                            case 0: offset = Vector.Create(0, spacing * 2); break;
                            case 1: offset = Vector.Create(spacing, spacing); break;
                            case 2: offset = Vector.Create(-spacing, spacing); break;
                        }
                        break;
                }
                offset = offset.RotatedClockwise(commanderOrientation);
                x += offset.X;
                y += offset.Y;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Lost follow target event
		/// </summary>
		/// <param name="target"></param>
		protected override void OnFollowLostTarget(GameObject target)
		{
			if (target == Owner)
			{
				GameEventMgr.Notify(GameLivingEvent.PetReleased, Body);
				return;
			}
			FollowOwner();
		}

		/// <summary>
		/// The interval for thinking, 1.5 seconds
		/// </summary>
		public override int ThinkInterval
		{
			get { return 1500; }
		}

		/// <summary>
		/// Standard think method for all the pets
		/// </summary>
		public override void Think()
		{
			GamePlayer playerowner = GetPlayerOwner();
			if (playerowner != null && (GameTimer.GetTickCount() - playerowner.Client.GameObjectUpdateArray[new Tuple<ushort, ushort>(Body.CurrentRegionID, (ushort)Body.ObjectID)]) > ThinkInterval)
			{
				playerowner.Out.SendObjectUpdate(Body);
			}

			//See if the pet is too far away, if so release it!
			if (!Body.IsWithinRadius(Owner, MAX_OWNER_FOLLOW_DIST))
			{
				if (Body.IsCasting)
					Body.StopCurrentSpellcast();
				else
					GameEventMgr.Notify(GameLivingEvent.PetReleased, Body);
			}

			if ((!Body.AttackState && !Body.IsCasting && !Body.InCombat && m_orderAttackTarget == null) || AggressionState == eAggressionState.Passive)
			{
				FollowOwner();
			}

			//Check for buffs, heals, etc
			if (!Body.InCombat) CheckSpells(eCheckSpellType.Defensive);

			if (AggressionState == eAggressionState.Aggressive)
			{
				CheckPlayerAggro();
				CheckNPCAggro();
			}

			if (AggressionState != eAggressionState.Passive)
				AttackMostWanted();
		}

		public override void Attack(GameObject target)
		{
			base.Attack(target);
			//Check for any abilities
			CheckAbilities();
		}

		public override eWalkState WalkState
		{
			get
			{
				return eWalkState.Follow;
			}
			set	{ }
		}
	}
}