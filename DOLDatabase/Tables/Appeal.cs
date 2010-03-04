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

using DOL.Database;
using DOL.Database.Attributes;

    /// <summary>
    /// Saves an Appeal
    /// </summary>
    [DataTable(TableName = "Appeal")]
    public class DBAppeal : DataObject
    {
        private string m_name;
        private int m_severity;
        private string m_status;
        private string m_timestamp;
        private string m_text;
        private bool m_autoSave;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Player's name</param>
        /// <param name="severity">The severity of the appeal (low, medium, high, critical)</param>
        /// <param name="status">The status of the appeal (Open, Being Helped)</param>
        /// <param name="timestamp">When the appeal was first created</param>
        /// <param name="text">Content of the appeal (text)</param>
        public DBAppeal()
        {
            m_name = "";
            m_severity = 0;
            m_status = "";
            m_timestamp = "";
            m_text = "";
        }

        public DBAppeal(string name, int severity, string status, string timestamp, string text)
        {
            m_name = name;
            m_severity = severity;
            m_status = status;
            m_timestamp = timestamp;
            m_text = text;
        }

        override public bool AutoSave
        {
            get { return m_autoSave; }
            set { m_autoSave = value; }
        }


        [DataElement(AllowDbNull = false)]
        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        [DataElement(AllowDbNull = false)]
        public int Severity
        {
            get { return m_severity; }
            set { m_severity = value; }
        }

        public string SeverityToName
        {
            get
            {
                switch (Severity)
                {
                    case 1:
                        return "Low";
                    case 2:
                        return "Medium";
                    case 3:
                        return "High";
                    case 4:
                        return "Critical";
                    default:
                        return "none";
                }
            }
            set { }
        }

        [DataElement(AllowDbNull = false)]
        public string Status
        {
            get { return m_status; }
            set { m_status = value; }
        }

        [DataElement(AllowDbNull = false)]
        public string Timestamp
        {
            get { return m_timestamp; }
            set { m_timestamp = value; }
        }
        [DataElement(AllowDbNull = false)]
        public string Text
        {
            get { return m_text; }
            set { m_text = value; }
        }

    }
