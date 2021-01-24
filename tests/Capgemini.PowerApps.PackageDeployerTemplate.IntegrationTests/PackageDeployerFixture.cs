namespace Capgemini.PowerApps.PackageDeployerTemplate.IntegrationTests
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using Microsoft.Xrm.Sdk.Query;
    using Microsoft.Xrm.Tooling.Connector;

    public class PackageDeployerFixture : IDisposable
    {
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
                $"Url={Environment.GetEnvironmentVariable("CAPGEMINI_PACKAGE_DEPLOYER_TESTS_URL")}; " +
                $"Username={Environment.GetEnvironmentVariable("CAPGEMINI_PACKAGE_DEPLOYER_TESTS_USERNAME")}; " +
                $"Password={Environment.GetEnvironmentVariable("CAPGEMINI_PACKAGE_DEPLOYER_TESTS_PASSWORD")}; " +
                "AuthType=OAuth; " +
                "AppId=51f81489-12ee-4a9e-aaae-a2591f45987d; " +
                "RedirectUri=app://58145B91-0C36-4500-8554-080854F2AC97");
        }

        public CrmServiceClient ServiceClient { get; private set; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            this.DeleteWordTemplates();
            this.DeleteData();
            this.UninstallSolution();

            this.ServiceClient.Dispose();
        }

        private void UninstallSolution()
        {
            var solutionQuery = new QueryExpression(Constants.Solution.LogicalName)
            {
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.Or,
                    Filters =
                          {
                            new FilterExpression
                            {
                              FilterOperator = LogicalOperator.Or,
                              Conditions =
                              {
                                new ConditionExpression(Constants.Solution.Fields.UniqueName, ConditionOperator.Equal, "pdt_PackageDeployerTemplate_MockSolution"),
                              },
                            },
                          },
                },
            };

            var solutionRecord = this.ServiceClient.RetrieveMultiple(solutionQuery).Entities;

            solutionRecord.ToList().ForEach(solution =>
            {
                this.ServiceClient.Delete(Constants.Solution.LogicalName, solution.Id);
            });
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
            var wordTemplateQuery = new QueryByAttribute(Constants.DocumentTemplate.LogicalName);
            wordTemplateQuery.AddAttributeValue(Constants.DocumentTemplate.Fields.Name, "Contact Profile");
            var wordTemplate = this.ServiceClient.RetrieveMultiple(wordTemplateQuery).Entities.FirstOrDefault();

            if (wordTemplate != null)
            {
                this.ServiceClient.Delete(Constants.DocumentTemplate.LogicalName, wordTemplate.Id);
            }
        }
    }
}
