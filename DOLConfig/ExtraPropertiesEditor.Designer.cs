namespace DOLConfig
{
    partial class ExtraPropertiesEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExtraPropertiesEditor));
            this.save_button = new System.Windows.Forms.Button();
            this.cancel_button = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.property_name_textbox = new System.Windows.Forms.TextBox();
            this.property_value_textbox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.property_type_selectbox = new System.Windows.Forms.ComboBox();
            this.edit_property_error_label = new System.Windows.Forms.Label();
            this.property_description_label = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // save_button
            // 
            this.save_button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.save_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.save_button.Image = ((System.Drawing.Image)(resources.GetObject("save_button.Image")));
            this.save_button.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.save_button.Location = new System.Drawing.Point(342, 187);
            this.save_button.Name = "save_button";
            this.save_button.Size = new System.Drawing.Size(130, 25);
            this.save_button.TabIndex = 0;
            this.save_button.Text = "Edit/add property";
            this.save_button.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.save_button.UseVisualStyleBackColor = true;
            this.save_button.Click += new System.EventHandler(this.save_button_Click);
            // 
            // cancel_button
            // 
            this.cancel_button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cancel_button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancel_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cancel_button.Image = ((System.Drawing.Image)(resources.GetObject("cancel_button.Image")));
            this.cancel_button.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.cancel_button.Location = new System.Drawing.Point(12, 187);
            this.cancel_button.Name = "cancel_button";
            this.cancel_button.Size = new System.Drawing.Size(72, 25);
            this.cancel_button.TabIndex = 1;
            this.cancel_button.Text = "Cancel";
            this.cancel_button.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cancel_button.UseVisualStyleBackColor = true;
            this.cancel_button.Click += new System.EventHandler(this.cancel_button_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Property name:";
            // 
            // property_name_textbox
            // 
            this.property_name_textbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.property_name_textbox.Location = new System.Drawing.Point(101, 6);
            this.property_name_textbox.Name = "property_name_textbox";
            this.property_name_textbox.Size = new System.Drawing.Size(371, 20);
            this.property_name_textbox.TabIndex = 3;
            this.property_name_textbox.TextChanged += new System.EventHandler(this.property_name_textbox_TextChanged);
            // 
            // property_value_textbox
            // 
            this.property_value_textbox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.property_value_textbox.Location = new System.Drawing.Point(101, 102);
            this.property_value_textbox.Multiline = true;
            this.property_value_textbox.Name = "property_value_textbox";
            this.property_value_textbox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.property_value_textbox.Size = new System.Drawing.Size(371, 77);
            this.property_value_textbox.TabIndex = 5;
            this.property_value_textbox.TextChanged += new System.EventHandler(this.property_value_textbox_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 105);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Value:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 77);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Value type:";
            // 
            // property_type_selectbox
            // 
            this.property_type_selectbox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.property_type_selectbox.FormattingEnabled = true;
            this.property_type_selectbox.Items.AddRange(new object[] {
            "string",
            "integer",
            "boolean"});
            this.property_type_selectbox.Location = new System.Drawing.Point(101, 75);
            this.property_type_selectbox.Name = "property_type_selectbox";
            this.property_type_selectbox.Size = new System.Drawing.Size(99, 21);
            this.property_type_selectbox.TabIndex = 7;
            this.property_type_selectbox.SelectedIndexChanged += new System.EventHandler(this.property_type_selectbox_SelectedIndexChanged);
            // 
            // edit_property_error_label
            // 
            this.edit_property_error_label.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.edit_property_error_label.ForeColor = System.Drawing.Color.Red;
            this.edit_property_error_label.Location = new System.Drawing.Point(90, 187);
            this.edit_property_error_label.Name = "edit_property_error_label";
            this.edit_property_error_label.Size = new System.Drawing.Size(246, 23);
            this.edit_property_error_label.TabIndex = 8;
            this.edit_property_error_label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // property_description_label
            // 
            this.property_description_label.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.property_description_label.Location = new System.Drawing.Point(101, 33);
            this.property_description_label.Name = "property_description_label";
            this.property_description_label.Size = new System.Drawing.Size(371, 39);
            this.property_description_label.TabIndex = 9;
            this.property_description_label.Text = "no description";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 33);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(63, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Description:";
            // 
            // ExtraPropertiesEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancel_button;
            this.ClientSize = new System.Drawing.Size(484, 222);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.property_description_label);
            this.Controls.Add(this.edit_property_error_label);
            this.Controls.Add(this.property_type_selectbox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.property_value_textbox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.property_name_textbox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cancel_button);
            this.Controls.Add(this.save_button);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(500, 260);
            this.Name = "ExtraPropertiesEditor";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Edit/add extra properties";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button save_button;
        private System.Windows.Forms.Button cancel_button;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox property_name_textbox;
        private System.Windows.Forms.TextBox property_value_textbox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox property_type_selectbox;
        private System.Windows.Forms.Label edit_property_error_label;
        private System.Windows.Forms.Label property_description_label;
        private System.Windows.Forms.Label label5;
    }
}