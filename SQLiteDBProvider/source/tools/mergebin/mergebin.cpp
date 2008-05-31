/********************************************************
 * mergebin
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

#include "stdafx.h"
#include "MetaData.h"
#include "MetaDataTables.h"
#include "TableData.h"

void DumpCLRInfo(LPCTSTR pszFile);
void MergeModules(LPCTSTR pszAssembly, LPCTSTR pszNative, LPCTSTR pszSection, DWORD dwAdjust);
void DumpCLRPragma(LPCTSTR pszAssembly, LPCTSTR pszSection);

typedef struct EXTRA_STUFF
{
  DWORD dwNativeEntryPoint;
} EXTRA_STUFF, *LPEXTRA_STUFF;

int _tmain(int argc, _TCHAR* argv[])
{
  if (argc == 1)
  {
    _tprintf(_T(
"MERGEBIN - Merges a pure .NET assembly with a native DLL\n \
Syntax: MERGEBIN [/I:assembly] [/S:sectionname assembly nativedll]\n \
/I:assembly            Returns the number of bytes required\n \
                       to consume the assembly\n \
/S:sectionname         The name of the section in the nativedll\n \
                       to insert the CLR data\n \
/P:assembly            Outputs the C++ pragma code that can be used\n \
                       as additional input to a C++ app to reserve\n \
                       a section block large enough for the managed code.\n \
/A:#                   adjust .data segment virtual size to the specified\n \
                       amount for Windows CE fixing up.\n \
                       \n \
The native DLL must have an unused section in it, into which the\n \
.NET assembly will be inserted.  You can do this with the following code:\n \
  #pragma data_seg(\".clr\")\n \
  #pragma comment(linker, \"/SECTION:.clr,ER\")\n \
   char __ph[92316] = {0}; // 92316 is the number of bytes to reserve\n \
  #pragma data_seg()\n \
You would then specify /SECTION:.CLR in the command-line for the location to\n \
insert the .NET assembly.  The number of bytes reserved in the section needs\n \
to be equal to or more than the number of bytes returned by the /I parameter.\n \
\n \
The native DLL must also export a function that calls _CorDllMain in \n \
MSCOREE.DLL.  This function must have the same parameters and calling\n \
conventions as DllMain, and its name must have the word \"CORDLLMAIN\"\n \
in it.\n \
"));
    return 0;
  }

  LPTSTR pszAssembly = NULL;
  LPTSTR pszNative = NULL;
  LPTSTR pszSection = NULL;
  BOOL bDoPragma = FALSE;
  DWORD dwAdjust = 0;

  for (int n = 1; n < argc; n++)
  {
    if (argv[n][0] != '-' && argv[n][0] != '/')
    {
      if (pszAssembly == NULL)
        pszAssembly = argv[n];
      else if (pszNative == NULL)
        pszNative = argv[n];
      else
      {
        _tprintf(_T("Too many files specified\n"));
        return 0;
      }
      continue;
    }

    switch(argv[n][1])
    {
    case 'I':
    case 'i':
      pszAssembly = &argv[n][3];
      if (argv[n][2] != ':' || lstrlen(pszAssembly) == 0)
      {
        _tprintf(_T("/I requires an assembly name\n"));
        return 0;
      }
      DumpCLRInfo(pszAssembly);
      return 0;
      break;
    case 'P':
    case 'p':
      pszAssembly = &argv[n][3];
      if (argv[n][2] != ':' || lstrlen(pszAssembly) == 0)
      {
        _tprintf(_T("/P requires an assembly name\n"));
        return 0;
      }
      bDoPragma = TRUE;
      break;
    case 'S':
    case 's':
      pszSection = &argv[n][3];
      if (argv[n][2] != ':' || lstrlen(pszSection) == 0)
      {
        _tprintf(_T("/S requires a section name\n"));
        return 0;
      }
      break;
    case 'A':
    case 'a':
      if (argv[n][2] != ':')
      {
        _tprintf(_T("A parameter requires a numeric value\n"));
        return 0;
      }
      dwAdjust = _ttol(&argv[n][3]);
      break;
    }
  }

  if (pszAssembly && pszNative && pszSection && !bDoPragma)
    MergeModules(pszAssembly, pszNative, pszSection, dwAdjust);

  if (pszAssembly && bDoPragma)
    DumpCLRPragma(pszAssembly, pszSection);

  return 0;
}

BOOL GetMinMaxCOR20RVA(CPEFile& file, DWORD& dwMin, DWORD& dwMax)
{
  PIMAGE_COR20_HEADER pCor = file;
  dwMin = MAXDWORD;
  dwMax = 0;

  if (!pCor) return FALSE;

  if (pCor->MetaData.Size) dwMin = min(dwMin, pCor->MetaData.VirtualAddress);
  if (pCor->Resources.Size) dwMin = min(dwMin, pCor->Resources.VirtualAddress);
  if (pCor->StrongNameSignature.Size) dwMin = min(dwMin, pCor->StrongNameSignature.VirtualAddress);
  if (pCor->CodeManagerTable.Size) dwMin = min(dwMin, pCor->CodeManagerTable.VirtualAddress);
  if (pCor->VTableFixups.Size) dwMin = min(dwMin, pCor->VTableFixups.VirtualAddress);
  if (pCor->ExportAddressTableJumps.Size) dwMin = min(dwMin, pCor->ExportAddressTableJumps.VirtualAddress);
  if (pCor->ManagedNativeHeader.Size) dwMin = min(dwMin, pCor->ManagedNativeHeader.VirtualAddress);

  dwMax = max(dwMax, (pCor->MetaData.VirtualAddress + pCor->MetaData.Size));
  dwMax = max(dwMax, (pCor->Resources.VirtualAddress + pCor->Resources.Size));
  dwMax = max(dwMax, (pCor->StrongNameSignature.VirtualAddress + pCor->StrongNameSignature.Size));
  dwMax = max(dwMax, (pCor->CodeManagerTable.VirtualAddress + pCor->CodeManagerTable.Size));
  dwMax = max(dwMax, (pCor->VTableFixups.VirtualAddress + pCor->VTableFixups.Size));
  dwMax = max(dwMax, (pCor->ExportAddressTableJumps.VirtualAddress + pCor->ExportAddressTableJumps.Size));
  dwMax = max(dwMax, (pCor->ManagedNativeHeader.VirtualAddress + pCor->ManagedNativeHeader.Size));

  CMetadata meta(file);
  CMetadataTables tables(meta);
  CTableData *p;
  DWORD *pdwRVA;
  DWORD dwRows;

  for (int n = 0; n < 2; n++)
  {
    p = tables.GetTable((n == 0) ? ttMethodDef : ttFieldRVA);
    if (p)
    {
      dwRows = p->GetRowCount();
      for (UINT uRow = 0; uRow < dwRows; uRow ++)
      {
        pdwRVA = (DWORD *)p->Column(uRow, (UINT)0);
        if (*pdwRVA)
          dwMin = min(dwMin, (*pdwRVA));
      }
    }
  }
  return TRUE;
}

void DumpCLRInfo(LPCTSTR pszFile)
{
  CPEFile peFile;
  HRESULT hr;
  hr = peFile.Open(pszFile);
  if (FAILED(hr)) return;

  DWORD dwMinRVA;
  DWORD dwMaxRVA;

  if (!GetMinMaxCOR20RVA(peFile, dwMinRVA, dwMaxRVA))
  {
    _tprintf(_T("Unable to retrieve .NET assembly information for file %s\n"), pszFile);
    return;
  }

  _tprintf(_T("%d Bytes required to merge %s\n"), (dwMaxRVA - dwMinRVA) + ((PIMAGE_COR20_HEADER)peFile)->cb + sizeof(EXTRA_STUFF), pszFile);
}

void DumpCLRPragma(LPCTSTR pszAssembly, LPCTSTR pszSection)
{
  CPEFile peFile;
  HRESULT hr;
  DWORD dwMinRVA;
  DWORD dwMaxRVA;

  hr = peFile.Open(pszAssembly);
  if (FAILED(hr)) return;
  
  if (pszSection == NULL) pszSection = _T(".clr");

  if (!GetMinMaxCOR20RVA(peFile, dwMinRVA, dwMaxRVA))
  {
    _tprintf(_T("// Unable to retrieve .NET assembly information for file %s\n"), pszAssembly);
    return;
  }

  _tprintf(_T("// This code was automatically generated from assembly\n\
// %s\n\n\
#include <windef.h>\n\n\
#pragma data_seg(push,clrseg,\"%s\")\n\
#pragma comment(linker, \"/SECTION:%s,ER\")\n\
  char __ph[%d] = {0}; // The number of bytes to reserve\n\
#pragma data_seg(pop,clrseg)\n\n\
typedef BOOL (WINAPI *DLLMAIN)(HANDLE, DWORD, LPVOID);\n\
typedef struct EXTRA_STUFF\n\
{\n\
  DWORD dwNativeEntryPoint;\n\
} EXTRA_STUFF, *LPEXTRA_STUFF;\n\n\
__declspec(dllexport) BOOL WINAPI _CorDllMainStub(HANDLE hModule, DWORD dwReason, LPVOID pvReserved)\n\
{\n\
  HANDLE hMod;\n\
  DLLMAIN proc;\n\
  LPEXTRA_STUFF pExtra;\n\n\
  hMod = GetModuleHandle(_T(\"mscoree\"));\n\
  if (hMod)\n\
    proc = (DLLMAIN)GetProcAddress(hMod, _T(\"_CorDllMain\"));\n\
  else\n\
  {\n\
    MEMORY_BASIC_INFORMATION mbi;\n\n\
    VirtualQuery(_CorDllMainStub, &mbi, sizeof(mbi));\n\
    pExtra = (LPEXTRA_STUFF)__ph;\n\
    proc = (DLLMAIN)(pExtra->dwNativeEntryPoint + (DWORD)mbi.AllocationBase);\n\
  }\n\
  return proc(hModule, dwReason, pvReserved);\n\
}\n\
"), pszAssembly, pszSection, pszSection, (dwMaxRVA - dwMinRVA) + ((PIMAGE_COR20_HEADER)peFile)->cb + sizeof(EXTRA_STUFF));
}

/*   When merged, the native DLL's entrypoint must go to _CorDllMain in MSCOREE.DLL.
  ** In order to do this, we need to change the DLL's entrypoint to "something" that will
  ** call CorDllMain.  Since its too much hassle to add imports to the DLL and make drastic
  ** changes to it, we rely on the native DLL to export a function that we can call which will
  ** forward to CorDllMain.  Exported functions are easy to identify and get an RVA for.
  ** The exported function must have the same calling conventions and parameters as DllMain,
  ** and must contain the letters "CORDLLMAIN" in the name.  The search is case-insensitive. */
DWORD GetExportedCorDllMainRVA(CPEFile& file)
{
  PIMAGE_EXPORT_DIRECTORY pExportDir;
  PIMAGE_SECTION_HEADER header;
  INT delta; 
  DWORD i;
  DWORD *pdwFunctions;
  PWORD pwOrdinals;
  DWORD *pszFuncNames;
  DWORD exportsStartRVA;
  DWORD exportsEndRVA;
  CHAR szName[MAX_PATH + 1];
  PIMAGE_NT_HEADERS32 pNT = file;
  PIMAGE_NT_HEADERS64 pNT64 = file;

  if (pNT)
  {
    exportsStartRVA = pNT->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_EXPORT].VirtualAddress;
    exportsEndRVA = exportsStartRVA + pNT->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_EXPORT].Size;
  }
  else
  {
    exportsStartRVA = pNT64->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_EXPORT].VirtualAddress;
    exportsEndRVA = exportsStartRVA + pNT64->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_EXPORT].Size;
  }

  header = file.GetEnclosingSectionHeader(exportsStartRVA);
  if (!header)
    return 0;

  delta = (INT)(header->VirtualAddress - header->PointerToRawData);

  pExportDir   =  (PIMAGE_EXPORT_DIRECTORY)file.GetPtrFromRVA(exportsStartRVA);
  pdwFunctions =	(PDWORD)file.GetPtrFromRVA(pExportDir->AddressOfFunctions);
  pwOrdinals   =	(PWORD)file.GetPtrFromRVA(pExportDir->AddressOfNameOrdinals);
  pszFuncNames =	(DWORD *)file.GetPtrFromRVA(pExportDir->AddressOfNames);

  for (i = 0; i < pExportDir->NumberOfFunctions; i++, pdwFunctions++)
  {
    DWORD entryPointRVA = *pdwFunctions;

    if ( entryPointRVA == 0 )
      continue;

    for (UINT j = 0; j < pExportDir->NumberOfNames; j++)
    {
      if (pwOrdinals[j] == i)
      {
        lstrcpynA(szName, (LPSTR)file.GetPtrFromRVA(pszFuncNames[j]), MAX_PATH);
        szName[MAX_PATH] = 0;
        CharUpper(szName);
        if (strstr(szName, "CORDLLMAIN") != 0) return entryPointRVA;
      }
    }
  }
  return 0;
}

// Merges a pure .NET assembly with a native DLL, inserting it into the specified section
void MergeModules(LPCTSTR pszAssembly, LPCTSTR pszNative, LPCTSTR pszSection, DWORD dwAdjust)
{
  CPEFile peFile;
  CPEFile peDest;
  HRESULT hr;
  DWORD dwMinRVA;
  DWORD dwMaxRVA;
  DWORD dwDestRVA;
  PIMAGE_SECTION_HEADER pSection;
  LPBYTE pSrc;
  LPBYTE pDest;
  DWORD dwSize;
  DWORD dwNewEntrypoint;
  PIMAGE_COR20_HEADER pCor;
  PIMAGE_NT_HEADERS32 pNT;
  PIMAGE_NT_HEADERS64 pNT64;
  int diffRVA;
  CTableData *p;
  DWORD *pdwRVA;
  DWORD dwRows;
  LPEXTRA_STUFF pExtra;

  // Open the .NET assembly
  hr = peFile.Open(pszAssembly);
  if (FAILED(hr)) return;

  // Scan the .NET assembly and find the block of .NET code specified in the .NET metadata
  if (!GetMinMaxCOR20RVA(peFile, dwMinRVA, dwMaxRVA))
  {
    _tprintf(_T("Unable to retrieve .NET assembly information for file %s\n"), pszAssembly);
    return;
  }
  // Total number of bytes of the block of .NET code we're going to merge
  dwSize = (dwMaxRVA - dwMinRVA) + ((PIMAGE_COR20_HEADER)peFile)->cb;

  // Open the destination file for readwrite access
  hr = peDest.Open(pszNative, FALSE);
  if (FAILED(hr)) return;

  // Make sure it has the section specified in the command-line
  pSection = peDest.GetSectionHeader(pszSection);
  if (!pSection)
  {
    _tprintf(_T("Unable to find section %s in file\n"), pszSection);
    return;
  }

  // If the section isn't large enough, tell the user how large it needs to be
  if (pSection->Misc.VirtualSize < (dwSize + sizeof(EXTRA_STUFF)))
  {
    _tprintf(_T("Not enough room in section for data.  Need %d bytes\n"), dwSize + sizeof(EXTRA_STUFF));
    return;
  }

  /*
  ** Find a new entrypoint to use for the DLL.  The old entrypoint is written into the .NET header
  */
  dwNewEntrypoint = GetExportedCorDllMainRVA(peDest);
  if (!dwNewEntrypoint)
  {
    _tprintf(_T("Native DLL must export a function that calls _CorDllMain, and its name must contain the word \"CorDllMain\".\n"));
    return;
  }

  // Change this section's flags
  pSection->Characteristics = IMAGE_SCN_CNT_CODE | IMAGE_SCN_MEM_EXECUTE | IMAGE_SCN_MEM_READ;
  dwDestRVA = pSection->VirtualAddress;

  pExtra = (LPEXTRA_STUFF)peDest.GetPtrFromRVA(dwDestRVA);
  dwDestRVA += sizeof(EXTRA_STUFF);

  // If the native DLL has been merged with an assembly beforehand, we need to strip the .NET stuff and restore the entrypoint
  pCor = peDest;
  if (pCor)
  {
    if (pCor->Flags & 0x10)
    {
      pNT = peDest;
      pNT64 = peDest;

      if (pNT)
        pNT->OptionalHeader.AddressOfEntryPoint = pCor->EntryPointToken;
      else
        pNT64->OptionalHeader.AddressOfEntryPoint = pCor->EntryPointToken;
    }
  }

  // Copy the assembly's .NET header into the section
  dwSize = ((PIMAGE_COR20_HEADER)peFile)->cb;
  pSrc = (LPBYTE)(PIMAGE_COR20_HEADER)peFile;
  pDest = (LPBYTE)peDest.GetPtrFromRVA(dwDestRVA);
  CopyMemory(pDest, pSrc, dwSize);

  pNT = peDest;
  pNT64 = peDest;

  // Fixup the NT header on the native DLL to include the new .NET header
  if (pNT)
  {
    pExtra->dwNativeEntryPoint = pNT->OptionalHeader.AddressOfEntryPoint;
    pNT->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_COM_DESCRIPTOR].VirtualAddress = dwDestRVA;
    pNT->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_COM_DESCRIPTOR].Size = dwSize;
  }
  else
  {
    pExtra->dwNativeEntryPoint = pNT64->OptionalHeader.AddressOfEntryPoint;
    pNT64->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_COM_DESCRIPTOR].VirtualAddress = dwDestRVA;
    pNT64->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_COM_DESCRIPTOR].Size = dwSize;
  }
  dwDestRVA += dwSize;
  if (dwDestRVA % 4) dwDestRVA += (4 - (dwDestRVA % 4));

  // Copy the .NET block of code and metadata into the section, after the header
  dwSize = dwMaxRVA - dwMinRVA;
  pSrc = (LPBYTE)peFile.GetPtrFromRVA(dwMinRVA);
  pDest = (LPBYTE)peDest.GetPtrFromRVA(dwDestRVA);
  CopyMemory(pDest, pSrc, dwSize);

  // Figure out by how much we need to change the RVA's to compensate for the relocation
  diffRVA = dwDestRVA - dwMinRVA;
  pCor = peDest;

  // Fixup the DLL entrypoints
  if (pNT)
  {
    if (pNT->OptionalHeader.AddressOfEntryPoint != dwNewEntrypoint)
    {
      pCor->EntryPointToken = pNT->OptionalHeader.AddressOfEntryPoint;
      pNT->OptionalHeader.AddressOfEntryPoint = dwNewEntrypoint;
    }
  }
  else
  {
    if (pNT64->OptionalHeader.AddressOfEntryPoint != dwNewEntrypoint)
    {
      pCor->EntryPointToken = pNT64->OptionalHeader.AddressOfEntryPoint;
      pNT64->OptionalHeader.AddressOfEntryPoint = dwNewEntrypoint;
    }
  }
  // Adjust the .NET headers to indicate we're a mixed DLL
  pCor->Flags = (pCor->Flags & 0xFFFE) | 0x10;

  // Fixup the metadata header RVA's
  if (pCor->MetaData.VirtualAddress) pCor->MetaData.VirtualAddress += diffRVA;
  if (pCor->Resources.VirtualAddress) pCor->Resources.VirtualAddress += diffRVA;
  if (pCor->StrongNameSignature.VirtualAddress) pCor->StrongNameSignature.VirtualAddress += diffRVA;
  if (pCor->CodeManagerTable.VirtualAddress) pCor->CodeManagerTable.VirtualAddress += diffRVA;
  if (pCor->VTableFixups.VirtualAddress) pCor->VTableFixups.VirtualAddress += diffRVA;
  if (pCor->ExportAddressTableJumps.VirtualAddress) pCor->ExportAddressTableJumps.VirtualAddress += diffRVA;
  if (pCor->ManagedNativeHeader.VirtualAddress) pCor->ManagedNativeHeader.VirtualAddress += diffRVA;

  CMetadata meta(peDest);
  CMetadataTables tables(meta);

  // Fixup all the RVA's for methods and fields that have them in the .NET code
  for (int n = 0; n < 2; n++)
  {
    p = tables.GetTable((n == 0) ? ttMethodDef : ttFieldRVA);
    if (p)
    {
      dwRows = p->GetRowCount();
      for (UINT uRow = 0; uRow < dwRows; uRow ++)
      {
        pdwRVA = (DWORD *)p->Column(uRow, (UINT)0);
        if (*pdwRVA)
          *pdwRVA = (*pdwRVA) + diffRVA;
      }
    }
  }

  // If this is a CE file, then change the processor to x86
  if (pNT)
  {
    if (pNT->OptionalHeader.Subsystem == IMAGE_SUBSYSTEM_WINDOWS_CE_GUI 
      || pNT->FileHeader.Machine == IMAGE_FILE_MACHINE_ARM)
    {
      pNT->FileHeader.Machine = IMAGE_FILE_MACHINE_I386;
      pNT->OptionalHeader.Subsystem = IMAGE_SUBSYSTEM_WINDOWS_CUI;
    }

    if (pNT->OptionalHeader.Subsystem == IMAGE_SUBSYSTEM_WINDOWS_CUI && (pCor->Flags & 0x08))
    {
      PIMAGE_SECTION_HEADER section = IMAGE_FIRST_SECTION(pNT);
      for (UINT i=0; i < pNT->FileHeader.NumberOfSections; i++, section++)
      {
        if (section->SizeOfRawData < section->Misc.VirtualSize)
        {
          if (_tcscmp((LPCSTR)section->Name, _T(".data")) == 0 && dwAdjust > 0)
          {
            _tprintf(_T("\nWARNING: %s section has a RawData size of %d, less than its VirtualSize of %d, adjusting VirtualSize to %d\n"), section->Name, section->SizeOfRawData, section->Misc.VirtualSize, dwAdjust);
            section->Misc.VirtualSize = dwAdjust;
          }
          else
          {
            _tprintf(_T("\nWARNING: %s section has a RawData size of %d and a VirtualSize of %d, strong named image may not run on Windows CE\n"), section->Name, section->SizeOfRawData, section->Misc.VirtualSize);
          }
        }
      }
    }
  }

  if (pCor->Flags & 0x08)
    _tprintf(_T("\nWARNING: %s must be re-signed before it can be used!\n"), pszNative);

  _tprintf(_T("Success!\n"));
}