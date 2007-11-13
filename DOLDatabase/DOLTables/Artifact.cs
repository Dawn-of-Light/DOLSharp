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
		private int m_levelReq;
		private String m_scrollID;
		private String m_bookID;
		private String m_questID;
        private String m_zone;
		private String m_scholarID;
        private int m_reuseTimer;
        private int m_xpRate;

        /// <summary>
        /// Create a new artifact object.
        /// </summary>
        public Artifact()
            : base() { }

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
		/// The level requirement for the artifact.
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public int LevelRequirement
		{
			get { return m_levelReq; }
			set
			{
				Dirty = true;
				m_levelReq = value;
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
        /// The zone this artifact belongs to.
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public String Zone
        {
            get { return m_zone; }
            set
            {
                Dirty = true;
                m_zone = value;
            }
        }

		/// <summary>
		/// The scholar(s) studying this artifact.
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public String ScholarID
		{
			get { return m_scholarID; }
			set
			{
				Dirty = true;
				m_scholarID = value;
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
        /// The rate at which this artifact gains xp (in percent).
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public int XPRate
        {
            get { return m_xpRate; }
            set
            {
                Dirty = true;
                m_xpRate = value;
            }
        }
    }
}
