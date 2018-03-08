using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Knit
{
    /// <summary>
    /// The generic Step base class used for all of the other steps.
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

        // Methods
        /// <summary>
        /// The abstract method that is called by the UI to perform the step actions.
        /// </summary>
        public abstract bool Perform(Dictionary<string, object> valueCache);

        // Events
        public delegate void ReportProgressEventHandler(object sender, ReportProgressEventArgs e);

        public event ReportProgressEventHandler ReportProgress;

        protected virtual void OnReportProgress(ReportProgressEventArgs e)
        {
            ReportProgress?.Invoke(this, e);
        }

        /// <summary>
        /// Step completion event delegate.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        public delegate void StepCompleteEventHandler(object sender, StepCompleteEventArgs e);

        /// <summary>
        /// 
        /// </summary>
        public event StepCompleteEventHandler StepComplete;

        protected virtual void OnStepComplete(StepCompleteEventArgs e)
        {
            StepComplete?.Invoke(this, e);
        }
    }

    /// <summary>
    /// The ReportProgressEventArgs class passes completion percentages to the UI.
    /// </summary>
    public class ReportProgressEventArgs : EventArgs
    {
        /// <summary>
        /// The current progress percentage being reported between 0 and 100.
        /// </summary>
        public float Completion { get; set; }

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
    /// The StepCompleteEventArgs class passes completion status to the UI.
    /// </summary>
    public class StepCompleteEventArgs : EventArgs
    {
        /// <summary>
        /// Whether or not the step completed successfully.
        /// </summary>
        public StepCompletionStatus Status { get; set; }

        /// <summary>
        /// Step completion message to be logged.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Create a new instance of StepCompleteEventArgs.
        /// </summary>
        /// <param name="status">The completion status being reported to the UI.</param>
        /// <param name="message">The completion message being reported to the UI.</param>
        public StepCompleteEventArgs(StepCompletionStatus status, string message)
        {
            Status = status;
            Message = message;
        }
    }

    public enum StepCompletionStatus
    {
        Success,
        Failure,
        Cancel,
        Skip,
        Error
    }
}