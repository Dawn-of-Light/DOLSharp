/********************************************************
 * mergebin
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

#pragma once

#include "pefile.h"

class CMetadata
{
public:
  class CStream
  {
    friend CMetadata;
  public:
    operator LPBYTE() const;

  protected:
    DWORD *m_pdwOffset;
    DWORD *m_pdwSize;
    LPSTR  m_pszName;
    LPBYTE m_pbData;
  };
public:
  CMetadata(CPEFile& peFile);
  virtual ~CMetadata(void);

protected:
  CPEFile& m_peFile;

  DWORD   *m_pdwSignature;
  WORD    *m_pwMajorVersion;
  WORD    *m_pwMinorVersion;
  DWORD   *m_pdwVersionLength;
  LPSTR    m_pszVersion;
  WORD    *m_pwStreams;
  CStream *m_pStreams;

public:
  operator CPEFile&() const;

  WORD *               StreamCount () const;
  CMetadata::CStream * GetStream   (UINT uiStream);
  CMetadata::CStream * GetStream   (LPCSTR pszStreamName);
};
