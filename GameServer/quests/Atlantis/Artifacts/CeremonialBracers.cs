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

using System.Collections.Generic;
using DOL.Database;
using DOL.GS.PacketHandler;
using log4net;
using System.Reflection;

namespace DOL.GS.Quests.Atlantis.Artifacts
{
    /// <summary>
    /// Quest for the Ceremonial Bracers artifact.
    /// </summary>
    /// <author>Aredhel</author>
    public class CeremonialBracers : ArtifactQuest
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public CeremonialBracers()
        { }

        public CeremonialBracers(GamePlayer questingPlayer)
            : base(questingPlayer) { }

        /// <summary>
        /// This constructor is needed to load quests from the DB.
        /// </summary>
        /// <param name="questingPlayer"></param>
        /// <param name="dbQuest"></param>
        public CeremonialBracers(GamePlayer questingPlayer, DBQuest dbQuest)
            : base(questingPlayer, dbQuest) { }

        private static string m_artifactID = "Ceremonial Bracers";

        /// <summary>
        /// Quest initialisation.
        /// </summary>
        public static void Init()
        {
            Init(m_artifactID, typeof(CeremonialBracers));
        }

        /// <summary>
        /// Check if player is eligible for this quest.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool CheckQuestQualification(GamePlayer player)
        {
            if (!base.CheckQuestQualification(player))
            {
                return false;
            }

            // TODO: Check if this is the correct level for the quest.
            return player.Level >= 45;
        }

        /// <summary>
        /// Handle an item given to the scholar.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="item"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public override bool ReceiveItem(GameLiving source, GameLiving target, InventoryItem item)
        {
            if (base.ReceiveItem(source, target, item))
            {
                return true;
            }

            if (!(source is GamePlayer player) || !(target is Scholar scholar))
            {
                return false;
            }

            if (Step == 2 && ArtifactMgr.GetArtifactID(item.Name) == ArtifactId)
            {
                scholar.TurnTo(player);
                if (RemoveItem(player, item))
                {
                    string reply ="Ahh. These notes are well-preserved. Here, these bracers are unlike any other artifact. I can unlock one power, [strength], [constitution], [dexterity], [quickness] or [casting]. You can only choose one, and can not return for another. Choose wisely.";
                    scholar.SayTo(player, eChatLoc.CL_PopupWindow, reply);
                    Step = 3;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Handle whispers to the scholar.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public override bool WhisperReceive(GameLiving source, GameLiving target, string text)
        {
            if (base.WhisperReceive(source, target, text))
            {
                return true;
            }

            if (!(source is GamePlayer player) || !(target is Scholar scholar))
            {
                return false;
            }

            if (Step == 1 && text.ToLower() == ArtifactId.ToLower())
            {
                string reply = "The scholars have often remarked about the craftsmanship that went into these bracers. I\'m impressed. I don\'t think we can make anything to compare with it. Hmm. Well, I\'m here to study the lifestyles of Atlanteans through their written words, not their crafts. Hand me the Arbiter\'s Personal Papers please. If you don\'t have it, I suggest you hunt the creatures of Oceanus til you find it.";
                scholar.TurnTo(player);
                scholar.SayTo(player, eChatLoc.CL_PopupWindow, reply);
                Step = 2;
                return true;
            }
            else if (Step == 3)
            {
                switch (text.ToLower())
                {
                    case "strength":
                    case "constitution":
                    case "dexterity":
                    case "quickness":
                    case "casting":
                        {
                            string versionId = $";;{text.ToLower()}";
                            Dictionary<string, ItemTemplate> versions = ArtifactMgr.GetArtifactVersions(ArtifactId, (eCharacterClass)player.CharacterClass.ID, player.Realm);
                            ItemTemplate template = versions[versionId];
                            if (template == null)
                            {
                                Log.Warn($"Artifact version {versionId} not found");
                                return false;
                            }

                            if (GiveItem(scholar, player, ArtifactId, template))
                            {
                                string reply = "You have made your choice. Here is your bracer. Do not lose it. It is irreplaceable.";
                                scholar.TurnTo(player);
                                scholar.SayTo(player, eChatLoc.CL_PopupWindow, reply);
                                FinishQuest();
                                return true;
                            }

                            return false;
                        }
                }

                return false;
            }

            return false;
        }

        /// <summary>
        /// Description for the current step.
        /// </summary>
        public override string Description
        {
            get
            {
                switch (Step)
                {
                    case 1:
                        return "Defeat a wavelord.";
                    case 2:
                        return "Turn in the completed book.";
                    case 3: // TODO: Get correct journal entry.
                        return "Choose between a [strength], [constitution], [dexterity], [quickness] or [casting] version of the Ceremonial Bracers.";
                    default:
                        return base.Description;
                }
            }
        }

        /// <summary>
        /// The name of the quest (not necessarily the same as
        /// the name of the reward).
        /// </summary>
        public override string Name => "Bracers";

        /// <summary>
        /// The artifact ID.
        /// </summary>
        public override string ArtifactId => m_artifactID;
    }
}
