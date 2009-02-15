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
using log4net;
using System.Reflection;
using DOL.Events;
using DOL.Database;

namespace DOL.GS
{
    /// <summary>
    /// Ancient bound djinn (Atlantis teleporter).
    /// </summary>
    /// <author>Aredhel</author>
    public class AncientBoundDjinn : GameTeleporter
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const int NpcTemplateId = 3000;
        private const int InvisibleModelId = 0x29a;
        private const int VisibleModelId = 0x4aa;
        private const int SummonSpellEffect = 0x1818;
        private const int ZOffset = 63;
        private DjinnTimer m_summonTimer;
        private object m_syncObject = new object();

        /// <summary>
        /// Creates a new (invisible) djinn.
        /// </summary>
        public AncientBoundDjinn(DjinnStone djinnStone) : base()
        {
            NpcTemplate npcTemplate = NpcTemplateMgr.GetTemplate(NpcTemplateId);

            if (npcTemplate == null)
                throw new ArgumentNullException("Can't find NPC template for ancient bound djinn");

            LoadTemplate(npcTemplate);

            CurrentRegion = djinnStone.CurrentRegion;
            Heading = (ushort)((djinnStone.Heading + 2048) % 4096);
            Realm = eRealm.None;
            X = djinnStone.X;
            Y = djinnStone.Y;
            Z = djinnStone.Z + ZOffset;
        }

        /// <summary>
        /// Teleporter type, needed to pick the right TeleportID.
        /// </summary>
        public override String Type
        {
            get { return "Djinn"; }
        }

        /// <summary>
        /// Starts the summon.
        /// </summary>
        public void Summon()
        {
            lock (m_syncObject)
            {
                if (CurrentRegion == null || m_summoned)
                    return;

                if (m_summonTimer == null)
                    m_summonTimer = new DjinnTimer(this);

                m_summoned = true;
                m_summonTimer.Start(7, 1000, true, SummonEvent.SummonCompleted);
            }
        }

        private bool m_summoned = false;

        /// <summary>
        /// Returns true if djinn has been summoned, else false.
        /// </summary>
        public bool IsSummoned
        {
            get
            {
                lock (m_syncObject)
                    return m_summoned;
            }

            private set
            {
                lock (m_syncObject)
                    m_summoned = value;
            }
        }

        /// <summary>
        /// Player right-clicked the djinn.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player))
                return false;

            String intro = String.Format("According to the rules set down by the Atlantean [masters], {0} {1} {2} {3}",
                "you are authorized for expeditious transport to your homeland or any of the Havens. Please state",
                "your destination: [Castle Sauvage], [Oceanus], [Stygia], [Volcanus], [Aerus], the [dungeons of Atlantis],",
                "[Snowdonia Fortress], [Camelot], [Gothwaite Harbor], [Inconnu Crypt], your [Guild] house, your",
                "[Personal] house, your [Hearth] bind, or to the [Caerwent] housing area?");
            SayTo(player, intro);
            return true;
        }

        /// <summary>
		/// Talk to the djinn.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="text"></param>
		/// <returns></returns>
        public override bool WhisperReceive(GameLiving source, string text)
        {
            if (!base.WhisperReceive(source, text))
                return false;

            GamePlayer player = source as GamePlayer;
            if (player == null)
                return false;

            switch (text.ToLower())
            {
                case "masters":
                    String reply = String.Format("The Atlantean masters are a great and powerful people to whom [we] are bound.");
                    SayTo(player, reply);
                    return true;
                case "we":
                    return true;    // No reply on live.
            }

            return true;
        }

        /// <summary>
        /// Player has picked a subselection.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="subSelection"></param>
        protected override void OnSubSelectionPicked(GamePlayer player, Teleport subSelection)
        {
            switch (subSelection.TeleportID.ToLower())
            {
                case "oceanus":
                    {
                        String reply = String.Format("I can transport you to the Haven of Oceanus in {0} {1}",
                            "Oceanus [Hesperos], the mouth of [Cetus' Pit], or the heights of the great",
                            "[Temple] of Sobekite Eternal.");
                        SayTo(player, reply);
                        return;
                    }
                case "stygia":
                    {
                        String reply = String.Format("Do you seek the sandy Haven of Stygia in the Stygian {0}",
                            "[Delta] or the distant [Land of Atum]?");
                        SayTo(player, reply);
                        return;
                    }
                case "volcanus":
                    {
                        String reply = String.Format("Do you wish to approach [Typhon's Reach] from the Haven {0} {1}",
                            "of Volcanus or do you perhaps have more ambitious plans, such as attacking",
                            "[Thusia Nesos], the Temple of [Apollo], [Vazul's Fortress], or the [Chimera] herself?");
                        SayTo(player, reply);
                        return;
                    }
                case "aerus":
                    {
                        String reply = String.Format("Do you seek the Haven of Aerus outside [Green Glades] or {0}",
                            "perhaps the Temple of [Talos]?");
                        SayTo(player, reply);
                        return;
                    }
                case "dungeons of atlantis":
                    {
                        String reply = String.Format("I can provide access to [Sobekite Eternal], the {0} {1}",
                            "Temple of [Twilight], the [Great Pyramid], the [Halls of Ma'ati], [Deep] within",
                            "Volcanus, or even the [City] of Aerus.");
                        SayTo(player, reply);
                        return;
                    }
            }
            base.OnSubSelectionPicked(player, subSelection);
        }

        /// <summary>
        /// Processes events coming from the timer.
        /// </summary>
        /// <param name="e"></param>
        public override void Notify(DOLEvent e)
        {
            base.Notify(e);

            if (e == SummonEvent.SummonStarted)
                return;  // No action yet.
            else if (e == SummonEvent.SummonCompleted)
            {
                // Show ourselves.

                this.Model = VisibleModelId;

                foreach (GamePlayer player in this.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                    player.Out.SendModelChange(this, this.Model);

                Say("Greetings, great one.");
                m_summonTimer.Start(150, 1000, false, SummonEvent.BanishStarted);   // 2.5mins to hiding again.
            }
            else if (e == SummonEvent.BanishStarted)
            {
                // Go into hiding and show the smoke again.

                Say("My time here is done.");
                this.Model = InvisibleModelId;

                foreach (GamePlayer player in this.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                    player.Out.SendModelChange(this, this.Model);

                m_summonTimer.Start(5, 1000, true, SummonEvent.BanishCompleted);
            }
            else if (e == SummonEvent.BanishCompleted)
                IsSummoned = false;
        }

        /// <summary>
        /// Provides a timer for a djinn teleporter.
        /// </summary>
        private class DjinnTimer : GameTimer
        {
            private GameObject m_owner;
            private int m_maxTicks = 0;
            private int m_ticks = 0;
            private bool m_smoke = false;
            private SummonEvent m_event;

            /// <summary>
            /// Constructs a new SummonTimer.
            /// </summary>
            /// <param name="timerOwner">The owner of this timer (the djinn).</param>
            public DjinnTimer(GameObject owner)
                : base(owner.CurrentRegion.TimeManager)
            {
                m_owner = owner;
            }

            private bool m_isRunning = false;

            /// <summary>
            /// Whether the timer is running or not.
            /// </summary>
            public bool IsRunning
            {
                get { return m_isRunning; }
                private set { m_isRunning = value; }
            }

            /// <summary>
            /// Restarts the timer.
            /// </summary>
            public void Restart()
            {
                if (IsRunning)
                {
                    this.Stop();
                    m_ticks = 0;
                    this.Start(100);
                }
            }

            /// <summary>
            /// Starts the timer
            /// </summary>
            /// <param name="maxTicks">Number of ticks before the timer stops.</param>
            /// <param name="interval">Time between individual ticks.</param>
            /// <param name="smoke">Whether to show smoke spell effect on each tick or not.</param>
            /// <param name="e">The event to send when timer is stopped.</param>
            public void Start(int maxTicks, int interval, bool smoke, SummonEvent e)
            {
                m_maxTicks = maxTicks;
                Interval = interval;
                m_smoke = smoke;
                m_event = e;
                m_ticks = 0;
                this.Start(100);
                IsRunning = true;
            }

            /// <summary>
            /// Called on every timer tick.
            /// </summary>
            protected override void OnTick()
            {
                m_ticks++;

                if (m_ticks < m_maxTicks)
                {
                    if (m_smoke)
                    {
                        // Send smoke animation to players in visibility range.

                        foreach (GamePlayer player in m_owner.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                            player.Out.SendSpellEffectAnimation(m_owner, m_owner, SummonSpellEffect, 0, false, 0x01);
                    }
                }
                else
                {
                    // We're done, stop the timer and notify owner.

                    this.Stop();
                    IsRunning = false;
                    m_owner.Notify(m_event);
                }
            }
        }
    }
}
