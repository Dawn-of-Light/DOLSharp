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

namespace DOL.GS.Spells
{
	/// <summary>
	/// Bolt Spell Helper allow to add a Bolt Component to any spell with needed Timers and animation.
	/// </summary>
	public class BoltSpellHelper
	{
		private IBoltSpellHandler m_spellHandler;
		
		public BoltSpellHelper(IBoltSpellHandler handler)
		{
			m_spellHandler = handler;
		}
		
		public virtual void StartBoltTimer(GameLiving target, double effectiveness)
		{
			BoltOnTargetAction boltAction = new BoltOnTargetAction(m_spellHandler.Caster, target, effectiveness, this);
			int ticksToTarget = m_spellHandler.Caster.GetDistanceTo(target) * 100 / 85; // 85 units per 1/10s
			int delay = 1 + ticksToTarget / 100;
			// animation
			m_spellHandler.SendEffectAnimation(target, (ushort)delay, false, 1);
			// start timer
			boltAction.Start(1 + ticksToTarget);
		}
		
		public virtual void OnBoltArrival(GameLiving target, double effectiveness)
		{
			if (target == null)
				return;
			if (target.CurrentRegionID != m_spellHandler.Caster.CurrentRegionID)
				return;
			if (target.ObjectState != GameObject.eObjectState.Active)
				return;
			if (!target.IsAlive)
				return;
			
			m_spellHandler.OnBoltArrival(target, effectiveness);
		}
		
		/// <summary>
		/// Delayed action when bolt reach the target
		/// </summary>
		protected class BoltOnTargetAction : RegionAction
		{
			/// <summary>
			/// The bolt target
			/// </summary>
			protected readonly GameLiving m_boltTarget;
			
			/// <summary>
			/// The bolt Effectiveness
			/// </summary>
			protected readonly double m_boltEffectiveness;
			
			/// <summary>
			/// The bolt Helper handling the spell.
			/// </summary>
			protected readonly BoltSpellHelper m_boltSpellHelper;
			
			
			public BoltOnTargetAction(GameLiving actionSource, GameLiving boltTarget, double effectiveness, BoltSpellHelper spellHelper) : base(actionSource)
			{
				if (boltTarget == null)
					throw new ArgumentNullException("boltTarget");
				if (spellHelper == null)
					throw new ArgumentNullException("spellHandler");
				
				m_boltTarget = boltTarget;
				m_boltEffectiveness = effectiveness;
				m_boltSpellHelper = spellHelper;
			}
			
			/// <summary>
			/// On Bolt Arrival !
			/// </summary>
			protected override void OnTick()
			{
				m_boltSpellHelper.OnBoltArrival(m_boltTarget, m_boltEffectiveness);
			}
			
		}
	}
}
