namespace Kuriimu
{
	partial class Editor
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
			this.fIndToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.gBATempToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.gitHubToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.aboutToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.pnlMain = new System.Windows.Forms.Panel();
			this.splMain = new System.Windows.Forms.SplitContainer();
			this.lstEntries = new System.Windows.Forms.ListBox();
			this.tlsEntries = new System.Windows.Forms.ToolStrip();
			this.tslEntries = new System.Windows.Forms.ToolStripLabel();
			this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
			this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
			this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
			this.tsbProperties = new System.Windows.Forms.ToolStripButton();
			this.splContent = new System.Windows.Forms.SplitContainer();
			this.splText = new System.Windows.Forms.SplitContainer();
			this.txtEdit = new System.Windows.Forms.TextBox();
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.tslText = new System.Windows.Forms.ToolStripLabel();
			this.toolStripSplitButton1 = new System.Windows.Forms.ToolStripDropDownButton();
			this.manageProfilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.txtOriginal = new System.Windows.Forms.TextBox();
			this.toolStrip2 = new System.Windows.Forms.ToolStrip();
			this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
			this.mnuMain.SuspendLayout();
			this.pnlMain.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splMain)).BeginInit();
			this.splMain.Panel1.SuspendLayout();
			this.splMain.Panel2.SuspendLayout();
			this.splMain.SuspendLayout();
			this.tlsEntries.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splContent)).BeginInit();
			this.splContent.Panel1.SuspendLayout();
			this.splContent.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splText)).BeginInit();
			this.splText.Panel1.SuspendLayout();
			this.splText.Panel2.SuspendLayout();
			this.splText.SuspendLayout();
			this.toolStrip1.SuspendLayout();
			this.toolStrip2.SuspendLayout();
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
			this.mnuMain.Size = new System.Drawing.Size(1113, 24);
			this.mnuMain.TabIndex = 0;
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
			this.openToolStripMenuItem.Image = global::Kuriimu.Properties.Resources.menu_open;
			this.openToolStripMenuItem.Name = "openToolStripMenuItem";
			this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.openToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
			this.openToolStripMenuItem.Text = "&Open";
			this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
			// 
			// saveToolStripMenuItem
			// 
			this.saveToolStripMenuItem.Image = global::Kuriimu.Properties.Resources.menu_save;
			this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
			this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.saveToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
			this.saveToolStripMenuItem.Text = "&Save";
			this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
			// 
			// saveAsToolStripMenuItem
			// 
			this.saveAsToolStripMenuItem.Image = global::Kuriimu.Properties.Resources.menu_save_as;
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
			this.exitToolStripMenuItem.Image = global::Kuriimu.Properties.Resources.menu_exit;
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
			this.exitToolStripMenuItem.Text = "E&xit";
			// 
			// editToolStripMenuItem
			// 
			this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fIndToolStripMenuItem});
			this.editToolStripMenuItem.Name = "editToolStripMenuItem";
			this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
			this.editToolStripMenuItem.Text = "&Edit";
			// 
			// fIndToolStripMenuItem
			// 
			this.fIndToolStripMenuItem.Image = global::Kuriimu.Properties.Resources.menu_find;
			this.fIndToolStripMenuItem.Name = "fIndToolStripMenuItem";
			this.fIndToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
			this.fIndToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
			this.fIndToolStripMenuItem.Text = "&Find";
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
			this.gBATempToolStripMenuItem.Image = global::Kuriimu.Properties.Resources.menu_gbatemp;
			this.gBATempToolStripMenuItem.Name = "gBATempToolStripMenuItem";
			this.gBATempToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.gBATempToolStripMenuItem.Text = "GBATemp";
			// 
			// gitHubToolStripMenuItem
			// 
			this.gitHubToolStripMenuItem.Image = global::Kuriimu.Properties.Resources.menu_git;
			this.gitHubToolStripMenuItem.Name = "gitHubToolStripMenuItem";
			this.gitHubToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.gitHubToolStripMenuItem.Text = "GitHub";
			// 
			// aboutToolStripMenuItem1
			// 
			this.aboutToolStripMenuItem1.Image = global::Kuriimu.Properties.Resources.menu_about;
			this.aboutToolStripMenuItem1.Name = "aboutToolStripMenuItem1";
			this.aboutToolStripMenuItem1.ShortcutKeys = System.Windows.Forms.Keys.F1;
			this.aboutToolStripMenuItem1.Size = new System.Drawing.Size(152, 22);
			this.aboutToolStripMenuItem1.Text = "&About";
			// 
			// pnlMain
			// 
			this.pnlMain.Controls.Add(this.splMain);
			this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlMain.Location = new System.Drawing.Point(0, 24);
			this.pnlMain.Name = "pnlMain";
			this.pnlMain.Padding = new System.Windows.Forms.Padding(6);
			this.pnlMain.Size = new System.Drawing.Size(1113, 644);
			this.pnlMain.TabIndex = 1;
			// 
			// splMain
			// 
			this.splMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splMain.Location = new System.Drawing.Point(6, 6);
			this.splMain.Name = "splMain";
			// 
			// splMain.Panel1
			// 
			this.splMain.Panel1.Controls.Add(this.lstEntries);
			this.splMain.Panel1.Controls.Add(this.tlsEntries);
			// 
			// splMain.Panel2
			// 
			this.splMain.Panel2.Controls.Add(this.splContent);
			this.splMain.Size = new System.Drawing.Size(1101, 632);
			this.splMain.SplitterDistance = 256;
			this.splMain.SplitterWidth = 6;
			this.splMain.TabIndex = 0;
			// 
			// lstEntries
			// 
			this.lstEntries.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lstEntries.FormattingEnabled = true;
			this.lstEntries.IntegralHeight = false;
			this.lstEntries.Location = new System.Drawing.Point(0, 27);
			this.lstEntries.Name = "lstEntries";
			this.lstEntries.Size = new System.Drawing.Size(256, 605);
			this.lstEntries.Sorted = true;
			this.lstEntries.TabIndex = 1;
			this.lstEntries.SelectedIndexChanged += new System.EventHandler(this.lstEntries_SelectedIndexChanged);
			this.lstEntries.KeyUp += new System.Windows.Forms.KeyEventHandler(this.lstEntries_KeyUp);
			// 
			// tlsEntries
			// 
			this.tlsEntries.AutoSize = false;
			this.tlsEntries.BackColor = System.Drawing.Color.Transparent;
			this.tlsEntries.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.tlsEntries.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tslEntries,
            this.toolStripButton1,
            this.toolStripButton2,
            this.toolStripButton3,
            this.tsbProperties});
			this.tlsEntries.Location = new System.Drawing.Point(0, 0);
			this.tlsEntries.Name = "tlsEntries";
			this.tlsEntries.Padding = new System.Windows.Forms.Padding(2, 1, 2, 1);
			this.tlsEntries.Size = new System.Drawing.Size(256, 27);
			this.tlsEntries.TabIndex = 0;
			// 
			// tslEntries
			// 
			this.tslEntries.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.tslEntries.Name = "tslEntries";
			this.tslEntries.Size = new System.Drawing.Size(42, 22);
			this.tslEntries.Text = "Entries";
			// 
			// toolStripButton1
			// 
			this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButton1.Enabled = false;
			this.toolStripButton1.Image = global::Kuriimu.Properties.Resources.menu_add;
			this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton1.Name = "toolStripButton1";
			this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
			this.toolStripButton1.Text = "Add Entry";
			// 
			// toolStripButton2
			// 
			this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButton2.Enabled = false;
			this.toolStripButton2.Image = global::Kuriimu.Properties.Resources.menu_field_properties;
			this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton2.Name = "toolStripButton2";
			this.toolStripButton2.Size = new System.Drawing.Size(23, 22);
			this.toolStripButton2.Text = "Rename Entry";
			// 
			// toolStripButton3
			// 
			this.toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButton3.Enabled = false;
			this.toolStripButton3.Image = global::Kuriimu.Properties.Resources.menu_delete;
			this.toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton3.Name = "toolStripButton3";
			this.toolStripButton3.Size = new System.Drawing.Size(23, 22);
			this.toolStripButton3.Text = "Delete Entry";
			// 
			// tsbProperties
			// 
			this.tsbProperties.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbProperties.Enabled = false;
			this.tsbProperties.Image = global::Kuriimu.Properties.Resources.menu_properties;
			this.tsbProperties.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbProperties.Name = "tsbProperties";
			this.tsbProperties.Size = new System.Drawing.Size(23, 22);
			this.tsbProperties.Text = "Entry Properties";
			this.tsbProperties.Click += new System.EventHandler(this.tsbProperties_Click);
			// 
			// splContent
			// 
			this.splContent.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splContent.Location = new System.Drawing.Point(0, 0);
			this.splContent.Name = "splContent";
			this.splContent.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splContent.Panel1
			// 
			this.splContent.Panel1.Controls.Add(this.splText);
			this.splContent.Size = new System.Drawing.Size(839, 632);
			this.splContent.SplitterDistance = 309;
			this.splContent.SplitterWidth = 6;
			this.splContent.TabIndex = 0;
			// 
			// splText
			// 
			this.splText.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splText.Location = new System.Drawing.Point(0, 0);
			this.splText.Name = "splText";
			// 
			// splText.Panel1
			// 
			this.splText.Panel1.Controls.Add(this.txtEdit);
			this.splText.Panel1.Controls.Add(this.toolStrip1);
			// 
			// splText.Panel2
			// 
			this.splText.Panel2.Controls.Add(this.txtOriginal);
			this.splText.Panel2.Controls.Add(this.toolStrip2);
			this.splText.Size = new System.Drawing.Size(839, 309);
			this.splText.SplitterDistance = 416;
			this.splText.SplitterWidth = 6;
			this.splText.TabIndex = 2;
			// 
			// txtEdit
			// 
			this.txtEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtEdit.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtEdit.Location = new System.Drawing.Point(0, 27);
			this.txtEdit.Multiline = true;
			this.txtEdit.Name = "txtEdit";
			this.txtEdit.Size = new System.Drawing.Size(416, 282);
			this.txtEdit.TabIndex = 0;
			this.txtEdit.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtEdit_KeyUp);
			// 
			// toolStrip1
			// 
			this.toolStrip1.AutoSize = false;
			this.toolStrip1.BackColor = System.Drawing.Color.Transparent;
			this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tslText,
            this.toolStripSplitButton1});
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Padding = new System.Windows.Forms.Padding(2, 1, 2, 1);
			this.toolStrip1.Size = new System.Drawing.Size(416, 27);
			this.toolStrip1.TabIndex = 2;
			// 
			// tslText
			// 
			this.tslText.Name = "tslText";
			this.tslText.Size = new System.Drawing.Size(32, 22);
			this.tslText.Text = "Text:";
			// 
			// toolStripSplitButton1
			// 
			this.toolStripSplitButton1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.toolStripSplitButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.manageProfilesToolStripMenuItem,
            this.toolStripSeparator2});
			this.toolStripSplitButton1.Image = global::Kuriimu.Properties.Resources.game_none;
			this.toolStripSplitButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripSplitButton1.Name = "toolStripSplitButton1";
			this.toolStripSplitButton1.Size = new System.Drawing.Size(140, 22);
			this.toolStripSplitButton1.Text = "No game selected...";
			// 
			// manageProfilesToolStripMenuItem
			// 
			this.manageProfilesToolStripMenuItem.Image = global::Kuriimu.Properties.Resources.menu_manage;
			this.manageProfilesToolStripMenuItem.Name = "manageProfilesToolStripMenuItem";
			this.manageProfilesToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F9;
			this.manageProfilesToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
			this.manageProfilesToolStripMenuItem.Text = "&Manage Profiles";
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(175, 6);
			// 
			// txtOriginal
			// 
			this.txtOriginal.BackColor = System.Drawing.SystemColors.Window;
			this.txtOriginal.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtOriginal.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtOriginal.Location = new System.Drawing.Point(0, 27);
			this.txtOriginal.Multiline = true;
			this.txtOriginal.Name = "txtOriginal";
			this.txtOriginal.ReadOnly = true;
			this.txtOriginal.Size = new System.Drawing.Size(417, 282);
			this.txtOriginal.TabIndex = 4;
			// 
			// toolStrip2
			// 
			this.toolStrip2.AutoSize = false;
			this.toolStrip2.BackColor = System.Drawing.Color.Transparent;
			this.toolStrip2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1});
			this.toolStrip2.Location = new System.Drawing.Point(0, 0);
			this.toolStrip2.Name = "toolStrip2";
			this.toolStrip2.Padding = new System.Windows.Forms.Padding(2, 1, 2, 1);
			this.toolStrip2.Size = new System.Drawing.Size(417, 27);
			this.toolStrip2.TabIndex = 3;
			// 
			// toolStripLabel1
			// 
			this.toolStripLabel1.Name = "toolStripLabel1";
			this.toolStripLabel1.Size = new System.Drawing.Size(52, 22);
			this.toolStripLabel1.Text = "Original:";
			// 
			// Editor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1113, 668);
			this.Controls.Add(this.pnlMain);
			this.Controls.Add(this.mnuMain);
			this.MainMenuStrip = this.mnuMain;
			this.Name = "Editor";
			this.Text = "Kuriimu Editor";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Editor_FormClosed);
			this.Load += new System.EventHandler(this.Editor_Load);
			this.mnuMain.ResumeLayout(false);
			this.mnuMain.PerformLayout();
			this.pnlMain.ResumeLayout(false);
			this.splMain.Panel1.ResumeLayout(false);
			this.splMain.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splMain)).EndInit();
			this.splMain.ResumeLayout(false);
			this.tlsEntries.ResumeLayout(false);
			this.tlsEntries.PerformLayout();
			this.splContent.Panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splContent)).EndInit();
			this.splContent.ResumeLayout(false);
			this.splText.Panel1.ResumeLayout(false);
			this.splText.Panel1.PerformLayout();
			this.splText.Panel2.ResumeLayout(false);
			this.splText.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splText)).EndInit();
			this.splText.ResumeLayout(false);
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.toolStrip2.ResumeLayout(false);
			this.toolStrip2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip mnuMain;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
		private System.Windows.Forms.Panel pnlMain;
		private System.Windows.Forms.SplitContainer splMain;
		private System.Windows.Forms.ToolStrip tlsEntries;
		private System.Windows.Forms.ListBox lstEntries;
		private System.Windows.Forms.ToolStripLabel tslEntries;
		private System.Windows.Forms.ToolStripButton toolStripButton1;
		private System.Windows.Forms.ToolStripButton toolStripButton2;
		private System.Windows.Forms.ToolStripButton toolStripButton3;
		private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
		private System.Windows.Forms.SplitContainer splContent;
		private System.Windows.Forms.TextBox txtEdit;
		private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
		private System.Windows.Forms.SplitContainer splText;
		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripLabel tslText;
		private System.Windows.Forms.ToolStrip toolStrip2;
		private System.Windows.Forms.ToolStripLabel toolStripLabel1;
		private System.Windows.Forms.TextBox txtOriginal;
		private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
		private System.Windows.Forms.ToolStripButton tsbProperties;
		private System.Windows.Forms.ToolStripMenuItem fIndToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripDropDownButton toolStripSplitButton1;
		private System.Windows.Forms.ToolStripMenuItem manageProfilesToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem gBATempToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem gitHubToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem1;
	}
}