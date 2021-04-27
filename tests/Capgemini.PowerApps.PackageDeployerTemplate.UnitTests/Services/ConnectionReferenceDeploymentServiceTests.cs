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

            var connectionReferences = this.MockConnectionReferencesForConnectionMap(connectionMap);
            this.MockUpdateConnectionReferencesResponse(new ExecuteMultipleResponse { Results = { { "IsFaulted", false } } });

            this.connectionReferenceSvc.ConnectConnectionReferences(connectionMap);

            this.crmSvc.Verify(
                svc => svc.Execute(
                    It.Is<ExecuteMultipleRequest>(
                        execMultiReq => execMultiReq.Requests.Cast<UpdateRequest>().Any(
                            req =>
                                req.Target.GetAttributeValue<string>(Constants.ConnectionReference.Fields.ConnectionId) == connectionMap.Values.First() &&
                                req.Target.Id == connectionReferences.Entities.First().Id))));
        }

        [Fact]
        public void ConnectConnectionReferences_WithConnectionOwner_UpdatesAsConnectionOwner()
        {
            var connectionOwner = "licenseduser@domaincom";
            var connectionMap = new Dictionary<string, string>
            {
                { "pdt_sharedapprovals_d7dcb", "12038109da0wud01" },
            };
            this.MockConnectionReferencesForConnectionMap(connectionMap);
            this.MockUpdateConnectionReferencesResponse(new ExecuteMultipleResponse { Results = { { "IsFaulted", false } } });

            this.connectionReferenceSvc.ConnectConnectionReferences(connectionMap, connectionOwner);

            this.crmSvc.Verify(svc => svc.Execute<ExecuteMultipleResponse>(It.IsAny<OrganizationRequest>(), connectionOwner, true));
        }

        [Fact]
        public void ConnectConnectionReferences_WithErrorUpdating_Continues()
        {
            var connectionMap = new Dictionary<string, string>
            {
                { "pdt_sharedapprovals_d7dcb", "12038109da0wud01" },
            };
            this.MockConnectionReferencesForConnectionMap(connectionMap);
            var response = new ExecuteMultipleResponse
            {
                Results =
                {
                    {
                        "IsFaulted",
                        true
                    },
                    {
                        "Responses",
                        new ExecuteMultipleResponseItemCollection()
                        {
                            new ExecuteMultipleResponseItem
                            {
                                Fault = new OrganizationServiceFault(),
                            },
                        }
                    },
                },
            };
            this.MockUpdateConnectionReferencesResponse(response);

            this.connectionReferenceSvc.ConnectConnectionReferences(connectionMap);

            this.loggerMock.VerifyLog(l => l.LogError(It.IsAny<string>()));
        }

        [Fact]
        public void ConnectConnectionReferences_CustomConnectionReferenceIdsDoNotMatch_UpdateConnectorId()
        {
            var connector = new Entity(Constants.Connector.LogicalName, Guid.NewGuid());
            connector.Attributes[Constants.Connector.Fields.ConnectorInternalId] = "some-api-id-123";

            var connectionReference = new Entity(Constants.ConnectionReference.LogicalName, Guid.NewGuid());
            connectionReference.Attributes[Constants.ConnectionReference.Fields.CustomConnectorId] = connector.ToEntityReference();
            connectionReference.Attributes[Constants.ConnectionReference.Fields.ConnectionReferenceLogicalName] = "pdt_sharedpdt5fjson20placeholder5fe62e8ad62fbbb64a_d283a";
            connectionReference.Attributes[Constants.ConnectionReference.Fields.ConnectorId] =
                "/providers/Microsoft.PowerApps/apis/some-api-id-abc"; // Difference from the connector defined above.

            var connectionMap = new Dictionary<string, string>
            {
                { connectionReference.GetAttributeValue<string>(Constants.ConnectionReference.Fields.ConnectionReferenceLogicalName), "52d97d7652b845878eb6f3b61818e03d" },
            };

            this.crmSvc.Setup(
                c => c.RetrieveMultipleByAttribute(
                    Constants.ConnectionReference.LogicalName,
                    Constants.ConnectionReference.Fields.ConnectionReferenceLogicalName,
                    connectionMap.Keys,
                    It.IsAny<ColumnSet>()))
                .Returns(new EntityCollection(new List<Entity>(new Entity[] { connectionReference })));

            this.crmSvc.Setup(
                c => c.Retrieve(
                    Constants.Connector.LogicalName,
                    connector.Id,
                    It.IsAny<ColumnSet>()))
                .Returns(connector)
                .Verifiable();

            this.crmSvc.Setup(
                c => c.Update(It.Is<Entity>(e =>
                    e.LogicalName == Constants.ConnectionReference.LogicalName &&
                    e.Id == connectionReference.Id &&
                    e.Attributes.ContainsKey(Constants.ConnectionReference.Fields.ConnectorId) &&
                    e.GetAttributeValue<string>(Constants.ConnectionReference.Fields.ConnectorId).EndsWith(connector.GetAttributeValue<string>(Constants.Connector.Fields.ConnectorInternalId)))))
                .Verifiable();

            this.MockUpdateConnectionReferencesResponse(new ExecuteMultipleResponse { Results = { { "IsFaulted", false } } });

            this.connectionReferenceSvc.ConnectConnectionReferences(connectionMap);

            this.crmSvc.Verify();
        }

        [Fact]
        public void ConnectConnectionReferences_CustomConnectionReferenceIdsMatch_DoNotUpdateConnectorId()
        {
            var connector = new Entity(Constants.Connector.LogicalName, Guid.NewGuid());
            connector.Attributes[Constants.Connector.Fields.ConnectorInternalId] = "some-api-id-123";

            var connectionReference = new Entity(Constants.ConnectionReference.LogicalName, Guid.NewGuid());
            connectionReference.Attributes[Constants.ConnectionReference.Fields.CustomConnectorId] = connector.ToEntityReference();
            connectionReference.Attributes[Constants.ConnectionReference.Fields.ConnectionReferenceLogicalName] = "pdt_sharedpdt5fjson20placeholder5fe62e8ad62fbbb64a_d283a";
            connectionReference.Attributes[Constants.ConnectionReference.Fields.ConnectorId] =
                $"/providers/Microsoft.PowerApps/apis/{connector.Attributes[Constants.Connector.Fields.ConnectorInternalId]}"; // Same as the connector defined above.

            var connectionMap = new Dictionary<string, string>
            {
                { connectionReference.GetAttributeValue<string>(Constants.ConnectionReference.Fields.ConnectionReferenceLogicalName), "52d97d7652b845878eb6f3b61818e03d" },
            };

            this.crmSvc.Setup(
                c => c.RetrieveMultipleByAttribute(
                    Constants.ConnectionReference.LogicalName,
                    Constants.ConnectionReference.Fields.ConnectionReferenceLogicalName,
                    connectionMap.Keys,
                    It.IsAny<ColumnSet>()))
                .Returns(new EntityCollection(new List<Entity>(new Entity[] { connectionReference })));

            this.crmSvc.Setup(
                c => c.Retrieve(
                    Constants.Connector.LogicalName,
                    connector.Id,
                    It.IsAny<ColumnSet>()))
                .Returns(connector);

            this.MockUpdateConnectionReferencesResponse(new ExecuteMultipleResponse { Results = { { "IsFaulted", false } } });

            this.connectionReferenceSvc.ConnectConnectionReferences(connectionMap);

            this.crmSvc.Verify(
                c => c.Update(It.Is<Entity>(e =>
                    e.LogicalName == Constants.ConnectionReference.LogicalName &&
                    e.Id == connectionReference.Id &&
                    e.Attributes.ContainsKey(Constants.ConnectionReference.Fields.ConnectorId) &&
                    e.GetAttributeValue<string>(Constants.ConnectionReference.Fields.ConnectorId).EndsWith(connector.GetAttributeValue<string>(Constants.Connector.Fields.ConnectorInternalId)))),
                Times.Never);
        }

        private void MockUpdateConnectionReferencesResponse(ExecuteMultipleResponse response)
        {
            this.crmSvc.Setup(svc => svc.Execute(It.IsAny<ExecuteMultipleRequest>())).Returns(response);
            this.crmSvc.Setup(svc => svc.Execute<ExecuteMultipleResponse>(It.IsAny<ExecuteMultipleRequest>(), It.IsAny<string>(), true)).Returns(response);
        }

        private EntityCollection MockConnectionReferencesForConnectionMap(Dictionary<string, string> connectionMap)
        {
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

            return connectionReferences;
        }
    }
}
