using System;
using System.IO;
using System.Reflection;

namespace Capgemini.PowerApps.PackageDeployerTemplate.UnitTests
{
    public static class TestUtilities
    {
        public static string GetResourcePath(string resourceFileName)
        {
            var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
            var dirPath = Path.GetDirectoryName(codeBasePath);

            return Path.Combine(dirPath, "Resources", resourceFileName);
        }
    }
}
