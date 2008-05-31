// setup.cpp : Defines the entry point for the application.
//

#include "stdafx.h"
#include "setup.h"

int APIENTRY _tWinMain(HINSTANCE hInstance,
                     HINSTANCE /*hPrevInstance*/,
                     LPTSTR    /*lpCmdLine*/,
                     int       /*nCmdShow*/)
{
  HRSRC hRes = FindResource(hInstance, MAKEINTRESOURCE(1), _T("MSI"));
  HGLOBAL hGlob = LoadResource(hInstance, hRes);
  DWORD dwSize = SizeofResource(hInstance, hRes);
  LPVOID pv  = LockResource(hGlob);
  TCHAR szDir[MAX_PATH];
  TCHAR szPath[MAX_PATH];

  GetTempPath(MAX_PATH, szDir);
  GetTempFileName(szDir, _T("tmp"), 0, szPath);
  DeleteFile(szPath);
  lstrcat(szPath, _T(".msi"));

  HANDLE hFile = CreateFile(szPath, GENERIC_WRITE, 0, NULL, CREATE_ALWAYS, 0, NULL);
  WriteFile(hFile, pv, dwSize, &dwSize, NULL);
  CloseHandle(hFile);

  SHELLEXECUTEINFO shex;

  ZeroMemory(&shex, sizeof(shex));

  shex.cbSize = sizeof(shex);
  shex.fMask = SEE_MASK_NOCLOSEPROCESS;

  shex.lpFile = szPath;

  if (ShellExecuteEx(&shex))
  {
    if (shex.hProcess)
    {
      WaitForSingleObject(shex.hProcess, INFINITE);
    }
  }
  while (!DeleteFile(szPath))
    Sleep(250);

  return 0;
}
