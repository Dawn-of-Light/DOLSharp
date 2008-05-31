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
  using System.Runtime.InteropServices;
  using System.Collections.Generic;

  /// <summary>
  /// This internal class provides the foundation of SQLite support.  It defines all the abstract members needed to implement
  /// a SQLite data provider, and inherits from SQLiteConvert which allows for simple translations of string to and from SQLite.
  /// </summary>
  internal abstract class SQLiteBase : SQLiteConvert, IDisposable
  {
    internal SQLiteBase(SQLiteDateFormats fmt)
      : base(fmt) { }

    /// <summary>
    /// Returns a string representing the active version of SQLite
    /// </summary>
    internal abstract string Version { get; }
    /// <summary>
    /// Returns the number of changes the last executing insert/update caused.
    /// </summary>
    internal abstract int Changes { get; }
    /// <summary>
    /// Opens a database.
    /// </summary>
    /// <param name="usePool">If true, the connection can be pulled from the connection pool</param>
    /// <remarks>
    /// Implementers should call SQLiteFunction.BindFunctions() and save the array after opening a connection
    /// to bind all attributed user-defined functions and collating sequences to the new connection.
    /// </remarks>
    /// <param name="strFilename">The filename of the database to open.  SQLite automatically creates it if it doesn't exist.</param>
    internal abstract void Open(string strFilename, bool usePool);
    /// <summary>
    /// Closes the currently-open database.
    /// </summary>
    /// <remarks>
    /// After the database has been closed implemeters should call SQLiteFunction.UnbindFunctions() to deallocate all interop allocated
    /// memory associated with the user-defined functions and collating sequences tied to the closed connection.
    /// </remarks>
    internal abstract void Close();
    /// <summary>
    /// Sets the busy timeout on the connection.  SQLiteCommand will call this before executing any command.
    /// </summary>
    /// <param name="nTimeoutMS">The number of milliseconds to wait before returning SQLITE_BUSY</param>
    internal abstract void SetTimeout(int nTimeoutMS);
    /// <summary>
    /// Returns the text of the last error issued by SQLite
    /// </summary>
    /// <returns></returns>
    internal abstract string SQLiteLastError();

    /// <summary>
    /// Prepares a SQL statement for execution.
    /// </summary>
    /// <param name="strSql">The SQL command text to prepare</param>
    /// <param name="previous">The previous statement in a multi-statement command, or null if no previous statement exists</param>
    /// <param name="strRemain">The remainder of the statement that was not processed.  Each call to prepare parses the
    /// SQL up to to either the end of the text or to the first semi-colon delimiter.  The remaining text is returned
    /// here for a subsequent call to Prepare() until all the text has been processed.</param>
    /// <returns>Returns an initialized SQLiteStatement.</returns>
    internal abstract SQLiteStatement Prepare(string strSql, SQLiteStatement previous, out string strRemain);
    /// <summary>
    /// Steps through a prepared statement.
    /// </summary>
    /// <param name="stmt">The SQLiteStatement to step through</param>
    /// <returns>True if a row was returned, False if not.</returns>
    internal abstract bool Step(SQLiteStatement stmt);
    /// <summary>
    /// Resets a prepared statement so it can be executed again.  If the error returned is SQLITE_SCHEMA, 
    /// transparently attempt to rebuild the SQL statement and throw an error if that was not possible.
    /// </summary>
    /// <param name="stmt">The statement to reset</param>
    /// <returns>Returns -1 if the schema changed while resetting, 0 if the reset was sucessful or 6 (SQLITE_LOCKED) if the reset failed due to a lock</returns>
    internal abstract int Reset(SQLiteStatement stmt);

    internal abstract void Cancel();

    internal abstract void Bind_Double(SQLiteStatement stmt, int index, double value);
    internal abstract void Bind_Int32(SQLiteStatement stmt, int index, Int32 value);
    internal abstract void Bind_Int64(SQLiteStatement stmt, int index, Int64 value);
    internal abstract void Bind_Text(SQLiteStatement stmt, int index, string value);
    internal abstract void Bind_Blob(SQLiteStatement stmt, int index, byte[] blobData);
    internal abstract void Bind_DateTime(SQLiteStatement stmt, int index, DateTime dt);
    internal abstract void Bind_Null(SQLiteStatement stmt, int index);

    internal abstract int Bind_ParamCount(SQLiteStatement stmt);
    internal abstract string Bind_ParamName(SQLiteStatement stmt, int index);
    internal abstract int Bind_ParamIndex(SQLiteStatement stmt, string paramName);

    internal abstract int ColumnCount(SQLiteStatement stmt);
    internal abstract string ColumnName(SQLiteStatement stmt, int index);
    internal abstract TypeAffinity ColumnAffinity(SQLiteStatement stmt, int index);
    internal abstract string ColumnType(SQLiteStatement stmt, int index, out TypeAffinity nAffinity);
    internal abstract int ColumnIndex(SQLiteStatement stmt, string columnName);
    internal abstract string ColumnOriginalName(SQLiteStatement stmt, int index);
    internal abstract string ColumnDatabaseName(SQLiteStatement stmt, int index);
    internal abstract string ColumnTableName(SQLiteStatement stmt, int index);
    internal abstract void ColumnMetaData(string dataBase, string table, string column, out string dataType, out string collateSequence, out bool notNull, out bool primaryKey, out bool autoIncrement);

    internal abstract double GetDouble(SQLiteStatement stmt, int index);
    internal abstract Int32 GetInt32(SQLiteStatement stmt, int index);
    internal abstract Int64 GetInt64(SQLiteStatement stmt, int index);
    internal abstract string GetText(SQLiteStatement stmt, int index);
    internal abstract long GetBytes(SQLiteStatement stmt, int index, int nDataoffset, byte[] bDest, int nStart, int nLength);
    internal abstract long GetChars(SQLiteStatement stmt, int index, int nDataoffset, char[] bDest, int nStart, int nLength);
    internal abstract DateTime GetDateTime(SQLiteStatement stmt, int index);
    internal abstract bool IsNull(SQLiteStatement stmt, int index);

    internal abstract IntPtr CreateCollation(string strCollation, SQLiteCollation func);
    internal abstract IntPtr CreateFunction(string strFunction, int nArgs, SQLiteCallback func, SQLiteCallback funcstep, SQLiteCallback funcfinal);

    internal abstract int AggregateCount(IntPtr context);
    internal abstract IntPtr AggregateContext(IntPtr context);

    internal abstract long GetParamValueBytes(IntPtr ptr, int nDataOffset, byte[] bDest, int nStart, int nLength);
    internal abstract double GetParamValueDouble(IntPtr ptr);
    internal abstract int GetParamValueInt32(IntPtr ptr);
    internal abstract Int64 GetParamValueInt64(IntPtr ptr);
    internal abstract string GetParamValueText(IntPtr ptr);
    internal abstract TypeAffinity GetParamValueType(IntPtr ptr);

    internal abstract void ReturnBlob(IntPtr context, byte[] value);
    internal abstract void ReturnDouble(IntPtr context, double value);
    internal abstract void ReturnError(IntPtr context, string value);
    internal abstract void ReturnInt32(IntPtr context, Int32 value);
    internal abstract void ReturnInt64(IntPtr context, Int64 value);
    internal abstract void ReturnNull(IntPtr context);
    internal abstract void ReturnText(IntPtr context, string value);

    internal abstract void SetPassword(byte[] passwordBytes);
    internal abstract void ChangePassword(byte[] newPasswordBytes);

    internal abstract void SetUpdateHook(SQLiteUpdateCallback func);
    internal abstract void SetCommitHook(SQLiteCommitCallback func);
    internal abstract void SetRollbackHook(SQLiteRollbackCallback func);

    internal abstract int GetCursorForTable(SQLiteStatement stmt, int database, int rootPage);
    internal abstract long GetRowIdForCursor(SQLiteStatement stmt, int cursor);
    internal abstract void DetachAll();

    internal abstract object GetValue(SQLiteStatement stmt, int index, SQLiteType typ);

    protected virtual void Dispose(bool bDisposing)
    {
    }

    public void Dispose()
    {
      Dispose(true);
    }

    // These statics are here for lack of a better place to put them.
    // They exist here because they are called during the finalization of
    // a SQLiteStatementHandle, SQLiteConnectionHandle, and SQLiteFunctionCookieHandle.
    // Therefore these functions have to be static, and have to be low-level.

    internal static string SQLiteLastError(SQLiteConnectionHandle db)
    {
      int len;
      return UTF8ToString(UnsafeNativeMethods.sqlite3_errmsg_interop(db, out len), len);
    }

    internal static string SQLiteLastError(SQLiteStatementHandle stmt)
    {
      int len;
      return UTF8ToString(UnsafeNativeMethods.sqlite3_errmsg_stmt_interop(stmt, out len), len);
    }

    internal static void FinalizeStatement(SQLiteStatementHandle stmt)
    {
      int n = UnsafeNativeMethods.sqlite3_finalize_interop(stmt);
      if (n > 0) throw new SQLiteException(n, SQLiteLastError(stmt));
    }

    internal static void CloseConnection(SQLiteConnectionHandle db)
    {
      int n = UnsafeNativeMethods.sqlite3_close_interop(db);
      if (n > 0) throw new SQLiteException(n, SQLiteLastError(db));
    }

    internal static void FreeFunction(IntPtr nCookie)
    {
      UnsafeNativeMethods.sqlite3_function_free_callbackcookie(nCookie);
    }

  }
}
