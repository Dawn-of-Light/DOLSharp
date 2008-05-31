using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace install
{
  static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Environment.CurrentDirectory = System.IO.Path.GetDirectoryName(Application.ExecutablePath);

      InstallDesigner designer = new InstallDesigner();
      if (designer._remove == false)
        designer.ShowDialog();
    }
  }
}