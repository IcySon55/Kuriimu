namespace Kuriimu
{
	partial class frmEditor
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
			this.findToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.gBATempToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.gitHubToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.aboutToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.pnlMain = new System.Windows.Forms.Panel();
			this.splMain = new System.Windows.Forms.SplitContainer();
			this.lstEntries = new System.Windows.Forms.ListBox();
			this.tlsEntries = new System.Windows.Forms.ToolStrip();
			this.tslEntries = new System.Windows.Forms.ToolStripLabel();
			this.tsbEntryAdd = new System.Windows.Forms.ToolStripButton();
			this.tsbEntryRename = new System.Windows.Forms.ToolStripButton();
			this.tsbEntryDelete = new System.Windows.Forms.ToolStripButton();
			this.tsbEntryProperties = new System.Windows.Forms.ToolStripButton();
			this.splContent = new System.Windows.Forms.SplitContainer();
			this.splText = new System.Windows.Forms.SplitContainer();
			this.txtEdit = new System.Windows.Forms.TextBox();
			this.tlsEdit = new System.Windows.Forms.ToolStrip();
			this.tslText = new System.Windows.Forms.ToolStripLabel();
			this.tsbGameSelect = new System.Windows.Forms.ToolStripDropDownButton();
			this.txtOriginal = new System.Windows.Forms.TextBox();
			this.tlsOriginal = new System.Windows.Forms.ToolStrip();
			this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
			this.splPreview = new System.Windows.Forms.SplitContainer();
			this.ptbPreview = new System.Windows.Forms.PictureBox();
			this.tlsPreview = new System.Windows.Forms.ToolStrip();
			this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
			this.hbxHexView = new Be.Windows.Forms.HexBox();
			this.tlsHexView = new System.Windows.Forms.ToolStrip();
			this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
			this.tlsMain = new System.Windows.Forms.ToolStrip();
			this.tsbOpen = new System.Windows.Forms.ToolStripButton();
			this.tsbSave = new System.Windows.Forms.ToolStripButton();
			this.tsbSaveAs = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.tsbFind = new System.Windows.Forms.ToolStripButton();
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
			this.tlsEdit.SuspendLayout();
			this.tlsOriginal.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splPreview)).BeginInit();
			this.splPreview.Panel1.SuspendLayout();
			this.splPreview.Panel2.SuspendLayout();
			this.splPreview.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ptbPreview)).BeginInit();
			this.tlsPreview.SuspendLayout();
			this.tlsHexView.SuspendLayout();
			this.tlsMain.SuspendLayout();
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
			this.openToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.openToolStripMenuItem.Text = "&Open";
			this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
			// 
			// saveToolStripMenuItem
			// 
			this.saveToolStripMenuItem.Enabled = false;
			this.saveToolStripMenuItem.Image = global::Kuriimu.Properties.Resources.menu_save;
			this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
			this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.saveToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.saveToolStripMenuItem.Text = "&Save";
			this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
			// 
			// saveAsToolStripMenuItem
			// 
			this.saveAsToolStripMenuItem.Enabled = false;
			this.saveAsToolStripMenuItem.Image = global::Kuriimu.Properties.Resources.menu_save_as;
			this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
			this.saveAsToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F12;
			this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.saveAsToolStripMenuItem.Text = "S&ave As...";
			this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(149, 6);
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Image = global::Kuriimu.Properties.Resources.menu_exit;
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.exitToolStripMenuItem.Text = "E&xit";
			// 
			// editToolStripMenuItem
			// 
			this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.findToolStripMenuItem});
			this.editToolStripMenuItem.Name = "editToolStripMenuItem";
			this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
			this.editToolStripMenuItem.Text = "&Edit";
			// 
			// findToolStripMenuItem
			// 
			this.findToolStripMenuItem.Enabled = false;
			this.findToolStripMenuItem.Image = global::Kuriimu.Properties.Resources.menu_find;
			this.findToolStripMenuItem.Name = "findToolStripMenuItem";
			this.findToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
			this.findToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.findToolStripMenuItem.Text = "&Find";
			this.findToolStripMenuItem.Click += new System.EventHandler(this.findToolStripMenuItem_Click);
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
			this.gBATempToolStripMenuItem.Click += new System.EventHandler(this.gBATempToolStripMenuItem_Click);
			// 
			// gitHubToolStripMenuItem
			// 
			this.gitHubToolStripMenuItem.Image = global::Kuriimu.Properties.Resources.menu_git;
			this.gitHubToolStripMenuItem.Name = "gitHubToolStripMenuItem";
			this.gitHubToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.gitHubToolStripMenuItem.Text = "GitHub";
			this.gitHubToolStripMenuItem.Click += new System.EventHandler(this.gitHubToolStripMenuItem_Click);
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
			this.pnlMain.Location = new System.Drawing.Point(0, 51);
			this.pnlMain.Name = "pnlMain";
			this.pnlMain.Padding = new System.Windows.Forms.Padding(6);
			this.pnlMain.Size = new System.Drawing.Size(1113, 617);
			this.pnlMain.TabIndex = 1;
			// 
			// splMain
			// 
			this.splMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splMain.Enabled = false;
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
			this.splMain.Size = new System.Drawing.Size(1101, 605);
			this.splMain.SplitterDistance = 256;
			this.splMain.SplitterWidth = 6;
			this.splMain.TabIndex = 0;
			// 
			// lstEntries
			// 
			this.lstEntries.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lstEntries.Enabled = false;
			this.lstEntries.FormattingEnabled = true;
			this.lstEntries.IntegralHeight = false;
			this.lstEntries.Location = new System.Drawing.Point(0, 27);
			this.lstEntries.Name = "lstEntries";
			this.lstEntries.ScrollAlwaysVisible = true;
			this.lstEntries.Size = new System.Drawing.Size(256, 578);
			this.lstEntries.Sorted = true;
			this.lstEntries.TabIndex = 1;
			this.lstEntries.SelectedIndexChanged += new System.EventHandler(this.lstEntries_SelectedIndexChanged);
			this.lstEntries.DoubleClick += new System.EventHandler(this.lstEntries_DoubleClick);
			this.lstEntries.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lstEntries_KeyDown);
			// 
			// tlsEntries
			// 
			this.tlsEntries.AutoSize = false;
			this.tlsEntries.BackColor = System.Drawing.Color.Transparent;
			this.tlsEntries.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.tlsEntries.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tslEntries,
            this.tsbEntryAdd,
            this.tsbEntryRename,
            this.tsbEntryDelete,
            this.tsbEntryProperties});
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
			// tsbEntryAdd
			// 
			this.tsbEntryAdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbEntryAdd.Enabled = false;
			this.tsbEntryAdd.Image = global::Kuriimu.Properties.Resources.menu_add;
			this.tsbEntryAdd.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbEntryAdd.Name = "tsbEntryAdd";
			this.tsbEntryAdd.Size = new System.Drawing.Size(23, 22);
			this.tsbEntryAdd.Text = "Add Entry";
			this.tsbEntryAdd.Click += new System.EventHandler(this.tsbEntryAdd_Click);
			// 
			// tsbEntryRename
			// 
			this.tsbEntryRename.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbEntryRename.Enabled = false;
			this.tsbEntryRename.Image = global::Kuriimu.Properties.Resources.menu_field_properties;
			this.tsbEntryRename.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbEntryRename.Name = "tsbEntryRename";
			this.tsbEntryRename.Size = new System.Drawing.Size(23, 22);
			this.tsbEntryRename.Text = "Rename Entry";
			this.tsbEntryRename.Click += new System.EventHandler(this.tsbEntryRename_Click);
			// 
			// tsbEntryDelete
			// 
			this.tsbEntryDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbEntryDelete.Enabled = false;
			this.tsbEntryDelete.Image = global::Kuriimu.Properties.Resources.menu_delete;
			this.tsbEntryDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbEntryDelete.Name = "tsbEntryDelete";
			this.tsbEntryDelete.Size = new System.Drawing.Size(23, 22);
			this.tsbEntryDelete.Text = "Delete Entry";
			// 
			// tsbEntryProperties
			// 
			this.tsbEntryProperties.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbEntryProperties.Enabled = false;
			this.tsbEntryProperties.Image = global::Kuriimu.Properties.Resources.menu_properties;
			this.tsbEntryProperties.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbEntryProperties.Name = "tsbEntryProperties";
			this.tsbEntryProperties.Size = new System.Drawing.Size(23, 22);
			this.tsbEntryProperties.Text = "Entry Properties";
			this.tsbEntryProperties.Click += new System.EventHandler(this.tsbEntryProperties_Click);
			// 
			// splContent
			// 
			this.splContent.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splContent.Enabled = false;
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
			this.splContent.Size = new System.Drawing.Size(839, 605);
			this.splContent.SplitterDistance = 282;
			this.splContent.SplitterWidth = 6;
			this.splContent.TabIndex = 0;
			// 
			// splText
			// 
			this.splText.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splText.Enabled = false;
			this.splText.Location = new System.Drawing.Point(0, 0);
			this.splText.Name = "splText";
			// 
			// splText.Panel1
			// 
			this.splText.Panel1.Controls.Add(this.txtEdit);
			this.splText.Panel1.Controls.Add(this.tlsEdit);
			// 
			// splText.Panel2
			// 
			this.splText.Panel2.Controls.Add(this.txtOriginal);
			this.splText.Panel2.Controls.Add(this.tlsOriginal);
			this.splText.Size = new System.Drawing.Size(839, 282);
			this.splText.SplitterDistance = 416;
			this.splText.SplitterWidth = 6;
			this.splText.TabIndex = 2;
			// 
			// txtEdit
			// 
			this.txtEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtEdit.Enabled = false;
			this.txtEdit.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtEdit.Location = new System.Drawing.Point(0, 27);
			this.txtEdit.Multiline = true;
			this.txtEdit.Name = "txtEdit";
			this.txtEdit.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtEdit.Size = new System.Drawing.Size(416, 255);
			this.txtEdit.TabIndex = 0;
			this.txtEdit.TextChanged += new System.EventHandler(this.txtEdit_TextChanged);
			this.txtEdit.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtEdit_KeyDown);
			this.txtEdit.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtEdit_KeyUp);
			this.txtEdit.MouseUp += new System.Windows.Forms.MouseEventHandler(this.txtEdit_MouseUp);
			// 
			// tlsEdit
			// 
			this.tlsEdit.AutoSize = false;
			this.tlsEdit.BackColor = System.Drawing.Color.Transparent;
			this.tlsEdit.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.tlsEdit.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tslText,
            this.tsbGameSelect});
			this.tlsEdit.Location = new System.Drawing.Point(0, 0);
			this.tlsEdit.Name = "tlsEdit";
			this.tlsEdit.Padding = new System.Windows.Forms.Padding(2, 1, 2, 1);
			this.tlsEdit.Size = new System.Drawing.Size(416, 27);
			this.tlsEdit.TabIndex = 2;
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
			this.tsbGameSelect.Size = new System.Drawing.Size(86, 22);
			this.tsbGameSelect.Text = "No Game";
			// 
			// txtOriginal
			// 
			this.txtOriginal.BackColor = System.Drawing.SystemColors.Window;
			this.txtOriginal.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtOriginal.Enabled = false;
			this.txtOriginal.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtOriginal.Location = new System.Drawing.Point(0, 27);
			this.txtOriginal.Multiline = true;
			this.txtOriginal.Name = "txtOriginal";
			this.txtOriginal.ReadOnly = true;
			this.txtOriginal.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtOriginal.Size = new System.Drawing.Size(417, 255);
			this.txtOriginal.TabIndex = 4;
			// 
			// tlsOriginal
			// 
			this.tlsOriginal.AutoSize = false;
			this.tlsOriginal.BackColor = System.Drawing.Color.Transparent;
			this.tlsOriginal.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.tlsOriginal.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1});
			this.tlsOriginal.Location = new System.Drawing.Point(0, 0);
			this.tlsOriginal.Name = "tlsOriginal";
			this.tlsOriginal.Padding = new System.Windows.Forms.Padding(2, 1, 2, 1);
			this.tlsOriginal.Size = new System.Drawing.Size(417, 27);
			this.tlsOriginal.TabIndex = 3;
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
			this.splPreview.Enabled = false;
			this.splPreview.Location = new System.Drawing.Point(0, 0);
			this.splPreview.Name = "splPreview";
			// 
			// splPreview.Panel1
			// 
			this.splPreview.Panel1.Controls.Add(this.ptbPreview);
			this.splPreview.Panel1.Controls.Add(this.tlsPreview);
			// 
			// splPreview.Panel2
			// 
			this.splPreview.Panel2.Controls.Add(this.hbxHexView);
			this.splPreview.Panel2.Controls.Add(this.tlsHexView);
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
			// tlsPreview
			// 
			this.tlsPreview.AutoSize = false;
			this.tlsPreview.BackColor = System.Drawing.Color.Transparent;
			this.tlsPreview.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.tlsPreview.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel2});
			this.tlsPreview.Location = new System.Drawing.Point(0, 0);
			this.tlsPreview.Name = "tlsPreview";
			this.tlsPreview.Padding = new System.Windows.Forms.Padding(2, 1, 2, 1);
			this.tlsPreview.Size = new System.Drawing.Size(416, 27);
			this.tlsPreview.TabIndex = 3;
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
			this.hbxHexView.Enabled = false;
			this.hbxHexView.Font = new System.Drawing.Font("Consolas", 9.75F);
			this.hbxHexView.Location = new System.Drawing.Point(0, 27);
			this.hbxHexView.Name = "hbxHexView";
			this.hbxHexView.ShadowSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(60)))), ((int)(((byte)(188)))), ((int)(((byte)(255)))));
			this.hbxHexView.Size = new System.Drawing.Size(417, 290);
			this.hbxHexView.StringViewVisible = true;
			this.hbxHexView.TabIndex = 4;
			this.hbxHexView.VScrollBarVisible = true;
			// 
			// tlsHexView
			// 
			this.tlsHexView.AutoSize = false;
			this.tlsHexView.BackColor = System.Drawing.Color.Transparent;
			this.tlsHexView.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.tlsHexView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel3});
			this.tlsHexView.Location = new System.Drawing.Point(0, 0);
			this.tlsHexView.Name = "tlsHexView";
			this.tlsHexView.Padding = new System.Windows.Forms.Padding(2, 1, 2, 1);
			this.tlsHexView.Size = new System.Drawing.Size(417, 27);
			this.tlsHexView.TabIndex = 4;
			// 
			// toolStripLabel3
			// 
			this.toolStripLabel3.Name = "toolStripLabel3";
			this.toolStripLabel3.Size = new System.Drawing.Size(58, 22);
			this.toolStripLabel3.Text = "Hex View:";
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
            this.toolStripSeparator2,
            this.tsbFind});
			this.tlsMain.Location = new System.Drawing.Point(0, 24);
			this.tlsMain.Name = "tlsMain";
			this.tlsMain.Padding = new System.Windows.Forms.Padding(2, 1, 2, 1);
			this.tlsMain.Size = new System.Drawing.Size(1113, 27);
			this.tlsMain.TabIndex = 2;
			// 
			// tsbOpen
			// 
			this.tsbOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbOpen.Image = global::Kuriimu.Properties.Resources.menu_open;
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
			this.tsbSave.Image = global::Kuriimu.Properties.Resources.menu_save;
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
			this.tsbSaveAs.Image = global::Kuriimu.Properties.Resources.menu_save_as;
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
			// tsbFind
			// 
			this.tsbFind.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbFind.Enabled = false;
			this.tsbFind.Image = global::Kuriimu.Properties.Resources.menu_find;
			this.tsbFind.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbFind.Name = "tsbFind";
			this.tsbFind.Size = new System.Drawing.Size(23, 22);
			this.tsbFind.Text = "Find";
			this.tsbFind.Click += new System.EventHandler(this.tsbFind_Click);
			// 
			// frmEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1113, 668);
			this.Controls.Add(this.pnlMain);
			this.Controls.Add(this.tlsMain);
			this.Controls.Add(this.mnuMain);
			this.MainMenuStrip = this.mnuMain;
			this.Name = "frmEditor";
			this.Text = "Kuriimu";
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
			this.tlsEdit.ResumeLayout(false);
			this.tlsEdit.PerformLayout();
			this.tlsOriginal.ResumeLayout(false);
			this.tlsOriginal.PerformLayout();
			this.splPreview.Panel1.ResumeLayout(false);
			this.splPreview.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splPreview)).EndInit();
			this.splPreview.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ptbPreview)).EndInit();
			this.tlsPreview.ResumeLayout(false);
			this.tlsPreview.PerformLayout();
			this.tlsHexView.ResumeLayout(false);
			this.tlsHexView.PerformLayout();
			this.tlsMain.ResumeLayout(false);
			this.tlsMain.PerformLayout();
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
		private System.Windows.Forms.ToolStripButton tsbEntryAdd;
		private System.Windows.Forms.ToolStripButton tsbEntryRename;
		private System.Windows.Forms.ToolStripButton tsbEntryDelete;
		private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
		private System.Windows.Forms.SplitContainer splContent;
		private System.Windows.Forms.TextBox txtEdit;
		private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
		private System.Windows.Forms.SplitContainer splText;
		private System.Windows.Forms.ToolStrip tlsEdit;
		private System.Windows.Forms.ToolStripLabel tslText;
		private System.Windows.Forms.ToolStrip tlsOriginal;
		private System.Windows.Forms.ToolStripLabel toolStripLabel1;
		private System.Windows.Forms.TextBox txtOriginal;
		private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
		private System.Windows.Forms.ToolStripButton tsbEntryProperties;
		private System.Windows.Forms.ToolStripMenuItem findToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripDropDownButton tsbGameSelect;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem gBATempToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem gitHubToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem1;
		private System.Windows.Forms.SplitContainer splPreview;
		private System.Windows.Forms.ToolStrip tlsPreview;
		private System.Windows.Forms.ToolStripLabel toolStripLabel2;
		private System.Windows.Forms.ToolStrip tlsHexView;
		private System.Windows.Forms.ToolStripLabel toolStripLabel3;
		private Be.Windows.Forms.HexBox hbxHexView;
		private System.Windows.Forms.PictureBox ptbPreview;
		private System.Windows.Forms.ToolStrip tlsMain;
		private System.Windows.Forms.ToolStripButton tsbOpen;
		private System.Windows.Forms.ToolStripButton tsbSave;
		private System.Windows.Forms.ToolStripButton tsbSaveAs;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripButton tsbFind;
	}
}