using System;
using System.Drawing;
using System.Windows.Forms;
using Kuriimu.Contract;

namespace text_kup
{
    public partial class EntryProperties : Form
    {
        private Entry _entry = null;
        private bool _hasChanges = false;

        #region Properties

        public Entry Entry
        {
            get { return _entry; }
            set { _entry = value; }
        }

        public bool HasChanges
        {
            get { return _hasChanges; }
            private set { _hasChanges = value; }
        }

        #endregion

        public EntryProperties(Entry entry, Icon icon)
        {
            InitializeComponent();

            Icon = icon;
            _entry = entry;
        }

        private void EntryProperties_Load(object sender, EventArgs e)
        {
            txtOffset.Text = _entry.Offset;
            chkRelocatable.Checked = _entry.Relocatable;
            txtMaxLength.Text = _entry.MaxLength.ToString();
            lblPointerCount.Text = "Pointers: " + _entry.Pointers.Count;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (_entry.Offset != txtOffset.Text.Trim() || _entry.Relocatable != chkRelocatable.Checked || _entry.MaxLength.ToString() != txtMaxLength.Text.Trim())
                _hasChanges = true;

            _entry.Offset = txtOffset.Text;
            _entry.Relocatable = chkRelocatable.Checked;
            int maxLength = 0;
            int.TryParse(txtMaxLength.Text, out maxLength);
            _entry.MaxLength = maxLength;

            DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}