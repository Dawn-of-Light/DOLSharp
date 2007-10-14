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
using log4net;
using System.Reflection;

namespace DOL.AI.Brain
{
	/// <summary>
	/// A retriever type mob is an NPC that is spawned from a boss-like
	/// mob (its master). Upon spawning, the master mob orders the
	/// retriever to make for a certain location; once the retriever
	/// has reached its target it reports back to its master. The player's
	/// task usually is to prevent the retriever from reaching its target,
	/// because bad things may happen should it succeed.
	/// </summary>
    class RetrieverMobBrain : StandardMobBrain
    {
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private enum State { Passive, GettingHelp, Aggressive };
        private State m_state;

        /// <summary>
        /// Mob brain main loop.
        /// </summary>
        public override void Think()
        {
            if (m_state != State.Aggressive) return;
            base.Think();
        }

        private GameNPC m_master = null;

        /// <summary>
        /// The NPC that spawned this retriever.
        /// </summary>
        public GameNPC Master
        {
            get { return m_master; }
			set { m_master = value; }
        }

        /// <summary>
        /// Called whenever the NPC's body sends something to its brain.
        /// </summary>
        /// <param name="e">The event that occured.</param>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The event details.</param>
        public override void Notify(DOL.Events.DOLEvent e, object sender, EventArgs args)
        {
            // When we get the WalkTo event we start running towards the target
            // location; once we've arrived we'll tell our master. If someone
            // attacks us before we can get to the target location, we'll do what
            // any other mob would do.

            base.Notify(e, sender, args);
            if (e == GameNPCEvent.WalkTo && m_state == State.Passive)
                m_state = State.GettingHelp;
            else if (e == GameNPCEvent.ArriveAtTarget && m_state == State.GettingHelp)
            {
				if (Master != null && Master.Brain != null)
					Master.Brain.Notify(GameNPCEvent.ArriveAtTarget, this.Body, new EventArgs());
                m_state = State.Aggressive;
            }
            else if (e == GameNPCEvent.TakeDamage)
                m_state = State.Aggressive;
        }
    }
}
