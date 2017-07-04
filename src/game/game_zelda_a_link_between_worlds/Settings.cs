using System;
using System.Drawing;
using System.Windows.Forms;
using game_zelda_a_link_between_worlds.Properties;

namespace game_zelda_a_link_between_worlds
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
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.PlayerName != txtPlayerName.Text)
                HasChanges = true;

            if (HasChanges)
            {
                Properties.Settings.Default.PlayerName = txtPlayerName.Text;
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