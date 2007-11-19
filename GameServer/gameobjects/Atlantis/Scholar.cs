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

			IList quests = QuestListToGive;
			if (quests.Count == 0)
				intro += " It seems your database does not provide any artifact quests for me";
			else
			{
				lock (quests.SyncRoot)
				{
					int numQuests = quests.Count;
					foreach (ArtifactQuest quest in quests)
					{
						if (quests.Count > 1 && numQuests < quests.Count)
							intro += (numQuests == 1) ? ", or " : ", ";
						intro += String.Format("[{0}]", quest.ArtifactID);
						--numQuests;
					}
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

			lock (QuestListToGive.SyncRoot)
			{
				// Start new quest...

				foreach (ArtifactQuest quest in QuestListToGive)
				{
					if (text.ToLower() == quest.ArtifactID.ToLower())
					{
						if (quest.CheckQuestQualification(player))
							GiveArtifactQuest(player, quest.GetType());
						else
							DenyArtifactQuest(player);
						return true;
					}
				}

				// ...or continuing a quest?

				foreach (AbstractQuest quest in player.QuestList)
				{
					if (quest is ArtifactQuest && QuestListToGive.Contains(quest))
						if ((quest as ArtifactQuest).WhisperReceive(player, this, text))
							return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Deny a quest to a player.
		/// </summary>
		/// <param name="player"></param>
		private void DenyArtifactQuest(GamePlayer player)
		{
			if (player != null)
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
			}
			return;
		}

		/// <summary>
		/// Give the artifact quest to the player.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="questType"></param>
		private void GiveArtifactQuest(GamePlayer player, Type questType)
		{
			if (player == null || questType == null)
				return;

			ArtifactQuest quest = (ArtifactQuest)Activator.CreateInstance(questType,
				new object[] { player });

			if (quest == null)
				return;

			player.AddQuest(quest);
			quest.WhisperReceive(player, this, quest.ArtifactID);
		}

		/// <summary>
		/// Invoked when scholar receives an item.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public override bool ReceiveItem(GameLiving source, InventoryItem item)
		{
			if (base.ReceiveItem(source, item))
				return true;;

			GamePlayer player = source as GamePlayer;
			if (player != null)
			{
				lock (QuestListToGive.SyncRoot)
				{
					foreach (AbstractQuest quest in player.QuestList)
						if (quest is ArtifactQuest && (HasQuest(quest.GetType()) != null))
							if ((quest as ArtifactQuest).ReceiveItem(player, this, item))
								return true;
				}
			}

			return false;
		}
    }
}
