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
using DOL.GS.Movement;
using log4net;

namespace DOL.AI.Brain
{
	/// <summary>
	/// A brain that make npc walk on rounds with way point
	/// </summary>
	public class RoundsBrain : StandardMobBrain
	{
		/// <summary>
		/// Load the path of the mob
		/// </summary>
		/// <returns>True if is ok</returns>
		public override bool Start()
		{
			if (!base.Start()) return false;
			Body.CurrentWayPoint = MovementMgr.LoadPath(Body.PathID != "" ? Body.PathID : Body.InternalID + " Rounds");
			Body.MoveOnPath(Body.CurrentWayPoint.MaxSpeed);
			return true;
		}
		/// <summary>
		/// Add living to the aggrolist
		/// save path of player before attack to walk back to way point after fight
		/// </summary>
		/// <param name="living"></param>
		/// <param name="aggroamount"></param>
		public override void AddToAggroList(GameLiving living, int aggroamount)
		{
			//save current position in path go to here and reload path point
			//insert path in pathpoint
			PathPoint temporaryPathPoint = new PathPoint(Body.X, Body.Y, Body.Z, Body.CurrentSpeed, Body.CurrentWayPoint.Type);
			temporaryPathPoint.Next = Body.CurrentWayPoint;
			temporaryPathPoint.Prev = Body.CurrentWayPoint.Prev;
			Body.CurrentWayPoint = temporaryPathPoint;
			//this path point will be not available after the following point because no link to itself
			base.AddToAggroList(living, aggroamount);
		}

		/// <summary>
		/// Returns the best target to attack
		/// if no target go to saved pathpoint to continue the round
		/// </summary>
		/// <returns>the best target</returns>
		protected override GameLiving CalculateNextAttackTarget()
		{
			GameLiving living = base.CalculateNextAttackTarget();
			if (living == null)
				Body.MoveOnPath(Body.CurrentWayPoint.MaxSpeed);
			return living;
		}
	}
}
