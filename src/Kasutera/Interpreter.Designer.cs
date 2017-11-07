namespace Kasutera
{
    partial class Interpreter
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
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.compressionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.encryptionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hashToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gBATempToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gitHubToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.aboutToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlMain = new System.Windows.Forms.Panel();
            this.splMain = new System.Windows.Forms.SplitContainer();
            this.tlsTools = new System.Windows.Forms.ToolStrip();
            this.tslZoom = new System.Windows.Forms.ToolStripLabel();
            this.tslTool = new System.Windows.Forms.ToolStripLabel();
            this.splProperties = new System.Windows.Forms.SplitContainer();
            this.treNodes = new System.Windows.Forms.TreeView();
            this.pptNodeProperties = new System.Windows.Forms.PropertyGrid();
            this.tlsProperties = new System.Windows.Forms.ToolStrip();
            this.tsbExtendedProperties = new System.Windows.Forms.ToolStripButton();
            this.tslProperties = new System.Windows.Forms.ToolStripLabel();
            this.imbPreview = new Cyotek.Windows.Forms.ImageBox();
            this.imlNodes = new System.Windows.Forms.ImageList(this.components);
            this.mnuMain.SuspendLayout();
            this.pnlMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splMain)).BeginInit();
            this.splMain.Panel1.SuspendLayout();
            this.splMain.Panel2.SuspendLayout();
            this.splMain.SuspendLayout();
            this.tlsTools.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splProperties)).BeginInit();
            this.splProperties.Panel1.SuspendLayout();
            this.splProperties.Panel2.SuspendLayout();
            this.splProperties.SuspendLayout();
            this.tlsProperties.SuspendLayout();
            this.SuspendLayout();
            // 
            // mnuMain
            // 
            this.mnuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.mnuMain.Location = new System.Drawing.Point(0, 0);
            this.mnuMain.Name = "mnuMain";
            this.mnuMain.Padding = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.mnuMain.Size = new System.Drawing.Size(1101, 24);
            this.mnuMain.TabIndex = 2;
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
            this.openToolStripMenuItem.Image = global::Kasutera.Properties.Resources.menu_open;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.openToolStripMenuItem.Text = "&Open";
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Enabled = false;
            this.saveToolStripMenuItem.Image = global::Kasutera.Properties.Resources.menu_save;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.saveToolStripMenuItem.Text = "&Save";
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Enabled = false;
            this.saveAsToolStripMenuItem.Image = global::Kasutera.Properties.Resources.menu_save_as;
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F12;
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.saveAsToolStripMenuItem.Text = "S&ave As...";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(149, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Image = global::Kasutera.Properties.Resources.menu_exit;
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.compressionToolStripMenuItem,
            this.encryptionToolStripMenuItem,
            this.hashToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.toolsToolStripMenuItem.Text = "&Tools";
            // 
            // compressionToolStripMenuItem
            // 
            this.compressionToolStripMenuItem.Image = global::Kasutera.Properties.Resources.menu_compression;
            this.compressionToolStripMenuItem.Name = "compressionToolStripMenuItem";
            this.compressionToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.compressionToolStripMenuItem.Text = "&Compression";
            // 
            // encryptionToolStripMenuItem
            // 
            this.encryptionToolStripMenuItem.Name = "encryptionToolStripMenuItem";
            this.encryptionToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.encryptionToolStripMenuItem.Text = "&Encryption";
            // 
            // hashToolStripMenuItem
            // 
            this.hashToolStripMenuItem.Name = "hashToolStripMenuItem";
            this.hashToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.hashToolStripMenuItem.Text = "Hash";
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
            this.gBATempToolStripMenuItem.Image = global::Kasutera.Properties.Resources.menu_gbatemp;
            this.gBATempToolStripMenuItem.Name = "gBATempToolStripMenuItem";
            this.gBATempToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.gBATempToolStripMenuItem.Text = "GBATemp";
            // 
            // gitHubToolStripMenuItem
            // 
            this.gitHubToolStripMenuItem.Image = global::Kasutera.Properties.Resources.menu_git;
            this.gitHubToolStripMenuItem.Name = "gitHubToolStripMenuItem";
            this.gitHubToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.gitHubToolStripMenuItem.Text = "GitHub";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(149, 6);
            // 
            // aboutToolStripMenuItem1
            // 
            this.aboutToolStripMenuItem1.Image = global::Kasutera.Properties.Resources.menu_about;
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
            this.pnlMain.Size = new System.Drawing.Size(1101, 674);
            this.pnlMain.TabIndex = 5;
            // 
            // splMain
            // 
            this.splMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splMain.Location = new System.Drawing.Point(6, 6);
            this.splMain.Name = "splMain";
            // 
            // splMain.Panel1
            // 
            this.splMain.Panel1.Controls.Add(this.splProperties);
            // 
            // splMain.Panel2
            // 
            this.splMain.Panel2.Controls.Add(this.imbPreview);
            this.splMain.Panel2.Controls.Add(this.tlsTools);
            this.splMain.Size = new System.Drawing.Size(1089, 662);
            this.splMain.SplitterDistance = 256;
            this.splMain.TabIndex = 0;
            // 
            // tlsTools
            // 
            this.tlsTools.AutoSize = false;
            this.tlsTools.BackColor = System.Drawing.Color.Transparent;
            this.tlsTools.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tlsTools.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tlsTools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tslZoom,
            this.tslTool});
            this.tlsTools.Location = new System.Drawing.Point(0, 635);
            this.tlsTools.Name = "tlsTools";
            this.tlsTools.Padding = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.tlsTools.Size = new System.Drawing.Size(829, 27);
            this.tlsTools.TabIndex = 7;
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
            this.tslTool.Size = new System.Drawing.Size(69, 22);
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
            this.splProperties.Panel1.Controls.Add(this.treNodes);
            // 
            // splProperties.Panel2
            // 
            this.splProperties.Panel2.Controls.Add(this.pptNodeProperties);
            this.splProperties.Panel2.Controls.Add(this.tlsProperties);
            this.splProperties.Size = new System.Drawing.Size(256, 662);
            this.splProperties.SplitterDistance = 331;
            this.splProperties.TabIndex = 2;
            // 
            // treNodes
            // 
            this.treNodes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treNodes.FullRowSelect = true;
            this.treNodes.HideSelection = false;
            this.treNodes.ImageIndex = 0;
            this.treNodes.ImageList = this.imlNodes;
            this.treNodes.ItemHeight = 18;
            this.treNodes.Location = new System.Drawing.Point(0, 0);
            this.treNodes.Name = "treNodes";
            this.treNodes.SelectedImageIndex = 0;
            this.treNodes.ShowLines = false;
            this.treNodes.ShowPlusMinus = false;
            this.treNodes.ShowRootLines = false;
            this.treNodes.Size = new System.Drawing.Size(256, 331);
            this.treNodes.TabIndex = 1;
            // 
            // pptNodeProperties
            // 
            this.pptNodeProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pptNodeProperties.LineColor = System.Drawing.SystemColors.ControlDark;
            this.pptNodeProperties.Location = new System.Drawing.Point(0, 27);
            this.pptNodeProperties.Name = "pptNodeProperties";
            this.pptNodeProperties.Size = new System.Drawing.Size(256, 300);
            this.pptNodeProperties.TabIndex = 0;
            this.pptNodeProperties.ToolbarVisible = false;
            // 
            // tlsProperties
            // 
            this.tlsProperties.AutoSize = false;
            this.tlsProperties.BackColor = System.Drawing.Color.Transparent;
            this.tlsProperties.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tlsProperties.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbExtendedProperties,
            this.tslProperties});
            this.tlsProperties.Location = new System.Drawing.Point(0, 0);
            this.tlsProperties.Name = "tlsProperties";
            this.tlsProperties.Padding = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.tlsProperties.Size = new System.Drawing.Size(256, 27);
            this.tlsProperties.TabIndex = 8;
            // 
            // tsbExtendedProperties
            // 
            this.tsbExtendedProperties.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tsbExtendedProperties.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbExtendedProperties.Enabled = false;
            this.tsbExtendedProperties.Image = global::Kasutera.Properties.Resources.menu_properties;
            this.tsbExtendedProperties.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbExtendedProperties.Name = "tsbExtendedProperties";
            this.tsbExtendedProperties.Size = new System.Drawing.Size(23, 22);
            this.tsbExtendedProperties.Text = "Extended Properties";
            // 
            // tslProperties
            // 
            this.tslProperties.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tslProperties.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tslProperties.Name = "tslProperties";
            this.tslProperties.Size = new System.Drawing.Size(63, 22);
            this.tslProperties.Text = "Properties:";
            // 
            // imbPreview
            // 
            this.imbPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.imbPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imbPreview.GridCellSize = 16;
            this.imbPreview.GridColor = System.Drawing.Color.Silver;
            this.imbPreview.ImageBorderColor = System.Drawing.Color.Black;
            this.imbPreview.ImageBorderStyle = Cyotek.Windows.Forms.ImageBoxBorderStyle.FixedSingleDropShadow;
            this.imbPreview.Location = new System.Drawing.Point(0, 0);
            this.imbPreview.Name = "imbPreview";
            this.imbPreview.SelectionMode = Cyotek.Windows.Forms.ImageBoxSelectionMode.Zoom;
            this.imbPreview.Size = new System.Drawing.Size(829, 635);
            this.imbPreview.TabIndex = 8;
            // 
            // imlNodes
            // 
            this.imlNodes.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imlNodes.ImageSize = new System.Drawing.Size(16, 16);
            this.imlNodes.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // Interpreter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1101, 698);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.mnuMain);
            this.Name = "Interpreter";
            this.Text = "Kasutera";
            this.mnuMain.ResumeLayout(false);
            this.mnuMain.PerformLayout();
            this.pnlMain.ResumeLayout(false);
            this.splMain.Panel1.ResumeLayout(false);
            this.splMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splMain)).EndInit();
            this.splMain.ResumeLayout(false);
            this.tlsTools.ResumeLayout(false);
            this.tlsTools.PerformLayout();
            this.splProperties.Panel1.ResumeLayout(false);
            this.splProperties.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splProperties)).EndInit();
            this.splProperties.ResumeLayout(false);
            this.tlsProperties.ResumeLayout(false);
            this.tlsProperties.PerformLayout();
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
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem compressionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem encryptionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hashToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gBATempToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gitHubToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem1;
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.SplitContainer splMain;
        private System.Windows.Forms.ToolStrip tlsTools;
        private System.Windows.Forms.ToolStripLabel tslZoom;
        private System.Windows.Forms.ToolStripLabel tslTool;
        private System.Windows.Forms.SplitContainer splProperties;
        private System.Windows.Forms.TreeView treNodes;
        private System.Windows.Forms.PropertyGrid pptNodeProperties;
        private System.Windows.Forms.ToolStrip tlsProperties;
        private System.Windows.Forms.ToolStripButton tsbExtendedProperties;
        private System.Windows.Forms.ToolStripLabel tslProperties;
        private Cyotek.Windows.Forms.ImageBox imbPreview;
        private System.Windows.Forms.ImageList imlNodes;
    }
}

