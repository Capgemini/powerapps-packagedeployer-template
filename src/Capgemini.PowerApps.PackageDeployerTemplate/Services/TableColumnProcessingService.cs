namespace Capgemini.PowerApps.PackageDeployerTemplate.Services
{
    using System;
    using System.Collections.Generic;
    using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
    using Capgemini.PowerApps.PackageDeployerTemplate.Config;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Extensions.Logging;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Service responsible for setting Auto-number seeds.
    /// </summary>
    public class TableColumnProcessingService
    {
        private readonly ILogger logger;
        private readonly ICrmServiceAdapter crmSvc;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableColumnProcessingService"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="crmSvc">The <see cref="ICrmServiceAdapter"/>.</param>
        public TableColumnProcessingService(ILogger logger, ICrmServiceAdapter crmSvc)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.crmSvc = crmSvc ?? throw new ArgumentNullException(nameof(crmSvc));
        }

        /// <summary>
        /// Processes tables with columns that have specific configurations, such as setting Auto-number seeds.
        /// </summary>
        /// <param name="tableConfigs">A collection of table elements to process specific attributes for.</param>
        public void ProcessTables(IEnumerable<TableConfig> tableConfigs)
        {
            List<OrganizationRequest> requests = new List<OrganizationRequest>();

            // Get all of the Auto-number seed configurations in the collection
            requests.AddRange(this.GenerateAutonumberSeedRequests(tableConfigs));

            // Check if we have requests that need processing.
            if (requests.Count > 0)
            {
                this.logger.LogInformation("Executing requests for table columns.");
                this.crmSvc.ExecuteMultiple(requests);
            }
            else
            {
                this.logger.LogInformation("No requests for table columns were added.");
            }
        }

        /// <summary>
        /// Sets the Auto-number seeds in a given target environment.
        /// </summary>
        /// <param name="tableConfigs">A collection of table elements to check for Auto-number seed setting configurations.</param>
        private List<SetAutoNumberSeedRequest> GenerateAutonumberSeedRequests(IEnumerable<TableConfig> tableConfigs)
        {
            List<SetAutoNumberSeedRequest> autonumberSeedRequests = new List<SetAutoNumberSeedRequest>();

            // Loop through the tables and their columns to find any auto-number configurations.
            foreach (TableConfig tableConfig in tableConfigs)
            {
                foreach (ColumnConfig column in tableConfig.Columns)
                {
                    // Ensure the seed value has been populated. This indicates that this element is an auto-number configuration.
                    if (column.AutonumberSeedValue != null)
                    {
                        this.logger.LogInformation($"Adding auto-number seed request. Entity Name: {tableConfig.Name}. Auto-number Attribute: {column.Name}. Value: {column.AutonumberSeedValue}");
                        autonumberSeedRequests.Add(new SetAutoNumberSeedRequest
                        {
                            EntityName = tableConfig.Name,
                            AttributeName = column.Name,
                            Value = (int)column.AutonumberSeedValue,
                        });
                    }
                }
            }

            // Check if we did not find any autonumber seed configurations and output a message to the log file.
            if (autonumberSeedRequests.Count == 0)
            {
                this.logger.LogInformation("No requests for Auto-number seeds were added.");
            }

            return autonumberSeedRequests;
        }
    }
}
