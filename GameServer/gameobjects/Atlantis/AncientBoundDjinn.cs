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
using DOL.GS.PacketHandler;
using DOL.GS.Housing;

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
        private const int InvisibleModel = 0x29a;
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
            Heading = djinnStone.Heading;
            Realm = eRealm.None;
            X = djinnStone.X;
            Y = djinnStone.Y;
            Z = djinnStone.Z + ZOffset;
        }

        /// <summary>
        /// Teleporter type, needed to pick the right TeleportID.
        /// </summary>
        protected override String Type
        {
            get { return "Djinn"; }
        }

        /// <summary>
        /// The destination realm.
        /// </summary>
        protected override eRealm DestinationRealm
        {
            get
            {
                switch (CurrentRegion.ID)
                {
                    case 73:
                        return eRealm.Albion;
                    case 30:
                        return eRealm.Midgard;
                    case 130:
                        return eRealm.Hibernia;
                    default:
                        return eRealm.None;
                }
            }
        }

        /// <summary>
        /// Pick a model for this zone.
        /// </summary>
        protected ushort VisibleModel
        {
            get
            {
                switch (CurrentZone.ID)
                {
                    // Oceanus Hesperos.

                    case 73:            // Albion.
                    case 30:            // Midgard.
                    case 130:           // Hibernia.
                        return 0x4aa;

                    // Stygian Delta.

                    case 81:
                    case 38:
                    case 138:
                        return 0x4ac;

                    // Oceanus Notos.

                    case 76:
                    case 33:
                    case 133:
                        return 0x4ae;

                    // Oceanus Anatole:

                    case 77:
                    case 34:
                    case 134:
                        return 0x4aa;

                    default:
                        return 0x4aa;
                }
            }
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
                m_summonTimer.Start(1, 100, true, SummonEvent.SummonStarted);
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

            lock (m_syncObject)
                m_summonTimer.Restart();

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
        public override bool WhisperReceive(GameLiving source, String text)
        {
            if (!(source is GamePlayer))
                return false;

            lock (m_syncObject)
                m_summonTimer.Restart();

            GamePlayer player = source as GamePlayer;

            // Manage the chit-chat.

            switch (text.ToLower())
            {
                case "masters":
                    String reply = String.Format("The Atlantean masters are a great and powerful people to whom [we] are bound.");
                    SayTo(player, reply);
                    return true;
                case "we":
                    return true;    // No reply on live.
            }

            return base.WhisperReceive(source, text);
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
                case "twilight":
                    {
                        String reply = String.Format("Do you seek an audience with one of the great ladies {0} {1}",
                            "of that dark temple? I'm sure that [Moirai], [Kepa], [Casta], [Laodameia], [Antioos],",
                            "[Sinovia], or even [Medusa] would love to have you over for dinner.");
                        SayTo(player, reply);
                        return;
                    }
                case "halls of ma'ati":
                    {
                        String reply = String.Format("Which interests you, the [entrance], the [Anubite] side, {0} {1}",
                            "or the [An-Uat] side? Or are you already ready to face your final fate in the",
                            "[Chamber of Ammut]?");
                        SayTo(player, reply);
                        return;
                    }
                case "deep":
                    {
                        String reply = String.Format("Do you wish to meet with the Mediators of the [southwest] {0} {1}",
                            "or [northeast] hall, face [Katorii's] gaze, or are you foolish enough to battle the",
                            "likes of [Typhon himself]?");
                        SayTo(player, reply);
                        return;
                    }
                case "city":
                    {
                        String reply = String.Format("I can send you to the entrance near the [portal], the great {0} {1} {2}",
                            "Unifier, [Lethos], the [ancient kings] remembered now only for their reputations, the",
                            "famous teacher, [Nelos], the most well known and honored avriel, [Katri], or even",
                            "the [Phoenix] itself.");
                        SayTo(player, reply);
                        return;
                    }

            }
            base.OnSubSelectionPicked(player, subSelection);
        }

        protected override void OnDestinationPicked(GamePlayer player, Teleport destination)
        {
            if (player == null)
                return;

            String teleportInfo = "The magic of the {0} delivers you to the Haven of {1}.";

            switch (destination.TeleportID.ToLower())
            {
                case "hesperos":
                    {
                        player.Out.SendMessage(String.Format(teleportInfo, Name, "Oceanus"),
                            eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        base.OnTeleport(player, destination);
                        return;
                    }
                case "delta":
                    {
                        player.Out.SendMessage(String.Format(teleportInfo, Name, "Stygia"),
                            eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        base.OnTeleport(player, destination);
                        return;
                    }
                case "green glades":
                    {
                        player.Out.SendMessage(String.Format(teleportInfo, Name, "Aerus"),
                            eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        base.OnTeleport(player, destination);
                        return;
                    }
            }

            base.OnDestinationPicked(player, destination);
        }

        /// <summary>
        /// Teleport the player to the designated coordinates. 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="destination"></param>
        protected override void OnTeleport(GamePlayer player, Teleport destination)
        {
            player.Out.SendMessage("There is an odd distortion in the air around you...",
                eChatType.CT_System, eChatLoc.CL_SystemWindow);

            base.OnTeleport(player, destination);
        }

        /// <summary>
        /// "Say" content sent to the system window.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool Say(String message)
        {
            foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.SAY_DISTANCE))
                player.Out.SendMessage(String.Format("The {0} says, \"{1}\"", this.Name, message), 
                    eChatType.CT_System, eChatLoc.CL_SystemWindow);

            return true;
        }

        /// <summary>
        /// Processes events coming from the timer.
        /// </summary>
        /// <param name="e"></param>
        public override void Notify(DOLEvent e)
        {
            base.Notify(e);

            if (e == SummonEvent.SummonStarted)
            {
                lock (m_syncObject)
                {
                    this.AddToWorld();
                    m_summonTimer.Start(7, 1000, true, SummonEvent.SummonCompleted);
                }
            }
            else if (e == SummonEvent.SummonCompleted)
            {
                // Show ourselves.

                lock (m_syncObject)
                {
                    this.Model = VisibleModel;

                    foreach (GamePlayer player in this.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                        player.Out.SendModelChange(this, this.Model);

                    Say("Greetings, great one.");
                    m_summonTimer.Start(150, 1000, false, SummonEvent.BanishStarted);   // 2.5mins to hiding again.
                }
            }
            else if (e == SummonEvent.BanishStarted)
            {
                // Go into hiding and show the smoke again.

                lock (m_syncObject)
                {
                    Say("My time here is done.");
                    this.Model = InvisibleModel;

                    foreach (GamePlayer player in this.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                        player.Out.SendModelChange(this, this.Model);

                    m_summonTimer.Start(5, 1000, true, SummonEvent.BanishCompleted);
                }
            }
            else if (e == SummonEvent.BanishCompleted)
            {
                lock (m_syncObject)
                {
                    this.RemoveFromWorld();
                    m_summoned = false;
                }
            }
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
