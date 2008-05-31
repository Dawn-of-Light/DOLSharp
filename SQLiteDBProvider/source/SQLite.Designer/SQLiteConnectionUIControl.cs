/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace SQLite.Designer
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Data;
  using System.Drawing;
  using System.Text;
  using System.Windows.Forms;
  using Microsoft.VisualStudio.Data;
  using Microsoft.Win32;

  /// <summary>
  /// Provides a UI to edit/create SQLite database connections
  /// </summary>
  [ToolboxItem(false)]
  public partial class SQLiteConnectionUIControl : DataConnectionUIControl
  {
    public SQLiteConnectionUIControl()
    {
      InitializeComponent();
    }

    private void browseButton_Click(object sender, EventArgs e)
    {
      OpenFileDialog dlg = new OpenFileDialog();
      dlg.FileName = fileTextBox.Text;
      dlg.Title = "Select SQLite Database File";

      if (dlg.ShowDialog(this) == DialogResult.OK)
      {
        fileTextBox.Text = dlg.FileName;
        fileTextBox_Leave(sender, e);
      }
    }

    private void newDatabase_Click(object sender, EventArgs e)
    {
      SaveFileDialog dlg = new SaveFileDialog();
      dlg.Title = "Create SQLite Database File";
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        fileTextBox.Text = dlg.FileName;
        fileTextBox_Leave(sender, e);
      }
    }

    #region IDataConnectionUIControl Members

    public override void LoadProperties()
    {
      fileTextBox.Text = ConnectionProperties["Data Source"] as string;
      passwordTextBox.Text = ConnectionProperties["Password"] as string;
    }

    #endregion

    private void passwordTextBox_Leave(object sender, EventArgs e)
    {
      if (String.IsNullOrEmpty(passwordTextBox.Text))
        ConnectionProperties.Remove("Password");
      else
        ConnectionProperties["Password"] = passwordTextBox.Text;
    }

    private void encoding_Changed(object sender, EventArgs e)
    {
      if (utf8RadioButton.Checked == true)
        ConnectionProperties.Remove("UseUTF16Encoding");
      else
        ConnectionProperties["UseUTF16Encoding"] = utf16RadioButton.Checked;
    }

    private void datetime_Changed(object sender, EventArgs e)
    {
      if (iso8601RadioButton.Checked == true)
        ConnectionProperties.Remove("DateTimeFormat");
      else
        ConnectionProperties["DateTimeFormat"] = "Ticks";
    }

    private void sync_Changed(object sender, EventArgs e)
    {
      string sync = "Normal";
      if (fullRadioButton.Checked == true) sync = "Full";
      else if (offRadioButton.Checked == true) sync = "Off";

      if (sync == "Normal")
        ConnectionProperties.Remove("Synchronous");
      else
        ConnectionProperties["Synchronous"] = sync;
    }

    private void pageSizeTextBox_Leave(object sender, EventArgs e)
    {
      int n = Convert.ToInt32(pageSizeTextBox.Text);
      ConnectionProperties["Page Size"] = n;
    }

    private void cacheSizeTextbox_Leave(object sender, EventArgs e)
    {
      int n = Convert.ToInt32(cacheSizeTextbox.Text);
      ConnectionProperties["Cache Size"] = n;
    }

    private void fileTextBox_Leave(object sender, EventArgs e)
    {
      ConnectionProperties["Data Source"] = fileTextBox.Text;
    }
  }
}