using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WMPLib;

namespace Knit
{
    public partial class frmMain : Form
    {
        private bool Patching { get; set; }
        private bool Cancelled { get; set; }
        private string BaseDiriectory { get; set; }
        private string MetaDirectory { get; set; }
        private string ExitButtonText { get; set; }

        private bool MusicOn { get; set; }
        private string MusicPath { get; set; }
        private Image MusicOnIcon { get; set; }
        private Image MusicOffIcon { get; set; }
        private System.Media.SoundPlayer player = new System.Media.SoundPlayer();

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            // Initialize
            BaseDiriectory = "patch";
            MetaDirectory = Path.Combine(BaseDiriectory, "meta");

            // TODO: Create hash verification step

            LoadMeta();
            UpdateForm();
            UpdateMusic();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            player.Stop();
        }

        private void LoadMeta()
        {
            // Meta
            var meta = Meta.Load(Path.Combine(MetaDirectory, "meta.xml"));
            var background = new Bitmap(Path.Combine(MetaDirectory, meta.Background));
            var icon = new Icon(Path.Combine(MetaDirectory, meta.Icon));

            // Apply meta info
            Text = $"{meta.Title}{(meta.Version != "" ? $" {meta.Version}" : "")}";
            Icon = icon;
            ClientSize = background.Size;

            BackgroundImage = background;

            // Progress Bar
            prgProgress.Location = meta.Layout.ProgressBar.Location;
            prgProgress.Size = meta.Layout.ProgressBar.Size;

            // Patch Button
            btnPatch.Text = meta.Layout.PatchButton.Text;
            btnPatch.Location = meta.Layout.PatchButton.Location;
            btnPatch.Size = meta.Layout.PatchButton.Size;

            // Exit Button
            ExitButtonText = meta.Layout.ExitButton.Text;
            btnExit.Text = meta.Layout.ExitButton.Text;
            btnExit.Location = meta.Layout.ExitButton.Location;
            btnExit.Size = meta.Layout.ExitButton.Size;

            // Music
            MusicOn = true;
            MusicPath = Path.Combine(MetaDirectory, meta.Music);
            var onPath = Path.Combine(MetaDirectory, meta.Layout.MusicButton.On);
            var offPath = Path.Combine(MetaDirectory, meta.Layout.MusicButton.Off);
            if (File.Exists(onPath))
                MusicOnIcon = Image.FromFile(onPath);
            if (File.Exists(offPath))
                MusicOffIcon = Image.FromFile(offPath);
            btnMusic.Text = meta.Layout.MusicButton.Text;
            btnMusic.Location = meta.Layout.MusicButton.Location;
            btnMusic.Size = meta.Layout.MusicButton.Size;
            btnMusic.Visible = !string.IsNullOrWhiteSpace(MusicPath);

            // Status Bar
            txtStatus.BorderStyle = meta.Layout.StatusTextBox.Border;
            txtStatus.Location = meta.Layout.StatusTextBox.Location;
            txtStatus.Size = meta.Layout.StatusTextBox.Size;

            meta.Save(Path.Combine(MetaDirectory, "meta.xml"));
        }

        private void UpdateForm()
        {
            btnPatch.Enabled = !Patching;
            btnExit.Text = !Patching ? ExitButtonText : "Cancel";
            btnExit.Enabled = true;
        }

        private void UpdateMusic()
        {
            btnMusic.Image = MusicOn ? MusicOnIcon : MusicOffIcon;

            if (!File.Exists(MusicPath)) return;
            player.SoundLocation = MusicPath;

            try
            {
                if (MusicOn)
                    player.PlayLooping();
                else if (!MusicOn)
                    player.Stop();
            }
            catch (Exception) {}
        }

        private async void btnPatch_Click(object sender, EventArgs e)
        {
            // Startup
            Patching = true;
            prgProgress.Value = 0;
            txtStatus.Text = string.Empty;
            UpdateForm();

            // Variables
            var error = false;
            var percentage = 0;
            var variableCache = new Dictionary<string, object>();

            // Steps
            var patch = Patch.Load(Path.Combine(MetaDirectory, "patch.xml"));
            prgProgress.Maximum = patch.Steps.Sum(s => s.Weight);

            foreach (var step in patch.Steps)
            {
                step.WorkingDirectory = Path.Combine(Environment.CurrentDirectory, BaseDiriectory);

                var progress = new Progress<ProgressReport>(p =>
                {
                    prgProgress.Value = percentage + (int)(p.Percentage / 100 * step.Weight);
                    if (p.HasMessage)
                        txtStatus.AppendText(p.Message.Trim() + "\r\n");
                });

                try
                {
                    var results = await step.Perform(variableCache, progress);

                    switch (results.Status)
                    {
                        case StepStatus.Failure:
                        case StepStatus.Cancel:
                        case StepStatus.Error:
                            error = true;
                            break;
                        case StepStatus.Success:
                        case StepStatus.Skip:
                            percentage += step.Weight;
                            break;
                    }

                    if (results.HasMessage)
                        txtStatus.AppendText(results.Message.Trim() + "\r\n");
                }
                catch (Exception ex)
                {
                    txtStatus.AppendText("Exception: " + ex.Message.Trim() + "\r\n");
                    error = true;
                }

                if (error)
                    break;
                if (Cancelled)
                    break;
            }

            if (!error && !Cancelled)
                txtStatus.AppendText("Patch applied successfully!\r\n");
            if (Cancelled)
                txtStatus.AppendText("Cancelled...\r\n");
            Patching = false;
            Cancelled = false;
            UpdateForm();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            if (!Patching)
                Close();
            else
            {
                Patching = false;
                Cancelled = true;
                btnExit.Text = "Cancelling...";
                btnExit.Enabled = false;
            }
        }

        private void btnMusic_Click(object sender, EventArgs e)
        {
            MusicOn = !MusicOn;
            UpdateMusic();
        }
    }
}
