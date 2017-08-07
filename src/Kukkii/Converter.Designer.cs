﻿namespace Kukkii
{
    partial class Converter
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
            this.mnuMain = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openRawToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportPNGToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importPNGToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.compressionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.encryptionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hashToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.batchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.batchExportPNGToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.batchImportPNGToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.batchScanSubdirectoriesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gBATempToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gitHubToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.aboutToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.tlsMain = new System.Windows.Forms.ToolStrip();
            this.tsbOpen = new System.Windows.Forms.ToolStripButton();
            this.tsbSave = new System.Windows.Forms.ToolStripButton();
            this.tsbSaveAs = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbExportPNG = new System.Windows.Forms.ToolStripButton();
            this.tsbImportPNG = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbBatchExport = new System.Windows.Forms.ToolStripButton();
            this.tsbBatchImport = new System.Windows.Forms.ToolStripButton();
            this.tsbBatchScanSubdirectories = new System.Windows.Forms.ToolStripButton();
            this.tsbKarameru = new System.Windows.Forms.ToolStripButton();
            this.tsbKuriimu = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbGridColor1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.tsbGridColor2 = new System.Windows.Forms.ToolStripDropDownButton();
            this.tsbImageBorderStyle = new System.Windows.Forms.ToolStripDropDownButton();
            this.tsbImageBorderColor = new System.Windows.Forms.ToolStripDropDownButton();
            this.pnlMain = new System.Windows.Forms.Panel();
            this.splMain = new System.Windows.Forms.SplitContainer();
            this.imbPreview = new Cyotek.Windows.Forms.ImageBox();
            this.cmsPreview = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cmsPreviewCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.tslTools = new System.Windows.Forms.ToolStrip();
            this.tslZoom = new System.Windows.Forms.ToolStripLabel();
            this.tslTool = new System.Windows.Forms.ToolStripLabel();
            this.splProperties = new System.Windows.Forms.SplitContainer();
            this.treBitmaps = new System.Windows.Forms.TreeView();
            this.imlBitmaps = new System.Windows.Forms.ImageList(this.components);
            this.pptImageProperties = new System.Windows.Forms.PropertyGrid();
            this.clrDialog = new System.Windows.Forms.ColorDialog();
            this.mnuMain.SuspendLayout();
            this.tlsMain.SuspendLayout();
            this.pnlMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splMain)).BeginInit();
            this.splMain.Panel1.SuspendLayout();
            this.splMain.Panel2.SuspendLayout();
            this.splMain.SuspendLayout();
            this.cmsPreview.SuspendLayout();
            this.tslTools.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splProperties)).BeginInit();
            this.splProperties.Panel1.SuspendLayout();
            this.splProperties.Panel2.SuspendLayout();
            this.splProperties.SuspendLayout();
            this.SuspendLayout();
            // 
            // mnuMain
            // 
            this.mnuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.batchToolStripMenuItem,
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
            this.openRawToolStripMenuItem,
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
            this.openToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.openToolStripMenuItem.Text = "&Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // openRawToolStripMenuItem
            // 
            this.openRawToolStripMenuItem.Image = global::Kukkii.Properties.Resources.menu_open;
            this.openRawToolStripMenuItem.Name = "openRawToolStripMenuItem";
            this.openRawToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
            this.openRawToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.openRawToolStripMenuItem.Text = "Open Raw Image";
            this.openRawToolStripMenuItem.Click += new System.EventHandler(this.openRawToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Enabled = false;
            this.saveToolStripMenuItem.Image = global::Kukkii.Properties.Resources.menu_save;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.saveToolStripMenuItem.Text = "&Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Enabled = false;
            this.saveAsToolStripMenuItem.Image = global::Kukkii.Properties.Resources.menu_save_as;
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F12;
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.saveAsToolStripMenuItem.Text = "S&ave As...";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(204, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Image = global::Kukkii.Properties.Resources.menu_exit;
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportPNGToolStripMenuItem,
            this.importPNGToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.editToolStripMenuItem.Text = "&Image";
            // 
            // exportPNGToolStripMenuItem
            // 
            this.exportPNGToolStripMenuItem.Enabled = false;
            this.exportPNGToolStripMenuItem.Image = global::Kukkii.Properties.Resources.menu_export;
            this.exportPNGToolStripMenuItem.Name = "exportPNGToolStripMenuItem";
            this.exportPNGToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.exportPNGToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.exportPNGToolStripMenuItem.Text = "&Export PNG";
            this.exportPNGToolStripMenuItem.Click += new System.EventHandler(this.exportPNGToolStripMenuItem_Click);
            // 
            // importPNGToolStripMenuItem
            // 
            this.importPNGToolStripMenuItem.Enabled = false;
            this.importPNGToolStripMenuItem.Image = global::Kukkii.Properties.Resources.import_import;
            this.importPNGToolStripMenuItem.Name = "importPNGToolStripMenuItem";
            this.importPNGToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I)));
            this.importPNGToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.importPNGToolStripMenuItem.Text = "&Import PNG";
            this.importPNGToolStripMenuItem.Click += new System.EventHandler(this.importPNGToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.compressionToolStripMenuItem,
            this.encryptionToolStripMenuItem,
            this.hashToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(47, 20);
            this.toolsToolStripMenuItem.Text = "&Tools";
            // 
            // compressionToolStripMenuItem
            // 
            this.compressionToolStripMenuItem.Image = global::Kukkii.Properties.Resources.menu_compression;
            this.compressionToolStripMenuItem.Name = "compressionToolStripMenuItem";
            this.compressionToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.compressionToolStripMenuItem.Text = "&Compression";
            // 
            // encryptionToolStripMenuItem
            // 
            this.encryptionToolStripMenuItem.Name = "encryptionToolStripMenuItem";
            this.encryptionToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.encryptionToolStripMenuItem.Text = "&Encryption";
            // 
            // hashToolStripMenuItem
            // 
            this.hashToolStripMenuItem.Name = "hashToolStripMenuItem";
            this.hashToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.hashToolStripMenuItem.Text = "Hash";
            // 
            // batchToolStripMenuItem
            // 
            this.batchToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.batchExportPNGToolStripMenuItem,
            this.batchImportPNGToolStripMenuItem,
            this.batchScanSubdirectoriesToolStripMenuItem});
            this.batchToolStripMenuItem.Name = "batchToolStripMenuItem";
            this.batchToolStripMenuItem.Size = new System.Drawing.Size(49, 20);
            this.batchToolStripMenuItem.Text = "&Batch";
            // 
            // batchExportPNGToolStripMenuItem
            // 
            this.batchExportPNGToolStripMenuItem.Image = global::Kukkii.Properties.Resources.menu_batch_export;
            this.batchExportPNGToolStripMenuItem.Name = "batchExportPNGToolStripMenuItem";
            this.batchExportPNGToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.E)));
            this.batchExportPNGToolStripMenuItem.Size = new System.Drawing.Size(292, 22);
            this.batchExportPNGToolStripMenuItem.Text = "Batch &Export PNG";
            this.batchExportPNGToolStripMenuItem.Click += new System.EventHandler(this.batchExportPNGToolStripMenuItem_Click);
            // 
            // batchImportPNGToolStripMenuItem
            // 
            this.batchImportPNGToolStripMenuItem.Image = global::Kukkii.Properties.Resources.menu_batch_import;
            this.batchImportPNGToolStripMenuItem.Name = "batchImportPNGToolStripMenuItem";
            this.batchImportPNGToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.I)));
            this.batchImportPNGToolStripMenuItem.Size = new System.Drawing.Size(292, 22);
            this.batchImportPNGToolStripMenuItem.Text = "Batch &Import PNG";
            this.batchImportPNGToolStripMenuItem.Click += new System.EventHandler(this.batchImportPNGToolStripMenuItem_Click);
            // 
            // batchScanSubdirectoriesToolStripMenuItem
            // 
            this.batchScanSubdirectoriesToolStripMenuItem.Image = global::Kukkii.Properties.Resources.menu_scan_subdirectories_off;
            this.batchScanSubdirectoriesToolStripMenuItem.Name = "batchScanSubdirectoriesToolStripMenuItem";
            this.batchScanSubdirectoriesToolStripMenuItem.Size = new System.Drawing.Size(292, 22);
            this.batchScanSubdirectoriesToolStripMenuItem.Text = "Don\'t Scan Sub-directories";
            this.batchScanSubdirectoriesToolStripMenuItem.Click += new System.EventHandler(this.batchScanSubdirectoriesToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gBATempToolStripMenuItem,
            this.gitHubToolStripMenuItem,
            this.toolStripSeparator4,
            this.aboutToolStripMenuItem1});
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.aboutToolStripMenuItem.Text = "&Help";
            // 
            // gBATempToolStripMenuItem
            // 
            this.gBATempToolStripMenuItem.Image = global::Kukkii.Properties.Resources.menu_gbatemp;
            this.gBATempToolStripMenuItem.Name = "gBATempToolStripMenuItem";
            this.gBATempToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.gBATempToolStripMenuItem.Text = "GBATemp";
            // 
            // gitHubToolStripMenuItem
            // 
            this.gitHubToolStripMenuItem.Image = global::Kukkii.Properties.Resources.menu_git;
            this.gitHubToolStripMenuItem.Name = "gitHubToolStripMenuItem";
            this.gitHubToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.gitHubToolStripMenuItem.Text = "GitHub";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(123, 6);
            // 
            // aboutToolStripMenuItem1
            // 
            this.aboutToolStripMenuItem1.Image = global::Kukkii.Properties.Resources.menu_about;
            this.aboutToolStripMenuItem1.Name = "aboutToolStripMenuItem1";
            this.aboutToolStripMenuItem1.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.aboutToolStripMenuItem1.Size = new System.Drawing.Size(126, 22);
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
            this.toolStripSeparator2,
            this.tsbExportPNG,
            this.tsbImportPNG,
            this.toolStripSeparator3,
            this.tsbBatchExport,
            this.tsbBatchImport,
            this.tsbBatchScanSubdirectories,
            this.tsbKarameru,
            this.tsbKuriimu,
            this.toolStripSeparator5,
            this.tsbGridColor1,
            this.tsbGridColor2,
            this.tsbImageBorderStyle,
            this.tsbImageBorderColor});
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
            // tsbExportPNG
            // 
            this.tsbExportPNG.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbExportPNG.Enabled = false;
            this.tsbExportPNG.Image = global::Kukkii.Properties.Resources.menu_export;
            this.tsbExportPNG.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbExportPNG.Name = "tsbExportPNG";
            this.tsbExportPNG.Size = new System.Drawing.Size(23, 22);
            this.tsbExportPNG.Text = "Export PNG";
            this.tsbExportPNG.Click += new System.EventHandler(this.tsbExportPNG_Click);
            // 
            // tsbImportPNG
            // 
            this.tsbImportPNG.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbImportPNG.Enabled = false;
            this.tsbImportPNG.Image = global::Kukkii.Properties.Resources.import_import;
            this.tsbImportPNG.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbImportPNG.Name = "tsbImportPNG";
            this.tsbImportPNG.Size = new System.Drawing.Size(23, 22);
            this.tsbImportPNG.Text = "Import PNG";
            this.tsbImportPNG.Click += new System.EventHandler(this.tsbImportPNG_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbBatchExport
            // 
            this.tsbBatchExport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbBatchExport.Image = global::Kukkii.Properties.Resources.menu_batch_export;
            this.tsbBatchExport.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbBatchExport.Name = "tsbBatchExport";
            this.tsbBatchExport.Size = new System.Drawing.Size(23, 22);
            this.tsbBatchExport.Text = "Batch Export PNG";
            this.tsbBatchExport.Click += new System.EventHandler(this.tsbBatchExport_Click);
            // 
            // tsbBatchImport
            // 
            this.tsbBatchImport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbBatchImport.Image = global::Kukkii.Properties.Resources.menu_batch_import;
            this.tsbBatchImport.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbBatchImport.Name = "tsbBatchImport";
            this.tsbBatchImport.Size = new System.Drawing.Size(23, 22);
            this.tsbBatchImport.Text = "Batch Import PNG";
            this.tsbBatchImport.Click += new System.EventHandler(this.tsbBatchImport_Click);
            // 
            // tsbBatchScanSubdirectories
            // 
            this.tsbBatchScanSubdirectories.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbBatchScanSubdirectories.Image = global::Kukkii.Properties.Resources.menu_scan_subdirectories_off;
            this.tsbBatchScanSubdirectories.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbBatchScanSubdirectories.Name = "tsbBatchScanSubdirectories";
            this.tsbBatchScanSubdirectories.Size = new System.Drawing.Size(23, 22);
            this.tsbBatchScanSubdirectories.Text = "Don\'t Scan Sub-directories";
            this.tsbBatchScanSubdirectories.Click += new System.EventHandler(this.tsbBatchScanSubdirectories_Click);
            // 
            // tsbKarameru
            // 
            this.tsbKarameru.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tsbKarameru.Image = global::Kukkii.Properties.Resources.karameru;
            this.tsbKarameru.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbKarameru.Name = "tsbKarameru";
            this.tsbKarameru.Size = new System.Drawing.Size(78, 22);
            this.tsbKarameru.Text = "Karameru";
            this.tsbKarameru.Click += new System.EventHandler(this.tsbKarameru_Click);
            // 
            // tsbKuriimu
            // 
            this.tsbKuriimu.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tsbKuriimu.Image = global::Kukkii.Properties.Resources.kuriimu;
            this.tsbKuriimu.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbKuriimu.Name = "tsbKuriimu";
            this.tsbKuriimu.Size = new System.Drawing.Size(69, 22);
            this.tsbKuriimu.Text = "Kuriimu";
            this.tsbKuriimu.ToolTipText = "Kuriimu";
            this.tsbKuriimu.Click += new System.EventHandler(this.tsbKuriimu_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbGridColor1
            // 
            this.tsbGridColor1.BackColor = System.Drawing.Color.Transparent;
            this.tsbGridColor1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbGridColor1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.tsbGridColor1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbGridColor1.Name = "tsbGridColor1";
            this.tsbGridColor1.Size = new System.Drawing.Size(13, 22);
            this.tsbGridColor1.Text = "Grid Color 1";
            this.tsbGridColor1.Click += new System.EventHandler(this.tsbGridColor1_Click);
            // 
            // tsbGridColor2
            // 
            this.tsbGridColor2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbGridColor2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbGridColor2.Name = "tsbGridColor2";
            this.tsbGridColor2.Size = new System.Drawing.Size(13, 22);
            this.tsbGridColor2.Text = "Grid Color 2";
            this.tsbGridColor2.Click += new System.EventHandler(this.tsbGridColor2_Click);
            // 
            // tsbImageBorderStyle
            // 
            this.tsbImageBorderStyle.Image = global::Kukkii.Properties.Resources.menu_border_none;
            this.tsbImageBorderStyle.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbImageBorderStyle.Name = "tsbImageBorderStyle";
            this.tsbImageBorderStyle.Size = new System.Drawing.Size(65, 22);
            this.tsbImageBorderStyle.Text = "None";
            this.tsbImageBorderStyle.ToolTipText = "Image Border Style";
            // 
            // tsbImageBorderColor
            // 
            this.tsbImageBorderColor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbImageBorderColor.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbImageBorderColor.Name = "tsbImageBorderColor";
            this.tsbImageBorderColor.Size = new System.Drawing.Size(13, 22);
            this.tsbImageBorderColor.Text = "Image Border Color";
            this.tsbImageBorderColor.Click += new System.EventHandler(this.tsbImageBorderColor_Click);
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
            this.splMain.Panel1.Controls.Add(this.imbPreview);
            this.splMain.Panel1.Controls.Add(this.tslTools);
            // 
            // splMain.Panel2
            // 
            this.splMain.Panel2.Controls.Add(this.splProperties);
            this.splMain.Size = new System.Drawing.Size(972, 599);
            this.splMain.SplitterDistance = 740;
            this.splMain.TabIndex = 0;
            // 
            // imbPreview
            // 
            this.imbPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.imbPreview.ContextMenuStrip = this.cmsPreview;
            this.imbPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imbPreview.GridCellSize = 16;
            this.imbPreview.GridColor = System.Drawing.Color.Silver;
            this.imbPreview.ImageBorderColor = System.Drawing.Color.Black;
            this.imbPreview.ImageBorderStyle = Cyotek.Windows.Forms.ImageBoxBorderStyle.FixedSingleDropShadow;
            this.imbPreview.Location = new System.Drawing.Point(0, 0);
            this.imbPreview.Name = "imbPreview";
            this.imbPreview.SelectionMode = Cyotek.Windows.Forms.ImageBoxSelectionMode.Zoom;
            this.imbPreview.Size = new System.Drawing.Size(740, 572);
            this.imbPreview.TabIndex = 6;
            this.imbPreview.Zoomed += new System.EventHandler<Cyotek.Windows.Forms.ImageBoxZoomEventArgs>(this.imbPreview_Zoomed);
            this.imbPreview.KeyDown += new System.Windows.Forms.KeyEventHandler(this.imbPreview_KeyDown);
            this.imbPreview.KeyUp += new System.Windows.Forms.KeyEventHandler(this.imbPreview_KeyUp);
            this.imbPreview.MouseEnter += new System.EventHandler(this.imbPreview_MouseEnter);
            // 
            // cmsPreview
            // 
            this.cmsPreview.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmsPreviewCopy});
            this.cmsPreview.Name = "cmsPreview";
            this.cmsPreview.Size = new System.Drawing.Size(103, 26);
            // 
            // cmsPreviewCopy
            // 
            this.cmsPreviewCopy.Image = global::Kukkii.Properties.Resources.menu_copy;
            this.cmsPreviewCopy.Name = "cmsPreviewCopy";
            this.cmsPreviewCopy.Size = new System.Drawing.Size(102, 22);
            this.cmsPreviewCopy.Text = "&Copy";
            this.cmsPreviewCopy.Click += new System.EventHandler(this.cmsPreviewCopy_Click);
            // 
            // tslTools
            // 
            this.tslTools.AutoSize = false;
            this.tslTools.BackColor = System.Drawing.Color.Transparent;
            this.tslTools.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tslTools.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tslTools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tslZoom,
            this.tslTool});
            this.tslTools.Location = new System.Drawing.Point(0, 572);
            this.tslTools.Name = "tslTools";
            this.tslTools.Padding = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.tslTools.Size = new System.Drawing.Size(740, 27);
            this.tslTools.TabIndex = 7;
            // 
            // tslZoom
            // 
            this.tslZoom.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tslZoom.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tslZoom.Name = "tslZoom";
            this.tslZoom.Size = new System.Drawing.Size(73, 22);
            this.tslZoom.Text = "Zoom: 100%";
            // 
            // tslTool
            // 
            this.tslTool.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tslTool.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tslTool.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tslTool.Name = "tslTool";
            this.tslTool.Size = new System.Drawing.Size(68, 22);
            this.tslTool.Text = "Tool: Zoom";
            // 
            // splProperties
            // 
            this.splProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splProperties.Location = new System.Drawing.Point(0, 0);
            this.splProperties.Name = "splProperties";
            this.splProperties.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splProperties.Panel1
            // 
            this.splProperties.Panel1.Controls.Add(this.treBitmaps);
            // 
            // splProperties.Panel2
            // 
            this.splProperties.Panel2.Controls.Add(this.pptImageProperties);
            this.splProperties.Size = new System.Drawing.Size(228, 599);
            this.splProperties.SplitterDistance = 300;
            this.splProperties.TabIndex = 2;
            // 
            // treBitmaps
            // 
            this.treBitmaps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treBitmaps.FullRowSelect = true;
            this.treBitmaps.HideSelection = false;
            this.treBitmaps.ImageIndex = 0;
            this.treBitmaps.ImageList = this.imlBitmaps;
            this.treBitmaps.Location = new System.Drawing.Point(0, 0);
            this.treBitmaps.Name = "treBitmaps";
            this.treBitmaps.SelectedImageIndex = 0;
            this.treBitmaps.ShowLines = false;
            this.treBitmaps.ShowPlusMinus = false;
            this.treBitmaps.ShowRootLines = false;
            this.treBitmaps.Size = new System.Drawing.Size(228, 300);
            this.treBitmaps.TabIndex = 1;
            this.treBitmaps.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treBitmaps_AfterSelect);
            this.treBitmaps.MouseEnter += new System.EventHandler(this.treBitmaps_MouseEnter);
            // 
            // imlBitmaps
            // 
            this.imlBitmaps.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imlBitmaps.ImageSize = new System.Drawing.Size(96, 64);
            this.imlBitmaps.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // pptImageProperties
            // 
            this.pptImageProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pptImageProperties.LineColor = System.Drawing.SystemColors.ControlDark;
            this.pptImageProperties.Location = new System.Drawing.Point(0, 0);
            this.pptImageProperties.Name = "pptImageProperties";
            this.pptImageProperties.Size = new System.Drawing.Size(228, 295);
            this.pptImageProperties.TabIndex = 0;
            this.pptImageProperties.ToolbarVisible = false;
            // 
            // Converter
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 662);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.tlsMain);
            this.Controls.Add(this.mnuMain);
            this.Name = "Converter";
            this.Text = "Kukki";
            this.Load += new System.EventHandler(this.frmConverter_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.frmConverter_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.frmConverter_DragEnter);
            this.mnuMain.ResumeLayout(false);
            this.mnuMain.PerformLayout();
            this.tlsMain.ResumeLayout(false);
            this.tlsMain.PerformLayout();
            this.pnlMain.ResumeLayout(false);
            this.splMain.Panel1.ResumeLayout(false);
            this.splMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splMain)).EndInit();
            this.splMain.ResumeLayout(false);
            this.cmsPreview.ResumeLayout(false);
            this.tslTools.ResumeLayout(false);
            this.tslTools.PerformLayout();
            this.splProperties.Panel1.ResumeLayout(false);
            this.splProperties.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splProperties)).EndInit();
            this.splProperties.ResumeLayout(false);
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
        private Cyotek.Windows.Forms.ImageBox imbPreview;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem exportPNGToolStripMenuItem;
        private System.Windows.Forms.ToolStrip tslTools;
        private System.Windows.Forms.ToolStripLabel tslZoom;
        private System.Windows.Forms.ToolStripLabel tslTool;
        private System.Windows.Forms.ToolStripButton tsbBatchExport;
        private System.Windows.Forms.PropertyGrid pptImageProperties;
        private System.Windows.Forms.ToolStripMenuItem batchToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton tsbExportPNG;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton tsbBatchImport;
        private System.Windows.Forms.ToolStripButton tsbImportPNG;
        private System.Windows.Forms.ToolStripMenuItem importPNGToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton tsbKuriimu;
        private System.Windows.Forms.ToolStripButton tsbKarameru;
        private System.Windows.Forms.ToolStripMenuItem batchExportPNGToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem batchImportPNGToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem compressionToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripDropDownButton tsbGridColor1;
        private System.Windows.Forms.ToolStripDropDownButton tsbGridColor2;
        private System.Windows.Forms.ColorDialog clrDialog;
        private System.Windows.Forms.ToolStripButton tsbBatchScanSubdirectories;
        private System.Windows.Forms.ToolStripMenuItem batchScanSubdirectoriesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem encryptionToolStripMenuItem;
        private System.Windows.Forms.TreeView treBitmaps;
        private System.Windows.Forms.ImageList imlBitmaps;
        private System.Windows.Forms.SplitContainer splProperties;
        private System.Windows.Forms.ToolStripDropDownButton tsbImageBorderColor;
        private System.Windows.Forms.ToolStripDropDownButton tsbImageBorderStyle;
        private System.Windows.Forms.ContextMenuStrip cmsPreview;
        private System.Windows.Forms.ToolStripMenuItem cmsPreviewCopy;
        private System.Windows.Forms.ToolStripMenuItem hashToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openRawToolStripMenuItem;
    }
}

