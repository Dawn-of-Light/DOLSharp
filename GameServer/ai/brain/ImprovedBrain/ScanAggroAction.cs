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
	/// ScanAggroAction is the standard Action Handler for Aggroing Mobs and Aggro Handling in Fights
	/// </summary>
	public class ScanAggroAction : ABrainAction
	{
			
		public ScanAggroAction(ImprovedNpcBrain brain)
			: base(brain)
		{
			Duration = 0;
		}
		
		private GameObject m_nextTarget = null;
		
		public GameObject NextTarget
		{
			get
			{
				return m_nextTarget;
			}
			private set
			{
				m_nextTarget = value;
			}
		}

		
		#region IBrainAction Implementation
		public override long Analyze() {
			
			// doesn't handle "dying", changing Aggro would not give any results...
			switch(Brain.GetBrainState()) {
				// Just scan for aggro
				case eBrainStatus.idle :
					return ScanForAggroInVincity();
				
				// Check for aggro amount changing target
				case eBrainStatus.incoming :
				case eBrainStatus.fighting :
				case eBrainStatus.neardeath :
					return UpdateAggroTarget();
				
			}
			
			return long.MinValue;
		}
		
		
		
		public override void Action() 
		{			
			switch(Brain.GetBrainState()) {
				// If we're asked to do something on Idle it must be aggroing !
				case eBrainStatus.idle :
					Brain.SetBrainState(eBrainStatus.incoming);
					Brain.CurrentTarget = NextTarget;
					// TODO :
					
					// Fire some Ambient sentence
					
					// Add Target to aggro list
					
				break;
				// Check for aggro amount changing target
				case eBrainStatus.incoming :
				case eBrainStatus.fighting :
				case eBrainStatus.neardeath :
					Brain.CurrentTarget = NextTarget;
				break;
			}
		}
		
		#endregion
		
		#region IBrainNotifier Implementation

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			// TODO : if someone sneeze AGGRO !
		}

		#endregion
		
		#region Methods
		
		private long ScanForAggroInVincity() 
		{
			
			// TODO : LoS checks
			
			// check player in the vincinity
			foreach(GamePlayer player in Brain.Body.GetPlayersInRadius((ushort)Brain.AggroRange)) {
				// check for specific aggro to this target
				if(Util.Chance(Brain.CalculateAggroLevelToTarget(player))) {
					// too bad it aggro...
					NextTarget = player;
					return 1;
				}
			
			}
			// check npcs in the vincinity
			foreach(GameNPC npc in Brain.Body.GetNPCsInRadius((ushort)Brain.AggroRange)) {
				// check for specific aggro to this target
				if(Util.Chance(Brain.CalculateAggroLevelToTarget(npc))) {
					// too bad it aggro...
					NextTarget = npc;
					return 1;
				}
					
			}
			
			return long.MinValue;
		}
		
		private long UpdateAggroTarget() 
		{
			
			
			return long.MinValue;			
		}
		
		#endregion

	}
}
