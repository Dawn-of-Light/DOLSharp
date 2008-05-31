/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace SQLite.Designer
{
  using System;
  using Microsoft.VisualStudio.Data.AdoDotNet;
  using Microsoft.VisualStudio.Data;
  using Microsoft.Win32;

  /// <summary>
  /// Provides rudimentary connectionproperties support
  /// </summary>
  internal sealed class SQLiteConnectionProperties : AdoDotNetConnectionProperties
  {
    public SQLiteConnectionProperties() : base("System.Data.SQLite")
    {
    }

    public SQLiteConnectionProperties(string connectionString) : base("System.Data.SQLite", connectionString)
    {
    }

    public override string[] GetBasicProperties()
    {
      return new string[] { "Data Source" };
    }

    public override bool  IsComplete
    {
      get 
      {
        return true;
      }
    }

    public override bool EquivalentTo(DataConnectionProperties connectionProperties)
    {
      SQLiteConnectionProperties props = connectionProperties as SQLiteConnectionProperties;

      if (props == null) return false;

      return (String.Compare((string)this["Data Source"], (string)props["Data Source"], true) == 0);
    }
    // Provides automatic locating and loading of the SQLite assembly if its not registered in the GAC.
    // However, if it's not registered in the GAC, then certain design-time elements will fail.
    //
    //private static System.Reflection.Assembly _sqlite = null;
    //static SQLiteConnectionProperties()
    //{
    //  AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
    //}

    //private static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    //{
    //  if (args.Name.StartsWith("System.Data.SQLite", StringComparison.InvariantCultureIgnoreCase))
    //  {
    //    return SQLiteAssembly;
    //  }
    //  return null;
    //}

    //internal static System.Reflection.Assembly SQLiteAssembly
    //{
    //  get
    //  {
    //    if (_sqlite == null)
    //    {
    //      using (RegistryKey key = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\.NETFramework\\v2.0.50727\\AssemblyFoldersEx\\SQLite"))
    //      {
    //        if (key != null)
    //        {
    //          _sqlite = System.Reflection.Assembly.LoadFrom(System.IO.Path.Combine(key.GetValue(null).ToString(), "System.Data.SQLite.DLL"));
    //        }
    //      }
    //    }
    //    return _sqlite;
    //  }
    //}
  }
}
