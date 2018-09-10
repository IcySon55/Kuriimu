using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Knit.steps
{
    public class StepSelectDirectory : Step
    {
        // Properties
        [XmlAttribute("selectDirectoryDescription")]
        public string SelectDirectoryDescription { get; set; } = string.Empty;

        // Methods
        public override async Task<StepResults> Perform(Dictionary<string, object> variableCache, IProgress<ProgressReport> progress)
        {
            var progressReport = new ProgressReport();
            var stepResults = new StepResults();

            if (Variable == string.Empty)
            {
                stepResults.Status = StepStatus.Error;
                stepResults.Message = $"{nameof(StepSelectDirectory)} requires a variable but none was provided.";
            }

            if (stepResults.Status == StepStatus.Success)
                try
                {
                    var ofd = new FolderBrowserDialog
                    {
                        Description = SelectDirectoryDescription
                    };
                    
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        variableCache[Variable] = ofd.SelectedPath + "\\";
                        progressReport.Percentage = 100;
                        stepResults.Message = $"Directory selected: \"{ofd.SelectedPath}\".";
                    }
                    else
                    {
                        stepResults.Status = StepStatus.Cancel;
                        stepResults.Message = "Directory selection cancelled.";
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
