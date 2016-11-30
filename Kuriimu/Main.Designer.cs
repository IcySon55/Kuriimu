namespace Kuriimu
{
	partial class Main
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
			this.btnInjector = new System.Windows.Forms.Button();
			this.btnDirector = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// btnDumping
			// 
			this.btnDumping.Location = new System.Drawing.Point(160, 42);
			this.btnDumping.Name = "btnDumping";
			this.btnDumping.Size = new System.Drawing.Size(96, 96);
			this.btnDumping.TabIndex = 0;
			this.btnDumping.Text = "Dumping";
			this.btnDumping.UseVisualStyleBackColor = true;
			this.btnDumping.Click += new System.EventHandler(this.btnDumping_Click);
			// 
			// btnEditor
			// 
			this.btnEditor.Image = global::Kuriimu.Properties.Resources.btn_kuriimu;
			this.btnEditor.Location = new System.Drawing.Point(58, 42);
			this.btnEditor.Name = "btnEditor";
			this.btnEditor.Size = new System.Drawing.Size(96, 96);
			this.btnEditor.TabIndex = 1;
			this.btnEditor.Text = "Editor";
			this.btnEditor.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.btnEditor.UseVisualStyleBackColor = true;
			this.btnEditor.Click += new System.EventHandler(this.btnEditor_Click);
			// 
			// btnInjector
			// 
			this.btnInjector.Location = new System.Drawing.Point(262, 42);
			this.btnInjector.Name = "btnInjector";
			this.btnInjector.Size = new System.Drawing.Size(96, 96);
			this.btnInjector.TabIndex = 2;
			this.btnInjector.Text = "Injector";
			this.btnInjector.UseVisualStyleBackColor = true;
			// 
			// btnDirector
			// 
			this.btnDirector.Location = new System.Drawing.Point(160, 144);
			this.btnDirector.Name = "btnDirector";
			this.btnDirector.Size = new System.Drawing.Size(96, 30);
			this.btnDirector.TabIndex = 3;
			this.btnDirector.Text = "Director";
			this.btnDirector.UseVisualStyleBackColor = true;
			this.btnDirector.Click += new System.EventHandler(this.btnDirector_Click);
			// 
			// Main
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(433, 186);
			this.Controls.Add(this.btnDirector);
			this.Controls.Add(this.btnInjector);
			this.Controls.Add(this.btnEditor);
			this.Controls.Add(this.btnDumping);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "Main";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Kuriimu";
			this.Load += new System.EventHandler(this.Main_Load);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button btnDumping;
		private System.Windows.Forms.Button btnEditor;
		private System.Windows.Forms.Button btnInjector;
		private System.Windows.Forms.Button btnDirector;
	}
}