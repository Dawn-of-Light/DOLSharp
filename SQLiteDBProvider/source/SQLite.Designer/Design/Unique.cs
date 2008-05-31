using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace SQLite.Designer.Design
{
  [TypeConverter(typeof(ExpandableObjectConverter))]
  internal class Unique
  {
    private bool _isUnique;
    private ConflictEnum _conflict;
    private Column _column;

    internal Unique(Column col)
    {
      _column = col;
    }

    [DefaultValue(false)]
    [DisplayName("Enabled")]
    public bool Enabled
    {
      get { return _isUnique; }
      set { _isUnique = value; }
    }

    [DefaultValue(ConflictEnum.Abort)]
    [DisplayName("On Conflict")]
    public ConflictEnum Conflict
    {
      get { return _conflict; }
      set { _conflict = value; }
    }

    public override string ToString()
    {
      if (_isUnique == false)
        return Convert.ToString(false);
      else
        return String.Format("{0} ({1})", Convert.ToString(true), Convert.ToString(Conflict));
    }
  }

  public enum ConflictEnum
  {
    Abort = 0,
    Rollback,
    Fail,
    Ignore,
    Replace,
  }
}
