using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Knit.steps
{
    public class StepOptions : Step
    {
        // Properties
        [XmlAttribute("setOptionsTitle")]
        public string SetOptionsTitle { get; set; } = "Select options...";

        [XmlElement("option")]
        public List<Option> Options { get; set; } = new List<Option>();

        // Methods
        public override async Task<StepResults> Perform(Dictionary<string, object> variableCache, IProgress<ProgressReport> progress)
        {
            var progressReport = new ProgressReport();
            var stepResults = new StepResults();

            try
            {
                var meta = Meta.Load(Path.Combine(WorkingDirectory, "meta", "meta.xml"));
                var frmOptions = new StepOptionsForm(Options, variableCache) {Icon = new Icon(Path.Combine(WorkingDirectory, "meta", meta.Icon)), StartPosition = FormStartPosition.CenterParent};
                var dr = frmOptions.ShowDialog();

                if (dr == DialogResult.OK)
                {
                    stepResults.Status = StepStatus.Success;
                    stepResults.Message = "Options were selected!";
                }
                else if (dr == DialogResult.Abort)
                {
                    stepResults.Status = StepStatus.Skip;
                    stepResults.Message = "Options were provided but none could be rendered.";
                }
                else
                {
                    stepResults.Status = StepStatus.Cancel;
                    stepResults.Message = "Option selection cancelled.";
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

    public class Option
    {
        [XmlAttribute("name")]
        public string Name { get; set; } = string.Empty;

        [XmlAttribute("variable")]
        public string Variable { get; set; } = string.Empty;

        [XmlAttribute("default")]
        public string Default { get; set; } = string.Empty;

        [XmlAttribute("description")]
        public string Description { get; set; } = string.Empty;

        [XmlElement("option")]
        public List<Option> Options { get; set; } = new List<Option>();
    }
}
