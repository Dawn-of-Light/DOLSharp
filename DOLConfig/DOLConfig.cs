/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
using System;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using System.Text.RegularExpressions;

using MySql.Data.MySqlClient;
using DOL.Database.Connection;
using DOL.GS;


namespace DOLConfig
{
	public partial class DolConfig : Form
	{
		/// <summary>
		/// Current GameServerconfiguration holder
		/// </summary>
		private GameServerConfiguration _currentConfig = null;

		/// <summary>
		/// Getter and setter of current GameServerconfiguration
		/// </summary>
		public GameServerConfiguration currentConfig
		{
			get { return _currentConfig; }
			set { _currentConfig = value; }
		}

		/// <summary>
		/// Holds the properties
		/// </summary>
		private DataSet _extraOptions;

		/// <summary>
		/// Gets and Sets property DataSet
		/// </summary>
		public DataSet extraOptions
		{
			get { return _extraOptions; }
			set { _extraOptions = value; }
		}

		/// <summary>
		/// Sets the status label
		/// </summary>
		public string toolstripStatusLabelValue
		{
			set
			{
				if(value == null) {
                    toolstrip_status_label.Text = "Ready to configurate your Server.";
					return;
				}
                toolstrip_status_label.Text = value;
			}
		}
		
		/// <summary>
		/// Intializes components
		/// </summary>
		public DolConfig()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Form onload event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DOLConfig_Load(object sender, EventArgs e)
		{
			//load data from current config file
			try
			{
				toolstripStatusLabelValue = "Loading current configuration ...";
				currentConfig = DOLConfigParser.getCurrentConfiguration();
				loadConfig();
				toolstripStatusLabelValue = null;
			}
			catch (System.IO.FileNotFoundException)
			{
				DialogResult result = MessageBox.Show("There is no configuration file present." + Environment.NewLine + "Do you want me to create the default configuration?" + Environment.NewLine + "Otherwise: Start the GameServer first.", "Config file not found", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
				if (result == DialogResult.Yes)
				{
                    currentConfig = new GameServerConfiguration();
                    try
                    {
                        loadConfig();
                    }
                    catch(System.IO.FileNotFoundException) { }
                    
					saveConfig();
					DOLConfig_Load(sender, e);
					return;
				}
				else
				{
                    Close();
				}
			}
			catch (FormatException ex)
			{
				MessageBox.Show("There are not allowed values in the config file. Please edit them manually." + Environment.NewLine + "Exception: " + ex.Message, "Error in config file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		/// <summary>
		/// Event on change database module
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void database_type_selectbox_SelectedIndexChanged(object sender, EventArgs e)
		{
			ComboBox bc = (ComboBox)sender;
            sp_tab.Enabled = false;

			switch (bc.SelectedItem.ToString().ToLower())
			{
				case "mysql":
                    mysql_groupbox.Enabled = true;
                    xml_groupbox.Enabled = false;
                    sp_tab.Enabled = true;
					break;
                case "xml":
                    mysql_groupbox.Enabled = false;
                    xml_groupbox.Enabled = true;
                    break;
                case "sqlite":
                    mysql_groupbox.Enabled = false;
                    xml_groupbox.Enabled = true;
                    sp_tab.Enabled = true;
                    break;
			}
		}
		
		#region Load configuration

		/// <summary>
		/// Set all data to default button event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void set_default_values_button_Click(object sender, EventArgs e)
		{
			if (tabControl1.SelectedTab == sp_tab)
				ResetSP();
			else
			{
				currentConfig = new GameServerConfiguration();
				loadConfig();
			}
		}

		/// <summary>
		/// Loads data vom current config to the form
		/// </summary>
		private void loadConfig()
		{
			if (currentConfig == null) return;

            //Full Server Name and Short Server Name
            full_server_name_textbox.Text = currentConfig.ServerName;
            short_server_name_textbox.Text = currentConfig.ServerNameShort;

			switch (currentConfig.ServerType)
			{
				case DOL.eGameServerType.GST_PvP:
                    game_type_selectbox.SelectedItem = "PvP";
					break;
				case DOL.eGameServerType.GST_PvE:
                    game_type_selectbox.SelectedItem = "PvE";
					break;
				case DOL.eGameServerType.GST_Roleplay:
                    game_type_selectbox.SelectedItem = "Roleplay";
					break;
				case DOL.eGameServerType.GST_Casual:
                    game_type_selectbox.SelectedItem = "Casual";
					break;
				case DOL.eGameServerType.GST_Test:
                    game_type_selectbox.SelectedItem = "Test";
					break;
				case DOL.eGameServerType.GST_Normal:
				default:
                    game_type_selectbox.SelectedItem = "Normal";
					break;
			}

            //Parse Auto Account creation
            auto_account_creation_checkbox.Checked = currentConfig.AutoAccountCreation;

            //Ip and Port settings
            ip_textbox.Text = currentConfig.IP.ToString();
            port_textbox.Text = currentConfig.Port.ToString();
            udp_port_textbox.Text = currentConfig.UDPPort.ToString();
            detect_region_ip_checkbox.Checked = currentConfig.DetectRegionIP;
            region_ip_textbox.Text = currentConfig.RegionIP.ToString();
            region_port_textbox.Text = currentConfig.RegionPort.ToString();

            //Database Settings
            database_autosave_checkbox.Checked = currentConfig.AutoSave;
            database_autosave_interval_textbox.Text = currentConfig.SaveInterval.ToString();

			switch (currentConfig.DBType)
			{
				case ConnectionType.DATABASE_XML:
                    database_type_selectbox.SelectedItem = "XML";
                    xml_path_textbox.Text = currentConfig.DBConnectionString;
					break;
				case ConnectionType.DATABASE_SQLITE:
                    database_type_selectbox.SelectedItem = "SQLite";
                    xml_path_textbox.Text = currentConfig.DBConnectionString;
					break;
				case ConnectionType.DATABASE_MYSQL:
				default:
                    database_type_selectbox.SelectedItem = "MySQL";
					MySqlConnectionStringBuilder sb = new MySqlConnectionStringBuilder(currentConfig.DBConnectionString);
                    mysql_host_textbox.Text = sb.Server;
                    mysql_port_textbox.Text = sb.Port.ToString();
                    mysql_database_name_textbox.Text = sb.Database;
                    mysql_username_textbox.Text = sb.UserID;
                    mysql_password_textbox.Text = sb.Password;

					break;
			}

            //Load extra options
            extraOptions = DOLConfigParser.loadExtraOptions();
			if (extraOptions != null)
			{
				extra_options_datagrid.DataSource = extraOptions;
			}
		}

		#endregion

		#region Save configuration

		/// <summary>
		/// Save configuration button event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void save_config_button_Click(object sender, EventArgs e)
		{
			wrong_data_error_handler.Clear();
			
			if (tabControl1.SelectedTab == sp_tab)
				SaveSP();
			else
				saveConfig();
		}

		/// <summary>
		/// Saves current config out of the form
		/// </summary>
		private void saveConfig()
		{
			toolstripStatusLabelValue = "Try to save configuration ...";
			if (currentConfig == null)
			{
				currentConfig = new GameServerConfiguration();
			}

			//Full Server Name
			if (full_server_name_textbox.Text.Length == 0)
			{
				addWrongValueErrorHandler(full_server_name_textbox, "The value of \"Full Server Name\" is not set.");
				return;
			}
			currentConfig.ServerName = full_server_name_textbox.Text;

			//Short Server Name
			if (short_server_name_textbox.Text.Length == 0)
			{
				addWrongValueErrorHandler(short_server_name_textbox, "The value of \"Short Server Name\" is not set.");
				return;
			}
			currentConfig.ServerNameShort = short_server_name_textbox.Text;

			switch (game_type_selectbox.SelectedItem.ToString().ToLower())
			{
				case "pvp":
					currentConfig.ServerType = DOL.eGameServerType.GST_PvP;
					break;
				case "pve":
					currentConfig.ServerType = DOL.eGameServerType.GST_PvE;
					break;
				case "roleplay":
					currentConfig.ServerType = DOL.eGameServerType.GST_Roleplay;
					break;
				case "casual":
					currentConfig.ServerType = DOL.eGameServerType.GST_Casual;
					break;
				case "test":
					currentConfig.ServerType = DOL.eGameServerType.GST_Test;
					break;
				case "normal":
				default:
					currentConfig.ServerType = DOL.eGameServerType.GST_Normal;
					break;
			}

			//Parse Auto Account creation
			currentConfig.AutoAccountCreation = auto_account_creation_checkbox.Checked;

			//Ip
			if (ip_textbox.Text.Length == 0)
			{
				addWrongValueErrorHandler(ip_textbox, "The value of \"IP\" is not set.");
				return;
			}
			try
			{
				currentConfig.IP = new System.Net.IPAddress(ipToByteArray(ip_textbox.Text));
			}
			catch (Exception)
			{
				addWrongValueErrorHandler(ip_textbox, "The value of \"IP\" is not allowed.");
				return;
			}

			//currentConfig.Ip = new System.Net.IPAddress();
			//Port
			if (port_textbox.Text.Length == 0 || Convert.ToUInt16(port_textbox.Text) == 0)
			{
				addWrongValueErrorHandler(port_textbox, "The value of \"TCP Port\" is not allowed.");
				return;
			}
			currentConfig.Port = Convert.ToUInt16(port_textbox.Text);

			//UDP port
			if (udp_port_textbox.Text.Length == 0 || Convert.ToUInt16(udp_port_textbox.Text) == 0)
			{
				addWrongValueErrorHandler(udp_port_textbox, "The value of \"UDP Port\" is not allowed.");
				return;
			}
			currentConfig.UDPPort = Convert.ToUInt16(udp_port_textbox.Text);

			//Detect Region IPs
			currentConfig.DetectRegionIP = detect_region_ip_checkbox.Checked;

			//Region IP
			if (region_ip_textbox.Text.Length == 0)
			{
				addWrongValueErrorHandler(region_ip_textbox, "The value of \"Region IP\" is not set.");
				return;
			}
			try
			{
				currentConfig.RegionIP = new System.Net.IPAddress(ipToByteArray(region_ip_textbox.Text));
			}
			catch (Exception)
			{
				addWrongValueErrorHandler(region_ip_textbox, "The value of \"Region IP\" is not allowed.");
				return;
			}


			//Region port
			if (region_port_textbox.Text.Length == 0 || Convert.ToUInt16(region_port_textbox.Text) == 0)
			{
				addWrongValueErrorHandler(region_port_textbox, "The value of \"Region Port\" is not allowed.");
				return;
			}
			currentConfig.RegionPort = Convert.ToUInt16(region_port_textbox.Text);

			//Database Settings
			currentConfig.AutoSave = database_autosave_checkbox.Checked;

			//Auto database save interval
			if (database_autosave_interval_textbox.Text.Length == 0 || Convert.ToInt32(database_autosave_interval_textbox.Text) == 0)
			{
				addWrongValueErrorHandler(database_autosave_interval_textbox, "The value of \"Autosave Interval\" is not allowed.");
				return;
			}
			currentConfig.SaveInterval = Convert.ToInt32(database_autosave_interval_textbox.Text);

			//Database settings
			switch (database_type_selectbox.SelectedItem.ToString().ToLower())
			{
				case "xml":
					currentConfig.DBType = ConnectionType.DATABASE_XML;
					if(xml_path_textbox.Text.Length == 0) {
						addWrongValueErrorHandler(xml_path_textbox, "The value of \"Directory\" in \"XML Database settings\" is not set.");
						return;
					}
					currentConfig.DBConnectionString = xml_path_textbox.Text;
					break;
				case "sqlite":
					currentConfig.DBType = ConnectionType.DATABASE_SQLITE;
					currentConfig.DBConnectionString = xml_path_textbox.Text;
					break;
				case "mysql":
					currentConfig.DBType = ConnectionType.DATABASE_MYSQL;

					//Mysql connection string builder
					MySqlConnectionStringBuilder sb = new MySqlConnectionStringBuilder();

					//Host
					if (mysql_host_textbox.Text.Length == 0)
					{
						addWrongValueErrorHandler(mysql_host_textbox, "The value of \"Server Address\" in \"MySQL Database settings\" is not set.");
						return;
					}
					sb.Server = mysql_host_textbox.Text;

					//Port
					if (mysql_port_textbox.Text.Length == 0 || Convert.ToUInt16(mysql_port_textbox.Text) == 0)
					{
						addWrongValueErrorHandler(mysql_port_textbox, "The value of \"Port\" in \"MySQL Database settings\" is not allowed.");
						return;
					}
					sb.Port = Convert.ToUInt16(mysql_port_textbox.Text);

					//Database Name
					if (mysql_database_name_textbox.Text.Length == 0)
					{
						addWrongValueErrorHandler(mysql_database_name_textbox, "The value of \"Database Name\" in \"MySQL Database settings\" is not set.");
						return;
					}
					sb.Database = mysql_database_name_textbox.Text;

					//Username
					if (mysql_username_textbox.Text.Length == 0)
					{
						addWrongValueErrorHandler(mysql_username_textbox, "The value of \"Username\" in \"MySQL Database settings\" is not set.");
						return;
					}
					sb.UserID = mysql_username_textbox.Text;

					//Password
					sb.Password = mysql_password_textbox.Text;

					//Treat tiny as boolean
					sb.TreatTinyAsBoolean = false;

					//Set generated connection string
					currentConfig.DBConnectionString = sb.ConnectionString;

					//Just for fun: Test the connection
					mysql_test_button_Click(null, null);

					break;
				default:
					addWrongValueErrorHandler(database_type_selectbox, "There is no database connection selected.");
					return;
			}

			//Finally save all configuration
			DOLConfigParser.saveCurrentConfiguration(currentConfig);

			//And write extra properties
			if(extraOptions != null) {
				DOLConfigParser.saveExtraOptions(extraOptions);
			}

			toolstripStatusLabelValue = "Configuration saved.";
			toolstripTimer.Start();
		}

		/// <summary>
		/// Sets the toolstrip label back to the basics after save
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void toolstripTimer_Tick(object sender, EventArgs e)
		{
            toolstripStatusLabelValue = null;
			((Timer)sender).Stop();
		}

		/// <summary>
		/// Convert a string value to a byte array
		/// </summary>
		/// <param name="value">string</param>
		/// <returns>byte[]</returns>
		private byte[] ipToByteArray(string value)
		{
			string[] temp = value.Split('.');
			int size = temp.Length;
			byte[] ip_address = new byte[size];
			for (int i = 0; i < size; i++)
			{
				ip_address[i] = Convert.ToByte(Convert.ToInt32(temp[i]));
			}
			return ip_address;
		}


		/// <summary>
		/// Adds an error handler to a control
		/// </summary>
		/// <param name="recipient"></param>
		/// <param name="error"></param>
		private void addWrongValueErrorHandler(Control recipient, string error)
		{
            wrong_data_error_handler.SetError(recipient, error);
			toolstripStatusLabelValue = error;
		}
		#endregion

		#region XML path select
		/// <summary>
		/// Select xml database path by dialog
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void xml_database_path_button_Click(object sender, EventArgs e)
		{
		    var currentConnString = @xml_path_textbox.Text;
            xml_database_path_select_dialog.CheckFileExists = false;
            xml_database_path_select_dialog.InitialDirectory = new System.IO.FileInfo(Regex.Replace(currentConnString, ".*Data ?Source=([^;]+)(;?).*", "$1")).DirectoryName;
			DialogResult result = xml_database_path_select_dialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
			    xml_path_textbox.Text = Regex.Replace(currentConnString, "Data ?Source=([^;]+)(;?)", string.Format("Data Source={0}$2", xml_database_path_select_dialog.FileName));
			}
		}
		#endregion

		#region MySQL Testing feature

		/// <summary>
		/// MySQL connection test button event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void mysql_test_button_Click(object sender, EventArgs e)
		{
			if (mysql_test_background_worker.IsBusy)
			{
				mysql_test_label.Text = "still testing ... please wait";
				return;
			}

			mysql_test_button.Enabled = false;
			mysql_test_progressbar.Visible = true;
			mysql_test_label.ForeColor = System.Drawing.SystemColors.ControlText;
			mysql_test_label.Text = "testing ...";
			mysql_test_background_worker.RunWorkerAsync();
		}

		/// <summary>
		/// Backgroundworker process for testing MySQL connection
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void mysql_test_background_worker_DoWork(object sender, DoWorkEventArgs e)
		{
			
			MySqlConnectionStringBuilder sb = new MySqlConnectionStringBuilder();
			sb.Server = mysql_host_textbox.Text;
			sb.Port = Convert.ToUInt32(mysql_port_textbox.Text);
			sb.Database = mysql_database_name_textbox.Text;
			sb.UserID = mysql_username_textbox.Text;
			sb.Password = mysql_password_textbox.Text;
			sb.ConnectionTimeout = 2;

			MySqlConnection con = new MySqlConnection(sb.ConnectionString);
			try
			{
				con.Open();
				e.Result = "Congratulations! I am connected!"; ;
			}
			catch (MySqlException ex)
			{
				e.Result = ex;
			}
			finally
			{
				con.Close();
			}
		}

		/// <summary>
		/// OnComplete events of backgroundworker for testing MySQL connection
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void mysql_test_background_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Result.GetType() == typeof(MySqlException))
			{
				mysql_test_label.ForeColor = System.Drawing.Color.Red;
                mysql_test_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
				mysql_test_label.Text = ((MySqlException)e.Result).Message;
			}
			else
			{
				mysql_test_label.ForeColor = System.Drawing.Color.Green;
                mysql_test_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
				mysql_test_label.Text = "Congratulations! I am connected!";
			}
			mysql_test_button.Enabled = true;
			mysql_test_progressbar.Visible = false;
		}

		/// <summary>
		/// Event of start testing by hit enter on a textbox
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void mysql_textbox_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)13)
			{
				mysql_test_button_Click(null, null);
				e.Handled = true;
			}
		}

		#endregion

		#region Extra properties section

		private void extra_options_datagrid_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
		{
            edit_property(e.RowIndex);
		}

		private void edit_property_button_Click(object sender, EventArgs e)
		{
			if (extra_options_datagrid.CurrentRow == null) return;
            edit_property(extra_options_datagrid.CurrentRow.Index);
		}

		private void edit_property(int rowIndex) {
			string property_name = Convert.ToString(extra_options_datagrid.Rows[rowIndex].Cells["property"].Value);
			string property_type = Convert.ToString(extra_options_datagrid.Rows[rowIndex].Cells["type"].Value);
			object property_value = extra_options_datagrid.Rows[rowIndex].Cells["value"].Value;
			string property_description = Convert.ToString(extra_options_datagrid.Rows[rowIndex].Cells["description"].Value);

			ExtraPropertiesEditor property_editor = new ExtraPropertiesEditor(property_name, property_type, property_value, property_description);
			DialogResult result = property_editor.ShowDialog();
			if (result == DialogResult.OK)
			{
				extra_options_datagrid.Rows[rowIndex].Cells["property"].Value = (object)property_editor.propertyName;
				extra_options_datagrid.Rows[rowIndex].Cells["type"].Value = (object)property_editor.propertyType;
				extra_options_datagrid.Rows[rowIndex].Cells["value"].Value = (object)property_editor.propertyValue;
			}
		}

		private void add_property_button_Click(object sender, EventArgs e)
		{
			ExtraPropertiesEditor property_editor = new ExtraPropertiesEditor("", "", "", "");
			DialogResult result = property_editor.ShowDialog();
			if (result == DialogResult.OK)
			{
				//Add a new Row
				DOLConfigParser.addExtraOptionsRow(extraOptions, property_editor.propertyName, property_editor.propertyType, (object)property_editor.propertyValue, "");
			}
		}

		private void delete_property_button_Click(object sender, EventArgs e)
		{
			if (extra_options_datagrid.CurrentRow == null) return;

			DOLConfigParser.removeExtraOptionsRow(extraOptions, extra_options_datagrid.Rows[extra_options_datagrid.CurrentRow.Index].Cells["property"].Value);
		}

		#endregion

		#region Mouseover helpers

		private void reset_mouse_enter_toolstrip_values(object sender, EventArgs e)
		{
            toolstripStatusLabelValue = null;
		}

		private void full_server_name_textbox_MouseEnter(object sender, EventArgs e)
		{
            toolstripStatusLabelValue = "The full name for the server.";
		}

		private void short_server_name_textbox_MouseEnter(object sender, EventArgs e)
		{
            toolstripStatusLabelValue = "The short name for the server.";
		}

		private void game_type_selectbox_MouseEnter(object sender, EventArgs e)
		{
            toolstripStatusLabelValue = "The gaming type for the server.";
		}

		private void auto_account_creation_checkbox_MouseEnter(object sender, EventArgs e)
		{
            toolstripStatusLabelValue = "Create accounts automatically when a user loggs in.";
		}

		private void database_type_selectbox_MouseEnter(object sender, EventArgs e)
		{
            toolstripStatusLabelValue = "Set the database type for this server.";
		}

		private void database_autosave_checkbox_MouseEnter(object sender, EventArgs e)
		{
            toolstripStatusLabelValue = "Autosave database after a set interval.";
		}

		private void database_autosave_interval_textbox_MouseEnter(object sender, EventArgs e)
		{
            toolstripStatusLabelValue = "The interval (in minutes) in which the database should save automatically.";
		}

		private void mysql_host_textbox_MouseEnter(object sender, EventArgs e)
		{
            toolstripStatusLabelValue = "Set the MySQL host for this server.";
		}

		private void mysql_port_textbox_MouseEnter(object sender, EventArgs e)
		{
            toolstripStatusLabelValue = "Set the MySQL port for this server.";
		}

		private void mysql_database_name_textbox_MouseEnter(object sender, EventArgs e)
		{
            toolstripStatusLabelValue = "Set the MySQL database name for this server.";
		}

		private void mysql_username_textbox_MouseEnter(object sender, EventArgs e)
		{
            toolstripStatusLabelValue = "Set the MySQL username for this server.";
		}

		private void mysql_password_textbox_MouseEnter(object sender, EventArgs e)
		{
            toolstripStatusLabelValue = "Set the MySQL password for this server.";
		}

		private void mysql_test_button_MouseEnter(object sender, EventArgs e)
		{
            toolstripStatusLabelValue = "Test the connectivity of the MySQL server.";
		}

		private void xml_path_textbox_MouseEnter(object sender, EventArgs e)
		{
            toolstripStatusLabelValue = "The path where the XML database should be saved.";
		}

		private void xml_database_path_button_MouseEnter(object sender, EventArgs e)
		{
            toolstripStatusLabelValue = "Select the location by using a wizard.";
		}

		private void set_default_values_button_MouseEnter(object sender, EventArgs e)
		{
            toolstripStatusLabelValue = "Reset all data to its default values.";
		}

		private void save_config_button_MouseEnter(object sender, EventArgs e)
		{
            toolstripStatusLabelValue = "Save all entered configuration values.";
		}

		private void delete_property_button_MouseEnter(object sender, EventArgs e)
		{
            toolstripStatusLabelValue = "Delete the current selected property";
		}

		private void edit_property_button_MouseEnter(object sender, EventArgs e)
		{
            toolstripStatusLabelValue = "Edit the current selected property.";
		}

		private void add_property_button_MouseEnter(object sender, EventArgs e)
		{
            toolstripStatusLabelValue = "Add a new property.";
		}

		#endregion

	}
}
