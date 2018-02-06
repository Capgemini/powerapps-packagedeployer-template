using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace Capgemini.Xrm.Deployment.IntegrationTests.ImportTests
{
    [TestClass]
    public class VersionTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            Version ver1 = new Version("1.0.0.0");
            Version ver2 = new Version("1.0.0.1");
            Version ver3 = new Version("1.0.0.01");

            Debug.Write("Version " + ver1 + " test");
            Assert.IsTrue(ver1 < ver2);
            Assert.IsTrue(ver2 > ver1);
            Assert.IsTrue(ver2 == ver3);
        }
    }
}