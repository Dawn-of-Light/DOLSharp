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
using DOL.Events;
using DOL.GS.Quests;
using DOL.Database;
using DOL.GS.PacketHandler;
using System.Collections;

namespace DOL.GS.Quests.Atlantis.Artifacts
{
	/// <summary>
	/// Quest for the Eternal Plant artifact.
	/// </summary>
	/// <author>Aredhel</author>
	class EternalPlant : ArtifactQuest
	{
		public EternalPlant()
			: base() { }

		public EternalPlant(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		/// <summary>
		/// This constructor is needed to load quests from the DB.
		/// </summary>
		/// <param name="questingPlayer"></param>
		/// <param name="dbQuest"></param>
        public EternalPlant(GamePlayer questingPlayer, DBQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Quest initialisation.
		/// </summary>
		public static void Init()
		{
            ArtifactQuest.Init("Eternal Plant", typeof(EternalPlant));
		}

        /// <summary>
        /// Check if player is eligible for this quest.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool CheckQuestQualification(GamePlayer player)
        {
            if (!base.CheckQuestQualification(player))
                return false;

            // TODO: Check if this is the correct level for the quest.
            return (player.Level >= 45);
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
				return true;

			GamePlayer player = source as GamePlayer;
			Scholar scholar = target as Scholar;
			if (player == null || scholar == null)
				return false;

			if (Step == 2 && ArtifactMgr.GetArtifactID(item.Name) == ArtifactID)
			{
				Dictionary<String, ItemTemplate> versions = ArtifactMgr.GetArtifactVersions(ArtifactID,
					(eCharacterClass)player.CharacterClass.ID, (eRealm)player.Realm);
				
				if (versions.Count > 0 && RemoveItem(player, item))
				{
					GiveItem(scholar, player, ArtifactID, versions[";;"]);
					String reply = String.Format("Brilliant, thank you! Here, take the artifact. {0} {1} {2}",
						"I've unlocked its powers for you. As I've said before, I'm more interested",
						"in the stories and the history behind these artifacts than the actual items",
						"themselves.");
					scholar.TurnTo(player);
					scholar.SayTo(player, eChatLoc.CL_PopupWindow, reply);
					FinishQuest();
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
				return true;

			GamePlayer player = source as GamePlayer;
			Scholar scholar = target as Scholar;
			if (player == null || scholar == null)
				return false;

			if (Step == 1 && text.ToLower() == ArtifactID.ToLower())
			{
                String reply = "Oh, the mysterious Eternal Plant. Do you have the scrolls on it? I've found a few that allude to its true nature, but haven't found anything with any detail.";
				scholar.TurnTo(player);
				scholar.SayTo(player, eChatLoc.CL_PopupWindow, reply);
				Step = 2;
				return true;
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
						return "Defeat Sobekhotep.";
					case 2:
						return "Turn in the completed book.";
					default:
						return base.Description;
				}
			}
		}

		/// <summary>
		/// The name of the quest (not necessarily the same as
		/// the name of the reward).
		/// </summary>
		public override string Name
		{
            get { return "Eternal Plant"; }
		}

		/// <summary>
		/// The reward for this quest.
		/// </summary>
		public override String ArtifactID
		{
            get { return "Eternal Plant"; }
		}
	}
}
