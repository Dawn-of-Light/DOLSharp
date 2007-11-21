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
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;
using System.Reflection;
using System.Collections;

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
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public ArtifactQuest()
			: base() { }

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
		public static void Init(String artifactID, Type questType)
		{
			if (artifactID == null || questType == null)
				return;

			String[] scholars = ArtifactMgr.GetScholarsFromArtifactID(artifactID);
			if (scholars != null)
			{
				int realm = 1;
				GameNPC[] npcs;
				foreach (String scholar in scholars)
				{
					npcs = WorldMgr.GetNPCsByName(String.Format("Scholar {0}", scholar), (eRealm)realm);
					if (npcs.Length == 0)
					{
						log.Warn(String.Format("Scholar {0} not found in {1}",
							scholar, GlobalConstants.RealmToName((eRealm)realm)));
					}
					else
					{
						npcs[0].AddQuestToGive(questType);
					}

					++realm;
				}
			}
		}

		/// <summary>
		/// The artifact ID.
		/// </summary>
		public virtual String ArtifactID
		{
			get { return "UNDEFINED"; }
		}

		/// <summary>
		/// Check if player is eligible for this quest.
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override bool CheckQuestQualification(GamePlayer player)
		{
			// Must have the encounter, must have the book; must not be on the quest
			// and must not have the quest finished either.

			Type encounterType = 
				ArtifactMgr.GetEncounterType(ArtifactID);

			return (player != null &&
				encounterType != null &&
				player.Level >= Level &&
                ArtifactMgr.HasBookForArtifact(player, ArtifactID) &&
				player.HasFinishedQuest(encounterType) > 0 &&
				player.IsDoingQuest(this.GetType()) == null &&
				player.HasFinishedQuest(this.GetType()) == 0);
		}

		/// <summary>
		/// Check if player can use the reward.
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public virtual bool CheckRewardQualification(GamePlayer player)
		{
			if (player == null)
				return false;

			Hashtable versions = ArtifactMgr.GetArtifactVersionsFromClass(ArtifactID,
				(eCharacterClass)player.CharacterClass.ID, (eRealm)player.Realm);

			return (versions.Count > 0);
		}

		/// <summary>
		/// Minimum level requirement, adjust this for lowbie artifacts.
		/// </summary>
		public override int Level
		{
			get { return 45; }
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
		/// Remove an item from the player's inventory.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public virtual bool RemoveItem(GamePlayer player, InventoryItem item)
		{
			lock (player.Inventory)
				return player.Inventory.RemoveItem(item);
		}

		/// <summary>
		/// When finishing an artifact quest remove the encounter
		/// credit.
		/// </summary>
		public override void FinishQuest()
		{
			base.FinishQuest();

			Type encounterType =
				ArtifactMgr.GetEncounterType(ArtifactID);

			QuestPlayer.RemoveEncounterCredit(encounterType);
		}
    }
}
