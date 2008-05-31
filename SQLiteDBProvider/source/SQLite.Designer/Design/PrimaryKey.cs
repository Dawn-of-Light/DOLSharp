using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace SQLite.Designer.Design
{
  [TypeConverter(typeof(ExpandableObjectConverter))]
  internal class PrimaryKey
  {
    private bool _primaryKey;
    private bool _autoIncrement;
    private IndexDirection _direction;
    private ConflictEnum _conflict;
    Column _column;

    internal PrimaryKey(Column col)
    {
      _column = col;
    }

    [RefreshProperties(RefreshProperties.All)]
    [DefaultValue(false)]
    public bool Enabled
    {
      get { return _primaryKey; }
      set
      {
        _primaryKey = value;

        if (_primaryKey == false)
          AutoIncrement = false;

        _column.RefreshGrid();
      }
    }

    [RefreshProperties(RefreshProperties.All)]
    [DefaultValue(false)]
    [DisplayName("Auto Increment")]
    public bool AutoIncrement
    {
      get { return _autoIncrement; }
      set
      {
        if (_primaryKey == false && value == true)
          Enabled = true;

        _autoIncrement = value;
      }
    }

    [DefaultValue(ConflictEnum.Abort)]
    [DisplayName("On Conflict")]
    public ConflictEnum Conflict
    {
      get { return _conflict; }
      set { _conflict = value; }
    }

    [DefaultValue(IndexDirection.Ascending)]
    [DisplayName("Sort Mode")]
    public IndexDirection SortMode
    {
      get { return _direction; }
      set { _direction = value; }
    }

    public override string ToString()
    {
      return Enabled.ToString();
    }
  }

  internal enum IndexDirection
  {
    Ascending = 0,
    Descending = 1,
  }
}
