using Capgemini.Xrm.Deployment.IntegrationTests.Core;
using Capgemini.Xrm.Deployment.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Capgemini.Xrm.Deployment.IntegrationTests.RepositoriesTest
{
    [TestClass]
    public class OrganizationRepoTest : IntegrationTestBase
    {
        [TestMethod]
        public void GetSetting_Test()
        {
            var repo = new CrmOrganizationRepository(this.GetCrmAccessLogged("Url=https://sadfsdfs.crm4.dynamics.com; Username=ciauth@NHSBloodandTransplant.onmicrosoft.com; Password=Xafo2011; AuthType=Office365; Timeout=00:10:00;"));

            var setting = repo.GetOrganizationSetting("isauditenabled");

            Assert.IsNotNull(setting);
        }

        [TestMethod]
        public void DisableAuditTest_Test()
        {
            var repo = new CrmOrganizationRepository(this.GetCrmAccessLogged("Url=https://sadfsdfs.crm4.dynamics.com; Username=ciauth@NHSBloodandTransplant.onmicrosoft.com; Password=Xafo2011; AuthType=Office365; Timeout=00:10:00;"));

            repo.SetOrganizationSetting("isauditenabled", "False", "System.Boolean");

            var setting = repo.GetOrganizationSetting("isauditenabled");

            Assert.IsNotNull(setting);
            Assert.AreEqual(setting.GetAttributeValue<bool>("isauditenabled"), false);
        }

        [TestMethod]
        public void EnableAuditTest_Test()
        {
            var repo = new CrmOrganizationRepository(this.GetCrmAccessLogged("Url=https://sadfsdfs.crm4.dynamics.com; Username=ciauth@NHSBloodandTransplant.onmicrosoft.com; Password=Xafo2011; AuthType=Office365; Timeout=00:10:00;"));

            repo.SetOrganizationSetting("isauditenabled", "True", "System.Boolean");

            var setting = repo.GetOrganizationSetting("isauditenabled");

            Assert.IsNotNull(setting);
            Assert.AreEqual(setting.GetAttributeValue<bool>("isauditenabled"), true);
        }
    }
}