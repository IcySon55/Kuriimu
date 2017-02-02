namespace Kukkii
{
	partial class frmConverter
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
			this.mnuMain = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.gBATempToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.gitHubToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.aboutToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.tlsMain = new System.Windows.Forms.ToolStrip();
			this.tsbOpen = new System.Windows.Forms.ToolStripButton();
			this.tsbSave = new System.Windows.Forms.ToolStripButton();
			this.tsbSaveAs = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.pnlMain = new System.Windows.Forms.Panel();
			this.splMain = new System.Windows.Forms.SplitContainer();
			this.pnlPreview = new System.Windows.Forms.Panel();
			this.pbxPreview = new System.Windows.Forms.PictureBox();
			this.mnuMain.SuspendLayout();
			this.tlsMain.SuspendLayout();
			this.pnlMain.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splMain)).BeginInit();
			this.splMain.Panel1.SuspendLayout();
			this.splMain.SuspendLayout();
			this.pnlPreview.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbxPreview)).BeginInit();
			this.SuspendLayout();
			// 
			// mnuMain
			// 
			this.mnuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.aboutToolStripMenuItem});
			this.mnuMain.Location = new System.Drawing.Point(0, 0);
			this.mnuMain.Name = "mnuMain";
			this.mnuMain.Padding = new System.Windows.Forms.Padding(2, 2, 0, 2);
			this.mnuMain.Size = new System.Drawing.Size(984, 24);
			this.mnuMain.TabIndex = 1;
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "&File";
			// 
			// openToolStripMenuItem
			// 
			this.openToolStripMenuItem.Image = global::Kukkii.Properties.Resources.menu_open;
			this.openToolStripMenuItem.Name = "openToolStripMenuItem";
			this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.openToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
			this.openToolStripMenuItem.Text = "&Open";
			this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
			// 
			// saveToolStripMenuItem
			// 
			this.saveToolStripMenuItem.Enabled = false;
			this.saveToolStripMenuItem.Image = global::Kukkii.Properties.Resources.menu_save;
			this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
			this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.saveToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
			this.saveToolStripMenuItem.Text = "&Save";
			this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
			// 
			// saveAsToolStripMenuItem
			// 
			this.saveAsToolStripMenuItem.Enabled = false;
			this.saveAsToolStripMenuItem.Image = global::Kukkii.Properties.Resources.menu_save_as;
			this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
			this.saveAsToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F12;
			this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
			this.saveAsToolStripMenuItem.Text = "S&ave As...";
			this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(145, 6);
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Image = global::Kukkii.Properties.Resources.menu_exit;
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
			this.exitToolStripMenuItem.Text = "E&xit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
			// 
			// editToolStripMenuItem
			// 
			this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportToolStripMenuItem});
			this.editToolStripMenuItem.Name = "editToolStripMenuItem";
			this.editToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
			this.editToolStripMenuItem.Text = "&Image";
			// 
			// exportToolStripMenuItem
			// 
			this.exportToolStripMenuItem.Enabled = false;
			this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
			this.exportToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
			this.exportToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
			this.exportToolStripMenuItem.Text = "&Export to PNG";
			this.exportToolStripMenuItem.Click += new System.EventHandler(this.exportToolStripMenuItem_Click);
			// 
			// aboutToolStripMenuItem
			// 
			this.aboutToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gBATempToolStripMenuItem,
            this.gitHubToolStripMenuItem,
            this.aboutToolStripMenuItem1});
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			this.aboutToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
			this.aboutToolStripMenuItem.Text = "&Help";
			// 
			// gBATempToolStripMenuItem
			// 
			this.gBATempToolStripMenuItem.Image = global::Kukkii.Properties.Resources.menu_gbatemp;
			this.gBATempToolStripMenuItem.Name = "gBATempToolStripMenuItem";
			this.gBATempToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
			this.gBATempToolStripMenuItem.Text = "GBATemp";
			// 
			// gitHubToolStripMenuItem
			// 
			this.gitHubToolStripMenuItem.Image = global::Kukkii.Properties.Resources.menu_git;
			this.gitHubToolStripMenuItem.Name = "gitHubToolStripMenuItem";
			this.gitHubToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
			this.gitHubToolStripMenuItem.Text = "GitHub";
			// 
			// aboutToolStripMenuItem1
			// 
			this.aboutToolStripMenuItem1.Image = global::Kukkii.Properties.Resources.menu_about;
			this.aboutToolStripMenuItem1.Name = "aboutToolStripMenuItem1";
			this.aboutToolStripMenuItem1.ShortcutKeys = System.Windows.Forms.Keys.F1;
			this.aboutToolStripMenuItem1.Size = new System.Drawing.Size(128, 22);
			this.aboutToolStripMenuItem1.Text = "&About";
			// 
			// tlsMain
			// 
			this.tlsMain.AutoSize = false;
			this.tlsMain.BackColor = System.Drawing.Color.Transparent;
			this.tlsMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.tlsMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbOpen,
            this.tsbSave,
            this.tsbSaveAs,
            this.toolStripSeparator2});
			this.tlsMain.Location = new System.Drawing.Point(0, 24);
			this.tlsMain.Name = "tlsMain";
			this.tlsMain.Padding = new System.Windows.Forms.Padding(2, 1, 2, 1);
			this.tlsMain.Size = new System.Drawing.Size(984, 27);
			this.tlsMain.TabIndex = 3;
			// 
			// tsbOpen
			// 
			this.tsbOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbOpen.Image = global::Kukkii.Properties.Resources.menu_open;
			this.tsbOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbOpen.Name = "tsbOpen";
			this.tsbOpen.Size = new System.Drawing.Size(23, 22);
			this.tsbOpen.Text = "Open";
			this.tsbOpen.Click += new System.EventHandler(this.tsbOpen_Click);
			// 
			// tsbSave
			// 
			this.tsbSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbSave.Enabled = false;
			this.tsbSave.Image = global::Kukkii.Properties.Resources.menu_save;
			this.tsbSave.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbSave.Name = "tsbSave";
			this.tsbSave.Size = new System.Drawing.Size(23, 22);
			this.tsbSave.Text = "Save";
			this.tsbSave.Click += new System.EventHandler(this.tsbSave_Click);
			// 
			// tsbSaveAs
			// 
			this.tsbSaveAs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbSaveAs.Enabled = false;
			this.tsbSaveAs.Image = global::Kukkii.Properties.Resources.menu_save_as;
			this.tsbSaveAs.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbSaveAs.Name = "tsbSaveAs";
			this.tsbSaveAs.Size = new System.Drawing.Size(23, 22);
			this.tsbSaveAs.Text = "Save As...";
			this.tsbSaveAs.Click += new System.EventHandler(this.tsbSaveAs_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
			// 
			// pnlMain
			// 
			this.pnlMain.Controls.Add(this.splMain);
			this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlMain.Location = new System.Drawing.Point(0, 51);
			this.pnlMain.Name = "pnlMain";
			this.pnlMain.Padding = new System.Windows.Forms.Padding(6);
			this.pnlMain.Size = new System.Drawing.Size(984, 611);
			this.pnlMain.TabIndex = 4;
			// 
			// splMain
			// 
			this.splMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splMain.Location = new System.Drawing.Point(6, 6);
			this.splMain.Name = "splMain";
			// 
			// splMain.Panel1
			// 
			this.splMain.Panel1.Controls.Add(this.pnlPreview);
			this.splMain.Size = new System.Drawing.Size(972, 599);
			this.splMain.SplitterDistance = 740;
			this.splMain.TabIndex = 0;
			// 
			// pnlPreview
			// 
			this.pnlPreview.AutoScroll = true;
			this.pnlPreview.BackColor = System.Drawing.Color.DarkGray;
			this.pnlPreview.BackgroundImage = global::Kukkii.Properties.Resources.background;
			this.pnlPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pnlPreview.Controls.Add(this.pbxPreview);
			this.pnlPreview.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlPreview.Location = new System.Drawing.Point(0, 0);
			this.pnlPreview.Name = "pnlPreview";
			this.pnlPreview.Size = new System.Drawing.Size(740, 599);
			this.pnlPreview.TabIndex = 6;
			// 
			// pbxPreview
			// 
			this.pbxPreview.BackColor = System.Drawing.Color.Transparent;
			this.pbxPreview.Location = new System.Drawing.Point(0, 0);
			this.pbxPreview.Name = "pbxPreview";
			this.pbxPreview.Size = new System.Drawing.Size(128, 128);
			this.pbxPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.pbxPreview.TabIndex = 4;
			this.pbxPreview.TabStop = false;
			// 
			// frmConverter
			// 
			this.AllowDrop = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(984, 662);
			this.Controls.Add(this.pnlMain);
			this.Controls.Add(this.tlsMain);
			this.Controls.Add(this.mnuMain);
			this.Name = "frmConverter";
			this.Text = "Kukki";
			this.Load += new System.EventHandler(this.frmConverter_Load);
			this.DragDrop += new System.Windows.Forms.DragEventHandler(this.frmConverter_DragDrop);
			this.DragEnter += new System.Windows.Forms.DragEventHandler(this.frmConverter_DragEnter);
			this.Resize += new System.EventHandler(this.frmConverter_Resize);
			this.mnuMain.ResumeLayout(false);
			this.mnuMain.PerformLayout();
			this.tlsMain.ResumeLayout(false);
			this.tlsMain.PerformLayout();
			this.pnlMain.ResumeLayout(false);
			this.splMain.Panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splMain)).EndInit();
			this.splMain.ResumeLayout(false);
			this.pnlPreview.ResumeLayout(false);
			this.pnlPreview.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbxPreview)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip mnuMain;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem gBATempToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem gitHubToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem1;
		private System.Windows.Forms.ToolStrip tlsMain;
		private System.Windows.Forms.ToolStripButton tsbOpen;
		private System.Windows.Forms.ToolStripButton tsbSave;
		private System.Windows.Forms.ToolStripButton tsbSaveAs;
		private System.Windows.Forms.Panel pnlMain;
		private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
		private System.Windows.Forms.SplitContainer splMain;
		private System.Windows.Forms.Panel pnlPreview;
		private System.Windows.Forms.PictureBox pbxPreview;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
	}
}

