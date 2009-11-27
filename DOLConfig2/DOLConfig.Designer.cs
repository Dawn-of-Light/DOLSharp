namespace DOLConfig
{
    partial class DolConfig
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.GroupBox groupBox1;
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label1;
            System.Windows.Forms.GroupBox groupBox2;
            System.Windows.Forms.Label label8;
            System.Windows.Forms.Label label7;
            System.Windows.Forms.Label label6;
            System.Windows.Forms.Label label5;
            System.Windows.Forms.Label label4;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DolConfig));
            this.auto_account_creation_checkbox = new System.Windows.Forms.CheckBox();
            this.game_type_selectbox = new System.Windows.Forms.ComboBox();
            this.short_server_name_textbox = new System.Windows.Forms.TextBox();
            this.full_server_name_textbox = new System.Windows.Forms.TextBox();
            this.detect_region_ip_checkbox = new System.Windows.Forms.CheckBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.database_tab = new System.Windows.Forms.TabPage();
            this.general_tab = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label17 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.database_autosave_checkbox = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.database_type_selectbox = new System.Windows.Forms.ComboBox();
            this.xml_groupbox = new System.Windows.Forms.GroupBox();
            this.xml_database_path_button = new System.Windows.Forms.Button();
            this.xml_path_textbox = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.mysql_groupbox = new System.Windows.Forms.GroupBox();
            this.mysql_test_progressbar = new System.Windows.Forms.ProgressBar();
            this.mysql_says_label = new System.Windows.Forms.Label();
            this.mysql_password_textbox = new System.Windows.Forms.TextBox();
            this.mysql_test_label = new System.Windows.Forms.Label();
            this.mysql_test_button = new System.Windows.Forms.Button();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.tab_extra = new System.Windows.Forms.TabPage();
            this.edit_property_button = new System.Windows.Forms.Button();
            this.delete_property_button = new System.Windows.Forms.Button();
            this.add_property_button = new System.Windows.Forms.Button();
            this.extra_options_datagrid = new System.Windows.Forms.DataGridView();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolstrip_status_label = new System.Windows.Forms.ToolStripStatusLabel();
            this.xml_database_path_select_dialog = new System.Windows.Forms.FolderBrowserDialog();
            this.mysql_test_background_worker = new System.ComponentModel.BackgroundWorker();
            this.wrong_data_error_handler = new System.Windows.Forms.ErrorProvider(this.components);
            this.save_config_button = new System.Windows.Forms.Button();
            this.set_default_values_button = new System.Windows.Forms.Button();
            this.toolstripTimer = new System.Windows.Forms.Timer(this.components);
            this.property = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.value = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.description = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.type = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.region_port_textbox = new RegExControls.RegExTextBox();
            this.region_ip_textbox = new RegExControls.RegExTextBox();
            this.udp_port_textbox = new RegExControls.RegExTextBox();
            this.port_textbox = new RegExControls.RegExTextBox();
            this.ip_textbox = new RegExControls.RegExTextBox();
            this.database_autosave_interval_textbox = new RegExControls.RegExTextBox();
            this.mysql_database_name_textbox = new RegExControls.RegExTextBox();
            this.mysql_username_textbox = new RegExControls.RegExTextBox();
            this.mysql_port_textbox = new RegExControls.RegExTextBox();
            this.mysql_host_textbox = new RegExControls.RegExTextBox();
            groupBox1 = new System.Windows.Forms.GroupBox();
            label3 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            groupBox2 = new System.Windows.Forms.GroupBox();
            label8 = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.database_tab.SuspendLayout();
            this.general_tab.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.xml_groupbox.SuspendLayout();
            this.mysql_groupbox.SuspendLayout();
            this.tab_extra.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.extra_options_datagrid)).BeginInit();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.wrong_data_error_handler)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            groupBox1.Controls.Add(this.auto_account_creation_checkbox);
            groupBox1.Controls.Add(this.game_type_selectbox);
            groupBox1.Controls.Add(this.short_server_name_textbox);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(this.full_server_name_textbox);
            groupBox1.Controls.Add(label1);
            groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            groupBox1.Location = new System.Drawing.Point(6, 6);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(390, 130);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Gameserver specific (required)";
            // 
            // auto_account_creation_checkbox
            // 
            this.auto_account_creation_checkbox.AutoSize = true;
            this.auto_account_creation_checkbox.Location = new System.Drawing.Point(112, 103);
            this.auto_account_creation_checkbox.Name = "auto_account_creation_checkbox";
            this.auto_account_creation_checkbox.Size = new System.Drawing.Size(132, 17);
            this.auto_account_creation_checkbox.TabIndex = 3;
            this.auto_account_creation_checkbox.Text = "Auto Account creation";
            this.auto_account_creation_checkbox.UseVisualStyleBackColor = true;
            this.auto_account_creation_checkbox.MouseLeave += new System.EventHandler(this.reset_mouse_enter_toolstrip_values);
            this.auto_account_creation_checkbox.MouseEnter += new System.EventHandler(this.auto_account_creation_checkbox_MouseEnter);
            // 
            // game_type_selectbox
            // 
            this.game_type_selectbox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.game_type_selectbox.FormattingEnabled = true;
            this.game_type_selectbox.Items.AddRange(new object[] {
            "Normal",
            "PvP",
            "PvE",
            "Casual",
            "Roleplay",
            "Test"});
            this.game_type_selectbox.Location = new System.Drawing.Point(112, 76);
            this.game_type_selectbox.Name = "game_type_selectbox";
            this.game_type_selectbox.Size = new System.Drawing.Size(196, 21);
            this.game_type_selectbox.TabIndex = 2;
            this.game_type_selectbox.MouseEnter += new System.EventHandler(this.game_type_selectbox_MouseEnter);
            this.game_type_selectbox.MouseLeave += new System.EventHandler(this.reset_mouse_enter_toolstrip_values);
            // 
            // short_server_name_textbox
            // 
            this.short_server_name_textbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.short_server_name_textbox.Location = new System.Drawing.Point(112, 50);
            this.short_server_name_textbox.Name = "short_server_name_textbox";
            this.short_server_name_textbox.Size = new System.Drawing.Size(251, 20);
            this.short_server_name_textbox.TabIndex = 1;
            this.short_server_name_textbox.MouseLeave += new System.EventHandler(this.reset_mouse_enter_toolstrip_values);
            this.short_server_name_textbox.MouseEnter += new System.EventHandler(this.short_server_name_textbox_MouseEnter);
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(6, 53);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(100, 13);
            label3.TabIndex = 3;
            label3.Text = "Short Server Name:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(6, 79);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(65, 13);
            label2.TabIndex = 2;
            label2.Text = "Game Type:";
            // 
            // full_server_name_textbox
            // 
            this.full_server_name_textbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.full_server_name_textbox.Location = new System.Drawing.Point(112, 24);
            this.full_server_name_textbox.Name = "full_server_name_textbox";
            this.full_server_name_textbox.Size = new System.Drawing.Size(251, 20);
            this.full_server_name_textbox.TabIndex = 0;
            this.full_server_name_textbox.MouseLeave += new System.EventHandler(this.reset_mouse_enter_toolstrip_values);
            this.full_server_name_textbox.MouseEnter += new System.EventHandler(this.full_server_name_textbox_MouseEnter);
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(6, 27);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(91, 13);
            label1.TabIndex = 0;
            label1.Text = "Full Server Name:";
            // 
            // groupBox2
            // 
            groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            groupBox2.Controls.Add(this.region_port_textbox);
            groupBox2.Controls.Add(this.region_ip_textbox);
            groupBox2.Controls.Add(this.udp_port_textbox);
            groupBox2.Controls.Add(this.port_textbox);
            groupBox2.Controls.Add(this.ip_textbox);
            groupBox2.Controls.Add(label8);
            groupBox2.Controls.Add(label7);
            groupBox2.Controls.Add(label6);
            groupBox2.Controls.Add(label5);
            groupBox2.Controls.Add(this.detect_region_ip_checkbox);
            groupBox2.Controls.Add(label4);
            groupBox2.Location = new System.Drawing.Point(6, 142);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(390, 181);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Server connection specific (optional)";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(6, 78);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(55, 13);
            label8.TabIndex = 16;
            label8.Text = "UDP Port:";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(6, 153);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(66, 13);
            label7.TabIndex = 14;
            label7.Text = "Region Port:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(6, 127);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(57, 13);
            label6.TabIndex = 12;
            label6.Text = "Region IP:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(6, 26);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(20, 13);
            label5.TabIndex = 10;
            label5.Text = "IP:";
            // 
            // detect_region_ip_checkbox
            // 
            this.detect_region_ip_checkbox.AutoSize = true;
            this.detect_region_ip_checkbox.Location = new System.Drawing.Point(112, 101);
            this.detect_region_ip_checkbox.Name = "detect_region_ip_checkbox";
            this.detect_region_ip_checkbox.Size = new System.Drawing.Size(113, 17);
            this.detect_region_ip_checkbox.TabIndex = 3;
            this.detect_region_ip_checkbox.Text = "Detect Region IPs";
            this.detect_region_ip_checkbox.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(6, 52);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(53, 13);
            label4.TabIndex = 8;
            label4.Text = "TCP Port:";
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.database_tab);
            this.tabControl1.Controls.Add(this.general_tab);
            this.tabControl1.Controls.Add(this.tab_extra);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(410, 446);
            this.tabControl1.TabIndex = 2;
            // 
            // database_tab
            // 
            this.database_tab.Controls.Add(groupBox2);
            this.database_tab.Controls.Add(groupBox1);
            this.database_tab.Location = new System.Drawing.Point(4, 22);
            this.database_tab.Name = "database_tab";
            this.database_tab.Padding = new System.Windows.Forms.Padding(3);
            this.database_tab.Size = new System.Drawing.Size(402, 420);
            this.database_tab.TabIndex = 0;
            this.database_tab.Text = "General configuration";
            this.database_tab.UseVisualStyleBackColor = true;
            // 
            // general_tab
            // 
            this.general_tab.Controls.Add(this.groupBox3);
            this.general_tab.Controls.Add(this.xml_groupbox);
            this.general_tab.Controls.Add(this.mysql_groupbox);
            this.general_tab.Location = new System.Drawing.Point(4, 22);
            this.general_tab.Name = "general_tab";
            this.general_tab.Padding = new System.Windows.Forms.Padding(3);
            this.general_tab.Size = new System.Drawing.Size(402, 420);
            this.general_tab.TabIndex = 1;
            this.general_tab.Text = "Database";
            this.general_tab.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.database_autosave_interval_textbox);
            this.groupBox3.Controls.Add(this.label17);
            this.groupBox3.Controls.Add(this.label16);
            this.groupBox3.Controls.Add(this.database_autosave_checkbox);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.database_type_selectbox);
            this.groupBox3.Location = new System.Drawing.Point(9, 6);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(387, 108);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Database Settings (required)";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(178, 79);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(43, 13);
            this.label17.TabIndex = 11;
            this.label17.Text = "minutes";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(6, 79);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(93, 13);
            this.label16.TabIndex = 5;
            this.label16.Text = "Autosave Interval:";
            // 
            // database_autosave_checkbox
            // 
            this.database_autosave_checkbox.AutoSize = true;
            this.database_autosave_checkbox.Location = new System.Drawing.Point(102, 53);
            this.database_autosave_checkbox.Name = "database_autosave_checkbox";
            this.database_autosave_checkbox.Size = new System.Drawing.Size(120, 17);
            this.database_autosave_checkbox.TabIndex = 1;
            this.database_autosave_checkbox.Text = "Autosave Database";
            this.database_autosave_checkbox.UseVisualStyleBackColor = true;
            this.database_autosave_checkbox.MouseLeave += new System.EventHandler(this.reset_mouse_enter_toolstrip_values);
            this.database_autosave_checkbox.MouseEnter += new System.EventHandler(this.database_autosave_checkbox_MouseEnter);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 22);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(79, 13);
            this.label9.TabIndex = 2;
            this.label9.Text = "Database type:";
            // 
            // database_type_selectbox
            // 
            this.database_type_selectbox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.database_type_selectbox.FormattingEnabled = true;
            this.database_type_selectbox.Items.AddRange(new object[] {
            "MySQL",
            "XML"});
            this.database_type_selectbox.Location = new System.Drawing.Point(102, 19);
            this.database_type_selectbox.Name = "database_type_selectbox";
            this.database_type_selectbox.Size = new System.Drawing.Size(121, 21);
            this.database_type_selectbox.TabIndex = 0;
            this.database_type_selectbox.SelectedIndexChanged += new System.EventHandler(this.database_type_selectbox_SelectedIndexChanged);
            this.database_type_selectbox.MouseEnter += new System.EventHandler(this.database_type_selectbox_MouseEnter);
            this.database_type_selectbox.MouseLeave += new System.EventHandler(this.reset_mouse_enter_toolstrip_values);
            // 
            // xml_groupbox
            // 
            this.xml_groupbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.xml_groupbox.Controls.Add(this.xml_database_path_button);
            this.xml_groupbox.Controls.Add(this.xml_path_textbox);
            this.xml_groupbox.Controls.Add(this.label15);
            this.xml_groupbox.Location = new System.Drawing.Point(9, 361);
            this.xml_groupbox.Name = "xml_groupbox";
            this.xml_groupbox.Size = new System.Drawing.Size(387, 56);
            this.xml_groupbox.TabIndex = 1;
            this.xml_groupbox.TabStop = false;
            this.xml_groupbox.Text = "XML Database settings";
            // 
            // xml_database_path_button
            // 
            this.xml_database_path_button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.xml_database_path_button.Image = ((System.Drawing.Image)(resources.GetObject("xml_database_path_button.Image")));
            this.xml_database_path_button.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.xml_database_path_button.Location = new System.Drawing.Point(281, 25);
            this.xml_database_path_button.Name = "xml_database_path_button";
            this.xml_database_path_button.Size = new System.Drawing.Size(100, 23);
            this.xml_database_path_button.TabIndex = 1;
            this.xml_database_path_button.Text = "Select folder ...";
            this.xml_database_path_button.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.xml_database_path_button.UseVisualStyleBackColor = true;
            this.xml_database_path_button.MouseLeave += new System.EventHandler(this.reset_mouse_enter_toolstrip_values);
            this.xml_database_path_button.Click += new System.EventHandler(this.xml_database_path_button_Click);
            this.xml_database_path_button.MouseEnter += new System.EventHandler(this.xml_database_path_button_MouseEnter);
            // 
            // xml_path_textbox
            // 
            this.xml_path_textbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.xml_path_textbox.Location = new System.Drawing.Point(102, 27);
            this.xml_path_textbox.Name = "xml_path_textbox";
            this.xml_path_textbox.Size = new System.Drawing.Size(157, 20);
            this.xml_path_textbox.TabIndex = 0;
            this.xml_path_textbox.MouseLeave += new System.EventHandler(this.reset_mouse_enter_toolstrip_values);
            this.xml_path_textbox.MouseEnter += new System.EventHandler(this.xml_path_textbox_MouseEnter);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(9, 30);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(52, 13);
            this.label15.TabIndex = 10;
            this.label15.Text = "Directory:";
            // 
            // mysql_groupbox
            // 
            this.mysql_groupbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mysql_groupbox.Controls.Add(this.mysql_test_progressbar);
            this.mysql_groupbox.Controls.Add(this.mysql_says_label);
            this.mysql_groupbox.Controls.Add(this.mysql_password_textbox);
            this.mysql_groupbox.Controls.Add(this.mysql_database_name_textbox);
            this.mysql_groupbox.Controls.Add(this.mysql_username_textbox);
            this.mysql_groupbox.Controls.Add(this.mysql_port_textbox);
            this.mysql_groupbox.Controls.Add(this.mysql_host_textbox);
            this.mysql_groupbox.Controls.Add(this.mysql_test_label);
            this.mysql_groupbox.Controls.Add(this.mysql_test_button);
            this.mysql_groupbox.Controls.Add(this.label14);
            this.mysql_groupbox.Controls.Add(this.label13);
            this.mysql_groupbox.Controls.Add(this.label12);
            this.mysql_groupbox.Controls.Add(this.label11);
            this.mysql_groupbox.Controls.Add(this.label10);
            this.mysql_groupbox.Location = new System.Drawing.Point(9, 120);
            this.mysql_groupbox.Name = "mysql_groupbox";
            this.mysql_groupbox.Size = new System.Drawing.Size(387, 235);
            this.mysql_groupbox.TabIndex = 0;
            this.mysql_groupbox.TabStop = false;
            this.mysql_groupbox.Text = "MySQL Database settings";
            // 
            // mysql_test_progressbar
            // 
            this.mysql_test_progressbar.Location = new System.Drawing.Point(217, 158);
            this.mysql_test_progressbar.Name = "mysql_test_progressbar";
            this.mysql_test_progressbar.Size = new System.Drawing.Size(97, 15);
            this.mysql_test_progressbar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.mysql_test_progressbar.TabIndex = 24;
            this.mysql_test_progressbar.Visible = false;
            // 
            // mysql_says_label
            // 
            this.mysql_says_label.AutoSize = true;
            this.mysql_says_label.Location = new System.Drawing.Point(6, 185);
            this.mysql_says_label.Name = "mysql_says_label";
            this.mysql_says_label.Size = new System.Drawing.Size(69, 13);
            this.mysql_says_label.TabIndex = 23;
            this.mysql_says_label.Text = "MySQL says:";
            // 
            // mysql_password_textbox
            // 
            this.mysql_password_textbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mysql_password_textbox.Location = new System.Drawing.Point(102, 128);
            this.mysql_password_textbox.Name = "mysql_password_textbox";
            this.mysql_password_textbox.PasswordChar = '*';
            this.mysql_password_textbox.Size = new System.Drawing.Size(260, 20);
            this.mysql_password_textbox.TabIndex = 4;
            this.mysql_password_textbox.MouseLeave += new System.EventHandler(this.reset_mouse_enter_toolstrip_values);
            this.mysql_password_textbox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.mysql_textbox_KeyPress);
            this.mysql_password_textbox.MouseEnter += new System.EventHandler(this.mysql_password_textbox_MouseEnter);
            // 
            // mysql_test_label
            // 
            this.mysql_test_label.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mysql_test_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mysql_test_label.ForeColor = System.Drawing.SystemColors.ControlText;
            this.mysql_test_label.Location = new System.Drawing.Point(99, 185);
            this.mysql_test_label.Name = "mysql_test_label";
            this.mysql_test_label.Size = new System.Drawing.Size(282, 47);
            this.mysql_test_label.TabIndex = 11;
            this.mysql_test_label.Text = "configure me ...";
            // 
            // mysql_test_button
            // 
            this.mysql_test_button.Image = ((System.Drawing.Image)(resources.GetObject("mysql_test_button.Image")));
            this.mysql_test_button.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.mysql_test_button.Location = new System.Drawing.Point(99, 154);
            this.mysql_test_button.Name = "mysql_test_button";
            this.mysql_test_button.Size = new System.Drawing.Size(109, 23);
            this.mysql_test_button.TabIndex = 5;
            this.mysql_test_button.Text = "Test connection";
            this.mysql_test_button.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.mysql_test_button.UseVisualStyleBackColor = true;
            this.mysql_test_button.MouseLeave += new System.EventHandler(this.reset_mouse_enter_toolstrip_values);
            this.mysql_test_button.Click += new System.EventHandler(this.mysql_test_button_Click);
            this.mysql_test_button.MouseEnter += new System.EventHandler(this.mysql_test_button_MouseEnter);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(6, 131);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(56, 13);
            this.label14.TabIndex = 8;
            this.label14.Text = "Password:";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(6, 105);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(58, 13);
            this.label13.TabIndex = 6;
            this.label13.Text = "Username:";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(6, 79);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(87, 13);
            this.label12.TabIndex = 4;
            this.label12.Text = "Database Name:";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 53);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(29, 13);
            this.label11.TabIndex = 2;
            this.label11.Text = "Port:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(6, 27);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(82, 13);
            this.label10.TabIndex = 0;
            this.label10.Text = "Server Address:";
            // 
            // tab_extra
            // 
            this.tab_extra.Controls.Add(this.edit_property_button);
            this.tab_extra.Controls.Add(this.delete_property_button);
            this.tab_extra.Controls.Add(this.add_property_button);
            this.tab_extra.Controls.Add(this.extra_options_datagrid);
            this.tab_extra.Location = new System.Drawing.Point(4, 22);
            this.tab_extra.Name = "tab_extra";
            this.tab_extra.Size = new System.Drawing.Size(402, 420);
            this.tab_extra.TabIndex = 2;
            this.tab_extra.Text = "Extra properties";
            this.tab_extra.UseVisualStyleBackColor = true;
            // 
            // edit_property_button
            // 
            this.edit_property_button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.edit_property_button.Image = ((System.Drawing.Image)(resources.GetObject("edit_property_button.Image")));
            this.edit_property_button.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.edit_property_button.Location = new System.Drawing.Point(210, 394);
            this.edit_property_button.Name = "edit_property_button";
            this.edit_property_button.Size = new System.Drawing.Size(91, 23);
            this.edit_property_button.TabIndex = 1;
            this.edit_property_button.Text = "Edit property";
            this.edit_property_button.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.edit_property_button.UseVisualStyleBackColor = true;
            this.edit_property_button.MouseLeave += new System.EventHandler(this.reset_mouse_enter_toolstrip_values);
            this.edit_property_button.Click += new System.EventHandler(this.edit_property_button_Click);
            this.edit_property_button.MouseEnter += new System.EventHandler(this.edit_property_button_MouseEnter);
            // 
            // delete_property_button
            // 
            this.delete_property_button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.delete_property_button.Image = ((System.Drawing.Image)(resources.GetObject("delete_property_button.Image")));
            this.delete_property_button.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.delete_property_button.Location = new System.Drawing.Point(3, 394);
            this.delete_property_button.Name = "delete_property_button";
            this.delete_property_button.Size = new System.Drawing.Size(101, 23);
            this.delete_property_button.TabIndex = 0;
            this.delete_property_button.Text = "Delete property";
            this.delete_property_button.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.delete_property_button.UseVisualStyleBackColor = true;
            this.delete_property_button.MouseLeave += new System.EventHandler(this.reset_mouse_enter_toolstrip_values);
            this.delete_property_button.Click += new System.EventHandler(this.delete_property_button_Click);
            this.delete_property_button.MouseEnter += new System.EventHandler(this.delete_property_button_MouseEnter);
            // 
            // add_property_button
            // 
            this.add_property_button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.add_property_button.Image = ((System.Drawing.Image)(resources.GetObject("add_property_button.Image")));
            this.add_property_button.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.add_property_button.Location = new System.Drawing.Point(307, 394);
            this.add_property_button.Name = "add_property_button";
            this.add_property_button.Size = new System.Drawing.Size(92, 23);
            this.add_property_button.TabIndex = 2;
            this.add_property_button.Text = "Add property";
            this.add_property_button.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.add_property_button.UseVisualStyleBackColor = true;
            this.add_property_button.MouseLeave += new System.EventHandler(this.reset_mouse_enter_toolstrip_values);
            this.add_property_button.Click += new System.EventHandler(this.add_property_button_Click);
            this.add_property_button.MouseEnter += new System.EventHandler(this.add_property_button_MouseEnter);
            // 
            // extra_options_datagrid
            // 
            this.extra_options_datagrid.AllowUserToAddRows = false;
            this.extra_options_datagrid.AllowUserToDeleteRows = false;
            this.extra_options_datagrid.AllowUserToResizeRows = false;
            this.extra_options_datagrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.extra_options_datagrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.extra_options_datagrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.extra_options_datagrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.property,
            this.value,
            this.description,
            this.type});
            this.extra_options_datagrid.DataMember = "Server";
            this.extra_options_datagrid.Location = new System.Drawing.Point(3, 3);
            this.extra_options_datagrid.MultiSelect = false;
            this.extra_options_datagrid.Name = "extra_options_datagrid";
            this.extra_options_datagrid.ReadOnly = true;
            this.extra_options_datagrid.RowHeadersVisible = false;
            this.extra_options_datagrid.RowHeadersWidth = 30;
            this.extra_options_datagrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.extra_options_datagrid.ShowEditingIcon = false;
            this.extra_options_datagrid.Size = new System.Drawing.Size(396, 385);
            this.extra_options_datagrid.TabIndex = 3;
            this.extra_options_datagrid.VirtualMode = true;
            this.extra_options_datagrid.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.extra_options_datagrid_CellMouseDoubleClick);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolstrip_status_label});
            this.statusStrip1.Location = new System.Drawing.Point(0, 490);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(434, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolstrip_status_label
            // 
            this.toolstrip_status_label.Name = "toolstrip_status_label";
            this.toolstrip_status_label.Size = new System.Drawing.Size(181, 17);
            this.toolstrip_status_label.Text = "Ready to configurate your server.";
            // 
            // xml_database_path_select_dialog
            // 
            this.xml_database_path_select_dialog.Description = "XML Database path";
            this.xml_database_path_select_dialog.SelectedPath = "C:\\DOL\\DOLSharp";
            // 
            // mysql_test_background_worker
            // 
            this.mysql_test_background_worker.WorkerReportsProgress = true;
            this.mysql_test_background_worker.WorkerSupportsCancellation = true;
            this.mysql_test_background_worker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.mysql_test_background_worker_DoWork);
            this.mysql_test_background_worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.mysql_test_background_worker_RunWorkerCompleted);
            // 
            // wrong_data_error_handler
            // 
            this.wrong_data_error_handler.ContainerControl = this;
            // 
            // save_config_button
            // 
            this.save_config_button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.save_config_button.Image = ((System.Drawing.Image)(resources.GetObject("save_config_button.Image")));
            this.save_config_button.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.save_config_button.Location = new System.Drawing.Point(301, 464);
            this.save_config_button.Name = "save_config_button";
            this.save_config_button.Size = new System.Drawing.Size(121, 23);
            this.save_config_button.TabIndex = 0;
            this.save_config_button.Text = "Save configuration";
            this.save_config_button.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.save_config_button.UseVisualStyleBackColor = true;
            this.save_config_button.MouseLeave += new System.EventHandler(this.reset_mouse_enter_toolstrip_values);
            this.save_config_button.Click += new System.EventHandler(this.save_config_button_Click);
            this.save_config_button.MouseEnter += new System.EventHandler(this.save_config_button_MouseEnter);
            // 
            // set_default_values_button
            // 
            this.set_default_values_button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.set_default_values_button.Image = ((System.Drawing.Image)(resources.GetObject("set_default_values_button.Image")));
            this.set_default_values_button.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.set_default_values_button.Location = new System.Drawing.Point(12, 464);
            this.set_default_values_button.Name = "set_default_values_button";
            this.set_default_values_button.Size = new System.Drawing.Size(118, 23);
            this.set_default_values_button.TabIndex = 3;
            this.set_default_values_button.Text = "Reset all to default";
            this.set_default_values_button.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.set_default_values_button.UseVisualStyleBackColor = true;
            this.set_default_values_button.MouseLeave += new System.EventHandler(this.reset_mouse_enter_toolstrip_values);
            this.set_default_values_button.Click += new System.EventHandler(this.set_default_values_button_Click);
            this.set_default_values_button.MouseEnter += new System.EventHandler(this.set_default_values_button_MouseEnter);
            // 
            // toolstripTimer
            // 
            this.toolstripTimer.Interval = 3000;
            this.toolstripTimer.Tick += new System.EventHandler(this.toolstripTimer_Tick);
            // 
            // property
            // 
            this.property.DataPropertyName = "property";
            this.property.FillWeight = 30F;
            this.property.HeaderText = "Property";
            this.property.Name = "property";
            this.property.ReadOnly = true;
            // 
            // value
            // 
            this.value.DataPropertyName = "value";
            this.value.FillWeight = 35F;
            this.value.HeaderText = "Value";
            this.value.Name = "value";
            this.value.ReadOnly = true;
            // 
            // description
            // 
            this.description.DataPropertyName = "description";
            this.description.FillWeight = 35F;
            this.description.HeaderText = "Description";
            this.description.Name = "description";
            this.description.ReadOnly = true;
            // 
            // type
            // 
            this.type.DataPropertyName = "type";
            this.type.HeaderText = "type";
            this.type.Name = "type";
            this.type.ReadOnly = true;
            this.type.Visible = false;
            // 
            // region_port_textbox
            // 
            this.region_port_textbox.Location = new System.Drawing.Point(112, 150);
            this.region_port_textbox.Name = "region_port_textbox";
            this.region_port_textbox.Regular_Expression = "^[0-9]+$";
            this.region_port_textbox.Size = new System.Drawing.Size(61, 20);
            this.region_port_textbox.TabIndex = 5;
            // 
            // region_ip_textbox
            // 
            this.region_ip_textbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.region_ip_textbox.Location = new System.Drawing.Point(112, 124);
            this.region_ip_textbox.Name = "region_ip_textbox";
            this.region_ip_textbox.Regular_Expression = "^[0-9\\.]+$";
            this.region_ip_textbox.Size = new System.Drawing.Size(251, 20);
            this.region_ip_textbox.TabIndex = 4;
            // 
            // udp_port_textbox
            // 
            this.udp_port_textbox.Location = new System.Drawing.Point(112, 75);
            this.udp_port_textbox.Name = "udp_port_textbox";
            this.udp_port_textbox.Regular_Expression = "^[0-9]+$";
            this.udp_port_textbox.Size = new System.Drawing.Size(61, 20);
            this.udp_port_textbox.TabIndex = 2;
            // 
            // port_textbox
            // 
            this.port_textbox.Location = new System.Drawing.Point(112, 49);
            this.port_textbox.Name = "port_textbox";
            this.port_textbox.Regular_Expression = "^[0-9]+$";
            this.port_textbox.Size = new System.Drawing.Size(61, 20);
            this.port_textbox.TabIndex = 1;
            // 
            // ip_textbox
            // 
            this.ip_textbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ip_textbox.Location = new System.Drawing.Point(112, 23);
            this.ip_textbox.Name = "ip_textbox";
            this.ip_textbox.Regular_Expression = "^[0-9\\.]+$";
            this.ip_textbox.Size = new System.Drawing.Size(251, 20);
            this.ip_textbox.TabIndex = 0;
            // 
            // database_autosave_interval_textbox
            // 
            this.database_autosave_interval_textbox.Location = new System.Drawing.Point(102, 76);
            this.database_autosave_interval_textbox.Name = "database_autosave_interval_textbox";
            this.database_autosave_interval_textbox.Regular_Expression = "^[0-9]+$";
            this.database_autosave_interval_textbox.Size = new System.Drawing.Size(61, 20);
            this.database_autosave_interval_textbox.TabIndex = 2;
            this.database_autosave_interval_textbox.MouseLeave += new System.EventHandler(this.reset_mouse_enter_toolstrip_values);
            this.database_autosave_interval_textbox.MouseEnter += new System.EventHandler(this.database_autosave_interval_textbox_MouseEnter);
            // 
            // mysql_database_name_textbox
            // 
            this.mysql_database_name_textbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mysql_database_name_textbox.Location = new System.Drawing.Point(102, 76);
            this.mysql_database_name_textbox.Name = "mysql_database_name_textbox";
            this.mysql_database_name_textbox.Regular_Expression = "^[0-9a-zA-Z_-]+$";
            this.mysql_database_name_textbox.Size = new System.Drawing.Size(260, 20);
            this.mysql_database_name_textbox.TabIndex = 2;
            this.mysql_database_name_textbox.Text = "dol";
            this.mysql_database_name_textbox.MouseLeave += new System.EventHandler(this.reset_mouse_enter_toolstrip_values);
            this.mysql_database_name_textbox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.mysql_textbox_KeyPress);
            this.mysql_database_name_textbox.MouseEnter += new System.EventHandler(this.mysql_database_name_textbox_MouseEnter);
            // 
            // mysql_username_textbox
            // 
            this.mysql_username_textbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mysql_username_textbox.Location = new System.Drawing.Point(102, 102);
            this.mysql_username_textbox.Name = "mysql_username_textbox";
            this.mysql_username_textbox.Regular_Expression = "^[0-9a-zA-Z_-]+$";
            this.mysql_username_textbox.Size = new System.Drawing.Size(260, 20);
            this.mysql_username_textbox.TabIndex = 3;
            this.mysql_username_textbox.Text = "dol";
            this.mysql_username_textbox.MouseLeave += new System.EventHandler(this.reset_mouse_enter_toolstrip_values);
            this.mysql_username_textbox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.mysql_textbox_KeyPress);
            this.mysql_username_textbox.MouseEnter += new System.EventHandler(this.mysql_username_textbox_MouseEnter);
            // 
            // mysql_port_textbox
            // 
            this.mysql_port_textbox.Location = new System.Drawing.Point(102, 50);
            this.mysql_port_textbox.Name = "mysql_port_textbox";
            this.mysql_port_textbox.Regular_Expression = "^[0-9]+$";
            this.mysql_port_textbox.Size = new System.Drawing.Size(61, 20);
            this.mysql_port_textbox.TabIndex = 1;
            this.mysql_port_textbox.Text = "3306";
            this.mysql_port_textbox.MouseLeave += new System.EventHandler(this.reset_mouse_enter_toolstrip_values);
            this.mysql_port_textbox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.mysql_textbox_KeyPress);
            this.mysql_port_textbox.MouseEnter += new System.EventHandler(this.mysql_port_textbox_MouseEnter);
            // 
            // mysql_host_textbox
            // 
            this.mysql_host_textbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mysql_host_textbox.Location = new System.Drawing.Point(102, 24);
            this.mysql_host_textbox.Name = "mysql_host_textbox";
            this.mysql_host_textbox.Regular_Expression = "^[\\S]+$";
            this.mysql_host_textbox.Size = new System.Drawing.Size(260, 20);
            this.mysql_host_textbox.TabIndex = 0;
            this.mysql_host_textbox.Text = "localhost";
            this.mysql_host_textbox.MouseLeave += new System.EventHandler(this.reset_mouse_enter_toolstrip_values);
            this.mysql_host_textbox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.mysql_textbox_KeyPress);
            this.mysql_host_textbox.MouseEnter += new System.EventHandler(this.mysql_host_textbox_MouseEnter);
            // 
            // DolConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(434, 512);
            this.Controls.Add(this.set_default_values_button);
            this.Controls.Add(this.save_config_button);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.tabControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(450, 550);
            this.Name = "DolConfig";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Dawn of Light Configuration Service";
            this.Load += new System.EventHandler(this.DOLConfig_Load);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.database_tab.ResumeLayout(false);
            this.general_tab.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.xml_groupbox.ResumeLayout(false);
            this.xml_groupbox.PerformLayout();
            this.mysql_groupbox.ResumeLayout(false);
            this.mysql_groupbox.PerformLayout();
            this.tab_extra.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.extra_options_datagrid)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.wrong_data_error_handler)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage database_tab;
        private System.Windows.Forms.TabPage general_tab;
        private System.Windows.Forms.TextBox full_server_name_textbox;
        private System.Windows.Forms.ComboBox game_type_selectbox;
        private System.Windows.Forms.TextBox short_server_name_textbox;
        private System.Windows.Forms.CheckBox auto_account_creation_checkbox;
        private System.Windows.Forms.CheckBox detect_region_ip_checkbox;
        private System.Windows.Forms.ComboBox database_type_selectbox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.GroupBox xml_groupbox;
        private System.Windows.Forms.GroupBox mysql_groupbox;
        private System.Windows.Forms.TabPage tab_extra;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Button xml_database_path_button;
        private System.Windows.Forms.TextBox xml_path_textbox;
        private System.Windows.Forms.FolderBrowserDialog xml_database_path_select_dialog;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.CheckBox database_autosave_checkbox;
        private System.Windows.Forms.Label mysql_test_label;
        private System.Windows.Forms.Button mysql_test_button;

        private RegExControls.RegExTextBox region_port_textbox;
        private RegExControls.RegExTextBox region_ip_textbox;
        private RegExControls.RegExTextBox udp_port_textbox;
        private RegExControls.RegExTextBox port_textbox;
        private RegExControls.RegExTextBox ip_textbox;
        private RegExControls.RegExTextBox mysql_username_textbox;
        private RegExControls.RegExTextBox mysql_port_textbox;
        private RegExControls.RegExTextBox mysql_host_textbox;
        private RegExControls.RegExTextBox mysql_database_name_textbox;
        private RegExControls.RegExTextBox database_autosave_interval_textbox;
        private System.Windows.Forms.TextBox mysql_password_textbox;
        private System.Windows.Forms.Label mysql_says_label;
        private System.Windows.Forms.ProgressBar mysql_test_progressbar;
        private System.ComponentModel.BackgroundWorker mysql_test_background_worker;
        private System.Windows.Forms.ErrorProvider wrong_data_error_handler;
        private System.Windows.Forms.Button save_config_button;
        private System.Windows.Forms.Button set_default_values_button;
        private System.Windows.Forms.DataGridView extra_options_datagrid;
        private System.Windows.Forms.Button edit_property_button;
        private System.Windows.Forms.Button delete_property_button;
        private System.Windows.Forms.Button add_property_button;
        private System.Windows.Forms.ToolStripStatusLabel toolstrip_status_label;
        private System.Windows.Forms.Timer toolstripTimer;
        private System.Windows.Forms.DataGridViewTextBoxColumn property;
        private System.Windows.Forms.DataGridViewTextBoxColumn value;
        private System.Windows.Forms.DataGridViewTextBoxColumn description;
        private System.Windows.Forms.DataGridViewTextBoxColumn type;
    }
}

