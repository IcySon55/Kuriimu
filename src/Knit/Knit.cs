using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Knit
{
    public partial class frmMain : Form
    {
        private bool Patching { get; set; }
        private string PatchDir { get; set; }
        private string MetaDir { get; set; }

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            // Initialize
            PatchDir = "patch";
            MetaDir = Path.Combine(PatchDir, "meta");

            // Confirm Status
            //var currentDirectory = Path.GetDirectoryName(Application.ExecutablePath);
            //Directory.SetCurrentDirectory(currentDirectory);

            LoadMeta();
            UpdateForm();
        }

        private void LoadMeta()
        {
            // Meta
            var meta = Meta.Load(Path.Combine(MetaDir, "meta.xml"));
            var background = new Bitmap(Path.Combine(MetaDir, meta.Background));
            var icon = new Icon(Path.Combine(MetaDir, meta.Icon));

            // Apply meta info
            Text = $"{meta.Title}{(meta.Version != "" ? $" {meta.Version}" : "")}";
            Icon = icon;
            BackgroundImage = background;
            ClientSize = background.Size;
            btnPatch.Text = meta.Button;
        }

        private void UpdateForm()
        {
            btnPatch.Enabled = !Patching;
            btnExit.Text = !Patching ? "Exit" : "Cancel";
        }

        private async void btnPatch_Click(object sender, EventArgs e)
        {
            // Startup
            Patching = true;
            pgbPatch.Value = 0;
            UpdateForm();

            // Variables
            var stop = false;
            var progress = 0;
            var valueCache = new Dictionary<string, object>();

            // Steps
            var patch = Patch.Load(Path.Combine(MetaDir, "patch.xml"));
            pgbPatch.Maximum = patch.Steps.Sum(s => s.Weight);

            foreach (var step in patch.Steps)
            {
                var pgr = new Progress<ProgressReport>(p =>
                {
                    pgbPatch.Value = progress + (int)(p.Percentage / 100 * step.Weight);
                    if (p.HasMessage)
                        lblStatus.Text = p.Message;
                });

                var results = await step.Perform(valueCache, pgr);

                lblStatus.Text = results.Message;
                switch (results.Status)
                {
                    case StepStatus.Failure:
                    case StepStatus.Cancel:
                    case StepStatus.Error:
                        stop = true;
                        break;
                    case StepStatus.Success:
                    case StepStatus.Skip:
                        progress += step.Weight;
                        break;
                }

                if (stop)
                    break;
            }

            if (!stop)
                lblStatus.Text = "Patch applied successfully!";
            Patching = false;
            UpdateForm();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            if (!Patching)
                Close();
            else
            {
                Patching = false;
                UpdateForm();
            }
        }
    }
}
