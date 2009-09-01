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
using DOL.Language;
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


		/// <summary>
		/// if quest qualification fails use this string to store the reason why
		/// </summary>
		protected string m_reasonFailQualification = "";
		public string ReasonFailQualification
		{
			get { return m_reasonFailQualification; }
		}


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

			String[] scholars = ArtifactMgr.GetScholars(artifactID);
			if (scholars != null)
			{
                eRealm realm = eRealm.Albion;
				GameNPC[] npcs;

				foreach (String scholar in scholars)
				{
                    String title;

                    switch (realm)
                    {
                        case eRealm.Albion:
                            title = "Scholar";
                            npcs = WorldMgr.GetNPCsByName(String.Format("{0} {1}", 
                                title, scholar), realm);
                            break;
                        case eRealm.Midgard:
                            title = "Loremaster";
                            npcs = WorldMgr.GetNPCsByName(String.Format("{0} {1}", 
                                title, scholar), realm);

                            if (npcs.Length == 0)
                            {
                                title = "Loremistress";
                                npcs = WorldMgr.GetNPCsByName(String.Format("{0} {1}", 
                                    title, scholar), realm);
                            }
                            break;
                        case eRealm.Hibernia:
                            title = "Sage";
                            npcs = WorldMgr.GetNPCsByName(String.Format("{0} {1}", 
                                title, scholar), realm);
                            break;
                        default:
                            title = "<unknown title>";
                            npcs = new GameNPC[0];
                            break;
                    }

					if (npcs.Length == 0)
					{
						log.Warn(String.Format("{0} {1} not found in {1}",
							title, scholar, GlobalConstants.RealmToName(realm)));
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

			Type encounterType = ArtifactMgr.GetEncounterType(ArtifactID);

			if (encounterType == null)
			{
				m_reasonFailQualification = "It does not appear this encounter type is set up correctly.";
				log.Error("ArtifactQuest: EncounterType is null for ArtifactID " + ArtifactID);
				return false;
			}

			if (player == null)
			{
				m_reasonFailQualification = "Player is null for quest, serious error!";
				log.Error("ArtifactQuest: Player is null for ArtifactID " + ArtifactID + " encounterType " + encounterType.FullName);
				return false;
			}

			if (player.Level < Level)
			{
				m_reasonFailQualification = "You must be at least level " + Level + " in order to complete this quest.";
				return false;
			}

			if (player.HasFinishedQuest(encounterType) <= 0)
			{
				m_reasonFailQualification = "You must first get the encounter credit for this artifact.";
				return false;
			}

			if (!ArtifactMgr.HasBook(player, ArtifactID))
			{
				m_reasonFailQualification = "You are missing the correct book for this artifact.";
				return false;
			}

			if (player.IsDoingQuest(this.GetType()) != null)
			{
				m_reasonFailQualification = "You've already started the quest for this artifact.";
				return false;
			}

			if (player.HasFinishedQuest(this.GetType()) != 0)
			{
				m_reasonFailQualification = "You've already completed the quest for this artifact.";
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
				return false;

			Dictionary<String, ItemTemplate> versions = ArtifactMgr.GetArtifactVersions(ArtifactID,
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
		/// Hand out an artifact.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="player"></param>
		/// <param name="artifactID"></param>
		/// <param name="itemTemplate"></param>
		protected static bool GiveItem(GameLiving source, GamePlayer player, String artifactID, ItemTemplate itemTemplate)
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
