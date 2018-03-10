using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knit.steps
{
    public class StepTest : Step
    {
        // Properties

        // Methods
        public override async Task<StepResults> Perform(Dictionary<string, object> valueCache)
        {
            for (var i = 1; i <= Weight; i++)
            {
                OnReportProgress(new ReportProgressEventArgs((int)((float)i / Weight * 100)));
                await Task.Delay(50);
            }

            return new StepResults(StepStatus.Success, $"Test step looped {Weight} time(s).");
        }
    }
}
