/********************************************************
 * mergebin
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

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
  ttDeclSecurity = 14,
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
  ttImplMap = 28,
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
  ttGenericParam = 42,
  ttMethodSpec = 43,
  ttGenericParamConstraint = 44,
} TableTypes;

typedef struct TABLE_COLUMN
{
  UINT uSize;
  LPCTSTR pszName;
  DWORD dwOffset;
} TABLE_COLUMNS;

class CTableData;

/*
** Helpers
*/
#define STRING_INDEXSIZE (m_tables.GetStringIndexSize())
#define GUID_INDEXSIZE (m_tables.GetGuidIndexSize())
#define BLOB_INDEXSIZE (m_tables.GetBlobIndexSize())
#define TABLE_ROWCOUNT(x) (m_tables.TableRowCount(x)[0])
#define TABLE_INDEXSIZE(x) (TABLE_ROWCOUNT(x) > 65535 ? 4 : 2)
#define MAX_INDEXSIZE(x) (m_tables.GetMaxIndexSizeOf(x))

#define DECLARE_TABLE(classname, typ, nam) \
  public: \
  classname##(CMetadataTables& tables) : CTableData(tables) {} \
  UINT GetType() { return typ; } \
  LPCTSTR GetName() { return _T(nam); }

#define BEGIN_COLUMN_MAP() \
  protected: \
  TABLE_COLUMN *_CreateColumns() \
  { \
    TABLE_COLUMN map[] = {

#define END_COLUMN_MAP() \
      { 0, NULL } \
    }; \
    TABLE_COLUMN *p = new TABLE_COLUMN[sizeof(map) / sizeof(TABLE_COLUMN)]; \
    CopyMemory(p, map, sizeof(map)); \
    return p; \
  }

#define COLUMN_ENTRY(name, size) { size, _T(name), 0 },
/*
** End Of Helpers
*/

class CTableData
{
public:
  CTableData(CMetadataTables& tables);
  virtual ~CTableData(void);

  virtual UINT          GetType        () = 0;
  virtual LPCTSTR       GetName        () = 0;
  virtual TABLE_COLUMN *_CreateColumns () = 0;

  template<class C> static CTableData * CALLBACK CreateInstance(CMetadataTables *ptables);

  virtual DWORD         GetRowCount ();
  virtual UINT          GetRowSize  ();

  int           GetColumnIndex (LPCTSTR pszName);
  UINT          GetColumnSize  (UINT uIndex);
  UINT          GetColumnSize  (LPCTSTR pszName);
  UINT          GetColumnCount ();
  LPBYTE        Column         (UINT uRow, UINT uIndex);
  LPBYTE        Column         (UINT uRow, LPCTSTR pszName);
  TABLE_COLUMN *GetColumns     ();

protected:
  CMetadataTables& m_tables;
  LPBYTE           m_pbData;
  UINT             m_uiRowSize;
  TABLE_COLUMN    *m_pColumns;

private:
  void Init ();
};

class CModuleTable : public CTableData
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

static UINT TypeDefOrRefIndex[]        = { ttTypeDef,   ttTypeRef,     ttTypeSpec,     0 };
static UINT HasConstantIndex[]         = { ttField,     ttParam,       ttProperty,     0 };
static UINT HasCustomAttributeIndex[]  = { ttMethodDef, ttField,       ttTypeRef,      ttTypeDef,   ttParam,    ttInterfaceImpl, ttMemberRef, ttModule, ttDeclSecurity, ttProperty, ttEvent, ttStandAloneSig, ttModuleRef, ttTypeSpec, ttAssembly, ttAssemblyRef, ttFile, ttExportedType, ttManifestResource, 0 };
static UINT HasFieldMarshalIndex[]     = { ttField,     ttParam,       0 };
static UINT HasDeclSecurityIndex[]     = { ttTypeDef,   ttMethodDef,   ttAssembly,     0 };
static UINT MemberRefParentIndex[]     = { ttTypeDef,   ttTypeRef,     ttModuleRef,    ttMethodDef, ttTypeSpec, 0 };
static UINT HasSemanticsIndex[]        = { ttEvent,     ttProperty,    0 };
static UINT MethodDefOrRefIndex[]      = { ttMethodDef, ttMemberRef,   0 };
static UINT MemberForwardedIndex[]     = { ttField,     ttMethodDef,   0 };
static UINT ImplementationIndex[]      = { ttFile,      ttAssemblyRef, ttExportedType, 0 };
static UINT CustomAttributeTypeIndex[] = { 63,          63,            ttMethodDef,    ttMemberRef, 63,         0 };
static UINT ResolutionScopeIndex[]     = { ttModule,    ttModuleRef,   ttAssemblyRef,  ttTypeRef,   0 };
static UINT TypeOrMethodDefIndex[]     = { ttTypeDef,   ttMethodDef,   0 };

class CTypeRefTable : public CTableData
{
  DECLARE_TABLE(CTypeRefTable, ttTypeRef, "TypeRef")

  BEGIN_COLUMN_MAP()
    COLUMN_ENTRY("ResolutionScope", MAX_INDEXSIZE(ResolutionScopeIndex))
    COLUMN_ENTRY("TypeName",        STRING_INDEXSIZE)
    COLUMN_ENTRY("TypeNamespace",   STRING_INDEXSIZE)
  END_COLUMN_MAP()
};

class CTypeDefTable : public CTableData
{
  DECLARE_TABLE(CTypeDefTable, ttTypeDef, "TypeDef")

  BEGIN_COLUMN_MAP()
    COLUMN_ENTRY("Flags",         sizeof(DWORD))
    COLUMN_ENTRY("TypeName",      STRING_INDEXSIZE)
    COLUMN_ENTRY("TypeNamespace", STRING_INDEXSIZE)
    COLUMN_ENTRY("Extends",       MAX_INDEXSIZE(TypeDefOrRefIndex))
    COLUMN_ENTRY("FieldList",     TABLE_INDEXSIZE(ttField))
    COLUMN_ENTRY("MethodList",    TABLE_INDEXSIZE(ttMethodDef))
  END_COLUMN_MAP()
};

class CFieldPtrTable : public CTableData
{
  DECLARE_TABLE(CFieldPtrTable, ttFieldPtr, "FieldPtr")

  BEGIN_COLUMN_MAP()
    COLUMN_ENTRY("Field", TABLE_INDEXSIZE(ttField))
  END_COLUMN_MAP()
};

class CFieldTable : public CTableData
{
  DECLARE_TABLE(CFieldTable, ttField, "Field")

  BEGIN_COLUMN_MAP()
    COLUMN_ENTRY("Flags",     sizeof(WORD))
    COLUMN_ENTRY("Name",      STRING_INDEXSIZE)
    COLUMN_ENTRY("Signature", BLOB_INDEXSIZE)
  END_COLUMN_MAP()
};

class CMethodPtrTable : public CTableData
{
  DECLARE_TABLE(CMethodPtrTable, ttMethodPtr, "MethodPtr")

  BEGIN_COLUMN_MAP()
    COLUMN_ENTRY("Method", TABLE_INDEXSIZE(ttMethodDef))
  END_COLUMN_MAP()
};

class CMethodTable : public CTableData
{
  DECLARE_TABLE(CMethodTable, ttMethodDef, "Method")

  BEGIN_COLUMN_MAP()
    COLUMN_ENTRY("RVA",        sizeof(DWORD))
    COLUMN_ENTRY("ImplFlags",  sizeof(WORD))
    COLUMN_ENTRY("Flags",      sizeof(WORD))
    COLUMN_ENTRY("Name",       STRING_INDEXSIZE)
    COLUMN_ENTRY("Signature",  BLOB_INDEXSIZE)
    COLUMN_ENTRY("Parameters", TABLE_INDEXSIZE(ttParam))
  END_COLUMN_MAP()
};

class CParamPtrTable : public CTableData
{
  DECLARE_TABLE(CParamPtrTable, ttParamPtr, "ParamPtr")

  BEGIN_COLUMN_MAP()
    COLUMN_ENTRY("Param", TABLE_INDEXSIZE(ttParam))
  END_COLUMN_MAP()
};

class CParamTable : public CTableData
{
  DECLARE_TABLE(CParamTable, ttParam, "Param")

  BEGIN_COLUMN_MAP()
    COLUMN_ENTRY("Flags",    sizeof(WORD))
    COLUMN_ENTRY("Sequence", sizeof(WORD))
    COLUMN_ENTRY("Name",     STRING_INDEXSIZE)
  END_COLUMN_MAP()
};

class CInterfaceImplTable : public CTableData
{
  DECLARE_TABLE(CInterfaceImplTable, ttInterfaceImpl, "Interface")

  BEGIN_COLUMN_MAP()
    COLUMN_ENTRY("Class",     TABLE_INDEXSIZE(ttTypeDef))
    COLUMN_ENTRY("Interface", MAX_INDEXSIZE(TypeDefOrRefIndex))
  END_COLUMN_MAP()
};

class CMemberRefTable : public CTableData
{
  DECLARE_TABLE(CMemberRefTable, ttMemberRef, "Member")

  BEGIN_COLUMN_MAP()
    COLUMN_ENTRY("Class",     MAX_INDEXSIZE(MemberRefParentIndex))
    COLUMN_ENTRY("Name",      STRING_INDEXSIZE)
    COLUMN_ENTRY("Signature", BLOB_INDEXSIZE)
  END_COLUMN_MAP()
};

class CConstantTable : public CTableData
{
  DECLARE_TABLE(CConstantTable, ttConstant, "Constant")

  BEGIN_COLUMN_MAP()
    COLUMN_ENTRY("Type",   sizeof(WORD))
    COLUMN_ENTRY("Parent", MAX_INDEXSIZE(HasConstantIndex))
    COLUMN_ENTRY("Value",  BLOB_INDEXSIZE)
  END_COLUMN_MAP()
};

class CCustomAttributeTable : public CTableData
{
  DECLARE_TABLE(CCustomAttributeTable, ttCustomAttribute, "CustomAttribute")

  BEGIN_COLUMN_MAP()
    COLUMN_ENTRY("Parent", MAX_INDEXSIZE(HasCustomAttributeIndex))
    COLUMN_ENTRY("Type",   MAX_INDEXSIZE(CustomAttributeTypeIndex))
    COLUMN_ENTRY("Value",  BLOB_INDEXSIZE)
  END_COLUMN_MAP()
};

class CFieldMarshalTable : public CTableData
{
  DECLARE_TABLE(CFieldMarshalTable, ttFieldMarshal, "FieldMarshal")

  BEGIN_COLUMN_MAP()
    COLUMN_ENTRY("Parent",     MAX_INDEXSIZE(HasFieldMarshalIndex))
    COLUMN_ENTRY("NativeType", BLOB_INDEXSIZE)
  END_COLUMN_MAP()
};

class CDeclSecurityTable : public CTableData
{
  DECLARE_TABLE(CDeclSecurityTable, ttDeclSecurity, "DeclSecurity")

  BEGIN_COLUMN_MAP()
    COLUMN_ENTRY("Action",        sizeof(WORD))
    COLUMN_ENTRY("Parent",        MAX_INDEXSIZE(HasDeclSecurityIndex))
    COLUMN_ENTRY("PermissionSet", BLOB_INDEXSIZE)
  END_COLUMN_MAP()
};

class CClassLayoutTable : public CTableData
{
  DECLARE_TABLE(CClassLayoutTable, ttClassLayout, "ClassLayout")
  
  BEGIN_COLUMN_MAP()
    COLUMN_ENTRY("PackingSize", sizeof(WORD))
    COLUMN_ENTRY("ClassSize",   sizeof(DWORD))
    COLUMN_ENTRY("Parent",      TABLE_INDEXSIZE(ttTypeDef))
  END_COLUMN_MAP()
};

class CFieldLayoutTable : public CTableData
{
  DECLARE_TABLE(CFieldLayoutTable, ttFieldLayout, "FieldLayout")

  BEGIN_COLUMN_MAP()
    COLUMN_ENTRY("Offset", sizeof(DWORD))
    COLUMN_ENTRY("Field",  TABLE_INDEXSIZE(ttField))
  END_COLUMN_MAP()
};

class CStandAloneSigTable : public CTableData
{
  DECLARE_TABLE(CStandAloneSigTable, ttStandAloneSig, "StandAloneSig")

  BEGIN_COLUMN_MAP()
    COLUMN_ENTRY("Signature", BLOB_INDEXSIZE)
  END_COLUMN_MAP()
};

class CEventMapTable : public CTableData
{
  DECLARE_TABLE(CEventMapTable, ttEventMap, "EventMap")

  BEGIN_COLUMN_MAP()
    COLUMN_ENTRY("Parent",    TABLE_INDEXSIZE(ttTypeDef))
    COLUMN_ENTRY("EventList", TABLE_INDEXSIZE(ttEvent))
  END_COLUMN_MAP()
};

class CEventTable : public CTableData
{
  DECLARE_TABLE(CEventTable, ttEvent, "Event")

  BEGIN_COLUMN_MAP()
    COLUMN_ENTRY("EventFlags", sizeof(WORD))
    COLUMN_ENTRY("Name",       STRING_INDEXSIZE)
    COLUMN_ENTRY("EventType",  MAX_INDEXSIZE(TypeDefOrRefIndex))
  END_COLUMN_MAP()
};

class CEventPtrTable : public CTableData
{
  DECLARE_TABLE(CEventPtrTable, ttEventPtr, "EventPtr")

  BEGIN_COLUMN_MAP()
    COLUMN_ENTRY("Event", TABLE_INDEXSIZE(ttEvent))
  END_COLUMN_MAP()
};

class CPropertyMapTable : public CTableData
{
  DECLARE_TABLE(CPropertyMapTable, ttPropertyMap, "PropertyMap")

  BEGIN_COLUMN_MAP()
    COLUMN_ENTRY("Parent",       TABLE_INDEXSIZE(ttTypeDef))
    COLUMN_ENTRY("PropertyList", TABLE_INDEXSIZE(ttProperty))
  END_COLUMN_MAP()
};

class CPropertyPtrTable : public CTableData
{
  DECLARE_TABLE(CPropertyPtrTable, ttEventPtr, "PropertyPtr")

  BEGIN_COLUMN_MAP()
    COLUMN_ENTRY("Property", TABLE_INDEXSIZE(ttProperty))
  END_COLUMN_MAP()
};

class CPropertyTable : public CTableData
{
  DECLARE_TABLE(CPropertyTable, ttProperty, "Property")

  BEGIN_COLUMN_MAP()
    COLUMN_ENTRY("Flags", sizeof(WORD))
    COLUMN_ENTRY("Name",  STRING_INDEXSIZE)
    COLUMN_ENTRY("Type",  BLOB_INDEXSIZE)
  END_COLUMN_MAP()
};

class CMethodSemanticsTable : public CTableData
{
  DECLARE_TABLE(CMethodSemanticsTable, ttMethodSemantics, "MethodSemantics")

  BEGIN_COLUMN_MAP()
    COLUMN_ENTRY("Semantics",   sizeof(WORD))
    COLUMN_ENTRY("Method",      TABLE_INDEXSIZE(ttMethodDef))
    COLUMN_ENTRY("Association", MAX_INDEXSIZE(HasSemanticsIndex))
  END_COLUMN_MAP()
};

class CMethodImplTable : public CTableData
{
  DECLARE_TABLE(CMethodImplTable, ttMethodImpl, "MethodImpl")

  BEGIN_COLUMN_MAP()
    COLUMN_ENTRY("Class",             TABLE_INDEXSIZE(ttTypeDef))
    COLUMN_ENTRY("MethodBody",        MAX_INDEXSIZE(MethodDefOrRefIndex))
    COLUMN_ENTRY("MethodDeclaration", MAX_INDEXSIZE(MethodDefOrRefIndex))
  END_COLUMN_MAP()
};

class CModuleRefTable : public CTableData
{
  DECLARE_TABLE(CModuleRefTable, ttModuleRef, "ModuleRef")

  BEGIN_COLUMN_MAP()
    COLUMN_ENTRY("Name", STRING_INDEXSIZE)
  END_COLUMN_MAP()
};

class CTypeSpecTable : public CTableData
{
  DECLARE_TABLE(CTypeSpecTable, ttTypeSpec, "TypeSpec")

  BEGIN_COLUMN_MAP()
    COLUMN_ENTRY("Signature", BLOB_INDEXSIZE)
  END_COLUMN_MAP()
};

class CImplMapTable : public CTableData
{
  DECLARE_TABLE(CImplMapTable, ttImplMap, "ImplMap")

  BEGIN_COLUMN_MAP()
    COLUMN_ENTRY("MappingFlags",    sizeof(WORD))
    COLUMN_ENTRY("MemberForwarded", MAX_INDEXSIZE(MemberForwardedIndex))
    COLUMN_ENTRY("ImportName",      STRING_INDEXSIZE)
    COLUMN_ENTRY("ImportScope",     TABLE_INDEXSIZE(ttModuleRef))
  END_COLUMN_MAP()
};

class CFieldRVATable : public CTableData
{
  DECLARE_TABLE(CFieldRVATable, ttFieldRVA, "FieldRVA")

  BEGIN_COLUMN_MAP()
    COLUMN_ENTRY("RVA",   sizeof(DWORD))
    COLUMN_ENTRY("Field", TABLE_INDEXSIZE(ttField))
  END_COLUMN_MAP()
};

// Only tables up to ttFieldRVA are mapped, because they're all I needed.  If you need the tables beyond that, map them yourself!

typedef CTableData* (CALLBACK* CREATEINSTANCE)(CMetadataTables *);
extern CREATEINSTANCE g_arTableTypes[64];