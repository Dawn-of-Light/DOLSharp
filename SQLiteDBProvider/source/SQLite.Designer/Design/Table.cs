namespace SQLite.Designer.Design
{
  using System;
  using System.Data.Common;
  using System.ComponentModel.Design;
  using System.ComponentModel;
  using System.Drawing.Design;
  using System.Data;
  using SQLite.Designer.Editors;

  internal class Table
  {
    private string _name;
    string _catalog;

    internal TableDesignerDoc _owner;
    internal DbConnection _connection;
    internal Table(string tableName, DbConnection connection, TableDesignerDoc owner)
    {
      _owner = owner;
      _connection = connection;
      Name = tableName;
      Catalog = _connection.Database; // main
    }

    [Category("Storage")]
    [RefreshProperties(RefreshProperties.All)]
    public string Name
    {
      get { return _name; }
      set
      {
        _name = value;
        _owner.Name = value;
      }
    }

    [Category("Storage")]
    [Editor(typeof(CatalogTypeEditor), typeof(UITypeEditor))]
    [DefaultValue("main")]
    [RefreshProperties(RefreshProperties.All)]
    public string Catalog
    {
      get { return _catalog; }
      set
      {
        string catalogs = "";
        using (DataTable table = _connection.GetSchema("Catalogs"))
        {
          foreach (DataRow row in table.Rows)
          {
            catalogs += (row[0].ToString() + ",");
          }
        }

        if (catalogs.IndexOf(value + ",", StringComparison.OrdinalIgnoreCase) == -1)
          throw new ArgumentOutOfRangeException("Unrecognized catalog!");

        _catalog = value;
      }
    }
  }

  internal class CatalogTypeEditor : ObjectSelectorEditor
  {
    public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
    {
      return UITypeEditorEditStyle.DropDown;
    }

    protected override void FillTreeWithData(Selector selector, ITypeDescriptorContext context, IServiceProvider provider)
    {
      base.FillTreeWithData(selector, context, provider);
      Table source = context.Instance as Table;

      if (source == null) return;

      using (DataTable table = source._connection.GetSchema("Catalogs"))
      {
        foreach (DataRow row in table.Rows)
        {
          selector.AddNode(row[0].ToString(), row[0], null);
        }
      }
    }
  }
}
