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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DOL.Database2;
using System.Reflection;
using log4net;
using DOL.GS.PacketHandler;
using DOL.GS.Quests;
using DOL.Events;

namespace DOL.GS
{
	/// <summary>
	/// The artifact manager.
	/// </summary>
	/// <author>Aredhel</author>
    public sealed class ArtifactMgr
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static Dictionary<String, Artifact> m_artifacts;
        private static Dictionary<String, List<ArtifactXItem>> m_artifactVersions;
		private static List<ArtifactBonus> m_artifactBonuses;

        public enum Book { NoPage = 0x0, Page1 = 0x1, Page2 = 0x2, Page3 = 0x4, AllPages = 0x7 };


        public static bool Init()
        {
            return LoadArtifacts();
        }

        /// <summary>
        /// Load artifacts from the DB.
        /// </summary>
        public static bool LoadArtifacts()
        {
			// Load artifacts and books.

            DatabaseObject[] dbo = GameServer.Database.SelectObjects(typeof(Artifact));
			m_artifacts = new Dictionary<String, Artifact>();
            foreach (Artifact artifact in dbo)
                m_artifacts.Add(artifact.ArtifactID, artifact);

			// Load artifact versions.

            dbo = GameServer.Database.SelectObjects(typeof(ArtifactXItem));
			m_artifactVersions = new Dictionary<String, List<ArtifactXItem>>();
			List<ArtifactXItem> versionList;
            foreach (ArtifactXItem artifactVersion in dbo)
            {
				if (m_artifactVersions.ContainsKey(artifactVersion.ArtifactID))
					versionList = m_artifactVersions[artifactVersion.ArtifactID];
				else
                {
					versionList = new List<ArtifactXItem>();
                    m_artifactVersions.Add(artifactVersion.ArtifactID, versionList);
                }
                versionList.Add(artifactVersion);
            }

			// Load artifact bonuses.

			dbo = GameServer.Database.SelectObjects(typeof(ArtifactBonus));
			m_artifactBonuses = new List<ArtifactBonus>();
			foreach (ArtifactBonus artifactBonus in dbo)
				m_artifactBonuses.Add(artifactBonus);

			// Install event handlers.

			GameEventMgr.AddHandler(GamePlayerEvent.GainedExperience, 
				new DOLEventHandler(PlayerGainedExperience));

            log.Info(String.Format("{0} artifacts loaded", m_artifacts.Count));
            return true;
		}

		/// <summary>
		/// Find the matching artifact for the item
		/// </summary>
		/// <param name="itemID"></param>
		/// <returns></returns>
		public static String GetArtifactIDFromItemID(String itemID)
		{
			if (itemID == null)
				return null;

			String artifactID = null;
			lock (m_artifactVersions)
			{
				foreach (List<ArtifactXItem> list in m_artifactVersions.Values)
				{
					foreach (ArtifactXItem AxI in list)
					{
						if (AxI.ItemID == itemID)
						{
							artifactID = AxI.ArtifactID;
							break;
						}
					}
				}
			}

			return artifactID;
		}

        /// <summary>
        /// Find all artifacts from a particular zone.
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
		public static List<Artifact> GetArtifacts(String zone)
        {
			List<Artifact> artifacts = new List<Artifact>();

            if (zone != null)
                lock (m_artifacts)
                    foreach (Artifact artifact in m_artifacts.Values)
                        if (artifact.Zone == zone)
                            artifacts.Add(artifact);

            return artifacts;
        }

		/// <summary>
		/// Get the cooldown for this artifact.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static int GetReuseTimer(InventoryArtifact item)
		{
			if (item == null)
				return 0;

			String artifactID = GetArtifactIDFromItemID(item.Id_nb);
			lock (m_artifacts)
			{
				if (!m_artifacts.ContainsKey(artifactID))
				{
					log.Warn(String.Format("Can't find artifact for ID '{0}'", artifactID));
					return 0;
				}

				Artifact artifact = m_artifacts[artifactID];
				return artifact.ReuseTimer;
			}
		}

		/// <summary>
		/// Get a list of all scholars studying this artifact.
		/// </summary>
		/// <param name="artifactID"></param>
		/// <returns></returns>
		public static String[] GetScholars(String artifactID)
		{
			if (artifactID != null)
				lock (m_artifacts)
					if (m_artifacts.ContainsKey(artifactID))
						return m_artifacts[artifactID].ScholarID.Split(';');

			return null;
		}

		/// <summary>
		/// Checks whether or not the item is an artifact.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool IsArtifact(InventoryItem item)
		{
			if (item == null)
				return false;

			if (item is InventoryArtifact)
				return true;

			lock (m_artifactVersions)
				foreach (List<ArtifactXItem> versions in m_artifactVersions.Values)
					foreach (ArtifactXItem version in versions)
						if (version.ItemID == item.Id_nb)
							return true;
			return false;
		}

		#region Experience/Level

		private static readonly long[] m_xpForLevel =
			{
				0,				// xp to level 0
				50000000,		// xp to level 1
				100000000,		// xp to level 2
				150000000,		// xp to level 3
				200000000,		// xp to level 4
				250000000,		// xp to level 5
				300000000,		// xp to level 6
				350000000,		// xp to level 7
				400000000,		// xp to level 8
				450000000,		// xp to level 9
				500000000		// xp to level 10
			};

		/// <summary>
		/// Determine artifact level from total XP.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static int GetCurrentLevel(InventoryArtifact item)
		{
			if (item != null)
			{
				for (int level = 10; level >= 0; --level)
					if (item.Experience >= m_xpForLevel[level])
						return level;
			}
			return 0;
		}

		/// <summary>
		/// Calculate the XP gained towards the next level (in percent).
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static int GetXPGainedForLevel(InventoryArtifact item)
		{
			if (item != null)
			{
				int level = GetCurrentLevel(item);
				if (level < 10)
				{
					double xpGained = item.Experience - m_xpForLevel[level];
					double xpNeeded = m_xpForLevel[level + 1] - m_xpForLevel[level];
					return (int)(xpGained * 100 / xpNeeded);
				}
			}
			return 0;
		}

		/// <summary>
		/// Get a list of all level requirements for this artifact.
		/// </summary>
		/// <param name="artifactID"></param>
		/// <returns></returns>
		public static int[] GetLevelRequirements(String artifactID)
		{
			int[] requirements = new int[ArtifactBonus.ID.Max - ArtifactBonus.ID.Min + 1];

			lock (m_artifactBonuses)
				foreach (ArtifactBonus bonus in m_artifactBonuses)
					if (bonus.ArtifactID == artifactID)
						requirements[bonus.BonusID] = bonus.Level;

			return requirements;
		}

		/// <summary>
		/// What this artifact gains XP from.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static String GetEarnsXP(InventoryArtifact item)
		{
			return "Slaying enemies and monsters found anywhere.";
		}

		/// <summary>
		/// Called from GameEventMgr when player has gained experience.
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public static void PlayerGainedExperience(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			GainedExperienceEventArgs xpArgs = args as GainedExperienceEventArgs;
			if (player == null || xpArgs == null)
				return;

			// Suffice to calculate total XP once for all artifacts.

			long xpAmount = xpArgs.ExpBase +
					xpArgs.ExpCampBonus +
					xpArgs.ExpGroupBonus +
					xpArgs.ExpOutpostBonus;

			// Only currently equipped artifacts can gain experience.

			ICollection equippedItems = player.Inventory.GetItemRange(eInventorySlot.MinEquipable,
				eInventorySlot.MaxEquipable);

			foreach (InventoryItem item in equippedItems)
				if (item != null && item is InventoryArtifact)
					ArtifactGainedExperience(player, item as InventoryArtifact, xpAmount);
		}

		/// <summary>
		/// Called when an artifact has gained experience.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="item"></param>
		/// <param name="xpAmount"></param>
		private static void ArtifactGainedExperience(GamePlayer player, InventoryArtifact item, long xpAmount)
		{
			if (player == null || item == null)
				return;

			long artifactXPOld = item.Experience;
			if (artifactXPOld >= m_xpForLevel[10])	// Can't go past level 10.
				return;

			// All artifacts share the same XP table, we make them level
			// at different rates by tweaking the XP rate.

			int xpRate;

			lock (m_artifacts)
			{
				Artifact artifact = m_artifacts[item.ArtifactID];
				if (artifact == null)
					return;
				xpRate = artifact.XPRate;
			}

			long artifactXPNew = (long)(artifactXPOld + (xpAmount * xpRate)/100);
			item.Experience = artifactXPNew;

			player.Out.SendMessage(String.Format("Your {0} has gained experience.", item.Name),
				eChatType.CT_Important, eChatLoc.CL_SystemWindow);

			// Now let's see if this artifact has gained a new level yet.

			for (int level = 1; level <= 10; ++level)
			{
				if (artifactXPOld < m_xpForLevel[level] && artifactXPNew >= m_xpForLevel[level])
				{
					player.Out.SendMessage(String.Format("Your {0} has gained a level!", item.Name),
						eChatType.CT_Important, eChatLoc.CL_SystemWindow);
					item.OnLevelGained(player, level);
					return;
				}
			}
		}

		#endregion

		#region Artifact Versions

		/// <summary>
		/// Get a list of all versions for this artifact.
		/// </summary>
		/// <param name="artifactID"></param>
		/// <param name="realm"></param>
		/// <returns></returns>
		private static List<ArtifactXItem> GetArtifactVersions(String artifactID, eRealm realm)
        {
			List<ArtifactXItem> versions = new List<ArtifactXItem>();
            if (artifactID != null)
            {
				lock (m_artifactVersions)
				{
					if (m_artifactVersions.ContainsKey(artifactID))
					{
						List<ArtifactXItem> allVersions = m_artifactVersions[artifactID];
						foreach (ArtifactXItem version in allVersions)
							if (version.Realm == 0 || version.Realm == (int)realm)
								versions.Add(version);
					}
				}
            }

            return versions;
        }

        /// <summary>
        /// Create a hashtable containing all item templates that are valid for
        /// this class.
        /// </summary>
        /// <param name="artifactID"></param>
        /// <param name="charClass"></param>
		/// <param name="realm"></param>
        /// <returns></returns>
		public static Dictionary<String, ItemTemplate> GetArtifactVersions(String artifactID, 
			eCharacterClass charClass, eRealm realm)
        {
            if (artifactID == null)
                return null;

            List<ArtifactXItem> allVersions = GetArtifactVersions(artifactID, realm);
			Dictionary<String, ItemTemplate> classVersions = new Dictionary<String, ItemTemplate>();

            lock (allVersions)
            {
                ItemTemplate itemTemplate;
                foreach (ArtifactXItem version in allVersions)
                {
                    itemTemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate),
                        version.ItemID);

                    if (itemTemplate == null)
                    {
                        log.Warn(String.Format("Artifact item template '{0}' is missing",
                            version.ItemID));
                    }
                    else
                    {
                        String[] classIDs = itemTemplate.AllowedClasses.Split(';');
                        foreach (String classID in classIDs)
                        {
                            try
                            {
                                if (Int16.Parse(classID) == (int)charClass)
                                {
                                    classVersions.Add(version.Version, itemTemplate);
                                    break;
                                }
                            }
                            catch
                            {
                                log.Warn(String.Format("Invalid class ID '{0}' for item template '{1}'",
                                    classID, itemTemplate.Id_nb));
                            }
                        }
                    }
                }
            }

            return classVersions;
		}

		#endregion

		#region Encounters & Quests

		/// <summary>
		/// Get the quest type from the quest type string.
		/// </summary>
		/// <param name="questTypeString"></param>
		/// <returns></returns>
		public static Type GetQuestType(String questTypeString)
		{
			Type questType = null;
			foreach (Assembly asm in ScriptMgr.Scripts)
			{
				questType = asm.GetType(questTypeString);
				if (questType != null)
					break;
			}

			if (questType == null)
				questType = Assembly.GetAssembly(typeof(GameServer)).GetType(questTypeString);

			return questType;
		}

		/// <summary>
		/// Get the quest type for the encounter from the artifact ID.
		/// </summary>
		/// <param name="artifactID"></param>
		/// <returns></returns>
		public static Type GetEncounterType(String artifactID)
		{
			if (artifactID != null)
				lock (m_artifacts)
					if (m_artifacts.ContainsKey(artifactID))
						return GetQuestType(m_artifacts[artifactID].EncounterID);

			return null;
		}

		/// <summary>
		/// Grant credit for an artifact.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="artifactID"></param>
		/// <returns></returns>
		public static bool GrantArtifactCredit(GamePlayer player, String artifactID)
		{
			if (player == null || artifactID == null)
				return false;

			Artifact artifact;
			lock (m_artifacts)
			{
				if (!m_artifacts.ContainsKey(artifactID))
					return false;
				artifact = m_artifacts[artifactID];
			}

			if (artifact == null)
				return false;

			Type questType = GetQuestType(artifact.EncounterID);
			if (questType == null)
				return false;

			if (player.HasFinishedQuest(questType) > 0)
				return false;

			AbstractQuest quest = (AbstractQuest)(System.Activator.CreateInstance(questType,
				new object[] { player }));

			if (quest == null)
				return false;

			quest.FinishQuest();
			return true;
		}

		#endregion

		#region Scrolls & Books

        /// <summary>
        /// Find the matching artifact for this book.
        /// </summary>
        /// <param name="bookID"></param>
        /// <returns></returns>
        public static String GetArtifactID(String bookID)
        {
            if (bookID != null)
				lock (m_artifacts)
					foreach (Artifact artifact in m_artifacts.Values)
						if (artifact.BookID == bookID)
							return artifact.ArtifactID;

			return null;
        }

		/// <summary>
		/// Check whether these 2 items can be combined.
		/// </summary>
		/// <param name="item1"></param>
		/// <param name="item2"></param>
		/// <returns></returns>
		public static bool CanCombine(InventoryItem item1, InventoryItem item2)
		{
            String artifactID1 = null;
            Book pageNumbers1 = GetPageNumbers(item1, ref artifactID1);
            if (pageNumbers1 == Book.NoPage || pageNumbers1 == Book.AllPages)
                return false;

            String artifactID2 = null;
            Book pageNumbers2 = GetPageNumbers(item2, ref artifactID2);
            if (pageNumbers2 == Book.NoPage || pageNumbers2 == Book.AllPages)
                return false;

            if (artifactID1 != artifactID2 ||
                (Book)((int)pageNumbers1 & (int)pageNumbers2) != Book.NoPage)
                return false;

            return true;
		}

        /// <summary>
        /// Check which scroll pages are in this item.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="artifactID"></param>
        /// <returns></returns>
        public static Book GetPageNumbers(InventoryItem item, ref String artifactID)
        {
            if (item == null || item.Object_Type != (int)eObjectType.Magical
                || item.Item_Type != (int)eInventorySlot.FirstBackpack)
                return Book.NoPage;

            lock (m_artifacts)
            {
                foreach (Artifact artifact in m_artifacts.Values)
                {
                    artifactID = artifact.ArtifactID;
                    if (item.Name == artifact.Scroll1)
                        return Book.Page1;
                    else if (item.Name == artifact.Scroll2)
                        return Book.Page2;
                    else if (item.Name == artifact.Scroll3)
                        return Book.Page3;
                    else if (item.Name == artifact.Scroll12)
                        return (Book)((int)Book.Page1 | (int)Book.Page2);
                    else if (item.Name == artifact.Scroll13)
                        return (Book)((int)Book.Page1 | (int)Book.Page3);
                    else if (item.Name == artifact.Scroll23)
                        return (Book)((int)Book.Page2 | (int)Book.Page3);
                    else if (item.Name == artifact.BookID)
                        return Book.AllPages;
                }
            }

            return Book.NoPage;
        }

		/// <summary>
		/// Find all artifacts that this player carries.
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public static List<String> GetArtifacts(GamePlayer player)
		{
		    List<String> artifacts = new List<String>();
			lock (player.Inventory.AllItems.SyncRoot)
			{
				foreach (InventoryItem item in player.Inventory.AllItems)
				{
					String artifactID = GetArtifactIDFromItemID(item.Id_nb);
					if (artifactID != null)
						artifacts.Add(artifactID);
				}
			}
			return artifacts;
		}

		/// <summary>
		/// Get the artifact for this scroll/book item.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static Artifact GetArtifact(InventoryItem item)
		{
			String artifactID = "";
			if (GetPageNumbers(item, ref artifactID) == Book.NoPage)
				return null;

			lock (m_artifacts)
				return m_artifacts[artifactID];
		}

        /// <summary>
        /// Whether or not the player has the complete book for this 
        /// artifact in his backpack.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="artifactID"></param>
        /// <returns></returns>
        public static bool HasBook(GamePlayer player, String artifactID)
        {
			if (player == null || artifactID == null)
				return false;

			// Find out which book is needed.

			String bookID;
			lock (m_artifacts)
			{
				Artifact artifact = m_artifacts[artifactID];
				if (artifact== null)
				{
					log.Warn(String.Format("Can't find book for artifact \"{0}\"", artifactID));
					return false;
				}
				bookID = artifact.BookID;
			}

			// Now check if the player has got it.

			ICollection backpack = player.Inventory.GetItemRange(eInventorySlot.FirstBackpack,
				eInventorySlot.LastBackpack);
			foreach (InventoryItem item in backpack)
			{
				if (item.Object_Type == (int)eObjectType.Magical &&
					item.Item_Type == (int)eInventorySlot.FirstBackpack &&
					item.Name == bookID)
					return true;
			}

			return false;
        }

		/// <summary>
		/// Whether or not the item is an artifact scroll.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool IsArtifactScroll(InventoryItem item)
		{
            String artifactID = null;
            Book pageNumbers = GetPageNumbers(item, ref artifactID);
            return (pageNumbers != Book.NoPage && pageNumbers != Book.AllPages);
		}

		/// <summary>
		/// Combine 2 scrolls.
		/// </summary>
		/// <param name="scroll1"></param>
		/// <param name="scroll2"></param>
		/// <param name="combinesToBook"></param>
		/// <returns></returns>
		public static GameInventoryItem CombineScrolls(InventoryItem scroll1, InventoryItem scroll2,
			ref bool combinesToBook)
		{
			if (!CanCombine(scroll1, scroll2))
				return null;

            String artifactID = null;
            Book combinedPages = (Book)((int)GetPageNumbers(scroll1, ref artifactID) |
                (int)GetPageNumbers(scroll2, ref artifactID));

			combinesToBook = (combinedPages == Book.AllPages);
            return CreatePages(artifactID, combinedPages);
		}

        /// <summary>
        /// Create a scroll or book containing the given page numbers.
        /// </summary>
        /// <param name="artifactID"></param>
        /// <param name="pageNumbers"></param>
        /// <returns></returns>
        private static GameInventoryItem CreatePages(String artifactID, Book pageNumbers)
        {
            if (artifactID == null || pageNumbers == Book.NoPage)
                return null;

            Artifact artifact;
            lock (m_artifacts)
                artifact = m_artifacts[artifactID];

            if (artifact == null)
                return null;

            GameInventoryItem scroll = GameInventoryItem.CreateFromTemplate("artifact_scroll");
            if (scroll == null)
                return null;

            String scrollTitle = null;
			int scrollModel = 499;
            switch (pageNumbers)
            {
                case Book.Page1: 
					scrollTitle = artifact.Scroll1;
					scrollModel = artifact.ScrollModel1;
                    break;
                case Book.Page2: 
					scrollTitle = artifact.Scroll2;
					scrollModel = artifact.ScrollModel1;
                    break;
                case Book.Page3: 
					scrollTitle = artifact.Scroll3;
					scrollModel = artifact.ScrollModel1;
                    break;
                case (Book)((int)Book.Page1 | (int)Book.Page2): 
					scrollTitle = artifact.Scroll12;
					scrollModel = artifact.ScrollModel2;
                    break;
                case (Book)((int)Book.Page1 | (int)Book.Page3): 
					scrollTitle = artifact.Scroll13;
					scrollModel = artifact.ScrollModel2;
                    break;
                case (Book)((int)Book.Page2 | (int)Book.Page3): 
					scrollTitle = artifact.Scroll23;
					scrollModel = artifact.ScrollModel2;
                    break;
                case Book.AllPages: 
					scrollTitle = artifact.BookID;
					scrollModel = artifact.BookModel;
                    break;
            }

            scroll.Name = scrollTitle;
            scroll.Item.Name = scrollTitle;
			scroll.Model = (ushort)scrollModel;
			scroll.Item.Model = (ushort)scrollModel;
            return scroll;
        }

		/// <summary>
        /// Create a scroll from a particular book.
        /// </summary>
        /// <param name="artifactID">The artifact's ID.</param>
        /// <param name="pageNumber">Scroll page number (1-3).</param>
        /// <returns>An item that can be picked up by a player (or null).</returns>
        public static GameInventoryItem CreateScroll(String artifactID, int pageNumber)
        {
			if (pageNumber < 1 || pageNumber > 3)
				return null;

            switch (pageNumber)
            {
                case 1: return CreatePages(artifactID, Book.Page1);
                case 2: return CreatePages(artifactID, Book.Page2);
                case 3: return CreatePages(artifactID, Book.Page3);
            }

            return null;
        }

		#endregion
    }
}
