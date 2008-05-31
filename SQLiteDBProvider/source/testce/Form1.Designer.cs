namespace test
{
	partial class Form1
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.MainMenu mainMenu1;

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
      this.mainMenu1 = new System.Windows.Forms.MainMenu();
      this.textBox1 = new System.Windows.Forms.TextBox();
      this.menuItem1 = new System.Windows.Forms.MenuItem();
      this.SuspendLayout();
      // 
      // mainMenu1
      // 
      this.mainMenu1.MenuItems.Add(this.menuItem1);
      // 
      // textBox1
      // 
      this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textBox1.Location = new System.Drawing.Point(0, 0);
      this.textBox1.Multiline = true;
      this.textBox1.Name = "textBox1";
      this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.textBox1.Size = new System.Drawing.Size(240, 268);
      this.textBox1.TabIndex = 0;
      this.textBox1.WordWrap = false;
      // 
      // menuItem1
      // 
      this.menuItem1.Text = "Exit";
      this.menuItem1.Click += new System.EventHandler(this.menuItem1_Click);
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
      this.ClientSize = new System.Drawing.Size(240, 268);
      this.Controls.Add(this.textBox1);
      this.Menu = this.mainMenu1;
      this.Name = "Form1";
      this.Text = "Form1";
      this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TextBox textBox1;
    private System.Windows.Forms.MenuItem menuItem1;
	}
}