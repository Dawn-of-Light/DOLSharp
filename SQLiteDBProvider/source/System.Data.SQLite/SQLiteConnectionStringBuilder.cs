/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace System.Data.SQLite
{
  using System;
  using System.Data.Common;
  using System.ComponentModel;
  using System.Collections;
  using System.Globalization;
  using System.Reflection;

#if !PLATFORM_COMPACTFRAMEWORK
  using System.ComponentModel.Design;

  /// <summary>
  /// SQLite implementation of DbConnectionStringBuilder.
  /// </summary>
  [DefaultProperty("DataSource")]
  [DefaultMember("Item")]
  public sealed class SQLiteConnectionStringBuilder : DbConnectionStringBuilder
  {
    /// <summary>
    /// Properties of this class
    /// </summary>
    private Hashtable _properties;

    /// <overloads>
    /// Constructs a new instance of the class
    /// </overloads>
    /// <summary>
    /// Default constructor
    /// </summary>
    public SQLiteConnectionStringBuilder()
    {
      Initialize(null);
    }

    /// <summary>
    /// Constructs a new instance of the class using the specified connection string.
    /// </summary>
    /// <param name="connectionString">The connection string to parse</param>
    public SQLiteConnectionStringBuilder(string connectionString)
    {
      Initialize(connectionString);
    }

    /// <summary>
    /// Private initializer, which assigns the connection string and resets the builder
    /// </summary>
    /// <param name="cnnString">The connection string to assign</param>
    private void Initialize(string cnnString)
    {
      _properties = new Hashtable();
      base.GetProperties(_properties);

      if (String.IsNullOrEmpty(cnnString) == false)
        ConnectionString = cnnString;
    }

    /// <summary>
    /// Gets/Sets the default version of the SQLite engine to instantiate.  Currently the only valid value is 3, indicating version 3 of the sqlite library.
    /// </summary>
    [Browsable(true)]
    [DefaultValue(3)]
    public int Version
    {
      get
      {
        object value;
        TryGetValue("Version", out value);
        return Convert.ToInt32(value, CultureInfo.CurrentCulture);
      }
      set
      {
        if (value != 3)
          throw new NotSupportedException();

        this["Version"] = value;
      }
    }

    /// <summary>
    /// Gets/Sets the synchronous mode of the connection string.  Default is "Normal".
    /// </summary>
    [DisplayName("Synchronous")]
    [Browsable(true)]
    [DefaultValue(SynchronizationModes.Normal)]
    public SynchronizationModes SyncMode
    {
      get
      {
        object value;
        TryGetValue("Synchronous", out value);
        if (value is string)
          return (SynchronizationModes)TypeDescriptor.GetConverter(typeof(SynchronizationModes)).ConvertFrom(value);
        else return (SynchronizationModes)value;
      }
      set
      {
        this["Synchronous"] = value;
      }
    }

    /// <summary>
    /// Gets/Sets the encoding for the connection string.  The default is "False" which indicates UTF-8 encoding.
    /// </summary>
    [Browsable(true)]
    [DefaultValue(false)]
    public bool UseUTF16Encoding
    {
      get
      {
        object value;
        TryGetValue("UseUTF16Encoding", out value);
        return Convert.ToBoolean(value, CultureInfo.CurrentCulture);
      }
      set
      {
        this["UseUTF16Encoding"] = value;
      }
    }

    /// <summary>
    /// Gets/Sets whether or not to use connection pooling.  The default is "False"
    /// </summary>
    [Browsable(true)]
    [DefaultValue(false)]
    public bool Pooling
    {
      get
      {
        object value;
        TryGetValue("Pooling", out value);
        return Convert.ToBoolean(value, CultureInfo.CurrentCulture);
      }
      set
      {
        this["Pooling"] = value;
      }
    }

    /// <summary>
    /// Gets/Sets whethor not to store GUID's in binary format.  The default is True
    /// which saves space in the database.
    /// </summary>
    [Browsable(true)]
    [DefaultValue(true)]
    public bool BinaryGUID
    {
      get
      {
        object value;
        TryGetValue("BinaryGUID", out value);
        return Convert.ToBoolean(value, CultureInfo.CurrentCulture);
      }
      set
      {
        this["BinaryGUID"] = value;
      }
    }

    /// <summary>
    /// Gets/Sets the filename to open on the connection string.
    /// </summary>
    [DisplayName("Data Source")]
    [Browsable(true)]
    [DefaultValue("")]
    public string DataSource
    {
      get
      {
        object value;
        TryGetValue("Data Source", out value);
        return value.ToString();
      }
      set
      {
        this["Data Source"] = value;
      }
    }

    /// <summary>
    /// Gets/sets the default command timeout for newly-created commands.  This is especially useful for 
    /// commands used internally such as inside a SQLiteTransaction, where setting the timeout is not possible.
    /// </summary>
    [DisplayName("Default Timeout")]
    [Browsable(true)]
    [DefaultValue(30)]
    public int DefaultTimeout
    {
      get
      {
        object value;
        TryGetValue("Default Timeout", out value);
        return Convert.ToInt32(value, CultureInfo.CurrentCulture);
      }
      set
      {
        this["Default Timeout"] = value;
      }
    }

    /// <summary>
    /// Determines whether or not the connection will automatically participate
    /// in the current distributed transaction (if one exists)
    /// </summary>
    [Browsable(true)]
    [DefaultValue(true)]
    public bool Enlist
    {
      get
      {
        object value;
        TryGetValue("Enlist", out value);
        return Convert.ToBoolean(value, CultureInfo.CurrentCulture);
      }
      set
      {
        this["Enlist"] = value;
      }
    }

    /// <summary>
    /// If set to true, will throw an exception if the database specified in the connection
    /// string does not exist.  If false, the database will be created automatically.
    /// </summary>
    [Browsable(true)]
    [DefaultValue(false)]
    [DisplayName("Fail If Missing")]
    public bool FailIfMissing
    {
      get
      {
        object value;
        TryGetValue("FailIfMissing", out value);
        return Convert.ToBoolean(value, CultureInfo.CurrentCulture);
      }
      set
      {
        this["FailIfMissing"] = value;
      }
    }

    /// <summary>
    /// If enabled, uses the legacy 3.xx format for maximum compatibility, but results in larger
    /// database sizes.
    /// </summary>
    [DisplayName("Legacy Format")]
    [Browsable(true)]
    [DefaultValue(true)]
    public bool LegacyFormat
    {
      get
      {
        object value;
        TryGetValue("Legacy Format", out value);
        return Convert.ToBoolean(value, CultureInfo.CurrentCulture);
      }
      set
      {
        this["Legacy Format"] = value;
      }
    }

    /// <summary>
    /// Gets/sets the database encryption password
    /// </summary>
    [Browsable(true)]
    [PasswordPropertyText(true)]
    [DefaultValue("")]
    public string Password
    {
      get
      {
        object value;
        TryGetValue("Password", out value);
        return value.ToString();
      }
      set
      {
        this["Password"] = value;
      }
    }

    /// <summary>
    /// Gets/Sets the page size for the connection.
    /// </summary>
    [DisplayName("Page Size")]
    [Browsable(true)]
    [DefaultValue(1024)]
    public int PageSize
    {
      get
      {
        object value;
        TryGetValue("Page Size", out value);
        return Convert.ToInt32(value, CultureInfo.CurrentCulture);
      }
      set
      {
        this["Page Size"] = value;
      }
    }

    /// <summary>
    /// Gets/Sets the maximum number of pages the database may hold
    /// </summary>
    [DisplayName("Max Page Count")]
    [Browsable(true)]
    [DefaultValue(0)]
    public int MaxPageCount
    {
      get
      {
        object value;
        TryGetValue("Max Page Count", out value);
        return Convert.ToInt32(value, CultureInfo.CurrentCulture);
      }
      set
      {
        this["Max Page Count"] = value;
      }
    }

    /// <summary>
    /// Gets/Sets the cache size for the connection.
    /// </summary>
    [DisplayName("Cache Size")]
    [Browsable(true)]
    [DefaultValue(2000)]
    public int CacheSize
    {
      get
      {
        object value;
        TryGetValue("Cache Size", out value);
        return Convert.ToInt32(value, CultureInfo.CurrentCulture);
      }
      set
      {
        this["Cache Size"] = value;
      }
    }

    /// <summary>
    /// Gets/Sets the datetime format for the connection.
    /// </summary>
    [Browsable(true)]
    [DefaultValue(SQLiteDateFormats.ISO8601)]
    public SQLiteDateFormats DateTimeFormat
    {
      get
      {
        object value;
        TryGetValue("DateTimeFormat", out value);
        if (value is string)
          return (SQLiteDateFormats)TypeDescriptor.GetConverter(typeof(SQLiteDateFormats)).ConvertFrom(value);
        else return (SQLiteDateFormats)value;
      }
      set
      {
        this["DateTimeFormat"] = value;
      }
    }

    /// <summary>
    /// Helper function for retrieving values from the connectionstring
    /// </summary>
    /// <param name="keyword">The keyword to retrieve settings for</param>
    /// <param name="value">The resulting parameter value</param>
    /// <returns>Returns true if the value was found and returned</returns>
    public override bool TryGetValue(string keyword, out object value)
    {
      bool b = base.TryGetValue(keyword, out value);

      if (!_properties.ContainsKey(keyword)) return b;

      PropertyDescriptor pd = _properties[keyword] as PropertyDescriptor;

      if (pd == null) return b;

      if (b)
      {
        value = TypeDescriptor.GetConverter(pd.PropertyType).ConvertFrom(value);
      }
      else
      {
        DefaultValueAttribute att = pd.Attributes[typeof(DefaultValueAttribute)] as DefaultValueAttribute;
        if (att != null)
        {
          value = att.Value;
          b = true;
        }
      }
      return b;
    }
  }
#endif
}
