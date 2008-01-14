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
using System.Collections.Generic;
using System.Text;
using DOL.GS;
using System.Collections;
using System.Reflection;
using log4net;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;

namespace DOL.AI.Brain
{
	/// <summary>
	/// A brain for the commanders
	/// </summary>
	public class CommanderBrain : ControlledNpc
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public CommanderBrain(GameLiving owner)
			: base(owner)
		{
		}

		/// <summary>
		/// Determines if a given controlled brain is part of the commanders subpets
		/// </summary>
		/// <param name="brain">The brain to check</param>
		/// <returns>True if found, else false</returns>
		public bool FindPet(IControlledBrain brain)
		{
			if (Body.ControlledNpcList != null)
			{
				lock (Body.ControlledNpcList)
				{
					foreach (IControlledBrain icb in Body.ControlledNpcList)
						if (brain == icb)
							return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Direct the subpets to attack too
		/// </summary>
		/// <param name="target">The target to attack</param>
		public override void Attack(GameObject target)
		{
			base.Attack(target);
			//Check for any abilities
			CheckAbilities();
			if (Body.ControlledNpcList != null)
			{
				lock (Body.ControlledNpcList)
				{
					foreach (BDPetBrain icb in Body.ControlledNpcList)
						if (icb != null)
							icb.Attack(target);
				}
			}
		}

		/// <summary>
		/// Make sure the subpets are following the commander
		/// </summary>
		/// <param name="target"></param>
		public override void Follow(GameObject target)
		{
			base.Follow(target);
			SubpetFollow();
		}

		/// <summary>
		/// Direct all the sub pets to follow the commander
		/// </summary>
		private void SubpetFollow()
		{
			lock (Body.ControlledNpcList)
			{
				foreach (BDPetBrain icb in Body.ControlledNpcList)
					if (icb != null)
						icb.FollowOwner();
			}
		}

		/// <summary>
		/// Direct all the sub pets to follow the commander
		/// </summary>
		public override void Stay()
		{
			base.Stay();
			SubpetFollow();
		}

		/// <summary>
		/// Direct all the sub pets to follow the commander
		/// </summary>
		public override void ComeHere()
		{
			base.ComeHere();
			SubpetFollow();
		}

		/// <summary>
		/// Direct all the sub pets to follow the commander
		/// </summary>
		/// <param name="target"></param>
		public override void Goto(GameObject target)
		{
			base.Goto(target);
			SubpetFollow();
		}

		public override void SetAggressionState(eAggressionState state)
		{
			base.SetAggressionState(state);
			if (Body.ControlledNpcList != null)
			{
				lock (Body.ControlledNpcList)
				{
					foreach (BDPetBrain icb in Body.ControlledNpcList)
						if (icb != null)
							icb.SetAggressionState(state);
				}
			}
		}
	}
}
