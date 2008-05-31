using System;
using System.Data;
using System.Text;
using System.Data.Common;
using System.Data.SQLite;
using System.Data.SqlClient;
using System.Transactions;

namespace test
{
  class Program
  {
    static void Main(string[] args)
    {
      DbProviderFactory fact;
      fact = DbProviderFactories.GetFactory("System.Data.SQLite");

      System.IO.File.Delete("test.db3");

      SQLiteConnection cnn = new SQLiteConnection();
      {
        cnn.ConnectionString = "Data Source=test.db3;Pooling=False";
        cnn.Open();

        TestCases.Run(fact, cnn);
      }

      Console.ReadKey();
    }

    static void cnn_RollBack(object sender, EventArgs e)
    {
    }

    static void cnn_Commit(object sender, CommitEventArgs e)
    {
    }

    static void cnn_Updated(object sender, UpdateEventArgs e)
    {
    }
  }
}
