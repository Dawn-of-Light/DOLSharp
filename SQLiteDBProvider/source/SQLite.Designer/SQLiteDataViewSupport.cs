/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace SQLite.Designer
{
  using System;
  using System.Collections.Generic;
  using System.Text;
  using Microsoft.VisualStudio.Data;
  using Microsoft.VisualStudio.OLE.Interop;
  using Microsoft.VisualStudio.Data.AdoDotNet;

  /// <summary>
  /// Provides DataViewSupport with a location where the XML file is for the Server Explorer's view.
  /// </summary>
  internal sealed class SQLiteDataViewSupport : DataViewSupport
  {
    public SQLiteDataViewSupport()
      : base("SQLite.Designer.SQLiteDataViewSupport", typeof(SQLiteDataViewSupport).Assembly)
    {
    }
  }
}
