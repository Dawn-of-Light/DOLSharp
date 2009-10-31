namespace DOLConfig
{
	partial class Form1
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.autoAccountCreationCheckBox = new System.Windows.Forms.CheckBox();
			this.FullNameTextBox = new System.Windows.Forms.TextBox();
			this.shortNameTextBox = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.serverTypeComboBox = new System.Windows.Forms.ComboBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.mysqlPasswordTextBox = new System.Windows.Forms.TextBox();
			this.mysqlUsernameTextBox = new System.Windows.Forms.TextBox();
			this.mysqlDatabaseTextBox = new System.Windows.Forms.TextBox();
			this.mysqlHostTextBox = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.databaseTypeComboBox = new System.Windows.Forms.ComboBox();
			this.label4 = new System.Windows.Forms.Label();
			this.saveButton = new System.Windows.Forms.Button();
			this.closeButton = new System.Windows.Forms.Button();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
			this.spTvCat = new System.Windows.Forms.TreeView();
			this.spLblKey = new System.Windows.Forms.Label();
			this.spCbValue = new System.Windows.Forms.ComboBox();
			this.button1 = new System.Windows.Forms.Button();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.spLblDefault = new System.Windows.Forms.Label();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.spLblDesc = new System.Windows.Forms.Label();
			this.spSave = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.statusStrip1.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.groupBox5.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.autoAccountCreationCheckBox);
			this.groupBox1.Controls.Add(this.FullNameTextBox);
			this.groupBox1.Controls.Add(this.shortNameTextBox);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.serverTypeComboBox);
			this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.groupBox1.Location = new System.Drawing.Point(12, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(197, 117);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Server Settings";
			// 
			// autoAccountCreationCheckBox
			// 
			this.autoAccountCreationCheckBox.AutoSize = true;
			this.autoAccountCreationCheckBox.Checked = true;
			this.autoAccountCreationCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.autoAccountCreationCheckBox.Location = new System.Drawing.Point(9, 95);
			this.autoAccountCreationCheckBox.Name = "autoAccountCreationCheckBox";
			this.autoAccountCreationCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.autoAccountCreationCheckBox.Size = new System.Drawing.Size(133, 17);
			this.autoAccountCreationCheckBox.TabIndex = 6;
			this.autoAccountCreationCheckBox.Text = "Auto Account Creation";
			this.autoAccountCreationCheckBox.UseVisualStyleBackColor = true;
			this.autoAccountCreationCheckBox.MouseEnter += new System.EventHandler(this.autoAccountCreationCheckBox_MouseEnter);
			// 
			// FullNameTextBox
			// 
			this.FullNameTextBox.Location = new System.Drawing.Point(80, 69);
			this.FullNameTextBox.Name = "FullNameTextBox";
			this.FullNameTextBox.Size = new System.Drawing.Size(112, 20);
			this.FullNameTextBox.TabIndex = 5;
			this.FullNameTextBox.Text = "Dawn of Light";
			this.FullNameTextBox.MouseEnter += new System.EventHandler(this.FullNameTextBox_MouseEnter);
			// 
			// shortNameTextBox
			// 
			this.shortNameTextBox.Location = new System.Drawing.Point(81, 43);
			this.shortNameTextBox.Name = "shortNameTextBox";
			this.shortNameTextBox.Size = new System.Drawing.Size(111, 20);
			this.shortNameTextBox.TabIndex = 4;
			this.shortNameTextBox.Text = "DOLSERVER";
			this.shortNameTextBox.MouseEnter += new System.EventHandler(this.shortNameTextBox_MouseEnter);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(9, 72);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(57, 13);
			this.label3.TabIndex = 3;
			this.label3.Text = "Full Name:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(9, 46);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(66, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Short Name:";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(9, 19);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(68, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Server Type:";
			// 
			// serverTypeComboBox
			// 
			this.serverTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.serverTypeComboBox.FormattingEnabled = true;
			this.serverTypeComboBox.Items.AddRange(new object[] {
									"Normal",
									"PvP",
									"PvE"});
			this.serverTypeComboBox.Location = new System.Drawing.Point(117, 16);
			this.serverTypeComboBox.Name = "serverTypeComboBox";
			this.serverTypeComboBox.Size = new System.Drawing.Size(75, 21);
			this.serverTypeComboBox.TabIndex = 0;
			this.serverTypeComboBox.MouseEnter += new System.EventHandler(this.serverTypeComboBox_MouseEnter);
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.mysqlPasswordTextBox);
			this.groupBox2.Controls.Add(this.mysqlUsernameTextBox);
			this.groupBox2.Controls.Add(this.mysqlDatabaseTextBox);
			this.groupBox2.Controls.Add(this.mysqlHostTextBox);
			this.groupBox2.Controls.Add(this.label8);
			this.groupBox2.Controls.Add(this.label7);
			this.groupBox2.Controls.Add(this.label6);
			this.groupBox2.Controls.Add(this.label5);
			this.groupBox2.Controls.Add(this.databaseTypeComboBox);
			this.groupBox2.Controls.Add(this.label4);
			this.groupBox2.Location = new System.Drawing.Point(215, 12);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(217, 146);
			this.groupBox2.TabIndex = 1;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Database Settings";
			// 
			// mysqlPasswordTextBox
			// 
			this.mysqlPasswordTextBox.Location = new System.Drawing.Point(106, 117);
			this.mysqlPasswordTextBox.Name = "mysqlPasswordTextBox";
			this.mysqlPasswordTextBox.PasswordChar = '*';
			this.mysqlPasswordTextBox.Size = new System.Drawing.Size(100, 20);
			this.mysqlPasswordTextBox.TabIndex = 9;
			this.mysqlPasswordTextBox.MouseEnter += new System.EventHandler(this.mysqlPasswordTextBox_MouseEnter);
			// 
			// mysqlUsernameTextBox
			// 
			this.mysqlUsernameTextBox.Location = new System.Drawing.Point(106, 92);
			this.mysqlUsernameTextBox.Name = "mysqlUsernameTextBox";
			this.mysqlUsernameTextBox.Size = new System.Drawing.Size(100, 20);
			this.mysqlUsernameTextBox.TabIndex = 8;
			this.mysqlUsernameTextBox.MouseEnter += new System.EventHandler(this.mysqlUsernameTextBox_MouseEnter);
			// 
			// mysqlDatabaseTextBox
			// 
			this.mysqlDatabaseTextBox.Location = new System.Drawing.Point(106, 67);
			this.mysqlDatabaseTextBox.Name = "mysqlDatabaseTextBox";
			this.mysqlDatabaseTextBox.Size = new System.Drawing.Size(100, 20);
			this.mysqlDatabaseTextBox.TabIndex = 7;
			this.mysqlDatabaseTextBox.MouseEnter += new System.EventHandler(this.mysqlDatabaseTextBox_MouseEnter);
			// 
			// mysqlHostTextBox
			// 
			this.mysqlHostTextBox.Location = new System.Drawing.Point(106, 42);
			this.mysqlHostTextBox.Name = "mysqlHostTextBox";
			this.mysqlHostTextBox.Size = new System.Drawing.Size(100, 20);
			this.mysqlHostTextBox.TabIndex = 6;
			this.mysqlHostTextBox.MouseEnter += new System.EventHandler(this.mysqlHostTextBox_MouseEnter);
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(6, 120);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(56, 13);
			this.label8.TabIndex = 5;
			this.label8.Text = "Password:";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(6, 95);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(58, 13);
			this.label7.TabIndex = 4;
			this.label7.Text = "Username:";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(6, 70);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(56, 13);
			this.label6.TabIndex = 3;
			this.label6.Text = "Database:";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(6, 45);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(32, 13);
			this.label5.TabIndex = 2;
			this.label5.Text = "Host:";
			// 
			// databaseTypeComboBox
			// 
			this.databaseTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.databaseTypeComboBox.FormattingEnabled = true;
			this.databaseTypeComboBox.Items.AddRange(new object[] {
									"XML",
									"MySQL"});
			this.databaseTypeComboBox.Location = new System.Drawing.Point(106, 16);
			this.databaseTypeComboBox.Name = "databaseTypeComboBox";
			this.databaseTypeComboBox.Size = new System.Drawing.Size(100, 21);
			this.databaseTypeComboBox.TabIndex = 1;
			this.databaseTypeComboBox.SelectionChangeCommitted += new System.EventHandler(this.databaseTypeComboBox_SelectionChangeCommitted);
			this.databaseTypeComboBox.MouseEnter += new System.EventHandler(this.databaseTypeComboBox_MouseEnter);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(6, 19);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(83, 13);
			this.label4.TabIndex = 0;
			this.label4.Text = "Database Type:";
			// 
			// saveButton
			// 
			this.saveButton.Location = new System.Drawing.Point(12, 135);
			this.saveButton.Name = "saveButton";
			this.saveButton.Size = new System.Drawing.Size(103, 23);
			this.saveButton.TabIndex = 2;
			this.saveButton.Text = "Save Settings";
			this.saveButton.UseVisualStyleBackColor = true;
			this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
			this.saveButton.MouseEnter += new System.EventHandler(this.saveButton_MouseEnter);
			// 
			// closeButton
			// 
			this.closeButton.Location = new System.Drawing.Point(121, 135);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = new System.Drawing.Size(88, 23);
			this.closeButton.TabIndex = 3;
			this.closeButton.Text = "Close";
			this.closeButton.UseVisualStyleBackColor = true;
			this.closeButton.Click += new System.EventHandler(this.button2_Click);
			this.closeButton.MouseEnter += new System.EventHandler(this.closeButton_MouseEnter);
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.toolStripStatusLabel1});
			this.statusStrip1.Location = new System.Drawing.Point(0, 507);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(444, 22);
			this.statusStrip1.TabIndex = 4;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// toolStripStatusLabel1
			// 
			this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
			this.toolStripStatusLabel1.Size = new System.Drawing.Size(155, 17);
			this.toolStripStatusLabel1.Text = "Please configure your server";
			// 
			// spTvCat
			// 
			this.spTvCat.Location = new System.Drawing.Point(6, 19);
			this.spTvCat.Name = "spTvCat";
			this.spTvCat.Size = new System.Drawing.Size(282, 259);
			this.spTvCat.TabIndex = 5;
			this.spTvCat.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.SpTvCatNodeMouseClick);
			// 
			// spLblKey
			// 
			this.spLblKey.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.spLblKey.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.spLblKey.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.spLblKey.Location = new System.Drawing.Point(6, 281);
			this.spLblKey.Name = "spLblKey";
			this.spLblKey.Size = new System.Drawing.Size(282, 20);
			this.spLblKey.TabIndex = 6;
			// 
			// spCbValue
			// 
			this.spCbValue.FormattingEnabled = true;
			this.spCbValue.Location = new System.Drawing.Point(6, 309);
			this.spCbValue.Name = "spCbValue";
			this.spCbValue.Size = new System.Drawing.Size(282, 21);
			this.spCbValue.TabIndex = 7;
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(357, 412);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 8;
			this.button1.Text = "Set default";
			this.button1.UseVisualStyleBackColor = true;
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.spSave);
			this.groupBox3.Controls.Add(this.groupBox5);
			this.groupBox3.Controls.Add(this.groupBox4);
			this.groupBox3.Controls.Add(this.spCbValue);
			this.groupBox3.Controls.Add(this.spLblKey);
			this.groupBox3.Controls.Add(this.spTvCat);
			this.groupBox3.Location = new System.Drawing.Point(12, 164);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(420, 338);
			this.groupBox3.TabIndex = 10;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Server Properties";
			// 
			// groupBox5
			// 
			this.groupBox5.Controls.Add(this.spLblDefault);
			this.groupBox5.Location = new System.Drawing.Point(294, 209);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(120, 94);
			this.groupBox5.TabIndex = 9;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "Default";
			// 
			// spLblDefault
			// 
			this.spLblDefault.Location = new System.Drawing.Point(6, 25);
			this.spLblDefault.Name = "spLblDefault";
			this.spLblDefault.Size = new System.Drawing.Size(109, 59);
			this.spLblDefault.TabIndex = 10;
			this.spLblDefault.Text = "True";
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.spLblDesc);
			this.groupBox4.Location = new System.Drawing.Point(294, 19);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(120, 184);
			this.groupBox4.TabIndex = 8;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Description";
			// 
			// spLblDesc
			// 
			this.spLblDesc.Location = new System.Drawing.Point(6, 28);
			this.spLblDesc.Name = "spLblDesc";
			this.spLblDesc.Size = new System.Drawing.Size(108, 129);
			this.spLblDesc.TabIndex = 9;
			this.spLblDesc.Text = "Use the new Keeps (v 1.90)";
			// 
			// spSave
			// 
			this.spSave.Location = new System.Drawing.Point(294, 309);
			this.spSave.Name = "spSave";
			this.spSave.Size = new System.Drawing.Size(120, 23);
			this.spSave.TabIndex = 10;
			this.spSave.Text = "Save Properties";
			this.spSave.UseVisualStyleBackColor = true;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(444, 529);
			this.Controls.Add(this.statusStrip1);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.closeButton);
			this.Controls.Add(this.saveButton);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "Form1";
			this.Text = "Dawn of Light Configuration";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.groupBox5.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		private System.Windows.Forms.Button spSave;
		public System.Windows.Forms.TreeView spTvCat;
		public System.Windows.Forms.Label spLblKey;
		public System.Windows.Forms.ComboBox spCbValue;
		public System.Windows.Forms.Label spLblDefault;
		public System.Windows.Forms.Label spLblDesc;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.GroupBox groupBox5;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Button button1;

		#endregion

		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button saveButton;
		private System.Windows.Forms.Button closeButton;
		public System.Windows.Forms.ComboBox serverTypeComboBox;
		public System.Windows.Forms.CheckBox autoAccountCreationCheckBox;
		public System.Windows.Forms.TextBox FullNameTextBox;
		public System.Windows.Forms.TextBox shortNameTextBox;
		public System.Windows.Forms.TextBox mysqlPasswordTextBox;
		public System.Windows.Forms.TextBox mysqlUsernameTextBox;
		public System.Windows.Forms.TextBox mysqlDatabaseTextBox;
		public System.Windows.Forms.TextBox mysqlHostTextBox;
		public System.Windows.Forms.ComboBox databaseTypeComboBox;
		private System.Windows.Forms.StatusStrip statusStrip1;
		public System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
	}
}

