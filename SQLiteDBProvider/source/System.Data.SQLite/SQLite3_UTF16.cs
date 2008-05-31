/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace System.Data.SQLite
{
  using System;
  using System.Runtime.InteropServices;

  /// <summary>
  /// Alternate SQLite3 object, overriding many text behaviors to support UTF-16 (Unicode)
  /// </summary>
  internal class SQLite3_UTF16 : SQLite3
  {
    internal SQLite3_UTF16(SQLiteDateFormats fmt)
      : base(fmt)
    {
    }

    /// <summary>
    /// Overrides SQLiteConvert.ToString() to marshal UTF-16 strings instead of UTF-8
    /// </summary>
    /// <param name="b">A pointer to a UTF-16 string</param>
    /// <param name="nbytelen">The length (IN BYTES) of the string</param>
    /// <returns>A .NET string</returns>
    public override string ToString(IntPtr b, int nbytelen)
    {
      return UTF16ToString(b, nbytelen);
    }

    public static string UTF16ToString(IntPtr b, int nbytelen)
    {
      if (nbytelen == 0) return "";
      return Marshal.PtrToStringUni(b, nbytelen / 2);
    }

    internal override string Version
    {
      get
      {
        int len;
        return UTF8ToString(UnsafeNativeMethods.sqlite3_libversion_interop(out len), len);
      }
    }

    internal override void Open(string strFilename, bool usePool)
    {
      if (_sql != null) return;

      _usePool = usePool;
      if (usePool)
      {
        _fileName = strFilename;
        _sql = SQLiteConnectionPool.Remove(strFilename);
      }

      if (_sql == null)
      {
        IntPtr db;

        int n = UnsafeNativeMethods.sqlite3_open16_interop(strFilename, out db);
        if (n > 0) throw new SQLiteException(n, null);

        _sql = db;
      }
      _functionsArray = SQLiteFunction.BindFunctions(this);
    }

    internal override void Bind_DateTime(SQLiteStatement stmt, int index, DateTime dt)
    {
      Bind_Text(stmt, index, ToString(dt));
    }

    internal override void Bind_Text(SQLiteStatement stmt, int index, string value)
    {
      int n = UnsafeNativeMethods.sqlite3_bind_text16_interop(stmt._sqlite_stmt, index, value, value.Length * 2, -1);
      if (n > 0) throw new SQLiteException(n, SQLiteLastError());
    }

    internal override string ColumnName(SQLiteStatement stmt, int index)
    {
      int len;
      return UTF16ToString(UnsafeNativeMethods.sqlite3_column_name16_interop(stmt._sqlite_stmt, index, out len), len);
    }

    internal override DateTime GetDateTime(SQLiteStatement stmt, int index)
    {
      return ToDateTime(GetText(stmt, index));
    }

    internal override string GetText(SQLiteStatement stmt, int index)
    {
      int len;
      return UTF16ToString(UnsafeNativeMethods.sqlite3_column_text16_interop(stmt._sqlite_stmt, index, out len), len);
    }

    internal override string ColumnOriginalName(SQLiteStatement stmt, int index)
    {
      int len;
      return UTF16ToString(UnsafeNativeMethods.sqlite3_column_origin_name16_interop(stmt._sqlite_stmt, index, out len), len);
    }

    internal override string ColumnDatabaseName(SQLiteStatement stmt, int index)
    {
      int len;
      return UTF16ToString(UnsafeNativeMethods.sqlite3_column_database_name16_interop(stmt._sqlite_stmt, index, out len), len);
    }

    internal override string ColumnTableName(SQLiteStatement stmt, int index)
    {
      int len;
      return UTF16ToString(UnsafeNativeMethods.sqlite3_column_table_name16_interop(stmt._sqlite_stmt, index, out len), len);
    }

    internal override IntPtr CreateFunction(string strFunction, int nArgs, SQLiteCallback func, SQLiteCallback funcstep, SQLiteCallback funcfinal)
    {
      IntPtr nCookie;

      int n = UnsafeNativeMethods.sqlite3_create_function16_interop(_sql, strFunction, nArgs, 4, func, funcstep, funcfinal, out nCookie);
      if (n > 0) throw new SQLiteException(n, SQLiteLastError());

      return nCookie;
    }

    internal override IntPtr CreateCollation(string strCollation, SQLiteCollation func)
    {
      IntPtr nCookie;

      int n = UnsafeNativeMethods.sqlite3_create_collation16_interop(_sql, strCollation, 4, 0, func, out nCookie);
      if (n > 0) throw new SQLiteException(n, SQLiteLastError());

      return nCookie;
    }

    internal override string GetParamValueText(IntPtr ptr)
    {
      int len;
      return UTF16ToString(UnsafeNativeMethods.sqlite3_value_text16_interop(ptr, out len), len);
    }

    internal override void ReturnError(IntPtr context, string value)
    {
      UnsafeNativeMethods.sqlite3_result_error16_interop(context, value, value.Length);
    }

    internal override void ReturnText(IntPtr context, string value)
    {
      UnsafeNativeMethods.sqlite3_result_text16_interop(context, value, value.Length, (IntPtr)(-1));
    }
  }
}
