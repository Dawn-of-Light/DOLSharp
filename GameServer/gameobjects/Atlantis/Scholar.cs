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

namespace DOL.GS
{
    /// <summary>
    /// The scholars handing out the artifacts.
    /// </summary>
    /// <author>Aredhel</author>
    public class Scholar : Researcher
    {
		private ArrayList m_artifacts;

		/// <summary>
		/// Create a new scholar and load the list of artifacts
		/// he's studying.
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

        public override bool ReceiveItem(GameLiving source, InventoryItem item)
        {
            if (!(source is GamePlayer))
                return false;

            GamePlayer player = source as GamePlayer;

            //if (!ArtifactMgr.IsArtifactBook(item))
            //{
            //    player.Out.SendMessage(String.Format("{0} does not want that item.", GetName(0, true)));
            //    return false;
            //}

            return false;
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

			if (subject == null)
				return false;

			HandOutArtifact(player, subject);
			return true;
		}

		/// <summary>
		/// If player has the book in his backpack and the quest completed
		/// in his log, hand out the artifact, else turn him down.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="artifact"></param>
		private void HandOutArtifact(GamePlayer player, Artifact artifact)
		{
			if (PlayerHasBook(player, artifact) && PlayerHasQuest(player, artifact))
			{
				// TODO!
				return;
			}
			
			String reply = String.Format("{0}, I cannot activate that artifact for you. ",
				player.Name);
			reply += "This could be because you have already activated it, or you are in the ";
			reply += "process of activating it, or you may not have completed everything ";
			reply += "you need to do. Remember that the activation process requires you to ";
			reply += "have credit for the artifact's encounter, as well as the artifact's ";
			reply += "complete book of scrolls.";

			SayTo(player, eChatLoc.CL_PopupWindow, reply);
		}

		/// <summary>
		/// Check if player has the book for this artifact.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="artifact"></param>
		/// <returns></returns>
		private bool PlayerHasBook(GamePlayer player, Artifact artifact)
		{
			ICollection backpack = player.Inventory.GetItemRange(eInventorySlot.FirstBackpack,
				eInventorySlot.LastBackpack);

			foreach (InventoryItem item in backpack)
			{
				if (item == null || !ArtifactMgr.IsArtifactBook(item))
					continue;

				if (ArtifactMgr.GetArtifactFromBookID(item.Name).ArtifactID == artifact.ArtifactID)
					return true;
			}

			return false;
		}

		/// <summary>
		/// Check if player has the quest completed for this artifact.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="artifact"></param>
		/// <returns></returns>
		private bool PlayerHasQuest(GamePlayer player, Artifact artifact)
		{
			IList finishedQuests = player.QuestListFinished;
			if (finishedQuests == null)
				return false;

			foreach (AbstractQuest quest in finishedQuests)
				if (quest.Name == artifact.QuestID)
					return true;

			return false;
		}
    }
}
