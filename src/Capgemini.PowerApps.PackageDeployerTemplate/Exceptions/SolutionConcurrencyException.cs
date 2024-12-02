namespace Capgemini.PowerApps.PackageDeployerTemplate.Exceptions
{
    using System;

    /// <summary>
    /// Represents an exception that is thrown when ...
    /// </summary>
    public class SolutionConcurrencyException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionConcurrencyException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public SolutionConcurrencyException(string message)
            : base(message)
        {
        }
    }
}
