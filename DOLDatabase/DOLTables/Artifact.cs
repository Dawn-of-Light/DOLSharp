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
using DOL.Database.Attributes;

namespace DOL.Database
{
    /// <summary>
    /// An artifact.
    /// </summary>
	/// <author>Aredhel</author>
    [DataTable(TableName = "Artifact")]
    public class Artifact : DataObject
    {
        private String m_artifactID;
		private String m_version;
        private String m_itemID;
		private String m_scrollID;
		private String m_bookID;
		private String m_questID;
        private int m_reuseTimer;
		private UInt32[] m_experience;

        /// <summary>
        /// Create a new artifact object.
        /// </summary>
        public Artifact()
            : base() 
        {
			m_experience = new UInt32[10];
        }

		/// <summary>
		/// Returns the XP needed for the given level.
		/// </summary>
		/// <param name="level">Level.</param>
		/// <returns>Experience required for this level.</returns>
		public UInt32 GetXPForLevel(int level)
		{
			if (level < 1 || level > 10)
				throw new Exception("Invalid level");
			return m_experience[level - 1];
		}

        /// <summary>
        /// Whether to auto-save this object or not.
        /// </summary>
        public override bool AutoSave
        {
            get { return false; }
            set { }
        }

        /// <summary>
        /// The artifact ID.
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public String ArtifactID
        {
            get { return m_artifactID; }
            set 
            {
                Dirty = true;
                m_artifactID = value; 
            }
        }

		/// <summary>
		/// The version of the artifact (1H/2H, cloth, chain, etc.)
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public String Version
		{
			get { return m_version; }
			set
			{
				Dirty = true;
				m_version = value;
			}
		}

        /// <summary>
        /// The item ID for the artifact.
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public String ItemID
        {
            get { return m_itemID; }
            set 
            {
                Dirty = true;
                m_itemID = value;
            }
        }

		/// <summary>
		/// The ID for the scrolls that make up the book for this 
		/// artifact.
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public String ScrollID
		{
			get { return m_scrollID; }
			set
			{
				Dirty = true;
				m_scrollID = value;
			}
		}

		/// <summary>
		/// The ID for the book required to unlock this artifact.
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public String BookID
		{
			get { return m_bookID; }
			set
			{
				Dirty = true;
				m_bookID = value;
			}
		}

		/// <summary>
		/// The ID for the quest that needs to be completed in order
		/// to unlock this artifact.
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public String QuestID
		{
			get { return m_questID; }
			set
			{
				Dirty = true;
				m_questID = value;
			}
		}
		/// <summary>
		/// The reuse timer for the artifact.
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public int ReuseTimer
		{
			get { return m_reuseTimer; }
			set
			{
				Dirty = true;
				m_reuseTimer = value;
			}
		}

		/// <summary>
		/// Experience required for level 1.
		/// </summary>
		/// <returns>The experience required for this level.</returns>
		[DataElement(AllowDbNull = false)]
		public UInt32 Level1Experience
		{
			get { return m_experience[0]; }
			set { m_experience[0] = value; }
		}

		/// <summary>
		/// Experience required for level 2.
		/// </summary>
		/// <returns>The experience required for this level.</returns>
		[DataElement(AllowDbNull = false)]
		public UInt32 Level2Experience
		{
			get { return m_experience[1]; }
			set { m_experience[1] = value; }
		}

		/// <summary>
		/// Experience required for level 3.
		/// </summary>
		/// <returns>The experience required for this level.</returns>
		[DataElement(AllowDbNull = false)]
		public UInt32 Level3Experience
		{
			get { return m_experience[2]; }
			set { m_experience[2] = value; }
		}

		/// <summary>
		/// Experience required for level 4.
		/// </summary>
		/// <returns>The experience required for this level.</returns>
		[DataElement(AllowDbNull = false)]
		public UInt32 Level4Experience
		{
			get { return m_experience[3]; }
			set { m_experience[3] = value; }
		}

		/// <summary>
		/// Experience required for level 5.
		/// </summary>
		/// <returns>The experience required for this level.</returns>
		[DataElement(AllowDbNull = false)]
		public UInt32 Level5Experience
		{
			get { return m_experience[4]; }
			set { m_experience[4] = value; }
		}

        /// <summary>
        /// Experience required for level 6.
        /// </summary>
        /// <returns>The experience required for this level.</returns>
		[DataElement(AllowDbNull = false)]
		public UInt32 Level6Experience
		{
			get { return m_experience[5]; }
			set { m_experience[5] = value; }
		}

		/// <summary>
		/// Experience required for level 7.
		/// </summary>
		/// <returns>The experience required for this level.</returns>
		[DataElement(AllowDbNull = false)]
		public UInt32 Level7Experience
		{
			get { return m_experience[6]; }
			set { m_experience[6] = value; }
		}

		/// <summary>
		/// Experience required for level 8.
		/// </summary>
		/// <returns>The experience required for this level.</returns>
		[DataElement(AllowDbNull = false)]
		public UInt32 Level8Experience
		{
			get { return m_experience[7]; }
			set { m_experience[7] = value; }
		}

		/// <summary>
		/// Experience required for level 9.
		/// </summary>
		/// <returns>The experience required for this level.</returns>
		[DataElement(AllowDbNull = false)]
		public UInt32 Level9Experience
		{
			get { return m_experience[8]; }
			set { m_experience[8] = value; }
		}

		/// <summary>
		/// Experience required for level 10.
		/// </summary>
		/// <returns>The experience required for this level.</returns>
		[DataElement(AllowDbNull = false)]
		public UInt32 Level10Experience
		{
			get { return m_experience[9]; }
			set { m_experience[9] = value; }
		}
    }
}
