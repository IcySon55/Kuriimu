using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Knit.steps
{
    public class StepMessage : Step
    {
        // Properties
        [XmlAttribute("message")]
        public string Message { get; set; } = string.Empty;

        // Methods
        public override async Task<StepResults> Perform(Dictionary<string, object> variableCache, IProgress<ProgressReport> progress)
        {
            return new StepResults { Message = Common.ProcessVariableTokens(Message, variableCache), Status = StepStatus.Success };
        }
    }
}
