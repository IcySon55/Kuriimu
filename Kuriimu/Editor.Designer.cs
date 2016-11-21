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
			this.pnlMain = new System.Windows.Forms.Panel();
			this.splMain = new System.Windows.Forms.SplitContainer();
			this.lstEntries = new System.Windows.Forms.ListBox();
			this.tlsEntries = new System.Windows.Forms.ToolStrip();
			this.tslEntries = new System.Windows.Forms.ToolStripLabel();
			this.splContent = new System.Windows.Forms.SplitContainer();
			this.txtEdit = new System.Windows.Forms.TextBox();
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.tslText = new System.Windows.Forms.ToolStripLabel();
			this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
			this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
			this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
			this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
			this.toolStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// mnuMain
			// 
			this.mnuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
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
            this.saveAsToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "&File";
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
            this.toolStripButton3});
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
			// splContent
			// 
			this.splContent.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splContent.Location = new System.Drawing.Point(0, 0);
			this.splContent.Name = "splContent";
			this.splContent.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splContent.Panel1
			// 
			this.splContent.Panel1.Controls.Add(this.txtEdit);
			this.splContent.Panel1.Controls.Add(this.toolStrip1);
			this.splContent.Size = new System.Drawing.Size(839, 632);
			this.splContent.SplitterDistance = 309;
			this.splContent.SplitterWidth = 6;
			this.splContent.TabIndex = 0;
			// 
			// txtEdit
			// 
			this.txtEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtEdit.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtEdit.Location = new System.Drawing.Point(0, 27);
			this.txtEdit.Multiline = true;
			this.txtEdit.Name = "txtEdit";
			this.txtEdit.Size = new System.Drawing.Size(839, 282);
			this.txtEdit.TabIndex = 0;
			// 
			// toolStrip1
			// 
			this.toolStrip1.AutoSize = false;
			this.toolStrip1.BackColor = System.Drawing.Color.Transparent;
			this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tslText});
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Padding = new System.Windows.Forms.Padding(2, 1, 2, 1);
			this.toolStrip1.Size = new System.Drawing.Size(839, 27);
			this.toolStrip1.TabIndex = 1;
			// 
			// tslText
			// 
			this.tslText.Name = "tslText";
			this.tslText.Size = new System.Drawing.Size(32, 22);
			this.tslText.Text = "Text:";
			// 
			// toolStripButton1
			// 
			this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButton1.Enabled = false;
			this.toolStripButton1.Image = global::Kuriimu.Properties.Resources.menu_add;
			this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton1.Name = "toolStripButton1";
			this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
			this.toolStripButton1.Text = "toolStripButton1";
			// 
			// toolStripButton2
			// 
			this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButton2.Enabled = false;
			this.toolStripButton2.Image = global::Kuriimu.Properties.Resources.menu_field_properties;
			this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton2.Name = "toolStripButton2";
			this.toolStripButton2.Size = new System.Drawing.Size(23, 22);
			this.toolStripButton2.Text = "toolStripButton2";
			// 
			// toolStripButton3
			// 
			this.toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButton3.Enabled = false;
			this.toolStripButton3.Image = global::Kuriimu.Properties.Resources.menu_delete;
			this.toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton3.Name = "toolStripButton3";
			this.toolStripButton3.Size = new System.Drawing.Size(23, 22);
			this.toolStripButton3.Text = "toolStripButton3";
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
			this.saveToolStripMenuItem.Image = global::Kuriimu.Properties.Resources.menu_save;
			this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
			this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.saveToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.saveToolStripMenuItem.Text = "&Save";
			this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
			// 
			// saveAsToolStripMenuItem
			// 
			this.saveAsToolStripMenuItem.Image = global::Kuriimu.Properties.Resources.menu_save_as;
			this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
			this.saveAsToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F12;
			this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.saveAsToolStripMenuItem.Text = "Save As...";
			this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
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
			this.splContent.Panel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splContent)).EndInit();
			this.splContent.ResumeLayout(false);
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
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
		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripLabel tslText;
		private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
	}
}