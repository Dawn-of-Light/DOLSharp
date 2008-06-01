using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DOLConfig
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

		private void saveButton_Click(object sender, EventArgs e)
		{
			Program.SaveSettings();
		}

		private void serverTypeComboBox_MouseEnter(object sender, EventArgs e)
		{
			toolStripStatusLabel1.Text = "The type of server, Normal (RvR), PvP or PvE (Coop)";
		}

		private void shortNameTextBox_MouseEnter(object sender, EventArgs e)
		{
			toolStripStatusLabel1.Text = "The short name for the server";
		}

		private void FullNameTextBox_MouseEnter(object sender, EventArgs e)
		{
			toolStripStatusLabel1.Text = "The full name for the server";
		}

		private void autoAccountCreationCheckBox_MouseEnter(object sender, EventArgs e)
		{
			toolStripStatusLabel1.Text = "Allow players to automatically create accounts for themselves as they log in";
		}

		private void saveButton_MouseEnter(object sender, EventArgs e)
		{
			toolStripStatusLabel1.Text = "Save the server configuration";
		}

		private void closeButton_MouseEnter(object sender, EventArgs e)
		{
			toolStripStatusLabel1.Text = "Close this application";
		}

		private void databaseTypeComboBox_MouseEnter(object sender, EventArgs e)
		{
			toolStripStatusLabel1.Text = "Set the database type for this server";
		}

		private void mysqlHostTextBox_MouseEnter(object sender, EventArgs e)
		{
			toolStripStatusLabel1.Text = "Set the MySQL host for this server";
		}

		private void mysqlDatabaseTextBox_MouseEnter(object sender, EventArgs e)
		{
			toolStripStatusLabel1.Text = "Set the MySQL database name for this server";
		}

		private void mysqlUsernameTextBox_MouseEnter(object sender, EventArgs e)
		{
			toolStripStatusLabel1.Text = "Set the MySQL username for this server";
		}

		private void mysqlPasswordTextBox_MouseEnter(object sender, EventArgs e)
		{
			toolStripStatusLabel1.Text = "Set the MySQL password for this server";
		}

	}
}