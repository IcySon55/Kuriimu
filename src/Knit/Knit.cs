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
            _patching = true;
            UpdateForm();

            // Value Cache
            var valueCache = new Dictionary<string, object>();

            // Directories
            var patchDir = "patch";
            var metaDir = Path.Combine(patchDir, "meta");

            // Steps
            var patch = Patch.Load(Path.Combine(metaDir, "patch.xml"));
            pgbPatch.Maximum = patch.Steps.Count * 10;

            foreach (var step in patch.Steps)
            {
                step.ReportProgress += (o, args) =>
                {
                    pgbPatch.Value += (int)(args.Completion / 10);
                };

                step.StepComplete += (o, args) =>
                {
                    lblStatus.Text = args.Message;
                };

                if (!step.Perform(valueCache))
                    break;
            }

            //lblStatus.Text = "Patch applied successfully";
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
