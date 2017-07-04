using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using text_kup.Properties;
using Kuriimu.Kontract;

namespace text_kup
{
    public partial class FileProperties : Form
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

        public FileProperties(KUP kup, Icon icon)
        {
            InitializeComponent();

            Icon = icon;
            _kup = kup;
        }

        private void FileProperties_Load(object sender, EventArgs e)
        {
            Text = Settings.Default.PluginName + " - Properties";

            Tools.LoadSupportedEncodings(cmbEncoding, _kup.Encoding);
            txtMemoryOffset.Text = _kup.RamOffset;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (_kup.Encoding != (Encoding)cmbEncoding.SelectedValue || _kup.RamOffset != txtMemoryOffset.Text.Trim())
                _hasChanges = true;

            _kup.Encoding = (Encoding)cmbEncoding.SelectedValue;
            _kup.RamOffset = txtMemoryOffset.Text;

            DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}