using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Kuriimu.Kontract;

namespace image_nintendo.SMDH
{
    public partial class SmdhProperties : Form
    {
        private SMDH _smdh = null;
        private bool _hasChanges = false;

        #region Properties

        public SMDH Smdh
        {
            get { return _smdh; }
            set { _smdh = value; }
        }

        public bool HasChanges
        {
            get { return _hasChanges; }
            private set { _hasChanges = value; }
        }

        #endregion

        public SmdhProperties(SMDH smdh, Icon icon)
        {
            InitializeComponent();

            Icon = icon;
            _smdh = smdh;
        }

        private void FileProperties_Load(object sender, EventArgs e)
        {
            Text = "SMDH Extended Properties";

            // Populate Form
            //txtMemoryOffset.Text = _smdh.RamOffset;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            // Check for changes
            //if (_smdh.Property != localValue)
            //    _hasChanges = true;

            // Set values
            //_smdh.Property = localValue;

            DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}