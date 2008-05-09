using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOL.Database2.Providers
{
    public class NullDatabaseProvider : DatabaseProvider
    {
        private UInt64 m_currentID = 0;
        public override string ConnectionString
        {
            get{ return "/dev/null virtual implementation ;-)";}
            set { }
        }

        public override void OpenConnection()
        {
        }

        public override void CloseConnection()
        {
        }

        public override Queue<DatabaseObjectInformation> GetAllObjects()
        {
            return new Queue<DatabaseObjectInformation>();
        }

        public override DatabaseObjectInformation GetObjectByID(ulong ID)
        {
            return null;
        }

        public override DatabaseObjectInformation[] GetObjectsByType(Type Type)
        {
            return new DatabaseObjectInformation[0];
        }

        public override DatabaseObjectInformation[] GetObjectsByTypeName(string TypeName)
        {
            return new DatabaseObjectInformation[0];
        }
        /// <summary>
        /// Keying is not implemented - ok?
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        public override DatabaseObjectInformation[] GetObjectsByKey(object Key)
        {
            throw new NotImplementedException();
        }

        public override void InsertOrUpdateObjectData(ulong ObjectID, Type Type, System.IO.Stream Data)
        {
            return;
        }

        public override void InsertOrUpdateObjectData(ulong ObjectID, Type Type, System.IO.Stream Data, object[] Keys)
        {
            return;
        }

        public override ulong GetNewUniqueID()
        {
            return m_currentID++;
        }

        public override void DeleteObject(ulong ObjectID)
        {
            return;
        }
    }
}
