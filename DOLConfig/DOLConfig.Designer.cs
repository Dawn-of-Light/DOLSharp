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
        	System.Windows.Forms.GroupBox groupBox4;
        	System.Windows.Forms.Label label18;
        	System.Windows.Forms.Label label19;
        	System.Windows.Forms.Label label20;
        	System.Windows.Forms.Label label21;
        	System.Windows.Forms.Label label22;
        	System.Windows.Forms.GroupBox groupBox5;
        	System.Windows.Forms.Label label23;
        	System.Windows.Forms.Label label24;
        	System.Windows.Forms.Label label25;
        	System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DolConfig));
        	this.auto_account_creation_checkbox = new System.Windows.Forms.CheckBox();
        	this.game_type_selectbox = new System.Windows.Forms.ComboBox();
        	this.short_server_name_textbox = new System.Windows.Forms.TextBox();
        	this.full_server_name_textbox = new System.Windows.Forms.TextBox();
        	this.region_port_textbox = new RegExControls.RegExTextBox();
        	this.region_ip_textbox = new RegExControls.RegExTextBox();
        	this.udp_port_textbox = new RegExControls.RegExTextBox();
        	this.port_textbox = new RegExControls.RegExTextBox();
        	this.ip_textbox = new RegExControls.RegExTextBox();
        	this.detect_region_ip_checkbox = new System.Windows.Forms.CheckBox();
        	this.regExTextBox1 = new RegExControls.RegExTextBox();
        	this.regExTextBox2 = new RegExControls.RegExTextBox();
        	this.regExTextBox3 = new RegExControls.RegExTextBox();
        	this.regExTextBox4 = new RegExControls.RegExTextBox();
        	this.regExTextBox5 = new RegExControls.RegExTextBox();
        	this.checkBox1 = new System.Windows.Forms.CheckBox();
        	this.checkBox2 = new System.Windows.Forms.CheckBox();
        	this.comboBox1 = new System.Windows.Forms.ComboBox();
        	this.textBox1 = new System.Windows.Forms.TextBox();
        	this.textBox2 = new System.Windows.Forms.TextBox();
        	this.tabControl1 = new System.Windows.Forms.TabControl();
        	this.database_tab = new System.Windows.Forms.TabPage();
        	this.general_tab = new System.Windows.Forms.TabPage();
        	this.groupBox3 = new System.Windows.Forms.GroupBox();
        	this.database_autosave_interval_textbox = new RegExControls.RegExTextBox();
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
        	this.mysql_database_name_textbox = new RegExControls.RegExTextBox();
        	this.mysql_username_textbox = new RegExControls.RegExTextBox();
        	this.mysql_port_textbox = new RegExControls.RegExTextBox();
        	this.mysql_host_textbox = new RegExControls.RegExTextBox();
        	this.mysql_test_label = new System.Windows.Forms.Label();
        	this.mysql_test_button = new System.Windows.Forms.Button();
        	this.label14 = new System.Windows.Forms.Label();
        	this.label13 = new System.Windows.Forms.Label();
        	this.label12 = new System.Windows.Forms.Label();
        	this.label11 = new System.Windows.Forms.Label();
        	this.label10 = new System.Windows.Forms.Label();
        	this.sp_tab = new System.Windows.Forms.TabPage();
        	this.cb_spType = new System.Windows.Forms.ComboBox();
        	this.label37 = new System.Windows.Forms.Label();
        	this.tb_spValue = new System.Windows.Forms.TextBox();
        	this.label38 = new System.Windows.Forms.Label();
        	this.lbl_spName = new System.Windows.Forms.Label();
        	this.tv_spShow = new System.Windows.Forms.TreeView();
        	this.tab_extra = new System.Windows.Forms.TabPage();
        	this.edit_property_button = new System.Windows.Forms.Button();
        	this.delete_property_button = new System.Windows.Forms.Button();
        	this.add_property_button = new System.Windows.Forms.Button();
        	this.extra_options_datagrid = new System.Windows.Forms.DataGridView();
        	this.property = new System.Windows.Forms.DataGridViewTextBoxColumn();
        	this.value = new System.Windows.Forms.DataGridViewTextBoxColumn();
        	this.description = new System.Windows.Forms.DataGridViewTextBoxColumn();
        	this.type = new System.Windows.Forms.DataGridViewTextBoxColumn();
        	this.statusStrip1 = new System.Windows.Forms.StatusStrip();
        	this.toolstrip_status_label = new System.Windows.Forms.ToolStripStatusLabel();
        	this.xml_database_path_select_dialog = new System.Windows.Forms.FolderBrowserDialog();
        	this.mysql_test_background_worker = new System.ComponentModel.BackgroundWorker();
        	this.wrong_data_error_handler = new System.Windows.Forms.ErrorProvider(this.components);
        	this.save_config_button = new System.Windows.Forms.Button();
        	this.set_default_values_button = new System.Windows.Forms.Button();
        	this.toolstripTimer = new System.Windows.Forms.Timer(this.components);
        	this.tabPage1 = new System.Windows.Forms.TabPage();
        	this.tabPage2 = new System.Windows.Forms.TabPage();
        	this.groupBox6 = new System.Windows.Forms.GroupBox();
        	this.regExTextBox6 = new RegExControls.RegExTextBox();
        	this.label26 = new System.Windows.Forms.Label();
        	this.label27 = new System.Windows.Forms.Label();
        	this.checkBox3 = new System.Windows.Forms.CheckBox();
        	this.label28 = new System.Windows.Forms.Label();
        	this.comboBox2 = new System.Windows.Forms.ComboBox();
        	this.groupBox7 = new System.Windows.Forms.GroupBox();
        	this.button1 = new System.Windows.Forms.Button();
        	this.textBox3 = new System.Windows.Forms.TextBox();
        	this.label29 = new System.Windows.Forms.Label();
        	this.groupBox8 = new System.Windows.Forms.GroupBox();
        	this.progressBar1 = new System.Windows.Forms.ProgressBar();
        	this.label30 = new System.Windows.Forms.Label();
        	this.textBox4 = new System.Windows.Forms.TextBox();
        	this.regExTextBox7 = new RegExControls.RegExTextBox();
        	this.regExTextBox8 = new RegExControls.RegExTextBox();
        	this.regExTextBox9 = new RegExControls.RegExTextBox();
        	this.regExTextBox10 = new RegExControls.RegExTextBox();
        	this.label31 = new System.Windows.Forms.Label();
        	this.button2 = new System.Windows.Forms.Button();
        	this.label32 = new System.Windows.Forms.Label();
        	this.label33 = new System.Windows.Forms.Label();
        	this.label34 = new System.Windows.Forms.Label();
        	this.label35 = new System.Windows.Forms.Label();
        	this.label36 = new System.Windows.Forms.Label();
        	this.tabPage3 = new System.Windows.Forms.TabPage();
        	this.button3 = new System.Windows.Forms.Button();
        	this.button4 = new System.Windows.Forms.Button();
        	this.button5 = new System.Windows.Forms.Button();
        	this.dataGridView1 = new System.Windows.Forms.DataGridView();
        	this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
        	this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
        	this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
        	this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
        	this.bu_spChange = new System.Windows.Forms.Button();
        	this.tb_spDesc = new System.Windows.Forms.TextBox();
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
        	groupBox4 = new System.Windows.Forms.GroupBox();
        	label18 = new System.Windows.Forms.Label();
        	label19 = new System.Windows.Forms.Label();
        	label20 = new System.Windows.Forms.Label();
        	label21 = new System.Windows.Forms.Label();
        	label22 = new System.Windows.Forms.Label();
        	groupBox5 = new System.Windows.Forms.GroupBox();
        	label23 = new System.Windows.Forms.Label();
        	label24 = new System.Windows.Forms.Label();
        	label25 = new System.Windows.Forms.Label();
        	groupBox1.SuspendLayout();
        	groupBox2.SuspendLayout();
        	groupBox4.SuspendLayout();
        	groupBox5.SuspendLayout();
        	this.tabControl1.SuspendLayout();
        	this.database_tab.SuspendLayout();
        	this.general_tab.SuspendLayout();
        	this.groupBox3.SuspendLayout();
        	this.xml_groupbox.SuspendLayout();
        	this.mysql_groupbox.SuspendLayout();
        	this.sp_tab.SuspendLayout();
        	this.tab_extra.SuspendLayout();
        	((System.ComponentModel.ISupportInitialize)(this.extra_options_datagrid)).BeginInit();
        	this.statusStrip1.SuspendLayout();
        	((System.ComponentModel.ISupportInitialize)(this.wrong_data_error_handler)).BeginInit();
        	this.tabPage1.SuspendLayout();
        	this.tabPage2.SuspendLayout();
        	this.groupBox6.SuspendLayout();
        	this.groupBox7.SuspendLayout();
        	this.groupBox8.SuspendLayout();
        	this.tabPage3.SuspendLayout();
        	((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
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
        	// groupBox4
        	// 
        	groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	groupBox4.Controls.Add(this.regExTextBox1);
        	groupBox4.Controls.Add(this.regExTextBox2);
        	groupBox4.Controls.Add(this.regExTextBox3);
        	groupBox4.Controls.Add(this.regExTextBox4);
        	groupBox4.Controls.Add(this.regExTextBox5);
        	groupBox4.Controls.Add(label18);
        	groupBox4.Controls.Add(label19);
        	groupBox4.Controls.Add(label20);
        	groupBox4.Controls.Add(label21);
        	groupBox4.Controls.Add(this.checkBox1);
        	groupBox4.Controls.Add(label22);
        	groupBox4.Location = new System.Drawing.Point(6, 142);
        	groupBox4.Name = "groupBox4";
        	groupBox4.Size = new System.Drawing.Size(390, 181);
        	groupBox4.TabIndex = 1;
        	groupBox4.TabStop = false;
        	groupBox4.Text = "Server connection specific (optional)";
        	// 
        	// regExTextBox1
        	// 
        	this.regExTextBox1.Location = new System.Drawing.Point(112, 150);
        	this.regExTextBox1.Name = "regExTextBox1";
        	this.regExTextBox1.Regular_Expression = "^[0-9]+$";
        	this.regExTextBox1.Size = new System.Drawing.Size(61, 20);
        	this.regExTextBox1.TabIndex = 5;
        	// 
        	// regExTextBox2
        	// 
        	this.regExTextBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.regExTextBox2.Location = new System.Drawing.Point(112, 124);
        	this.regExTextBox2.Name = "regExTextBox2";
        	this.regExTextBox2.Regular_Expression = "^[0-9\\.]+$";
        	this.regExTextBox2.Size = new System.Drawing.Size(251, 20);
        	this.regExTextBox2.TabIndex = 4;
        	// 
        	// regExTextBox3
        	// 
        	this.regExTextBox3.Location = new System.Drawing.Point(112, 75);
        	this.regExTextBox3.Name = "regExTextBox3";
        	this.regExTextBox3.Regular_Expression = "^[0-9]+$";
        	this.regExTextBox3.Size = new System.Drawing.Size(61, 20);
        	this.regExTextBox3.TabIndex = 2;
        	// 
        	// regExTextBox4
        	// 
        	this.regExTextBox4.Location = new System.Drawing.Point(112, 49);
        	this.regExTextBox4.Name = "regExTextBox4";
        	this.regExTextBox4.Regular_Expression = "^[0-9]+$";
        	this.regExTextBox4.Size = new System.Drawing.Size(61, 20);
        	this.regExTextBox4.TabIndex = 1;
        	// 
        	// regExTextBox5
        	// 
        	this.regExTextBox5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.regExTextBox5.Location = new System.Drawing.Point(112, 23);
        	this.regExTextBox5.Name = "regExTextBox5";
        	this.regExTextBox5.Regular_Expression = "^[0-9\\.]+$";
        	this.regExTextBox5.Size = new System.Drawing.Size(251, 20);
        	this.regExTextBox5.TabIndex = 0;
        	// 
        	// label18
        	// 
        	label18.AutoSize = true;
        	label18.Location = new System.Drawing.Point(6, 78);
        	label18.Name = "label18";
        	label18.Size = new System.Drawing.Size(55, 13);
        	label18.TabIndex = 16;
        	label18.Text = "UDP Port:";
        	// 
        	// label19
        	// 
        	label19.AutoSize = true;
        	label19.Location = new System.Drawing.Point(6, 153);
        	label19.Name = "label19";
        	label19.Size = new System.Drawing.Size(66, 13);
        	label19.TabIndex = 14;
        	label19.Text = "Region Port:";
        	// 
        	// label20
        	// 
        	label20.AutoSize = true;
        	label20.Location = new System.Drawing.Point(6, 127);
        	label20.Name = "label20";
        	label20.Size = new System.Drawing.Size(57, 13);
        	label20.TabIndex = 12;
        	label20.Text = "Region IP:";
        	// 
        	// label21
        	// 
        	label21.AutoSize = true;
        	label21.Location = new System.Drawing.Point(6, 26);
        	label21.Name = "label21";
        	label21.Size = new System.Drawing.Size(20, 13);
        	label21.TabIndex = 10;
        	label21.Text = "IP:";
        	// 
        	// checkBox1
        	// 
        	this.checkBox1.AutoSize = true;
        	this.checkBox1.Location = new System.Drawing.Point(112, 101);
        	this.checkBox1.Name = "checkBox1";
        	this.checkBox1.Size = new System.Drawing.Size(113, 17);
        	this.checkBox1.TabIndex = 3;
        	this.checkBox1.Text = "Detect Region IPs";
        	this.checkBox1.UseVisualStyleBackColor = true;
        	// 
        	// label22
        	// 
        	label22.AutoSize = true;
        	label22.Location = new System.Drawing.Point(6, 52);
        	label22.Name = "label22";
        	label22.Size = new System.Drawing.Size(53, 13);
        	label22.TabIndex = 8;
        	label22.Text = "TCP Port:";
        	// 
        	// groupBox5
        	// 
        	groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	groupBox5.Controls.Add(this.checkBox2);
        	groupBox5.Controls.Add(this.comboBox1);
        	groupBox5.Controls.Add(this.textBox1);
        	groupBox5.Controls.Add(label23);
        	groupBox5.Controls.Add(label24);
        	groupBox5.Controls.Add(this.textBox2);
        	groupBox5.Controls.Add(label25);
        	groupBox5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	groupBox5.Location = new System.Drawing.Point(6, 6);
        	groupBox5.Name = "groupBox5";
        	groupBox5.Size = new System.Drawing.Size(390, 130);
        	groupBox5.TabIndex = 0;
        	groupBox5.TabStop = false;
        	groupBox5.Text = "Gameserver specific (required)";
        	// 
        	// checkBox2
        	// 
        	this.checkBox2.AutoSize = true;
        	this.checkBox2.Location = new System.Drawing.Point(112, 103);
        	this.checkBox2.Name = "checkBox2";
        	this.checkBox2.Size = new System.Drawing.Size(132, 17);
        	this.checkBox2.TabIndex = 3;
        	this.checkBox2.Text = "Auto Account creation";
        	this.checkBox2.UseVisualStyleBackColor = true;
        	// 
        	// comboBox1
        	// 
        	this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        	this.comboBox1.FormattingEnabled = true;
        	this.comboBox1.Items.AddRange(new object[] {
        	        	        	"Normal",
        	        	        	"PvP",
        	        	        	"PvE",
        	        	        	"Casual",
        	        	        	"Roleplay",
        	        	        	"Test"});
        	this.comboBox1.Location = new System.Drawing.Point(112, 76);
        	this.comboBox1.Name = "comboBox1";
        	this.comboBox1.Size = new System.Drawing.Size(196, 21);
        	this.comboBox1.TabIndex = 2;
        	// 
        	// textBox1
        	// 
        	this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.textBox1.Location = new System.Drawing.Point(112, 50);
        	this.textBox1.Name = "textBox1";
        	this.textBox1.Size = new System.Drawing.Size(251, 20);
        	this.textBox1.TabIndex = 1;
        	// 
        	// label23
        	// 
        	label23.AutoSize = true;
        	label23.Location = new System.Drawing.Point(6, 53);
        	label23.Name = "label23";
        	label23.Size = new System.Drawing.Size(100, 13);
        	label23.TabIndex = 3;
        	label23.Text = "Short Server Name:";
        	// 
        	// label24
        	// 
        	label24.AutoSize = true;
        	label24.Location = new System.Drawing.Point(6, 79);
        	label24.Name = "label24";
        	label24.Size = new System.Drawing.Size(65, 13);
        	label24.TabIndex = 2;
        	label24.Text = "Game Type:";
        	// 
        	// textBox2
        	// 
        	this.textBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.textBox2.Location = new System.Drawing.Point(112, 24);
        	this.textBox2.Name = "textBox2";
        	this.textBox2.Size = new System.Drawing.Size(251, 20);
        	this.textBox2.TabIndex = 0;
        	// 
        	// label25
        	// 
        	label25.AutoSize = true;
        	label25.Location = new System.Drawing.Point(6, 27);
        	label25.Name = "label25";
        	label25.Size = new System.Drawing.Size(91, 13);
        	label25.TabIndex = 0;
        	label25.Text = "Full Server Name:";
        	// 
        	// tabControl1
        	// 
        	this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        	        	        	| System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.tabControl1.Controls.Add(this.database_tab);
        	this.tabControl1.Controls.Add(this.general_tab);
        	this.tabControl1.Controls.Add(this.tab_extra);
        	this.tabControl1.Controls.Add(this.sp_tab);
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
        	// sp_tab
        	// 
        	this.sp_tab.Controls.Add(this.tb_spDesc);
        	this.sp_tab.Controls.Add(this.bu_spChange);
        	this.sp_tab.Controls.Add(this.cb_spType);
        	this.sp_tab.Controls.Add(this.label37);
        	this.sp_tab.Controls.Add(this.tb_spValue);
        	this.sp_tab.Controls.Add(this.label38);
        	this.sp_tab.Controls.Add(this.lbl_spName);
        	this.sp_tab.Controls.Add(this.tv_spShow);
        	this.sp_tab.Location = new System.Drawing.Point(4, 22);
        	this.sp_tab.Name = "sp_tab";
        	this.sp_tab.Size = new System.Drawing.Size(402, 420);
        	this.sp_tab.TabIndex = 4;
        	this.sp_tab.Text = "Server properties";
        	this.sp_tab.UseVisualStyleBackColor = true;
        	// 
        	// cb_spType
        	// 
        	this.cb_spType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        	this.cb_spType.FormattingEnabled = true;
        	this.cb_spType.Items.AddRange(new object[] {
        	        	        	"string",
        	        	        	"integer",
        	        	        	"boolean"});
        	this.cb_spType.Location = new System.Drawing.Point(310, 351);
        	this.cb_spType.Name = "cb_spType";
        	this.cb_spType.Size = new System.Drawing.Size(88, 21);
        	this.cb_spType.TabIndex = 15;
        	// 
        	// label37
        	// 
        	this.label37.AutoSize = true;
        	this.label37.Location = new System.Drawing.Point(310, 335);
        	this.label37.Name = "label37";
        	this.label37.Size = new System.Drawing.Size(60, 13);
        	this.label37.TabIndex = 14;
        	this.label37.Text = "Value type:";
        	// 
        	// tb_spValue
        	// 
        	this.tb_spValue.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        	        	        	| System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.tb_spValue.Location = new System.Drawing.Point(3, 351);
        	this.tb_spValue.Multiline = true;
        	this.tb_spValue.Name = "tb_spValue";
        	this.tb_spValue.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        	this.tb_spValue.Size = new System.Drawing.Size(301, 66);
        	this.tb_spValue.TabIndex = 13;
        	// 
        	// label38
        	// 
        	this.label38.AutoSize = true;
        	this.label38.Location = new System.Drawing.Point(3, 335);
        	this.label38.Name = "label38";
        	this.label38.Size = new System.Drawing.Size(37, 13);
        	this.label38.TabIndex = 12;
        	this.label38.Text = "Value:";
        	// 
        	// lbl_spName
        	// 
        	this.lbl_spName.AutoSize = true;
        	this.lbl_spName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.lbl_spName.Location = new System.Drawing.Point(151, 256);
        	this.lbl_spName.Name = "lbl_spName";
        	this.lbl_spName.Size = new System.Drawing.Size(101, 15);
        	this.lbl_spName.TabIndex = 10;
        	this.lbl_spName.Text = "- Property name -";
        	// 
        	// tv_spShow
        	// 
        	this.tv_spShow.Location = new System.Drawing.Point(3, 3);
        	this.tv_spShow.Name = "tv_spShow";
        	this.tv_spShow.Size = new System.Drawing.Size(396, 245);
        	this.tv_spShow.TabIndex = 0;
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
        	// statusStrip1
        	// 
        	this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
        	        	        	this.toolstrip_status_label});
        	this.statusStrip1.Location = new System.Drawing.Point(0, 492);
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
        	// tabPage1
        	// 
        	this.tabPage1.Controls.Add(groupBox4);
        	this.tabPage1.Controls.Add(groupBox5);
        	this.tabPage1.Location = new System.Drawing.Point(4, 22);
        	this.tabPage1.Name = "tabPage1";
        	this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
        	this.tabPage1.Size = new System.Drawing.Size(402, 420);
        	this.tabPage1.TabIndex = 0;
        	this.tabPage1.Text = "General configuration";
        	this.tabPage1.UseVisualStyleBackColor = true;
        	// 
        	// tabPage2
        	// 
        	this.tabPage2.Controls.Add(this.groupBox6);
        	this.tabPage2.Controls.Add(this.groupBox7);
        	this.tabPage2.Controls.Add(this.groupBox8);
        	this.tabPage2.Location = new System.Drawing.Point(4, 22);
        	this.tabPage2.Name = "tabPage2";
        	this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
        	this.tabPage2.Size = new System.Drawing.Size(402, 420);
        	this.tabPage2.TabIndex = 1;
        	this.tabPage2.Text = "Database";
        	this.tabPage2.UseVisualStyleBackColor = true;
        	// 
        	// groupBox6
        	// 
        	this.groupBox6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.groupBox6.Controls.Add(this.regExTextBox6);
        	this.groupBox6.Controls.Add(this.label26);
        	this.groupBox6.Controls.Add(this.label27);
        	this.groupBox6.Controls.Add(this.checkBox3);
        	this.groupBox6.Controls.Add(this.label28);
        	this.groupBox6.Controls.Add(this.comboBox2);
        	this.groupBox6.Location = new System.Drawing.Point(9, 6);
        	this.groupBox6.Name = "groupBox6";
        	this.groupBox6.Size = new System.Drawing.Size(387, 108);
        	this.groupBox6.TabIndex = 4;
        	this.groupBox6.TabStop = false;
        	this.groupBox6.Text = "Database Settings (required)";
        	// 
        	// regExTextBox6
        	// 
        	this.regExTextBox6.Location = new System.Drawing.Point(102, 76);
        	this.regExTextBox6.Name = "regExTextBox6";
        	this.regExTextBox6.Regular_Expression = "^[0-9]+$";
        	this.regExTextBox6.Size = new System.Drawing.Size(61, 20);
        	this.regExTextBox6.TabIndex = 2;
        	// 
        	// label26
        	// 
        	this.label26.AutoSize = true;
        	this.label26.Location = new System.Drawing.Point(178, 79);
        	this.label26.Name = "label26";
        	this.label26.Size = new System.Drawing.Size(43, 13);
        	this.label26.TabIndex = 11;
        	this.label26.Text = "minutes";
        	// 
        	// label27
        	// 
        	this.label27.AutoSize = true;
        	this.label27.Location = new System.Drawing.Point(6, 79);
        	this.label27.Name = "label27";
        	this.label27.Size = new System.Drawing.Size(93, 13);
        	this.label27.TabIndex = 5;
        	this.label27.Text = "Autosave Interval:";
        	// 
        	// checkBox3
        	// 
        	this.checkBox3.AutoSize = true;
        	this.checkBox3.Location = new System.Drawing.Point(102, 53);
        	this.checkBox3.Name = "checkBox3";
        	this.checkBox3.Size = new System.Drawing.Size(120, 17);
        	this.checkBox3.TabIndex = 1;
        	this.checkBox3.Text = "Autosave Database";
        	this.checkBox3.UseVisualStyleBackColor = true;
        	// 
        	// label28
        	// 
        	this.label28.AutoSize = true;
        	this.label28.Location = new System.Drawing.Point(6, 22);
        	this.label28.Name = "label28";
        	this.label28.Size = new System.Drawing.Size(79, 13);
        	this.label28.TabIndex = 2;
        	this.label28.Text = "Database type:";
        	// 
        	// comboBox2
        	// 
        	this.comboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        	this.comboBox2.FormattingEnabled = true;
        	this.comboBox2.Items.AddRange(new object[] {
        	        	        	"MySQL",
        	        	        	"XML"});
        	this.comboBox2.Location = new System.Drawing.Point(102, 19);
        	this.comboBox2.Name = "comboBox2";
        	this.comboBox2.Size = new System.Drawing.Size(121, 21);
        	this.comboBox2.TabIndex = 0;
        	// 
        	// groupBox7
        	// 
        	this.groupBox7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.groupBox7.Controls.Add(this.button1);
        	this.groupBox7.Controls.Add(this.textBox3);
        	this.groupBox7.Controls.Add(this.label29);
        	this.groupBox7.Location = new System.Drawing.Point(9, 361);
        	this.groupBox7.Name = "groupBox7";
        	this.groupBox7.Size = new System.Drawing.Size(387, 56);
        	this.groupBox7.TabIndex = 1;
        	this.groupBox7.TabStop = false;
        	this.groupBox7.Text = "XML Database settings";
        	// 
        	// button1
        	// 
        	this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        	this.button1.Image = ((System.Drawing.Image)(resources.GetObject("button1.Image")));
        	this.button1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
        	this.button1.Location = new System.Drawing.Point(281, 25);
        	this.button1.Name = "button1";
        	this.button1.Size = new System.Drawing.Size(100, 23);
        	this.button1.TabIndex = 1;
        	this.button1.Text = "Select folder ...";
        	this.button1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        	this.button1.UseVisualStyleBackColor = true;
        	// 
        	// textBox3
        	// 
        	this.textBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.textBox3.Location = new System.Drawing.Point(102, 27);
        	this.textBox3.Name = "textBox3";
        	this.textBox3.Size = new System.Drawing.Size(157, 20);
        	this.textBox3.TabIndex = 0;
        	// 
        	// label29
        	// 
        	this.label29.AutoSize = true;
        	this.label29.Location = new System.Drawing.Point(9, 30);
        	this.label29.Name = "label29";
        	this.label29.Size = new System.Drawing.Size(52, 13);
        	this.label29.TabIndex = 10;
        	this.label29.Text = "Directory:";
        	// 
        	// groupBox8
        	// 
        	this.groupBox8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.groupBox8.Controls.Add(this.progressBar1);
        	this.groupBox8.Controls.Add(this.label30);
        	this.groupBox8.Controls.Add(this.textBox4);
        	this.groupBox8.Controls.Add(this.regExTextBox7);
        	this.groupBox8.Controls.Add(this.regExTextBox8);
        	this.groupBox8.Controls.Add(this.regExTextBox9);
        	this.groupBox8.Controls.Add(this.regExTextBox10);
        	this.groupBox8.Controls.Add(this.label31);
        	this.groupBox8.Controls.Add(this.button2);
        	this.groupBox8.Controls.Add(this.label32);
        	this.groupBox8.Controls.Add(this.label33);
        	this.groupBox8.Controls.Add(this.label34);
        	this.groupBox8.Controls.Add(this.label35);
        	this.groupBox8.Controls.Add(this.label36);
        	this.groupBox8.Location = new System.Drawing.Point(9, 120);
        	this.groupBox8.Name = "groupBox8";
        	this.groupBox8.Size = new System.Drawing.Size(387, 235);
        	this.groupBox8.TabIndex = 0;
        	this.groupBox8.TabStop = false;
        	this.groupBox8.Text = "MySQL Database settings";
        	// 
        	// progressBar1
        	// 
        	this.progressBar1.Location = new System.Drawing.Point(217, 158);
        	this.progressBar1.Name = "progressBar1";
        	this.progressBar1.Size = new System.Drawing.Size(97, 15);
        	this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
        	this.progressBar1.TabIndex = 24;
        	this.progressBar1.Visible = false;
        	// 
        	// label30
        	// 
        	this.label30.AutoSize = true;
        	this.label30.Location = new System.Drawing.Point(6, 185);
        	this.label30.Name = "label30";
        	this.label30.Size = new System.Drawing.Size(69, 13);
        	this.label30.TabIndex = 23;
        	this.label30.Text = "MySQL says:";
        	// 
        	// textBox4
        	// 
        	this.textBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.textBox4.Location = new System.Drawing.Point(102, 128);
        	this.textBox4.Name = "textBox4";
        	this.textBox4.PasswordChar = '*';
        	this.textBox4.Size = new System.Drawing.Size(260, 20);
        	this.textBox4.TabIndex = 4;
        	// 
        	// regExTextBox7
        	// 
        	this.regExTextBox7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.regExTextBox7.Location = new System.Drawing.Point(102, 76);
        	this.regExTextBox7.Name = "regExTextBox7";
        	this.regExTextBox7.Regular_Expression = "^[0-9a-zA-Z_-]+$";
        	this.regExTextBox7.Size = new System.Drawing.Size(260, 20);
        	this.regExTextBox7.TabIndex = 2;
        	this.regExTextBox7.Text = "dol";
        	// 
        	// regExTextBox8
        	// 
        	this.regExTextBox8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.regExTextBox8.Location = new System.Drawing.Point(102, 102);
        	this.regExTextBox8.Name = "regExTextBox8";
        	this.regExTextBox8.Regular_Expression = "^[0-9a-zA-Z_-]+$";
        	this.regExTextBox8.Size = new System.Drawing.Size(260, 20);
        	this.regExTextBox8.TabIndex = 3;
        	this.regExTextBox8.Text = "dol";
        	// 
        	// regExTextBox9
        	// 
        	this.regExTextBox9.Location = new System.Drawing.Point(102, 50);
        	this.regExTextBox9.Name = "regExTextBox9";
        	this.regExTextBox9.Regular_Expression = "^[0-9]+$";
        	this.regExTextBox9.Size = new System.Drawing.Size(61, 20);
        	this.regExTextBox9.TabIndex = 1;
        	this.regExTextBox9.Text = "3306";
        	// 
        	// regExTextBox10
        	// 
        	this.regExTextBox10.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.regExTextBox10.Location = new System.Drawing.Point(102, 24);
        	this.regExTextBox10.Name = "regExTextBox10";
        	this.regExTextBox10.Regular_Expression = "^[\\S]+$";
        	this.regExTextBox10.Size = new System.Drawing.Size(260, 20);
        	this.regExTextBox10.TabIndex = 0;
        	this.regExTextBox10.Text = "localhost";
        	// 
        	// label31
        	// 
        	this.label31.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.label31.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.label31.ForeColor = System.Drawing.SystemColors.ControlText;
        	this.label31.Location = new System.Drawing.Point(99, 185);
        	this.label31.Name = "label31";
        	this.label31.Size = new System.Drawing.Size(282, 47);
        	this.label31.TabIndex = 11;
        	this.label31.Text = "configure me ...";
        	// 
        	// button2
        	// 
        	this.button2.Image = ((System.Drawing.Image)(resources.GetObject("button2.Image")));
        	this.button2.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
        	this.button2.Location = new System.Drawing.Point(99, 154);
        	this.button2.Name = "button2";
        	this.button2.Size = new System.Drawing.Size(109, 23);
        	this.button2.TabIndex = 5;
        	this.button2.Text = "Test connection";
        	this.button2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        	this.button2.UseVisualStyleBackColor = true;
        	// 
        	// label32
        	// 
        	this.label32.AutoSize = true;
        	this.label32.Location = new System.Drawing.Point(6, 131);
        	this.label32.Name = "label32";
        	this.label32.Size = new System.Drawing.Size(56, 13);
        	this.label32.TabIndex = 8;
        	this.label32.Text = "Password:";
        	// 
        	// label33
        	// 
        	this.label33.AutoSize = true;
        	this.label33.Location = new System.Drawing.Point(6, 105);
        	this.label33.Name = "label33";
        	this.label33.Size = new System.Drawing.Size(58, 13);
        	this.label33.TabIndex = 6;
        	this.label33.Text = "Username:";
        	// 
        	// label34
        	// 
        	this.label34.AutoSize = true;
        	this.label34.Location = new System.Drawing.Point(6, 79);
        	this.label34.Name = "label34";
        	this.label34.Size = new System.Drawing.Size(87, 13);
        	this.label34.TabIndex = 4;
        	this.label34.Text = "Database Name:";
        	// 
        	// label35
        	// 
        	this.label35.AutoSize = true;
        	this.label35.Location = new System.Drawing.Point(6, 53);
        	this.label35.Name = "label35";
        	this.label35.Size = new System.Drawing.Size(29, 13);
        	this.label35.TabIndex = 2;
        	this.label35.Text = "Port:";
        	// 
        	// label36
        	// 
        	this.label36.AutoSize = true;
        	this.label36.Location = new System.Drawing.Point(6, 27);
        	this.label36.Name = "label36";
        	this.label36.Size = new System.Drawing.Size(82, 13);
        	this.label36.TabIndex = 0;
        	this.label36.Text = "Server Address:";
        	// 
        	// tabPage3
        	// 
        	this.tabPage3.Controls.Add(this.button3);
        	this.tabPage3.Controls.Add(this.button4);
        	this.tabPage3.Controls.Add(this.button5);
        	this.tabPage3.Controls.Add(this.dataGridView1);
        	this.tabPage3.Location = new System.Drawing.Point(4, 22);
        	this.tabPage3.Name = "tabPage3";
        	this.tabPage3.Size = new System.Drawing.Size(402, 420);
        	this.tabPage3.TabIndex = 2;
        	this.tabPage3.Text = "Extra properties";
        	this.tabPage3.UseVisualStyleBackColor = true;
        	// 
        	// button3
        	// 
        	this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        	this.button3.Image = ((System.Drawing.Image)(resources.GetObject("button3.Image")));
        	this.button3.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
        	this.button3.Location = new System.Drawing.Point(210, 394);
        	this.button3.Name = "button3";
        	this.button3.Size = new System.Drawing.Size(91, 23);
        	this.button3.TabIndex = 1;
        	this.button3.Text = "Edit property";
        	this.button3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        	this.button3.UseVisualStyleBackColor = true;
        	// 
        	// button4
        	// 
        	this.button4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        	this.button4.Image = ((System.Drawing.Image)(resources.GetObject("button4.Image")));
        	this.button4.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
        	this.button4.Location = new System.Drawing.Point(3, 394);
        	this.button4.Name = "button4";
        	this.button4.Size = new System.Drawing.Size(101, 23);
        	this.button4.TabIndex = 0;
        	this.button4.Text = "Delete property";
        	this.button4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        	this.button4.UseVisualStyleBackColor = true;
        	// 
        	// button5
        	// 
        	this.button5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        	this.button5.Image = ((System.Drawing.Image)(resources.GetObject("button5.Image")));
        	this.button5.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
        	this.button5.Location = new System.Drawing.Point(307, 394);
        	this.button5.Name = "button5";
        	this.button5.Size = new System.Drawing.Size(92, 23);
        	this.button5.TabIndex = 2;
        	this.button5.Text = "Add property";
        	this.button5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        	this.button5.UseVisualStyleBackColor = true;
        	// 
        	// dataGridView1
        	// 
        	this.dataGridView1.AllowUserToAddRows = false;
        	this.dataGridView1.AllowUserToDeleteRows = false;
        	this.dataGridView1.AllowUserToResizeRows = false;
        	this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        	        	        	| System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        	this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        	this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
        	        	        	this.dataGridViewTextBoxColumn1,
        	        	        	this.dataGridViewTextBoxColumn2,
        	        	        	this.dataGridViewTextBoxColumn3,
        	        	        	this.dataGridViewTextBoxColumn4});
        	this.dataGridView1.DataMember = "Server";
        	this.dataGridView1.Location = new System.Drawing.Point(3, 3);
        	this.dataGridView1.MultiSelect = false;
        	this.dataGridView1.Name = "dataGridView1";
        	this.dataGridView1.ReadOnly = true;
        	this.dataGridView1.RowHeadersVisible = false;
        	this.dataGridView1.RowHeadersWidth = 30;
        	this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        	this.dataGridView1.ShowEditingIcon = false;
        	this.dataGridView1.Size = new System.Drawing.Size(396, 385);
        	this.dataGridView1.TabIndex = 3;
        	this.dataGridView1.VirtualMode = true;
        	// 
        	// dataGridViewTextBoxColumn1
        	// 
        	this.dataGridViewTextBoxColumn1.DataPropertyName = "property";
        	this.dataGridViewTextBoxColumn1.FillWeight = 30F;
        	this.dataGridViewTextBoxColumn1.HeaderText = "Property";
        	this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
        	this.dataGridViewTextBoxColumn1.ReadOnly = true;
        	// 
        	// dataGridViewTextBoxColumn2
        	// 
        	this.dataGridViewTextBoxColumn2.DataPropertyName = "value";
        	this.dataGridViewTextBoxColumn2.FillWeight = 35F;
        	this.dataGridViewTextBoxColumn2.HeaderText = "Value";
        	this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
        	this.dataGridViewTextBoxColumn2.ReadOnly = true;
        	// 
        	// dataGridViewTextBoxColumn3
        	// 
        	this.dataGridViewTextBoxColumn3.DataPropertyName = "description";
        	this.dataGridViewTextBoxColumn3.FillWeight = 35F;
        	this.dataGridViewTextBoxColumn3.HeaderText = "Description";
        	this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
        	this.dataGridViewTextBoxColumn3.ReadOnly = true;
        	// 
        	// dataGridViewTextBoxColumn4
        	// 
        	this.dataGridViewTextBoxColumn4.DataPropertyName = "type";
        	this.dataGridViewTextBoxColumn4.HeaderText = "type";
        	this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
        	this.dataGridViewTextBoxColumn4.ReadOnly = true;
        	this.dataGridViewTextBoxColumn4.Visible = false;
        	// 
        	// bu_spChange
        	// 
        	this.bu_spChange.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        	this.bu_spChange.Image = ((System.Drawing.Image)(resources.GetObject("bu_spChange.Image")));
        	this.bu_spChange.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
        	this.bu_spChange.Location = new System.Drawing.Point(310, 394);
        	this.bu_spChange.Name = "bu_spChange";
        	this.bu_spChange.Size = new System.Drawing.Size(91, 23);
        	this.bu_spChange.TabIndex = 17;
        	this.bu_spChange.Text = "Edit property";
        	this.bu_spChange.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        	this.bu_spChange.UseVisualStyleBackColor = true;
        	// 
        	// tb_spDesc
        	// 
        	this.tb_spDesc.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        	this.tb_spDesc.Location = new System.Drawing.Point(22, 283);
        	this.tb_spDesc.Multiline = true;
        	this.tb_spDesc.Name = "tb_spDesc";
        	this.tb_spDesc.Size = new System.Drawing.Size(359, 42);
        	this.tb_spDesc.TabIndex = 18;
        	// 
        	// DolConfig
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.ClientSize = new System.Drawing.Size(434, 514);
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
        	groupBox4.ResumeLayout(false);
        	groupBox4.PerformLayout();
        	groupBox5.ResumeLayout(false);
        	groupBox5.PerformLayout();
        	this.tabControl1.ResumeLayout(false);
        	this.database_tab.ResumeLayout(false);
        	this.general_tab.ResumeLayout(false);
        	this.groupBox3.ResumeLayout(false);
        	this.groupBox3.PerformLayout();
        	this.xml_groupbox.ResumeLayout(false);
        	this.xml_groupbox.PerformLayout();
        	this.mysql_groupbox.ResumeLayout(false);
        	this.mysql_groupbox.PerformLayout();
        	this.sp_tab.ResumeLayout(false);
        	this.sp_tab.PerformLayout();
        	this.tab_extra.ResumeLayout(false);
        	((System.ComponentModel.ISupportInitialize)(this.extra_options_datagrid)).EndInit();
        	this.statusStrip1.ResumeLayout(false);
        	this.statusStrip1.PerformLayout();
        	((System.ComponentModel.ISupportInitialize)(this.wrong_data_error_handler)).EndInit();
        	this.tabPage1.ResumeLayout(false);
        	this.tabPage2.ResumeLayout(false);
        	this.groupBox6.ResumeLayout(false);
        	this.groupBox6.PerformLayout();
        	this.groupBox7.ResumeLayout(false);
        	this.groupBox7.PerformLayout();
        	this.groupBox8.ResumeLayout(false);
        	this.groupBox8.PerformLayout();
        	this.tabPage3.ResumeLayout(false);
        	((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
        	this.ResumeLayout(false);
        	this.PerformLayout();
        }
        private System.Windows.Forms.TreeView tv_spShow;
        private System.Windows.Forms.ComboBox cb_spType;
        private System.Windows.Forms.TextBox tb_spValue;
        private System.Windows.Forms.Label lbl_spName;
        private System.Windows.Forms.Button bu_spChange;
        private System.Windows.Forms.TextBox tb_spDesc;
        private System.Windows.Forms.Label label38;
        private System.Windows.Forms.Label label37;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Label label36;
        private System.Windows.Forms.Label label35;
        private System.Windows.Forms.Label label34;
        private System.Windows.Forms.Label label33;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label31;
        private RegExControls.RegExTextBox regExTextBox10;
        private RegExControls.RegExTextBox regExTextBox9;
        private RegExControls.RegExTextBox regExTextBox8;
        private RegExControls.RegExTextBox regExTextBox7;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.GroupBox groupBox8;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.Label label26;
        private RegExControls.RegExTextBox regExTextBox6;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox checkBox1;
        private RegExControls.RegExTextBox regExTextBox5;
        private RegExControls.RegExTextBox regExTextBox4;
        private RegExControls.RegExTextBox regExTextBox3;
        private RegExControls.RegExTextBox regExTextBox2;
        private RegExControls.RegExTextBox regExTextBox1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage sp_tab;

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

