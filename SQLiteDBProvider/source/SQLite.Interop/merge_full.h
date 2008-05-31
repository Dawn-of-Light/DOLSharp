// This code was automatically generated from assembly
// C:\Src\SQLite.NET\System.Data.SQLite\bin\System.Data.SQLite.dll

#include <windef.h>

#pragma data_seg(push,clrseg,".clr")
#pragma comment(linker, "/SECTION:.clr,ER")
  char __ph[144600] = {0}; // The number of bytes to reserve
#pragma data_seg(pop,clrseg)

typedef BOOL (WINAPI *DLLMAIN)(HANDLE, DWORD, LPVOID);
typedef struct EXTRA_STUFF
{
  DWORD dwNativeEntryPoint;
} EXTRA_STUFF, *LPEXTRA_STUFF;

__declspec(dllexport) BOOL WINAPI _CorDllMainStub(HANDLE hModule, DWORD dwReason, LPVOID pvReserved)
{
  HANDLE hMod;
  DLLMAIN proc;
  LPEXTRA_STUFF pExtra;

  hMod = GetModuleHandle(_T("mscoree"));
  if (hMod)
    proc = (DLLMAIN)GetProcAddress(hMod, _T("_CorDllMain"));
  else
  {
    MEMORY_BASIC_INFORMATION mbi;

    VirtualQuery(_CorDllMainStub, &mbi, sizeof(mbi));
    pExtra = (LPEXTRA_STUFF)__ph;
    proc = (DLLMAIN)(pExtra->dwNativeEntryPoint + (DWORD)mbi.AllocationBase);
  }
  return proc(hModule, dwReason, pvReserved);
}
