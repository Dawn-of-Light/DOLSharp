/********************************************************
 * mergebin
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

#pragma once
#include "metadata.h"

class CTableData;

class CMetadataTables :
  public CMetadata::CStream
{
  friend CTableData;

public:
  CMetadataTables(CMetadata& metaData);
  virtual ~CMetadataTables(void);

protected:
  CMetadata&  m_meta;
  BYTE       *m_pbMajorVersion;
  BYTE       *m_pbMinorVersion;
  BYTE       *m_pbHeapOffsetSizes;
  UINT64     *m_pullMaskValid;
  UINT64     *m_pullMaskSorted;
  DWORD      *m_pdwTableLengths;

  DWORD      *m_pdwTableLengthIndex[64];
  DWORD       m_dwTables;

  CTableData *m_pTables[64];

public:
  UINT        GetStringIndexSize ();
  UINT        GetGuidIndexSize   ();
  UINT        GetBlobIndexSize   ();
  DWORD       GetMaxIndexSizeOf  (UINT *);

  DWORD *     TableRowCount      (UINT uType);

  CTableData *GetTable           (UINT uId);
};
