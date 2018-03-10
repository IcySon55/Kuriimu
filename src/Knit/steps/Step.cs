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
        public abstract Task<StepResults> Perform(Dictionary<string, object> valueCache);

        // Events
        /// <summary>
        /// An event delegate for handling progress reported by steps.
        /// </summary>
        /// <param name="sender">The sender is always the Step being performed.</param>
        /// <param name="e">The progress event args being reported.</param>
        public delegate void ReportProgressEventHandler(object sender, ReportProgressEventArgs e);

        /// <summary>
        /// An event for handling progress reported by steps.
        /// </summary>
        public event ReportProgressEventHandler ReportProgress;

        /// <summary>
        /// Allows derived steps to report progress.
        /// </summary>
        /// <param name="e">The progress event args being reported.</param>
        protected virtual void OnReportProgress(ReportProgressEventArgs e)
        {
            ReportProgress?.Invoke(this, e);
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// The ReportProgressEventArgs class passes completion percentages to the UI.
    /// </summary>
    public class ReportProgressEventArgs : EventArgs
    {
        /// <summary>
        /// The current progress percentage being reported between 0 and 100.
        /// </summary>
        public float Completion { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Create a new instance of ReportProgressEventArgs.
        /// </summary>
        /// <param name="completion">The current progress percentage being reported between 0 and 100.</param>
        public ReportProgressEventArgs(float completion)
        {
            Completion = completion;
        }
    }

    /// <summary>
    /// The StepResults class passes completion status to the UI.
    /// </summary>
    public class StepResults
    {
        /// <summary>
        /// Whether or not the step completed successfully.
        /// </summary>
        public StepStatus Status { get; set; }

        /// <summary>
        /// Step completion message to be logged.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Create a new instance of StepResults.
        /// </summary>
        /// <param name="status">The completion status being reported to the UI.</param>
        /// <param name="message">The completion message being reported to the UI.</param>
        public StepResults(StepStatus status, string message)
        {
            Status = status;
            Message = message;
        }
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