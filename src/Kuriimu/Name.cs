using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Kuriimu.Properties;

namespace Kuriimu
{
    public partial class Name : Form
    {
        private string _name = null;
        private bool _namesMustBeUnique = false;
        private IEnumerable<string> _nameList = null;
        private string _validNameRegex = ".*";
        private int _maxLength = 0;
        private bool _isNew = false;

        #region Properties

        public string Entry
        {
            set => _name = value;
        }

        public bool NamesMustBeUnique
        {
            set => _namesMustBeUnique = value;
        }

        public IEnumerable<string> NameList
        {
            set => _nameList = value;
        }

        public bool IsNew
        {
            set => _isNew = value;
        }

        public string NewName { get; private set; } = string.Empty;

        public bool NameChanged { get; private set; } = false;

        #endregion

        public Name(string name, bool namesMustBeUnique = false, IEnumerable<string> nameList = null, string validNameRegex = ".*", int maxLength = 0, bool isNew = false)
        {
            InitializeComponent();

            _name = name;
            _namesMustBeUnique = namesMustBeUnique;
            _nameList = nameList;
            _validNameRegex = validNameRegex;
            _maxLength = maxLength;
            _isNew = isNew;
        }

        private void Name_Load(object sender, EventArgs e)
        {
            Text = _isNew ? "Add Entry" : "Rename Entry";
            Icon = Resources.kuriimu;

            txtName.MaxLength = _maxLength == 0 ? int.MaxValue : _maxLength;
            txtName.Text = _name;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string oldName = _name.Trim();
            string newName = txtName.Text.Trim();
            NameChanged = oldName != newName;

            if (txtName.Text.Trim().Length <= _maxLength || _maxLength == 0)
                if (Regex.IsMatch(newName, _validNameRegex))
                    if (_namesMustBeUnique)
                    {
                        if (_nameList != null)
                        {
                            if (!_nameList.Contains(newName) || (oldName == newName && !_isNew))
                            {
                                NewName = newName;
                                DialogResult = DialogResult.OK;
                            }
                            else
                                MessageBox.Show("Entry names must be unique. " + newName + " already exists.", "Must Be Unique", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                        else
                            MessageBox.Show("Entry names must be unique but a name list was not provided by the file plugin.", "File Plugin Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                    else
                    {
                        NewName = newName;
                        DialogResult = DialogResult.OK;
                    }
                else
                    MessageBox.Show("The name entered contains invalid characters. Valid names must satisfy this regular expression: " + _validNameRegex, "Name is Invalid", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            else
                MessageBox.Show("The name entered is too long. Valid names can only be " + _maxLength + " character(s) long.", "Name Too Long", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}