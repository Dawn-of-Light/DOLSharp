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

using DOL.GS;
using DOL.GS.Geometry;

namespace DOL.AI.Brain
{
	public class FearBrain : StandardMobBrain
	{
		/// <summary>
		/// Fixed thinking Interval for Fleeing
		/// </summary>
		public override int ThinkInterval {
			get {
				return 3000;
			}
		}
		
		/// <summary>
		/// Flee from Players on Brain Think
		/// </summary>
		public override void Think()
		{
			foreach (GamePlayer player in Body.GetPlayersInRadius((ushort)Math.Max(AggroRange, 750)))
			{
				CalculateFleeTarget(player);
				break;
			}
		}

		protected virtual void CalculateFleeTarget(GameLiving target)
		{
			var targetAngle = Body.Coordinate.GetOrientationTo(target.Coordinate) + Angle.Degrees(180);

			Body.StopFollowing();
			Body.StopAttack();
            var destination = Body.Position + Vector.Create(targetAngle, length: 300);
			Body.PathTo(destination.Coordinate, Body.MaxSpeed);
		}
	}
} 
