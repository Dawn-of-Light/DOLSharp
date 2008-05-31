/********************************************************
 * mergebin
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

#include "StdAfx.h"
#include "MetaData.h"

CMetadata::CStream::operator LPBYTE() const
{
  return m_pbData;
}

CMetadata::CMetadata(CPEFile& peFile) : m_peFile(peFile)
{
  PIMAGE_COR20_HEADER pCor = m_peFile;
  if (!pCor) throw;

  LPBYTE pb = (LPBYTE)m_peFile.GetPtrFromRVA(pCor->MetaData.VirtualAddress);
  LPBYTE pbRoot = pb;
  size_t x;
  if (!pb) throw;

  m_pdwSignature     = (LPDWORD)pb;
  m_pwMajorVersion   = (LPWORD)(m_pdwSignature + 1);
  m_pwMinorVersion   = m_pwMajorVersion + 1;
  m_pdwVersionLength = (LPDWORD)(m_pwMinorVersion + 3);
  m_pszVersion       = (LPSTR)(m_pdwVersionLength + 1);

  pb = (LPBYTE)m_pszVersion;
  x = *m_pdwVersionLength;
  if (x % 4) x += 4 - (x % 4);
  pb += x;
  pb += 2;
  
  m_pwStreams        = (LPWORD)pb;
  m_pStreams = new CStream[*m_pwStreams];
  pb = (LPBYTE)(m_pwStreams + 1);

  for (WORD n = 0; n < *m_pwStreams; n++)
  {
    m_pStreams[n].m_pdwOffset  = (LPDWORD)pb;
    m_pStreams[n].m_pdwSize    = m_pStreams[n].m_pdwOffset + 1;
    m_pStreams[n].m_pszName    = (LPSTR)(m_pStreams[n].m_pdwSize + 1);
    m_pStreams[n].m_pbData     = pbRoot + (*m_pStreams[n].m_pdwOffset);

    x = strlen(m_pStreams[n].m_pszName) + 1;
    if (x % 4) x += 4 - (x % 4);

    pb = (LPBYTE)m_pStreams[n].m_pszName + x;
  }
}

CMetadata::~CMetadata(void)
{
  delete[] m_pStreams;
}

CMetadata::operator CPEFile&() const
{
  return m_peFile;
}

CMetadata::CStream * CMetadata::GetStream(UINT uiStream)
{
  if (uiStream >= *m_pwStreams) return NULL;
  return &m_pStreams[uiStream];
}

CMetadata::CStream * CMetadata::GetStream(LPCSTR pszStreamName)
{
  for (WORD n = 0; n < *m_pwStreams; n++)
  {
    if (_stricmp(pszStreamName, m_pStreams[n].m_pszName) == 0)
      return &m_pStreams[n];
  }
  return NULL;
}

WORD * CMetadata::StreamCount() const
{
  return m_pwStreams;
}

