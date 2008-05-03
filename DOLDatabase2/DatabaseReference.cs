using System;
using System.Runtime.Serialization;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using log4net;
namespace DOL.Database2
{
    /// <summary>
    /// Required to refer to an Object WITHOUT storing its ID field manually
    /// </summary>
    [Serializable]
    class DatabaseReference <T> where T : DatabaseObject
    {
        //TODO : tune so the ID is only stored during serialization, would save some memory
        private static ILog log =  LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        [NonSerialized]
        private static FieldInfo m_IDInfo = typeof(DatabaseObject).GetField("ID"); // The Unique ID
        [NonSerialized]
        private DatabaseObject m_target = null;
        private UInt64 m_targetID = 0;
        public T Target
        {
            get
            {
                return m_target as T;
            }
            set
            {
                m_target = value as DatabaseObject;
                m_targetID = (ulong)m_IDInfo.GetValue(m_target);
            }
        }
        public DatabaseReference (T Object)
        {
            m_target =  Object as DatabaseObject;
            m_targetID = m_target.ID;
        }
        [OnDeserialized]
        public void RestoreReference()
        {
            m_target = DatabaseLayer.Instance.GetDatabaseObjectFromID(m_targetID);
        }
        
    }
}
