namespace ext_fenceposts
{
	partial class frmExtension
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
			this.prgBottom = new System.Windows.Forms.ProgressBar();
			this.prgTop = new System.Windows.Forms.ProgressBar();
			this.txtFilename = new System.Windows.Forms.TextBox();
			this.btnBrowse = new System.Windows.Forms.Button();
			this.btnDump = new System.Windows.Forms.Button();
			this.lstStatus = new System.Windows.Forms.ListBox();
			this.cmsStatus = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.copyOffsetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.lstStringBounds = new System.Windows.Forms.ListBox();
			this.lstPointerTables = new System.Windows.Forms.ListBox();
			this.pnlMain = new System.Windows.Forms.Panel();
			this.splConfigure = new System.Windows.Forms.SplitContainer();
			this.splBounds = new System.Windows.Forms.SplitContainer();
			this.tlsPointerTables = new System.Windows.Forms.ToolStrip();
			this.tslEntries = new System.Windows.Forms.ToolStripLabel();
			this.tsbPointerTableAdd = new System.Windows.Forms.ToolStripButton();
			this.tsbPointerTableProperties = new System.Windows.Forms.ToolStripButton();
			this.tsbPointerTableDelete = new System.Windows.Forms.ToolStripButton();
			this.tlsStringBounds = new System.Windows.Forms.ToolStrip();
			this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
			this.tsbStringBoundAdd = new System.Windows.Forms.ToolStripButton();
			this.tsbStringBoundProperties = new System.Windows.Forms.ToolStripButton();
			this.tsbStringBoundDelete = new System.Windows.Forms.ToolStripButton();
			this.splStatus = new System.Windows.Forms.SplitContainer();
			this.pnlCommands = new System.Windows.Forms.Panel();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.chkCleanDump = new System.Windows.Forms.CheckBox();
			this.lblFile = new System.Windows.Forms.Label();
			this.btnInject = new System.Windows.Forms.Button();
			this.txtRamOffset = new System.Windows.Forms.TextBox();
			this.lblRamOffset = new System.Windows.Forms.Label();
			this.pnlProgress = new System.Windows.Forms.Panel();
			this.mnuMain = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.tsbNew = new System.Windows.Forms.ToolStripButton();
			this.tsbOpen = new System.Windows.Forms.ToolStripButton();
			this.tsbSave = new System.Windows.Forms.ToolStripButton();
			this.tsbSaveAs = new System.Windows.Forms.ToolStripButton();
			this.tlsMain = new System.Windows.Forms.ToolStrip();
			this.tsbGameSelect = new System.Windows.Forms.ToolStripDropDownButton();
			this.tslDumpUsing = new System.Windows.Forms.ToolStripLabel();
			this.cmsStatus.SuspendLayout();
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
			this.tlsStringBounds.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splStatus)).BeginInit();
			this.splStatus.Panel1.SuspendLayout();
			this.splStatus.SuspendLayout();
			this.pnlCommands.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.pnlProgress.SuspendLayout();
			this.mnuMain.SuspendLayout();
			this.tlsMain.SuspendLayout();
			this.SuspendLayout();
			// 
			// prgBottom
			// 
			this.prgBottom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.prgBottom.Location = new System.Drawing.Point(0, 35);
			this.prgBottom.Margin = new System.Windows.Forms.Padding(0);
			this.prgBottom.Maximum = 1000;
			this.prgBottom.Name = "prgBottom";
			this.prgBottom.Size = new System.Drawing.Size(863, 23);
			this.prgBottom.Step = 1;
			this.prgBottom.TabIndex = 0;
			// 
			// prgTop
			// 
			this.prgTop.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.prgTop.Location = new System.Drawing.Point(0, 6);
			this.prgTop.Margin = new System.Windows.Forms.Padding(0);
			this.prgTop.Maximum = 1000;
			this.prgTop.Name = "prgTop";
			this.prgTop.Size = new System.Drawing.Size(863, 23);
			this.prgTop.Step = 1;
			this.prgTop.TabIndex = 1;
			// 
			// txtFilename
			// 
			this.txtFilename.Location = new System.Drawing.Point(90, 21);
			this.txtFilename.Name = "txtFilename";
			this.txtFilename.Size = new System.Drawing.Size(482, 20);
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
			this.btnDump.Location = new System.Drawing.Point(347, 47);
			this.btnDump.Name = "btnDump";
			this.btnDump.Size = new System.Drawing.Size(150, 23);
			this.btnDump.TabIndex = 5;
			this.btnDump.Text = "Dump!";
			this.btnDump.UseVisualStyleBackColor = true;
			this.btnDump.Click += new System.EventHandler(this.btnDump_Click);
			// 
			// lstStatus
			// 
			this.lstStatus.ContextMenuStrip = this.cmsStatus;
			this.lstStatus.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lstStatus.Enabled = false;
			this.lstStatus.FormattingEnabled = true;
			this.lstStatus.IntegralHeight = false;
			this.lstStatus.Location = new System.Drawing.Point(0, 81);
			this.lstStatus.Name = "lstStatus";
			this.lstStatus.Size = new System.Drawing.Size(662, 371);
			this.lstStatus.TabIndex = 6;
			this.lstStatus.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lstStatus_KeyDown);
			this.lstStatus.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lstStatus_MouseDown);
			// 
			// cmsStatus
			// 
			this.cmsStatus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyOffsetToolStripMenuItem});
			this.cmsStatus.Name = "cmsStatus";
			this.cmsStatus.Size = new System.Drawing.Size(138, 26);
			this.cmsStatus.Opening += new System.ComponentModel.CancelEventHandler(this.cmsStatus_Opening);
			// 
			// copyOffsetToolStripMenuItem
			// 
			this.copyOffsetToolStripMenuItem.Image = global::ext_fenceposts.Properties.Resources.menu_copy;
			this.copyOffsetToolStripMenuItem.Name = "copyOffsetToolStripMenuItem";
			this.copyOffsetToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
			this.copyOffsetToolStripMenuItem.Text = "&Copy Offset";
			this.copyOffsetToolStripMenuItem.Click += new System.EventHandler(this.copyOffsetToolStripMenuItem_Click);
			// 
			// lstStringBounds
			// 
			this.lstStringBounds.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lstStringBounds.Enabled = false;
			this.lstStringBounds.FormattingEnabled = true;
			this.lstStringBounds.IntegralHeight = false;
			this.lstStringBounds.Location = new System.Drawing.Point(0, 27);
			this.lstStringBounds.Name = "lstStringBounds";
			this.lstStringBounds.Size = new System.Drawing.Size(197, 218);
			this.lstStringBounds.TabIndex = 9;
			this.lstStringBounds.SelectedIndexChanged += new System.EventHandler(this.lstStringBounds_SelectedIndexChanged);
			this.lstStringBounds.DoubleClick += new System.EventHandler(this.lstStringBounds_DoubleClick);
			// 
			// lstPointerTables
			// 
			this.lstPointerTables.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lstPointerTables.Enabled = false;
			this.lstPointerTables.FormattingEnabled = true;
			this.lstPointerTables.IntegralHeight = false;
			this.lstPointerTables.Location = new System.Drawing.Point(0, 27);
			this.lstPointerTables.Name = "lstPointerTables";
			this.lstPointerTables.Size = new System.Drawing.Size(197, 176);
			this.lstPointerTables.TabIndex = 10;
			this.lstPointerTables.SelectedIndexChanged += new System.EventHandler(this.lstPointerTables_SelectedIndexChanged);
			this.lstPointerTables.DoubleClick += new System.EventHandler(this.lstPointerTables_DoubleClick);
			// 
			// pnlMain
			// 
			this.pnlMain.Controls.Add(this.splConfigure);
			this.pnlMain.Controls.Add(this.pnlProgress);
			this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlMain.Location = new System.Drawing.Point(0, 51);
			this.pnlMain.Name = "pnlMain";
			this.pnlMain.Padding = new System.Windows.Forms.Padding(6);
			this.pnlMain.Size = new System.Drawing.Size(875, 522);
			this.pnlMain.TabIndex = 14;
			// 
			// splConfigure
			// 
			this.splConfigure.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splConfigure.Enabled = false;
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
			this.splConfigure.Size = new System.Drawing.Size(863, 452);
			this.splConfigure.SplitterDistance = 197;
			this.splConfigure.TabIndex = 0;
			// 
			// splBounds
			// 
			this.splBounds.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splBounds.Enabled = false;
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
			this.splBounds.Panel2.Controls.Add(this.lstStringBounds);
			this.splBounds.Panel2.Controls.Add(this.tlsStringBounds);
			this.splBounds.Size = new System.Drawing.Size(197, 452);
			this.splBounds.SplitterDistance = 203;
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
			this.tsbPointerTableAdd.Image = global::ext_fenceposts.Properties.Resources.menu_add;
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
			this.tsbPointerTableProperties.Image = global::ext_fenceposts.Properties.Resources.menu_properties;
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
			this.tsbPointerTableDelete.Image = global::ext_fenceposts.Properties.Resources.menu_delete;
			this.tsbPointerTableDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbPointerTableDelete.Name = "tsbPointerTableDelete";
			this.tsbPointerTableDelete.Size = new System.Drawing.Size(23, 22);
			this.tsbPointerTableDelete.Text = "Delete Pointer Table";
			this.tsbPointerTableDelete.Click += new System.EventHandler(this.tsbPointerTableDelete_Click);
			// 
			// tlsStringBounds
			// 
			this.tlsStringBounds.AutoSize = false;
			this.tlsStringBounds.BackColor = System.Drawing.Color.Transparent;
			this.tlsStringBounds.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.tlsStringBounds.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.tsbStringBoundAdd,
            this.tsbStringBoundProperties,
            this.tsbStringBoundDelete});
			this.tlsStringBounds.Location = new System.Drawing.Point(0, 0);
			this.tlsStringBounds.Name = "tlsStringBounds";
			this.tlsStringBounds.Padding = new System.Windows.Forms.Padding(2, 1, 2, 1);
			this.tlsStringBounds.Size = new System.Drawing.Size(197, 27);
			this.tlsStringBounds.TabIndex = 2;
			// 
			// toolStripLabel1
			// 
			this.toolStripLabel1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.toolStripLabel1.Name = "toolStripLabel1";
			this.toolStripLabel1.Size = new System.Drawing.Size(81, 22);
			this.toolStripLabel1.Text = "String Bounds";
			// 
			// tsbStringBoundAdd
			// 
			this.tsbStringBoundAdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbStringBoundAdd.Enabled = false;
			this.tsbStringBoundAdd.Image = global::ext_fenceposts.Properties.Resources.menu_add;
			this.tsbStringBoundAdd.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbStringBoundAdd.Name = "tsbStringBoundAdd";
			this.tsbStringBoundAdd.Size = new System.Drawing.Size(23, 22);
			this.tsbStringBoundAdd.Text = "Add Dumping Bound";
			this.tsbStringBoundAdd.Click += new System.EventHandler(this.tsbStringBoundAdd_Click);
			// 
			// tsbStringBoundProperties
			// 
			this.tsbStringBoundProperties.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbStringBoundProperties.Enabled = false;
			this.tsbStringBoundProperties.Image = global::ext_fenceposts.Properties.Resources.menu_properties;
			this.tsbStringBoundProperties.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbStringBoundProperties.Name = "tsbStringBoundProperties";
			this.tsbStringBoundProperties.Size = new System.Drawing.Size(23, 22);
			this.tsbStringBoundProperties.Text = "Dumping Bound Properties";
			this.tsbStringBoundProperties.Click += new System.EventHandler(this.tsbStringBoundProperties_Click);
			// 
			// tsbStringBoundDelete
			// 
			this.tsbStringBoundDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbStringBoundDelete.Enabled = false;
			this.tsbStringBoundDelete.Image = global::ext_fenceposts.Properties.Resources.menu_delete;
			this.tsbStringBoundDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbStringBoundDelete.Name = "tsbStringBoundDelete";
			this.tsbStringBoundDelete.Size = new System.Drawing.Size(23, 22);
			this.tsbStringBoundDelete.Text = "Delete Dumping Bound";
			this.tsbStringBoundDelete.Click += new System.EventHandler(this.tsbStringBoundDelete_Click);
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
			this.splStatus.Panel1.Controls.Add(this.lstStatus);
			this.splStatus.Panel1.Controls.Add(this.pnlCommands);
			this.splStatus.Panel2Collapsed = true;
			this.splStatus.Size = new System.Drawing.Size(662, 452);
			this.splStatus.SplitterDistance = 327;
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
			this.groupBox1.Controls.Add(this.chkCleanDump);
			this.groupBox1.Controls.Add(this.lblFile);
			this.groupBox1.Controls.Add(this.btnInject);
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
			this.groupBox1.Text = "Control Panel";
			// 
			// chkCleanDump
			// 
			this.chkCleanDump.Location = new System.Drawing.Point(215, 50);
			this.chkCleanDump.Name = "chkCleanDump";
			this.chkCleanDump.Size = new System.Drawing.Size(126, 19);
			this.chkCleanDump.TabIndex = 10;
			this.chkCleanDump.Text = "Clean Dump";
			this.chkCleanDump.UseVisualStyleBackColor = true;
			this.chkCleanDump.CheckedChanged += new System.EventHandler(this.chkCleanDump_CheckedChanged);
			// 
			// lblFile
			// 
			this.lblFile.AutoSize = true;
			this.lblFile.Location = new System.Drawing.Point(58, 24);
			this.lblFile.Name = "lblFile";
			this.lblFile.Size = new System.Drawing.Size(26, 13);
			this.lblFile.TabIndex = 9;
			this.lblFile.Text = "File:";
			// 
			// btnInject
			// 
			this.btnInject.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.btnInject.Location = new System.Drawing.Point(503, 47);
			this.btnInject.Name = "btnInject";
			this.btnInject.Size = new System.Drawing.Size(150, 23);
			this.btnInject.TabIndex = 8;
			this.btnInject.Text = "Inject!";
			this.btnInject.UseVisualStyleBackColor = true;
			this.btnInject.Click += new System.EventHandler(this.btnInject_Click);
			// 
			// txtRamOffset
			// 
			this.txtRamOffset.Location = new System.Drawing.Point(90, 49);
			this.txtRamOffset.Name = "txtRamOffset";
			this.txtRamOffset.Size = new System.Drawing.Size(119, 20);
			this.txtRamOffset.TabIndex = 7;
			this.txtRamOffset.TextChanged += new System.EventHandler(this.txtRamOffset_TextChanged);
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
			// pnlProgress
			// 
			this.pnlProgress.Controls.Add(this.prgTop);
			this.pnlProgress.Controls.Add(this.prgBottom);
			this.pnlProgress.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pnlProgress.Location = new System.Drawing.Point(6, 458);
			this.pnlProgress.Name = "pnlProgress";
			this.pnlProgress.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
			this.pnlProgress.Size = new System.Drawing.Size(863, 58);
			this.pnlProgress.TabIndex = 1;
			// 
			// mnuMain
			// 
			this.mnuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
			this.mnuMain.Location = new System.Drawing.Point(0, 0);
			this.mnuMain.Name = "mnuMain";
			this.mnuMain.Padding = new System.Windows.Forms.Padding(2, 2, 0, 2);
			this.mnuMain.Size = new System.Drawing.Size(875, 24);
			this.mnuMain.TabIndex = 16;
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
			this.newToolStripMenuItem.Image = global::ext_fenceposts.Properties.Resources.menu_new;
			this.newToolStripMenuItem.Name = "newToolStripMenuItem";
			this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
			this.newToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
			this.newToolStripMenuItem.Text = "&New";
			this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
			// 
			// openToolStripMenuItem
			// 
			this.openToolStripMenuItem.Image = global::ext_fenceposts.Properties.Resources.menu_open;
			this.openToolStripMenuItem.Name = "openToolStripMenuItem";
			this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.openToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
			this.openToolStripMenuItem.Text = "&Open";
			this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
			// 
			// saveToolStripMenuItem
			// 
			this.saveToolStripMenuItem.Enabled = false;
			this.saveToolStripMenuItem.Image = global::ext_fenceposts.Properties.Resources.menu_save;
			this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
			this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.saveToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
			this.saveToolStripMenuItem.Text = "&Save";
			this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
			// 
			// saveAsToolStripMenuItem
			// 
			this.saveAsToolStripMenuItem.Enabled = false;
			this.saveAsToolStripMenuItem.Image = global::ext_fenceposts.Properties.Resources.menu_save_as;
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
			this.exitToolStripMenuItem.Image = global::ext_fenceposts.Properties.Resources.menu_exit;
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
			this.exitToolStripMenuItem.Text = "E&xit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
			// 
			// tsbNew
			// 
			this.tsbNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbNew.Image = global::ext_fenceposts.Properties.Resources.menu_new;
			this.tsbNew.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbNew.Name = "tsbNew";
			this.tsbNew.Size = new System.Drawing.Size(23, 22);
			this.tsbNew.Text = "New KUP File";
			this.tsbNew.Click += new System.EventHandler(this.tsbNew_Click);
			// 
			// tsbOpen
			// 
			this.tsbOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbOpen.Image = global::ext_fenceposts.Properties.Resources.menu_open;
			this.tsbOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbOpen.Name = "tsbOpen";
			this.tsbOpen.Size = new System.Drawing.Size(23, 22);
			this.tsbOpen.Text = "Open KUP File";
			this.tsbOpen.Click += new System.EventHandler(this.tsbOpen_Click);
			// 
			// tsbSave
			// 
			this.tsbSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbSave.Enabled = false;
			this.tsbSave.Image = global::ext_fenceposts.Properties.Resources.menu_save;
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
			this.tsbSaveAs.Image = global::ext_fenceposts.Properties.Resources.menu_save_as;
			this.tsbSaveAs.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbSaveAs.Name = "tsbSaveAs";
			this.tsbSaveAs.Size = new System.Drawing.Size(23, 22);
			this.tsbSaveAs.Text = "Save As...";
			this.tsbSaveAs.Click += new System.EventHandler(this.tsbSaveAs_Click);
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
            this.tsbGameSelect,
            this.tslDumpUsing});
			this.tlsMain.Location = new System.Drawing.Point(0, 24);
			this.tlsMain.Name = "tlsMain";
			this.tlsMain.Padding = new System.Windows.Forms.Padding(2, 1, 2, 1);
			this.tlsMain.Size = new System.Drawing.Size(875, 27);
			this.tlsMain.TabIndex = 15;
			// 
			// tsbGameSelect
			// 
			this.tsbGameSelect.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.tsbGameSelect.Enabled = false;
			this.tsbGameSelect.Image = global::ext_fenceposts.Properties.Resources.game_none;
			this.tsbGameSelect.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbGameSelect.Name = "tsbGameSelect";
			this.tsbGameSelect.Size = new System.Drawing.Size(86, 22);
			this.tsbGameSelect.Text = "No Game";
			// 
			// tslDumpUsing
			// 
			this.tslDumpUsing.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.tslDumpUsing.Name = "tslDumpUsing";
			this.tslDumpUsing.Size = new System.Drawing.Size(75, 22);
			this.tslDumpUsing.Text = "Dump using:";
			// 
			// frmExtension
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(875, 573);
			this.Controls.Add(this.pnlMain);
			this.Controls.Add(this.tlsMain);
			this.Controls.Add(this.mnuMain);
			this.Name = "frmExtension";
			this.Text = "Fenceposts";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Fenceposts_FormClosing);
			this.Load += new System.EventHandler(this.Fenceposts_Load);
			this.cmsStatus.ResumeLayout(false);
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
			this.tlsStringBounds.ResumeLayout(false);
			this.tlsStringBounds.PerformLayout();
			this.splStatus.Panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splStatus)).EndInit();
			this.splStatus.ResumeLayout(false);
			this.pnlCommands.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.pnlProgress.ResumeLayout(false);
			this.mnuMain.ResumeLayout(false);
			this.mnuMain.PerformLayout();
			this.tlsMain.ResumeLayout(false);
			this.tlsMain.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ProgressBar prgBottom;
		private System.Windows.Forms.ProgressBar prgTop;
		private System.Windows.Forms.TextBox txtFilename;
		private System.Windows.Forms.Button btnBrowse;
		private System.Windows.Forms.Button btnDump;
		private System.Windows.Forms.ListBox lstStatus;
		private System.Windows.Forms.ListBox lstStringBounds;
		private System.Windows.Forms.ListBox lstPointerTables;
		private System.Windows.Forms.Panel pnlMain;
		private System.Windows.Forms.SplitContainer splConfigure;
		private System.Windows.Forms.Panel pnlProgress;
		private System.Windows.Forms.SplitContainer splBounds;
		private System.Windows.Forms.ToolStrip tlsPointerTables;
		private System.Windows.Forms.ToolStripLabel tslEntries;
		private System.Windows.Forms.ToolStripButton tsbPointerTableAdd;
		private System.Windows.Forms.ToolStripButton tsbPointerTableProperties;
		private System.Windows.Forms.ToolStripButton tsbPointerTableDelete;
		private System.Windows.Forms.ToolStrip tlsStringBounds;
		private System.Windows.Forms.ToolStripLabel toolStripLabel1;
		private System.Windows.Forms.ToolStripButton tsbStringBoundAdd;
		private System.Windows.Forms.ToolStripButton tsbStringBoundProperties;
		private System.Windows.Forms.ToolStripButton tsbStringBoundDelete;
		private System.Windows.Forms.SplitContainer splStatus;
		private System.Windows.Forms.Panel pnlCommands;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label lblRamOffset;
		private System.Windows.Forms.TextBox txtRamOffset;
		private System.Windows.Forms.Button btnInject;
		private System.Windows.Forms.Label lblFile;
		private System.Windows.Forms.MenuStrip mnuMain;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripButton tsbNew;
		private System.Windows.Forms.ToolStripButton tsbOpen;
		private System.Windows.Forms.ToolStripButton tsbSave;
		private System.Windows.Forms.ToolStripButton tsbSaveAs;
		private System.Windows.Forms.ToolStrip tlsMain;
		private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
		private System.Windows.Forms.ToolStripDropDownButton tsbGameSelect;
		private System.Windows.Forms.ToolStripLabel tslDumpUsing;
		private System.Windows.Forms.ContextMenuStrip cmsStatus;
		private System.Windows.Forms.ToolStripMenuItem copyOffsetToolStripMenuItem;
		private System.Windows.Forms.CheckBox chkCleanDump;
	}
}