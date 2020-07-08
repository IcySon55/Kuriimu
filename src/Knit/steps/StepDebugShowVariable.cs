using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Knit.steps
{
    public class StepDebugShowVariable : Step, IIsDebugStep
    {
        // Methods
        public override async Task<StepResults> Perform(Dictionary<string, object> variableCache, IProgress<ProgressReport> progress)
        {
            if (variableCache.ContainsKey(Variable))
            {
                MessageBox.Show($"{Variable}: {variableCache[Variable]}", "Debug - Show Variable", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                progress.Report(new ProgressReport { Message = $"Debug: \"{Variable}\" was not found in the variable cache." });
            }

            return new StepResults { Status = StepStatus.Success };
        }
    }
}
