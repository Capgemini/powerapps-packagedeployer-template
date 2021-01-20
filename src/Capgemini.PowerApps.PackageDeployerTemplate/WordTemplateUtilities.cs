namespace Capgemini.PowerApps.PackageDeployerTemplate
{
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using DocumentFormat.OpenXml.Packaging;
    using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;

    /// <summary>
    /// Utilities relating to word templates.
    /// </summary>
    public static class WordTemplateUtilities
    {
        /// <summary>
        /// Gets the entity logical name associated to the provided word template.
        /// </summary>
        /// <param name="filePath">The path to the word template.</param>
        /// <returns>The entity logical name.</returns>
        public static string GetEntityLogicalName(string filePath)
        {
            return FindInWordDocument(filePath, @"urn:microsoft-crm/document-template/(.*)/\d*/");
        }

        /// <summary>
        /// Gets the entity type code associated to the provided word template.
        /// </summary>
        /// <param name="filePath">The path to the word template.</param>
        /// <returns>The entity type code.</returns>
        public static string GetEntityTypeCode(string filePath)
        {
            return FindInWordDocument(filePath, @"urn:microsoft-crm/document-template/.*/(\d*)/");
        }

        /// <summary>
        /// Updates the entity logical name and type code associated to the provided word template.
        /// </summary>
        /// <param name="filePath">The path to the word template.</param>
        /// <param name="logicalName">The logical name to set.</param>
        /// <param name="typeCode">The entity type code to set.</param>
        public static void SetEntity(string filePath, string logicalName, string typeCode)
        {
            var pattern = @"urn:microsoft-crm/document-template/.*/\d*/";
            var replace = $@"urn:microsoft-crm/document-template/{logicalName}/{typeCode}/";

            using var doc = WordprocessingDocument.Open(filePath, true, new OpenSettings { AutoSave = true });
            doc.MainDocumentPart.Document.InnerXml = Regex.Replace(
                doc.MainDocumentPart.Document.InnerXml,
                pattern,
                replace);

            foreach (var customXmlPart in doc.MainDocumentPart.CustomXmlParts)
            {
                using var sr = new StreamReader(customXmlPart.GetStream());
                var updatedXmlPart = Regex.Replace(sr.ReadToEnd(), pattern, replace);
                using var ms = new MemoryStream(Encoding.UTF8.GetBytes(updatedXmlPart));
                customXmlPart.FeedData(ms);
            }
        }

        private static string FindInWordDocument(string filePath, string regexPattern)
        {
            using var doc = WordprocessingDocument.Open(filePath, true, new OpenSettings { AutoSave = true });
            foreach (var customXmlPart in doc.MainDocumentPart.CustomXmlParts)
            {
                using var sr = new StreamReader(customXmlPart.GetStream());
                var match = Regex.Match(sr.ReadToEnd(), regexPattern);

                if (match.Groups.Count > 1)
                {
                    return match.Groups[1].Value;
                }
            }

            throw new PackageDeployerException("Unable to find entity logical name and type code in template.");
        }
    }
}