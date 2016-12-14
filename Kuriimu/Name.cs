using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Kuriimu.Properties;
using KuriimuContract;

namespace Kuriimu
{
	public partial class frmName : Form
	{
		private IEntry _entry = null;
		private bool _namesMustBeUnique = false;
		private List<string> _nameList = null;
		private bool _isNew = false;
		private bool _nameChanged = false;

		public event EventHandler<NameFormEventArgs> NameSubmitted;

		#region Properties

		public IEntry Entry
		{
			set { _entry = value; }
		}

		public bool NamesMustBeUnique
		{
			set { _namesMustBeUnique = value; }
		}

		public List<string> NameList
		{
			set { _nameList = value; }
		}

		public bool IsNew
		{
			set { _isNew = value; }
		}

		public bool NameChanged
		{
			get { return _nameChanged; }
		}

		#endregion

		public frmName(IEntry entry, bool namesMustBeUnique = false, List<string> nameList = null, bool isNew = false)
		{
			InitializeComponent();

			_entry = entry;
			_namesMustBeUnique = namesMustBeUnique;
			_nameList = nameList;
			_isNew = isNew;
		}

		private void Name_Load(object sender, EventArgs e)
		{
			Text = Settings.Default.ApplicationName;
			Icon = Resources.kuriimu;

			txtName.Text = _entry.Name;
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			string oldName = _entry.Name.Trim();
			string newName = txtName.Text.Trim();
			_nameChanged = oldName != newName;

			if (_namesMustBeUnique)
			{
				if (!_nameList.Contains(newName) || (oldName == newName && !_isNew))
				{
					_entry.Name = newName;
					DialogResult = DialogResult.OK;
					OnNameSubmitted(new NameFormEventArgs(oldName, newName));
				}
				else
					MessageBox.Show("Entry names must be unique. " + newName + " already exists.", "Must Be Unique", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
			else
			{
				_entry.Name = newName;
				DialogResult = DialogResult.OK;
				OnNameSubmitted(new NameFormEventArgs(oldName, newName));
			}
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		protected virtual void OnNameSubmitted(NameFormEventArgs e)
		{
			EventHandler<NameFormEventArgs> handler = NameSubmitted;
			if (handler != null)
				handler(_entry, e);
		}
	}

	public class NameFormEventArgs : EventArgs
	{
		public string OldName { get; set; }
		public string NewName { get; set; }

		public NameFormEventArgs(string oldName, string newName)
		{
			OldName = oldName;
			NewName = newName;
		}
	}
}