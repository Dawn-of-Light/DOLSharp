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
using DOL.Events;
using DOL.GS;

namespace DOL.AI.Brain
{
	public class TheurgistPetBrain : StandardMobBrain
	{
		private GamePlayer m_owner;

		public TheurgistPetBrain(GamePlayer owner)
		{
			m_owner = owner;
			AggroLevel = 100;
		}

		public override void Think()
		{
			AttackMostWanted();
		}

		/// <summary>
		/// Receives all messages of the body.
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			if (!IsActive) return;

			if (sender == Body)
			{
				if (e == GameLivingEvent.Dying)
				{
					ClearAggroList();
					return;
				}

				if (e == GameNPCEvent.FollowLostTarget)
				{
					FollowLostTargetEventArgs eArgs = args as FollowLostTargetEventArgs;
					if (eArgs == null) return;
					OnFollowLostTarget(eArgs.LostTarget);
					return;
				}
			}
		}
	}
}
