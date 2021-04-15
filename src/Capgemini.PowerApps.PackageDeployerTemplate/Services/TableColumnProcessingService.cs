namespace Capgemini.PowerApps.PackageDeployerTemplate.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
    using Capgemini.PowerApps.PackageDeployerTemplate.Config;
    using Capgemini.PowerApps.PackageDeployerTemplate.Extensions;
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
                var executeMultipleResponse = this.crmSvc.ExecuteMultiple(requests);

                if (executeMultipleResponse.IsFaulted)
                {
                        this.logger.LogError("Error processing requests for table columns");
                        this.logger.LogExecuteMultipleErrors(executeMultipleResponse);
                }
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
                foreach (ColumnConfig column in tableConfig.Columns.Where(c => c.AutonumberSeedValue.HasValue && !this.AutonumberSeedAlreadySet(tableConfig.Name, c)))
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

            // Check if we did not find any autonumber seed configurations and output a message to the log file.
            if (autonumberSeedRequests.Count == 0)
            {
                this.logger.LogInformation("No requests for Auto-number seeds were added.");
            }

            return autonumberSeedRequests;
        }

        /// <summary>
        /// Checks if the Auto-number seed value is already set to the desired value. If so, we do not want to set the value as it will reset the next number in the sequence to the seed value.
        /// </summary>
        /// <returns>Boolean indicating if the seed value is already set in the target environment.</returns>
        private bool AutonumberSeedAlreadySet(string tableName, ColumnConfig columnConfig)
        {
            GetAutoNumberSeedRequest request = new GetAutoNumberSeedRequest
            {
                EntityName = tableName,
                AttributeName = columnConfig.Name,
            };

            GetAutoNumberSeedResponse response = (GetAutoNumberSeedResponse)this.crmSvc.Execute(request);

            if (response != null && response.AutoNumberSeedValue == columnConfig.AutonumberSeedValue)
            {
                this.logger.LogInformation($"Auto-number seed {columnConfig.Name} for {tableName} is already set to value: {columnConfig.AutonumberSeedValue}");
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
