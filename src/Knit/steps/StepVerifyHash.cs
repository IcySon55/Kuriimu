using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Knit.Hashing;

namespace Knit.steps
{
    public class StepVerifyFileHash : Step
    {
        // Properties
        [XmlAttribute("type")]
        public HashTypes Type { get; set; } = HashTypes.None;

        [XmlAttribute("hash")]
        public string Hash { get; set; } = string.Empty;

        [XmlAttribute("startMessage")]
        public string StartMessage { get; set; } = string.Empty;

        [XmlAttribute("validMessage")]
        public string ValidMessage { get; set; } = string.Empty;

        [XmlAttribute("invalidMessage")]
        public string InvalidMessage { get; set; } = string.Empty;

        // Methods
        public override async Task<StepResults> Perform(Dictionary<string, object> variableCache, IProgress<ProgressReport> progress)
        {
            var progressReport = new ProgressReport();
            var stepResults = new StepResults { Message = Common.ProcessVariableTokens(ValidMessage, variableCache), Status = StepStatus.Success };
            Hash = Hash.ToUpper();

            progress.Report(new ProgressReport { Message = StartMessage, Percentage = 0 });

            if (Variable == string.Empty || !variableCache.ContainsKey(Variable))
            {
                stepResults.Message = $"{Name}: A file was not provided.";
                stepResults.Status = StepStatus.Failure;
            }
            else if (!File.Exists(variableCache[Variable].ToString()))
            {
                stepResults.Message = $"{Name}: The file provided does not exist.";
                stepResults.Status = StepStatus.Failure;
            }
            else if (Hash == string.Empty)
            {
                stepResults.Message = $"{Name}: A hash was not provided.";
                stepResults.Status = StepStatus.Failure;
            }

            var result = string.Empty;
            if (stepResults.Status == StepStatus.Success)
                switch (Type)
                {
                    case HashTypes.None:
                        stepResults.Message = $"{Name}: A hash type was not selected.";
                        stepResults.Status = StepStatus.Failure;
                        break;
                    case HashTypes.SHA1:
                        result = await SHA1Hash.ComputeHashAsync(File.OpenRead(variableCache[Variable].ToString()), progress);
                        if (Hash != result)
                        {
                            stepResults.Message = $"Expected {Hash} but got {result} instead.\r\n" + Common.ProcessVariableTokens(InvalidMessage, variableCache);
                            stepResults.Status = StepStatus.Failure;
                        }
                        break;
                    case HashTypes.SHA256:
                        result = await SHA256Hash.ComputeHashAsync(File.OpenRead(variableCache[Variable].ToString()), progress);
                        if (Hash != result)
                        {
                            stepResults.Message = $"Expected {Hash} but got {result} instead.\r\n" + Common.ProcessVariableTokens(InvalidMessage, variableCache);
                            stepResults.Status = StepStatus.Failure;
                        }
                        break;
                }

            progress.Report(progressReport);
            return stepResults;
        }
    }
}
