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

namespace DOL.AI.Brain
{
	public abstract class BDPetBrain : ControlledNpc
	{
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
				double heading = ((double)commander.Heading) * Point2D.HEADING_TO_RADIAN;
				//Get which place we should put minion
				int i = 0;
				//How much do we want to slide back and left/right
				int perp_slide = 0;
				int par_slide = 0;
				for (; i < commander.ControlledNpcList.Length; i++)
				{
					if (commander.ControlledNpcList[i] == this)
						break;
				}
				switch (commander.Formation)
				{
					case GameNPC.eFormationType.Triangle:
						par_slide = BASEFORMATIONDIST;
						perp_slide = BASEFORMATIONDIST;
						if (i != 0)
							par_slide = BASEFORMATIONDIST * 2;
						break;
					case GameNPC.eFormationType.Line:
						par_slide = BASEFORMATIONDIST * (i + 1);
						break;
					case GameNPC.eFormationType.Protect:
						switch (i)
						{
							case 0:
								par_slide = -BASEFORMATIONDIST * 2;
								break;
							case 1:
							case 2:
								par_slide = -BASEFORMATIONDIST;
								perp_slide = BASEFORMATIONDIST;
								break;
						}
						break;
				}
				//Slide backwards - every pet will need to do this anyways
				x += (int)(((double)commander.FormationSpacing * par_slide) * Math.Cos(heading - Math.PI / 2));
				y += (int)(((double)commander.FormationSpacing * par_slide) * Math.Sin(heading - Math.PI / 2));
				//In addition with sliding backwards, slide the other two pets sideways
				switch (i)
				{
					case 1:
						x += (int)(((double)commander.FormationSpacing * perp_slide) * Math.Cos(heading - Math.PI));
						y += (int)(((double)commander.FormationSpacing * perp_slide) * Math.Sin(heading - Math.PI));
						break;
					case 2:
						x += (int)(((double)commander.FormationSpacing * perp_slide) * Math.Cos(heading));
						y += (int)(((double)commander.FormationSpacing * perp_slide) * Math.Sin(heading));
						break;
				}
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
			//edit for BD
			GamePlayer playerowner = GetPlayerOwner();
			if (!playerowner.CurrentUpdateArray[Body.ObjectID - 1])
			{
				playerowner.Out.SendObjectUpdate(Body);
				playerowner.CurrentUpdateArray[Body.ObjectID - 1] = true;
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
			CheckSpells(eCheckSpellType.Defensive);

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