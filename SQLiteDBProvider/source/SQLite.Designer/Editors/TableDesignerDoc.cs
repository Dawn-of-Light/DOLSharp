namespace SQLite.Designer.Editors
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Data;
  using System.Data.Common;
  using System.Drawing;
  using System.Text;
  using System.Windows.Forms;
  using Microsoft.VisualStudio.Shell.Interop;
  using Microsoft.VisualStudio.OLE.Interop;
  using Microsoft.VisualStudio;
  using Microsoft.VisualStudio.Data;
  using SQLite.Designer.Design;

  public partial class TableDesignerDoc : UserControl,
    IVsPersistDocData,
    IVsWindowPane,
    IOleCommandTarget,
    ISelectionContainer,
    IVsWindowPaneCommit,
    IVsWindowFrameNotify
  {
    private static Dictionary<int, string> _editingTables = new Dictionary<int, string>();
    
    internal DataConnection _connection;
    internal Microsoft.VisualStudio.Data.ServiceProvider _serviceProvider;
    internal Table _table;

    public TableDesignerDoc(DataConnection cnn, string tableName)
    {
      _connection = cnn;

      InitializeComponent();

      StringBuilder tables = new StringBuilder();

      using (DataReader reader = cnn.Command.Execute("SELECT * FROM sqlite_master", 1, null, 30))
      {
        while (reader.Read())
        {
          tables.Append(reader.GetItem(2).ToString());
          tables.Append(",");
        }
      }

      int n = 1;

      if (String.IsNullOrEmpty(tableName))
      {
        string alltables = tables.ToString();

        do
        {
          tableName = String.Format("Table{0}", n);
          n++;
        } while (alltables.IndexOf(tableName + ",", StringComparison.OrdinalIgnoreCase) > -1 || _editingTables.ContainsValue(tableName));

        _editingTables.Add(GetHashCode(), tableName);
      }
      _table = new Table(tableName, _connection.ConnectionSupport.ProviderObject as DbConnection, this);
    }

    void SetPropertyWindow()
    {
      IVsTrackSelectionEx track = _serviceProvider.GetService(typeof(SVsTrackSelectionEx)) as IVsTrackSelectionEx;
      if (track != null)
      {
        track.OnSelectChange(this);
      }
    }

    public new string Name
    {
      get
      {
        if (_table != null)
          return _table.Name;
        else return base.Name;
      }
      set
      {
        string caption = "SQLite:" + value;
        base.Name = value;

        if (_serviceProvider != null)
        {
          IVsWindowFrame frame = _serviceProvider.GetService(typeof(IVsWindowFrame)) as IVsWindowFrame;
          if (frame != null)
          {
            frame.SetProperty((int)__VSFPROPID.VSFPROPID_EditorCaption, value);
          }
        }
      }
    }

    //public void NotifyChanges()
    //{
    //  if (_serviceProvider == null) return;

    //  // Get a reference to the Running Document Table
    //  IVsRunningDocumentTable runningDocTable = (IVsRunningDocumentTable)_serviceProvider.GetService(typeof(SVsRunningDocumentTable));

    //  // Lock the document
    //  uint docCookie;
    //  IVsHierarchy hierarchy;
    //  uint itemID;
    //  IntPtr docData;
    //  int hr = runningDocTable.FindAndLockDocument(
    //      (uint)_VSRDTFLAGS.RDT_ReadLock,
    //      base.Name,
    //      out hierarchy,
    //      out itemID,
    //      out docData,
    //      out docCookie
    //  );
    //  ErrorHandler.ThrowOnFailure(hr);

    //  // Send the notification
    //  hr = runningDocTable.NotifyDocumentChanged(docCookie, (uint)__VSRDTATTRIB.RDTA_DocDataReloaded);

    //  // Unlock the document.
    //  // Note that we have to unlock the document even if the previous call failed.
    //  runningDocTable.UnlockDocument((uint)_VSRDTFLAGS.RDT_ReadLock, docCookie);

    //  // Check ff the call to NotifyDocChanged failed.
    //  ErrorHandler.ThrowOnFailure(hr);
    //}

    #region IVsPersistDocData Members

    int IVsPersistDocData.Close()
    {
      return VSConstants.S_OK;
    }

    public int GetGuidEditorType(out Guid pClassID)
    {
      return ((IPersistFileFormat)this).GetClassID(out pClassID);
    }

    public int IsDocDataDirty(out int pfDirty)
    {
      pfDirty = 1;
      return VSConstants.S_OK;
    }

    public int IsDocDataReloadable(out int pfReloadable)
    {
      pfReloadable = 0;
      return VSConstants.S_OK;
    }

    public int LoadDocData(string pszMkDocument)
    {
      return ((IPersistFileFormat)this).Load(pszMkDocument, 0, 0);
    }

    public int OnRegisterDocData(uint docCookie, IVsHierarchy pHierNew, uint itemidNew)
    {
      return VSConstants.S_OK;
    }

    public int ReloadDocData(uint grfFlags)
    {
      return VSConstants.E_NOTIMPL;
    }

    public int RenameDocData(uint grfAttribs, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew)
    {
      return VSConstants.E_NOTIMPL;
    }

    public int SaveDocData(VSSAVEFLAGS dwSave, out string pbstrMkDocumentNew, out int pfSaveCanceled)
    {
      pbstrMkDocumentNew = _table.Name;
      pfSaveCanceled = 0;

      return VSConstants.S_OK;
    }

    public int SetUntitledDocPath(string pszDocDataPath)
    {
      return ((IPersistFileFormat)this).InitNew(0);
    }

    #endregion

    #region IVsWindowPane Members

    public int ClosePane()
    {
      this.Dispose(true);
      return VSConstants.S_OK;
    }

    public int CreatePaneWindow(IntPtr hwndParent, int x, int y, int cx, int cy, out IntPtr hwnd)
    {
      Win32Methods.SetParent(Handle, hwndParent);
      hwnd = Handle;

      Size = new System.Drawing.Size(cx - x, cy - y);
      return VSConstants.S_OK;
    }

    public int GetDefaultSize(Microsoft.VisualStudio.OLE.Interop.SIZE[] size)
    {
      if (size.Length >= 1)
      {
        size[0].cx = Size.Width;
        size[0].cy = Size.Height;
      }

      return VSConstants.S_OK;
    }

    public int LoadViewState(Microsoft.VisualStudio.OLE.Interop.IStream pStream)
    {
      return VSConstants.S_OK;
    }

    public int SaveViewState(Microsoft.VisualStudio.OLE.Interop.IStream pStream)
    {
      return VSConstants.S_OK;
    }

    public void RefreshToolbars()
    {
      if (_serviceProvider == null) return;

      IVsUIShell shell = _serviceProvider.GetService(typeof(IVsUIShell)) as IVsUIShell;

      if (shell != null)
      {
        shell.UpdateCommandUI(1);
      }
    }

    public int SetSite(Microsoft.VisualStudio.OLE.Interop.IServiceProvider psp)
    {
      _serviceProvider = new ServiceProvider(psp);
      return VSConstants.S_OK;
    }

    public int TranslateAccelerator(Microsoft.VisualStudio.OLE.Interop.MSG[] lpmsg)
    {
      return VSConstants.S_FALSE;
    }

    #endregion

    #region IOleCommandTarget Members

    public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
    {
      return (int)(Microsoft.VisualStudio.OLE.Interop.Constants.OLECMDERR_E_NOTSUPPORTED);
    }

    public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
    {
      System.Diagnostics.Debug.WriteLine(pguidCmdGroup.ToString());

      if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97)
      {
        switch ((VSConstants.VSStd97CmdID)prgCmds[0].cmdID)
        {
          case VSConstants.VSStd97CmdID.PrimaryKey:
          case VSConstants.VSStd97CmdID.GenerateChangeScript:
            prgCmds[0].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
            System.Diagnostics.Debug.Write(".");
            break;
          default:
            return (int)(Microsoft.VisualStudio.OLE.Interop.Constants.OLECMDERR_E_NOTSUPPORTED);
        }
        return VSConstants.S_OK;
      }

      if (pguidCmdGroup == SQLiteCommandHandler.guidDavinci)
      {
        switch (prgCmds[0].cmdID)
        {
          case (uint)VSConstants.VSStd97CmdID.ManageRelationships:
          case (uint)VSConstants.VSStd97CmdID.ManageIndexes:
          case (uint)VSConstants.VSStd97CmdID.ManageConstraints:
          //case 10: // Table View -> Custom
          //case 14: // Table View -> Modify Custom
          //case 33: // Database Diagram -> Add Table
          //case 1: // Database Diagram -> Add Related Tables
          //case 12: // Database Diagram -> Delete From Database
          //case 51: // Database Diagram -> Remove From Diagram
          //case 13: // Database Diagram -> Autosize Selected Tables
          //case 3: // Database Diagram -> Arrange Selection
          //case 2: // Database Diagram -> Arrange Tables
          //case 16: // Database Diagram -> Zoom -> 200%
          //case 17: // Database Diagram -> Zoom -> 150%
          //case 18: // Database Diagram -> Zoom -> 100%
          //case 19: // Database Diagram -> Zoom -> 75%
          //case 20: // Database Diagram -> Zoom -> 50%
          //case 21: // Database Diagram -> Zoom -> 25%
          //case 22: // Database Diagram -> Zoom -> 10%
          //case 24: // Database Diagram -> Zoom -> To Fit
          //case 6: // Database Diagram -> New Text Annotation
          //case 15: // Database Diagram -> Set Text Annotation Font
          //case 7: // Database Diagram -> Show Relationship Labels
          //case 8: // Database Diagram -> View Page Breaks
          //case 9: // Database Diagram -> Recalculate Page Breaks
          //case 43: // Database Diagram -> Copy Diagram to Clipboard
          //case 41: // Query Designer -> Table Display -> Column Names
          //case 42: // Query Designer -> Table Display -> Name Only
          //case 39: // Query Designer -> Add Table
          case 4: // Insert Column
          case 5: // Delete Column
            prgCmds[0].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
            break;
          default:
            return (int)(Microsoft.VisualStudio.OLE.Interop.Constants.OLECMDERR_E_NOTSUPPORTED);
        }
        return VSConstants.S_OK;
      }

      return (int)(Microsoft.VisualStudio.OLE.Interop.Constants.OLECMDERR_E_NOTSUPPORTED);
    }

    #endregion

    #region ISelectionContainer Members

    int ISelectionContainer.CountObjects(uint dwFlags, out uint pc)
    {
      pc = 1;
      return VSConstants.S_OK;
    }

    int ISelectionContainer.GetObjects(uint dwFlags, uint cObjects, object[] apUnkObjects)
    {
      apUnkObjects[0] = _table;
      return VSConstants.S_OK;
    }

    int ISelectionContainer.SelectObjects(uint cSelect, object[] apUnkSelect, uint dwFlags)
    {
      apUnkSelect[0] = _table;
      return VSConstants.S_OK;
    }

    #endregion

    #region IVsWindowPaneCommit Members

    int IVsWindowPaneCommit.CommitPendingEdit(out int pfCommitFailed)
    {
      pfCommitFailed = 0;
      return VSConstants.S_OK;
    }

    #endregion

    #region IVsWindowFrameNotify Members

    int IVsWindowFrameNotify.OnDockableChange(int fDockable)
    {
      return VSConstants.S_OK;
    }

    int IVsWindowFrameNotify.OnMove()
    {
      return VSConstants.S_OK;
    }

    int IVsWindowFrameNotify.OnShow(int fShow)
    {
      switch ((__FRAMESHOW)fShow)
      {
        case __FRAMESHOW.FRAMESHOW_TabActivated:
        case __FRAMESHOW.FRAMESHOW_WinShown:
          SetPropertyWindow();
          break;
      }
      return VSConstants.S_OK;
    }

    int IVsWindowFrameNotify.OnSize()
    {
      return VSConstants.S_OK;
    }

    #endregion

    private void _dataGrid_CellEnter(object sender, DataGridViewCellEventArgs e)
    {
      if (e.ColumnIndex > -1)
      {
        _dataGrid.BeginEdit(true);
        _dataGrid_SelectionChanged(sender, e);
      }
    }

    private void _dataGrid_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
    {
      _dataGrid.EndEdit();
      if (e.Button == MouseButtons.Right)
      {
        IVsUIShell shell = _serviceProvider.GetService(typeof(IVsUIShell)) as IVsUIShell;
        if (shell != null)
        {
          Guid guid;
          POINTS[] p = new POINTS[1];
          int ret;

          p[0].x = (short)Control.MousePosition.X;
          p[0].y = (short)Control.MousePosition.Y;

          guid = new Guid("732abe74-cd80-11d0-a2db-00aa00a3efff");

          ret = shell.ShowContextMenu(0, ref guid, 259, p, this);
        }
      }
    }

    private void _dataGrid_CellClick(object sender, DataGridViewCellEventArgs e)
    {
      if (e.ColumnIndex == -1 && e.RowIndex == -1)
      {
        _dataGrid.EndEdit();
      }
      _dataGrid_SelectionChanged(sender, e);
    }

    private void _dataGrid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
    {
      if (e.ColumnIndex > -1 || e.RowIndex < 0) return;

      Column col = _dataGrid.Rows[e.RowIndex].Tag as Column;

      if (col == null) return;

      if (col.PrimaryKey.Enabled == true)
      {
        e.Paint(e.ClipBounds, DataGridViewPaintParts.All);
        _imageList.Draw(e.Graphics, e.CellBounds.Left, e.CellBounds.Top + ((e.CellBounds.Bottom - e.CellBounds.Top) - _imageList.ImageSize.Height) / 2, 0);
        e.Handled = true;
      }
    }

    private void _dataGrid_SelectionChanged(object sender, EventArgs e)
    {
      List<object> items = new List<object>();

      for (int n = 0; n < _dataGrid.Rows.Count; n++)
      {
        if (_dataGrid.Rows[n].Selected || (_dataGrid.CurrentCell.RowIndex == n && _dataGrid.IsCurrentCellInEditMode == true))
        {
          if (_dataGrid.Rows[n].Tag != null)
            items.Add(_dataGrid.Rows[n].Tag);
        }
      }

      object[] objs = new object[items.Count];
      items.CopyTo(objs);

      _propertyGrid.SelectedObjects = objs;
    }

    private void _dataGrid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      _propertyGrid.SelectedObjects = _propertyGrid.SelectedObjects;
    }

    private void _dataGrid_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
    {
      _dataGrid_SelectionChanged(sender, e);
    }
  }

  internal class FakeHierarchy : IVsUIHierarchy, IVsPersistHierarchyItem2
  {
    TableDesignerDoc _control;
    IVsUIHierarchy _owner;
    Dictionary<uint, IVsHierarchyEvents> _events = new Dictionary<uint, IVsHierarchyEvents>();

    internal FakeHierarchy(TableDesignerDoc control, IVsUIHierarchy owner)
    {
      _control = control;
      _owner = owner;
    }

    #region IVsUIHierarchy Members

    int IVsUIHierarchy.AdviseHierarchyEvents(IVsHierarchyEvents pEventSink, out uint pdwCookie)
    {
      pdwCookie = 100;
      while (_events.ContainsKey(pdwCookie))
        pdwCookie++;

      _events[pdwCookie] = pEventSink;

      return VSConstants.S_OK;
    }

    int IVsUIHierarchy.Close()
    {
      return VSConstants.S_OK;
    }

    int IVsUIHierarchy.ExecCommand(uint itemid, ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
    {
      return VSConstants.E_NOTIMPL;
    }

    int IVsUIHierarchy.GetCanonicalName(uint itemid, out string pbstrName)
    {
      pbstrName = _control._table.Name;
      return VSConstants.S_OK;
    }

    int IVsUIHierarchy.GetGuidProperty(uint itemid, int propid, out Guid pguid)
    {
      return _owner.GetGuidProperty(itemid, propid, out pguid);
    }

    int IVsUIHierarchy.GetNestedHierarchy(uint itemid, ref Guid iidHierarchyNested, out IntPtr ppHierarchyNested, out uint pitemidNested)
    {
      ppHierarchyNested = IntPtr.Zero;
      pitemidNested = 0;

      return VSConstants.E_NOTIMPL;
    }

    int IVsUIHierarchy.GetProperty(uint itemid, int propid, out object pvar)
    {
      pvar = null;

      switch ((__VSHPROPID)propid)
      {
        case __VSHPROPID.VSHPROPID_AllowEditInRunMode:
          pvar = true;
          break;
        case __VSHPROPID.VSHPROPID_CanBuildFromMemory:
          pvar = true;
          break;
        case __VSHPROPID.VSHPROPID_Caption:
        case __VSHPROPID.VSHPROPID_SaveName:
          pvar = _control._table.Name;
          break;
        case __VSHPROPID.VSHPROPID_IsHiddenItem:
          pvar = true;
          break;
        case __VSHPROPID.VSHPROPID_IsNewUnsavedItem:
          pvar = true;
          break;
        case __VSHPROPID.VSHPROPID_ShowOnlyItemCaption:
          pvar = true;
          break;
        case __VSHPROPID.VSHPROPID_IconImgList:
          pvar = 0;
          break;
        case __VSHPROPID.VSHPROPID_IconHandle:
          pvar = null;
          return VSConstants.S_OK;
      }

      //switch ((__VSHPROPID2)propid)
      //{
      //  case __VSHPROPID2.VSHPROPID_StatusBarClientText:
      //    pvar = "SQLite Table Editor";
      //    break;
      //}

      if (pvar == null)
        return _owner.GetProperty(itemid, propid, out pvar);
      else
        return VSConstants.S_OK;
    }

    int IVsUIHierarchy.GetSite(out Microsoft.VisualStudio.OLE.Interop.IServiceProvider ppSP)
    {
      ppSP = null;
      return VSConstants.E_NOTIMPL;
    }

    int IVsUIHierarchy.ParseCanonicalName(string pszName, out uint pitemid)
    {
      pitemid = 0;
      return VSConstants.E_NOTIMPL;
    }

    int IVsUIHierarchy.QueryClose(out int pfCanClose)
    {
      pfCanClose = 1;
      return VSConstants.S_OK;
    }

    int IVsUIHierarchy.QueryStatusCommand(uint itemid, ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
    {
      return VSConstants.E_NOTIMPL;
    }

    int IVsUIHierarchy.SetGuidProperty(uint itemid, int propid, ref Guid rguid)
    {
      return VSConstants.E_NOTIMPL;
    }

    int IVsUIHierarchy.SetProperty(uint itemid, int propid, object var)
    {
      foreach(IVsHierarchyEvents listener in _events.Values)
      {
        listener.OnPropertyChanged(itemid, propid, 0);
      }
      return VSConstants.S_OK;
    }

    int IVsUIHierarchy.SetSite(Microsoft.VisualStudio.OLE.Interop.IServiceProvider psp)
    {
      return VSConstants.E_NOTIMPL;
    }

    int IVsUIHierarchy.UnadviseHierarchyEvents(uint dwCookie)
    {
      _events.Remove(dwCookie);
      return VSConstants.S_OK;
    }

    int IVsUIHierarchy.Unused0()
    {
      return VSConstants.E_NOTIMPL;
    }

    int IVsUIHierarchy.Unused1()
    {
      return VSConstants.E_NOTIMPL;
    }

    int IVsUIHierarchy.Unused2()
    {
      return VSConstants.E_NOTIMPL;
    }

    int IVsUIHierarchy.Unused3()
    {
      return VSConstants.E_NOTIMPL;
    }

    int IVsUIHierarchy.Unused4()
    {
      return VSConstants.E_NOTIMPL;
    }

    #endregion

    #region IVsHierarchy Members

    int IVsHierarchy.AdviseHierarchyEvents(IVsHierarchyEvents pEventSink, out uint pdwCookie)
    {
      return ((IVsUIHierarchy)this).AdviseHierarchyEvents(pEventSink, out pdwCookie);
    }

    int IVsHierarchy.Close()
    {
      return ((IVsUIHierarchy)this).Close();
    }

    int IVsHierarchy.GetCanonicalName(uint itemid, out string pbstrName)
    {
      return ((IVsUIHierarchy)this).GetCanonicalName(itemid, out pbstrName);
    }

    int IVsHierarchy.GetGuidProperty(uint itemid, int propid, out Guid pguid)
    {
      return ((IVsUIHierarchy)this).GetGuidProperty(itemid, propid, out pguid);
    }

    int IVsHierarchy.GetNestedHierarchy(uint itemid, ref Guid iidHierarchyNested, out IntPtr ppHierarchyNested, out uint pitemidNested)
    {
      return ((IVsUIHierarchy)this).GetNestedHierarchy(itemid, ref iidHierarchyNested, out ppHierarchyNested, out pitemidNested);
    }

    int IVsHierarchy.GetProperty(uint itemid, int propid, out object pvar)
    {
      return ((IVsUIHierarchy)this).GetProperty(itemid, propid, out pvar);
    }

    int IVsHierarchy.GetSite(out Microsoft.VisualStudio.OLE.Interop.IServiceProvider ppSP)
    {
      ppSP = null;
      return VSConstants.E_NOTIMPL;
    }

    int IVsHierarchy.ParseCanonicalName(string pszName, out uint pitemid)
    {
      pitemid = 0;
      return VSConstants.E_NOTIMPL;
    }

    int IVsHierarchy.QueryClose(out int pfCanClose)
    {
      return ((IVsUIHierarchy)this).QueryClose(out pfCanClose);
    }

    int IVsHierarchy.SetGuidProperty(uint itemid, int propid, ref Guid rguid)
    {
      return ((IVsUIHierarchy)this).SetGuidProperty(itemid, propid, ref rguid);
    }

    int IVsHierarchy.SetProperty(uint itemid, int propid, object var)
    {
      return ((IVsUIHierarchy)this).SetProperty(itemid, propid, var);
    }

    int IVsHierarchy.SetSite(Microsoft.VisualStudio.OLE.Interop.IServiceProvider psp)
    {
      return ((IVsUIHierarchy)this).SetSite(psp);
    }

    int IVsHierarchy.UnadviseHierarchyEvents(uint dwCookie)
    {
      return ((IVsUIHierarchy)this).UnadviseHierarchyEvents(dwCookie);
    }

    int IVsHierarchy.Unused0()
    {
      return VSConstants.E_NOTIMPL;
    }

    int IVsHierarchy.Unused1()
    {
      return VSConstants.E_NOTIMPL;
    }

    int IVsHierarchy.Unused2()
    {
      return VSConstants.E_NOTIMPL;
    }

    int IVsHierarchy.Unused3()
    {
      return VSConstants.E_NOTIMPL;
    }

    int IVsHierarchy.Unused4()
    {
      return VSConstants.E_NOTIMPL;
    }

    #endregion

    #region IVsPersistHierarchyItem Members

    int IVsPersistHierarchyItem.IsItemDirty(uint itemid, IntPtr punkDocData, out int pfDirty)
    {
      return ((IVsPersistDocData)_control).IsDocDataDirty(out pfDirty);
    }

    int IVsPersistHierarchyItem.SaveItem(VSSAVEFLAGS dwSave, string pszSilentSaveAsName, uint itemid, IntPtr punkDocData, out int pfCanceled)
    {
      return ((IVsPersistDocData)_control).SaveDocData(dwSave, out pszSilentSaveAsName, out pfCanceled);
    }

    #endregion

    #region IVsPersistHierarchyItem2 Members

    int IVsPersistHierarchyItem2.IgnoreItemFileChanges(uint itemid, int fIgnore)
    {
      return VSConstants.E_NOTIMPL;
    }

    int IVsPersistHierarchyItem2.IsItemDirty(uint itemid, IntPtr punkDocData, out int pfDirty)
    {
      return ((IVsPersistDocData)_control).IsDocDataDirty(out pfDirty);
    }

    int IVsPersistHierarchyItem2.IsItemReloadable(uint itemid, out int pfReloadable)
    {
      return ((IVsPersistDocData)_control).IsDocDataReloadable(out pfReloadable);
    }

    int IVsPersistHierarchyItem2.ReloadItem(uint itemid, uint dwReserved)
    {
      return ((IVsPersistDocData)_control).ReloadDocData(dwReserved);
    }

    int IVsPersistHierarchyItem2.SaveItem(VSSAVEFLAGS dwSave, string pszSilentSaveAsName, uint itemid, IntPtr punkDocData, out int pfCanceled)
    {
      return ((IVsPersistDocData)_control).SaveDocData(dwSave, out pszSilentSaveAsName, out pfCanceled);
    }

    #endregion
  }
}