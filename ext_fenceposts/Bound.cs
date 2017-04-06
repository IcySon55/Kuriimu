using System;
using System.Windows.Forms;
using ext_fenceposts.Properties;
using Kuriimu.Contract;

namespace ext_fenceposts
{
    public partial class frmBound : Form
    {
        private Bound _bound = null;
        private bool _hasChanges = false;

        #region Properties

        public Bound Bound
        {
            get { return _bound; }
            set { _bound = value; }
        }

        public bool HasChanges
        {
            get { return _hasChanges; }
            private set { _hasChanges = value; }
        }

        #endregion

        public frmBound(Bound bound)
        {
            InitializeComponent();

            _bound = bound;
        }

        private void frmBound_Load(object sender, EventArgs e)
        {
            Text = Settings.Default.PluginName + " - Bound Properties";
            Icon = Resources.fenceposts;

            txtStart.Text = _bound.Start;
            txtEnd.Text = _bound.End;
            txtNotes.Text = _bound.Notes;
            chkDumpable.Checked = _bound.Dumpable;
            chkInjectable.Checked = _bound.Injectable;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            _bound.Start = txtStart.Text;
            _bound.End = txtEnd.Text;
            _bound.Notes = txtNotes.Text;
            _bound.Dumpable = chkDumpable.Checked;
            _bound.Injectable = chkInjectable.Checked;

            DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}