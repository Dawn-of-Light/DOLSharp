using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using DOL.Database2;
namespace DOL.Database2.Providers
{
    public class SQLiteDBProvider : DatabaseProvider
    {
        private SQLiteConnectionStringBuilder m_connectionstring = new SQLiteConnectionStringBuilder();
        private SQLiteConnection m_connection = new SQLiteConnection();
        private SQLiteCommandBuilder m_commandbuilder = new SQLiteCommandBuilder();
        private UInt64 m_currentID = 1;
        private int m_currentTypeID = 1;
        Dictionary<int,string> m_types = new Dictionary<int,string>();
        Dictionary<string, int> m_typeids = new Dictionary<string, int>();
        
        public SQLiteDBProvider()
        {
            m_connectionstring.CacheSize = 4096;
            m_connectionstring.DataSource = "Database.dol";
            m_connectionstring.DateTimeFormat = SQLiteDateFormats.ISO8601;
            m_connectionstring.DefaultTimeout = 600;
            m_connectionstring.LegacyFormat   = false;
            m_connectionstring.Version = 3;
            m_connectionstring.BinaryGUID = true;
            m_connectionstring.Enlist = true;
            m_connectionstring.FailIfMissing = false;
        }
        public override string ConnectionString
        {
            get
            {
                return m_connectionstring.ConnectionString;
            }
            set
            {
                m_connectionstring.ConnectionString = value;
            }
        }

        public override void OpenConnection()
        {
            /*
            if (!File.Exists(m_connectionstring.DataSource))
                SQLiteConnection.CreateFile(m_connectionstring.DataSource.Split("=")[1]); */
            m_connection.ConnectionString =  m_connectionstring.ConnectionString;
            m_connection.Open();
            SQLiteCommand Command = new SQLiteCommand("CREATE TABLE IF NOT EXISTS objects"+
              "(ObjectID INTEGER(4) PRIMARY KEY,TypeID INTEGER,Data BLOB)",m_connection);
            Command.ExecuteNonQuery();
            SQLiteCommand Command2 = new SQLiteCommand("CREATE TABLE IF NOT EXISTS types" +
              "(TypeID INTEGER PRIMARY KEY,Name Text UNIQUE)", m_connection);
            Command2.ExecuteNonQuery();
            m_Connected = true;
            SQLiteCommand Command3 = new SQLiteCommand("SELECT count(*) FROM objects", m_connection);
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
            SQLiteCommand Command = new SQLiteCommand("SELECT ObjectID,TypeID,Data from objects",m_connection);
            SQLiteDataReader rdr = Command.ExecuteReader();
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
            SQLiteCommand Command = new SQLiteCommand("SELECT ObjectID,TypeID,Data from objects WHERE ObjectID = @ObjectID",m_connection);
            Command.Parameters.Add(new SQLiteParameter("ObjectID", ID));
            SQLiteDataReader rdr = Command.ExecuteReader();
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
            SQLiteCommand Command = new SQLiteCommand("SELECT ObjectID,TypeID,Data from objects WHERE TypeID = @TypeID",m_connection);
            Command.Parameters.Add(new SQLiteParameter("TypeID", Typeid));
            SQLiteDataReader rdr = Command.ExecuteReader();
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
                SQLiteCommand Command2 = new SQLiteCommand("INSERT OR REPLACE INTO types(TypeID,Name) VALUES (" 
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
            SQLiteCommand Command = new SQLiteCommand("INSERT OR REPLACE INTO objects(ObjectID,TypeID,Data) VALUES("+
            "@ObjectID,@TypeID,@Data) ",m_connection);
            Command.Parameters.Add(new SQLiteParameter("ObjectID",ObjectID));
            Command.Parameters.Add(new SQLiteParameter("TypeID",TypeID));
            Command.Parameters.Add(new SQLiteParameter("Data",Data.GetBuffer()));
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
            SQLiteCommand Command = new SQLiteCommand("DELETE * FROM objects WHERE ObjectID=@ID", m_connection);
            Command.Parameters.Add(new SQLiteParameter("ID", ObjectID));
            Command.ExecuteNonQuery();
        }
        private bool UpdateTypes()
        {
            if (!m_Connected)
                return false;
            m_types.Clear();
            m_typeids.Clear();
            SQLiteCommand Command = new SQLiteCommand("SELECT TypeID,Name from types", m_connection);
            SQLiteDataReader rdr = Command.ExecuteReader();
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
