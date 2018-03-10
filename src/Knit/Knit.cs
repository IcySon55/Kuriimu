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
        private bool _patching = false;

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            // Confirm Status
            //var currentDirectory = Path.GetDirectoryName(Application.ExecutablePath);
            //Directory.SetCurrentDirectory(currentDirectory);

            // Directories
            var patchDir = "patch";
            var metaDir = Path.Combine(patchDir, "meta");

            // Meta
            var meta = Meta.Load(Path.Combine(metaDir, "meta.xml"));
            var background = new Bitmap(Path.Combine(metaDir, meta.Background));
            var icon = new Icon(Path.Combine(metaDir, meta.Icon));

            // Apply meta info
            Text = $"{meta.Title} {(meta.Version != "" ? $"{meta.Version}" : "")}";
            Icon = icon;
            BackgroundImage = background;
            ClientSize = background.Size;
            btnPatch.Text = meta.Button;
        }

        private void UpdateForm()
        {
            btnPatch.Enabled = !_patching;
            btnExit.Text = !_patching ? "Exit" : "Cancel";
        }

        private async void btnPatch_Click(object sender, EventArgs e)
        {
            // Startup
            _patching = true;
            pgbPatch.Value = 0;
            UpdateForm();

            // Variables
            var stop = false;
            var progress = 0;
            var valueCache = new Dictionary<string, object>();

            // Directories
            var patchDir = "patch";
            var metaDir = Path.Combine(patchDir, "meta");

            // Steps
            var patch = Patch.Load(Path.Combine(metaDir, "patch.xml"));
            pgbPatch.Maximum = patch.Steps.Sum(s => s.Weight);

            foreach (var step in patch.Steps)
            {
                step.ReportProgress += (o, args) =>
                {
                    pgbPatch.Value = progress + (int)(args.Completion / 100 * step.Weight);
                };

                var results = await step.Perform(valueCache);

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
            _patching = false;
            UpdateForm();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            if (!_patching)
                Close();
            else
            {
                _patching = false;
                UpdateForm();
            }
        }
    }
}
