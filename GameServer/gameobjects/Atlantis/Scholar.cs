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
using DOL.GS.PacketHandler;
using DOL.Database;
using System.Collections;
using DOL.GS.Quests;
using log4net;
using System.Reflection;
using DOL.Events;
using DOL.GS.Quests.Atlantis;

namespace DOL.GS
{
    /// <summary>
    /// The scholars handing out the artifacts.
    /// </summary>
    /// <author>Aredhel</author>
    public class Scholar : Researcher
    {
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private enum TurnDownReason { NoCredit, NoBook, NoUse };
		private ArrayList m_artifacts;

		/// <summary>
		/// Create a new scholar.
		/// </summary>
        public Scholar()
            : base() { }

        /// <summary>
        /// Interact with scholar.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player)) 
                return false;

            String intro = String.Format("Which artifact may I assist you with, {0}? {1} ",
                player.CharacterClass.Name,
                "I study the lore and magic of the following artifacts:");

			if (m_artifacts == null)
				m_artifacts = ArtifactMgr.GetArtifactsFromScholar(Name);

			int numArtifacts = m_artifacts.Count;

			lock (m_artifacts.SyncRoot)
			{
				foreach (Artifact artifact in m_artifacts)
				{
					if (m_artifacts.Count > 1 && numArtifacts < m_artifacts.Count)
						intro += (numArtifacts == 1) ? ", or " : ", ";

					intro += String.Format("[{0}]", artifact.ArtifactID);
					--numArtifacts;
				}
			}

			intro += ".";

            SayTo(player, eChatLoc.CL_PopupWindow, intro);

            intro = String.Format("{0}, did you find any of the stories that chronicle the powers of the {1} {2} ",
                player.Name,
                "artifacts? We can unlock the powers of these artifacts by studying the stories.",
                "I can take the story and unlock the artifact's magic.");

            SayTo(player, eChatLoc.CL_PopupWindow, intro);
            return true;
        }

		/// <summary>
		/// Talk to the scholar.
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

			if (m_artifacts == null)
				m_artifacts = ArtifactMgr.GetArtifactsFromScholar(Name);

			// Talking about artifacts?

			Artifact subject = null;
			lock (m_artifacts.SyncRoot)
			{
				foreach (Artifact artifact in m_artifacts)
				{
					if (text.ToLower() == artifact.ArtifactID.ToLower())
					{
						subject = artifact;
						break;
					}
				}
			}

            if (subject != null)
            {
				GiveArtifactQuest(player, subject);
				return true;
            }

            // Talking about versions?

			return false;
		}

		/// <summary>
		/// Give the artifact quest to the player.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="artifact"></param>
		private void GiveArtifactQuest(GamePlayer player, Artifact artifact)
		{
			if (artifact == null)
				return;

			Type encounterType = ArtifactMgr.GetQuestType(artifact.EncounterID);
			Type questType = ArtifactMgr.GetQuestType(artifact.QuestID);

			if (questType == null)
			{
				log.Warn(String.Format("Can't find quest type for {0}", artifact.QuestID));
				return;
			}

			ArtifactQuest artifactQuest = (ArtifactQuest)Activator.CreateInstance(questType,
				new object[] { encounterType });

			if (artifactQuest.CheckQuestQualification(player))
			{
				Hashtable versions = null;
				if (!PlayerCanUseArtifact(player, artifact, ref versions))
				{
					artifactQuest.DeclineQuest(this);
					return;
				}

				artifactQuest.QuestPlayer = player;
				artifactQuest.Step = 1;
				artifactQuest.SaveIntoDatabase();
				artifactQuest.StartQuest(this);
				player.AddQuest(artifactQuest);
				artifactQuest.Notify(GamePlayerEvent.AcceptQuest, player, new EventArgs());
			}
			else
			{
                String reply = String.Format("{0} I cannot activate that artifact for you. {1} {2} {3} {4} {5}",
                    player.Name,
                    "This could be because you have already activated it, or you are in the",
                    "process of activating it, or you may not have completed everything",
                    "you need to do. Remember that the activation process requires you to",
                    "have credit for the artifact's encounter, as well as the artifact's",
                    "complete book of scrolls.");
				TurnTo(player);
				SayTo(player, eChatLoc.CL_PopupWindow, reply);
				return;
			}
		}

		/// <summary>
		/// Invoked when scholar receives an item.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public override bool ReceiveItem(GameLiving source, InventoryItem item)
		{
			GamePlayer player = source as GamePlayer;
			if (player != null)
			{
				Artifact artifact = ArtifactMgr.GetArtifactFromBook(item);
				if (artifact != null)
				{
					Type questType = ArtifactMgr.GetQuestType(artifact.QuestID);
					AbstractQuest quest;
					if (questType != null && (quest = player.IsDoingQuest(questType)) != null)
					{
						(quest as ArtifactQuest).EndQuest(this);
						player.Inventory.RemoveItem(item);
						player.RemoveEncounterCredit(ArtifactMgr.GetQuestType(artifact.EncounterID));
						return true;
					}
				}
			}

			return base.ReceiveItem(source, item);
		}

        /// <summary>
        /// Check if the player can actually use the artifact.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="artifact"></param>
        /// <param name="versions"></param>
        /// <returns></returns>
        private bool PlayerCanUseArtifact(GamePlayer player, Artifact artifact, ref Hashtable versions)
        {
            versions = ArtifactMgr.GetArtifactVersionsFromClass(artifact.ArtifactID,
                (eCharacterClass)player.CharacterClass.ID);

            return (versions.Count > 0);
        }
    }
}
