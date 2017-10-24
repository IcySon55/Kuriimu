namespace Kuriimu
{
    partial class SearchDirectory
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
            this.label1 = new System.Windows.Forms.Label();
            this.txtSearchText = new System.Windows.Forms.TextBox();
            this.btnSearch = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lstResults = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSearchDirectory = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.chkSearchSubfolders = new System.Windows.Forms.CheckBox();
            this.chkMatchCase = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 41);
            this.label1.Margin = new System.Windows.Forms.Padding(4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Find what:";
            // 
            // txtSearchText
            // 
            this.txtSearchText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearchText.Location = new System.Drawing.Point(74, 38);
            this.txtSearchText.Margin = new System.Windows.Forms.Padding(4);
            this.txtSearchText.Name = "txtSearchText";
            this.txtSearchText.Size = new System.Drawing.Size(320, 20);
            this.txtSearchText.TabIndex = 2;
            this.txtSearchText.TextChanged += new System.EventHandler(this.txtSearchText_TextChanged);
            // 
            // btnSearch
            // 
            this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSearch.Location = new System.Drawing.Point(402, 36);
            this.btnSearch.Margin = new System.Windows.Forms.Padding(4);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(75, 23);
            this.btnSearch.TabIndex = 3;
            this.btnSearch.Text = "Find";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnFindText_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(402, 67);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lstResults
            // 
            this.lstResults.FormattingEnabled = true;
            this.lstResults.IntegralHeight = false;
            this.lstResults.Location = new System.Drawing.Point(13, 98);
            this.lstResults.Margin = new System.Windows.Forms.Padding(4);
            this.lstResults.Name = "lstResults";
            this.lstResults.Size = new System.Drawing.Size(458, 290);
            this.lstResults.TabIndex = 7;
            this.lstResults.DoubleClick += new System.EventHandler(this.lstResults_DoubleClick);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(27, 13);
            this.label2.Margin = new System.Windows.Forms.Padding(4);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Find in:";
            // 
            // txtSearchDirectory
            // 
            this.txtSearchDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearchDirectory.BackColor = System.Drawing.SystemColors.Window;
            this.txtSearchDirectory.Location = new System.Drawing.Point(74, 10);
            this.txtSearchDirectory.Margin = new System.Windows.Forms.Padding(4);
            this.txtSearchDirectory.Name = "txtSearchDirectory";
            this.txtSearchDirectory.ReadOnly = true;
            this.txtSearchDirectory.Size = new System.Drawing.Size(320, 20);
            this.txtSearchDirectory.TabIndex = 0;
            this.txtSearchDirectory.TabStop = false;
            this.txtSearchDirectory.TextChanged += new System.EventHandler(this.txtSearchDirectory_TextChanged);
            // 
            // btnBrowse
            // 
            this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowse.Location = new System.Drawing.Point(402, 8);
            this.btnBrowse.Margin = new System.Windows.Forms.Padding(4);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 1;
            this.btnBrowse.Text = "Browse...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // chkSearchSubfolders
            // 
            this.chkSearchSubfolders.AutoSize = true;
            this.chkSearchSubfolders.Location = new System.Drawing.Point(105, 71);
            this.chkSearchSubfolders.Margin = new System.Windows.Forms.Padding(4);
            this.chkSearchSubfolders.Name = "chkSearchSubfolders";
            this.chkSearchSubfolders.Size = new System.Drawing.Size(113, 17);
            this.chkSearchSubfolders.TabIndex = 5;
            this.chkSearchSubfolders.Text = "Search Subfolders";
            this.chkSearchSubfolders.UseVisualStyleBackColor = true;
            this.chkSearchSubfolders.CheckedChanged += new System.EventHandler(this.chkSearchSubfolders_CheckedChanged);
            // 
            // chkMatchCase
            // 
            this.chkMatchCase.AutoSize = true;
            this.chkMatchCase.Location = new System.Drawing.Point(15, 71);
            this.chkMatchCase.Margin = new System.Windows.Forms.Padding(4);
            this.chkMatchCase.Name = "chkMatchCase";
            this.chkMatchCase.Size = new System.Drawing.Size(82, 17);
            this.chkMatchCase.TabIndex = 8;
            this.chkMatchCase.Text = "Match case";
            this.chkMatchCase.UseVisualStyleBackColor = true;
            this.chkMatchCase.CheckedChanged += new System.EventHandler(this.chkMatchCase_CheckedChanged);
            // 
            // SearchDirectory
            // 
            this.AcceptButton = this.btnSearch;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(484, 401);
            this.Controls.Add(this.chkMatchCase);
            this.Controls.Add(this.chkSearchSubfolders);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.txtSearchDirectory);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lstResults);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.txtSearchText);
            this.Controls.Add(this.label1);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SearchDirectory";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Search Directory";
            this.Load += new System.EventHandler(this.frmSearchDirectory_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtSearchText;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ListBox lstResults;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtSearchDirectory;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.CheckBox chkSearchSubfolders;
        private System.Windows.Forms.CheckBox chkMatchCase;
    }
}