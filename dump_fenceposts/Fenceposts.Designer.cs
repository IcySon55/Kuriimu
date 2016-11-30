namespace dump_fenceposts
{
	partial class Fenceposts
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
			this.prgBottom = new System.Windows.Forms.ProgressBar();
			this.prgTop = new System.Windows.Forms.ProgressBar();
			this.txtOutput = new System.Windows.Forms.TextBox();
			this.txtFilename = new System.Windows.Forms.TextBox();
			this.btnBrowse = new System.Windows.Forms.Button();
			this.btnDump = new System.Windows.Forms.Button();
			this.lstStatus = new System.Windows.Forms.ListBox();
			this.lstDumpingBounds = new System.Windows.Forms.ListBox();
			this.lstPointerTables = new System.Windows.Forms.ListBox();
			this.mnuMain = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.pnlMain = new System.Windows.Forms.Panel();
			this.splConfigure = new System.Windows.Forms.SplitContainer();
			this.splBounds = new System.Windows.Forms.SplitContainer();
			this.tlsPointerTables = new System.Windows.Forms.ToolStrip();
			this.tslEntries = new System.Windows.Forms.ToolStripLabel();
			this.tsbPointerTableAdd = new System.Windows.Forms.ToolStripButton();
			this.tsbPointerTableProperties = new System.Windows.Forms.ToolStripButton();
			this.tsbPointerTableDelete = new System.Windows.Forms.ToolStripButton();
			this.tlsDumpingBounds = new System.Windows.Forms.ToolStrip();
			this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
			this.tsbDumpingBoundAdd = new System.Windows.Forms.ToolStripButton();
			this.tsbDumpingBoundProperties = new System.Windows.Forms.ToolStripButton();
			this.tsbDumpingBoundDelete = new System.Windows.Forms.ToolStripButton();
			this.splStatus = new System.Windows.Forms.SplitContainer();
			this.pnlCommands = new System.Windows.Forms.Panel();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.pnlProgress = new System.Windows.Forms.Panel();
			this.lblRamOffset = new System.Windows.Forms.Label();
			this.txtRamOffset = new System.Windows.Forms.TextBox();
			this.mnuMain.SuspendLayout();
			this.pnlMain.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splConfigure)).BeginInit();
			this.splConfigure.Panel1.SuspendLayout();
			this.splConfigure.Panel2.SuspendLayout();
			this.splConfigure.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splBounds)).BeginInit();
			this.splBounds.Panel1.SuspendLayout();
			this.splBounds.Panel2.SuspendLayout();
			this.splBounds.SuspendLayout();
			this.tlsPointerTables.SuspendLayout();
			this.tlsDumpingBounds.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splStatus)).BeginInit();
			this.splStatus.Panel1.SuspendLayout();
			this.splStatus.Panel2.SuspendLayout();
			this.splStatus.SuspendLayout();
			this.pnlCommands.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.pnlProgress.SuspendLayout();
			this.SuspendLayout();
			// 
			// prgBottom
			// 
			this.prgBottom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.prgBottom.Location = new System.Drawing.Point(0, 35);
			this.prgBottom.Margin = new System.Windows.Forms.Padding(0);
			this.prgBottom.Name = "prgBottom";
			this.prgBottom.Size = new System.Drawing.Size(863, 23);
			this.prgBottom.TabIndex = 0;
			// 
			// prgTop
			// 
			this.prgTop.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.prgTop.Location = new System.Drawing.Point(0, 6);
			this.prgTop.Margin = new System.Windows.Forms.Padding(0);
			this.prgTop.Name = "prgTop";
			this.prgTop.Size = new System.Drawing.Size(863, 23);
			this.prgTop.TabIndex = 1;
			// 
			// txtOutput
			// 
			this.txtOutput.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtOutput.Enabled = false;
			this.txtOutput.Location = new System.Drawing.Point(0, 81);
			this.txtOutput.Multiline = true;
			this.txtOutput.Name = "txtOutput";
			this.txtOutput.Size = new System.Drawing.Size(662, 294);
			this.txtOutput.TabIndex = 2;
			// 
			// txtFilename
			// 
			this.txtFilename.Location = new System.Drawing.Point(6, 21);
			this.txtFilename.Name = "txtFilename";
			this.txtFilename.Size = new System.Drawing.Size(566, 20);
			this.txtFilename.TabIndex = 3;
			this.txtFilename.TextChanged += new System.EventHandler(this.txtFilename_TextChanged);
			// 
			// btnBrowse
			// 
			this.btnBrowse.Location = new System.Drawing.Point(578, 19);
			this.btnBrowse.Name = "btnBrowse";
			this.btnBrowse.Size = new System.Drawing.Size(75, 23);
			this.btnBrowse.TabIndex = 4;
			this.btnBrowse.Text = "...";
			this.btnBrowse.UseVisualStyleBackColor = true;
			this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
			// 
			// btnDump
			// 
			this.btnDump.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.btnDump.Location = new System.Drawing.Point(331, 47);
			this.btnDump.Name = "btnDump";
			this.btnDump.Size = new System.Drawing.Size(322, 23);
			this.btnDump.TabIndex = 5;
			this.btnDump.Text = "Dump!";
			this.btnDump.UseVisualStyleBackColor = true;
			this.btnDump.Click += new System.EventHandler(this.btnDump_Click);
			// 
			// lstStatus
			// 
			this.lstStatus.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lstStatus.Enabled = false;
			this.lstStatus.FormattingEnabled = true;
			this.lstStatus.IntegralHeight = false;
			this.lstStatus.Location = new System.Drawing.Point(0, 0);
			this.lstStatus.Name = "lstStatus";
			this.lstStatus.Size = new System.Drawing.Size(662, 100);
			this.lstStatus.TabIndex = 6;
			// 
			// lstDumpingBounds
			// 
			this.lstDumpingBounds.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lstDumpingBounds.Enabled = false;
			this.lstDumpingBounds.FormattingEnabled = true;
			this.lstDumpingBounds.IntegralHeight = false;
			this.lstDumpingBounds.Location = new System.Drawing.Point(0, 27);
			this.lstDumpingBounds.Name = "lstDumpingBounds";
			this.lstDumpingBounds.Size = new System.Drawing.Size(197, 218);
			this.lstDumpingBounds.TabIndex = 9;
			this.lstDumpingBounds.SelectedIndexChanged += new System.EventHandler(this.lstDumpingBounds_SelectedIndexChanged);
			// 
			// lstPointerTables
			// 
			this.lstPointerTables.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lstPointerTables.Enabled = false;
			this.lstPointerTables.FormattingEnabled = true;
			this.lstPointerTables.IntegralHeight = false;
			this.lstPointerTables.Location = new System.Drawing.Point(0, 27);
			this.lstPointerTables.Name = "lstPointerTables";
			this.lstPointerTables.Size = new System.Drawing.Size(197, 203);
			this.lstPointerTables.TabIndex = 10;
			this.lstPointerTables.SelectedIndexChanged += new System.EventHandler(this.lstPointerTables_SelectedIndexChanged);
			// 
			// mnuMain
			// 
			this.mnuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
			this.mnuMain.Location = new System.Drawing.Point(0, 0);
			this.mnuMain.Name = "mnuMain";
			this.mnuMain.Padding = new System.Windows.Forms.Padding(2, 2, 0, 2);
			this.mnuMain.Size = new System.Drawing.Size(875, 24);
			this.mnuMain.TabIndex = 13;
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
			this.newToolStripMenuItem.Image = global::dump_fenceposts.Properties.Resources.menu_new;
			this.newToolStripMenuItem.Name = "newToolStripMenuItem";
			this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
			this.newToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
			this.newToolStripMenuItem.Text = "&New";
			this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
			// 
			// openToolStripMenuItem
			// 
			this.openToolStripMenuItem.Image = global::dump_fenceposts.Properties.Resources.menu_open;
			this.openToolStripMenuItem.Name = "openToolStripMenuItem";
			this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.openToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
			this.openToolStripMenuItem.Text = "&Open";
			this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
			// 
			// saveToolStripMenuItem
			// 
			this.saveToolStripMenuItem.Image = global::dump_fenceposts.Properties.Resources.menu_save;
			this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
			this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.saveToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
			this.saveToolStripMenuItem.Text = "&Save";
			this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
			// 
			// saveAsToolStripMenuItem
			// 
			this.saveAsToolStripMenuItem.Image = global::dump_fenceposts.Properties.Resources.menu_save_as;
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
			this.exitToolStripMenuItem.Image = global::dump_fenceposts.Properties.Resources.menu_exit;
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
			this.exitToolStripMenuItem.Text = "E&xit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
			// 
			// pnlMain
			// 
			this.pnlMain.Controls.Add(this.splConfigure);
			this.pnlMain.Controls.Add(this.pnlProgress);
			this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlMain.Location = new System.Drawing.Point(0, 24);
			this.pnlMain.Name = "pnlMain";
			this.pnlMain.Padding = new System.Windows.Forms.Padding(6);
			this.pnlMain.Size = new System.Drawing.Size(875, 549);
			this.pnlMain.TabIndex = 14;
			// 
			// splConfigure
			// 
			this.splConfigure.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splConfigure.Location = new System.Drawing.Point(6, 6);
			this.splConfigure.Name = "splConfigure";
			// 
			// splConfigure.Panel1
			// 
			this.splConfigure.Panel1.Controls.Add(this.splBounds);
			// 
			// splConfigure.Panel2
			// 
			this.splConfigure.Panel2.Controls.Add(this.splStatus);
			this.splConfigure.Size = new System.Drawing.Size(863, 479);
			this.splConfigure.SplitterDistance = 197;
			this.splConfigure.TabIndex = 0;
			// 
			// splBounds
			// 
			this.splBounds.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splBounds.Location = new System.Drawing.Point(0, 0);
			this.splBounds.Name = "splBounds";
			this.splBounds.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splBounds.Panel1
			// 
			this.splBounds.Panel1.Controls.Add(this.lstPointerTables);
			this.splBounds.Panel1.Controls.Add(this.tlsPointerTables);
			// 
			// splBounds.Panel2
			// 
			this.splBounds.Panel2.Controls.Add(this.lstDumpingBounds);
			this.splBounds.Panel2.Controls.Add(this.tlsDumpingBounds);
			this.splBounds.Size = new System.Drawing.Size(197, 479);
			this.splBounds.SplitterDistance = 230;
			this.splBounds.TabIndex = 0;
			// 
			// tlsPointerTables
			// 
			this.tlsPointerTables.AutoSize = false;
			this.tlsPointerTables.BackColor = System.Drawing.Color.Transparent;
			this.tlsPointerTables.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.tlsPointerTables.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tslEntries,
            this.tsbPointerTableAdd,
            this.tsbPointerTableProperties,
            this.tsbPointerTableDelete});
			this.tlsPointerTables.Location = new System.Drawing.Point(0, 0);
			this.tlsPointerTables.Name = "tlsPointerTables";
			this.tlsPointerTables.Padding = new System.Windows.Forms.Padding(2, 1, 2, 1);
			this.tlsPointerTables.Size = new System.Drawing.Size(197, 27);
			this.tlsPointerTables.TabIndex = 1;
			// 
			// tslEntries
			// 
			this.tslEntries.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.tslEntries.Name = "tslEntries";
			this.tslEntries.Size = new System.Drawing.Size(82, 22);
			this.tslEntries.Text = "Pointer Tables";
			// 
			// tsbPointerTableAdd
			// 
			this.tsbPointerTableAdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbPointerTableAdd.Enabled = false;
			this.tsbPointerTableAdd.Image = global::dump_fenceposts.Properties.Resources.menu_add;
			this.tsbPointerTableAdd.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbPointerTableAdd.Name = "tsbPointerTableAdd";
			this.tsbPointerTableAdd.Size = new System.Drawing.Size(23, 22);
			this.tsbPointerTableAdd.Text = "Add Pointer Table";
			this.tsbPointerTableAdd.Click += new System.EventHandler(this.tsbPointerTableAdd_Click);
			// 
			// tsbPointerTableProperties
			// 
			this.tsbPointerTableProperties.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbPointerTableProperties.Enabled = false;
			this.tsbPointerTableProperties.Image = global::dump_fenceposts.Properties.Resources.menu_properties;
			this.tsbPointerTableProperties.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbPointerTableProperties.Name = "tsbPointerTableProperties";
			this.tsbPointerTableProperties.Size = new System.Drawing.Size(23, 22);
			this.tsbPointerTableProperties.Text = "Pointer Table Properties";
			this.tsbPointerTableProperties.Click += new System.EventHandler(this.tsbPointerTableProperties_Click);
			// 
			// tsbPointerTableDelete
			// 
			this.tsbPointerTableDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbPointerTableDelete.Enabled = false;
			this.tsbPointerTableDelete.Image = global::dump_fenceposts.Properties.Resources.menu_delete;
			this.tsbPointerTableDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbPointerTableDelete.Name = "tsbPointerTableDelete";
			this.tsbPointerTableDelete.Size = new System.Drawing.Size(23, 22);
			this.tsbPointerTableDelete.Text = "Delete Pointer Table";
			this.tsbPointerTableDelete.Click += new System.EventHandler(this.tsbPointerTableDelete_Click);
			// 
			// tlsDumpingBounds
			// 
			this.tlsDumpingBounds.AutoSize = false;
			this.tlsDumpingBounds.BackColor = System.Drawing.Color.Transparent;
			this.tlsDumpingBounds.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.tlsDumpingBounds.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.tsbDumpingBoundAdd,
            this.tsbDumpingBoundProperties,
            this.tsbDumpingBoundDelete});
			this.tlsDumpingBounds.Location = new System.Drawing.Point(0, 0);
			this.tlsDumpingBounds.Name = "tlsDumpingBounds";
			this.tlsDumpingBounds.Padding = new System.Windows.Forms.Padding(2, 1, 2, 1);
			this.tlsDumpingBounds.Size = new System.Drawing.Size(197, 27);
			this.tlsDumpingBounds.TabIndex = 2;
			// 
			// toolStripLabel1
			// 
			this.toolStripLabel1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.toolStripLabel1.Name = "toolStripLabel1";
			this.toolStripLabel1.Size = new System.Drawing.Size(100, 22);
			this.toolStripLabel1.Text = "Dumping Bounds";
			// 
			// tsbDumpingBoundAdd
			// 
			this.tsbDumpingBoundAdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbDumpingBoundAdd.Enabled = false;
			this.tsbDumpingBoundAdd.Image = global::dump_fenceposts.Properties.Resources.menu_add;
			this.tsbDumpingBoundAdd.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbDumpingBoundAdd.Name = "tsbDumpingBoundAdd";
			this.tsbDumpingBoundAdd.Size = new System.Drawing.Size(23, 22);
			this.tsbDumpingBoundAdd.Text = "Add Dumping Bound";
			this.tsbDumpingBoundAdd.Click += new System.EventHandler(this.tsbDumpingBoundAdd_Click);
			// 
			// tsbDumpingBoundProperties
			// 
			this.tsbDumpingBoundProperties.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbDumpingBoundProperties.Enabled = false;
			this.tsbDumpingBoundProperties.Image = global::dump_fenceposts.Properties.Resources.menu_properties;
			this.tsbDumpingBoundProperties.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbDumpingBoundProperties.Name = "tsbDumpingBoundProperties";
			this.tsbDumpingBoundProperties.Size = new System.Drawing.Size(23, 22);
			this.tsbDumpingBoundProperties.Text = "Dumping Bound Properties";
			this.tsbDumpingBoundProperties.Click += new System.EventHandler(this.tsbDumpingBoundProperties_Click);
			// 
			// tsbDumpingBoundDelete
			// 
			this.tsbDumpingBoundDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbDumpingBoundDelete.Enabled = false;
			this.tsbDumpingBoundDelete.Image = global::dump_fenceposts.Properties.Resources.menu_delete;
			this.tsbDumpingBoundDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbDumpingBoundDelete.Name = "tsbDumpingBoundDelete";
			this.tsbDumpingBoundDelete.Size = new System.Drawing.Size(23, 22);
			this.tsbDumpingBoundDelete.Text = "Delete Dumping Bound";
			this.tsbDumpingBoundDelete.Click += new System.EventHandler(this.tsbDumpingBoundDelete_Click);
			// 
			// splStatus
			// 
			this.splStatus.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splStatus.Location = new System.Drawing.Point(0, 0);
			this.splStatus.Name = "splStatus";
			this.splStatus.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splStatus.Panel1
			// 
			this.splStatus.Panel1.Controls.Add(this.txtOutput);
			this.splStatus.Panel1.Controls.Add(this.pnlCommands);
			// 
			// splStatus.Panel2
			// 
			this.splStatus.Panel2.Controls.Add(this.lstStatus);
			this.splStatus.Size = new System.Drawing.Size(662, 479);
			this.splStatus.SplitterDistance = 375;
			this.splStatus.TabIndex = 0;
			// 
			// pnlCommands
			// 
			this.pnlCommands.Controls.Add(this.groupBox1);
			this.pnlCommands.Dock = System.Windows.Forms.DockStyle.Top;
			this.pnlCommands.Location = new System.Drawing.Point(0, 0);
			this.pnlCommands.Name = "pnlCommands";
			this.pnlCommands.Padding = new System.Windows.Forms.Padding(0, 0, 0, 4);
			this.pnlCommands.Size = new System.Drawing.Size(662, 81);
			this.pnlCommands.TabIndex = 0;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.txtRamOffset);
			this.groupBox1.Controls.Add(this.lblRamOffset);
			this.groupBox1.Controls.Add(this.txtFilename);
			this.groupBox1.Controls.Add(this.btnDump);
			this.groupBox1.Controls.Add(this.btnBrowse);
			this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBox1.Location = new System.Drawing.Point(0, 0);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(662, 77);
			this.groupBox1.TabIndex = 8;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Dumping Control";
			// 
			// pnlProgress
			// 
			this.pnlProgress.Controls.Add(this.prgTop);
			this.pnlProgress.Controls.Add(this.prgBottom);
			this.pnlProgress.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pnlProgress.Location = new System.Drawing.Point(6, 485);
			this.pnlProgress.Name = "pnlProgress";
			this.pnlProgress.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
			this.pnlProgress.Size = new System.Drawing.Size(863, 58);
			this.pnlProgress.TabIndex = 1;
			// 
			// lblRamOffset
			// 
			this.lblRamOffset.AutoSize = true;
			this.lblRamOffset.Location = new System.Drawing.Point(6, 52);
			this.lblRamOffset.Name = "lblRamOffset";
			this.lblRamOffset.Size = new System.Drawing.Size(78, 13);
			this.lblRamOffset.TabIndex = 6;
			this.lblRamOffset.Text = "Memory Offset:";
			// 
			// txtRamOffset
			// 
			this.txtRamOffset.Location = new System.Drawing.Point(90, 49);
			this.txtRamOffset.Name = "txtRamOffset";
			this.txtRamOffset.Size = new System.Drawing.Size(235, 20);
			this.txtRamOffset.TabIndex = 7;
			this.txtRamOffset.TextChanged += new System.EventHandler(this.txtRamOffset_TextChanged);
			// 
			// Fenceposts
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(875, 573);
			this.Controls.Add(this.pnlMain);
			this.Controls.Add(this.mnuMain);
			this.Name = "Fenceposts";
			this.Text = "Fenceposts";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Fenceposts_FormClosing);
			this.Load += new System.EventHandler(this.Fenceposts_Load);
			this.mnuMain.ResumeLayout(false);
			this.mnuMain.PerformLayout();
			this.pnlMain.ResumeLayout(false);
			this.splConfigure.Panel1.ResumeLayout(false);
			this.splConfigure.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splConfigure)).EndInit();
			this.splConfigure.ResumeLayout(false);
			this.splBounds.Panel1.ResumeLayout(false);
			this.splBounds.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splBounds)).EndInit();
			this.splBounds.ResumeLayout(false);
			this.tlsPointerTables.ResumeLayout(false);
			this.tlsPointerTables.PerformLayout();
			this.tlsDumpingBounds.ResumeLayout(false);
			this.tlsDumpingBounds.PerformLayout();
			this.splStatus.Panel1.ResumeLayout(false);
			this.splStatus.Panel1.PerformLayout();
			this.splStatus.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splStatus)).EndInit();
			this.splStatus.ResumeLayout(false);
			this.pnlCommands.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.pnlProgress.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ProgressBar prgBottom;
		private System.Windows.Forms.ProgressBar prgTop;
		private System.Windows.Forms.TextBox txtOutput;
		private System.Windows.Forms.TextBox txtFilename;
		private System.Windows.Forms.Button btnBrowse;
		private System.Windows.Forms.Button btnDump;
		private System.Windows.Forms.ListBox lstStatus;
		private System.Windows.Forms.ListBox lstDumpingBounds;
		private System.Windows.Forms.ListBox lstPointerTables;
		private System.Windows.Forms.MenuStrip mnuMain;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.Panel pnlMain;
		private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
		private System.Windows.Forms.SplitContainer splConfigure;
		private System.Windows.Forms.Panel pnlProgress;
		private System.Windows.Forms.SplitContainer splBounds;
		private System.Windows.Forms.ToolStrip tlsPointerTables;
		private System.Windows.Forms.ToolStripLabel tslEntries;
		private System.Windows.Forms.ToolStripButton tsbPointerTableAdd;
		private System.Windows.Forms.ToolStripButton tsbPointerTableProperties;
		private System.Windows.Forms.ToolStripButton tsbPointerTableDelete;
		private System.Windows.Forms.ToolStrip tlsDumpingBounds;
		private System.Windows.Forms.ToolStripLabel toolStripLabel1;
		private System.Windows.Forms.ToolStripButton tsbDumpingBoundAdd;
		private System.Windows.Forms.ToolStripButton tsbDumpingBoundProperties;
		private System.Windows.Forms.ToolStripButton tsbDumpingBoundDelete;
		private System.Windows.Forms.SplitContainer splStatus;
		private System.Windows.Forms.Panel pnlCommands;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label lblRamOffset;
		private System.Windows.Forms.TextBox txtRamOffset;
	}
}