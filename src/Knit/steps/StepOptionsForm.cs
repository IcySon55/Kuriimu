using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Knit.steps
{
    public sealed partial class StepOptionsForm : Form
    {
        private Dictionary<string, object> _variableCache;
        private int _fieldCount = 0;

        public StepOptionsForm()
        {
            InitializeComponent();
        }

        public StepOptionsForm(IEnumerable<Option> options, Dictionary<string, object> variableCache)
        {
            InitializeComponent();
            _variableCache = variableCache;

            const int margin = 10;
            const int padding = 6;
            var nextY = margin;
            var width = margin * 2;

            foreach (var option in options)
            {
                if (option.Variable.Trim() == string.Empty) continue;

                // Checkbox / Dropdown (Phase 2)
                var chkOption = new CheckBox { Text = option.Name, Location = new Point(margin, nextY), Size = new Size(Size.Width, 20), TextAlign = ContentAlignment.MiddleLeft, Tag = option.Variable};
                splMain.Panel1.Controls.Add(chkOption);
                nextY += chkOption.Height + padding;
                _fieldCount++;

                // Default
                if (bool.TryParse(option.Default, out var def))
                    chkOption.Checked = def;

                // Description
                if (option.Description.Trim() == string.Empty) continue;

                var lblDescription = new Label { AutoSize = true, Text = option.Description, Location = new Point(margin, nextY - padding), MaximumSize = new Size(300, 0) };
                splMain.Panel1.Controls.Add(lblDescription);
                nextY += lblDescription.Height + padding;
                width = margin * 2 + lblDescription.Width;
            }

            SetClientSizeCore(width, nextY + splMain.Panel2.Height);
        }

        private void StepOptionsForm_Load(object sender, System.EventArgs e)
        {
            if (_fieldCount == 0)
                DialogResult = DialogResult.Abort;
        }

        private void btnOK_Click(object sender, System.EventArgs e)
        {
            foreach (var ctrl in splMain.Panel1.Controls)
            {
                if (!(ctrl is CheckBox)) continue;
                var chk = ctrl as CheckBox;

                var v = chk.Tag.ToString();
                _variableCache[v] = chk.Checked;
            }

            DialogResult = DialogResult.OK;
        }
    }
}
