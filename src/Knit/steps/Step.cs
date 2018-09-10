using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Knit
{
    /// <summary>
    /// The generic Step base class used by all of the other steps.
    /// </summary>
    public abstract class Step
    {
        // Properties
        /// <summary>
        /// The name of the step.
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// The name of the variable that this step might store a value to.
        /// </summary>
        [XmlAttribute("variable")]
        public string Variable { get; set; } = string.Empty;

        /// <summary>
        /// Determines the weight of the step compared to the other steps.
        /// </summary>
        [XmlAttribute("weight")]
        public int Weight { get; set; }

        /// <summary>
        /// Points to a valueCahce variable that determines whether this step will run or not.
        /// </summary>
        [XmlAttribute("run")]
        public string Run { get; set; } = string.Empty;

        /// <summary>
        /// Allows the UI to pass the working directory to every step.
        /// </summary>
        [XmlIgnore]
        public string WorkingDirectory { get; set; }

        // Methods
        /// <summary>
        /// The method that is called by the UI to perform the step actions.
        /// </summary>
        public abstract Task<StepResults> Perform(Dictionary<string, object> variableCache, IProgress<ProgressReport> progress);
    }

    /// <summary>
    /// The ProgressReport class passes completion percentages to the UI.
    /// </summary>
    public class ProgressReport
    {
        /// <summary>
        /// The current progress percentage being reported between 0 and 100.
        /// </summary>
        public double Percentage { get; set; } = 0.0;

        /// <summary>
        /// The current status message of the step for this progress report.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Simple check for whether or not there is a message.
        /// </summary>
        public bool HasMessage => !string.IsNullOrWhiteSpace(Message);

        /// <summary>
        /// Determines whether or not a new line is appended to messages received through a ProgressReport.
        /// </summary>
        public bool NewLine { get; set; } = true;
    }

    /// <summary>
    /// The StepResults class passes completion status to the UI.
    /// </summary>
    public class StepResults
    {
        /// <summary>
        /// Whether or not the step completed successfully.
        /// </summary>
        public StepStatus Status { get; set; } = StepStatus.Success;

        /// <summary>
        /// Step completion message to be logged.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Simple check for whether or not there is a message.
        /// </summary>
        public bool HasMessage => !string.IsNullOrWhiteSpace(Message);
    }

    /// <summary>
    /// Determines UI behaviour after a step completes.
    /// </summary>
    public enum StepStatus
    {
        Success,
        Failure,
        Cancel,
        Skip,
        Error
    }

    /// <summary>
    /// Marker interface that tells the UI the step is meant for debugging only.
    /// </summary>
    public interface IIsDebugStep { }
}