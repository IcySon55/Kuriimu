namespace Kuriimu
{
	partial class frmDirector
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
			this.txtFile = new System.Windows.Forms.TextBox();
			this.btnBrowse = new System.Windows.Forms.Button();
			this.txtOffset = new System.Windows.Forms.TextBox();
			this.btnLookup = new System.Windows.Forms.Button();
			this.txtResults = new System.Windows.Forms.TextBox();
			this.txtLeneance = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// txtFile
			// 
			this.txtFile.Location = new System.Drawing.Point(12, 12);
			this.txtFile.Name = "txtFile";
			this.txtFile.Size = new System.Drawing.Size(407, 20);
			this.txtFile.TabIndex = 0;
			this.txtFile.TextChanged += new System.EventHandler(this.txtFile_TextChanged);
			// 
			// btnBrowse
			// 
			this.btnBrowse.Location = new System.Drawing.Point(425, 11);
			this.btnBrowse.Name = "btnBrowse";
			this.btnBrowse.Size = new System.Drawing.Size(94, 22);
			this.btnBrowse.TabIndex = 1;
			this.btnBrowse.Text = "...";
			this.btnBrowse.UseVisualStyleBackColor = true;
			this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
			// 
			// txtOffset
			// 
			this.txtOffset.Location = new System.Drawing.Point(12, 38);
			this.txtOffset.Name = "txtOffset";
			this.txtOffset.Size = new System.Drawing.Size(305, 20);
			this.txtOffset.TabIndex = 2;
			this.txtOffset.TextChanged += new System.EventHandler(this.txtOffset_TextChanged);
			// 
			// btnLookup
			// 
			this.btnLookup.Location = new System.Drawing.Point(425, 37);
			this.btnLookup.Name = "btnLookup";
			this.btnLookup.Size = new System.Drawing.Size(94, 22);
			this.btnLookup.TabIndex = 3;
			this.btnLookup.Text = "Lookup!";
			this.btnLookup.UseVisualStyleBackColor = true;
			this.btnLookup.Click += new System.EventHandler(this.btnLookup_Click);
			// 
			// txtResults
			// 
			this.txtResults.Location = new System.Drawing.Point(12, 64);
			this.txtResults.Multiline = true;
			this.txtResults.Name = "txtResults";
			this.txtResults.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtResults.Size = new System.Drawing.Size(507, 308);
			this.txtResults.TabIndex = 4;
			// 
			// txtLeneance
			// 
			this.txtLeneance.Location = new System.Drawing.Point(323, 38);
			this.txtLeneance.Name = "txtLeneance";
			this.txtLeneance.Size = new System.Drawing.Size(96, 20);
			this.txtLeneance.TabIndex = 5;
			this.txtLeneance.TextChanged += new System.EventHandler(this.txtLeneance_TextChanged);
			// 
			// frmDirector
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(531, 384);
			this.Controls.Add(this.txtLeneance);
			this.Controls.Add(this.txtResults);
			this.Controls.Add(this.btnLookup);
			this.Controls.Add(this.txtOffset);
			this.Controls.Add(this.btnBrowse);
			this.Controls.Add(this.txtFile);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.Name = "frmDirector";
			this.Text = "Director";
			this.Load += new System.EventHandler(this.Director_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox txtFile;
		private System.Windows.Forms.Button btnBrowse;
		private System.Windows.Forms.TextBox txtOffset;
		private System.Windows.Forms.Button btnLookup;
		private System.Windows.Forms.TextBox txtResults;
		private System.Windows.Forms.TextBox txtLeneance;
	}
}