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
    /// Quest for the Battler artifact.
    /// </summary>
    /// <author>Aredhel</author>
    public class Battler : ArtifactQuest
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public Battler()
        { }

        public Battler(GamePlayer questingPlayer)
            : base(questingPlayer) { }

        /// <summary>
        /// This constructor is needed to load quests from the DB.
        /// </summary>
        /// <param name="questingPlayer"></param>
        /// <param name="dbQuest"></param>
        public Battler(GamePlayer questingPlayer, DBQuest dbQuest)
            : base(questingPlayer, dbQuest) { }

        private static string m_artifactID = "Battler";

        /// <summary>
        /// Quest initialisation.
        /// </summary>
        public static void Init()
        {
            Init(m_artifactID, typeof(Battler));
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
                    string reply = $"You now have a decision to make, {player.CharacterClass.Name}. I can unlock your Battler so it uses [slashing] skills or so it uses [crushing] skills. In both cases, I can unlock it as a one-handed weapon or a two-handed one. All you must do is decide which kind of damage you would like to do to your enemies. Once you have chosen, you cannot change your mind.";
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
                string reply = "Battler, eh? Careful, when I unlock this sword's abilities, for it has a thirst for blood, from what we've learned of it. Do you have the scrolls that talk about Battler?";
                scholar.TurnTo(player);
                scholar.SayTo(player, eChatLoc.CL_PopupWindow, reply);
                Step = 2;
                return true;
            }

            if (Step == 3)
            {
                switch (text.ToLower())
                {
                    case "slashing":
                    case "crushing":
                    {
                        SetCustomProperty("DamageType", text.ToLower());
                        string reply = $"Now, would you like your Battler{(text.ToLower() == "slashing" ? string.Empty : " Mace")} to be [one handed] or [two handed]?";
                        scholar.TurnTo(player);
                        scholar.SayTo(player, eChatLoc.CL_PopupWindow, reply);
                        Step = 4;
                        return true;
                    }
                }

                return false;
            }

            if (Step == 4)
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
                            string reply = $"Here you are. Do not lose {template.Name}, for it is irreplaceable.";
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
            // TODO: Get correct journal entries!
            get
            {
                switch (Step)
                {
                    case 1:
                        return "Defeat Battler.";
                    case 2:
                        return "Turn in the completed book.";
                    case 3:
                        return "Choose between a [slashing] version of Battler or a [crushing] one. Both one-handed and two-handed versions are available for both.";
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
        public override string Name => "Battler";

        /// <summary>
        /// The artifact ID.
        /// </summary>
        public override string ArtifactId => m_artifactID;
    }
}
