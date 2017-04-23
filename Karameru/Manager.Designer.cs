namespace Karameru
{
    partial class Manager
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Manager));
            this.mnuMain = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.propertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gBATempToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gitHubToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.aboutToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.tlsMain = new System.Windows.Forms.ToolStrip();
            this.tsbNew = new System.Windows.Forms.ToolStripButton();
            this.tsbOpen = new System.Windows.Forms.ToolStripButton();
            this.tsbSave = new System.Windows.Forms.ToolStripButton();
            this.tsbSaveAs = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbFind = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbKukkii = new System.Windows.Forms.ToolStripButton();
            this.tsbProperties = new System.Windows.Forms.ToolStripButton();
            this.tsbKuriimu = new System.Windows.Forms.ToolStripButton();
            this.pnlMain = new System.Windows.Forms.Panel();
            this.splMain = new System.Windows.Forms.SplitContainer();
            this.treDirectories = new System.Windows.Forms.TreeView();
            this.mnuDirectories = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.extractDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.replaceDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tlsFiles = new System.Windows.Forms.ToolStrip();
            this.tslDirectories = new System.Windows.Forms.ToolStripLabel();
            this.lstFiles = new System.Windows.Forms.ListView();
            this.clmName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clmSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.mnuFiles = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.extractFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.replaceFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editListToolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.editInKuriimuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editInKukkiiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editInKarameruToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tlsFileDetails = new System.Windows.Forms.ToolStrip();
            this.tslFileCount = new System.Windows.Forms.ToolStripLabel();
            this.tlsPreview = new System.Windows.Forms.ToolStrip();
            this.tsbFileAdd = new System.Windows.Forms.ToolStripButton();
            this.tsbFileExtract = new System.Windows.Forms.ToolStripButton();
            this.tsbFileReplace = new System.Windows.Forms.ToolStripButton();
            this.tsbFileRename = new System.Windows.Forms.ToolStripButton();
            this.tsbFileDelete = new System.Windows.Forms.ToolStripButton();
            this.tsbFileProperties = new System.Windows.Forms.ToolStripButton();
            this.viewToolStripDropDownButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.largeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.smallToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.listToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.detailsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imlFiles = new System.Windows.Forms.ImageList(this.components);
            this.imlFilesLarge = new System.Windows.Forms.ImageList(this.components);
            this.mnuMain.SuspendLayout();
            this.tlsMain.SuspendLayout();
            this.pnlMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splMain)).BeginInit();
            this.splMain.Panel1.SuspendLayout();
            this.splMain.Panel2.SuspendLayout();
            this.splMain.SuspendLayout();
            this.mnuDirectories.SuspendLayout();
            this.tlsFiles.SuspendLayout();
            this.mnuFiles.SuspendLayout();
            this.tlsFileDetails.SuspendLayout();
            this.tlsPreview.SuspendLayout();
            this.SuspendLayout();
            // 
            // mnuMain
            // 
            this.mnuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.tolsToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.mnuMain.Location = new System.Drawing.Point(0, 0);
            this.mnuMain.Name = "mnuMain";
            this.mnuMain.Padding = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.mnuMain.Size = new System.Drawing.Size(949, 24);
            this.mnuMain.TabIndex = 1;
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Enabled = false;
            this.newToolStripMenuItem.Image = global::Karameru.Properties.Resources.menu_new;
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.newToolStripMenuItem.Text = "&New";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Image = global::Karameru.Properties.Resources.menu_open;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.openToolStripMenuItem.Text = "&Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Enabled = false;
            this.saveToolStripMenuItem.Image = global::Karameru.Properties.Resources.menu_save;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.saveToolStripMenuItem.Text = "&Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Enabled = false;
            this.saveAsToolStripMenuItem.Image = global::Karameru.Properties.Resources.menu_save_as;
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
            this.exitToolStripMenuItem.Image = global::Karameru.Properties.Resources.menu_exit;
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.findToolStripMenuItem,
            this.toolStripMenuItem1,
            this.propertiesToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "&Edit";
            // 
            // findToolStripMenuItem
            // 
            this.findToolStripMenuItem.Enabled = false;
            this.findToolStripMenuItem.Image = global::Karameru.Properties.Resources.menu_find;
            this.findToolStripMenuItem.Name = "findToolStripMenuItem";
            this.findToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.findToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.findToolStripMenuItem.Text = "&Find";
            this.findToolStripMenuItem.Click += new System.EventHandler(this.findToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(143, 6);
            // 
            // propertiesToolStripMenuItem
            // 
            this.propertiesToolStripMenuItem.Enabled = false;
            this.propertiesToolStripMenuItem.Image = global::Karameru.Properties.Resources.menu_properties;
            this.propertiesToolStripMenuItem.Name = "propertiesToolStripMenuItem";
            this.propertiesToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F8;
            this.propertiesToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.propertiesToolStripMenuItem.Text = "&Properties";
            this.propertiesToolStripMenuItem.Click += new System.EventHandler(this.propertiesToolStripMenuItem_Click);
            // 
            // tolsToolStripMenuItem
            // 
            this.tolsToolStripMenuItem.Name = "tolsToolStripMenuItem";
            this.tolsToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.tolsToolStripMenuItem.Text = "&Tools";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gBATempToolStripMenuItem,
            this.gitHubToolStripMenuItem,
            this.toolStripSeparator7,
            this.aboutToolStripMenuItem1});
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.aboutToolStripMenuItem.Text = "&Help";
            // 
            // gBATempToolStripMenuItem
            // 
            this.gBATempToolStripMenuItem.Image = global::Karameru.Properties.Resources.menu_gbatemp;
            this.gBATempToolStripMenuItem.Name = "gBATempToolStripMenuItem";
            this.gBATempToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.gBATempToolStripMenuItem.Text = "GBATemp";
            this.gBATempToolStripMenuItem.Click += new System.EventHandler(this.gBATempToolStripMenuItem_Click);
            // 
            // gitHubToolStripMenuItem
            // 
            this.gitHubToolStripMenuItem.Image = global::Karameru.Properties.Resources.menu_git;
            this.gitHubToolStripMenuItem.Name = "gitHubToolStripMenuItem";
            this.gitHubToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.gitHubToolStripMenuItem.Text = "GitHub";
            this.gitHubToolStripMenuItem.Click += new System.EventHandler(this.gitHubToolStripMenuItem_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(125, 6);
            // 
            // aboutToolStripMenuItem1
            // 
            this.aboutToolStripMenuItem1.Image = global::Karameru.Properties.Resources.menu_about;
            this.aboutToolStripMenuItem1.Name = "aboutToolStripMenuItem1";
            this.aboutToolStripMenuItem1.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.aboutToolStripMenuItem1.Size = new System.Drawing.Size(128, 22);
            this.aboutToolStripMenuItem1.Text = "&About";
            this.aboutToolStripMenuItem1.Click += new System.EventHandler(this.aboutToolStripMenuItem1_Click);
            // 
            // tlsMain
            // 
            this.tlsMain.AutoSize = false;
            this.tlsMain.BackColor = System.Drawing.Color.Transparent;
            this.tlsMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tlsMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbNew,
            this.tsbOpen,
            this.tsbSave,
            this.tsbSaveAs,
            this.toolStripSeparator2,
            this.tsbFind,
            this.toolStripSeparator3,
            this.tsbKukkii,
            this.tsbProperties,
            this.tsbKuriimu});
            this.tlsMain.Location = new System.Drawing.Point(0, 24);
            this.tlsMain.Name = "tlsMain";
            this.tlsMain.Padding = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.tlsMain.Size = new System.Drawing.Size(949, 27);
            this.tlsMain.TabIndex = 3;
            // 
            // tsbNew
            // 
            this.tsbNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbNew.Enabled = false;
            this.tsbNew.Image = global::Karameru.Properties.Resources.menu_new;
            this.tsbNew.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbNew.Name = "tsbNew";
            this.tsbNew.Size = new System.Drawing.Size(23, 22);
            this.tsbNew.Text = "New";
            // 
            // tsbOpen
            // 
            this.tsbOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbOpen.Image = global::Karameru.Properties.Resources.menu_open;
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
            this.tsbSave.Image = global::Karameru.Properties.Resources.menu_save;
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
            this.tsbSaveAs.Image = global::Karameru.Properties.Resources.menu_save_as;
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
            this.tsbFind.Image = global::Karameru.Properties.Resources.menu_find;
            this.tsbFind.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbFind.Name = "tsbFind";
            this.tsbFind.Size = new System.Drawing.Size(23, 22);
            this.tsbFind.Text = "Find";
            this.tsbFind.Click += new System.EventHandler(this.tsbFind_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbKukkii
            // 
            this.tsbKukkii.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tsbKukkii.Image = global::Karameru.Properties.Resources.kukkii;
            this.tsbKukkii.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbKukkii.Name = "tsbKukkii";
            this.tsbKukkii.Size = new System.Drawing.Size(59, 22);
            this.tsbKukkii.Text = "Kukkii";
            this.tsbKukkii.Click += new System.EventHandler(this.tsbKukkii_Click);
            // 
            // tsbProperties
            // 
            this.tsbProperties.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbProperties.Enabled = false;
            this.tsbProperties.Image = global::Karameru.Properties.Resources.menu_properties;
            this.tsbProperties.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbProperties.Name = "tsbProperties";
            this.tsbProperties.Size = new System.Drawing.Size(23, 22);
            this.tsbProperties.Text = "Properties";
            this.tsbProperties.Click += new System.EventHandler(this.tsbProperties_Click);
            // 
            // tsbKuriimu
            // 
            this.tsbKuriimu.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tsbKuriimu.Image = global::Karameru.Properties.Resources.kuriimu;
            this.tsbKuriimu.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbKuriimu.Name = "tsbKuriimu";
            this.tsbKuriimu.Size = new System.Drawing.Size(69, 22);
            this.tsbKuriimu.Text = "Kuriimu";
            this.tsbKuriimu.Click += new System.EventHandler(this.tsbKuriimu_Click);
            // 
            // pnlMain
            // 
            this.pnlMain.Controls.Add(this.splMain);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 51);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Padding = new System.Windows.Forms.Padding(6);
            this.pnlMain.Size = new System.Drawing.Size(949, 576);
            this.pnlMain.TabIndex = 4;
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
            this.splMain.Panel1.Controls.Add(this.treDirectories);
            this.splMain.Panel1.Controls.Add(this.tlsFiles);
            // 
            // splMain.Panel2
            // 
            this.splMain.Panel2.Controls.Add(this.lstFiles);
            this.splMain.Panel2.Controls.Add(this.tlsFileDetails);
            this.splMain.Panel2.Controls.Add(this.tlsPreview);
            this.splMain.Size = new System.Drawing.Size(937, 564);
            this.splMain.SplitterDistance = 320;
            this.splMain.SplitterWidth = 6;
            this.splMain.TabIndex = 1;
            // 
            // treDirectories
            // 
            this.treDirectories.AllowDrop = true;
            this.treDirectories.ContextMenuStrip = this.mnuDirectories;
            this.treDirectories.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treDirectories.FullRowSelect = true;
            this.treDirectories.HideSelection = false;
            this.treDirectories.HotTracking = true;
            this.treDirectories.ItemHeight = 21;
            this.treDirectories.Location = new System.Drawing.Point(0, 27);
            this.treDirectories.Name = "treDirectories";
            this.treDirectories.ShowLines = false;
            this.treDirectories.Size = new System.Drawing.Size(320, 537);
            this.treDirectories.TabIndex = 4;
            this.treDirectories.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.treEntries_AfterCollapse);
            this.treDirectories.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.treEntries_AfterExpand);
            this.treDirectories.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treEntries_AfterSelect);
            // 
            // mnuDirectories
            // 
            this.mnuDirectories.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.extractDirectoryToolStripMenuItem,
            this.replaceDirectoryToolStripMenuItem});
            this.mnuDirectories.Name = "mnuEntries";
            this.mnuDirectories.Size = new System.Drawing.Size(125, 48);
            // 
            // extractDirectoryToolStripMenuItem
            // 
            this.extractDirectoryToolStripMenuItem.Image = global::Karameru.Properties.Resources.menu_export;
            this.extractDirectoryToolStripMenuItem.Name = "extractDirectoryToolStripMenuItem";
            this.extractDirectoryToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.extractDirectoryToolStripMenuItem.Text = "E&xtract...";
            // 
            // replaceDirectoryToolStripMenuItem
            // 
            this.replaceDirectoryToolStripMenuItem.Name = "replaceDirectoryToolStripMenuItem";
            this.replaceDirectoryToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.replaceDirectoryToolStripMenuItem.Text = "&Replace...";
            // 
            // tlsFiles
            // 
            this.tlsFiles.AutoSize = false;
            this.tlsFiles.BackColor = System.Drawing.Color.Transparent;
            this.tlsFiles.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tlsFiles.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tslDirectories});
            this.tlsFiles.Location = new System.Drawing.Point(0, 0);
            this.tlsFiles.Name = "tlsFiles";
            this.tlsFiles.Padding = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.tlsFiles.Size = new System.Drawing.Size(320, 27);
            this.tlsFiles.TabIndex = 0;
            // 
            // tslDirectories
            // 
            this.tslDirectories.Name = "tslDirectories";
            this.tslDirectories.Size = new System.Drawing.Size(84, 22);
            this.tslDirectories.Text = "Directory Tree:";
            // 
            // lstFiles
            // 
            this.lstFiles.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.lstFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clmName,
            this.clmSize});
            this.lstFiles.ContextMenuStrip = this.mnuFiles;
            this.lstFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstFiles.FullRowSelect = true;
            this.lstFiles.Location = new System.Drawing.Point(0, 27);
            this.lstFiles.MultiSelect = false;
            this.lstFiles.Name = "lstFiles";
            this.lstFiles.ShowGroups = false;
            this.lstFiles.ShowItemToolTips = true;
            this.lstFiles.Size = new System.Drawing.Size(611, 510);
            this.lstFiles.TabIndex = 5;
            this.lstFiles.TileSize = new System.Drawing.Size(168, 36);
            this.lstFiles.UseCompatibleStateImageBehavior = false;
            this.lstFiles.View = System.Windows.Forms.View.Details;
            this.lstFiles.SelectedIndexChanged += new System.EventHandler(this.lstFiles_SelectedIndexChanged);
            // 
            // clmName
            // 
            this.clmName.Text = "Name";
            this.clmName.Width = 281;
            // 
            // clmSize
            // 
            this.clmSize.Text = "Size";
            this.clmSize.Width = 81;
            // 
            // mnuFiles
            // 
            this.mnuFiles.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.extractFileToolStripMenuItem,
            this.replaceFileToolStripMenuItem,
            this.renameFileToolStripMenuItem,
            this.deleteFileToolStripMenuItem,
            this.editListToolStripSeparator,
            this.editInKuriimuToolStripMenuItem,
            this.editInKukkiiToolStripMenuItem,
            this.editInKarameruToolStripMenuItem});
            this.mnuFiles.Name = "mnuEntries";
            this.mnuFiles.Size = new System.Drawing.Size(162, 164);
            this.mnuFiles.Opening += new System.ComponentModel.CancelEventHandler(this.mnuFiles_Opening);
            // 
            // extractFileToolStripMenuItem
            // 
            this.extractFileToolStripMenuItem.Image = global::Karameru.Properties.Resources.menu_export;
            this.extractFileToolStripMenuItem.Name = "extractFileToolStripMenuItem";
            this.extractFileToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.extractFileToolStripMenuItem.Text = "E&xtract...";
            this.extractFileToolStripMenuItem.Click += new System.EventHandler(this.extractFileToolStripMenuItem_Click);
            // 
            // replaceFileToolStripMenuItem
            // 
            this.replaceFileToolStripMenuItem.Image = global::Karameru.Properties.Resources.menu_import;
            this.replaceFileToolStripMenuItem.Name = "replaceFileToolStripMenuItem";
            this.replaceFileToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.replaceFileToolStripMenuItem.Text = "&Replace...";
            this.replaceFileToolStripMenuItem.Click += new System.EventHandler(this.replaceFileToolStripMenuItem_Click);
            // 
            // renameFileToolStripMenuItem
            // 
            this.renameFileToolStripMenuItem.Image = global::Karameru.Properties.Resources.menu_field_properties;
            this.renameFileToolStripMenuItem.Name = "renameFileToolStripMenuItem";
            this.renameFileToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.renameFileToolStripMenuItem.Text = "Re&name...";
            // 
            // deleteFileToolStripMenuItem
            // 
            this.deleteFileToolStripMenuItem.Image = global::Karameru.Properties.Resources.menu_delete;
            this.deleteFileToolStripMenuItem.Name = "deleteFileToolStripMenuItem";
            this.deleteFileToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.deleteFileToolStripMenuItem.Text = "&Delete";
            // 
            // editListToolStripSeparator
            // 
            this.editListToolStripSeparator.Name = "editListToolStripSeparator";
            this.editListToolStripSeparator.Size = new System.Drawing.Size(158, 6);
            // 
            // editInKuriimuToolStripMenuItem
            // 
            this.editInKuriimuToolStripMenuItem.Image = global::Karameru.Properties.Resources.kuriimu;
            this.editInKuriimuToolStripMenuItem.Name = "editInKuriimuToolStripMenuItem";
            this.editInKuriimuToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.editInKuriimuToolStripMenuItem.Tag = "";
            this.editInKuriimuToolStripMenuItem.Text = "Edit in Kuriimu";
            this.editInKuriimuToolStripMenuItem.Click += new System.EventHandler(this.editFileToolStripMenuItem_Click);
            // 
            // editInKukkiiToolStripMenuItem
            // 
            this.editInKukkiiToolStripMenuItem.Image = global::Karameru.Properties.Resources.kukkii;
            this.editInKukkiiToolStripMenuItem.Name = "editInKukkiiToolStripMenuItem";
            this.editInKukkiiToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.editInKukkiiToolStripMenuItem.Text = "Edit in Kukkii";
            this.editInKukkiiToolStripMenuItem.Click += new System.EventHandler(this.editFileToolStripMenuItem_Click);
            // 
            // editInKarameruToolStripMenuItem
            // 
            this.editInKarameruToolStripMenuItem.Image = global::Karameru.Properties.Resources.karameru1;
            this.editInKarameruToolStripMenuItem.Name = "editInKarameruToolStripMenuItem";
            this.editInKarameruToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.editInKarameruToolStripMenuItem.Text = "Edit in Karameru";
            this.editInKarameruToolStripMenuItem.Click += new System.EventHandler(this.editFileToolStripMenuItem_Click);
            // 
            // tlsFileDetails
            // 
            this.tlsFileDetails.AutoSize = false;
            this.tlsFileDetails.BackColor = System.Drawing.Color.Transparent;
            this.tlsFileDetails.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tlsFileDetails.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tlsFileDetails.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tslFileCount});
            this.tlsFileDetails.Location = new System.Drawing.Point(0, 537);
            this.tlsFileDetails.Name = "tlsFileDetails";
            this.tlsFileDetails.Padding = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.tlsFileDetails.Size = new System.Drawing.Size(611, 27);
            this.tlsFileDetails.TabIndex = 6;
            // 
            // tslFileCount
            // 
            this.tslFileCount.Name = "tslFileCount";
            this.tslFileCount.Size = new System.Drawing.Size(58, 22);
            this.tslFileCount.Text = "FileCount";
            // 
            // tlsPreview
            // 
            this.tlsPreview.AutoSize = false;
            this.tlsPreview.BackColor = System.Drawing.Color.Transparent;
            this.tlsPreview.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tlsPreview.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbFileAdd,
            this.tsbFileExtract,
            this.tsbFileReplace,
            this.tsbFileRename,
            this.tsbFileDelete,
            this.tsbFileProperties,
            this.viewToolStripDropDownButton});
            this.tlsPreview.Location = new System.Drawing.Point(0, 0);
            this.tlsPreview.Name = "tlsPreview";
            this.tlsPreview.Padding = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.tlsPreview.Size = new System.Drawing.Size(611, 27);
            this.tlsPreview.TabIndex = 4;
            // 
            // tsbFileAdd
            // 
            this.tsbFileAdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbFileAdd.Enabled = false;
            this.tsbFileAdd.Image = global::Karameru.Properties.Resources.menu_add;
            this.tsbFileAdd.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbFileAdd.Name = "tsbFileAdd";
            this.tsbFileAdd.Size = new System.Drawing.Size(23, 22);
            this.tsbFileAdd.Text = "Add File";
            this.tsbFileAdd.Click += new System.EventHandler(this.tsbAddFile_Click);
            // 
            // tsbFileExtract
            // 
            this.tsbFileExtract.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbFileExtract.Enabled = false;
            this.tsbFileExtract.Image = global::Karameru.Properties.Resources.menu_export;
            this.tsbFileExtract.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbFileExtract.Name = "tsbFileExtract";
            this.tsbFileExtract.Size = new System.Drawing.Size(23, 22);
            this.tsbFileExtract.Text = "Extract File";
            this.tsbFileExtract.Click += new System.EventHandler(this.tsbFileExtract_Click);
            // 
            // tsbFileReplace
            // 
            this.tsbFileReplace.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbFileReplace.Enabled = false;
            this.tsbFileReplace.Image = global::Karameru.Properties.Resources.menu_import;
            this.tsbFileReplace.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbFileReplace.Name = "tsbFileReplace";
            this.tsbFileReplace.Size = new System.Drawing.Size(23, 22);
            this.tsbFileReplace.Text = "Replace File";
            this.tsbFileReplace.Click += new System.EventHandler(this.tsbFileReplace_Click);
            // 
            // tsbFileRename
            // 
            this.tsbFileRename.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbFileRename.Enabled = false;
            this.tsbFileRename.Image = global::Karameru.Properties.Resources.menu_field_properties;
            this.tsbFileRename.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbFileRename.Name = "tsbFileRename";
            this.tsbFileRename.Size = new System.Drawing.Size(23, 22);
            this.tsbFileRename.Text = "Rename File";
            // 
            // tsbFileDelete
            // 
            this.tsbFileDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbFileDelete.Enabled = false;
            this.tsbFileDelete.Image = global::Karameru.Properties.Resources.menu_delete;
            this.tsbFileDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbFileDelete.Name = "tsbFileDelete";
            this.tsbFileDelete.Size = new System.Drawing.Size(23, 22);
            this.tsbFileDelete.Text = "Delete File";
            // 
            // tsbFileProperties
            // 
            this.tsbFileProperties.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbFileProperties.Enabled = false;
            this.tsbFileProperties.Image = global::Karameru.Properties.Resources.menu_properties;
            this.tsbFileProperties.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbFileProperties.Name = "tsbFileProperties";
            this.tsbFileProperties.Size = new System.Drawing.Size(23, 22);
            this.tsbFileProperties.Text = "File Properties";
            // 
            // viewToolStripDropDownButton
            // 
            this.viewToolStripDropDownButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.viewToolStripDropDownButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.viewToolStripDropDownButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.largeToolStripMenuItem,
            this.smallToolStripMenuItem,
            this.listToolStripMenuItem,
            this.detailsToolStripMenuItem,
            this.tileToolStripMenuItem});
            this.viewToolStripDropDownButton.Image = ((System.Drawing.Image)(resources.GetObject("viewToolStripDropDownButton.Image")));
            this.viewToolStripDropDownButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.viewToolStripDropDownButton.Name = "viewToolStripDropDownButton";
            this.viewToolStripDropDownButton.Size = new System.Drawing.Size(45, 22);
            this.viewToolStripDropDownButton.Text = "View";
            // 
            // largeToolStripMenuItem
            // 
            this.largeToolStripMenuItem.Image = global::Karameru.Properties.Resources.view_large;
            this.largeToolStripMenuItem.Name = "largeToolStripMenuItem";
            this.largeToolStripMenuItem.Size = new System.Drawing.Size(109, 22);
            this.largeToolStripMenuItem.Text = "Large";
            this.largeToolStripMenuItem.Click += new System.EventHandler(this.viewToolStripMenuItem_Click);
            // 
            // smallToolStripMenuItem
            // 
            this.smallToolStripMenuItem.Image = global::Karameru.Properties.Resources.view_small;
            this.smallToolStripMenuItem.Name = "smallToolStripMenuItem";
            this.smallToolStripMenuItem.Size = new System.Drawing.Size(109, 22);
            this.smallToolStripMenuItem.Text = "Small";
            this.smallToolStripMenuItem.Click += new System.EventHandler(this.viewToolStripMenuItem_Click);
            // 
            // listToolStripMenuItem
            // 
            this.listToolStripMenuItem.Image = global::Karameru.Properties.Resources.view_list;
            this.listToolStripMenuItem.Name = "listToolStripMenuItem";
            this.listToolStripMenuItem.Size = new System.Drawing.Size(109, 22);
            this.listToolStripMenuItem.Text = "List";
            this.listToolStripMenuItem.Click += new System.EventHandler(this.viewToolStripMenuItem_Click);
            // 
            // detailsToolStripMenuItem
            // 
            this.detailsToolStripMenuItem.Image = global::Karameru.Properties.Resources.view_details;
            this.detailsToolStripMenuItem.Name = "detailsToolStripMenuItem";
            this.detailsToolStripMenuItem.Size = new System.Drawing.Size(109, 22);
            this.detailsToolStripMenuItem.Text = "Details";
            this.detailsToolStripMenuItem.Click += new System.EventHandler(this.viewToolStripMenuItem_Click);
            // 
            // tileToolStripMenuItem
            // 
            this.tileToolStripMenuItem.Image = global::Karameru.Properties.Resources.view_tiles;
            this.tileToolStripMenuItem.Name = "tileToolStripMenuItem";
            this.tileToolStripMenuItem.Size = new System.Drawing.Size(109, 22);
            this.tileToolStripMenuItem.Text = "Tiles";
            this.tileToolStripMenuItem.Click += new System.EventHandler(this.viewToolStripMenuItem_Click);
            // 
            // imlFiles
            // 
            this.imlFiles.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imlFiles.ImageSize = new System.Drawing.Size(16, 16);
            this.imlFiles.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // imlFilesLarge
            // 
            this.imlFilesLarge.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imlFilesLarge.ImageSize = new System.Drawing.Size(32, 32);
            this.imlFilesLarge.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // Manager
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(949, 627);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.tlsMain);
            this.Controls.Add(this.mnuMain);
            this.Name = "Manager";
            this.Text = "Karameru";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmManager_FormClosing);
            this.Load += new System.EventHandler(this.frmManager_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.frmManager_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.frmManager_DragEnter);
            this.mnuMain.ResumeLayout(false);
            this.mnuMain.PerformLayout();
            this.tlsMain.ResumeLayout(false);
            this.tlsMain.PerformLayout();
            this.pnlMain.ResumeLayout(false);
            this.splMain.Panel1.ResumeLayout(false);
            this.splMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splMain)).EndInit();
            this.splMain.ResumeLayout(false);
            this.mnuDirectories.ResumeLayout(false);
            this.tlsFiles.ResumeLayout(false);
            this.tlsFiles.PerformLayout();
            this.mnuFiles.ResumeLayout(false);
            this.tlsFileDetails.ResumeLayout(false);
            this.tlsFileDetails.PerformLayout();
            this.tlsPreview.ResumeLayout(false);
            this.tlsPreview.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip mnuMain;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem propertiesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gBATempToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gitHubToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem1;
        private System.Windows.Forms.ToolStrip tlsMain;
        private System.Windows.Forms.ToolStripButton tsbNew;
        private System.Windows.Forms.ToolStripButton tsbOpen;
        private System.Windows.Forms.ToolStripButton tsbSave;
        private System.Windows.Forms.ToolStripButton tsbSaveAs;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton tsbFind;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton tsbKukkii;
        private System.Windows.Forms.ToolStripButton tsbProperties;
        private System.Windows.Forms.ToolStripButton tsbKuriimu;
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.SplitContainer splMain;
        private System.Windows.Forms.TreeView treDirectories;
        private System.Windows.Forms.ToolStrip tlsFiles;
        private System.Windows.Forms.ToolStripLabel tslDirectories;
        private System.Windows.Forms.ToolStrip tlsPreview;
        private System.Windows.Forms.ImageList imlFiles;
        private System.Windows.Forms.ContextMenuStrip mnuFiles;
        private System.Windows.Forms.ToolStripMenuItem extractFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem replaceFileToolStripMenuItem;
        private System.Windows.Forms.ListView lstFiles;
        private System.Windows.Forms.ColumnHeader clmName;
        private System.Windows.Forms.ColumnHeader clmSize;
        private System.Windows.Forms.ContextMenuStrip mnuDirectories;
        private System.Windows.Forms.ToolStripMenuItem extractDirectoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem replaceDirectoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton tsbFileExtract;
        private System.Windows.Forms.ToolStripButton tsbFileAdd;
        private System.Windows.Forms.ToolStripButton tsbFileRename;
        private System.Windows.Forms.ToolStripButton tsbFileReplace;
        private System.Windows.Forms.ToolStripButton tsbFileDelete;
        private System.Windows.Forms.ToolStripButton tsbFileProperties;
        private System.Windows.Forms.ToolStrip tlsFileDetails;
        private System.Windows.Forms.ToolStripLabel tslFileCount;
        private System.Windows.Forms.ToolStripSeparator editListToolStripSeparator;
        private System.Windows.Forms.ToolStripMenuItem deleteFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editInKuriimuToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editInKukkiiToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editInKarameruToolStripMenuItem;
        private System.Windows.Forms.ImageList imlFilesLarge;
        private System.Windows.Forms.ToolStripDropDownButton viewToolStripDropDownButton;
        private System.Windows.Forms.ToolStripMenuItem largeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem smallToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem listToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem detailsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renameFileToolStripMenuItem;
    }
}

