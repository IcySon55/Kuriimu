using System.Collections.Generic;
using System.Drawing;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Knit.steps
{
    public sealed partial class StepOptionsForm : Form
    {
        private readonly Dictionary<string, object> _variableCache;
        private readonly int _fieldCount;

        public StepOptionsForm()
        {
            InitializeComponent();
        }

        public StepOptionsForm(IEnumerable<OptionGroup> optionGroups, Dictionary<string, object> variableCache)
        {
            InitializeComponent();
            _variableCache = variableCache;

            const int margin = 12;
            const int padding = 6;
            var nextGroupY = 0;
            var width = margin * 2;

            foreach (var optionGroup in optionGroups)
            {
                if (optionGroup.Options.Count == 0) continue;

                var glpOptions = new GroupBox
                {
                    Text = optionGroup.Name,
                    Location = new Point(0, nextGroupY),
                    Width = pnlOptions.Width,
                    Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
                };
                pnlOptions.Controls.Add(glpOptions);

                var nextOptionY = padding + margin;
                foreach (var option in optionGroup.Options)
                {
                    // Description
                    if (!string.IsNullOrWhiteSpace(option.Description))
                    {
                        var lblDescription = new Label
                        {
                            Text = option.Description,
                            Location = new Point(margin, nextOptionY),
                            TextAlign = ContentAlignment.MiddleLeft,
                            MaximumSize = new Size(300, 0),
                            AutoSize = true
                        };
                        glpOptions.Controls.Add(lblDescription);
                        nextOptionY += lblDescription.Height + padding;
                        width = System.Math.Max(margin * 3 + lblDescription.Width, width);
                    }

                    switch (option.Mode)
                    {
                        case OptionsMode.CheckBox:
                            var chkOption = new CheckBox
                            {
                                Text = option.Name,
                                Location = new Point(margin, nextOptionY),
                                Size = new Size(glpOptions.Width, 16),
                                TextAlign = ContentAlignment.MiddleLeft,
                                Tag = option.Variable
                            };

                            // Default
                            if (bool.TryParse(option.Default, out var def))
                                chkOption.Checked = def;

                            glpOptions.Controls.Add(chkOption);
                            nextOptionY += chkOption.Height + padding;
                            break;
                        case OptionsMode.RadioButton:
                            if (option.Values.Count == 0) continue;

                            foreach (var value in option.Values)
                            {
                                var rbnOption = new RadioButton
                                {
                                    Text = value.Text,
                                    Location = new Point(margin, nextOptionY),
                                    Size = new Size(glpOptions.Width, 16),
                                    TextAlign = ContentAlignment.MiddleLeft,
                                    Tag = value.Variable
                                };
                                glpOptions.Controls.Add(rbnOption);
                                nextOptionY += rbnOption.Height + padding;
                            }
                            break;
                        case OptionsMode.DropDown:
                            if (option.Values.Count == 0) continue;

                            var ddlOption = new ComboBox
                            {
                                Location = new Point(margin, nextOptionY),
                                Size = new Size(glpOptions.Width, 21),
                                DropDownStyle = ComboBoxStyle.DropDownList,
                                ValueMember = "Text",
                                DisplayMember = "Text"
                            };
                            ddlOption.Items.AddRange(option.Values.ToArray());
                            ddlOption.SelectedIndex = 0;

                            glpOptions.Controls.Add(ddlOption);
                            nextOptionY += ddlOption.Height + padding;
                            break;
                    }

                    _fieldCount++;
                }

                glpOptions.Height = nextOptionY + padding;
                nextGroupY += glpOptions.Height + padding;
            }

            SetClientSizeCore(width, nextGroupY + margin + splMain.Panel2.Height);
        }

        private void StepOptionsForm_Load(object sender, System.EventArgs e)
        {
            if (_fieldCount == 0)
                DialogResult = DialogResult.Abort;
        }

        private void btnOK_Click(object sender, System.EventArgs e)
        {
            foreach (var ctrl in pnlOptions.Controls)
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
