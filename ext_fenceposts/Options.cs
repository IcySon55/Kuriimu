using System;
using System.Text;
using System.Windows.Forms;
using ext_fenceposts.Properties;
using Kuriimu.Contract;

namespace ext_fenceposts
{
    public partial class frmOptions : Form
    {
        private KUP _kup = null;
        private bool _hasChanges = false;

        #region Properties

        public KUP Kup
        {
            get { return _kup; }
            set { _kup = value; }
        }

        public bool HasChanges
        {
            get { return _hasChanges; }
            private set { _hasChanges = value; }
        }

        #endregion

        public frmOptions(KUP kup)
        {
            InitializeComponent();

            _kup = kup;
        }

        private void frmOptions_Load(object sender, EventArgs e)
        {
            Text = Settings.Default.PluginName + " - Options";
            Icon = Resources.fenceposts;

            Tools.LoadSupportedEncodings(cmbEncoding, _kup.Encoding);
            txtMemoryOffset.Text = _kup.RamOffset;
            chkOptimizeStrings.Checked = _kup.OptimizeStrings;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (_kup.Encoding != (Encoding)cmbEncoding.SelectedValue || _kup.RamOffset != txtMemoryOffset.Text.Trim() || _kup.OptimizeStrings != chkOptimizeStrings.Checked)
                _hasChanges = true;

            _kup.Encoding = (Encoding)cmbEncoding.SelectedValue;
            _kup.RamOffset = txtMemoryOffset.Text;
            _kup.OptimizeStrings = chkOptimizeStrings.Checked;

            DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}