namespace Kontract
{
    partial class RawSelector
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
            this.rawList = new System.Windows.Forms.ListBox();
            this.OKbtn = new System.Windows.Forms.Button();
            this.Cancelbtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // rawList
            // 
            this.rawList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rawList.FormattingEnabled = true;
            this.rawList.Location = new System.Drawing.Point(13, 13);
            this.rawList.MultiColumn = true;
            this.rawList.Name = "rawList";
            this.rawList.Size = new System.Drawing.Size(264, 199);
            this.rawList.Sorted = true;
            this.rawList.TabIndex = 0;
            // 
            // OKbtn
            // 
            this.OKbtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OKbtn.Location = new System.Drawing.Point(115, 218);
            this.OKbtn.Name = "OKbtn";
            this.OKbtn.Size = new System.Drawing.Size(78, 26);
            this.OKbtn.TabIndex = 1;
            this.OKbtn.Text = "OK";
            this.OKbtn.UseVisualStyleBackColor = true;
            // 
            // Cancelbtn
            // 
            this.Cancelbtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Cancelbtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancelbtn.Location = new System.Drawing.Point(199, 218);
            this.Cancelbtn.Name = "Cancelbtn";
            this.Cancelbtn.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Cancelbtn.Size = new System.Drawing.Size(78, 26);
            this.Cancelbtn.TabIndex = 2;
            this.Cancelbtn.Text = "Cancel";
            this.Cancelbtn.UseVisualStyleBackColor = true;
            // 
            // RawSelector
            // 
            this.AcceptButton = this.OKbtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancelbtn;
            this.ClientSize = new System.Drawing.Size(289, 256);
            this.Controls.Add(this.Cancelbtn);
            this.Controls.Add(this.OKbtn);
            this.Controls.Add(this.rawList);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RawSelector";
            this.Text = "RawSelector";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox rawList;
        private System.Windows.Forms.Button OKbtn;
        private System.Windows.Forms.Button Cancelbtn;
    }
}