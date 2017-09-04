namespace Kukkii
{
    partial class OpenRaw
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
            this.imbPreview = new Cyotek.Windows.Forms.ImageBox();
            this.cmbFormat = new System.Windows.Forms.ComboBox();
            this.lblName = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.numWidth = new System.Windows.Forms.NumericUpDown();
            this.numHeight = new System.Windows.Forms.NumericUpDown();
            this.lblFilename = new System.Windows.Forms.Label();
            this.chkZOrder = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tileSize = new System.Windows.Forms.NumericUpDown();
            this.btnSaveRaw = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbOrientation = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.numWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tileSize)).BeginInit();
            this.SuspendLayout();
            // 
            // imbPreview
            // 
            this.imbPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.imbPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.imbPreview.GridCellSize = 16;
            this.imbPreview.GridColor = System.Drawing.Color.Silver;
            this.imbPreview.Location = new System.Drawing.Point(12, 12);
            this.imbPreview.Name = "imbPreview";
            this.imbPreview.Size = new System.Drawing.Size(360, 276);
            this.imbPreview.TabIndex = 7;
            // 
            // cmbFormat
            // 
            this.cmbFormat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmbFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFormat.FormattingEnabled = true;
            this.cmbFormat.Location = new System.Drawing.Point(101, 377);
            this.cmbFormat.Margin = new System.Windows.Forms.Padding(4);
            this.cmbFormat.Name = "cmbFormat";
            this.cmbFormat.Size = new System.Drawing.Size(149, 21);
            this.cmbFormat.TabIndex = 9;
            this.cmbFormat.SelectedIndexChanged += new System.EventHandler(this.cmbPixelFormat_SelectedIndexChanged);
            // 
            // lblName
            // 
            this.lblName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.lblName.Location = new System.Drawing.Point(31, 321);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(63, 20);
            this.lblName.TabIndex = 11;
            this.lblName.Text = "Width:";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.label1.Location = new System.Drawing.Point(31, 349);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 20);
            this.label1.TabIndex = 12;
            this.label1.Text = "Height:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.label2.Location = new System.Drawing.Point(31, 377);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 21);
            this.label2.TabIndex = 13;
            this.label2.Text = "Format:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(271, 433);
            this.btnSave.Margin = new System.Windows.Forms.Padding(4);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(101, 23);
            this.btnSave.TabIndex = 14;
            this.btnSave.Text = "Save as PNG";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(271, 462);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(101, 23);
            this.btnCancel.TabIndex = 15;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // numWidth
            // 
            this.numWidth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.numWidth.Increment = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.numWidth.Location = new System.Drawing.Point(101, 321);
            this.numWidth.Margin = new System.Windows.Forms.Padding(4);
            this.numWidth.Maximum = new decimal(new int[] {
            2048,
            0,
            0,
            0});
            this.numWidth.Name = "numWidth";
            this.numWidth.Size = new System.Drawing.Size(149, 20);
            this.numWidth.TabIndex = 16;
            this.numWidth.Value = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.numWidth.ValueChanged += new System.EventHandler(this.numWidth_ValueChanged);
            // 
            // numHeight
            // 
            this.numHeight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.numHeight.Increment = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.numHeight.Location = new System.Drawing.Point(101, 349);
            this.numHeight.Margin = new System.Windows.Forms.Padding(4);
            this.numHeight.Maximum = new decimal(new int[] {
            2048,
            0,
            0,
            0});
            this.numHeight.Name = "numHeight";
            this.numHeight.Size = new System.Drawing.Size(149, 20);
            this.numHeight.TabIndex = 17;
            this.numHeight.Value = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.numHeight.ValueChanged += new System.EventHandler(this.numHeight_ValueChanged);
            // 
            // lblFilename
            // 
            this.lblFilename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFilename.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblFilename.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.lblFilename.Location = new System.Drawing.Point(12, 291);
            this.lblFilename.Name = "lblFilename";
            this.lblFilename.Size = new System.Drawing.Size(360, 22);
            this.lblFilename.TabIndex = 18;
            this.lblFilename.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // chkZOrder
            // 
            this.chkZOrder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkZOrder.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F);
            this.chkZOrder.Location = new System.Drawing.Point(101, 463);
            this.chkZOrder.Name = "chkZOrder";
            this.chkZOrder.Size = new System.Drawing.Size(73, 20);
            this.chkZOrder.TabIndex = 20;
            this.chkZOrder.Text = "Z-Order";
            this.chkZOrder.UseVisualStyleBackColor = true;
            this.chkZOrder.CheckedChanged += new System.EventHandler(this.chkZOrder_CheckedChanged);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.label3.Location = new System.Drawing.Point(31, 406);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 20);
            this.label3.TabIndex = 21;
            this.label3.Text = "TileSize:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tileSize
            // 
            this.tileSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tileSize.Increment = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.tileSize.Location = new System.Drawing.Point(101, 406);
            this.tileSize.Margin = new System.Windows.Forms.Padding(4);
            this.tileSize.Maximum = new decimal(new int[] {
            2048,
            0,
            0,
            0});
            this.tileSize.Name = "tileSize";
            this.tileSize.Size = new System.Drawing.Size(149, 20);
            this.tileSize.TabIndex = 22;
            this.tileSize.Value = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.tileSize.ValueChanged += new System.EventHandler(this.tileSize_ValueChanged);
            // 
            // btnSaveRaw
            // 
            this.btnSaveRaw.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveRaw.Location = new System.Drawing.Point(271, 404);
            this.btnSaveRaw.Margin = new System.Windows.Forms.Padding(4);
            this.btnSaveRaw.Name = "btnSaveRaw";
            this.btnSaveRaw.Size = new System.Drawing.Size(101, 23);
            this.btnSaveRaw.TabIndex = 23;
            this.btnSaveRaw.Text = "Save as RAW";
            this.btnSaveRaw.UseVisualStyleBackColor = true;
            this.btnSaveRaw.Click += new System.EventHandler(this.btnSaveRaw_Click);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.label4.Location = new System.Drawing.Point(11, 434);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(83, 21);
            this.label4.TabIndex = 25;
            this.label4.Text = "Orientation:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmbOrientation
            // 
            this.cmbOrientation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmbOrientation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbOrientation.FormattingEnabled = true;
            this.cmbOrientation.Location = new System.Drawing.Point(101, 434);
            this.cmbOrientation.Margin = new System.Windows.Forms.Padding(4);
            this.cmbOrientation.Name = "cmbOrientation";
            this.cmbOrientation.Size = new System.Drawing.Size(149, 21);
            this.cmbOrientation.TabIndex = 24;
            this.cmbOrientation.SelectedIndexChanged += new System.EventHandler(this.cmbOrientation_SelectedIndexChanged);
            // 
            // OpenRaw
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 497);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cmbOrientation);
            this.Controls.Add(this.btnSaveRaw);
            this.Controls.Add(this.tileSize);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.chkZOrder);
            this.Controls.Add(this.lblFilename);
            this.Controls.Add(this.numHeight);
            this.Controls.Add(this.numWidth);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.cmbFormat);
            this.Controls.Add(this.imbPreview);
            this.MinimumSize = new System.Drawing.Size(400, 460);
            this.Name = "OpenRaw";
            this.Text = "Open Raw Image";
            this.Load += new System.EventHandler(this.OpenRaw_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tileSize)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private Cyotek.Windows.Forms.ImageBox imbPreview;
        private System.Windows.Forms.ComboBox cmbFormat;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.NumericUpDown numWidth;
        private System.Windows.Forms.NumericUpDown numHeight;
        private System.Windows.Forms.Label lblFilename;
        private System.Windows.Forms.CheckBox chkZOrder;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown tileSize;
        private System.Windows.Forms.Button btnSaveRaw;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cmbOrientation;
    }
}