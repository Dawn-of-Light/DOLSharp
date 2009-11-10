using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using DOL.Database;
using DOL.Database.Connection;
using DOL.GS;

namespace DOLConfig
{
	static class Program
	{
		static Form1 MainForm = null;
		static GameServerConfiguration Config = null;
		static FileInfo ConfigFile = null;
		static List<ServerProperty> spList = new List<ServerProperty>();
		static List<ServerPropertyCategory> scList = new List<ServerPropertyCategory>();

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			//AppDomain.CurrentDomain.AppendPrivatePath("." + Path.DirectorySeparatorChar + "lib");
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			MainForm = new Form1();
			LoadExistingSettings();

			Application.Run(MainForm);
		}

		static void LoadExistingSettings()
		{
			try
			{
				if (!File.Exists(Application.StartupPath + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar + "serverconfig.xml"))
					return;

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
				else
				{
					Config.SaveToXMLFile(ConfigFile);
				}
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString());
			}
		}

		public static void SaveSettings()
		{
			try
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
					Config.DBConnectionString = "Server=" + MainForm.mysqlHostTextBox.Text + ";Database=" + MainForm.mysqlDatabaseTextBox.Text + ";User ID=" + MainForm.mysqlUsernameTextBox.Text + ";Password=" + MainForm.mysqlPasswordTextBox.Text + ";Treat Tiny As Boolean=False";
				Config.SaveToXMLFile(ConfigFile);
				MessageBox.Show("Settings saved successfully");
			}
			catch (Exception e)
			{
				MessageBox.Show("Cannot save settings: " + e.Message);
			}
		}
		
		public static void LoadProperties()
		{
			// open data connection
			ServerProperty[] sp;
			ServerPropertyCategory[] sc;
			try
			{
				DataConnection dc = new DataConnection(Config.DBType, Config.DBConnectionString);
				ObjectDatabase db = new ObjectDatabase(dc);
				sp = (ServerProperty[])db.SelectAllObjects(typeof(ServerProperty));
				sc = (ServerPropertyCategory[])db.SelectAllObjects(typeof(ServerPropertyCategory));
			}
			catch
			{
				MainForm.toolStripStatusLabel1.Text="Unable to connect/read SQL database !";
				return;
			}
			
			// bufferisation of the datas
			spList.Clear();
			scList.Clear();
			foreach(var current in sp)
				spList.Add(current);
			foreach(var current in sc)
				scList.Add(current);
			
			// creation of the nodemap
			foreach(var current in scList)
			{
				if (string.IsNullOrEmpty(current.ParentCategory) && !string.IsNullOrEmpty(current.BaseCategory))
					CreateMap(MainForm.spTvCat.Nodes, current.BaseCategory, current.ObjectId);
			}
			
			// filling the nodemap
			//FillMap();
			
			// how many SP we have ? 1.6millions ? :D
			//MainForm.toolStripStatusLabel1.Text ="Loaded: " + spList.Count() + " server properties."; //result.Count() + " properties on " + (from i in sp select i).Count() + " total.";
		}

		static void CreateMap(TreeNodeCollection nodes, string parent, string id)
		{
			// create root node
			foreach (var current in scList)
			{
				if (current.ParentCategory == parent && current.ObjectId != id)
				{
					nodes.Add(current.BaseCategory);
					nodes[nodes.Count - 1].ForeColor = Color.Green;
					CreateMap(nodes, current.ParentCategory, current.ObjectId);
				}
			}
		}
		
		static void FillMap()
		{
			// attach sps to root node
			foreach (var current in spList)
			{
				if (string.IsNullOrEmpty(current.Category))
				{
					MainForm.spTvCat.Nodes.Add(current.Key);
					MainForm.spTvCat.Nodes[MainForm.spTvCat.Nodes.Count - 1].ForeColor = Color.Blue;
				}
			}
		}
		
		public static void SelectProperty(string spkey)
		{
			// find the SP
			var target = (from i in spList where i.Key == spkey select i).Single();
			MainForm.spLblKey.Text = target.Key.ToUpper();
			MainForm.spLblDesc.Text = target.Description;
			MainForm.spLblDefault.Text = target.DefaultValue;
			MainForm.spCbValue.Text = target.Value;
		}
	}
}