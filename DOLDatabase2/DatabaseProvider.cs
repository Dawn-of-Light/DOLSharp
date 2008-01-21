using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.IO;
using log4net;
namespace DOL.Database2
{
    /// <summary>
    /// Provides a DBMS-near format for saving Object information
    /// </summary>
    class DatabaseObjectInformation
    {
        public UInt64 UniqueID; 
        public string TypeName;
        public Stream Data;
    }
    /// <summary>
    /// Provides Database Service
    /// </summary>
    abstract class DatabaseProvider
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        protected bool m_Connected;
        protected string m_ConnectionString;
        /// <summary>
        /// Connection state. If this is set to true , the DatabaseProvider should establish a connection.
        /// </summary>
        public bool Connected
        {
            get{return m_Connected;}
            set{
                if(value && !m_Connected)
                    OpenConnection();
                if(!value && m_Connected)
                    CloseConnection();
            }                    
        }
        public string ConnectionString
        {
            get{ return m_ConnectionString;}
            set{ SetConnectionString(value); }
        }
        public abstract void OpenConnection();
        public abstract void CloseConnection();
        public abstract void SetConnectionString(string ConnectionString);
        public abstract Queue<DatabaseObjectInformation> GetAllObjects();
        public abstract DatabaseObjectInformation GetObjectByID(UInt64 ID);
        public abstract DatabaseObjectInformation[] GetObjectsByType(Type Type);
        public abstract DatabaseObjectInformation[] GetObjectsByTypeName(string TypeName);
        public abstract DatabaseObjectInformation[] GetObjectsByKey(object Key);
        public abstract void InsertOrUpdateObjectData(UInt64 ObjectID, Type Type, Stream Data );
        public abstract UInt64 GetNewUniqueID();
        public abstract void DeleteObject(UInt64 ObjectID);
    }
}
