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
using DOL.Database;
using DOL.Language;
using DOL.GS.PacketHandler;
using log4net;
using System.Reflection;

namespace DOL.GS.Quests.Atlantis
{
    /// <summary>
    /// Base class for all artifact quests.
    /// </summary>
    /// <author>Aredhel</author>
    public class ArtifactQuest : BaseQuest
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public string ReasonFailQualification { get; private set; } = string.Empty;

        public ArtifactQuest()
        { }

        public ArtifactQuest(GamePlayer questingPlayer)
            : base(questingPlayer) { }

        /// <summary>
        /// This constructor is needed to load quests from the DB.
        /// </summary>
        /// <param name="questingPlayer"></param>
        /// <param name="dbQuest"></param>
        public ArtifactQuest(GamePlayer questingPlayer, DBQuest dbQuest)
            : base(questingPlayer, dbQuest) { }

        /// <summary>
        /// Quest initialisation.
        /// </summary>
        public static void Init(string artifactId, Type questType)
        {
            if (artifactId == null || questType == null)
            {
                return;
            }

            string[] scholars = ArtifactMgr.GetScholars(artifactId);
            if (scholars != null)
            {
                eRealm realm = eRealm.Albion;

                foreach (string scholar in scholars)
                {
                    string title;

                    GameNPC[] npcs;
                    switch (realm)
                    {
                        case eRealm.Albion:
                            title = "Scholar";
                            npcs = WorldMgr.GetObjectsByName<GameNPC>($"{title} {scholar}", realm);
                            break;
                        case eRealm.Midgard:
                            title = "Loremaster";
                            npcs = WorldMgr.GetObjectsByName<GameNPC>($"{title} {scholar}", realm);

                            if (npcs.Length == 0)
                            {
                                title = "Loremistress";
                                npcs = WorldMgr.GetObjectsByName<GameNPC>($"{title} {scholar}", realm);
                            }

                            break;
                        case eRealm.Hibernia:
                            title = "Sage";
                            npcs = WorldMgr.GetObjectsByName<GameNPC>($"{title} {scholar}", realm);
                            break;
                        default:
                            title = "<unknown title>";
                            npcs = new GameNPC[0];
                            break;
                    }

                    if (npcs.Length == 0)
                    {
                        Log.Warn($"ARTIFACTQUEST: {title} {scholar} not found in {GlobalConstants.RealmToName(realm)} for artifact {artifactId}");
                    }
                    else
                    {
                        npcs[0].AddQuestToGive(questType);
                    }

                    ++realm;
                }
            }
            else
            {
                Log.Warn($"ARTIFACTQUEST: scholars is null for artifact {artifactId}");
            }
        }

        /// <summary>
        /// The artifact ID.
        /// </summary>
        public virtual string ArtifactId => "UNDEFINED";

        /// <summary>
        /// Check if player is eligible for this quest.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool CheckQuestQualification(GamePlayer player)
        {
            // Must have the encounter, must have the book; must not be on the quest
            // and must not have the quest finished either.
            Type encounterType = ArtifactMgr.GetEncounterType(ArtifactId);

            if (encounterType == null)
            {
                ReasonFailQualification = "It does not appear this encounter type is set up correctly.";
                Log.Warn($"ArtifactQuest: EncounterType is null for ArtifactID {ArtifactId}");
                return false;
            }

            if (player == null)
            {
                ReasonFailQualification = "Player is null for quest, serious error!";
                Log.Warn($"ArtifactQuest: Player is null for ArtifactID {ArtifactId} encounterType {encounterType.FullName}");
                return false;
            }

            if (player.CanReceiveArtifact(ArtifactId) == false)
            {
                ReasonFailQualification = "Your class is not eligible for this artifact.";
                return false;
            }

            if (player.Level < Level)
            {
                ReasonFailQualification = $"You must be at least level {Level} in order to complete this quest.";
                return false;
            }

            if (player.HasFinishedQuest(GetType()) != 0)
            {
                ReasonFailQualification = "You've already completed the quest for this artifact.";
                return false;
            }

            if (player.HasFinishedQuest(encounterType) <= 0)
            {
                ReasonFailQualification = "You must first get the encounter credit for this artifact.";
                return false;
            }

            if (!ArtifactMgr.HasBook(player, ArtifactId))
            {
                ReasonFailQualification = "You are missing the correct book for this artifact.";
                return false;
            }

            if (player.IsDoingQuest(GetType()) != null)
            {
                ReasonFailQualification = "You've already started the quest for this artifact.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if player can use the reward.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public virtual bool CheckRewardQualification(GamePlayer player)
        {
            if (player == null)
            {
                return false;
            }

            Dictionary<string, ItemTemplate> versions = ArtifactMgr.GetArtifactVersions(ArtifactId, (eCharacterClass)player.CharacterClass.ID, player.Realm);

            return versions.Count > 0;
        }

        /// <summary>
        /// Minimum level requirement, adjust this for lowbie artifacts.
        /// </summary>
        public override int Level => 45;

        /// <summary>
        /// Hand out an artifact.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="player"></param>
        /// <param name="artifactId"></param>
        /// <param name="itemTemplate"></param>
        protected static bool GiveItem(GameLiving source, GamePlayer player, string artifactId, ItemTemplate itemTemplate)
        {
            InventoryItem item = new InventoryArtifact(itemTemplate);
            if (!player.ReceiveItem(source, item))
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "ArtifactQuest.GiveItem.BackpackFull"), eChatType.CT_Important, eChatLoc.CL_PopupWindow);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Handle an item given to the scholar.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="item"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual bool ReceiveItem(GameLiving source, GameLiving target, InventoryItem item)
        {
            return false;
        }

        /// <summary>
        /// Handle whispers to the scholar.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="text"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual bool WhisperReceive(GameLiving source, GameLiving target, string text)
        {
            return false;
        }

        /// <summary>
        /// Handle interaction with the scholar.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="text"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual bool Interact(Scholar scholar, GamePlayer player)
        {
            return false;
        }

        /// <summary>
        /// Remove an item from the player's inventory.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual bool RemoveItem(GamePlayer player, InventoryItem item)
        {
            lock (player.Inventory)
            {
                if (player.Inventory.RemoveItem(item))
                {
                    InventoryLogging.LogInventoryAction(player, $"(ARTIFACT;{Name})", eInventoryActionType.Quest, item.Template, item.Count);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// When finishing an artifact quest remove the encounter
        /// credit.
        /// </summary>
        public override void FinishQuest()
        {
            base.FinishQuest();

            Type encounterType = ArtifactMgr.GetEncounterType(ArtifactId);

            QuestPlayer.RemoveEncounterCredit(encounterType);
        }
    }
}
