using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Kuriimu.Kontract;

namespace game_unchained_blades_exxiv
{
    public partial class Settings : Form
    {
        #region Properties

        public bool HasChanges { get; set; } = false;

        #endregion

        public Settings(Icon icon)
        {
            InitializeComponent();

            Icon = icon;
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            cmbScene.DisplayMember = "Text";
            cmbScene.ValueMember = "Value";
            cmbScene.DataSource = Enum.GetNames(typeof(BubbleType)).Select(o => new ListItem(o, o)).ToList();
            cmbScene.SelectedValue = Properties.Settings.Default.BubbleType;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.BubbleType != cmbScene.SelectedValue.ToString())
                HasChanges = true;

            if (HasChanges)
            {
                Properties.Settings.Default.BubbleType = cmbScene.SelectedValue.ToString();
                Properties.Settings.Default.Save();
            }

            DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}