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
    public abstract class AncientBoundDjinn : GameTeleporter
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const int NpcTemplateId = 3000;      
        private const int ZOffset = 63;

        /// <summary>
        /// Creates a new djinn.
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
            Flags ^= (uint)GameNPC.eFlags.FLYING | (uint)GameNPC.eFlags.PEACE;
            X = djinnStone.X;
            Y = djinnStone.Y;
            Z = djinnStone.Z + HoverHeight;
            base.Size = Size;
        }

        /// <summary>
        /// Gets the actual size.
        /// </summary>
        virtual protected new byte Size
        {
            get { return base.Size; }
        }

        /// <summary>
        /// Gets the height at which the djinn is hovering.
        /// </summary>
        virtual protected int HoverHeight
        {
            get { return 63; }
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
				return CurrentZone.GetRealm();
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
                    
                    // Temple of Twilight:
                    case 80:
                    case 37:
                    case 137:
                        return 0x4ad;

                    default:
                        return 0x4aa;
                }
            }
        }

        /// <summary>
        /// Summon the djinn.
        /// </summary>
        public virtual void Summon()
        {
        }

        /// <summary>
        /// Whether or not the djinn is summoned.
        /// </summary>
        public virtual bool IsSummoned
        {
            get { return false; }
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

            String intro = String.Format("According to the rules set down by the Atlantean [masters], {0} {1} ",
                "you are authorized for expeditious transport to your homeland or any of the Havens. Please state",
                "your destination:");

            String destinations;

            switch (player.Realm)
            {
                case eRealm.Albion:
                    destinations = String.Format("[Castle Sauvage], [Oceanus], [Stygia], [Volcanus], [Aerus], the [dungeons of Atlantis], {0} {1}",
                        "[Snowdonia Fortress], [Camelot], [Gothwaite Harbor], [Inconnu Crypt], your [Guild] house, your",
                        "[Personal] house, your [Hearth] bind, or to the [Caerwent] housing area?");
                    break;
                case eRealm.Midgard:
                    destinations = String.Format("[Svasud Faste], [Oceanus], [Stygia], [Volcanus], [Aerus], the [dungeons of Atlantis], {0} {1}",
                        "[Vindsaul Faste], [Jordheim], [Aegirhamn], [Kobold] Undercity, your [Guild] house, your",
                        "[Personal] house, your [Hearth] bind, or to the [Erikstaad] housing area?");
                    break;
                case eRealm.Hibernia:
                    destinations = String.Format("[Druim Ligen], [Oceanus], [Stygia], [Volcanus], [Aerus], the [dungeons of Atlantis], {0} {1}",
                        "[Druim Cain], the [Grove of Domnann], [Tir na Nog], [Shar Labyrinth], your [Guild] house, your",
                        "[Personal] house, your [Hearth] bind, or to the [Meath] housing area?");
                    break;
                default:
                    SayTo(player, "I don't know you, which realm are you from?");
                    return true;
            }

            SayTo(player, String.Format("{0}{1}", intro, destinations));
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


		protected override bool GetTeleportLocation(GamePlayer player, string text)
		{
			// special cases
			if (text.ToLower() == "battlegrounds" || text.ToLower() == "personal")
			{
				return base.GetTeleportLocation(player, text);
			}

			// Find the teleport location in the database.  For Djinns use the player realm to match Interact list given.
			Teleport port = WorldMgr.GetTeleportLocation(player.Realm, String.Format("{0}:{1}", Type, text));
			if (port != null)
			{
				if (port.RegionID == 0 && port.X == 0 && port.Y == 0 && port.Z == 0)
				{
					OnSubSelectionPicked(player, port);
				}
				else
				{
					OnDestinationPicked(player, port);
				}
				return false;
			}

			return true;	// Needs further processing.
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

        /// <summary>
        /// Player has picked a teleport destination.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="destination"></param>
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
            player.Out.SendMessage("There is an odd distortion in the air around you...", eChatType.CT_System, eChatLoc.CL_SystemWindow);

			// if player is porting in atlantis to another atlantis location then keep in the same region
			// this allows us to support cooperative atlantis zones
			if ((player.CurrentRegionID == 30 || player.CurrentRegionID == 73 || player.CurrentRegionID == 130) &&
				(destination.RegionID == 30 || destination.RegionID == 73 || destination.RegionID == 130))
			{
				destination.RegionID = player.CurrentRegionID;
			}

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
			{
				player.Out.SendMessage(String.Format("The {0} says, \"{1}\"", this.Name, message), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}

            return true;
        }
    }
}
