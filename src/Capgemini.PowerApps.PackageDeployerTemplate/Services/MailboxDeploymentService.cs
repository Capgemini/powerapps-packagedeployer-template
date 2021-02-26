namespace Capgemini.PowerApps.PackageDeployerTemplate.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
    using Capgemini.PowerApps.PackageDeployerTemplate.Config;
    using Microsoft.Extensions.Logging;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Deployment functionality related to SLAs.
    /// </summary>
    public class MailboxDeploymentService
    {
        private readonly ILogger logger;
        private readonly ICrmServiceAdapter crmSvc;

        /// <summary>
        /// Initializes a new instance of the <see cref="MailboxDeploymentService"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="crmSvc">The <see cref="ICrmServiceAdapter"/>.</param>
        public MailboxDeploymentService(ILogger logger, ICrmServiceAdapter crmSvc)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.crmSvc = crmSvc ?? throw new ArgumentNullException(nameof(crmSvc));
        }

        /// <summary>
        /// Update, approve and enable mailboxes.
        /// </summary>
        /// <param name="mailboxConfigs">The mailbox configs to use.</param>
        public void UpdateApproveAndEnableMailboxes(IDictionary<string, string> mailboxConfigs)
        {
            this.logger.LogInformation($"{nameof(MailboxDeploymentService)}: START Approve-Test&Enable Mailboxes");
            if (mailboxConfigs is null || !mailboxConfigs.Any())
            {
                this.logger.LogInformation("No mailboxes have been configured.");
                return;
            }

            foreach (var mailboxConfig in mailboxConfigs)
            {
                this.logger.LogInformation($"{nameof(MailboxDeploymentService)}: Mailbox Approval Process Started:{mailboxConfig.Key}");
                Guid entityid = this.UpdateEmailAddress(mailboxConfig);
                if (entityid != Guid.Empty)
                {
                    var isApproved = this.ApproveEmail(entityid);
                    if (isApproved)
                    {
                        this.EnableMailbox(entityid);
                        this.logger.LogInformation($"{nameof(MailboxDeploymentService)}: Mailbox Approved and Enabled:{mailboxConfig.Value}");
                    }
                }
            }
        }

        /// <summary>
        /// Update queue's email address.
        /// </summary>
        /// <param name="mailboxConfig">The mailbox config to use.</param>
        /// <returns>Returns Queue ID.</returns>
        private Guid UpdateEmailAddress(KeyValuePair<string, string> mailboxConfig)
        {
            var retrieveMultipleResponse = this.crmSvc.RetrieveMultipleByAttribute(Constants.Queue.LogicalName, Constants.Queue.Fields.EmailAddress, new object[] { mailboxConfig.Key });
            var entity = retrieveMultipleResponse?.Entities?.FirstOrDefault();
            if (entity == null)
            {
                this.logger.LogInformation($"No queue exist with emailaddress:{mailboxConfig.Key}.");
                return Guid.Empty;
            }

            entity[Constants.Queue.Fields.EmailAddress] = mailboxConfig.Value;
            this.crmSvc.Update(entity);
            return entity.Id;
        }

        /// <summary>
        /// Approve the Email.
        /// </summary>
        /// <param name="entityid">Queue ID to use.</param>
        /// <returns>Returns approval status(true/false).</returns>
        private bool ApproveEmail(Guid entityid)
        {
            bool isApproved = true;
            Entity updateEntity = new Entity(Constants.Queue.LogicalName, entityid);
            updateEntity.Attributes.Add(new KeyValuePair<string, object>(Constants.Queue.Fields.EmailRouterAccessApproval, new OptionSetValue(Constants.Queue.EmailRouterAccessApprovalApproved)));
            this.crmSvc.Update(updateEntity);

            var entity = this.crmSvc.Retrieve(updateEntity.LogicalName, entityid, new ColumnSet(Constants.Queue.Fields.EmailRouterAccessApproval));

            int i = 0;
            int count = 1;
            int delay = 30;
            while (entity.GetAttributeValue<OptionSetValue>(Constants.Queue.Fields.EmailRouterAccessApproval).Value != Constants.Queue.EmailRouterAccessApprovalApproved)
            {
                i++;
                if (i > count)
                {
                    this.logger.LogWarning("[Failure] Email Address not Approved within allotted timeframe.");
                    break;
                }

                Thread.Sleep(1000 * delay);
                entity = this.crmSvc.Retrieve(updateEntity.LogicalName, entityid, new ColumnSet(Constants.Queue.Fields.EmailRouterAccessApproval));
            }

            if (entity.GetAttributeValue<OptionSetValue>(Constants.Queue.Fields.EmailRouterAccessApproval).Value != Constants.Queue.EmailRouterAccessApprovalApproved)
            {
                this.logger.LogWarning("[Failure] Email Address not Approved.");
                isApproved = false;
            }

            return isApproved;
        }

        /// <summary>
        /// Test and Enable mailbox.
        /// </summary>
        /// <param name="entityid">Queue ID to use.</param>
        private void EnableMailbox(Guid entityid)
        {
            var retrieveMultipleResponse = this.crmSvc.RetrieveMultipleByAttribute(Constants.Mailbox.LogicalName, Constants.Mailbox.Fields.RegardingObjectid, new object[] { entityid });
            var entity = retrieveMultipleResponse?.Entities?.FirstOrDefault();
            if (entity == null)
            {
                this.logger.LogInformation($"No mailbox exist.");
                return;
            }

            entity.Attributes.Add(new KeyValuePair<string, object>(Constants.Mailbox.Fields.TestEmailConfigurationScheduled, true));
            this.crmSvc.Update(entity);

            Entity updatedEntity = this.crmSvc.Retrieve(entity.LogicalName, entity.Id, new ColumnSet(new string[] { Constants.Mailbox.Fields.MailboxStatus }));

            int i = 0;
            int count = 3;
            int delay = 30;
            while (updatedEntity.GetAttributeValue<OptionSetValue>(Constants.Mailbox.Fields.MailboxStatus).Value != Constants.Mailbox.MailboxStatusSuccess)
            {
                i++;
                if (i > count)
                {
                    this.logger.LogWarning("[Failure] Mailbox is not Approved within allotted timeframe.");
                    break;
                }

                Thread.Sleep(1000 * delay);
                updatedEntity = this.crmSvc.Retrieve(entity.LogicalName, entity.Id, new ColumnSet(new string[] { Constants.Mailbox.Fields.MailboxStatus }));
            }

            if (updatedEntity.GetAttributeValue<OptionSetValue>(Constants.Mailbox.Fields.MailboxStatus).Value != Constants.Mailbox.MailboxStatusSuccess)
            {
                this.logger.LogWarning("[Failure] Mailbox is not enabled");
            }
        }
    }
}