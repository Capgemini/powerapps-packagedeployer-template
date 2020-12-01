using System;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace Capgemini.PowerApps.PackageDeployerTemplate
{
    public static class ZipFileUtilities
    {
        /// <summary>
        /// Perform a regex find and replace for all files within a zip.
        /// </summary>
        /// <param name="zipPath">The path of the zip file.</param>
        /// <param name="pattern">The pattern to search for.</param>
        /// <param name="replacement">The string to replace with.</param>
        public static void FindAndReplace(string zipPath, string pattern, string replacement)
        {
            var fileInfo = new FileInfo(zipPath);
            var tempExtractFolder = Path.Combine(Path.GetTempPath(), $"{fileInfo.Name}-{Guid.NewGuid()}");

            ZipFile.ExtractToDirectory(zipPath, tempExtractFolder);
            foreach (string file in Directory.GetFiles(tempExtractFolder, "*.*", SearchOption.AllDirectories))
            {
                File.WriteAllText(
                    file,
                    Regex.Replace(
                        File.ReadAllText(file),
                        pattern,
                        replacement));
            }
            File.Delete(zipPath);
            ZipFile.CreateFromDirectory(tempExtractFolder, zipPath);
            Directory.Delete(tempExtractFolder, true);
        }
    }
}
