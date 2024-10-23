namespace Capgemini.PowerApps.PackageDeployerTemplate.Exceptions
{
    using System;

    /// <summary>
    /// Represents an exception that is thrown when an operation related to Solution History fails.
    /// </summary>
    public class SolutionHistoryOperationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionHistoryOperationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public SolutionHistoryOperationException(string message)
            : base(message)
            {
            }
    }
}
