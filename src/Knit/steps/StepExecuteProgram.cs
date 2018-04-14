using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Knit
{
    public class StepExecuteProgram : Step
    {
        // Properties
        [XmlAttribute("executable")]
        public string Executable { get; set; } = string.Empty;

        [XmlElement("arguments")]
        public string Arguments { get; set; } = string.Empty;

        [XmlElement("percentageRegex")]
        public string PercentageRegex { get; set; } = string.Empty;

        [XmlAttribute("startMessage")]
        public string StartMessage { get; set; } = string.Empty;

        [XmlAttribute("endMessage")]
        public string EndMessage { get; set; } = string.Empty;

        // Methods
        public override async Task<StepResults> Perform(Dictionary<string, object> variableCache, IProgress<ProgressReport> progress)
        {
            progress.Report(new ProgressReport { Message = StartMessage });
            var arguments = variableCache.Aggregate(Arguments, (str, pair) => str.Replace("{" + pair.Key + "}", pair.Value.ToString()));

            // Handle variable modifiers
            foreach (var key in variableCache.Keys)
            {
                var pathKey = "{" + key + @":DIR}";
                if (Arguments.Contains(pathKey))
                    arguments = Regex.Replace(arguments, pathKey + @"\\?", Path.GetDirectoryName(variableCache[key].ToString()) + "\\");
            }

            var startInfo = new ProcessStartInfo()
            {
                FileName = Path.Combine(WorkingDirectory, Executable),
                WorkingDirectory = WorkingDirectory,
                Arguments = arguments
            };

            // Process Async
            var rpa = new RunProcessAsync();
            rpa.OutputDataReceived += HandleOutput;
            rpa.ErrorDataReceived += HandleOutput;
            rpa.Exited += (sender, args) =>
            {
                progress.Report(new ProgressReport { Message = EndMessage });
            };

            void HandleOutput(object sender, EventArgs e)
            {
                var dr = (DataReceivedEventArgs)e;

                if (PercentageRegex != string.Empty)
                {
                    double.TryParse(Regex.Match(dr.Data, PercentageRegex, RegexOptions.IgnoreCase).Value, out var percentage);
                    progress.Report(new ProgressReport { Message = string.Empty, Percentage = percentage });
                }
                else
                    progress.Report(new ProgressReport { Message = dr.Data, Percentage = 0 });
            }

            await rpa.StartAsync(startInfo);

            return new StepResults { Status = StepStatus.Success };
        }

    }
}
