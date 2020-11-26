using DocumentFormat.OpenXml.Packaging;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;

namespace Capgemini.PowerApps.Deployment
{
    public static class WordTemplateUtilities
    {
        public static string GetEntityLogicalName(string filePath)
        {
            return FindInWordDocument(filePath, @"urn:microsoft-crm/document-template/(.*)/\d*/");
        }

        public static string GetEntityTypeCode(string filePath)
        {
            return FindInWordDocument(filePath, @"urn:microsoft-crm/document-template/.*/(\d*)/");
        }

        public static void SetEntity(string filePath, string logicalName, string typeCode)
        {
            var pattern = @"urn:microsoft-crm/document-template/.*/\d*/";
            var replace = $@"urn:microsoft-crm/document-template/{logicalName}/{typeCode}/";

            using (var doc = WordprocessingDocument.Open(filePath, true, new OpenSettings { AutoSave = true }))
            {
                doc.MainDocumentPart.Document.InnerXml = Regex.Replace(
                    doc.MainDocumentPart.Document.InnerXml,
                    pattern,
                    replace);

                foreach (var customXmlPart in doc.MainDocumentPart.CustomXmlParts)
                {
                    using (var sr = new StreamReader(customXmlPart.GetStream()))
                    {
                        var updatedXmlPart = Regex.Replace(sr.ReadToEnd(), pattern, replace);
                        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(updatedXmlPart)))
                        {
                            customXmlPart.FeedData(ms);
                        }
                    }
                }
            }
        }

        private static string FindInWordDocument(string filePath, string regexPattern)
        {
            using (var doc = WordprocessingDocument.Open(filePath, true, new OpenSettings { AutoSave = true }))
            {
                foreach (var customXmlPart in doc.MainDocumentPart.CustomXmlParts)
                {
                    using (var sr = new StreamReader(customXmlPart.GetStream()))
                    {
                        var match = Regex.Match(sr.ReadToEnd(), regexPattern);

                        if (match.Groups.Count > 1)
                        {
                            return match.Groups[1].Value;
                        }
                    }
                }

                throw new PackageDeployerException("Unable to find entity logical name and type code in template.");
            }
        }
    }
}