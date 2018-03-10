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
    public class StepTest : Step
    {
        // Properties
        public override bool IsAsync => true;

        // Methods
        public override async Task Perform(Dictionary<string, object> valueCache)
        {
            for (var i = 1; i <= Weight; i++)
            {
                OnReportProgress(new ReportProgressEventArgs(i / Weight * 100));
                await Task.Delay(10);
            }

            OnStepComplete(new StepCompleteEventArgs(StepCompletionStatus.Success, $"Test step looped {Weight} time(s)."));
        }
    }
}
