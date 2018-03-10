using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knit.steps
{
    public class StepTest : Step
    {
        // Properties

        // Methods
        public override Task<StepCompleteEventArgs> Perform(Dictionary<string, object> valueCache)
        {
            for (var i = 1; i <= Weight; i++)
            {
                OnReportProgress(new ReportProgressEventArgs(i / Weight * 100));
                Task.Delay(100);
            }

            OnStepComplete(new StepCompleteEventArgs(StepCompletionStatus.Success, $"Test step looped {Weight} time(s)."));

            return Task.FromResult(new StepCompleteEventArgs(StepCompletionStatus.Success, "Done!"));
        }
    }
}
