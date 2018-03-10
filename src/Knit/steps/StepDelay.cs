using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Knit.steps
{
    public class StepDelay : Step
    {
        // Properties
        [XmlAttribute("delay")]
        public int Delay { get; set; } = 1000;

        // Methods
        public override async Task<StepResults> Perform(Dictionary<string, object> valueCache, IProgress<ProgressReport> progress)
        {
            if (Weight > 0)
                for (var i = 1; i <= Weight; i++)
                {
                    progress.Report(new ProgressReport
                    {
                        Percentage = (float)i / Weight * 100
                    });
                    await Task.Delay(Delay / Weight);
                }

            return new StepResults { Status = StepStatus.Success };
        }
    }
}
