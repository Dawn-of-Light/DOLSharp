#include "StdAfx.h"
#include "StreamTable.h"

CREATEINSTANCE g_arTableTypes[64] = {
  CStreamTable::CreateInstance<CModuleTable>,
  CStreamTable::CreateInstance<CTypeRefTable>,
  CStreamTable::CreateInstance<CTypeDefTable>,
  NULL,
};

CStreamTable::CStreamTable(CMetadataTables& tables) : m_tables(tables)
{
}

CStreamTable::~CStreamTable(void)
{
}

template<class C>
static CStreamTable * CALLBACK CStreamTable::CreateInstance(CMetadataTables *ptables)
{
  CStreamTable *p = new C(*ptables);
  p->Init();

  return p;
}

void CStreamTable::Init()
{
  m_pbData = m_tables.m_pbData;
  CStreamTable *p;

  for (UINT n = 0; n < GetType(); n++)
  {
    p = m_tables.GetTable(n);
    if (p)
    {
      m_pbData += (p->GetRowSize() * p->GetRowCount());
    }
  }
}

DWORD CStreamTable::GetRowCount()
{
  return *m_tables.TableRowCount(GetType());
}

UINT CStreamTable::GetRowSize()
{
  TABLE_COLUMN *pc = GetColumns();
  UINT ulen = 0;

  while (pc->uSize)
  {
    ulen += pc->uSize;
    pc ++;
  }

  return ulen;
}

UINT CStreamTable::GetColumnCount()
{
  TABLE_COLUMN *pc = GetColumns();
  UINT ucount = 0;

  while (pc->uSize)
  {
    ucount ++;
    pc ++;
  }
  return ucount;
}

LPBYTE CStreamTable::operator[](UINT uRow)
{
  return m_pbData + (uRow * GetRowSize());
}
