namespace Capgemini.PowerApps.PackageDeployerTemplate
{
    /// <summary>
    /// Constants.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Constants relating to settings.
        /// </summary>
        public static class Settings
        {
            /// <summary>
            /// The prefix for all connection reference settings.
            /// </summary>
            public const string ConnectionReferencePrefix = "ConnRef";

            /// <summary>
            /// The prefix for all environment variables.
            /// </summary>
            public const string EnvironmentVariablePrefix = "PACKAGEDEPLOYER_SETTINGS_";

            /// <summary>
            /// The username of a licensed deployment user.
            /// </summary>
            public const string LicensedUsername = "LicensedUsername";

            /// <summary>
            /// The password of a licensed deployment user.
            /// </summary>
            public const string LicensedPassword = "LicensedPassword";
        }

        /// <summary>
        /// Constants related to the process entity.
        /// </summary>
        public static class Process
        {
            /// <summary>
            /// The logical name.
            /// </summary>
            public const string LogicalName = "workflow";

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

            /// <summary>
            /// Field logical names.
            /// </summary>
            public static class Fields
            {
                /// <summary>
                /// The name of the process.
                /// </summary>
                public const string Name = "name";
            }
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
            /// The logical name.
            /// </summary>
            public const string LogicalName = "solutioncomponent";

            /// <summary>
            /// Solution component type for connection references.
            /// </summary>
            public const int ComponentTypeConnectionReference = 10016;

            /// <summary>
            /// Solution component type for flows.
            /// </summary>
            public const int ComponentTypeFlow = 29;

            /// <summary>
            /// Field logical names.
            /// </summary>
            public static class Fields
            {
                /// <summary>
                /// The object ID.
                /// </summary>
                public const string ObjectId = "objectid";

                /// <summary>
                /// The component type.
                /// </summary>
                public const string ComponentType = "componenttype";

                /// <summary>
                /// The solution ID.
                /// </summary>
                public const string SolutionId = "solutionid";
            }
        }

        /// <summary>
        /// Constants related to the solution entity.
        /// </summary>
        public static class Solution
        {
            /// <summary>
            /// The logical name.
            /// </summary>
            public const string LogicalName = "solution";

            /// <summary>
            /// Field logical names.
            /// </summary>
            public static class Fields
            {
                /// <summary>
                /// The unique name of the solution.
                /// </summary>
                public const string UniqueName = "uniquename";

                /// <summary>
                /// The solution ID.
                /// </summary>
                public const string SolutionId = "solutionid";
            }
        }

        /// <summary>
        /// Constants related to the connection reference entity.
        /// </summary>
        public static class ConnectionReference
        {
            /// <summary>
            /// The logical name.
            /// </summary>
            public const string LogicalName = "connectionreference";

            /// <summary>
            /// Field logical names.
            /// </summary>
            public static class Fields
            {
                /// <summary>
                /// The connection reference ID.
                /// </summary>
                public const string ConnectionId = "connectionid";

                /// <summary>
                /// The connection reference ID.
                /// </summary>
                public const string ConnectionReferenceId = "connectionreferenceid";

                /// <summary>
                /// The logical name of the connection reference.
                /// </summary>
                public const string ConnectionReferenceLogicalName = "connectionreferencelogicalname";
            }
        }
    }
}
