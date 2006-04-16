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
using System.Collections;
using DOL.Events;

namespace DOL.GS
{
	/// <summary>
	/// Interface for all moving objects handled in the geometry engine
	/// </summary>
	public interface IMovingGameObject
	{
		/// <summary>
		/// Returns the region where we are
		/// </summary>
		Region	Region { get; }

		/// <summary>
		/// Gets the movement target position.
		/// </summary>
		Point	Position { get; set; }
		
		/// <summary>
		/// Gets the current direction the Object is facing
		/// </summary>
		int Heading { get; set; }
		
		/// <summary>
		/// Gets or sets the current speed of this object
		/// </summary>
		int CurrentSpeed { get; set; }

		/// <summary>
		/// Gets the movement target position.
		/// </summary>
		Point TargetPosition { get; set; }
		
		/// <summary>
		/// Gets  / set the current movement action of the object
		/// </summary>
		MovementAction	MovementAction { get; set; }

		/// <summary>
		/// The WalkTo method
		/// </summary>
		void WalkTo(Point walkTarget, int speed);
		
		/// <summary>
		/// The StopMoving method
		/// </summary>
		void StopMoving();
		
		/// <summary>
		/// Gets the last time this mob was updated
		/// </summary>
		uint LastUpdateTickCount { get; }

		/// <summary>
		/// Broadcasts the moving object to all players around
		/// </summary>
		void BroadcastUpdate();

		/// <summary>
		/// All notify method used to fire event
		/// </summary>
		void Notify(DOLEvent e, object sender, EventArgs args);
		void Notify(DOLEvent e, object sender);
		void Notify(DOLEvent e);
		void Notify(DOLEvent e, EventArgs args);
	}
}
