/********************************************************
 * mergebin
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

#include "StdAfx.h"
#include "MetaDataTables.h"
#include "TableData.h"

CMetadataTables::CMetadataTables(CMetadata& metaData) : m_meta(metaData)
{
  CMetadata::CStream *ps = m_meta.GetStream("#~");
  if (!ps) throw;

  *static_cast<CMetadata::CStream *>(this) = *ps;

  LPBYTE pb = m_pbData + sizeof(DWORD);
  m_pbMajorVersion = pb;
  m_pbMinorVersion = m_pbMajorVersion + 1;
  m_pbHeapOffsetSizes = m_pbMinorVersion + 1;
  // Skip a byte
  m_pullMaskValid = (UINT64 *)(m_pbHeapOffsetSizes + 2);
  m_pullMaskSorted = m_pullMaskValid + 1;

  m_pdwTableLengths = (LPDWORD)(m_pullMaskSorted + 1);

  m_dwTables = 0;
  for (int n = 0; n < 64; n++)
  {
    if ((((*m_pullMaskValid) >> n) & 1) == 1)
    {
      m_pdwTableLengthIndex[n] = &m_pdwTableLengths[m_dwTables ++];
    }
    else
    {
      m_pdwTableLengthIndex[n] = NULL;
    }
  }
  m_pbData = (LPBYTE)(m_pdwTableLengths + m_dwTables);

  for (int n = 0; n < 64; n++)
  {
    if (m_pdwTableLengthIndex[n] && g_arTableTypes[n])
    {
      m_pTables[n] = g_arTableTypes[n](this);
    }
    else
      m_pTables[n] = 0;
  }
}

CMetadataTables::~CMetadataTables(void)
{
  for (int n = 0; n < 64; n++)
  {
    if (m_pTables[n])
      delete m_pTables[n];
  }
}

UINT CMetadataTables::GetStringIndexSize(void)
{
  return ((*m_pbHeapOffsetSizes) & 0x01) == 0 ? sizeof(WORD) : sizeof(DWORD);
}

UINT CMetadataTables::GetGuidIndexSize(void)
{
  return ((*m_pbHeapOffsetSizes) & 0x02) == 0 ? sizeof(WORD) : sizeof(DWORD);
}

UINT CMetadataTables::GetBlobIndexSize(void)
{
  return ((*m_pbHeapOffsetSizes) & 0x04) == 0 ? sizeof(WORD) : sizeof(DWORD);
}

DWORD *CMetadataTables::TableRowCount(UINT uType)
{
  return m_pdwTableLengthIndex[uType];
}

DWORD CMetadataTables::GetMaxIndexSizeOf(UINT * puiTables)
{
  DWORD dwMaxRows = 0;
  DWORD *pdwLength;
  UINT uCount = 0;

  while (*puiTables)
  {
    uCount ++;
    pdwLength = m_pdwTableLengthIndex[*puiTables];
    if (pdwLength)
      dwMaxRows = max(dwMaxRows, pdwLength[0]);

    puiTables ++;
  }

  return (dwMaxRows > 0xFFFF) ? 4 : 2;
  //return (dwMaxRows > (ULONG)(2 << (16 - uCount))) ? 4 : 2;
}

CTableData *CMetadataTables::GetTable(UINT uId)
{
  return m_pTables[uId];
}