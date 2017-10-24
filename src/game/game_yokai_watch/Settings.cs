using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Kontract;

namespace game_yokai_watch
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
            txtPlayerName.Text = Properties.Settings.Default.PlayerName;
            cmbScene.DisplayMember = "Text";
            cmbScene.ValueMember = "Value";
            cmbScene.DataSource = Enum.GetNames(typeof(Scenes)).Select(o => new ListItem(o, o)).ToList();
            cmbScene.SelectedValue = Properties.Settings.Default.Scene;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.PlayerName != txtPlayerName.Text)
                HasChanges = true;

            if (Properties.Settings.Default.Scene != cmbScene.SelectedValue.ToString())
                HasChanges = true;

            if (HasChanges)
            {
                Properties.Settings.Default.PlayerName = txtPlayerName.Text;
                Properties.Settings.Default.Scene = cmbScene.SelectedValue.ToString();
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