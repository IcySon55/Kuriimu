namespace Kuriimu
{
	partial class frmMain
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
			this.btnDumping = new System.Windows.Forms.Button();
			this.btnEditor = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// btnDumping
			// 
			this.btnDumping.Image = global::Kuriimu.Properties.Resources.btn_extensions;
			this.btnDumping.Location = new System.Drawing.Point(142, 32);
			this.btnDumping.Name = "btnDumping";
			this.btnDumping.Size = new System.Drawing.Size(104, 104);
			this.btnDumping.TabIndex = 0;
			this.btnDumping.Text = "Extensions";
			this.btnDumping.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.btnDumping.UseVisualStyleBackColor = true;
			this.btnDumping.Click += new System.EventHandler(this.btnDumping_Click);
			// 
			// btnEditor
			// 
			this.btnEditor.Image = global::Kuriimu.Properties.Resources.btn_editor;
			this.btnEditor.Location = new System.Drawing.Point(32, 32);
			this.btnEditor.Name = "btnEditor";
			this.btnEditor.Size = new System.Drawing.Size(104, 104);
			this.btnEditor.TabIndex = 1;
			this.btnEditor.Text = "Editor";
			this.btnEditor.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.btnEditor.UseVisualStyleBackColor = true;
			this.btnEditor.Click += new System.EventHandler(this.btnEditor_Click);
			// 
			// frmMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(291, 170);
			this.Controls.Add(this.btnEditor);
			this.Controls.Add(this.btnDumping);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "frmMain";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Kuriimu";
			this.Load += new System.EventHandler(this.Main_Load);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button btnDumping;
		private System.Windows.Forms.Button btnEditor;
	}
}