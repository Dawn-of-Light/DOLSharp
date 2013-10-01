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
using DOL.Events;

namespace DOL.AI.Brain
{
	/// <summary>
	/// Abstract Brain Action to make a base implementation of Interface Brain Action
	/// </summary>
	public abstract class ABrainAction : IBrainAction, IDOLEventHandler
	{
		
		/// <summary>
		/// Brain Action need NPC Brain to change his behavior or get Environnement data
		/// </summary>
		public ABrainAction(ImprovedNpcBrain brain)
		{
			this.m_brain = brain;
		}
		
		/// <summary>
		/// The stored brain, set only once.
		/// </summary>		
		private readonly ImprovedNpcBrain m_brain;

		/// <summary>
		/// brain retriever, read only.
		/// </summary>		
		public ImprovedNpcBrain Brain
		{
			get
			{
				return m_brain;
			}
		}

		private bool m_break;
		
		public bool Breaking
		{
			get
			{
				return m_break;
			}
		}
		
		/// <summary>
		/// Duration of the computed Action(s).
		/// Should not be set outside of BrainAction subclass
		/// </summary>		
		private ulong m_duration = 0;
		
		public virtual ulong Duration
		{
			get
			{
				return m_duration;
			}
			protected set
			{
				m_duration = value;
			}
		}

		/// <summary>
		/// Check the status around this Action and prepare a move
		/// </summary>
		/// <returns>Score of this Analyze, Higher Score = more chance to trigger</returns>
		public abstract long Analyze();

		/// <summary>
		/// Do what expected after Analyze
		/// </summary>		
		public abstract void Action();

		/// <summary>
		/// Break Any Pending Action
		/// </summary>		
		public virtual void Break()
		{
			Duration = 0;
			m_break = true;
		}

		/// <summary>
		/// Get Messages from Brain.
		/// </summary>
		public abstract void Notify(DOLEvent e, object sender, EventArgs args);
		
	}
}
