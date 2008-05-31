/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace System.Data.SQLite
{
  using System;
  using System.Collections.Generic;

  internal static class SQLiteConnectionPool
  {
    private static SortedList<string, Queue<WeakReference>> _connections = new SortedList<string, Queue<WeakReference>>(StringComparer.OrdinalIgnoreCase);
    
    internal static SQLiteConnectionHandle Remove(string fileName)
    {
      lock (_connections)
      {
        Queue<WeakReference> queue;

        if (_connections.TryGetValue(fileName, out queue) == false) return null;

        while (queue.Count > 0)
        {
          WeakReference cnn = queue.Dequeue();
          SQLiteConnectionHandle hdl = cnn.Target as SQLiteConnectionHandle;
          if (hdl != null)
            return hdl;
        }
        return null;
      }
    }

    internal static void Add(string fileName, SQLiteConnectionHandle hdl)
    {
      lock (_connections)
      {
        Queue<WeakReference> queue;
        if (_connections.TryGetValue(fileName, out queue) == false)
        {
          queue = new Queue<WeakReference>();
          _connections.Add(fileName, queue);
        }
        queue.Enqueue(new WeakReference(hdl, false));
        GC.KeepAlive(hdl);
      }
    }
  }
}
