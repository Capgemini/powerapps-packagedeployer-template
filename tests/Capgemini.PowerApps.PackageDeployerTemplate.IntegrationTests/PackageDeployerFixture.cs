namespace Capgemini.PowerApps.PackageDeployerTemplate.IntegrationTests
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using Microsoft.Xrm.Sdk.Query;
    using Microsoft.Xrm.Tooling.Connector;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    public class PackageDeployerFixture : IDisposable
    {
        private readonly IMessageSink diagnosticMessageSink;

        public PackageDeployerFixture(IMessageSink diagnosticMessageSink)
        {
            this.diagnosticMessageSink = diagnosticMessageSink;

            // Check values are set.
            _ = GetApprovalsConnection();
            _ = GetTestEnvironmentVariable();
            _ = GetExampleConnectorBaseUrl();

            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/C powershell -ExecutionPolicy ByPass ./Resources/DeployPackage.ps1",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            this.LogDiagnosticMessage("Running `DeployPackage.ps1`...");

            var process = Process.Start(startInfo);

            this.LogDiagnosticMessage("Script output: \n" + process.StandardOutput.ReadToEnd());
            this.LogDiagnosticMessage("Script error output: \n" + process.StandardError.ReadToEnd());

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception("Script `DeployPackage.ps1` failed");
            }

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            this.ServiceClient = new CrmServiceClient(
                $"Url={GetUrl()}; " +
                $"Username={GetUsername()}; " +
                $"Password={GetPassword()}; " +
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

            this.LogDiagnosticMessage($"{nameof(PackageDeployerFixture)} clean up complete.");
        }

        protected static string GetApprovalsConnection() =>
            GetRequiredEnvironmentVariable("PACKAGEDEPLOYER_SETTINGS_CONNREF_PDT_SHAREDAPPROVALS_D7DCB", "No environment variable configured to set approvals connection.");

        protected static string GetLicensedUsername() =>
            GetRequiredEnvironmentVariable("PACKAGEDEPLOYER_SETTINGS_LICENSEDUSERNAME", "No environment variable configured to connect and activate flows.");

        protected static string GetUrl() =>
            GetRequiredEnvironmentVariable("CAPGEMINI_PACKAGE_DEPLOYER_TESTS_URL", "No environment variable configured to set deployment URL.");

        protected static string GetUsername() =>
            GetRequiredEnvironmentVariable("CAPGEMINI_PACKAGE_DEPLOYER_TESTS_USERNAME", "No environment variable configured to set deployment username.");

        protected static string GetPassword() =>
            GetRequiredEnvironmentVariable("CAPGEMINI_PACKAGE_DEPLOYER_TESTS_PASSWORD", "No environment variable configured to set deployment password.");

        protected static string GetTestEnvironmentVariable() =>
            GetRequiredEnvironmentVariable("PACKAGEDEPLOYER_SETTINGS_ENVVAR_PDT_TESTVARIABLE", "No environment variable configured to set power apps test environment variable.");

        protected static string GetExampleConnectorBaseUrl() =>
            GetRequiredEnvironmentVariable("PACKAGEDEPLOYER_SETTINGS_CONNBASEURL_pdt_5Fexample-20api", "No environment variable configured to set custom connector base url.");

        private static string GetRequiredEnvironmentVariable(string name, string exceptionMessage)
        {
            var url = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrEmpty(url))
            {
                throw new Exception(exceptionMessage);
            }

            return url;
        }

        private void UninstallSolution()
        {
            this.LogDiagnosticMessage("Uninstalling solution...");

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
            this.LogDiagnosticMessage("Deleting migrated data...");

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
            this.LogDiagnosticMessage("Deleting included word templates...");

            var wordTemplateQuery = new QueryByAttribute(Constants.DocumentTemplate.LogicalName);
            wordTemplateQuery.AddAttributeValue(Constants.DocumentTemplate.Fields.Name, "Contact Profile");
            var wordTemplate = this.ServiceClient.RetrieveMultiple(wordTemplateQuery).Entities.FirstOrDefault();

            if (wordTemplate != null)
            {
                this.ServiceClient.Delete(Constants.DocumentTemplate.LogicalName, wordTemplate.Id);
            }
        }

        private void LogDiagnosticMessage(string message)
        {
            this.diagnosticMessageSink.OnMessage(new DiagnosticMessage(message));
        }
    }
}
