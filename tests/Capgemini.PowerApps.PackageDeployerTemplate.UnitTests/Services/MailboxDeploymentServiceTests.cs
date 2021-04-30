namespace Capgemini.PowerApps.PackageDeployerTemplate.UnitTests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
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
        public void UpdateApproveAndEnableMailboxes_LogNoQueueExist()
        {
            var sourceEmails = new string[] { "source_1@fake.com" };
            var mailboxConfigs = new Dictionary<string, string>
            {
                { "source@fake.com", "target@fake.com" },
            };

            var entityCollection = new EntityCollection
            {
                EntityName = Constants.Queue.LogicalName,
                Entities =
                {
                    new Entity(Constants.Queue.LogicalName, Guid.NewGuid()),
                },
            };
            this.crmServiceAdapterMock
                .Setup(x => x.RetrieveMultipleByAttribute(Constants.Queue.LogicalName, Constants.Queue.Fields.EmailAddress, sourceEmails, null))
                .Returns(entityCollection).Verifiable();

            this.mailboxDeploymentService.UpdateApproveAndEnableMailboxes(mailboxConfigs);
            this.loggerMock.VerifyLog(x => x.LogInformation($"No queue exist with emailaddress:{mailboxConfigs.ElementAt(0).Key}."));
        }

        [Fact]
        public void UpdateApproveAndEnableMailboxes_LogEmailNotApproved()
        {
            var sourceEmails = new string[] { "source@fake.com" };
            var mailboxConfigs = new Dictionary<string, string>
            {
                { "source@fake.com", "target@fake.com" },
            };

            var entity = new Entity
            {
                LogicalName = Constants.Queue.LogicalName,
                Id = Guid.NewGuid(),
            };
            entity.Attributes.Add(new KeyValuePair<string, object>(Constants.Queue.Fields.EmailRouterAccessApproval, new OptionSetValue(Constants.Queue.EmailRouterAccessApprovalEmpty)));
            var entityCollection = new EntityCollection
            {
                EntityName = Constants.Queue.LogicalName,
            };
            entityCollection.Entities.AddRange(entity);
            this.crmServiceAdapterMock
                .Setup(x => x.RetrieveMultipleByAttribute(Constants.Queue.LogicalName, Constants.Queue.Fields.EmailAddress, sourceEmails, null))
                .Returns(entityCollection).Verifiable();
            this.crmServiceAdapterMock.Setup(t => t.Retrieve(
                It.Is<string>(e => e.ToUpper() == Constants.Queue.LogicalName.ToUpper()),
                It.Is<Guid>(e => e == entity.Id),
                It.IsAny<ColumnSet>()))
                .Returns(entity);
            this.mailboxDeploymentService.UpdateApproveAndEnableMailboxes(mailboxConfigs);
            this.loggerMock.VerifyLog(x => x.LogWarning("[Failure] Email Address not Approved."));
        }

        [Fact]
        public void UpdateApproveAndEnableMailboxes_LogNoMailboxExist()
        {
            var sourceEmails = new string[] { "source@fake.com" };
            var mailboxConfigs = new Dictionary<string, string>
            {
                { "source@fake.com", "target@fake.com" },
            };

            var entity = new Entity
            {
                LogicalName = Constants.Queue.LogicalName,
                Id = Guid.NewGuid(),
            };
            entity.Attributes.Add(new KeyValuePair<string, object>(Constants.Queue.Fields.EmailRouterAccessApproval, new OptionSetValue(Constants.Queue.EmailRouterAccessApprovalApproved)));
            var entityCollection = new EntityCollection
            {
                EntityName = Constants.Queue.LogicalName,
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
                .Setup(x => x.RetrieveMultipleByAttribute(Constants.Queue.LogicalName, Constants.Queue.Fields.EmailAddress, sourceEmails, null))
                .Returns(entityCollection).Verifiable();
            this.crmServiceAdapterMock.Setup(t => t.Retrieve(
                It.Is<string>(e => e.ToUpper() == Constants.Queue.LogicalName.ToUpper()),
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
            var mailboxConfigs = new Dictionary<string, string>
            {
                { "source@fake.com", "target@fake.com" },
            };

            var entity = new Entity
            {
                LogicalName = Constants.Queue.LogicalName,
                Id = Guid.NewGuid(),
            };
            entity.Attributes.Add(new KeyValuePair<string, object>(Constants.Queue.Fields.EmailRouterAccessApproval, new OptionSetValue(Constants.Queue.EmailRouterAccessApprovalApproved)));
            var entityCollection = new EntityCollection
            {
                EntityName = Constants.Queue.LogicalName,
            };
            entityCollection.Entities.AddRange(entity);
            var mailboxEntity = new Entity
            {
                LogicalName = Constants.Mailbox.LogicalName,
                Id = Guid.NewGuid(),
            };
            mailboxEntity.Attributes.Add(new KeyValuePair<string, object>(Constants.Mailbox.Fields.MailboxStatus, new OptionSetValue(Constants.Mailbox.MailboxStatusNotRun)));
            var mailboxEntityCollection = new EntityCollection
            {
                EntityName = Constants.Mailbox.LogicalName,
            };
            mailboxEntityCollection.Entities.AddRange(mailboxEntity);
            this.crmServiceAdapterMock
                .Setup(x => x.RetrieveMultipleByAttribute(Constants.Queue.LogicalName, Constants.Queue.Fields.EmailAddress, sourceEmails, null))
                .Returns(entityCollection).Verifiable();
            this.crmServiceAdapterMock.Setup(t => t.Retrieve(
                It.Is<string>(e => e.ToUpper() == Constants.Queue.LogicalName.ToUpper()),
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
            this.loggerMock.VerifyLog(x => x.LogWarning("[Failure] Mailbox is not enabled"));
        }
    }
}
