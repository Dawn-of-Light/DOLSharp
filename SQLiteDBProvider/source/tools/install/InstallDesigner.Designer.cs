namespace install
{
  partial class InstallDesigner
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
      System.Windows.Forms.ColumnHeader columnHeader1;
      System.Windows.Forms.Label label1;
      System.Windows.Forms.Label label3;
      System.Windows.Forms.Label label2;
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InstallDesigner));
      this.installList = new System.Windows.Forms.ListView();
      this.warningPanel = new System.Windows.Forms.Panel();
      this.closeButton = new System.Windows.Forms.Button();
      columnHeader1 = new System.Windows.Forms.ColumnHeader();
      label1 = new System.Windows.Forms.Label();
      label3 = new System.Windows.Forms.Label();
      label2 = new System.Windows.Forms.Label();
      this.warningPanel.SuspendLayout();
      this.SuspendLayout();
      // 
      // columnHeader1
      // 
      columnHeader1.Text = "Environment";
      columnHeader1.Width = 800;
      // 
      // label1
      // 
      label1.AutoSize = true;
      label1.Location = new System.Drawing.Point(13, 13);
      label1.Name = "label1";
      label1.Size = new System.Drawing.Size(325, 13);
      label1.TabIndex = 0;
      label1.Text = "Install SQLite Design-Time Support for the following environments:";
      // 
      // label3
      // 
      label3.AutoSize = true;
      label3.Location = new System.Drawing.Point(0, 0);
      label3.Name = "label3";
      label3.Size = new System.Drawing.Size(13, 13);
      label3.TabIndex = 5;
      label3.Text = "*";
      // 
      // label2
      // 
      label2.Location = new System.Drawing.Point(10, 0);
      label2.Name = "label2";
      label2.Size = new System.Drawing.Size(340, 61);
      label2.TabIndex = 4;
      label2.Text = resources.GetString("label2.Text");
      // 
      // installList
      // 
      this.installList.CheckBoxes = true;
      this.installList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeader1});
      this.installList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
      this.installList.Location = new System.Drawing.Point(13, 30);
      this.installList.Name = "installList";
      this.installList.Size = new System.Drawing.Size(350, 149);
      this.installList.TabIndex = 1;
      this.installList.UseCompatibleStateImageBehavior = false;
      this.installList.View = System.Windows.Forms.View.List;
      this.installList.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.installList_ItemChecked);
      // 
      // warningPanel
      // 
      this.warningPanel.Controls.Add(label3);
      this.warningPanel.Controls.Add(label2);
      this.warningPanel.Location = new System.Drawing.Point(13, 186);
      this.warningPanel.Name = "warningPanel";
      this.warningPanel.Size = new System.Drawing.Size(350, 73);
      this.warningPanel.TabIndex = 2;
      this.warningPanel.Visible = false;
      // 
      // closeButton
      // 
      this.closeButton.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.closeButton.Location = new System.Drawing.Point(288, 270);
      this.closeButton.Name = "closeButton";
      this.closeButton.Size = new System.Drawing.Size(75, 23);
      this.closeButton.TabIndex = 3;
      this.closeButton.Text = "&Close";
      this.closeButton.UseVisualStyleBackColor = true;
      this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
      // 
      // InstallDesigner
      // 
      this.AcceptButton = this.closeButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.closeButton;
      this.ClientSize = new System.Drawing.Size(375, 305);
      this.Controls.Add(this.closeButton);
      this.Controls.Add(this.warningPanel);
      this.Controls.Add(this.installList);
      this.Controls.Add(label1);
      this.Font = new System.Drawing.Font("MS Shell Dlg 2", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "InstallDesigner";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "SQLite Designer Installation";
      this.TopMost = true;
      this.warningPanel.ResumeLayout(false);
      this.warningPanel.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ListView installList;
    private System.Windows.Forms.Panel warningPanel;
    private System.Windows.Forms.Button closeButton;
  }
}