using DocumentFormat.OpenXml.Packaging;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Capgemini.PowerApps.Deployment
{
    public static class WordTemplateUtilities
    {
        public static string GetEntityLogicalName(string filePath)
        {
            using (var doc = WordprocessingDocument.Open(filePath, true, new OpenSettings { AutoSave = true }))
            {
                foreach (var customXmlPart in doc.MainDocumentPart.CustomXmlParts)
                {
                    using (var sr = new StreamReader(customXmlPart.GetStream()))
                    {
                        var match = Regex.Match(
                            sr.ReadToEnd(),
                            @"urn:microsoft-crm/document-template/(.*)/\d*/");

                        if (match.Groups.Count > 1)
                        {
                            return match.Groups[1].Value;
                        }
                    }
                }

                throw new Exception("Unable to find entity logical name and type code in template.");
            }
        }

        public static string GetEntityTypeCode(string filePath)
        {
            using (var doc = WordprocessingDocument.Open(filePath, true, new OpenSettings { AutoSave = true }))
            {
                foreach (var customXmlPart in doc.MainDocumentPart.CustomXmlParts)
                {
                    using (var sr = new StreamReader(customXmlPart.GetStream()))
                    { 
                        var match = Regex.Match(
                            sr.ReadToEnd(),
                            @"urn:microsoft-crm/document-template/.*/(\d*)/");

                        if (match.Groups.Count > 1)
                        {
                            return match.Groups[1].Value;
                        }
                    }
                }

                throw new Exception("Unable to find entity logical name and type code in template.");
            }
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
                    using (var s = customXmlPart.GetStream())
                    using (var sr = new StreamReader(s))
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
    }
}