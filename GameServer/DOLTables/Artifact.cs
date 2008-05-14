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


namespace DOL.Database2
{
    /// <summary>
    /// An artifact.
    /// </summary>
	/// <author>Aredhel</author>
    [Serializable]//TableName = "Artifact")]
    public class Artifact : DatabaseObject
    {
        private String m_artifactID;
		private String m_encounterID;
		private String m_questID;
        private String m_zone;
		private String m_scholarID;
        private int m_reuseTimer;
        private int m_xpRate;
		private String m_bookID;
		private int m_bookModel;
		private String m_scroll1, m_scroll2, m_scroll3;
		private String m_scroll12, m_scroll13, m_scroll23;
		private int m_scrollModel1, m_scrollModel2;
		private int m_scrollLevel;
		private String m_messageUse;
		private String m_messageCombineScrolls, m_messageCombineBook;
		private String m_messageReceiveScrolls, m_messageReceiveBook;

        /// <summary>
        /// Create a new artifact object.
        /// </summary>
        public Artifact()
            : base() { }



        /// <summary>
        /// The artifact ID.
        /// </summary>
        
        public String ArtifactID
        {
            get { return m_artifactID; }
            set 
            {
                m_Dirty = true;
                m_artifactID = value; 
            }
        }

		/// <summary>
		/// The ID for the encounter required to get the quest for 
		/// this artifact.
		/// </summary>
		
		public String EncounterID
		{
			get { return m_encounterID; }
			set
			{
				m_Dirty = true;
				m_encounterID = value;
			}
		}

		/// <summary>
		/// The ID for the quest that needs to be completed in order
		/// to unlock this artifact.
		/// </summary>
		
		public String QuestID
		{
			get { return m_questID; }
			set
			{
				m_Dirty = true;
				m_questID = value;
			}
		}

        /// <summary>
        /// The zone this artifact belongs to.
        /// </summary>
        
        public String Zone
        {
            get { return m_zone; }
            set
            {
                m_Dirty = true;
                m_zone = value;
            }
        }

		/// <summary>
		/// The scholar(s) studying this artifact.
		/// </summary>
		
		public String ScholarID
		{
			get { return m_scholarID; }
			set
			{
				m_Dirty = true;
				m_scholarID = value;
			}
		}

		/// <summary>
		/// The reuse timer for the artifact.
		/// </summary>
		
		public int ReuseTimer
		{
			get { return m_reuseTimer; }
			set
			{
				m_Dirty = true;
				m_reuseTimer = value;
			}
		}

        /// <summary>
        /// The rate at which this artifact gains xp (in percent).
        /// </summary>
        
        public int XPRate
        {
            get { return m_xpRate; }
            set
            {
                m_Dirty = true;
                m_xpRate = value;
            }
        }

		/// <summary>
		/// The book ID.
		/// </summary>
		
		public String BookID
		{
			get { return m_bookID; }
			set
			{
				m_Dirty = true;
				m_bookID = value;
			}
		}

		/// <summary>
		/// The book model (icon).
		/// </summary>
		
		public int BookModel
		{
			get { return m_bookModel; }
			set
			{
				m_Dirty = true;
				m_bookModel = value;
			}
		}

		/// <summary>
		/// Scroll 1 name.
		/// </summary>
		
		public String Scroll1
		{
			get { return m_scroll1; }
			set
			{
				m_Dirty = true;
				m_scroll1 = value;
			}
		}

		/// <summary>
		/// Scroll 2 name.
		/// </summary>
		
		public String Scroll2
		{
			get { return m_scroll2; }
			set
			{
				m_Dirty = true;
				m_scroll2 = value;
			}
		}

		/// <summary>
		/// Scroll 3 name.
		/// </summary>
		
		public String Scroll3
		{
			get { return m_scroll3; }
			set
			{
				m_Dirty = true;
				m_scroll3 = value;
			}
		}

		/// <summary>
		/// Scrolls 1+2 name.
		/// </summary>
		
		public String Scroll12
		{
			get { return m_scroll12; }
			set
			{
				m_Dirty = true;
				m_scroll12 = value;
			}
		}

		/// <summary>
		/// Scrolls 1+3 name.
		/// </summary>
		
		public String Scroll13
		{
			get { return m_scroll13; }
			set
			{
				m_Dirty = true;
				m_scroll13 = value;
			}
		}

		/// <summary>
		/// Scrolls 2+3 name.
		/// </summary>
		
		public String Scroll23
		{
			get { return m_scroll23; }
			set
			{
				m_Dirty = true;
				m_scroll23 = value;
			}
		}

		/// <summary>
		/// Scroll model (icon) for a single scroll.
		/// </summary>
		
		public int ScrollModel1
		{
			get { return m_scrollModel1; }
			set
			{
				m_Dirty = true;
				m_scrollModel1 = value;
			}
		}

		/// <summary>
		/// Scroll model (icon) for 2 combined scrolls.
		/// </summary>
		
		public int ScrollModel2
		{
			get { return m_scrollModel2; }
			set
			{
				m_Dirty = true;
				m_scrollModel2 = value;
			}
		}

		/// <summary>
		/// Scroll level.
		/// </summary>
		
		public int ScrollLevel
		{
			get { return m_scrollLevel; }
			set
			{
				m_Dirty = true;
				m_scrollLevel = value;
			}
		}

		/// <summary>
		/// Message issued when scroll is used.
		/// </summary>
		
		public String MessageUse
		{
			get { return m_messageUse; }
			set
			{
				m_Dirty = true;
				m_messageUse = value;
			}
		}

		/// <summary>
		/// Message issued when scrolls are combined.
		/// </summary>
		
		public String MessageCombineScrolls
		{
			get { return m_messageCombineScrolls; }
			set
			{
				m_Dirty = true;
				m_messageCombineScrolls = value;
			}
		}

		/// <summary>
		/// Message issued when book is combined.
		/// </summary>
		
		public String MessageCombineBook
		{
			get { return m_messageCombineBook; }
			set
			{
				m_Dirty = true;
				m_messageCombineBook = value;
			}
		}

		/// <summary>
		/// Message issued when player receives scrolls.
		/// </summary>
		
		public String MessageReceiveScrolls
		{
			get { return m_messageReceiveScrolls; }
			set
			{
				m_Dirty = true;
				m_messageReceiveScrolls = value;
			}
		}

		/// <summary>
		/// Message issued when player receives the book.
		/// </summary>
		
		public String MessageReceiveBook
		{
			get { return m_messageReceiveBook; }
			set
			{
				m_Dirty = true;
				m_messageReceiveBook = value;
			}
		}
    }
}
