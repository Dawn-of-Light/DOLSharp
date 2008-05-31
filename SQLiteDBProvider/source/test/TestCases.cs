using System;
using System.Data.Common;
using System.Data;
using System.Data.SQLite;
using System.Transactions;

namespace test
{

  /// <summary>
  /// Scalar user-defined function.  In this example, the same class is declared twice with 
  /// different function names to demonstrate how to use alias names for user-defined functions.
  /// </summary>
  [SQLiteFunction(Name = "Foo", Arguments = 2, FuncType = FunctionType.Scalar)]
  [SQLiteFunction(Name = "TestFunc", Arguments = 2, FuncType = FunctionType.Scalar)]
  class TestFunc : SQLiteFunction
  {
    public override object Invoke(object[] args)
    {
      if (args[0].GetType() != typeof(int)) return args[0];

      int Param1 = Convert.ToInt32(args[0]); // First parameter
      int Param2 = Convert.ToInt32(args[1]); // Second parameter

      return Param1 + Param2;
    }
  }

  /// <summary>
  /// Aggregate user-defined function.  Arguments = -1 means any number of arguments is acceptable
  /// </summary>
  [SQLiteFunction(Name = "MyCount", Arguments = -1, FuncType = FunctionType.Aggregate)]
  class MyCount : SQLiteFunction
  {
    public override void Step(object[] args, int nStep, ref object contextData)
    {
      if (contextData == null)
      {
        contextData = 1;
      }
      else
        contextData = (int)contextData + 1;
    }

    public override object Final(object contextData)
    {
      return contextData;
    }
  }

  /// <summary>
  /// Sample regular expression function.  Example Usage:
  /// SELECT * FROM foo WHERE name REGEXP '$bar'
  /// SELECT * FROM foo WHERE REGEXP('$bar', name)
  /// 
  /// </summary>
  [SQLiteFunction(Name = "REGEXP", Arguments = 2, FuncType = FunctionType.Scalar)]
  class MyRegEx : SQLiteFunction
  {
    public override object Invoke(object[] args)
    {
      return System.Text.RegularExpressions.Regex.IsMatch(Convert.ToString(args[1]), Convert.ToString(args[0]));
    }
  }

  /// <summary>
  /// User-defined collating sequence.
  /// </summary>
  [SQLiteFunction(Name = "MYSEQUENCE", FuncType = FunctionType.Collation)]
  class MySequence : SQLiteFunction
  {
    public override int Compare(string param1, string param2)
    {
      // Make sure the string "Field3" is sorted out of order
      if (param1 == "Field3") return 1;
      if (param2 == "Field3") return -1;
      return String.Compare(param1, param2, true);
    }
  }

  internal class TestCases
  {
    internal static void Run(DbProviderFactory fact, DbConnection cnn)
    {
      Console.WriteLine("\r\nBeginning Test on " + cnn.GetType().ToString());
      try { CreateTable(cnn); Console.WriteLine("SUCCESS - CreateTable"); }
      catch (Exception) { Console.WriteLine("FAIL - CreateTable"); }

      try { FullTextTest(cnn); Console.WriteLine("SUCCESS - Full Text Search"); }
      catch (Exception) { Console.WriteLine("FAIL - Full Text Search"); }

      try { DataTypeTest(cnn); Console.WriteLine("SUCCESS - DataType Test"); }
      catch (Exception) { Console.WriteLine("FAIL - DataType Test"); }

      try { DisposePattenTest(cnn); Console.WriteLine("SUCCESS - Dispose pattern test"); }
      catch (Exception) { Console.WriteLine("FAIL - Dispose pattern test"); }

      try { KeyInfoTest(fact, cnn); Console.WriteLine("SUCCESS - KeyInfo Fetch"); }
      catch (Exception) { Console.WriteLine("FAIL - KeyInfo Fetch"); }

      try { TransactionTest(cnn); Console.WriteLine("SUCCESS - Transaction Enlistment"); }
      catch (Exception) { Console.WriteLine("FAIL - Transaction Enlistment"); }

      try { GuidTest(cnn); Console.WriteLine("SUCCESS - Guid Test"); }
      catch (Exception) { Console.WriteLine("FAIL - Guid Test"); }

      try { InsertTable(cnn); Console.WriteLine("SUCCESS - InsertTable"); }
      catch (Exception) { Console.WriteLine("FAIL - InsertTable"); }

      try { VerifyInsert(cnn); Console.WriteLine("SUCCESS - VerifyInsert"); }
      catch (Exception) { Console.WriteLine("FAIL - VerifyInsert"); }

      try { CoersionTest(cnn); Console.WriteLine("FAIL - CoersionTest"); }
      catch (Exception) { Console.WriteLine("SUCCESS - CoersionTest"); }

      try { ParameterizedInsert(cnn); Console.WriteLine("SUCCESS - ParameterizedInsert"); }
      catch (Exception) { Console.WriteLine("FAIL - ParameterizedInsert"); }

      try { BinaryInsert(cnn); Console.WriteLine("SUCCESS - BinaryInsert (using named parameter)"); }
      catch (Exception) { Console.WriteLine("FAIL - BinaryInsert"); }

      try { VerifyBinaryData(cnn); Console.WriteLine("SUCCESS - VerifyBinaryData"); }
      catch (Exception) { Console.WriteLine("FAIL - VerifyBinaryData"); }

      try { LockTest(cnn); Console.WriteLine("SUCCESS - LockTest"); }
      catch (Exception) { Console.WriteLine("FAIL - LockTest"); }

      try { ParameterizedInsertMissingParams(cnn); Console.WriteLine("FAIL - ParameterizedInsertMissingParams\r\n"); }
      catch (Exception) { Console.WriteLine("SUCCESS - ParameterizedInsertMissingParams\r\n"); }

      //try { TimeoutTest(cnn); Console.WriteLine("SUCCESS - TimeoutTest"); }
      //catch (Exception) { Console.WriteLine("FAIL - TimeoutTest"); }

      try { DataAdapter(fact, cnn, false); Console.WriteLine(""); }
      catch (Exception) { Console.WriteLine("FAIL - DataAdapter"); }

      try { DataAdapter(fact, cnn, true); Console.WriteLine(""); }
      catch (Exception) { Console.WriteLine("FAIL - DataAdapterWithIdentityFetch"); }

      try { FastInsertMany(cnn); Console.WriteLine(""); }
      catch (Exception) { Console.WriteLine("FAIL - FastInsertMany"); }

      try { IterationTest(cnn); Console.WriteLine(""); }
      catch (Exception) { Console.WriteLine("FAIL - Iteration Test"); }

      try { UserFunction(cnn); Console.WriteLine(""); }
      catch (Exception) { Console.WriteLine("FAIL - UserFunction"); }

      try { UserAggregate(cnn); Console.WriteLine(""); }
      catch (Exception) { Console.WriteLine("FAIL - UserAggregate"); }

      try { UserCollation(cnn); Console.WriteLine("SUCCESS - UserCollation"); }
      catch (Exception) { Console.WriteLine("FAIL - UserCollation"); }

      try { DropTable(cnn); Console.WriteLine("SUCCESS - DropTable"); }
      catch (Exception) { Console.WriteLine("FAIL - DropTable"); }

      Console.WriteLine("\r\nTests Finished.");
    }

    internal static void DisposePattenTest(DbConnection cnn)
    {
      using (DbConnection newcnn = ((ICloneable)cnn).Clone() as DbConnection)
      {
        for (int x = 0; x < 10000; x++)
        {
          DbCommand cmd = newcnn.CreateCommand();
          cmd.CommandText = "SELECT * FROM TestCase";
          DbDataReader reader = cmd.ExecuteReader();
          reader.Read();
        }
      }
    }

    internal static void DataTypeTest(DbConnection cnn)
    {
      DateTime now = DateTime.Now;
      using (DbCommand cmd = cnn.CreateCommand())
      {
        cmd.CommandText = "create table datatypetest(id integer primary key, myvalue, datetimevalue datetime, decimalvalue decimal)";
        cmd.ExecuteNonQuery();

        cmd.CommandText = "insert into datatypetest(myvalue, datetimevalue, decimalvalue) values(?,?,?)";
        DbParameter p1 = cmd.CreateParameter();
        DbParameter p2 = cmd.CreateParameter();
        DbParameter p3 = cmd.CreateParameter();

        cmd.Parameters.Add(p1);
        cmd.Parameters.Add(p2);
        cmd.Parameters.Add(p3);

        p1.Value = 1;
        p2.Value = DateTime.MinValue;
        p3.Value = (Decimal)1.05;
        cmd.ExecuteNonQuery();

        p1.ResetDbType();
        p2.ResetDbType();
        p3.ResetDbType();

        p1.Value = "One";
        p2.Value = "2001-01-01";
        p3.Value = (Decimal)1.0;
        cmd.ExecuteNonQuery();

        p1.ResetDbType();
        p2.ResetDbType();
        p3.ResetDbType();

        p1.Value = 1.01;
        p2.Value = now;
        p3.Value = (Decimal)9.91;
        cmd.ExecuteNonQuery();

        cmd.CommandText = "select myvalue, datetimevalue, decimalvalue from datatypetest";
        using (DbDataReader reader = cmd.ExecuteReader())
        {
          for (int n = 0; n < 3; n++)
          {
            reader.Read();
            if (reader.GetValue(1).GetType() != reader.GetDateTime(1).GetType()) throw new ArgumentOutOfRangeException();
            if (reader.GetValue(2).GetType() != reader.GetDecimal(2).GetType()) throw new ArgumentOutOfRangeException();

            switch (n)
            {
              case 0:
                if (reader.GetValue(0).GetType() != typeof(long)) throw new ArgumentOutOfRangeException();

                if (reader.GetValue(0).Equals((long)1) == false) throw new ArgumentOutOfRangeException();
                if (reader.GetValue(1).Equals(DateTime.MinValue) == false) throw new ArgumentOutOfRangeException();
                if (reader.GetValue(2).Equals((Decimal)1.05) == false) throw new ArgumentOutOfRangeException();

                if (reader.GetInt64(0) != (long)1) throw new ArgumentOutOfRangeException();
                if (reader.GetValue(1).Equals(reader.GetDateTime(1)) == false) throw new ArgumentOutOfRangeException();
                if (reader.GetValue(2).Equals(reader.GetDecimal(2)) == false) throw new ArgumentOutOfRangeException();
                break;
              case 1:
                if (reader.GetValue(0).GetType() != typeof(string)) throw new ArgumentOutOfRangeException();
                if (reader.GetValue(0).Equals("One") == false) throw new ArgumentOutOfRangeException();
                if (reader.GetValue(1).Equals(new DateTime(2001, 1, 1)) == false) throw new ArgumentOutOfRangeException();
                if (reader.GetValue(2).Equals((Decimal)1.0) == false) throw new ArgumentOutOfRangeException();

                if (reader.GetString(0) != "One") throw new ArgumentOutOfRangeException();
                if (reader.GetValue(1).Equals(reader.GetDateTime(1)) == false) throw new ArgumentOutOfRangeException();
                if (reader.GetValue(2).Equals(reader.GetDecimal(2)) == false) throw new ArgumentOutOfRangeException();
                break;
              case 2:
                if (reader.GetValue(0).GetType() != typeof(double)) throw new ArgumentOutOfRangeException();
                if (reader.GetValue(0).Equals(1.01) == false) throw new ArgumentOutOfRangeException();
                if (reader.GetValue(1).Equals(now) == false) throw new ArgumentOutOfRangeException();
                if (reader.GetValue(2).Equals((Decimal)9.91) == false) throw new ArgumentOutOfRangeException();

                if (reader.GetDouble(0) != 1.01) throw new ArgumentOutOfRangeException();
                if (reader.GetValue(1).Equals(reader.GetDateTime(1)) == false) throw new ArgumentOutOfRangeException();
                if (reader.GetValue(2).Equals(reader.GetDecimal(2)) == false) throw new ArgumentOutOfRangeException();
                break;
            }
          }
        }
      }
    }

    internal static void KeyInfoTest(DbProviderFactory fact, DbConnection cnn)
    {
      using (DbCommand cmd = cnn.CreateCommand())
      {
        // First test against integer primary key (optimized) keyinfo fetch
        cmd.CommandText = "Create table keyinfotest (id integer primary key, myuniquevalue integer unique not null, myvalue varchar(50))";
        cmd.ExecuteNonQuery();

        cmd.CommandText = "Select * from keyinfotest";
        using (DbDataReader reader = cmd.ExecuteReader(CommandBehavior.KeyInfo | CommandBehavior.SchemaOnly))
        {
          using (DataTable tbl = reader.GetSchemaTable())
          {
            if (tbl.Rows.Count != 3) throw new ArgumentOutOfRangeException("Wrong number of columns returned");
          }
        }

        cmd.CommandText = "SELECT MyValue FROM keyinfotest";
        using (DbDataReader reader = cmd.ExecuteReader(CommandBehavior.KeyInfo | CommandBehavior.SchemaOnly))
        {
          using (DataTable tbl = reader.GetSchemaTable())
          {
            if (tbl.Rows.Count != 2) throw new ArgumentOutOfRangeException("Wrong number of columns returned");
          }
        }

        cmd.CommandText = "DROP TABLE keyinfotest";
        cmd.ExecuteNonQuery();

        // Now test against non-integer primary key (unoptimized) subquery keyinfo fetch
        cmd.CommandText = "Create table keyinfotest (id char primary key, myuniquevalue integer unique not null, myvalue varchar(50))";
        cmd.ExecuteNonQuery();

        cmd.CommandText = "SELECT MyValue FROM keyinfotest";
        using (DbDataReader reader = cmd.ExecuteReader(CommandBehavior.KeyInfo | CommandBehavior.SchemaOnly))
        {
          using (DataTable tbl = reader.GetSchemaTable())
          {
            if (tbl.Rows.Count != 2) throw new ArgumentOutOfRangeException("Wrong number of columns returned");
          }
        }

        cmd.CommandText = "Select * from keyinfotest";
        using (DbDataReader reader = cmd.ExecuteReader(CommandBehavior.KeyInfo | CommandBehavior.SchemaOnly))
        {
          using (DataTable tbl = reader.GetSchemaTable())
          {
            if (tbl.Rows.Count != 3) throw new ArgumentOutOfRangeException("Wrong number of columns returned");
          }
        }

        // Make sure commandbuilder can generate an update command with the correct parameter count
        using (DbDataAdapter adp = fact.CreateDataAdapter())
        using (DbCommandBuilder builder = fact.CreateCommandBuilder())
        {
          adp.SelectCommand = cmd;
          builder.DataAdapter = adp;
          builder.ConflictOption = ConflictOption.OverwriteChanges;

          using (DbCommand updatecmd = builder.GetUpdateCommand())
          {
            if (updatecmd.Parameters.Count != 4)
              throw new ArgumentOutOfRangeException("Wrong number of parameters in update command!");
          }
        }
      }
    }

    internal static void FullTextTest(DbConnection cnn)
    {
      using (DbCommand cmd = cnn.CreateCommand())
      {
        cmd.CommandText = "CREATE VIRTUAL TABLE FullText USING FTS3(name, ingredients);";
        cmd.ExecuteNonQuery();

        string[] names = { "broccoli stew", "pumpkin stew", "broccoli pie", "pumpkin pie" };
        string[] ingredients = { "broccoli peppers cheese tomatoes", "pumpkin onions garlic celery", "broccoli cheese onions flour", "pumpkin sugar flour butter" };
        int n;

        cmd.CommandText = "insert into FullText (name, ingredients) values (@name, @ingredient);";
        DbParameter name = cmd.CreateParameter();
        DbParameter ingredient = cmd.CreateParameter();

        name.ParameterName = "@name";
        ingredient.ParameterName = "@ingredient";

        cmd.Parameters.Add(name);
        cmd.Parameters.Add(ingredient);

        for (n = 0; n < names.Length; n++)
        {
          name.Value = names[n];
          ingredient.Value = ingredients[n];

          cmd.ExecuteNonQuery();
        }

        cmd.CommandText = "select rowid, name, ingredients from FullText where name match 'pie';";

        int[] rowids = { 3, 4 };
        n = 0;

        using (DbDataReader reader = cmd.ExecuteReader())
        {
          while (reader.Read())
          {
            if (reader.GetInt64(0) != rowids[n++])
              throw new ArgumentException("Unexpected rowid returned");

            if (n > rowids.Length) throw new ArgumentException("Too many rows returned");
          }
        }
      }
    }
    internal static void TransactionTest(DbConnection cnn)
    {
      using (TransactionScope scope = new TransactionScope())
      {
        using (DbConnection cnn2 = ((ICloneable)cnn).Clone() as DbConnection)
        {
          using (DbCommand cmd = cnn2.CreateCommand())
          {
            // Created a table inside the transaction scope
            cmd.CommandText = "CREATE TABLE VolatileTable (ID INTEGER PRIMARY KEY, MyValue VARCHAR(50))";
            cmd.ExecuteNonQuery();

            using (DbCommand cmd2 = cnn2.CreateCommand())
            {
              using (cmd2.Transaction = cnn2.BeginTransaction())
              {
                // Inserting a value inside the table, inside a transaction which is inside the transaction scope
                cmd2.CommandText = "INSERT INTO VolatileTable (ID, MyValue) VALUES(1, 'Hello')";
                cmd2.ExecuteNonQuery();
                cmd2.Transaction.Commit();
              }
            }
          }
          // Connection is disposed before the transactionscope leaves, thereby forcing the connection to stay open
        }
        // Exit the transactionscope without committing it, causing a rollback of both the create table and the insert
      }

      // Verify that the table does not exist
      using (DbCommand cmd = cnn.CreateCommand())
      {
        cmd.CommandText = "SELECT COUNT(*) FROM VolatileTable";
        try
        {
          object o = cmd.ExecuteScalar();
          throw new InvalidOperationException("Transaction failed! The table exists!");
        }
        catch(SQLiteException)
        {
          return; // Succeeded, the table should not have existed
        }
      }
    }

    internal static void CreateTable(DbConnection cnn)
    {
      using (DbCommand cmd = cnn.CreateCommand())
      {
        cmd.CommandText = "CREATE TABLE TestCase (ID integer primary key autoincrement, Field1 int, Field2 Float, Field3 VARCHAR(50), Field4 CHAR(10), Field5 DateTime, Field6 Image)";
        //cmd.CommandText = "CREATE TABLE TestCase (ID bigint primary key identity, Field1 Integer, Field2 Float, Field3 VARCHAR(50), Field4 CHAR(10), Field5 DateTime, Field6 Image)";
        cmd.ExecuteNonQuery();
      }
    }

    internal static void DropTable(DbConnection cnn)
    {
      using (DbCommand cmd = cnn.CreateCommand())
      {
        cmd.CommandText = "DROP TABLE TestCase";
        cmd.ExecuteNonQuery();
      }
    }

    internal static void InsertTable(DbConnection cnn)
    {
      using (DbCommand cmd = cnn.CreateCommand())
      {
        cmd.CommandText = "INSERT INTO TestCase(Field1, Field2, Field3, Field4, Field5) VALUES(1, 3.14159, 'Field3', 'Field4', '2005-01-01 13:49:00')";
        cmd.ExecuteNonQuery();
      }
    }

    internal static void GuidTest(DbConnection cnn)
    {
      using (DbCommand cmd = cnn.CreateCommand())
      {
        Guid guid = Guid.NewGuid();

        cmd.CommandText = "CREATE TABLE GuidTest(MyGuid GUID)";
        cmd.ExecuteNonQuery();

        // Insert a guid as a default binary representation
        cmd.CommandText = "INSERT INTO GuidTest(MyGuid) VALUES(@b)";
        ((SQLiteParameterCollection)cmd.Parameters).AddWithValue("@b", guid);

        // Insert a guid as text
        cmd.ExecuteNonQuery();
        cmd.Parameters[0].Value = guid.ToString();
        cmd.Parameters[0].DbType = DbType.String;
        cmd.ExecuteNonQuery();

        cmd.CommandText = "SELECT MyGuid FROM GuidTest";
        using (DbDataReader reader = cmd.ExecuteReader())
        {
          reader.Read();
          if (reader.GetFieldType(0) != typeof(Guid)) throw new ArgumentException("Column is not a Guid");
          if (reader.GetGuid(0) != guid) throw new ArgumentException("Guids don't match!");

          reader.Read();
          if (reader.GetFieldType(0) != typeof(Guid)) throw new ArgumentException("Column is not a Guid");
          if (reader.GetGuid(0) != guid) throw new ArgumentException("Guids don't match!");
        }
      }
    }
 
    internal static void VerifyInsert(DbConnection cnn)
    {
      using (DbCommand cmd = cnn.CreateCommand())
      {
        cmd.CommandText = "SELECT Field1, Field2, Field3, Field4, Field5 FROM TestCase";
        cmd.Prepare();
        using (DbDataReader rd = cmd.ExecuteReader())
        {
          if (rd.Read())
          {
            long Field1 = rd.GetInt64(0);
            double Field2 = rd.GetDouble(1);
            string Field3 = rd.GetString(2);
            string Field4 = rd.GetString(3).TrimEnd();
            DateTime Field5 = rd.GetDateTime(4);

            if (Field1 != 1) throw new ArgumentOutOfRangeException("Non-Match on Field1");
            if (Field2 != 3.14159) throw new ArgumentOutOfRangeException("Non-Match on Field2");
            if (Field3 != "Field3") throw new ArgumentOutOfRangeException("Non-Match on Field3");
            if (Field4 != "Field4") throw new ArgumentOutOfRangeException("Non-Match on Field4");
            if (Field5.CompareTo(DateTime.Parse("2005-01-01 13:49:00")) != 0) throw new ArgumentOutOfRangeException("Non-Match on Field5");
          }
          else throw new ArgumentOutOfRangeException("No data in table");
        }
      }
    }
    
    internal static void CoersionTest(DbConnection cnn)
    {
      using (DbCommand cmd = cnn.CreateCommand())
      {
        cmd.CommandText = "SELECT Field1, Field2, Field3, Field4, Field5, 'A', 1, 1 + 1, 3.14159 FROM TestCase";
        using (DbDataReader rd = cmd.ExecuteReader())
        {
          if (rd.Read())
          {
            object Field1 = rd.GetInt32(0);
            object Field2 = rd.GetDouble(1);
            object Field3 = rd.GetString(2);
            object Field4 = rd.GetString(3).TrimEnd();
            object Field5 = rd.GetDateTime(4);

            // The next statement should cause an exception
            Field1 = rd.GetString(0);
            Field2 = rd.GetString(1);
            Field3 = rd.GetString(2);
            Field4 = rd.GetString(3);
            Field5 = rd.GetString(4);

            Field1 = rd.GetInt32(0);
            Field2 = rd.GetInt32(1);
            Field3 = rd.GetInt32(2);
            Field4 = rd.GetInt32(3);
            Field5 = rd.GetInt32(4);

            Field1 = rd.GetDecimal(0);
            Field2 = rd.GetDecimal(1);
            Field3 = rd.GetDecimal(2);
            Field4 = rd.GetDecimal(3);
            Field5 = rd.GetDecimal(4);
          }
          else throw new ArgumentOutOfRangeException("No data in table");
        }
      }
    }

    internal static void ParameterizedInsert(DbConnection cnn)
    {
      using (DbCommand cmd = cnn.CreateCommand())
      {
        cmd.CommandText = "INSERT INTO TestCase(Field1, Field2, Field3, Field4, Field5) VALUES(?,?,?,?,?)";
        DbParameter Field1 = cmd.CreateParameter();
        DbParameter Field2 = cmd.CreateParameter();
        DbParameter Field3 = cmd.CreateParameter();
        DbParameter Field4 = cmd.CreateParameter();
        DbParameter Field5 = cmd.CreateParameter();

        Field1.Value = 2;
        Field2.Value = 3.14159;
        Field3.Value = "Param Field3";
        Field4.Value = "Field4 Par";
        Field5.Value = DateTime.Now;

        cmd.Parameters.Add(Field1);
        cmd.Parameters.Add(Field2);
        cmd.Parameters.Add(Field3);
        cmd.Parameters.Add(Field4);
        cmd.Parameters.Add(Field5);

        cmd.ExecuteNonQuery();
      }
    }

    // Inserts binary data into the database using a named parameter
    internal static void BinaryInsert(DbConnection cnn)
    {
      using (DbCommand cmd = cnn.CreateCommand())
      {
        cmd.CommandText = "INSERT INTO TestCase(Field6) VALUES(@bin)";
        DbParameter Field6 = cmd.CreateParameter();

        byte[] b = new byte[4000];
        b[0] = 1;
        b[100] = 2;
        b[1000] = 3;
        b[2000] = 4;
        b[3000] = 5;

        Field6.ParameterName = "@bin";
        Field6.Value = b;

        cmd.Parameters.Add(Field6);

        cmd.ExecuteNonQuery();
      }
    }

    internal static void VerifyBinaryData(DbConnection cnn)
    {
      using (DbCommand cmd = cnn.CreateCommand())
      {
        cmd.CommandText = "SELECT Field6 FROM TestCase WHERE Field6 IS NOT NULL";
        byte[] b = new byte[4000];

        using (DbDataReader rd = cmd.ExecuteReader())
        {
          if (rd.Read() == false) throw new ArgumentOutOfRangeException();

          rd.GetBytes(0, 0, b, 0, 4000);

          if (b[0] != 1) throw new ArgumentException();
          if (b[100] != 2) throw new ArgumentException();
          if (b[1000] != 3) throw new ArgumentException();
          if (b[2000] != 4) throw new ArgumentException();
          if (b[3000] != 5) throw new ArgumentException();
        }
      }
    }

    internal static void LockTest(DbConnection cnn)
    {
      using (DbCommand cmd = cnn.CreateCommand())
      {
        cmd.CommandText = "SELECT Field6 FROM TestCase WHERE Field6 IS NOT NULL";
        byte[] b = new byte[4000];

        using (DbDataReader rd = cmd.ExecuteReader())
        {
          if (rd.Read() == false) throw new ArgumentOutOfRangeException();

          rd.GetBytes(0, 0, b, 0, 4000);

          if (b[0] != 1) throw new ArgumentException();
          if (b[100] != 2) throw new ArgumentException();
          if (b[1000] != 3) throw new ArgumentException();
          if (b[2000] != 4) throw new ArgumentException();
          if (b[3000] != 5) throw new ArgumentException();

          using (DbConnection clone = (DbConnection)((ICloneable)cnn).Clone())
          {
            using (DbCommand newcmd = clone.CreateCommand())
            {
              newcmd.CommandText = "DELETE FROM TestCase WHERE Field6 IS NULL";
              newcmd.CommandTimeout = 2;
              int cmdStart = Environment.TickCount;
              int cmdEnd;

              try
              {
                newcmd.ExecuteNonQuery(); // should fail because there's a reader on the database
                throw new ArgumentException(); // If we got here, the test failed
              }
              catch
              {
                cmdEnd = Environment.TickCount;
                if (cmdEnd - cmdStart < 2000 || cmdEnd - cmdStart > 3000)
                  throw new ArgumentException(); // Didn't wait the right amount of time

              }
            }
          }
        }
      }
    }

    internal static void ParameterizedInsertMissingParams(DbConnection cnn)
    {
      using (DbCommand cmd = cnn.CreateCommand())
      {
        cmd.CommandText = "INSERT INTO TestCase(Field1, Field2, Field3, Field4, Field5) VALUES(?,?,?,?,?)";
        DbParameter Field1 = cmd.CreateParameter();
        DbParameter Field2 = cmd.CreateParameter();
        DbParameter Field3 = cmd.CreateParameter();
        DbParameter Field4 = cmd.CreateParameter();
        DbParameter Field5 = cmd.CreateParameter();

        Field1.DbType = System.Data.DbType.Int32;

        Field1.Value = 2;
        Field2.Value = 3.14159;
        Field3.Value = "Field3 Param";
        Field4.Value = "Field4 Par";
        Field5.Value = DateTime.Now;

        cmd.Parameters.Add(Field1);
        cmd.Parameters.Add(Field2);
        cmd.Parameters.Add(Field3);
        cmd.Parameters.Add(Field4);

        // Assertion here, not enough parameters
        cmd.ExecuteNonQuery();
      }
    }

    // Utilizes the SQLiteCommandBuilder, 
    // which in turn utilizes SQLiteDataReader's GetSchemaTable() functionality
    // This insert is slow because it must raise callbacks before and after every update.
    // For a fast update, see the FastInsertMany function beneath this one
    internal static void DataAdapter(DbProviderFactory fact, DbConnection cnn, bool bWithIdentity)
    {
      using (DbTransaction dbTrans = cnn.BeginTransaction())
      {
        using (DbDataAdapter adp = fact.CreateDataAdapter())
        {
          using (DbCommand cmd = cnn.CreateCommand())
          {
            cmd.Transaction = dbTrans;
            cmd.CommandText = "SELECT * FROM TestCase WHERE 1 = 2";
            adp.SelectCommand = cmd;

            using (DbCommandBuilder bld = fact.CreateCommandBuilder())
            {
              bld.DataAdapter = adp;
              using (adp.InsertCommand = (DbCommand)((ICloneable)bld.GetInsertCommand()).Clone())
              {
                if (bWithIdentity)
                {
                  adp.InsertCommand.CommandText += ";SELECT last_insert_rowid() AS [ID]";
                  adp.InsertCommand.UpdatedRowSource = UpdateRowSource.FirstReturnedRecord;
                }
                bld.DataAdapter = null;

                using (DataTable tbl = new DataTable())
                {
                  adp.Fill(tbl);
                  for (int n = 0; n < 10000; n++)
                  {
                    DataRow row = tbl.NewRow();
                    row[1] = n + (50000 * ((bWithIdentity == true) ? 2 : 1));
                    tbl.Rows.Add(row);
                  }

                  Console.WriteLine(String.Format("          Inserting using CommandBuilder and DataAdapter\r\n          ->{0} (10,000 rows) ...", (bWithIdentity == true) ? "(with identity fetch)" : ""));
                  int dtStart = Environment.TickCount;
                  adp.Update(tbl);
                  int dtEnd = Environment.TickCount;
                  dtEnd -= dtStart;
                  Console.Write(String.Format("          -> Insert Ends in {0} ms ... ", (dtEnd)));

                  dtStart = Environment.TickCount;
                  dbTrans.Commit();
                  dtEnd = Environment.TickCount;
                  dtEnd -= dtStart;
                  Console.WriteLine(String.Format("Commits in {0} ms", (dtEnd)));
                }

                using (DataTable tbl = new DataTable())
                {
                  adp.SelectCommand.CommandText = "SELECT * FROM TestCase";
                  adp.Fill(tbl);
                }
              }
            }
          }
        }
      }
    }

    internal static void FastInsertMany(DbConnection cnn)
    {
      using (DbTransaction dbTrans = cnn.BeginTransaction())
      {
        int dtStart;
        int dtEnd;

        using (DbCommand cmd = cnn.CreateCommand())
        {
          cmd.CommandText = "INSERT INTO TestCase(Field1) VALUES(?)";
          DbParameter Field1 = cmd.CreateParameter();

          cmd.Parameters.Add(Field1);

          Console.WriteLine(String.Format("          Fast insert using parameters and prepared statement\r\n          -> (100,000 rows) Begins ... "));
          dtStart = Environment.TickCount;
          for (int n = 0; n < 100000; n++)
          {
            Field1.Value = n + 200000;
            cmd.ExecuteNonQuery();
          }

          dtEnd = Environment.TickCount;
          dtEnd -= dtStart;
          Console.Write(String.Format("          -> Ends in {0} ms ... ", (dtEnd)));
        }

        dtStart = Environment.TickCount;
        dbTrans.Commit();
        dtEnd = Environment.TickCount;
        dtEnd -= dtStart;
        Console.WriteLine(String.Format("Commits in {0} ms", (dtEnd)));
      }
    }

    // Causes the user-defined function to be called
    internal static void UserFunction(DbConnection cnn)
    {
      using (DbCommand cmd = cnn.CreateCommand())
      {
        int nTimes;
        int dtStart;
        
        nTimes = 0;
        cmd.CommandText = "SELECT Foo('ee','foo')";
        dtStart = Environment.TickCount;
        while (Environment.TickCount - dtStart < 1000)
        {
          cmd.ExecuteNonQuery();
          nTimes++;
        }
        Console.WriteLine(String.Format("          User (text)  command executed {0} times in 1 second.", nTimes));

        nTimes = 0;
        cmd.CommandText = "SELECT Foo(10,11)";
        dtStart = Environment.TickCount;
        while (Environment.TickCount - dtStart < 1000)
        {
          cmd.ExecuteNonQuery();
          nTimes++;
        }
        Console.WriteLine(String.Format("          UserFunction command executed {0} times in 1 second.", nTimes));

        nTimes = 0;
        cmd.CommandText = "SELECT ABS(1)";
        dtStart = Environment.TickCount;
        while (Environment.TickCount - dtStart < 1000)
        {
          cmd.ExecuteNonQuery();
          nTimes++;
        }
        Console.WriteLine(String.Format("          Intrinsic    command executed {0} times in 1 second.", nTimes));

        nTimes = 0;
        cmd.CommandText = "SELECT lower('FOO')";
        dtStart = Environment.TickCount;
        while (Environment.TickCount - dtStart < 1000)
        {
          cmd.ExecuteNonQuery();
          nTimes++;
        }
        Console.WriteLine(String.Format("          Intrin (txt) command executed {0} times in 1 second.", nTimes));

        nTimes = 0;
        cmd.CommandText = "SELECT 1";
        dtStart = Environment.TickCount;
        while (Environment.TickCount - dtStart < 1000)
        {
          cmd.ExecuteNonQuery();
          nTimes++;
        }
        Console.WriteLine(String.Format("          Raw Value    command executed {0} times in 1 second.", nTimes));
      }
    }

    internal static void IterationTest(DbConnection cnn)
    {
      using (DbCommand cmd = cnn.CreateCommand())
      {
        int dtStart;
        int dtEnd;
        int nCount;
        long n;

        cmd.CommandText = "SELECT Foo(ID, ID) FROM TestCase";
        cmd.Prepare();
        dtStart = Environment.TickCount;
        nCount = 0;
        using (DbDataReader rd = cmd.ExecuteReader())
        {
          while (rd.Read())
          {
            n = rd.GetInt64(0);
            nCount++;
          }
          dtEnd = Environment.TickCount;
        }
        Console.WriteLine(String.Format("          User Function iteration of {0} records in {1} ms", nCount, (dtEnd - dtStart)));

        cmd.CommandText = "SELECT ID FROM TestCase";
        cmd.Prepare();
        dtStart = Environment.TickCount;
        nCount = 0;
        using (DbDataReader rd = cmd.ExecuteReader())
        {
          while (rd.Read())
          {
            n = rd.GetInt64(0);
            nCount++;
          }
          dtEnd = Environment.TickCount;
        }
        Console.WriteLine(String.Format("          Raw iteration of {0} records in {1} ms", nCount, (dtEnd - dtStart)));

        cmd.CommandText = "SELECT ABS(ID) FROM TestCase";
        cmd.Prepare();
        dtStart = Environment.TickCount;
        nCount = 0;
        using (DbDataReader rd = cmd.ExecuteReader())
        {
          while (rd.Read())
          {
            n = rd.GetInt64(0);
            nCount++;
          }
          dtEnd = Environment.TickCount;
        }
        Console.WriteLine(String.Format("          Intrinsic Function iteration of {0} records in {1} ms", nCount, (dtEnd - dtStart)));

      }
    }

    // Open a reader and then attempt to write to test the writer's command timeout property
    // SQLite doesn't allow a write when a reader is active.
    // *** NOTE AS OF 3.3.8 this test no longer blocks because SQLite now allows you to update table(s)
    // while a reader is active on the same connection.  Therefore the timeout test is invalid
    internal static void TimeoutTest(DbConnection cnn)
    {
      using (DbCommand cmdRead = cnn.CreateCommand())
      {
        cmdRead.CommandText = "SELECT ID FROM TestCase";
        using (DbDataReader rd = cmdRead.ExecuteReader())
        {
          using (DbCommand cmdwrite = cnn.CreateCommand())
          {
            cmdwrite.CommandText = "UPDATE [KeyInfoTest] SET [ID] = [ID]";
            cmdwrite.CommandTimeout = 5;

            int dwtick = Environment.TickCount;
            try
            {
              cmdwrite.ExecuteNonQuery();
            }
            catch (SQLiteException)
            {
              dwtick = (Environment.TickCount - dwtick) / 1000;
              if (dwtick < 5 || dwtick > 6)
                throw new ArgumentOutOfRangeException();

              return;
            }
            throw new ArgumentOutOfRangeException("Operation Completed successfully");
          }
        }
      }
    }

    // Causes the user-defined aggregate to be iterated through
    internal static void UserAggregate(DbConnection cnn)
    {
      using (DbCommand cmd = cnn.CreateCommand())
      {
        int dtStart;
        int n = 0;
        int nCount;

        cmd.CommandText = "SELECT MyCount(*) FROM TestCase";

        nCount = 0;
        dtStart = Environment.TickCount;
        while (Environment.TickCount - dtStart < 1000)
        {
          n = Convert.ToInt32(cmd.ExecuteScalar());
          nCount++;
        }
        if (n != 120003) throw new ArgumentOutOfRangeException("Unexpected count");
        Console.WriteLine(String.Format("          UserAggregate executed {0} times in 1 second.", nCount));
      }
    }

    // Causes the user-defined collation sequence to be iterated through
    internal static void UserCollation(DbConnection cnn)
    {
      using (DbCommand cmd = cnn.CreateCommand())
      {
        // Using a default collating sequence in descending order, "Param Field3" will appear at the top
        // and "Field3" will be next, followed by a NULL.  Our user-defined collating sequence will 
        // deliberately place them out of order so Field3 is first.
        cmd.CommandText = "SELECT Field3 FROM TestCase ORDER BY Field3 COLLATE MYSEQUENCE DESC";
        string s = (string)cmd.ExecuteScalar();
        if (s != "Field3") throw new ArgumentOutOfRangeException("MySequence didn't sort properly");
      }
    }
  }
}
