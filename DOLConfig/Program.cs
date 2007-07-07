using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;
using DOL.GS;

namespace DOLConfig
{
	static class Program
	{
		static Form1 MainForm = null;
		static GameServerConfiguration Config = null;
		static FileInfo ConfigFile = null;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			AppDomain.CurrentDomain.AppendPrivatePath("." + Path.DirectorySeparatorChar + "lib");
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			MainForm = new Form1();
			try
			{
				LoadExistingSettings();
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString());
			}
			Application.Run(MainForm);
		}

		static void LoadExistingSettings()
		{
			ConfigFile = new FileInfo(Application.StartupPath + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar + "serverconfig.xml");
			Config = new GameServerConfiguration();

			if (ConfigFile.Exists)
			{
				Config.LoadFromXMLFile(ConfigFile);
				MainForm.serverTypeComboBox.Text = Config.ServerType.ToString().Replace("GST_", "");
				MainForm.shortNameTextBox.Text = Config.ServerNameShort;
				MainForm.FullNameTextBox.Text = Config.ServerName;
				MainForm.autoAccountCreationCheckBox.Checked = Config.AutoAccountCreation;
				MainForm.databaseTypeComboBox.Text = Config.DBType.ToString().Replace("DATABASE_", "");
				//parse connection string
				if (Config.DBType == DOL.Database.Connection.ConnectionType.DATABASE_MYSQL)
				{
					string[] sets = Config.DBConnectionString.Split(';');
					foreach (string set in sets)
					{
						string[] result = set.Split('=');
						switch (result[0])
						{
							case "Server": MainForm.mysqlHostTextBox.Text = result[1]; break;
							case "Database": MainForm.mysqlDatabaseTextBox.Text = result[1]; break;
							case "User ID": MainForm.mysqlUsernameTextBox.Text = result[1]; break;
							case "Password": MainForm.mysqlPasswordTextBox.Text = result[1]; break;
						}
					}
				}
			}
		}

		public static void SaveSettings()
		{
			switch (MainForm.serverTypeComboBox.Text)
			{
				case "Normal": Config.ServerType = DOL.eGameServerType.GST_Normal; break;
				case "PvE": Config.ServerType = DOL.eGameServerType.GST_PvE; break;
				case "PvP": Config.ServerType = DOL.eGameServerType.GST_PvP; break;
			}
			Config.ServerNameShort = MainForm.shortNameTextBox.Text;
			Config.ServerName = MainForm.FullNameTextBox.Text;
			Config.AutoAccountCreation = MainForm.autoAccountCreationCheckBox.Checked;
			switch (MainForm.databaseTypeComboBox.Text)
			{
				case "MySQL": Config.DBType = DOL.Database.Connection.ConnectionType.DATABASE_MYSQL; break;
			}
			if (Config.DBType == DOL.Database.Connection.ConnectionType.DATABASE_MYSQL)
				Config.DBConnectionString = "Server=" + MainForm.mysqlHostTextBox.Text + ";Database=" + MainForm.mysqlDatabaseTextBox.Text + ";User ID=" + MainForm.mysqlUsernameTextBox.Text + ";Password=" + MainForm.mysqlPasswordTextBox.Text;
			Config.SaveToXMLFile(ConfigFile);
		}
	}
}