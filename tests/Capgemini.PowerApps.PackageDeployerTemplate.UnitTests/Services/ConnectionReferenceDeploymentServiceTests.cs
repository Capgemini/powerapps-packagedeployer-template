namespace Capgemini.PowerApps.PackageDeployerTemplate.UnitTests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
    using Capgemini.PowerApps.PackageDeployerTemplate.Services;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Query;
    using Moq;
    using Xunit;

    public class ConnectionReferenceDeploymentServiceTests
    {
        private readonly Mock<ILogger> loggerMock;
        private readonly Mock<ICrmServiceAdapter> crmSvc;

        private readonly ConnectionReferenceDeploymentService connectionReferenceSvc;

        public ConnectionReferenceDeploymentServiceTests()
        {
            this.loggerMock = new Mock<ILogger>();
            this.crmSvc = new Mock<ICrmServiceAdapter>();

            this.connectionReferenceSvc = new ConnectionReferenceDeploymentService(this.loggerMock.Object, this.crmSvc.Object);
        }

        [Fact]
        public void ConnectConnectionReferences_NullConnectionMap_DoesNotThrow()
        {
            Action callingConnectConnectionReferencesWithNullConnectionMap = () => this.connectionReferenceSvc.ConnectConnectionReferences(null);

            callingConnectConnectionReferencesWithNullConnectionMap.Should().NotThrow<ArgumentNullException>();
        }

        [Fact]
        public void ConnectConnectionReferences_EmptyConnectionMap_DoesNotThrow()
        {
            Action callingConnectConnectionReferencesWithNullConnectionMap = () => this.connectionReferenceSvc.ConnectConnectionReferences(new Dictionary<string, string>());

            callingConnectConnectionReferencesWithNullConnectionMap.Should().NotThrow<ArgumentNullException>();
        }

        [Fact]
        public void ConnectConnectionReferences_ConnectionMappingPassed_UpdatesConnectionReferences()
        {
            var connectionMap = new Dictionary<string, string>
            {
                { "pdt_sharedapprovals_d7dcb", "12038109da0wud01" },
            };
            var connectionReferences = new EntityCollection(
                connectionMap.Keys.Select(k =>
                {
                    var entity = new Entity(Constants.ConnectionReference.LogicalName, Guid.NewGuid());
                    entity.Attributes.Add(Constants.ConnectionReference.Fields.ConnectionReferenceLogicalName, k);
                    return entity;
                }).ToList());
            this.crmSvc.Setup(
                c => c.RetrieveMultipleByAttribute(
                    Constants.ConnectionReference.LogicalName,
                    Constants.ConnectionReference.Fields.ConnectionReferenceLogicalName,
                    connectionMap.Keys,
                    It.IsAny<ColumnSet>()))
                .Returns(connectionReferences);
            this.crmSvc.Setup(c => c.ExecuteMultiple(
                    It.Is<List<UpdateRequest>>(
                        reqs => reqs.All(r =>
                            r.Target.LogicalName == Constants.ConnectionReference.LogicalName &&
                            connectionReferences.Entities.Any(e => e.Id == r.Target.Id) &&
                            connectionMap.Values.Contains(r.Target.Attributes[Constants.ConnectionReference.Fields.ConnectionId]))),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(new ExecuteMultipleResponse { Results = { { "IsFaulted", false } } })
                .Verifiable();

            this.connectionReferenceSvc.ConnectConnectionReferences(connectionMap);

            this.crmSvc.VerifyAll();
        }
    }
}
