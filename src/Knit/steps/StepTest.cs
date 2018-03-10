using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knit.steps
{
    public class StepTest : Step
    {
        // Properties

        // Methods
        public override async Task<StepResults> Perform(Dictionary<string, object> valueCache, IProgress<ProgressReport> progress)
        {
            for (var i = 1; i <= Weight; i++)
            {
                progress.Report(new ProgressReport
                {
                    Percentage = (float)i / Weight * 100,
                    Message = $"Test step iteration {i:000}."
                });
                await Task.Delay(50);
            }

            return new StepResults
            {
                Status = StepStatus.Success,
                Message = $"Test step looped {Weight} time(s)."
            };
        }
    }
}
