namespace DOLStudio
{
    partial class ObjectEditorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ObjectEditorForm));
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.lblID = new System.Windows.Forms.Label();
            this.IDfield = new System.Windows.Forms.NumericUpDown();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.Idnb = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.Fieldchooser = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.ComparisonValue = new System.Windows.Forms.TextBox();
            this.QueryBtn = new System.Windows.Forms.Button();
            this.IDnbquery = new System.Windows.Forms.Button();
            this.IDbutton = new System.Windows.Forms.Button();
            this.ResultView = new System.Windows.Forms.ListView();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.createNewItem = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.IDfield)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // propertyGrid
            // 
            resources.ApplyResources(this.propertyGrid, "propertyGrid");
            this.propertyGrid.Name = "propertyGrid";
            // 
            // lblID
            // 
            resources.ApplyResources(this.lblID, "lblID");
            this.lblID.Name = "lblID";
            // 
            // IDfield
            // 
            resources.ApplyResources(this.IDfield, "IDfield");
            this.IDfield.Maximum = new decimal(new int[] {
            -1,
            -1,
            0,
            0});
            this.IDfield.Name = "IDfield";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLabel});
            resources.ApplyResources(this.statusStrip1, "statusStrip1");
            this.statusStrip1.Name = "statusStrip1";
            // 
            // StatusLabel
            // 
            this.StatusLabel.Name = "StatusLabel";
            resources.ApplyResources(this.StatusLabel, "StatusLabel");
            // 
            // treeView1
            // 
            this.treeView1.Cursor = System.Windows.Forms.Cursors.Default;
            resources.ApplyResources(this.treeView1, "treeView1");
            this.treeView1.Name = "treeView1";
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            // 
            // Idnb
            // 
            resources.ApplyResources(this.Idnb, "Idnb");
            this.Idnb.Name = "Idnb";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // Fieldchooser
            // 
            this.Fieldchooser.FormattingEnabled = true;
            resources.ApplyResources(this.Fieldchooser, "Fieldchooser");
            this.Fieldchooser.Name = "Fieldchooser";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // ComparisonValue
            // 
            resources.ApplyResources(this.ComparisonValue, "ComparisonValue");
            this.ComparisonValue.Name = "ComparisonValue";
            // 
            // QueryBtn
            // 
            resources.ApplyResources(this.QueryBtn, "QueryBtn");
            this.QueryBtn.Name = "QueryBtn";
            this.QueryBtn.UseVisualStyleBackColor = true;
            this.QueryBtn.Click += new System.EventHandler(this.QueryBtn_Click);
            // 
            // IDnbquery
            // 
            resources.ApplyResources(this.IDnbquery, "IDnbquery");
            this.IDnbquery.Name = "IDnbquery";
            this.IDnbquery.UseVisualStyleBackColor = true;
            this.IDnbquery.Click += new System.EventHandler(this.IDnbquery_Click);
            // 
            // IDbutton
            // 
            resources.ApplyResources(this.IDbutton, "IDbutton");
            this.IDbutton.Name = "IDbutton";
            this.IDbutton.UseVisualStyleBackColor = true;
            this.IDbutton.Click += new System.EventHandler(this.IDbutton_Click);
            // 
            // ResultView
            // 
            resources.ApplyResources(this.ResultView, "ResultView");
            this.ResultView.Name = "ResultView";
            this.ResultView.UseCompatibleStateImageBehavior = false;
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // createNewItem
            // 
            resources.ApplyResources(this.createNewItem, "createNewItem");
            this.createNewItem.Name = "createNewItem";
            this.createNewItem.UseVisualStyleBackColor = true;
            this.createNewItem.Click += new System.EventHandler(this.createNewItem_Click);
            // 
            // ObjectEditorForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.createNewItem);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.ResultView);
            this.Controls.Add(this.IDbutton);
            this.Controls.Add(this.IDnbquery);
            this.Controls.Add(this.QueryBtn);
            this.Controls.Add(this.ComparisonValue);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.Fieldchooser);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Idnb);
            this.Controls.Add(this.treeView1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.IDfield);
            this.Controls.Add(this.lblID);
            this.Controls.Add(this.propertyGrid);
            this.Name = "ObjectEditorForm";
            this.Load += new System.EventHandler(this.ObjectEditorForm_Load);
            this.Shown += new System.EventHandler(this.ObjectEditorForm_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.IDfield)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PropertyGrid propertyGrid;
        private System.Windows.Forms.Label lblID;
        private System.Windows.Forms.NumericUpDown IDfield;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.TextBox Idnb;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox Fieldchooser;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox ComparisonValue;
        private System.Windows.Forms.Button QueryBtn;
        private System.Windows.Forms.Button IDnbquery;
        private System.Windows.Forms.Button IDbutton;
        private System.Windows.Forms.ListView ResultView;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button createNewItem;
    }
}