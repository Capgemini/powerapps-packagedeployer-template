namespace Capgemini.PowerApps.PackageDeployerTemplate.IntegrationTests
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using Xunit;

    public class CapgeminiPackageTemplateTests : IClassFixture<PackageDeployerFixture>
    {
        private readonly PackageDeployerFixture fixture;

        public CapgeminiPackageTemplateTests(PackageDeployerFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void CapgeminiPackageTemplate_WordTemplateConfigured_WordTemplateIsImported()
        {
            var wordTemplateQuery = new QueryByAttribute("documenttemplate");
            wordTemplateQuery.AddAttributeValue("name", "Case Survey");

            this.fixture.ServiceClient.RetrieveMultiple(wordTemplateQuery).Entities.Should().NotBeEmpty();
        }

        [Fact]
        public void CapgeminiPackageTemplate_PreDeploymentDataConfigured_PreDeploymentDataIsImported()
        {
            var subjectQuery = new QueryByAttribute("subject");
            subjectQuery.AddAttributeValue("title", "Integration Test Subject");

            this.fixture.ServiceClient.RetrieveMultiple(subjectQuery).Entities.Should().NotBeEmpty();
        }

        [Fact]
        public void CapgeminiPackageTemplate_PreDeploymentDataConfigured_PreDeploymentDataIsImportedPreDeployment()
        {
            var preDeploymentRecordQuery = new QueryByAttribute("subject");
            preDeploymentRecordQuery.AddAttributeValue("title", "Integration Test Subject");
            preDeploymentRecordQuery.ColumnSet = new ColumnSet("createdon");
            var preDeploymentRecord = this.fixture.ServiceClient.RetrieveMultiple(preDeploymentRecordQuery).Entities.FirstOrDefault();

            var solutionQuery = new QueryByAttribute("solution");
            solutionQuery.AddAttributeValue("uniquename", "cap_PackageDeployerTemplate_IntegrationTest");
            solutionQuery.ColumnSet = new ColumnSet("createdon");
            var solutionRecord = this.fixture.ServiceClient.RetrieveMultiple(solutionQuery).Entities.FirstOrDefault();

            preDeploymentRecord["createdon"].As<DateTime>().Should().BeBefore(solutionRecord["createdon"].As<DateTime>());
        }

        [Fact]
        public void CapgeminiPackageTemplate_PostDeploymentDataConfigured_PostDeploymentDataIsImported()
        {
            var businessUnitQuery = new QueryByAttribute("businessunit");
            businessUnitQuery.AddAttributeValue("name", "Package Deployer Template Test BU");

            this.fixture.ServiceClient.RetrieveMultiple(businessUnitQuery).Entities.Should().NotBeEmpty();
        }

        [Fact]
        public void CapgeminiPackageTemplate_PostDeploymentDataConfigured_PostDeploymentDataIsImportedPostDeployment()
        {
            var postDeploymentRecordQuery = new QueryByAttribute("businessunit");
            postDeploymentRecordQuery.AddAttributeValue("name", "Package Deployer Template Test BU");
            postDeploymentRecordQuery.ColumnSet = new ColumnSet("createdon");
            var postDeploymentRecord = this.fixture.ServiceClient.RetrieveMultiple(postDeploymentRecordQuery).Entities.FirstOrDefault();

            var solutionQuery = new QueryByAttribute("solution");
            solutionQuery.AddAttributeValue("uniquename", "cap_PackageDeployerTemplate_IntegrationTest");
            solutionQuery.ColumnSet = new ColumnSet("createdon");
            var solutionRecord = this.fixture.ServiceClient.RetrieveMultiple(solutionQuery).Entities.FirstOrDefault();

            postDeploymentRecord["createdon"].As<DateTime>().Should().BeAfter(solutionRecord["createdon"].As<DateTime>());
        }

        [Fact]
        public void CapgeminiPackageTemplate_SlaInSolution_SlaIsActivated()
        {
            var slaQuery = new QueryByAttribute("sla");
            slaQuery.AddAttributeValue("name", "Standard Case SLA");
            slaQuery.ColumnSet = new ColumnSet("statecode");

            var sla = this.fixture.ServiceClient.RetrieveMultiple(slaQuery).Entities.FirstOrDefault();

            sla["statecode"].As<OptionSetValue>().Value.Should().Be(1);
        }

        [Fact]
        public void CapgeminiPackageTemplate_DefaultSlaConfigured_SlasIsSetToDefault()
        {
            var slaQuery = new QueryByAttribute("sla");
            slaQuery.AddAttributeValue("name", "Standard Case SLA");
            slaQuery.ColumnSet = new ColumnSet("isdefault");

            var sla = this.fixture.ServiceClient.RetrieveMultiple(slaQuery).Entities.FirstOrDefault();

            sla["isdefault"].As<bool>().Should().BeTrue();
        }

        [Fact]
        public void CapgeminiPackageTemplate_ProcessConfiguredToDeactivate_ProcessIsDeactivated()
        {
            var subjectQuery = new QueryByAttribute("workflow");
            subjectQuery.AddAttributeValue("name", "Case: Set Name");
            subjectQuery.AddAttributeValue("type", 1);
            subjectQuery.ColumnSet = new ColumnSet("statecode");

            var workflow = this.fixture.ServiceClient.RetrieveMultiple(subjectQuery).Entities.FirstOrDefault();

            workflow["statecode"].As<OptionSetValue>().Value.Should().Be(0);
        }

        [Fact]
        public void CapgeminiPackageTemplate_SdkStepConfiguredToDeactivate_SdkStepIsDeactivated()
        {
            var subjectQuery = new QueryByAttribute("sdkmessageprocessingstep");
            subjectQuery.AddAttributeValue("name", "Case: Publish Update of Case Reference to ASB");
            subjectQuery.ColumnSet = new ColumnSet("statecode");
            var workflow = this.fixture.ServiceClient.RetrieveMultiple(subjectQuery).Entities.FirstOrDefault();

            workflow["statecode"].As<OptionSetValue>().Value.Should().Be(1);
        }

        [Theory]
        [InlineData("Account Creation Trigger -> Terminate", Constants.Process.StateCodeInactive)]
        [InlineData("Account Creation Trigger1 -> Terminate", Constants.Process.StateCodeActive)]
        public void CapgeminiPackageTemplate_FlowsAreActivated(string workflowName, int stateCode)
        {
            var workflowQuery = new QueryByAttribute("workflow");
            workflowQuery.AddAttributeValue("name", workflowName);
            workflowQuery.ColumnSet = new ColumnSet("statecode");
            var workflow = this.fixture.ServiceClient.RetrieveMultiple(workflowQuery).Entities.FirstOrDefault();

            workflow["statecode"].As<OptionSetValue>().Value.Should().Be(stateCode);
        }
    }
}
