using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            Text = $"{meta.Title} {(meta.Version != "" ? $"v{meta.Version}" : "")}";
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

        private void btnPatch_Click(object sender, EventArgs e)
        {
            // Startup
            var stop = false;
            _patching = true;
            UpdateForm();

            // Value Cache
            var valueCache = new Dictionary<string, object>();

            // Directories
            var patchDir = "patch";
            var metaDir = Path.Combine(patchDir, "meta");

            // Steps
            var patch = Patch.Load(Path.Combine(metaDir, "patch.xml"));
            var progress = 0;
            pgbPatch.Maximum = patch.Steps.Sum(s => s.Weight);

            foreach (var step in patch.Steps)
            {
                step.ReportProgress += (o, args) =>
                {
                    pgbPatch.Value = progress + (int)(args.Completion / 100 * step.Weight);
                };

                step.StepComplete += (o, args) =>
                {
                    lblStatus.Text = args.Message;
                    switch (args.Status)
                    {
                        case StepCompletionStatus.Failure:
                        case StepCompletionStatus.Cancel:
                        case StepCompletionStatus.Error:
                            stop = true;
                            break;
                    }
                    progress += step.Weight;
                };

                if (!step.IsAsync)
                    RunStep(step, valueCache);
                else
                    RunStepAsync(step, valueCache);

                if (stop)
                {
                    //lblStatus.Text = "Execution stopped!";
                    break;
                }
            }

            if (!stop)
                lblStatus.Text = "Patch applied successfully";
            _patching = false;
            UpdateForm();
        }

        private void RunStep(Step step, Dictionary<string, object> valueCache)
        {
            step.Perform(valueCache);
        }

        private async void RunStepAsync(Step step, Dictionary<string, object> valueCache)
        {
            await step.Perform(valueCache);
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
