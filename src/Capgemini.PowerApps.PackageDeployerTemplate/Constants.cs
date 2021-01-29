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
        /// Constants related to the workflow entity.
        /// </summary>
        public static class Workflow
        {
            /// <summary>
            /// Definition option set value for type option set.
            /// </summary>
            public const int TypeDefinition = 1;

            /// <summary>
            /// Modern flow option set value for type option set.
            /// </summary>
            public const int CategoryModernFlow = 5;

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
                /// The workflow ID.
                /// </summary>
                public const string WorkflowId = "workflowid";

                /// <summary>
                /// The workflow category.
                /// </summary>
                public const string Category = "category";

                /// <summary>
                /// The process type.
                /// </summary>
                public const string Type = "type";

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
            /// The logical name.
            /// </summary>
            public const string LogicalName = "sla";

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
                /// The name.
                /// </summary>
                public const string Name = "name";

                /// <summary>
                /// The status.
                /// </summary>
                public const string StateCode = "statecode";

                /// <summary>
                /// The status reason.
                /// </summary>
                public const string StatusCode = "statuscode";

                /// <summary>
                /// The Is Default field.
                /// </summary>
                public const string IsDefault = "isdefault";
            }
        }

        /// <summary>
        /// Constants related to the SDK message processing step entity.
        /// </summary>
        public static class SdkMessageProcessingStep
        {
            /// <summary>
            /// The logical name.
            /// </summary>
            public const string LogicalName = "sdkmessageprocessingstep";

            /// <summary>
            /// An active state code.
            /// </summary>
            public const int StateCodeActive = 0;

            /// <summary>
            /// An active status code.
            /// </summary>
            public const int StatusCodeActive = 1;

            /// <summary>
            /// An active state code.
            /// </summary>
            public const int StateCodeInactive = 1;

            /// <summary>
            /// An inactive status code.
            /// </summary>
            public const int StatusCodeInactive = 2;

            /// <summary>
            /// Field logical names.
            /// </summary>
            public static class Fields
            {
                /// <summary>
                /// The name of the SDK message processing step.
                /// </summary>
                public const string Name = "name";
            }
        }

        /// <summary>
        /// Constants elated to the document template entity.
        /// </summary>
        public static class DocumentTemplate
        {
            /// <summary>
            /// The logical name.
            /// </summary>
            public const string LogicalName = "documenttemplate";

            /// <summary>
            /// Document type option set value for Excel documents.
            /// </summary>
            public const int DocumentTypeExcel = 1;

            /// <summary>
            /// Document type option set value for Word documents.
            /// </summary>
            public const int DocumentTypeWord = 2;

            /// <summary>
            /// Field logical names.
            /// </summary>
            public static class Fields
            {
                /// <summary>
                /// The document template content.
                /// </summary>
                public const string Content = "content";

                /// <summary>
                /// The name.
                /// </summary>
                public const string Name = "name";

                /// <summary>
                /// The document type.
                /// </summary>
                public const string DocumentType = "documenttype";

                /// <summary>
                /// The associated entity type code.
                /// </summary>
                public const string AssociatedEntityTypeCode = "associatedentitytypecode";
            }
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
            /// Solution component type for workflows.
            /// </summary>
            public const int ComponentTypeWorkflow = 29;

            /// <summary>
            /// Solution component type for SDK steps.
            /// </summary>
            public const int ComponentTypeSdkStep = 92;

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
