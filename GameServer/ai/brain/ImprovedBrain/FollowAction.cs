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

using DOL.GS;
using DOL.Events;


namespace DOL.AI.Brain
{
	/// <summary>
	/// Follow action for controlled pet following their master.
	/// </summary>
	public class FollowAction : ABrainAction
	{
		
		private GameLiving m_master;
		
		public GameLiving Master
		{
			get
			{
				return m_master;
			}
			private set
			{
				m_master = value;
			}
		}
		
		public override ulong Duration {
			get { return (ulong)Math.Abs(Brain.Body.GetTicksToArriveAt(Master, Brain.Body.CurrentSpeed)); }
		}
		
		public FollowAction(ImprovedNpcBrain brain)
			: base(brain)
		{
		}
		
		public override long Analyze()
		{
			// This is controlled brain
			if(Brain is IControlledBrain) {
				Master = ((IControlledBrain)Brain).GetLivingOwner();
			}
			
			if(Brain.GetBrainState() == eBrainStatus.idle) 
			{
				return ImprovedNpcBrain.MAX_BRAIN_ACTION_SCORE;
			}
			else
			{
				return ImprovedNpcBrain.MIN_BRAIN_ACTION_SCORE;
			}
		}
		
		public override void Action()
		{
			if(Master == null)
				Master = (GameLiving)Brain.CurrentTarget;
			
			if(Brain.Body.CurrentFollowTarget != Master)
				Brain.Body.Follow(Master, 100, 100);
		}
		
		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			if((sender is GameNPC) && sender == Brain.Body && e is GameNPCEvent && e == GameNPCEvent.ArriveAtTarget) {
				Break();
			}
		}
		
		public override void Break()
		{
			base.Break();
			Brain.Body.StopFollowing();
		}
	}
}
