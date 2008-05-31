/********************************************************
 * mergebin
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

#include "StdAfx.h"
#include "TableData.h"

CREATEINSTANCE g_arTableTypes[64] = {
  CTableData::CreateInstance<CModuleTable>,
  CTableData::CreateInstance<CTypeRefTable>,
  CTableData::CreateInstance<CTypeDefTable>,
  CTableData::CreateInstance<CFieldPtrTable>,
  CTableData::CreateInstance<CFieldTable>,
  CTableData::CreateInstance<CMethodPtrTable>,
  CTableData::CreateInstance<CMethodTable>,
  CTableData::CreateInstance<CParamPtrTable>,
  CTableData::CreateInstance<CParamTable>,
  CTableData::CreateInstance<CInterfaceImplTable>,
  CTableData::CreateInstance<CMemberRefTable>,
  CTableData::CreateInstance<CConstantTable>,
  CTableData::CreateInstance<CCustomAttributeTable>,
  CTableData::CreateInstance<CFieldMarshalTable>,
  CTableData::CreateInstance<CDeclSecurityTable>,
  CTableData::CreateInstance<CClassLayoutTable>,
  CTableData::CreateInstance<CFieldLayoutTable>,
  CTableData::CreateInstance<CStandAloneSigTable>,
  CTableData::CreateInstance<CEventMapTable>,
  CTableData::CreateInstance<CEventPtrTable>,
  CTableData::CreateInstance<CEventTable>,
  CTableData::CreateInstance<CPropertyMapTable>,
  CTableData::CreateInstance<CPropertyPtrTable>,
  CTableData::CreateInstance<CPropertyTable>,
  CTableData::CreateInstance<CMethodSemanticsTable>,
  CTableData::CreateInstance<CMethodImplTable>,
  CTableData::CreateInstance<CModuleRefTable>,
  CTableData::CreateInstance<CTypeSpecTable>,
  CTableData::CreateInstance<CImplMapTable>,
  CTableData::CreateInstance<CFieldRVATable>,
  NULL,
};

CTableData::CTableData(CMetadataTables& tables) : m_tables(tables)
{
  m_uiRowSize = 0;
}

CTableData::~CTableData(void)
{
  if (m_pColumns) 
    delete[] m_pColumns;
}

template<class C>
static CTableData * CALLBACK CTableData::CreateInstance(CMetadataTables *ptables)
{
  CTableData *p = new C(*ptables);
  p->Init();

  return p;
}

void CTableData::Init()
{
  m_pbData = m_tables.m_pbData;
  CTableData *p;
  UINT n = GetType();
  
  while(n--)
  {
    p = m_tables.GetTable(n);
    if (p)
    {
      m_pbData = p->m_pbData + (p->GetRowSize() * p->GetRowCount());
      break;
    }
  }

  m_pColumns = _CreateColumns();
  TABLE_COLUMNS *tc = m_pColumns + 1;

  while (tc->pszName && !tc->dwOffset)
  {
    tc->dwOffset = tc[-1].dwOffset + tc[-1].uSize;
    tc ++;
  }
}

TABLE_COLUMN * CTableData::GetColumns()
{
  return m_pColumns;
}

DWORD CTableData::GetRowCount()
{
  return *m_tables.TableRowCount(GetType());
}

UINT CTableData::GetRowSize()
{
  if (m_uiRowSize == 0)
  {
    TABLE_COLUMN *pc = m_pColumns;

    while (pc->uSize)
    {
      m_uiRowSize += pc->uSize;
      pc ++;
    }
  }

  return m_uiRowSize;
}

UINT CTableData::GetColumnCount()
{
  TABLE_COLUMN *pc = m_pColumns;
  UINT ucount = 0;

  while (pc->uSize)
  {
    ucount ++;
    pc ++;
  }
  return ucount;
}

int CTableData::GetColumnIndex(LPCTSTR pszName)
{
  TABLE_COLUMN *pc = m_pColumns;
  for (int n = 0; pc[n].pszName; n++)
  {
    if (lstrcmpi(pszName, pc[n].pszName) == 0) return n;
  }
  return -1;
}

UINT CTableData::GetColumnSize(UINT uIndex)
{
  return m_pColumns[uIndex].uSize;
}

UINT CTableData::GetColumnSize(LPCTSTR pszName)
{
  int n = GetColumnIndex(pszName);
  if (n < 0) return 0;

  return GetColumnSize(n);
}

LPBYTE CTableData::Column(UINT uRow, UINT uIndex)
{
  TABLE_COLUMN *pc = m_pColumns;
  LPBYTE pb = m_pbData + (uRow * GetRowSize()) + pc[uIndex].dwOffset;

  return pb;
}

LPBYTE CTableData::Column(UINT uRow, LPCTSTR pszName)
{
  int n = GetColumnIndex(pszName);
  if (n < 0) return NULL;
  return Column(uRow, n);
}
