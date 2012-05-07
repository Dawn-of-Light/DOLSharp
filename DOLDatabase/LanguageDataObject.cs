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

using DOL.Database.Attributes;

namespace DOL.Database
{
    public abstract class LanguageDataObject : DataObject
    {
        #region Enums
        public enum eTranslationIdentifier
            : byte
        {
            eArea = 0,
            eDoor = 1,
            eItem = 2,
            eNPC = 3,
            eObject = 4,
            eSystem = 5,
            eZone = 6
        }
        #endregion Enums

        #region Variables
        private string m_lng = "";
        private string m_tid = "";
        private string m_tag = "";
        #endregion Variables

        public LanguageDataObject() { }

        #region Properties
        public abstract eTranslationIdentifier TranslationIdentifier
        {
            get;
        }

        /// <summary>
        /// Gets or sets the translation id.
        /// </summary>
        [DataElement(AllowDbNull = false, Index = true)]
        public string TranslationId
        {
            get { return m_tid; }
            set
            {
                Dirty = true;
                m_tid = value;
            }
        }

        /// <summary>
        /// Gets or sets the language.
        /// </summary>
        [DataElement(AllowDbNull = false, Index = true)]
        public string Language
        {
            get { return m_lng; }
            set
            {
                Dirty = true;
                m_lng = value.ToUpper();
            }
        }

        /// <summary>
        /// Gets or sets costum data of / for the database row. Can be used to sort rows.
        /// </summary>
        [DataElement(AllowDbNull = true)]
        public string Tag
        {
            get { return m_tag; }
            set
            {
                Dirty = true;
                m_tag = value;
            }
        }
        #endregion Properties
    }
}