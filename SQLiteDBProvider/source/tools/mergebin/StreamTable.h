#pragma once
#include "MetaDataTables.h"

typedef enum TableTypes
{
  ttModule = 0,
  ttTypeRef = 1,
  ttTypeDef = 2,
  ttFieldPtr = 3, 
  ttField = 4,
  ttMethodPtr = 5,
  ttMethodDef = 6,
  ttParamPtr = 7, 
  ttParam = 8,
  ttInterfaceImpl = 9,
  ttMemberRef = 10,
  ttConstant = 11, 
  ttCustomAttribute = 12,
  ttFieldMarshal = 13,
  ttPermission = 14,
  ttClassLayout = 15, 
  ttFieldLayout = 16,
  ttStandAloneSig = 17,
  ttEventMap = 18,
  ttEventPtr = 19, 
  ttEvent = 20,
  ttPropertyMap = 21,
  ttPropertyPtr = 22,
  ttProperty = 23, 
  ttMethodSemantics = 24,
  ttMethodImpl = 25,
  ttModuleRef = 26,
  ttTypeSpec = 27, 
  ttImplMap = 28, //lidin book is wrong again here?  It has enclog at 28
  ttFieldRVA = 29,
  ttENCLog = 30,
  ttENCMap = 31, 
  ttAssembly = 32,
  ttAssemblyProcessor= 33,
  ttAssemblyOS = 34,
  ttAssemblyRef = 35, 
  ttAssemblyRefProcessor = 36,
  ttAssemblyRefOS = 37,
  ttFile = 38,
  ttExportedType = 39, 
  ttManifestResource = 40,
  ttNestedClass = 41,
  ttTypeTyPar = 42,
  ttMethodTyPar = 43,
} TableTypes;

#define STRING_INDEXSIZE (m_tables.GetStringIndexSize())
#define GUID_INDEXSIZE (m_tables.GetGuidIndexSize())
#define BLOB_INDEXSIZE (m_tables.GetBlobIndexSize())
#define TABLE_ROWCOUNT(x) (m_tables.TableRowCount(x)[0])
#define TABLE_INDEXSIZE(x) (TABLE_ROWCOUNT(x) > 65535 ? 4 : 2)
#define MAX_INDEXSIZE(x) (m_tables.GetMaxIndexSizeOf(x))

class CStreamTable;

typedef CStreamTable* (CALLBACK* CREATEINSTANCE)(CMetadataTables *);

#define DECLARE_TABLE(classname, typ, nam) \
  public: \
  classname##(CMetadataTables& tables) : CStreamTable(tables) {} \
  UINT GetType() { return typ; } \
  LPCTSTR GetName() { return _T(nam); }

typedef struct TABLE_COLUMN
{
  UINT uSize;
  LPCTSTR pszName;
} TABLE_COLUMNS;

#define BEGIN_COLUMN_MAP() \
  protected: \
  TABLE_COLUMN *GetColumns() \
  { \
    static TABLE_COLUMN map[] = {

#define END_COLUMN_MAP() \
      { 0, NULL } \
    }; \
    return map; \
  }

#define COLUMN_ENTRY(name, size) { size, _T(name) },

class CStreamTable
{
public:
  CStreamTable(CMetadataTables& tables);
  virtual ~CStreamTable(void);

  DWORD GetRowCount();
  LPBYTE operator[](UINT uRow);

  virtual UINT GetType() = 0;
  virtual LPCTSTR GetName() = 0;

  template<class C> static CStreamTable * CALLBACK CreateInstance(CMetadataTables *ptables);

protected:
  CMetadataTables& m_tables;
  LPBYTE m_pbData;

  virtual UINT GetRowSize();
  virtual UINT GetColumnCount();

  virtual TABLE_COLUMN *GetColumns() = 0;

private:
  void Init();
};

class CModuleTable : public CStreamTable
{
  DECLARE_TABLE(CModuleTable, ttModule, "Module")
  
  BEGIN_COLUMN_MAP()
    COLUMN_ENTRY("Generation", sizeof(WORD))
    COLUMN_ENTRY("Name",       STRING_INDEXSIZE)
    COLUMN_ENTRY("Mvid",       GUID_INDEXSIZE)
    COLUMN_ENTRY("EncId",      GUID_INDEXSIZE)
    COLUMN_ENTRY("EncBaseId",  GUID_INDEXSIZE)
  END_COLUMN_MAP()
};

static UINT ResolutionScopeIndex[] = {ttModule, ttModuleRef, ttAssemblyRef, ttTypeRef, 0};

class CTypeRefTable : public CStreamTable
{
  DECLARE_TABLE(CTypeRefTable, ttTypeRef, "TypeRef")

  BEGIN_COLUMN_MAP()
    COLUMN_ENTRY("ResolutionScope", MAX_INDEXSIZE(ResolutionScopeIndex))
    COLUMN_ENTRY("TypeName", STRING_INDEXSIZE)
    COLUMN_ENTRY("TypeNamespace", STRING_INDEXSIZE)
  END_COLUMN_MAP()
};

static UINT TypeDefOrRefIndex[] = {ttTypeDef, ttTypeRef, ttTypeSpec, 0};

class CTypeDefTable : public CStreamTable
{
  DECLARE_TABLE(CTypeDefTable, ttTypeDef, "TypeDef")

  BEGIN_COLUMN_MAP()
    COLUMN_ENTRY("Flags", sizeof(DWORD))
    COLUMN_ENTRY("TypeName", STRING_INDEXSIZE)
    COLUMN_ENTRY("TypeNamespace", STRING_INDEXSIZE)
    COLUMN_ENTRY("Extends", MAX_INDEXSIZE(TypeDefOrRefIndex))
    COLUMN_ENTRY("FieldList", TABLE_INDEXSIZE(ttField))
    COLUMN_ENTRY("MethodList", TABLE_INDEXSIZE(ttMethodDef))
  END_COLUMN_MAP()
};

extern CREATEINSTANCE g_arTableTypes[64];