using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using System.Drawing.Design;

namespace SQLite.Designer.Design
{
  internal class Column
  {
    private bool _allowNulls = false;
    private string _dataType = "";
    private string _defaultValue = "";
    private string _columnName = "";
    private CollationTypeEnum _collate = CollationTypeEnum.Binary;
    private DataGridViewRow _parent;
    private string _checkconstraint = "";
    private Unique _unique;
    private PrimaryKey _primaryKey;
    private Table _table;

    internal Column(DataGridViewRow row)
    {
      _parent = row;
      _table = row.Tag as Table;
      _unique = new Unique(this);
      _primaryKey = new PrimaryKey(this);
    }

    [Browsable(false)]
    internal Table Table
    {
      get { return _table; }
    }

    internal void RefreshGrid()
    {
      _parent.DataGridView.Refresh();
    }

    internal void CellValueChanged()
    {
      if (_parent.DataGridView.CurrentCell.RowIndex != _parent.Index) return;

      object value;

      if (_parent.DataGridView.CurrentCell.IsInEditMode == true)
      {
        if (_parent.DataGridView.EditingControl != null)
          value = ((IDataGridViewEditingControl)_parent.DataGridView.EditingControl).EditingControlFormattedValue;
        else
          value = _parent.DataGridView.CurrentCell.EditedFormattedValue;
      }
      else
        value = _parent.DataGridView.CurrentCell.Value;

      switch (_parent.DataGridView.CurrentCell.ColumnIndex)
      {
        case 0:
          ColumnName = value.ToString();
          break;
        case 1:
          DataType = value.ToString();
          break;
        case 2:
          AllowNulls = Convert.ToBoolean(value);
          break;
      }
    }

    [DisplayName("Check")]
    [Category("Constraints")]
    public virtual string CheckConstraint
    {
      get { return _checkconstraint; }
      set { _checkconstraint = value; }
    }

    [DefaultValue(CollationTypeEnum.Binary)]
    [Category("Constraints")]
    public virtual CollationTypeEnum CollationType
    {
      get { return _collate; }
      set { _collate = value; }
    }

    [Category("Constraints")]
    public virtual Unique Unique
    {
      get { return _unique; }
    }

    [Browsable(false)]
    public virtual string ColumnName
    {
      get { return _columnName; }
      set { _columnName = value; }
    }

    [DisplayName("Primary Key")]
    [Category("Constraints")]
    public virtual PrimaryKey PrimaryKey
    {
      get { return _primaryKey; }
    }

    [DefaultValue(false)]
    [DisplayName("Allow Nulls")]
    [Category("Constraints")]
    public virtual bool AllowNulls
    {
      get { return _allowNulls; }
      set
      {
        if (value != _allowNulls)
        {
          _allowNulls = value;
          _parent.Cells[2].Value = _allowNulls;
        }
      }
    }

    [Browsable(false)]
    public virtual string DataType
    {
      get { return _dataType; }
      set { _dataType = value; }
    }

    [DisplayName("Default Value")]
    [Category("Constraints")]
    public virtual string DefaultValue
    {
      get { return _defaultValue; }
      set { _defaultValue = value; }
    }
  }

  public enum CollationTypeEnum
  {
    Binary = 0,
    CaseInsensitive = 1,
  }
}
