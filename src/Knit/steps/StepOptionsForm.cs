using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

                var gpbOptions = new GroupBox
                {
                    Text = optionGroup.Name,
                    Location = new Point(0, nextGroupY),
                    Width = pnlOptions.Width,
                    Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
                };
                pnlOptions.Controls.Add(gpbOptions);

                var innerControlWidth = gpbOptions.Width - margin * 2;

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
                            AutoSize = true,
                            Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
                        };
                        gpbOptions.Controls.Add(lblDescription);
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
                                Size = new Size(innerControlWidth, 16),
                                TextAlign = ContentAlignment.MiddleLeft,
                                Tag = option.Variable,
                                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
                            };

                            // Default
                            if (bool.TryParse(option.Default, out var chkDef))
                                chkOption.Checked = chkDef;

                            gpbOptions.Controls.Add(chkOption);
                            nextOptionY += chkOption.Height + padding;
                            break;
                        case OptionsMode.RadioButton:
                            if (option.Values.Count == 0)
                            {
                                var rbnOption = new RadioButton
                                {
                                    Text = option.Name,
                                    Location = new Point(margin, nextOptionY),
                                    Size = new Size(innerControlWidth, 16),
                                    TextAlign = ContentAlignment.MiddleLeft,
                                    Tag = option.Variable,
                                    Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
                                };

                                // Default
                                if (bool.TryParse(option.Default, out var rbnDef))
                                    rbnOption.Checked = rbnDef;

                                gpbOptions.Controls.Add(rbnOption);
                                nextOptionY += rbnOption.Height + padding;
                            }
                            else
                            {
                                foreach (var value in option.Values)
                                {
                                    var rbnOption = new RadioButton
                                    {
                                        Text = value.Text,
                                        Location = new Point(margin, nextOptionY),
                                        Size = new Size(innerControlWidth, 16),
                                        TextAlign = ContentAlignment.MiddleLeft,
                                        Tag = value.Variable,
                                        Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
                                    };

                                    // Default
                                    if (option.Default == value.Variable)
                                        rbnOption.Checked = true;

                                    gpbOptions.Controls.Add(rbnOption);
                                    nextOptionY += rbnOption.Height + padding;
                                }
                            }
                            break;
                        case OptionsMode.DropDown:
                            if (option.Values.Count == 0) continue;

                            var ddlOption = new ComboBox
                            {
                                Location = new Point(margin, nextOptionY),
                                Size = new Size(innerControlWidth, 21),
                                DropDownStyle = ComboBoxStyle.DropDownList,
                                ValueMember = "Variable",
                                DisplayMember = "Text"
                            };
                            ddlOption.Items.AddRange(option.Values.ToArray());

                            // Default
                            ddlOption.SelectedIndex = Math.Max(ddlOption.Items.IndexOf(option.Values.FirstOrDefault(v => v.Variable == option.Default)), 0);

                            gpbOptions.Controls.Add(ddlOption);
                            nextOptionY += ddlOption.Height + padding;

                            break;
                    }

                    width = System.Math.Max(innerControlWidth, width);

                    _fieldCount++;
                }

                gpbOptions.Height = nextOptionY + padding;
                nextGroupY += gpbOptions.Height + padding;
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
            foreach (var gpb in pnlOptions.Controls)
            {
                if (!(gpb is GroupBox groupBox)) continue;

                foreach (Control opt in groupBox.Controls)
                {
                    switch (opt)
                    {
                        case Label lbl:
                            continue;
                        case CheckBox chk:
                            _variableCache[chk.Tag.ToString()] = chk.Checked;
                            break;
                        case RadioButton rbn:
                            _variableCache[rbn.Tag.ToString()] = rbn.Checked;
                            break;
                        case ComboBox ddl:
                            foreach (OptionValue item in ddl.Items)
                            {
                                var selected = ddl.SelectedItem == item;
                                _variableCache[item.Variable] = selected;
                            }
                            break;
                    }
                }
            }

            DialogResult = DialogResult.OK;
        }
    }
}
