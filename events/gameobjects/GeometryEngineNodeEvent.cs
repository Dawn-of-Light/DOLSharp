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
using DOL.GS;

namespace DOL.Events
{
	/// <summary>
	/// This class holds all possible GeometryEngineNode events.
	/// Only constants defined here!
	/// </summary>
	public class GeometryEngineNodeEvent : DOLEvent
	{
		/// <summary>
		/// Constructs a new GameObjectEvent
		/// </summary>
		/// <param name="name">the name of the event</param>
		protected GeometryEngineNodeEvent(string name) : base(name)
		{
		}

		/// <summary>
		/// Tests if this event is valid for the specified object
		/// </summary>
		/// <param name="o">The object for which the event wants to be registered</param>
		/// <returns>true if valid, false if not</returns>
		public override bool IsValidFor(object o)
		{
			return o is GeometryEngineNode;
		}

		/// <summary>
		/// The AddToWorld event is fired whenever the object is added to the world
		/// </summary>
		public static readonly GeometryEngineNodeEvent AddToWorld = new GeometryEngineNodeEvent("GeometryEngineNodeEvent.AddToWorld");
		/// <summary>
		/// The RemoveFromWorld event is fired whenever the object is removed from the world
		/// </summary>
		public static readonly GeometryEngineNodeEvent RemoveFromWorld = new GeometryEngineNodeEvent("GeometryEngineNodeEvent.RemoveFromWorld");
		/// <summary>
		/// The MoveTo event is fired whenever the object is moved to a new position by the MoveTo method
		/// <seealso cref="MoveToEventArgs"/>
		/// </summary>
		public static readonly GeometryEngineNodeEvent MoveTo = new GeometryEngineNodeEvent("GeometryEngineNodeEvent.MoveTo");
	}
}
