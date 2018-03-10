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
        public override async Task<StepResults> Perform(Dictionary<string, object> valueCache, IProgress<ProgressReport> progress)
        {
            var progressReport = new ProgressReport();
            var stepResults = new StepResults();

            if (StoreTo == string.Empty)
            {
                stepResults.Status = StepStatus.Error;
                stepResults.Message = "StepSelectFile requires a StoreTo variable but none was provided.";
            }

            if (stepResults.Status == StepStatus.Success)
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
                        progressReport.Percentage = 100;
                        stepResults.Message = $"User selected {ofd.FileName}.";
                    }
                    else
                    {
                        stepResults.Status = StepStatus.Cancel;
                        stepResults.Message = "User cancelled selecting a file.";
                    }
                }
                catch (Exception ex)
                {
                    stepResults.Status = StepStatus.Error;
                    stepResults.Message = ex.ToString();
                }

            progress.Report(progressReport);
            return stepResults;
        }
    }
}
