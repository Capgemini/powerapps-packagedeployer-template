using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;

namespace Capgemini.PowerApps.PackageDeployerTemplate.IntegrationTests
{
    public class PackageDeployerFixture : IDisposable
    {
        public CrmServiceClient ServiceClient { get; private set; }

        public PackageDeployerFixture()
        {
            var process = new Process();
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/C powershell ./Resources/DeployPackage.ps1",
            };
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception("Script `DeployPackage.ps1` failed");
            }

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            this.ServiceClient = new CrmServiceClient(
                $"Url={Environment.GetEnvironmentVariable("CAPGEMINI_PACKAGE_DEPLOYER_TESTS_URL")}; Username={Environment.GetEnvironmentVariable("CAPGEMINI_PACKAGE_DEPLOYER_TESTS_USERNAME")}; Password={Environment.GetEnvironmentVariable("CAPGEMINI_PACKAGE_DEPLOYER_TESTS_PASSWORD")}; authtype=Office365");

        }

        public void Dispose()
        {
            DeleteWordTemplates();
            DeleteData();
            UninstallSolution();

            this.ServiceClient.Dispose();
        }

        private void UninstallSolution()
        {
            var slaQuery = new QueryByAttribute("sla");
            slaQuery.AddAttributeValue("name", "Standard Case SLA");
            var sla = this.ServiceClient.RetrieveMultiple(slaQuery).Entities.FirstOrDefault();

            if (sla != null)
            {
                this.ServiceClient.UpdateStateAndStatusForEntity("sla", sla.Id, 0, 1);
            }

            var solutionQuery = new QueryByAttribute("solution");
            solutionQuery.AddAttributeValue("uniquename", "cap_PackageDeployerTemplate_IntegrationTest");
            var solutionRecord = this.ServiceClient.RetrieveMultiple(solutionQuery).Entities.FirstOrDefault();

            if (solutionRecord != null)
            {
                this.ServiceClient.Delete("solution", solutionRecord.Id);
            }
        }

        private void DeleteData()
        {
            var preDeploymentRecordQuery = new QueryByAttribute("subject");
            preDeploymentRecordQuery.AddAttributeValue("title", "Integration Test Subject");
            var preDeploymentRecord = this.ServiceClient.RetrieveMultiple(preDeploymentRecordQuery).Entities.FirstOrDefault();

            if (preDeploymentRecord != null)
            {
                this.ServiceClient.Delete("subject", preDeploymentRecord.Id);
            }

            var postDeploymentRecordQuery = new QueryByAttribute("businessunit");
            postDeploymentRecordQuery.AddAttributeValue("name", "Package Deployer Template Test BU");
            var postDeploymentRecord = this.ServiceClient.RetrieveMultiple(postDeploymentRecordQuery).Entities.FirstOrDefault();

            if (postDeploymentRecord != null)
            {
                postDeploymentRecord.Attributes.Add("isdisabled", true);
                this.ServiceClient.Update(postDeploymentRecord);
                this.ServiceClient.Delete("businessunit", postDeploymentRecord.Id);
            }
        }

        private void DeleteWordTemplates()
        {
            var wordTemplateQuery = new QueryByAttribute("documenttemplate");
            wordTemplateQuery.AddAttributeValue("name", "Case Survey");
            var wordTemplate = this.ServiceClient.RetrieveMultiple(wordTemplateQuery).Entities.FirstOrDefault();

            if (wordTemplate != null)
            {
                this.ServiceClient.Delete("documenttemplate", wordTemplate.Id);
            }
        }
    }
}
