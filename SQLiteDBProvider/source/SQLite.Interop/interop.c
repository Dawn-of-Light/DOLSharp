//#include "../temp/sqlite.interop/src/sqlite3.c"
#include "src/sqlite3.c"
#include "crypt.c"
#include <tchar.h>

#if NDEBUG

#if _WIN32_WCE
#include "merge.h"
#else
#include "merge_full.h"
#endif // _WIN32_WCE
#endif // NDEBUG

#ifdef OS_WIN

// Additional flag for sqlite3.flags, we use it as a reference counter
#define SQLITE_WantClose 0x10000000

typedef void (WINAPI *SQLITEUSERFUNC)(void *, int, void **);
typedef int  (WINAPI *SQLITECOLLATION)(int, const void *, int, const void*);

typedef void (WINAPI *SQLITEUPDATEHOOK)(int, const char *, int, const char *, int, sqlite_int64);
typedef int  (WINAPI *SQLITECOMMITHOOK)();
typedef void (WINAPI *SQLITEROLLBACKHOOK)();

typedef HANDLE (WINAPI *CREATEFILEW)(
    LPCWSTR,
    DWORD,
    DWORD,
    LPSECURITY_ATTRIBUTES,
    DWORD,
    DWORD,
    HANDLE);

// Callback wrappers
int sqlite3_interop_collationfunc(void *pv, int len1, const void *pv1, int len2, const void *pv2)
{
  SQLITECOLLATION *p = (SQLITECOLLATION *)pv;
  return p[0](len1, pv1, len2, pv2);
}

void sqlite3_interop_func(sqlite3_context *pctx, int n, sqlite3_value **pv)
{
  SQLITEUSERFUNC *pf = (SQLITEUSERFUNC *)sqlite3_user_data(pctx);
  pf[0](pctx, n, (void **)pv);
}

void sqlite3_interop_step(sqlite3_context *pctx, int n, sqlite3_value **pv)
{
  SQLITEUSERFUNC *pf = (SQLITEUSERFUNC *)sqlite3_user_data(pctx);
  pf[1](pctx, n, (void **)pv);
}

void sqlite3_interop_final(sqlite3_context *pctx)
{
  SQLITEUSERFUNC *pf = (SQLITEUSERFUNC *)sqlite3_user_data(pctx);
  pf[2](pctx, 0, 0);
}

__declspec(dllexport) void WINAPI sqlite3_sleep_interop(int milliseconds)
{
  Sleep(milliseconds);
}

void InitializeDbMutex(sqlite3 *pdb)
{
  //pdb->pTraceArg = (CRITICAL_SECTION *)sqlite3_malloc(sizeof(CRITICAL_SECTION));
  //InitializeCriticalSection(pdb->pTraceArg);
}

void EnterDbMutex(sqlite3 *pdb)
{
  //if (pdb->pTraceArg)
  //{
  //  EnterCriticalSection(pdb->pTraceArg);
  //}
}

void LeaveDbMutex(sqlite3 *pdb)
{
  //if (pdb->pTraceArg)
  //{
  //  LeaveCriticalSection(pdb->pTraceArg);
  //}
}

int sqlite3_closeAndFreeMutex(sqlite3 *db)
{
  //CRITICAL_SECTION *pcrit = db->pTraceArg;
  int ret;
  //EnterDbMutex(db);

  ret = sqlite3_close(db);
  //if (ret == SQLITE_OK)
  //{
  //  if (pcrit)
  //  {
  //    LeaveCriticalSection(pcrit);
  //    DeleteCriticalSection(pcrit);
  //    sqlite3_free(pcrit);
  //  }
  //}
  //else
  //  LeaveDbMutex(db);

  return ret;
}

int SetCompression(const wchar_t *pwszFilename, unsigned short ufLevel)
{
#ifdef FSCTL_SET_COMPRESSION
  HMODULE hMod = GetModuleHandle(_T("KERNEL32"));
  CREATEFILEW pfunc;
  HANDLE hFile;
  unsigned long dw = 0;
  int n;

  if (hMod == NULL)
  {
    SetLastError(ERROR_NOT_SUPPORTED);
    return 0;
  }

  pfunc = (CREATEFILEW)GetProcAddress(hMod, _T("CreateFileW"));
  if (pfunc == NULL)
  {
    SetLastError(ERROR_NOT_SUPPORTED);
    return 0;
  }

  hFile = pfunc(pwszFilename, GENERIC_READ|GENERIC_WRITE, 0, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
  if (hFile == NULL)
    return 0;

  n = DeviceIoControl(hFile, FSCTL_SET_COMPRESSION, &ufLevel, sizeof(ufLevel), NULL, 0, &dw, NULL);

  CloseHandle(hFile);

  return n;
#else
  SetLastError(ERROR_NOT_SUPPORTED);
  return 0;
#endif
}

__declspec(dllexport) int WINAPI sqlite3_compressfile(const wchar_t *pwszFilename)
{
  return SetCompression(pwszFilename, COMPRESSION_FORMAT_DEFAULT);
}

__declspec(dllexport) int WINAPI sqlite3_decompressfile(const wchar_t *pwszFilename)
{
  return SetCompression(pwszFilename, COMPRESSION_FORMAT_NONE);
}

__declspec(dllexport) void WINAPI sqlite3_function_free_callbackcookie(void *pCookie)
{
  if (pCookie)
    free(pCookie);
}

// sqlite3 wrappers
__declspec(dllexport) const char * WINAPI sqlite3_libversion_interop(int *plen)
{
  const char *val = sqlite3_libversion();
  *plen = (val != 0) ? strlen(val) : 0;

  return val;
}

__declspec(dllexport) int WINAPI sqlite3_libversion_number_interop(void)
{
  return sqlite3_libversion_number();
}

/*
    The goal of this version of close is different than that of sqlite3_close(), and is designed to lend itself better to .NET's non-deterministic finalizers and
    the GC thread.  SQLite will not close a database if statements are open on it -- but for our purposes, we'd rather finalize all active statements
    and forcibly close the database.  The reason is simple -- a lot of people don't Dispose() of their objects correctly and let the garbage collector
    do it.  This leads to unexpected behavior when a user thinks they've closed a database, but it's still open because not all the statements have
    hit the GC yet.

    So, here we have a problem ... .NET has a pointer to any number of sqlite3_stmt objects.  We can't call sqlite3_finalize() on these because
    their memory is freed and can be used for something else.  The GC thread could potentially try and call finalize again on the statement after
    that memory was deallocated.  BAD.  So, what we need to do is make a copy of each statement, and call finalize() on the copy -- so that the original
    statement's memory is preserved, and marked as BAD, but we can still manage to finalize everything and forcibly close the database.  Later when the 
    GC gets around to calling finalize_interop() on the "bad" statement, we detect that and finish deallocating the pointer.
*/
__declspec(dllexport) int WINAPI sqlite3_close_interop(sqlite3 *db)
{
  int ret = sqlite3_closeAndFreeMutex(db);

  if (ret == SQLITE_BUSY && db->pVdbe)
  {
    while (db->pVdbe)
    {
      // Make a copy of the first prepared statement
      Vdbe *p = (Vdbe *)sqlite3_malloc(sizeof(Vdbe));
      Vdbe *po = db->pVdbe;

      if (!p) return SQLITE_NOMEM;

      CopyMemory(p, po, sizeof(Vdbe));

      // Put it on the chain so we can free it
      db->pVdbe = p;
      ret = sqlite3_finalize((sqlite3_stmt *)p); // This will also free the copy's memory
      if (ret)
      {
        // finalize failed -- so we must put back anything we munged
        CopyMemory(po, p, sizeof(Vdbe));
        db->pVdbe = po;
        break;
      }
      else
      {
        ZeroMemory(po, sizeof(Vdbe));
        po->magic = VDBE_MAGIC_DEAD;
      }
    }
    ret = sqlite3_closeAndFreeMutex(db);
  }

  return ret;
}

// Returns the number of databases attached to this one
__declspec(dllexport) void WINAPI sqlite3_detach_all_interop(sqlite3 *db)
{
}

__declspec(dllexport) int WINAPI sqlite3_exec_interop(sqlite3 *db, const char *sql, sqlite3_callback cb, void *pv, char **errmsg, int *plen)
{ 
  int n;

  EnterDbMutex(db);
  n = sqlite3_exec(db, sql, cb, pv, errmsg);
  *plen = (*errmsg != 0) ? strlen(*errmsg) : 0;
  LeaveDbMutex(db);

  return n;
}

__declspec(dllexport) sqlite_int64 WINAPI sqlite3_last_insert_rowid_interop(sqlite3 *db)
{
  return sqlite3_last_insert_rowid(db);
}

__declspec(dllexport) int WINAPI sqlite3_changes_interop(sqlite3 *db)
{
  return sqlite3_changes(db);
}

__declspec(dllexport) int WINAPI sqlite3_total_changes_interop(sqlite3 *db)
{
  return sqlite3_total_changes(db);
}

__declspec(dllexport) void WINAPI sqlite3_interrupt_interop(sqlite3 *db)
{
  sqlite3_interrupt(db);
}

__declspec(dllexport) int WINAPI sqlite3_complete_interop(const char *sql)
{
  return sqlite3_complete(sql);
}

__declspec(dllexport) int WINAPI sqlite3_complete16_interop(const void *sql)
{
  return sqlite3_complete16(sql);
}

__declspec(dllexport) int WINAPI sqlite3_busy_handler_interop(sqlite3 *db, int(*cb)(void *, int), void *pv)
{
  return sqlite3_busy_handler(db, cb, pv);
}

__declspec(dllexport) int WINAPI sqlite3_busy_timeout_interop(sqlite3 *db, int ms)
{
  return sqlite3_busy_timeout(db, ms);
}

__declspec(dllexport) int WINAPI sqlite3_get_table_interop(sqlite3 *db, const char *sql, char ***resultp, int *nrow, int *ncolumn, char **errmsg, int *plen)
{
  int n = sqlite3_get_table(db, sql, resultp, nrow, ncolumn, errmsg);
  *plen = (*errmsg != 0) ? strlen((char *)*errmsg) : 0;
  return n;
}

__declspec(dllexport) void WINAPI sqlite3_free_table_interop(char **result)
{
  sqlite3_free_table(result);
}

__declspec(dllexport) void WINAPI sqlite3_free_interop(char *z)
{
  sqlite3_free(z);
}

__declspec(dllexport) int WINAPI sqlite3_open_interop(const char*filename, sqlite3 **ppdb)
{
  int ret = sqlite3_open(filename, ppdb);
  if (ret == SQLITE_OK) InitializeDbMutex(*ppdb);

  return ret;
}

__declspec(dllexport) int WINAPI sqlite3_open16_interop(const void *filename, sqlite3 **ppdb)
{
  int ret = sqlite3_open16(filename, ppdb);
  if (ret == SQLITE_OK) InitializeDbMutex(*ppdb);

  return ret;
}

__declspec(dllexport) int WINAPI sqlite3_errcode_interop(sqlite3 *db)
{
  return sqlite3_errcode(db);
}

__declspec(dllexport) const char * WINAPI sqlite3_errmsg_interop(sqlite3 *db, int *plen)
{
  const char *pval = sqlite3_errmsg(db);
  *plen = (pval != 0) ? strlen(pval) : 0;
  return pval;
}

__declspec(dllexport) const char * WINAPI sqlite3_errmsg_stmt_interop(sqlite3_stmt *stmt, int *plen)
{
  Vdbe *p = (Vdbe *)stmt;

  return sqlite3_errmsg_interop(p->db, plen);
}

__declspec(dllexport) int WINAPI sqlite3_prepare_interop(sqlite3 *db, const char *sql, int nbytes, sqlite3_stmt **ppstmt, const char **pztail, int *plen)
{
  int n;

  EnterDbMutex(db);
  n = sqlite3_prepare(db, sql, nbytes, ppstmt, pztail);
  *plen = (*pztail != 0) ? strlen(*pztail) : 0;
  LeaveDbMutex(db);
  return n;
}

__declspec(dllexport) int WINAPI sqlite3_prepare16_interop(sqlite3 *db, const void *sql, int nchars, sqlite3_stmt **ppstmt, const void **pztail, int *plen)
{
  int n;

  EnterDbMutex(db);
  n = sqlite3_prepare16(db, sql, nchars * sizeof(wchar_t), ppstmt, pztail);
  *plen = (*pztail != 0) ? wcslen((wchar_t *)*pztail) * sizeof(wchar_t) : 0;
  LeaveDbMutex(db);
  return n;
}

__declspec(dllexport) int WINAPI sqlite3_bind_blob_interop(sqlite3_stmt *stmt, int iCol, const void *pv, int n, void(*cb)(void*))
{
  return sqlite3_bind_blob(stmt, iCol, pv, n, cb);
}

__declspec(dllexport) int WINAPI sqlite3_bind_double_interop(sqlite3_stmt *stmt, int iCol, double *val)
{
	return sqlite3_bind_double(stmt,iCol,*val);
}

__declspec(dllexport) int WINAPI sqlite3_bind_int_interop(sqlite3_stmt *stmt, int iCol, int val)
{
  return sqlite3_bind_int(stmt, iCol, val);
}

__declspec(dllexport) int WINAPI sqlite3_bind_int64_interop(sqlite3_stmt *stmt, int iCol, sqlite_int64 *val)
{
	return sqlite3_bind_int64(stmt,iCol,*val);
}

__declspec(dllexport) int WINAPI sqlite3_bind_null_interop(sqlite3_stmt *stmt, int iCol)
{
  return sqlite3_bind_null(stmt, iCol);
}

__declspec(dllexport) int WINAPI sqlite3_bind_text_interop(sqlite3_stmt *stmt, int iCol, const char *val, int n, void(*cb)(void *))
{
  return sqlite3_bind_text(stmt, iCol, val, n, cb);
}

__declspec(dllexport) int WINAPI sqlite3_bind_text16_interop(sqlite3_stmt *stmt, int iCol, const void *val, int n, void(*cb)(void *))
{
  return sqlite3_bind_text16(stmt, iCol, val, n, cb);
}

__declspec(dllexport) int WINAPI sqlite3_bind_parameter_count_interop(sqlite3_stmt *stmt)
{
  return sqlite3_bind_parameter_count(stmt);
}

__declspec(dllexport) const char * WINAPI sqlite3_bind_parameter_name_interop(sqlite3_stmt *stmt, int iCol, int *plen)
{
  const char *pval = sqlite3_bind_parameter_name(stmt, iCol);
  *plen = (pval != 0) ? strlen(pval) : 0;
  return pval;
}

__declspec(dllexport) int WINAPI sqlite3_bind_parameter_index_interop(sqlite3_stmt *stmt, const char *zName)
{
  return sqlite3_bind_parameter_index(stmt, zName);
}

__declspec(dllexport) int WINAPI sqlite3_column_count_interop(sqlite3_stmt *stmt)
{
  return sqlite3_column_count(stmt);
}

__declspec(dllexport) const char * WINAPI sqlite3_column_name_interop(sqlite3_stmt *stmt, int iCol, int *plen)
{
  const char *pval = sqlite3_column_name(stmt, iCol);
  *plen = (pval != 0) ? strlen(pval) : 0;
  return pval;
}

__declspec(dllexport) const void * WINAPI sqlite3_column_name16_interop(sqlite3_stmt *stmt, int iCol, int *plen)
{
  const void *pval = sqlite3_column_name16(stmt, iCol);
  *plen = (pval != 0) ? wcslen((wchar_t *)pval) * sizeof(wchar_t) : 0;
  return pval;
}

__declspec(dllexport) const char * WINAPI sqlite3_column_decltype_interop(sqlite3_stmt *stmt, int iCol, int *plen)
{
  const char *pval = sqlite3_column_decltype(stmt, iCol);
  *plen = (pval != 0) ? strlen(pval) : 0;
  return pval;
}

__declspec(dllexport) const void * WINAPI sqlite3_column_decltype16_interop(sqlite3_stmt *stmt, int iCol, int *plen)
{
  const void *pval = sqlite3_column_decltype16(stmt, iCol);
  *plen = (pval != 0) ? wcslen((wchar_t *)pval) * sizeof(wchar_t) : 0;
  return pval;
}

__declspec(dllexport) int WINAPI sqlite3_step_interop(sqlite3_stmt *stmt)
{
  int ret;

  EnterDbMutex(((Vdbe *)stmt)->db);

  if (((Vdbe *)stmt)->magic == VDBE_MAGIC_DEAD) return SQLITE_ERROR;
  ret = sqlite3_step(stmt);
  LeaveDbMutex(((Vdbe *)stmt)->db);

  return ret;
}

__declspec(dllexport) int WINAPI sqlite3_data_count_interop(sqlite3_stmt *stmt)
{
  return sqlite3_data_count(stmt);
}

__declspec(dllexport) const void * WINAPI sqlite3_column_blob_interop(sqlite3_stmt *stmt, int iCol)
{
  return sqlite3_column_blob(stmt, iCol);
}

__declspec(dllexport) int WINAPI sqlite3_column_bytes_interop(sqlite3_stmt *stmt, int iCol)
{
  return sqlite3_column_bytes(stmt, iCol);
}

__declspec(dllexport) int WINAPI sqlite3_column_bytes16_interop(sqlite3_stmt *stmt, int iCol)
{
  return sqlite3_column_bytes16(stmt, iCol);
}

__declspec(dllexport) void WINAPI sqlite3_column_double_interop(sqlite3_stmt *stmt, int iCol, double *val)
{
	*val = sqlite3_column_double(stmt,iCol);
}

__declspec(dllexport) int WINAPI sqlite3_column_int_interop(sqlite3_stmt *stmt, int iCol)
{
  return sqlite3_column_int(stmt, iCol);
}

__declspec(dllexport) void WINAPI sqlite3_column_int64_interop(sqlite3_stmt *stmt, int iCol, sqlite_int64 *val)
{
	*val = sqlite3_column_int64(stmt,iCol);
}

__declspec(dllexport) const unsigned char * WINAPI sqlite3_column_text_interop(sqlite3_stmt *stmt, int iCol, int *plen)
{
  const unsigned char *pval = sqlite3_column_text(stmt, iCol);
  *plen = (pval != 0) ? strlen((char *)pval) : 0;
  return pval;
}

__declspec(dllexport) const void * WINAPI sqlite3_column_text16_interop(sqlite3_stmt *stmt, int iCol, int *plen)
{
  const void *pval = sqlite3_column_text16(stmt, iCol);
  *plen = (pval != 0) ? wcslen((wchar_t *)pval) * sizeof(wchar_t): 0;
  return pval;
}

__declspec(dllexport) int WINAPI sqlite3_column_type_interop(sqlite3_stmt *stmt, int iCol)
{
  return sqlite3_column_type(stmt, iCol);
}

__declspec(dllexport) int WINAPI sqlite3_finalize_interop(sqlite3_stmt *stmt)
{
  // Try and finalize a statement, and close the database it belonged to if the database was marked for closing
  Vdbe *p = (Vdbe *)stmt;
  sqlite3 *db = (p == NULL) ? NULL : p->db;
  int ret;

  if (p->magic == VDBE_MAGIC_DEAD && p->db == NULL)
  {
    sqlite3_free(p);
    return SQLITE_OK;
  }
  EnterDbMutex(db);
  ret = sqlite3_finalize(stmt);
  LeaveDbMutex(db);

  if (ret == SQLITE_OK)
  {
    if (db->flags & SQLITE_WantClose)
    {
      if (db->pVdbe == NULL)
      {
        sqlite3_closeAndFreeMutex(db);
      }
    }
  }

  return ret;
}

__declspec(dllexport) int WINAPI sqlite3_reset_interop(sqlite3_stmt *stmt)
{
  int ret;

  EnterDbMutex(((Vdbe *)stmt)->db);
  if (((Vdbe *)stmt)->magic == VDBE_MAGIC_DEAD) return SQLITE_SCHEMA;
  ret = sqlite3_reset(stmt);
  LeaveDbMutex(((Vdbe *)stmt)->db);
  return ret;
}

__declspec(dllexport) int WINAPI sqlite3_create_function_interop(sqlite3 *psql, const char *zFunctionName, int nArg, int eTextRep, SQLITEUSERFUNC func, SQLITEUSERFUNC funcstep, SQLITEUSERFUNC funcfinal, void **ppCookie)
{
  int n;
  SQLITEUSERFUNC *p = (SQLITEUSERFUNC *)malloc(sizeof(SQLITEUSERFUNC) * 3);

  p[0] = func;
  p[1] = funcstep;
  p[2] = funcfinal;

  *ppCookie = 0;

  n = sqlite3_create_function(psql, zFunctionName, nArg, eTextRep, p, (func != 0) ? sqlite3_interop_func : 0, (funcstep != 0) ? sqlite3_interop_step : 0, (funcfinal != 0) ? sqlite3_interop_final : 0);
  if (n != 0)
    free(p);
  else
    *ppCookie = p;

  return n;
}

__declspec(dllexport) int WINAPI sqlite3_create_function16_interop(sqlite3 *psql, void *zFunctionName, int nArg, int eTextRep, SQLITEUSERFUNC func, SQLITEUSERFUNC funcstep, SQLITEUSERFUNC funcfinal, void **ppCookie)
{
  int n;
  SQLITEUSERFUNC *p = (SQLITEUSERFUNC *)malloc(sizeof(SQLITEUSERFUNC) * 3);

  p[0] = func;
  p[1] = funcstep;
  p[2] = funcfinal;

  *ppCookie = 0;

  n = sqlite3_create_function16(psql, zFunctionName, nArg, eTextRep, p, (func != 0) ? sqlite3_interop_func : 0, (funcstep != 0) ? sqlite3_interop_step : 0, (funcfinal != 0) ? sqlite3_interop_final : 0);
  if (n != 0)
    free(p);
  else
    *ppCookie = p;

  return n;
}

__declspec(dllexport) int WINAPI sqlite3_create_collation_interop(sqlite3* db, const char *zName, int eTextRep, void* pvUser, SQLITECOLLATION func, void **ppCookie)
{
  int n;
  SQLITECOLLATION *p = (SQLITECOLLATION *)malloc(sizeof(SQLITECOLLATION));
  
  p[0] = func;

  *ppCookie = 0;

  n = sqlite3_create_collation(db, zName, eTextRep, p, sqlite3_interop_collationfunc);
  if (n != 0)
    free(p);
  else
    *ppCookie = p;

  return n;
}

__declspec(dllexport) int WINAPI sqlite3_create_collation16_interop(sqlite3* db, const void *zName, int eTextRep, void* pvUser, SQLITECOLLATION func, void **ppCookie)
{
  int n;
  SQLITECOLLATION *p = (SQLITECOLLATION *)malloc(sizeof(SQLITECOLLATION));
  
  p[0] = func;

  *ppCookie = 0;

  n = sqlite3_create_collation16(db, (const char *)zName, eTextRep, p, sqlite3_interop_collationfunc);
  if (n != 0)
    free(p);
  else
    *ppCookie = p;

  return n;
}

__declspec(dllexport) int WINAPI sqlite3_aggregate_count_interop(sqlite3_context *pctx)
{
  return sqlite3_aggregate_count(pctx);
}

__declspec(dllexport) const void * WINAPI sqlite3_value_blob_interop(sqlite3_value *val)
{
  return sqlite3_value_blob(val);
}

__declspec(dllexport) int WINAPI sqlite3_value_bytes_interop(sqlite3_value *val)
{
  return sqlite3_value_bytes(val);
}

__declspec(dllexport) int WINAPI sqlite3_value_bytes16_interop(sqlite3_value *val)
{
  return sqlite3_value_bytes16(val);
}

__declspec(dllexport) void WINAPI sqlite3_value_double_interop(sqlite3_value *pval, double *val)
{
  *val = sqlite3_value_double(pval);
}

__declspec(dllexport) int WINAPI sqlite3_value_int_interop(sqlite3_value *val)
{
  return sqlite3_value_int(val);
}

__declspec(dllexport) void WINAPI sqlite3_value_int64_interop(sqlite3_value *pval, sqlite_int64 *val)
{
  *val = sqlite3_value_int64(pval);
}

__declspec(dllexport) const unsigned char * WINAPI sqlite3_value_text_interop(sqlite3_value *val, int *plen)
{
  const unsigned char *pval = sqlite3_value_text(val);
  *plen = (pval != 0) ? strlen((char *)pval) : 0;
  return pval;
}

__declspec(dllexport) const void * WINAPI sqlite3_value_text16_interop(sqlite3_value *val, int *plen)
{
  const void *pval = sqlite3_value_text16(val);
  *plen = (pval != 0) ? wcslen((wchar_t *)pval) * sizeof(wchar_t) : 0;
  return pval;
}

__declspec(dllexport) int WINAPI sqlite3_value_type_interop(sqlite3_value *val)
{
  return sqlite3_value_type(val);
}

__declspec(dllexport) void * WINAPI sqlite3_aggregate_context_interop(sqlite3_context *pctx, int n)
{
  return sqlite3_aggregate_context(pctx, n);
}

__declspec(dllexport) void WINAPI sqlite3_result_blob_interop(sqlite3_context *ctx, const void *pv, int n, void(*cb)(void *))
{
  sqlite3_result_blob(ctx, pv, n, cb);
}

__declspec(dllexport) void WINAPI sqlite3_result_double_interop(sqlite3_context *pctx, double *val)
{
  sqlite3_result_double(pctx, *val);
}

__declspec(dllexport) void WINAPI sqlite3_result_int_interop(sqlite3_context *pctx, int val)
{
  sqlite3_result_int(pctx, val);
}

__declspec(dllexport) void WINAPI sqlite3_result_int64_interop(sqlite3_context *pctx, sqlite_int64 *val)
{
  sqlite3_result_int64(pctx, *val);
}

__declspec(dllexport) void WINAPI sqlite3_result_null_interop(sqlite3_context *pctx)
{
  sqlite3_result_null(pctx);
}

__declspec(dllexport) void WINAPI sqlite3_result_error_interop(sqlite3_context *ctx, const char *pv, int n)
{
  sqlite3_result_error(ctx, pv, n);
}

__declspec(dllexport) void WINAPI sqlite3_result_error16_interop(sqlite3_context *ctx, const void *pv, int n)
{
  sqlite3_result_error16(ctx, pv, n);
}

__declspec(dllexport) void WINAPI sqlite3_result_text_interop(sqlite3_context *ctx, const char *pv, int n, void(*cb)(void *))
{
  sqlite3_result_text(ctx, pv, n, cb);
}

__declspec(dllexport) void WINAPI sqlite3_result_text16_interop(sqlite3_context *ctx, const void *pv, int n, void(*cb)(void *))
{
  sqlite3_result_text16(ctx, pv, n, cb);
}

__declspec(dllexport) const char * WINAPI sqlite3_column_database_name_interop(sqlite3_stmt *stmt, int iCol, int *plen)
{
  const char *pval = sqlite3_column_database_name(stmt, iCol);
  *plen = (pval != 0) ? strlen(pval) : 0;
  return pval;
}

__declspec(dllexport) const void * WINAPI sqlite3_column_database_name16_interop(sqlite3_stmt *stmt, int iCol, int *plen)
{
  const void *pval = sqlite3_column_database_name16(stmt, iCol);
  *plen = (pval != 0) ? wcslen((wchar_t *)pval) * sizeof(wchar_t) : 0;
  return pval;
}

__declspec(dllexport) const char * WINAPI sqlite3_column_table_name_interop(sqlite3_stmt *stmt, int iCol, int *plen)
{
  const char *pval = sqlite3_column_table_name(stmt, iCol);
  *plen = (pval != 0) ? strlen(pval) : 0;
  return pval;
}

__declspec(dllexport) const void * WINAPI sqlite3_column_table_name16_interop(sqlite3_stmt *stmt, int iCol, int *plen)
{
  const void *pval = sqlite3_column_table_name16(stmt, iCol);
  *plen = (pval != 0) ? wcslen((wchar_t *)pval) * sizeof(wchar_t) : 0;
  return pval;
}

__declspec(dllexport) const char * WINAPI sqlite3_column_origin_name_interop(sqlite3_stmt *stmt, int iCol, int *plen)
{
  const char *pval = sqlite3_column_origin_name(stmt, iCol);
  *plen = (pval != 0) ? strlen(pval) : 0;
  return pval;
}

__declspec(dllexport) const void * WINAPI sqlite3_column_origin_name16_interop(sqlite3_stmt *stmt, int iCol, int *plen)
{
  const void *pval = sqlite3_column_origin_name16(stmt, iCol);
  *plen = (pval != 0) ? wcslen((wchar_t *)pval) * sizeof(wchar_t) : 0;
  return pval;
}

__declspec(dllexport) int WINAPI sqlite3_table_column_metadata_interop(sqlite3 *db, const char *zDbName, const char *zTableName, const char *zColumnName, char **pzDataType, char **pzCollSeq, int *pNotNull, int *pPrimaryKey, int *pAutoinc, int *pdtLen, int *pcsLen)
{
  int n;
  
  EnterDbMutex(db);
  n = sqlite3_table_column_metadata(db, zDbName, zTableName, zColumnName, pzDataType, pzCollSeq, pNotNull, pPrimaryKey, pAutoinc);
  *pdtLen = (*pzDataType != 0) ? strlen(*pzDataType) : 0;
  *pcsLen = (*pzCollSeq != 0) ? strlen(*pzCollSeq) : 0;
  LeaveDbMutex(db);

  return n;
}

void sqlite3_update_callback(void *pArg, int type, const char *pDatabase, const char *pTable, sqlite_int64 rowid)
{
  SQLITEUPDATEHOOK func = (SQLITEUPDATEHOOK)pArg;

  func(type, pDatabase, strlen(pDatabase), pTable, strlen(pTable), rowid);
}

int sqlite3_commit_callback(void *pArg)
{
  return ((SQLITECOMMITHOOK)pArg)();
}

void sqlite3_rollback_callback(void *pArg)
{
  ((SQLITEROLLBACKHOOK)pArg)();
}

__declspec(dllexport) void * WINAPI sqlite3_update_hook_interop(sqlite3 *pDb, SQLITEUPDATEHOOK func)
{
  return sqlite3_update_hook(pDb, sqlite3_update_callback, func);
}

__declspec(dllexport) void * WINAPI sqlite3_commit_hook_interop(sqlite3 *pDb, SQLITECOMMITHOOK func)
{
  return sqlite3_commit_hook(pDb, sqlite3_commit_callback, func);
}

__declspec(dllexport) void * WINAPI sqlite3_rollback_hook_interop(sqlite3 *pDb, SQLITEROLLBACKHOOK func)
{
  return sqlite3_rollback_hook(pDb, sqlite3_rollback_callback, func);
}

__declspec(dllexport) int WINAPI sqlite3_table_cursor(sqlite3_stmt *pstmt, int iDb, Pgno tableRootPage)
{
  Vdbe *p = (Vdbe *)pstmt;
  sqlite3 *db = (p == NULL) ? NULL : p->db;
  int n;
  int ret = -1;

  EnterDbMutex(db);
  for (n = 0; n < p->nCursor && p->apCsr[n] != NULL; n++)
  {
    if (p->apCsr[n]->isTable == FALSE) continue;
    if (p->apCsr[n]->iDb != iDb) continue;
    if (p->apCsr[n]->pCursor->pgnoRoot == tableRootPage)
    {
      ret = n;
      break;
    }
  }
  LeaveDbMutex(db);
  return ret;
}

__declspec(dllexport) int WINAPI sqlite3_cursor_rowid(sqlite3_stmt *pstmt, int cursor, sqlite_int64 *prowid)
{
  Vdbe *p = (Vdbe *)pstmt;
  sqlite3 *db = (p == NULL) ? NULL : p->db;
  int rc = 0;
  Cursor *pC;
  int ret = 0;

  EnterDbMutex(db);
  while (1)
  {
    if (cursor < 0 || cursor >= p->nCursor)
    {
      ret = SQLITE_ERROR;
      break;
    }
    if (p->apCsr[cursor] == NULL)
    {
      ret = SQLITE_ERROR;
      break;
    }

    pC = p->apCsr[cursor];

    ret = sqlite3VdbeCursorMoveto(pC);
    if(ret)
      break;

    if(pC->rowidIsValid)
    {
      *prowid = pC->lastRowid;
    }
    else if(pC->pseudoTable)
    {
      *prowid = keyToInt(pC->iKey);
    }
    else if(pC->nullRow || pC->pCursor==0)
    {
      ret = SQLITE_ERROR;
      break;
    }
    else
    {
      if (pC->pCursor == NULL)
      {
        ret = SQLITE_ERROR;
        break;
      }
      sqlite3BtreeKeySize(pC->pCursor, prowid);
      *prowid = keyToInt(*prowid);
    }
    break;
  }
  LeaveDbMutex(db);

  return ret;
}


// IMPORTANT: This placeholder is here for a reason!!!
// On the Compact Framework the .data section of the DLL must have its RawDataSize larger than the VirtualSize!
// If its not, strong name validation will fail and other bad things will happen.
#if _WIN32_WCE
__int64 _ph[128] = {1};
#endif // _WIN32_WCE

#endif // OS_WIN
