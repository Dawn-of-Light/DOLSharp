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
using DOL.Events;
using log4net;
using System.Reflection;

namespace DOL.AI.Brain
{
    /// <summary>
    /// Brain for scout mobs. Scout mobs are NPCs that will not aggro
    /// on a player of their own accord, instead, they'll go searching
    /// for adds around the area and make those aggro on a player.
    /// </summary>
    class ScoutMobBrain : StandardMobBrain
    {
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Mob brain main loop.
        /// </summary>
        public override void Think()
        {
            if (IsGettingHelp) return;      // Ignore everyone while running for help.
            base.Think();
        }

        private bool m_scouting = true;

        /// <summary>
        /// Whether this mob is scouting or not; if a mob is scouting it
        /// means the mob is still looking for players.
        /// </summary>
        public virtual bool IsScouting
        {
            get { return m_scouting; }
            set { m_scouting = value; }
        }

        private ArrayList m_targetList = new ArrayList();

        /// <summary>
        /// Check if there are any players around.
        /// </summary>
        protected override void CheckPlayerAggro()
        {
            // If mob is not scouting anymore, it is either still on its way to
            // get help or it has finished doing that, in which case it will
            // behave like an ordinary mob and aggro players.

			log.Info(String.Format("--> CheckPlayerAggro(): IsScouting = {0}, IsGettingHelp = {1}",
				IsScouting ? "true" : "false",
				IsGettingHelp ? "true" : "false"));

            if (!IsScouting)
            {
				log.Info("    default behaviour");
                if (!IsGettingHelp) base.CheckPlayerAggro();
				log.Info("<--- CheckPlayerAggro() [1]");
                return;
            }

            // Add all players in range to this scout's target list. The scout
            // will pick a random target from its list to make the add aggro on.

            m_targetList.Clear();
            foreach (GamePlayer player in Body.GetPlayersInRadius((ushort)AggroRange))
                if (!m_targetList.Contains(player))
                    m_targetList.Add(player);

			log.Info(String.Format("    {0} players in range", m_targetList.Count));

            // Once we got at least one player we stop scouting and run for help.

			if (m_targetList.Count > 0)
			{
				log.Info("    getting help");
				IsScouting = false;
				GetHelp();
			}
			log.Info("<--- CheckPlayerAggro() [2]");
        }

        private ushort m_scoutRange = 3000;

        /// <summary>
        /// The range the scout will look for adds in.
        /// </summary>
        public ushort ScoutRange
        {
            get { return m_scoutRange; }
        }

        private bool m_gettingHelp = false;

        /// <summary>
        /// Whether or not this mob is on its way to get help.
        /// </summary>
        public bool IsGettingHelp
        {
            get { return m_gettingHelp; }
            set { m_gettingHelp = value; }
        }

        /// <summary>
        /// The NPC this scout has picked to help.
        /// </summary>
        private GameNPC m_helperNPC = null;

        /// <summary>
        /// Look for potential adds in the area and be on your way.
        /// </summary>
        /// <returns></returns>
        protected void GetHelp()
        {
            // Nothing to get help for.

            if (m_targetList.Count == 0) return;

            // Find all mobs in scout range.

            ArrayList addList = new ArrayList();
            foreach (GameNPC npc in Body.GetNPCsInRadius(ScoutRange))
                if (npc.IsFriend(Body) && npc.IsAggressive && npc.IsAvailable)
                    addList.Add(npc);

            // If there is no help available, fall back on standard mob
            // behaviour.

            if (addList.Count == 0)
            {
				ReportTargets(Body);
                m_targetList.Clear();
                IsGettingHelp = false;
                return;
            }

            // Pick a random NPC from the list and go for it.

            IsGettingHelp = true;
            m_helperNPC = (GameNPC) addList[Util.Random(1, addList.Count)-1];
            Body.Follow(m_helperNPC, 90, ScoutRange);
        }

		/// <summary>
		/// Add targets to an NPC's aggro table.
		/// </summary>
		/// <param name="npc">The NPC to aggro on the targets.</param>
		private void ReportTargets(GameNPC npc)
		{
			if (npc == null) return;

			// Assign a random amount of aggro for each target, that way 
			// different NPCs will attack different targets first.

			StandardMobBrain brain = npc.Brain as StandardMobBrain;
			foreach (GameLiving target in m_targetList)
				brain.AddToAggroList(target, Util.Random(1, m_targetList.Count));
		}

        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
            base.Notify(e, sender, args);
            if (e == GameNPCEvent.ArriveAtTarget && IsGettingHelp && m_targetList.Count > 0)
            {
                // We arrived at our target mob, let's have a look around
                // and see if we can get multiple adds.

				log.Info("Follow target reached, report enemies.");
                foreach (GameNPC npc in Body.GetNPCsInRadius(500))
					if (npc.IsFriend(Body) && npc.IsAggressive && npc.IsAvailable)
						ReportTargets(npc);

                // Once that's done, aggro on a target of our own and run back.

				ReportTargets(Body);
                m_targetList.Clear();
                IsGettingHelp = false;
                AttackMostWanted();
            }
            else if (e == GameNPCEvent.TakeDamage)
            {
                // If we are attacked at any point we'll stop scouting or
                // running for help.

                IsScouting = false;
                IsGettingHelp = false;
                m_targetList.Clear();
            }
        }
    }
}
