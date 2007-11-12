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

        private static Hashtable m_artifacts;
		private static Hashtable m_scrolls, m_books, m_quests;

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
            Artifact artifact;
			m_artifacts = new Hashtable();
			m_scrolls = new Hashtable();
			m_books = new Hashtable();
			m_quests = new Hashtable();
            for (int i = 0; i < dbo.Length; i++)
            {
                artifact = dbo[i] as Artifact;
                m_artifacts[artifact.ItemID] = artifact;

				if (!m_scrolls.Contains(artifact.ArtifactID))
					m_scrolls[artifact.ArtifactID] = artifact.ScrollID;
				if (!m_books.Contains(artifact.ArtifactID))
					m_books[artifact.ArtifactID] = artifact.BookID;
				if (!m_quests.Contains(artifact.ArtifactID))
					m_quests[artifact.ArtifactID] = artifact.QuestID;
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
                IDictionaryEnumerator artifactEnum = m_artifacts.GetEnumerator();
                while (artifactEnum.MoveNext())
                    if ((artifactEnum.Value as Artifact).Zone == zone)
                        artifacts.Add(artifactEnum.Value);
            }

            return artifacts;
        }

		#region Scrolls & Books

        /// <summary>
        /// Find the matching artifacts for this book.
        /// </summary>
        /// <param name="bookID">The title of the book.</param>
        /// <returns>A list of all the artifacts that match this book.</returns>
        private static ArrayList GetArtifactsFromBookID(String bookID)
        {
            ArrayList artifacts = new ArrayList();

            lock (m_artifacts.SyncRoot)
            {
                IDictionaryEnumerator artifactEnum = m_artifacts.GetEnumerator();
                while (artifactEnum.MoveNext())
                    if ((artifactEnum.Value as Artifact).BookID == bookID)
                        artifacts.Add(artifactEnum.Value);
            }

            return artifacts;
        }

		/// <summary>
		/// Get the artifact ID for this book.
		/// </summary>
		/// <param name="bookID"></param>
		/// <returns></returns>
		public static String GetArtifactIDFromBookID(String bookID)
		{
			lock (m_books.SyncRoot)
			{
				IDictionaryEnumerator bookEnum = m_books.GetEnumerator();
				while (bookEnum.MoveNext())
					if ((bookEnum.Value as String) == bookID)
						return bookEnum.Key as String;
			}
			return null;
		}

		/// <summary>
		/// Get the artifact ID for this scroll.
		/// </summary>
		/// <param name="scrollID"></param>
		/// <returns></returns>
		public static String GetArtifactIDFromScrollID(String scrollID)
		{
			lock (m_scrolls.SyncRoot)
			{
				IDictionaryEnumerator scrollEnum = m_scrolls.GetEnumerator();
				while (scrollEnum.MoveNext())
					if ((scrollEnum.Value as String) == scrollID)
						return scrollEnum.Key as String;
			}
			return null;
		}

		/// <summary>
		/// Returns the scroll's ID for this item or null, if it isn't
		/// a valid scroll.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static String GetScrollIDFromItem(InventoryItem item)
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

			lock (m_books.SyncRoot)
				return m_books.ContainsValue(item.Name);
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

			lock (m_scrolls.SyncRoot)
				return m_scrolls.ContainsValue(parts[0]);
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

			String artifactID = GetArtifactIDFromBookID(bookID);
			if (artifactID == null)
				return null;

			String scrollID = null;
			lock (m_scrolls.SyncRoot)
				scrollID = m_scrolls[artifactID] as String;
			if (scrollID == null)
				return null;

            GameInventoryItem scroll = GameInventoryItem.CreateFromTemplate("artifact_scroll");

            if (scroll != null)
            {
                scroll.Item.Flags = 1 << (pageNumber - 1);
				scroll.Item.Name = CreateScrollName(scroll.Item, artifactID);
				scroll.Name = scroll.Item.Name;
            }

            return scroll;
        }

		/// <summary>
		/// Creates the name of the scroll (or book).
		/// </summary>
		/// <param name="scroll">Scroll to get the name for.</param>
		/// <param name="artifactID">The ID of the artifact this scroll is intended for.</param>
		/// <returns>The ingame name.</returns>
		public static String CreateScrollName(InventoryItem scroll, String artifactID)
		{
			String scrollID, bookID;

			lock (m_scrolls.SyncRoot)
				scrollID = m_scrolls[artifactID] as String;
			lock (m_books.SyncRoot)
				bookID = m_books[artifactID] as String;

			switch (scroll.Flags & (int)Book.AllPages)
			{
				case (int)Book.AllPages:
					return bookID;
				case (int)Book.Page1:
					return scrollID + ", page 1 of 3";
				case (int)Book.Page2:
					return scrollID + ", page 2 of 3";
				case (int)Book.Page3:
					return scrollID + ", page 3 of 3";
				case (int)Book.Page1 | (int)Book.Page2:
					return scrollID + ", pages 1 and 2";
				case (int)Book.Page1 | (int)Book.Page3:
					return scrollID + ", pages 1 and 3";
				case (int)Book.Page2 | (int)Book.Page3:
					return scrollID + ", pages 2 and 3";
				default:
					return "<undefined>";
			}
		}

		#endregion
    }
}
