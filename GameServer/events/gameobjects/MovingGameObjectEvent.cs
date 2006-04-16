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
	/// This class holds all possible GameNPC events.
	/// Only constants defined here!
	/// </summary>
	public class MovingGameObjectEvent : DOLEvent
	{
		/// <summary>
		/// Constructs a new GameNPCEvent
		/// </summary>
		/// <param name="name">the event name</param>
		protected MovingGameObjectEvent(string name) : base(name)
		{
		}

		/// <summary>
		/// Tests if this event is valid for the specified object
		/// </summary>
		/// <param name="o">The object for which the event wants to be registered</param>
		/// <returns>true if valid, false if not</returns>
		public override bool IsValidFor(object o)
		{
			return o is IMovingGameObject;
		}

		/// <summary>
		/// The WalkTo event is fired whenever the npc is commanded to walk to a specific target
		/// </summary>
		public static readonly MovingGameObjectEvent WalkTo = new MovingGameObjectEvent("MovingGeometryEngineNodeEvent.WalkTo");
		
		/// <summary>
		/// The TurnTo event is fired whenever the npc turns towards some coordinates
		/// </summary>
		public static readonly MovingGameObjectEvent ArriveAtTarget = new MovingGameObjectEvent("MovingGeometryEngineNodeEvent.ArriveAtTarget");
	}
}
