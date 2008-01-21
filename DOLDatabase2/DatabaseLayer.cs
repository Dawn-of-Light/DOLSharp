using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using log4net;



namespace DOL.Database2
{
    public class DatabaseLayer 
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
#region Singleton
		/// <summary>
        /// The Singleton Instance
        /// </summary>
        private static volatile DatabaseLayer m_instance = null;
        /// <summary>
        /// Singleton Lock Object
        /// </summary>
        private static object syncRoot = new Object();
        private DatabaseLayer() { }
        public static DatabaseLayer Instance
        {
            get
            {
                if (m_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (m_instance == null)
                            m_instance = new DatabaseLayer();
                    }
                }

                return m_instance;
            }
        } 
	#endregion
        private bool m_connected = false;
        private DatabaseProvider m_provider = null;
        public readonly Dictionary<UInt64,DatabaseObject> DatabaseObjects = new Dictionary<UInt64,DatabaseObject>();
        #region RestoreWorldState
        public void RestoreWorldState()
        {
            Queue <DatabaseObjectInformation> DatabaseInformation =  m_provider.GetAllObjects();
            IFormatter formatter = new BinaryFormatter();
            foreach(DatabaseObjectInformation objinf in DatabaseInformation)
            {
                Type ObjectType = null ;
                foreach(Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    ObjectType = asm.GetType(objinf.TypeName);
                    if(ObjectType != null)
                        break;
                }
                if(ObjectType == null)
                {
                    log.Error("Type "+objinf.TypeName+" referenced by ObjectID "+objinf.UniqueID+" was not found");
                }
                else
                {
                    DatabaseObject Object;
                    try
                    {
                        Object = (DatabaseObject)formatter.Deserialize(objinf.Data);
                        DatabaseObjects.Add(Object.ID, Object);
                    }
                    catch(SerializationException e)
                    {
                        log.Error("Could not deserialize "+objinf.TypeName+" with ObjectID "+objinf.UniqueID,e);
                    }                    
                }
            }
        }
        #endregion
        #region SaveWorldState,SaveObject

        public void SaveWorldState()
        {
            foreach (DatabaseObject obj in DatabaseObjects.Values)
            {
                SaveObject(obj);
            }
        }
        /// <summary>
        /// Saves a DatabaseObject
        /// </summary>
        /// <param name="Object">DatabaseObject to save</param>
        public void SaveObject(DatabaseObject Object)
        {
            try
            {
                //TODO: Tuning would be possible here , think about passing an optional BinaryFormatter 
                IFormatter formatter = new BinaryFormatter();
                //TODO: Tuning possible here , keep the MemoryStream
                MemoryStream memstream = new MemoryStream();
                formatter.Serialize(memstream, Object);
                m_provider.InsertOrUpdateObjectData(Object.ID, Object.GetType(), memstream);
            }
            catch (SerializationException e)
            {
                if (log.IsErrorEnabled)
                    log.Error("Could not serialize DatabaseObject with Key" + Object.ID, e);
            }
        }
#endregion
        #region Select
        public DatabaseObject SelectObject(Type type, string MemberName, object Value)
        {
            FieldInfo field = type.GetField(MemberName);
            PropertyInfo property = type.GetProperty(MemberName);
            if (property != null)
            {
                return SelectObject(property, Value);
            }
            else if (field != null)
            {
                return SelectObject(field, Value);
            }
            else
            {
                throw new PropertyFieldNotFoundException(MemberName, type);
            }
        }
        public DatabaseObject SelectObject(FieldInfo field, object Value)
        {
            foreach (DatabaseObject dbo in DatabaseObjects.Values)
            {
                if (field.DeclaringType == dbo.GetType())
                {
                    if (field.GetValue(dbo) == Value)
                    {
                        return dbo;
                    }
                }
            }
            return null;
        }
        public DatabaseObject SelectObject(PropertyInfo property, object Value)
        {
            foreach (DatabaseObject dbo in DatabaseObjects.Values)
            {
                if (property.DeclaringType == dbo.GetType())
                {
                    if (property.GetValue(dbo,null) == Value)
                    {
                        return dbo;
                    }
                }
            }
            return null;
        }
        public IEnumerable<DatabaseObject> SelectObjects(Type type)
        {
            LinkedList<DatabaseObject> list = new LinkedList<DatabaseObject>();
            foreach (DatabaseObject dbo in DatabaseObjects.Values)
            {
                if(dbo.GetType() == type)
                {
                    list.AddLast(dbo);
                }
            }
            return list;
        }
        public IEnumerable<DatabaseObject> SelectObjects(Type type,string MemberName,object value)
        {
            FieldInfo field = type.GetField(MemberName);
            PropertyInfo property = type.GetProperty(MemberName);
            if (property != null)
            {
                return SelectObjects(property, value);
            }
            else if (field != null)
            {
                return SelectObjects(field, value);
            }
            throw new PropertyFieldNotFoundException(MemberName, type);
        }
        public IEnumerable<DatabaseObject> SelectObjects(PropertyInfo info,object value)
        {
            LinkedList<DatabaseObject> list = new LinkedList<DatabaseObject>();
            foreach (DatabaseObject dbo in DatabaseObjects.Values)
            {
                if (dbo.GetType() == info.DeclaringType)
                {
                    if(info.GetValue(dbo,null) == value)
                        list.AddLast(dbo);
                }
            }
            return list;
        }
        public IEnumerable<DatabaseObject> SelectObjects(FieldInfo info, object value)
        {
            LinkedList<DatabaseObject> list = new LinkedList<DatabaseObject>();
            foreach (DatabaseObject dbo in DatabaseObjects.Values)
            {
                if (dbo.GetType() == info.DeclaringType)
                {
                    if (info.GetValue(dbo) == value)
                        list.AddLast(dbo);
                }
            }
            return list;
        }
        #endregion
        public UInt64 GetNewUniqueID()
        {
            return m_provider.GetNewUniqueID();
        }
        public void DeleteObject(DatabaseObject Object)
        {
            DatabaseObjects.Remove(Object.ID);
            m_provider.DeleteObject(Object.ID);
        }
        
    }
}
