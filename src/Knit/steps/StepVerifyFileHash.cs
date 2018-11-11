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
        [XmlAttribute("path")]
        public string Path { get; set; } = string.Empty;

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
            var column = StartMessage.Trim().Length > 0 ? ": " : "";

            var progressReport = new ProgressReport();
            var stepResults = new StepResults { Message = column + Common.ProcessVariableTokens(ValidMessage, variableCache), Status = StepStatus.Success };

            progress.Report(new ProgressReport { Message = Common.ProcessVariableTokens(StartMessage, variableCache), Percentage = 0, NewLine = false });
            await Task.Delay(10);

            if ((Variable == string.Empty && Path == string.Empty) || (Variable != string.Empty && !variableCache.ContainsKey(Variable)))
            {
                stepResults.Message = $"{column}A file was not provided.";
                stepResults.Status = StepStatus.Failure;
            }
            else if (Hash == string.Empty)
            {
                stepResults.Message = $"{column}A hash was not provided.";
                stepResults.Status = StepStatus.Failure;
            }

            var file = string.Empty;
            if (stepResults.Status == StepStatus.Success)
            {
                if (variableCache.ContainsKey(Variable))
                    file = variableCache[Variable].ToString();
                else
                    file = Common.ProcessVariableTokens(Path, variableCache);

                if (!File.Exists(file))
                {
                    stepResults.Message = $"{column}The file provided does not exist.";
                    stepResults.Status = StepStatus.Failure;
                }
            }

            var result = string.Empty;
            if (stepResults.Status == StepStatus.Success)
                switch (Type)
                {
                    case HashTypes.None:
                        stepResults.Message = $"{column}A hash type was not selected.";
                        stepResults.Status = StepStatus.Failure;
                        break;
                    case HashTypes.SHA1:
                        result = await SHA1Hash.ComputeHashAsync(File.OpenRead(file), progress);
                        if (Hash.ToUpper() != result)
                        {
                            stepResults.Message = $"\r\nExpected {Hash.ToUpper()} but got {result.ToUpper()} instead.\r\n" + Common.ProcessVariableTokens(InvalidMessage, variableCache);
                            stepResults.Status = StepStatus.Failure;
                        }
                        break;
                    case HashTypes.SHA256:
                        result = await SHA256Hash.ComputeHashAsync(File.OpenRead(file), progress);
                        if (Hash.ToUpper() != result)
                        {
                            stepResults.Message = $"\r\nExpected {Hash.ToUpper()} but got {result.ToUpper()} instead.\r\n" + Common.ProcessVariableTokens(InvalidMessage, variableCache);
                            stepResults.Status = StepStatus.Failure;
                        }
                        break;
                }

            progress.Report(progressReport);
            return stepResults;
        }
    }
}
