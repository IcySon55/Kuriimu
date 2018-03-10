using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        public override Task Perform(Dictionary<string, object> valueCache)
        {
            var stop = false;

            if (StoreTo == string.Empty)
            {
                OnReportProgress(new ReportProgressEventArgs(0));
                OnStepComplete(new StepCompleteEventArgs(StepCompletionStatus.Error, "StepSelectFile requires a StoreTo variable but none was provided."));
                stop = true;
            }

            if (!stop)
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
                        OnReportProgress(new ReportProgressEventArgs(100));
                        OnStepComplete(new StepCompleteEventArgs(StepCompletionStatus.Success, $"User selected {ofd.FileName}."));
                    }
                    else
                    {
                        OnReportProgress(new ReportProgressEventArgs(0));
                        OnStepComplete(new StepCompleteEventArgs(StepCompletionStatus.Cancel, "User cancelled selecting a file."));
                    }
                }
                catch (Exception e)
                {
                    OnReportProgress(new ReportProgressEventArgs(0));
                    OnStepComplete(new StepCompleteEventArgs(StepCompletionStatus.Error, e.ToString()));
                }

            return new Task(() => { });
        }
    }
}
