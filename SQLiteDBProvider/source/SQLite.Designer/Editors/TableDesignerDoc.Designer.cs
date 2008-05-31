namespace SQLite.Designer.Editors
{
  partial class TableDesignerDoc
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

      _editingTables.Remove(GetHashCode());

      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.Windows.Forms.SplitContainer _splitter;
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TableDesignerDoc));
      this._dataGrid = new SQLite.Designer.Editors.DbGridView();
      this.name = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.type = new SQLite.Designer.Editors.AutoCompleteColumn();
      this.isnull = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this._propertyGrid = new System.Windows.Forms.PropertyGrid();
      this.autoCompleteColumn1 = new SQLite.Designer.Editors.AutoCompleteColumn();
      this._imageList = new System.Windows.Forms.ImageList(this.components);
      _splitter = new System.Windows.Forms.SplitContainer();
      _splitter.Panel1.SuspendLayout();
      _splitter.Panel2.SuspendLayout();
      _splitter.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this._dataGrid)).BeginInit();
      this.SuspendLayout();
      // 
      // _splitter
      // 
      _splitter.BackColor = System.Drawing.SystemColors.Control;
      _splitter.Dock = System.Windows.Forms.DockStyle.Fill;
      _splitter.Location = new System.Drawing.Point(0, 0);
      _splitter.Name = "_splitter";
      _splitter.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // _splitter.Panel1
      // 
      _splitter.Panel1.Controls.Add(this._dataGrid);
      // 
      // _splitter.Panel2
      // 
      _splitter.Panel2.Controls.Add(this._propertyGrid);
      _splitter.Size = new System.Drawing.Size(436, 401);
      _splitter.SplitterDistance = 197;
      _splitter.TabIndex = 0;
      // 
      // _dataGrid
      // 
      this._dataGrid.AllowUserToResizeRows = false;
      this._dataGrid.BackgroundColor = System.Drawing.SystemColors.Window;
      this._dataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this._dataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.name,
            this.type,
            this.isnull});
      this._dataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
      this._dataGrid.Location = new System.Drawing.Point(0, 0);
      this._dataGrid.Name = "_dataGrid";
      this._dataGrid.RowHeadersWidth = 42;
      this._dataGrid.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
      this._dataGrid.RowTemplate.Height = 23;
      this._dataGrid.Size = new System.Drawing.Size(436, 197);
      this._dataGrid.TabIndex = 1;
      this._dataGrid.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this._dataGrid_CellClick);
      this._dataGrid.RowHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this._dataGrid_RowHeaderMouseClick);
      this._dataGrid.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this._dataGrid_CellPainting);
      this._dataGrid.UserDeletedRow += new System.Windows.Forms.DataGridViewRowEventHandler(this._dataGrid_UserDeletedRow);
      this._dataGrid.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this._dataGrid_CellValueChanged);
      this._dataGrid.CellEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this._dataGrid_CellEnter);
      this._dataGrid.SelectionChanged += new System.EventHandler(this._dataGrid_SelectionChanged);
      // 
      // name
      // 
      this.name.Frozen = true;
      this.name.HeaderText = "Column Name";
      this.name.Name = "name";
      this.name.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      // 
      // type
      // 
      this.type.HeaderText = "Data Type";
      this.type.Name = "type";
      // 
      // isnull
      // 
      this.isnull.HeaderText = "Allow Nulls";
      this.isnull.Name = "isnull";
      // 
      // _propertyGrid
      // 
      this._propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
      this._propertyGrid.Location = new System.Drawing.Point(0, 0);
      this._propertyGrid.Name = "_propertyGrid";
      this._propertyGrid.Size = new System.Drawing.Size(436, 200);
      this._propertyGrid.TabIndex = 0;
      // 
      // autoCompleteColumn1
      // 
      this.autoCompleteColumn1.HeaderText = "Data Type";
      this.autoCompleteColumn1.Name = "autoCompleteColumn1";
      this.autoCompleteColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.True;
      this.autoCompleteColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
      // 
      // _imageList
      // 
      this._imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("_imageList.ImageStream")));
      this._imageList.TransparentColor = System.Drawing.Color.Magenta;
      this._imageList.Images.SetKeyName(0, "PrimaryKey.bmp");
      // 
      // TableDesignerDoc
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.Controls.Add(_splitter);
      this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.Name = "TableDesignerDoc";
      this.Size = new System.Drawing.Size(436, 401);
      _splitter.Panel1.ResumeLayout(false);
      _splitter.Panel2.ResumeLayout(false);
      _splitter.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this._dataGrid)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private DbGridView _dataGrid;
    private System.Windows.Forms.PropertyGrid _propertyGrid;
    private AutoCompleteColumn autoCompleteColumn1;
    private System.Windows.Forms.DataGridViewTextBoxColumn name;
    private AutoCompleteColumn type;
    private System.Windows.Forms.DataGridViewCheckBoxColumn isnull;
    private System.Windows.Forms.ImageList _imageList;

  }
}