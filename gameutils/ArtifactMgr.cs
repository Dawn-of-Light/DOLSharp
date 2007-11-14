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
using DOL.Database;
using System.Reflection;
using log4net;
using DOL.GS.PacketHandler;
using DOL.GS.Quests;
using DOL.GS.Scripts;

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

        private static ArrayList m_artifacts;
        private static Hashtable m_artifactVersions;

        public enum Book { Page1 = 0x1, Page2 = 0x2, Page3 = 0x4, AllPages = 0x7 };


        public static bool Init()
        {
            return LoadArtifacts();
        }

        /// <summary>
        /// Load artifacts from the DB.
        /// </summary>
        public static bool LoadArtifacts()
        {
            DataObject[] dbo = GameServer.Database.SelectAllObjects(typeof(Artifact));
			m_artifacts = new ArrayList();

            for (int i = 0; i < dbo.Length; i++)
                m_artifacts.Add(dbo[i]);

            dbo = GameServer.Database.SelectAllObjects(typeof(ArtifactXItem));
            m_artifactVersions = new Hashtable();
            ArrayList versionList;
            foreach (ArtifactXItem artifactVersion in dbo)
            {
                versionList = (ArrayList)m_artifactVersions[artifactVersion.ArtifactID];
                if (versionList == null)
                {
                    versionList = new ArrayList();
                    m_artifactVersions.Add(artifactVersion.ArtifactID, versionList);
                }
                versionList.Add(artifactVersion);
            }

            log.Info(String.Format("{0} artifacts loaded", m_artifacts.Count));
            return true;
		}

        /// <summary>
        /// Find all artifacts from a particular zone.
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        public static ArrayList GetArtifactsFromZone(String zone)
        {
            ArrayList artifacts = new ArrayList();

            lock (m_artifacts.SyncRoot)
            {
                foreach (Artifact artifact in m_artifacts)
                    if (artifact.Zone == zone)
                        artifacts.Add(artifact);
            }

            return artifacts;
        }

		/// <summary>
		/// Find all artifacts this scholar is studying.
		/// </summary>
		/// <param name="scholarID"></param>
		/// <returns></returns>
		public static ArrayList GetArtifactsFromScholar(String scholarID)
		{
            ArrayList artifacts = new ArrayList();

			lock (m_artifacts.SyncRoot)
			{
				String[] scholarIDs;
				foreach (Artifact artifact in m_artifacts)
				{
					scholarIDs = artifact.ScholarID.Split(';');
					foreach (String id in scholarIDs)
						if (String.Format("Scholar {0}", id) == scholarID)
							artifacts.Add(artifact);
				}
			}

			return artifacts;
		}

        /// <summary>
        /// Get a list of all versions for this artifact.
        /// </summary>
        /// <param name="artifactID"></param>
        /// <returns></returns>
        private static ArrayList GetArtifactVersions(String artifactID)
        {
            ArrayList versions = null;
            if (artifactID != null)
            {
                lock (m_artifactVersions.SyncRoot)
                    versions = (ArrayList)m_artifactVersions[artifactID];
            }

            return (versions == null) ? new ArrayList() : versions;
        }

        /// <summary>
        /// Create a hashtable containing all item templates that are valid for
        /// this class.
        /// </summary>
        /// <param name="artifactID"></param>
        /// <param name="charClass"></param>
        /// <returns></returns>
        public static Hashtable GetArtifactVersionsFromClass(String artifactID, 
            eCharacterClass charClass)
        {
            ArrayList allVersions = GetArtifactVersions(artifactID);
            Hashtable classVersions = new Hashtable();

            lock (allVersions.SyncRoot)
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

		#region Quests

		/// <summary>
		/// Find the quest ID for this artifact.
		/// </summary>
		/// <param name="artifactID"></param>
		/// <returns></returns>
		private static String GetQuestIDFromArtifactID(String artifactID)
		{
			lock (m_artifacts.SyncRoot)
			{
				foreach (Artifact artifact in m_artifacts)
					if (artifact.ArtifactID == artifactID)
						return artifact.QuestID;
			}

			return null;
		}

		/// <summary>
		/// Find the matching quest type for this artifact.
		/// </summary>
		/// <param name="artifactID"></param>
		/// <returns></returns>
		public static Type GetQuestTypeFromArtifactID(String artifactID)
		{
			String questID = ArtifactMgr.GetQuestIDFromArtifactID(artifactID);
			if (questID == null)
				return null;

			Type questType = null;
			foreach (Assembly asm in ScriptMgr.Scripts)
			{
				questType = asm.GetType(questID);
				if (questType != null)
					break;
			}

			if (questType == null)
				questType = Assembly.GetAssembly(typeof(GameServer)).GetType(questID);

			return questType;
		}

		/// <summary>
		/// Grant credit for an artifact.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="artifactID"></param>
		/// <returns></returns>
		public static bool GrantArtifactCredit(GamePlayer player, String artifactID)
		{
			if (player == null)
				return false;

			Type questType = GetQuestTypeFromArtifactID(artifactID);
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
        /// <param name="bookID">The title of the book.</param>
        /// <returns>The artifact that matches this book.</returns>
        public static Artifact GetArtifactFromBookID(String bookID)
        {
            ArrayList artifacts = new ArrayList();

            lock (m_artifacts.SyncRoot)
            {
                foreach (Artifact artifact in m_artifacts)
                    if (artifact.BookID == bookID)
                        return artifact;
            }

            return null;
        }

		/// <summary>
		/// Get the artifact for this scroll.
		/// </summary>
		/// <param name="scrollID"></param>
		/// <returns></returns>
		private static Artifact GetArtifactFromScrollID(String scrollID)
		{
			lock (m_artifacts.SyncRoot)
			{
                foreach (Artifact artifact in m_artifacts)
					if (artifact.ScrollID == scrollID)
						return artifact;
			}

			return null;
		}

		/// <summary>
		/// Returns the scroll's ID for this item or null, if it isn't
		/// a valid scroll.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		private static String GetScrollIDFromItem(InventoryItem item)
		{
			if (item == null)
				return null;

			String[] parts = item.Name.Split(',');
			if (parts.Length != 2)
				return null;

			return parts[0];
		}

		/// <summary>
		/// Check whether these 2 items can be combined.
		/// </summary>
		/// <param name="item1"></param>
		/// <param name="item2"></param>
		/// <returns></returns>
		public static bool CanCombine(InventoryItem item1, InventoryItem item2)
		{
			if (!IsArtifactScroll(item1) || !IsArtifactScroll(item2))
				return false;

			if (GetScrollIDFromItem(item1) != GetScrollIDFromItem(item2))
				return false;

			return ((item1.Flags & item2.Flags) == 0);
		}

		/// <summary>
		/// Whether or not the item is an artifact book.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool IsArtifactBook(InventoryItem item)
		{
			if (item.Object_Type != (int)eObjectType.Magical
				|| item.Item_Type != (int)eInventorySlot.FirstBackpack
				|| item.Flags != (int)Book.AllPages)
				return false;

            return (GetArtifactFromBookID(item.Name) != null);
		}

		/// <summary>
		/// Whether or not the item is an artifact scroll.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool IsArtifactScroll(InventoryItem item)
		{
			if (item.Object_Type != (int)eObjectType.Magical
				|| item.Item_Type != (int)eInventorySlot.FirstBackpack
				|| (item.Flags & (int)Book.AllPages) == 0
				|| item.Flags == (int)Book.AllPages)
				return false;

			String[] parts = item.Name.Split(',');

			if (parts.Length != 2)
				return false;

            return (GetArtifactFromScrollID(parts[0]) != null);
		}

		/// <summary>
		/// Combine 2 scrolls.
		/// </summary>
		/// <param name="scroll"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool CombineScrolls(InventoryItem scroll, InventoryItem item)
		{
			if (!CanCombine(scroll, item))
				return false;

			String scrollID = ArtifactMgr.GetScrollIDFromItem(scroll);
			if (scrollID == null)
				return false;

			Artifact artifact = ArtifactMgr.GetArtifactFromScrollID(scrollID);
			if (artifact == null)
				return false;

			scroll.Flags |= item.Flags;
			if ((scroll.Flags & (int)Book.AllPages) == (int)Book.AllPages)
				scroll.Model = 500;

			scroll.Name = CreateScrollName(scroll, artifact);
			return true;
		}

		/// <summary>
        /// Create a scroll from a particular book.
        /// </summary>
        /// <param name="bookID">Title of the book the scroll is a part of.</param>
        /// <param name="pageNumber">Scroll page number (1-3).</param>
        /// <returns>An item that can be picked up by a player (or null).</returns>
        public static GameInventoryItem CreateScroll(String bookID, int pageNumber)
        {
			if (pageNumber < 1 || pageNumber > 3)
				return null;

            Artifact artifact = GetArtifactFromBookID(bookID);
			if (artifact == null)
				return null;

            GameInventoryItem scroll = GameInventoryItem.CreateFromTemplate("artifact_scroll");
            if (scroll != null)
            {
                scroll.Item.Flags = 1 << (pageNumber - 1);
				scroll.Item.Name = CreateScrollName(scroll.Item, artifact);
				scroll.Name = scroll.Item.Name;
            }

            return scroll;
        }

		/// <summary>
		/// Creates the name of the scroll (or book).
		/// </summary>
		/// <param name="scroll">Scroll to get the name for.</param>
		/// <param name="artifact">The ID of the artifact this scroll is intended for.</param>
		/// <returns>The ingame name.</returns>
		private static String CreateScrollName(InventoryItem scroll, Artifact artifact)
		{
			switch (scroll.Flags & (int)Book.AllPages)
			{
				case (int)Book.AllPages:
					return artifact.BookID;
				case (int)Book.Page1:
					return artifact.ScrollID + ", page 1 of 3";
				case (int)Book.Page2:
                    return artifact.ScrollID + ", page 2 of 3";
				case (int)Book.Page3:
                    return artifact.ScrollID + ", page 3 of 3";
				case (int)Book.Page1 | (int)Book.Page2:
                    return artifact.ScrollID + ", pages 1 and 2";
				case (int)Book.Page1 | (int)Book.Page3:
                    return artifact.ScrollID + ", pages 1 and 3";
				case (int)Book.Page2 | (int)Book.Page3:
                    return artifact.ScrollID + ", pages 2 and 3";
				default:
					return "<undefined>";
			}
		}

		#endregion
    }
}
