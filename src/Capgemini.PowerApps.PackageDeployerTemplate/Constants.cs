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
            /// The prefix for all Power Apps environment variables.
            /// </summary>
            public const string PowerAppsEnvironmentVariablePrefix = "EnvVar";

            /// <summary>
            /// The prefix for all connector base urls.
            /// </summary>
            public const string CustomConnectorBaseUrlPrefix = "ConnBaseUrl";

            /// <summary>
            /// The prefix for all environment variables.
            /// </summary>
            public const string EnvironmentVariablePrefix = "PACKAGEDEPLOYER_SETTINGS_";

            /// <summary>
            /// The (optional) username of a licensed user to use for connecting connection references and activating flows.
            /// </summary>
            public const string LicensedUsername = "LicensedUsername";

            /// <summary>
            /// The prefix for all mailbox settings.
            /// </summary>
            public const string MailboxPrefix = "Mailbox";
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
        /// Constants related to the connector entity.
        /// </summary>
        public static class Connector
        {
            /// <summary>
            /// The logical name.
            /// </summary>
            public const string LogicalName = "connector";

            /// <summary>
            /// Field logical names.
            /// </summary>
            public static class Fields
            {
                /// <summary>
                /// The connector ID.
                /// </summary>
                public const string ConnectorId = "connectorid";

                /// <summary>
                /// The logical name of the name.
                /// </summary>
                public const string Name = "name";

                /// <summary>
                /// The logical name of the openapidefinition.
                /// </summary>
                public const string OpenApiDefinition = "openapidefinition";
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

        /// <summary>
        /// Constants related to the mailbox.
        /// </summary>
        public static class Mailbox
        {
            /// <summary>
            /// The logical name.
            /// </summary>
            public const string LogicalName = "mailbox";

            /// <summary>
            /// A not run mailbox status.
            /// </summary>
            public const int MailboxStatusNotRun = 0;

            /// <summary>
            /// A success mailbox status.
            /// </summary>
            public const int MailboxStatusSuccess = 1;

            /// <summary>
            /// Field logical names.
            /// </summary>
            public static class Fields
            {
                /// <summary>
                /// The mailbox regarding object ID.
                /// </summary>
                public const string RegardingObjectid = "regardingobjectid";

                /// <summary>
                /// The Test Email Configuration Scheduled.
                /// </summary>
                public const string TestEmailConfigurationScheduled = "testemailconfigurationscheduled";

                /// <summary>
                /// The Mailbox Status.
                /// </summary>
                public const string MailboxStatus = "mailboxstatus";
            }
        }

        /// <summary>
        /// Constants related to the queue entity.
        /// </summary>
        public static class Queue
        {
            /// <summary>
            /// The logical name.
            /// </summary>
            public const string LogicalName = "queue";

            /// <summary>
            /// An empty email router access approval.
            /// </summary>
            public const int EmailRouterAccessApprovalEmpty = 0;

            /// <summary>
            /// An approved email router access approval.
            /// </summary>
            public const int EmailRouterAccessApprovalApproved = 1;

            /// <summary>
            /// Field logical names.
            /// </summary>
            public static class Fields
            {
                /// <summary>
                /// The Email Address.
                /// </summary>
                public const string EmailAddress = "emailaddress";

                /// <summary>
                /// The Email Router Access Approval.
                /// </summary>
                public const string EmailRouterAccessApproval = "emailrouteraccessapproval";
            }
        }

        /// <summary>
        /// Constants relating to the systemuser entity.
        /// </summary>
        public static class SystemUser
        {
            /// <summary>
            /// The logical name.
            /// </summary>
            public const string LogicalName = "systemuser";

            /// <summary>
            /// Field logical names.
            /// </summary>
            public static class Fields
            {
                /// <summary>
                /// The domain name.
                /// </summary>
                public const string DomainName = "domainname";

                /// <summary>
                /// The Azure AD object ID.
                /// </summary>
                public const string AzureActiveDirectoryObjectId = "azureactivedirectoryobjectid";
            }
        }

        /// <summary>
        /// Constants relating to the environmentvariabledefinition entity.
        /// </summary>
        public static class EnvironmentVariableDefinition
        {
            /// <summary>
            /// The logical name.
            /// </summary>
            public const string LogicalName = "environmentvariabledefinition";

            /// <summary>
            /// Field logical names.
            /// </summary>
            public static class Fields
            {
                /// <summary>
                /// The schema name.
                /// </summary>
                public const string SchemaName = "schemaname";
            }
        }

        /// <summary>
        /// Constants relating to the environmentvariablevalue entity.
        /// </summary>
        public static class EnvironmentVariableValue
        {
            /// <summary>
            /// The logical name.
            /// </summary>
            public const string LogicalName = "environmentvariablevalue";

            /// <summary>
            /// Field logical names.
            /// </summary>
            public static class Fields
            {
                /// <summary>
                /// The variable definition id.
                /// </summary>
                public const string EnvironmentVariableDefinitonId = "environmentvariabledefinitionid";

                /// <summary>
                /// The variable value.
                /// </summary>
                public const string Value = "value";
            }
        }
    }
}