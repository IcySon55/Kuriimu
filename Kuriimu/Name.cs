using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Kuriimu.Properties;
using KuriimuContract;

namespace Kuriimu
{
	public partial class Name : Form
	{
		private IEntry _entry = null;
		private List<string> _nameList = null;
		private bool _mustBeUnique = false;
		private bool _isNew = false;

		public event EventHandler<NameFormEventArgs> NameSubmitted;

		#region Properties

		public IEntry Entry
		{
			get { return _entry;  }
			set { _entry = value; }
		}

		public bool IsNew
		{
			get { return _isNew; }
			set { _isNew = value; }
		}

		#endregion

		public Name(IEntry entry, List<string> nameList, bool mustBeUnique)
		{
			InitializeComponent();

			_entry = entry;
			_nameList = nameList;
			_mustBeUnique = mustBeUnique;
			_isNew = false;
		}

		private void Name_Load(object sender, EventArgs e)
		{
			this.Text = Settings.Default.ApplicationName;
			this.Icon = Resources.kuriimu;

			txtName.Text = _entry.Name;
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			string oldName = _entry.Name.Trim();
			string newName = txtName.Text.Trim();

			if (newName.Length > 0)
				if (_mustBeUnique)
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
			//else
			//    MessageBox.Show("The name field is required.", "Name Required", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		private void Name_FormClosing(object sender, FormClosingEventArgs e)
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