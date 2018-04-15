namespace Knit
{
    partial class frmMain
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnMusic = new System.Windows.Forms.Button();
            this.txtStatus = new System.Windows.Forms.TextBox();
            this.prgProgress = new System.Windows.Forms.ProgressBar();
            this.btnExit = new System.Windows.Forms.Button();
            this.btnPatch = new System.Windows.Forms.Button();
            this.btnAbout = new System.Windows.Forms.Button();
            this.btnWebsite = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Transparent;
            this.panel1.Controls.Add(this.btnWebsite);
            this.panel1.Controls.Add(this.btnAbout);
            this.panel1.Controls.Add(this.btnMusic);
            this.panel1.Controls.Add(this.txtStatus);
            this.panel1.Controls.Add(this.prgProgress);
            this.panel1.Controls.Add(this.btnExit);
            this.panel1.Controls.Add(this.btnPatch);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(584, 312);
            this.panel1.TabIndex = 0;
            // 
            // btnMusic
            // 
            this.btnMusic.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnMusic.BackColor = System.Drawing.Color.Transparent;
            this.btnMusic.Location = new System.Drawing.Point(6, 230);
            this.btnMusic.Margin = new System.Windows.Forms.Padding(0);
            this.btnMusic.Name = "btnMusic";
            this.btnMusic.Size = new System.Drawing.Size(23, 23);
            this.btnMusic.TabIndex = 9;
            this.btnMusic.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnMusic.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnMusic.UseVisualStyleBackColor = false;
            this.btnMusic.Click += new System.EventHandler(this.btnMusic_Click);
            // 
            // txtStatus
            // 
            this.txtStatus.BackColor = System.Drawing.SystemColors.Window;
            this.txtStatus.HideSelection = false;
            this.txtStatus.Location = new System.Drawing.Point(6, 284);
            this.txtStatus.Multiline = true;
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.ReadOnly = true;
            this.txtStatus.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtStatus.Size = new System.Drawing.Size(572, 23);
            this.txtStatus.TabIndex = 8;
            // 
            // prgProgress
            // 
            this.prgProgress.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.prgProgress.Location = new System.Drawing.Point(6, 256);
            this.prgProgress.Margin = new System.Windows.Forms.Padding(0);
            this.prgProgress.Name = "prgProgress";
            this.prgProgress.Size = new System.Drawing.Size(572, 23);
            this.prgProgress.Step = 1;
            this.prgProgress.TabIndex = 6;
            // 
            // btnExit
            // 
            this.btnExit.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnExit.Location = new System.Drawing.Point(504, 230);
            this.btnExit.Margin = new System.Windows.Forms.Padding(0);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(75, 23);
            this.btnExit.TabIndex = 4;
            this.btnExit.Text = "E&xit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // btnPatch
            // 
            this.btnPatch.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnPatch.BackColor = System.Drawing.Color.Transparent;
            this.btnPatch.Location = new System.Drawing.Point(426, 230);
            this.btnPatch.Margin = new System.Windows.Forms.Padding(0);
            this.btnPatch.Name = "btnPatch";
            this.btnPatch.Size = new System.Drawing.Size(75, 23);
            this.btnPatch.TabIndex = 5;
            this.btnPatch.Text = "&Patch...";
            this.btnPatch.UseVisualStyleBackColor = false;
            this.btnPatch.Click += new System.EventHandler(this.btnPatch_Click);
            // 
            // btnAbout
            // 
            this.btnAbout.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnAbout.BackColor = System.Drawing.Color.Transparent;
            this.btnAbout.Location = new System.Drawing.Point(29, 230);
            this.btnAbout.Margin = new System.Windows.Forms.Padding(0);
            this.btnAbout.Name = "btnAbout";
            this.btnAbout.Size = new System.Drawing.Size(23, 23);
            this.btnAbout.TabIndex = 10;
            this.btnAbout.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnAbout.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnAbout.UseVisualStyleBackColor = false;
            this.btnAbout.Click += new System.EventHandler(this.btnAbout_Click);
            // 
            // btnWebsite
            // 
            this.btnWebsite.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnWebsite.BackColor = System.Drawing.Color.Transparent;
            this.btnWebsite.Location = new System.Drawing.Point(52, 230);
            this.btnWebsite.Margin = new System.Windows.Forms.Padding(0);
            this.btnWebsite.Name = "btnWebsite";
            this.btnWebsite.Size = new System.Drawing.Size(23, 23);
            this.btnWebsite.TabIndex = 11;
            this.btnWebsite.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnWebsite.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnWebsite.UseVisualStyleBackColor = false;
            this.btnWebsite.Click += new System.EventHandler(this.btnWebsite_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 312);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "frmMain";
            this.Text = "Knit";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ProgressBar prgProgress;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Button btnPatch;
        private System.Windows.Forms.TextBox txtStatus;
        private System.Windows.Forms.Button btnMusic;
        private System.Windows.Forms.Button btnAbout;
        private System.Windows.Forms.Button btnWebsite;
    }
}

