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
        [XmlAttribute("storeTo")]
        public string StoreTo { get; set; } = string.Empty;

        /// <summary>
        /// Determines the weight of the step compared to the other steps.
        /// </summary>
        [XmlAttribute("weight")]
        public int Weight { get; set; }

        // Methods
        /// <summary>
        /// The method that is called by the UI to perform the step actions.
        /// </summary>
        public abstract Task<StepResults> Perform(Dictionary<string, object> valueCache, IProgress<ProgressReport> progress);
    }

    /// <summary>
    /// The ReportProgressEventArgs class passes completion percentages to the UI.
    /// </summary>
    public class ProgressReport
    {
        /// <summary>
        /// The current progress percentage being reported between 0 and 100.
        /// </summary>
        public float Percentage { get; set; } = 0.0f;

        /// <summary>
        /// The current status message of the step for this progress report.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Simple check for whether or not there is a message.
        /// </summary>
        public bool HasMessage => Message != string.Empty;
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
        public bool HasMessage => Message != string.Empty;
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
}