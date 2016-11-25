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
			this.tsbRename = new System.Windows.Forms.ToolStripButton();
			this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
			this.tsbProperties = new System.Windows.Forms.ToolStripButton();
			this.splContent = new System.Windows.Forms.SplitContainer();
			this.splText = new System.Windows.Forms.SplitContainer();
			this.txtEdit = new System.Windows.Forms.TextBox();
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.tslText = new System.Windows.Forms.ToolStripLabel();
			this.tsbGameSelect = new System.Windows.Forms.ToolStripDropDownButton();
			this.txtOriginal = new System.Windows.Forms.TextBox();
			this.toolStrip2 = new System.Windows.Forms.ToolStrip();
			this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
			this.splPreview = new System.Windows.Forms.SplitContainer();
			this.ptbPreview = new System.Windows.Forms.PictureBox();
			this.toolStrip3 = new System.Windows.Forms.ToolStrip();
			this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
			this.hbxHexView = new Be.Windows.Forms.HexBox();
			this.toolStrip4 = new System.Windows.Forms.ToolStrip();
			this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
			this.mnuMain.SuspendLayout();
			this.pnlMain.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splMain)).BeginInit();
			this.splMain.Panel1.SuspendLayout();
			this.splMain.Panel2.SuspendLayout();
			this.splMain.SuspendLayout();
			this.tlsEntries.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splContent)).BeginInit();
			this.splContent.Panel1.SuspendLayout();
			this.splContent.Panel2.SuspendLayout();
			this.splContent.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splText)).BeginInit();
			this.splText.Panel1.SuspendLayout();
			this.splText.Panel2.SuspendLayout();
			this.splText.SuspendLayout();
			this.toolStrip1.SuspendLayout();
			this.toolStrip2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splPreview)).BeginInit();
			this.splPreview.Panel1.SuspendLayout();
			this.splPreview.Panel2.SuspendLayout();
			this.splPreview.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ptbPreview)).BeginInit();
			this.toolStrip3.SuspendLayout();
			this.toolStrip4.SuspendLayout();
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
			this.gBATempToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
			this.gBATempToolStripMenuItem.Text = "GBATemp";
			// 
			// gitHubToolStripMenuItem
			// 
			this.gitHubToolStripMenuItem.Image = global::Kuriimu.Properties.Resources.menu_git;
			this.gitHubToolStripMenuItem.Name = "gitHubToolStripMenuItem";
			this.gitHubToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
			this.gitHubToolStripMenuItem.Text = "GitHub";
			// 
			// aboutToolStripMenuItem1
			// 
			this.aboutToolStripMenuItem1.Image = global::Kuriimu.Properties.Resources.menu_about;
			this.aboutToolStripMenuItem1.Name = "aboutToolStripMenuItem1";
			this.aboutToolStripMenuItem1.ShortcutKeys = System.Windows.Forms.Keys.F1;
			this.aboutToolStripMenuItem1.Size = new System.Drawing.Size(128, 22);
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
            this.tsbRename,
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
			this.tslEntries.Name = "tslEntries";
			this.tslEntries.Size = new System.Drawing.Size(45, 22);
			this.tslEntries.Text = "Entries:";
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
			// tsbRename
			// 
			this.tsbRename.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbRename.Enabled = false;
			this.tsbRename.Image = global::Kuriimu.Properties.Resources.menu_field_properties;
			this.tsbRename.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbRename.Name = "tsbRename";
			this.tsbRename.Size = new System.Drawing.Size(23, 22);
			this.tsbRename.Text = "Rename Entry";
			this.tsbRename.Click += new System.EventHandler(this.tsbRename_Click);
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
			// 
			// splContent.Panel2
			// 
			this.splContent.Panel2.Controls.Add(this.splPreview);
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
            this.tsbGameSelect});
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
			// tsbGameSelect
			// 
			this.tsbGameSelect.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.tsbGameSelect.Enabled = false;
			this.tsbGameSelect.Image = global::Kuriimu.Properties.Resources.game_none;
			this.tsbGameSelect.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbGameSelect.Name = "tsbGameSelect";
			this.tsbGameSelect.Size = new System.Drawing.Size(140, 22);
			this.tsbGameSelect.Text = "No game selected...";
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
			// splPreview
			// 
			this.splPreview.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splPreview.Location = new System.Drawing.Point(0, 0);
			this.splPreview.Name = "splPreview";
			// 
			// splPreview.Panel1
			// 
			this.splPreview.Panel1.Controls.Add(this.ptbPreview);
			this.splPreview.Panel1.Controls.Add(this.toolStrip3);
			// 
			// splPreview.Panel2
			// 
			this.splPreview.Panel2.Controls.Add(this.hbxHexView);
			this.splPreview.Panel2.Controls.Add(this.toolStrip4);
			this.splPreview.Size = new System.Drawing.Size(839, 317);
			this.splPreview.SplitterDistance = 416;
			this.splPreview.SplitterWidth = 6;
			this.splPreview.TabIndex = 0;
			// 
			// ptbPreview
			// 
			this.ptbPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.ptbPreview.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ptbPreview.Location = new System.Drawing.Point(0, 27);
			this.ptbPreview.Name = "ptbPreview";
			this.ptbPreview.Size = new System.Drawing.Size(416, 290);
			this.ptbPreview.TabIndex = 4;
			this.ptbPreview.TabStop = false;
			// 
			// toolStrip3
			// 
			this.toolStrip3.AutoSize = false;
			this.toolStrip3.BackColor = System.Drawing.Color.Transparent;
			this.toolStrip3.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip3.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel2});
			this.toolStrip3.Location = new System.Drawing.Point(0, 0);
			this.toolStrip3.Name = "toolStrip3";
			this.toolStrip3.Padding = new System.Windows.Forms.Padding(2, 1, 2, 1);
			this.toolStrip3.Size = new System.Drawing.Size(416, 27);
			this.toolStrip3.TabIndex = 3;
			// 
			// toolStripLabel2
			// 
			this.toolStripLabel2.Name = "toolStripLabel2";
			this.toolStripLabel2.Size = new System.Drawing.Size(51, 22);
			this.toolStripLabel2.Text = "Preview:";
			// 
			// hbxHexView
			// 
			this.hbxHexView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.hbxHexView.Font = new System.Drawing.Font("Consolas", 9.75F);
			this.hbxHexView.Location = new System.Drawing.Point(0, 27);
			this.hbxHexView.Name = "hbxHexView";
			this.hbxHexView.ShadowSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(60)))), ((int)(((byte)(188)))), ((int)(((byte)(255)))));
			this.hbxHexView.Size = new System.Drawing.Size(417, 290);
			this.hbxHexView.StringViewVisible = true;
			this.hbxHexView.TabIndex = 4;
			this.hbxHexView.VScrollBarVisible = true;
			// 
			// toolStrip4
			// 
			this.toolStrip4.AutoSize = false;
			this.toolStrip4.BackColor = System.Drawing.Color.Transparent;
			this.toolStrip4.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip4.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel3});
			this.toolStrip4.Location = new System.Drawing.Point(0, 0);
			this.toolStrip4.Name = "toolStrip4";
			this.toolStrip4.Padding = new System.Windows.Forms.Padding(2, 1, 2, 1);
			this.toolStrip4.Size = new System.Drawing.Size(417, 27);
			this.toolStrip4.TabIndex = 4;
			// 
			// toolStripLabel3
			// 
			this.toolStripLabel3.Name = "toolStripLabel3";
			this.toolStripLabel3.Size = new System.Drawing.Size(58, 22);
			this.toolStripLabel3.Text = "Hex View:";
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
			this.splContent.Panel2.ResumeLayout(false);
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
			this.splPreview.Panel1.ResumeLayout(false);
			this.splPreview.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splPreview)).EndInit();
			this.splPreview.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ptbPreview)).EndInit();
			this.toolStrip3.ResumeLayout(false);
			this.toolStrip3.PerformLayout();
			this.toolStrip4.ResumeLayout(false);
			this.toolStrip4.PerformLayout();
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
		private System.Windows.Forms.ToolStripButton tsbRename;
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
		private System.Windows.Forms.ToolStripDropDownButton tsbGameSelect;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem gBATempToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem gitHubToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem1;
		private System.Windows.Forms.SplitContainer splPreview;
		private System.Windows.Forms.ToolStrip toolStrip3;
		private System.Windows.Forms.ToolStripLabel toolStripLabel2;
		private System.Windows.Forms.ToolStrip toolStrip4;
		private System.Windows.Forms.ToolStripLabel toolStripLabel3;
		private Be.Windows.Forms.HexBox hbxHexView;
		private System.Windows.Forms.PictureBox ptbPreview;
	}
}