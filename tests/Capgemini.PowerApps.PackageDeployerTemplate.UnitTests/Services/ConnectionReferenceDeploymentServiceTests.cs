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
