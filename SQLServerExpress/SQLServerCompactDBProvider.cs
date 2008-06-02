using System;
using System.Data.SqlServerCe;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DOL.Database2;
namespace DOL.Database2.Providers.SQLExpress
{
    class SQLServerCompactDBProvider : DatabaseProvider
    {
        SqlCeConnection m_connection  = new SqlCeConnection();
        SqlCeCommandBuilder m_commandbuilder = new SqlCeCommandBuilder();
        private UInt64 m_currentID = 1;
        private int m_currentTypeID = 1;
        Dictionary<int,string> m_types = new Dictionary<int,string>();
        Dictionary<string, int> m_typeids = new Dictionary<string, int>();
        
        public SQLServerCompactDBProvider()
        {
        }
        public override string ConnectionString
        {
            get
            {
                return m_connection.ConnectionString;
            }
            set
            {
                m_connection.ConnectionString = value;
            }
        }

        public override void OpenConnection()
        {
            m_connection.Open();
            SqlCeCommand Command = new SqlCeCommand("CREATE TABLE IF NOT EXISTS objects"+
              "(ObjectID INTEGER(4) PRIMARY KEY,TypeID INTEGER,Data BLOB)",m_connection);
            Command.ExecuteNonQuery();
            SqlCeCommand Command2 = new SqlCeCommand("CREATE TABLE IF NOT EXISTS types" +
              "(TypeID INTEGER PRIMARY KEY,Name Text UNIQUE)", m_connection);
            Command2.ExecuteNonQuery();
            m_Connected = true;
            SqlCeCommand Command3 = new SqlCeCommand("SELECT count(*) FROM objects", m_connection);
            object m_rawCount = Command3.ExecuteScalar();
           m_currentID = (Convert.ToUInt64(m_rawCount) + 1);
            UpdateTypes();
        }

        public override void CloseConnection()
        {
            if(m_Connected)
            {
                m_connection.Close();
                m_Connected = false;
           }
        }

        public override Queue<DatabaseObjectInformation> GetAllObjects()
        {
            if (!m_Connected)
                return null;
            Queue<DatabaseObjectInformation> dbos = new Queue<DatabaseObjectInformation>();
            DatabaseObjectInformation dbo;
            SqlCeCommand Command = new SqlCeCommand("SELECT ObjectID,TypeID,Data from objects",m_connection);
            SqlCeDataReader rdr = Command.ExecuteReader();
            if(!rdr.HasRows)
                return dbos;
            rdr.Read();
            do 
            {
                int length = (int)rdr.GetBytes(2, 0, null, 0, 0);
                byte[] buf = new byte[length];
                dbo = new DatabaseObjectInformation();
                dbo.UniqueID = (UInt64)rdr.GetInt64(0);
                rdr.GetBytes(2, 0, buf, 0, length);
                dbo.Data = new MemoryStream(buf);
                dbo.TypeName = m_types[rdr.GetInt32(1)];
                dbos.Enqueue(dbo);
            } while (rdr.Read());
            return dbos;
        }

        public override DatabaseObjectInformation GetObjectByID(ulong ID)
        {
            if (!m_Connected)
                return null;
            DatabaseObjectInformation dbo;
            SqlCeCommand Command = new SqlCeCommand("SELECT ObjectID,TypeID,Data from objects WHERE ObjectID = @ObjectID",m_connection);
            Command.Parameters.Add(new SqlCeParameter("ObjectID", ID));
            SqlCeDataReader rdr = Command.ExecuteReader();
            if (!rdr.HasRows)
                return null;
            rdr.Read();
            int length = (int)rdr.GetBytes(2, 0, null, 0, 0);
            byte[] buf = new byte[length];
            dbo = new DatabaseObjectInformation();
            dbo.UniqueID = (UInt64)rdr.GetInt64(0);
            rdr.GetBytes(2, 0, buf, 0, length);
            dbo.Data = new MemoryStream(buf);
            dbo.TypeName = m_types[rdr.GetInt32(1)];
            return dbo;
        }

        public override Queue<DatabaseObjectInformation> GetObjectsByTypeName(string TypeName)
        {
            if (!m_Connected)
                return null;
            Queue<DatabaseObjectInformation> dbos = new Queue<DatabaseObjectInformation>();
            DatabaseObjectInformation dbo;
            int Typeid = (from s in m_types
                          where s.Value == TypeName
                          select s.Key).First();
            SqlCeCommand Command = new SqlCeCommand("SELECT ObjectID,TypeID,Data from objects WHERE TypeID = @TypeID",m_connection);
            Command.Parameters.Add(new SqlCeParameter("TypeID", Typeid));
            SqlCeDataReader rdr = Command.ExecuteReader();
            if (!rdr.HasRows)
                return dbos;
            rdr.Read();
            do
            {
                int length = (int)rdr.GetBytes(2, 0, null, 0, 0);
                byte[] buf = new byte[length];
                dbo = new DatabaseObjectInformation();
                dbo.UniqueID = (UInt64)rdr.GetInt64(0);
                rdr.GetBytes(2, 0, buf, 0, length);
                dbo.Data = new MemoryStream(buf);
                dbo.TypeName = m_types[rdr.GetInt32(1)];
                dbos.Enqueue(dbo);
            } while (rdr.Read());
            return dbos;
        }

        public override void InsertOrUpdateObjectData(ulong ObjectID, Type Type, System.IO.MemoryStream  Data)
        {
            if (!m_Connected)
                return;
            int TypeID;
            if(!m_typeids.ContainsKey(Type.FullName))
            {
                SqlCeCommand Command2 = new SqlCeCommand("INSERT OR REPLACE INTO types(TypeID,Name) VALUES (" 
                    +m_currentTypeID.ToString()+"," +"\""+Type.FullName+"\")",m_connection); //TODO: Maybe parameterize
                Command2.ExecuteNonQuery();
                m_types.Add(m_currentTypeID, Type.FullName);
                m_typeids.Add(Type.FullName, m_currentTypeID);
                TypeID = m_currentTypeID;
                m_currentTypeID++;
            }
            else
            {
                TypeID = m_typeids[Type.FullName];
            }
            SqlCeCommand Command = new SqlCeCommand("INSERT OR REPLACE INTO objects(ObjectID,TypeID,Data) VALUES("+
            "@ObjectID,@TypeID,@Data) ",m_connection);
            Command.Parameters.Add(new SqlCeParameter("ObjectID",ObjectID));
            Command.Parameters.Add(new SqlCeParameter("TypeID",TypeID));
            Command.Parameters.Add(new SqlCeParameter("Data",Data.GetBuffer()));
            Command.ExecuteNonQuery();
        }

        public override ulong GetNewUniqueID()
        {
            return m_currentID++;
        }

        public override void DeleteObject(ulong ObjectID)
        {
            if (!m_Connected)
                return;
            SqlCeCommand Command = new SqlCeCommand("DELETE * FROM objects WHERE ObjectID=@ID", m_connection);
            Command.Parameters.Add(new SqlCeParameter("ID", ObjectID));
            Command.ExecuteNonQuery();
        }
        private bool UpdateTypes()
        {
            if (!m_Connected)
                return false;
            m_types.Clear();
            m_typeids.Clear();
            SqlCeCommand Command = new SqlCeCommand("SELECT TypeID,Name from types", m_connection);
            SqlCeDataReader rdr = Command.ExecuteReader();
            if (!rdr.HasRows)
                return true;
            rdr.Read();
            do 
            {
                m_types.Add(rdr.GetInt32(0), rdr.GetString(1));
                m_typeids.Add( rdr.GetString(1),rdr.GetInt32(0));
            } while (rdr.Read());
            m_currentTypeID = m_types.Count + 1;
            return true;
        }
    }
}
