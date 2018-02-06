using Capgemini.Xrm.Deployment.IntegrationTests.Core;
using Capgemini.Xrm.Deployment.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Capgemini.Xrm.Deployment.IntegrationTests.RepositoriesTest
{
    [TestClass]
    public class SlaRepositoryTest : IntegrationTestBase
    {
        private const string ConnectionString = "Url=https://sadfsdfs.crm4.dynamics.com; Username=ciauth@NHSBloodandTransplant.onmicrosoft.com; Password=Xafo2011; AuthType=Office365; Timeout=00:10:00;";

        [TestMethod]
        public void GetAllSlasTest()
        {
            CrmSlaRepository repo = new CrmSlaRepository(this.GetCrmAccessLogged(ConnectionString));
            var slas = repo.GetAllSlas();
        }

        [TestMethod]
        public void DeactivateSlaTest()
        {
            CrmSlaRepository repo = new CrmSlaRepository(this.GetCrmAccessLogged(ConnectionString));
            Guid slaId = Guid.Parse("14197c2e-f115-e711-80ff-5065f38a9a01");

            var sla = repo.GetSlaById(slaId);
            repo.DeactivateSla(sla);
        }

        [TestMethod]
        public void ActivateSlaTest()
        {
            CrmSlaRepository repo = new CrmSlaRepository(this.GetCrmAccessLogged(ConnectionString));
            Guid slaId = Guid.Parse("14197c2e-f115-e711-80ff-5065f38a9a01");

            var sla = repo.GetSlaById(slaId);
            repo.ActivateSla(sla);
        }

        [TestMethod]
        public void MakeSlaDefaultTest()
        {
            CrmSlaRepository repo = new CrmSlaRepository(this.GetCrmAccessLogged(ConnectionString));
            string slaName = "Customer Service Request SLA";

            var sla = repo.GetSlaByName(slaName);
            repo.SetSlaDefault(sla);
        }
    }
}