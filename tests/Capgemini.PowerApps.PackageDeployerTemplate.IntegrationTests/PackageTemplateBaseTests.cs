namespace Capgemini.PowerApps.PackageDeployerTemplate.IntegrationTests
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using Xunit;

    public class PackageTemplateBaseTests : IClassFixture<PackageDeployerFixture>
    {
        private readonly PackageDeployerFixture fixture;

        public PackageTemplateBaseTests(PackageDeployerFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void PackageTemplateBase_WordTemplateConfigured_WordTemplateIsImported()
        {
            var wordTemplateQuery = new QueryByAttribute(Constants.DocumentTemplate.LogicalName);
            wordTemplateQuery.AddAttributeValue(Constants.DocumentTemplate.Fields.Name, "Contact Profile");

            this.fixture.ServiceClient.RetrieveMultiple(wordTemplateQuery).Entities.Should().NotBeEmpty();
        }

        [Fact]
        public void PackageTemplateBase_PreDeploymentDataConfigured_PreDeploymentDataIsImported()
        {
            var subjectQuery = new QueryByAttribute("subject");
            subjectQuery.AddAttributeValue("title", "Integration Test Subject");

            this.fixture.ServiceClient.RetrieveMultiple(subjectQuery).Entities.Should().NotBeEmpty();
        }

        [Fact]
        public void PackageTemplateBase_PreDeploymentDataConfigured_PreDeploymentDataIsImportedPreDeployment()
        {
            var preDeploymentRecordQuery = new QueryByAttribute("subject");
            preDeploymentRecordQuery.AddAttributeValue("title", "Integration Test Subject");
            preDeploymentRecordQuery.ColumnSet = new ColumnSet("createdon");
            var preDeploymentRecord = this.fixture.ServiceClient.RetrieveMultiple(preDeploymentRecordQuery).Entities.FirstOrDefault();

            var solutionQuery = new QueryByAttribute(Constants.Solution.LogicalName);
            solutionQuery.AddAttributeValue(Constants.Solution.Fields.UniqueName, "pdt_PackageDeployerTemplate_MockSolution");
            solutionQuery.ColumnSet = new ColumnSet("createdon");
            var solutionRecord = this.fixture.ServiceClient.RetrieveMultiple(solutionQuery).Entities.FirstOrDefault();

            preDeploymentRecord["createdon"].As<DateTime>().Should().BeBefore(solutionRecord["createdon"].As<DateTime>());
        }

        [Fact]
        public void PackageTemplateBase_PostDeploymentDataConfigured_PostDeploymentDataIsImported()
        {
            var businessUnitQuery = new QueryByAttribute("businessunit");
            businessUnitQuery.AddAttributeValue("name", "Package Deployer Template Test BU");

            this.fixture.ServiceClient.RetrieveMultiple(businessUnitQuery).Entities.Should().NotBeEmpty();
        }

        [Fact]
        public void PackageTemplateBase_PostDeploymentDataConfigured_PostDeploymentDataIsImportedPostDeployment()
        {
            var postDeploymentRecordQuery = new QueryByAttribute("businessunit");
            postDeploymentRecordQuery.AddAttributeValue("name", "Package Deployer Template Test BU");
            postDeploymentRecordQuery.ColumnSet = new ColumnSet("createdon");
            var postDeploymentRecord = this.fixture.ServiceClient.RetrieveMultiple(postDeploymentRecordQuery).Entities.FirstOrDefault();

            var solutionQuery = new QueryByAttribute(Constants.Solution.LogicalName);
            solutionQuery.AddAttributeValue(Constants.Solution.Fields.UniqueName, "pdt_PackageDeployerTemplate_MockSolution");
            solutionQuery.ColumnSet = new ColumnSet("createdon");
            var solutionRecord = this.fixture.ServiceClient.RetrieveMultiple(solutionQuery).Entities.First();

            postDeploymentRecord["createdon"].As<DateTime>().Should().BeAfter(solutionRecord["createdon"].As<DateTime>());
        }

        [Fact]
        public void PackageTemplateBase_ProcessConfiguredToDeactivate_ProcessIsDeactivated()
        {
            var workflowQuery = new QueryByAttribute(Constants.Workflow.LogicalName);
            workflowQuery.AddAttributeValue(Constants.Workflow.Fields.Name, "When a contact is created do nothing");
            workflowQuery.AddAttributeValue(Constants.Workflow.Fields.Type, Constants.Workflow.TypeDefinition);
            workflowQuery.ColumnSet = new ColumnSet("statecode");

            var workflow = this.fixture.ServiceClient.RetrieveMultiple(workflowQuery).Entities.FirstOrDefault();

            workflow["statecode"].As<OptionSetValue>().Value.Should().Be(Constants.Workflow.StateCodeInactive);
        }

        [Fact]
        public void PackageTemplateBase_ProcessConfiguredToActivate_ProcessIsActivated()
        {
            var workflowQuery = new QueryByAttribute(Constants.Workflow.LogicalName);
            workflowQuery.AddAttributeValue(Constants.Workflow.Fields.Name, "When an account is created do nothing");
            workflowQuery.AddAttributeValue(Constants.Workflow.Fields.Type, Constants.Workflow.TypeDefinition);
            workflowQuery.ColumnSet = new ColumnSet("statecode");

            var workflow = this.fixture.ServiceClient.RetrieveMultiple(workflowQuery).Entities.FirstOrDefault();

            workflow["statecode"].As<OptionSetValue>().Value.Should().Be(Constants.Workflow.StateCodeActive);
        }

        [Fact]
        public void PackageTemplateBase_SdkStepConfiguredToDeactivate_SdkStepIsDeactivated()
        {
            var sdkStepQuery = new QueryByAttribute(Constants.SdkMessageProcessingStep.LogicalName);
            sdkStepQuery.AddAttributeValue(Constants.SdkMessageProcessingStep.Fields.Name, "Test WebHook: Create of contact");
            sdkStepQuery.ColumnSet = new ColumnSet("statecode");
            var sdkStep = this.fixture.ServiceClient.RetrieveMultiple(sdkStepQuery).Entities.FirstOrDefault();

            sdkStep["statecode"].As<OptionSetValue>().Value.Should().Be(Constants.SdkMessageProcessingStep.StateCodeInactive);
        }

        [Fact]
        public void PackageTemplateBase_ConnectionNamePassed_ConnectionIsSet()
        {
            var connectionReferenceQuery = new QueryByAttribute(Constants.ConnectionReference.LogicalName);
            connectionReferenceQuery.AddAttributeValue(Constants.ConnectionReference.Fields.ConnectionReferenceLogicalName, "pdt_sharedapprovals_d7dcb");
            connectionReferenceQuery.ColumnSet = new ColumnSet(Constants.ConnectionReference.Fields.ConnectionId);

            var connectionReference = this.fixture.ServiceClient.RetrieveMultiple(connectionReferenceQuery).Entities.First();

            connectionReference.GetAttributeValue<string>(Constants.ConnectionReference.Fields.ConnectionId).Should().Be(Environment.GetEnvironmentVariable("PACKAGEDEPLOYER_SETTINGS_CONNREF_PDT_SHAREDAPPROVALS_D7DCB"));
        }

        [Fact]
        public void PackageTemplateBase_EnvironmentVariablePassed_EnvironmentVariableIsSet()
        {
            var variableDefinitionQuery = new QueryByAttribute(Constants.EnvironmentVariableDefinition.LogicalName);
            variableDefinitionQuery.AddAttributeValue(Constants.EnvironmentVariableDefinition.Fields.SchemaName, "pdt_testvariable");
            variableDefinitionQuery.ColumnSet = new ColumnSet(false);

            var variableDefinition = this.fixture.ServiceClient.RetrieveMultiple(variableDefinitionQuery).Entities.First();

            var variableValueQuery = new QueryByAttribute(Constants.EnvironmentVariableValue.LogicalName);
            variableValueQuery.AddAttributeValue(Constants.EnvironmentVariableValue.Fields.EnvironmentVariableDefinitonId, variableDefinition.Id);
            variableValueQuery.ColumnSet = new ColumnSet(Constants.EnvironmentVariableValue.Fields.Value);

            var variableValue = this.fixture.ServiceClient.RetrieveMultiple(variableValueQuery).Entities.First();

            variableValue.GetAttributeValue<string>(Constants.EnvironmentVariableValue.Fields.Value).Should().Be(Environment.GetEnvironmentVariable("PACKAGEDEPLOYER_SETTINGS_ENVVAR_PDT_TESTVARIABLE"));
        }

        [Theory]
        [InlineData("When a contact is created -> Terminate", Constants.Workflow.StateCodeInactive)]
        [InlineData("When a contact is created -> Create an approval", Constants.Workflow.StateCodeActive)]
        public void PackageTemplateBase_FlowsInPackage_AreActiveAccordingToConfig(string flowName, int stateCode)
        {
            var workflowQuery = new QueryByAttribute(Constants.Workflow.LogicalName);
            workflowQuery.AddAttributeValue(Constants.Workflow.Fields.Name, flowName);
            workflowQuery.ColumnSet = new ColumnSet("statecode");
            var workflow = this.fixture.ServiceClient.RetrieveMultiple(workflowQuery).Entities.FirstOrDefault();

            workflow["statecode"].As<OptionSetValue>().Value.Should().Be(stateCode);
        }

        [Theory]
        [InlineData("account", "pdt_testautonumber", 10000)]
        public void PackageTemplateBase_TableColumnProcessing_AutonumberSeedIsSet(string entityName, string attributeName, int expectedValue)
        {
            var req = new GetAutoNumberSeedRequest
            {
                EntityName = entityName,
                AttributeName = attributeName,
            };

            GetAutoNumberSeedResponse response = (GetAutoNumberSeedResponse)this.fixture.ServiceClient.Execute(req);

            response.AutoNumberSeedValue.Should().Be(expectedValue);
        }
    }
}
