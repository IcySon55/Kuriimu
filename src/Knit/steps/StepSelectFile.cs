using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Knit.steps
{
    public class StepSelectFile : Step
    {
        // Properties
        [XmlAttribute("openFileTitle")]
        public string OpenFileTitle { get; set; } = "Select a file...";

        [XmlAttribute("openFileFilter")]
        public string OpenFileFilter { get; set; } = "All files (*.*)|*.*";

        // Methods
        public override async Task<StepResults> Perform(Dictionary<string, object> valueCache)
        {
            var progress = 0;
            var status = StepStatus.Success;
            var message = "";

            if (StoreTo == string.Empty)
            {
                status = StepStatus.Error;
                message = "StepSelectFile requires a StoreTo variable but none was provided.";
            }

            if (status == StepStatus.Success)
                try
                {
                    var ofd = new OpenFileDialog
                    {
                        Title = OpenFileTitle,
                        Filter = OpenFileFilter
                    };

                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        valueCache[StoreTo] = ofd.FileName;
                        progress = 100;
                        message = $"User selected {ofd.FileName}.";
                    }
                    else
                    {
                        status = StepStatus.Cancel;
                        message = "User cancelled selecting a file.";
                    }
                }
                catch (Exception ex)
                {
                    status = StepStatus.Error;
                    message = ex.ToString();
                }

            OnReportProgress(new ReportProgressEventArgs(progress));
            return new StepResults(status, message);
        }
    }
}
