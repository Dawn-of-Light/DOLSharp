/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace System.Data.SQLite
{
  using System;
  using System.Data;
  using System.Data.Common;
  using System.Collections.Generic;
  using System.Globalization;
  using System.ComponentModel;

  public sealed partial class SQLiteConnection
  {
#if LINQ
    protected override DbProviderFactory DbProviderFactory
    {
      get { return SQLiteFactory.Instance; }
    }
#endif
    /// <summary>
    /// Returns a SQLiteProviderFactory object.  This will be mandatory in .NET 3.5, so we'll eat the warning for now
    /// and allow .NET 2.0 objects to have this capability
    /// </summary>
    public new SQLiteFactory DbProviderFactory
    {
      get { return SQLiteFactory.Instance; }
    }
  }
}

