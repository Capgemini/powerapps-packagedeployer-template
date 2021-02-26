namespace Capgemini.PowerApps.PackageDeployerTemplate.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
    using Capgemini.PowerApps.PackageDeployerTemplate.Extensions;
    using Microsoft.Extensions.Logging;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Functionality related to deploying connection references.
    /// </summary>
    public class ConnectionReferenceDeploymentService
    {
        private readonly ILogger logger;
        private readonly ICrmServiceAdapter crmSvc;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionReferenceDeploymentService"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="crmSvc">The <see cref="ICrmServiceAdapter"/>.</param>
        public ConnectionReferenceDeploymentService(ILogger logger, ICrmServiceAdapter crmSvc)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.crmSvc = crmSvc ?? throw new ArgumentNullException(nameof(crmSvc));
        }

        /// <summary>
        /// Updates connection references to used the provided connection names.
        /// </summary>
        /// <param name="connectionMap">Connection name by connection reference ID.</param>
        public void ConnectConnectionReferences(IDictionary<string, string> connectionMap)
        {
            if (connectionMap is null || !connectionMap.Any())
            {
                this.logger.LogInformation("No connections have been configured.");

                return;
            }

            var updateRequests = this
                .GetConnectionReferences(connectionMap.Keys.ToArray())
                .Select(e => new UpdateRequest
                {
                    Target = new Entity(Constants.ConnectionReference.LogicalName)
                    {
                        Id = e.Id,
                        Attributes =
                        {
                            {
                                Constants.ConnectionReference.Fields.ConnectionReferenceId,
                                e.Id
                            },
                            {
                                Constants.ConnectionReference.Fields.ConnectionId,
                                connectionMap[e.GetAttributeValue<string>(Constants.ConnectionReference.Fields.ConnectionReferenceLogicalName)]
                            },
                        },
                    },
                }).ToList();

            var result = this.crmSvc.ExecuteMultiple(updateRequests);
            if (result.IsFaulted)
            {
                this.logger.LogExecuteMultipleErrors(result);
            }
        }

        private IEnumerable<Entity> GetConnectionReferences(params string[] logicalNames)
        {
            if (logicalNames is null)
            {
                throw new ArgumentNullException(nameof(logicalNames));
            }

            return this.crmSvc.RetrieveMultipleByAttribute(
                    Constants.ConnectionReference.LogicalName,
                    Constants.ConnectionReference.Fields.ConnectionReferenceLogicalName,
                    logicalNames,
                    new ColumnSet(true)).Entities.ToList();
        }
    }
}