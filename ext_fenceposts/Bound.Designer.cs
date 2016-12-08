namespace ext_fenceposts
{
	partial class frmBound
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
			this.lblStart = new System.Windows.Forms.Label();
			this.txtStart = new System.Windows.Forms.TextBox();
			this.btnOK = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.txtEnd = new System.Windows.Forms.TextBox();
			this.lblEnd = new System.Windows.Forms.Label();
			this.txtNotes = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.chkInjectable = new System.Windows.Forms.CheckBox();
			this.chkDumpable = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// lblStart
			// 
			this.lblStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
			this.lblStart.Location = new System.Drawing.Point(8, 10);
			this.lblStart.Name = "lblStart";
			this.lblStart.Size = new System.Drawing.Size(90, 22);
			this.lblStart.TabIndex = 0;
			this.lblStart.Text = "Start Offset:";
			this.lblStart.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// txtStart
			// 
			this.txtStart.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtStart.Location = new System.Drawing.Point(104, 12);
			this.txtStart.Name = "txtStart";
			this.txtStart.Size = new System.Drawing.Size(221, 20);
			this.txtStart.TabIndex = 1;
			// 
			// btnOK
			// 
			this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOK.Location = new System.Drawing.Point(234, 194);
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
			this.btnCancel.Location = new System.Drawing.Point(315, 194);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 7;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// txtEnd
			// 
			this.txtEnd.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtEnd.Location = new System.Drawing.Point(104, 38);
			this.txtEnd.Name = "txtEnd";
			this.txtEnd.Size = new System.Drawing.Size(221, 20);
			this.txtEnd.TabIndex = 2;
			// 
			// lblEnd
			// 
			this.lblEnd.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
			this.lblEnd.Location = new System.Drawing.Point(8, 36);
			this.lblEnd.Name = "lblEnd";
			this.lblEnd.Size = new System.Drawing.Size(90, 22);
			this.lblEnd.TabIndex = 4;
			this.lblEnd.Text = "End Offset:";
			this.lblEnd.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// txtNotes
			// 
			this.txtNotes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtNotes.Location = new System.Drawing.Point(104, 64);
			this.txtNotes.Multiline = true;
			this.txtNotes.Name = "txtNotes";
			this.txtNotes.Size = new System.Drawing.Size(221, 74);
			this.txtNotes.TabIndex = 3;
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
			this.label1.Location = new System.Drawing.Point(8, 62);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(90, 22);
			this.label1.TabIndex = 6;
			this.label1.Text = "Notes:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// chkInjectable
			// 
			this.chkInjectable.Location = new System.Drawing.Point(104, 169);
			this.chkInjectable.Name = "chkInjectable";
			this.chkInjectable.Size = new System.Drawing.Size(154, 19);
			this.chkInjectable.TabIndex = 5;
			this.chkInjectable.Text = "Injectable Space";
			this.chkInjectable.UseVisualStyleBackColor = true;
			// 
			// chkDumpable
			// 
			this.chkDumpable.Location = new System.Drawing.Point(104, 144);
			this.chkDumpable.Name = "chkDumpable";
			this.chkDumpable.Size = new System.Drawing.Size(154, 19);
			this.chkDumpable.TabIndex = 4;
			this.chkDumpable.Text = "Dumpable Strings";
			this.chkDumpable.UseVisualStyleBackColor = true;
			// 
			// frmBound
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(396, 224);
			this.Controls.Add(this.chkDumpable);
			this.Controls.Add(this.chkInjectable);
			this.Controls.Add(this.txtNotes);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.txtEnd);
			this.Controls.Add(this.lblEnd);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.txtStart);
			this.Controls.Add(this.lblStart);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmBound";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Bound";
			this.Load += new System.EventHandler(this.frmBound_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblStart;
		private System.Windows.Forms.TextBox txtStart;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.TextBox txtEnd;
		private System.Windows.Forms.Label lblEnd;
		private System.Windows.Forms.TextBox txtNotes;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.CheckBox chkInjectable;
		private System.Windows.Forms.CheckBox chkDumpable;
	}
}