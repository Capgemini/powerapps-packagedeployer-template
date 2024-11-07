namespace Capgemini.PowerApps.PackageDeployerTemplate.Exceptions
{
    using System;

    /// <summary>
    /// Represents an exception that is thrown when a customization lock occurs.
    /// </summary>
    public class CustomizationLockException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomizationLockException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public CustomizationLockException(string message)
            : base(message)
        {
        }
    }
}
