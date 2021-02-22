namespace Capgemini.PowerApps.PackageDeployerTemplate.UnitTests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
    using Capgemini.PowerApps.PackageDeployerTemplate.Config;
    using Capgemini.PowerApps.PackageDeployerTemplate.Services;
    using Microsoft.Extensions.Logging;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using Moq;
    using Xunit;

    public class MailboxDeploymentServiceTests
    {
        private readonly Mock<ILogger> loggerMock;
        private readonly Mock<ICrmServiceAdapter> crmServiceAdapterMock;
        private readonly MailboxDeploymentService mailboxDeploymentService;

        public MailboxDeploymentServiceTests()
        {
            this.loggerMock = new Mock<ILogger>();
            this.crmServiceAdapterMock = new Mock<ICrmServiceAdapter>();

            this.mailboxDeploymentService = new MailboxDeploymentService(this.loggerMock.Object, this.crmServiceAdapterMock.Object);
        }

        [Fact]
        public void Construct_NullPackageLog_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new MailboxDeploymentService(null, this.crmServiceAdapterMock.Object);
            });
        }

        [Fact]
        public void Construct_NullCrmService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new MailboxDeploymentService(this.loggerMock.Object, null);
            });
        }

        [Fact]
        public void UpdateApproveAndEnableMailboxes_LogNoConfig()
        {
            this.mailboxDeploymentService.UpdateApproveAndEnableMailboxes(null);

            this.loggerMock.VerifyLog(x => x.LogInformation("No mailboxes have been configured."));
        }

        [Fact]
        public void UpdateApproveAndEnableMailboxes_NoEnviromentPrefix_LogNoConfig()
        {
            var sourceEmails = new string[] { "source_1@fake.com" };
            var mailboxConfig = new MailboxConfig
            {
                EnvironmentPrefix = "dev",
                SourceEmailaddress = "source_2@fake.com",
            };

            List<MailboxConfig> mailboxConfigs = new List<MailboxConfig>();
            mailboxConfigs.Add(mailboxConfig);

            var entityCollection = new EntityCollection
            {
                EntityName = Constants.Mailbox.Queue.LogicalName,
                Entities =
                {
                    new Entity(Constants.Mailbox.Queue.LogicalName, Guid.NewGuid()),
                },
            };
            this.mailboxDeploymentService.UpdateApproveAndEnableMailboxes(mailboxConfigs
                .Where(m => m.EnvironmentPrefix == Environment.GetEnvironmentVariable(Constants.Settings.EnvironmentPrefix)));
            this.loggerMock.VerifyLog(x => x.LogInformation("No mailboxes have been configured."));
        }

        [Fact]
        public void UpdateApproveAndEnableMailboxes_LogNoQueueExist()
        {
            var sourceEmails = new string[] { "source_1@fake.com" };
            var mailboxConfig = new MailboxConfig
            {
                SourceEmailaddress = "source_2@fake.com",
            };

            List<MailboxConfig> mailboxConfigs = new List<MailboxConfig>();
            mailboxConfigs.Add(mailboxConfig);

            var entityCollection = new EntityCollection
            {
                EntityName = Constants.Mailbox.Queue.LogicalName,
                Entities =
                {
                    new Entity(Constants.Mailbox.Queue.LogicalName, Guid.NewGuid()),
                },
            };
            this.crmServiceAdapterMock
                .Setup(x => x.RetrieveMultipleByAttribute(Constants.Mailbox.Queue.LogicalName, Constants.Mailbox.Queue.Fields.EmailAddress, sourceEmails, null))
                .Returns(entityCollection).Verifiable();

            this.mailboxDeploymentService.UpdateApproveAndEnableMailboxes(mailboxConfigs);
            this.loggerMock.VerifyLog(x => x.LogInformation($"No queue exist with emailaddress:{mailboxConfig.SourceEmailaddress}."));
        }

        [Fact]
        public void UpdateApproveAndEnableMailboxes_LogEmailNotApproved()
        {
            var sourceEmails = new string[] { "source@fake.com" };
            var mailboxConfig = new MailboxConfig
            {
                SourceEmailaddress = "source@fake.com",
                TargetEmailaddress = "target@fake.com",
            };

            List<MailboxConfig> mailboxConfigs = new List<MailboxConfig>();
            mailboxConfigs.Add(mailboxConfig);

            var entity = new Entity
            {
                LogicalName = Constants.Mailbox.Queue.LogicalName,
                Id = Guid.NewGuid(),
            };
            entity.Attributes.Add(new KeyValuePair<string, object>(Constants.Mailbox.Queue.Fields.EmailrouterAccessApproval, new OptionSetValue((int)EmailRouterAccessApproval.Empty)));
            var entityCollection = new EntityCollection
            {
                EntityName = Constants.Mailbox.Queue.LogicalName,
            };
            entityCollection.Entities.AddRange(entity);
            this.crmServiceAdapterMock
                .Setup(x => x.RetrieveMultipleByAttribute(Constants.Mailbox.Queue.LogicalName, Constants.Mailbox.Queue.Fields.EmailAddress, sourceEmails, null))
                .Returns(entityCollection).Verifiable();
            this.crmServiceAdapterMock.Setup(t => t.Retrieve(
                It.Is<string>(e => e.ToUpper() == Constants.Mailbox.Queue.LogicalName.ToUpper()),
                It.Is<Guid>(e => e == entity.Id),
                It.IsAny<ColumnSet>()))
                .Returns(entity);
            this.mailboxDeploymentService.UpdateApproveAndEnableMailboxes(mailboxConfigs);
            this.loggerMock.VerifyLog(x => x.LogInformation("[Failure] Email Address not Approved."));
        }

        [Fact]
        public void UpdateApproveAndEnableMailboxes_LogNoMailboxExist()
        {
            var sourceEmails = new string[] { "source@fake.com" };
            var mailboxConfig = new MailboxConfig
            {
                SourceEmailaddress = "source@fake.com",
                TargetEmailaddress = "target@fake.com",
            };

            List<MailboxConfig> mailboxConfigs = new List<MailboxConfig>();
            mailboxConfigs.Add(mailboxConfig);

            var entity = new Entity
            {
                LogicalName = Constants.Mailbox.Queue.LogicalName,
                Id = Guid.NewGuid(),
            };
            entity.Attributes.Add(new KeyValuePair<string, object>(Constants.Mailbox.Queue.Fields.EmailrouterAccessApproval, new OptionSetValue((int)EmailRouterAccessApproval.Approved)));
            var entityCollection = new EntityCollection
            {
                EntityName = Constants.Mailbox.Queue.LogicalName,
            };
            entityCollection.Entities.AddRange(entity);
            var mailboxEntity = new Entity
            {
                LogicalName = Constants.Mailbox.LogicalName,
                Id = Guid.NewGuid(),
            };
            var mailboxEntityCollection = new EntityCollection
            {
                EntityName = Constants.Mailbox.LogicalName,
            };
            mailboxEntityCollection.Entities.AddRange(mailboxEntity);
            this.crmServiceAdapterMock
                .Setup(x => x.RetrieveMultipleByAttribute(Constants.Mailbox.Queue.LogicalName, Constants.Mailbox.Queue.Fields.EmailAddress, sourceEmails, null))
                .Returns(entityCollection).Verifiable();
            this.crmServiceAdapterMock.Setup(t => t.Retrieve(
                It.Is<string>(e => e.ToUpper() == Constants.Mailbox.Queue.LogicalName.ToUpper()),
                It.Is<Guid>(e => e == entity.Id),
                It.IsAny<ColumnSet>()))
                .Returns(entity);
            this.crmServiceAdapterMock
               .Setup(x => x.RetrieveMultipleByAttribute(Constants.Mailbox.LogicalName, Constants.Mailbox.Fields.RegardingObjectid, new object[] { Guid.NewGuid() }, null))
               .Returns(mailboxEntityCollection).Verifiable();
            this.mailboxDeploymentService.UpdateApproveAndEnableMailboxes(mailboxConfigs);
            this.loggerMock.VerifyLog(x => x.LogInformation("No mailbox exist."));
        }

        [Fact]
        public void UpdateApproveAndEnableMailboxes_LogMailboxNotEnabled()
        {
            var sourceEmails = new string[] { "source@fake.com" };
            var mailboxConfig = new MailboxConfig
            {
                SourceEmailaddress = "source@fake.com",
                TargetEmailaddress = "target@fake.com",
            };

            List<MailboxConfig> mailboxConfigs = new List<MailboxConfig>();
            mailboxConfigs.Add(mailboxConfig);

            var entity = new Entity
            {
                LogicalName = Constants.Mailbox.Queue.LogicalName,
                Id = Guid.NewGuid(),
            };
            entity.Attributes.Add(new KeyValuePair<string, object>(Constants.Mailbox.Queue.Fields.EmailrouterAccessApproval, new OptionSetValue((int)EmailRouterAccessApproval.Approved)));
            var entityCollection = new EntityCollection
            {
                EntityName = Constants.Mailbox.Queue.LogicalName,
            };
            entityCollection.Entities.AddRange(entity);
            var mailboxEntity = new Entity
            {
                LogicalName = Constants.Mailbox.LogicalName,
                Id = Guid.NewGuid(),
            };
            mailboxEntity.Attributes.Add(new KeyValuePair<string, object>(Constants.Mailbox.Fields.MailboxStatus, new OptionSetValue((int)MailboxStatus.NotRun)));
            var mailboxEntityCollection = new EntityCollection
            {
                EntityName = Constants.Mailbox.LogicalName,
            };
            mailboxEntityCollection.Entities.AddRange(mailboxEntity);
            this.crmServiceAdapterMock
                .Setup(x => x.RetrieveMultipleByAttribute(Constants.Mailbox.Queue.LogicalName, Constants.Mailbox.Queue.Fields.EmailAddress, sourceEmails, null))
                .Returns(entityCollection).Verifiable();
            this.crmServiceAdapterMock.Setup(t => t.Retrieve(
                It.Is<string>(e => e.ToUpper() == Constants.Mailbox.Queue.LogicalName.ToUpper()),
                It.Is<Guid>(e => e == entity.Id),
                It.IsAny<ColumnSet>()))
                .Returns(entity);
            this.crmServiceAdapterMock
               .Setup(x => x.RetrieveMultipleByAttribute(Constants.Mailbox.LogicalName, Constants.Mailbox.Fields.RegardingObjectid, new object[] { entity.Id }, null))
               .Returns(mailboxEntityCollection).Verifiable();
            this.crmServiceAdapterMock.Setup(t => t.Retrieve(
                It.Is<string>(e => e.ToUpper() == Constants.Mailbox.LogicalName.ToUpper()),
                It.Is<Guid>(e => e == mailboxEntity.Id),
                It.IsAny<ColumnSet>()))
                .Returns(mailboxEntity);
            this.mailboxDeploymentService.UpdateApproveAndEnableMailboxes(mailboxConfigs);
            this.loggerMock.VerifyLog(x => x.LogInformation("[Failure] Mailbox is not enabled"));
        }
    }
}
