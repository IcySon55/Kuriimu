namespace Kuriimu
{
    partial class Find
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
            this.txtFindText = new System.Windows.Forms.TextBox();
            this.btnFindText = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.chkMatchCase = new System.Windows.Forms.CheckBox();
            this.lstResults = new System.Windows.Forms.ListBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tslResultCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabFindReplace = new System.Windows.Forms.TabControl();
            this.tabFind = new System.Windows.Forms.TabPage();
            this.tabReplace = new System.Windows.Forms.TabPage();
            this.chkReplaceAll = new System.Windows.Forms.CheckBox();
            this.txtReplaceText = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtFindTextReplace = new System.Windows.Forms.TextBox();
            this.lstResultsReplace = new System.Windows.Forms.ListBox();
            this.btnReplaceText = new System.Windows.Forms.Button();
            this.chkMatchCaseReplace = new System.Windows.Forms.CheckBox();
            this.btnCancelReplace = new System.Windows.Forms.Button();
            this.pnlFind = new System.Windows.Forms.Panel();
            this.statusStrip1.SuspendLayout();
            this.tabFindReplace.SuspendLayout();
            this.tabFind.SuspendLayout();
            this.tabReplace.SuspendLayout();
            this.pnlFind.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 10);
            this.label1.Margin = new System.Windows.Forms.Padding(4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Find what:";
            // 
            // txtFindText
            // 
            this.txtFindText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFindText.Location = new System.Drawing.Point(71, 7);
            this.txtFindText.Margin = new System.Windows.Forms.Padding(4);
            this.txtFindText.Name = "txtFindText";
            this.txtFindText.Size = new System.Drawing.Size(308, 20);
            this.txtFindText.TabIndex = 1;
            this.txtFindText.TextChanged += new System.EventHandler(this.txtFindText_TextChanged);
            // 
            // btnFindText
            // 
            this.btnFindText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFindText.Location = new System.Drawing.Point(387, 5);
            this.btnFindText.Margin = new System.Windows.Forms.Padding(4);
            this.btnFindText.Name = "btnFindText";
            this.btnFindText.Size = new System.Drawing.Size(75, 23);
            this.btnFindText.TabIndex = 2;
            this.btnFindText.Text = "Find";
            this.btnFindText.UseVisualStyleBackColor = true;
            this.btnFindText.Click += new System.EventHandler(this.btnFindText_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(387, 36);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Close";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // chkMatchCase
            // 
            this.chkMatchCase.AutoSize = true;
            this.chkMatchCase.Location = new System.Drawing.Point(71, 40);
            this.chkMatchCase.Margin = new System.Windows.Forms.Padding(4);
            this.chkMatchCase.Name = "chkMatchCase";
            this.chkMatchCase.Size = new System.Drawing.Size(82, 17);
            this.chkMatchCase.TabIndex = 3;
            this.chkMatchCase.Text = "Match case";
            this.chkMatchCase.UseVisualStyleBackColor = true;
            this.chkMatchCase.CheckedChanged += new System.EventHandler(this.chkMatchCase_CheckedChanged);
            // 
            // lstResults
            // 
            this.lstResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstResults.FormattingEnabled = true;
            this.lstResults.IntegralHeight = false;
            this.lstResults.Location = new System.Drawing.Point(7, 67);
            this.lstResults.Margin = new System.Windows.Forms.Padding(4);
            this.lstResults.Name = "lstResults";
            this.lstResults.Size = new System.Drawing.Size(455, 237);
            this.lstResults.TabIndex = 5;
            this.lstResults.SelectedIndexChanged += new System.EventHandler(this.lstResults_SelectedIndexChanged);
            this.lstResults.DoubleClick += new System.EventHandler(this.lstResults_DoubleClick);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tslResultCount});
            this.statusStrip1.Location = new System.Drawing.Point(0, 349);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(489, 22);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tslResultCount
            // 
            this.tslResultCount.Name = "tslResultCount";
            this.tslResultCount.Size = new System.Drawing.Size(474, 17);
            this.tslResultCount.Spring = true;
            // 
            // tabFindReplace
            // 
            this.tabFindReplace.Controls.Add(this.tabFind);
            this.tabFindReplace.Controls.Add(this.tabReplace);
            this.tabFindReplace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabFindReplace.Location = new System.Drawing.Point(6, 6);
            this.tabFindReplace.Name = "tabFindReplace";
            this.tabFindReplace.SelectedIndex = 0;
            this.tabFindReplace.Size = new System.Drawing.Size(477, 337);
            this.tabFindReplace.TabIndex = 7;
            this.tabFindReplace.SelectedIndexChanged += new System.EventHandler(this.tabFindReplace_SelectedIndexChanged);
            // 
            // tabFind
            // 
            this.tabFind.Controls.Add(this.label1);
            this.tabFind.Controls.Add(this.txtFindText);
            this.tabFind.Controls.Add(this.lstResults);
            this.tabFind.Controls.Add(this.btnFindText);
            this.tabFind.Controls.Add(this.chkMatchCase);
            this.tabFind.Controls.Add(this.btnCancel);
            this.tabFind.Location = new System.Drawing.Point(4, 22);
            this.tabFind.Name = "tabFind";
            this.tabFind.Padding = new System.Windows.Forms.Padding(3);
            this.tabFind.Size = new System.Drawing.Size(469, 311);
            this.tabFind.TabIndex = 0;
            this.tabFind.Text = "Find";
            this.tabFind.UseVisualStyleBackColor = true;
            // 
            // tabReplace
            // 
            this.tabReplace.Controls.Add(this.chkReplaceAll);
            this.tabReplace.Controls.Add(this.txtReplaceText);
            this.tabReplace.Controls.Add(this.label3);
            this.tabReplace.Controls.Add(this.label2);
            this.tabReplace.Controls.Add(this.txtFindTextReplace);
            this.tabReplace.Controls.Add(this.lstResultsReplace);
            this.tabReplace.Controls.Add(this.btnReplaceText);
            this.tabReplace.Controls.Add(this.chkMatchCaseReplace);
            this.tabReplace.Controls.Add(this.btnCancelReplace);
            this.tabReplace.Location = new System.Drawing.Point(4, 22);
            this.tabReplace.Name = "tabReplace";
            this.tabReplace.Padding = new System.Windows.Forms.Padding(3);
            this.tabReplace.Size = new System.Drawing.Size(469, 311);
            this.tabReplace.TabIndex = 1;
            this.tabReplace.Text = "Replace";
            this.tabReplace.UseVisualStyleBackColor = true;
            // 
            // chkReplaceAll
            // 
            this.chkReplaceAll.AutoSize = true;
            this.chkReplaceAll.Location = new System.Drawing.Point(255, 71);
            this.chkReplaceAll.Margin = new System.Windows.Forms.Padding(4);
            this.chkReplaceAll.Name = "chkReplaceAll";
            this.chkReplaceAll.Size = new System.Drawing.Size(124, 17);
            this.chkReplaceAll.TabIndex = 12;
            this.chkReplaceAll.Text = "Replace in all entries";
            this.chkReplaceAll.UseVisualStyleBackColor = true;
            this.chkReplaceAll.CheckedChanged += new System.EventHandler(this.chkReplaceAll_CheckedChanged);
            // 
            // txtReplaceText
            // 
            this.txtReplaceText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtReplaceText.Location = new System.Drawing.Point(87, 38);
            this.txtReplaceText.Margin = new System.Windows.Forms.Padding(4);
            this.txtReplaceText.Name = "txtReplaceText";
            this.txtReplaceText.Size = new System.Drawing.Size(292, 20);
            this.txtReplaceText.TabIndex = 9;
            this.txtReplaceText.TextChanged += new System.EventHandler(this.txtReplaceText_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 41);
            this.label3.Margin = new System.Windows.Forms.Padding(4);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(72, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "Replace with:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 10);
            this.label2.Margin = new System.Windows.Forms.Padding(4);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Find what:";
            // 
            // txtFindTextReplace
            // 
            this.txtFindTextReplace.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFindTextReplace.Location = new System.Drawing.Point(87, 7);
            this.txtFindTextReplace.Margin = new System.Windows.Forms.Padding(4);
            this.txtFindTextReplace.Name = "txtFindTextReplace";
            this.txtFindTextReplace.Size = new System.Drawing.Size(292, 20);
            this.txtFindTextReplace.TabIndex = 7;
            this.txtFindTextReplace.TextChanged += new System.EventHandler(this.txtFindText_TextChanged);
            // 
            // lstResultsReplace
            // 
            this.lstResultsReplace.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstResultsReplace.FormattingEnabled = true;
            this.lstResultsReplace.IntegralHeight = false;
            this.lstResultsReplace.Location = new System.Drawing.Point(7, 98);
            this.lstResultsReplace.Margin = new System.Windows.Forms.Padding(4);
            this.lstResultsReplace.Name = "lstResultsReplace";
            this.lstResultsReplace.Size = new System.Drawing.Size(455, 207);
            this.lstResultsReplace.TabIndex = 13;
            this.lstResultsReplace.DoubleClick += new System.EventHandler(this.lstResultsReplace_DoubleClick);
            // 
            // btnReplaceText
            // 
            this.btnReplaceText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReplaceText.Location = new System.Drawing.Point(387, 5);
            this.btnReplaceText.Margin = new System.Windows.Forms.Padding(4);
            this.btnReplaceText.Name = "btnReplaceText";
            this.btnReplaceText.Size = new System.Drawing.Size(75, 23);
            this.btnReplaceText.TabIndex = 8;
            this.btnReplaceText.Text = "Replace";
            this.btnReplaceText.UseVisualStyleBackColor = true;
            this.btnReplaceText.Click += new System.EventHandler(this.btnReplaceText_Click);
            // 
            // chkMatchCaseReplace
            // 
            this.chkMatchCaseReplace.AutoSize = true;
            this.chkMatchCaseReplace.Location = new System.Drawing.Point(87, 71);
            this.chkMatchCaseReplace.Margin = new System.Windows.Forms.Padding(4);
            this.chkMatchCaseReplace.Name = "chkMatchCaseReplace";
            this.chkMatchCaseReplace.Size = new System.Drawing.Size(82, 17);
            this.chkMatchCaseReplace.TabIndex = 11;
            this.chkMatchCaseReplace.Text = "Match case";
            this.chkMatchCaseReplace.UseVisualStyleBackColor = true;
            this.chkMatchCaseReplace.CheckedChanged += new System.EventHandler(this.chkMatchCase_CheckedChanged);
            // 
            // btnCancelReplace
            // 
            this.btnCancelReplace.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelReplace.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancelReplace.Location = new System.Drawing.Point(387, 36);
            this.btnCancelReplace.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancelReplace.Name = "btnCancelReplace";
            this.btnCancelReplace.Size = new System.Drawing.Size(75, 23);
            this.btnCancelReplace.TabIndex = 10;
            this.btnCancelReplace.Text = "Close";
            this.btnCancelReplace.UseVisualStyleBackColor = true;
            this.btnCancelReplace.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // pnlFind
            // 
            this.pnlFind.Controls.Add(this.tabFindReplace);
            this.pnlFind.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlFind.Location = new System.Drawing.Point(0, 0);
            this.pnlFind.Name = "pnlFind";
            this.pnlFind.Padding = new System.Windows.Forms.Padding(6);
            this.pnlFind.Size = new System.Drawing.Size(489, 349);
            this.pnlFind.TabIndex = 8;
            // 
            // Find
            // 
            this.AcceptButton = this.btnFindText;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(489, 371);
            this.Controls.Add(this.pnlFind);
            this.Controls.Add(this.statusStrip1);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(500, 400);
            this.Name = "Find";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Find";
            this.Load += new System.EventHandler(this.frmFind_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabFindReplace.ResumeLayout(false);
            this.tabFind.ResumeLayout(false);
            this.tabFind.PerformLayout();
            this.tabReplace.ResumeLayout(false);
            this.tabReplace.PerformLayout();
            this.pnlFind.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtFindText;
        private System.Windows.Forms.Button btnFindText;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox chkMatchCase;
        private System.Windows.Forms.ListBox lstResults;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel tslResultCount;
        private System.Windows.Forms.TabControl tabFindReplace;
        private System.Windows.Forms.TabPage tabFind;
        private System.Windows.Forms.TabPage tabReplace;
        private System.Windows.Forms.Panel pnlFind;
        private System.Windows.Forms.TextBox txtReplaceText;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtFindTextReplace;
        private System.Windows.Forms.Button btnReplaceText;
        private System.Windows.Forms.CheckBox chkMatchCaseReplace;
        private System.Windows.Forms.Button btnCancelReplace;
        private System.Windows.Forms.CheckBox chkReplaceAll;
        private System.Windows.Forms.ListBox lstResultsReplace;
    }
}