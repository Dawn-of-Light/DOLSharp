using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DOL.Database2;
namespace SQLiteDBProvider
{
    public class SQLLiteDBProvider : DatabaseProvider
    {
        private string m_ConnectionString = "DOL.sqlite";
        public override string ConnectionString
        {
            get
            {
                return m_ConnectionString;
            }
            set
            {
                m_ConnectionString = value;
            }
        }

        public override void OpenConnection()
        {
            throw new NotImplementedException();
        }

        public override void CloseConnection()
        {
            throw new NotImplementedException();
        }

        public override Queue<DatabaseObjectInformation> GetAllObjects()
        {
            throw new NotImplementedException();
        }

        public override DatabaseObjectInformation GetObjectByID(ulong ID)
        {
            throw new NotImplementedException();
        }

        public override Queue<DatabaseObjectInformation> GetObjectsByTypeName(string TypeName)
        {
            throw new NotImplementedException();
        }

        public override void InsertOrUpdateObjectData(ulong ObjectID, Type Type, System.IO.Stream Data)
        {
            throw new NotImplementedException();
        }

        public override ulong GetNewUniqueID()
        {
            throw new NotImplementedException();
        }

        public override void DeleteObject(ulong ObjectID)
        {
            throw new NotImplementedException();
        }
    }
}
