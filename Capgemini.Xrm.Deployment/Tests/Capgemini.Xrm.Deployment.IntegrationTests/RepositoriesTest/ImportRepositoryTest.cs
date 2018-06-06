using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Capgemini.Xrm.Deployment.IntegrationTests.Core;
using Capgemini.Xrm.Deployment.Repository;

namespace Capgemini.Xrm.Deployment.IntegrationTests.RepositoriesTest
{
    [TestClass]
    public class ImportRepositoryTest : IntegrationTestBase
    {
        [TestMethod]
        public void ApplySolutionUpgradeTest()
        {
            var repo = new CrmImportRepository(this.GetCrmAccessLogged("Url=https://csmhubdev8.crm4.dynamics.com; Username=ciauth@NHSBloodandTransplant.onmicrosoft.com; Password=Xafo2011; AuthType=Office365; RequireNewInstance=True;"));

            var result = repo.ApplySolutionUpgrade("Nhsbt_Core", true, true, 5000, 1800);

        }
    }
}
