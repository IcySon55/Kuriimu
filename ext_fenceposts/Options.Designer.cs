namespace ext_fenceposts
{
	partial class frmOptions
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
			this.lblMemoryOffset = new System.Windows.Forms.Label();
			this.txtMemoryOffset = new System.Windows.Forms.TextBox();
			this.btnOK = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.chkOptimizeStrings = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// lblMemoryOffset
			// 
			this.lblMemoryOffset.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
			this.lblMemoryOffset.Location = new System.Drawing.Point(8, 10);
			this.lblMemoryOffset.Name = "lblMemoryOffset";
			this.lblMemoryOffset.Size = new System.Drawing.Size(106, 22);
			this.lblMemoryOffset.TabIndex = 0;
			this.lblMemoryOffset.Text = "Memory Offset:";
			this.lblMemoryOffset.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// txtMemoryOffset
			// 
			this.txtMemoryOffset.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtMemoryOffset.Location = new System.Drawing.Point(120, 12);
			this.txtMemoryOffset.Name = "txtMemoryOffset";
			this.txtMemoryOffset.Size = new System.Drawing.Size(205, 20);
			this.txtMemoryOffset.TabIndex = 1;
			// 
			// btnOK
			// 
			this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOK.Location = new System.Drawing.Point(234, 62);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(75, 23);
			this.btnOK.TabIndex = 6;
			this.btnOK.Text = "OK";
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.Location = new System.Drawing.Point(315, 62);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 7;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// chkOptimizeStrings
			// 
			this.chkOptimizeStrings.Location = new System.Drawing.Point(120, 38);
			this.chkOptimizeStrings.Name = "chkOptimizeStrings";
			this.chkOptimizeStrings.Size = new System.Drawing.Size(138, 19);
			this.chkOptimizeStrings.TabIndex = 4;
			this.chkOptimizeStrings.Text = "Optimize Strings";
			this.chkOptimizeStrings.UseVisualStyleBackColor = true;
			// 
			// frmOptions
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(396, 92);
			this.Controls.Add(this.chkOptimizeStrings);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.txtMemoryOffset);
			this.Controls.Add(this.lblMemoryOffset);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmOptions";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Bound";
			this.Load += new System.EventHandler(this.frmBound_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblMemoryOffset;
		private System.Windows.Forms.TextBox txtMemoryOffset;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.CheckBox chkOptimizeStrings;
	}
}