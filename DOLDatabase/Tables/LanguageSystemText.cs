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
    [DataTable(TableName = "LanguageSystemText")]
    public class DBLanguageSystemText : DataObject
    {
        private string m_id;
        private string m_text;
        private string m_translationUnique;
        private string m_language;

        public DBLanguageSystemText() { }

        /// <summary>
        /// The translation id
        /// </summary>
        [DataElement(AllowDbNull = false, Index = true)]
        public string TranslationId
        {
            get { return m_id; }
            set
            {
                Dirty = true;
                m_id = value;
            }
        }

        /// <summary>
        /// The translated system text
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public string Text
        {
            get { return m_text; }
            set
            {
                Dirty = true;
                m_text = value;
            }
        }

        /// <summary>
        /// The translation unique column. This column is used to differentiate between two or more objects. For players
        /// we have 2 titles: male and female. For this, the unique column is used to select the correct gender.
        /// </summary>
        [DataElement(AllowDbNull = true, Index = true)]
        public string TranslationUnique
        {
            get { return m_translationUnique; }
            set
            {
                Dirty = true;
                m_translationUnique = value;
            }
        }

        /// <summary>
        /// The language
        /// </summary>
        [DataElement(AllowDbNull = false, Index = true)]
        public string Language
        {
            get { return m_language; }
            set
            {
                Dirty = true;
                m_language = value;
            }
        }
    }
}