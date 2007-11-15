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

            String intro = String.Format("Which artifact may I assist you with, {0}? ",
                player.CharacterClass.Name);
            intro += "I study the lore and magic of the following artifacts: ";

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

            intro = String.Format("{0}, did you find any of the stories that chronicle the powers of the artifacts? ",
                player.Name);
            intro += "We can unlock the powers of these artifacts by studying the stories. ";
            intro += "I can take the story and unlock the artifact's magic.";

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
                HandOutArtifact(player, subject);
                return true;
            }

            // Talking about versions?

			return false;
		}

		/// <summary>
		/// If player has the book in his backpack and the quest completed
		/// in his log, hand out the artifact, else turn him down.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="artifact"></param>
		private void HandOutArtifact(GamePlayer player, Artifact artifact)
		{
            InventoryItem book = null;
            AbstractQuest credit= null;

            // No credit/already activated/no book.

            if (!PlayerHasQuest(player, artifact, ref credit) ||
                credit.GetCustomProperty("artifact") == "true" ||
                (credit.GetCustomProperty("book") != "true" && !PlayerHasBook(player, artifact, ref book)))
            {
                String reply = String.Format("{0}, I cannot activate that artifact for you. ", player.Name);
                reply += "This could be because you have already activated it, or you are in the ";
                reply += "process of activating it, or you may not have completed everything ";
                reply += "you need to do. Remember that the activation process requires you to ";
                reply += "have credit for the artifact's encounter, as well as the artifact's ";
                reply += "complete book of scrolls.";
                SayTo(player, eChatLoc.CL_PopupWindow, reply);
                return;
            }

            Hashtable versions = null;
            if (!PlayerCanUseArtifact(player, artifact, ref versions))
            {
                // TODO: Player can not use this artifact, how is that handled?
                return;
            }

            // If we haven't received the book yet, we will take it from the
            // player's backpack now.

            if (credit.GetCustomProperty("book") != "true")
            {
                credit.SetCustomProperty("book", "true");
                player.Inventory.RemoveItem(book);
            }

			IDictionaryEnumerator versionEnum = versions.GetEnumerator();
			if (versions.Count == 1)
			{
				versionEnum.MoveNext();
                GameInventoryItem artifactItem = 
                    GameInventoryItem.CreateFromTemplate((ItemTemplate)versionEnum.Value);
				player.ReceiveItem(this, artifactItem);
                credit.SetCustomProperty("artifact", "true");
                return;
			}

            // TODO: Check on live.

            int numVersions = versions.Count;
            String question = "Would you like ";
            while (versionEnum.MoveNext())
            {
                if (numVersions < versions.Count)
                {
                    if (versions.Count > 2)
                        question += (numVersions == 1) ? ", or " : ", ";
                    else
                        question += (numVersions == 1) ? " or " : " ";
                }
                question += String.Format("a [{0}] version", versionEnum.Value);
                --numVersions;
            }

            question += "?";
            SayTo(player, eChatLoc.CL_PopupWindow, question);
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

		/// <summary>
		/// Check if player has the book for this artifact.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="artifact"></param>
		/// <param name="book"></param>
		/// <returns></returns>
		private bool PlayerHasBook(GamePlayer player, Artifact artifact, ref InventoryItem book)
		{
			ICollection backpack = player.Inventory.GetItemRange(eInventorySlot.FirstBackpack,
				eInventorySlot.LastBackpack);

			foreach (InventoryItem item in backpack)
			{
				if (item == null || !ArtifactMgr.IsArtifactBook(item))
					continue;

                if (ArtifactMgr.GetArtifactIDFromBookID(item.Name) == artifact.ArtifactID)
                {
                    book = item;
                    return true;
                }
			}

			return false;
		}

		/// <summary>
		/// Check if player has the quest completed for this artifact.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="artifact"></param>
		/// <param name="credit"></param>
		/// <returns></returns>
		private bool PlayerHasQuest(GamePlayer player, Artifact artifact, ref AbstractQuest credit)
		{
            IList finishedQuests = player.QuestListFinished;
            
            foreach (AbstractQuest quest in finishedQuests)
            {
                if (quest.GetType() == ArtifactMgr.GetQuestTypeFromArtifactID(artifact.ArtifactID))
                {
                    credit = quest;
                    return true;
                }
            }

			return false;
		}
    }
}
