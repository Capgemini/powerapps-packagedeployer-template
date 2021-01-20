namespace Capgemini.PowerApps.PackageDeployerTemplate
{
    /// <summary>
    /// Constants.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Constants related to the process entity.
        /// </summary>
        public static class Process
        {
            /// <summary>
            /// An active state code.
            /// </summary>
            public const int StateCodeActive = 1;

            /// <summary>
            /// An inactive state code.
            /// </summary>
            public const int StateCodeInactive = 0;

            /// <summary>
            /// An active status code.
            /// </summary>
            public const int StatusCodeActive = 2;

            /// <summary>
            /// An inactive status code.
            /// </summary>
            public const int StatusCodeInactive = 1;
        }

        /// <summary>
        /// Constants related to the SLA entity.
        /// </summary>
        public static class Sla
        {
            /// <summary>
            /// An active state code.
            /// </summary>
            public const int StateCodeActive = 1;

            /// <summary>
            /// An inactive state code.
            /// </summary>
            public const int StateCodeInactive = 0;

            /// <summary>
            /// An active status code.
            /// </summary>
            public const int StatusCodeActive = 2;

            /// <summary>
            /// An inactive status code.
            /// </summary>
            public const int StatusCodeInactive = 1;
        }

        /// <summary>
        /// Constants related to the SDK message processing step entity.
        /// </summary>
        public static class SdkMessageProcessingStep
        {
            /// <summary>
            /// An active state code.
            /// </summary>
            public const int StateCodeInactive = 1;

            /// <summary>
            /// An inactive status code.
            /// </summary>
            public const int StatusCodeInactive = 2;
        }

        /// <summary>
        /// Constants related to the solution component entity.
        /// </summary>
        public static class SolutionComponent
        {
            /// <summary>
            /// Solution component type for flows.
            /// </summary>
            public const int WorkflowTypeFlow = 29;
        }
    }
}
