namespace Kuriimu
{
	partial class Search
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
			this.statusStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 13);
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
			this.txtFindText.Location = new System.Drawing.Point(74, 10);
			this.txtFindText.Margin = new System.Windows.Forms.Padding(4);
			this.txtFindText.Name = "txtFindText";
			this.txtFindText.Size = new System.Drawing.Size(320, 20);
			this.txtFindText.TabIndex = 1;
			this.txtFindText.TextChanged += new System.EventHandler(this.txtFindText_TextChanged);
			// 
			// btnFindText
			// 
			this.btnFindText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnFindText.Location = new System.Drawing.Point(402, 8);
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
			this.btnCancel.Location = new System.Drawing.Point(402, 39);
			this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 4;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// chkMatchCase
			// 
			this.chkMatchCase.AutoSize = true;
			this.chkMatchCase.Location = new System.Drawing.Point(15, 43);
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
			this.lstResults.Location = new System.Drawing.Point(13, 70);
			this.lstResults.Margin = new System.Windows.Forms.Padding(4);
			this.lstResults.Name = "lstResults";
			this.lstResults.Size = new System.Drawing.Size(458, 274);
			this.lstResults.TabIndex = 5;
			this.lstResults.DoubleClick += new System.EventHandler(this.lstResults_DoubleClick);
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tslResultCount});
			this.statusStrip1.Location = new System.Drawing.Point(0, 358);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(484, 22);
			this.statusStrip1.TabIndex = 6;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// tslResultCount
			// 
			this.tslResultCount.Name = "tslResultCount";
			this.tslResultCount.Size = new System.Drawing.Size(469, 17);
			this.tslResultCount.Spring = true;
			// 
			// Search
			// 
			this.AcceptButton = this.btnFindText;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(484, 380);
			this.Controls.Add(this.statusStrip1);
			this.Controls.Add(this.lstResults);
			this.Controls.Add(this.chkMatchCase);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnFindText);
			this.Controls.Add(this.txtFindText);
			this.Controls.Add(this.label1);
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Search";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Find";
			this.Load += new System.EventHandler(this.frmSearch_Load);
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
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
	}
}