#ifndef SQLITE_OMIT_DISKIO
#ifdef SQLITE_HAS_CODEC

#include <windows.h>
#include <wincrypt.h>

// Extra padding before and after the cryptographic buffer
#define CRYPT_OFFSET 8

typedef struct _CRYPTBLOCK
{
  Pager    *pPager;       // Pager this cryptblock belongs to
  HCRYPTKEY hReadKey;     // Key used to read from the database and write to the journal
  HCRYPTKEY hWriteKey;    // Key used to write to the database
  DWORD     dwPageSize;   // Size of pages
  LPVOID    pvCrypt;      // A buffer for encrypting/decrypting (if necessary)
  DWORD     dwCryptSize;  // Equal to or greater than dwPageSize.  If larger, pvCrypt is valid and this is its size
} CRYPTBLOCK, *LPCRYPTBLOCK;

HCRYPTPROV g_hProvider = 0; // Global instance of the cryptographic provider

#define SQLITECRYPTERROR_PROVIDER "Cryptographic provider not available"

// Needed for re-keying
static void * sqlite3pager_get_codecarg(Pager *pPager)
{
  return (pPager->xCodec) ? pPager->pCodecArg: NULL;
}

void sqlite3_activate_see(const char *info)
{
}

// Create a cryptographic context.  Use the enhanced provider because it is available on
// most platforms
static BOOL InitializeProvider()
{
  if (g_hProvider) return TRUE;

  if (!CryptAcquireContext(&g_hProvider, NULL, MS_ENHANCED_PROV, PROV_RSA_FULL, CRYPT_VERIFYCONTEXT))
  {
    return FALSE;
  }
  return TRUE;
}

// Create or update a cryptographic context for a pager.
// This function will automatically determine if the encryption algorithm requires
// extra padding, and if it does, will create a temp buffer big enough to provide
// space to hold it.
static LPCRYPTBLOCK CreateCryptBlock(HCRYPTKEY hKey, Pager *pager, LPCRYPTBLOCK pExisting)
{
  LPCRYPTBLOCK pBlock;

  if (!pExisting) // Creating a new cryptblock
  {
    pBlock = sqlite3_malloc(sizeof(CRYPTBLOCK));
    ZeroMemory(pBlock, sizeof(CRYPTBLOCK));
    pBlock->hReadKey = hKey;
    pBlock->hWriteKey = hKey;
  }
  else // Updating an existing cryptblock
  {
    pBlock = pExisting;
  }

  pBlock->pPager = pager;
  pBlock->dwPageSize = (DWORD)pager->pageSize;
  pBlock->dwCryptSize = pBlock->dwPageSize;

  // Existing cryptblocks may have a buffer, if so, delete it
  if (pBlock->pvCrypt)
  {
    sqlite3_free(pBlock->pvCrypt);
    pBlock->pvCrypt = NULL;
  }

  // Figure out how big to make our spare crypt block
  CryptEncrypt(hKey, 0, TRUE, 0, NULL, &pBlock->dwCryptSize, pBlock->dwCryptSize * 2);
  pBlock->pvCrypt = sqlite3_malloc(pBlock->dwCryptSize + (CRYPT_OFFSET * 2));

  return pBlock;
}

// Destroy a cryptographic context and any buffers and keys allocated therein
static void DestroyCryptBlock(LPCRYPTBLOCK pBlock)
{
  // Destroy the read key if there is one
  if (pBlock->hReadKey)
  {
    CryptDestroyKey(pBlock->hReadKey);
  }

  // If there's a writekey and its not equal to the readkey, destroy it
  if (pBlock->hWriteKey && pBlock->hWriteKey != pBlock->hReadKey)
  {
    CryptDestroyKey(pBlock->hWriteKey);
  }

  // If there's extra buffer space allocated, free it as well
  if (pBlock->pvCrypt)
  {
    sqlite3_free(pBlock->pvCrypt);
  }

  // All done with this cryptblock
  sqlite3_free(pBlock);
}

// Encrypt/Decrypt functionality, called by pager.c
void * sqlite3Codec(void *pArg, void *data, Pgno nPageNum, int nMode)
{
  LPCRYPTBLOCK pBlock = (LPCRYPTBLOCK)pArg;
  DWORD dwPageSize;
  LPVOID pvTemp;

  if (!pBlock) return data;

  // Make sure the page size for the pager is still the same as the page size
  // for the cryptblock.  If the user changed it, we need to adjust!
  if (nMode != 2)
  {
    if (pBlock->pPager->pageSize != pBlock->dwPageSize)
    {
      // Update the cryptblock to reflect the new page size
      CreateCryptBlock(pBlock->hReadKey, pBlock->pPager, pBlock);
    }
  }

  switch(nMode)
  {
  case 0: // Undo a "case 7" journal file encryption
  case 2: // Reload a page
  case 3: // Load a page
    if (!pBlock->hReadKey) break;

    /* Block ciphers often need to write extra padding beyond the 
    data block.  We don't have that luxury for a given page of data so
    we must copy the page data to a buffer that IS large enough to hold
    the padding.  We then encrypt the block and write the buffer back to
    the page without the unnecessary padding.
    We only use the special block of memory if its absolutely necessary. */
    if (pBlock->dwCryptSize != pBlock->dwPageSize)
    {
      CopyMemory(((LPBYTE)pBlock->pvCrypt) + CRYPT_OFFSET, data, pBlock->dwPageSize);
      pvTemp = data;
      data = ((LPBYTE)pBlock->pvCrypt) + CRYPT_OFFSET;
    }

    dwPageSize = pBlock->dwCryptSize;
    CryptDecrypt(pBlock->hReadKey, 0, TRUE, 0, (LPBYTE)data, &dwPageSize);

    // If the encryption algorithm required extra padding and we were forced to encrypt or
    // decrypt a copy of the page data to a temp buffer, then write the contents of the temp
    // buffer back to the page data minus any padding applied.
    if (pBlock->dwCryptSize != pBlock->dwPageSize)
    {
      CopyMemory(pvTemp, data, pBlock->dwPageSize);
      data = pvTemp;
    }
    break;
  case 6: // Encrypt a page for the main database file
    if (!pBlock->hWriteKey) break;

    CopyMemory(((LPBYTE)pBlock->pvCrypt) + CRYPT_OFFSET, data, pBlock->dwPageSize);
    data = ((LPBYTE)pBlock->pvCrypt) + CRYPT_OFFSET;

    dwPageSize = pBlock->dwPageSize;
    CryptEncrypt(pBlock->hWriteKey, 0, TRUE, 0, ((LPBYTE)pBlock->pvCrypt) + CRYPT_OFFSET, &dwPageSize, pBlock->dwCryptSize);
    break;
  case 7: // Encrypt a page for the journal file
    /* Under normal circumstances, the readkey is the same as the writekey.  However,
    when the database is being rekeyed, the readkey is not the same as the writekey.
    The rollback journal must be written using the original key for the
    database file because it is, by nature, a rollback journal.
    Therefore, for case 7, when the rollback is being written, always encrypt using
    the database's readkey, which is guaranteed to be the same key that was used to
    read the original data.
    */
    if (!pBlock->hReadKey) break;

    CopyMemory(((LPBYTE)pBlock->pvCrypt) + CRYPT_OFFSET, data, pBlock->dwPageSize);
    data = ((LPBYTE)pBlock->pvCrypt) + CRYPT_OFFSET;

    dwPageSize = pBlock->dwPageSize;
    CryptEncrypt(pBlock->hReadKey, 0, TRUE, 0, ((LPBYTE)pBlock->pvCrypt) + CRYPT_OFFSET, &dwPageSize, pBlock->dwCryptSize);
    break;
  }

  return data;
}

// Derive an encryption key from a user-supplied buffer
static HCRYPTKEY DeriveKey(const void *pKey, int nKeyLen)
{
  HCRYPTHASH hHash = 0;
  HCRYPTKEY  hKey;

  if (!pKey || !nKeyLen) return 0;

  if (!InitializeProvider())
  {
    return MAXDWORD;
  }

  if (CryptCreateHash(g_hProvider, CALG_SHA1, 0, 0, &hHash))
  {
    if (CryptHashData(hHash, (LPBYTE)pKey, nKeyLen, 0))
    {
      CryptDeriveKey(g_hProvider, CALG_RC4, hHash, 0, &hKey);
    }
    CryptDestroyHash(hHash);
  }  
  return hKey;
}

// Called by sqlite and sqlite3_key_interop to attach a key to a database.
int sqlite3CodecAttach(sqlite3 *db, int nDb, const void *pKey, int nKeyLen)
{
  int rc = SQLITE_ERROR;
  HCRYPTKEY hKey = 0;

  // No key specified, could mean either use the main db's encryption or no encryption
  if (!pKey || !nKeyLen)
  {
    if (!nDb)
    {
      return SQLITE_OK; // Main database, no key specified so not encrypted
    }
    else // Attached database, use the main database's key
    {
      // Get the encryption block for the main database and attempt to duplicate the key
      // for use by the attached database
      LPCRYPTBLOCK pBlock = (LPCRYPTBLOCK)sqlite3pager_get_codecarg(sqlite3BtreePager(db->aDb[0].pBt));

      if (!pBlock) return SQLITE_OK; // Main database is not encrypted so neither will be any attached database
      if (!pBlock->hReadKey) return SQLITE_OK; // Not encrypted

      if (!CryptDuplicateKey(pBlock->hReadKey, NULL, 0, &hKey))
        return rc; // Unable to duplicate the key
    }
  }
  else // User-supplied passphrase, so create a cryptographic key out of it
  {
    hKey = DeriveKey(pKey, nKeyLen);
    if (hKey == MAXDWORD)
    {
      sqlite3Error(db, rc, SQLITECRYPTERROR_PROVIDER);
      return rc;
    }
  }

  // Create a new encryption block and assign the codec to the new attached database
  if (hKey)
  {
    LPCRYPTBLOCK pBlock = CreateCryptBlock(hKey, sqlite3BtreePager(db->aDb[nDb].pBt), NULL);
    sqlite3PagerSetCodec(sqlite3BtreePager(db->aDb[nDb].pBt), sqlite3Codec, pBlock);
    db->aDb[nDb].pAux = pBlock;
    db->aDb[nDb].xFreeAux = DestroyCryptBlock;

    rc = SQLITE_OK;
  }
  return rc;
}

// Once a password has been supplied and a key created, we don't keep the 
// original password for security purposes.  Therefore return NULL.
void sqlite3CodecGetKey(sqlite3 *db, int nDb, void **ppKey, int *pnKeyLen)
{
  *ppKey = NULL;
  *pnKeyLen = 0;
}

// We do not attach this key to the temp store, only the main database.
__declspec(dllexport) int WINAPI sqlite3_key_interop(sqlite3 *db, const void *pKey, int nKeySize)
{
  return sqlite3CodecAttach(db, 0, pKey, nKeySize);
}

// Changes the encryption key for an existing database.
__declspec(dllexport) int WINAPI sqlite3_rekey_interop(sqlite3 *db, const void *pKey, int nKeySize)
{
  Btree *pbt = db->aDb[0].pBt;
  Pager *p = sqlite3BtreePager(pbt);
  LPCRYPTBLOCK pBlock = (LPCRYPTBLOCK)sqlite3pager_get_codecarg(p);
  HCRYPTKEY hKey = DeriveKey(pKey, nKeySize);
  int rc = SQLITE_ERROR;

  if (hKey == MAXDWORD)
  {
    sqlite3Error(db, rc, SQLITECRYPTERROR_PROVIDER);
    return rc;
  }

  if (!pBlock && !hKey) return SQLITE_OK; // Wasn't encrypted to begin with

  // To rekey a database, we change the writekey for the pager.  The readkey remains
  // the same
  if (!pBlock) // Encrypt an unencrypted database
  {
    pBlock = CreateCryptBlock(hKey, p, NULL);
    pBlock->hReadKey = 0; // Original database is not encrypted
    sqlite3PagerSetCodec(sqlite3BtreePager(pbt), sqlite3Codec, pBlock);
    db->aDb[0].pAux = pBlock;
    db->aDb[0].xFreeAux = DestroyCryptBlock;
  }
  else // Change the writekey for an already-encrypted database
  {
    pBlock->hWriteKey = hKey;
  }

  // Start a transaction
  rc = sqlite3BtreeBeginTrans(pbt, 1);

  if (!rc)
  {
    // Rewrite all the pages in the database using the new encryption key
    Pgno nPage = sqlite3PagerPagecount(p);
    Pgno nSkip = PAGER_MJ_PGNO(p);
    DbPage *pPage;
    Pgno n;

    for(n = 1; rc == SQLITE_OK && n <= nPage; n ++)
    {
      if (n == nSkip) continue;
      rc = sqlite3PagerGet(p, n, &pPage);
      if(!rc)
      {
        rc = sqlite3PagerWrite(pPage);
        sqlite3PagerUnref(pPage);
      }
    }
  }

  // If we succeeded, try and commit the transaction
  if (!rc)
  {
    rc = sqlite3BtreeCommit(pbt);
  }

  // If we failed, rollback
  if (rc)
  {
    sqlite3BtreeRollback(pbt);
  }

  // If we succeeded, destroy any previous read key this database used
  // and make the readkey equal to the writekey
  if (!rc)
  {
    if (pBlock->hReadKey)
    {
      CryptDestroyKey(pBlock->hReadKey);
    }
    pBlock->hReadKey = pBlock->hWriteKey;
  }
  // We failed.  Destroy the new writekey (if there was one) and revert it back to
  // the original readkey
  else
  {
    if (pBlock->hWriteKey)
    {
      CryptDestroyKey(pBlock->hWriteKey);
    }
    pBlock->hWriteKey = pBlock->hReadKey;
  }

  // If the readkey and writekey are both empty, there's no need for a codec on this
  // pager anymore.  Destroy the crypt block and remove the codec from the pager.
  if (!pBlock->hReadKey && !pBlock->hWriteKey)
  {
    sqlite3PagerSetCodec(p, NULL, NULL);
    db->aDb[0].pAux = NULL;
    db->aDb[0].xFreeAux = NULL;
    DestroyCryptBlock(pBlock);
  }

  return rc;
}

int sqlite3_key(sqlite3 *db, const void *pKey, int nKey)
{
  return sqlite3_key_interop(db, pKey, nKey);
}

int sqlite3_rekey(sqlite3 *db, const void *pKey, int nKey)
{
  return sqlite3_rekey_interop(db, pKey, nKey);
}

#endif // SQLITE_HAS_CODEC

#endif // SQLITE_OMIT_DISKIO
