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
    [DataTable(TableName = "LanguageNPC")]
    public class DBLanguageNPC : LanguageDataObject
    {
        #region Variables
        private string m_name = "";
        private string m_suffix = "";
        private string m_guildName = "";
        private string m_examineArticle = "";
        private string m_messageArticle = "";
        #endregion Variables

        public DBLanguageNPC()
            : base() { }

        #region Properties
        public override eTranslationIdentifier TranslationIdentifier
        {
            get { return eTranslationIdentifier.eNPC; }
        }

        /// <summary>
        /// Gets or sets the translated name.
        /// </summary>
        [DataElement(AllowDbNull = true)]
        public string Name
        {
            get { return m_name; }
            set
            {
                Dirty = true;
                m_name = value;
            }
        }

        /// <summary>
        /// Gets or sets the name suffix (currently used by necromancer pets).
        /// 
        /// The XYZ spell is no longer in the Death Servant's queue.
        /// 
        /// 's = the suffix.
        /// </summary>
        [DataElement(AllowDbNull = true)]
        public string Suffix
        {
            get { return m_suffix; }
            set
            {
                Dirty = true;
                m_suffix = value;
            }
        }

        /// <summary>
        /// Gets or sets the translated guild name.
        /// </summary>
        [DataElement(AllowDbNull = true)]
        public string GuildName
        {
            get { return m_guildName; }
            set
            {
                Dirty = true;
                m_guildName = value;
            }
        }

        /// <summary>
        /// Gets or sets the translated examine article.
        /// 
        /// You examine the Tree.
        /// 
        /// the = the examine article.
        /// </summary>
        [DataElement(AllowDbNull = true)]
        public string ExamineArticle
        {
            get { return m_examineArticle; }
            set
            {
                Dirty = true;
                m_examineArticle = value;
            }
        }

        /// <summary>
        /// Gets or sets the translated message article.
        /// 
        /// GamePlayer has been killed by a Tree.
        /// 
        /// a = the message article.
        /// </summary>
        [DataElement(AllowDbNull = true)]
        public string MessageArticle
        {
            get { return m_messageArticle; }
            set
            {
                Dirty = true;
                m_messageArticle = value;
            }
        }
        #endregion Properties
    }
}