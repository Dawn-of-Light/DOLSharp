using System;
using System.Collections.Generic;
using System.Text;
using DOL.Database;
using DOL.Database.Attributes;

namespace DOL.Database
{
    [DataTable(TableName = "artifact_level")]
    public class ArtifactLevel : DataObject
    {
        string m_id_nb;
        byte m_level;
        long m_experience;
        public static bool m_autosave;

        public ArtifactLevel()
        {
            m_id_nb = "";
            m_level = 0;
            m_experience = 0;
            m_autosave = false;
        }

        override public bool AutoSave
        {
            get
            {
                return m_autosave;
            }
            set
            {
                m_autosave = value;
            }
        }

        [DataElement(AllowDbNull = false)]
        public string Id_nb
        {
            get { return m_id_nb; }
            set
            {
                Dirty = true;
                m_id_nb = value;
            }
        }

        [DataElement(AllowDbNull = false)]
        public byte Level
        {
            get { return m_level; }
            set
            {
                Dirty = true;
                m_level = value;
            }
        }

        [DataElement(AllowDbNull = false)]
        public long Experience
        {
            get { return m_experience; }
            set
            {
                Dirty = true;
                m_experience = value;
            }
        }
    }
}
