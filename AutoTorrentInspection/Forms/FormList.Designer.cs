namespace AutoTorrentInspection.Forms
{
    partial class FormList
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.listView_font = new System.Windows.Forms.ListView();
            this.listView_style = new System.Windows.Forms.ListView();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.listView_tag = new System.Windows.Forms.ListView();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(12, 13);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(260, 335);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.listView_font);
            this.tabPage1.Location = new System.Drawing.Point(4, 26);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage1.Size = new System.Drawing.Size(252, 305);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "font";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.listView_style);
            this.tabPage2.Location = new System.Drawing.Point(4, 26);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage2.Size = new System.Drawing.Size(252, 305);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "style";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // listView_font
            // 
            this.listView_font.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_font.Location = new System.Drawing.Point(0, 0);
            this.listView_font.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.listView_font.Name = "listView_font";
            this.listView_font.Size = new System.Drawing.Size(252, 305);
            this.listView_font.TabIndex = 0;
            this.listView_font.UseCompatibleStateImageBehavior = false;
            this.listView_font.View = System.Windows.Forms.View.List;
            // 
            // listView_style
            // 
            this.listView_style.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_style.Location = new System.Drawing.Point(0, 0);
            this.listView_style.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.listView_style.Name = "listView_style";
            this.listView_style.Size = new System.Drawing.Size(252, 305);
            this.listView_style.TabIndex = 0;
            this.listView_style.UseCompatibleStateImageBehavior = false;
            this.listView_style.View = System.Windows.Forms.View.List;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.listView_tag);
            this.tabPage3.Location = new System.Drawing.Point(4, 26);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(252, 305);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "tag";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // listView_tag
            // 
            this.listView_tag.Location = new System.Drawing.Point(0, 0);
            this.listView_tag.Name = "listView_tag";
            this.listView_tag.Size = new System.Drawing.Size(252, 305);
            this.listView_tag.TabIndex = 0;
            this.listView_tag.UseCompatibleStateImageBehavior = false;
            this.listView_tag.View = System.Windows.Forms.View.List;
            // 
            // FormList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 361);
            this.Controls.Add(this.tabControl1);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MinimumSize = new System.Drawing.Size(300, 400);
            this.Name = "FormList";
            this.Text = "字幕信息";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.ListView listView_font;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ListView listView_style;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.ListView listView_tag;
    }
}