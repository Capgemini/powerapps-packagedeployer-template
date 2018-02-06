using Capgemini.Xrm.Deployment.IntegrationTests.Core;
using Capgemini.Xrm.Deployment.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;

namespace Capgemini.Xrm.Deployment.IntegrationTests.RepositoriesTest
{
    [TestClass]
    public class WorkflowRepoTest : IntegrationTestBase
    {
        [TestMethod]
        public void GetAllWorkflows_Test()
        {
            var repo = new CrmWorkflowsRepository(CrmAccessLogged);

            var wfs = repo.GetAllWorkflows();
        }

        [TestMethod]
        public void GetAllSDKSteps_Test()
        {
            var repo = new CrmWorkflowsRepository(CrmAccessLogged);

            var wfs = repo.GetAllCustomizableSDKSteps();
        }

        [TestMethod]
        public void DeactivateSDKSteps_Test()
        {
            var repo = new CrmWorkflowsRepository(CrmAccessLogged);

            var sdksteps = repo.GetAllCustomizableSDKSteps();

            foreach (var item in sdksteps)
            {
                var eventhandler = item.GetAttributeValue<EntityReference>("eventhandler");
                var name = item.GetAttributeValue<string>("name");

                if (name == "Uplift Persistence on Update of nhs_supplyplan" && eventhandler.Name == "Nhsbt.Donor.Plugins.CollectionPlanUpliftPersistence")
                {
                    repo.DeActivateSDKStep(item.Id);
                }
            }
        }

        [TestMethod]
        public void ActivateSDKSteps_Test()
        {
            var repo = new CrmWorkflowsRepository(CrmAccessLogged);

            var sdksteps = repo.GetAllCustomizableSDKSteps();

            foreach (var item in sdksteps)
            {
                var eventhandler = item.GetAttributeValue<EntityReference>("eventhandler");
                var name = item.GetAttributeValue<string>("name");

                if (name == "Uplift Persistence on Update of nhs_supplyplan" && eventhandler.Name == "Nhsbt.Donor.Plugins.CollectionPlanUpliftPersistence")
                {
                    repo.ActivateSDKStep(item.Id);
                }
            }
        }
    }
}