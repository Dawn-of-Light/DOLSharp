/********************************************************
 * mergebin
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

#include "StdAfx.h"
#include "PEFile.h"

#define MakePtr( cast, ptr, addValue ) (cast)( (DWORD_PTR)(ptr) + (DWORD_PTR)(addValue))

CPEFile::CPEFile(void)
{
  m_hMap = NULL;
  m_hFile = INVALID_HANDLE_VALUE;
  m_pBase = NULL;
}

CPEFile::~CPEFile(void)
{
  Close();
}

HRESULT CPEFile::Open(LPCTSTR pszFile, BOOL bReadOnly)
{
  HRESULT hr = S_OK;
  Close();

  m_hFile = CreateFile(pszFile, GENERIC_READ | ((bReadOnly == FALSE) ? GENERIC_WRITE: 0), FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
  if (m_hFile == INVALID_HANDLE_VALUE)
  {
    hr = HRESULT_FROM_WIN32(GetLastError());
  }
  else
  {
    m_hMap = CreateFileMapping(m_hFile, NULL, (bReadOnly == TRUE) ? PAGE_READONLY:PAGE_READWRITE, 0, 0, NULL);
    if (!m_hMap)
    {
      hr = HRESULT_FROM_WIN32(GetLastError());
    }
    else
    {
      m_pBase = (PIMAGE_DOS_HEADER)MapViewOfFile(m_hMap, FILE_MAP_READ | ((bReadOnly == FALSE) ? FILE_MAP_WRITE:0), 0, 0, 0);
      if (!m_pBase)
      {
        hr = HRESULT_FROM_WIN32(GetLastError());
      }
    }
  }

  if (SUCCEEDED(hr))
  {
    PIMAGE_FILE_HEADER pImageHeader = (PIMAGE_FILE_HEADER)m_pBase;
    if (m_pBase->e_magic != IMAGE_DOS_SIGNATURE)
    {
      hr = HRESULT_FROM_WIN32(ERROR_BAD_FORMAT);
    }

    m_pNTHeader = MakePtr(PIMAGE_NT_HEADERS, m_pBase, m_pBase->e_lfanew);
    if (IsBadReadPtr(m_pNTHeader, sizeof(m_pNTHeader->Signature)))
    {
      hr = HRESULT_FROM_WIN32(ERROR_BAD_FORMAT);
    }
    else
    {
      if (m_pNTHeader->Signature != IMAGE_NT_SIGNATURE)
      {
        hr = HRESULT_FROM_WIN32(ERROR_BAD_FORMAT);
      }
    }
    m_bIs64Bit = (m_pNTHeader->OptionalHeader.Magic == IMAGE_NT_OPTIONAL_HDR64_MAGIC);
  }

  if (FAILED(hr)) Close();

  return hr;
}

void CPEFile::Close(void)
{
  if (m_pBase)
  {
    UnmapViewOfFile(m_pBase);
  }

  if (m_hMap)
    CloseHandle(m_hMap);

  if (m_hFile != INVALID_HANDLE_VALUE)
    CloseHandle(m_hFile);

  m_hMap = NULL;
  m_hFile = INVALID_HANDLE_VALUE;
  m_pBase = NULL;
}

PIMAGE_SECTION_HEADER CPEFile::GetEnclosingSectionHeader(DWORD rva) const
{
  PIMAGE_SECTION_HEADER section;
  
  if (m_bIs64Bit)
    section = IMAGE_FIRST_SECTION((PIMAGE_NT_HEADERS64)m_pNTHeader);
  else
    section = IMAGE_FIRST_SECTION(m_pNTHeader);

  for (UINT i=0; i < m_pNTHeader->FileHeader.NumberOfSections; i++, section++ )
  {
    DWORD size = section->Misc.VirtualSize;
    if (!size)
      size = section->SizeOfRawData;

    if ( (rva >= section->VirtualAddress) && 
      (rva < (section->VirtualAddress + size)))
      return section;
  }
  return NULL;
}

LPVOID CPEFile::GetPtrFromRVA(DWORD rva) const
{
  PIMAGE_SECTION_HEADER pSectionHdr;
  INT delta;

  pSectionHdr = GetEnclosingSectionHeader(rva);
  if ( !pSectionHdr )
    return 0;

  delta = (INT)(pSectionHdr->VirtualAddress-pSectionHdr->PointerToRawData);
  return (PVOID) (((LPBYTE)m_pBase) + rva - delta);
}

PIMAGE_SECTION_HEADER CPEFile::GetSectionHeader(LPCSTR name) const
{
  PIMAGE_SECTION_HEADER section;
  
  if (m_bIs64Bit)
    section = IMAGE_FIRST_SECTION((PIMAGE_NT_HEADERS64)m_pNTHeader);
  else
    section = IMAGE_FIRST_SECTION(m_pNTHeader);

  for (UINT i=0; i < m_pNTHeader->FileHeader.NumberOfSections; i++, section++)
  {
    if (_strnicmp((char *)section->Name,name,IMAGE_SIZEOF_SHORT_NAME) == 0)
      return section;
  }

  return NULL;
}

CPEFile::operator PIMAGE_NT_HEADERS32() const
{
  if (!m_pBase || m_bIs64Bit) return NULL;
  return m_pNTHeader;
}

CPEFile::operator PIMAGE_NT_HEADERS64() const
{
  if (!m_pBase || !m_bIs64Bit) return NULL;
  return (PIMAGE_NT_HEADERS64)m_pNTHeader;
}

CPEFile::operator PIMAGE_DOS_HEADER() const
{
  return m_pBase;
}

CPEFile::operator PIMAGE_COR20_HEADER() const
{
  if (!m_pBase) return NULL;

  DWORD dwRVA;

  if (m_bIs64Bit)
    dwRVA = ((PIMAGE_NT_HEADERS64)m_pNTHeader)->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_COM_DESCRIPTOR].VirtualAddress;
  else
    dwRVA = m_pNTHeader->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_COM_DESCRIPTOR].VirtualAddress;
  if (!dwRVA)
  {
    return NULL;
  }

  return ((PIMAGE_COR20_HEADER)GetPtrFromRVA(dwRVA));
}
