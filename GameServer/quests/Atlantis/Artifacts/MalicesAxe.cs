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
    /// Quest for the Malice's Axe artifact.
    /// </summary>
    /// <author>Aredhel</author>
    public class MalicesAxe : ArtifactQuest
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MalicesAxe()
        { }

        public MalicesAxe(GamePlayer questingPlayer)
            : base(questingPlayer) { }

        /// <summary>
        /// This constructor is needed to load quests from the DB.
        /// </summary>
        /// <param name="questingPlayer"></param>
        /// <param name="dbQuest"></param>
        public MalicesAxe(GamePlayer questingPlayer, DBQuest dbQuest)
            : base(questingPlayer, dbQuest) { }

        private static string m_artifactID = "Malice's Axe";

        /// <summary>
        /// Quest initialisation.
        /// </summary>
        public static void Init()
        {
            Init(m_artifactID, typeof(MalicesAxe));
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
                    string reply = $"You now have a decision to make, {player.CharacterClass.Name}. I can unlock your Malice Axe so it uses [slashing] skills or so it uses [crushing] skills. In both cases, I can unlock it as a one-handed weapon or a two-handed one. All you must do is decide which kind of damage you would like to do to your enemies. Once you have chosen, you cannot change your mind.";
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
                string reply = "Ah, yes, the axe of Malice. It has an interesting tale, but I'm not sure I believe it. Did you find the story of the axe? If you have, please give it to me now.";
                scholar.TurnTo(player);
                scholar.SayTo(player, eChatLoc.CL_PopupWindow, reply);
                Step = 2;
                return true;
            }
            else if (Step == 3)
            {
                switch (text.ToLower())
                {
                    case "slashing":
                    case "crushing":
                        {
                            SetCustomProperty("DamageType", text.ToLower());
                            string reply = $"Would you like your {text.ToLower()} Malice\'s Axe to be [one handed] or [two handed]?";
                            scholar.TurnTo(player);
                            scholar.SayTo(player, eChatLoc.CL_PopupWindow, reply);
                            Step = 4;
                            return true;
                        }
                }

                return false;
            }
            else if (Step == 4)
            {
                switch (text.ToLower())
                {
                    case "one handed":
                    case "two handed":
                        {
                            string versionId = $"{GetCustomProperty("DamageType")};{text.ToLower()};";
                            Dictionary<string, ItemTemplate> versions = ArtifactMgr.GetArtifactVersions(ArtifactId, (eCharacterClass)player.CharacterClass.ID, player.Realm);
                            ItemTemplate template = versions[versionId];
                            if (template == null)
                            {
                                Log.Warn($"Artifact version {versionId} not found");
                                return false;
                            }

                            if (GiveItem(scholar, player, ArtifactId, template))
                            {
                                string reply = $"Here\'s your {template.Name}. May it serve you well. Just don\'t lose it. You can\'t ever replace it.";
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
                        return "Defeat Malamis.";
                    case 2:
                        return "Turn in the completed book.";
                    case 3:
                        return "Choose between a [slashing] version of Malice's Axe or a [crushing] one. Both one-handed and two-handed versions are available for both.";
                    case 4:
                        return "Choose between one handed or two handed versions.";
                    default:
                        return base.Description;
                }
            }
        }

        /// <summary>
        /// The name of the quest (not necessarily the same as
        /// the name of the reward).
        /// </summary>
        public override string Name => "Malice's Axe";

        /// <summary>
        /// The artifact ID.
        /// </summary>
        public override string ArtifactId => m_artifactID;
    }
}
