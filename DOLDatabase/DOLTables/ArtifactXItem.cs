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
    /// Relation between artifacts and the actual items.
    /// </summary>
    /// <author>Aredhel</author>
    [DataTable(TableName = "ArtifactXItem")]
    class ArtifactXItem : DataObject
    {
        private String m_artifactID;
        private String m_itemID;
        private String m_version;

        /// <summary>
        /// Create a new artifact/item relation.
        /// </summary>
        public ArtifactXItem()
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
        /// The item ID.
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
        /// The artifact ID.
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
    }
}
