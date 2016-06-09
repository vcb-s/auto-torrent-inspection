namespace AutoTorrentInspection
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.btnLoadFile = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.cbCategory = new System.Windows.Forms.ComboBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.ColPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColFileName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColSize = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.btnAnnounceList = new System.Windows.Forms.Button();
            this.cbShowAll = new System.Windows.Forms.CheckBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.contextMenuOpenFolder = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.OpenFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.OpenFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cbFixCue = new System.Windows.Forms.CheckBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel_Status = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel_Encode = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnWebP = new System.Windows.Forms.Button();
            this.btnCompare = new System.Windows.Forms.Button();
            this.btnTreeView = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.contextMenuOpenFolder.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnLoadFile
            // 
            this.btnLoadFile.Location = new System.Drawing.Point(12, 12);
            this.btnLoadFile.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnLoadFile.Name = "btnLoadFile";
            this.btnLoadFile.Size = new System.Drawing.Size(87, 33);
            this.btnLoadFile.TabIndex = 0;
            this.btnLoadFile.Text = "载入";
            this.btnLoadFile.UseVisualStyleBackColor = true;
            this.btnLoadFile.Click += new System.EventHandler(this.btnLoadFile_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.AllowDrop = true;
            this.btnRefresh.Enabled = false;
            this.btnRefresh.Location = new System.Drawing.Point(12, 53);
            this.btnRefresh.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(87, 33);
            this.btnRefresh.TabIndex = 1;
            this.btnRefresh.Text = "刷新";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // cbCategory
            // 
            this.cbCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbCategory.Enabled = false;
            this.cbCategory.FormattingEnabled = true;
            this.cbCategory.Location = new System.Drawing.Point(13, 135);
            this.cbCategory.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cbCategory.Name = "cbCategory";
            this.cbCategory.Size = new System.Drawing.Size(85, 25);
            this.cbCategory.TabIndex = 3;
            this.cbCategory.MouseEnter += new System.EventHandler(this.cbCategory_MouseEnter);
            this.cbCategory.MouseLeave += new System.EventHandler(this.cbCategory_MouseLeave);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.dataGridView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridView1.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColPath,
            this.ColFileName,
            this.ColSize});
            this.dataGridView1.GridColor = System.Drawing.SystemColors.Highlight;
            this.dataGridView1.Location = new System.Drawing.Point(105, 13);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.Size = new System.Drawing.Size(467, 415);
            this.dataGridView1.TabIndex = 9;
            this.dataGridView1.TabStop = false;
            this.dataGridView1.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView1_CellMouseClick);
            this.dataGridView1.KeyUp += new System.Windows.Forms.KeyEventHandler(this.dataGridView1_KeyUp);
            // 
            // ColPath
            // 
            this.ColPath.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ColPath.HeaderText = "路径";
            this.ColPath.Name = "ColPath";
            this.ColPath.ReadOnly = true;
            this.ColPath.Width = 57;
            // 
            // ColFileName
            // 
            this.ColFileName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ColFileName.HeaderText = "文件名";
            this.ColFileName.Name = "ColFileName";
            this.ColFileName.ReadOnly = true;
            this.ColFileName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.ColFileName.Width = 50;
            // 
            // ColSize
            // 
            this.ColSize.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ColSize.HeaderText = "大小";
            this.ColSize.Name = "ColSize";
            this.ColSize.ReadOnly = true;
            this.ColSize.Width = 57;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "torrent File|*.torrent";
            // 
            // btnAnnounceList
            // 
            this.btnAnnounceList.Enabled = false;
            this.btnAnnounceList.Location = new System.Drawing.Point(12, 94);
            this.btnAnnounceList.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnAnnounceList.Name = "btnAnnounceList";
            this.btnAnnounceList.Size = new System.Drawing.Size(87, 33);
            this.btnAnnounceList.TabIndex = 2;
            this.btnAnnounceList.Text = "Tracker";
            this.btnAnnounceList.UseVisualStyleBackColor = true;
            this.btnAnnounceList.Click += new System.EventHandler(this.btnAnnounceList_Click);
            // 
            // cbShowAll
            // 
            this.cbShowAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbShowAll.AutoSize = true;
            this.cbShowAll.Location = new System.Drawing.Point(13, 386);
            this.cbShowAll.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cbShowAll.Name = "cbShowAll";
            this.cbShowAll.Size = new System.Drawing.Size(75, 21);
            this.cbShowAll.TabIndex = 4;
            this.cbShowAll.Text = "显示全部";
            this.cbShowAll.UseVisualStyleBackColor = true;
            // 
            // contextMenuOpenFolder
            // 
            this.contextMenuOpenFolder.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OpenFolderToolStripMenuItem,
            this.toolStripSeparator1,
            this.OpenFileToolStripMenuItem});
            this.contextMenuOpenFolder.Name = "contextMenuOpenFolder";
            this.contextMenuOpenFolder.Size = new System.Drawing.Size(161, 54);
            // 
            // OpenFolderToolStripMenuItem
            // 
            this.OpenFolderToolStripMenuItem.Name = "OpenFolderToolStripMenuItem";
            this.OpenFolderToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.OpenFolderToolStripMenuItem.Text = "打开所在文件夹";
            this.OpenFolderToolStripMenuItem.Click += new System.EventHandler(this.OpenFolderToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(157, 6);
            // 
            // OpenFileToolStripMenuItem
            // 
            this.OpenFileToolStripMenuItem.Name = "OpenFileToolStripMenuItem";
            this.OpenFileToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.OpenFileToolStripMenuItem.Text = "打开选中文件";
            this.OpenFileToolStripMenuItem.Click += new System.EventHandler(this.OpenFileToolStripMenuItem_Click);
            // 
            // cbFixCue
            // 
            this.cbFixCue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbFixCue.AutoSize = true;
            this.cbFixCue.Enabled = false;
            this.cbFixCue.Location = new System.Drawing.Point(13, 412);
            this.cbFixCue.Name = "cbFixCue";
            this.cbFixCue.Size = new System.Drawing.Size(75, 21);
            this.cbFixCue.TabIndex = 10;
            this.cbFixCue.Text = "cue 修复";
            this.cbFixCue.UseVisualStyleBackColor = true;
            this.cbFixCue.MouseEnter += new System.EventHandler(this.cbFixCue_MouseEnter);
            this.cbFixCue.MouseLeave += new System.EventHandler(this.cbFixCue_MouseLeave);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel_Status,
            this.toolStripStatusLabel_Encode});
            this.statusStrip1.Location = new System.Drawing.Point(0, 439);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(584, 22);
            this.statusStrip1.TabIndex = 11;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel_Status
            // 
            this.toolStripStatusLabel_Status.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripStatusLabel_Status.Name = "toolStripStatusLabel_Status";
            this.toolStripStatusLabel_Status.Size = new System.Drawing.Size(284, 17);
            this.toolStripStatusLabel_Status.Spring = true;
            this.toolStripStatusLabel_Status.Text = "请载入文件或文件夹";
            this.toolStripStatusLabel_Status.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusLabel_Encode
            // 
            this.toolStripStatusLabel_Encode.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripStatusLabel_Encode.Font = new System.Drawing.Font("Consolas", 9F);
            this.toolStripStatusLabel_Encode.Name = "toolStripStatusLabel_Encode";
            this.toolStripStatusLabel_Encode.Size = new System.Drawing.Size(284, 17);
            this.toolStripStatusLabel_Encode.Spring = true;
            this.toolStripStatusLabel_Encode.Text = " (0.00)";
            this.toolStripStatusLabel_Encode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnWebP
            // 
            this.btnWebP.Enabled = false;
            this.btnWebP.Location = new System.Drawing.Point(12, 344);
            this.btnWebP.Name = "btnWebP";
            this.btnWebP.Size = new System.Drawing.Size(87, 33);
            this.btnWebP.TabIndex = 12;
            this.btnWebP.Text = "WebP";
            this.btnWebP.UseVisualStyleBackColor = true;
            this.btnWebP.Visible = false;
            this.btnWebP.Click += new System.EventHandler(this.btnWebP_Click);
            // 
            // btnCompare
            // 
            this.btnCompare.Enabled = false;
            this.btnCompare.Location = new System.Drawing.Point(12, 177);
            this.btnCompare.Name = "btnCompare";
            this.btnCompare.Size = new System.Drawing.Size(87, 33);
            this.btnCompare.TabIndex = 13;
            this.btnCompare.Text = "对比";
            this.btnCompare.UseVisualStyleBackColor = true;
            this.btnCompare.Visible = false;
            this.btnCompare.Click += new System.EventHandler(this.btnCompare_Click);
            // 
            // btnTreeView
            // 
            this.btnTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnTreeView.Enabled = false;
            this.btnTreeView.Location = new System.Drawing.Point(13, 343);
            this.btnTreeView.Name = "btnTreeView";
            this.btnTreeView.Size = new System.Drawing.Size(87, 33);
            this.btnTreeView.TabIndex = 14;
            this.btnTreeView.Text = "查看结构";
            this.btnTreeView.UseVisualStyleBackColor = true;
            this.btnTreeView.Visible = false;
            this.btnTreeView.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(584, 461);
            this.Controls.Add(this.btnTreeView);
            this.Controls.Add(this.btnCompare);
            this.Controls.Add(this.btnWebP);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.cbFixCue);
            this.Controls.Add(this.cbShowAll);
            this.Controls.Add(this.btnAnnounceList);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.cbCategory);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.btnLoadFile);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MinimumSize = new System.Drawing.Size(380, 300);
            this.Name = "Form1";
            this.Text = "Auto Torrent Inspection";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Form1_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Form1_DragEnter);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.contextMenuOpenFolder.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnLoadFile;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.ComboBox cbCategory;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button btnAnnounceList;
        private System.Windows.Forms.CheckBox cbShowAll;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ContextMenuStrip contextMenuOpenFolder;
        private System.Windows.Forms.ToolStripMenuItem OpenFolderToolStripMenuItem;
        private System.Windows.Forms.CheckBox cbFixCue;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_Status;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_Encode;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem OpenFileToolStripMenuItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColSize;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColFileName;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColPath;
        private System.Windows.Forms.Button btnWebP;
        private System.Windows.Forms.Button btnCompare;
        private System.Windows.Forms.Button btnTreeView;
    }
}

