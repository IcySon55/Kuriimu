using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Knit.steps
{
    public class StepDebugShowVariables : Step, IIsDebugStep
    {
        // Methods
        public override async Task<StepResults> Perform(Dictionary<string, object> variableCache, IProgress<ProgressReport> progress)
        {
            var sb = new StringBuilder();

            foreach (var keu in variableCache.Keys)
                sb.AppendLine($"[{keu}]: " + variableCache[keu]);

            if (variableCache.Count == 0)
                sb.AppendLine("There are currently no variables stored.");

            MessageBox.Show(sb.ToString(), "Debug - Show Variables", MessageBoxButtons.OK, MessageBoxIcon.Information);

            return new StepResults { Status = StepStatus.Success };
        }
    }
}
