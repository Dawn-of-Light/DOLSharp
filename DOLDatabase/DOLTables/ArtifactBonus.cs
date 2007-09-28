using System;
using System.Collections.Generic;
using System.Text;
using DOL.Database;
using DOL.Database.Attributes;

namespace DOL.Database
{
    [DataTable(TableName = "artifact_bonuses")]
    public class ArtifactBonus : DataObject
    {
        string m_id_nb;
        byte m_level;
        short m_bonustype;
        short m_bonusamount;
        public static bool m_autosave;

        public ArtifactBonus()
        {
            m_id_nb = "";
            m_level = 0;
            m_bonustype = -1;
            m_bonusamount = -1;
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
        public short BonusType
        {
            get { return m_bonustype; }
            set
            {
                Dirty = true;
                m_bonustype = value;
            }
        }
        [DataElement(AllowDbNull = false)]
        public short BonusAmount
        {
            get { return m_bonusamount; }
            set
            {
                Dirty = true;
                m_bonusamount = value;
            }
        }
    }
}
