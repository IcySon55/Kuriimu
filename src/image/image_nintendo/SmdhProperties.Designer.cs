namespace image_nintendo.SMDH
{
    partial class SmdhProperties
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
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.pptExtendedProperties = new System.Windows.Forms.PropertyGrid();
            this.pnlExtendedProperties = new System.Windows.Forms.Panel();
            this.splExtendedProperties = new System.Windows.Forms.SplitContainer();
            this.pnlExtendedProperties.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splExtendedProperties)).BeginInit();
            this.splExtendedProperties.Panel1.SuspendLayout();
            this.splExtendedProperties.Panel2.SuspendLayout();
            this.splExtendedProperties.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(318, 0);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(399, 0);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // pptExtendedProperties
            // 
            this.pptExtendedProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pptExtendedProperties.LineColor = System.Drawing.SystemColors.ControlDark;
            this.pptExtendedProperties.Location = new System.Drawing.Point(0, 0);
            this.pptExtendedProperties.Name = "pptExtendedProperties";
            this.pptExtendedProperties.Size = new System.Drawing.Size(474, 382);
            this.pptExtendedProperties.TabIndex = 5;
            this.pptExtendedProperties.ToolbarVisible = false;
            // 
            // pnlExtendedProperties
            // 
            this.pnlExtendedProperties.Controls.Add(this.splExtendedProperties);
            this.pnlExtendedProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlExtendedProperties.Location = new System.Drawing.Point(0, 0);
            this.pnlExtendedProperties.Name = "pnlExtendedProperties";
            this.pnlExtendedProperties.Padding = new System.Windows.Forms.Padding(6);
            this.pnlExtendedProperties.Size = new System.Drawing.Size(486, 425);
            this.pnlExtendedProperties.TabIndex = 6;
            // 
            // splExtendedProperties
            // 
            this.splExtendedProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splExtendedProperties.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splExtendedProperties.IsSplitterFixed = true;
            this.splExtendedProperties.Location = new System.Drawing.Point(6, 6);
            this.splExtendedProperties.Name = "splExtendedProperties";
            this.splExtendedProperties.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splExtendedProperties.Panel1
            // 
            this.splExtendedProperties.Panel1.Controls.Add(this.pptExtendedProperties);
            this.splExtendedProperties.Panel1.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
            // 
            // splExtendedProperties.Panel2
            // 
            this.splExtendedProperties.Panel2.Controls.Add(this.btnCancel);
            this.splExtendedProperties.Panel2.Controls.Add(this.btnOK);
            this.splExtendedProperties.Panel2MinSize = 23;
            this.splExtendedProperties.Size = new System.Drawing.Size(474, 413);
            this.splExtendedProperties.SplitterDistance = 384;
            this.splExtendedProperties.SplitterWidth = 6;
            this.splExtendedProperties.TabIndex = 3;
            // 
            // SmdhProperties
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(486, 425);
            this.Controls.Add(this.pnlExtendedProperties);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SmdhProperties";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SMDH Extended Properties";
            this.Load += new System.EventHandler(this.FileProperties_Load);
            this.pnlExtendedProperties.ResumeLayout(false);
            this.splExtendedProperties.Panel1.ResumeLayout(false);
            this.splExtendedProperties.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splExtendedProperties)).EndInit();
            this.splExtendedProperties.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.PropertyGrid pptExtendedProperties;
        private System.Windows.Forms.Panel pnlExtendedProperties;
        private System.Windows.Forms.SplitContainer splExtendedProperties;
    }
}