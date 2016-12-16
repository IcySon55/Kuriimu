﻿using System;
using System.Windows.Forms;

namespace file_kup
{
	public partial class frmProperties : Form
	{
		public Entry Entry { get; set; }

		public frmProperties()
		{
			InitializeComponent();
		}

		private void EntryProperties_Load(object sender, EventArgs e)
		{
			if (Entry != null)
			{
				txtOffset.Text = Entry.Offset;
				chkRelocatable.Checked = Entry.Relocatable;
				txtMaxLength.Text = Entry.MaxLength.ToString();
				lblPointerCount.Text = "Pointers: " + Entry.Pointers.Count;
			}
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			Entry.Offset = txtOffset.Text;
			Entry.Relocatable = chkRelocatable.Checked;
			int maxLength = 0;
			int.TryParse(txtMaxLength.Text, out maxLength);
			Entry.MaxLength = maxLength;
			DialogResult = DialogResult.OK;
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}
	}
}