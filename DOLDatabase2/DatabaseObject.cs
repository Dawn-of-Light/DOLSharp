using System;
using System.Collections.Generic;
using System.Text;

namespace DOL.Database2
{
    [Serializable]
    public class DatabaseObject : IDatabaseObject
    {
        public DatabaseObject()
        {
            m_ID = DatabaseLayer.Instance.GetNewUniqueID();
        }
        private  UInt64 m_ID = 0;
        [NonSerialized]
        private bool m_writetodatabase = false;
        protected bool m_Dirty;
        public bool Dirty
        {
            get { return m_Dirty; }
        }
        /// <summary>
        /// Contains the DatabaseObjects UniqueID
        /// </summary>
        public UInt64 ObjectId
        {
            get { return m_ID; }
        }
        public UInt64 ID
        {
            get { return m_ID; }
        }
        /// <summary>
        /// If this is set  to  false the object will not be saved to the persistant storage.
        /// </summary>
        public bool WriteToDatabase
        {
            get { return (m_writetodatabase && m_Dirty); }
            set { m_writetodatabase = value; }
        }
        /// <summary>
        /// Mostly for compatibility
        /// </summary>
        public bool AutoSave
        {
            get { return m_writetodatabase; }
            set { m_writetodatabase = value; }
        }
        /// <summary>
        /// Saves this object into database
        /// </summary>
        public void Save()
        {
            if(m_writetodatabase)
                DatabaseLayer.Instance.SaveObject(this);
        }
        public void DeleteDB()
        {
        }
    }
}
