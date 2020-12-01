using System;
using System.IO;
using System.Reflection;

namespace Capgemini.PowerApps.Deployment.IntegrationTests
{
    public static class TestUtilities
    {
        public static string GetOutputFolderPath()
        {
            var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
            return Path.GetDirectoryName(codeBasePath);
        }
    }
}
