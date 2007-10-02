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

namespace DOL.AI.Brain
{
	/// <summary>
	/// Defines walk state when brain is not in combat
	/// </summary>
	public enum eWalkState
	{
		/// <summary>
		/// Follow the owner
		/// </summary>
		Follow,
		/// <summary>
		/// Don't move if not in combat
		/// </summary>
		Stay,
		ComeHere,
		GoTarget,
	}

	/// <summary>
	/// Defines aggression level of the brain
	/// </summary>
	public enum eAggressionState
	{
		/// <summary>
		/// Attack any enemy in range
		/// </summary>
		Aggressive,
		/// <summary>
		/// Attack anything that attacks brain owner or owner of brain owner
		/// </summary>
		Defensive,
		/// <summary>
		/// Attack only on order
		/// </summary>
		Passive,
	}

	/// <summary>
	/// Interface for controllable brains
	/// </summary>
	public interface IControlledBrain
	{
		eWalkState WalkState { get; }
		eAggressionState AggressionState { get; set; }
		GameNPC Body { get; }
		GameLiving Owner { get; }
		void Attack(GameObject target);
		void Follow(GameObject target);
        void FollowOwner();
		void Stay();
		void ComeHere();
		void Goto(GameObject target);
		void UpdatePetWindow();
		GamePlayer GetPlayerOwner();
		bool IsMinion { get; set; }
        bool IsMainPet { get; set; }
	}
}
