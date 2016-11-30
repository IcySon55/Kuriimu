using System;
using System.Collections.Generic;
using System.Windows.Forms;
using dump_fenceposts.Properties;
using KuriimuContract;
using file_kup;

namespace dump_fenceposts
{
	public partial class frmBound : Form
	{
		private Bound _bound = null;

		public event EventHandler<EventArgs> BoundSubmitted;

		#region Properties

		public Bound Bound
		{
			get { return _bound;  }
			set { _bound = value; }
		}

		#endregion

		public frmBound(Bound bound)
		{
			InitializeComponent();

			_bound = bound;
		}

		private void frmBound_Load(object sender, EventArgs e)
		{
			this.Text = Settings.Default.PluginName;
			this.Icon = Resources.fenceposts;

			txtStart.Text = _bound.Start;
			txtEnd.Text = _bound.End;
			txtNotes.Text = _bound.Notes;
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			_bound.Start = txtStart.Text;
			_bound.End = txtEnd.Text;
			_bound.Notes = txtNotes.Text;

			OnBoundSubmitted(new EventArgs());
			DialogResult = DialogResult.OK;
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		protected virtual void OnBoundSubmitted(EventArgs e)
		{
			EventHandler<EventArgs> handler = BoundSubmitted;
			if (handler != null)
				handler(_bound, e);
		}
	}
}