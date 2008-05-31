/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace install
{
  using System;
  using System.ComponentModel;
  using System.Data;
  using System.Drawing;
  using System.Text;
  using System.Windows.Forms;
  using Microsoft.Win32;
  using System.IO;
  using System.GACManagedAccess;
  using System.Xml;
  using System.Diagnostics;
  using System.Collections.Generic;

  public partial class InstallDesigner : Form
  {
    private static Guid standardDataProviderGuid = new Guid("{0EBAAB6E-CA80-4b4a-8DDF-CBE6BF058C70}");
    private static Guid standardDataSourcesGuid = new Guid("{0EBAAB6E-CA80-4b4a-8DDF-CBE6BF058C71}");
    private static Guid standardCFDataSourcesGuid = new Guid("{0EBAAB6E-CA80-4b4a-8DDF-CBE6BF058C72}");
    private static Guid oledbDataProviderGuid = new Guid("{7F041D59-D76A-44ed-9AA2-FBF6B0548B80}");
    private static Guid oledbAltDataProviderGuid = new Guid("{7F041D59-D76A-44ed-9AA2-FBF6B0548B81}");
    private static Guid jetDataSourcesGuid = new Guid("{466CE797-67A4-4495-B75C-A3FD282E7FC3}");
    private static Guid jetAltDataSourcesGuid = new Guid("{466CE797-67A4-4495-B75C-A3FD282E7FC4}");
    private static string[] compactFrameworks = new string[] { "PocketPC", "SmartPhone", "WindowsCE" };

    internal bool _remove = false;
    //private string _regRoot = "8.0";
    private System.Reflection.Assembly _assm = null;
    private bool _ignoreChecks = true;
    private string _assmLocation;

    private Dictionary<string, string> _regRoots = new Dictionary<string,string>();

    string SQLiteLocation
    {
      get
      {
        System.Reflection.Assembly assm = SQLite;
        return _assmLocation;
      }
    }

    System.Reflection.Assembly SQLite
    {
      get
      {
        if (_assm == null)
        {
          Environment.CurrentDirectory = Path.GetDirectoryName(typeof(InstallDesigner).Assembly.Location);

          try
          {
            _assmLocation = Path.GetFullPath("..\\System.Data.SQLite.DLL");
            _assm = System.Reflection.Assembly.LoadFrom(_assmLocation);
          }
          catch
          {
          }
        }

        OpenFileDialog dlg = new OpenFileDialog();
        while (_assm == null)
        {
          dlg.Multiselect = false;
          dlg.InitialDirectory = Environment.CurrentDirectory;
          dlg.FileName = "System.Data.SQLite.DLL";
          dlg.Filter = "System.Data.SQLite.DLL|System.Data.SQLite.DLL";
          if (dlg.ShowDialog() == DialogResult.OK)
          {
            try
            {
              _assmLocation = dlg.FileName;
              _assm = System.Reflection.Assembly.LoadFrom(dlg.FileName);
            }
            catch
            {
            }
          }
          else
            throw new ArgumentException("Unable to find or load System.Data.SQLite.DLL");
        }
        return _assm;
      }

      set
      {
        _assm = value;
      }
    }

    public InstallDesigner()
    {
      string[] args = Environment.GetCommandLineArgs();

      _regRoots.Add("8.0", "2005");
      _regRoots.Add("9.0", "2008");

      for (int n = 0; n < args.Length; n++)
      {
        if (String.Compare(args[n], "/regroot", true) == 0 ||
          String.Compare(args[n], "-regroot", true) == 0)
        {
          _regRoots.Add(args[n + 1], args[n + 1]);
          break;
        }
        else if (String.Compare(args[n], "/remove", true) == 0 ||
          String.Compare(args[n], "-remove", true) == 0)
        {
          _remove = true;
        }
      }

      InitializeComponent();

      RegistryKey key;

      foreach (KeyValuePair<string, string> pair in _regRoots)
      {
        using (key = Registry.LocalMachine.OpenSubKey("Software\\Microsoft"))
        {
          AddItem(key, pair.Key, "VisualStudio", String.Format("Visual Studio {0} (full editions)", pair.Value), standardDataProviderGuid, null);
          AddItem(key, pair.Key, "VWDExpress", String.Format("Visual Web Developer Express {0} Edition", pair.Value), standardDataProviderGuid, null);

          warningPanel.Visible = (AddItem(key, pair.Key, "VCSExpress", String.Format("Visual C# Express {0} Edition *", pair.Value), oledbDataProviderGuid, oledbAltDataProviderGuid)
           | AddItem(key, pair.Key, "VCExpress", String.Format("Visual C++ Express {0} Edition *", pair.Value), oledbDataProviderGuid, oledbAltDataProviderGuid)
           | AddItem(key, pair.Key, "VBExpress", String.Format("Visual Basic Express {0} Edition *", pair.Value), oledbDataProviderGuid, oledbAltDataProviderGuid)
           | AddItem(key, pair.Key, "VJSExpress", String.Format("Visual J# Express {0} Edition *", pair.Value), oledbDataProviderGuid, oledbAltDataProviderGuid));
        }
        GlobalAddRemove(pair.Key);
      }

      _ignoreChecks = false;
    }

    private bool AddItem(RegistryKey parent, string version, string subkeyname, string itemName, Guid lookFor, object isChecked)
    {
      RegistryKey subkey;

      try
      {
        using (subkey = parent.OpenSubKey(String.Format("{0}\\{1}", subkeyname, version)))
        {
          ListViewItem item = new ListViewItem(itemName);

          item.Tag = new string[] { subkeyname, version };

          using (RegistryKey subsubkey = subkey.OpenSubKey("DataProviders"))
          {
            if (subsubkey == null)
              throw new ArgumentException("Edition not installed");
          }

          using (RegistryKey subsubkey = subkey.OpenSubKey(String.Format("DataProviders\\{0}", (isChecked == null) ? lookFor.ToString("B") : ((Guid)isChecked).ToString("B"))))
          {
            if (subsubkey == null)
            {
              DoInstallUninstall(item);
            }
            else
            {
              bool itemChecked = (subsubkey.GetValue(null) != null);
              DoInstallUninstall(item);
              if (_remove == false) item.Checked = itemChecked;
            }
          }

          installList.Items.Add(item);
          if (item.Checked)
          {
            DoInstallUninstall(item);
          }
          return true;
        }
      }
      catch
      {
        return false;
      }
    }

    private void closeButton_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void installList_ItemChecked(object sender, ItemCheckedEventArgs e)
    {
      if (_ignoreChecks) return;

      string[] arr = (string[])e.Item.Tag;

      DoInstallUninstall(e.Item);

      GlobalAddRemove(arr[1]);
    }

    private void DoInstallUninstall(ListViewItem Item)
    {
      string[] arr = (string[])Item.Tag;
      if (Item.Checked == false)
      {
        if (Item.Text.IndexOf('*') > -1)
          RestoreJet(arr[0], arr[1]);
        else
          Uninstall(arr[0], arr[1], standardDataProviderGuid, standardDataSourcesGuid);
      }
      else
      {
        if (Item.Text.IndexOf('*') > -1)
          ReplaceJet(arr[0], arr[1]);
        else
          Install(arr[0], arr[1], standardDataProviderGuid, standardDataSourcesGuid);
      }
    }

    private void GlobalAddRemove(string version)
    {
      bool install = false;
//      bool installed;

      //// Check to see if SQLite is installed in the GAC
      //try
      //{
      //  string file = AssemblyCache.QueryAssemblyInfo("System.Data.SQLite");
      //  installed = true;
      //}
      //catch
      //{
      //  installed = false;
      //}

      // Check to see if any checkboxes in the list are checked
      for (int n = 0; n < installList.Items.Count; n++)
      {
        if (installList.Items[n].Checked == true)
        {
          install = true;
          break;
        }
      }

      // If at least 1 item is checked, then install some global settings
      if (install)
      {
        string path = Path.GetDirectoryName(SQLiteLocation);

        using (RegistryKey key = Registry.LocalMachine.CreateSubKey("Software\\Microsoft\\.NETFramework\\v2.0.50727\\AssemblyFoldersEx\\SQLite", RegistryKeyPermissionCheck.ReadWriteSubTree))
        {
          key.SetValue(null, path);
        }

        while (String.IsNullOrEmpty(path) == false)
        {
          if (File.Exists(path + "\\CompactFramework\\System.Data.SQLite.DLL") == false)
          {
            path = Path.GetDirectoryName(path);
          }
          else break;
        }

        if (String.IsNullOrEmpty(path) == false)
        {
          path += "\\CompactFramework\\";

          for (int n = 0; n < compactFrameworks.Length; n++)
          {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(String.Format("Software\\Microsoft\\.NETCompactFramework\\v2.0.0.0\\{0}\\AssemblyFoldersEx", compactFrameworks[n]), true))
            {

              if (key != null)
              {
                using (RegistryKey subkey = key.CreateSubKey("SQLite", RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                  subkey.SetValue(null, path);
                }
              }
            }
          }
        }

        for (int n = 0; n < 2; n++)
        {
          // Add factory support to the machine.config file.
          try
          {
            string xmlFileName = Environment.ExpandEnvironmentVariables(String.Format("%WinDir%\\Microsoft.NET\\{0}\\v2.0.50727\\CONFIG\\machine.config", (n == 0) ? "Framework" : "Framework64"));
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.Load(xmlFileName);


            XmlNode xmlNode = xmlDoc.SelectSingleNode("configuration/system.data/DbProviderFactories/add[@invariant=\"System.Data.SQLite\"]");
            if (xmlNode == null)
            {
              XmlNode xmlConfig = xmlDoc.SelectSingleNode("configuration");
              if (xmlConfig != null)
              {
                XmlNode xmlData = xmlConfig.SelectSingleNode("system.data");
                if (xmlData == null)
                {
                  xmlData = xmlDoc.CreateNode(XmlNodeType.Element, "system.data", "");
                  xmlConfig.AppendChild(xmlData);
                }
                XmlNode xmlParent = xmlData.SelectSingleNode("DbProviderFactories");
                if (xmlParent == null)
                {
                  xmlParent = xmlDoc.CreateNode(XmlNodeType.Element, "DbProviderFactories", "");
                  xmlData.AppendChild(xmlParent);
                }

                //xmlNode = xmlDoc.CreateNode(XmlNodeType.Element, "remove", "");
                //xmlNode.Attributes.SetNamedItem(xmlDoc.CreateAttribute("invariant"));
                //xmlParent.AppendChild(xmlNode);
                //xmlNode.Attributes.GetNamedItem("invariant").Value = "System.Data.SQLite";

                xmlNode = xmlDoc.CreateNode(XmlNodeType.Element, "add", "");
                xmlNode.Attributes.SetNamedItem(xmlDoc.CreateAttribute("name"));
                xmlNode.Attributes.SetNamedItem(xmlDoc.CreateAttribute("invariant"));
                xmlNode.Attributes.SetNamedItem(xmlDoc.CreateAttribute("description"));
                xmlNode.Attributes.SetNamedItem(xmlDoc.CreateAttribute("type"));
                xmlParent.AppendChild(xmlNode);
              }
            }
            xmlNode.Attributes.GetNamedItem("name").Value = "SQLite Data Provider";
            xmlNode.Attributes.GetNamedItem("invariant").Value = "System.Data.SQLite";
            xmlNode.Attributes.GetNamedItem("description").Value = ".Net Framework Data Provider for SQLite";
            xmlNode.Attributes.GetNamedItem("type").Value = "System.Data.SQLite.SQLiteFactory, " + SQLite.GetName().FullName;

            xmlDoc.Save(xmlFileName);
          }
          catch
          {
          }
        }
      }
      else // No checkboxes are checked, remove some global settings
      {
        try
        {
          Registry.LocalMachine.DeleteSubKey("Software\\Microsoft\\.NETFramework\\v2.0.50727\\AssemblyFoldersEx\\SQLite");

          for (int n = 0; n < compactFrameworks.Length; n++)
          {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(String.Format("Software\\Microsoft\\.NETCompactFramework\\v2.0.0.0\\{0}\\DataProviders", compactFrameworks[n]), true))
            {
              try
              {
                if (key != null) key.DeleteSubKey(standardDataProviderGuid.ToString("B"));
              }
              catch
              {
              }
            }
          }

          for (int n = 0; n < compactFrameworks.Length; n++)
          {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(String.Format("Software\\Microsoft\\.NETCompactFramework\\v2.0.0.0\\{0}\\AssemblyFoldersEx", compactFrameworks[n]), true))
            {
              try
              {
                if (key != null) key.DeleteSubKey("SQLite");
              }
              catch
              {
              }
            }
          }

          for (int n = 0; n < 2; n++)
          {
            try
            {
              // Remove any entries in the machine.config if they're still there
              string xmlFileName = Environment.ExpandEnvironmentVariables(String.Format("%WinDir%\\Microsoft.NET\\{0}\\v2.0.50727\\CONFIG\\machine.config", (n == 0) ? "Framework" : "Framework64"));
              XmlDocument xmlDoc = new XmlDocument();
              xmlDoc.PreserveWhitespace = true;
              xmlDoc.Load(xmlFileName);

              XmlNode xmlNode = xmlDoc.SelectSingleNode("configuration/system.data/DbProviderFactories/add[@invariant=\"System.Data.SQLite\"]");

              if (xmlNode != null)
                xmlNode.ParentNode.RemoveChild(xmlNode);

              xmlNode = xmlDoc.SelectSingleNode("configuration/system.data/DbProviderFactories/remove[@invariant=\"System.Data.SQLite\"]");
              if (xmlNode != null)
                xmlNode.ParentNode.RemoveChild(xmlNode);

              xmlDoc.Save(xmlFileName);
            }
            catch
            {
            }
          }
        }
        catch
        {
        }
      }

      try
      {
        if (!install) // Remove SQLite from the GAC if its there
        {
          AssemblyCacheUninstallDisposition disp;

          string s;
          AssemblyCacheEnum entries = new AssemblyCacheEnum("System.Data.SQLite");
          while (true)
          {
            s = entries.GetNextAssembly();
            if (String.IsNullOrEmpty(s)) break;

            AssemblyCache.UninstallAssembly(s, null, out disp);
          }

          entries = new AssemblyCacheEnum("SQLite.Designer");
          while (true)
          {
            s = entries.GetNextAssembly();
            if (String.IsNullOrEmpty(s)) break;

            AssemblyCache.UninstallAssembly(s, null, out disp);
          } SQLite = null;
        }
        else // Install SQLite into the GAC
        {
          byte[] cfdt = Properties.Resources.System_Data_SQLite;
          string tempPath = Path.GetTempPath();
          tempPath = Path.Combine(tempPath, "System.Data.SQLite.DLL");
          using (FileStream fs = File.Open(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
          {
            fs.Write(cfdt, 0, cfdt.Length);
          }

          try
          {
            AssemblyCache.InstallAssembly(tempPath, null, AssemblyCommitFlags.Default);
            AssemblyCache.InstallAssembly(Path.Combine(Path.GetDirectoryName(SQLiteLocation), "x64\\System.Data.SQLite.DLL"), null, AssemblyCommitFlags.Default);
            AssemblyCache.InstallAssembly(Path.Combine(Path.GetDirectoryName(SQLiteLocation), "itanium\\System.Data.SQLite.DLL"), null, AssemblyCommitFlags.Default);
          }
          catch
          {
          }
          finally
          {
            File.Delete(tempPath);
            AssemblyCache.InstallAssembly(Path.GetFullPath("SQLite.Designer.DLL"), null, AssemblyCommitFlags.Default);
            AssemblyCache.InstallAssembly(SQLiteLocation, null, AssemblyCommitFlags.Default);
          }
        }
      }
      catch
      {
        throw;
      }

      FixXmlLibPaths(install, version);
    }

    private void ReplaceJet(string keyname, string version)
    {
      using (RegistryKey key = Registry.LocalMachine.OpenSubKey(String.Format("Software\\Microsoft\\{0}\\{1}\\DataProviders", keyname, version), true))
      {
        using (RegistryKey source = key.OpenSubKey(oledbDataProviderGuid.ToString("B")))
        {
          using (RegistryKey dest = key.CreateSubKey(oledbAltDataProviderGuid.ToString("B")))
          {
            if (source == null) return;
            CopyKey(source, dest);
          }
        }
        key.DeleteSubKeyTree(oledbDataProviderGuid.ToString("B"));
      }

      using (RegistryKey key = Registry.LocalMachine.OpenSubKey(String.Format("Software\\Microsoft\\{0}\\{1}\\DataSources", keyname, version), true))
      {
        using (RegistryKey source = key.OpenSubKey(jetDataSourcesGuid.ToString("B")))
        {
          using (RegistryKey dest = key.CreateSubKey(jetAltDataSourcesGuid.ToString("B")))
          {
            if (source == null) return;
            CopyKey(source, dest);
          }
        }
        key.DeleteSubKeyTree(jetDataSourcesGuid.ToString("B"));
      }

      Install(keyname, version, oledbDataProviderGuid, jetDataSourcesGuid);
    }

    private void RestoreJet(string keyname, string version)
    {
      using (RegistryKey key = Registry.LocalMachine.OpenSubKey(String.Format("Software\\Microsoft\\{0}\\{1}\\DataProviders", keyname, version), true))
      {
        using (RegistryKey source = key.OpenSubKey(oledbAltDataProviderGuid.ToString("B")))
        {
          if (source == null) return;
        }
      }

      Uninstall(keyname, version, oledbDataProviderGuid, jetDataSourcesGuid);

      using (RegistryKey key = Registry.LocalMachine.OpenSubKey(String.Format("Software\\Microsoft\\{0}\\{1}\\DataProviders", keyname, version), true))
      {
        using (RegistryKey source = key.OpenSubKey(oledbAltDataProviderGuid.ToString("B")))
        {
          if (source != null)
          {
            using (RegistryKey dest = key.CreateSubKey(oledbDataProviderGuid.ToString("B")))
            {
              CopyKey(source, dest);
            }
            key.DeleteSubKeyTree(oledbAltDataProviderGuid.ToString("B"));
          }
        }
      }

      using (RegistryKey key = Registry.LocalMachine.OpenSubKey(String.Format("Software\\Microsoft\\{0}\\{1}\\DataSources", keyname, version), true))
      {
        using (RegistryKey source = key.OpenSubKey(jetAltDataSourcesGuid.ToString("B")))
        {
          if (source != null)
          {
            using (RegistryKey dest = key.CreateSubKey(jetDataSourcesGuid.ToString("B")))
            {
              CopyKey(source, dest);
            }
            key.DeleteSubKeyTree(jetAltDataSourcesGuid.ToString("B"));
          }
        }
      }
    }

    private void Install(string keyname, string version, Guid provider, Guid source)
    {
      bool usePackage = (keyname == "VisualStudio");

      using (RegistryKey key = Registry.LocalMachine.OpenSubKey(String.Format("Software\\Microsoft\\{0}\\{1}\\DataProviders", keyname, version), true))
      {
        using (RegistryKey subkey = key.CreateSubKey(provider.ToString("B"), RegistryKeyPermissionCheck.ReadWriteSubTree))
        {
          subkey.SetValue(null, ".NET Framework Data Provider for SQLite");
          subkey.SetValue("InvariantName", "System.Data.SQLite");
          subkey.SetValue("Technology", "{77AB9A9D-78B9-4ba7-91AC-873F5338F1D2}");
          subkey.SetValue("CodeBase", Path.GetFullPath("SQLite.Designer.DLL"));
          

          if (usePackage)
           subkey.SetValue("FactoryService", "{DCBE6C8D-0E57-4099-A183-98FF74C64D9D}");

          using (RegistryKey subsubkey = subkey.CreateSubKey("SupportedObjects", RegistryKeyPermissionCheck.ReadWriteSubTree))
          {
            using (RegistryKey subsubsubkey = subsubkey.CreateSubKey("DataConnectionUIControl", RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
              if (!usePackage)
                subsubsubkey.SetValue(null, "SQLite.Designer.SQLiteConnectionUIControl");
            }
            using (RegistryKey subsubsubkey = subsubkey.CreateSubKey("DataConnectionProperties", RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
              if (!usePackage)
                subsubsubkey.SetValue(null, "SQLite.Designer.SQLiteConnectionProperties");
            }

            subsubkey.CreateSubKey("DataObjectSupport").Close();
            subsubkey.CreateSubKey("DataViewSupport").Close();
            using (RegistryKey subsubsubkey = subsubkey.CreateSubKey("DataConnectionSupport", RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
              if (!usePackage)
                subsubsubkey.SetValue(null, "SQLite.Designer.SQLiteDataConnectionSupport");
            }
          }
        }
      }

      using (RegistryKey key = Registry.LocalMachine.OpenSubKey(String.Format("Software\\Microsoft\\{0}\\{1}\\DataSources", keyname, version), true))
      {
        using (RegistryKey subkey = key.CreateSubKey(source.ToString("B"), RegistryKeyPermissionCheck.ReadWriteSubTree))
        {
          subkey.SetValue(null, "SQLite Database File");
          using (RegistryKey subsubkey = subkey.CreateSubKey("SupportingProviders", RegistryKeyPermissionCheck.ReadWriteSubTree))
          {
            subsubkey.CreateSubKey(provider.ToString("B")).Close();
          }
        }
      }

      //try
      //{
      //  using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Environment", true))
      //  {
      //    string libpath = (string)key.GetValue("LIB");
      //    string path = Path.GetDirectoryName(SQLiteLocation);

      //    if (libpath.IndexOf(path, StringComparison.InvariantCultureIgnoreCase) == -1)
      //    {
      //      libpath += (";" + path);
      //      key.SetValue("LIB", libpath);
      //    }
      //  }
      //}
      //catch
      //{
      //}

      for (int n = 0; n < compactFrameworks.Length; n++)
      {
        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(String.Format("Software\\Microsoft\\.NETCompactFramework\\v2.0.0.0\\{0}\\DataProviders", compactFrameworks[n]), true))
        {
          if (key != null)
          {
            using (RegistryKey subkey = key.CreateSubKey(standardDataProviderGuid.ToString("B"), RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
              subkey.SetValue(null, ".NET Framework Data Provider for SQLite");
              subkey.SetValue("InvariantName", "System.Data.SQLite");
              subkey.SetValue("RuntimeAssembly", "System.Data.SQLite.DLL");
            }
          }
        }
      }

      if (usePackage)
      {
        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(String.Format("Software\\Microsoft\\{0}\\{1}\\Packages", keyname, version), true))
        {
          using (RegistryKey subkey = key.CreateSubKey("{DCBE6C8D-0E57-4099-A183-98FF74C64D9C}", RegistryKeyPermissionCheck.ReadWriteSubTree))
          {
            subkey.SetValue(null, "SQLite Designer Package");
            subkey.SetValue("Class", "SQLite.Designer.SQLitePackage");
            subkey.SetValue("CodeBase", Path.GetFullPath("SQLite.Designer.DLL"));
            subkey.SetValue("ID", 400);
            subkey.SetValue("InprocServer32", "mscoree.dll");
            subkey.SetValue("CompanyName", "Black Castle Software, LLC");
            subkey.SetValue("MinEdition", "standard");
            subkey.SetValue("ProductName", "SQLite Data Provider");
            subkey.SetValue("ProductVersion", "1.0");
          }
        }

        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(String.Format("Software\\Microsoft\\{0}\\{1}\\Menus", keyname, version), true))
        {
          key.SetValue("{DCBE6C8D-0E57-4099-A183-98FF74C64D9C}", ", 1000, 1");
        }

        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(String.Format("Software\\Microsoft\\{0}\\{1}\\Services", keyname, version), true))
        {
          using (RegistryKey subkey = key.CreateSubKey("{DCBE6C8D-0E57-4099-A183-98FF74C64D9D}", RegistryKeyPermissionCheck.ReadWriteSubTree))
          {
            subkey.SetValue(null, "{DCBE6C8D-0E57-4099-A183-98FF74C64D9C}");
            subkey.SetValue("Name", "SQLite Provider Object Factory");
          }
        }
      }
    }

    private XmlDocument GetConfig(string keyname, string version, out string xmlFileName)
    {
      try
      {
        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(String.Format("Software\\Microsoft\\{0}\\{1}", keyname, version), true))
        {
          xmlFileName = (string)key.GetValue("InstallDir");
          if (String.Compare(keyname, "VisualStudio", true) == 0)
            xmlFileName += "devenv.exe.config";
          else
            xmlFileName += keyname + ".exe.config";
        }

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.PreserveWhitespace = true;
        xmlDoc.Load(xmlFileName);

        return xmlDoc;
      }
      catch
      {
        xmlFileName = null;
      }
      return null;
    }

    private void Uninstall(string keyname, string version, Guid provider, Guid source)
    {
      try
      {
        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(String.Format("Software\\Microsoft\\{0}\\{1}\\DataProviders", keyname, version), true))
        {
          if (key != null) key.DeleteSubKeyTree(provider.ToString("B"));
        }
        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(String.Format("Software\\Microsoft\\{0}\\{1}\\DataSources", keyname, version), true))
        {
          if (key != null) key.DeleteSubKeyTree(source.ToString("B"));
        }

        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(String.Format("Software\\Microsoft\\{0}\\{1}\\Packages", keyname, version), true))
        {
          if (key != null) key.DeleteSubKeyTree("{DCBE6C8D-0E57-4099-A183-98FF74C64D9C}");
        }

        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(String.Format("Software\\Microsoft\\{0}\\{1}\\Services", keyname, version), true))
        {
          if (key != null) key.DeleteSubKeyTree("{DCBE6C8D-0E57-4099-A183-98FF74C64D9D}");
        }

        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(String.Format("Software\\Microsoft\\{0}\\{1}\\Menus", keyname, version), true))
        {
          key.DeleteValue("{DCBE6C8D-0E57-4099-A183-98FF74C64D9C}");
        }

        //using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Environment", true))
        //{
        //  string libpath = (string)key.GetValue("LIB");
        //  string path = ";" + Path.GetDirectoryName(SQLiteLocation);

        //  libpath = libpath.Replace(path, "");
        //  key.SetValue("LIB", libpath);
        //}
      }
      catch
      {
      }

      // Remove factory support from the development environment config file
      string xmlFileName;
      XmlDocument xmlDoc = GetConfig(keyname, version, out xmlFileName);

      if (xmlDoc == null) return;

      XmlNode xmlNode = xmlDoc.SelectSingleNode("configuration/system.data/DbProviderFactories/add[@invariant=\"System.Data.SQLite\"]");
      if (xmlNode != null)
        xmlNode.ParentNode.RemoveChild(xmlNode);

      xmlNode = xmlDoc.SelectSingleNode("configuration/system.data/DbProviderFactories/remove[@invariant=\"System.Data.SQLite\"]");
      if (xmlNode != null)
        xmlNode.ParentNode.RemoveChild(xmlNode);

      xmlDoc.Save(xmlFileName);
    }


    private static void CopyKey(RegistryKey keySource, RegistryKey keyDest)
    {
      if (keySource.SubKeyCount > 0)
      {
        string[] subkeys = keySource.GetSubKeyNames();
        for (int n = 0; n < subkeys.Length; n++)
        {
          using (RegistryKey subkeysource = keySource.OpenSubKey(subkeys[n]))
          {
            using (RegistryKey subkeydest = keyDest.CreateSubKey(subkeys[n], RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
              CopyKey(subkeysource, subkeydest);
            }
          }
        }
      }
      string[] values = keySource.GetValueNames();
      for (int n = 0; n < values.Length; n++)
      {
        keyDest.SetValue(values[n], keySource.GetValue(values[n]), keySource.GetValueKind(values[n]));
      }
    }

    private void FixXmlLibPaths(bool install, string version)
    {
      string installDir = null;
      RegistryKey key = null;

      try
      {
        key = Registry.LocalMachine.OpenSubKey(String.Format("Software\\Microsoft\\VisualStudio\\{0}", version));
        if (key != null)
        {
          try
          {
            installDir = (string)key.GetValue("InstallDir");
          }
          catch
          {
          }
          finally
          {
            if (String.IsNullOrEmpty(installDir))
            {
              ((IDisposable)key).Dispose();
              key = null;
            }
          }
        }

        if (key == null)
        {
          key = Registry.LocalMachine.OpenSubKey(String.Format("Software\\Microsoft\\VCExpress\\{0}", version));
          if (key == null) return;
        }

        try
        {
          installDir = (string)key.GetValue("InstallDir");
        }
        catch
        {
        }
      }
      finally
      {
        if (key != null) ((IDisposable)key).Dispose();
      }

      if (String.IsNullOrEmpty(installDir)) return;

      installDir = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(installDir))), "VC");

      string currentDir;
      string[] lookIn = new string[] { "vcpackages", "bin\\amd64", "bin\\ia64" };
      string sqlitePath = Path.GetDirectoryName(SQLiteLocation);

      foreach (string subfolder in lookIn)
      {
        try
        {
          currentDir = Path.Combine(installDir, subfolder);
          FixXmlLibPaths(currentDir, "VCProjectEngine.DLL*.config", sqlitePath, install);
          FixXmlLibPaths(currentDir, "AMD64.VCPlatform.config", Path.Combine(sqlitePath, "x64"), install);
          FixXmlLibPaths(currentDir, "Itanium.VCPlatform.config", Path.Combine(sqlitePath, "itanium"), install);
          FixXmlLibPaths(currentDir, "WCE.VCPlatform.config", Path.Combine(sqlitePath, "CompactFramework"), install);
        }
        catch
        {
        }
      }

      FixLocalUserPaths(install);
    }

    private void FixLocalUserPaths(bool install)
    {
      string file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft\\VisualStudio\\8.0\\VCComponents.dat");
      StringBuilder output = new StringBuilder();
      string line;
      string sqlitePath = Path.GetDirectoryName(SQLiteLocation);
      string currPath = sqlitePath;

      try
      {
        using (StreamReader rd = new StreamReader(file))
        {
          while (rd.EndOfStream == false)
          {
            line = rd.ReadLine();
            line = line.Trim();
            if (String.IsNullOrEmpty(line)) continue;
            if (line[0] == '[')
            {
              if (line.IndexOf("Win32", StringComparison.InvariantCultureIgnoreCase) != -1)
                currPath = sqlitePath;
              else if (line.IndexOf("x64", StringComparison.InvariantCultureIgnoreCase) != -1)
                currPath = Path.Combine(sqlitePath, "x64");
              else if (line.IndexOf("Itanium", StringComparison.InvariantCultureIgnoreCase) != -1)
                currPath = Path.Combine(sqlitePath, "x64");
              else if (line.IndexOf("ARM", StringComparison.InvariantCultureIgnoreCase) != -1)
                currPath = Path.Combine(sqlitePath, "CompactFramework");
            }
            else if (line.StartsWith("Reference Dirs", StringComparison.InvariantCultureIgnoreCase) == true)
            {
              int n = line.IndexOf(";" + currPath, StringComparison.InvariantCultureIgnoreCase);
              if (n > -1) line = line.Remove(n, currPath.Length + 1);

              if (install)
              {
                if (line[line.Length - 1] == '=')
                  line += currPath;
                else
                  line += (";" + currPath);
              }
            }

            output.AppendLine(line);
          }
          rd.Close();
        }

        File.Delete(file);
        using (StreamWriter writer = new StreamWriter(file, false, Encoding.Unicode))
        {          
          writer.Write(output.ToString());
          writer.Close();
        }
      }
      catch
      {
      }
    }

    private void FixXmlLibPaths(string path, string lookFor, string sqlitePath, bool install)
    {
      // Win32
      string[] files = Directory.GetFiles(path, lookFor);
      if (files.Length > 0)
      {
        foreach (string file in files)
        {
          FixXmlLibPath(file, sqlitePath, install);
        }
      }
    }

    private void FixXmlLibPath(string fileName, string sqlitePath, bool install)
    {
      XmlDocument xmlDoc = new XmlDocument();
      xmlDoc.PreserveWhitespace = true;
      xmlDoc.Load(fileName);
      
      XmlNodeList xmlNodes = xmlDoc.SelectNodes("VCPlatformConfigurationFile/Platform/Directories");
      if (xmlNodes == null) return;

      foreach(XmlNode xmlNode in xmlNodes)
      {
        string libpath = xmlNode.Attributes.GetNamedItem("Reference").Value;
        if (String.Compare(libpath, sqlitePath, true) == 0)
          libpath = "";
        else
        {
          int n = libpath.IndexOf(";" + sqlitePath, StringComparison.InvariantCultureIgnoreCase);
          if (n > -1) libpath = libpath.Remove(n, sqlitePath.Length + 1);
        }

        if (install)
        {
          if (String.IsNullOrEmpty(libpath)) libpath = sqlitePath;
          else libpath += (";" + sqlitePath);
        }
        xmlNode.Attributes.GetNamedItem("Reference").Value = libpath;
      }

      xmlDoc.Save(fileName);
    }
  }
}