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
        public override async Task<StepResults> Perform(Dictionary<string, object> variableCache, IProgress<ProgressReport> progress)
        {
            if (Weight > 0)
                for (var i = 1; i <= Weight; i++)
                {
                    progress.Report(new ProgressReport
                    {
                        Percentage = (double)i / Weight * 100
                    });
                    await Task.Delay(Delay / Weight);
                }
            else
                await Task.Delay(Delay);

            return new StepResults { Status = StepStatus.Success };
        }
    }
}
