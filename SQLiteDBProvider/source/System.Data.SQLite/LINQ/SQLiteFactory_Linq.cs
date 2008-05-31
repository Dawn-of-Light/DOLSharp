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
  using System.Reflection;
  using System.Security.Permissions;

  /// <summary>
  /// SQLite implementation of DbProviderFactory.
  /// </summary>
  public sealed partial class SQLiteFactory : IServiceProvider
  {
    private static Type _dbProviderServicesType;

    static SQLiteFactory()
    {
      _dbProviderServicesType = Type.GetType("System.Data.Common.DbProviderServices, System.Data.Entity, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", false);
    }

    /// <summary>
    /// Will provide a DbProviderServices object in .NET 3.5
    /// </summary>
    /// <param name="serviceType">The class or interface type to query for</param>
    /// <returns></returns>
    [ReflectionPermission(SecurityAction.Assert, MemberAccess = true)]
    object IServiceProvider.GetService(Type serviceType)
    {
      if (_dbProviderServicesType != null)
      {
        if (serviceType == _dbProviderServicesType)
        {
          Type type = Type.GetType("System.Data.SQLite.SQLiteProviderServices, System.Data.SQLite.Linq, Version=2.0.33.0, Culture=neutral, PublicKeyToken=db937bc2d44ff139", false);
          if (type != null)
          {
            FieldInfo field = type.GetField("Instance", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            return field.GetValue(null);
          }
        }
      }
      return null;
    }
  }
}
