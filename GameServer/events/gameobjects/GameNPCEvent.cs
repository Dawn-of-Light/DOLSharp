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
	public class GameNPCEvent : GameLivingEvent
	{
		/// <summary>
		/// Constructs a new GameNPCEvent
		/// </summary>
		/// <param name="name">the event name</param>
		protected GameNPCEvent(string name) : base(name)
		{
		}

		/// <summary>
		/// Tests if this event is valid for the specified object
		/// </summary>
		/// <param name="o">The object for which the event wants to be registered</param>
		/// <returns>true if valid, false if not</returns>
		public override bool IsValidFor(object o)
		{
			return o is GameNPC;
		}

		/// <summary>
		/// The TurnTo event is fired whenever the npc turns towards some coordinates
		/// <seealso cref="TurnToEventArgs"/>
		/// </summary>
		public static readonly GameNPCEvent TurnTo = new GameNPCEvent("GameNPC.TurnTo");
		/// <summary>
		/// The TurnToHeading event is fired whenever the npc turns towards a specific heading
		/// <seealso cref="TurnToHeadingEventArgs"/>
		/// </summary>
		public static readonly GameNPCEvent TurnToHeading = new GameNPCEvent("GameNPC.TurnToHeading");
		/// <summary>
		/// The ArriveAtTarget event is fired whenever the npc arrives at it's WalkTo target
		/// <see cref="DOL.GS.GameNPC.WalkTo(int, int, int, int)"/>
		/// </summary>
		public static readonly GameNPCEvent ArriveAtTarget = new GameNPCEvent("GameNPC.ArriveAtTarget");
		/// <summary>
		/// The CloseToTarget event is fired whenever the npc is close to it's WalkTo target
		/// <see cref="DOL.GS.GameNPC.WalkTo(int, int, int, int)"/>
		/// </summary>
		public static readonly GameNPCEvent CloseToTarget = new GameNPCEvent("GameNPC.CloseToTarget");
		/// <summary>
		/// The WalkTo event is fired whenever the npc is commanded to walk to a specific target
		/// <seealso cref="WalkToEventArgs"/>
		/// </summary>
		public static readonly GameNPCEvent WalkTo = new GameNPCEvent("GameNPC.WalkTo");
		/// <summary>
		/// The Walk event is fired whenever the npc is commanded to walk
		/// <seealso cref="WalkEventArgs"/>
		/// </summary>
		public static readonly GameNPCEvent Walk = new GameNPCEvent("GameNPC.Walk");
		/// <summary>
		/// The RiderMount event is fired whenever the npc is mounted by a ride
		/// <seealso cref="RiderMountEventArgs"/>
		/// </summary>
		public static readonly GameNPCEvent RiderMount = new GameNPCEvent("GameNPC.RiderMount");
		/// <summary>
		/// The RiderDismount event is fired whenever the rider dismounts from the npc
		/// <seealso cref="RiderDismountEventArgs"/>
		/// </summary>
		public static readonly GameNPCEvent RiderDismount = new GameNPCEvent("GameNPC.RiderDismount");
		/// <summary>
		/// Fired when pathing starts
		/// </summary>
		public static readonly GameNPCEvent PathMoveStarts = new GameNPCEvent("GameNPC.PathMoveStarts");
		/// <summary>
		/// Fired when npc is on end of path
		/// </summary>
		public static readonly GameNPCEvent PathMoveEnds = new GameNPCEvent("GameNPC.PathMoveEnds");
		/// <summary>
		/// Fired on every AI callback
		/// </summary>
		public static readonly GameNPCEvent OnAICallback = new GameNPCEvent("GameNPC.OnAICallback");
		/// <summary>
		/// Fired whenever following NPC lost its target
		/// </summary>
		public static readonly GameNPCEvent FollowLostTarget = new GameNPCEvent("GameNPC.FollowLostTarget");
		/// <summary>
		/// Fired whenever pet is supposed to cast a spell.
		/// </summary>
		public static readonly GameNPCEvent PetSpell = new GameNPCEvent("GameNPC.PetSpell");
		/// <summary>
		/// Fired whenever pet is out of tether range (necromancer).
		/// </summary>
		public static readonly GameNPCEvent OutOfTetherRange = new GameNPCEvent("GameNPC.OutOfTetherRange");
		/// <summary>
		/// Fired when pet is lost (necromancer).
		/// </summary>
		public static readonly GameNPCEvent PetLost = new GameNPCEvent("GameNPC.PetLost");
	}
}
