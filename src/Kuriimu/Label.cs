using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Kuriimu.Properties;

namespace Kuriimu
{
    public partial class LabelForm : Form
    {
        private string _name;
        private string _color;
        private IEnumerable<string> _nameList;
        private bool _isNew;

        #region Properties

        public string Entry
        {
            set => _name = value;
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
        public string NewColor { get; private set; } = string.Empty;
        public bool NameChanged { get; private set; }
        public bool ColorChanged { get; private set; }

        #endregion

        public LabelForm(string name, string color, IEnumerable<string> nameList = null, bool isNew = false)
        {
            InitializeComponent();

            _name = name;
            _color = color;
            _nameList = nameList;
            _isNew = isNew;
        }

        private void Name_Load(object sender, EventArgs e)
        {
            Text = _isNew ? "New Label" : "Edit Label";
            Icon = Resources.kuriimu;

            txtName.Text = _name;
            btnColor.BackColor = ColorTranslator.FromHtml(_color);
        }

        private void btnColor_Click(object sender, EventArgs e)
        {
            clrDialog.Color = ColorTranslator.FromHtml(_color);
            if (clrDialog.ShowDialog() != DialogResult.OK) return;

            btnColor.BackColor = clrDialog.Color;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            var oldName = _name.Trim();
            var newName = txtName.Text.Trim();
            NameChanged = oldName != newName;

            var oldColor = _color;
            var newColor = ColorTranslator.ToHtml(btnColor.BackColor);
            ColorChanged = oldColor != newColor;

            if (_nameList != null)
            {
                if (!_nameList.Contains(newName) || (oldName == newName && !_isNew))
                {
                    NewName = newName;
                    NewColor = newColor;
                    DialogResult = DialogResult.OK;
                }
                else
                    MessageBox.Show("Label names must be unique. " + newName + " already exists.", "Must Be Unique", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
                MessageBox.Show("Label names must be unique but a name list was not provided.", "Name List Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}